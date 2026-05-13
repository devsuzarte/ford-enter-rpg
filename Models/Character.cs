namespace FordEnterRPG.Models
{
    public class Character
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public int Life { get; set; }
        public int Damage { get; set; }
        public int Level { get; set; } = 1;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public bool IsDead { get; set; } = false;
        public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
        public ICollection<Battle> Battles { get; set; } = new List<Battle>();
    }
}
