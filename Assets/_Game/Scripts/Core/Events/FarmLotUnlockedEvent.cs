namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_41 — publicado quando um lote de expansão da fazenda é destravado (Locked→Owned).
    /// Consumido por toast/feedback. Publicado UMA vez por destravamento (idempotência no serviço).
    /// </summary>
    public readonly struct FarmLotUnlockedEvent
    {
        public string LotId { get; }
        public string DisplayName { get; }

        public FarmLotUnlockedEvent(string lotId, string displayName)
        {
            LotId = lotId;
            DisplayName = displayName;
        }
    }
}
