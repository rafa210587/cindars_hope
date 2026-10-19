using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class RangedLunarRuntimeTests
    {
        [TearDown]
        public void TearDown()
        {
            GameEventBus.Clear<DamageAppliedEvent>();
            GameEventBus.Clear<SkillTreeRespecCompletedEvent>();
            RangedLunarModifierProvider.Source = null;
        }

        [Test]
        public void Curves_AreExactAtEveryRank()
        {
            Assert.That(RangedLunarRules.FocusDuration(1), Is.EqualTo(6f));
            Assert.That(RangedLunarRules.FocusDuration(2), Is.EqualTo(8f));
            Assert.That(RangedLunarRules.FocusDuration(3), Is.EqualTo(10f));
            Assert.That(RangedLunarRules.AlihanaLaunch(1).RangeMultiplier, Is.EqualTo(1.08f));
            Assert.That(RangedLunarRules.AlihanaLaunch(2).RangeMultiplier, Is.EqualTo(1.12f));
            Assert.That(RangedLunarRules.AlihanaLaunch(3).RangeMultiplier, Is.EqualTo(1.16f));
            Assert.That(RangedLunarRules.AlihanaLaunch(3).ProjectileSpeedMultiplier,
                Is.EqualTo(1.15f));
            Assert.That(RangedLunarRules.SenyaSecondaryDamage(100, 1), Is.EqualTo(8));
            Assert.That(RangedLunarRules.SenyaSecondaryDamage(100, 2), Is.EqualTo(12));
            Assert.That(RangedLunarRules.SenyaSecondaryDamage(100, 3), Is.EqualTo(16));
        }

        [Test]
        public void FirstTarget_RemainsConsumedAfterExpiryDeathAndRespec()
        {
            var state = Encounter("enemy-a", "enemy-b");
            Assert.That(state.TryActivateLunarFocus("enemy-a", "nyx", 1, 6f, "token-a"),
                Is.True);
            Assert.That(state.AdvanceLunarFocusTime(6f, out _), Is.True);
            Assert.That(state.TryActivateLunarFocus("enemy-b", "nyx", 1, 6f, "token-b"),
                Is.False);

            state.ClearActiveLunarFocus();
            Assert.That(state.LunarConsumedFirstTargetInstanceId, Is.EqualTo("enemy-a"));
            state.RemoveEnemyAndResolveIfEmpty("enemy-a", out _);
            Assert.That(state.TryActivateLunarFocus("enemy-b", "nyx", 1, 6f, "token-b"),
                Is.False);
        }

        [Test]
        public void SaveLoad_PreservesConsumedTargetAndActiveFocusWithoutRetrigger()
        {
            var source = Encounter("enemy-a", "enemy-b");
            source.TryActivateLunarFocus("enemy-a", "alihana", 2, 8f, "token-a");
            source.AdvanceLunarFocusTime(2f, out _);
            var restored = new SurvivalSkillState();
            restored.BeginRestore();
            restored.RestoreFromSaveData(source.CaptureSaveData(), "run-a", 2);
            restored.EndRestore();

            Assert.That(restored.LunarConsumedFirstTargetInstanceId, Is.EqualTo("enemy-a"));
            Assert.That(restored.ActiveLunarTargetInstanceId, Is.EqualTo("enemy-a"));
            Assert.That(restored.ActiveLunarRemainingSeconds, Is.EqualTo(6f));
            Assert.That(restored.TryActivateLunarFocus(
                "enemy-b", "alihana", 2, 8f, "token-b"), Is.False);
        }

        [Test]
        public void Nyx_IsolationUsesInclusiveTwoPointFiveTileBoundary()
        {
            var onlyTarget = new List<RangedHostileSnapshot>
            {
                new RangedHostileSnapshot("target", 0f, 0f)
            };
            Assert.That(RangedLunarRules.IsIsolated("target", 0f, 0f, onlyTarget), Is.True);

            onlyTarget.Add(new RangedHostileSnapshot("other", 2.5f, 0f));
            Assert.That(RangedLunarRules.IsIsolated("target", 0f, 0f, onlyTarget), Is.False);
            onlyTarget[1] = new RangedHostileSnapshot("other", 2.5001f, 0f);
            Assert.That(RangedLunarRules.IsIsolated("target", 0f, 0f, onlyTarget), Is.True);

            var rankThree = RangedLunarRules.NyxImpact(3, true);
            Assert.That(rankThree.CriticalChanceBonus, Is.EqualTo(.15f));
            Assert.That(rankThree.CriticalDamageBonus, Is.EqualTo(.15f));
            Assert.That(RangedLunarRules.ApplyCritical(100, false, .19f,
                rankThree, out bool critical), Is.EqualTo(165));
            Assert.That(critical, Is.True);
        }

        [Test]
        public void Senya_UsesLastConfirmedMagicAndSecondaryCannotRecurseOrReact()
        {
            var state = Encounter("enemy-a");
            var hostiles = new List<RangedHostileSnapshot>
            {
                new RangedHostileSnapshot("enemy-a", 0f, 0f)
            };
            DamageRequest secondary = null;
            var runtime = new RangedLunarRuntime(state, () => 3, () => "senya",
                () => hostiles, (_, request) => secondary = request);
            runtime.Enable();
            try
            {
                Assert.That(runtime.TryActivateFirstTarget("enemy-a", "mark-token"), Is.True);
                GameEventBus.Publish(new PlayerMagicCastCommittedEvent(
                    "spiritual", "spiritual-token", SpellDiscipline.Spiritual,
                    DamageType.Ice.ToString()));
                Assert.That(state.LastOffensiveMagicDamageType,
                    Is.EqualTo(DamageType.Physical));

                GameEventBus.Publish(new PlayerMagicCastCommittedEvent(
                    "offensive", "offensive-token", SpellDiscipline.Offensive,
                    DamageType.Fire.ToString()));
                GameEventBus.Publish(new DamageAppliedEvent(new DamageResult
                {
                    TargetId = "enemy",
                    TargetInstanceId = "enemy-a",
                    SourceInstanceId = "player",
                    ActionToken = "arrow-token",
                    FinalDamage = 100,
                    DamageType = DamageType.Physical,
                    SourceKind = DamageSourceKind.PlayerRanged,
                    IsPrimaryDamage = true,
                    CanTriggerCapstones = true
                }));

                Assert.That(secondary, Is.Not.Null);
                Assert.That(secondary.BaseDamage, Is.EqualTo(16));
                Assert.That(secondary.DamageType, Is.EqualTo(DamageType.Fire));
                Assert.That(secondary.SourceKind, Is.EqualTo(DamageSourceKind.CapstoneSecondary));
                Assert.That(secondary.IsPrimaryDamage, Is.False);
                Assert.That(secondary.IsCritical, Is.False);
                Assert.That(secondary.CanTriggerCapstones, Is.False);
                Assert.That(secondary.CanTriggerStatusEffects, Is.False);
                Assert.That(secondary.CanTriggerReactions, Is.False);
            }
            finally
            {
                runtime.Dispose();
            }
        }

        private static SurvivalSkillState Encounter(params string[] enemies)
        {
            var state = new SurvivalSkillState();
            for (int i = 0; i < enemies.Length; i++)
                state.RegisterEnemyAggro("run-a", 2, enemies[i]);
            return state;
        }
    }
}
