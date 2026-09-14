using CindarsHope.Combat.Telemetry;
using CindarsHope.Skills;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class SkillBalanceObservationEvaluatorTests
    {
        [Test]
        public void MedianUsesRepeatedSamplesOfOneScenario_AndRejectsOutOfBandTtk()
        {
            Assert.IsTrue(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 6f, 3f, 4f, 5f }, 0f, 5f, out var within));
            Assert.AreEqual(4.5f, within.TtkMedianSeconds);
            Assert.AreEqual(4, within.SampleCount);
            Assert.AreEqual(TelemetryEvaluation.WithinTarget, within.TtkEvaluation);

            Assert.IsTrue(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 2f, 2.5f, 2.9f }, 0f, 5f, out var below));
            Assert.AreEqual(TelemetryEvaluation.Below, below.TtkEvaluation);
            Assert.IsTrue(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 6.1f, 7f, 9f }, 0f, 5f, out var above));
            Assert.AreEqual(TelemetryEvaluation.Above, above.TtkEvaluation);
        }

        [Test]
        public void InvalidOrMissingSamplesCannotBeSummarized()
        {
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new float[0], 0f, 5f, out _));
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { float.NaN }, 0f, 5f, out _));
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 4f }, -1f, 5f, out _));
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 4f }, 1f, 0f, out _));
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Common, new[] { 4f }, 6f, 5f, out _));
            Assert.IsFalse(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Unknown, new[] { 4f }, 0f, 5f, out _));
        }

        [TestCase(TelemetryCreatureClass.Common, 65f, true)]
        [TestCase(TelemetryCreatureClass.Common, 65.1f, false)]
        [TestCase(TelemetryCreatureClass.Elite, 40f, true)]
        [TestCase(TelemetryCreatureClass.Elite, 40.1f, false)]
        [TestCase(TelemetryCreatureClass.Boss, 20f, true)]
        [TestCase(TelemetryCreatureClass.Boss, 20.1f, false)]
        public void StrongControlBoundaryIsInclusive(TelemetryCreatureClass target, float seconds, bool expected)
        {
            Assert.IsTrue(SkillBalanceObservationEvaluator.TrySummarize(
                target, new[] { target == TelemetryCreatureClass.Common ? 4f : target == TelemetryCreatureClass.Elite ? 15f : 150f },
                seconds, 100f, out var result));
            Assert.AreEqual(expected, result.WithinStrongControlCap);
        }

        [Test]
        public void MinibossHasTtkBandButNoInventedStrongControlCap()
        {
            Assert.IsTrue(SkillBalanceObservationEvaluator.TrySummarize(
                TelemetryCreatureClass.Miniboss, new[] { 60f }, 5f, 100f, out var result));
            Assert.AreEqual(TelemetryEvaluation.WithinTarget, result.TtkEvaluation);
            Assert.IsNull(result.WithinStrongControlCap);
        }

        [Test]
        public void AGapAboveTwentyFivePercentRequiresMeasuredTradeoffReview()
        {
            Assert.IsFalse(SkillBalanceObservationEvaluator.RequiresTradeoffReview(4f, 5f));
            Assert.IsTrue(SkillBalanceObservationEvaluator.RequiresTradeoffReview(4f, 5.01f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                SkillBalanceObservationEvaluator.RequiresTradeoffReview(float.NaN, 5f));
        }
    }
}
