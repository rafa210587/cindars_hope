using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class CraftingPassiveRngSaveTests
    {
        [Test]
        public void PrepareRoll_WithoutCommit_RepeatsSameAttemptAndOutcome()
        {
            var state = new CraftingPassiveRngState("save-a");

            Assert.That(state.TryPrepareRoll("harvest", "plot-03", 0.15f, out var first), Is.True);
            Assert.That(state.TryPrepareRoll("harvest", "plot-03", 0.15f, out var second), Is.True);

            Assert.That(first.Attempt, Is.Zero);
            Assert.That(second.Attempt, Is.Zero);
            Assert.That(second.Seed, Is.EqualTo(first.Seed));
            Assert.That(second.Value, Is.EqualTo(first.Value));
            Assert.That(second.Succeeded, Is.EqualTo(first.Succeeded));
        }

        [Test]
        public void Commit_AdvancesExactlyOnce_AndRejectsStaleTicket()
        {
            var state = new CraftingPassiveRngState("save-a");
            state.TryPrepareRoll("salvage", "equipment-07", 0.10f, out var ticket);

            Assert.That(state.Commit(ticket), Is.True);
            Assert.That(state.Commit(ticket), Is.False);
            Assert.That(state.GetNextAttempt("salvage", "equipment-07"), Is.EqualTo(1));

            state.TryPrepareRoll("salvage", "equipment-07", 0.10f, out var next);
            Assert.That(next.Attempt, Is.EqualTo(1));
            Assert.That(next.Seed, Is.Not.EqualTo(ticket.Seed));
        }

        [Test]
        public void CaptureRestore_PreservesScopeAttemptAndNextOutcome()
        {
            var source = new CraftingPassiveRngState("save-scope-42");
            source.TryPrepareRoll("harvest", "plot-01", 0.15f, out var consumed);
            Assert.That(source.Commit(consumed), Is.True);
            source.TryPrepareRoll("harvest", "plot-01", 0.15f, out var expectedNext);

            var restored = new CraftingPassiveRngState("different-default");
            restored.RestoreFromSaveData(source.CaptureSaveData());
            Assert.That(restored.TryPrepareRoll("harvest", "plot-01", 0.15f, out var actualNext), Is.True);

            Assert.That(restored.SaveScopeId, Is.EqualTo("save-scope-42"));
            Assert.That(actualNext.Attempt, Is.EqualTo(expectedNext.Attempt));
            Assert.That(actualNext.Seed, Is.EqualTo(expectedNext.Seed));
            Assert.That(actualNext.Value, Is.EqualTo(expectedNext.Value));
            Assert.That(actualNext.Succeeded, Is.EqualTo(expectedNext.Succeeded));
        }

        [Test]
        public void OperationAndInstance_AreIndependentLedgerDimensions()
        {
            var state = new CraftingPassiveRngState("save-a");
            state.TryPrepareRoll("harvest", "shared", 1f, out var harvest);
            state.TryPrepareRoll("salvage", "shared", 1f, out var salvage);
            state.TryPrepareRoll("harvest", "other", 1f, out var other);

            Assert.That(state.Commit(harvest), Is.True);
            Assert.That(state.GetNextAttempt("harvest", "shared"), Is.EqualTo(1));
            Assert.That(state.GetNextAttempt("salvage", "shared"), Is.Zero);
            Assert.That(state.GetNextAttempt("harvest", "other"), Is.Zero);
            Assert.That(salvage.Seed, Is.Not.EqualTo(harvest.Seed));
            Assert.That(other.Seed, Is.Not.EqualTo(harvest.Seed));
        }

        [Test]
        public void Provider_RoundTripAndLegacyFallback_PreserveState()
        {
            var source = new CraftingPassiveRngState("save-a");
            source.TryPrepareRoll("harvest", "plot-01", 1f, out var ticket);
            source.Commit(ticket);
            var sourceProvider = new CraftingPassiveRngSectionProvider(source);

            var target = new CraftingPassiveRngState("fallback");
            var targetProvider = new CraftingPassiveRngSectionProvider(target);
            targetProvider.Restore(sourceProvider.Capture(null));

            Assert.That(target.SaveScopeId, Is.EqualTo("save-a"));
            Assert.That(target.GetNextAttempt("harvest", "plot-01"), Is.EqualTo(1));
            Assert.That(sourceProvider.ProviderId, Is.EqualTo("crafting_passive_rng"));

            var fallback = new CraftingPassiveRngSaveData { SaveScopeId = "legacy-existing" };
            var nullRuntimeProvider = new CraftingPassiveRngSectionProvider(null);
            Assert.That(
                nullRuntimeProvider.Capture(new GameSaveData { CraftingPassiveRng = fallback }),
                Is.SameAs(fallback));
            Assert.DoesNotThrow(() => targetProvider.Restore(null));
            Assert.That(target.SaveScopeId, Is.EqualTo("fallback"));
            Assert.That(target.GetNextAttempt("harvest", "plot-01"), Is.Zero);
        }

        [Test]
        public void Chance_IsClampedWithoutChangingDeterministicRoll()
        {
            var state = new CraftingPassiveRngState("save-a");
            state.TryPrepareRoll("harvest", "plot-01", -1f, out var impossible);
            state.TryPrepareRoll("harvest", "plot-01", 2f, out var certain);

            Assert.That(impossible.Value, Is.EqualTo(certain.Value));
            Assert.That(impossible.Succeeded, Is.False);
            Assert.That(certain.Succeeded, Is.True);
        }
    }
}
