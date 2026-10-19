namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_55 — publicado quando o output de um job de processamento de fazenda
    /// (queijaria/barril) é coletado com sucesso na estação física.
    /// IDs estáveis apenas (sem referências Unity), consumido por feedback/toast.
    /// </summary>
    public readonly struct ProcessingJobCompletedEvent
    {
        public string StationId { get; }
        public string RecipeId { get; }
        public string OutputItemId { get; }
        public int OutputQuantity { get; }

        public ProcessingJobCompletedEvent(string stationId, string recipeId, string outputItemId, int outputQuantity)
        {
            StationId = stationId ?? string.Empty;
            RecipeId = recipeId ?? string.Empty;
            OutputItemId = outputItemId ?? string.Empty;
            OutputQuantity = outputQuantity;
        }
    }
}
