namespace CindarsHope.Foundation
{
    /// <summary>
    /// Hash estavel FNV-1a 32-bit puro (sem engine nativa). Copia byte-identica de
    /// CaveEnemySpawnPlanner.StableHash / CaveLayoutStableHash.Compute, extraida para
    /// Foundation para que o modulo Enemy derive seeds deterministicos pelo MESMO
    /// algoritmo sem depender do modulo Cave (ADR-0005 / cave-stable-run).
    /// </summary>
    public static class StableHash32
    {
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
    }
}
