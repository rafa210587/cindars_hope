using System.Collections.Generic;

namespace CindarsHope.Farm.Trees
{
    public class TreeChopService
    {
        private readonly Dictionary<string, TreeDefinition> _definitions;

        public TreeChopService(Dictionary<string, TreeDefinition> definitions)
        {
            _definitions = definitions ?? new Dictionary<string, TreeDefinition>();
        }

        public TreeChopResult Chop(TreeInstanceState tree, string axeTier, int currentDay)
        {
            if (tree == null)
                return TreeChopResult.Fail("TreeInstanceNotFound");

            if (!_definitions.TryGetValue(tree.TreeId, out var def))
                return TreeChopResult.Fail("TreeDefinitionNotFound");

            if (def.IsLoreProtected)
                return TreeChopResult.Fail("LoreProtected");

            if (!tree.IsCuttable)
                return TreeChopResult.Fail("TreeNotCuttable");

            if (def.RequiredAxeTier != axeTier && def.RequiredAxeTier != "Basic")
                return TreeChopResult.Fail("AxeTierInsufficient");

            tree.RemainingHits--;
            tree.LastChoppedDay = currentDay;

            if (tree.RemainingHits <= 0)
            {
                var drops = ResolveDrops(def.DropTableId);
                tree.Stage = string.IsNullOrEmpty(def.StumpDefinitionId) ? TreeGrowthStage.Removed : TreeGrowthStage.Stump;
                if (def.CanRegrow)
                    tree.NextRegrowthEligibleDay = currentDay + 7;
                return TreeChopResult.Felled(def.StumpDefinitionId, drops.Item1, drops.Item2);
            }

            return TreeChopResult.Hit();
        }

        public TreeChopResult RemoveStump(TreeInstanceState tree, string axeTier, int currentDay)
        {
            if (tree == null || !tree.IsStump)
                return TreeChopResult.Fail("NotAStump");

            if (!_definitions.TryGetValue(tree.TreeId, out var def))
                return TreeChopResult.Fail("TreeDefinitionNotFound");

            if (!def.CanBeRemoved)
                return TreeChopResult.Fail("StumpCannotBeRemoved");

            tree.Stage = TreeGrowthStage.Removed;
            tree.LastChoppedDay = currentDay;
            return new TreeChopResult { Success = true, DroppedItemIds = new[] { "item_wood_scrap" }, DroppedQuantities = new[] { 1 } };
        }

        private (string[], int[]) ResolveDrops(string dropTableId)
        {
            if (string.IsNullOrEmpty(dropTableId))
                return (new[] { "item_wood" }, new[] { 2 });
            return (new[] { "item_wood", "item_fiber" }, new[] { 2, 1 });
        }
    }
}
