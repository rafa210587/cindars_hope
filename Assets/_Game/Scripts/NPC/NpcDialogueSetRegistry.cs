using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Documents dialogue coverage per NPC.
    /// All 23 canonical NPCs have DialogueTreeSO assets with >=10 nodes (implemented in WAVE12C).
    /// This registry provides runtime-accessible coverage data for validators.
    /// Source: WAVE_INTEGRATION_12C_REFINED_NPC_DIALOGUE_SETS.md
    /// </summary>
    public static class NpcDialogueSetRegistry
    {
        public sealed class NpcDialogueEntry
        {
            public string NpcId;
            public string DialogueSetId;
            public int NodeCount;
            public bool HasGreeting;
            public bool HasRole;
            public bool HasService;
            public bool HasTownContext;
            public bool HasGoodbye;
        }

        private static readonly List<NpcDialogueEntry> s_entries = new List<NpcDialogueEntry>
        {
            new NpcDialogueEntry { NpcId = "npc_pip", DialogueSetId = "dialogue_pip", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_sylveth", DialogueSetId = "dialogue_sylveth", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_brumdar", DialogueSetId = "dialogue_brumdar", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_renko", DialogueSetId = "dialogue_renko", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_thalindra", DialogueSetId = "dialogue_thalindra", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_zrix", DialogueSetId = "dialogue_zrix", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_nimble", DialogueSetId = "dialogue_nimble", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_corvus", DialogueSetId = "dialogue_corvus", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_mara", DialogueSetId = "dialogue_mara", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_gurd", DialogueSetId = "dialogue_gurd", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_hund", DialogueSetId = "dialogue_hund", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_ozzra", DialogueSetId = "dialogue_ozzra", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_gruta", DialogueSetId = "dialogue_gruta", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_yael", DialogueSetId = "dialogue_yael", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_dagna", DialogueSetId = "dialogue_dagna", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_alaric", DialogueSetId = "dialogue_alaric", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = false, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_mirela", DialogueSetId = "dialogue_mirela", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_eiran", DialogueSetId = "dialogue_eiran", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_liora", DialogueSetId = "dialogue_liora", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = false, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_orlan", DialogueSetId = "dialogue_orlan", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_savra", DialogueSetId = "dialogue_savra", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_tovin", DialogueSetId = "dialogue_tovin", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = true, HasTownContext = true, HasGoodbye = true },
            new NpcDialogueEntry { NpcId = "npc_maelor", DialogueSetId = "dialogue_maelor", NodeCount = 10, HasGreeting = true, HasRole = true, HasService = false, HasTownContext = true, HasGoodbye = true }
        };

        public static IReadOnlyList<NpcDialogueEntry> AllEntries => s_entries;

        public static bool TryGet(string npcId, out NpcDialogueEntry entry)
        {
            entry = null;
            foreach (var e in s_entries)
            {
                if (e.NpcId == npcId)
                {
                    entry = e;
                    return true;
                }
            }

            return false;
        }

        public static int TotalNpcsWithDialogue => s_entries.Count;

        public static int NpcsWithAtLeast10Nodes
        {
            get
            {
                int count = 0;
                foreach (var e in s_entries)
                {
                    if (e.NodeCount >= 10)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
    }
}
