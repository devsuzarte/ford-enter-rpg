using FordEnterRPG.Data;
using FordEnterRPG.DTOs;
using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Services
{
    public class CharacterService : ICharacterService
    {
        private static readonly Dictionary<string, (int life, int damage)> BaseStats = new()
        {
            { "Warrior", (25, 7) },
            { "Mage",    (18, 10) },
            { "Archer",  (20, 8) }
        };

        private readonly AppDbContext _db;
        public CharacterService(AppDbContext db) => _db = db;

        public async Task<Character?> CreateAsync(int userId, CreateCharacterDto dto)
        {
            if (!BaseStats.TryGetValue(dto.Class, out var stats)) return null;

            var character = new Character
            {
                UserId = userId,
                Name = dto.Name,
                Class = dto.Class,
                Life = stats.life,
                Damage = stats.damage,
                Level = 1
            };
            _db.Characters.Add(character);
            await _db.SaveChangesAsync();

            // 3 common skills from class + 1 random of any rarity
            var rng = new Random();
            var common = await _db.Skills
                .Where(s => s.ClassName == dto.Class && s.Rarity == "Common")
                .OrderBy(_ => Guid.NewGuid()).Take(3).ToListAsync();

            var rareOrEpic = await _db.Skills
                .Where(s => s.ClassName == dto.Class && s.Rarity != "Common")
                .OrderBy(_ => Guid.NewGuid()).Take(1).ToListAsync();

            var starting = common.Concat(rareOrEpic).Take(4).ToList();

            for (int i = 0; i < starting.Count; i++)
                _db.CharacterSkills.Add(new CharacterSkill { CharacterId = character.Id, SkillId = starting[i].Id, Slot = i + 1 });

            await _db.SaveChangesAsync();
            return character;
        }

        public async Task<Character?> GetByUserIdAsync(int userId) =>
            await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Character?> GetByIdWithSkillsAsync(int characterId) =>
            await _db.Characters
                .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.Id == characterId);
    }
}
