using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Fonte;
using CindarsHope.MainProgression;
using CindarsHope.MainProgression.Runtime;
using CindarsHope.UI;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    /// <summary>
    /// fable_43 — FinalChoiceRuntimeAdapter (idempotency + event), HudSuppressionConsumer (restore),
    /// and a pure save round-trip of the additive endgame fields (CA-4).
    /// </summary>
    [TestFixture]
    public class FinalChoiceRuntimeTests
    {
        private MainProgressionService _progressionService;
        private FinalChoiceRuntimeAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _progressionService = new MainProgressionService();
            _adapter = new FinalChoiceRuntimeAdapter();
        }

        private MainProgressionSection ReadyProgression()
        {
            var prog = new MainProgressionSection { Version = 1 };
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Water, 1, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Memory, 2, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Life, 3, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Hope, 4, false);
            prog.Level100GateState = Level100GateStatus.Entered;
            prog.Level101AccessState = Level101AccessStatus.Unlocked;
            prog.FinalChoiceState = FinalChoiceStatus.Available;
            prog.CurrentAct = MainAct.Act4_Level100101AndHope;
            return prog;
        }

        // ---- Adapter apply / idempotency / event ----

        [Test]
        public void Apply_Protect_PublishesResolvedEventOnce()
        {
            var prog = ReadyProgression();
            var fonte = new FonteAnyaSection { Version = 1 };

            var events = new List<FinalChoiceResolvedEvent>();
            void Handler(FinalChoiceResolvedEvent e) => events.Add(e);
            GameEventBus.Subscribe<FinalChoiceResolvedEvent>(Handler);
            try
            {
                var r1 = _adapter.Apply(prog, fonte, FinalChoiceType.Protect, 100);
                Assert.IsTrue(r1.Success);
                Assert.IsTrue(r1.ChoiceApplied);
                Assert.AreEqual("ending_protect", r1.EndingEffectProfileId);
                Assert.AreEqual(MainAct.PostGame, prog.CurrentAct);
                Assert.AreEqual(FonteState.FinalizedProtected, fonte.FonteState);
                Assert.AreEqual(1, events.Count);

                // Repeat: AlreadyApplied, NO second event.
                var r2 = _adapter.Apply(prog, fonte, FinalChoiceType.Seal, 101);
                Assert.IsTrue(r2.AlreadyApplied);
                Assert.AreEqual(1, events.Count);
            }
            finally
            {
                GameEventBus.Unsubscribe<FinalChoiceResolvedEvent>(Handler);
            }
        }

        [Test]
        public void Preview_DoesNotMutateOrPublish()
        {
            var prog = ReadyProgression();
            var fonte = new FonteAnyaSection { Version = 1 };

            var events = new List<FinalChoiceResolvedEvent>();
            void Handler(FinalChoiceResolvedEvent e) => events.Add(e);
            GameEventBus.Subscribe<FinalChoiceResolvedEvent>(Handler);
            try
            {
                var r = _adapter.Preview(prog, fonte, FinalChoiceType.Use, 100);
                Assert.IsTrue(r.Success);
                Assert.IsFalse(r.ChoiceApplied);
                Assert.AreEqual("ending_use", r.EndingEffectProfileId);
                Assert.AreEqual(MainAct.Act4_Level100101AndHope, prog.CurrentAct); // unchanged
                Assert.AreEqual(0, events.Count);
            }
            finally
            {
                GameEventBus.Unsubscribe<FinalChoiceResolvedEvent>(Handler);
            }
        }

        [Test]
        public void Apply_BlockedWithoutLevel101Access()
        {
            var prog = ReadyProgression();
            prog.Level101AccessState = Level101AccessStatus.Locked;
            var r = _adapter.Apply(prog, new FonteAnyaSection { Version = 1 }, FinalChoiceType.Protect, 100);
            Assert.IsFalse(r.Success);
            Assert.AreEqual("FINAL_CHOICE_REQUIRES_LEVEL101_ACCESS", r.FailureReason);
        }

        [Test]
        public void Apply_EachEndingMapsToIthryndorBranch()
        {
            foreach (var (choice, branch) in new[]
            {
                (FinalChoiceType.Protect, IthryndorBranch.AllyNoFight),
                (FinalChoiceType.Seal, IthryndorBranch.CeremonialPartial),
                (FinalChoiceType.Use, IthryndorBranch.FullFourPhase)
            })
            {
                var prog = ReadyProgression();
                var r = _adapter.Apply(prog, new FonteAnyaSection { Version = 1 }, choice, 100);
                Assert.AreEqual(branch, _adapter.BranchForEnding(r.EndingEffectProfileId));
            }
        }

        // ---- HUD consumer: restoration guaranteed ----

        [Test]
        public void HudConsumer_HidesThenRestores()
        {
            var consumer = new HudSuppressionConsumer();
            consumer.Apply(new HudSuppressionChangedEvent(2, new List<string>
            {
                EndgameEncounterCatalog.WidgetMinimap, EndgameEncounterCatalog.WidgetBossHpBar
            }));
            Assert.IsFalse(consumer.AllVisible);
            Assert.IsTrue(consumer.IsHidden(EndgameEncounterCatalog.WidgetMinimap));

            consumer.Apply(HudSuppressionChangedEvent.RestoreAll());
            Assert.IsTrue(consumer.AllVisible);
            Assert.IsFalse(consumer.IsHidden(EndgameEncounterCatalog.WidgetMinimap));
        }

        [Test]
        public void HudConsumer_UnsubscribeRestoresHud()
        {
            var consumer = new HudSuppressionConsumer();
            consumer.Subscribe();
            GameEventBus.Publish(new HudSuppressionChangedEvent(1, new List<string> { EndgameEncounterCatalog.WidgetMinimap }));
            Assert.IsFalse(consumer.AllVisible);

            consumer.Unsubscribe(); // defensive restore on teardown
            Assert.IsTrue(consumer.AllVisible);
        }

        // ---- Save round-trip of the additive endgame fields (pure, mirrors SaveManager copy) ----

        [Test]
        public void EndgameSaveFields_RoundTrip()
        {
            var prog = ReadyProgression();
            _adapter.Apply(prog, new FonteAnyaSection { Version = 1 }, FinalChoiceType.Seal, 100);

            // Capture (same field copy SaveManager.CaptureMainProgressionSaveData performs).
            int currentAct = (int)prog.CurrentAct;
            int gate100 = (int)prog.Level100GateState;
            int access101 = (int)prog.Level101AccessState;
            int finalChoice = (int)prog.FinalChoiceState;
            string ending = prog.PostGameWorldState;

            Assert.AreEqual((int)MainAct.PostGame, currentAct);
            Assert.AreEqual((int)FinalChoiceStatus.Resolved, finalChoice);
            Assert.AreEqual("ending_seal", ending);

            // Restore into a fresh section (same copy RestoreEndgameState performs).
            var restored = new MainProgressionSection();
            restored.CurrentAct = (MainAct)currentAct;
            restored.Level100GateState = (Level100GateStatus)gate100;
            restored.Level101AccessState = (Level101AccessStatus)access101;
            restored.FinalChoiceState = (FinalChoiceStatus)finalChoice;
            restored.PostGameWorldState = string.IsNullOrEmpty(ending) ? null : ending;

            Assert.AreEqual(MainAct.PostGame, restored.CurrentAct);
            Assert.AreEqual(FinalChoiceStatus.Resolved, restored.FinalChoiceState);
            Assert.AreEqual("ending_seal", restored.PostGameWorldState);
        }

        [Test]
        public void LegacySave_DefaultsToNotStarted()
        {
            // A null/legacy MainProgressionSaveData => defaults; restoring 0/0/0/0/null = not started.
            var restored = new MainProgressionSection();
            restored.CurrentAct = (MainAct)0;
            restored.Level100GateState = (Level100GateStatus)0;
            restored.Level101AccessState = (Level101AccessStatus)0;
            restored.FinalChoiceState = (FinalChoiceStatus)0;
            restored.PostGameWorldState = null;

            Assert.AreEqual(MainAct.None, restored.CurrentAct);
            Assert.AreEqual(Level100GateStatus.Locked, restored.Level100GateState);
            Assert.AreEqual(Level101AccessStatus.Locked, restored.Level101AccessState);
            Assert.AreEqual(FinalChoiceStatus.Unavailable, restored.FinalChoiceState);
            Assert.IsNull(restored.PostGameWorldState);
        }
    }
}
