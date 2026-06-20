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
    /// fable_35 — the 12 NPC side-quest chains (sq_&lt;npc&gt;_&lt;n&gt;) (CA-1..CA-4).
    ///
    /// Pure/deterministic coverage: the catalog has 12 chains × 3 steps with canonical ids and no new
    /// objective type; the composite eligibility gate (previous step done + min friendship + act flag);
    /// chaining (step 2 invisible until step 1 done); the done/service-unlock flags granted idempotently;
    /// count and single-shot objective progression; 3 chains driven end-to-end (accept → complete → next
    /// → service unlock); and a dynamic-instance save round-trip. Everything rides the existing fable_34
    /// QuestService flow (no parallel quest system).
    /// </summary>
    [TestFixture]
    public class NpcChainsTests
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

        // Friendship probe: a simple per-NPC level dictionary the test drives directly.
        private readonly Dictionary<string, int> _friendship = new Dictionary<string, int>();
        private bool FriendshipAtLeast(string npcId, int level) =>
            (_friendship.TryGetValue(npcId, out var l) ? l : 0) >= level;

        private NpcQuestChainService MakeChainService(QuestService quest) =>
            new NpcQuestChainService(quest, _flagService, FriendshipAtLeast);

        // ─── CA-1: catalog invariants ──────────────────────────────────────────────

        [Test]
        public void Catalog_Has12Chains_36Steps_CanonicalIds()
        {
            Assert.AreEqual(12, NpcQuestChainCatalog.ChainCount);
            Assert.AreEqual(36, NpcQuestChainCatalog.AllSteps.Count);
            Assert.AreEqual(12, NpcQuestChainCatalog.ChainNpcIds.Count);

            foreach (var npcId in NpcQuestChainCatalog.ChainNpcIds)
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
        public void Catalog_NoSilentlyCutObjective_EveryStepMapsToExistingType()
        {
            foreach (var step in NpcQuestChainCatalog.AllSteps)
            {
                // No dormant steps in v1: every objective mapped to an existing type.
                Assert.IsFalse(step.Dormant, $"{step.QuestId} should not be dormant in v1");
                Assert.IsFalse(string.IsNullOrEmpty(step.TargetId), $"{step.QuestId} has a target");

                // The kind resolves to a real QuestObjectiveType (no new type introduced).
                var type = NpcQuestChainCatalog.ObjectiveTypeFor(step.ObjectiveKind);
                Assert.IsTrue(System.Enum.IsDefined(typeof(QuestObjectiveType), type),
                    $"{step.QuestId} maps to a defined QuestObjectiveType");
            }
        }

        [Test]
        public void Catalog_FinalStepsUnlockServices_ChainGatesAreAuthored()
        {
            // Each chain's step 3 grants exactly one service-unlock flag.
            foreach (var npcId in NpcQuestChainCatalog.ChainNpcIds)
            {
                var steps = NpcQuestChainCatalog.ForNpc(npcId);
                var final = steps[2];
                Assert.IsFalse(string.IsNullOrEmpty(final.ServiceUnlockFlagId),
                    $"{final.QuestId} unlocks a service");
            }

            // The two cited cross-act gates exist (Têmpera ← Brumdar-3 + Ato 1; Corvus-3 ties Act 4).
            Assert.AreEqual(NpcQuestChainCatalog.ActOneDoneFlag,
                NpcQuestChainCatalog.FindByQuestId("sq_brumdar_3").RequiredActFlagId);
            Assert.AreEqual(NpcQuestChainCatalog.ActThreeDoneFlag,
                NpcQuestChainCatalog.FindByQuestId("sq_corvus_3").RequiredActFlagId);
        }

        [Test]
        public void BuildInstance_ScalesRewards_AndCarriesDoneFlag()
        {
            var step = NpcQuestChainCatalog.FindByQuestId("sq_brumdar_1");
            var instance = NpcQuestChainCatalog.BuildInstance(step);

            Assert.AreEqual("sq_brumdar_1", instance.QuestId);
            Assert.AreEqual(QuestSource.Npc, instance.Source);
            Assert.AreEqual(step.ReferenceLevel, instance.QuestLevel);
            Assert.AreEqual(QuestRewardScaling.Scale(step.BaseGold, step.ReferenceLevel), instance.RewardGold);
            Assert.AreEqual(QuestRewardScaling.Scale(step.BaseXp, step.ReferenceLevel), instance.RewardXp);

            // The done flag is always attached (gates the next step).
            Assert.IsTrue(instance.AdditionalRewards.Exists(
                r => r.GrantedFlagId == NpcQuestChainCatalog.DoneFlag("sq_brumdar_1")));
        }

        // ─── CA-2: chaining + composite gate ───────────────────────────────────────

        [Test]
        public void Step2_IsNotOfferable_UntilStep1Done()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_ozzra"] = 5; // friendship is high; only the flag gate should block

            // Step 1 offerable; step 2 not (previous not done).
            Assert.IsTrue(chains.IsEligible("sq_ozzra_1"));
            Assert.IsFalse(chains.IsEligible("sq_ozzra_2"));

            // Complete step 1 → its done flag is set → step 2 becomes offerable.
            CompleteChainStep(quest, chains, "sq_ozzra_1");
            Assert.IsTrue(_flagService.IsSet(NpcQuestChainCatalog.DoneFlag("sq_ozzra_1")));
            Assert.IsTrue(chains.IsEligible("sq_ozzra_2"));
        }

        [Test]
        public void Step2_IsGatedByMinFriendship()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            CompleteChainStep(quest, chains, "sq_ozzra_1"); // sets done flag

            _friendship["npc_ozzra"] = 1; // below ChainProgressFriendshipMin (2)
            Assert.IsFalse(chains.IsEligible("sq_ozzra_2"), "low friendship blocks step 2");

            _friendship["npc_ozzra"] = NpcQuestChainCatalog.ChainProgressFriendshipMin;
            Assert.IsTrue(chains.IsEligible("sq_ozzra_2"), "min friendship unblocks step 2");
        }

        [Test]
        public void FinalStep_IsGatedByActFlag()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_brumdar"] = 5;
            CompleteChainStep(quest, chains, "sq_brumdar_1");
            CompleteChainStep(quest, chains, "sq_brumdar_2");

            // Step 3 needs act_1_done in addition to the done flag + friendship.
            Assert.IsFalse(chains.IsEligible("sq_brumdar_3"), "act gate blocks final step");

            _flagService.GrantFlag(NpcQuestChainCatalog.ActOneDoneFlag, "test");
            Assert.IsTrue(chains.IsEligible("sq_brumdar_3"), "act flag unblocks final step");
        }

        // ─── CA-3: rewards + service-unlock flag (idempotent) ───────────────────────

        [Test]
        public void FinalStep_UnlocksService_AndIsIdempotent()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_ozzra"] = 5;

            CompleteChainStep(quest, chains, "sq_ozzra_1");
            CompleteChainStep(quest, chains, "sq_ozzra_2");
            CompleteChainStep(quest, chains, "sq_ozzra_3");

            var final = NpcQuestChainCatalog.FindByQuestId("sq_ozzra_3");
            Assert.IsTrue(_flagService.IsSet(final.ServiceUnlockFlagId), "service unlocked by q3");

            // A repeat turn-in is a no-op (already completed): no double grant.
            var result = quest.TurnIn("sq_ozzra_3");
            Assert.IsTrue(result.WasAlreadyCompleted);
        }

        [Test]
        public void GoldAndXp_GrantedOnce_OnTurnIn()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            var step = NpcQuestChainCatalog.FindByQuestId("sq_dagna_1");

            CompleteChainStep(quest, chains, "sq_dagna_1");

            Assert.AreEqual(QuestRewardScaling.Scale(step.BaseGold, step.ReferenceLevel), _gold.Gold);
            Assert.AreEqual(QuestRewardScaling.Scale(step.BaseXp, step.ReferenceLevel), _prog.Xp);
        }

        // ─── CA-2/CA-4: objective progression ──────────────────────────────────────

        [Test]
        public void CountObjective_CompletesAfterRequiredAmount()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            _friendship["npc_eiran"] = 5;
            CompleteChainStep(quest, chains, "sq_eiran_1"); // reach hen
            chains.Offer("sq_eiran_2"); // defeat 5 grimfangs

            for (int i = 0; i < 4; i++) chains.OnEnemyDefeated("band_grimfang");
            Assert.AreNotEqual((int)QuestStateStatus.ReadyToComplete, quest.GetQuestState("sq_eiran_2").State);

            chains.OnEnemyDefeated("band_grimfang"); // 5th
            Assert.AreEqual((int)QuestStateStatus.ReadyToComplete, quest.GetQuestState("sq_eiran_2").State);
        }

        [Test]
        public void SingleShotObjective_CompletesOnMatch()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            chains.Offer("sq_zrix_1"); // reach: mark levels

            chains.OnLocationReached("cave_mark_levels");
            Assert.AreEqual((int)QuestStateStatus.ReadyToComplete, quest.GetQuestState("sq_zrix_1").State);
        }

        // ─── CA-4: 3 chains end-to-end ─────────────────────────────────────────────

        [Test]
        public void ThreeChains_EndToEnd_AcceptCompleteNextService()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);

            // Brumdar (act-gated final), Ozzra (plain), Tovin (talk-heavy).
            _friendship["npc_brumdar"] = 5;
            _friendship["npc_ozzra"] = 5;
            _friendship["npc_tovin"] = 5;
            _flagService.GrantFlag(NpcQuestChainCatalog.ActOneDoneFlag, "test");

            foreach (var npcId in new[] { "npc_brumdar", "npc_ozzra", "npc_tovin" })
            {
                var steps = NpcQuestChainCatalog.ForNpc(npcId);
                for (int i = 0; i < steps.Count; i++)
                {
                    // The chain offers exactly the next step at each point.
                    var offerable = chains.GetOfferableStep(npcId);
                    Assert.IsNotNull(offerable, $"{npcId} step {i + 1} should be offerable");
                    Assert.AreEqual(steps[i].QuestId, offerable.QuestId);
                    CompleteChainStep(quest, chains, steps[i].QuestId);
                }

                // Chain finished → no more offers, service flag set.
                Assert.IsNull(chains.GetOfferableStep(npcId), $"{npcId} chain complete");
                Assert.IsTrue(_flagService.IsSet(steps[2].ServiceUnlockFlagId), $"{npcId} service unlocked");
            }
        }

        // ─── Save round-trip ───────────────────────────────────────────────────────

        [Test]
        public void DynamicInstance_SurvivesSaveRoundTrip()
        {
            var quest = MakeService();
            var chains = MakeChainService(quest);
            chains.Offer("sq_brumdar_1");

            var saveData = CaptureSection(quest.GetSaveSection());

            // New service from a fresh registry; restore the persisted state.
            var registry2 = new QuestRegistry();
            var section2 = new QuestStateSection();
            var flag2 = new QuestFlagService(new QuestFlagRegistry());
            var quest2 = new QuestService(registry2, section2, _inv, _gold, flag2, _prog);
            quest2.RestoreFromSaveData(saveData);

            var restored = quest2.GetQuestState("sq_brumdar_1");
            Assert.IsNotNull(restored, "chain quest restored");
            Assert.IsTrue(restored.IsDynamicInstance);
            Assert.AreEqual((int)QuestStateStatus.Active, restored.State);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        // Offers the step, force-completes its single objective, and turns it in.
        private void CompleteChainStep(QuestService quest, NpcQuestChainService chains, string questId)
        {
            var accepted = chains.Offer(questId);
            Assert.AreEqual(questId, accepted, $"{questId} accepted");
            quest.MarkObjectiveComplete(questId, "obj_" + questId);
            var result = quest.TurnIn(questId);
            Assert.IsTrue(result.Succeeded, $"{questId} turned in: {result.FailureReason}");
        }

        // Mirrors QuestRuntimeBootstrap.CaptureSaveData (same as FestivalQuestsTests).
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
