using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Documents dialogue coverage per NPC for runtime validators.
    /// Coverage is derived from TownNpcDialogueLibrary (single source of truth for the
    /// expanded dialogue content), so the registry can never drift from the actual lines.
    /// Assets are synchronized via the editor menu
    /// "CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)".
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

        private static List<NpcDialogueEntry> s_entries;

        private static List<NpcDialogueEntry> Entries
        {
            get
            {
                if (s_entries == null)
                {
                    s_entries = new List<NpcDialogueEntry>();
                    foreach (var content in TownNpcDialogueLibrary.AllContent)
                    {
                        s_entries.Add(new NpcDialogueEntry
                        {
                            NpcId = content.NpcId,
                            DialogueSetId = content.DialogueSetId,
                            NodeCount = TownNpcDialogueLibrary.NodesPerNpc,
                            HasGreeting = content.Greetings != null && content.Greetings.Length > 0,
                            HasRole = !string.IsNullOrEmpty(content.Role1),
                            HasService = content.HasShop,
                            HasTownContext = !string.IsNullOrEmpty(content.Town1),
                            HasGoodbye = content.Goodbyes != null && content.Goodbyes.Length > 0
                        });
                    }
                }

                return s_entries;
            }
        }

        public static IReadOnlyList<NpcDialogueEntry> AllEntries => Entries;

        public static bool TryGet(string npcId, out NpcDialogueEntry entry)
        {
            entry = null;
            foreach (var e in Entries)
            {
                if (e.NpcId == npcId)
                {
                    entry = e;
                    return true;
                }
            }

            return false;
        }

        public static int TotalNpcsWithDialogue => Entries.Count;

        public static int NpcsWithAtLeast10Nodes
        {
            get
            {
                int count = 0;
                foreach (var e in Entries)
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
