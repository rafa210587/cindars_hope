using NUnit.Framework;
using CindarsHope.Player.Conditions;

namespace CindarsHope.Tests.EditMode.Player
{
    [TestFixture]
    public class FatigueSystemTests
    {
        private FatigueSystem _system;

        [SetUp]
        public void Setup()
        {
            _system = new FatigueSystem(new FatigueState { FatigueValue = 0f });
        }

        [Test]
        public void AddFatigue_IncreasesValue()
        {
            _system.AddFatigue(new FatigueGainContext { Source = FatigueGainSource.TimePassing, BaseAmount = 10f, Multiplier = 1.0f });
            Assert.AreEqual(10f, _system.State.FatigueValue, 0.01f);
        }

        [Test]
        public void FatigueValue_Clamped_At_100()
        {
            _system.State.FatigueValue = 95f;
            _system.AddFatigue(new FatigueGainContext { Source = FatigueGainSource.TimePassing, BaseAmount = 20f, Multiplier = 1.0f });
            Assert.AreEqual(100f, _system.State.FatigueValue, 0.01f);
        }

        [Test]
        public void FatigueValue_Clamped_At_0()
        {
            _system.State.FatigueValue = 0f;
            var result = _system.ApplySleepRecovery(new SleepRecoveryContext { SleepStartHour = 22, SleepEndHour = 6, WentToBedLate = false, HungerAtSleep = 1.0f });
            Assert.AreEqual(0f, _system.State.FatigueValue);
        }

        [Test]
        public void Threshold_Rested_At_Zero()
        {
            Assert.AreEqual(FatigueThreshold.Rested, _system.CurrentThreshold);
        }

        [Test]
        public void Threshold_Exhausted_At_90Plus()
        {
            _system.State.FatigueValue = 92f;
            Assert.AreEqual(FatigueThreshold.Exhausted, _system.CurrentThreshold);
            Assert.IsTrue(_system.IsExhausted);
        }

        [Test]
        public void Threshold_Tired_At_50()
        {
            _system.State.FatigueValue = 55f;
            Assert.AreEqual(FatigueThreshold.Tired, _system.CurrentThreshold);
        }

        [Test]
        public void StaminaSpend_AddsFatigue()
        {
            _system.AddFatigueFromStaminaSpend(30f);
            Assert.Greater(_system.State.FatigueValue, 0f);
        }

        [Test]
        public void StaminaSpend_LowHunger_AddMoreFatigue()
        {
            var sysNormalHunger = new FatigueSystem();
            var sysLowHunger = new FatigueSystem();
            sysNormalHunger.AddFatigueFromStaminaSpend(30f, 1.0f);
            sysLowHunger.AddFatigueFromStaminaSpend(30f, 0.1f); // low hunger
            Assert.Greater(sysLowHunger.State.FatigueValue, sysNormalHunger.State.FatigueValue);
        }

        [Test]
        public void SleepRecovery_ReducesFatigue()
        {
            _system.State.FatigueValue = 80f;
            _system.ApplySleepRecovery(new SleepRecoveryContext
            {
                SleepStartHour = 21, SleepEndHour = 6, WentToBedLate = false, HungerAtSleep = 1.0f
            });
            Assert.Less(_system.State.FatigueValue, 80f);
        }

        [Test]
        public void SleepRecovery_LateTobed_LessRecovery()
        {
            var sys1 = new FatigueSystem(new FatigueState { FatigueValue = 80f });
            var sys2 = new FatigueSystem(new FatigueState { FatigueValue = 80f });
            sys1.ApplySleepRecovery(new SleepRecoveryContext { WentToBedLate = false, HungerAtSleep = 1.0f });
            sys2.ApplySleepRecovery(new SleepRecoveryContext { WentToBedLate = true, HungerAtSleep = 1.0f });
            Assert.Less(sys1.State.FatigueValue, sys2.State.FatigueValue); // sys1 recovered more
        }

        [Test]
        public void CaveExploration_HigherFatigue()
        {
            var sysFarm = new FatigueSystem();
            var sysCave = new FatigueSystem();
            sysFarm.AddFatigue(new FatigueGainContext { Source = FatigueGainSource.RepeatedPhysicalAction, BaseAmount = 10f, Multiplier = 1.0f });
            sysCave.AddFatigue(new FatigueGainContext { Source = FatigueGainSource.CaveExploration, BaseAmount = 10f, Multiplier = 1.0f });
            Assert.Greater(sysCave.State.FatigueValue, sysFarm.State.FatigueValue);
        }

        [Test]
        public void PlayerConditionSnapshot_ReflectsConditions()
        {
            _system.State.FatigueValue = 95f;
            var snapshot = new PlayerConditionSnapshot
            {
                HP = 100f, MaxHP = 100f, Stamina = 20f, MaxStamina = 100f,
                Hunger = 10f, MaxHunger = 100f, Fatigue = _system.State.FatigueValue,
                FatigueThreshold = _system.CurrentThreshold
            };
            Assert.IsTrue(snapshot.IsExhausted);
            Assert.IsTrue(snapshot.IsLowStamina);
            Assert.IsTrue(snapshot.IsHungry);
        }
    }
}
