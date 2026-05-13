namespace FordEnterRPG.ViewModels
{
    public class TurnEvent
    {
        public string   Type      { get; init; } = "";  // attack | stun | win | lose | levelup
        public string   Actor     { get; init; } = "";
        public bool     IsPlayer  { get; init; }
        public string   Skill     { get; init; } = "";
        public int      Damage    { get; init; }
        public string[] Tags      { get; init; } = Array.Empty<string>();
        public string   Advantage { get; init; } = "";  // vantagem | desvantagem | ""
        public string   Message   { get; init; } = "";
    }
}
