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
            // Raça exibida no painel de retrato. Lore-consistente com Vaalara; editável aqui.
            public string Race = "Humano";
            public bool HasShop;
            public string ShopId;
            public NpcPriorityTier PriorityTier;
            public NpcImplementationStatus Status;
        }

        private static readonly List<NpcRosterEntry> s_entries = new List<NpcRosterEntry>
        {
            // MVP tier (WAVE12 original 7)
            new NpcRosterEntry { NpcId = "npc_pip", DisplayName = "Pip Semente-Solta", Race = "Halfling", Role = "Comerciante/Explorador", HasShop = true, ShopId = "shop_pip", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },
            new NpcRosterEntry { NpcId = "npc_sylveth", DisplayName = "Sylveth", Race = "Humana", Role = "Plantador/Curandeiro", HasShop = true, ShopId = "shop_sylveth", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_brumdar", DisplayName = "Brumdar Ferro-Quieto", Race = "Anão", Role = "Artesao/Combatente", HasShop = true, ShopId = "shop_brumdar", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_renko", DisplayName = "Renko Tres-Sorrisos", Race = "Humano", Role = "Comerciante/Artesao", HasShop = true, ShopId = "shop_renko", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_thalindra", DisplayName = "Thalindra Veu-de-Lua", Race = "Elfa", Role = "Pesquisador/Alquimista", HasShop = true, ShopId = "shop_thalindra", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },
            new NpcRosterEntry { NpcId = "npc_zrix", DisplayName = "Zrix das Estradas", Race = "Goblin", Role = "Explorador/Comerciante", HasShop = true, ShopId = "shop_zrix", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_nimble", DisplayName = "Nimble Galhobaixo", Race = "Gnomo", Role = "Construtor/Artesao", HasShop = true, ShopId = "shop_nimble", PriorityTier = NpcPriorityTier.MVP, Status = NpcImplementationStatus.ShopRuntimeBasicWithServiceDebt },

            // Extended tier (WAVE12C additional 16)
            new NpcRosterEntry { NpcId = "npc_corvus", DisplayName = "Padre Corvus", Race = "Humano", Role = "Curandeiro/Guardiao", HasShop = true, ShopId = "shop_corvus", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_mara", DisplayName = "Mara Vellum", Race = "Humana", Role = "Escriba/Comerciante", HasShop = true, ShopId = "shop_mara", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_gurd", DisplayName = "Gurd Carvalho-Torto", Race = "Meio-orc", Role = "Construtor/Combatente", HasShop = true, ShopId = "shop_gurd", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_hund", DisplayName = "Hund Carvalho-Torto", Race = "Meio-orc", Role = "Guardiao/Construtor", HasShop = true, ShopId = "shop_hund", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_ozzra", DisplayName = "Ozzra Fumacazul", Race = "Gnoma", Role = "Alquimista/Artesao", HasShop = true, ShopId = "shop_ozzra", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_gruta", DisplayName = "Gruta Panela-Funda", Race = "Meia-orc", Role = "Comerciante/Musico", HasShop = true, ShopId = "shop_gruta", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_yael", DisplayName = "Yael Noite-Mansa", Race = "Humana", Role = "Comerciante/Explorador", HasShop = true, ShopId = "shop_yael", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_dagna", DisplayName = "Dagna Rocha-Morna", Race = "Anã", Role = "Minerador/Combatente", HasShop = true, ShopId = "shop_dagna", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_alaric", DisplayName = "Ser Alaric Veyr", Race = "Humano", Role = "Guardiao/Combatente", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },
            new NpcRosterEntry { NpcId = "npc_mirela", DisplayName = "Mirela dos Lacos", Race = "Humana", Role = "Artesao/Comerciante", HasShop = true, ShopId = "shop_mirela", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_eiran", DisplayName = "Eiran Valeclaro", Race = "Humano", Role = "Tratador/Plantador", HasShop = true, ShopId = "shop_eiran", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_liora", DisplayName = "Liora Canta-Rio", Race = "Elfa", Role = "Musico/Pesquisador", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },
            new NpcRosterEntry { NpcId = "npc_orlan", DisplayName = "Orlan Pouso-Curto", Race = "Halfling", Role = "Comerciante/Escriba", HasShop = true, ShopId = "shop_orlan", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_savra", DisplayName = "Savra Escama-Verde", Race = "Povo-lagarto", Role = "Curandeiro/Explorador", HasShop = true, ShopId = "shop_savra", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_tovin", DisplayName = "Tovin Maos-de-Selo", Race = "Humano", Role = "Escriba/Artesao", HasShop = true, ShopId = "shop_tovin", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_maelor", DisplayName = "Maelor Cinza", Race = "Humano", Role = "Explorador/Pesquisador", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },
            // Líder da aldeia — Ancião Velorin, um Ninrorin (elfo cinzento) idoso. Sem loja; mora na Mansão.
            new NpcRosterEntry { NpcId = "npc_velorin", DisplayName = "Anciao Velorin", Race = "Ninrorin", Role = "Lider da Aldeia / Anciao", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },

            // Autossuficiência da vila (slice village_economy) — 4 NPCs novos, cada um com gancho próprio.
            new NpcRosterEntry { NpcId = "npc_sael", DisplayName = "Sael Mare-Quieta", Race = "Tiefling", Role = "Pescador / Comerciante", HasShop = true, ShopId = "shop_sael", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_mella", DisplayName = "Mella Forno-Quente", Race = "Humana", Role = "Padeira / Moleira", HasShop = true, ShopId = "shop_mella", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_hess", DisplayName = "Hess Couro-Fundo", Race = "Draconato", Role = "Curtidor / Comerciante", HasShop = true, ShopId = "shop_hess", PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.SceneShopRuntime },
            new NpcRosterEntry { NpcId = "npc_tibbet", DisplayName = "Tibbet Vela-Torta", Race = "Gnomo", Role = "Coveiro / Coroinha", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Extended, Status = NpcImplementationStatus.DialogueOnly },

            // Legacy retained
            new NpcRosterEntry { NpcId = "npc_vaalara_wanderer_01", DisplayName = "Peregrino de Vaalara", Race = "Desconhecida", Role = "Lore wanderer", HasShop = false, ShopId = null, PriorityTier = NpcPriorityTier.Legacy, Status = NpcImplementationStatus.LegacyRetained }
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

        public static int CanonicalCount => 28; // 23 originais + Velorin + Sael/Mella/Hess/Tibbet (village_economy)
        public static int MvpCount => 7;
    }
}
