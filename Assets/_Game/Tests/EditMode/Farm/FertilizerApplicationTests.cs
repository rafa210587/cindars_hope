using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Fertilizer;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FertilizerApplicationTests
    {
        private Dictionary<string, FertilizerDefinition> _defs;
        private Dictionary<string, SoilModifierState> _plotModifiers;
        private FertilizerApplicationService _service;

        [SetUp]
        public void Setup()
        {
            _defs = new Dictionary<string, FertilizerDefinition>();
            _plotModifiers = new Dictionary<string, SoilModifierState>();

            var simple = new FertilizerDefinition
            {
                FertilizerId = "fertilizer_simple",
                Tier = FertilizerTier.Simple,
                QualityModifier = 10f,
                StackingPolicy = FertilizerStackingPolicy.ReplaceSameTier,
                IsEndgameReserved = false,
                RequiredFarmLevel = 0
            };
            var lunar = new FertilizerDefinition
            {
                FertilizerId = "fertilizer_lunar",
                Tier = FertilizerTier.LunarFuture,
                IsEndgameReserved = true
            };
            _defs["fertilizer_simple"] = simple;
            _defs["fertilizer_lunar"] = lunar;
            _service = new FertilizerApplicationService(_defs, _plotModifiers);
        }

        [Test]
        public void Apply_Simple_Succeeds()
        {
            var result = _service.Apply("plot_01", "fertilizer_simple", 1);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.AppliedModifier);
            Assert.AreEqual(10f, result.AppliedModifier.QualityModifierSnapshot);
        }

        [Test]
        public void Apply_Lunar_Blocked_EndgameReserved()
        {
            var result = _service.Apply("plot_01", "fertilizer_lunar", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(FertilizerApplicationFailure.EndgameReserved, result.FailureReason);
        }

        [Test]
        public void Apply_Unknown_Fails_NotFound()
        {
            var result = _service.Apply("plot_01", "fertilizer_unknown", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(FertilizerApplicationFailure.FertilizerNotFound, result.FailureReason);
        }

        [Test]
        public void HasActiveFertilizer_TrueAfterApply()
        {
            _service.Apply("plot_01", "fertilizer_simple", 1);
            Assert.IsTrue(_service.HasActiveFertilizer("plot_01"));
        }

        [Test]
        public void HasActiveFertilizer_FalseWhenNoneApplied()
        {
            Assert.IsFalse(_service.HasActiveFertilizer("plot_02"));
        }

        [Test]
        public void StackingPolicy_RejectIfAny_BlocksSecondApplication()
        {
            var rejectDef = new FertilizerDefinition
            {
                FertilizerId = "fertilizer_reject",
                Tier = FertilizerTier.Simple,
                StackingPolicy = FertilizerStackingPolicy.RejectIfAnyFertilizer,
                IsEndgameReserved = false
            };
            _defs["fertilizer_reject"] = rejectDef;
            _service.Apply("plot_01", "fertilizer_simple", 1);
            var result = _service.Apply("plot_01", "fertilizer_reject", 2);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(FertilizerApplicationFailure.StackingRejected, result.FailureReason);
        }

        [Test]
        public void SoilModifierState_ConsumptionMakesInactive()
        {
            var modifier = new SoilModifierState { RemainingUses = 1, IsActive = true };
            modifier.Consume();
            Assert.IsFalse(modifier.IsActive);
            Assert.AreEqual(0, modifier.RemainingUses);
        }

        [Test]
        public void SoilModifierState_ExpiresOnCorrectDay()
        {
            var modifier = new SoilModifierState { ExpiresDay = 5, IsActive = true };
            Assert.IsFalse(modifier.IsExpired(4));
            Assert.IsTrue(modifier.IsExpired(5));
        }

        [Test]
        public void FertilizerDefaults_ContainSimpleAndImproved()
        {
            var defaults = FertilizerDefinition.GetDefaults();
            Assert.IsTrue(defaults.Exists(d => d.FertilizerId == "fertilizer_simple"));
            Assert.IsTrue(defaults.Exists(d => d.FertilizerId == "fertilizer_improved"));
        }

        [Test]
        public void FertilizerDefaults_LunarIsEndgameReserved()
        {
            var defaults = FertilizerDefinition.GetDefaults();
            var lunar = defaults.Find(d => d.FertilizerId == "fertilizer_lunar");
            Assert.IsNotNull(lunar);
            Assert.IsTrue(lunar.IsEndgameReserved);
        }
    }
}
