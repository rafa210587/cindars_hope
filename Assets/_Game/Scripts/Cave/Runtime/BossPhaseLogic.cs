using System.Collections.Generic;
using CindarsHope.Cave.Data;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_05 — pure, deterministic boss phase rules, extracted out of <c>BossBrainController</c>
    /// (a MonoBehaviour) so they get EditMode coverage (CA-1/CA-3): phase resolution from HP,
    /// one-way phase progression, and deterministic add positions seeded by the cave run
    /// (ADR-0005 / cave_rules.md). No UnityEngine.Time, no scene access — callers pass everything in.
    /// Reuses <see cref="CaveEnemySpawnPlanner.StableHash"/> so add seeding matches the rest of the
    /// cave stable-run determinism.
    /// </summary>
    public static class BossPhaseLogic
    {
        /// <summary>
        /// Resolve the active phase index from the current HP fraction (0..1). Phases are evaluated in
        /// descending threshold order: the active phase is the LAST phase whose threshold is still
        /// at or above the current HP% (i.e. the lowest-threshold phase the boss has already crossed
        /// into). Returns 0 (the first/full-HP phase) when nothing else matches. Returns -1 only when
        /// there are no phases at all.
        ///
        /// Example with thresholds [100, 66, 33]:
        ///   hp 1.00 -> phase 0; hp 0.66 -> phase 1; hp 0.50 -> phase 1; hp 0.33 -> phase 2; hp 0.10 -> phase 2.
        /// </summary>
        public static int ResolvePhaseIndex(IReadOnlyList<BossPhase> phasesDescending, float hpFraction)
        {
            if (phasesDescending == null || phasesDescending.Count == 0)
            {
                return -1;
            }

            float hpPercent = Mathf.Clamp01(hpFraction) * 100f;

            int active = 0;
            for (int i = 0; i < phasesDescending.Count; i++)
            {
                var phase = phasesDescending[i];
                if (phase == null)
                {
                    continue;
                }

                // HP has dropped to or below this phase's upper bound -> the boss is at least in this phase.
                // Because phases descend, the last match is the deepest (lowest-HP) phase reached.
                if (hpPercent <= phase.HpThresholdPercent + Mathf.Epsilon)
                {
                    active = i;
                }
            }

            return active;
        }

        /// <summary>
        /// One-way progression guard (skill: enemy-ai-authoring — "no phase regression unless spec says
        /// so"). Returns the index the boss should actually be in: never lower (earlier/higher-HP) than
        /// the phase it has already entered. If HP is restored (e.g. Vampiric/heal), the boss keeps its
        /// reached phase rather than regressing to an easier one.
        /// </summary>
        public static int ClampForwardOnly(int currentPhaseIndex, int resolvedPhaseIndex)
        {
            if (resolvedPhaseIndex < 0)
            {
                return currentPhaseIndex;
            }

            return Mathf.Max(currentPhaseIndex, resolvedPhaseIndex);
        }

        /// <summary>
        /// Build the deterministic seed string for a phase's adds. Same CaveRunSeed + level + bossId +
        /// phase + slot -> same seed on every revisit (ADR-0005). Mirrors the format used by
        /// <see cref="CaveEnemySpawnPlanner.BuildEnemyInstanceId"/>.
        /// </summary>
        public static string BuildAddSeedSource(
            string worldSeed,
            string runSeed,
            int caveLevel,
            string bossId,
            int phaseIndex,
            int addIndex)
        {
            return $"{worldSeed}|{runSeed}|{caveLevel}|{bossId}|boss_add|{phaseIndex}|{addIndex}";
        }

        /// <summary>
        /// Pick <paramref name="count"/> deterministic walkable tiles near the boss for phase adds. The
        /// candidate tiles are ordered by a stable hash of (seed source | tile) so the SAME tiles come
        /// out in the SAME order on every revisit (cave-stable-run / ADR-0005). No GUIDs, timestamps or
        /// unseeded Random. Tiles already equal to the boss tile are skipped; duplicates are avoided.
        /// Returns fewer than <paramref name="count"/> if the candidate set is too small.
        /// </summary>
        public static List<Vector2Int> ResolveAddTiles(
            IReadOnlyList<Vector2Int> candidateWalkableTiles,
            Vector2Int bossTile,
            string worldSeed,
            string runSeed,
            int caveLevel,
            string bossId,
            int phaseIndex,
            int count)
        {
            var result = new List<Vector2Int>();
            if (candidateWalkableTiles == null || candidateWalkableTiles.Count == 0 || count <= 0)
            {
                return result;
            }

            // Stable ordering: sort a copy of the candidates by a hash that folds in the phase seed so
            // different phases of the same boss spread to different (but still deterministic) tiles.
            string seedSource = BuildAddSeedSource(worldSeed, runSeed, caveLevel, bossId, phaseIndex, 0);
            var ordered = new List<Vector2Int>(candidateWalkableTiles);
            ordered.Sort((a, b) =>
            {
                int ha = CaveEnemySpawnPlanner.StableHash($"{seedSource}|{a.x}|{a.y}");
                int hb = CaveEnemySpawnPlanner.StableHash($"{seedSource}|{b.x}|{b.y}");
                int cmp = ha.CompareTo(hb);
                if (cmp != 0)
                {
                    return cmp;
                }

                // Tie-break deterministically on coordinates so equal hashes never depend on input order.
                cmp = a.x.CompareTo(b.x);
                return cmp != 0 ? cmp : a.y.CompareTo(b.y);
            });

            var used = new HashSet<Vector2Int>();
            foreach (var tile in ordered)
            {
                if (result.Count >= count)
                {
                    break;
                }

                if (tile == bossTile || used.Contains(tile))
                {
                    continue;
                }

                used.Add(tile);
                result.Add(tile);
            }

            return result;
        }
    }
}
