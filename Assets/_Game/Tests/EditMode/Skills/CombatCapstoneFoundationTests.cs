using CindarsHope.Combat;
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class CombatCapstoneFoundationTests
    {
        [TearDown]
        public void TearDown()
        {
            PlayerControlResistanceProvider.Source = null;
            SpellCastPreparationProvider.Source = null;
        }

        [Test]
        public void LegacyDamageRequestAndResultKeepNeutralMetadata()
        {
            var request = new DamageRequest("slime", 7);
            var result = new DamageResult(request);

            Assert.AreEqual(DamageSourceKind.None, result.SourceKind);
            Assert.AreEqual(string.Empty, result.SourceInstanceId);
            Assert.AreEqual(string.Empty, result.TargetInstanceId);
            Assert.AreEqual(string.Empty, result.ActionToken);
            Assert.IsFalse(result.IsCritical);
            Assert.IsTrue(result.IsPrimaryDamage);
            Assert.IsFalse(result.CanTriggerCapstones);
            Assert.IsTrue(result.CanTriggerStatusEffects);
            Assert.IsTrue(result.CanTriggerReactions);
        }

        [Test]
        public void DamageResultCopiesExplicitActionIdentityAndGates()
        {
            var request = new DamageRequest("slime", 9)
            {
                SourceKind = DamageSourceKind.PlayerRanged,
                SourceInstanceId = "player-1",
                TargetInstanceId = "slime-4",
                ActionToken = "shot-10",
                IsCritical = true,
                IsPrimaryDamage = false,
                CanTriggerCapstones = true,
                CanTriggerStatusEffects = false,
                CanTriggerReactions = false
            };

            var result = new DamageResult(request);
            Assert.AreEqual(DamageSourceKind.PlayerRanged, result.SourceKind);
            Assert.AreEqual("player-1", result.SourceInstanceId);
            Assert.AreEqual("slime-4", result.TargetInstanceId);
            Assert.AreEqual("shot-10", result.ActionToken);
            Assert.IsTrue(result.IsCritical);
            Assert.IsFalse(result.IsPrimaryDamage);
            Assert.IsTrue(result.CanTriggerCapstones);
            Assert.IsFalse(result.CanTriggerStatusEffects);
            Assert.IsFalse(result.CanTriggerReactions);
        }

        [Test]
        public void EnemyHealthOverridesCallerTargetInstanceBeforePublishingDamage()
        {
            var enemyObject = new GameObject("phase19a-enemy");
            var health = enemyObject.AddComponent<EnemyHealth>();
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.enemyId = "slime";
            data.maxHp = 20;
            health.Configure(data);
            health.ConfigureLootContext("slime-instance-7", "run");
            DamageAppliedEvent observed = null;
            System.Action<DamageAppliedEvent> handler = evt => observed = evt;
            GameEventBus.Subscribe(handler);

            health.TakeDamage(new DamageRequest("slime", 3)
            {
                TargetInstanceId = "caller-stale-id"
            });

            Assert.IsNotNull(observed);
            Assert.AreEqual("slime-instance-7", observed.DamageResult.TargetInstanceId);
            GameEventBus.Unsubscribe(handler);
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void LegacyCombatEventsRemainConstructibleWithNeutralCausality()
        {
            var posture = new EnemyPostureBrokenEvent("slime");
            var block = new PlayerPerfectBlockEvent("claw", 12);

            Assert.AreEqual("slime", posture.EnemyId);
            Assert.IsFalse(posture.CausedByPlayer);
            Assert.AreEqual(string.Empty, posture.ResolutionId);
            Assert.AreEqual("claw", block.SourceId);
            Assert.AreEqual(string.Empty, block.ResolutionId);
        }

        [Test]
        public void ExpandedPostureEventCarriesPlayerCausality()
        {
            var posture = new EnemyPostureBrokenEvent(
                "slime", "slime-2", "perfect_block", "player", true, "resolution-1");

            Assert.AreEqual("slime-2", posture.EnemyInstanceId);
            Assert.AreEqual("perfect_block", posture.SourceId);
            Assert.AreEqual("player", posture.SourceInstanceId);
            Assert.IsTrue(posture.CausedByPlayer);
            Assert.AreEqual("resolution-1", posture.ResolutionId);
        }

        [Test]
        public void ControlProviderIsNeutralUntilSourceIsRegistered()
        {
            Assert.AreEqual(8f, PlayerControlResistanceProvider.ResolveKnockbackForce(8f));
            Assert.AreEqual(3f, PlayerControlResistanceProvider.ResolveStunDuration(3f));

            PlayerControlResistanceProvider.Source = new FixedControlResistance(0.25f, 0.2f);
            Assert.AreEqual(6f, PlayerControlResistanceProvider.ResolveKnockbackForce(8f), 0.001f);
            Assert.AreEqual(2.4f, PlayerControlResistanceProvider.ResolveStunDuration(3f), 0.001f);
            Assert.AreEqual(2.4f, PlayerStatusReceiver.ApplyStunDurationReduction(3f), 0.001f);

            PlayerControlResistanceProvider.Source = new FixedControlResistance(0f, 0.9f);
            Assert.AreEqual(1f, PlayerStatusReceiver.ApplyStunDurationReduction(3f),
                "The canonical final status-duration floor remains one second.");
        }

        [Test]
        public void SpellAssetsDefaultToUnclassifiedDiscipline()
        {
            var spell = ScriptableObject.CreateInstance<SpellDataSO>();
            var action = ScriptableObject.CreateInstance<SkillActionSO>();
            Assert.AreEqual(SpellDiscipline.None, spell.Discipline);
            Assert.AreEqual(SpellDiscipline.None, action.SpellDiscipline);
            Object.DestroyImmediate(spell);
            Object.DestroyImmediate(action);
        }

        [Test]
        public void NeutralPreparationPreservesBaseCostAndCancelRefundsReservationOnce()
        {
            var request = new SpellCastPreparationRequest(
                "fireball", "cast-1", SpellDiscipline.Offensive, 12);
            var transaction = new SpellCastTransaction(SpellCastPreparationProvider.Prepare(request));
            int spent = 0;
            int refunded = 0;

            Assert.AreEqual(12, transaction.Preparation.ManaCost);
            Assert.IsTrue(transaction.TryReserve(cost => { spent += cost; return true; }));
            Assert.AreEqual(SpellCastTransactionState.Reserved, transaction.State);
            Assert.IsTrue(transaction.Cancel(amount => refunded += amount));
            Assert.AreEqual(12, spent);
            Assert.AreEqual(12, refunded);
            Assert.IsFalse(transaction.Cancel(amount => refunded += amount));
            Assert.AreEqual(12, refunded);
        }

        [Test]
        public void FailedReservationCanRetryAndCommittedCastCannotRefund()
        {
            var request = new SpellCastPreparationRequest(
                "heal", "cast-2", SpellDiscipline.Spiritual, 6, true);
            var transaction = new SpellCastTransaction(SpellCastPreparationProvider.Prepare(request));

            Assert.IsFalse(transaction.TryReserve(_ => false));
            Assert.AreEqual(SpellCastTransactionState.Prepared, transaction.State);
            Assert.IsTrue(transaction.TryReserve(_ => true));
            Assert.IsTrue(transaction.Commit());
            Assert.IsFalse(transaction.Cancel());
        }

        [Test]
        public void ProjectileContextDefaultsAreNeutralAndImpactResolverIsExplicit()
        {
            var request = new ProjectileSpawnRequest();
            Assert.AreEqual(DamageSourceKind.None, request.SourceKind);
            Assert.IsFalse(request.CanTriggerCapstones);
            Assert.IsTrue(request.CanTriggerStatusEffects);
            Assert.IsTrue(request.CanTriggerReactions);
            Assert.IsNull(request.ImpactDamageResolver);

            int calls = 0;
            request.ImpactDamageResolver = guaranteed =>
            {
                calls++;
                return new ProjectileImpactDamage(guaranteed ? 15 : 10, guaranteed);
            };
            var impact = request.ImpactDamageResolver(true);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(15, impact.Damage);
            Assert.IsTrue(impact.IsCritical);
        }

        private sealed class FixedControlResistance : IPlayerControlResistanceSource
        {
            public float KnockbackReductionFraction { get; }
            public float StunDurationReductionFraction { get; }

            public FixedControlResistance(float knockback, float stun)
            {
                KnockbackReductionFraction = knockback;
                StunDurationReductionFraction = stun;
            }
        }
    }
}
