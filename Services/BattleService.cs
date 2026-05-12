using FordEnterRPG.Data;
using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Services
{
    public class BattleService : IBattleService
    {
        // ── Tabelas estáticas ─────────────────────────────────────────────

        private static readonly Dictionary<string, (int life, int damage)> EnemyBaseStats = new()
        {
            { "Warrior", (22, 6) },
            { "Mage",    (16, 9) },
            { "Archer",  (18, 7) }
        };

        // Movimentos do inimigo: (nome, dano-base-proprio)
        private static readonly Dictionary<string, (string name, int dmg)[]> EnemyMoves = new()
        {
            { "Warrior", [("Golpe de Espada", 5), ("Ataque Brutal", 8), ("Investida", 6)] },
            { "Mage",    [("Bola de Fogo", 6),    ("Raio Arcano", 8),   ("Explosão Mágica", 7)] },
            { "Archer",  [("Flecha Rápida", 5),   ("Tiro Preciso", 7),  ("Chuva de Flechas", 6)] }
        };

        private static readonly string[] EnemyPrefixes =
            ["Sombra", "Ferro", "Sangue", "Gelo", "Chamas", "Trovão", "Veneno", "Névoa", "Abismo", "Cinza"];

        private static readonly Dictionary<string, string[]> EnemySuffixes = new()
        {
            { "Warrior", ["Guardião", "Berserker", "Cavaleiro", "Paladino"] },
            { "Mage",    ["Feiticeiro", "Necromante", "Archimago", "Bruxo"] },
            { "Archer",  ["Caçador", "Sentinela", "Batedor", "Atirador"] }
        };

        private readonly AppDbContext _db;
        public BattleService(AppDbContext db) => _db = db;

        // ── Iniciar Batalha ───────────────────────────────────────────────

        public async Task<Battle> StartBattleAsync(Character player)
        {
            var rng = new Random();
            var classes = new[] { "Warrior", "Mage", "Archer" };
            var enemyClass = classes[rng.Next(classes.Length)];
            var stats = EnemyBaseStats[enemyClass];
            int scale = player.Level - 1;

            var battle = new Battle
            {
                CharacterId    = player.Id,
                EnemyName      = $"{EnemyPrefixes[rng.Next(EnemyPrefixes.Length)]} {EnemySuffixes[enemyClass][rng.Next(EnemySuffixes[enemyClass].Length)]}",
                EnemyClass     = enemyClass,
                EnemyLevel     = player.Level,
                EnemyMaxLife   = stats.life   + scale * 3,
                EnemyCurrentLife = stats.life + scale * 3,
                EnemyDamage    = stats.damage + scale,
                PlayerCurrentLife = player.Life,
                Status         = "Active",
                CreatedAt      = DateTime.UtcNow
            };

            _db.Battles.Add(battle);
            await _db.SaveChangesAsync();
            return battle;
        }

        // ── Consultas ─────────────────────────────────────────────────────

        public async Task<Battle?> GetBattleWithLogsAsync(int battleId) =>
            await _db.Battles
                .Include(b => b.Logs.OrderBy(l => l.Turn))
                .FirstOrDefaultAsync(b => b.Id == battleId);

        public async Task<Battle?> GetActiveBattleAsync(int characterId) =>
            await _db.Battles
                .FirstOrDefaultAsync(b => b.CharacterId == characterId && b.Status == "Active");

        // ── Executar Turno ────────────────────────────────────────────────
        //
        // Ordem:
        //   1. Jogador age (ou pula se atordoado)
        //   2. Verifica se inimigo morreu → VITÓRIA
        //   3. Inimigo age (ou pula se atordoado)
        //   4. Verifica se jogador morreu → DERROTA
        //
        // Atordoamento: sempre afeta o PRÓXIMO turno de quem foi atingido,
        // porque o atordoamento é aplicado no final da fase de quem stun,
        // e consumido/zerado no início da fase da vítima no turno seguinte.

        public async Task ExecuteTurnAsync(Battle battle, Character player, Skill skill)
        {
            var rng  = new Random();
            var log  = new List<string>();
            battle.TurnCount++;

            log.Add($"=== Turno {battle.TurnCount} ===");

            // ── 1. Fase do Jogador ────────────────────────────────────────
            if (battle.PlayerStunned)
            {
                log.Add($"[ATORDOADO] {player.Name} não consegue agir neste turno.");
                battle.PlayerStunned = false;
            }
            else
            {
                int dmg = CalcDamage(
                    skill.BaseDamage, player.Damage,
                    player.Class, battle.EnemyClass,
                    skill.EffectType, rng,
                    out string tags);

                battle.EnemyCurrentLife = Math.Max(0, battle.EnemyCurrentLife - dmg);

                // Efeitos do jogador
                if (skill.EffectType == "Heal")
                {
                    int healed = Math.Min(skill.EffectValue, player.Life - battle.PlayerCurrentLife);
                    battle.PlayerCurrentLife += healed;
                    tags += $" [CURA +{healed} HP]";
                }

                if (skill.EffectType == "Stun" && rng.NextDouble() < 0.30)
                {
                    battle.EnemyStunned = true;
                    tags += " [ATORDOOU!]";
                }

                string advText = AdvantageLabel(player.Class, battle.EnemyClass);
                log.Add($"ATAQUE: {player.Name} usa '{skill.Name}'{advText}");
                log.Add($"  Dano causado: {dmg}{tags}");
                log.Add($"  Inimigo: {battle.EnemyCurrentLife}/{battle.EnemyMaxLife} HP");
            }

            // ── 2. Checar morte do inimigo ────────────────────────────────
            if (battle.EnemyCurrentLife <= 0)
            {
                battle.Status = "Won";
                player.Wins++;
                player.Level++;
                log.Add($"");
                log.Add($"VITORIA! {battle.EnemyName} foi derrotado!");
                log.Add($"{player.Name} subiu para o nivel {player.Level}!");
            }
            else
            {
                // ── 3. Fase do Inimigo ────────────────────────────────────
                if (battle.EnemyStunned)
                {
                    log.Add($"[ATORDOADO] {battle.EnemyName} nao consegue agir neste turno.");
                    battle.EnemyStunned = false;
                }
                else
                {
                    var moves  = EnemyMoves[battle.EnemyClass];
                    var move   = moves[rng.Next(moves.Length)];
                    float adv  = AdvantageMultiplier(battle.EnemyClass, player.Class);
                    int eDmg   = Math.Max(1, (int)((move.dmg + battle.EnemyDamage) * adv));

                    battle.PlayerCurrentLife = Math.Max(0, battle.PlayerCurrentLife - eDmg);

                    bool stuns  = rng.NextDouble() < 0.10;
                    string eFx  = stuns ? " [ATORDOOU!]" : "";
                    if (stuns) battle.PlayerStunned = true;

                    string eAdv = AdvantageLabel(battle.EnemyClass, player.Class);
                    log.Add($"ATAQUE: {battle.EnemyName} usa '{move.name}'{eAdv}");
                    log.Add($"  Dano causado: {eDmg}{eFx}");
                    log.Add($"  {player.Name}: {battle.PlayerCurrentLife}/{player.Life} HP");
                }

                // ── 4. Checar morte do jogador ────────────────────────────
                if (battle.PlayerCurrentLife <= 0)
                {
                    battle.Status = "Lost";
                    player.Losses++;
                    log.Add($"");
                    log.Add($"DERROTA. {player.Name} foi derrotado por {battle.EnemyName}.");
                }
            }

            _db.BattleLogs.Add(new BattleLog
            {
                BattleId      = battle.Id,
                Turn          = battle.TurnCount,
                Description   = string.Join("\n", log),
                PlayerHpAfter = battle.PlayerCurrentLife,
                EnemyHpAfter  = battle.EnemyCurrentLife
            });

            _db.Battles.Update(battle);
            _db.Characters.Update(player);
            await _db.SaveChangesAsync();
        }

        // ── Recompensa ────────────────────────────────────────────────────

        public async Task ClaimRewardAsync(Battle battle, Character player, string choice)
        {
            switch (choice)
            {
                case "Life":
                    player.Life += 2;
                    break;

                case "Damage":
                    player.Damage += 1;
                    break;

                case "Skill":
                    var ownedIds = await _db.CharacterSkills
                        .Where(cs => cs.CharacterId == player.Id)
                        .Select(cs => cs.SkillId)
                        .ToListAsync();

                    var newSkill = await _db.Skills
                        .Where(s => (s.ClassName == player.Class || s.ClassName == "Any")
                                    && !ownedIds.Contains(s.Id))
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefaultAsync();

                    if (newSkill != null)
                    {
                        var slots = await _db.CharacterSkills
                            .Include(cs => cs.Skill)
                            .Where(cs => cs.CharacterId == player.Id)
                            .OrderBy(cs => cs.Skill.BaseDamage)
                            .ToListAsync();

                        if (slots.Count >= 4)
                        {
                            var weakest    = slots.First();
                            weakest.SkillId = newSkill.Id;
                            _db.CharacterSkills.Update(weakest);
                        }
                        else
                        {
                            _db.CharacterSkills.Add(new CharacterSkill
                            {
                                CharacterId = player.Id,
                                SkillId     = newSkill.Id,
                                Slot        = slots.Count + 1
                            });
                        }
                    }
                    break;
            }

            battle.RewardClaimed = true;
            _db.Battles.Update(battle);
            _db.Characters.Update(player);
            await _db.SaveChangesAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static float AdvantageMultiplier(string atk, string def)
        {
            if ((atk == "Warrior" && def == "Mage")    ||
                (atk == "Mage"    && def == "Archer")  ||
                (atk == "Archer"  && def == "Warrior")) return 1.2f;

            if ((atk == "Mage"    && def == "Warrior") ||
                (atk == "Archer"  && def == "Mage")    ||
                (atk == "Warrior" && def == "Archer"))  return 0.8f;

            return 1.0f;
        }

        private static string AdvantageLabel(string atk, string def)
        {
            float m = AdvantageMultiplier(atk, def);
            return m > 1f ? " (VANTAGEM)" : m < 1f ? " (DESVANTAGEM)" : "";
        }

        private static int CalcDamage(int baseDmg, int bonus, string atk, string def,
                                       string effectType, Random rng, out string tags)
        {
            float adv  = AdvantageMultiplier(atk, def);
            int   total = (int)((baseDmg + bonus) * adv);
            tags = "";

            if (effectType == "Crit" && rng.NextDouble() < 0.50)
            {
                total = (int)(total * 1.5);
                tags += " [CRITICO! x1.5]";
            }

            return Math.Max(1, total);
        }
    }
}
