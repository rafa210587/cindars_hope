using System.Collections.Generic;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillCastTimelineTests
    {
        [Test]
        public void Timeline_M1_CommitsOnceAtPointTen_AndCompletesAtPointForty()
        {
            var timeline = new SkillCastTimeline();
            var phases = new List<SkillCastPhase>();
            timeline.PhaseChanged += (phase, _) => phases.Add(phase);

            Assert.That(timeline.Begin(.10f, .10f, .20f).ShouldCommit, Is.False);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Windup));
            Assert.That(timeline.Tick(.099f).ShouldCommit, Is.False);

            var commit = timeline.Tick(.001f);
            Assert.That(commit.ShouldCommit, Is.True);
            Assert.That(timeline.HasCommitted, Is.False, "A request is not a successful commit yet.");
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Windup));
            Assert.That(timeline.ContinueAfterCommit().Completed, Is.False);
            Assert.That(timeline.HasCommitted, Is.True);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Active));
            Assert.That(timeline.Tick(.10f).ShouldCommit, Is.False);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Recovery));

            var completed = timeline.Tick(.20f);
            Assert.That(completed.ShouldCommit, Is.False);
            Assert.That(completed.Completed, Is.True);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Idle));
            CollectionAssert.AreEqual(new[]
            {
                SkillCastPhase.Windup,
                SkillCastPhase.Active,
                SkillCastPhase.Recovery,
                SkillCastPhase.Idle
            }, phases);
        }

        [Test]
        public void Timeline_CancelBeforeCommit_NeverRequestsCommit()
        {
            var timeline = new SkillCastTimeline();
            timeline.Begin(.20f, .18f, .38f);
            timeline.Tick(.05f);

            var cancelled = timeline.Cancel();

            Assert.That(cancelled.Cancelled, Is.True);
            Assert.That(cancelled.ShouldCommit, Is.False);
            Assert.That(timeline.HasCommitted, Is.False);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Cancelled));
            Assert.That(timeline.Tick(10f).ShouldCommit, Is.False);
        }

        [Test]
        public void Timeline_ZeroProfile_CommitsAndCompletesImmediately()
        {
            var timeline = new SkillCastTimeline();

            var result = timeline.Begin(0f, 0f, 0f);

            Assert.That(result.ShouldCommit, Is.True);
            Assert.That(result.Completed, Is.False);
            Assert.That(timeline.ContinueAfterCommit().Completed, Is.True);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Idle));
        }

        [Test]
        public void Timeline_LargeTick_CarriesRemainderAcrossAllPhases()
        {
            var timeline = new SkillCastTimeline();
            timeline.Begin(.20f, .18f, .38f);

            var result = timeline.Tick(.76f);

            Assert.That(result.ShouldCommit, Is.True);
            Assert.That(timeline.ContinueAfterCommit().Completed, Is.True);
            Assert.That(timeline.Phase, Is.EqualTo(SkillCastPhase.Idle));
        }

        [Test]
        public void CanonicalMeleeProfiles_AreDataDrivenM1AndM2()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var m1 = actions.Find(action => action.SkillActionId == "skill_melee_offhand_cut");
                var m2 = actions.Find(action => action.SkillActionId == "skill_melee_whirl_cut");
                Assert.That(m1.TimingProfileId, Is.EqualTo("M1"));
                Assert.That(m1.WindupSeconds, Is.EqualTo(.10f));
                Assert.That(m1.ActiveSeconds, Is.EqualTo(.10f));
                Assert.That(m1.RecoverySeconds, Is.EqualTo(.20f));
                Assert.That(m2.TimingProfileId, Is.EqualTo("M2"));
                Assert.That(m2.WindupSeconds, Is.EqualTo(.20f));
                Assert.That(m2.ActiveSeconds, Is.EqualTo(.18f));
                Assert.That(m2.RecoverySeconds, Is.EqualTo(.38f));
            }
            finally
            {
                foreach (var action in actions)
                    UnityEngine.Object.DestroyImmediate(action);
            }
        }
    }
}
