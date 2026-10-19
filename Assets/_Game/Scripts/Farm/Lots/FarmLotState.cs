namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — estado de um lote de expansão.
    /// Locked: cercado, conteúdo interno inativo, placa "à venda".
    /// Owned: escritura usada — cerca removida, conteúdo ativo (idempotente; reload mantém 1×).
    /// </summary>
    public enum FarmLotState
    {
        Locked = 0,
        Owned = 1
    }
}
