using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Forage;
using CindarsHope.Farm.Fishing;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmForageFishingTests
    {
        private Dictionary<string, ForageDefinition> _forageDefs;
        private FarmForageSpawnService _forageService;
        private Dictionary<string, FarmFishingSpotDefinition> _spotDefs;
        private FarmFishingService _fishService;

        [SetUp]
        public void Setup()
        {
            _forageDefs = new Dictionary<string, ForageDefinition>
            {
                ["herb_spring"] = new ForageDefinition
                {
                    ForageId = "herb_spring", ItemId = "item_herb", DisplayName = "Erva de Primavera",
                    AllowedSeasons = new List<string> { "Spring" }, RespawnAfterDays = 2,
                    CanRespawnSameSeason = true, Rarity = ForageRarity.Common
                },
                ["forage_lunar"] = new ForageDefinition
                {
                    ForageId = "forage_lunar", ItemId = "item_lunar_forage",
                    Rarity = ForageRarity.LunarFuture
                },
                ["forage_lore"] = new ForageDefinition
                {
                    ForageId = "forage_lore", ItemId = "item_lore", IsLoreProtected = true
                }
            };
            _forageService = new FarmForageSpawnService(_forageDefs);

            _spotDefs = new Dictionary<string, FarmFishingSpotDefinition>
            {
                ["lake_farm"] = new FarmFishingSpotDefinition
                {
                    FishingSpotId = "lake_farm", DisplayName = "Lago da Fazenda",
                    ZoneId = "zone_farm_lake", WaterBodyId = "lake_01",
                    CatchTableId = "catch_lake_common", AllowsEndgameFish = false, DailyCatchSoftLimit = 5
                },
                ["lake_lore"] = new FarmFishingSpotDefinition
                {
                    FishingSpotId = "lake_lore", IsLoreProtected = true
                }
            };
            _fishService = new FarmFishingService(_spotDefs);
        }

        private ForageSpawnState FreshHerb(string zone = "zone_farm") =>
            new ForageSpawnState
            {
                ForageInstanceId = "herb_01", ForageId = "herb_spring",
                ZoneId = zone, CurrentState = ForageNodeState.Available
            };

        [Test]
        public void Forage_Collect_HappyPath()
        {
            var spawn = FreshHerb();
            var result = _forageService.Collect(spawn, 1, "Spring");
            Assert.IsTrue(result.Success);
            Assert.AreEqual("item_herb", result.ItemId);
            Assert.AreEqual(ForageNodeState.Collected, spawn.CurrentState);
        }

        [Test]
        public void Forage_Collect_SecondCall_Fails()
        {
            var spawn = FreshHerb();
            _forageService.Collect(spawn, 1, "Spring");
            var second = _forageService.Collect(spawn, 1, "Spring");
            Assert.IsFalse(second.Success);
            Assert.AreEqual("ForageNotAvailable", second.FailureReason);
        }

        [Test]
        public void Forage_WrongSeason_Fails()
        {
            var spawn = FreshHerb();
            var result = _forageService.Collect(spawn, 1, "Winter");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("WrongSeason", result.FailureReason);
        }

        [Test]
        public void Forage_ForbiddenZone_Fails()
        {
            var spawn = FreshHerb("zone_fonte");
            var result = _forageService.Collect(spawn, 1, "Spring");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("ForbiddenZone", result.FailureReason);
        }

        [Test]
        public void Forage_LoreProtected_Fails()
        {
            var spawn = new ForageSpawnState { ForageInstanceId = "l_01", ForageId = "forage_lore", CurrentState = ForageNodeState.Available };
            var result = _forageService.Collect(spawn, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("ForageNotCollectable", result.FailureReason);
        }

        [Test]
        public void Forage_EndgameReserved_Fails()
        {
            var spawn = new ForageSpawnState { ForageInstanceId = "lu_01", ForageId = "forage_lunar", CurrentState = ForageNodeState.Available };
            var result = _forageService.Collect(spawn, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("ForageNotCollectable", result.FailureReason);
        }

        [Test]
        public void Forage_Respawn_AfterEligibleDay()
        {
            var spawn = FreshHerb();
            _forageService.Collect(spawn, 1, "Spring");
            Assert.IsFalse(spawn.IsAvailable);
            var refreshed = _forageService.TryRespawn(spawn, 3, "Spring"); // day 1 + 2 = day 3
            Assert.IsTrue(refreshed);
            Assert.IsTrue(spawn.IsAvailable);
        }

        [Test]
        public void Forage_Respawn_BeforeEligibleDay_NoOp()
        {
            var spawn = FreshHerb();
            _forageService.Collect(spawn, 1, "Spring");
            Assert.IsFalse(_forageService.TryRespawn(spawn, 2, "Spring")); // too early
        }

        [Test]
        public void Fishing_HappyPath_ReturnsFish()
        {
            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1 };
            var result = _fishService.AttemptCatch("lake_farm", daily, 1);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(string.IsNullOrEmpty(result.FishItemId));
        }

        [Test]
        public void Fishing_NoEndgameFish_OnFarmLake()
        {
            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1 };
            var result = _fishService.AttemptCatch("lake_farm", daily, 1);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(FishRarity.Common, result.Rarity);
        }

        [Test]
        public void Fishing_SoftLimit_BlocksAfterLimit()
        {
            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1, CatchesToday = 5 };
            var result = _fishService.AttemptCatch("lake_farm", daily, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("DailySoftLimitReached", result.FailureReason);
        }

        [Test]
        public void Fishing_LoreProtected_Fails()
        {
            var result = _fishService.AttemptCatch("lake_lore", null, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("LoreProtected", result.FailureReason);
        }
    }
}
