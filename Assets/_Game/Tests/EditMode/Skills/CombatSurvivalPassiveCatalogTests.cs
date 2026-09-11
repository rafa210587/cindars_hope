using System.Collections.Generic;
using System.Linq;
using CindarsHope.Foundation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Skills.Tests
{
    public sealed class CombatSurvivalPassiveCatalogTests
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
        public void Catalog_Phase17Passives_UseNormativeChannelsValuesAndCaps()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes()).ToDictionary(node => node.SkillNodeId);

            AssertModifier(nodes, "melee_iron_grip", SkillModifierType.MeleeAttackFlat, 1f, 5);
            AssertModifier(nodes, "melee_guarded_stance", SkillModifierType.GuardedDefenseFlat, 1f, 3);
            AssertModifier(nodes, "melee_dual_wield_flow", SkillModifierType.DualWieldRecoverySpeed, .05f, 3);
            AssertModifier(nodes, "melee_two_handed_momentum", SkillModifierType.TwoHandedDamageBonus, .05f, 5);
            AssertModifier(nodes, "melee_dodge_training", SkillModifierType.DodgeCostReduction, .10f, 3);

            AssertModifier(nodes, "ranged_steady_hand", SkillModifierType.BowDamageFlat, 1f, 5);
            AssertModifier(nodes, "ranged_long_sight", SkillModifierType.BowRangeFlat, .5f, 3);
            AssertModifier(nodes, "ranged_quick_nock", SkillModifierType.BowRecoverySpeed, .08f, 3);
            AssertModifier(nodes, "ranged_kiting_steps", SkillModifierType.KitingMoveSpeedBonus, .04f, 5);
            AssertModifier(nodes, "ranged_projectile_tuning", SkillModifierType.BowProjectileSpeedFlat, 1f, 5);

            AssertModifier(nodes, "magic_mana_well", SkillModifierType.MaxManaFlat, 10f, 5);
            AssertModifier(nodes, "magic_quick_channel", SkillModifierType.ManaRegenBasePercent, .08f, 3);
            AssertModifier(nodes, "magic_arcane_edge", SkillModifierType.MagicAttackFlat, 1f, 5);
            AssertModifier(nodes, "magic_arcane_bolt_mastery", SkillModifierType.ArcaneBoltDamageFlat, 1f, 5);

            AssertModifier(nodes, "survival_low_rations", SkillModifierType.HungerDrainReduction, .10f, 3);
            AssertModifier(nodes, "survival_status_recovery", SkillModifierType.StatusDurationReduction, .10f, 3);
            AssertModifier(nodes, "survival_safe_step", SkillModifierType.TerrainPenaltyRecovery, .15f, 3);
        }

        [Test]
        public void ModifierEnum_LegacyOrdinalsRemainStableAndTypedChannelsAppend()
        {
            Assert.That((int)SkillModifierType.AttackFlat, Is.EqualTo(0));
            Assert.That((int)SkillModifierType.AttackSpeedBonus, Is.EqualTo(20));
            Assert.That((int)SkillModifierType.MeleeAttackFlat, Is.EqualTo(21));
            Assert.That((int)SkillModifierType.TerrainPenaltyRecovery, Is.EqualTo(27));
        }

        [Test]
        public void RangedAndMagicCapstones_KeepTypedRewardsAndFinalBranchContracts()
        {
            var nodes = Own(DefaultSkillCatalog.BuildAllNodes()).ToDictionary(node => node.SkillNodeId);

            var ranged = nodes["ranged_capstone_eagle_focus"];
            Assert.That(ranged.IsCapstone, Is.True);
            Assert.That(ranged.Tier, Is.EqualTo(5));
            Assert.That(ranged.AuthoredMaxRank, Is.EqualTo(3));
            Assert.That(ranged.PrerequisiteNodeIds,
                Is.EquivalentTo(new[] { "ranged_bleeding_arrow", "ranged_projectile_tuning" }));
            Assert.That(ranged.PassiveModifiers, Has.Count.EqualTo(2));
            Assert.That(ranged.PassiveModifiers.Single(modifier =>
                modifier.ModifierType == SkillModifierType.BowRangeFlat).Value,
                Is.EqualTo(1f).Within(.0001f));
            Assert.That(ranged.PassiveModifiers.Single(modifier =>
                modifier.ModifierType == SkillModifierType.BowProjectileSpeedFlat).Value,
                Is.EqualTo(1f).Within(.0001f));

            var magic = nodes["magic_capstone_elemental_confluence"];
            Assert.That(magic.IsCapstone, Is.True);
            Assert.That(magic.Tier, Is.EqualTo(5));
            Assert.That(magic.AuthoredMaxRank, Is.EqualTo(3));
            Assert.That(magic.PrerequisiteNodeIds,
                Is.EquivalentTo(new[] { "magic_elemental_ward", "magic_slowing_sigils" }));
            Assert.That(magic.CapstoneVariants, Is.EquivalentTo(new[] { "anya", "senya" }));
            Assert.That(magic.PassiveModifiers, Has.Count.EqualTo(2));
            Assert.That(magic.PassiveModifiers.Single(modifier =>
                modifier.ModifierType == SkillModifierType.MagicAttackFlat).Value,
                Is.EqualTo(1f).Within(.0001f));
            Assert.That(magic.PassiveModifiers.Single(modifier =>
                modifier.ModifierType == SkillModifierType.ManaRegenFlat).Value,
                Is.EqualTo(1f).Within(.0001f));
        }

        [Test]
        public void Recompute_ReplacesSnapshotAndPreservesModifierOrigin()
        {
            var node = Own(DefaultSkillCatalog.BuildAllNodes())
                .Single(candidate => candidate.SkillNodeId == "melee_iron_grip");
            var index = new Dictionary<string, SkillNodeDataSO> { { node.SkillNodeId, node } };
            var state = new SkillTreeState(5);
            state.Purchase(node.SkillNodeId, 1, node.TreeId);
            state.RankUp(node.SkillNodeId, node.TreeId);
            var aggregator = new SkillEffectAggregator(index);

            aggregator.Recompute(state);
            aggregator.Recompute(state);

            Assert.That(aggregator.ActiveModifiers, Has.Count.EqualTo(1));
            var modifier = aggregator.ActiveModifiers[0];
            Assert.That(modifier.ModifierType, Is.EqualTo(SkillModifierType.MeleeAttackFlat));
            Assert.That(modifier.Value, Is.EqualTo(2f).Within(.0001f));
            Assert.That(modifier.SourceNodeId, Is.EqualTo("melee_iron_grip"));
            Assert.That(modifier.SourceTreeId, Is.EqualTo("melee"));
        }

        [Test]
        public void Recompute_ClearThenReapply_DoesNotStackPreviousRanks()
        {
            var node = Own(DefaultSkillCatalog.BuildAllNodes())
                .Single(candidate => candidate.SkillNodeId == "survival_low_rations");
            var index = new Dictionary<string, SkillNodeDataSO> { { node.SkillNodeId, node } };
            var state = new SkillTreeState(5);
            state.Purchase(node.SkillNodeId, 1, node.TreeId);
            var aggregator = new SkillEffectAggregator(index);

            aggregator.Recompute(state);
            Assert.That(aggregator.GetTotalModifier(SkillModifierType.HungerDrainReduction),
                Is.EqualTo(.10f).Within(.0001f));

            aggregator.Clear();
            aggregator.Recompute(state);
            Assert.That(aggregator.GetTotalModifier(SkillModifierType.HungerDrainReduction),
                Is.EqualTo(.10f).Within(.0001f));
        }

        private List<SkillNodeDataSO> Own(List<SkillNodeDataSO> nodes)
        {
            _owned.AddRange(nodes);
            return nodes;
        }

        private static void AssertModifier(
            IReadOnlyDictionary<string, SkillNodeDataSO> nodes,
            string nodeId,
            SkillModifierType type,
            float value,
            int maxRank)
        {
            Assert.That(nodes.ContainsKey(nodeId), Is.True, nodeId);
            var node = nodes[nodeId];
            Assert.That(node.AuthoredMaxRank, Is.EqualTo(maxRank), nodeId);
            Assert.That(node.PassiveModifiers, Has.Count.EqualTo(1), nodeId);
            Assert.That(node.PassiveModifiers[0].ModifierType, Is.EqualTo(type), nodeId);
            Assert.That(node.PassiveModifiers[0].Value, Is.EqualTo(value).Within(.0001f), nodeId);
        }
    }
}
