using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestSaveLoadTests
    {
        private QuestStateNormalizer _normalizer;

        [SetUp]
        public void SetUp() { _normalizer = new QuestStateNormalizer(); }

        private QuestStateSection MakeSection(params QuestStateRecord[] records)
        {
            var sec = new QuestStateSection();
            foreach (var r in records) sec.QuestStates.Add(r);
            return sec;
        }

        [Test]
        public void Section_GetQuestState_Found()
        {
            var rec = new QuestStateRecord { QuestId = "quest_intro", State = (int)QuestStateStatus.Active };
            var sec = MakeSection(rec);
            var found = sec.GetQuestState("quest_intro");
            Assert.IsNotNull(found);
            Assert.AreEqual("quest_intro", found.QuestId);
        }

        [Test]
        public void Section_GetQuestState_NotFound_ReturnsNull()
        {
            var sec = MakeSection();
            Assert.IsNull(sec.GetQuestState("quest_unknown"));
        }

        [Test]
        public void Section_IsRewardGranted_True()
        {
            var rec = new QuestStateRecord { QuestId = "quest_01" };
            rec.GrantedRewardIds.Add("rw_gold_100");
            var sec = MakeSection(rec);
            Assert.IsTrue(sec.IsRewardGranted("quest_01", "rw_gold_100"));
        }

        [Test]
        public void Section_IsRewardGranted_False()
        {
            var rec = new QuestStateRecord { QuestId = "quest_01" };
            var sec = MakeSection(rec);
            Assert.IsFalse(sec.IsRewardGranted("quest_01", "rw_gold_100"));
        }

        [Test]
        public void Normalizer_EmptySection_NoIssues()
        {
            var sec = new QuestStateSection();
            var issues = _normalizer.Normalize(sec);
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Normalizer_TerminalWithActiveStep_Clears()
        {
            var rec = new QuestStateRecord { QuestId = "q1", State = (int)QuestStateStatus.Completed, CurrentStepId = "step_1" };
            var sec = MakeSection(rec);
            var issues = _normalizer.Normalize(sec);
            Assert.IsTrue(issues.Exists(i => i.Code == "QUEST_TERMINAL_HAS_ACTIVE_STEP" && i.WasRecovered));
            Assert.IsNull(rec.CurrentStepId);
        }

        [Test]
        public void Normalizer_CompletedNoDay_RecoverWithStart()
        {
            var rec = new QuestStateRecord { QuestId = "q2", State = (int)QuestStateStatus.Completed, StartedAtDay = 10, CompletedAtDay = null };
            var sec = MakeSection(rec);
            _normalizer.Normalize(sec);
            Assert.AreEqual(10, rec.CompletedAtDay);
        }

        [Test]
        public void Normalizer_ActiveNoStep_DemotedToAvailable()
        {
            var rec = new QuestStateRecord { QuestId = "q3", State = (int)QuestStateStatus.Active };
            var sec = MakeSection(rec);
            _normalizer.Normalize(sec);
            Assert.AreEqual((int)QuestStateStatus.Available, rec.State);
        }

        [Test]
        public void Normalizer_DuplicateQuests_IssueReported()
        {
            var r1 = new QuestStateRecord { QuestId = "q_dup", State = (int)QuestStateStatus.Active };
            var r2 = new QuestStateRecord { QuestId = "q_dup", State = (int)QuestStateStatus.Active };
            var sec = MakeSection(r1, r2);
            var issues = _normalizer.Normalize(sec);
            Assert.IsTrue(issues.Exists(i => i.Code == "QUEST_DUPLICATE_ID"));
        }

        [Test]
        public void Normalizer_NormalizationVersionIncrements()
        {
            var sec = new QuestStateSection { LastQuestStateNormalizationVersion = 3 };
            _normalizer.Normalize(sec);
            Assert.AreEqual(4, sec.LastQuestStateNormalizationVersion);
        }

        [Test]
        public void QuestRecord_NoUnityRefs()
        {
            var rec = new QuestStateRecord { QuestId = "q1", State = (int)QuestStateStatus.Active };
            rec.GrantedFlagIds.Add("flag_vaelrion_introduced");
            rec.GrantedRewardIds.Add("rw_gold");
            rec.ChoiceHistory.Add(new QuestChoiceRecord { StepId = "s1", ChoiceId = "choice_help", ChosenAtDay = 5 });
            // Just check it compiles and holds data — no Unity references
            Assert.AreEqual(1, rec.GrantedFlagIds.Count);
            Assert.AreEqual(1, rec.ChoiceHistory.Count);
        }
    }
}
