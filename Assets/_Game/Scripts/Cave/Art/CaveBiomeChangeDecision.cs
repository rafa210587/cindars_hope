namespace CindarsHope.Cave.Art
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01) — decisão PURA de "a banda de bioma mudou desde a
    /// última entrada de nível?". Extraída do CaveLevelRuntimeController para ser 100% testável em
    /// EditMode sem MonoBehaviour/cena. Usa -1 como sentinela de "nenhuma banda publicada ainda" —
    /// a primeira entrada da run sempre conta como mudança (critério 14.4 da spec).
    /// </summary>
    public static class CaveBiomeChangeDecision
    {
        public const int NoPreviousBand = -1;

        /// <summary>True se currentBandId difere de previousBandId (incl. previousBandId == NoPreviousBand).</summary>
        public static bool HasBandChanged(int previousBandId, int currentBandId)
        {
            return previousBandId != currentBandId;
        }
    }
}
