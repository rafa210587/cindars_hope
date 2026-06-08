using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Trees;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class TreeChopServiceTests
    {
        private Dictionary<string, TreeDefinition> _defs;
        private TreeChopService _service;

        [SetUp]
        public void Setup()
        {
            _defs = new Dictionary<string, TreeDefinition>();
            _defs["tree_oak"] = new TreeDefinition { TreeId = "tree_oak", Kind = TreeKind.CommonWild, MaxHits = 3, DropTableId = "drop_wood", StumpDefinitionId = "stump_oak", CanRegrow = true, CanBeRemoved = true };
            _defs["tree_mana"] = new TreeDefinition { TreeId = "tree_mana", Kind = TreeKind.ManaReserved, MaxHits = 0, CanRegrow = false, CanBeRemoved = false };
            _service = new TreeChopService(_defs);
        }

        private TreeInstanceState MatureOak() => new TreeInstanceState
        {
            TreeInstanceId = "inst_01",
            TreeId = "tree_oak",
            Stage = TreeGrowthStage.Mature,
            RemainingHits = 3
        };

        [Test]
        public void Chop_Hit_ReducesRemainingHits()
        {
            var tree = MatureOak();
            var result = _service.Chop(tree, "Basic", 1);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.TreeFelled);
            Assert.AreEqual(2, tree.RemainingHits);
        }

        [Test]
        public void Chop_FinalHit_FellsTree()
        {
            var tree = MatureOak();
            tree.RemainingHits = 1;
            var result = _service.Chop(tree, "Basic", 1);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.TreeFelled);
            Assert.IsTrue(result.BecameStump);
        }

        [Test]
        public void Chop_FelledTree_BecomesStump()
        {
            var tree = MatureOak();
            tree.RemainingHits = 1;
            _service.Chop(tree, "Basic", 5);
            Assert.AreEqual(TreeGrowthStage.Stump, tree.Stage);
        }

        [Test]
        public void Chop_ManaTree_Blocked()
        {
            var mana = new TreeInstanceState { TreeInstanceId = "mana_01", TreeId = "tree_mana", Stage = TreeGrowthStage.Mature, RemainingHits = 99 };
            var result = _service.Chop(mana, "Basic", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("LoreProtected", result.FailureReason);
        }

        [Test]
        public void Chop_StumpTree_Blocked()
        {
            var stump = new TreeInstanceState { TreeInstanceId = "inst_01", TreeId = "tree_oak", Stage = TreeGrowthStage.Stump };
            var result = _service.Chop(stump, "Basic", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("TreeNotCuttable", result.FailureReason);
        }

        [Test]
        public void RemoveStump_Succeeds()
        {
            var stump = new TreeInstanceState { TreeInstanceId = "inst_01", TreeId = "tree_oak", Stage = TreeGrowthStage.Stump };
            var result = _service.RemoveStump(stump, "Basic", 5);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(TreeGrowthStage.Removed, stump.Stage);
        }

        [Test]
        public void FelledTree_WithRegrowth_SetsNextRegrowthDay()
        {
            var tree = MatureOak();
            tree.RemainingHits = 1;
            _service.Chop(tree, "Basic", 10);
            Assert.Greater(tree.NextRegrowthEligibleDay, 10);
        }

        [Test]
        public void TreeDefinition_ManaIsLoreProtected()
        {
            var manaDef = _defs["tree_mana"];
            Assert.IsTrue(manaDef.IsLoreProtected);
        }

        [Test]
        public void TreeDefinition_CommonNotLoreProtected()
        {
            var oakDef = _defs["tree_oak"];
            Assert.IsFalse(oakDef.IsLoreProtected);
        }
    }
}
