using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests.Conditions;
using CindarsHope.Quests.Triggers;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestConditionTriggerTests
    {
        private QuestConditionResolver _resolver;
        private QuestTriggerRouter _router;
        private QuestConditionContext _ctx;

        [SetUp]
        public void SetUp()
        {
            _resolver = new QuestConditionResolver();
            _router = new QuestTriggerRouter(_resolver);
            _ctx = new QuestConditionContext
            {
                CurrentDay = 5, CurrentSeason = "Spring", CurrentWeather = "Clear",
                CurrentLunarPhase = "Alihana", FarmLevel = 2, CaveProgress = 0, PlayerReputation = 40
            };
        }

        private QuestConditionDefinition FlagCondition(string id, string flagId) => new QuestConditionDefinition
        { ConditionId = id, ConditionType = QuestConditionType.QuestCondition, TargetId = flagId, Operator = QuestConditionOperator.IsSet };

        private QuestConditionDefinition InvCondition(string id, string itemId, int amount) => new QuestConditionDefinition
        { ConditionId = id, ConditionType = QuestConditionType.InventoryCondition, TargetId = itemId, Amount = amount };

        private QuestTriggerDefinition DialogueTrigger(string id, string eventName, string targetId = null) => new QuestTriggerDefinition
        { TriggerId = id, TriggerType = QuestTriggerType.Dialogue, EventName = eventName, TargetId = targetId };

        private QuestEventEnvelope Event(string name, string targetId = null, string eventId = "ev_001") => new QuestEventEnvelope
        { EventId = eventId, EventName = name, TargetId = targetId, Day = 5, SourceSystem = "DialogueSystem" };

        [Test]
        public void Condition_FlagSet_Pass()
        {
            _ctx.ActiveFlags["flag_vaelrion_introduced"] = "true";
            var c = FlagCondition("c1", "flag_vaelrion_introduced");
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Condition_FlagNotSet_Fail()
        {
            var c = FlagCondition("c1", "flag_not_set");
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailedConditionIds.Contains("c1"));
        }

        [Test]
        public void Condition_Inventory_Sufficient_Pass()
        {
            _ctx.ItemCounts["item_ironore"] = 5;
            var c = InvCondition("c2", "item_ironore", 3);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Condition_Inventory_Insufficient_Fail()
        {
            _ctx.ItemCounts["item_ironore"] = 1;
            var c = InvCondition("c2", "item_ironore", 5);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Condition_FutureCondition_AlwaysPass()
        {
            var c = new QuestConditionDefinition
            { ConditionId = "c_pet_future", ConditionType = QuestConditionType.PetConditionFuture };
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsTrue(result.Success, "Future conditions must not block main quest");
        }

        [Test]
        public void Condition_WeatherMatch_Pass()
        {
            var c = new QuestConditionDefinition
            { ConditionId = "c_weather", ConditionType = QuestConditionType.WeatherCondition, ExpectedValue = "Clear" };
            Assert.IsTrue(_resolver.Evaluate(c, _ctx).Success);
        }

        [Test]
        public void Condition_WeatherMismatch_Fail()
        {
            var c = new QuestConditionDefinition
            { ConditionId = "c_weather", ConditionType = QuestConditionType.WeatherCondition, ExpectedValue = "Storm" };
            Assert.IsFalse(_resolver.Evaluate(c, _ctx).Success);
        }

        [Test]
        public void Condition_LunarMatch_Pass()
        {
            var c = new QuestConditionDefinition
            { ConditionId = "c_lunar", ConditionType = QuestConditionType.LunarCondition, ExpectedValue = "Alihana" };
            Assert.IsTrue(_resolver.Evaluate(c, _ctx).Success);
        }

        [Test]
        public void Condition_EvaluateAll_AllPass()
        {
            _ctx.ActiveFlags["flag_cindar_diary_read"] = "true";
            _ctx.ItemCounts["item_ore"] = 10;
            var conditions = new List<QuestConditionDefinition>
            {
                FlagCondition("c1", "flag_cindar_diary_read"),
                InvCondition("c2", "item_ore", 5)
            };
            var result = _resolver.EvaluateAll(conditions, _ctx);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Condition_EvaluateAll_OneFails_Combined()
        {
            _ctx.ActiveFlags["flag_cindar_diary_read"] = "true";
            var conditions = new List<QuestConditionDefinition>
            {
                FlagCondition("c1", "flag_cindar_diary_read"),
                InvCondition("c2", "item_missing", 5)
            };
            var result = _resolver.EvaluateAll(conditions, _ctx);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailedConditionIds.Contains("c2"));
        }

        [Test]
        public void Trigger_Match_Route()
        {
            var trigger = DialogueTrigger("t1", "dialogue_completed", "npc_vaelrion");
            var evt = Event("dialogue_completed", "npc_vaelrion");
            var result = _router.Route(evt, trigger, new List<QuestConditionDefinition>(), _ctx);
            Assert.IsTrue(result.Matched);
        }

        [Test]
        public void Trigger_EventNameMismatch_NoMatch()
        {
            var trigger = DialogueTrigger("t1", "dialogue_completed");
            var evt = Event("item_collected");
            var result = _router.Route(evt, trigger, new List<QuestConditionDefinition>(), _ctx);
            Assert.IsFalse(result.Matched);
        }

        [Test]
        public void Trigger_Deduplicated_SecondCall_NoMatch()
        {
            var trigger = DialogueTrigger("t1", "dialogue_completed", "npc_renko");
            var evt = Event("dialogue_completed", "npc_renko", "ev_001");
            _router.Route(evt, trigger, new List<QuestConditionDefinition>(), _ctx);
            var second = _router.Route(evt, trigger, new List<QuestConditionDefinition>(), _ctx);
            Assert.IsFalse(second.Matched);
            Assert.IsTrue(second.WasDeduplicated);
        }

        [Test]
        public void Trigger_ConditionsBlocked_NoMatch()
        {
            var trigger = DialogueTrigger("t1", "dialogue_completed");
            trigger.RequiredConditionIds.Add("c_missing_flag");
            var requiredConds = new List<QuestConditionDefinition> { FlagCondition("c_missing_flag", "flag_not_set") };
            var evt = Event("dialogue_completed", eventId: "ev_999");
            var result = _router.Route(evt, trigger, requiredConds, _ctx);
            Assert.IsFalse(result.Matched);
            Assert.IsTrue(result.ConditionsBlocked);
        }

        [Test]
        public void Trigger_ManualAllowRepeat_CanFireTwice()
        {
            var trigger = DialogueTrigger("t1", "item_collected");
            trigger.DeduplicationPolicy = QuestTriggerDeduplicationPolicy.ManualAllowRepeat;
            var evt1 = Event("item_collected", "item_wood", "ev_1");
            var evt2 = Event("item_collected", "item_wood", "ev_2");
            var r1 = _router.Route(evt1, trigger, new List<QuestConditionDefinition>(), _ctx);
            var r2 = _router.Route(evt2, trigger, new List<QuestConditionDefinition>(), _ctx);
            Assert.IsTrue(r1.Matched);
            Assert.IsTrue(r2.Matched);
        }
    }
}
