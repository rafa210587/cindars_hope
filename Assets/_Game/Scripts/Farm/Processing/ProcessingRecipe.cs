namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — receita de processamento físico da fazenda: transforma um insumo
    /// (goat_milk / grape) num output de valor agregado (goat_cheese / vale_wine) após
    /// N dias na estação correta. Definida em código no v1 (sem ScriptableObject asset).
    ///
    /// Apenas IDs estáveis e inteiros (compatível com a regra de save / ADR-0006 caso
    /// venha a ser serializada; aqui é dado de configuração puro).
    /// </summary>
    public sealed class ProcessingRecipe
    {
        public string RecipeId { get; }
        public string StationId { get; }
        public string InputItemId { get; }
        public int InputQuantity { get; }
        public string OutputItemId { get; }
        public int OutputQuantity { get; }
        public int ProcessingDays { get; }

        public ProcessingRecipe(
            string recipeId,
            string stationId,
            string inputItemId,
            int inputQuantity,
            string outputItemId,
            int outputQuantity,
            int processingDays)
        {
            RecipeId = recipeId;
            StationId = stationId;
            InputItemId = inputItemId;
            InputQuantity = inputQuantity < 1 ? 1 : inputQuantity;
            OutputItemId = outputItemId;
            OutputQuantity = outputQuantity < 1 ? 1 : outputQuantity;
            ProcessingDays = processingDays < 1 ? 1 : processingDays;
        }
    }
}
