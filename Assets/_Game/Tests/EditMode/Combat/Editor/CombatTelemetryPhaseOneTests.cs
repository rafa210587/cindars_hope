using CindarsHope.Combat.Telemetry;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    public class CombatTelemetryPhaseOneTests
    {
        [Test]
        public void RecordBlock_IncrementsSessionAndReportCounters()
        {
            var session = new CombatTelemetrySession(null);
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordBlock(1f);
            session.RecordBlock(2f);
            CombatTelemetryReport report = session.BuildReport("now", null);

            Assert.That(session.Blocks, Is.EqualTo(2));
            Assert.That(report.Blocks, Is.EqualTo(2));
        }

        [Test]
        public void NormalBlockEvent_PreservesIncomingAndMitigatedDamage()
        {
            var evt = new PlayerNormalBlockEvent("enemy_guard", 11, 6);

            Assert.That(evt.SourceId, Is.EqualTo("enemy_guard"));
            Assert.That(evt.IncomingDamage, Is.EqualTo(11));
            Assert.That(evt.MitigatedDamage, Is.EqualTo(6));
        }
    }
}
