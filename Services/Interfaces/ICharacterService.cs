using FordEnterRPG.DTOs;
using FordEnterRPG.Models;

namespace FordEnterRPG.Services
{
    public interface ICharacterService
    {
        Task<Character?> CreateAsync(int userId, CreateCharacterDto dto);
        Task<Character?> GetByUserIdAsync(int userId);
        Task<Character?> GetByIdWithSkillsAsync(int characterId);
        Task ResetAsync(int userId);
    }
}
