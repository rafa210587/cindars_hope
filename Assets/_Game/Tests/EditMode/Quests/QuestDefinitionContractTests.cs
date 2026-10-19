using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestDefinitionContractTests
    {
        private QuestDefinitionValidator _validator;

        [SetUp]
        public void SetUp() { _validator = new QuestDefinitionValidator(); }

        private QuestDefinition MainQuest(string id = "quest_main_01") => new QuestDefinition
        { QuestId = id, Category = QuestCategory.Main, Trackable = true, SpoilerTier = 0 };

        private QuestDefinition SideQuest(string id = "quest_side_01") => new QuestDefinition
        { QuestId = id, Category = QuestCategory.Side, Trackable = true };

        [Test]
        public void QuestDefinition_HasCanonicalFields()
        {
            var def = MainQuest();
            def.StartConditionIds.Add("cond_intro");
            def.StepIds.Add("step_1");
            Assert.AreEqual(QuestCategory.Main, def.Category);
            Assert.IsTrue(def.Trackable);
        }

        [Test]
        public void QuestState_CanonicalEnum_Exists()
        {
            var state = new QuestState { QuestId = "quest_01", StateStatus = QuestStateStatus.Active };
            Assert.AreEqual(QuestStateStatus.Active, state.StateStatus);
        }

        [Test]
        public void QuestState_IsTerminal_Completed()
        {
            var state = new QuestState { StateStatus = QuestStateStatus.Completed };
            Assert.IsTrue(state.IsTerminal());
        }

        [Test]
        public void QuestState_IsTerminal_Active_False()
        {
            var state = new QuestState { StateStatus = QuestStateStatus.Active };
            Assert.IsFalse(state.IsTerminal());
        }

        [Test]
        public void QuestDefinition_MainCannotExpire()
        {
            var def = MainQuest();
            def.ExpiryRuleIds.Add("expire_after_30_days");
            var issues = _validator.Validate(def);
            Assert.IsTrue(issues.Exists(i => i.Code == "QUEST_MAIN_EXPIRABLE" && i.IsBlocker));
        }

        [Test]
        public void QuestDefinition_SideCanExpire()
        {
            var def = SideQuest();
            def.ExpiryRuleIds.Add("expire_30");
            Assert.IsTrue(def.CanExpire());
            var issues = _validator.Validate(def);
            Assert.IsFalse(issues.Exists(i => i.Code == "QUEST_MAIN_EXPIRABLE"));
        }

        [Test]
        public void QuestDefinition_FarmOrderCanExpire()
        {
            var def = new QuestDefinition { QuestId = "order_01", Category = QuestCategory.FarmOrder };
            def.ExpiryRuleIds.Add("expire_season");
            Assert.IsTrue(def.CanExpire());
        }

        [Test]
        public void QuestObjective_FutureType_Detected()
        {
            var obj = new QuestObjective { ObjectiveId = "obj_pet", ObjectiveType = QuestObjectiveType.PetInteractionFuture };
            Assert.IsTrue(obj.IsFutureObjective());
        }

        [Test]
        public void QuestObjective_NonFutureType_NotFuture()
        {
            var obj = new QuestObjective { ObjectiveId = "obj_talk", ObjectiveType = QuestObjectiveType.TalkToNpc };
            Assert.IsFalse(obj.IsFutureObjective());
        }

        [Test]
        public void QuestEventName_IsKnown()
        {
            Assert.IsTrue(QuestEventName.IsKnown(QuestEventName.ItemCollected));
            Assert.IsFalse(QuestEventName.IsKnown("unknown_event_xyz"));
        }

        [Test]
        public void QuestDefinition_FlagGrantMainProgression_IsBlocker()
        {
            var def = MainQuest();
            def.QuestFlagGrantIds.Add("flag_mainprogression_level3");
            var issues = _validator.Validate(def);
            Assert.IsTrue(issues.Exists(i => i.Code == "QUEST_FLAG_MAIN_PROGRESSION_SWALLOW" && i.IsBlocker));
        }

        [Test]
        public void QuestStep_CompletionMode_AllRequired()
        {
            var step = new QuestStepDefinition
            {
                StepId = "step_1",
                ObjectiveIds = new List<string> { "obj_talk", "obj_deliver" },
                CompletionMode = CompletionMode.AllObjectivesRequired
            };
            Assert.AreEqual(CompletionMode.AllObjectivesRequired, step.CompletionMode);
        }

        [Test]
        public void QuestCategory_FutureReservation_NoStepsWarning()
        {
            var def = new QuestDefinition { QuestId = "quest_social_future", Category = QuestCategory.SocialFuture };
            def.StepIds.Add("step_001");
            var issues = _validator.Validate(def);
            Assert.IsTrue(issues.Exists(i => i.Code == "QUEST_FUTURE_HAS_STEPS"));
        }
    }
}
