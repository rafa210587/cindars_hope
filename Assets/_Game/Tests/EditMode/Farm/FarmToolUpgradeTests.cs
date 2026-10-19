using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Tools;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmToolUpgradeTests
    {
        private Dictionary<string, FarmToolDefinition> _toolDefs;
        private Dictionary<string, ToolUpgradeDefinition> _upgradeDefs;
        private FarmToolUpgradeService _service;
        private FarmToolCapabilityResolver _resolver;

        [SetUp]
        public void Setup()
        {
            _toolDefs = new Dictionary<string, FarmToolDefinition>
            {
                ["hoe_basic"] = new FarmToolDefinition
                {
                    ToolId = "hoe_basic", ToolType = FarmToolType.Hoe, Tier = FarmToolTier.Tier0_Improvised,
                    DurabilityMax = 50, CanRepair = true, CanUpgrade = true
                },
                ["hoe_copper"] = new FarmToolDefinition
                {
                    ToolId = "hoe_copper", ToolType = FarmToolType.Hoe, Tier = FarmToolTier.Tier1_Copper,
                    DurabilityMax = 80, CanRepair = true, CanUpgrade = true
                },
                ["hoe_meteoric"] = new FarmToolDefinition
                {
                    ToolId = "hoe_meteoric", ToolType = FarmToolType.Hoe, Tier = FarmToolTier.Tier6_Meteoric,
                    DurabilityMax = 200, CanRepair = true, CanUpgrade = false
                }
            };
            _upgradeDefs = new Dictionary<string, ToolUpgradeDefinition>
            {
                ["upgrade_hoe_basic_to_copper"] = new ToolUpgradeDefinition
                {
                    UpgradeId = "upgrade_hoe_basic_to_copper",
                    FromToolId = "hoe_basic", ToToolId = "hoe_copper",
                    RequiredGold = 50,
                    RequiredMaterials = new List<ToolMaterialRequirement>
                    {
                        new ToolMaterialRequirement { ItemId = "item_copper_ingot", Quantity = 3 }
                    },
                    RequiredFarmLevel = 0
                }
            };
            _service = new FarmToolUpgradeService(_toolDefs, _upgradeDefs);
            _resolver = new FarmToolCapabilityResolver(_toolDefs);
        }

        private ToolInstanceState BasicHoe() =>
            new ToolInstanceState
            {
                ToolInstanceId = "hoe_inst_01", ToolId = "hoe_basic",
                CurrentTier = FarmToolTier.Tier0_Improvised,
                CurrentDurability = 50, MaxDurability = 50
            };

        private PlayerMaterials SufficientMaterials() =>
            new PlayerMaterials
            {
                Gold = 100,
                Items = new Dictionary<string, int> { ["item_copper_ingot"] = 5 }
            };

        [Test]
        public void Upgrade_HappyPath_UpgradesTier()
        {
            var inst = BasicHoe();
            var mats = SufficientMaterials();
            var result = _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats, 0);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("hoe_copper", inst.ToolId);
            Assert.AreEqual(FarmToolTier.Tier1_Copper, inst.CurrentTier);
        }

        [Test]
        public void Upgrade_DeductsMaterials()
        {
            var inst = BasicHoe();
            var mats = SufficientMaterials();
            _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats, 0);
            Assert.AreEqual(50, mats.Gold); // 100 - 50
            Assert.AreEqual(2, mats.Items["item_copper_ingot"]); // 5 - 3
        }

        [Test]
        public void Upgrade_InsufficientMaterials_Fails()
        {
            var inst = BasicHoe();
            var mats = new PlayerMaterials { Gold = 100, Items = new Dictionary<string, int> { ["item_copper_ingot"] = 1 } };
            var result = _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats, 0);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("InsufficientMaterials", result.FailureReason);
        }

        [Test]
        public void Upgrade_IdempotencyGuard_BlocksDoubleUpgrade()
        {
            var inst = BasicHoe();
            var mats = SufficientMaterials();
            _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats, 0);

            // Attempting same upgrade again should fail
            inst.ToolId = "hoe_basic"; // simulate reload without this check
            inst.LastAppliedUpgradeId = "upgrade_hoe_basic_to_copper";
            var mats2 = SufficientMaterials();
            var second = _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats2, 0);
            Assert.IsFalse(second.Success);
            Assert.AreEqual("UpgradeAlreadyApplied", second.FailureReason);
        }

        [Test]
        public void Upgrade_FarmLevelInsufficient_Fails()
        {
            _upgradeDefs["upgrade_hoe_basic_to_copper"].RequiredFarmLevel = 3;
            var inst = BasicHoe();
            var mats = SufficientMaterials();
            var result = _service.Upgrade(inst, "upgrade_hoe_basic_to_copper", mats, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("FarmLevelInsufficient", result.FailureReason);
        }

        [Test]
        public void Repair_RestoresDurability()
        {
            var inst = BasicHoe();
            inst.CurrentDurability = 10;
            var mats = new PlayerMaterials { Gold = 100 };
            var result = _service.Repair(inst, 20, mats);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(50, inst.CurrentDurability);
            Assert.AreEqual(80, mats.Gold);
        }

        [Test]
        public void Repair_AlreadyFull_Fails()
        {
            var inst = BasicHoe(); // durability full
            var mats = new PlayerMaterials { Gold = 100 };
            var result = _service.Repair(inst, 20, mats);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("ToolAlreadyFullDurability", result.FailureReason);
        }

        [Test]
        public void Capability_HigherTierTool_CanAccessHigherNode()
        {
            Assert.IsTrue(_resolver.CanAccessResourceNode("hoe_copper", FarmToolTier.Tier1_Copper));
            Assert.IsFalse(_resolver.CanAccessResourceNode("hoe_basic", FarmToolTier.Tier1_Copper));
        }

        [Test]
        public void Tool_MeteoricTier_IsEndgameReserved()
        {
            Assert.IsTrue(_resolver.IsEndgameReserved("hoe_meteoric"));
            Assert.IsFalse(_resolver.IsEndgameReserved("hoe_basic"));
        }
    }
}
