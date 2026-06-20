using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.CaveContracts;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_51 — Zrix's cave contracts (CA-1..CA-5).
    ///
    /// Pure/deterministic coverage: weekly rotation determinism, rematch eligibility (only defeated
    /// gates), depth-milestone completion by synthetic cave event + idempotency, no-hit state machine
    /// (damage dirties; clean clear completes), and dynamic-instance save round-trip carrying the
    /// additive non-gold rewards. Everything rides the existing fable_34 QuestService flow.
    /// </summary>
    [TestFixture]
    public class CaveContractsTests
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

        private static QuestService MakeService(out QuestStateSection section, out FakeInventory inv,
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

        private static CaveContractService MakeContractService(QuestService quest,
            string seed = "world", int level = 10, int bandMin = 1, int bandMax = 5)
        {
            return new CaveContractService(
                quest,
                worldSeedProvider: () => seed,
                playerLevelProvider: () => level,
                playerBandProvider: () => (bandMin, bandMax));
        }

        // ─── CA-1 Source filter ─────────────────────────────────────────────────────

        [Test]
        public void Milestone_Instance_HasCaveContractSource_AndContractsTab()
        {
            var inst = CaveContractCatalog.BuildMilestoneInstance(15);
            Assert.AreEqual(QuestSource.CaveContract, inst.Source);
            Assert.AreEqual(QuestLogTab.Contracts, QuestSourceMapper.TabFor(inst.Source));
            Assert.AreEqual("cc_depth_15", inst.QuestId);
        }

        // ─── CA-2 Depth milestones 1× ───────────────────────────────────────────────

        [Test]
        public void Milestone_Completes_OnReachingDepth_ByCaveEvent()
        {
            var service = MakeService(out _, out _, out _, out var prog);
            var cave = MakeContractService(service);
            cave.OfferMilestones();

            // Accept the depth-15 milestone, then reach level 15 (synthetic).
            Assert.IsTrue(service.AcceptQuest(CaveContractCatalog.MilestoneId(15)));
            service.OnCaveLevelEntered(15);

            Assert.IsTrue(service.CanTurnIn(CaveContractCatalog.MilestoneId(15)));
            var result = service.TurnIn(CaveContractCatalog.MilestoneId(15));
            Assert.IsTrue(result.Succeeded);
            // Scaled XP applied once (QuestLevel = 15).
            Assert.AreEqual(QuestRewardScaling.Scale(CaveContractCatalog.MilestoneBaseXp(15), 15), prog.Xp);
            // map_segment flag granted.
            Assert.Contains(CaveContractCatalog.MapSegmentFlag(15), result.FlagsGranted);
        }

        [Test]
        public void Milestone_NotCompleted_BelowTargetDepth()
        {
            var service = MakeService(out _, out _, out _, out _);
            MakeContractService(service).OfferMilestones();

            Assert.IsTrue(service.AcceptQuest(CaveContractCatalog.MilestoneId(30)));
            service.OnCaveLevelEntered(10); // not deep enough
            Assert.IsFalse(service.CanTurnIn(CaveContractCatalog.MilestoneId(30)));
        }

        [Test]
        public void Milestone_RewardsOnlyOnce_NoDuplicateAfterReTurnIn()
        {
            var service = MakeService(out _, out _, out _, out var prog);
            MakeContractService(service).OfferMilestones();

            Assert.IsTrue(service.AcceptQuest(CaveContractCatalog.MilestoneId(5)));
            service.OnCaveLevelEntered(5);
            Assert.IsTrue(service.TurnIn(CaveContractCatalog.MilestoneId(5)).Succeeded);
            int xpAfterFirst = prog.Xp;

            // Re-turn-in is a no-op (already completed) — XP unchanged.
            var second = service.TurnIn(CaveContractCatalog.MilestoneId(5));
            Assert.IsTrue(second.WasAlreadyCompleted);
            Assert.AreEqual(xpAfterFirst, prog.Xp);
        }

        // ─── CA-3 Weekly rematch determinism + eligibility ──────────────────────────

        [Test]
        public void Rematch_SameWeekSameSeed_SameBoss()
        {
            var defeated = new List<string> { "gate_a", "gate_b", "gate_c" };
            var a = CaveContractCatalog.SelectRematchBoss("world", 3, defeated);
            var b = CaveContractCatalog.SelectRematchBoss("world", 3, defeated);
            Assert.AreEqual(a, b);
            CollectionAssert.Contains(defeated, a);
        }

        [Test]
        public void Rematch_OnlyAmongDefeatedGates()
        {
            var defeated = new List<string> { "gate_b" };
            // Whatever week, the only eligible boss is the single defeated one.
            for (int w = 0; w < 12; w++)
                Assert.AreEqual("gate_b", CaveContractCatalog.SelectRematchBoss("world", w, defeated));
        }

        [Test]
        public void Rematch_Unavailable_WhenNoGateDefeated()
        {
            Assert.IsNull(CaveContractCatalog.SelectRematchBoss("world", 1, new List<string>()));
            Assert.IsNull(CaveContractCatalog.BuildBossRematchInstance("world", 1, new List<string>(), 10));
        }

        [Test]
        public void Rematch_WeeksRotate_OverEligibleSet()
        {
            var defeated = new List<string> { "gate_a", "gate_b", "gate_c", "gate_d", "gate_e" };
            var seen = new HashSet<string>();
            for (int w = 0; w < 20; w++)
                seen.Add(CaveContractCatalog.SelectRematchBoss("world", w, defeated));
            // Over many weeks the selection visits more than one boss (it rotates, not stuck).
            Assert.Greater(seen.Count, 1, "Rematch selection did not rotate across weeks.");
        }

        [Test]
        public void RefreshWeek_NoGateDefeated_OffersOnlyNoHit()
        {
            var service = MakeService(out _, out _, out _, out _);
            var cave = MakeContractService(service);
            var offered = cave.RefreshWeek(0);
            // Only the no-hit weekly is offered (boss rematch unavailable without a defeated gate).
            Assert.AreEqual(1, offered.Count);
            Assert.AreEqual(CaveContractCatalog.NoHitFloorId, offered[0].QuestTemplateId);
        }

        [Test]
        public void RefreshWeek_WithDefeatedGate_OffersBothWeeklies()
        {
            var service = MakeService(out _, out _, out _, out _);
            var cave = MakeContractService(service);
            cave.RegisterDefeatedGateBoss("gate_a");
            var offered = cave.RefreshWeek(2);
            Assert.AreEqual(2, offered.Count);
        }

        [Test]
        public void RematchInstance_TurnIn_GrantsGuaranteedEssence()
        {
            var service = MakeService(out _, out var inv, out _, out _);
            var cave = MakeContractService(service);
            cave.RegisterDefeatedGateBoss("gate_a");
            var offered = cave.RefreshWeek(1);
            var rematch = offered.Find(o => o.QuestTemplateId == CaveContractCatalog.BossRematchId);
            Assert.IsNotNull(rematch);

            Assert.IsTrue(service.AcceptQuest(rematch.QuestId));
            service.OnEnemyKilled(rematch.TargetId); // defeat the gate boss band again
            Assert.IsTrue(service.CanTurnIn(rematch.QuestId));
            var result = service.TurnIn(rematch.QuestId);
            Assert.IsTrue(result.Succeeded);
            Assert.Contains(CaveContractCatalog.EssenceItemForBoss(rematch.TargetId), result.ItemsGiven);
        }

        // ─── CA-4 No-hit state machine ──────────────────────────────────────────────

        [Test]
        public void NoHit_CleanClear_ReturnsTrue()
        {
            var t = new NoHitFloorTracker();
            t.EnterLevel(7);
            Assert.IsTrue(t.EvaluateClear(7));
        }

        [Test]
        public void NoHit_DamageDirtiesLevel()
        {
            var t = new NoHitFloorTracker();
            t.EnterLevel(7);
            t.MarkDamaged();
            Assert.IsFalse(t.EvaluateClear(7));
        }

        [Test]
        public void NoHit_DamageBeforeEntry_DoesNotDirty()
        {
            var t = new NoHitFloorTracker();
            t.MarkDamaged();        // no current level — ignored
            t.EnterLevel(7);
            Assert.IsTrue(t.EvaluateClear(7));
        }

        [Test]
        public void NoHit_EvaluateWrongLevel_Fails()
        {
            var t = new NoHitFloorTracker();
            t.EnterLevel(7);
            Assert.IsFalse(t.EvaluateClear(8));
        }

        [Test]
        public void NoHit_Contract_Completes_OnCleanClear()
        {
            var service = MakeService(out _, out _, out _, out _);
            var cave = MakeContractService(service, bandMin: 7, bandMax: 7); // pin no-hit target to level 7
            var offered = cave.RefreshWeek(0);
            var noHit = offered.Find(o => o.QuestTemplateId == CaveContractCatalog.NoHitFloorId);
            Assert.IsNotNull(noHit);
            Assert.AreEqual("7", noHit.TargetId);

            Assert.IsTrue(service.AcceptQuest(noHit.QuestId));
            cave.OnLevelEntered(7);
            cave.OnLevelCleared(7); // clean
            Assert.IsTrue(service.CanTurnIn(noHit.QuestId));
            var result = service.TurnIn(noHit.QuestId);
            Assert.IsTrue(result.Succeeded);
            Assert.Contains(CaveContractCatalog.NoHitTitleFlag(7), result.FlagsGranted);
            Assert.Contains("item_accessory_charm_no_hit", result.ItemsGiven);
        }

        [Test]
        public void NoHit_Contract_NotCompleted_WhenDamaged()
        {
            var service = MakeService(out _, out _, out _, out _);
            var cave = MakeContractService(service, bandMin: 7, bandMax: 7);
            var offered = cave.RefreshWeek(0);
            var noHit = offered.Find(o => o.QuestTemplateId == CaveContractCatalog.NoHitFloorId);

            Assert.IsTrue(service.AcceptQuest(noHit.QuestId));
            cave.OnLevelEntered(7);
            cave.OnPlayerDamaged();
            cave.OnLevelCleared(7);
            Assert.IsFalse(service.CanTurnIn(noHit.QuestId));
        }

        // ─── CA-5 Save round-trip ───────────────────────────────────────────────────

        [Test]
        public void Milestone_SurvivesSaveLoad_AndStillRewardsOnce()
        {
            var service = MakeService(out _, out _, out _, out _);
            MakeContractService(service).OfferMilestones();
            Assert.IsTrue(service.AcceptQuest(CaveContractCatalog.MilestoneId(15)));

            var saveData = CaptureSection(service.GetSaveSection());

            // Restore into a fresh service (empty registry — must re-register the instance).
            var inv2 = new FakeInventory();
            var gold2 = new FakeGold();
            var prog2 = new FakeProgression();
            var service2 = new QuestService(new QuestRegistry(), new QuestStateSection(), inv2, gold2,
                new QuestFlagService(new QuestFlagRegistry()), prog2);
            service2.RestoreFromSaveData(saveData);

            var restored = service2.GetQuestState(CaveContractCatalog.MilestoneId(15));
            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.IsDynamicInstance);
            Assert.AreEqual((int)QuestSource.CaveContract, restored.Source);

            // Still resolves in the live flow after load: reach depth -> turn in -> scaled XP once.
            service2.OnCaveLevelEntered(15);
            Assert.IsTrue(service2.CanTurnIn(CaveContractCatalog.MilestoneId(15)));
            Assert.IsTrue(service2.TurnIn(CaveContractCatalog.MilestoneId(15)).Succeeded);
            Assert.AreEqual(QuestRewardScaling.Scale(CaveContractCatalog.MilestoneBaseXp(15), 15), prog2.Xp);
        }

        [Test]
        public void WeekForDay_MatchesCanonicalSevenDayWeek()
        {
            Assert.AreEqual(0, CaveContractService.WeekForDay(1));
            Assert.AreEqual(0, CaveContractService.WeekForDay(7));
            Assert.AreEqual(1, CaveContractService.WeekForDay(8));
            Assert.AreEqual(1, CaveContractService.WeekForDay(14));
            Assert.AreEqual(2, CaveContractService.WeekForDay(15));
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
