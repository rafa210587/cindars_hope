using CindarsHope.Combat.Telemetry;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    public sealed class Phase21TelemetryTargetSafetyTests
    {
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(0f)]
        public void TtkWithoutFinitePositiveObservation_CannotFallInsideBand(float value)
        {
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, value, 1));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(-1f)]
        public void DamageWithoutValidObservation_CannotFallInsideBand(float value)
        {
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Elite, value, 1));
        }

        [Test]
        public void InvalidTargetBounds_CannotPass()
        {
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateRange(4f, 6f, 3f));
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateRange(4f, float.NaN, 6f));
        }
    }
}
