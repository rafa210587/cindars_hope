namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Nomes das sorting layers usadas pelo conteúdo procedural da cave (contrato de sorting da
    /// FASE 1 de correção da cidade — mesma taxonomia Ground/World/Roof do TagManager). Const
    /// compartilhada evita literais soltos repetidos por materializer (rule id-stability /
    /// code-minimalism-ladder).
    /// </summary>
    internal static class CaveWorldSortingLayers
    {
        /// <summary>Chão, sem volume: piso da cave. Sem participação no Y-sort de profundidade.</summary>
        internal const string Ground = "Ground";

        /// <summary>Tudo com volume que participa da profundidade via Y-sort: paredes, player,
        /// inimigos, boss, hazards, recursos, saídas, mercador.</summary>
        internal const string World = "World";
    }
}
