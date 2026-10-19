namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_49 — publicado quando o jogador APRENDE uma receita de tier alto (1×, via first-kill do
    /// boss de gate). Consumido pelo toast existente (PlayerActionFeedbackEvent) — nenhum evento
    /// existente muda. Idempotente na origem: re-kill não republica.
    /// </summary>
    public readonly struct RecipeLearnedEvent
    {
        public readonly string RecipeUnlockId; // estável: recipe_unlock_mithril_work, ...

        public RecipeLearnedEvent(string recipeUnlockId)
        {
            RecipeUnlockId = recipeUnlockId ?? string.Empty;
        }
    }
}
