namespace CindarsHope.Cave.Generation
{
    /// <summary>
    /// fable_09 — hash estável (FNV-1a 32-bit) idêntico ao CaveEnemySpawnPlanner.StableHash e ao
    /// CaveSnapshotService.StableHash. Vive em Generation para que perfis/geração derivem tamanho,
    /// hazards e tesouro pelo MESMO algoritmo determinístico (ADR-0005 / cave-stable-run) sem que
    /// Generation dependa de Runtime. Nunca usar GUID/timestamp/Random não-semeado para conteúdo estável.
    /// </summary>
    public static class CaveLayoutStableHash
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
