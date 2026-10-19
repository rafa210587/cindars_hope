namespace CindarsHope.NPC
{
    // NPC stats using project-specific attributes.
    // Forbidden fields: Folego, Breath, BR — these do not exist as NPC stats.
    public class NpcStats
    {
        public int HP { get; set; } = 10;
        public int MP { get; set; } = 0;
        public int Stamina { get; set; } = 10;
        public int Forca { get; set; } = 5;
        public int Constituicao { get; set; } = 5;
        public int Destreza { get; set; } = 5;
        public int Inteligencia { get; set; } = 5;
        public int Vontade { get; set; } = 5;
        public int Carisma { get; set; } = 5;
        // No Folego/Breath/BR fields — intentionally absent per canon
    }
}
