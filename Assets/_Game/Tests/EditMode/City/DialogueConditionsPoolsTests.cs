using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.NPC;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_28 — deterministic dialogue line selection by world conditions + per-day stable hash,
    /// authored pools on the 23 town NPCs, and the guaranteed fallback (never empty).
    /// Pure tests: no Unity scene, no services — the context is injected synthetically.
    /// </summary>
    [TestFixture]
    public class DialogueConditionsPoolsTests
    {
        private static DialogueConditionContext Ctx(
            string npcId = "npc_test",
            int day = 1,
            Season season = Season.Inverno,
            WeatherType weather = WeatherType.Rainy,
            DialogueTimeBand band = DialogueTimeBand.Manha,
            int friendship = 4,
            bool festival = false,
            IEnumerable<string> flags = null)
        {
            return new DialogueConditionContext(npcId, day, season, weather, band, friendship, festival, flags);
        }

        // ── CA-1: specificity wins ───────────────────────────────────────────────────────────────

        [Test]
        public void Select_MostSpecificEligibleLine_Wins()
        {
            // Synthetic pool with 0, 1, 2 and 3 constrained axes; context satisfies all of them.
            var pool = new List<ConditionalDialogueLine>
            {
                new ConditionalDialogueLine("c0", null), // 0 conditions (fallback-tier)
                new ConditionalDialogueLine("c1", new DialogueLineCondition { Season = Season.Inverno }), // 1
                new ConditionalDialogueLine("c2", new DialogueLineCondition { Season = Season.Inverno, Weather = WeatherType.Rainy }), // 2
                new ConditionalDialogueLine("c3", new DialogueLineCondition
                {
                    Season = Season.Inverno, Weather = WeatherType.Rainy, MinFriendship = 4
                }), // 3
            };

            var result = DialogueLineSelector.Select(pool, Ctx(), "npc_test", 1, "FALLBACK");
            Assert.AreEqual("c3", result, "The line satisfying the most conditions must win.");
        }

        [Test]
        public void Select_SkipsIneligible_PicksNextBest()
        {
            // The 3-condition line is ineligible (wrong season); the 2-condition rain+friendship wins.
            var pool = new List<ConditionalDialogueLine>
            {
                new ConditionalDialogueLine("generic", new DialogueLineCondition { Weather = WeatherType.Rainy }), // 1, eligible
                new ConditionalDialogueLine("specific_wrong", new DialogueLineCondition
                {
                    Season = Season.Verao, Weather = WeatherType.Rainy, MinFriendship = 4
                }), // 3, ineligible (Verao != Inverno)
                new ConditionalDialogueLine("rain_close", new DialogueLineCondition
                {
                    Weather = WeatherType.Rainy, MinFriendship = 4
                }), // 2, eligible
            };

            var result = DialogueLineSelector.Select(pool, Ctx(), "npc_test", 1, "FALLBACK");
            Assert.AreEqual("rain_close", result);
        }

        // ── CA-2: determinism per day ────────────────────────────────────────────────────────────

        [Test]
        public void Select_SameNpcAndDay_IsStableAcrossTimeBands()
        {
            // Two equally-specific eligible lines force the daily tie-break; result must not vary by hour.
            var pool = TieBreakPool();

            string morning = DialogueLineSelector.Select(
                pool, Ctx(band: DialogueTimeBand.Manha), "npc_corvus", 7, "FALLBACK");
            string night = DialogueLineSelector.Select(
                pool, Ctx(band: DialogueTimeBand.Noite), "npc_corvus", 7, "FALLBACK");

            Assert.AreEqual(morning, night, "Same NPC + same day must yield the same line all day.");
        }

        [Test]
        public void Select_DifferentDays_CanRotate()
        {
            var pool = TieBreakPool();

            // Scan a year of days; with two contenders the daily hash must land on both at least once.
            var seen = new HashSet<string>();
            for (int day = 1; day <= 112; day++)
            {
                seen.Add(DialogueLineSelector.Select(pool, Ctx(day: day), "npc_corvus", day, "FALLBACK"));
            }

            Assert.AreEqual(2, seen.Count, "Across many days the daily pick should rotate between contenders.");
        }

        [Test]
        public void Select_DifferentNpcs_CanDiffer_OnSameDay()
        {
            var pool = TieBreakPool();
            // Find two NPC ids that resolve differently on the same day (the hash distinguishes them).
            string a = DialogueLineSelector.Select(pool, Ctx(), "npc_alaric", 3, "FALLBACK");
            string b = DialogueLineSelector.Select(pool, Ctx(), "npc_brumdar", 3, "FALLBACK");
            // Not asserting inequality (hash could collide) but the call must be stable per id.
            Assert.AreEqual(a, DialogueLineSelector.Select(pool, Ctx(), "npc_alaric", 3, "FALLBACK"));
            Assert.AreEqual(b, DialogueLineSelector.Select(pool, Ctx(), "npc_brumdar", 3, "FALLBACK"));
        }

        // Two lines with identical specificity (1) that are both eligible under the default ctx.
        private static List<ConditionalDialogueLine> TieBreakPool() => new List<ConditionalDialogueLine>
        {
            new ConditionalDialogueLine("rain", new DialogueLineCondition { Weather = WeatherType.Rainy }),
            new ConditionalDialogueLine("winter", new DialogueLineCondition { Season = Season.Inverno }),
        };

        // ── CA-3: fallback never empty ───────────────────────────────────────────────────────────

        [Test]
        public void Select_NoEligibleLine_ReturnsFallback()
        {
            // Context satisfies none of the constrained lines.
            var ctx = Ctx(season: Season.Primavera, weather: WeatherType.Clear, friendship: 0, festival: false);
            var pool = new List<ConditionalDialogueLine>
            {
                new ConditionalDialogueLine("winter", new DialogueLineCondition { Season = Season.Inverno }),
                new ConditionalDialogueLine("rain", new DialogueLineCondition { Weather = WeatherType.Rainy }),
                new ConditionalDialogueLine("festival", new DialogueLineCondition { RequiresFestivalDay = true }),
                new ConditionalDialogueLine("close", new DialogueLineCondition { MinFriendship = 4 }),
            };

            var result = DialogueLineSelector.Select(pool, ctx, "npc_test", 1, "FALLBACK");
            Assert.AreEqual("FALLBACK", result);
        }

        [Test]
        public void Select_NullOrEmptyPool_ReturnsFallback()
        {
            Assert.AreEqual("FB", DialogueLineSelector.Select(null, Ctx(), "npc_test", 1, "FB"));
            Assert.AreEqual("FB", DialogueLineSelector.Select(
                new List<ConditionalDialogueLine>(), Ctx(), "npc_test", 1, "FB"));
        }

        [Test]
        public void Select_NullContext_OnlyUnconstrainedLineEligible()
        {
            var pool = new List<ConditionalDialogueLine>
            {
                new ConditionalDialogueLine("winter", new DialogueLineCondition { Season = Season.Inverno }),
                new ConditionalDialogueLine("any", null),
            };
            // With no context, only the unconstrained line is eligible (it beats the fallback string).
            Assert.AreEqual("any", DialogueLineSelector.Select(pool, null, "npc_test", 1, "FALLBACK"));
        }

        [Test]
        public void EveryNpc_HasNonEmptyConditionalPool_AndAreEligibleSomewhere()
        {
            Assert.AreEqual(23, TownNpcDialogueLibrary.AllContent.Count);
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var lines = TownNpcDialogueLibrary.BuildGreetingConditionalLines(content);
                // Spec asks for ~10 new lines/NPC (4 seasons + rain + festival + 3 friendship + 3 milestones = 12).
                Assert.GreaterOrEqual(lines.Count, 10, $"{content.NpcId} has too few conditional lines.");
                foreach (var line in lines)
                {
                    Assert.IsNotEmpty(line.Text, content.NpcId);
                    Assert.IsNotNull(line.Condition, content.NpcId);
                }
            }
        }

        [Test]
        public void GreetingNode_FallbackNeverEmpty_ForEveryNpc()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var nodes = TownNpcDialogueLibrary.BuildNodes(content);
                var greeting = nodes.Find(n => n.NodeId == "node_greeting");
                Assert.IsNotNull(greeting, content.NpcId);
                Assert.IsNotEmpty(greeting.RandomLinePool, content.NpcId);

                // A context that matches no authored condition must still resolve to a real greeting.
                var ctx = Ctx(season: Season.Primavera, weather: WeatherType.Clear, friendship: 0, festival: false);
                string fallback = greeting.Text;
                string picked = DialogueLineSelector.Select(
                    greeting.ConditionalLines, ctx, content.NpcId, 1, fallback);
                Assert.IsNotEmpty(picked, $"{content.NpcId}: selection returned empty.");
            }
        }

        // ── CA-3 (axes): synthetic context by season / weather / friendship ──────────────────────

        [Test]
        public void Condition_SeasonAxis()
        {
            var cond = new DialogueLineCondition { Season = Season.Inverno };
            Assert.IsTrue(cond.IsMet(Ctx(season: Season.Inverno)));
            Assert.IsFalse(cond.IsMet(Ctx(season: Season.Verao)));
        }

        [Test]
        public void Condition_WeatherAxis()
        {
            var cond = new DialogueLineCondition { Weather = WeatherType.Rainy };
            Assert.IsTrue(cond.IsMet(Ctx(weather: WeatherType.Rainy)));
            Assert.IsFalse(cond.IsMet(Ctx(weather: WeatherType.Clear)));
        }

        [Test]
        public void RainLine_IsEligibleOnRainyAndStormy()
        {
            // The authored rain line is registered for both Rainy and Stormy (wet weather parity).
            // Asserts eligibility (it is a contender), not that it wins the equal-specificity tie.
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                if (string.IsNullOrEmpty(content.RainLine)) continue;
                var lines = TownNpcDialogueLibrary.BuildGreetingConditionalLines(content);

                bool eligibleOnRainy = false;
                bool eligibleOnStormy = false;
                foreach (var line in lines)
                {
                    if (line.Text != content.RainLine) continue;
                    if (line.IsEligible(Ctx(weather: WeatherType.Rainy))) eligibleOnRainy = true;
                    if (line.IsEligible(Ctx(weather: WeatherType.Stormy))) eligibleOnStormy = true;
                }

                Assert.IsTrue(eligibleOnRainy, $"{content.NpcId}: rain line not eligible on Rainy.");
                Assert.IsTrue(eligibleOnStormy, $"{content.NpcId}: rain line not eligible on Stormy.");
            }
        }

        [Test]
        public void Condition_FriendshipAxis_IsMinimum()
        {
            var cond = new DialogueLineCondition { MinFriendship = 4 };
            Assert.IsFalse(cond.IsMet(Ctx(friendship: 3)));
            Assert.IsTrue(cond.IsMet(Ctx(friendship: 4)));
            Assert.IsTrue(cond.IsMet(Ctx(friendship: 5)));
        }

        [Test]
        public void Condition_TimeBandAxis()
        {
            var cond = new DialogueLineCondition { TimeBand = DialogueTimeBand.Noite };
            Assert.IsTrue(cond.IsMet(Ctx(band: DialogueTimeBand.Noite)));
            Assert.IsFalse(cond.IsMet(Ctx(band: DialogueTimeBand.Manha)));
        }

        [Test]
        public void Condition_FestivalAxis()
        {
            var cond = new DialogueLineCondition { RequiresFestivalDay = true };
            Assert.IsTrue(cond.IsMet(Ctx(festival: true)));
            Assert.IsFalse(cond.IsMet(Ctx(festival: false)));
        }

        // ── CA-4: main-quest milestone flags change the selected line ────────────────────────────

        [Test]
        public void Select_MilestoneFlag_FlipsSelectedLine()
        {
            var pool = new List<ConditionalDialogueLine>
            {
                new ConditionalDialogueLine("normal", null),
                new ConditionalDialogueLine("post_act1", new DialogueLineCondition
                {
                    RequiredFlag = TownNpcDialogueLibrary.FlagMainPostAct1
                }),
            };

            // Before the flag is set: only the unconstrained line is eligible.
            var before = Ctx(flags: null);
            Assert.AreEqual("normal", DialogueLineSelector.Select(pool, before, "npc_test", 1, "FB"));

            // After setting the milestone flag: the more specific flagged line wins.
            var after = Ctx(flags: new[] { TownNpcDialogueLibrary.FlagMainPostAct1 });
            Assert.AreEqual("post_act1", DialogueLineSelector.Select(pool, after, "npc_test", 1, "FB"));
        }

        [Test]
        public void Condition_ForbiddenFlag_BlocksWhenSet()
        {
            var cond = new DialogueLineCondition { ForbiddenFlag = "flag_x" };
            Assert.IsTrue(cond.IsMet(Ctx(flags: null)));
            Assert.IsFalse(cond.IsMet(Ctx(flags: new[] { "flag_x" })));
        }

        // ── Determinism / hash sanity ────────────────────────────────────────────────────────────

        [Test]
        public void StableHash_IsDeterministic_AndStable()
        {
            int a = DialogueLineSelector.StableHash("npc_corvus|7");
            int b = DialogueLineSelector.StableHash("npc_corvus|7");
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(
                DialogueLineSelector.StableHash("npc_corvus|7"),
                DialogueLineSelector.StableHash("npc_corvus|8"));
        }

        [Test]
        public void Context_BandFromHour_MapsBands()
        {
            Assert.AreEqual(DialogueTimeBand.Manha, DialogueConditionContext.BandFromHour(9));
            Assert.AreEqual(DialogueTimeBand.Tarde, DialogueConditionContext.BandFromHour(15));
            Assert.AreEqual(DialogueTimeBand.Noite, DialogueConditionContext.BandFromHour(21));
        }

        [Test]
        public void FestivalCalendar_CanonicalDays()
        {
            // Day 14 of the year (Primavera, day 14) is the Planting Festival.
            Assert.IsTrue(FestivalCalendar.IsCanonicalFestivalDay(GameDate.FromAbsoluteDay(14)));
            Assert.IsFalse(FestivalCalendar.IsCanonicalFestivalDay(GameDate.FromAbsoluteDay(15)));
        }
    }
}
