namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — categoria de um elemento ambiental colocado deterministicamente no nível.
    /// Não bloqueante (decor visual), bloqueante (ocupa tile/colisão, nunca no caminho entrance↔exit),
    /// tile de lago (habilita criaturas aquáticas), ou nó minerável (reusa ResourceNode + snapshot de loot).
    /// </summary>
    public enum CaveEnvironmentElementKind
    {
        DecorNonBlocking = 0,
        DecorBlocking = 1,
        WaterTile = 2,
        MineableNode = 3
    }
}
