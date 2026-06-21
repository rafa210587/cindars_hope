using System;
using CindarsHope.Combat.Feel;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_71 — combat feel pass: deterministic logic for hit-stop (clamp 40-60 ms, restore guard,
    /// no-stack), screen shake decay/return-to-origin + no-op, and the HudSuppressionChangedEvent
    /// shape/defaults (phase 0 = empty = all visible). MonoBehaviour timeScale/camera restoration and
    /// the subjective "feel" are validated in the human Play Mode scenario.
    /// </summary>
    public class CombatFeelTests
    {
        // ------------------------------------------------ hit-stop clamp (CA-1)

        [Test]
        public void ClampHitStopMs_ClampsToCanonicalWindow()
        {
            Assert.AreEqual(40f, CombatFeelTuning.ClampHitStopMs(10f), "Below 40 ms clamps up to 40.");
            Assert.AreEqual(60f, CombatFeelTuning.ClampHitStopMs(120f), "Above 60 ms clamps down to 60.");
            Assert.AreEqual(50f, CombatFeelTuning.ClampHitStopMs(50f), "In-range value is unchanged.");
            Assert.AreEqual(40f, CombatFeelTuning.ClampHitStopMs(40f), "Lower bound inclusive.");
            Assert.AreEqual(60f, CombatFeelTuning.ClampHitStopMs(60f), "Upper bound inclusive.");
        }

        [Test]
        public void HitStopSeconds_IsClampedMsOverThousand()
        {
            Assert.AreEqual(0.040f, CombatFeelTuning.HitStopSeconds(10f), 1e-6f, "Clamped 40 ms = 0.040 s.");
            Assert.AreEqual(0.060f, CombatFeelTuning.HitStopSeconds(999f), 1e-6f, "Clamped 60 ms = 0.060 s.");
            Assert.AreEqual(0.050f, CombatFeelTuning.HitStopSeconds(50f), 1e-6f, "50 ms = 0.050 s.");
        }

        [Test]
        public void HitStopWindow_IsWithin40To60Ms()
        {
            Assert.GreaterOrEqual(CombatFeelTuning.HitStopMinMs, 40f);
            Assert.LessOrEqual(CombatFeelTuning.HitStopMaxMs, 60f);
            Assert.GreaterOrEqual(CombatFeelTuning.HitStopDefaultMs, CombatFeelTuning.HitStopMinMs);
            Assert.LessOrEqual(CombatFeelTuning.HitStopDefaultMs, CombatFeelTuning.HitStopMaxMs);
        }

        // ------------------------------------------------ no-stack + pause guard (CA-1)

        [Test]
        public void CanStartHitStop_FalseWhenAlreadyActive()
        {
            Assert.IsFalse(CombatFeelTuning.CanStartHitStop(1f, alreadyActive: true),
                "A hit-stop must not stack on top of an active one.");
        }

        [Test]
        public void CanStartHitStop_FalseWhenClockAlreadyFrozenByPause()
        {
            Assert.IsFalse(CombatFeelTuning.CanStartHitStop(0f, alreadyActive: false),
                "A hit-stop must not start over an active pause/modal (timescale ~0).");
            Assert.IsFalse(CombatFeelTuning.CanStartHitStop(CombatFeelTuning.PauseTimeScaleEpsilon / 2f, false),
                "Near-zero timescale is treated as paused.");
        }

        [Test]
        public void CanStartHitStop_TrueAtNormalTimeScale()
        {
            Assert.IsTrue(CombatFeelTuning.CanStartHitStop(1f, alreadyActive: false),
                "At normal timescale and not active, a hit-stop may start.");
        }

        // ------------------------------------------------ shake decay / return-to-origin (CA-2)

        [Test]
        public void ShakeDecay_FullAtStartZeroAtEnd()
        {
            Assert.AreEqual(1f, CombatFeelTuning.ShakeDecay(0f, 0.2f), 1e-6f, "Full magnitude at start.");
            Assert.AreEqual(0.5f, CombatFeelTuning.ShakeDecay(0.1f, 0.2f), 1e-6f, "Half-way = half decay.");
            Assert.AreEqual(0f, CombatFeelTuning.ShakeDecay(0.2f, 0.2f), 1e-6f, "Zero at the end (return to origin).");
            Assert.AreEqual(0f, CombatFeelTuning.ShakeDecay(0.5f, 0.2f), 1e-6f, "Past the end stays at zero.");
        }

        [Test]
        public void ShakeMagnitude_ReturnsToZeroAtEnd()
        {
            Assert.AreEqual(0f, CombatFeelTuning.ShakeMagnitude(0.18f, 0.22f, 0.22f), 1e-6f,
                "Magnitude is exactly 0 at the end so the camera returns to origin.");
            Assert.Greater(CombatFeelTuning.ShakeMagnitude(0.18f, 0f, 0.22f), 0f,
                "Magnitude is positive at the start.");
        }

        [Test]
        public void ClampAmplitudeAndDuration_StayLight()
        {
            Assert.AreEqual(0f, CombatFeelTuning.ClampAmplitude(-1f), "Negative amplitude clamps to 0.");
            Assert.AreEqual(CombatFeelTuning.ShakeMaxAmplitude, CombatFeelTuning.ClampAmplitude(99f),
                "Amplitude clamps to the light max.");
            Assert.AreEqual(0f, CombatFeelTuning.ClampDuration(-1f), "Negative duration clamps to 0.");
            Assert.AreEqual(CombatFeelTuning.ShakeMaxDurationSeconds, CombatFeelTuning.ClampDuration(99f),
                "Duration clamps to the light max.");
        }

        [Test]
        public void ShakeMagnitude_ZeroDurationIsNoOp()
        {
            Assert.AreEqual(0f, CombatFeelTuning.ShakeMagnitude(0.18f, 0f, 0f), 1e-6f,
                "Zero duration yields zero magnitude (no-op).");
        }

        // ------------------------------------------------ HudSuppressionChangedEvent shape (CA-3)

        [Test]
        public void HudSuppression_Phase0_IsAllVisibleEmptySet()
        {
            var evt = HudSuppressionChangedEvent.RestoreAll();
            Assert.AreEqual(0, evt.Phase, "RestoreAll is phase 0.");
            Assert.IsNotNull(evt.HiddenWidgets, "Hidden set is never null.");
            Assert.AreEqual(0, evt.HiddenWidgets.Count, "Phase 0 hides nothing = HUD fully visible.");
        }

        [Test]
        public void HudSuppression_NullWidgets_NormalizeToEmpty()
        {
            var evt = new HudSuppressionChangedEvent(2, null);
            Assert.IsNotNull(evt.HiddenWidgets, "Null hidden list normalizes to empty, never null.");
            Assert.AreEqual(0, evt.HiddenWidgets.Count);
        }

        [Test]
        public void HudSuppression_NegativePhase_NormalizesToZero()
        {
            var evt = new HudSuppressionChangedEvent(-5, Array.Empty<string>());
            Assert.AreEqual(0, evt.Phase, "Negative phase normalizes to the safe restore phase 0.");
        }

        [Test]
        public void HudSuppression_CarriesWidgetIds()
        {
            var evt = new HudSuppressionChangedEvent(1, new[] { "boss_hp_bar", "minimap" });
            Assert.AreEqual(1, evt.Phase);
            Assert.AreEqual(2, evt.HiddenWidgets.Count);
            CollectionAssert.Contains((System.Collections.IEnumerable)evt.HiddenWidgets, "minimap");
        }
    }
}
