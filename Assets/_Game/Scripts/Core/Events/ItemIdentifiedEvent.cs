namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_31 — Publicado quando um item não-identificado é identificado (swap 1:1 concluído).
    /// Consumido por toast/UI ("Identificado: &lt;nome&gt;!"). Payload usa apenas tipos simples.
    /// </summary>
    public readonly struct ItemIdentifiedEvent
    {
        /// <summary>ID do item não-identificado consumido (ex.: item_unidentified_trinket).</summary>
        public string UnidentifiedItemId { get; }

        /// <summary>ID do item real revelado e adicionado ao inventário.</summary>
        public string RevealedItemId { get; }

        public ItemIdentifiedEvent(string unidentifiedItemId, string revealedItemId)
        {
            UnidentifiedItemId = unidentifiedItemId;
            RevealedItemId = revealedItemId;
        }
    }
}
