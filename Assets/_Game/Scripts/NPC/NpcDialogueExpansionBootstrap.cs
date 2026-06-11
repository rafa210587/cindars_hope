using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Bootstrap that logs dialogue coverage confirmation at runtime.
    /// All 23 canonical NPC dialogue sets are pre-implemented via DialogueTreeSO assets (WAVE12C).
    /// This bootstrap validates and documents the coverage — no new dialogue content is added here.
    /// </summary>
    public static class NpcDialogueExpansionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            var total = NpcDialogueSetRegistry.TotalNpcsWithDialogue;
            var withTenPlus = NpcDialogueSetRegistry.NpcsWithAtLeast10Nodes;
            Debug.Log($"[NpcDialogueExpansionBootstrap] NPC dialogue coverage: {total} NPCs registered, {withTenPlus} with >=10 nodes.");

            if (withTenPlus < 5)
            {
                Debug.LogWarning($"[NpcDialogueExpansionBootstrap] Expected >=5 NPCs with >=10 dialogue nodes. Found {withTenPlus}. WAVE25 dialogue coverage check: FAIL.");
            }

            var rosterCount = NpcTownRosterRegistry.CanonicalCount;
            Debug.Log($"[NpcDialogueExpansionBootstrap] Town roster: {rosterCount} canonical NPCs ({NpcTownRosterRegistry.MvpCount} MVP tier).");
        }
    }
}
