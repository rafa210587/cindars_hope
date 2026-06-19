using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat.Bestiary;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_33 — deterministic per-band spawn composition derived from the canonical bestiary
    /// (CA-3). For a given cave level, the band's COMMON creatures whose faixa contains that level
    /// form a weight pool; the composition for N slots is chosen by hashing
    /// (CaveWorldSeed|CaveRunSeed|CaveLevel|slot) with the same FNV-1a used by CaveEnemySpawnPlanner,
    /// so the same seed always yields the same composition and revisits never reroll (stable-run,
    /// ADR-0005). Minibosses/bosses are NOT part of the regular weight pool (they are placed by the
    /// boss-gate / wandering-miniboss systems, not by per-slot rolls).
    ///
    /// Aquatic creatures are only eligible when the level has a lake; nocturnal eligibility is
    /// decided AT LEVEL GENERATION (passed in as <c>isNight</c>) so the stable-run beats the clock.
    ///
    /// This is a pure, deterministic helper (no Unity refs). The live materialization path still runs
    /// through EnemySpawnResolver/profiles; this table is the catalog-faithful, testable composition
    /// source the spec requires and the planner can consult.
    /// </summary>
    public static class CaveBandSpawnTable
    {
        /// <summary>A weighted spawn-pool entry for one creature eligible at a level.</summary>
        public struct WeightedEntry
        {
            public string EnemyId;
            public int Weight;
            public bool IsAquatic;
            public bool IsNocturnal;
            public bool IsNonAggressive;
        }

        // Commons get full weight; elites that are not minibosses are rarer in the regular pool.
        private const int CommonWeight = 10;
        private const int EliteWeight = 3;

        /// <summary>
        /// Build the eligible weight pool for a cave level, honoring band, faixa, lake and night flags.
        /// Minibosses/bosses are excluded. Deterministic ordering by EnemyId so the pool is stable.
        /// </summary>
        public static List<WeightedEntry> BuildPool(int caveLevel, bool hasLake, bool isNight)
        {
            var band = CaveBandScaling.BandForLevel(caveLevel);
            var pool = new List<WeightedEntry>();

            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                if (def.Band != band)
                {
                    continue;
                }

                if (def.IsBoss || def.IsMiniBoss)
                {
                    continue; // placed by gate/wandering systems, not per-slot rolls
                }

                if (caveLevel < def.MinLevel || caveLevel > def.MaxLevel)
                {
                    continue; // faixa gate
                }

                if (def.IsAquatic && !hasLake)
                {
                    continue; // aquatics need a lake on this level
                }

                if (def.IsNocturnal && !isNight)
                {
                    continue; // nocturnal spawn decided at level generation
                }

                pool.Add(new WeightedEntry
                {
                    EnemyId = def.EnemyId,
                    Weight = def.Role == Combat.EnemyRole.Elite ? EliteWeight : CommonWeight,
                    IsAquatic = def.IsAquatic,
                    IsNocturnal = def.IsNocturnal,
                    IsNonAggressive = def.IsNonAggressive,
                });
            }

            return pool.OrderBy(e => e.EnemyId, StringComparer.Ordinal).ToList();
        }

        /// <summary>
        /// Deterministic composition for <paramref name="slotCount"/> enemy slots at a level. The same
        /// (worldSeed, runSeed, caveLevel, slotCount, lake, night) inputs always return the identical
        /// list — this is the CA-3 contract. Returns enemy ids (may repeat), one per slot.
        /// </summary>
        public static List<string> ResolveComposition(
            string worldSeed,
            string runSeed,
            int caveLevel,
            int slotCount,
            bool hasLake,
            bool isNight)
        {
            var result = new List<string>();
            if (slotCount <= 0)
            {
                return result;
            }

            var pool = BuildPool(caveLevel, hasLake, isNight);
            if (pool.Count == 0)
            {
                return result;
            }

            var totalWeight = pool.Sum(e => e.Weight);
            if (totalWeight <= 0)
            {
                return result;
            }

            for (int slot = 0; slot < slotCount; slot++)
            {
                var hash = CaveEnemySpawnPlanner.StableHash(
                    $"{worldSeed}|{runSeed}|{caveLevel}|{slot}|band_spawn");
                var roll = (int)(((long)Math.Abs(hash)) % totalWeight);
                var cumulative = 0;
                var chosen = pool[pool.Count - 1].EnemyId;
                foreach (var entry in pool)
                {
                    cumulative += entry.Weight;
                    if (roll < cumulative)
                    {
                        chosen = entry.EnemyId;
                        break;
                    }
                }

                result.Add(chosen);
            }

            return result;
        }
    }
}
