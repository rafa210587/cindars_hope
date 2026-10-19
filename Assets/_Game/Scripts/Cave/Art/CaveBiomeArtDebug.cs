namespace CindarsHope.Cave.Art
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01), T011 — FATIA DE TESTE autorizada pelo humano
    /// (2026-07-03): toggle dev-only para forçar a banda de arte em todos os níveis, testando o
    /// pipeline visual (profile da Caverna de Pedra = banda 1) sem esperar descer 10 níveis reais.
    ///
    /// Afeta SOMENTE a resolução de ARTE (qual CaveBiomeArtProfileSO o CaveRuntimeMaterializer
    /// consulta) — NUNCA layout, spawn, loot ou snapshot, que continuam usando
    /// CaveBandScaling.BandForLevel(caveLevel) normalmente (cave-stable-run / ADR-0005 intacto).
    ///
    /// Default OFF (double-guard, mesmo padrão do CaveDebugLevelSkipController / spec_codex_11):
    /// mesmo com o build guard, ForcedBandId começa null e só é setado manualmente via
    /// CindarsHope/Dev/Cave/Forcar Banda De Arte (Bioma 1) no Editor.
    /// </summary>
    public static class CaveBiomeArtDebug
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Banda forçada para resolução de ARTE (1..7), ou null = usar CaveBandScaling.BandForLevel normalmente.</summary>
        public static int? ForcedBandId { get; set; } = null;
#else
        // Build de produção: propriedade inerte, sempre null — nunca compilada com valor setável.
        public static int? ForcedBandId => null;
#endif

        /// <summary>Resolve a banda de ARTE a usar: ForcedBandId (se setado, só em Editor/Dev build) ou naturalBandId.</summary>
        public static int ResolveBandForArt(int naturalBandId)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return ForcedBandId.HasValue ? ForcedBandId.Value : naturalBandId;
#else
            return naturalBandId;
#endif
        }
    }
}
