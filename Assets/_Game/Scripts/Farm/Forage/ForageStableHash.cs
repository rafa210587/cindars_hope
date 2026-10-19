using System.Collections.Generic;

namespace CindarsHope.Farm.Forage
{
    /// <summary>
    /// fable_54 — hash estavel (FNV-1a 32-bit), MESMO algoritmo de CaveLayoutStableHash, replicado
    /// no namespace Farm para que Farm NAO dependa de Cave. Usado para a selecao determinista dos
    /// pontos de forrageio do dia (rng-and-determinism / ADR-0005 generalizado): sem GUID/timestamp/
    /// Random nao-semeado. Mesmo (seed, day) => mesma selecao apos reload.
    /// </summary>
    public static class ForageStableHash
    {
        public const string Salt = "fable_54_forage_daily_v1";

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
        /// Seleciona deterministicamente ate <paramref name="count"/> indices distintos de uma
        /// colecao de tamanho <paramref name="poolSize"/>, em funcao de (worldSeed, day).
        /// Estavel: mesma entrada => mesma saida (ordenada). Sem alocacao de RNG do Unity.
        /// </summary>
        public static List<int> SelectDailyIndices(string worldSeed, int day, int poolSize, int count)
        {
            var result = new List<int>();
            if (poolSize <= 0 || count <= 0)
            {
                return result;
            }

            if (count >= poolSize)
            {
                for (var i = 0; i < poolSize; i++) result.Add(i);
                return result;
            }

            // Ordena os indices por uma chave-hash estavel derivada de (salt, seed, day, index) e
            // pega os 'count' primeiros. Determinista e sem vies de modulo.
            var ranked = new List<KeyValuePair<long, int>>(poolSize);
            for (var i = 0; i < poolSize; i++)
            {
                var key = $"{Salt}|{worldSeed}|{day}|{i}";
                long h = (uint)Compute(key);
                ranked.Add(new KeyValuePair<long, int>(h, i));
            }

            ranked.Sort((a, b) =>
            {
                var c = a.Key.CompareTo(b.Key);
                return c != 0 ? c : a.Value.CompareTo(b.Value);
            });

            for (var i = 0; i < count; i++)
            {
                result.Add(ranked[i].Value);
            }

            result.Sort();
            return result;
        }
    }
}
