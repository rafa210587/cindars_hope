using System.Collections.Generic;
using CindarsHope.Farm.Resources;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — pomar sazonal do lote OESTE (CA-3). 4 árvores frutíferas que dão fruta por
    /// estação. NÃO cria um segundo padrão de colheita: produz <see cref="ResourceNodeDefinition"/>
    /// para o motor existente <see cref="FarmResourceNodeService"/> (que já valida RequiredSeason
    /// e despleta de forma idempotente). Dado puro, sem Unity.
    ///
    /// Estações canônicas (CalendarSeason): "Spring", "Summer", "Autumn", "Winter".
    /// As 4 árvores cobrem as 4 estações (uma fruta por estação), reusando ResourceNodeRefreshPolicy.SeasonStart.
    /// </summary>
    public static class FarmOrchardCatalog
    {
        public const string ZoneId = FarmLotId.West;

        public const string NodeApple = "farm_orchard_apple";   // Spring
        public const string NodeCherry = "farm_orchard_cherry"; // Summer
        public const string NodePear = "farm_orchard_pear";     // Autumn
        public const string NodePlum = "farm_orchard_plum";     // Winter

        private static ResourceNodeDefinition Fruit(string nodeId, string season, string dropItemId)
        {
            return new ResourceNodeDefinition
            {
                NodeId = nodeId,
                NodeType = ResourceNodeType.Tree,
                DisplayName = nodeId,
                AllowedZones = new List<string> { ZoneId },
                RequiredFarmLevel = 0,           // 5.3: sem gate de nível/caverna; só posse do lote
                RequiredSeason = season,         // fruta apenas na estação correspondente (CA-3)
                RefreshPolicy = ResourceNodeRefreshPolicy.SeasonStart,
                RefreshAfterDays = 0,
                DropTableId = dropItemId,
                StaminaCost = 2,
                FatigueCost = 0,
                CanRegrow = true,
                CanBeRemoved = false,
                MaxHitsEnabled = false,
                MaxHits = 1
            };
        }

        /// <summary>As 4 definições do pomar, indexadas por NodeId (consumo pelo FarmResourceNodeService).</summary>
        public static Dictionary<string, ResourceNodeDefinition> BuildDefinitions()
        {
            return new Dictionary<string, ResourceNodeDefinition>
            {
                { NodeApple, Fruit(NodeApple, "Spring", "item_crop_apple") },
                { NodeCherry, Fruit(NodeCherry, "Summer", "item_crop_cherry") },
                { NodePear, Fruit(NodePear, "Autumn", "item_crop_pear") },
                { NodePlum, Fruit(NodePlum, "Winter", "item_crop_plum") },
            };
        }

        public static IReadOnlyList<string> NodeIds => new[] { NodeApple, NodeCherry, NodePear, NodePlum };
    }
}
