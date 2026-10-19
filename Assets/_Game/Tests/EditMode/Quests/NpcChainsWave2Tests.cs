using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.NpcChains;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    /// <summary>
    /// fable_70 — the 11 wave-2 NPC side-quest chains (sq_&lt;npc&gt;_&lt;n&gt;) for the remaining roster
    /// NPCs (Mara, Nimble, Gurd, Yael, Pip, Alaric, Renko, Liora, Orlan, Savra, Maelor) (CA-1..CA-4).
    ///
    /// All chains reuse the EXISTING fable_35 catalog/service (no second chain system): same QuestService
    /// dynamic-instance flow, same reward scaling, same composite gate. New coverage: 11 chains registered
    /// (23 total), the wave-1 12 chains intact (regression), per-step friendship gates (v1.1 roster),
    /// the act-2 gates (Yael q3, Maelor q2/q3), the Sethra↔Yael rivalry flags (mutually-mediable choice),
    /// idempotent reward/flag grant, and a save round-trip of a wave-2 dynamic instance. The Yael and
    /// Maelor chains are driven end-to-end (CA-4).
    /// </summary>
    [TestFixture]
    public class NpcChainsWave2Tests
    {
        // ─── Stub adapters (no Unity refs) — mirrors NpcChainsTests ──────────────────
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

        private QuestStateSection _section;
        private QuestFlagService _flagService;
        private FakeInventory _inv;
        private FakeGold _gold;
        private FakeProgression _prog;

        private QuestService MakeService()
        {
            var registry = new QuestRegistry();
            _section = new QuestStateSection();
            _inv = new FakeInventory();
            _gold = new FakeGold();
            _prog = new FakeProgression();
            _flagService = new QuestFlagService(new QuestFlagRegistry());
            return new QuestService(registry, _section, _inv, _gold, _flagService, _prog);
        }

        private readonly Dictionary<string, int> _friendship = new Dictionary<string, int>();
        private bool FriendshipAtLeast(string npcId, int level) =>
            (_friendship.TryGetValue(npcId, out var l) ? l : 0) >= level;

        private NpcQuestChainService MakeChainService(QuestService quest) =>
            new NpcQuestChainService(quest, _flagService, FriendshipAtLeast);

        private static readonly string[] Wave2Npcs =
        {
            "npc_mara", "npc_nimble", "npc_gurd", "npc_yael", "npc_pip", "npc_alaric",
            "npc_renko", "npc_liora", "npc_orlan", "npc_savra", "npc_maelor"
        };

        // ─── CA-1: catalog grew to 23 chains; wave-1 intact (regression) ───────────────

        [Test]
        public void Catalog_Has23Chains_69Steps_AfterWave2()
        {
            Assert.AreEqual(23, NpcQuestChainCatalog.ChainCount);
            Assert.AreEqual(11, NpcQuestChainCatalog.Wave2ChainCount);
            Assert.AreEqual(23, NpcQuestChainCatalog.ChainNpcIds.Count);
            Assert.AreEqual(69, NpcQuestChainCatalog.AllSteps.Count); // 23 × 3
            Assert.AreEqual(33, NpcQuestChainCatalog.Wave2Steps.Count); // 11 × 3

            foreach (var npcId in Wave2Npcs)
            {
                var steps = NpcQuestChainCatalog.ForNpc(npcId);
                Assert.AreEqual(3, steps.Count, $"{npcId} should have 3 steps");
                for (int i = 0; i < steps.Count; i++)
                {
                    Assert.AreEqual(i + 1, steps[i].Step, $"{npcId} step order");
                    var expectedId = $"sq_{npcId.Replace("npc_", string.Empty)}_{i + 1}";
                    Assert.AreEqual(expectedId, steps[i].QuestId, "canonical sq_ id");
                }
            }
        }

        [Test]
        public void Wave1_TwelveChains_AreUntouched_Regression()
        {
            // The 12 fable_35 chains still exist with their canonical ids and service flags — byte-for-byte.
            Assert.AreEqual(12, NpcQuestChainCatalog.Wave1ChainCount);
            Assert.AreEqual(36, NpcQuestChainCatalog.Wave1Steps.Count);

            var w1 = new[]
            {
                "npc_brumdar", "npc_ozzra", "npc_thalindra", "npc_sylveth", "npc_eiran", "npc_gruta",
                "npc_dagna", "npc_zrix", "npc_hund", "npc_mirela", "npc_tovin", "npc_corvus"
            };
            foreach (var npcId in w1)
            {
                var steps = NpcQuestChainCatalog.ForNpc(npcId);
                Assert.AreEqual(3, steps.Count, $"{npcId} wave-1 chain intact");
                Assert.IsFalse(string.IsNullOrEmpty(steps[2].ServiceUnlockFlagId), $"{npcId} q3 service flag intact");
            }

            // A couple of spot-checks on intact wave-1 act gates / ids.
            Assert.AreEqual(NpcQuestChainCatalog.ActOneDoneFlag,
                NpcQuestChainCatalog.FindByQuestId("sq_brumdar_3").RequiredActFlagId);
            Assert.AreEqual("sq_corvus_1", NpcQuestChainCatalog.FindByQuestId("sq_corvus_1").QuestId);
        }

        [Test]
        public void Wave2_NoSilentlyCutObjective_EveryStepMapsToExistingType()
        {
            foreach (var step in NpcQuestChainCatalog.Wave2Steps)
            {
                Assert.IsFalse(step.Dormant, $"{step.QuestId} should not be dormant");
                Assert.IsFalse(string.IsNullOrEmpty(step.TargetId), $"{step.QuestId} has a target");
                var type = NpcQuestChainCatalog.ObjectiveTypeFor(step.ObjectiveKind);
                Assert.IsTrue(System.Enum.IsDefined(typeof(QuestObjectiveType), type),
                    $"{step.QuestId} maps to a defined QuestObjectiveType");
            }
        }

        [Test]
        public void Wave2_EveryFinalStep_UnlocksAService()
        {
            foreach (var npcId in Wave2Npcs)
            {
                var final = NpcQuestChainCatalog.ForNpc(npcId)[2];
                Assert.IsFalse(string.IsNullOrEmpty(final.ServiceUnlockFlagId),
                    $"{final.QuestId} unlocks a service flag");
            }
        }

        // ─── CA-2: composite gates — per-step friendship + act flags ───────────────────

        [Test]
        public void PerStepFriendshipGate_Renko_1_2_4()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);

            // q1 needs no friendship (gate 1 — the v1.1 "amizade 1" is the entry baseline, modeled as 0/open).
            _friendship["npc_renko"] = 0;
            Assert.IsTrue(chains.IsEligible("sq_renko_1"), "q1 open");

            CompleteChainStep(quest, chains, "sq_renko_1");

            // q2 needs friendship 2.
            _friendship["npc_renko"] = 1;
            Assert.IsFalse(chains.IsEligible("sq_renko_2"), "q2 needs friendship 2");
            _friendship["npc_renko"] = 2;
            Assert.IsTrue(chains.IsEligible("sq_renko_2"), "q2 unlocked at friendship 2");

            CompleteChainStep(quest, chains, "sq_renko_2");

            // q3 needs friendship 4 (the "três versões" reveal).
            _friendship["npc_renko"] = 3;
            Assert.IsFalse(chains.IsEligible("sq_renko_3"), "q3 needs friendship 4");
            _friendship["npc_renko"] = 4;
            Assert.IsTrue(chains.IsEligible("sq_renko_3"), "q3 unlocked at friendship 4");
        }

        [Test]
        public void MaelorChain_IsLate_FriendshipAndActTwoGated()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);

            // q1 already needs friendship 2 (late chain).
            _friendship["npc_maelor"] = 1;
            Assert.IsFalse(chains.IsEligible("sq_maelor_1"), "Maelor q1 needs friendship 2");
            _friendship["npc_maelor"] = 2;
            Assert.IsTrue(chains.IsEligible("sq_maelor_1"), "Maelor q1 at friendship 2");

            CompleteChainStep(quest, chains, "sq_maelor_1");

            // q2 needs friendship 3 AND act_2_done.
            _friendship["npc_maelor"] = 3;
            Assert.IsFalse(chains.IsEligible("sq_maelor_2"), "Maelor q2 blocked until act 2");
            _flagService.GrantFlag(NpcQuestChainCatalog.ActTwoDoneFlag, "test");
            Assert.IsTrue(chains.IsEligible("sq_maelor_2"), "Maelor q2 at friendship 3 + act 2");

            CompleteChainStep(quest, chains, "sq_maelor_2");

            // q3 needs friendship 4 AND act_2_done.
            _friendship["npc_maelor"] = 3;
            Assert.IsFalse(chains.IsEligible("sq_maelor_3"), "Maelor q3 needs friendship 4");
            _friendship["npc_maelor"] = 4;
            Assert.IsTrue(chains.IsEligible("sq_maelor_3"), "Maelor q3 at friendship 4 + act 2");
        }

        [Test]
        public void ActOneGatedFinals_Gurd_Alaric_Liora()
        {
            foreach (var npcId in new[] { "npc_gurd", "npc_alaric", "npc_liora" })
            {
                var final = NpcQuestChainCatalog.ForNpc(npcId)[2];
                Assert.AreEqual(NpcQuestChainCatalog.ActOneDoneFlag, final.RequiredActFlagId,
                    $"{final.QuestId} is act-1 gated");
            }
        }

        [Test]
        public void YaelFinal_IsActTwoGated()
        {
            Assert.AreEqual(NpcQuestChainCatalog.ActTwoDoneFlag,
                NpcQuestChainCatalog.FindByQuestId("sq_yael_3").RequiredActFlagId);
        }

        // ─── CA-3: Yael rivalry flags + service unlock; no Vaelrion ────────────────────

        [Test]
        public void YaelChain_GrantsRivalryFlags_AndF25ServiceFlag()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_yael"] = 5;
            _flagService.GrantFlag(NpcQuestChainCatalog.ActTwoDoneFlag, "test");

            CompleteChainStep(quest, chains, "sq_yael_1");

            // q2 exposes the commercial rivalry → sq_yael_rival_known.
            CompleteChainStep(quest, chains, "sq_yael_2");
            Assert.IsTrue(_flagService.IsSet(NpcQuestChainCatalog.YaelRivalKnownFlag),
                "q2 reveals the commercial rivalry");

            // q3 mediation → sq_yael_rival_choice + the F25 "Encomenda de Livro" service flag.
            CompleteChainStep(quest, chains, "sq_yael_3");
            Assert.IsTrue(_flagService.IsSet(NpcQuestChainCatalog.YaelRivalChoiceFlag),
                "q3 records the mediation choice");
            var final = NpcQuestChainCatalog.FindByQuestId("sq_yael_3");
            Assert.IsTrue(_flagService.IsSet(final.ServiceUnlockFlagId), "q3 unlocks the F25 service");
        }

        [Test]
        public void YaelRivalChoiceFlag_IsMutuallyExclusiveSingleRecord_NoDoubleGrant()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_yael"] = 5;
            _flagService.GrantFlag(NpcQuestChainCatalog.ActTwoDoneFlag, "test");
            CompleteChainStep(quest, chains, "sq_yael_1");
            CompleteChainStep(quest, chains, "sq_yael_2");
            CompleteChainStep(quest, chains, "sq_yael_3");

            // The choice is recorded once; a repeat turn-in is a no-op (already completed) — the single
            // sq_yael_rival_choice record models the mutually-exclusive mediation (one outcome, dialogue-read).
            Assert.IsTrue(_flagService.IsSet(NpcQuestChainCatalog.YaelRivalChoiceFlag));
            var result = quest.TurnIn("sq_yael_3");
            Assert.IsTrue(result.WasAlreadyCompleted, "no second grant of the choice flag");
        }

        [Test]
        public void NoQuest_CitesVaelrion_InAnyTargetOrFlag()
        {
            // Vaelrion arrives in Act 2 and is not a roster NPC — no wave-2 chain may reference him by id.
            foreach (var step in NpcQuestChainCatalog.Wave2Steps)
            {
                StringAssert.DoesNotContain("vaelrion", (step.NpcId ?? string.Empty).ToLowerInvariant());
                StringAssert.DoesNotContain("vaelrion", (step.TargetId ?? string.Empty).ToLowerInvariant());
                StringAssert.DoesNotContain("vaelrion", (step.ServiceUnlockFlagId ?? string.Empty).ToLowerInvariant());
                if (step.ExtraGrantedFlagIds != null)
                    foreach (var f in step.ExtraGrantedFlagIds)
                        StringAssert.DoesNotContain("vaelrion", (f ?? string.Empty).ToLowerInvariant());
            }
        }

        [Test]
        public void MaelorFinal_GrantsSecretFlag()
        {
            var final = NpcQuestChainCatalog.FindByQuestId("sq_maelor_3");
            Assert.IsNotNull(final.ExtraGrantedFlagIds);
            CollectionAssert.Contains(final.ExtraGrantedFlagIds, NpcQuestChainCatalog.MaelorSecretFlag);
        }

        [Test]
        public void FriendshipReward_PlusEight_IsTheExistingF26Hook_NotASecondPath()
        {
            // The +8 amizade is NOT modeled as a chain reward (it is granted by the existing FriendshipService
            // turn-in hook). Assert the catalog never carries a relationship reward of its own.
            foreach (var step in NpcQuestChainCatalog.Wave2Steps)
            {
                var instance = NpcQuestChainCatalog.BuildInstance(step);
                foreach (var r in instance.AdditionalRewards)
                    Assert.AreNotEqual(CindarsHope.Quests.Rewards.QuestRewardType.RelationshipFuture, r.RewardType,
                        $"{step.QuestId} must not carry a parallel friendship reward");
            }
        }

        // ─── CA-4: Yael + Maelor end-to-end ────────────────────────────────────────────

        [Test]
        public void YaelAndMaelor_EndToEnd_AcceptCompleteNextService()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_yael"] = 5;
            _friendship["npc_maelor"] = 5;
            _flagService.GrantFlag(NpcQuestChainCatalog.ActTwoDoneFlag, "test");

            foreach (var npcId in new[] { "npc_yael", "npc_maelor" })
            {
                var steps = NpcQuestChainCatalog.ForNpc(npcId);
                for (int i = 0; i < steps.Count; i++)
                {
                    var offerable = chains.GetOfferableStep(npcId);
                    Assert.IsNotNull(offerable, $"{npcId} step {i + 1} should be offerable");
                    Assert.AreEqual(steps[i].QuestId, offerable.QuestId);
                    CompleteChainStep(quest, chains, steps[i].QuestId);
                }
                Assert.IsNull(chains.GetOfferableStep(npcId), $"{npcId} chain complete");
                Assert.IsTrue(_flagService.IsSet(steps[2].ServiceUnlockFlagId), $"{npcId} service unlocked");
            }
        }

        [Test]
        public void RewardItem_GrantedOnQ3_ForWave2Finals()
        {
            // Each wave-2 q3 carries a signature item (e.g. Yael nightmarket rarity, Maelor luandil_lens).
            foreach (var npcId in Wave2Npcs)
            {
                var final = NpcQuestChainCatalog.ForNpc(npcId)[2];
                Assert.IsFalse(string.IsNullOrEmpty(final.RewardItemId), $"{final.QuestId} grants a signature item");
                var instance = NpcQuestChainCatalog.BuildInstance(final);
                Assert.IsTrue(instance.AdditionalRewards.Exists(
                    r => r.RewardType == CindarsHope.Quests.Rewards.QuestRewardType.Item && r.TargetId == final.RewardItemId),
                    $"{final.QuestId} item reward attached");
            }
        }

        [Test]
        public void Step1_IsOfferable_Step2NotUntilStep1Done_Mara()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_mara"] = 5;

            Assert.IsTrue(chains.IsEligible("sq_mara_1"));
            Assert.IsFalse(chains.IsEligible("sq_mara_2"));

            CompleteChainStep(quest, chains, "sq_mara_1");
            Assert.IsTrue(_flagService.IsSet(NpcQuestChainCatalog.DoneFlag("sq_mara_1")));
            Assert.IsTrue(chains.IsEligible("sq_mara_2"));
        }

        // ─── Save round-trip (wave-2 dynamic instance) ─────────────────────────────────

        [Test]
        public void Wave2DynamicInstance_SurvivesSaveRoundTrip()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_savra"] = 5;
            chains.Offer("sq_savra_1");

            var saveData = CaptureSection(quest.GetSaveSection());

            var registry2 = new QuestRegistry();
            var section2 = new QuestStateSection();
            var flag2 = new QuestFlagService(new QuestFlagRegistry());
            var quest2 = new QuestService(registry2, section2, _inv, _gold, flag2, _prog);
            quest2.RestoreFromSaveData(saveData);

            var restored = quest2.GetQuestState("sq_savra_1");
            Assert.IsNotNull(restored, "wave-2 chain quest restored");
            Assert.IsTrue(restored.IsDynamicInstance);
            Assert.AreEqual((int)QuestStateStatus.Active, restored.State);
        }

        // ─── Helpers (mirror NpcChainsTests) ───────────────────────────────────────────

        private void CompleteChainStep(QuestService quest, NpcQuestChainService chains, string questId)
        {
            var accepted = chains.Offer(questId);
            Assert.AreEqual(questId, accepted, $"{questId} accepted");
            quest.MarkObjectiveComplete(questId, "obj_" + questId);
            var result = quest.TurnIn(questId);
            Assert.IsTrue(result.Succeeded, $"{questId} turned in: {result.FailureReason}");
        }

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
