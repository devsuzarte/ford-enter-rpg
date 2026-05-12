using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Character> Characters { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<CharacterSkill> CharacterSkills { get; set; } = null!;
        public DbSet<Battle> Battles { get; set; } = null!;
        public DbSet<BattleLog> BattleLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Name).IsRequired();
                e.Property(u => u.Email).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).IsRequired(false);
            });

            modelBuilder.Entity<Character>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
                e.Property(c => c.Class).IsRequired().HasMaxLength(20);
                e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Skill>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Name).IsRequired().HasMaxLength(100);
                e.Property(s => s.ClassName).IsRequired().HasMaxLength(20);
                e.Property(s => s.Rarity).IsRequired().HasMaxLength(20);
                e.Property(s => s.EffectType).IsRequired().HasMaxLength(20).HasDefaultValue("None");
                e.Property(s => s.Description).IsRequired();
            });

            modelBuilder.Entity<CharacterSkill>(e =>
            {
                e.HasKey(cs => new { cs.CharacterId, cs.SkillId });
                e.HasOne(cs => cs.Character).WithMany(c => c.CharacterSkills).HasForeignKey(cs => cs.CharacterId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cs => cs.Skill).WithMany().HasForeignKey(cs => cs.SkillId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Battle>(e =>
            {
                e.HasKey(b => b.Id);
                e.Property(b => b.EnemyName).IsRequired().HasMaxLength(100);
                e.Property(b => b.EnemyClass).IsRequired().HasMaxLength(20);
                e.Property(b => b.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                e.HasOne(b => b.Character).WithMany(c => c.Battles).HasForeignKey(b => b.CharacterId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BattleLog>(e =>
            {
                e.HasKey(l => l.Id);
                e.Property(l => l.Description).IsRequired();
                e.HasOne(l => l.Battle).WithMany(b => b.Logs).HasForeignKey(l => l.BattleId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
