using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class CombatCapstoneMeleeTests
    {
        private CombatCapstoneRuntime _runtime;
        private CombatCapstoneState _state;
        private int _rank;
        private string _variant;
        private int _health;
        private int _maxHealth;
        private int _staminaReward;

        [SetUp]
        public void SetUp()
        {
            _state = new CombatCapstoneState();
            _rank = 3;
            _variant = CombatCapstoneState.KanthorVariant;
            _health = 80;
            _maxHealth = 100;
            _staminaReward = 0;
            _runtime = new CombatCapstoneRuntime(
                _state, () => _rank, () => _variant, () => _health, () => _maxHealth,
                amount => _health += amount, amount => _staminaReward += amount);
            _runtime.Enable();
        }

        [TearDown]
        public void TearDown()
        {
            _runtime?.Dispose();
            CombatCapstoneModifierProvider.Source = null;
            PlayerControlResistanceProvider.Source = null;
        }

        [Test]
        public void PerfectBlockAndPostureBreak_SameResolution_ActivateKanthorOnce()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            Assert.That(_state.IsActive, Is.True);
            Assert.That(_state.HealChargeArmed, Is.True);

            _runtime.Tick(2f);
            GameEventBus.Publish(new EnemyPostureBrokenEvent(
                "slime", "slime-1", "perfect_block", "player", true, "resolution-1"));

            Assert.That(_state.RemainingSeconds, Is.EqualTo(6f).Within(0.001f),
                "O segundo evento da mesma resolução não pode refrescar a janela.");
        }

        [Test]
        public void DistinctTrigger_RefreshesWindowAndRearmsSingleCharge()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            PublishConfirmedMeleeDamage(10);
            Assert.That(_state.HealChargeArmed, Is.False);

            _runtime.Tick(3f);
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-2"));

            Assert.That(_state.RemainingSeconds, Is.EqualTo(8f));
            Assert.That(_state.HealChargeArmed, Is.True);
        }

        [Test]
        public void Kanthor_ConsumesOnlyConfirmedPrimaryMelee_AndUsesCeilHeal()
        {
            _rank = 2;
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            PublishDamage(DamageSourceKind.DamageOverTime, 10, true, true);
            PublishDamage(DamageSourceKind.PlayerMelee, 10, false, true);
            PublishDamage(DamageSourceKind.PlayerMelee, 0, true, true);
            Assert.That(_state.HealChargeArmed, Is.True);

            _maxHealth = 101;
            PublishConfirmedMeleeDamage(10);

            Assert.That(_health, Is.EqualTo(84), "ceil(101 × 3%) = 4.");
            Assert.That(_state.HealChargeArmed, Is.False);
        }

        [Test]
        public void KanthorRankThree_AtFullHealth_RestoresTenStaminaInstead()
        {
            _health = _maxHealth;
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            PublishConfirmedMeleeDamage(10);

            Assert.That(_staminaReward, Is.EqualTo(10));
            Assert.That(_health, Is.EqualTo(_maxHealth));
        }

        [Test]
        public void Kaand_PlayerPostureBreak_UsesRankValuesAndExpires()
        {
            _variant = CombatCapstoneState.KaandVariant;
            _rank = 2;
            GameEventBus.Publish(new EnemyPostureBrokenEvent(
                "slime", "slime-1", "sword", "player", true, "break-1"));

            Assert.That(_runtime.DirectMeleeDamageMultiplier, Is.EqualTo(1.24f).Within(0.0001f));
            Assert.That(_runtime.MeleeCriticalDamageBonus, Is.EqualTo(0.20f).Within(0.0001f));
            Assert.That(_runtime.KnockbackReductionFraction, Is.Zero);

            _runtime.Tick(8f);
            Assert.That(_state.IsActive, Is.False);
            Assert.That(_runtime.DirectMeleeDamageMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Kanthor_ResistanceAffectsOnlyKnockbackAndStunProviders()
        {
            CombatCapstoneModifierProvider.Source = _runtime;
            PlayerControlResistanceProvider.Source = _runtime;
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));

            Assert.That(PlayerControlResistanceProvider.ResolveKnockbackForce(8f), Is.EqualTo(6f).Within(0.001f));
            Assert.That(PlayerControlResistanceProvider.ResolveStunDuration(5f), Is.EqualTo(4f).Within(0.001f));
            Assert.That(CombatCapstoneModifierProvider.ResolveDirectMeleeDamageMultiplier(),
                Is.EqualTo(1.15f).Within(0.001f));
        }

        [Test]
        public void Respec_ClearsActiveWindowAndCharge()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            GameEventBus.Publish(new SkillTreeRespecCompletedEvent(5, 1));

            Assert.That(_state.IsActive, Is.False);
            Assert.That(_state.HealChargeArmed, Is.False);
        }

        [Test]
        public void SaveRoundTrip_PreservesRemainingChargeAndDedupeWithoutTrigger()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("claw", 20, "resolution-1"));
            _runtime.Tick(2.5f);
            CombatCapstoneSaveData save = _state.CaptureSaveData();
            var restored = new CombatCapstoneState();

            restored.RestoreFromSaveData(save);

            Assert.That(restored.ActiveVariant, Is.EqualTo(CombatCapstoneState.KanthorVariant));
            Assert.That(restored.ActiveRank, Is.EqualTo(3));
            Assert.That(restored.RemainingSeconds, Is.EqualTo(5.5f).Within(0.001f));
            Assert.That(restored.HealChargeArmed, Is.True);
            Assert.That(restored.TryActivate(CombatCapstoneState.KanthorVariant, 3, "resolution-1"), Is.False);
        }

        [Test]
        public void PlayerCombatStatsProvider_AppliesKaandDamageAndCriticalBonusOnce()
        {
            _variant = CombatCapstoneState.KaandVariant;
            _rank = 3;
            GameEventBus.Publish(new EnemyPostureBrokenEvent(
                "slime", "slime-1", "sword", "player", true, "break-1"));
            CombatCapstoneModifierProvider.Source = _runtime;
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.BaseDamage = 100;
            try
            {
                using var stats = new PlayerCombatStatsProvider(() => 0, () => null,
                    critRoll: () => 0f);
                int final = stats.FinalMeleeDamage(
                    weapon, EquipmentSlot.RightHand,
                    AttackWeight.Light, false, out bool crit);

                Assert.That(crit, Is.True);
                Assert.That(final, Is.EqualTo(228), "100 × 1.30 × (1.50 + 0.25), arredondado.");
            }
            finally
            {
                Object.DestroyImmediate(weapon);
            }
        }

        private static void PublishConfirmedMeleeDamage(int finalDamage)
            => PublishDamage(DamageSourceKind.PlayerMelee, finalDamage, true, true);

        private static void PublishDamage(DamageSourceKind kind, int finalDamage, bool primary, bool gate)
        {
            GameEventBus.Publish(new DamageAppliedEvent(new DamageResult
            {
                SourceKind = kind,
                FinalDamage = finalDamage,
                IsPrimaryDamage = primary,
                CanTriggerCapstones = gate
            }));
        }
    }
}
