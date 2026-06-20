namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_12 — publicado quando o jogador coleta um produto de um animal (ovo/leite).
    /// Consumido por feedback de HUD e, futuramente, por metas diárias de pecuária.
    /// Apenas IDs/strings simples — sem referências Unity.
    /// </summary>
    public readonly struct AnimalProductCollectedEvent
    {
        public string AnimalInstanceId { get; }
        public string AnimalDataId { get; }
        public string ProductItemId { get; }
        public int Quantity { get; }

        public AnimalProductCollectedEvent(string animalInstanceId, string animalDataId, string productItemId, int quantity)
        {
            AnimalInstanceId = animalInstanceId ?? string.Empty;
            AnimalDataId = animalDataId ?? string.Empty;
            ProductItemId = productItemId ?? string.Empty;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// fable_12 — publicado quando um animal de fazenda muda de estado de saúde por negligência
    /// (saudável → faminto → doente/crítico → morto). Usado para avisos progressivos ao jogador.
    /// EMENDA 2026-06-12-D (5.1-A): morte permanente após N dias sem cuidado.
    /// </summary>
    public readonly struct AnimalHealthChangedEvent
    {
        public string AnimalInstanceId { get; }
        public string AnimalDataId { get; }
        /// <summary>Valor estável correspondente a CindarsHope.Farm.Animals.AnimalHealthState.</summary>
        public int NewHealthState { get; }
        public int DaysWithoutFood { get; }

        public AnimalHealthChangedEvent(string animalInstanceId, string animalDataId, int newHealthState, int daysWithoutFood)
        {
            AnimalInstanceId = animalInstanceId ?? string.Empty;
            AnimalDataId = animalDataId ?? string.Empty;
            NewHealthState = newHealthState;
            DaysWithoutFood = daysWithoutFood;
        }
    }

    /// <summary>
    /// fable_12 — publicado quando um animal morre permanentemente por negligência prolongada
    /// (EMENDA 2026-06-12-D, decisão 5.1-A: N=7 dias consecutivos sem alimentação). Sem reviver.
    /// </summary>
    public readonly struct AnimalDiedEvent
    {
        public string AnimalInstanceId { get; }
        public string AnimalDataId { get; }
        public string HousingId { get; }

        public AnimalDiedEvent(string animalInstanceId, string animalDataId, string housingId)
        {
            AnimalInstanceId = animalInstanceId ?? string.Empty;
            AnimalDataId = animalDataId ?? string.Empty;
            HousingId = housingId ?? string.Empty;
        }
    }
}
