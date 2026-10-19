using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class CombatPassiveConsumerTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
                if (_created[i] != null) Object.DestroyImmediate(_created[i]);
            _created.Clear();
        }

        [Test]
        public void TypedDamageChannels_ReachOnlyTheirIntendedAttack()
        {
            var sword = Weapon(WeaponType.Sword, 10, WeaponWeightClass.Medium);
            var bow = Weapon(WeaponType.Bow, 10, WeaponWeightClass.Light);
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.MeleeAttackFlat, 2f, PassiveCombatModifierAdapter.IronGripNodeId),
                Mod(SkillModifierType.BowDamageFlat, 5f, PassiveCombatModifierAdapter.SteadyHandNodeId),
                Mod(SkillModifierType.MagicAttackFlat, 3f, PassiveCombatModifierAdapter.ArcaneEdgeNodeId),
                Mod(SkillModifierType.ArcaneBoltDamageFlat, 4f, PassiveCombatModifierAdapter.ArcaneBoltMasteryNodeId)
            };
            var slots = new Dictionary<EquipmentSlot, WeaponDataSO>
            {
                [EquipmentSlot.RightHand] = sword,
                [EquipmentSlot.LeftHand] = null
            };
            using var provider = Provider(passives, slots, baseAttack: 7);

            Assert.AreEqual(19, provider.FinalMeleeDamage(sword, EquipmentSlot.RightHand,
                AttackWeight.Light, false, out _));
            Assert.AreEqual(13, provider.FinalSpellDamage(10, "spell_fire", false, out _));
            Assert.AreEqual(17, provider.FinalSpellDamage(10,
                PassiveCombatModifierAdapter.ArcaneBoltSpellId, false, out _));

            slots[EquipmentSlot.RightHand] = bow;
            provider.Invalidate();
            Assert.AreEqual(22, provider.FinalBowDamage(bow, 0, EquipmentSlot.RightHand, false, out _));
        }

        [Test]
        public void DerivedStats_TypedChannelsRemainIndependentFromLegacyAndEachOther()
        {
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.MeleeAttackFlat, 2f, PassiveCombatModifierAdapter.IronGripNodeId),
                Mod(SkillModifierType.MagicAttackFlat, 3f, PassiveCombatModifierAdapter.ArcaneEdgeNodeId),
                Mod(SkillModifierType.ArcaneBoltDamageFlat, 4f, PassiveCombatModifierAdapter.ArcaneBoltMasteryNodeId),
                Mod(SkillModifierType.BowRecoverySpeed, .16f, PassiveCombatModifierAdapter.QuickNockNodeId),
                Mod(SkillModifierType.ManaRegenBasePercent, .24f, "magic_quick_channel"),
                Mod(SkillModifierType.TerrainPenaltyRecovery, .35f, "survival_safe_step"),
                Mod(SkillModifierType.KitingMoveSpeedBonus, .20f, PassiveCombatModifierAdapter.KitingStepsNodeId)
            };

            var stats = DerivedStatsCalculator.Calculate(
                100, 7, 3, 5f, 80, 2f, 1f, null, passives);

            Assert.That(stats.Attack, Is.EqualTo(7), "typed attack channels must not leak into legacy Attack");
            Assert.That(stats.MoveSpeed, Is.EqualTo(5f), "conditional movement channels must not alter base MoveSpeed");
            Assert.That(stats.MeleeAttackBonus, Is.EqualTo(2f));
            Assert.That(stats.MagicAttackBonus, Is.EqualTo(3f));
            Assert.That(stats.ArcaneBoltDamageBonus, Is.EqualTo(4f));
            Assert.That(stats.BowRecoverySpeed, Is.EqualTo(.16f).Within(.0001f));
            Assert.That(stats.ManaRegenBasePercent, Is.EqualTo(.24f).Within(.0001f));
            Assert.That(stats.TerrainPenaltyRecovery, Is.EqualTo(.35f).Within(.0001f));
            Assert.That(stats.KitingMoveSpeedBonus, Is.EqualTo(.20f).Within(.0001f));
        }

        [Test]
        public void EquipmentGates_RequireValidShieldLightPairAndEmptyTwoHandedOffhand()
        {
            var daggerA = Weapon(WeaponType.Dagger, 10, WeaponWeightClass.Light);
            var daggerB = Weapon(WeaponType.Dagger, 10, WeaponWeightClass.Light);
            var hammer = Weapon(WeaponType.Hammer, 10, WeaponWeightClass.Heavy, requiresTwoHands: true);
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.GuardedDefenseFlat, 3f, PassiveCombatModifierAdapter.GuardedStanceNodeId),
                Mod(SkillModifierType.DualWieldRecoverySpeed, 0.15f, PassiveCombatModifierAdapter.DualWieldFlowNodeId),
                Mod(SkillModifierType.TwoHandedDamageBonus, 0.25f, PassiveCombatModifierAdapter.TwoHandedMomentumNodeId)
            };

            var shield = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, null, offHandIsShield: true, offHandEmpty: false);
            Assert.AreEqual(3f, PassiveCombatModifierAdapter.GuardedDefenseFlat(passives, shield), 0.0001f);
            var brokenShield = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, null, offHandBroken: true, offHandIsShield: true, offHandEmpty: false);
            Assert.AreEqual(0f, PassiveCombatModifierAdapter.GuardedDefenseFlat(passives, brokenShield));
            var brokenGuardMain = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, null, mainHandBroken: true, offHandIsShield: true, offHandEmpty: false);
            Assert.AreEqual(0f, PassiveCombatModifierAdapter.GuardedDefenseFlat(passives, brokenGuardMain));

            var dual = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, daggerB, offHandEmpty: false);
            Assert.AreEqual(0.85f, PassiveCombatModifierAdapter.DualWieldRecoveryMultiplier(passives, dual), 0.0001f);
            var brokenDual = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, daggerB, offHandBroken: true, offHandEmpty: false);
            Assert.AreEqual(1f, PassiveCombatModifierAdapter.DualWieldRecoveryMultiplier(passives, brokenDual), 0.0001f);
            var brokenDualMain = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                daggerA, daggerB, mainHandBroken: true, offHandEmpty: false);
            Assert.AreEqual(1f, PassiveCombatModifierAdapter.DualWieldRecoveryMultiplier(passives, brokenDualMain), 0.0001f);

            var twoHanded = new PassiveCombatModifierAdapter.EquipmentSnapshot(hammer, null, offHandEmpty: true);
            Assert.AreEqual(1.25f, PassiveCombatModifierAdapter.TwoHandedDamageMultiplier(passives, twoHanded), 0.0001f);
            var occupied = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                hammer, daggerB, offHandEmpty: false);
            Assert.AreEqual(1f, PassiveCombatModifierAdapter.TwoHandedDamageMultiplier(passives, occupied), 0.0001f);
            var brokenTwoHanded = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                hammer, null, mainHandBroken: true, offHandEmpty: true);
            Assert.AreEqual(1f, PassiveCombatModifierAdapter.TwoHandedDamageMultiplier(passives, brokenTwoHanded), 0.0001f);
        }

        [Test]
        public void BowPassives_BrokenBowDisablesEveryEquipmentGatedBenefit()
        {
            var bow = Weapon(WeaponType.Bow, 10, WeaponWeightClass.Light);
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.BowDamageFlat, 5f, PassiveCombatModifierAdapter.SteadyHandNodeId),
                Mod(SkillModifierType.BowRangeFlat, 1.5f, PassiveCombatModifierAdapter.LongSightNodeId),
                Mod(SkillModifierType.BowRecoverySpeed, .24f, PassiveCombatModifierAdapter.QuickNockNodeId),
                Mod(SkillModifierType.BowProjectileSpeedFlat, 5f, PassiveCombatModifierAdapter.ProjectileTuningNodeId)
            };
            var broken = new PassiveCombatModifierAdapter.EquipmentSnapshot(
                bow, null, mainHandBroken: true);

            Assert.That(PassiveCombatModifierAdapter.BowDamageFlat(passives, broken), Is.Zero);
            Assert.That(PassiveCombatModifierAdapter.BowRangeFlat(passives, broken), Is.Zero);
            Assert.That(PassiveCombatModifierAdapter.BowRecoveryMultiplier(passives, broken), Is.EqualTo(1f));
            Assert.That(PassiveCombatModifierAdapter.BowProjectileSpeedFlat(passives, broken), Is.Zero);
        }

        [Test]
        public void BowPassives_ChangeRecoveryRangeAndActualProjectileSpeedOnlyForBow()
        {
            var bow = Weapon(WeaponType.Bow, 10, WeaponWeightClass.Light);
            var sword = Weapon(WeaponType.Sword, 10, WeaponWeightClass.Medium);
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.BowRangeFlat, 1.5f, PassiveCombatModifierAdapter.LongSightNodeId),
                Mod(SkillModifierType.BowRecoverySpeed, 0.24f, PassiveCombatModifierAdapter.QuickNockNodeId),
                Mod(SkillModifierType.BowProjectileSpeedFlat, 5f, PassiveCombatModifierAdapter.ProjectileTuningNodeId)
            };
            var slots = new Dictionary<EquipmentSlot, WeaponDataSO> { [EquipmentSlot.RightHand] = bow };
            using var provider = Provider(passives, slots);

            Assert.AreEqual(0.76f, provider.FinalBowRecovery(1f, EquipmentSlot.RightHand), 0.0001f);
            Assert.AreEqual(7.5f, provider.FinalBowRange(6f, EquipmentSlot.RightHand), 0.0001f);
            Assert.AreEqual(15f, provider.FinalBowProjectileSpeed(10f, EquipmentSlot.RightHand), 0.0001f);

            slots[EquipmentSlot.RightHand] = sword;
            provider.Invalidate();
            Assert.AreEqual(1f, provider.FinalBowRecovery(1f, EquipmentSlot.RightHand), 0.0001f);
            Assert.AreEqual(6f, provider.FinalBowRange(6f, EquipmentSlot.RightHand), 0.0001f);
            Assert.AreEqual(10f, provider.FinalBowProjectileSpeed(10f, EquipmentSlot.RightHand), 0.0001f);
        }

        [Test]
        public void SkillDerivedStatsChanged_InvalidatesCachedProviderValueIdempotently()
        {
            var passives = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.AttackFlat, 1f, "legacy_attack")
            };
            using var provider = new PlayerCombatStatsProvider(() => 0, () => passives, critRoll: () => 1f);
            Assert.AreEqual(1, provider.Current.Attack);

            passives[0].Value = 4f;
            Assert.AreEqual(1, provider.Current.Attack, "cache remains stable before the recompute event");
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            Assert.AreEqual(4, provider.Current.Attack);
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            Assert.AreEqual(4, provider.Current.Attack);
        }

        [Test]
        public void KitingDirection_RequiresMovementAwayAtInclusiveDotThreshold()
        {
            Assert.IsTrue(RangedKitingRuntime.ShouldApply(Vector2.right, Vector2.right, Vector2.zero));
            Assert.IsFalse(RangedKitingRuntime.ShouldApply(Vector2.left, Vector2.right, Vector2.zero));
            Assert.IsFalse(RangedKitingRuntime.ShouldApply(Vector2.zero, Vector2.right, Vector2.zero));

            Vector2 sixtyDegrees = new Vector2(0.5f, 0.8660254f);
            Assert.IsTrue(RangedKitingRuntime.ShouldApply(sixtyDegrees, Vector2.right, Vector2.zero));
        }

        [Test]
        public void DodgeTraining_ComposesMultiplicativelyWithRetreatAndKeepsFiftyPercentFloor()
        {
            var rankOne = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.DodgeCostReduction, 0.1f, "melee_dodge_training")
            };
            Assert.AreEqual(0.9f, DodgeCostModifierProvider.Resolve(rankOne), 0.0001f);
            Assert.AreEqual(0.63f, DodgeCostModifierProvider.Resolve(rankOne, 0.7f), 0.0001f);

            var rankThree = new List<SkillPassiveModifier>
            {
                Mod(SkillModifierType.DodgeCostReduction, 0.3f, "melee_dodge_training")
            };
            Assert.AreEqual(0.7f, DodgeCostModifierProvider.Resolve(rankThree), 0.0001f);
            Assert.AreEqual(0.5f, DodgeCostModifierProvider.Resolve(rankThree, 0.5f), 0.0001f);
        }

        private PlayerCombatStatsProvider Provider(
            List<SkillPassiveModifier> passives,
            Dictionary<EquipmentSlot, WeaponDataSO> slots,
            int baseAttack = 0)
        {
            var provider = new PlayerCombatStatsProvider(() => baseAttack, () => passives, critRoll: () => 1f);
            provider.ConfigureCombatEquipment(
                slot => slots.TryGetValue(slot, out var weapon) ? weapon : null,
                _ => false,
                _ => false,
                slot => !slots.TryGetValue(slot, out var weapon) || weapon == null);
            return provider;
        }

        private WeaponDataSO Weapon(
            WeaponType type,
            int damage,
            WeaponWeightClass weight,
            bool requiresTwoHands = false)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.Type = type;
            weapon.BaseDamage = damage;
            weapon.WeightClass = weight;
            weapon.RequiresTwoHands = requiresTwoHands;
            _created.Add(weapon);
            return weapon;
        }

        private static SkillPassiveModifier Mod(SkillModifierType type, float value, string nodeId)
            => new SkillPassiveModifier(type, value, nodeId, "test");
    }
}
