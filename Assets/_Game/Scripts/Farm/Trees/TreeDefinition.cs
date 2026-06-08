using System.Collections.Generic;

namespace CindarsHope.Farm.Trees
{
    public enum TreeKind
    {
        CommonWild = 0,
        CommonFruitFuture = 1,
        Decorative = 2,
        LoreProtected = 3,
        ManaReserved = 10
    }

    public enum RegrowthPolicy
    {
        None = 0,
        ControlledInZone = 1,
        PlayerFreeZone = 2,
        NeverAfterPlayerClear = 3
    }

    public class TreeDefinition
    {
        public string TreeId { get; set; }
        public TreeKind Kind { get; set; } = TreeKind.CommonWild;
        public string DisplayName { get; set; }
        public int RequiredFarmLevel { get; set; } = 0;
        public string RequiredAxeTier { get; set; } = "Basic";
        public int MaxHits { get; set; } = 3;
        public string DropTableId { get; set; }
        public string StumpDefinitionId { get; set; }
        public bool CanRegrow { get; set; } = true;
        public RegrowthPolicy RegrowthPolicy { get; set; } = RegrowthPolicy.ControlledInZone;
        public bool BlocksPath { get; set; } = true;
        public bool CanBeRemoved { get; set; } = true;
        public List<string> AllowedZones { get; set; } = new List<string>();
        public bool IsLoreProtected => Kind == TreeKind.LoreProtected || Kind == TreeKind.ManaReserved;

        public static List<TreeDefinition> GetDefaults()
        {
            return new List<TreeDefinition>
            {
                new TreeDefinition { TreeId = "tree_oak", Kind = TreeKind.CommonWild, DisplayName = "Carvalho Comum", MaxHits = 3, DropTableId = "drop_wood_common", CanRegrow = true },
                new TreeDefinition { TreeId = "tree_pine", Kind = TreeKind.CommonWild, DisplayName = "Pinheiro", MaxHits = 4, DropTableId = "drop_wood_pine", CanRegrow = true },
                new TreeDefinition { TreeId = "tree_mana", Kind = TreeKind.ManaReserved, DisplayName = "Arvore de Mana", MaxHits = 0, CanRegrow = false, CanBeRemoved = false },
            };
        }
    }
}
