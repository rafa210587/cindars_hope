using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Canonical registry of town NPCs for WAVE25 validation and runtime queries.
    /// All IDs are real IDs from existing NpcDataSO assets.
    /// Source: WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md
    /// </summary>
    public static class NpcTownRosterRegistry
    {
        public enum NpcPriorityTier { MVP, Extended, Legacy }
        public enum NpcImplementationStatus
        {
            SceneShopRuntime,
            ShopRuntimeBasicWithServiceDebt,
            DialogueOnly,
            LegacyRetained
        }

        public sealed class NpcRosterEntry
        {
            public string NpcId;
            public string DisplayName;
            public string Role;
            public bool HasShop;
            public string ShopId;
            public NpcPriorityTier PriorityTier;
            public NpcImplementationStatus Status;
        }

        private static readonly List<NpcRosterEntry> s_entries = new List<NpcRosterEntry>
        {
            // MVP tier (WAVE12 original 7)
            new NpcRosterEntry { NpcId = "npc_pip", DisplayName = "Pip Semente-Solta", Role = "Comerciante/Explorador", HasShop = true, ShopId = "shop_pip", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },
            new NpcRosterEntry { NpcId = "npc_sylveth", DisplayName = "Sylveth", Role = "Plantador/Curandeiro", HasShop = true, ShopId = "shop_sylveth", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_brumdar", DisplayName = "Brumdar Ferro-Quieto", Role = "Artesao/Combatente", HasShop = true, ShopId = "shop_brumdar", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_renko", DisplayName = "Renko Tres-Sorrisos", Role = "Comerciante/Artesao", HasShop = true, ShopId = "shop_renko", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_thalindra", DisplayName = "Thalindra Veu-de-Lua", Role = "Pesquisador/Alquimista", HasShop = true, ShopId = "shop_thalindra", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },
            new NpcRosterEntry { NpcId = "npc_zrix", DisplayName = "Zrix das Estradas", Role = "Explorador/Comerciante", HasShop = true, ShopId = "shop_zrix", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_nimble", DisplayName = "Nimble Galhobaixo", Role = "Construtor/Artesao", HasShop = true, ShopId = "shop_nimble", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },

            // Extended tier (WAVE12C additional 16)
            new NpcRosterEntry { NpcId = "npc_corvus", DisplayName = "Padre Corvus", Role = "Curandeiro/Guardiao", HasShop = true, ShopId = "shop_corvus", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_mara", DisplayName = "Mara Vellum", Role = "Escriba/Comerciante", HasShop = true, ShopId = "shop_mara", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_gurd", DisplayName = "Gurd Carvalho-Torto", Role = "Construtor/Combatente", HasShop = true, ShopId = "shop_gurd", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_hund", DisplayName = "Hund Carvalho-Torto", Role = "Guardiao/Construtor", HasShop = true, ShopId = "shop_hund", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_ozzra", DisplayName = "Ozzra Fumacazul", Role = "Alquimista/Artesao", HasShop = true, ShopId = "shop_ozzra", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_gruta", DisplayName = "Gruta Panela-Funda", Role = "Comerciante/Musico", HasShop = true, ShopId = "shop_gruta", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_yael", DisplayName = "Yael Noite-Mansa", Role = "Comerciante/Explorador", HasShop = true, ShopId = "shop_yael", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_dagna", DisplayName = "Dagna Rocha-Morna", Role = "Minerador/Combatente", HasShop = true, ShopId = "shop_dagna", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_alaric", DisplayName = "Ser Alaric Veyr", Role = "Guardiao/Combatente", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },
            new NpcRosterEntry { NpcId = "npc_mirela", DisplayName = "Mirela dos Lacos", Role = "Artesao/Comerciante", HasShop = true, ShopId = "shop_mirela", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_eiran", DisplayName = "Eiran Valeclaro", Role = "Tratador/Plantador", HasShop = true, ShopId = "shop_eiran", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_liora", DisplayName = "Liora Canta-Rio", Role = "Musico/Pesquisador", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },
            new NpcRosterEntry { NpcId = "npc_orlan", DisplayName = "Orlan Pouso-Curto", Role = "Comerciante/Escriba", HasShop = true, ShopId = "shop_orlan", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_savra", DisplayName = "Savra Escama-Verde", Role = "Curandeiro/Explorador", HasShop = true, ShopId = "shop_savra", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_tovin", DisplayName = "Tovin Maos-de-Selo", Role = "Escriba/Artesao", HasShop = true, ShopId = "shop_tovin", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_maelor", DisplayName = "Maelor Cinza", Role = "Explorador/Pesquisador", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },

            // Legacy retained
            new NpcRosterEntry { NpcId = "npc_vaalara_wanderer_01", DisplayName = "Peregrino de Vaalara", Role = "Lore wanderer", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Legacy, Status = NpcImplementationStatus.LegacyRetained }
        };

        public static IReadOnlyList<NpcRosterEntry> AllEntries => s_entries;

        public static bool TryGet(string npcId, out NpcRosterEntry entry)
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

        public static int CanonicalCount => 23;
        public static int MvpCount => 7;
    }
}
