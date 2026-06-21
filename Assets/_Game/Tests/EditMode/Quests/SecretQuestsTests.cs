using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;
using CindarsHope.Quests.SecretQuests;
using CindarsHope.Farm.Visitors;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_52 — the 8 cave-secret quests (scq_*) + the farm goblin visitor (CA-1..CA-5).
    ///
    /// Pure/deterministic coverage: discovery only via the fable_34 OfferSecretQuest channel; the 3
    /// merchant lists offered in sequence, 1x per run, stable per seed; Rare+ rarity gate; the honest
    /// pack duel (no pack death = truce, any pack death invalidates, deaths outside the window do not);
    /// world effects survive save/load and the goblin neutral band expires on a new run; deterministic
    /// goblin visit by day-seed with the post-truce dialogue variant. Everything rides the existing
    /// QuestService flow (no parallel quest system).
    /// </summary>
    [TestFixture]
    public class SecretQuestsTests
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

        private static QuestService MakeService(out QuestStateSection section, out QuestFlagRegistry flagRegistry,
            out QuestFlagService flagService, out FakeInventory inv)
        {
            var registry = new QuestRegistry();
            section = new QuestStateSection();
            inv = new FakeInventory();
            flagRegistry = new QuestFlagRegistry();
            SecretQuestFlagRegistration.RegisterAll(flagRegistry);
            flagService = new QuestFlagService(flagRegistry);
            return new QuestService(registry, section, inv, new FakeGold(), flagService, new FakeProgression());
        }

        private static SecretQuestService MakeSecretService(QuestService quest, QuestFlagService flags,
            QuestFlagRegistry registry, int level = 10)
        {
            var effects = new SecretQuestWorldEffects(flags, registry);
            return new SecretQuestService(quest, effects, () => level);
        }

        // ─── Catalog invariants ─────────────────────────────────────────────────────

        [Test]
        public void Catalog_Has8CanonicalIds()
        {
            Assert.AreEqual(8, SecretQuestCatalog.AllQuestIds.Count);
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_scrounger_bargain"));
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_merchant_list_2"));
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_warden_offering"));
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_goblin_truce"));
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_thrall_name"));
            Assert.IsTrue(SecretQuestCatalog.IsCanonical("scq_dragon_egg"));
            Assert.IsFalse(SecretQuestCatalog.IsCanonical("scq_not_a_quest"));
        }

        [Test]
        public void Build_AllCanonical_AreCaveSecretSource()
        {
            foreach (var id in SecretQuestCatalog.AllQuestIds)
            {
                var inst = SecretQuestCatalog.Build(id, caveLevel: 12, questLevel: 5);
                Assert.IsNotNull(inst, $"Build returned null for {id}");
                Assert.AreEqual(QuestSource.CaveSecret, inst.Source, $"{id} is not CaveSecret source");
                Assert.AreEqual(id, inst.QuestId);
            }
        }

        // ─── CA-1 Discovery only via the API ──────────────────────────────────────────

        [Test]
        public void Secret_NotDiscovered_UntilOffered()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);

            Assert.IsFalse(quest.IsSecretDiscovered(SecretQuestCatalog.WardenOfferingId),
                "Secret must be absent from the log before it is offered.");

            secret.Offer(SecretQuestCatalog.WardenOfferingId, caveLevel: 8);

            Assert.IsTrue(quest.IsSecretDiscovered(SecretQuestCatalog.WardenOfferingId),
                "Secret must be discovered after the offer.");
            Assert.IsNotNull(quest.GetQuestState(SecretQuestCatalog.WardenOfferingId),
                "Offer must accept the live instance so it can progress/turn-in.");
        }

        [Test]
        public void Offer_NonCanonical_IsRejected()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);
            Assert.IsNull(secret.Offer("scq_made_up", 1));
        }

        // ─── CA-2 Merchant lists in sequence, 1x/run, stable per seed ──────────────────

        [Test]
        public void MerchantLists_OfferedInSequence_OncePerRun()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);

            Assert.AreEqual(SecretQuestCatalog.MerchantList1Id, secret.OfferNextMerchantList("run_a", 3));
            Assert.AreEqual(SecretQuestCatalog.MerchantList2Id, secret.OfferNextMerchantList("run_a", 5));
            Assert.AreEqual(SecretQuestCatalog.MerchantList3Id, secret.OfferNextMerchantList("run_a", 7));
            Assert.IsNull(secret.OfferNextMerchantList("run_a", 9), "All 3 lists already offered this run.");
        }

        [Test]
        public void MerchantLists_ResetOnNewRun()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);

            secret.OfferNextMerchantList("run_a", 3);
            secret.OfferNextMerchantList("run_a", 4);
            // New run seed restarts the sequence from list 1.
            Assert.AreEqual(SecretQuestCatalog.MerchantList1Id, secret.OfferNextMerchantList("run_b", 3));
        }

        [Test]
        public void MerchantList_Completion_GrantsPermanentDiscount_NonStacking()
        {
            var quest = MakeService(out _, out var reg, out var flags, out var inv);
            var secret = MakeSecretService(quest, flags, reg);
            var effects = secret.WorldEffects;

            Assert.AreEqual(100, effects.ApplyMerchantDiscount(100), "No discount before completion.");

            // Offer + satisfy list 1 (5 glowcap), turn in.
            secret.OfferNextMerchantList("run_a", 3);
            inv.Items["item_material_glowcap"] = 5;
            quest.OnInventoryChanged("item_material_glowcap");
            var r1 = quest.TurnIn(SecretQuestCatalog.MerchantList1Id);
            Assert.IsTrue(r1.Succeeded, r1.FailureReason);

            Assert.IsTrue(effects.IsMerchantDiscountActive());
            Assert.AreEqual(90, effects.ApplyMerchantDiscount(100), "10% off after the discount flag.");

            // Completing a 2nd list does not deepen the discount (single flag, idempotent).
            secret.OfferNextMerchantList("run_a", 4);
            inv.Items["item_material_frost_core"] = 3;
            quest.OnInventoryChanged("item_material_frost_core");
            quest.TurnIn(SecretQuestCatalog.MerchantList2Id);
            Assert.AreEqual(90, effects.ApplyMerchantDiscount(100), "Discount must not stack.");
        }

        // ─── CA-1 Rare+ rarity gate (scq_scrounger_bargain) ────────────────────────────

        [Test]
        public void ScroungerBargain_RarityGate_OnlyRarePlusCounts()
        {
            int Lookup(string id)
            {
                switch (id)
                {
                    case "item_common": return 0;   // Common
                    case "item_uncommon": return 1; // Uncommon
                    case "item_rare": return 2;     // Rare
                    case "item_epic": return 3;     // Epic
                    default: return -1;             // unknown
                }
            }

            Assert.IsFalse(SecretQuestConditions.IsRarePlus("item_common", Lookup));
            Assert.IsFalse(SecretQuestConditions.IsRarePlus("item_uncommon", Lookup));
            Assert.IsTrue(SecretQuestConditions.IsRarePlus("item_rare", Lookup));
            Assert.IsTrue(SecretQuestConditions.IsRarePlus("item_epic", Lookup));
            Assert.IsFalse(SecretQuestConditions.IsRarePlus("item_unknown", Lookup));

            var bag = new[] { "item_common", "item_rare", "item_epic", "item_uncommon", "item_rare" };
            Assert.AreEqual(3, SecretQuestConditions.CountRarePlus(bag, Lookup, cap: 3));
        }

        // ─── CA-4 Honest duel ──────────────────────────────────────────────────────────

        [Test]
        public void Duel_NoPackDeath_EarnsTruce()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);
            secret.Offer(SecretQuestCatalog.GoblinTruceId, caveLevel: 10);

            secret.BeginWarchiefDuel("enc_1", new[] { "goblin_a", "goblin_b" });
            bool earned = secret.OnWarchiefDefeated("goblin_warchief");

            Assert.IsTrue(earned, "Defeating the warchief with the pack alive must earn the truce.");
            Assert.AreEqual((int)QuestStateStatus.ReadyToComplete,
                quest.GetQuestState(SecretQuestCatalog.GoblinTruceId).State);
        }

        [Test]
        public void Duel_PackDeath_Invalidates_NoSoftlock()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);
            secret.Offer(SecretQuestCatalog.GoblinTruceId, caveLevel: 10);

            secret.BeginWarchiefDuel("enc_1", new[] { "goblin_a", "goblin_b" });
            secret.OnEnemyDied("goblin_a"); // pack death dirties the duel
            bool earned = secret.OnWarchiefDefeated("goblin_warchief");

            Assert.IsFalse(earned, "A pack death must invalidate the truce.");
            // No softlock: still Active, re-offerable next run.
            Assert.AreEqual((int)QuestStateStatus.Active,
                quest.GetQuestState(SecretQuestCatalog.GoblinTruceId).State);
        }

        [Test]
        public void Duel_DeathOutsideWindow_DoesNotInvalidate()
        {
            var duel = new PackDuelTracker();
            duel.OnEnemyDied("goblin_a"); // before Begin — ignored
            duel.Begin("enc_1", new[] { "goblin_a" });
            duel.OnEnemyDied("some_other_enemy"); // not in the watched pack — ignored
            duel.OnWarchiefDefeated("warchief");
            Assert.IsTrue(duel.IsTruceEarned(), "Deaths outside the window/pack must not invalidate.");
        }

        // ─── CA-3 World effects persist; neutral band expires on new run ───────────────

        [Test]
        public void WardenPeaceful_SurvivesSaveLoad()
        {
            var quest = MakeService(out var section, out var reg, out var flags, out var inv);
            var secret = MakeSecretService(quest, flags, reg);

            secret.Offer(SecretQuestCatalog.WardenOfferingId, caveLevel: 8);
            inv.Items["item_agua_viva"] = 1;
            quest.OnInventoryChanged("item_agua_viva");
            Assert.IsTrue(quest.TurnIn(SecretQuestCatalog.WardenOfferingId).Succeeded);
            Assert.IsTrue(secret.WorldEffects.IsWardenPeacefulForever());

            // Simulate a reload: a fresh flag service rebuilt from the persisted GrantedFlagIds.
            var saveData = CaptureSection(section);
            var quest2 = MakeService(out _, out var reg2, out var flags2, out _);
            quest2.RestoreFromSaveData(saveData);
            var effects2 = new SecretQuestWorldEffects(flags2, reg2);

            var granted = new List<string>();
            foreach (var rec in quest2.GetSaveSection().QuestStates)
                granted.AddRange(rec.GrantedFlagIds);
            effects2.RehydrateFromGrantedFlags(granted);

            Assert.IsTrue(effects2.IsWardenPeacefulForever(),
                "warden_peaceful_forever must survive save/load.");
        }

        [Test]
        public void GoblinNeutralBand_ExpiresOnNewRun_PermanentFlagsDoNot()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);

            secret.Offer(SecretQuestCatalog.GoblinTruceId, caveLevel: 10);
            secret.BeginWarchiefDuel("enc_1", new[] { "g_a" });
            secret.OnWarchiefDefeated("warchief");
            Assert.IsTrue(quest.TurnIn(SecretQuestCatalog.GoblinTruceId).Succeeded);

            Assert.IsTrue(secret.WorldEffects.IsGoblinBandNeutralThisRun());

            secret.OnNewRunStarted("run_2");
            Assert.IsFalse(secret.WorldEffects.IsGoblinBandNeutralThisRun(),
                "Neutral band must expire on a new run.");
            Assert.IsTrue(flags.IsSet(SecretQuestCatalog.WarchiefCrestFlag),
                "Permanent crest flag must NOT be cleared by a new run.");
        }

        [Test]
        public void GoblinNeutralBand_NotRehydrated_AfterLoad()
        {
            var reg = new QuestFlagRegistry();
            SecretQuestFlagRegistration.RegisterAll(reg);
            var flags = new QuestFlagService(reg);
            var effects = new SecretQuestWorldEffects(flags, reg);

            // Even if the per-run flag is in the granted set, a reload starts neutral-free.
            effects.RehydrateFromGrantedFlags(new[]
            {
                SecretQuestCatalog.WardenPeacefulFlag,
                SecretQuestCatalog.GoblinBandNeutralFlag
            });

            Assert.IsTrue(effects.IsWardenPeacefulForever());
            Assert.IsFalse(effects.IsGoblinBandNeutralThisRun(), "Per-run band must not rehydrate.");
        }

        // ─── Dragon egg incubator ──────────────────────────────────────────────────────

        [Test]
        public void DragonEgg_HatchesAfterIncubationDays_Once()
        {
            var quest = MakeService(out _, out var reg, out var flags, out _);
            var secret = MakeSecretService(quest, flags, reg);
            secret.Offer(SecretQuestCatalog.DragonEggId, caveLevel: 30);

            secret.StartEggIncubation(currentDay: 10);
            Assert.IsFalse(secret.TickEggIncubation(10 + DragonEggIncubator.IncubationDays - 1),
                "Must not hatch before the incubation time.");
            Assert.IsTrue(secret.TickEggIncubation(10 + DragonEggIncubator.IncubationDays),
                "Must hatch once the time elapses.");
            Assert.IsFalse(secret.TickEggIncubation(10 + DragonEggIncubator.IncubationDays + 5),
                "Hatching is one-time.");
            Assert.IsTrue(secret.Incubator.IsHatched);
        }

        // ─── CA-5 Goblin visitor deterministic ─────────────────────────────────────────

        [Test]
        public void GoblinVisit_IsDeterministic_ForSameSeedAndDay()
        {
            for (int day = 1; day <= 200; day++)
            {
                bool a = GoblinFarmVisitor.ShouldVisit("world_x", day);
                bool b = GoblinFarmVisitor.ShouldVisit("world_x", day);
                Assert.AreEqual(a, b, $"Visit decision flipped between calls on day {day}.");
            }
        }

        [Test]
        public void GoblinVisit_RateIsNearConfiguredChance()
        {
            int visits = 0;
            const int samples = 4000;
            for (int day = 1; day <= samples; day++)
                if (GoblinFarmVisitor.ShouldVisit("world_y", day)) visits++;

            float rate = visits * 100f / samples;
            Assert.That(rate, Is.InRange(GoblinFarmVisitor.VisitChancePercent - 4f,
                                         GoblinFarmVisitor.VisitChancePercent + 4f),
                $"Observed visit rate {rate:F1}% too far from configured {GoblinFarmVisitor.VisitChancePercent}%.");
        }

        [Test]
        public void GoblinDialogue_PostTruceVariant_WhenTruceEarned()
        {
            Assert.AreEqual(GoblinFarmVisitor.DialogueVariant.Default,
                GoblinFarmVisitor.ResolveDialogueVariant(truceEarned: false, advancedActReached: false));
            Assert.AreEqual(GoblinFarmVisitor.DialogueVariant.AdvancedAct,
                GoblinFarmVisitor.ResolveDialogueVariant(truceEarned: false, advancedActReached: true));
            Assert.AreEqual(GoblinFarmVisitor.DialogueVariant.PostTruce,
                GoblinFarmVisitor.ResolveDialogueVariant(truceEarned: true, advancedActReached: true));
        }

        // ─── Anti-regression §13: secrets feed the main but are never required ──────────

        [Test]
        public void Secrets_FeedAct3_ViaFlags_ButAreOptional()
        {
            var quest = MakeService(out _, out var reg, out var flags, out var inv);
            var secret = MakeSecretService(quest, flags, reg);

            // Without ever touching a secret, the act-3 affluent flags are simply absent — the main
            // never blocks on them (they are consumed only if present).
            Assert.IsFalse(flags.IsSet(SecretQuestCatalog.NymirianEngravingFlag));
            Assert.IsFalse(flags.IsSet(SecretQuestCatalog.ThrallNameKnownFlag));

            // Completing the warden offering grants the engraving flag (a3 affluent).
            secret.Offer(SecretQuestCatalog.WardenOfferingId, 8);
            inv.Items["item_agua_viva"] = 1;
            quest.OnInventoryChanged("item_agua_viva");
            quest.TurnIn(SecretQuestCatalog.WardenOfferingId);
            Assert.IsTrue(flags.IsSet(SecretQuestCatalog.NymirianEngravingFlag));
        }

        // ─── Helper (mirrors QuestRuntimeBootstrap.CaptureSaveData) ──────────────────────

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
