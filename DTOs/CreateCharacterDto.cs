namespace FordEnterRPG.DTOs
{
    public class CreateCharacterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty; // Warrior, Mage, Archer
    }
}
