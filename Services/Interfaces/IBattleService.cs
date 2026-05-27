using FordEnterRPG.Models;

namespace FordEnterRPG.Services
{
    public interface IBattleService
    {
        Task<Battle> StartBattleAsync(Character player);
        Task<Battle?> GetBattleWithLogsAsync(int battleId);
        Task<Battle?> GetActiveBattleAsync(int characterId);
        Task ExecuteTurnAsync(Battle battle, Character player, Skill skill, bool? clientIsHit = null);
        Task<(Skill? replaced, Skill? acquired)> ClaimRewardAsync(Battle battle, Character player, string choice, string? rarity = null);
    }
}
