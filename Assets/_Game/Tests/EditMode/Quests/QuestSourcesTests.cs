using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_34 — quest source infrastructure (CA-1..CA-5 + EMENDA F34-B/F34-C).
    ///
    /// Pure/deterministic coverage: board rotation, template instancing, scaled rewards,
    /// secret-quest determinism, main-act skill point idempotency (incl. after save/load),
    /// dynamic-instance save round-trip, prerequisite gate.
    /// </summary>
    [TestFixture]
    public class QuestSourcesTests
    {
        // ─── Stub adapters (no Unity refs) ──────────────────────────────────────────
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

        private QuestService MakeService(out QuestStateSection section, out FakeInventory inv,
            out FakeGold gold, out FakeProgression prog)
        {
            var registry = new QuestRegistry();
            section = new QuestStateSection();
            inv = new FakeInventory();
            gold = new FakeGold();
            prog = new FakeProgression();
            var flagService = new QuestFlagService(new QuestFlagRegistry());
            return new QuestService(registry, section, inv, gold, flagService, prog);
        }

        // ─── CA-1 Board rotation determinism ────────────────────────────────────────

        [Test]
        public void Board_SameDay_SameContracts()
        {
            var board = new QuestBoardService();
            var a = board.GenerateDailyContracts("seed", 12, 10);
            var b = board.GenerateDailyContracts("seed", 12, 10);

            Assert.AreEqual(QuestBoardService.ContractsPerDay, a.Count);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].QuestTemplateId, b[i].QuestTemplateId);
                Assert.AreEqual(a[i].TargetId, b[i].TargetId);
                Assert.AreEqual(a[i].Quantity, b[i].Quantity);
            }
        }

        [Test]
        public void Board_DifferentDay_Rotates()
        {
            var board = new QuestBoardService();
            var d1 = board.GenerateDailyContracts("seed", 1, 10);
            var d2 = board.GenerateDailyContracts("seed", 2, 10);

            // At least one slot differs between consecutive days (rotation).
            bool anyDifferent = false;
            for (int i = 0; i < d1.Count && i < d2.Count; i++)
                if (d1[i].QuestTemplateId != d2[i].QuestTemplateId) anyDifferent = true;
            Assert.IsTrue(anyDifferent, "Board did not rotate between days.");
        }

        [Test]
        public void Board_GeneratesExactlyThreePerDay()
        {
            var board = new QuestBoardService();
            var contracts = board.GenerateDailyContracts("seed", 5, 1);
            Assert.AreEqual(3, contracts.Count);
            // distinct instance ids
            var ids = new HashSet<string>();
            foreach (var c in contracts) Assert.IsTrue(ids.Add(c.QuestId));
        }

        [Test]
        public void Board_NeverTargetsNonAggressiveCreature()
        {
            Assert.IsFalse(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Cull, "enemy_old_scrounger_king"));
            Assert.IsFalse(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Cull, "enemy_goblin_warchief"));
            Assert.IsFalse(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Cull, "enemy_silence_warden"));
            Assert.IsTrue(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Cull, "enemy_slime_basic"));
        }

        [Test]
        public void Board_NeverRequestsQuestItem()
        {
            Assert.IsFalse(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Gather, "item_quest_anya_fragment_water"));
            Assert.IsFalse(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Delivery, "item_quest_black_stone"));
            Assert.IsTrue(QuestBoardService.IsTargetAllowed(BoardTemplateKind.Gather, "item_material_wood"));
        }

        [Test]
        public void Board_DefaultTemplates_AllPassCanonicalRules()
        {
            foreach (var t in QuestBoardService.DefaultTemplates())
                Assert.IsTrue(QuestBoardService.IsTargetAllowed(t.Kind, t.TargetId),
                    $"Template {t.TemplateId} violates a canonical board rule.");
        }

        // ─── CA-2 Scaled reward formula ─────────────────────────────────────────────

        [Test]
        public void RewardScaling_FollowsCatalogCurve()
        {
            // base * (1 + 0.08 * level), floored.
            Assert.AreEqual(100, QuestRewardScaling.Scale(100, 0));
            Assert.AreEqual(108, QuestRewardScaling.Scale(100, 1));
            Assert.AreEqual(180, QuestRewardScaling.Scale(100, 10));
        }

        [Test]
        public void RewardScaling_NeverBelowBase_AndZeroSafe()
        {
            Assert.AreEqual(50, QuestRewardScaling.Scale(50, 0));
            Assert.AreEqual(50, QuestRewardScaling.Scale(50, -5)); // negative level clamps to 0
            Assert.AreEqual(0, QuestRewardScaling.Scale(0, 10));
        }

        [Test]
        public void Board_ScalesRewardByPlayerLevel()
        {
            var board = new QuestBoardService();
            var low = board.GenerateDailyContracts("seed", 7, 1);
            var high = board.GenerateDailyContracts("seed", 7, 50);
            for (int i = 0; i < low.Count; i++)
            {
                Assert.GreaterOrEqual(high[i].RewardGold, low[i].RewardGold);
                Assert.AreEqual(1, low[i].QuestLevel);
                Assert.AreEqual(50, high[i].QuestLevel);
            }
        }

        [Test]
        public void DynamicContract_CullObjective_CountsKills_AndAwardsScaledRewards()
        {
            var service = MakeService(out _, out _, out var gold, out var prog);
            var instance = new QuestInstance
            {
                QuestId = "board_contract_d1_0",
                QuestTemplateId = "bd_cull_slime",
                Source = QuestSource.Board,
                TargetId = "enemy_slime_basic",
                Quantity = 2,
                QuestLevel = 10,
                RewardGold = QuestRewardScaling.Scale(40, 10), // 72
                RewardXp = QuestRewardScaling.Scale(30, 10)    // 54
            };

            Assert.IsTrue(service.AcceptDynamicInstance(instance));

            // Two synthetic kills of the right band complete the objective.
            service.OnEnemyKilled("enemy_slime_basic");
            service.OnEnemyKilled("enemy_slime_basic");

            Assert.IsTrue(service.CanTurnIn("board_contract_d1_0"));
            var result = service.TurnIn("board_contract_d1_0");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(72, gold.Gold);
            Assert.AreEqual(54, prog.Xp);
        }

        // ─── CA-3 Secret quests determinism ─────────────────────────────────────────

        [Test]
        public void SecretOffer_ChanceIsDeterministicBySeed()
        {
            bool a = SecretQuestOffer.ShouldOfferByChance("seedA", "ctx", 15);
            bool b = SecretQuestOffer.ShouldOfferByChance("seedA", "ctx", 15);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void SecretOffer_BoundaryChances()
        {
            Assert.IsFalse(SecretQuestOffer.ShouldOfferByChance("x", "y", 0));
            Assert.IsTrue(SecretQuestOffer.ShouldOfferByChance("x", "y", 100));
        }

        [Test]
        public void SecretOffer_ApproximatelyFifteenPercentOverManySeeds()
        {
            int hits = 0;
            const int n = 4000;
            for (int i = 0; i < n; i++)
                if (SecretQuestOffer.ShouldOfferByChance("run", $"ctx_{i}", SecretQuestOffer.MerchantOfferChancePercent))
                    hits++;
            double pct = 100.0 * hits / n;
            // Wide tolerance — only asserts the gate is not stuck/biased, not a precise rate.
            Assert.That(pct, Is.InRange(9.0, 21.0), $"Secret offer rate {pct:F1}% is implausible for a 15% gate.");
        }

        [Test]
        public void SecretQuest_OnlyDiscoveredAppearsInLog()
        {
            var service = MakeService(out var section, out _, out _, out _);

            Assert.IsFalse(service.IsSecretDiscovered("scq_hidden_1"));
            Assert.IsTrue(service.OfferSecretQuest("scq_hidden_1"));   // first discovery
            Assert.IsFalse(service.OfferSecretQuest("scq_hidden_1"));  // idempotent
            Assert.IsTrue(service.IsSecretDiscovered("scq_hidden_1"));
            Assert.Contains("scq_hidden_1", section.DiscoveredSecretQuestIds);
        }

        // ─── CA-4 Main act skill point idempotency (incl. after reload) ──────────────

        [Test]
        public void MainAct_GrantsExactlyOneSkillPoint()
        {
            var service = MakeService(out _, out _, out _, out var prog);
            Assert.IsTrue(service.TryAwardActSkillPoint("act_1_water"));
            Assert.IsFalse(service.TryAwardActSkillPoint("act_1_water")); // no double grant
            Assert.AreEqual(1, prog.SkillPoints);
        }

        [Test]
        public void MainAct_NotDuplicatedAfterSaveLoad()
        {
            var service = MakeService(out _, out _, out _, out var prog);
            Assert.IsTrue(service.TryAwardActSkillPoint("act_1_water"));
            Assert.AreEqual(1, prog.SkillPoints);

            // Persist the section and restore into a fresh service.
            var saveData = CaptureSection(service.GetSaveSection());

            var registry2 = new QuestRegistry();
            var prog2 = new FakeProgression();
            var service2 = new QuestService(registry2, new QuestStateSection(),
                new FakeInventory(), new FakeGold(), new QuestFlagService(new QuestFlagRegistry()), prog2);
            service2.RestoreFromSaveData(saveData);

            // Re-awarding the same act after reload must NOT grant again.
            Assert.IsFalse(service2.TryAwardActSkillPoint("act_1_water"));
            Assert.AreEqual(0, prog2.SkillPoints);
        }

        // ─── CA-5 Dynamic instance save round-trip ──────────────────────────────────

        [Test]
        public void DynamicInstance_SurvivesSaveLoad_WithSimpleParams()
        {
            var service = MakeService(out _, out var inv, out var gold, out var prog);
            var instance = new QuestInstance
            {
                QuestId = "board_contract_d3_1",
                QuestTemplateId = "bd_gather_wood",
                Source = QuestSource.Board,
                TargetId = "item_material_wood",
                Quantity = 3,
                QuestLevel = 8,
                RewardGold = QuestRewardScaling.Scale(30, 8),
                RewardXp = QuestRewardScaling.Scale(20, 8),
                GeneratedForDay = 3
            };
            Assert.IsTrue(service.AcceptDynamicInstance(instance));

            var saveData = CaptureSection(service.GetSaveSection());

            // Restore into a fresh service (empty registry — must re-register the instance).
            var registry2 = new QuestRegistry();
            var inv2 = new FakeInventory();
            var gold2 = new FakeGold();
            var prog2 = new FakeProgression();
            var service2 = new QuestService(registry2, new QuestStateSection(), inv2, gold2,
                new QuestFlagService(new QuestFlagRegistry()), prog2);
            service2.RestoreFromSaveData(saveData);

            var restored = service2.GetQuestState("board_contract_d3_1");
            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.IsDynamicInstance);
            Assert.AreEqual((int)QuestSource.Board, restored.Source);
            Assert.AreEqual("item_material_wood", restored.InstanceTargetId);
            Assert.AreEqual(3, restored.InstanceQuantity);
            Assert.AreEqual(instance.RewardGold, restored.InstanceRewardGold);

            // And it must still resolve in the live flow: collecting items completes + turns in.
            inv2.Items["item_material_wood"] = 3;
            service2.OnInventoryChanged("item_material_wood");
            Assert.IsTrue(service2.CanTurnIn("board_contract_d3_1"));
            var result = service2.TurnIn("board_contract_d3_1");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(instance.RewardGold, gold2.Gold);
        }

        [Test]
        public void Section_DefaultSource_IsNpc_ForLegacyRecords()
        {
            // A record with no explicit source defaults to Npc (safe for legacy saves).
            var rec = new QuestStateRecord { QuestId = "q_legacy" };
            Assert.AreEqual((int)QuestSource.Npc, rec.Source);
            Assert.IsFalse(rec.IsDynamicInstance);
        }

        // ─── Source mapping + tabs (EMENDA F34-B) ────────────────────────────────────

        [Test]
        public void SourceMapper_CategoryToSource()
        {
            Assert.AreEqual(QuestSource.Main, QuestSourceMapper.FromCategory(QuestCategory.Main));
            Assert.AreEqual(QuestSource.Board, QuestSourceMapper.FromCategory(QuestCategory.FarmOrder));
            Assert.AreEqual(QuestSource.CaveContract, QuestSourceMapper.FromCategory(QuestCategory.CaveContract));
            Assert.AreEqual(QuestSource.CaveSecret, QuestSourceMapper.FromCategory(QuestCategory.Hidden));
            Assert.AreEqual(QuestSource.Npc, QuestSourceMapper.FromCategory(QuestCategory.Side));
        }

        [Test]
        public void SourceMapper_TabGrouping_CaveContractUnderContracts()
        {
            Assert.AreEqual(QuestLogTab.Main, QuestSourceMapper.TabFor(QuestSource.Main));
            Assert.AreEqual(QuestLogTab.Side, QuestSourceMapper.TabFor(QuestSource.Npc));
            Assert.AreEqual(QuestLogTab.Contracts, QuestSourceMapper.TabFor(QuestSource.Board));
            Assert.AreEqual(QuestLogTab.Contracts, QuestSourceMapper.TabFor(QuestSource.CaveContract));
            Assert.AreEqual(QuestLogTab.Secrets, QuestSourceMapper.TabFor(QuestSource.CaveSecret));
        }

        [Test]
        public void QuestSource_HasSixChannels_IncludingCaveContract()
        {
            // EMENDA 2026-06-12-B reserved the 6th value.
            var values = System.Enum.GetValues(typeof(QuestSource));
            Assert.AreEqual(6, values.Length);
            CollectionAssert.Contains(values, QuestSource.CaveContract);
        }

        // ─── Anti-regression: fixed quests still work in the existing flow ───────────

        [Test]
        public void FixedQuest_StillAcceptsAndTurnsIn_Unchanged()
        {
            var service = MakeService(out _, out var inv, out var gold, out _);
            // Smoke-test supply quest from the registry: CollectItem wood x2 + stone x2 → Gold 50.
            Assert.IsTrue(service.AcceptQuest(QuestRuntimeIds.SupplyQuestId));
            inv.Items["item_material_wood"] = 2;
            inv.Items["item_material_stone"] = 2;
            service.OnInventoryChanged("item_material_wood");
            service.OnInventoryChanged("item_material_stone");
            Assert.IsTrue(service.CanTurnIn(QuestRuntimeIds.SupplyQuestId));
            var result = service.TurnIn(QuestRuntimeIds.SupplyQuestId);
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(50, gold.Gold);
        }

        // ─── EMENDA F34-C: prerequisite gate (PREREQUISITE_UI_DEBT) ──────────────────

        [Test]
        public void Prerequisite_BlocksOfferUntilPrereqCompleted()
        {
            var service = MakeService(out _, out var inv, out var gold, out _);

            // quest_tools_for_the_town has PrerequisiteQuestIds = [quest_first_supplies_for_cindar].
            Assert.IsFalse(service.ArePrerequisitesComplete(QuestRuntimeIds.ToolsQuestId),
                "Offer should be gated before the prerequisite quest is completed.");

            // Complete the prerequisite (supply quest).
            Assert.IsTrue(service.AcceptQuest(QuestRuntimeIds.SupplyQuestId));
            inv.Items["item_material_wood"] = 2;
            inv.Items["item_material_stone"] = 2;
            service.OnInventoryChanged("item_material_wood");
            service.OnInventoryChanged("item_material_stone");
            Assert.IsTrue(service.TurnIn(QuestRuntimeIds.SupplyQuestId).Succeeded);

            // Now the gate opens.
            Assert.IsTrue(service.ArePrerequisitesComplete(QuestRuntimeIds.ToolsQuestId),
                "Offer should be allowed after the prerequisite quest is completed.");
        }

        [Test]
        public void Prerequisite_NoPrereq_AlwaysAllowed()
        {
            var service = MakeService(out _, out _, out _, out _);
            // Supply quest has no prerequisites.
            Assert.IsTrue(service.ArePrerequisitesComplete(QuestRuntimeIds.SupplyQuestId));
            // Unknown quest id — no gate to enforce.
            Assert.IsTrue(service.ArePrerequisitesComplete("quest_unknown_id"));
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        /// <summary>Mirrors QuestRuntimeBootstrap.CaptureSaveData (section → serializable DTO).</summary>
        private static QuestStateSectionSaveData CaptureSection(QuestStateSection section)
        {
            var dto = new QuestStateSectionSaveData
            {
                Version = section.Version,
                GlobalKnownHints = new List<string>(section.GlobalKnownHints),
                DiscoveredSecretQuestIds = new List<string>(section.DiscoveredSecretQuestIds),
                RewardedMainActIds = new List<string>(section.RewardedMainActIds),
                QuestStates = new List<QuestStateSaveData>()
            };
            foreach (var r in section.QuestStates)
            {
                var q = new QuestStateSaveData
                {
                    QuestId = r.QuestId,
                    State = r.State,
                    CurrentStepId = r.CurrentStepId,
                    KnownObjectiveIds = new List<string>(r.KnownObjectiveIds),
                    GrantedRewardIds = new List<string>(r.GrantedRewardIds),
                    GrantedFlagIds = new List<string>(r.GrantedFlagIds),
                    Tracked = r.Tracked,
                    Discovered = r.Discovered,
                    Source = r.Source,
                    IsDynamicInstance = r.IsDynamicInstance,
                    TemplateId = r.TemplateId,
                    InstanceTargetId = r.InstanceTargetId,
                    InstanceQuantity = r.InstanceQuantity,
                    QuestLevel = r.QuestLevel,
                    InstanceRewardGold = r.InstanceRewardGold,
                    InstanceRewardXp = r.InstanceRewardXp,
                    GeneratedForDay = r.GeneratedForDay,
                    ObjectiveStates = new List<QuestObjectiveStateSaveData>()
                };
                foreach (var o in r.ObjectiveStates)
                    q.ObjectiveStates.Add(new QuestObjectiveStateSaveData
                    {
                        ObjectiveId = o.ObjectiveId,
                        CurrentProgress = o.CurrentProgress,
                        RequiredProgress = o.RequiredProgress,
                        IsCompleted = o.IsCompleted,
                        IsFailed = o.IsFailed,
                        IsKnown = o.IsKnown
                    });
                dto.QuestStates.Add(q);
            }
            return dto;
        }
    }
}
