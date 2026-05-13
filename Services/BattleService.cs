using FordEnterRPG.Data;
using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Services
{
    public class BattleService : IBattleService
    {
        private static readonly Dictionary<string, (int life, int damage)> EnemyBaseStats = new()
        {
            { "Warrior", (22, 6) },
            { "Mage",    (16, 9) },
            { "Archer",  (18, 7) }
        };

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

        public async Task<Battle> StartBattleAsync(Character player)
        {
            var rng = new Random();
            var classes = new[] { "Warrior", "Mage", "Archer" };
            var enemyClass = classes[rng.Next(classes.Length)];
            var stats = EnemyBaseStats[enemyClass];
            int scale = player.Level - 1;

            int seq = await _db.Battles.CountAsync(b => b.CharacterId == player.Id) + 1;

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
                RunSequence    = seq,
                CreatedAt      = DateTime.UtcNow
            };

            _db.Battles.Add(battle);
            await _db.SaveChangesAsync();
            return battle;
        }

        public async Task<Battle?> GetBattleWithLogsAsync(int battleId) =>
            await _db.Battles
                .Include(b => b.Logs.OrderBy(l => l.Turn))
                .FirstOrDefaultAsync(b => b.Id == battleId);

        public async Task<Battle?> GetActiveBattleAsync(int characterId) =>
            await _db.Battles
                .FirstOrDefaultAsync(b => b.CharacterId == characterId && b.Status == "Active");

        public async Task ExecuteTurnAsync(Battle battle, Character player, Skill skill)
        {
            var rng  = new Random();
            var log  = new List<string>();
            battle.TurnCount++;

            log.Add($"=== Turno {battle.TurnCount} ===");

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

                if (battle.PlayerCurrentLife <= 0)
                {
                    battle.Status = "Lost";
                    player.Losses++;
                    player.IsDead = true;
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

        public async Task<(Skill? replaced, Skill? acquired)> ClaimRewardAsync(Battle battle, Character player, string choice)
        {
            Skill? replaced = null;
            Skill? acquired = null;

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

                    string counterClass = player.Class switch
                    {
                        "Warrior" => "Archer",
                        "Mage"    => "Warrior",
                        "Archer"  => "Mage",
                        _         => ""
                    };

                    int crossCount = await _db.CharacterSkills
                        .Include(cs => cs.Skill)
                        .Where(cs => cs.CharacterId == player.Id
                                  && cs.Skill.ClassName == counterClass)
                        .CountAsync();

                    var candidates = await _db.Skills
                        .Where(s => !ownedIds.Contains(s.Id)
                                 && (s.ClassName == player.Class
                                  || s.ClassName == "Any"
                                  || (s.ClassName == counterClass && crossCount < 2)))
                        .Select(s => new { s.Id, s.Rarity })
                        .ToListAsync();

                    Skill? newSkill = null;
                    if (candidates.Count > 0)
                    {
                        var weighted = candidates
                            .SelectMany(c => Enumerable.Repeat(c.Id, c.Rarity switch
                            {
                                "Epic" => 4,
                                "Rare" => 2,
                                _      => 1
                            }))
                            .ToList();

                        int pickedId = weighted[Random.Shared.Next(weighted.Count)];
                        newSkill = await _db.Skills.FindAsync(pickedId);
                    }

                    if (newSkill != null)
                    {
                        acquired = newSkill;
                        var slots = await _db.CharacterSkills
                            .Include(cs => cs.Skill)
                            .Where(cs => cs.CharacterId == player.Id)
                            .OrderBy(cs => cs.Skill.BaseDamage)
                            .ToListAsync();

                        if (slots.Count >= 4)
                        {
                            var weakest  = slots.First();
                            replaced     = weakest.Skill;
                            int keptSlot = weakest.Slot;
                            _db.CharacterSkills.Remove(weakest);
                            await _db.SaveChangesAsync();
                            _db.CharacterSkills.Add(new CharacterSkill
                            {
                                CharacterId = player.Id,
                                SkillId     = newSkill.Id,
                                Slot        = keptSlot
                            });
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

            return (replaced, acquired);
        }

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
