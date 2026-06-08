using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Dialogue;
using CindarsHope.City.FarmVisits;
using CindarsHope.City.Schedule;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class DialogueRumorFarmVisitTests
    {
        private DialogueResolver _resolver;
        private FarmVisitEligibilityResolver _farmResolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new DialogueResolver();
            _farmResolver = new FarmVisitEligibilityResolver();
        }

        private DialogueContext BasicCtx() => new DialogueContext
        {
            NpcId = "npc_liora", LocationId = "shop_seeds", CurrentPeriod = SchedulePeriod.WorkStart,
            RelationshipValue = 30, CityReputation = 50, Season = "Spring", LunarState = "Normal",
            StoryFlags = new List<string>(), ActiveQuestIds = new List<string>(), CompletedQuestIds = new List<string>()
        };

        // --- Dialogue resolver ---

        [Test]
        public void Dialogue_DefaultLine_WhenNoContextualMatch()
        {
            var set = new DialogueSetDefinition
            {
                DialogueSetId = "set_liora", NpcId = "npc_liora",
                DefaultLineTextKeys = new List<string> { "liora.default.greeting" }
            };
            var result = _resolver.Resolve(set, BasicCtx());
            Assert.IsTrue(result.Success);
            Assert.AreEqual("liora.default.greeting", result.SelectedTextKey);
            Assert.IsFalse(result.WasContextual);
        }

        [Test]
        public void Dialogue_ContextualLine_WhenConditionMet()
        {
            var ctx = BasicCtx();
            ctx.Season = "Winter";
            var set = new DialogueSetDefinition
            {
                NpcId = "npc_liora",
                DefaultLineTextKeys = new List<string> { "liora.default" },
                ContextualLines = new List<ConditionalDialogueLine>
                {
                    new ConditionalDialogueLine
                    {
                        LineId = "winter_line",
                        TextKey = "liora.winter.greeting",
                        Condition = new DialogueCondition { RequiredSeason = "Winter", Priority = 10 }
                    }
                }
            };
            var result = _resolver.Resolve(set, ctx);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("liora.winter.greeting", result.SelectedTextKey);
            Assert.IsTrue(result.WasContextual);
        }

        [Test]
        public void Dialogue_HigherPriorityLine_TakesPrecedence()
        {
            var ctx = BasicCtx();
            ctx.Season = "Spring";
            var set = new DialogueSetDefinition
            {
                NpcId = "npc_liora",
                DefaultLineTextKeys = new List<string> { "liora.default" },
                ContextualLines = new List<ConditionalDialogueLine>
                {
                    new ConditionalDialogueLine { LineId = "low", TextKey = "liora.low", Condition = new DialogueCondition { Priority = 1 } },
                    new ConditionalDialogueLine { LineId = "high", TextKey = "liora.high", Condition = new DialogueCondition { Priority = 10 } }
                }
            };
            var result = _resolver.Resolve(set, ctx);
            Assert.AreEqual("liora.high", result.SelectedTextKey);
        }

        [Test]
        public void Dialogue_Condition_RelationshipGate()
        {
            var ctx = BasicCtx();
            ctx.RelationshipValue = 20;
            var cond = new DialogueCondition { RequiredRelationshipMin = 25 };
            Assert.IsFalse(cond.IsMet(ctx));
            ctx.RelationshipValue = 25;
            Assert.IsTrue(cond.IsMet(ctx));
        }

        [Test]
        public void Dialogue_Condition_LunarGate_Nyx()
        {
            var ctx = BasicCtx();
            ctx.LunarState = "Nyx";
            var cond = new DialogueCondition { RequiredLunarState = "Nyx", SpoilerLevel = SpoilerLevel.Moderate };
            Assert.IsTrue(cond.IsMet(ctx));
        }

        [Test]
        public void Dialogue_Anya_LoreCriticalSpoilerLevel_RequiresFlag()
        {
            var ctx = BasicCtx();
            // Anya lore requires story flag
            var cond = new DialogueCondition
            {
                RequiredStoryFlags = new List<string> { "flag_anya_lore_unlocked" },
                SpoilerLevel = SpoilerLevel.LoreCritical
            };
            Assert.IsFalse(cond.IsMet(ctx), "Anya lore dialogue must be gated by story flag");
            ctx.StoryFlags.Add("flag_anya_lore_unlocked");
            Assert.IsTrue(cond.IsMet(ctx));
        }

        // --- Rumor pool ---

        [Test]
        public void Rumor_OnceOnly_FilteredAfterSeen()
        {
            var pool = new RumorPool
            {
                PoolId = "pool_general",
                Entries = new List<RumorEntry>
                {
                    new RumorEntry { RumorId = "rumor_cave_01", TextKey = "rumor.cave.01", OnceOnly = true }
                }
            };
            var seenOnce = new HashSet<string>();
            var available = pool.GetAvailable(BasicCtx(), seenOnce);
            Assert.AreEqual(1, available.Count);
            seenOnce.Add("rumor_cave_01");
            available = pool.GetAvailable(BasicCtx(), seenOnce);
            Assert.AreEqual(0, available.Count, "Once-only rumor must not appear after seen");
        }

        [Test]
        public void Rumor_NpcFilter_Respected()
        {
            var pool = new RumorPool
            {
                Entries = new List<RumorEntry>
                {
                    new RumorEntry { RumorId = "r1", NpcIdsAllowed = new List<string> { "npc_other" } }
                }
            };
            var available = pool.GetAvailable(BasicCtx(), new HashSet<string>());
            Assert.AreEqual(0, available.Count, "Rumor restricted to other NPC must not appear for npc_liora");
        }

        [Test]
        public void Rumor_ConditionGated_NotMetContext()
        {
            var pool = new RumorPool
            {
                Entries = new List<RumorEntry>
                {
                    new RumorEntry
                    {
                        RumorId = "r_spoiler",
                        RequiredCondition = new DialogueCondition { RequiredRelationshipMin = 75 }
                    }
                }
            };
            var available = pool.GetAvailable(BasicCtx(), new HashSet<string>());
            Assert.AreEqual(0, available.Count, "High-relationship rumor must not appear at relationship 30");
        }

        // --- Farm visit eligibility ---

        [Test]
        public void FarmVisit_Eligible_HappyPath()
        {
            var rule = new FarmVisitRule { FarmVisitRuleId = "visit_liora_social", NpcId = "npc_liora", RequiredRelationshipMin = 25, CooldownDays = 7 };
            var ctx = new FarmVisitContext { NpcId = "npc_liora", RelationshipValue = 30, CurrentDay = 10, LastVisitDay = 0, IsNpcAvailable = true };
            var result = _farmResolver.IsEligible(rule, ctx);
            Assert.IsTrue(result.Eligible);
        }

        [Test]
        public void FarmVisit_CooldownActive_NotEligible()
        {
            var rule = new FarmVisitRule { CooldownDays = 7, RequiredRelationshipMin = 0 };
            var ctx = new FarmVisitContext { RelationshipValue = 50, CurrentDay = 5, LastVisitDay = 3, IsNpcAvailable = true };
            var result = _farmResolver.IsEligible(rule, ctx);
            Assert.IsFalse(result.Eligible);
            Assert.IsTrue(result.BlockReason.Contains("Cooldown"));
        }

        [Test]
        public void FarmVisit_InsufficientRelationship_NotEligible()
        {
            var rule = new FarmVisitRule { RequiredRelationshipMin = 50, CooldownDays = 1 };
            var ctx = new FarmVisitContext { RelationshipValue = 30, CurrentDay = 10, LastVisitDay = 0, IsNpcAvailable = true };
            var result = _farmResolver.IsEligible(rule, ctx);
            Assert.IsFalse(result.Eligible);
        }

        [Test]
        public void FarmVisit_PetRelated_AlwaysDeferred()
        {
            var rule = new FarmVisitRule { IsPetRelated = true, RequiredRelationshipMin = 0, CooldownDays = 0 };
            var ctx = new FarmVisitContext { RelationshipValue = 100, CurrentDay = 10, IsNpcAvailable = true };
            var result = _farmResolver.IsEligible(rule, ctx);
            Assert.IsFalse(result.Eligible);
            Assert.IsTrue(result.BlockReason.Contains("Pet"));
        }

        [Test]
        public void FarmVisit_FestivalConflict_NotEligible()
        {
            var rule = new FarmVisitRule { RequiredRelationshipMin = 0, CooldownDays = 1 };
            var ctx = new FarmVisitContext { RelationshipValue = 50, CurrentDay = 10, IsNpcAvailable = true, IsConflictingFestivalActive = true };
            var result = _farmResolver.IsEligible(rule, ctx);
            Assert.IsFalse(result.Eligible);
        }
    }
}
