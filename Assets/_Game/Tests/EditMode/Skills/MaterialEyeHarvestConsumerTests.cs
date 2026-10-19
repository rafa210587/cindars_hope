using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class MaterialEyeHarvestConsumerTests
    {
        [Test]
        public void Prepare_UsesFirstExplicitlyCommonCandidate()
        {
            var state = new CraftingPassiveRngState("save-a");
            var consumer = new MaterialEyeHarvestConsumer(
                new ExplicitCommonHarvestItemPolicy(new[] { "item_crop_wheat" }));

            Assert.That(consumer.TryPrepare(
                state, 1f, "plot-01",
                new[] { "item_rare_essence", "item_crop_wheat" },
                out var prepared), Is.True);

            Assert.That(prepared.BonusItemId, Is.EqualTo("item_crop_wheat"));
            Assert.That(prepared.Roll.Succeeded, Is.True);
        }

        [Test]
        public void Prepare_UnknownOrRareOnlyCandidate_IsDeniedWithoutAttempt()
        {
            var state = new CraftingPassiveRngState("save-a");
            var consumer = new MaterialEyeHarvestConsumer(
                new ExplicitCommonHarvestItemPolicy(new[] { "item_crop_wheat" }));

            Assert.That(consumer.TryPrepare(
                state, 1f, "plot-01", new[] { "item_rare_essence" }, out _), Is.False);
            Assert.That(state.GetNextAttempt(MaterialEyeHarvestConsumer.OperationKind, "plot-01"), Is.Zero);
        }

        [Test]
        public void Prepare_MissingPolicy_DeniesByDefault()
        {
            var state = new CraftingPassiveRngState("save-a");
            var consumer = new MaterialEyeHarvestConsumer(null);

            Assert.That(consumer.TryPrepare(
                state, 1f, "plot-01", new[] { "item_crop_wheat" }, out _), Is.False);
        }

        [Test]
        public void Commit_FailedHarvestDoesNotConsume_ButCommittedHarvestConsumesEvenIfBonusIsFull()
        {
            var state = new CraftingPassiveRngState("save-a");
            var consumer = new MaterialEyeHarvestConsumer(
                new ExplicitCommonHarvestItemPolicy(new[] { "item_crop_wheat" }));
            consumer.TryPrepare(state, 1f, "plot-01", new[] { "item_crop_wheat" }, out var prepared);

            Assert.That(consumer.Commit(state, prepared, harvestCommitted: false, successfulBonusAdded: false), Is.False);
            Assert.That(consumer.Commit(state, prepared, harvestCommitted: true, successfulBonusAdded: false), Is.True);
            Assert.That(state.GetNextAttempt(MaterialEyeHarvestConsumer.OperationKind, "plot-01"), Is.EqualTo(1));
        }

        [Test]
        public void Commit_ConsumesFailedRollWithHarvest_AndWinningRollAfterBonusAdd()
        {
            var state = new CraftingPassiveRngState("save-a");
            var consumer = new MaterialEyeHarvestConsumer(
                new ExplicitCommonHarvestItemPolicy(new[] { "item_crop_wheat" }));

            consumer.TryPrepare(state, float.Epsilon, "plot-failed-roll", new[] { "item_crop_wheat" }, out var failedRoll);
            Assert.That(failedRoll.Roll.Succeeded, Is.False);
            Assert.That(consumer.Commit(state, failedRoll, true, false), Is.True);

            consumer.TryPrepare(state, 1f, "plot-winning-roll", new[] { "item_crop_wheat" }, out var winningRoll);
            Assert.That(consumer.Commit(state, winningRoll, true, true), Is.True);

            Assert.That(state.GetNextAttempt(MaterialEyeHarvestConsumer.OperationKind, "plot-failed-roll"), Is.EqualTo(1));
            Assert.That(state.GetNextAttempt(MaterialEyeHarvestConsumer.OperationKind, "plot-winning-roll"), Is.EqualTo(1));
        }
    }
}
