namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// spec_cave_decor_composition_runtime (CV03) — contexto de célula usado para decidir ONDE e QUAL
    /// pool de arte um elemento de decor visual (<see cref="CaveEnvironmentElementKind.DecorNonBlocking"/>
    /// / <see cref="CaveEnvironmentElementKind.DecorBlocking"/>) deve ocupar. Substitui a colocação
    /// "1 elemento por célula andável aleatória" por colocação por CONTEXTO — teto, encostado em
    /// parede, ou chão aberto em cluster — para o decor ler como cena curada em vez de confete.
    ///
    /// Não afeta <see cref="CaveEnvironmentElementKind.WaterTile"/> nem
    /// <see cref="CaveEnvironmentElementKind.MineableNode"/> — esses seguem o caminho existente do
    /// fable_78 inalterado. Puramente de apresentação/composição (mesmo espírito de
    /// <see cref="CindarsHope.Cave.Art.CaveBiomeArtProfileSO"/>): nunca influencia
    /// layout/spawn/loot/snapshot (cave-stable-run / ADR-0005).
    /// </summary>
    public enum CaveDecorPlacementContext
    {
        /// <summary>Chão aberto, sem parede adjacente — decor de chão colocado em clusters (não singleton).</summary>
        FloorCluster = 0,

        /// <summary>Célula andável adjacente a pelo menos 1 WallTile — decor encostado em parede (carrinho, entulho, minério de parede).</summary>
        WallHug = 1,

        /// <summary>Célula de parede cujo vizinho ao sul é andável (borda superior de parede visível) — decor de teto (estalactites).</summary>
        CeilingHang = 2,

        /// <summary>spec_cave_visual_polish_runtime (CV04): célula de chão aberto (mesma elegibilidade de
        /// <see cref="FloorCluster"/>) que recebe cascalho/litter miúdo e denso — distinto de FloorCluster
        /// (props grandes, esparsos, em clusters). Nunca persistido/planejado pelo
        /// CaveEnvironmentElementPlanner: recalculado a cada materialização a partir do
        /// CaveTileMaterializer, puramente de apresentação (mesmo espírito do overlay de borda de parede).</summary>
        GroundScatter = 3,

        /// <summary>spec_cave_visual_polish_runtime (CV04): célula de PAREDE que encosta em chão em
        /// QUALQUER direção ortogonal (não só ao sul, ao contrário de <see cref="CeilingHang"/>) — recebe
        /// musgo/vegetação na base com baixa chance determinística. Também puramente de apresentação, não
        /// persistido/planejado pelo CaveEnvironmentElementPlanner.</summary>
        WallSurface = 4
    }
}
