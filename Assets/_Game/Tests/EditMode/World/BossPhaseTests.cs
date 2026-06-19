using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Runtime;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_05 — pure decision rules for cave boss phases (extracted from BossBrainController so they
    /// are EditMode-testable): phase resolution by HP threshold (CA-1), one-way progression (no
    /// regression on heal), and deterministic/idempotent add positions seeded by the cave run
    /// (CA-3 / ADR-0005 / cave_rules.md). No scene, no UnityEngine.Time.
    /// </summary>
    [TestFixture]
    public class BossPhaseTests
    {
        private static List<BossPhase> ThreePhases()
        {
            // Authored descending: 100% / 66% / 33% (as the controller caches them).
            return new List<BossPhase>
            {
                new BossPhase { HpThresholdPercent = 100f, ActionSetId = "phase_a" },
                new BossPhase { HpThresholdPercent = 66f, ActionSetId = "phase_b" },
                new BossPhase { HpThresholdPercent = 33f, ActionSetId = "phase_c" },
            };
        }

        // ── CA-1: phase resolution by HP threshold ──────────────────────────────────────────────

        [Test]
        public void ResolvePhase_FullHealth_IsFirstPhase()
        {
            Assert.AreEqual(0, BossPhaseLogic.ResolvePhaseIndex(ThreePhases(), 1.00f), "100% HP -> phase 0.");
            Assert.AreEqual(0, BossPhaseLogic.ResolvePhaseIndex(ThreePhases(), 0.67f), "Just above 66% -> still phase 0.");
        }

        [Test]
        public void ResolvePhase_CrossesAt66And33()
        {
            var phases = ThreePhases();
            Assert.AreEqual(1, BossPhaseLogic.ResolvePhaseIndex(phases, 0.66f), "Exactly 66% -> phase 1 (boundary is inclusive).");
            Assert.AreEqual(1, BossPhaseLogic.ResolvePhaseIndex(phases, 0.50f), "Between 33% and 66% -> phase 1.");
            Assert.AreEqual(2, BossPhaseLogic.ResolvePhaseIndex(phases, 0.33f), "Exactly 33% -> phase 2 (boundary inclusive).");
            Assert.AreEqual(2, BossPhaseLogic.ResolvePhaseIndex(phases, 0.10f), "Low HP -> final phase.");
            Assert.AreEqual(2, BossPhaseLogic.ResolvePhaseIndex(phases, 0.00f), "Dead-ish -> final phase.");
        }

        [Test]
        public void ResolvePhase_NoPhases_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, BossPhaseLogic.ResolvePhaseIndex(new List<BossPhase>(), 0.5f));
            Assert.AreEqual(-1, BossPhaseLogic.ResolvePhaseIndex(null, 0.5f));
        }

        [Test]
        public void ResolvePhase_ClampsHpFractionOutOfRange()
        {
            var phases = ThreePhases();
            Assert.AreEqual(0, BossPhaseLogic.ResolvePhaseIndex(phases, 2f), "HP fraction > 1 clamps to full -> phase 0.");
            Assert.AreEqual(2, BossPhaseLogic.ResolvePhaseIndex(phases, -1f), "Negative HP clamps to 0 -> final phase.");
        }

        // ── One-way progression (no phase regression on heal) ───────────────────────────────────

        [Test]
        public void ClampForwardOnly_NeverRegresses()
        {
            // Boss reached phase 2; HP heals back to a phase-1 fraction -> stays phase 2.
            Assert.AreEqual(2, BossPhaseLogic.ClampForwardOnly(2, 1), "Healed back into phase-1 band -> keep phase 2.");
            Assert.AreEqual(2, BossPhaseLogic.ClampForwardOnly(2, 0), "Full heal -> still keep phase 2.");
            Assert.AreEqual(2, BossPhaseLogic.ClampForwardOnly(1, 2), "Damage into phase 2 -> advance.");
            Assert.AreEqual(1, BossPhaseLogic.ClampForwardOnly(1, 1), "No change -> stay.");
        }

        [Test]
        public void ClampForwardOnly_IgnoresUnresolved()
        {
            Assert.AreEqual(1, BossPhaseLogic.ClampForwardOnly(1, -1), "Unresolved (-1) keeps the current phase.");
        }

        // ── CA-3: deterministic + idempotent add positions ──────────────────────────────────────

        private static List<Vector2Int> Grid5x5()
        {
            var tiles = new List<Vector2Int>();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    tiles.Add(new Vector2Int(x, y));
                }
            }
            return tiles;
        }

        [Test]
        public void AddTiles_AreDeterministicForSameSeed()
        {
            var boss = new Vector2Int(2, 2);
            var a = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "run", 15, "boss_x", 2, 2);
            var b = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "run", 15, "boss_x", 2, 2);

            Assert.AreEqual(2, a.Count, "Requested 2 adds, grid is large enough.");
            CollectionAssert.AreEqual(a, b, "Same seed/level/boss/phase -> identical tiles in identical order (ADR-0005).");
        }

        [Test]
        public void AddTiles_DifferForDifferentRunSeed()
        {
            var boss = new Vector2Int(2, 2);
            var run1 = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "runA", 15, "boss_x", 2, 3);
            var run2 = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "runB", 15, "boss_x", 2, 3);

            // Different run seeds should generally produce a different ordering; assert they are not
            // identical (extremely unlikely to collide across the whole 3-tile set with this hash).
            Assert.IsFalse(AreSameSequence(run1, run2), "Different run seeds -> different (still deterministic) tiles.");
        }

        [Test]
        public void AddTiles_DifferPerPhase_SameRun()
        {
            var boss = new Vector2Int(2, 2);
            var p1 = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "run", 15, "boss_x", 1, 3);
            var p2 = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "run", 15, "boss_x", 2, 3);
            Assert.IsFalse(AreSameSequence(p1, p2), "Different phases of the same boss spread to different tiles.");
        }

        [Test]
        public void AddTiles_NeverPlaceOnBossTile_AndNoDuplicates()
        {
            var boss = new Vector2Int(0, 0);
            var tiles = BossPhaseLogic.ResolveAddTiles(Grid5x5(), boss, "world", "run", 1, "boss_x", 0, 5);

            CollectionAssert.DoesNotContain(tiles, boss, "Adds never spawn on the boss tile.");
            CollectionAssert.AllItemsAreUnique(tiles, "No duplicate add tiles.");
        }

        [Test]
        public void AddTiles_RespectsCount_AndDegradesWhenTooFewCandidates()
        {
            var boss = new Vector2Int(9, 9);
            var small = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1) };

            Assert.AreEqual(2, BossPhaseLogic.ResolveAddTiles(small, boss, "w", "r", 1, "b", 0, 5).Count,
                "Only 2 candidate tiles -> at most 2 adds (graceful degrade).");
            Assert.AreEqual(1, BossPhaseLogic.ResolveAddTiles(small, boss, "w", "r", 1, "b", 0, 1).Count,
                "Requested 1 -> 1.");
            Assert.AreEqual(0, BossPhaseLogic.ResolveAddTiles(small, boss, "w", "r", 1, "b", 0, 0).Count,
                "Requested 0 -> none.");
            Assert.AreEqual(0, BossPhaseLogic.ResolveAddTiles(null, boss, "w", "r", 1, "b", 0, 3).Count,
                "Null candidate set -> none.");
        }

        [Test]
        public void AddSeedSource_FoldsAllStableInputs()
        {
            var s = BossPhaseLogic.BuildAddSeedSource("world", "run", 15, "boss_x", 2, 0);
            StringAssert.Contains("world", s);
            StringAssert.Contains("run", s);
            StringAssert.Contains("15", s);
            StringAssert.Contains("boss_x", s);
            StringAssert.Contains("boss_add", s);
        }

        private static bool AreSameSequence(List<Vector2Int> a, List<Vector2Int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
