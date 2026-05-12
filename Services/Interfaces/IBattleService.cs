using FordEnterRPG.Models;

namespace FordEnterRPG.Services
{
    public interface IBattleService
    {
        Task<Battle> StartBattleAsync(Character player);
        Task<Battle?> GetBattleWithLogsAsync(int battleId);
        Task<Battle?> GetActiveBattleAsync(int characterId);
        Task ExecuteTurnAsync(Battle battle, Character player, Skill skill);
        Task ClaimRewardAsync(Battle battle, Character player, string choice);
    }
}
