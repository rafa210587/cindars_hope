using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.MainProgression;
using CindarsHope.MainProgression.Runtime;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    /// <summary>
    /// fable_43 — EndgameSequenceController state machine, EndgameGate (100->101) and the Ithryndor
    /// branch. Validates canonical order, controlled recovery, re-entrability, and the centralized
    /// HUD restoration contract (CA-2, CA-3).
    /// </summary>
    [TestFixture]
    public class EndgameSequenceTests
    {
        private FinalChoiceService _choiceService;
        private MainProgressionService _progressionService;

        [SetUp]
        public void SetUp()
        {
            _choiceService = new FinalChoiceService();
            _progressionService = new MainProgressionService();
        }

        // ---- EndgameGate: 100 -> 101 ----

        private MainProgressionSection LifeReady()
        {
            var prog = new MainProgressionSection { Version = 1 };
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Water, 1, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Memory, 2, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Life, 3, false);
            return prog;
        }

        [Test]
        public void Gate_BlockedWhenElderNotDefeated()
        {
            var prog = LifeReady(); // Life integrated but gate still Locked
            Assert.IsFalse(EndgameGate.CanEnterLevel101(_choiceService, prog));
        }

        [Test]
        public void Gate_UnlocksOnElderDefeated_WithLifeFragment()
        {
            var prog = LifeReady();
            bool unlocked = EndgameGate.TryUnlockLevel101OnElderDefeated(_choiceService, prog);
            Assert.IsTrue(unlocked);
            Assert.AreEqual(Level101AccessStatus.Unlocked, prog.Level101AccessState);
            Assert.IsTrue(EndgameGate.CanEnterLevel101(_choiceService, prog));
        }

        [Test]
        public void Gate_DoesNotUnlockWithoutLifeFragment()
        {
            var prog = new MainProgressionSection { Version = 1 };
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Water, 1, false);
            bool unlocked = EndgameGate.TryUnlockLevel101OnElderDefeated(_choiceService, prog);
            Assert.IsFalse(unlocked);
            Assert.AreEqual(Level101AccessStatus.Locked, prog.Level101AccessState);
        }

        [Test]
        public void Gate_IsIdempotent()
        {
            var prog = LifeReady();
            EndgameGate.TryUnlockLevel101OnElderDefeated(_choiceService, prog);
            // Second call: already unlocked, never regresses.
            Assert.IsTrue(EndgameGate.TryUnlockLevel101OnElderDefeated(_choiceService, prog));
            Assert.AreEqual(Level101AccessStatus.Unlocked, prog.Level101AccessState);
        }

        // ---- Ithryndor branch ----

        [Test]
        public void IthryndorBranch_PerChoice()
        {
            Assert.AreEqual(IthryndorBranch.AllyNoFight, EndgameGate.BranchFor(FinalChoiceType.Protect));
            Assert.AreEqual(IthryndorBranch.CeremonialPartial, EndgameGate.BranchFor(FinalChoiceType.Seal));
            Assert.AreEqual(IthryndorBranch.FullFourPhase, EndgameGate.BranchFor(FinalChoiceType.Use));
        }

        [Test]
        public void IthryndorBranch_PerEndingId()
        {
            Assert.AreEqual(IthryndorBranch.AllyNoFight, EndgameGate.BranchForEnding("ending_protect"));
            Assert.AreEqual(IthryndorBranch.CeremonialPartial, EndgameGate.BranchForEnding("ending_seal"));
            Assert.AreEqual(IthryndorBranch.FullFourPhase, EndgameGate.BranchForEnding("ending_use"));
            Assert.AreEqual(IthryndorBranch.None, EndgameGate.BranchForEnding("nonsense"));
        }

        // ---- Sequence order + recovery ----

        [Test]
        public void Sequence_RunsFourEncountersInCanonicalOrder()
        {
            var c = new EndgameSequenceController();
            c.Begin();
            Assert.AreEqual(EndgameEncounterCatalog.VelKaraumBossId, c.CurrentEncounter.BossId);

            Assert.IsTrue(c.DefeatCurrentEncounter()); // -> recovery
            Assert.AreEqual(EndgameSequenceController.Phase.Recovery, c.CurrentPhase);
            Assert.IsTrue(c.AdvanceEncounter());
            Assert.AreEqual(EndgameEncounterCatalog.CindrathelBossId, c.CurrentEncounter.BossId);

            c.DefeatCurrentEncounter(); c.AdvanceEncounter();
            Assert.AreEqual(EndgameEncounterCatalog.ArchivistBossId, c.CurrentEncounter.BossId);

            c.DefeatCurrentEncounter(); c.AdvanceEncounter();
            Assert.AreEqual(EndgameEncounterCatalog.IthryndorBossId, c.CurrentEncounter.BossId);

            c.DefeatCurrentEncounter(); // last -> complete
            Assert.IsTrue(c.IsComplete);
            Assert.IsNull(c.CurrentEncounter);
        }

        [Test]
        public void Sequence_RecoveryWindowBetweenEncounters()
        {
            var c = new EndgameSequenceController();
            c.Begin();
            c.DefeatCurrentEncounter();
            Assert.AreEqual(EndgameSequenceController.Phase.Recovery, c.CurrentPhase);
            // Cannot advance encounter while still in encounter (must be in recovery first).
            Assert.IsFalse(new EndgameSequenceController().AdvanceEncounter());
        }

        [Test]
        public void Sequence_ReentrableAfterDefeat()
        {
            var c = new EndgameSequenceController();
            c.Begin();
            c.DefeatCurrentEncounter(); c.AdvanceEncounter(); // now at Cindrathel
            int idx = c.CurrentIndex;
            Assert.IsTrue(c.RetryCurrent());
            Assert.AreEqual(idx, c.CurrentIndex); // same encounter, restarted
            Assert.AreEqual(EndgameEncounterCatalog.CindrathelBossId, c.CurrentEncounter.BossId);
        }

        // ---- HUD suppression: restoration always guaranteed (CA-3) ----

        [Test]
        public void HudSuppression_OnlyDuringArchivist()
        {
            var c = new EndgameSequenceController();
            c.Begin(); // Vel-Karaum: no HUD mechanic
            Assert.IsFalse(c.PublishHudSuppressionPhase(2));
        }

        [Test]
        public void HudSuppression_PublishesAndRestoresOnDefeat()
        {
            var c = new EndgameSequenceController();
            c.Begin();
            c.DefeatCurrentEncounter(); c.AdvanceEncounter(); // Cindrathel
            c.DefeatCurrentEncounter(); c.AdvanceEncounter(); // Archivist

            var events = new List<HudSuppressionChangedEvent>();
            void Handler(HudSuppressionChangedEvent e) => events.Add(e);
            GameEventBus.Subscribe<HudSuppressionChangedEvent>(Handler);
            try
            {
                Assert.IsTrue(c.PublishHudSuppressionPhase(2));
                Assert.AreEqual(1, events.Count);
                Assert.AreEqual(2, events[0].Phase);
                Assert.Greater(events[0].HiddenWidgets.Count, 0);

                // Defeating the Archivist restores the HUD (phase 0 / nothing hidden).
                Assert.IsTrue(c.DefeatCurrentEncounter());
                Assert.AreEqual(2, events.Count);
                Assert.AreEqual(0, events[1].Phase);
                Assert.AreEqual(0, events[1].HiddenWidgets.Count);
            }
            finally
            {
                GameEventBus.Unsubscribe<HudSuppressionChangedEvent>(Handler);
            }
        }

        [Test]
        public void HudSuppression_RestoredOnAbort()
        {
            var c = new EndgameSequenceController();
            c.Begin();
            c.DefeatCurrentEncounter(); c.AdvanceEncounter();
            c.DefeatCurrentEncounter(); c.AdvanceEncounter(); // Archivist
            c.PublishHudSuppressionPhase(3);

            var events = new List<HudSuppressionChangedEvent>();
            void Handler(HudSuppressionChangedEvent e) => events.Add(e);
            GameEventBus.Subscribe<HudSuppressionChangedEvent>(Handler);
            try
            {
                c.Abort();
                Assert.AreEqual(1, events.Count);
                Assert.AreEqual(0, events[0].Phase); // restore-all
            }
            finally
            {
                GameEventBus.Unsubscribe<HudSuppressionChangedEvent>(Handler);
            }
        }

        [Test]
        public void HudSuppression_WidgetSetGrowsWithPhase()
        {
            Assert.AreEqual(0, EndgameSequenceController.HiddenWidgetsForPhase(0).Count);
            Assert.AreEqual(1, EndgameSequenceController.HiddenWidgetsForPhase(1).Count);
            Assert.AreEqual(2, EndgameSequenceController.HiddenWidgetsForPhase(2).Count);
            Assert.AreEqual(3, EndgameSequenceController.HiddenWidgetsForPhase(3).Count);
        }
    }
}
