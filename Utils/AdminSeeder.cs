using FordEnterRPG.Data;
using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Utils
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            // Admin user
            if (!await context.Users.AnyAsync(u => u.Email == "admin@admin.com"))
            {
                context.Users.Add(new User
                {
                    Name = "Admin",
                    Email = "admin@admin.com",
                    PasswordHash = "admin",
                    Role = "Admin"
                });
                await context.SaveChangesAsync();
            }

            // Skills
            if (!await context.Skills.AnyAsync())
            {
                var skills = new List<Skill>
                {
                    // ── Guerreiro ──
                    new() { Name = "Corte",               ClassName = "Warrior", Rarity = "Common", BaseDamage = 6,  EffectType = "None", Description = "Um corte certeiro com a espada." },
                    new() { Name = "Golpe Pesado",         ClassName = "Warrior", Rarity = "Common", BaseDamage = 8,  EffectType = "None", Description = "Um golpe devastador que abala o inimigo." },
                    new() { Name = "Grito de Batalha",     ClassName = "Warrior", Rarity = "Common", BaseDamage = 4,  EffectType = "Heal", EffectValue = 5, Description = "Inspira-se e recupera 5 de vida." },
                    new() { Name = "Bloquear e Contra",    ClassName = "Warrior", Rarity = "Rare",   BaseDamage = 5,  EffectType = "Heal", EffectValue = 4, Description = "Bloqueia o ataque e responde com força." },
                    new() { Name = "Fúria Berserker",      ClassName = "Warrior", Rarity = "Rare",   BaseDamage = 10, EffectType = "Crit", Description = "Ataque frenético com 50% de chance de crítico." },
                    new() { Name = "Investida Brutal",     ClassName = "Warrior", Rarity = "Rare",   BaseDamage = 9,  EffectType = "Stun", Description = "Avança com força total, podendo atordoar." },
                    new() { Name = "Redemoinho",           ClassName = "Warrior", Rarity = "Epic",   BaseDamage = 12, EffectType = "None", Description = "Gira atacando tudo ao redor com força máxima." },
                    new() { Name = "Esmagar de Titã",      ClassName = "Warrior", Rarity = "Epic",   BaseDamage = 13, EffectType = "Stun", Description = "Impacto tão poderoso que atordoa o inimigo." },

                    // ── Mago ──
                    new() { Name = "Bola de Fogo",         ClassName = "Mage",    Rarity = "Common", BaseDamage = 7,  EffectType = "None", Description = "Uma esfera ardente lançada ao inimigo." },
                    new() { Name = "Fragmento de Gelo",    ClassName = "Mage",    Rarity = "Common", BaseDamage = 5,  EffectType = "Stun", Description = "Projétil de gelo que pode atordoar." },
                    new() { Name = "Míssil Mágico",        ClassName = "Mage",    Rarity = "Common", BaseDamage = 6,  EffectType = "None", Description = "Energia arcana concentrada e disparada." },
                    new() { Name = "Raio em Cadeia",       ClassName = "Mage",    Rarity = "Rare",   BaseDamage = 9,  EffectType = "None", Description = "Raio que salta entre vítimas causando grande dano." },
                    new() { Name = "Drenar Vida",          ClassName = "Mage",    Rarity = "Rare",   BaseDamage = 6,  EffectType = "Heal", EffectValue = 4, Description = "Absorve a força vital do inimigo." },
                    new() { Name = "Surto Arcano",         ClassName = "Mage",    Rarity = "Rare",   BaseDamage = 8,  EffectType = "Crit", Description = "Explosão mágica com 50% de chance de crítico." },
                    new() { Name = "Meteoro",              ClassName = "Mage",    Rarity = "Epic",   BaseDamage = 14, EffectType = "None", Description = "Um meteoro cai do céu sobre o inimigo." },
                    new() { Name = "Buraco Negro",         ClassName = "Mage",    Rarity = "Epic",   BaseDamage = 11, EffectType = "Stun", Description = "Abre uma singularidade que paralisa o alvo." },

                    // ── Arqueiro ──
                    new() { Name = "Tiro Rápido",          ClassName = "Archer",  Rarity = "Common", BaseDamage = 5,  EffectType = "None", Description = "Um disparo veloz e preciso." },
                    new() { Name = "Flecha Perfurante",    ClassName = "Archer",  Rarity = "Common", BaseDamage = 7,  EffectType = "None", Description = "Flecha que atravessa armaduras." },
                    new() { Name = "Flecha Venenosa",      ClassName = "Archer",  Rarity = "Common", BaseDamage = 4,  EffectType = "Stun", Description = "Toxina paralisante que pode atordoar." },
                    new() { Name = "Tiro Inabilitante",    ClassName = "Archer",  Rarity = "Rare",   BaseDamage = 6,  EffectType = "Stun", Description = "Acerta um ponto vital, atordoando o alvo." },
                    new() { Name = "Tiro Duplo",           ClassName = "Archer",  Rarity = "Rare",   BaseDamage = 8,  EffectType = "Crit", Description = "Dois disparos simultâneos com chance de crítico." },
                    new() { Name = "Foco do Caçador",      ClassName = "Archer",  Rarity = "Rare",   BaseDamage = 5,  EffectType = "Heal", EffectValue = 3, Description = "Concentração plena que restaura energia vital." },
                    new() { Name = "Chuva de Flechas",     ClassName = "Archer",  Rarity = "Epic",   BaseDamage = 12, EffectType = "None", Description = "Dezenas de flechas cobrem o céu." },
                    new() { Name = "Marca da Morte",       ClassName = "Archer",  Rarity = "Epic",   BaseDamage = 13, EffectType = "Crit", Description = "A flecha definitiva que sempre acerta em cheio." },

                    // ── Qualquer classe ──
                    new() { Name = "Garra das Sombras",    ClassName = "Any",     Rarity = "Rare",   BaseDamage = 7,  EffectType = "None", Description = "Uma garra sombria que transcende qualquer classe." },
                    new() { Name = "Explosão Elemental",   ClassName = "Any",     Rarity = "Epic",   BaseDamage = 11, EffectType = "Crit", Description = "Energia pura sem vínculo com nenhuma classe." },
                };

                context.Skills.AddRange(skills);
                await context.SaveChangesAsync();
            }
        }
    }
}
