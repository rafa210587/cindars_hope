using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Skills.Tests
{
    public sealed class SkillRankReadinessTests
    {
        private readonly List<Object> _owned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var item in _owned)
                if (item != null) Object.DestroyImmediate(item);
            _owned.Clear();
        }

        [Test]
        public void Catalog_HasSixtySixNodesAndThirtyOneActiveActions()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes());
            var actions = Own(DefaultSkillActionCatalog.BuildAll());
            Assert.That(nodes.Count, Is.EqualTo(66));
            Assert.That(nodes.Count(n => n.SkillCategory == SkillCategory.EquippableSkill), Is.EqualTo(31));
            Assert.That(nodes.Count(n => n.SkillCategory != SkillCategory.EquippableSkill), Is.EqualTo(35));
            Assert.That(actions.Count, Is.EqualTo(31));
            Assert.That(actions.Select(a => a.SkillActionId).Distinct().Count(), Is.EqualTo(31));
        }

        [Test]
        public void Capstone_EffectiveCapStopsAtThree()
        {
            var node = Node("cap", "melee", 3);
            var state = DeepState("melee", node.SkillNodeId);
            state.RankUp(node.SkillNodeId, node.TreeId);
            state.RankUp(node.SkillNodeId, node.TreeId);
            var service = new SkillPurchaseService(new[] { node });
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(3));
            Assert.That(service.ValidateRankUp(node.SkillNodeId, state, out _), Is.False);
        }

        [Test]
        public void CanonicalThreeRankActive_RefusesRankFour()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes());
            var node = nodes.Single(candidate => candidate.SkillNodeId == "melee_whirl_cut");
            var state = DeepState(node.TreeId, node.SkillNodeId);
            state.RankUp(node.SkillNodeId, node.TreeId);
            state.RankUp(node.SkillNodeId, node.TreeId);
            var service = new SkillPurchaseService(nodes);
            Assert.That(node.AuthoredMaxRank, Is.EqualTo(3));
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(3));
            Assert.That(service.ValidateRankUp(node.SkillNodeId, state, out _), Is.False);
        }

        [Test]
        public void ActiveR5_UsesDynamicCap()
        {
            var node = Node("active", "magic", 5);
            var service = new SkillPurchaseService(new[] { node });
            var state = new SkillTreeState(40);
            state.Purchase(node.SkillNodeId, 1, node.TreeId);
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(2));
            AddSpent(state, node.TreeId, 4);
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(3));
            AddSpent(state, node.TreeId, 6);
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(4));
            AddSpent(state, node.TreeId, 7);
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(5));
            AddSpent(state, node.TreeId, 8);
            Assert.That(service.GetEffectiveRankCap(node, state), Is.EqualTo(5));
        }

        [Test]
        public void Catalog_PrerequisitesExistInSameTree()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes());
            var index = nodes.ToDictionary(n => n.SkillNodeId);
            foreach (var node in nodes)
            foreach (var prerequisite in node.PrerequisiteNodeIds)
            {
                Assert.That(index.ContainsKey(prerequisite), Is.True, $"{node.SkillNodeId} -> {prerequisite}");
                Assert.That(index[prerequisite].TreeId, Is.EqualTo(node.TreeId), $"{node.SkillNodeId} -> {prerequisite}");
            }
        }

        [Test]
        public void Capstone_RequiresBothBranches()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes());
            var capstone = nodes.Single(n => n.SkillNodeId == "melee_capstone_battle_rhythm");
            var service = new SkillPurchaseService(nodes);
            var state = DeepState("melee", "melee_dodge_training");
            Assert.That(service.ValidatePurchase(capstone.SkillNodeId, state, 100, "kanthor", out var reason), Is.False);
            Assert.That(reason, Does.Contain("melee.investida_quebra_guarda"));
        }

        [Test]
        public void WhirlDamage_RanksResolveTenTwelveFourteen()
        {
            var action = Own(DefaultSkillActionCatalog.BuildAll()).Single(a => a.SkillActionId == "skill_melee_whirl_cut");
            Assert.That(action.ResolveRank(1).Damage, Is.EqualTo(10));
            Assert.That(action.ResolveRank(2).Damage, Is.EqualTo(12));
            Assert.That(action.ResolveRank(3).Damage, Is.EqualTo(14));
        }

        [Test]
        public void WhirlTargets_CapsSixAndFallsOffAfterThird()
        {
            var action = Own(DefaultSkillActionCatalog.BuildAll()).Single(a => a.SkillActionId == "skill_melee_whirl_cut");
            Assert.That(action.MaxTargets, Is.EqualTo(6));
            Assert.That(action.ResolveDamageForTargetIndex(3, 0), Is.EqualTo(14));
            Assert.That(action.ResolveDamageForTargetIndex(3, 2), Is.EqualTo(14));
            Assert.That(action.ResolveDamageForTargetIndex(3, 3), Is.EqualTo(10));
            Assert.That(action.ResolveDamageForTargetIndex(3, 5), Is.EqualTo(10));
        }

        [Test]
        public void DormantAction_IsExplicitInNodeAndActionCatalogs()
        {
            var node = Own(DefaultSkillCatalog.BuildAllNodes()).Single(candidate =>
                candidate.SkillNodeId == "ranged_marked_prey");
            var action = Own(DefaultSkillActionCatalog.BuildAll()).Single(candidate =>
                candidate.SkillActionId == node.UnlockedSkillActionId);
            Assert.That(node.NotYetExecutable, Is.True);
            Assert.That(action.NotYetExecutable, Is.True);
        }

        [Test]
        public void CraftedToxicBomb_AuthorsStaminaInsteadOfMana()
        {
            var action = Own(DefaultSkillActionCatalog.BuildAll()).Single(a =>
                a.SkillActionId == "skill_crafting_bomba_improvisada");
            var rank = action.ResolveRank(1);
            Assert.That(rank.StaminaCost, Is.EqualTo(20));
            Assert.That(rank.ManaCost, Is.Zero);
        }

        private SkillNodeDataSO Node(string id, string tree, int authoredMaxRank)
        {
            var node = ScriptableObject.CreateInstance<SkillNodeDataSO>();
            _owned.Add(node);
            node.SkillNodeId = id;
            node.TreeId = tree;
            node.AuthoredMaxRank = authoredMaxRank;
            return node;
        }

        private static SkillTreeState DeepState(string tree, string purchasedNodeId)
        {
            var state = new SkillTreeState(40);
            state.Purchase(purchasedNodeId, 1, tree);
            AddSpent(state, tree, 25);
            return state;
        }

        private static void AddSpent(SkillTreeState state, string tree, int count)
        {
            int start = state.SpentSkillPoints;
            for (int i = 0; i < count; i++) state.Purchase($"pump_{start}_{i}", 1, tree);
        }

        private List<T> Own<T>(List<T> items) where T : Object
        {
            _owned.AddRange(items.Cast<Object>());
            return items;
        }
    }
}
