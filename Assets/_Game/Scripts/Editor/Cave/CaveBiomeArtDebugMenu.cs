using CindarsHope.Cave.Art;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Cave
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01), T011 — ferramenta Dev/diagnóstico (carve-out da
    /// rule editor-generation-orchestration: submenu Dev/ não é geração canônica, não passa por
    /// InicializarProjeto/ValidarProjeto/RepararEReconstruir). Liga/desliga o toggle dev-only que
    /// força a resolução de ARTE da caverna para a banda 1 (Caverna de Pedra) em qualquer nível, para
    /// testar o pipeline visual de ponta a ponta sem descer 10 níveis reais. Nunca afeta
    /// layout/spawn/loot/snapshot — ver CaveBiomeArtDebug.
    /// </summary>
    public static class CaveBiomeArtDebugMenu
    {
        private const int ForcedTestBandId = 1;

        [MenuItem("CindarsHope/Dev/Cave/Forcar Banda De Arte (Bioma 1)")]
        public static void ForceBand1()
        {
            CaveBiomeArtDebug.ForcedBandId = ForcedTestBandId;
            Debug.Log($"[CaveBiomeArtDebug] ForcedBandId = {ForcedTestBandId} (Caverna de Pedra). Todos os níveis resolvem arte da banda 1 até desligar.");
        }

        [MenuItem("CindarsHope/Dev/Cave/Desligar Banda De Arte Forcada")]
        public static void ClearForcedBand()
        {
            CaveBiomeArtDebug.ForcedBandId = null;
            Debug.Log("[CaveBiomeArtDebug] ForcedBandId = null. Resolução de arte voltou ao normal (CaveBandScaling.BandForLevel).");
        }
    }
}
