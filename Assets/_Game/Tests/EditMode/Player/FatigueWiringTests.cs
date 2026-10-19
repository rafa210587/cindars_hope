using CindarsHope.Core.Events;
using CindarsHope.Player.Conditions;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// F16 — wiring de fadiga/sono/colapso. O FatigueSystem (WAVE 05) já tem testes próprios;
    /// aqui validamos o mapeamento de relógio, o gatilho de colapso e os ciclos de recuperação.
    /// </summary>
    public class FatigueWiringTests
    {
        // ------------------------------------------------------------------ relógio

        [Test]
        public void HourFromPhase_DayStart_Is6()
        {
            Assert.AreEqual(6f, PlayerConditionService.HourFromPhase(GamePhaseChangedEvent.GamePhase.Day, 0f), 0.01f);
        }

        [Test]
        public void HourFromPhase_DayEnd_Is20()
        {
            Assert.AreEqual(20f, PlayerConditionService.HourFromPhase(GamePhaseChangedEvent.GamePhase.Day, 1f), 0.01f);
        }

        [Test]
        public void HourFromPhase_NightStart_Is20()
        {
            Assert.AreEqual(20f, PlayerConditionService.HourFromPhase(GamePhaseChangedEvent.GamePhase.Night, 0f), 0.01f);
        }

        [Test]
        public void HourFromPhase_Night60Percent_Is2AM()
        {
            Assert.AreEqual(2f, PlayerConditionService.HourFromPhase(GamePhaseChangedEvent.GamePhase.Night, 0.6f), 0.01f);
        }

        // ------------------------------------------------------------------ colapso

        [Test]
        public void IsCollapseHour_OnlyAfter2AMDuringNight()
        {
            Assert.IsFalse(PlayerConditionService.IsCollapseHour(GamePhaseChangedEvent.GamePhase.Night, 0.59f), "01:54 ainda não colapsa.");
            Assert.IsTrue(PlayerConditionService.IsCollapseHour(GamePhaseChangedEvent.GamePhase.Night, 0.6f), "02:00 colapsa.");
            Assert.IsTrue(PlayerConditionService.IsCollapseHour(GamePhaseChangedEvent.GamePhase.Night, 0.9f));
            Assert.IsFalse(PlayerConditionService.IsCollapseHour(GamePhaseChangedEvent.GamePhase.Day, 0.9f), "Dia nunca colapsa.");
        }

        // ------------------------------------------------------------------ acúmulo

        [Test]
        public void FatigueSystem_TimePassing_2PerHour()
        {
            var system = new FatigueSystem();
            system.AddTimePassingFatigue(5f);
            Assert.AreEqual(10f, system.State.FatigueValue, 0.01f);
        }

        [Test]
        public void FatigueSystem_StaminaSpend_10Percent()
        {
            var system = new FatigueSystem();
            system.AddFatigueFromStaminaSpend(40f);
            Assert.AreEqual(4f, system.State.FatigueValue, 0.01f);
        }

        [Test]
        public void FatigueThresholds_Mapping()
        {
            Assert.AreEqual(FatigueThreshold.Rested, FatigueThresholdExtensions.FromValue(10f));
            Assert.AreEqual(FatigueThreshold.Tired, FatigueThresholdExtensions.FromValue(60f));
            Assert.AreEqual(FatigueThreshold.VeryTired, FatigueThresholdExtensions.FromValue(80f));
            Assert.AreEqual(FatigueThreshold.Exhausted, FatigueThresholdExtensions.FromValue(95f));
        }

        // ------------------------------------------------------------------ recuperação

        [Test]
        public void SleepRecovery_GoodSleep_RecoversMoreThanLateSleep()
        {
            var system = new FatigueSystem();
            system.State.FatigueValue = 80f;
            var good = system.ApplySleepRecovery(new SleepRecoveryContext { WentToBedLate = false, HungerAtSleep = 1f });
            var afterGood = system.State.FatigueValue;

            var lateSystem = new FatigueSystem();
            lateSystem.State.FatigueValue = 80f;
            var late = lateSystem.ApplySleepRecovery(new SleepRecoveryContext { WentToBedLate = true, HungerAtSleep = 1f });
            var afterLate = lateSystem.State.FatigueValue;

            Assert.Greater(good.FatigueReduction, late.FatigueReduction, "Dormir cedo recupera mais.");
            Assert.Less(afterGood, afterLate);
        }

        [Test]
        public void CollapseRecovery_LeavesResidualFatigue()
        {
            // Simula o cálculo do Collapse(): recuperação tardia + residual de 25.
            var system = new FatigueSystem();
            system.State.FatigueValue = 100f;
            system.ApplySleepRecovery(new SleepRecoveryContext { WentToBedLate = true, HungerAtSleep = 1f });
            system.AddFatigue(new FatigueGainContext
            {
                Source = FatigueGainSource.StatusEffect,
                BaseAmount = 25f,
                Multiplier = 1f
            });

            Assert.Greater(system.State.FatigueValue, 25f, "Colapso deixa fadiga residual relevante.");
            Assert.Less(system.State.FatigueValue, 100f, "Mas ainda recupera parte.");
        }

        [Test]
        public void FatigueState_ClampsAt0And100()
        {
            var state = new FatigueState { FatigueValue = 150f };
            state.Clamp();
            Assert.AreEqual(100f, state.FatigueValue);

            state.FatigueValue = -10f;
            state.Clamp();
            Assert.AreEqual(0f, state.FatigueValue);
        }
    }
}
