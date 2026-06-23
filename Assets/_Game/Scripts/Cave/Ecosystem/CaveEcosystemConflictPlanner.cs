using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — decisor PURO e DETERMINÍSTICO do conflito inter-monstro (seção 14.5).
    ///
    /// Conflito é COMPORTAMENTO por visita, não composição (ADR-0018): NÃO altera quais inimigos
    /// existem, contagem, posições ou IDs. O roll é seedado por (worldSeed, runSeed, caveLevel,
    /// entryIndex) reusando <see cref="CaveLayoutStableHash"/> (FNV-1a) — reproduzível para um dado
    /// entryIndex, mas variando a cada entrada. Sem novo RNG, sem UnityEngine.Random, sem GUID/timestamp.
    ///
    /// Regras:
    /// - chance efetiva = ConflictChance se !hasHadConflictBefore, senão ReducedChance;
    /// - &lt;2 espécies distintas presentes → ConflictActive=false (sem conflito possível);
    /// - quando ativo escolhe 2 enemyIds DIFERENTES de forma determinística (nunca o mesmo nos dois lados).
    /// </summary>
    public static class CaveEcosystemConflictPlanner
    {
        private const int RollResolution = 1_000_000;

        public static CaveEcosystemConflictPlan Decide(
            string worldSeed,
            string runSeed,
            int caveLevel,
            int entryIndex,
            bool hasHadConflictBefore,
            IReadOnlyList<string> presentEnemyIds,
            CaveEcosystemBalanceSO balance)
        {
            if (balance == null)
            {
                return CaveEcosystemConflictPlan.Inactive();
            }

            var distinctSpecies = CollectDistinctSpecies(presentEnemyIds);
            if (distinctSpecies.Count < 2)
            {
                return CaveEcosystemConflictPlan.Inactive();
            }

            var chance = hasHadConflictBefore
                ? balance.InterMonsterConflictReducedChance
                : balance.InterMonsterConflictChance;

            if (!RollConflict(worldSeed, runSeed, caveLevel, entryIndex, chance))
            {
                return CaveEcosystemConflictPlan.Inactive();
            }

            var (factionA, factionB) = SelectTwoDistinctSpecies(
                worldSeed, runSeed, caveLevel, entryIndex, distinctSpecies);

            return CaveEcosystemConflictPlan.Active(factionA, factionB);
        }

        private static bool RollConflict(string worldSeed, string runSeed, int caveLevel, int entryIndex, float chance)
        {
            if (chance <= 0f)
            {
                return false;
            }

            if (chance >= 1f)
            {
                return true;
            }

            var seedSource = $"{worldSeed}|{runSeed}|{caveLevel}|{entryIndex}|conflict_roll";
            var hash = CaveLayoutStableHash.Compute(seedSource);
            // Roll determinístico em [0, RollResolution) → comparado contra o limiar da chance.
            var roll = (long)(hash & 0x7fffffff) % RollResolution;
            var threshold = (long)(chance * RollResolution);
            return roll < threshold;
        }

        private static (string factionA, string factionB) SelectTwoDistinctSpecies(
            string worldSeed,
            string runSeed,
            int caveLevel,
            int entryIndex,
            List<string> distinctSpecies)
        {
            // Ordenação determinística estável das espécies presentes por hash semeado pela entrada.
            distinctSpecies.Sort((a, b) =>
            {
                var ha = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{caveLevel}|{entryIndex}|species|{a}");
                var hb = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{caveLevel}|{entryIndex}|species|{b}");
                return ha != hb ? ha.CompareTo(hb) : string.CompareOrdinal(a, b);
            });

            // Os dois primeiros da ordenação são, por construção, espécies DIFERENTES.
            return (distinctSpecies[0], distinctSpecies[1]);
        }

        private static List<string> CollectDistinctSpecies(IReadOnlyList<string> presentEnemyIds)
        {
            var distinct = new List<string>();
            if (presentEnemyIds == null)
            {
                return distinct;
            }

            var seen = new HashSet<string>();
            foreach (var id in presentEnemyIds)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (seen.Add(id))
                {
                    distinct.Add(id);
                }
            }

            return distinct;
        }
    }
}
