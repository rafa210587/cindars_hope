using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.City.Layout;
using CindarsHope.City.Schedule;
using CindarsHope.City.Validation;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class CityLayoutScheduleValidationTests
    {
        private CityLayoutScheduleValidator _validator;
        private NpcScheduleResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _validator = new CityLayoutScheduleValidator();
            _resolver = new NpcScheduleResolver();
        }

        // --- Door tests ---

        [Test]
        public void Door_ValidTarget_NoIssues()
        {
            var door = new DoorTriggerDefinition
            {
                DoorTriggerId = "door_shop_entry",
                SourceLocationId = "city_market",
                TargetSceneId = "ShopInterior",
                TargetSpawnPointId = "spawn_shop_entrance"
            };
            var issues = _validator.ValidateDoor(door);
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Door_MissingTarget_IsBlocker()
        {
            var door = new DoorTriggerDefinition { DoorTriggerId = "door_broken", SourceLocationId = "city" };
            var issues = _validator.ValidateDoor(door);
            Assert.IsTrue(issues.Exists(i => i.Code == "DOOR_INVALID_TARGET" && i.IsBlocker));
        }

        [Test]
        public void HiddenDoor_NoGate_NonBlockerWarning()
        {
            var door = new DoorTriggerDefinition
            {
                DoorTriggerId = "door_secret",
                TargetSceneId = "SecretRoom",
                TargetSpawnPointId = "spawn_01",
                IsHiddenDoor = true
            };
            var issues = _validator.ValidateDoor(door);
            Assert.IsTrue(issues.Exists(i => i.Code == "HIDDEN_DOOR_NO_GATE" && !i.IsBlocker));
        }

        [Test]
        public void Door_OpenHoursRule_WrapsMidnight()
        {
            var rule = new OpenHoursRule { OpenHour = 21, CloseHour = 3 };
            Assert.IsTrue(rule.IsOpenAt(22));
            Assert.IsTrue(rule.IsOpenAt(1));
            Assert.IsFalse(rule.IsOpenAt(10));
        }

        // --- Bed tests ---

        [Test]
        public void Bed_NpcOwned_NotPlayerUsable_OK()
        {
            var bed = new BedDefinition { BedId = "bed_liora", OwnerNpcIds = new List<string> { "npc_liora" }, BedType = BedType.Simple };
            var issues = _validator.ValidateBed(bed);
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Bed_NpcOwned_PlayerUsable_NotGuest_IsBlocker()
        {
            var bed = new BedDefinition
            {
                BedId = "bed_liora",
                OwnerNpcIds = new List<string> { "npc_liora" },
                BedType = BedType.Simple,
                CanPlayerUse = true
            };
            var issues = _validator.ValidateBed(bed);
            Assert.IsTrue(issues.Exists(i => i.Code == "BED_PLAYER_ON_NPC_BED" && i.IsBlocker));
        }

        [Test]
        public void Bed_GuestType_PlayerUsable_OK()
        {
            var bed = new BedDefinition
            {
                BedId = "bed_inn_guest",
                OwnerNpcIds = new List<string> { "npc_innkeeper" },
                BedType = BedType.Guest,
                CanPlayerUse = true
            };
            var issues = _validator.ValidateBed(bed);
            Assert.AreEqual(0, issues.Count);
        }

        // --- Schedule period tests ---

        [Test]
        public void SchedulePeriod_FromHour_Morning()
        {
            Assert.AreEqual(SchedulePeriod.Morning, SchedulePeriodHelper.FromHour(7));
        }

        [Test]
        public void SchedulePeriod_FromHour_Night()
        {
            Assert.AreEqual(SchedulePeriod.Night, SchedulePeriodHelper.FromHour(22));
        }

        [Test]
        public void SchedulePeriod_FromHour_SleepLateNight()
        {
            Assert.AreEqual(SchedulePeriod.SleepLateNight, SchedulePeriodHelper.FromHour(3));
        }

        // --- Schedule resolver tests ---

        [Test]
        public void Resolver_DefaultBlock_ReturnsCorrectLocation()
        {
            var schedule = new NpcScheduleDefinition
            {
                ScheduleId = "schedule_liora",
                NpcId = "npc_liora",
                DefaultPeriodBlocks = new List<SchedulePeriodBlock>
                {
                    new SchedulePeriodBlock { Period = SchedulePeriod.WorkStart, LocationId = "shop_seeds", WaypointId = "wp_counter" }
                },
                FallbackWaypointId = "wp_home"
            };
            var ctx = new ScheduleResolveContext { CurrentHour = 10 };
            var result = _resolver.Resolve(schedule, ctx);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("shop_seeds", result.LocationId);
            Assert.IsFalse(result.WasModified);
        }

        [Test]
        public void Resolver_FestivalOverride_TakesPriority()
        {
            var schedule = new NpcScheduleDefinition
            {
                NpcId = "npc_liora",
                DefaultPeriodBlocks = new List<SchedulePeriodBlock>
                {
                    new SchedulePeriodBlock { Period = SchedulePeriod.WorkStart, LocationId = "shop_seeds" }
                },
                FestivalOverrides = new List<ScheduleModifierBlock>
                {
                    new ScheduleModifierBlock { ModifierType = ScheduleModifierType.Festival, AffectedPeriod = SchedulePeriod.WorkStart, OverrideLocationId = "city_square_festival" }
                },
                FallbackWaypointId = "wp_home"
            };
            var ctx = new ScheduleResolveContext { CurrentHour = 10, IsFestivalDay = true };
            var result = _resolver.Resolve(schedule, ctx);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("city_square_festival", result.LocationId);
            Assert.IsTrue(result.WasModified);
            Assert.AreEqual("Festival", result.ModifierApplied);
        }

        [Test]
        public void Resolver_NyxLunar_OpensNightShop()
        {
            var schedule = new NpcScheduleDefinition
            {
                NpcId = "npc_yael",
                DefaultPeriodBlocks = new List<SchedulePeriodBlock>
                {
                    new SchedulePeriodBlock { Period = SchedulePeriod.Night, LocationId = "home_yael" }
                },
                LunarModifiers = new List<ScheduleModifierBlock>
                {
                    new ScheduleModifierBlock { ModifierType = ScheduleModifierType.Nyx, ConditionId = "Nyx", AffectedPeriod = SchedulePeriod.Night, OverrideLocationId = "shop_nocturna" }
                },
                FallbackWaypointId = "wp_yael_home"
            };
            var ctx = new ScheduleResolveContext { CurrentHour = 22, ActiveLunarPhase = "Nyx" };
            var result = _resolver.Resolve(schedule, ctx);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("shop_nocturna", result.LocationId);
            Assert.IsTrue(result.WasModified);
        }

        [Test]
        public void Resolver_RainModifier_ChangesOutdoorRoute()
        {
            var schedule = new NpcScheduleDefinition
            {
                NpcId = "npc_sylveth",
                DefaultPeriodBlocks = new List<SchedulePeriodBlock>
                {
                    new SchedulePeriodBlock { Period = SchedulePeriod.Morning, LocationId = "garden_outdoor" }
                },
                WeatherModifiers = new List<ScheduleModifierBlock>
                {
                    new ScheduleModifierBlock { ModifierType = ScheduleModifierType.Rain, AffectedPeriod = SchedulePeriod.Morning, OverrideLocationId = "shop_herbs_indoor" }
                },
                FallbackWaypointId = "wp_shop"
            };
            var ctx = new ScheduleResolveContext { CurrentHour = 7, IsRaining = true };
            var result = _resolver.Resolve(schedule, ctx);
            Assert.AreEqual("shop_herbs_indoor", result.LocationId);
            Assert.IsTrue(result.WasModified);
        }

        [Test]
        public void Resolver_NullSchedule_Fails()
        {
            var result = _resolver.Resolve(null, new ScheduleResolveContext());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Schedule_Validator_NoNpcId_IsBlocker()
        {
            var schedule = new NpcScheduleDefinition { ScheduleId = "sched_broken" };
            var issues = _validator.ValidateSchedule(schedule);
            Assert.IsTrue(issues.Exists(i => i.Code == "SCHEDULE_NO_NPC_ID" && i.IsBlocker));
        }
    }
}
