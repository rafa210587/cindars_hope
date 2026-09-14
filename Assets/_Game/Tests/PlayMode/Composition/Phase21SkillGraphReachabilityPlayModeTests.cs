using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Foundation;
using CindarsHope.Player.Progression;
using CindarsHope.Quests.Runtime;
using CindarsHope.Save;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class Phase21SkillGraphReachabilityPlayModeTests
    {
        private SkillTreeManager _skills;
        private PlayerProgressionManager _progression;
        private SkillTreeSaveData _originalSkills;
        private PlayerProgressionSaveData _originalProgression;
        private QuestStateSectionSaveData _originalQuests;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            for (int frame = 0; frame < 180 && QuestRuntimeBootstrap.QuestService == null; frame++)
                yield return null;

            _skills = SkillTreeManager.Instance;
            _progression = PlayerProgressionManager.Instance;
            Assert.That(_skills, Is.Not.Null, "FarmScene must load the canonical skill catalog.");
            Assert.That(_progression, Is.Not.Null);
            Assert.That(QuestRuntimeBootstrap.QuestService, Is.Not.Null);
            _originalSkills = _skills.CaptureSaveData();
            _originalProgression = _progression.CaptureSaveData();
            _originalQuests = QuestRuntimeBootstrap.CaptureSaveData();
            Assert.That(_originalQuests, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_originalProgression != null) _progression?.RestoreFromSaveData(_originalProgression);
            if (_originalSkills != null)
                _skills?.RestoreFromSaveData(_originalSkills, _originalProgression?.Level ?? 1);
            if (_originalQuests != null) QuestRuntimeBootstrap.RestoreFromSaveData(_originalQuests);
            yield return null;
        }

        [UnityTest, Explicit("Run alone: this test changes the global skill, progression and quest state.")]
        public IEnumerator AuthoredGraph_LegalStagePurchases_AndDormantNodeRejection()
        {
            ResetStage(Phase21BuildManifest.Level20);
            Assert.That(Purchase("survival.isca_improvisada", 20, out var reason), Is.True, reason);
            AssertAction("survival.isca_improvisada", "skill_survival_isca_improvisada", 20);
            Assert.That(_skills.TryPurchaseNode("survival.instinto_sobrevivencia", 20, out reason),
                Is.False, "Tier 3 needs 11 points spent; L20 has only 10 points total.");
            Assert.That(_skills.GetRank("survival.instinto_sobrevivencia"), Is.Zero);
            AssertBudget(20, 0, "survival");
            yield return null;

            ResetStage(Phase21BuildManifest.Level50);
            Assert.That(Purchase("survival.instinto_sobrevivencia", 50, out reason), Is.True, reason);
            Assert.That(_skills.GetRank("survival.isca_improvisada"), Is.EqualTo(1),
                "Instinct must be reached through its authored lure prerequisite.");
            AssertAction("survival.instinto_sobrevivencia", "skill_survival_instinto_sobrevivencia", 50);
            AssertBudget(50, 0, "survival");
            yield return null;

            ResetStage(Phase21BuildManifest.Level50);
            Assert.That(Purchase("crafting.irrigador_portatil", 50, out reason), Is.True, reason);
            Assert.That(_skills.GetRank("crafting_quick_repair"), Is.EqualTo(1),
                "Irrigator must be reached through the authored repair chain.");
            AssertAction("crafting.irrigador_portatil", "skill_crafting_irrigador_portatil", 50);
            AssertDormantPackOrder(50);
            AssertBudget(50, 0, "crafting");
            yield return null;

            foreach (var stage in new[] { Phase21BuildManifest.Level50, Phase21BuildManifest.Level54 })
            {
                ResetStage(stage);
                AwardTwoActs(stage);
                Assert.That(Purchase("crafting_shop_sense", stage, out reason), Is.True, reason);
                int expectedRank = stage == Phase21BuildManifest.Level50 ? 1 : 3;
                for (int rank = 1; rank < expectedRank; rank++)
                    Assert.That(_skills.TryRankUpNode("crafting_shop_sense", out reason), Is.True, reason);
                Assert.That(_skills.GetRank("crafting_shop_sense"), Is.EqualTo(expectedRank));
                Assert.That(_skills.GetChosenVariant("crafting_shop_sense"), Is.EqualTo("buy"));
                AssertBudget(stage, 2, "crafting");
                AssertDormantPackOrder(stage);

                var save = _skills.CaptureSaveData();
                _skills.RestoreFromSaveData(save, stage);
                Assert.That(_skills.GetRank("crafting_shop_sense"), Is.EqualTo(expectedRank),
                    "Market rank must survive a skill save roundtrip.");
                Assert.That(_skills.GetChosenVariant("crafting_shop_sense"), Is.EqualTo("buy"),
                    "The purchase focus must survive a skill save roundtrip.");
                AssertBudget(stage, 2, "crafting");
                yield return null;
            }

            TestContext.WriteLine("Skill graph: L20 Isca purchased, L20 Instinto rejected; " +
                "L50 Instinto/Irrigador and L50 R1/L54 R3 buy-focus purchased; " +
                "Mochila Ordenada rejected as DormantAction. Purchase/slot/save only; " +
                "effect execution is covered by separate Phase21 fixtures.");
        }

        private void ResetStage(int level)
        {
            var stage = _progression.CaptureSaveData();
            stage.TotalXp = ProgressionCurve.TotalXpForLevel(level);
            stage.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level);
            _progression.RestoreFromSaveData(stage);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), level);
            QuestRuntimeBootstrap.RestoreFromSaveData(new QuestStateSectionSaveData());
            Assert.That(_progression.Level, Is.EqualTo(level));
            Assert.That(_progression.UnspentSkillPoints,
                Is.EqualTo(PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level)));
        }

        private void AwardTwoActs(int level)
        {
            var service = QuestRuntimeBootstrap.QuestService;
            Assert.That(service, Is.Not.Null);
            foreach (var finale in QuestMainActsIds.Finales.Take(2))
                Assert.That(service.TryAwardActSkillPoint(finale.ActIdForSkillPoint), Is.True,
                    "The two canonical Act rewards must be awarded through QuestService.");
            Assert.That(_progression.UnspentSkillPoints,
                Is.EqualTo(PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level) + 2));
            var questSave = QuestRuntimeBootstrap.CaptureSaveData();
            foreach (var finale in QuestMainActsIds.Finales.Take(2))
                Assert.That(questSave.RewardedMainActIds, Does.Contain(finale.ActIdForSkillPoint));
            QuestRuntimeBootstrap.RestoreFromSaveData(questSave);
            foreach (var finale in QuestMainActsIds.Finales.Take(2))
                Assert.That(service.TryAwardActSkillPoint(finale.ActIdForSkillPoint), Is.False,
                    "An Act reward cannot be earned twice after quest restore.");
        }

        private bool Purchase(string nodeId, int level, out string reason)
            => Purchase(nodeId, level, new HashSet<string>(), out reason);

        private bool Purchase(string nodeId, int level, HashSet<string> visiting, out string reason)
        {
            reason = string.Empty;
            if (_skills.GetRank(nodeId) > 0) return true;
            if (!_skills.NodeIndex.TryGetValue(nodeId, out var node))
            { reason = "Missing canonical node: " + nodeId; return false; }
            if (!visiting.Add(nodeId))
            { reason = "Prerequisite cycle: " + nodeId; return false; }
            try
            {
                foreach (var prerequisite in node.PrerequisiteNodeIds ?? new List<string>())
                    if (!Purchase(prerequisite, level, visiting, out reason)) return false;

                for (int attempt = 0; attempt < _skills.NodeIndex.Count * SkillTierRules.AbsoluteRankCap; attempt++)
                {
                    string variant = nodeId == "crafting_shop_sense" ? "buy" : null;
                    if (_skills.TryPurchaseNode(nodeId, level, variant, out var feedback)) return true;
                    bool progressed = false;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(candidate => candidate.TreeId == node.TreeId &&
                            candidate.Tier <= node.Tier && !candidate.IsCapstone &&
                            !candidate.NotYetExecutable && _skills.GetRank(candidate.SkillNodeId) == 0 &&
                            candidate.SkillNodeId != nodeId && !visiting.Contains(candidate.SkillNodeId))
                        .OrderBy(candidate => candidate.Tier).ThenBy(candidate => candidate.SkillNodeId))
                    {
                        string fillerVariant = filler.SkillNodeId == "crafting_shop_sense" ? "buy" : null;
                        if (!_skills.TryPurchaseNode(filler.SkillNodeId, level, fillerVariant, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (progressed) continue;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(candidate => candidate.TreeId == node.TreeId && !candidate.IsCapstone &&
                            _skills.GetRank(candidate.SkillNodeId) > 0 && candidate.SkillNodeId != nodeId)
                        .OrderBy(candidate => candidate.Tier).ThenBy(candidate => candidate.SkillNodeId))
                    {
                        if (!_skills.TryRankUpNode(filler.SkillNodeId, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (!progressed) { reason = feedback; return false; }
                }
                reason = "Purchase attempt limit: " + nodeId;
                return false;
            }
            finally { visiting.Remove(nodeId); }
        }

        private void AssertAction(string nodeId, string actionId, int level)
        {
            Assert.That(_skills.GetRank(nodeId), Is.EqualTo(1));
            Assert.That(_skills.NodeIndex[nodeId].NotYetExecutable, Is.False);
            Assert.That(_skills.ActionIndex.ContainsKey(actionId), Is.True);
            Assert.That(_skills.ActionIndex[actionId].NotYetExecutable, Is.False);
            Assert.That(_skills.TryAssignActiveSlot(0, actionId), Is.True,
                "Purchased action must be available to the real slot manager.");
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(actionId));
            var save = _skills.CaptureSaveData();
            _skills.RestoreFromSaveData(save, level);
            Assert.That(_skills.GetRank(nodeId), Is.EqualTo(1));
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(actionId));
        }

        private void AssertDormantPackOrder(int level)
        {
            const string nodeId = "crafting_pack_order";
            Assert.That(_skills.NodeIndex.TryGetValue(nodeId, out var node), Is.True);
            Assert.That(node.NotYetExecutable, Is.True);
            int before = _progression.UnspentSkillPoints;
            Assert.That(_skills.TryPurchaseNode(nodeId, level, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo(SkillPurchaseService.NotYetExecutableFailureReason));
            Assert.That(_skills.GetRank(nodeId), Is.Zero);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(before));
        }

        private void AssertBudget(int level, int actBonus, string treeId)
        {
            int available = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level) + actBonus;
            int spent = _skills.GetPointsSpentInTree(treeId);
            Assert.That(spent, Is.InRange(0, available));
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(available - spent),
                "All purchases and rank-ups must consume canonical points exactly once.");
        }
    }
}
