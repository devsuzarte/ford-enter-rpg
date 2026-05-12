namespace FordEnterRPG.Models
{
    public class CharacterSkill
    {
        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;
        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;
        public int Slot { get; set; } // 1-4
    }
}
