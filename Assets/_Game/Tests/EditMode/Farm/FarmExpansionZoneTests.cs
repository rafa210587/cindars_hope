using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Expansion;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmExpansionZoneTests
    {
        private List<FarmExpansionZone> _zones;
        private FarmExpansionValidator _validator;

        [SetUp]
        public void Setup()
        {
            _zones = new List<FarmExpansionZone>
            {
                new FarmExpansionZone
                {
                    ZoneId = "zone_free_level1", ZoneType = FarmZoneType.Free,
                    RequiredLevel = FarmPropertyLevel.Level1_InicialPlot,
                    BoundsMinX = 0, BoundsMinY = 0, BoundsMaxX = 10, BoundsMaxY = 10,
                    IsUnlocked = true
                },
                new FarmExpansionZone
                {
                    ZoneId = "zone_blocked_level2", ZoneType = FarmZoneType.Blocked,
                    RequiredLevel = FarmPropertyLevel.Level2_OpenFarm,
                    BoundsMinX = 11, BoundsMinY = 0, BoundsMaxX = 20, BoundsMaxY = 10,
                    IsUnlocked = false
                },
                new FarmExpansionZone
                {
                    ZoneId = "zone_lore_fonte", ZoneType = FarmZoneType.Lore,
                    RequiredLevel = FarmPropertyLevel.Level5_FullFarm,
                    BoundsMinX = 21, BoundsMinY = 0, BoundsMaxX = 30, BoundsMaxY = 10,
                    IsUnlocked = false
                },
                new FarmExpansionZone
                {
                    ZoneId = "zone_path_lake", ZoneType = FarmZoneType.Path,
                    RequiredLevel = FarmPropertyLevel.Level1_InicialPlot,
                    BoundsMinX = 0, BoundsMinY = 11, BoundsMaxX = 5, BoundsMaxY = 15,
                    IsUnlocked = true
                },
                new FarmExpansionZone
                {
                    ZoneId = "zone_endgame_quarry", ZoneType = FarmZoneType.Endgame,
                    RequiredLevel = FarmPropertyLevel.Level5_FullFarm,
                    BoundsMinX = 31, BoundsMinY = 0, BoundsMaxX = 40, BoundsMaxY = 10,
                    IsUnlocked = false
                }
            };
            _validator = new FarmExpansionValidator(_zones);
        }

        [Test]
        public void CanPlaceBuilding_FreeUnlockedZone_Allowed()
        {
            var result = _validator.CanPlaceBuildingAt(5, 5, FarmPropertyLevel.Level1_InicialPlot);
            Assert.IsTrue(result.Allowed);
            Assert.AreEqual("zone_free_level1", result.ZoneId);
        }

        [Test]
        public void CanPlaceBuilding_BlockedZone_Denied()
        {
            var result = _validator.CanPlaceBuildingAt(15, 5, FarmPropertyLevel.Level1_InicialPlot);
            Assert.IsFalse(result.Allowed);
            Assert.AreEqual("ZoneNotUnlocked", result.BlockedReason);
        }

        [Test]
        public void CanPlaceBuilding_LoreZone_PermanentlyBlocked()
        {
            var result = _validator.CanPlaceBuildingAt(25, 5, FarmPropertyLevel.Level5_FullFarm);
            Assert.IsFalse(result.Allowed);
            Assert.AreEqual("LoreOrEndgameZone", result.BlockedReason);
        }

        [Test]
        public void CanPlaceBuilding_EndgameZone_PermanentlyBlocked()
        {
            var result = _validator.CanPlaceBuildingAt(35, 5, FarmPropertyLevel.Level5_FullFarm);
            Assert.IsFalse(result.Allowed);
            Assert.AreEqual("LoreOrEndgameZone", result.BlockedReason);
        }

        [Test]
        public void CanPlaceBuilding_PathZone_Blocked()
        {
            var result = _validator.CanPlaceBuildingAt(2, 12, FarmPropertyLevel.Level1_InicialPlot);
            Assert.IsFalse(result.Allowed);
            Assert.AreEqual("PathMustRemainClear", result.BlockedReason);
        }

        [Test]
        public void CanPlaceBuilding_OutsideAllZones_Blocked()
        {
            var result = _validator.CanPlaceBuildingAt(200, 200, FarmPropertyLevel.Level1_InicialPlot);
            Assert.IsFalse(result.Allowed);
            Assert.AreEqual("OutsideKnownZones", result.BlockedReason);
        }

        [Test]
        public void UnlockZone_SufficientLevel_Succeeds()
        {
            var unlocked = _validator.UnlockZone("zone_blocked_level2", FarmPropertyLevel.Level2_OpenFarm);
            Assert.IsTrue(unlocked);
            var result = _validator.CanPlaceBuildingAt(15, 5, FarmPropertyLevel.Level2_OpenFarm);
            Assert.IsTrue(result.Allowed);
        }

        [Test]
        public void UnlockZone_InsufficientLevel_Fails()
        {
            var unlocked = _validator.UnlockZone("zone_blocked_level2", FarmPropertyLevel.Level1_InicialPlot);
            Assert.IsFalse(unlocked);
        }

        [Test]
        public void UnlockZone_LoreZone_CannotUnlock()
        {
            var unlocked = _validator.UnlockZone("zone_lore_fonte", FarmPropertyLevel.Level5_FullFarm);
            Assert.IsFalse(unlocked);
        }
    }
}
