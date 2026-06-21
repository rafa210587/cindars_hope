using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.MainProgression;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_36 — Main Quest Acts 2-4 ("O Arco da Memoria", "A Pedra que Sussurra",
    /// "A Esperanca Enterrada"). Deterministic EditMode coverage (pure C#, no Unity scene refs):
    ///
    /// - CA-1: each act is 4-6 chained Main quests; act N+1's entry is gated on the prior act's finale.
    /// - CA-2: act milestone — act_N_done + +1 skill point (idempotent, incl. after reload) + lore record.
    /// - CA-3: Nymirian gated behind flag_nymirian_available (set on mq_act3_04).
    /// - CA-4: Act 1 stays intact (regression) — 5 quests, chain, Water-fragment finale unchanged.
    /// - Anti-softlock: every act quest is Main => never expires.
    /// - ActCompletedEvent published once per act; reward idempotency survives save/load.
    /// </summary>
    [TestFixture]
    public class MainActsTests
    {
        private sealed class FakeInventory : IQuestInventoryAccess
        {
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();
            public int GetItemCount(string itemId) => Items.TryGetValue(itemId, out var c) ? c : 0;
            public bool TryAddItem(string itemId, int count) { Items[itemId] = GetItemCount(itemId) + count; return true; }
            public bool TryRemoveItems(string itemId, int count)
            {
                if (GetItemCount(itemId) < count) return false;
                Items[itemId] -= count; return true;
            }
        }

        private sealed class FakeGold : IQuestGoldAccess
        {
            public int Gold;
            public void AddGold(int amount) => Gold += amount;
        }

        private sealed class FakeProgression : IQuestProgressionAccess
        {
            public int Xp;
            public int SkillPoints;
            public void AddXp(int amount) => Xp += amount;
            public void GrantSkillPoints(int amount) => SkillPoints += amount;
        }

        private QuestRegistry _registry;
        private QuestStateSection _section;
        private FakeInventory _inv;
        private FakeGold _gold;
        private FakeProgression _prog;
        private QuestService _service;

        [SetUp]
        public void SetUp()
        {
            _registry = new QuestRegistry();
            _section = new QuestStateSection();
            _inv = new FakeInventory();
            _gold = new FakeGold();
            _prog = new FakeProgression();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            _service = new QuestService(_registry, _section, _inv, _gold, flagService, _prog);
        }

        // ─── CA-1: registry wiring + chaining ───────────────────────────────────────

        [Test]
        public void Act2_HasFiveChainedMainQuests()
        {
            var ids = new[]
            {
                QuestMainActsIds.Act2Quest01RecordsOfSilver,
                QuestMainActsIds.Act2Quest02TheVeiledVisitor,
                QuestMainActsIds.Act2Quest03SongBelow,
                QuestMainActsIds.Act2Quest04GateOfFrost,
                QuestMainActsIds.Act2Quest05FragmentOfMemory
            };
            AssertChainedMain(ids, entryPrereq: QuestMainAct1Ids.Quest05FragmentoDaAgua);
        }

        [Test]
        public void Act3_HasFiveChainedMainQuests_EntryGatedOnAct2Finale()
        {
            var ids = new[]
            {
                QuestMainActsIds.Act3Quest01BlackstoneLedger,
                QuestMainActsIds.Act3Quest02ThrallMercy,
                QuestMainActsIds.Act3Quest03TheQuietPriest,
                QuestMainActsIds.Act3Quest04WardenOfSilence,
                QuestMainActsIds.Act3Quest05FragmentOfLife
            };
            AssertChainedMain(ids, entryPrereq: QuestMainActsIds.Act2Quest05FragmentOfMemory);
        }

        [Test]
        public void Act4_HasFourChainedMainQuests_EntryGatedOnAct3Finale()
        {
            var ids = new[]
            {
                QuestMainActsIds.Act4Quest01LitanyComplete,
                QuestMainActsIds.Act4Quest02TheJailer,
                QuestMainActsIds.Act4Quest03VelKaraum,
                QuestMainActsIds.Act4Quest04BrokenRemembrance
            };
            AssertChainedMain(ids, entryPrereq: QuestMainActsIds.Act3Quest05FragmentOfLife);
        }

        [Test]
        public void Act4_DoesNotAuthorFinalChoice_BoundaryToFable43()
        {
            // mq_act4_05_final_choice must NOT exist here (deferred to fable_43).
            Assert.IsFalse(_registry.TryGetQuest("mq_act4_05_final_choice", out _),
                "Final choice quest must be deferred to fable_43, not authored in fable_36.");
        }

        // ─── CA-1: act N+1 offered only after act_N_done ────────────────────────────

        [Test]
        public void Act2EntryPrerequisite_BlockedUntilAct1Finale()
        {
            Assert.IsFalse(_service.ArePrerequisitesComplete(QuestMainActsIds.Act2Quest01RecordsOfSilver),
                "Act 2 must not be offerable before Act 1 finale is completed.");
        }

        // ─── CA-2: act milestone — flag + skill point + lore (via the bridge) ───────

        [Test]
        public void Bridge_CompleteAct_GrantsExactlyOneSkillPointPerAct_Idempotent()
        {
            var bridge = new MainProgressionQuestBridge();
            bridge.SetQuestService(_service);

            var act2 = QuestMainActsIds.Finales.First(f => f.ActNumber == 2);

            bridge.CompleteAct(act2);
            Assert.AreEqual(1, _prog.SkillPoints, "Act 2 grants +1 skill point.");

            // Re-trigger (reload + re-turn-in): no double grant.
            bridge.CompleteAct(act2);
            Assert.AreEqual(1, _prog.SkillPoints, "Act 2 skill point must not duplicate on re-trigger.");
        }

        [Test]
        public void Bridge_SkillPoint_NotDuplicatedAcrossFreshServiceAfterReload()
        {
            // First service awards act 3; the awarded act id is persisted in RewardedMainActIds.
            Assert.IsTrue(_service.TryAwardActSkillPoint("act_3_life"));
            Assert.AreEqual(1, _prog.SkillPoints);

            // Reload: a fresh service restoring the same RewardedMainActIds must not re-grant.
            var dto = CaptureSave();
            var prog2 = new FakeProgression();
            var service2 = MakeFreshServiceFrom(dto, progression: prog2);
            Assert.IsFalse(service2.TryAwardActSkillPoint("act_3_life"),
                "Skill point must not be re-granted after reload.");
            Assert.AreEqual(0, prog2.SkillPoints);
        }

        [Test]
        public void Bridge_CompleteAct_PublishesActCompletedEventOncePerAct()
        {
            var bridge = new MainProgressionQuestBridge();
            bridge.SetQuestService(_service);

            var received = new List<ActCompletedEvent>();
            System.Action<ActCompletedEvent> handler = e => received.Add(e);
            GameEventBus.Subscribe(handler);
            try
            {
                var act2 = QuestMainActsIds.Finales.First(f => f.ActNumber == 2);
                bridge.CompleteAct(act2);
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(1, received.Count, "ActCompletedEvent must fire once.");
            Assert.AreEqual(2, received[0].ActNumber);
            Assert.AreEqual(QuestMainActsIds.FlagAct2Done, received[0].ActDoneFlagId);
            Assert.AreEqual(QuestMainActsIds.LoreRecordAct2, received[0].LoreRecordId);
        }

        [Test]
        public void Bridge_AllFourActFinales_MapToDistinctFragmentsInCanonicalOrder()
        {
            var byAct = QuestMainActsIds.Finales.OrderBy(f => f.ActNumber).ToList();
            Assert.AreEqual(4, byAct.Count);
            Assert.AreEqual(MainFragmentType.Water, byAct[0].Fragment);
            Assert.AreEqual(MainFragmentType.Memory, byAct[1].Fragment);
            Assert.AreEqual(MainFragmentType.Life, byAct[2].Fragment);
            Assert.AreEqual(MainFragmentType.Hope, byAct[3].Fragment);

            // Act 4 (Hope) is prep-only: NOT integrated here (fable_43 owns the final integration).
            Assert.IsTrue(byAct[0].IntegratesFragment);
            Assert.IsTrue(byAct[1].IntegratesFragment);
            Assert.IsTrue(byAct[2].IntegratesFragment);
            Assert.IsFalse(byAct[3].IntegratesFragment, "Hope must be hinted, not integrated (fable_43).");
        }

        [Test]
        public void Bridge_FinaleForQuest_ResolvesEachActFinalQuest()
        {
            Assert.AreEqual(2, QuestMainActsIds.FinaleForQuest(QuestMainActsIds.Act2Quest05FragmentOfMemory).ActNumber);
            Assert.AreEqual(3, QuestMainActsIds.FinaleForQuest(QuestMainActsIds.Act3Quest05FragmentOfLife).ActNumber);
            Assert.AreEqual(4, QuestMainActsIds.FinaleForQuest(QuestMainActsIds.Act4Quest04BrokenRemembrance).ActNumber);
            Assert.IsNull(QuestMainActsIds.FinaleForQuest(QuestMainActsIds.Act2Quest01RecordsOfSilver),
                "A non-final act quest must not resolve to a finale.");
        }

        // ─── CA-2: act_N_done flag is granted by the act-final quest reward ─────────

        [Test]
        public void Act2Finale_GrantsActDoneFlag_OnTurnIn()
        {
            DriveToAct2FinaleReady();
            var result = _service.TurnIn(QuestMainActsIds.Act2Quest05FragmentOfMemory);
            Assert.IsTrue(result.Succeeded);
            var record = _service.GetQuestState(QuestMainActsIds.Act2Quest05FragmentOfMemory);
            CollectionAssert.Contains(record.GrantedFlagIds, QuestMainActsIds.FlagAct2Done);
            CollectionAssert.Contains(record.GrantedFlagIds, QuestMainActsIds.FlagAct2Complete);
            CollectionAssert.Contains(record.GrantedFlagIds, QuestMainActsIds.FlagMainPostAct2);
        }

        // ─── CA-3: Nymirian gated by flag ───────────────────────────────────────────

        [Test]
        public void Nymirian_NotAvailableUntilFlagSet()
        {
            var setFlags = new HashSet<string>();
            System.Func<string, bool> isSet = f => setFlags.Contains(f);

            Assert.IsFalse(NymirianAppearance.IsAvailable(isSet), "Nymirian hidden before the flag.");
            setFlags.Add(QuestMainActsIds.FlagNymirianAvailable);
            Assert.IsTrue(NymirianAppearance.IsAvailable(isSet), "Nymirian appears once the flag is set.");
        }

        [Test]
        public void Nymirian_FailsClosed_WithNullPredicate()
        {
            Assert.IsFalse(NymirianAppearance.IsAvailable(null));
        }

        [Test]
        public void Act3Act04_GrantsNymirianAvailableFlag()
        {
            // The Warden of Silence quest turn-in is what makes Nymirian conversable.
            _registry.TryGetQuest(QuestMainActsIds.Act3Quest04WardenOfSilence, out var def);
            Assert.AreEqual(QuestCategory.Main, def.Category);
            // The reward set carries the nymirian-available flag (authored on this quest).
            var rewards = _registry.GetRewards(QuestMainActsIds.Act3Quest04WardenOfSilence);
            Assert.IsTrue(rewards.Any(r => r.GrantedFlagId == QuestMainActsIds.FlagNymirianAvailable),
                "mq_act3_04 must grant flag_nymirian_available.");
        }

        // ─── Soft depth advisory (narrative, never a hard gate) ─────────────────────

        [Test]
        public void DepthAdvisory_ShownOncePastActGate_NeverBlocks()
        {
            // Highest completed act = 2 opens depth 30; descending to 35 should advise.
            Assert.IsTrue(MainActDepthAdvisory.ShouldAdvise(currentDepth: 35, highestCompletedAct: 2, alreadyShownForThisAct: false));
            // Not past the gate: no advisory.
            Assert.IsFalse(MainActDepthAdvisory.ShouldAdvise(currentDepth: 25, highestCompletedAct: 2, alreadyShownForThisAct: false));
            // Already shown for this act: suppressed (shown once per act).
            Assert.IsFalse(MainActDepthAdvisory.ShouldAdvise(currentDepth: 35, highestCompletedAct: 2, alreadyShownForThisAct: true));
        }

        // ─── Anti-softlock ──────────────────────────────────────────────────────────

        [Test]
        public void AllActsQuests_AreMainAndNeverExpire()
        {
            foreach (var f in QuestMainActsIds.Finales)
            {
                // sanity: each finale resolves to a registered Main quest
                Assert.IsTrue(_registry.TryGetQuest(f.FinalQuestId, out var def), f.FinalQuestId);
                Assert.AreEqual(QuestCategory.Main, def.Category, f.FinalQuestId);
                Assert.IsFalse(def.CanExpire(), $"{f.FinalQuestId}: Main quests must never expire.");
            }
        }

        // ─── CA-4: Act 1 regression ─────────────────────────────────────────────────

        [Test]
        public void Regression_Act1_StillFiveQuestsAndWaterFinale()
        {
            var act1 = new[]
            {
                QuestMainAct1Ids.Quest01FonteAdormecida,
                QuestMainAct1Ids.Quest02RegistrosPerdidos,
                QuestMainAct1Ids.Quest03EcoDaAgua,
                QuestMainAct1Ids.Quest04GuardiaoDaAgua,
                QuestMainAct1Ids.Quest05FragmentoDaAgua
            };
            foreach (var id in act1)
            {
                Assert.IsTrue(_registry.TryGetQuest(id, out var def), id);
                Assert.AreEqual(QuestCategory.Main, def.Category, id);
            }
            // Act 1 finale still maps to Water and still integrates (unchanged contract).
            var f1 = QuestMainActsIds.Finales.First(f => f.ActNumber == 1);
            Assert.AreEqual(QuestMainAct1Ids.Quest05FragmentoDaAgua, f1.FinalQuestId);
            Assert.AreEqual(MainFragmentType.Water, f1.Fragment);
            Assert.IsTrue(f1.IntegratesFragment);
            Assert.AreEqual(QuestMainAct1Ids.Quest05FragmentoDaAgua, MainProgressionQuestBridge.FragmentGrantQuestId);
        }

        [Test]
        public void Regression_SmokeTestQuests_Intact()
        {
            Assert.IsTrue(_registry.TryGetQuest("quest_first_supplies_for_cindar", out _));
            Assert.IsTrue(_registry.TryGetQuest("quest_tools_for_the_town", out _));
            Assert.IsTrue(_registry.TryGetQuest("quest_echo_from_the_cave", out _));
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private void AssertChainedMain(string[] ids, string entryPrereq)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                Assert.IsTrue(_registry.TryGetQuest(ids[i], out var def), $"missing {ids[i]}");
                Assert.AreEqual(QuestCategory.Main, def.Category, ids[i]);
                Assert.IsTrue(def.IsMainProgression, ids[i]);

                if (i == 0)
                    CollectionAssert.Contains(def.PrerequisiteQuestIds, entryPrereq, $"{ids[i]} entry prereq");
                else
                    CollectionAssert.Contains(def.PrerequisiteQuestIds, ids[i - 1], $"{ids[i]} chained prereq");
            }
            Assert.GreaterOrEqual(ids.Length, 4, "Each act must have 4-6 quests.");
            Assert.LessOrEqual(ids.Length, 6, "Each act must have 4-6 quests.");
        }

        private void DriveToAct2FinaleReady()
        {
            // Act 1 finale must be completed first (Act 2 entry prereq).
            DriveAct1ToComplete();

            // act2_01: collect 4 + talk thalindra
            AcceptAndComplete(QuestMainActsIds.Act2Quest01RecordsOfSilver, () =>
            {
                _inv.Items["item_material_stone"] = 4; _service.OnInventoryChanged("item_material_stone");
                _service.OnNpcTalkedTo(QuestMainActsIds.ThalindraId);
            });
            // act2_02: reach 15 + talk vaelrion
            AcceptAndComplete(QuestMainActsIds.Act2Quest02TheVeiledVisitor, () =>
            {
                _service.OnCaveLevelEntered(15); _service.OnNpcTalkedTo(QuestMainActsIds.VaelrionId);
            });
            // act2_03: collect 6 + talk liora
            AcceptAndComplete(QuestMainActsIds.Act2Quest03SongBelow, () =>
            {
                _inv.Items["item_material_stone"] = 6; _service.OnInventoryChanged("item_material_stone");
                _service.OnNpcTalkedTo(QuestMainActsIds.LioraId);
            });
            // act2_04: reach 30 + defeat 1
            AcceptAndComplete(QuestMainActsIds.Act2Quest04GateOfFrost, () =>
            {
                _service.OnCaveLevelEntered(30); _service.OnEnemyKilled("enemy_frost_gate");
            });
            // act2_05: accept + talk corvus -> ready (caller turns in)
            Assert.IsTrue(_service.AcceptQuest(QuestMainActsIds.Act2Quest05FragmentOfMemory));
            _service.OnNpcTalkedTo(QuestMainActsIds.CorvusId);
            Assert.IsTrue(_service.CanTurnIn(QuestMainActsIds.Act2Quest05FragmentOfMemory));
        }

        private void DriveAct1ToComplete()
        {
            AcceptAndComplete(QuestMainAct1Ids.Quest01FonteAdormecida, () => _service.OnNpcTalkedTo("npc_thalindra"));
            AcceptAndComplete(QuestMainAct1Ids.Quest02RegistrosPerdidos, () =>
            {
                _inv.Items["item_material_stone"] = 5; _service.OnInventoryChanged("item_material_stone");
                _service.OnNpcTalkedTo("npc_maelor");
            });
            AcceptAndComplete(QuestMainAct1Ids.Quest03EcoDaAgua, () => _service.OnCaveLevelEntered(5));
            AcceptAndComplete(QuestMainAct1Ids.Quest04GuardiaoDaAgua, () =>
            {
                _service.OnCaveLevelEntered(10);
                _service.OnEnemyKilled("e1"); _service.OnEnemyKilled("e2"); _service.OnEnemyKilled("e3");
            });
            AcceptAndComplete(QuestMainAct1Ids.Quest05FragmentoDaAgua, () => _service.OnNpcTalkedTo("npc_corvus"));
            // reset inventory so leftover stones don't auto-satisfy later collect objectives
            _inv.Items["item_material_stone"] = 0;
        }

        private void AcceptAndComplete(string questId, System.Action satisfy)
        {
            Assert.IsTrue(_service.AcceptQuest(questId), $"accept {questId}");
            satisfy();
            Assert.IsTrue(_service.CanTurnIn(questId), $"{questId} ready");
            Assert.IsTrue(_service.TurnIn(questId).Succeeded, $"turn in {questId}");
        }

        private QuestStateSectionSaveData CaptureSave()
        {
            var section = _service.GetSaveSection();
            return new QuestStateSectionSaveData
            {
                Version = section.Version,
                GlobalKnownHints = new List<string>(section.GlobalKnownHints),
                DiscoveredSecretQuestIds = new List<string>(section.DiscoveredSecretQuestIds),
                RewardedMainActIds = new List<string>(section.RewardedMainActIds),
                QuestStates = new List<QuestStateSaveData>()
            };
        }

        private QuestService MakeFreshServiceFrom(QuestStateSectionSaveData saveData, FakeProgression progression)
        {
            var registry = new QuestRegistry();
            var section = new QuestStateSection();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            var service = new QuestService(registry, section, new FakeInventory(), new FakeGold(), flagService, progression);
            service.RestoreFromSaveData(saveData);
            return service;
        }
    }
}
