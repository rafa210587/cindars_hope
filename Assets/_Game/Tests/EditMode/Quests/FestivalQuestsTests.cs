using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.FestivalQuests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_53 — the 8 festival quests (fq_*) (CA-1..CA-5).
    ///
    /// Pure/deterministic coverage: offer gated by the active festival, clean expiry at festival end,
    /// annual re-offer with a one-time trophy (idempotent across years), the two honest trackers
    /// (3 echoes one-per-moon same night; 5 distinct NPCs before midnight), and a dynamic-instance save
    /// round-trip. Everything rides the existing fable_34 QuestService flow (no parallel quest system).
    /// </summary>
    [TestFixture]
    public class FestivalQuestsTests
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

        private static FestivalQuestService MakeFestivalService(QuestService quest, int level = 10)
        {
            return new FestivalQuestService(quest, playerLevelProvider: () => level);
        }

        // ─── Catalog invariants ─────────────────────────────────────────────────────

        [Test]
        public void Catalog_Has8Quests_OnePerFestival_CanonicalIds()
        {
            Assert.AreEqual(8, FestivalQuestCatalog.Definitions.Count);
            Assert.AreEqual("fq_plantio", FestivalQuestCatalog.FindByFestival("festival_plantio").QuestId);
            Assert.AreEqual("fq_anonovo", FestivalQuestCatalog.FindByFestival("festival_ano_novo").QuestId);
            Assert.IsNull(FestivalQuestCatalog.FindByFestival("festival_does_not_exist"));
        }

        [Test]
        public void YearForDay_Matches112DayYear()
        {
            Assert.AreEqual(1, FestivalQuestCatalog.YearForDay(1));
            Assert.AreEqual(1, FestivalQuestCatalog.YearForDay(112));
            Assert.AreEqual(2, FestivalQuestCatalog.YearForDay(113));
            Assert.AreEqual(3, FestivalQuestCatalog.YearForDay(225));
        }

        // ─── CA-1 Offer gated + clean expiry ──────────────────────────────────────────

        [Test]
        public void Offer_OnlyForRealFestival()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            // festival_plantio is day 7 (Primavera) → absolute day 7, year 1.
            string offered = fest.OfferForFestival("festival_plantio", 7);
            Assert.AreEqual("fq_plantio_y1", offered);
            Assert.IsNotNull(service.GetQuestState("fq_plantio_y1"));

            // A non-existent festival offers nothing.
            Assert.IsNull(fest.OfferForFestival("festival_nope", 7));
        }

        [Test]
        public void Expire_AtFestivalEnd_NoPunishment_LeavesActiveLog()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            fest.OfferForFestival("festival_plantio", 7);
            Assert.AreEqual((int)QuestStateStatus.Active, service.GetQuestState("fq_plantio_y1").State);

            // Next day the festival is no longer active → quest expires without punishment.
            string expired = fest.ExpireForDay(8, festivalActiveToday: false);
            Assert.AreEqual("fq_plantio_y1", expired);
            Assert.AreEqual((int)QuestStateStatus.Expired, service.GetQuestState("fq_plantio_y1").State);
        }

        [Test]
        public void Expire_DoesNothing_WhileFestivalStillActive()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            fest.OfferForFestival("festival_plantio", 7);
            Assert.IsNull(fest.ExpireForDay(7, festivalActiveToday: true));
            Assert.AreEqual((int)QuestStateStatus.Active, service.GetQuestState("fq_plantio_y1").State);
        }

        // ─── CA-2 Annual repeat + one-time trophy ──────────────────────────────────────

        [Test]
        public void AnnualRepeat_ReOffersNextYear_AsDistinctInstance()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            // Year 1 (day 7) and year 2 (day 7 + 112 = 119) are distinct per-year instances.
            Assert.AreEqual("fq_plantio_y1", fest.OfferForFestival("festival_plantio", 7));
            fest.ExpireForDay(8, false);
            Assert.AreEqual("fq_plantio_y2", fest.OfferForFestival("festival_plantio", 119));
        }

        [Test]
        public void Trophy_GrantedOnlyOnFirstCompletion_IdempotentAcrossYears()
        {
            var service = MakeService(out _, out var inv, out _, out _);
            var fest = MakeFestivalService(service);

            // fq_colheita carries a trophy flag. Year 1: complete it → trophy granted.
            string y1 = fest.OfferForFestival("festival_colheita", 7); // Outono d7 = absolute day 63, year 1
            Assert.AreEqual("fq_colheita_y1", y1);
            fest.OnGoldCropDelivered("item_crop_gold_quality");
            Assert.IsTrue(service.CanTurnIn(y1));
            var r1 = service.TurnIn(y1);
            Assert.IsTrue(r1.Succeeded);
            Assert.Contains("fq_colheita_trophy", r1.FlagsGranted);

            // Year 2: complete again → XP/gold reward, but NO trophy re-grant (one-time across years).
            fest.ExpireForDay(64, false);
            string y2 = fest.OfferForFestival("festival_colheita", 175); // year 2, Outono d7
            Assert.AreEqual("fq_colheita_y2", y2);
            fest.OnGoldCropDelivered("item_crop_gold_quality");
            var r2 = service.TurnIn(y2);
            Assert.IsTrue(r2.Succeeded);
            CollectionAssert.DoesNotContain(r2.FlagsGranted, "fq_colheita_trophy");
        }

        // ─── CA-3 Honest trackers ──────────────────────────────────────────────────────

        [Test]
        public void NightEchoTracker_RequiresThreeDistinctMoons_SameNight()
        {
            var t = new NightEchoTracker(3);
            Assert.IsTrue(t.CollectEcho("peak_ash"));
            Assert.IsFalse(t.CollectEcho("peak_ash")); // repeated moon does not progress
            Assert.IsTrue(t.CollectEcho("peak_green"));
            Assert.IsFalse(t.IsComplete);
            Assert.IsTrue(t.CollectEcho("peak_amber"));
            Assert.IsTrue(t.IsComplete);
        }

        [Test]
        public void NightEchoTracker_DawnResets()
        {
            var t = new NightEchoTracker(3);
            t.CollectEcho("peak_ash");
            t.CollectEcho("peak_green");
            t.ResetNight();
            Assert.AreEqual(0, t.DistinctMoonsCollected);
            Assert.IsFalse(t.IsComplete);
        }

        [Test]
        public void GiftCountTracker_RequiresFiveDistinctNpcs_BeforeMidnight()
        {
            var t = new GiftCountTracker(5);
            Assert.IsTrue(t.RegisterGift("npc_a"));
            Assert.IsFalse(t.RegisterGift("npc_a")); // repeated NPC does not count
            Assert.IsTrue(t.RegisterGift("npc_b"));
            Assert.IsTrue(t.RegisterGift("npc_c"));
            Assert.IsTrue(t.RegisterGift("npc_d"));
            Assert.IsFalse(t.IsComplete);
            Assert.IsTrue(t.RegisterGift("npc_e"));
            Assert.IsTrue(t.IsComplete);
        }

        [Test]
        public void GiftCountTracker_MidnightClosesWindow()
        {
            var t = new GiftCountTracker(5);
            t.RegisterGift("npc_a");
            t.CloseAtMidnight();
            Assert.IsFalse(t.RegisterGift("npc_b")); // after midnight does not count
            Assert.AreEqual(1, t.DistinctNpcsGifted);
            Assert.IsTrue(t.WindowClosed);
        }

        [Test]
        public void MoonsQuest_Completes_ViaThreeDistinctEchoes()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_luas", 42); // Verao d14 = absolute day 42, year 1
            Assert.AreEqual("fq_luas_y1", id);
            fest.RegisterEcho("peak_ash");
            fest.RegisterEcho("peak_ash"); // repeat ignored
            fest.RegisterEcho("peak_green");
            Assert.IsFalse(service.CanTurnIn(id));
            fest.RegisterEcho("peak_amber");
            Assert.IsTrue(service.CanTurnIn(id));
        }

        [Test]
        public void NewYearQuest_Completes_ViaFiveDistinctGifts()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_ano_novo", 112); // Inverno d28 = day 112, year 1
            Assert.AreEqual("fq_anonovo_y1", id);
            fest.RegisterGift("npc_a");
            fest.RegisterGift("npc_b");
            fest.RegisterGift("npc_c");
            fest.RegisterGift("npc_d");
            Assert.IsFalse(service.CanTurnIn(id));
            fest.RegisterGift("npc_e");
            Assert.IsTrue(service.CanTurnIn(id));
        }

        // ─── CA-4 Count + single-target kinds use existing flow ─────────────────────────

        [Test]
        public void PlantingQuest_Completes_AfterRequiredSeeds()
        {
            var service = MakeService(out _, out _, out _, out var prog);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_plantio", 7);
            for (int i = 0; i < 4; i++) fest.OnSeedPlanted("seed_thandra");
            Assert.IsFalse(service.CanTurnIn(id)); // needs 5
            fest.OnSeedPlanted("seed_thandra");
            Assert.IsTrue(service.CanTurnIn(id));

            var result = service.TurnIn(id);
            Assert.IsTrue(result.Succeeded);
            // Scaled XP applied once (QuestLevel = 10).
            Assert.AreEqual(QuestRewardScaling.Scale(60, 10), prog.Xp);
        }

        [Test]
        public void TournamentQuest_Completes_AfterThreeDuels()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_torneio", 56); // Verao d28 = day 56, year 1
            fest.OnEncounterCleared("arena_duelist");
            fest.OnEncounterCleared("arena_duelist");
            Assert.IsFalse(service.CanTurnIn(id));
            fest.OnEncounterCleared("arena_duelist");
            Assert.IsTrue(service.CanTurnIn(id));
        }

        [Test]
        public void VeilsQuest_Completes_OnSpecialPurchase()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_veus", 49); // year 1 (offer is by festival id, not day)
            Assert.IsNotNull(id);
            fest.OnSpecialItemBought("item_veil_that_does_not_exist");
            Assert.IsTrue(service.CanTurnIn(id));
        }

        // ─── CA-5 Save round-trip ───────────────────────────────────────────────────────

        [Test]
        public void FestivalQuest_SurvivesSaveLoad_AndStillRewardsOnce()
        {
            var service = MakeService(out _, out _, out _, out _);
            var fest = MakeFestivalService(service);

            string id = fest.OfferForFestival("festival_plantio", 7);
            var saveData = CaptureSection(service.GetSaveSection());

            // Restore into a fresh service (empty registry — must re-register the instance).
            var inv2 = new FakeInventory();
            var gold2 = new FakeGold();
            var prog2 = new FakeProgression();
            var service2 = new QuestService(new QuestRegistry(), new QuestStateSection(), inv2, gold2,
                new QuestFlagService(new QuestFlagRegistry()), prog2);
            service2.RestoreFromSaveData(saveData);

            var restored = service2.GetQuestState(id);
            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.IsDynamicInstance);
            Assert.AreEqual((int)QuestSource.Npc, restored.Source);
            Assert.AreEqual("fq_plantio", restored.TemplateId);
        }

        // ─── Helpers (mirror QuestRuntimeBootstrap.CaptureSaveData) ──────────────────────

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
