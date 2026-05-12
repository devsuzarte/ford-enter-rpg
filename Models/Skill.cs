namespace FordEnterRPG.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty; // Warrior, Mage, Archer, Any
        public string Rarity { get; set; } = string.Empty;    // Common, Rare, Epic
        public int BaseDamage { get; set; }
        public string EffectType { get; set; } = "None";      // None, Crit, Stun, Heal
        public int EffectValue { get; set; } = 0;             // HP restored (Heal) or unused
        public string Description { get; set; } = string.Empty;
    }
}
