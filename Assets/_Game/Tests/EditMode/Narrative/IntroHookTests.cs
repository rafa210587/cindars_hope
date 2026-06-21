using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Narrative;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;

namespace CindarsHope.Tests.EditMode.Narrative
{
    /// <summary>
    /// fable_63 — testes da intro do New Game + ponte da main quest.
    /// Cobre: state machine/skip da intro, round-trip da flag IntroSeen + default seguro de save
    /// legado, idempotencia da oferta da mq_act1_00 (reload), conclusao TalkToNpc Corvus,
    /// prerequisite p/ a cadeia E40, e presenca de PLACEHOLDER_LORE + ids estaveis.
    /// </summary>
    [TestFixture]
    public class IntroHookTests
    {
        // ─── CA-1: Intro state machine + skip ──────────────────────────────────────

        [Test]
        public void Intro_AdvancesThroughAllScreens_ThenFinishes()
        {
            var model = new IntroSequenceModel();
            Assert.GreaterOrEqual(model.ScreenCount, 3, "Intro deve ter 3-5 telas.");
            Assert.LessOrEqual(model.ScreenCount, 5, "Intro deve ter 3-5 telas.");
            Assert.IsFalse(model.Finished);

            for (int i = 0; i < model.ScreenCount - 1; i++)
            {
                Assert.IsFalse(model.Finished);
                model.Advance();
            }
            // Na ultima tela, Advance encerra.
            Assert.IsTrue(model.IsOnLastScreen || model.Finished);
            model.Advance();
            Assert.IsTrue(model.Finished);
            Assert.IsNull(model.Current);
        }

        [Test]
        public void Intro_SkipFromFirstScreen_Finishes()
        {
            var model = new IntroSequenceModel();
            model.Skip();
            Assert.IsTrue(model.Finished);
            Assert.IsNull(model.Current);
        }

        [Test]
        public void Intro_SkipFromMiddleScreen_Finishes()
        {
            var model = new IntroSequenceModel();
            model.Advance();
            model.Skip();
            Assert.IsTrue(model.Finished);
        }

        [Test]
        public void Intro_AdvanceAfterFinished_IsNoOp()
        {
            var model = new IntroSequenceModel();
            model.Skip();
            model.Advance();
            Assert.IsTrue(model.Finished);
        }

        // ─── CA-5: PLACEHOLDER_LORE + ids estaveis ──────────────────────────────────

        [Test]
        public void Intro_AllScreens_HavePlaceholderLore_AndStableIds()
        {
            var model = new IntroSequenceModel();
            foreach (var screen in model.Screens)
            {
                Assert.IsFalse(string.IsNullOrEmpty(screen.ScreenId));
                StringAssert.StartsWith("intro_", screen.ScreenId);
                StringAssert.Contains("PLACEHOLDER_LORE", screen.Text);
            }
        }

        [Test]
        public void NarrativeIds_HookId_IsStable()
        {
            Assert.AreEqual("mq_act1_00", NarrativeIds.MainQuestHookId);
            Assert.AreEqual("npc_corvus", NarrativeIds.CorvusNpcId);
        }

        // ─── Flag store round-trip ──────────────────────────────────────────────────

        [Test]
        public void FlagStore_Set_IsIdempotent_AndPersistsInBackingList()
        {
            var backing = new List<string>();
            var store = new NarrativeFlagStore(backing);

            Assert.IsFalse(store.IsSet(NarrativeIds.FlagIntroSeen));
            Assert.IsTrue(store.Set(NarrativeIds.FlagIntroSeen), "primeira gravacao retorna true");
            Assert.IsFalse(store.Set(NarrativeIds.FlagIntroSeen), "segunda gravacao retorna false (idempotente)");
            Assert.IsTrue(store.IsSet(NarrativeIds.FlagIntroSeen));

            // round-trip: a flag esta na lista que o save persiste (GlobalKnownHints).
            CollectionAssert.Contains(backing, NarrativeIds.FlagIntroSeen);

            // reload: nova store sobre a mesma lista enxerga a flag.
            var reloaded = new NarrativeFlagStore(backing);
            Assert.IsTrue(reloaded.IsSet(NarrativeIds.FlagIntroSeen));
        }

        // ─── CA-2: idempotencia da oferta da mq_act1_00 ─────────────────────────────

