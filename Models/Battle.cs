namespace FordEnterRPG.Models
{
    public class Battle
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        // Enemy snapshot (generated at battle start)
        public string EnemyName { get; set; } = string.Empty;
        public string EnemyClass { get; set; } = string.Empty;
        public int EnemyMaxLife { get; set; }
        public int EnemyCurrentLife { get; set; }
        public int EnemyDamage { get; set; }
        public int EnemyLevel { get; set; }
        public bool EnemyStunned { get; set; } = false;

        // Player HP tracked here (resets each battle)
        public int PlayerCurrentLife { get; set; }
        public bool PlayerStunned { get; set; } = false;

        public string Status { get; set; } = "Active"; // Active, Won, Lost
        public bool RewardClaimed { get; set; } = false;
        public int TurnCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BattleLog> Logs { get; set; } = new List<BattleLog>();
    }
}
