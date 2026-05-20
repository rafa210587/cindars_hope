namespace CindarsHope.Core.Events
{
    public readonly struct CaveLevelEnteredEvent
    {
        public readonly int CaveLevel;
        public readonly string BiomeId;
        public readonly string CaveRunSeed;

        public CaveLevelEnteredEvent(int caveLevel, string biomeId, string caveRunSeed)
        {
            CaveLevel = caveLevel;
            BiomeId = biomeId ?? string.Empty;
            CaveRunSeed = caveRunSeed ?? string.Empty;
        }
    }
}
