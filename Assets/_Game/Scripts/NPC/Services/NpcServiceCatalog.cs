using System.Collections.Generic;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — registro canônico dos 8 serviços únicos de NPC (CITY_NPC_ROSTER_SERVICES §serviços).
    /// PURO (sem Unity): consultado pelo NpcShopController (Conversar) e pelos testes.
    ///
    /// MAPEAMENTO Fase 0 (serviço → npcId canônico do <see cref="NpcTownRosterRegistry"/>): a spec cita
    /// alguns nomes (Sereth/Kael/Mirena/Veska) que NÃO existem no roster v1.1. Conforme o mandato da
    /// Fase 0 ("nomes que divirjam do roster devem ser resolvidos contra o roster, sem alterar o efeito
    /// do serviço"), cada serviço foi ancorado ao NPC canônico de função equivalente, mantendo UM
    /// serviço único por NPC e oito NPCs distintos:
    ///   1. Análise de Criatura      → npc_thalindra (Pesquisador/Alquimista — pesquisa/lore)        [roster ✓]
    ///   2. Reparo com Desconto      → npc_brumdar   (Artesao — ferramentas/armas/reparo)            [roster ✓]
    ///   3. Encomenda de Livro       → npc_yael      (itens raros/Nyx/segredos — loja noturna)       [roster ✓]
    ///   4. Banho Termal             → npc_gruta     (taverna/estalagem — hospedagem/descanso)        [Sereth → gruta]
    ///   5. Pasto Premium            → npc_eiran     (Tratador — animais/ração/rancho)               [roster ✓]
    ///   6. Contrato de Caça         → npc_zrix      (Explorador — mapas/caverna/contratos da guilda) [Kael → zrix]
    ///   7. Prato do Dia             → npc_orlan     (taverna/estalagem — comida; par da gruta)        [Mirena → orlan]
    ///   8. Identificação de Relíquia→ npc_ozzra     (Alquimista — reagentes/itens mágicos)           [Veska → ozzra]
    /// Os efeitos, custos e gates do escopo permanecem inalterados; só o npcId foi resolvido.
    /// </summary>
    public static class NpcServiceCatalog
    {
        // ── Preços canônicos do escopo (sinks de economia) ───────────────────────────────────────
        public const int AnalysisGoldCost = 80;
        public const int BookOrderGoldCost = 200;
        public const int ThermalBathGoldCost = 50;
        public const int RelicIdentificationGoldCost = 120;

        // ── Gates de amizade canônicos do escopo ─────────────────────────────────────────────────
        public const int RepairDiscountFriendshipMin = 3;   // Brumdar −30%
        public const int PremiumPastureFriendshipMin = 4;   // Eiran pasto 3 dias
        public const int DailyDishFriendshipMin = 2;         // Mirena/Orlan prato do dia

        // ── Parâmetros de efeito (consumidos pelos hooks/efeitos) ─────────────────────────────────
        public const float RepairDiscountFraction = 0.30f;  // −30%
        public const int BookOrderDeliveryDays = 3;          // chega no dia +3
        public const int PremiumPastureDays = 3;             // animais alimentados por 3 dias
        public const int HuntContractRewardMultiplier = 2;   // recompensa 2×

        // ── Service IDs estáveis ─────────────────────────────────────────────────────────────────
        public const string AnalysisServiceId = "service_thalindra_analysis";
        public const string RepairDiscountServiceId = "service_brumdar_repair_discount";
        public const string BookOrderServiceId = "service_yael_book_order";
        public const string ThermalBathServiceId = "service_gruta_thermal_bath";
        public const string PremiumPastureServiceId = "service_eiran_premium_pasture";
        public const string HuntContractServiceId = "service_zrix_hunt_contract";
        public const string DailyDishServiceId = "service_orlan_daily_dish";
        public const string RelicIdentificationServiceId = "service_ozzra_relic_identification";

        private static readonly List<NpcServiceDefinition> s_services = new List<NpcServiceDefinition>
        {
            // 1. Thalindra — Análise de Criatura: 80g + 1 parte do monstro → GrantKnowledge (F21).
            new NpcServiceDefinition
            {
                ServiceId = AnalysisServiceId,
                NpcId = "npc_thalindra",
                DisplayLabel = "Analise de Criatura",
                Effect = NpcServiceEffectType.CreatureAnalysis,
                GoldCost = AnalysisGoldCost,
                // O item exato (parte do monstro) é resolvido em runtime pelo executor a partir do
                // inventário do jogador; aqui o gate de item é "qualquer parte de monstro" (RequiredItemId
                // vazio = o executor pede ao alvo F21/inventário a parte mais valiosa faltante).
            },

            // 2. Brumdar — Reparo com Desconto: amizade 3+ → reparo −30% (hook no custo F49).
            new NpcServiceDefinition
            {
                ServiceId = RepairDiscountServiceId,
                NpcId = "npc_brumdar",
                DisplayLabel = "Reparo com Desconto",
                Effect = NpcServiceEffectType.RepairDiscount,
                FriendshipMin = RepairDiscountFriendshipMin,
            },

            // 3. Yael — Encomenda de Livro: 200g → após 3 dias chega livro de conhecimento.
            new NpcServiceDefinition
            {
                ServiceId = BookOrderServiceId,
                NpcId = "npc_yael",
                DisplayLabel = "Encomenda de Livro",
                Effect = NpcServiceEffectType.BookOrder,
                GoldCost = BookOrderGoldCost,
            },

            // 4. Sereth→gruta — Banho Termal: 50g → remove Fatigue + buff Rested (F16/F01).
            new NpcServiceDefinition
            {
                ServiceId = ThermalBathServiceId,
                NpcId = "npc_gruta",
                DisplayLabel = "Banho Termal",
                Effect = NpcServiceEffectType.ThermalBath,
                GoldCost = ThermalBathGoldCost,
            },

            // 5. Eiran — Pasto Premium: amizade 4+ → animais alimentados por 3 dias (F12).
            new NpcServiceDefinition
            {
                ServiceId = PremiumPastureServiceId,
                NpcId = "npc_eiran",
                DisplayLabel = "Pasto Premium",
                Effect = NpcServiceEffectType.PremiumPasture,
                FriendshipMin = PremiumPastureFriendshipMin,
            },

            // 6. Kael→zrix — Contrato de Caça Pessoal: semanal, alvo elite, recompensa 2× (flag/quest).
            new NpcServiceDefinition
            {
                ServiceId = HuntContractServiceId,
                NpcId = "npc_zrix",
                DisplayLabel = "Contrato de Caca",
                Effect = NpcServiceEffectType.HuntContract,
            },

            // 7. Mirena→orlan — Prato do Dia: 1×/dia grátis com amizade 2+ → comida com buff do dia.
            new NpcServiceDefinition
            {
                ServiceId = DailyDishServiceId,
                NpcId = "npc_orlan",
                DisplayLabel = "Prato do Dia",
                Effect = NpcServiceEffectType.DailyDish,
                FriendshipMin = DailyDishFriendshipMin,
            },

            // 8. Veska→ozzra — Identificação de Relíquia: itens unidentified_* (F31) → revela por 120g.
            new NpcServiceDefinition
            {
                ServiceId = RelicIdentificationServiceId,
                NpcId = "npc_ozzra",
                DisplayLabel = "Identificacao de Reliquia",
                Effect = NpcServiceEffectType.RelicIdentification,
                GoldCost = RelicIdentificationGoldCost,
            },
        };

        public static IReadOnlyList<NpcServiceDefinition> All => s_services;

        public static int ServiceCount => s_services.Count;

        /// <summary>True se este NPC oferece algum serviço único.</summary>
        public static bool IsServiceProvider(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return false;
            foreach (var s in s_services)
            {
                if (s.NpcId == npcId) return true;
            }

            return false;
        }

        /// <summary>Serviços oferecidos por este NPC (pode ser vazio).</summary>
        public static IReadOnlyList<NpcServiceDefinition> ForNpc(string npcId)
        {
            var list = new List<NpcServiceDefinition>();
            if (string.IsNullOrEmpty(npcId)) return list;
            foreach (var s in s_services)
            {
                if (s.NpcId == npcId) list.Add(s);
            }

            return list;
        }

        public static NpcServiceDefinition GetById(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId)) return null;
            foreach (var s in s_services)
            {
                if (s.ServiceId == serviceId) return s;
            }

            return null;
        }
    }
}
