using System.Collections.Generic;

namespace CindarsHope.Quests
{
    /// <summary>
    /// fable_34 — stable hash (FNV-1a 32-bit), the SAME algorithm as CaveLayoutStableHash /
    /// ForageStableHash, replicated in the Quests namespace so Quests does not depend on Cave/Farm.
    ///
    /// Used for deterministic daily board rotation (rng-and-determinism / ADR-0005 generalized):
    /// no GUID/timestamp/Unity Random. Same (seed, day) => same contracts after reload, on every
    /// machine.
    /// </summary>
    public static class QuestStableHash
    {
        public const string BoardSalt = "fable_34_board_daily_v1";

        public static int Compute(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        /// <summary>
        /// Deterministically selects up to <paramref name="count"/> distinct indices of a pool of
        /// size <paramref name="poolSize"/>, as a function of (seed, day, salt). Stable and ordered;
        /// no modulo bias.
        /// </summary>
        public static List<int> SelectDailyIndices(string salt, string seed, int day, int poolSize, int count)
        {
            var result = new List<int>();
            if (poolSize <= 0 || count <= 0) return result;
            if (count >= poolSize)
            {
                for (var i = 0; i < poolSize; i++) result.Add(i);
                return result;
            }

            var ranked = new List<KeyValuePair<long, int>>(poolSize);
            for (var i = 0; i < poolSize; i++)
            {
                var key = $"{salt}|{seed}|{day}|{i}";
                long h = (uint)Compute(key);
                ranked.Add(new KeyValuePair<long, int>(h, i));
            }

            ranked.Sort((a, b) =>
            {
                var c = a.Key.CompareTo(b.Key);
                return c != 0 ? c : a.Value.CompareTo(b.Value);
            });

            for (var i = 0; i < count; i++) result.Add(ranked[i].Value);
            result.Sort();
            return result;
        }
    }
}
