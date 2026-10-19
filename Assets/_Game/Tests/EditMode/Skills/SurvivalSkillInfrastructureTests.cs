using System.Collections.Generic;
using System.Linq;
using CindarsHope.Editor.Items;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class SurvivalSkillInfrastructureTests
    {
        [TearDown]
        public void TearDown()
        {
            DirectionalMobilityModifierProvider.Source = null;
        }

        [Test]
        public void SurvivalSupplyCatalog_DeclaresExactStableIdsAndNoRestorePayload()
        {
            var rows = CanonicalItemCatalog.BaseRows()
                .Where(row => row.Id == SurvivalSkillItemIds.ImprovisedLure
                    || row.Id == SurvivalSkillItemIds.FieldDressing
                    || row.Id == SurvivalSkillItemIds.CampSupply)
                .ToList();

            Assert.That(rows.Select(row => row.Id), Is.EquivalentTo(new[]
            {
                SurvivalSkillItemIds.ImprovisedLure,
                SurvivalSkillItemIds.FieldDressing,
                SurvivalSkillItemIds.CampSupply
            }));
            Assert.That(rows.All(row => row.Category == ItemCategory.Consumable), Is.True);
            Assert.That(rows.All(row => row.BaseValue > 0 && row.MaxStack > 0), Is.True);
            Assert.That(rows.All(row => row.HungerRestore == 0 && row.StaminaRestore == 0), Is.True,
                "Survival actions, not ordinary item use, own these effects.");
            Assert.That(rows.All(row => !row.IsEquippable), Is.True);
        }

        [Test]
        public void ItemCommit_InvalidOrMissingRequest_DoesNotMutateInventory()
        {
            var inventory = new FakeSkillInventory(SurvivalSkillItemIds.FieldDressing, 1);
            var transaction = new SkillItemTransaction(inventory);

            Assert.That(transaction.TryCommit(string.Empty).Failure,
                Is.EqualTo(SkillItemCommitFailure.InvalidRequest));
            Assert.That(transaction.TryCommit(SurvivalSkillItemIds.FieldDressing, 0).Failure,
                Is.EqualTo(SkillItemCommitFailure.InvalidRequest));
            Assert.That(transaction.TryCommit(SurvivalSkillItemIds.CampSupply).Failure,
                Is.EqualTo(SkillItemCommitFailure.MissingItem));
            Assert.That(inventory.Amount, Is.EqualTo(1));
            Assert.That(inventory.RemoveCalls, Is.Zero);
        }

        [Test]
        public void ItemCommit_RemoveFailureAndSuccess_PreserveAtomicContract()
        {
            var inventory = new FakeSkillInventory(SurvivalSkillItemIds.ImprovisedLure, 2)
            {
                RejectRemove = true
            };
            var transaction = new SkillItemTransaction(inventory);

            var rejected = transaction.TryCommit(SurvivalSkillItemIds.ImprovisedLure, 2);
            Assert.That(rejected.Failure, Is.EqualTo(SkillItemCommitFailure.RemoveFailed));
            Assert.That(inventory.Amount, Is.EqualTo(2));

            inventory.RejectRemove = false;
            var committed = transaction.TryCommit(SurvivalSkillItemIds.ImprovisedLure, 2);
            Assert.That(committed.Success, Is.True);
            Assert.That(committed.Failure, Is.EqualTo(SkillItemCommitFailure.None));
            Assert.That(inventory.Amount, Is.Zero);
            Assert.That(inventory.RemoveCalls, Is.EqualTo(2));
        }

        [Test]
        public void DirectionalMobilityProvider_MissingSourceIsNeutral_AndInvalidValuesAreSanitized()
        {
            var neutral = DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.SprintTick, 1f, 0f);
            Assert.That(neutral.CostMultiplier, Is.EqualTo(1f));
            Assert.That(neutral.SpeedMultiplier, Is.EqualTo(1f));

            DirectionalMobilityModifierProvider.Source = new FakeMobilityRuntime(
                new DirectionalMobilityModifier(float.NaN, -2f));
            var sanitized = DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.DodgeCommit, 0f, 1f);
            Assert.That(sanitized.CostMultiplier, Is.EqualTo(1f));
            Assert.That(sanitized.SpeedMultiplier, Is.Zero);
        }

        [Test]
        public void OffensiveCommitEvent_CarriesOnlyStablePrimitiveIdentity()
        {
            var evt = new PlayerOffensiveActionCommittedEvent("combat.melee.whirl_cut", "active_skill");
            Assert.That(evt.ActionId, Is.EqualTo("combat.melee.whirl_cut"));
            Assert.That(evt.SourceKind, Is.EqualTo("active_skill"));
        }

        [Test]
        public void TemporaryRevealRegistry_UsesInclusiveRadiusAndExactEligibility()
        {
            var registry = new TemporaryRevealRegistry();
            var resource = Target("resource", TemporaryRevealKind.Resource, 7f, 0f);
            var exhausted = Target("exhausted", TemporaryRevealKind.Resource, 1f, 0f, exhausted: true);
            var hazard = Target("hazard", TemporaryRevealKind.Hazard, 3f, 0f, active: true);
            var inactiveHazard = Target("inactive", TemporaryRevealKind.Hazard, 2f, 0f, active: false);
            var interactable = Target("interactable", TemporaryRevealKind.Interactable, 0f, 4f);
            var secret = Target("secret", TemporaryRevealKind.Interactable, 0f, 2f, secret: true);
            var disabled = Target("disabled", TemporaryRevealKind.Resource, 0f, 1f, enabled: false);
            var outside = Target("outside", TemporaryRevealKind.Resource, 7.01f, 0f);

            foreach (var target in new[]
            {
                resource, exhausted, hazard, inactiveHazard, interactable, secret, disabled, outside
            })
            {
                Assert.That(registry.Register(target), Is.True);
            }

            var count = registry.RevealEligible(0f, 0f, 7f, "survival.instinct", 12f);

            Assert.That(count, Is.EqualTo(3));
            Assert.That(resource.RevealCount, Is.EqualTo(1));
            Assert.That(hazard.RevealCount, Is.EqualTo(1));
            Assert.That(interactable.RevealCount, Is.EqualTo(1));
            Assert.That(new[] { exhausted, inactiveHazard, secret, disabled, outside }
                .All(target => target.RevealCount == 0), Is.True);
        }

        [Test]
        public void TemporaryRevealRegistry_DuplicateAndStaleUnregisterCannotReplaceLiveTarget()
        {
            var registry = new TemporaryRevealRegistry();
            var first = Target("same", TemporaryRevealKind.Resource, 0f, 0f);
            var duplicate = Target("same", TemporaryRevealKind.Resource, 0f, 0f);

            Assert.That(registry.Register(first), Is.True);
            Assert.That(registry.Register(duplicate), Is.False);
            Assert.That(registry.Unregister(duplicate), Is.False);
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.Unregister(first), Is.True);
            Assert.That(registry.Count, Is.Zero);
        }

        private static FakeRevealTarget Target(
            string id,
            TemporaryRevealKind kind,
            float x,
            float y,
            bool enabled = true,
            bool secret = false,
            bool exhausted = false,
            bool active = true)
        {
            return new FakeRevealTarget(id, kind, x, y, enabled, secret, exhausted, active);
        }

        private sealed class FakeSkillInventory : ISkillItemInventory
        {
            private readonly string _itemId;

            public int Amount { get; private set; }
            public int RemoveCalls { get; private set; }
            public bool RejectRemove { get; set; }

            public FakeSkillInventory(string itemId, int amount)
            {
                _itemId = itemId;
                Amount = amount;
            }

            public int GetAmount(string itemId)
            {
                return itemId == _itemId ? Amount : 0;
            }

            public bool RemoveItem(string itemId, int amount)
            {
                RemoveCalls++;
                if (RejectRemove || itemId != _itemId || amount <= 0 || Amount < amount)
                {
                    return false;
                }

                Amount -= amount;
                return true;
            }
        }

        private sealed class FakeMobilityRuntime : IDirectionalMobilityModifierRuntime
        {
            private readonly DirectionalMobilityModifier _modifier;

            public FakeMobilityRuntime(DirectionalMobilityModifier modifier)
            {
                _modifier = modifier;
            }

            public DirectionalMobilityModifier Resolve(
                MobilityActionKind actionKind,
                float directionX,
                float directionY)
            {
                return _modifier;
            }
        }

        private sealed class FakeRevealTarget : ITemporaryRevealTarget
        {
            public string RevealTargetId { get; }
            public TemporaryRevealKind RevealKind { get; }
            public float WorldX { get; }
            public float WorldY { get; }
            public bool IsEnabled { get; }
            public bool IsSecret { get; }
            public bool IsExhausted { get; }
            public bool IsActive { get; }
            public int RevealCount { get; private set; }

            public FakeRevealTarget(
                string id,
                TemporaryRevealKind kind,
                float x,
                float y,
                bool enabled,
                bool secret,
                bool exhausted,
                bool active)
            {
                RevealTargetId = id;
                RevealKind = kind;
                WorldX = x;
                WorldY = y;
                IsEnabled = enabled;
                IsSecret = secret;
                IsExhausted = exhausted;
                IsActive = active;
            }

            public void ApplyTemporaryReveal(string sourceId, float expiresAt)
            {
                RevealCount++;
            }
        }
    }
}
