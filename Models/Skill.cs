namespace FordEnterRPG.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public int BaseDamage { get; set; }
        public string EffectType { get; set; } = "None";
        public int EffectValue { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}
