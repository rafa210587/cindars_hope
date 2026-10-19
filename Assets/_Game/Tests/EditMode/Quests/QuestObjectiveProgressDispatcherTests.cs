using CindarsHope.Quests;
using CindarsHope.Quests.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Quests
{
    public class QuestObjectiveProgressDispatcherTests
    {
        [TestCase("any", "wheat", null, true)]
        [TestCase("seed_wheat", "seed_wheat", "item_wheat", true)]
        [TestCase("item_wheat", "seed_wheat", "item_wheat", true)]
        [TestCase("seed_turnip", "seed_wheat", "item_wheat", false)]
        public void HarvestMatcher_PreservesAnySeedAndItemRules(string target, string seed,
            string item, bool expected)
        {
            var objective = Objective(QuestObjectiveType.HarvestCrop, target);
            var signal = new QuestProgressSignal(QuestObjectiveType.HarvestCrop,
                QuestProgressApplication.Complete, seed, item);
            Assert.AreEqual(expected, QuestObjectiveProgressMatcher.Matches(objective, signal));
        }

        [TestCase("15", 15, true)]
        [TestCase("15", 20, true)]
        [TestCase("30", 20, false)]
        [TestCase("cave_level_20", 20, true)]
        public void CaveDepthMatcher_PreservesNumericAndStableIdRules(string target, int reached,
            bool expected)
        {
            var objective = Objective(QuestObjectiveType.ReachCaveDepth, target);
            var signal = new QuestProgressSignal(QuestObjectiveType.ReachCaveDepth,
                QuestProgressApplication.Complete, $"cave_level_{reached}", numericValue: reached);
            Assert.AreEqual(expected, QuestObjectiveProgressMatcher.Matches(objective, signal));
        }

        [Test]
        public void Matcher_RejectsDifferentObjectiveType()
        {
            var objective = Objective(QuestObjectiveType.DefeatEnemy, "enemy_slime");
            var signal = new QuestProgressSignal(QuestObjectiveType.TalkToNpc,
                QuestProgressApplication.Complete, "enemy_slime");
            Assert.IsFalse(QuestObjectiveProgressMatcher.Matches(objective, signal));
        }

        private static QuestObjective Objective(QuestObjectiveType type, string target) =>
            new QuestObjective
            {
                ObjectiveId = "obj_test",
                ObjectiveType = type,
                TargetId = target,
                RequiredAmount = 1
            };
    }
}
