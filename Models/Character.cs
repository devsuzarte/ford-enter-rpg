namespace FordEnterRPG.Models
{
    public class Character
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty; // Warrior, Mage, Archer
        public int Life { get; set; }    // max HP, increases with rewards
        public int Damage { get; set; }  // bonus damage, increases with rewards
        public int Level { get; set; } = 1;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
        public ICollection<Battle> Battles { get; set; } = new List<Battle>();
    }
}
