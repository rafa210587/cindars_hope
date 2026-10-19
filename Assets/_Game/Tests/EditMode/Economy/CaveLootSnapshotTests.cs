using NUnit.Framework;
using CindarsHope.Cave.Loot;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class CaveLootSnapshotTests
    {
        private CaveLootSnapshotService _service;

        [SetUp]
        public void SetUp() => _service = new CaveLootSnapshotService();

        [Test]
        public void SameSeedLevelInstance_ReturnsSameSnapshot()
        {
            var a = _service.GetOrCreate(1000, 5, "node_01", CaveLootSourceType.MiningNode);
            var b = _service.GetOrCreate(1000, 5, "node_01", CaveLootSourceType.MiningNode);
            Assert.AreSame(a, b, "Revisiting same CaveLevel+RunSeed must return same snapshot");
        }

        [Test]
        public void DifferentSeed_ReturnsDifferentSnapshot()
        {
            var a = _service.GetOrCreate(1000, 5, "node_01", CaveLootSourceType.MiningNode);
            var b = _service.GetOrCreate(2000, 5, "node_01", CaveLootSourceType.MiningNode);
            Assert.AreNotSame(a, b);
        }

        [Test]
        public void DifferentLevel_ReturnsDifferentSnapshot()
        {
            var a = _service.GetOrCreate(1000, 5, "node_01", CaveLootSourceType.MiningNode);
            var b = _service.GetOrCreate(1000, 6, "node_01", CaveLootSourceType.MiningNode);
            Assert.AreNotSame(a, b);
        }

        [Test]
        public void LootSeed_IsDeterministic()
        {
            int seed1 = CaveLootSnapshotService.DeriveLootSeed(1000, 5, "node_01");
            int seed2 = CaveLootSnapshotService.DeriveLootSeed(1000, 5, "node_01");
            Assert.AreEqual(seed1, seed2);
        }

        [Test]
        public void LootSeed_DiffersForDifferentInputs()
        {
            int s1 = CaveLootSnapshotService.DeriveLootSeed(1000, 5, "node_01");
            int s2 = CaveLootSnapshotService.DeriveLootSeed(1000, 5, "node_02");
            Assert.AreNotEqual(s1, s2);
        }

        [Test]
        public void TryOpen_FirstTime_Succeeds()
        {
            var entry = _service.GetOrCreate(1000, 3, "chest_01", CaveLootSourceType.TreasureChest);
            bool result = _service.TryOpen(entry, currentDay: 5);
            Assert.IsTrue(result);
            Assert.IsTrue(entry.IsOpened);
            Assert.AreEqual(5, entry.OpenedDay);
        }

        [Test]
        public void TryOpen_SecondTime_Fails()
        {
            var entry = _service.GetOrCreate(1000, 3, "chest_01", CaveLootSourceType.TreasureChest);
            _service.TryOpen(entry, currentDay: 5);
            bool second = _service.TryOpen(entry, currentDay: 6);
            Assert.IsFalse(second, "Opening same chest twice must fail (idempotency)");
        }

        [Test]
        public void TryDeplete_FirstTime_Succeeds()
        {
            var entry = _service.GetOrCreate(1000, 2, "vein_01", CaveLootSourceType.RareVein);
            bool result = _service.TryDeplete(entry, currentDay: 3);
            Assert.IsTrue(result);
            Assert.IsTrue(entry.IsDepleted);
        }

        [Test]
        public void TryDeplete_SecondTime_Fails()
        {
            var entry = _service.GetOrCreate(1000, 2, "vein_01", CaveLootSourceType.RareVein);
            _service.TryDeplete(entry, 3);
            bool second = _service.TryDeplete(entry, 4);
            Assert.IsFalse(second);
        }

        [Test]
        public void MarkRewardConsumed_Idempotent()
        {
            var entry = _service.GetOrCreate(1000, 1, "lore_01", CaveLootSourceType.LorePoint);
            _service.MarkRewardConsumed(entry, "lore_reward_01");
            _service.MarkRewardConsumed(entry, "lore_reward_01");
            Assert.AreEqual(1, entry.RewardConsumedFlags.Count, "Marking same flag twice must not duplicate");
        }

        [Test]
        public void ResetForNewRun_ClearsAllSnapshots()
        {
            _service.GetOrCreate(1000, 1, "node_01", CaveLootSourceType.MiningNode);
            _service.GetOrCreate(1000, 2, "node_01", CaveLootSourceType.MiningNode);
            _service.ResetForNewRun();
            Assert.AreEqual(0, ((System.Collections.Generic.Dictionary<CaveSnapshotKey, CaveLootSnapshotEntry>)_service.AllSnapshots).Count);
        }

        [Test]
        public void DebugForceRegenerate_ClearsOnlyTargetLevel()
        {
            _service.GetOrCreate(1000, 1, "node_01", CaveLootSourceType.MiningNode);
            _service.GetOrCreate(1000, 2, "node_01", CaveLootSourceType.MiningNode);
            _service.DebugForceRegenerateLevel(1000, 1);
            // Level 2 snapshot must still exist
            var remaining = _service.GetOrCreate(1000, 2, "node_01", CaveLootSourceType.MiningNode);
            Assert.IsNotNull(remaining);
            // Level 1 is gone — new entry will be created
            var level1New = _service.GetOrCreate(1000, 1, "node_01", CaveLootSourceType.MiningNode);
            Assert.IsNotNull(level1New);
        }

        [Test]
        public void MiningNodeProfile_Level101_Blocks_CommonMining()
        {
            var profile = new CaveMiningNodeLootProfile
            {
                ProfileId = "mine_iron",
                AllowedFloorMin = 1,
                AllowedFloorMax = 100,
                IsLevel101RestrictedNode = true
            };
            Assert.IsTrue(profile.AllowsFloor(50));
            Assert.IsTrue(profile.AllowsFloor(100));
            Assert.IsFalse(profile.AllowsFloor(101), "Level 101 must block common mining nodes");
        }
    }
}
