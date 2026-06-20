namespace CindarsHope.World.Fishing
{
    /// <summary>
    /// fable_50 — função PURA que escolhe o <c>tableId</c> de pesca de um lago de caverna a partir da
    /// banda (nível) e do bioma do nível. NÃO toca o snapshot nem o hook 10%/máx.1
    /// (CaveSnapshotService.CreateFishingSpotHook é INTOCADO e segue dono da existência do spot);
    /// esta classe só responde "qual tabela este spot consulta", de forma determinística e testável.
    ///
    /// Bandas aquáticas canônicas (CAVE_BESTIARY):
    /// - Lake Lurker 5-9  → banda baixa (fish_pale)      ⇒ cave_lake_band_1_10
    /// - Mirrorfin 27-34  → banda de gelo (mirrorfin)    ⇒ cave_lake_band_26_40
    /// - Moonless Pool    → sala especial (flag dedicada) ⇒ moonless_pool
    /// </summary>
    public static class CaveFishingTableSelector
    {
        public const string TableFarmPond = "fishing_farm_pond";
        public const string TableFarmPondWinter = "fishing_farm_pond_winter";
        public const string TableCaveBand1To10 = "fishing_cave_lake_band_1_10";
        public const string TableCaveBand26To40 = "fishing_cave_lake_band_26_40";
        public const string TableMoonlessPool = "fishing_moonless_pool";

        /// <summary>
        /// Resolve o tableId pelo nível e bioma. <paramref name="isMoonlessPool"/> tem prioridade
        /// (sala especial). Níveis altos (>=26) usam a tabela de gelo; o resto, a banda baixa.
        /// </summary>
        public static string ResolveTableId(int caveLevel, string biomeId, bool isMoonlessPool = false)
        {
            if (isMoonlessPool)
            {
                return TableMoonlessPool;
            }

            var biome = (biomeId ?? string.Empty).Trim().ToLowerInvariant();
            if (biome.Contains("moonless"))
            {
                return TableMoonlessPool;
            }

            // Banda de gelo (mirrorfin 27-34) e mais fundas que ela.
            if (caveLevel >= 26 || biome.Contains("ice") || biome.Contains("gelo") || biome.Contains("frost"))
            {
                return TableCaveBand26To40;
            }

            return TableCaveBand1To10;
        }
    }
}
