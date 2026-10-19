using CindarsHope.Skills;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillCooldownTrackerTests
    {
        [Test] public void SameActionInDifferentSlots_SharesDeadline_OtherActionIndependent()
        {
            var tracker = new SkillCooldownTracker();
            tracker.Start("fire", 10, 6);
            var slots = new[] { "fire", "ice", "fire", "" };
            Assert.That(tracker.Remaining(slots[0], 12), Is.EqualTo(4));
            Assert.That(tracker.Remaining(slots[2], 12), Is.EqualTo(4));
            slots[3] = slots[0]; slots[0] = "ice";
            Assert.That(tracker.Remaining(slots[3], 12), Is.EqualTo(4));
            Assert.That(tracker.Remaining(slots[0], 12), Is.Zero);
            Assert.That(tracker.Remaining("fire", 16), Is.Zero);
        }

        [Test] public void PausedClockPreservesRemaining_AdvancingGameplayClockExpires()
        {
            var tracker = new SkillCooldownTracker(); tracker.Start("skill", 0, 4);
            Assert.That(tracker.Remaining("skill", 1), Is.EqualTo(3));
            Assert.That(tracker.Remaining("skill", 1), Is.EqualTo(3));
            Assert.That(tracker.Remaining("skill", 4), Is.Zero);
        }

        [Test] public void InvalidInput_DoesNotPoisonHudValues()
        {
            var tracker = new SkillCooldownTracker();
            tracker.Start(null, 1, 2); tracker.Start("bad", 1, float.NaN);
            tracker.Start("overflow", float.MaxValue, float.MaxValue);
            Assert.That(tracker.Remaining(null, 2), Is.Zero);
            Assert.That(tracker.Total("bad"), Is.Zero);
            Assert.That(tracker.Total("overflow"), Is.Zero);
        }
    }
}