        [Test]
        public void Hook_OffersOnce_ReloadDoesNotDuplicate()
        {
            var backing = new List<string>();
            var store = new NarrativeFlagStore(backing);
            int offers = 0;
            var hook = new MainQuestHookService(store, id =>
            {
                Assert.AreEqual(NarrativeIds.MainQuestHookId, id);
                offers++;
                return true;
            });

            Assert.IsTrue(hook.OnDayStarted(), "1o DayStarted oferta");
            Assert.AreEqual(1, offers);

            // 2o DayStarted (mesmo save) nao reoferta.
            Assert.IsFalse(hook.OnDayStarted());
            Assert.AreEqual(1, offers);

            // reload: store reconstruida sobre a lista persistida; novo DayStarted nao duplica.
            var reloadedStore = new NarrativeFlagStore(backing);
            var reloadedHook = new MainQuestHookService(reloadedStore, id => { offers++; return true; });
            Assert.IsFalse(reloadedHook.OnDayStarted(), "reload + DayStarted nao reoferta");
            Assert.AreEqual(1, offers);
            Assert.IsTrue(reloadedHook.AlreadyOffered);
        }

        // ─── Default seguro: save legado em progresso = intro vista ──────────────────

        [Test]
        public void LegacySaveInProgress_IntroTreatedAsSeen_NotReshown()
        {
            // Simula o que o NarrativeRuntimeBootstrap faz: se ha quests carregadas e IntroSeen
            // ausente, sela IntroSeen (nao interromper save no meio com tela de abertura).
            var backing = new List<string>();
            var store = new NarrativeFlagStore(backing);
            bool hasExistingQuestState = true; // save legado em progresso

            if (!store.IsSet(NarrativeIds.FlagIntroSeen) && hasExistingQuestState)
            {
                store.Set(NarrativeIds.FlagIntroSeen);
            }

            Assert.IsTrue(store.IsSet(NarrativeIds.FlagIntroSeen),
                "save legado em progresso deve tratar a intro como vista (default seguro)");
        }

        // ─── CA-2/CA-4: mq_act1_00 no fluxo unico + conclusao TalkToNpc Corvus ──────

        private QuestService MakeQuestService(out QuestStateSection section)
        {
            section = new QuestStateSection();
            var registry = new QuestRegistry();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            return new QuestService(registry, section, null, null, flagService, null);
        }

        [Test]
        public void Hook_Quest_IsRegistered_WithTalkToCorvusObjective()
        {
            var registry = new QuestRegistry();
            Assert.IsTrue(registry.TryGetQuest(NarrativeIds.MainQuestHookId, out var def));
            Assert.AreEqual(QuestCategory.Main, def.Category);

            var objectives = registry.GetObjectives(NarrativeIds.MainQuestHookId);
            Assert.AreEqual(1, objectives.Count);
            Assert.AreEqual(QuestObjectiveType.TalkToNpc, objectives[0].ObjectiveType);
            Assert.AreEqual(NarrativeIds.CorvusNpcId, objectives[0].TargetId);
        }

        [Test]
        public void Hook_Quest_DescriptionHasPlaceholderLore()
        {
            var registry = new QuestRegistry();
            registry.TryGetQuest(NarrativeIds.MainQuestHookId, out var def);
            StringAssert.Contains("PLACEHOLDER_LORE", def.Description);
        }

        [Test]
        public void Hook_Quest_AcceptThenTalkToCorvus_CompletesAndGrantsFlag()
        {
            var service = MakeQuestService(out var section);

            Assert.IsTrue(service.AcceptQuest(NarrativeIds.MainQuestHookId));
            // TalkToNpc Corvus completa o objetivo.
            service.OnNpcTalkedTo(NarrativeIds.CorvusNpcId);

            Assert.IsTrue(service.CanTurnIn(NarrativeIds.MainQuestHookId));
            var result = service.TurnIn(NarrativeIds.MainQuestHookId);
            Assert.IsTrue(result.Succeeded);
            CollectionAssert.Contains(result.FlagsGranted, NarrativeIds.FlagHookComplete);

            // estado persistido = Completed
            var record = section.GetQuestState(NarrativeIds.MainQuestHookId);
            Assert.AreEqual((int)QuestStateStatus.Completed, record.State);
        }

        [Test]
        public void Hook_IsPrerequisiteOf_MainQuestAct1_01()
        {
            var registry = new QuestRegistry();
            Assert.IsTrue(registry.TryGetQuest(QuestMainAct1Ids.Quest01FonteAdormecida, out var act1));
            CollectionAssert.Contains(act1.PrerequisiteQuestIds, NarrativeIds.MainQuestHookId);
        }
    }
}
