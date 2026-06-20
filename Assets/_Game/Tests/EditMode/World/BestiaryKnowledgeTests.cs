using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_21 — discovery-knowledge core (EnemyKnowledgeService). Covers CA-1..CA-5 plus the
    /// EMENDA-B skill-point milestones and EMENDA-D FullyDocumented reward / boss-defeat reveal.
    /// Pure EditMode: the service is plain C# and is fed synthetic events; bus publications are
    /// counted via real GameEventBus subscriptions.
    /// </summary>
    [TestFixture]
    public class BestiaryKnowledgeTests
    {
        private const string Slime = "enemy_slime_basic";

        private EnemyKnowledgeService NewService(System.Func<string, int> tierSource = null)
        {
            var svc = new EnemyKnowledgeService();
            if (tierSource != null)
            {
                svc.SpoilerTierSource = tierSource;
            }

            return svc;
        }

        // ── CA-1 thresholds ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Sighting_RevealsIdentity()
        {
            var svc = NewService();
            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.Identity));

            svc.RecordSighting(Slime);

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.Identity));
            Assert.AreEqual(EnemyKnowledgeLevel.Seen, svc.GetLevel(Slime));
        }

        [Test]
        public void FiveKills_RevealCommonDrops()
        {
            var svc = NewService();
            for (int i = 0; i < 4; i++) svc.RecordKill(Slime);
            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.DropsCommon));

            svc.RecordKill(Slime); // 5th

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.DropsCommon));
        }

        [Test]
        public void ThreeEffectiveFireHits_RevealVulnerability()
        {
            var svc = NewService();
            svc.RecordEffectiveHit(Slime, "fire");
            svc.RecordEffectiveHit(Slime, "fire");
            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ElementVulnerability));

            svc.RecordEffectiveHit(Slime, "fire"); // 3rd on the same axis

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ElementVulnerability));
        }

        [Test]
        public void EffectiveHitsOnDifferentAxes_DoNotRevealVulnerability()
        {
            var svc = NewService();
            svc.RecordEffectiveHit(Slime, "fire");
            svc.RecordEffectiveHit(Slime, "ice");
            svc.RecordEffectiveHit(Slime, "lightning");

            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ElementVulnerability),
                "3 hits across 3 axes must NOT reveal vulnerability — the threshold is 3 on a single axis.");
        }

        [Test]
        public void SeeingSameActionThreeTimes_RevealsBehavior()
        {
            var svc = NewService();
            svc.RecordActionSeen(Slime, "action_slam");
            svc.RecordActionSeen(Slime, "action_slam");
            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.BehaviorSummary));

            svc.RecordActionSeen(Slime, "action_slam"); // 3rd

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.BehaviorSummary));
        }

        [Test]
        public void SufferingActionOnce_RevealsBehavior()
        {
            var svc = NewService();
            svc.RecordActionSeen(Slime, "action_slam", suffered: true);

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.BehaviorSummary));
        }

        [Test]
        public void ThreeResistedHits_RevealResistance()
        {
            var svc = NewService();
            svc.RecordResistedHit(Slime);
            svc.RecordResistedHit(Slime);
            Assert.IsFalse(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ResistanceTags));

            svc.RecordResistedHit(Slime); // 3rd

            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ResistanceTags));
        }

        // ── CA-2 external grant idempotency ──────────────────────────────────────────────────────

        [Test]
        public void GrantKnowledge_UnlocksWithoutGrind()
        {
            var svc = NewService();
            bool first = svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.ResistanceTags, "thalindra");

            Assert.IsTrue(first);
            Assert.IsTrue(svc.IsUnlocked(Slime, BestiaryKnowledgeCategory.ResistanceTags));
        }

        [Test]
        public void GrantKnowledge_IsIdempotent_NoDuplicateUnlockEvent()
        {
            var svc = NewService();
            int events = 0;
            System.Action<BestiaryKnowledgeUnlockedEvent> handler = _ => events++;
            GameEventBus.Subscribe(handler);
            try
            {
                Assert.IsTrue(svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.DropsRare, "book"));
                Assert.IsFalse(svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.DropsRare, "book"),
                    "Second grant of the same category must be a no-op.");
                Assert.AreEqual(1, events, "Unlock event must publish exactly once.");
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
        }

        // ── CA-5 single unlock event ─────────────────────────────────────────────────────────────

        [Test]
        public void CrossingThreshold_PublishesUnlockEventOnce()
        {
            var svc = NewService();
            int identityEvents = 0;
            System.Action<BestiaryKnowledgeUnlockedEvent> handler = e =>
            {
                if (e.Category == BestiaryKnowledgeCategories.Identity) identityEvents++;
            };
            GameEventBus.Subscribe(handler);
            try
            {
                svc.RecordSighting(Slime);
                svc.RecordSighting(Slime);
                svc.RecordSighting(Slime);
                Assert.AreEqual(1, identityEvents, "Identity must unlock — and publish — exactly once.");
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
        }

        [Test]
        public void UnlockEvent_CarriesSpoilerTier()
        {
            var svc = NewService(id => 2);
            int tierSeen = -1;
            System.Action<BestiaryKnowledgeUnlockedEvent> handler = e => tierSeen = e.SpoilerTier;
            GameEventBus.Subscribe(handler);
            try
            {
                svc.RecordSighting(Slime);
                Assert.AreEqual(2, tierSeen);
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
        }

        // ── CA-3 spoiler gate ────────────────────────────────────────────────────────────────────

        [Test]
        public void LowTierCategories_AreVisibleWhenUnlocked()
        {
            var svc = NewService(id => 1);
            svc.RecordSighting(Slime);

            Assert.IsTrue(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity),
                "Tier 0-2 categories are visible as soon as unlocked.");
        }

        [Test]
        public void UnseenCategory_IsNeverVisible()
        {
            var svc = NewService(id => 0);
            Assert.IsFalse(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity),
                "A category that is not unlocked is never visible regardless of tier.");
        }

        [Test]
        public void Tier3Category_HiddenUntilQuestFlagSet()
        {
            var svc = NewService(id => 3);
            svc.SpoilerTierFlagSource = tier => "flag_gate";
            bool flagSet = false;
            svc.QuestFlagSource = flag => flagSet && flag == "flag_gate";

            svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.Identity, "defeat");
            Assert.IsFalse(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity, 3),
                "Tier 3 stays hidden until the gating flag is set.");

            flagSet = true;
            Assert.IsTrue(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity, 3),
                "Tier 3 becomes visible once the gating flag is set.");
        }

        [Test]
        public void Tier4Category_HiddenUntilQuestFlagSet()
        {
            var svc = NewService(id => 4);
            svc.SpoilerTierFlagSource = tier => "flag_four";
            bool flagSet = false;
            svc.QuestFlagSource = flag => flagSet && flag == "flag_four";

            svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.Identity, "defeat");
            Assert.IsFalse(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity, 4));

            flagSet = true;
            Assert.IsTrue(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity, 4));
        }

        [Test]
        public void Tier3Category_WithNoFlagConfigured_StaysHidden()
        {
            var svc = NewService(id => 3);
            svc.SpoilerTierFlagSource = tier => null; // no flag configured
            svc.GrantKnowledge(Slime, BestiaryKnowledgeCategory.Identity, "defeat");

            Assert.IsFalse(svc.IsVisible(Slime, BestiaryKnowledgeCategory.Identity, 3),
                "Fail-safe: a gated tier with no flag never leaks the spoiler.");
        }

        // ── EMENDA-D boss reveal + FullyDocumented reward ────────────────────────────────────────

        [Test]
        public void BossDefeat_RevealsFullFicha()
        {
            var svc = NewService();
            svc.IsBossSource = id => id == "enemy_boss_gate";

            svc.RecordKill("enemy_boss_gate");

            Assert.IsTrue(svc.IsUnlocked("enemy_boss_gate", BestiaryKnowledgeCategory.Identity));
            Assert.IsTrue(svc.IsUnlocked("enemy_boss_gate", BestiaryKnowledgeCategory.BehaviorSummary));
            Assert.IsTrue(svc.IsUnlocked("enemy_boss_gate", BestiaryKnowledgeCategory.ElementVulnerability));
            Assert.IsTrue(svc.IsUnlocked("enemy_boss_gate", BestiaryKnowledgeCategory.DropsCommon));
            Assert.AreEqual(EnemyKnowledgeLevel.Studied, svc.GetLevel("enemy_boss_gate"));
        }

        [Test]
        public void FullyDocumented_GrantsSmallDamageBonus()
        {
            var svc = NewService();
            Assert.AreEqual(1f, svc.GetDamageMultiplierVs(Slime), 1e-4f);

            FullyDocument(svc, Slime);

            Assert.IsTrue(svc.IsFullyDocumented(Slime));
            Assert.AreEqual(1f + EnemyKnowledgeService.FullyDocumentedDamageBonus,
                svc.GetDamageMultiplierVs(Slime), 1e-4f);
        }

        // ── EMENDA-B skill-point milestones ──────────────────────────────────────────────────────

        [Test]
        public void TenStudiedCreatures_GrantOneSkillPointMilestone()
        {
            var svc = NewService();
            int grantedPoints = 0;
            svc.SkillPointGrantSink = amount => { grantedPoints += amount; return true; };

            for (int i = 0; i < 10; i++) FullyDocument(svc, "enemy_common_" + i);

            Assert.AreEqual(1, svc.MilestonesGranted);
            Assert.AreEqual(1, grantedPoints);
        }

        [Test]
        public void Milestone_PublishesEventOnce()
        {
            var svc = NewService();
            int milestoneEvents = 0;
            System.Action<BestiaryMilestoneReachedEvent> handler = _ => milestoneEvents++;
            GameEventBus.Subscribe(handler);
            try
            {
                for (int i = 0; i < 10; i++) FullyDocument(svc, "enemy_m_" + i);
                Assert.AreEqual(1, milestoneEvents);
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
        }

        [Test]
        public void Milestones_CapAtFive()
        {
            var svc = NewService();
            int grantedPoints = 0;
            svc.SkillPointGrantSink = amount => { grantedPoints += amount; return true; };

            // 60 studied creatures would be 6 milestones, but the cap is 5.
            for (int i = 0; i < 60; i++) FullyDocument(svc, "enemy_cap_" + i);

            Assert.AreEqual(EnemyKnowledgeService.MaxMilestones, svc.MilestonesGranted);
            Assert.AreEqual(EnemyKnowledgeService.MaxMilestones, grantedPoints);
        }

        [Test]
        public void Milestone_NotReGrantedAfterSaveLoad()
        {
            var svc = NewService();
            int grantedPointsBefore = 0;
            svc.SkillPointGrantSink = amount => { grantedPointsBefore += amount; return true; };

            for (int i = 0; i < 10; i++) FullyDocument(svc, "enemy_p_" + i);
            Assert.AreEqual(1, svc.MilestonesGranted);
            Assert.AreEqual(1, grantedPointsBefore);

            // Round-trip through save into a fresh service.
            var entries = svc.CaptureKnowledge();
            int milestones = svc.CaptureMilestonesGranted();

            var reloaded = NewService();
            int grantedPointsAfter = 0;
            reloaded.SkillPointGrantSink = amount => { grantedPointsAfter += amount; return true; };
            reloaded.RestoreKnowledge(entries, milestones);

            Assert.AreEqual(1, reloaded.MilestonesGranted, "Milestone count must persist.");
            Assert.AreEqual(0, grantedPointsAfter,
                "Restoring a save must NOT re-grant the milestone skill point.");
        }

        // ── CA-4 persistence and compatibility ───────────────────────────────────────────────────

        [Test]
        public void SaveRoundTrip_PreservesCountersAndCategories()
        {
            var svc = NewService();
            svc.RecordSighting(Slime);
            svc.RecordKill(Slime);
            svc.RecordKill(Slime);
            svc.RecordEffectiveHit(Slime, "fire");
            svc.RecordEffectiveHit(Slime, "fire");
            svc.RecordEffectiveHit(Slime, "fire"); // unlocks vulnerability
            svc.RecordResistedHit(Slime);

            var entries = svc.CaptureKnowledge();
            int milestones = svc.CaptureMilestonesGranted();

            var reloaded = NewService();
            reloaded.RestoreKnowledge(entries, milestones);

            var state = reloaded.GetState(Slime);
            Assert.IsNotNull(state);
            Assert.AreEqual(2, state.Kills);
            Assert.AreEqual(3, state.EffectiveHits["fire"]);
            Assert.AreEqual(1, state.ResistedHits);
            Assert.IsTrue(reloaded.IsUnlocked(Slime, BestiaryKnowledgeCategory.Identity));
            Assert.IsTrue(reloaded.IsUnlocked(Slime, BestiaryKnowledgeCategory.ElementVulnerability));
        }

        [Test]
        public void LegacyLoad_NullKnowledge_YieldsEmptyCodex()
        {
            var svc = NewService();
            svc.RestoreKnowledge(null, 0); // legacy save without the knowledge field

            Assert.AreEqual(0, svc.StatesCount);
            Assert.AreEqual(0, svc.MilestonesGranted);
            Assert.AreEqual(EnemyKnowledgeLevel.Unknown, svc.GetLevel(Slime));
        }

        [Test]
        public void InvalidEnemyId_IsIgnored()
        {
            var svc = NewService();
            svc.RecordSighting(null);
            svc.RecordSighting(string.Empty);
            svc.RecordKill("   ");
            svc.GrantKnowledge(null, BestiaryKnowledgeCategory.Identity, "x");

            Assert.AreEqual(0, svc.StatesCount, "Null/blank ids must never create state.");
        }

        [Test]
        public void RestoreEntry_WithBlankId_IsSkipped()
        {
            var svc = NewService();
            var entries = new List<BestiaryKnowledgeEntrySaveData>
            {
                new BestiaryKnowledgeEntrySaveData { EnemyId = "   " },
                new BestiaryKnowledgeEntrySaveData { EnemyId = Slime, Kills = 3 }
            };
            svc.RestoreKnowledge(entries, 0);

            Assert.AreEqual(1, svc.StatesCount);
            Assert.AreEqual(3, svc.GetState(Slime).Kills);
        }

        // ── helpers ──────────────────────────────────────────────────────────────────────────────

        private static void FullyDocument(EnemyKnowledgeService svc, string enemyId)
        {
            // Drive the four core categories that define K4 Studied via the public surface.
            svc.RecordSighting(enemyId);                       // Identity
            svc.RecordActionSeen(enemyId, "act", suffered: true); // BehaviorSummary
            svc.RecordEffectiveHit(enemyId, "fire");
            svc.RecordEffectiveHit(enemyId, "fire");
            svc.RecordEffectiveHit(enemyId, "fire");           // ElementVulnerability
            for (int i = 0; i < EnemyKnowledgeService.KillsForCommonDrops; i++)
            {
                svc.RecordKill(enemyId);                       // DropsCommon (5 kills)
            }
        }
    }
}
