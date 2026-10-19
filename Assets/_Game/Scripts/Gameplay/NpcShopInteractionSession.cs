namespace CindarsHope.NPC
{
    /// <summary>
    /// Pure interaction state machine. UI callbacks remain in the controller; legal lifecycle
    /// transitions live here so dialogue, shop and quest hand-off paths share one contract.
    /// </summary>
    public sealed class NpcShopInteractionSession
    {
        public bool IsInteracting { get; private set; }
        public bool IsClosing { get; private set; }
        public bool IsQuestOfferHandoff { get; private set; }

        public bool TryBegin()
        {
            if (IsInteracting) return false;
            IsInteracting = true;
            IsClosing = false;
            IsQuestOfferHandoff = false;
            return true;
        }

        public bool TryBeginClosing()
        {
            if (!IsInteracting || IsClosing) return false;
            IsClosing = true;
            return true;
        }

        public void MarkQuestOfferHandoff()
        {
            if (IsInteracting) IsQuestOfferHandoff = true;
        }

        public void Complete()
        {
            IsInteracting = false;
            IsClosing = false;
            IsQuestOfferHandoff = false;
        }
    }
}
