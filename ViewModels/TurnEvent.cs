namespace FordEnterRPG.ViewModels
{
    public class TurnEvent
    {
        public string   Type      { get; init; } = "";
        public string   Actor     { get; init; } = "";
        public bool     IsPlayer  { get; init; }
        public string   Skill     { get; init; } = "";
        public int      Damage    { get; init; }
        public string[] Tags      { get; init; } = Array.Empty<string>();
        public string   Advantage { get; init; } = "";
        public string   Message   { get; init; } = "";
    }
}
