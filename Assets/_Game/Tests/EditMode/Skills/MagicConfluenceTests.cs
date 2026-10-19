using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class MagicConfluenceTests
    {
        [TearDown]
        public void TearDown() => SpellCastPreparationProvider.Source = null;

        [Test]
        public void ThresholdCast_ArmsWithoutConsuming_AndWrongDisciplineDoesNotConsume()
        {
            var state = new MagicConfluenceState();
            var runtime = Runtime(state, MagicConfluenceState.AnyaVariant, 2);
            SpellCastPreparationProvider.Source = runtime;

            Commit("a", SpellDiscipline.Offensive, 20, 100);
            Commit("b", SpellDiscipline.Spiritual, 15, 100);

            Assert.That(state.IsArmed, Is.True);
            Assert.That(state.ArmedVariant, Is.EqualTo(MagicConfluenceState.AnyaVariant));
            Assert.That(state.ArmedRemainingSeconds, Is.EqualTo(10f));
            Assert.That(state.LockoutRemainingSeconds, Is.Zero);

            var wrong = Prepare("wrong", SpellDiscipline.Offensive, 20);
            Assert.That(wrong.ReservesConfluence, Is.False);
            Commit(wrong, 100);
            Assert.That(state.IsArmed, Is.True);

            var right = Prepare("right", SpellDiscipline.Spiritual, 20);
            Assert.That(right.ManaCost, Is.EqualTo(12));
            Assert.That(right.SpiritualOutputMultiplier, Is.EqualTo(1.28f).Within(.0001f));
            Assert.That(right.StaminaEchoAmount, Is.EqualTo(3));
            Commit(right, 100);
            Assert.That(state.IsArmed, Is.False);
            Assert.That(state.LockoutRemainingSeconds, Is.EqualTo(25f));
        }

        [Test]
        public void Senya_ReservesNormalPlusCeilingSurchargeAtomically_AndCancelKeepsArm()
        {
            var state = Armed(MagicConfluenceState.SenyaVariant, 3);
            var runtime = Runtime(state, MagicConfluenceState.SenyaVariant, 3);
            SpellCastPreparationProvider.Source = runtime;
            var preparation = Prepare("offensive", SpellDiscipline.Offensive, 21);

            Assert.That(preparation.ManaCost, Is.EqualTo(24));
            Assert.That(preparation.DirectDamageMultiplier, Is.EqualTo(1.35f).Within(.0001f));
            Assert.That(preparation.CriticalChanceBonus, Is.EqualTo(.15f).Within(.0001f));
            var failed = new SpellCastTransaction(preparation);
            Assert.That(failed.TryReserve(cost => cost <= 23), Is.False);
            SpellCastPreparationProvider.Cancel(preparation);
            Assert.That(state.IsArmed, Is.True);

            var retry = Prepare("retry", SpellDiscipline.Offensive, 21);
            var cancelled = new SpellCastTransaction(retry);
            Assert.That(cancelled.TryReserve(_ => true), Is.True);
            Assert.That(cancelled.Cancel(), Is.True);
            SpellCastPreparationProvider.Cancel(retry);
            Assert.That(state.IsArmed, Is.True);
            Assert.That(state.LockoutRemainingSeconds, Is.Zero);
        }

        [Test]
        public void TwoCastsPreparedTogether_CannotBothReserveTheSameSeed()
        {
            var state = Armed(MagicConfluenceState.SenyaVariant, 1);
            var first = Prepare("first", SpellDiscipline.Offensive, 20);
            var second = Prepare("second", SpellDiscipline.Offensive, 20);

            Assert.That(first.ReservesConfluence, Is.True);
            Assert.That(first.ManaCost, Is.EqualTo(21));
            Assert.That(second.ReservesConfluence, Is.False);
            Assert.That(second.ManaCost, Is.EqualTo(20));
            SpellCastPreparationProvider.Cancel(first);
        }

        [Test]
        public void SlidingWindow_ExpiresOldSpend_AndSnapshotsMaxManaOnFirstCommit()
        {
            var state = new MagicConfluenceState();
            var runtime = Runtime(state, MagicConfluenceState.SenyaVariant, 1);
            SpellCastPreparationProvider.Source = runtime;
            Commit("first", SpellDiscipline.Offensive, 20, 100);
            Assert.That(state.MaxManaSnapshot, Is.EqualTo(100));
            state.Advance(6.01f);
            Assert.That(state.LedgerManaSpent, Is.Zero);
            Commit("second", SpellDiscipline.Offensive, 20, 200);
            Assert.That(state.MaxManaSnapshot, Is.EqualTo(200));
            Assert.That(state.IsArmed, Is.False);
        }

        [Test]
        public void SaveRestore_IsSilentAndRespecPreservesStartedLockout()
        {
            var owner = new CombatCapstoneState();
            var source = owner.MagicConfluence;
            var runtime = Runtime(source, MagicConfluenceState.SenyaVariant, 1);
            SpellCastPreparationProvider.Source = runtime;
            Commit("arm", SpellDiscipline.Offensive, 35, 100);
            Commit(Prepare("consume", SpellDiscipline.Offensive, 10), 100);
            source.Advance(4f);

            var save = owner.CaptureSaveData();
            var restoredOwner = new CombatCapstoneState();
            restoredOwner.RestoreFromSaveData(save);
            restoredOwner.MagicConfluence.ClearForRespec();

            Assert.That(restoredOwner.MagicConfluence.LockoutRemainingSeconds,
                Is.EqualTo(21f).Within(.001f));
            Assert.That(restoredOwner.MagicConfluence.IsArmed, Is.False);
            Assert.That(restoredOwner.MagicConfluence.LedgerManaSpent, Is.Zero);
        }

        [Test]
        public void AnyaEcho_IsFlooredFromDiscountedCostAndDeliveredAcrossDuration()
        {
            var state = Armed(MagicConfluenceState.AnyaVariant, 3);
            int restored = 0;
            var runtime = new MagicConfluenceRuntime(state, () => 3,
                () => MagicConfluenceState.AnyaVariant, amount => restored += amount);
            SpellCastPreparationProvider.Source = runtime;
            var preparation = Prepare("heal", SpellDiscipline.Spiritual, 25);
            Assert.That(preparation.ManaCost, Is.EqualTo(13));
            Assert.That(preparation.StaminaEchoAmount, Is.EqualTo(4));
            Commit(preparation, 100);
            runtime.Tick(2f);
            Assert.That(restored, Is.EqualTo(2));
            runtime.Tick(2f);
            Assert.That(restored, Is.EqualTo(4));
        }

        private static MagicConfluenceRuntime Runtime(MagicConfluenceState state,
            string variant, int rank)
            => new MagicConfluenceRuntime(state, () => rank, () => variant, _ => { });

        private static MagicConfluenceState Armed(string variant, int rank)
        {
            var state = new MagicConfluenceState();
            var runtime = Runtime(state, variant, rank);
            SpellCastPreparationProvider.Source = runtime;
            Commit("arm", SpellDiscipline.Offensive, 35, 100);
            return state;
        }

        private static SpellCastPreparation Prepare(string token,
            SpellDiscipline discipline, int cost)
            => SpellCastPreparationProvider.Prepare(new SpellCastPreparationRequest(
                token, token, discipline, cost));

        private static void Commit(string token, SpellDiscipline discipline,
            int cost, int maxMana)
            => Commit(Prepare(token, discipline, cost), maxMana);

        private static void Commit(SpellCastPreparation preparation, int maxMana)
        {
            var transaction = new SpellCastTransaction(preparation);
            Assert.That(transaction.TryReserve(_ => true), Is.True);
            Assert.That(transaction.Commit(), Is.True);
            SpellCastPreparationProvider.Commit(preparation, maxMana);
        }
    }
}
