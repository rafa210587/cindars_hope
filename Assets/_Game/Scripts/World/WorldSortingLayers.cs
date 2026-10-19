namespace CindarsHope.World
{
    /// <summary>
    /// Nomes das sorting layers do contrato de sorting Ground/World/Roof (FASE 1 de correção da
    /// cidade — ver ProjectSettings/TagManager.asset e GraphicsSettings.asset com
    /// m_TransparencySortMode Custom Axis). Const compartilhada para runtime fora do Editor (Cave
    /// tem sua própria cópia em CindarsHope.Cave.Runtime.CaveWorldSortingLayers por não referenciar
    /// CindarsHope.World) — evita literais soltos repetidos por materializer/spawner
    /// (rule id-stability / code-minimalism-ladder).
    /// </summary>
    public static class WorldSortingLayers
    {
        /// <summary>Chão, sem volume: tilemaps de grama, ruas, piso da praça, piso interior, água.
        /// Sem participação relevante no Y-sort de profundidade.</summary>
        public const string Ground = "Ground";

        /// <summary>Tudo com volume que participa da profundidade via Y-sort: player, NPCs,
        /// inimigos, árvores, props, muralha, corpos/paredes de casas, portas.</summary>
        public const string World = "World";

        /// <summary>Sempre acima do mundo: telhados, cumeeira, chaminé, fumaça, signs, toldos.</summary>
        public const string Roof = "Roof";
    }
}
