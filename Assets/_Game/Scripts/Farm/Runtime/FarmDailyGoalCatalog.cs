using System.Collections.Generic;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// fable_65 — catálogo canônico das metas diárias de farm (4-6/dia). C# puro, testável.
    /// Ponto único da definição de meta {id estável, DisplayName PT-BR, RequiredProgress}.
    /// Toda meta listada aqui DEVE ter uma entrada em FarmDailyGoalRewardTable (teste de integridade).
    /// Ids estáveis — NUNCA renomear (id-stability). As 2 primeiras vêm do WI-24.
    ///
    /// Cada meta é alimentada por um evento REAL existente (verificado na Fase 0 — fable_65):
    ///   first_harvest  / harvest_three  -> CropHarvestedEvent
    ///   sell_first_crop                 -> EconomyTransactionCompletedEvent
    ///   plant_three                     -> SeedPlantedEvent
    ///   gather_resource                 -> TreeChoppedEvent
    ///   talk_to_npc                     -> NpcInteractionStartedEvent
    /// </summary>
    public static class FarmDailyGoalCatalog
    {
        public const string GoalFirstHarvest = "daily_goal_first_harvest";
        public const string GoalSellFirstCrop = "daily_goal_sell_first_crop";
        public const string GoalHarvestThree = "daily_goal_harvest_three";
        public const string GoalPlantThree = "daily_goal_plant_three";
        public const string GoalGatherResource = "daily_goal_gather_resource";
        public const string GoalTalkToNpc = "daily_goal_talk_to_npc";

        private static readonly List<FarmDailyGoalDefinition> Definitions = new List<FarmDailyGoalDefinition>
        {
            new FarmDailyGoalDefinition { GoalId = GoalFirstHarvest, DisplayName = "Primeira colheita do dia", RequiredProgress = 1 },
            new FarmDailyGoalDefinition { GoalId = GoalSellFirstCrop, DisplayName = "Vender primeiro item colhido", RequiredProgress = 1 },
            new FarmDailyGoalDefinition { GoalId = GoalHarvestThree, DisplayName = "Colher 3 cultivos", RequiredProgress = 3 },
            new FarmDailyGoalDefinition { GoalId = GoalPlantThree, DisplayName = "Plantar 3 sementes", RequiredProgress = 3 },
            new FarmDailyGoalDefinition { GoalId = GoalGatherResource, DisplayName = "Coletar 1 recurso da fazenda", RequiredProgress = 1 },
            new FarmDailyGoalDefinition { GoalId = GoalTalkToNpc, DisplayName = "Falar com 1 morador", RequiredProgress = 1 }
        };

        /// <summary>Lista imutável das definições de meta na ordem canônica.</summary>
        public static IReadOnlyList<FarmDailyGoalDefinition> All => Definitions;

        /// <summary>DisplayName PT-BR de um id; retorna o próprio id como fallback seguro (Null Object).</summary>
        public static string GetDisplayName(string goalId)
        {
            if (string.IsNullOrWhiteSpace(goalId))
            {
                return string.Empty;
            }

            foreach (var def in Definitions)
            {
                if (def.GoalId == goalId)
                {
                    return def.DisplayName;
                }
            }

            return goalId;
        }
    }
}
