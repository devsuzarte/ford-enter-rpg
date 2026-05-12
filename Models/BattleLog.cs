namespace FordEnterRPG.Models
{
    public class BattleLog
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public Battle Battle { get; set; } = null!;
        public int Turn { get; set; }
        public string Description { get; set; } = string.Empty;
        public int PlayerHpAfter { get; set; }
        public int EnemyHpAfter { get; set; }
    }
}
