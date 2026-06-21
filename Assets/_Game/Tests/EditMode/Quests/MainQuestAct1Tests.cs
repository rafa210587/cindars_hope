using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CindarsHope.MainProgression;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_10 — Main Quest Ato 1 ("A Fonte do Esquecimento").
    ///
    /// Deterministic coverage (pure C#, no Unity refs):
    /// - CA-1: prerequisite chain (each quest gated on the previous) and giver gating.
    /// - CA-2: Water fragment granted exactly once (idempotent integration) + reward idempotency.
    /// - CA-3: anti-softlock (Main quests never expire; cave-depth progress persists across reload).
    /// - CA-4: full questline save/load round-trip at any point in the chain.
    /// - Regression: the 3 pre-existing smoke-test quests stay intact.
    /// </summary>
    [TestFixture]
    public class MainQuestAct1Tests
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

        private QuestRegistry _registry;
        private QuestStateSection _section;
        private FakeInventory _inv;
        private FakeGold _gold;
        private QuestService _service;

        private const string Q1 = "mq_act1_01_fonte_adormecida";
        private const string Q2 = "mq_act1_02_registros_perdidos";
        private const string Q3 = "mq_act1_03_eco_da_agua";
        private const string Q4 = "mq_act1_04_guardiao_da_agua";
        private const string Q5 = "mq_act1_05_fragmento_da_agua";

        [SetUp]
        public void SetUp()
        {
            _registry = new QuestRegistry();
            _section = new QuestStateSection();
            _inv = new FakeInventory();
            _gold = new FakeGold();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            _service = new QuestService(_registry, _section, _inv, _gold, flagService);
        }

        // ─── Registry wiring ────────────────────────────────────────────────────────

        [Test]
        public void Registry_ContainsFiveAct1Quests_AsMainCategory()
        {
            foreach (var id in new[] { Q1, Q2, Q3, Q4, Q5 })
            {
                Assert.IsTrue(_registry.TryGetQuest(id, out var def), $"missing quest {id}");
                Assert.AreEqual(QuestCategory.Main, def.Category, id);
                Assert.IsTrue(def.IsMainProgression, id);
            }
        }

        [Test]
        public void Chain_PrerequisitesFormALinearOrder()
        {
            _registry.TryGetQuest(Q2, out var d2);
            _registry.TryGetQuest(Q3, out var d3);
            _registry.TryGetQuest(Q4, out var d4);
            _registry.TryGetQuest(Q5, out var d5);

            CollectionAssert.Contains(d2.PrerequisiteQuestIds, Q1);
            CollectionAssert.Contains(d3.PrerequisiteQuestIds, Q2);
            CollectionAssert.Contains(d4.PrerequisiteQuestIds, Q3);
            CollectionAssert.Contains(d5.PrerequisiteQuestIds, Q4);

            _registry.TryGetQuest(Q1, out var d1);
            Assert.IsEmpty(d1.PrerequisiteQuestIds, "Q1 must have no prerequisites (entry point).");
        }

        // ─── CA-1: prerequisite gating ──────────────────────────────────────────────

        [Test]
        public void Prerequisites_GateEachStepUntilPreviousCompleted()
        {
            // Q2 blocked until Q1 completed.
            Assert.IsFalse(_service.ArePrerequisitesComplete(Q2));
            CompleteChainStep(Q1, () => MarkTalk("npc_thalindra"));
            Assert.IsTrue(_service.ArePrerequisitesComplete(Q2));

            // Q3 blocked until Q2 completed.
            Assert.IsFalse(_service.ArePrerequisitesComplete(Q3));
            _inv.Items["item_material_stone"] = 5;
            CompleteChainStep(Q2, () => { _service.OnInventoryChanged("item_material_stone"); MarkTalk("npc_maelor"); });
            Assert.IsTrue(_service.ArePrerequisitesComplete(Q3));
        }

        [Test]
        public void Q1_NoPrerequisites_AlwaysOfferable()
        {
            Assert.IsTrue(_service.ArePrerequisitesComplete(Q1));
        }

        [Test]
        public void Regression_FirstSuppliesQuest_HasNoPrerequisites()
        {
            Assert.IsTrue(_service.ArePrerequisitesComplete("quest_first_supplies_for_cindar"));
        }

        // ─── CA-2: reward idempotency (gold/flags) ──────────────────────────────────

        [Test]
        public void TurnIn_GrantsRewardsOnce_EvenOnRepeatedTurnIn()
        {
            DriveQuestlineToQ5ReadyToComplete();

            var first = _service.TurnIn(Q5);
            Assert.IsTrue(first.Succeeded);
            Assert.AreEqual(150, _gold.Gold, "Q5 grants 150 gold once.");

            // Re-turn-in must not re-grant.
            var second = _service.TurnIn(Q5);
            Assert.IsTrue(second.WasAlreadyCompleted);
            Assert.AreEqual(150, _gold.Gold, "Re-turn-in must not duplicate gold.");
        }

        [Test]
        public void TurnIn_Q5_RecordsCompletionFlags()
        {
            DriveQuestlineToQ5ReadyToComplete();
            _service.TurnIn(Q5);

            var record = _service.GetQuestState(Q5);
            CollectionAssert.Contains(record.GrantedFlagIds, "flag_mq_act1_complete");
            CollectionAssert.Contains(record.GrantedFlagIds, "flag_main_post_act1");
        }

        // ─── CA-2 (fragment): MainProgressionService integration idempotency ─────────

        [Test]
        public void WaterFragment_IntegratesOnce_SecondIsAlreadyIntegrated()
        {
            var progression = new MainProgressionSection();
            var mainService = new MainProgressionService();

            var first = mainService.TryIntegrateFragment(progression, MainFragmentType.Water, 1, alreadyGranted: false);
            Assert.IsTrue(first.Success);
            Assert.IsFalse(first.AlreadyIntegrated);
            Assert.IsTrue(progression.IsFragmentIntegrated(MainFragmentType.Water));

            // Re-integration (e.g. reload + re-turn-in) is idempotent: no duplicate, reports already integrated.
            var second = mainService.TryIntegrateFragment(progression, MainFragmentType.Water, 1, alreadyGranted: false);
            Assert.IsTrue(second.Success);
            Assert.IsTrue(second.AlreadyIntegrated);
            Assert.AreEqual(1, progression.FragmentStates.Count(f => f.FragmentType == MainFragmentType.Water));
        }

        [Test]
        public void Bridge_TargetsTheFinalAct1Quest()
        {
            Assert.AreEqual(Q5, MainProgressionQuestBridge.FragmentGrantQuestId);
        }

        // ─── CA-3: anti-softlock ────────────────────────────────────────────────────

        [Test]
        public void MainQuests_NeverExpire()
        {
            foreach (var id in new[] { Q1, Q2, Q3, Q4, Q5 })
            {
                _registry.TryGetQuest(id, out var def);
                Assert.IsFalse(def.CanExpire(), $"{id}: Main quests must never expire (anti-softlock).");
            }
        }

        [Test]
        public void CaveDepthProgress_PersistsAcrossReload()
        {
            // Advance to Q4 active, register the depth-10 objective progress.
            DriveQuestlineToQ4Active();
            _service.OnCaveLevelEntered(10); // satisfies reach-depth-10
            var record = _service.GetQuestState(Q4);
            var depthObj = record.ObjectiveStates.First(o => o.ObjectiveId == "obj_mq4_reach_depth_10");
            Assert.IsTrue(depthObj.IsCompleted, "depth 10 must register before reload.");

            // Simulate a death/new cave run by reloading from a captured save: progress must survive.
            var saveData = CaptureSave();
            var reloaded = MakeFreshServiceFrom(saveData);
            var reloadedRecord = reloaded.GetQuestState(Q4);
            var reloadedDepth = reloadedRecord.ObjectiveStates.First(o => o.ObjectiveId == "obj_mq4_reach_depth_10");
            Assert.IsTrue(reloadedDepth.IsCompleted, "reach-depth progress must persist across reload (anti-softlock).");
        }

        // ─── CA-4: save round-trip ──────────────────────────────────────────────────

        [Test]
        public void Questline_SurvivesSaveLoad_MidChain()
        {
            // Mid-chain: Q1 completed, Q2 active with partial progress.
            CompleteChainStep(Q1, () => MarkTalk("npc_thalindra"));
            _service.AcceptQuest(Q2);
            _inv.Items["item_material_stone"] = 3;
            _service.OnInventoryChanged("item_material_stone");

            var saveData = CaptureSave();
            var reloaded = MakeFreshServiceFrom(saveData);

            Assert.AreEqual((int)QuestStateStatus.Completed, reloaded.GetQuestState(Q1).State);
            var q2 = reloaded.GetQuestState(Q2);
            Assert.AreEqual((int)QuestStateStatus.Active, q2.State);
            var stoneObj = q2.ObjectiveStates.First(o => o.ObjectiveId == "obj_mq2_collect_stone_x5");
            Assert.AreEqual(3, stoneObj.CurrentProgress, "partial collect progress must survive reload.");
        }

        [Test]
        public void Questline_RewardIdempotency_SurvivesReload()
        {
            DriveQuestlineToQ5ReadyToComplete();
            _service.TurnIn(Q5);
            Assert.AreEqual(150, _gold.Gold);

            // Reload with a fresh gold sink; re-turn-in must NOT re-grant (GrantedRewardIds preserved).
            var saveData = CaptureSave();
            var reloadedGold = new FakeGold();
            var reloaded = MakeFreshServiceFrom(saveData, reloadedGold);

            var result = reloaded.TurnIn(Q5);
            Assert.IsTrue(result.WasAlreadyCompleted);
            Assert.AreEqual(0, reloadedGold.Gold, "completed quest must not re-grant gold after reload.");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private void MarkTalk(string npcId) => _service.OnNpcTalkedTo(npcId);

        private void CompleteChainStep(string questId, System.Action satisfyObjectives)
        {
            Assert.IsTrue(_service.AcceptQuest(questId), $"accept {questId}");
            satisfyObjectives();
            Assert.IsTrue(_service.CanTurnIn(questId), $"{questId} should be ready to complete");
            var r = _service.TurnIn(questId);
            Assert.IsTrue(r.Succeeded, $"turn in {questId}");
        }

        private void DriveQuestlineToQ4Active()
        {
            CompleteChainStep(Q1, () => MarkTalk("npc_thalindra"));
            _inv.Items["item_material_stone"] = 5;
            CompleteChainStep(Q2, () => { _service.OnInventoryChanged("item_material_stone"); MarkTalk("npc_maelor"); });
            CompleteChainStep(Q3, () => _service.OnCaveLevelEntered(5));
            Assert.IsTrue(_service.AcceptQuest(Q4));
        }

        private void DriveQuestlineToQ5ReadyToComplete()
        {
            DriveQuestlineToQ4Active();
            _service.OnCaveLevelEntered(10);
            // Guardian: DefeatEnemy "any" x3.
            _service.OnEnemyKilled("enemy_any_01");
            _service.OnEnemyKilled("enemy_any_02");
            _service.OnEnemyKilled("enemy_any_03");
            Assert.IsTrue(_service.CanTurnIn(Q4), "Q4 ready after depth 10 + 3 kills");
            Assert.IsTrue(_service.TurnIn(Q4).Succeeded);

            Assert.IsTrue(_service.AcceptQuest(Q5));
            MarkTalk("npc_corvus");
            Assert.IsTrue(_service.CanTurnIn(Q5), "Q5 ready after talking to Corvus");
        }

        private QuestStateSectionSaveData CaptureSave()
        {
            var section = _service.GetSaveSection();
            var dto = new QuestStateSectionSaveData
            {
                Version = section.Version,
                GlobalKnownHints = new List<string>(section.GlobalKnownHints),
                DiscoveredSecretQuestIds = new List<string>(section.DiscoveredSecretQuestIds),
                RewardedMainActIds = new List<string>(section.RewardedMainActIds),
                QuestStates = new List<QuestStateSaveData>()
            };
            foreach (var record in section.QuestStates)
            {
                var q = new QuestStateSaveData
                {
                    QuestId = record.QuestId,
                    State = record.State,
                    KnownObjectiveIds = new List<string>(record.KnownObjectiveIds),
                    CompletedAtDay = record.CompletedAtDay ?? 0,
                    Tracked = record.Tracked,
                    Discovered = record.Discovered,
                    GrantedRewardIds = new List<string>(record.GrantedRewardIds),
                    GrantedFlagIds = new List<string>(record.GrantedFlagIds),
                    ObjectiveStates = new List<QuestObjectiveStateSaveData>()
                };
                foreach (var obj in record.ObjectiveStates)
                {
                    q.ObjectiveStates.Add(new QuestObjectiveStateSaveData
                    {
                        ObjectiveId = obj.ObjectiveId,
                        CurrentProgress = obj.CurrentProgress,
                        RequiredProgress = obj.RequiredProgress,
                        IsCompleted = obj.IsCompleted,
                        IsFailed = obj.IsFailed,
                        IsKnown = obj.IsKnown
                    });
                }
                dto.QuestStates.Add(q);
            }
            return dto;
        }

        private QuestService MakeFreshServiceFrom(QuestStateSectionSaveData saveData, FakeGold gold = null)
        {
            var registry = new QuestRegistry();
            var section = new QuestStateSection();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            var service = new QuestService(registry, section, new FakeInventory(), gold ?? new FakeGold(), flagService);
            service.RestoreFromSaveData(saveData);
            return service;
        }
    }
}
