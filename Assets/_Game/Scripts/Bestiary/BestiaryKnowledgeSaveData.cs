using System.Collections.Generic;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_21 — save DTO for the discovery-knowledge layer, appended to the EXISTING
    /// <see cref="BestiarySaveData"/> (no second save path; WI-18 section ownership: BestiaryManager).
    /// Simple types and stable ids only — NO Unity references. Backward-compatible: a legacy save
    /// without these fields deserializes to empty lists / zero milestones, yielding an empty
    /// knowledge codex with no error.
    /// </summary>
    [System.Serializable]
    public class BestiaryKnowledgeEntrySaveData
    {
        public string EnemyId;
        public int Sightings;
        public int Kills;
        public float SecondsFought;
        public int ResistedHits;
        public bool SufferedAction;

        // Parallel key/value lists keep the DTO Unity-serializable (no Dictionary serialization).
        public List<string> ActionsSeenKeys = new List<string>();
        public List<int> ActionsSeenCounts = new List<int>();
        public List<string> EffectiveHitsKeys = new List<string>();
        public List<int> EffectiveHitsCounts = new List<int>();

        public List<string> UnlockedCategories = new List<string>();

        public BestiaryKnowledgeEntrySaveData() { }

        public BestiaryKnowledgeEntrySaveData(EnemyKnowledgeState state)
        {
            EnemyId = state?.EnemyId ?? string.Empty;
            if (state == null)
            {
                return;
            }

            Sightings = state.Sightings;
            Kills = state.Kills;
            SecondsFought = state.SecondsFought;
            ResistedHits = state.ResistedHits;
            SufferedAction = state.SufferedAction;

            if (state.ActionsSeen != null)
            {
                foreach (var pair in state.ActionsSeen)
                {
                    ActionsSeenKeys.Add(pair.Key);
                    ActionsSeenCounts.Add(pair.Value);
                }
            }

            if (state.EffectiveHits != null)
            {
                foreach (var pair in state.EffectiveHits)
                {
                    EffectiveHitsKeys.Add(pair.Key);
                    EffectiveHitsCounts.Add(pair.Value);
                }
            }

            if (state.UnlockedCategories != null)
            {
                UnlockedCategories = new List<string>(state.UnlockedCategories);
            }
        }

        public EnemyKnowledgeState ToState()
        {
            var state = new EnemyKnowledgeState
            {
                EnemyId = EnemyId ?? string.Empty,
                Sightings = System.Math.Max(0, Sightings),
                Kills = System.Math.Max(0, Kills),
                SecondsFought = SecondsFought < 0f ? 0f : SecondsFought,
                ResistedHits = System.Math.Max(0, ResistedHits),
                SufferedAction = SufferedAction
            };

            RebuildCounter(state.ActionsSeen, ActionsSeenKeys, ActionsSeenCounts);
            RebuildCounter(state.EffectiveHits, EffectiveHitsKeys, EffectiveHitsCounts);

            if (UnlockedCategories != null)
            {
                foreach (var category in UnlockedCategories)
                {
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        state.UnlockedCategories.Add(category);
                    }
                }
            }

            return state;
        }

        private static void RebuildCounter(Dictionary<string, int> target, List<string> keys, List<int> counts)
        {
            if (target == null || keys == null || counts == null)
            {
                return;
            }

            int count = System.Math.Min(keys.Count, counts.Count);
            for (int i = 0; i < count; i++)
            {
                var key = keys[i];
                if (!string.IsNullOrWhiteSpace(key))
                {
                    target[key] = System.Math.Max(0, counts[i]);
                }
            }
        }
    }
}
