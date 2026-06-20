namespace CindarsHope.Cave.Generation
{
    /// <summary>
    /// fable_09 — tipos de hazard de tile materializados na caverna. Visual placeholder por cor
    /// (telegraph de tile, CA-2). Semântica em CaveHazardTile.
    /// </summary>
    public enum CaveHazardKind
    {
        /// <summary>Poça tóxica: aplica Poison (F01) ao pisar; fallback dano direto se sem status.</summary>
        ToxicPool = 0,

        /// <summary>Gelo escorregadio: reduz atrito/controle por ~1.5s ao pisar.</summary>
        IceSlick = 1,

        /// <summary>Estalactite/queda de rocha: dano único telegrafado ao entrar no tile.</summary>
        FallingRock = 2
    }
}
