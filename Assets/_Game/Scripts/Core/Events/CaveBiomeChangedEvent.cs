namespace CindarsHope.Core.Events
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01) — publicado quando a BANDA de bioma da caverna
    /// muda entre uma entrada de nível e a anterior (incl. a primeira entrada da run, que sempre
    /// conta como mudança). Consumidor futuro: sistema de música por bioma (musicTrackId do
    /// CaveBiomeArtProfileSO); nenhum subscriber nesta spec. Não confundir com CaveLevelEnteredEvent
    /// (publicado em toda entrada de nível, mudando banda ou não).
    /// </summary>
    public readonly struct CaveBiomeChangedEvent
    {
        public readonly string PreviousBiomeId;
        public readonly string BiomeId;
        public readonly int BandId;
        public readonly int CaveLevel;

        public CaveBiomeChangedEvent(string previousBiomeId, string biomeId, int bandId, int caveLevel)
        {
            PreviousBiomeId = previousBiomeId ?? string.Empty;
            BiomeId = biomeId ?? string.Empty;
            BandId = bandId;
            CaveLevel = caveLevel;
        }
    }
}
