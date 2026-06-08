using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Resources;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmResourceNodeRefreshTests
    {
        private Dictionary<string, ResourceNodeDefinition> _defs;
        private FarmResourceNodeService _service;
        private FarmResourceRefreshProcessor _refresher;

        [SetUp]
        public void Setup()
        {
            _defs = new Dictionary<string, ResourceNodeDefinition>
            {
                ["node_rock"] = new ResourceNodeDefinition
                {
                    NodeId = "node_rock", NodeType = ResourceNodeType.Rock,
                    RefreshPolicy = ResourceNodeRefreshPolicy.FixedDays, RefreshAfterDays = 3,
                    CanRegrow = true, DropTableId = "drop_stone"
                },
                ["node_forage"] = new ResourceNodeDefinition
                {
                    NodeId = "node_forage", NodeType = ResourceNodeType.Forage,
                    RefreshPolicy = ResourceNodeRefreshPolicy.NextDayChance, NextDayRefreshChance = 1.0f,
                    CanRegrow = true, DropTableId = "drop_herb"
                },
                ["node_lore"] = new ResourceNodeDefinition
                {
                    NodeId = "node_lore", NodeType = ResourceNodeType.SpecialLoreNode,
                    CanRegrow = false, IsLoreProtected = true
                },
                ["node_endgame"] = new ResourceNodeDefinition
                {
                    NodeId = "node_endgame", NodeType = ResourceNodeType.EndgameNode,
                    RefreshPolicy = ResourceNodeRefreshPolicy.EndgameOnly
                }
            };
            _service = new FarmResourceNodeService(_defs);
            _refresher = new FarmResourceRefreshProcessor(_defs);
        }

        private ResourceNodeInstanceState FreshRock(string zone = "zone_farm") =>
            new ResourceNodeInstanceState
            {
                NodeInstanceId = "rock_01", NodeId = "node_rock",
                ZoneId = zone, CurrentState = ResourceNodeCurrentState.Available, RemainingHits = 1
            };

        private ResourceNodeRefreshContext DefaultCtx(int day = 5) =>
            new ResourceNodeRefreshContext { CurrentDay = day, FarmLevel = 1 };

        [Test]
        public void Harvest_HappyPath_DepletesNode()
        {
            var node = FreshRock();
            var result = _service.Harvest(node, 1);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ResourceNodeCurrentState.Harvested, node.CurrentState);
        }

        [Test]
        public void Harvest_AlreadyHarvested_Fails()
        {
            var node = FreshRock();
            _service.Harvest(node, 1);
            var second = _service.Harvest(node, 1);
            Assert.IsFalse(second.Success);
            Assert.AreEqual("NodeNotAvailable", second.FailureReason);
        }

        [Test]
        public void Harvest_LoreNode_Fails()
        {
            var lore = new ResourceNodeInstanceState { NodeInstanceId = "l_01", NodeId = "node_lore", CurrentState = ResourceNodeCurrentState.Available };
            var result = _service.Harvest(lore, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("NodeNotCollectable", result.FailureReason);
        }

        [Test]
        public void Refresh_FixedDays_RefreshesAfterEligibleDay()
        {
            var node = FreshRock();
            _service.Harvest(node, 1);
            Assert.IsTrue(node.IsDepleted);
            var ctx = DefaultCtx(4); // day 1 + 3 = day 4
            var refreshed = _refresher.TryRefresh(node, ctx);
            Assert.IsTrue(refreshed);
            Assert.IsTrue(node.IsAvailable);
        }

        [Test]
        public void Refresh_FixedDays_NoRefreshBeforeEligibleDay()
        {
            var node = FreshRock();
            _service.Harvest(node, 1);
            var ctx = DefaultCtx(3); // too early
            Assert.IsFalse(_refresher.TryRefresh(node, ctx));
            Assert.IsTrue(node.IsDepleted);
        }

        [Test]
        public void Refresh_AlreadyAvailable_NoOp()
        {
            var node = FreshRock();
            // Node is already Available — should not "refresh"
            var ctx = DefaultCtx(10);
            Assert.IsFalse(_refresher.TryRefresh(node, ctx));
        }

        [Test]
        public void Refresh_ForbiddenZone_NoRefresh()
        {
            var node = FreshRock("zone_fonte");
            _service.Harvest(node, 1);
            var ctx = DefaultCtx(10);
            Assert.IsFalse(_refresher.TryRefresh(node, ctx));
        }

        [Test]
        public void Refresh_EndgameNode_NeverRefreshes()
        {
            var endgame = new ResourceNodeInstanceState
            {
                NodeInstanceId = "e_01", NodeId = "node_endgame",
                CurrentState = ResourceNodeCurrentState.Harvested, NextEligibleRefreshDay = 0
            };
            var ctx = DefaultCtx(999);
            Assert.IsFalse(_refresher.TryRefresh(endgame, ctx));
        }

        [Test]
        public void Refresh_LoreNode_NeverRefreshes()
        {
            var lore = new ResourceNodeInstanceState
            {
                NodeInstanceId = "l_01", NodeId = "node_lore",
                CurrentState = ResourceNodeCurrentState.Harvested
            };
            var ctx = DefaultCtx(999);
            Assert.IsFalse(_refresher.TryRefresh(lore, ctx));
        }

        [Test]
        public void Refresh_NextDayChance_RefreshesWhenChanceIs100()
        {
            var forage = new ResourceNodeInstanceState
            {
                NodeInstanceId = "f_01", NodeId = "node_forage",
                CurrentState = ResourceNodeCurrentState.Harvested, LastHarvestedDay = 1, RandomSeed = 42
            };
            var ctx = DefaultCtx(2); // next day
            // With NextDayRefreshChance = 1.0, deterministic roll always < 1.0
            Assert.IsTrue(_refresher.TryRefresh(forage, ctx));
        }

        [Test]
        public void ProcessBatch_RefreshesMultipleNodes()
        {
            var rock1 = FreshRock();
            var rock2 = FreshRock();
            rock2.NodeInstanceId = "rock_02";
            _service.Harvest(rock1, 1);
            _service.Harvest(rock2, 1);
            var ctx = DefaultCtx(4);
            int count = _refresher.ProcessBatch(new[] { rock1, rock2 }, ctx);
            Assert.AreEqual(2, count);
        }
    }
}
