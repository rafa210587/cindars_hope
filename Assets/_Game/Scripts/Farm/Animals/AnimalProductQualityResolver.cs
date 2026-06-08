namespace CindarsHope.Farm.Animals
{
    public static class AnimalProductQualityResolver
    {
        private const int ExcelentThreshold = 80;
        private const int BoaThreshold = 50;

        public static AnimalProductQuality Resolve(AnimalInstanceState state, AnimalProductDefinition definition)
        {
            if (state == null || definition == null || !definition.QualityEnabled)
                return AnimalProductQuality.Normal;

            if (definition.IsLateGameReserved)
                return AnimalProductQuality.Lunar;

            int score = state.CareScore + state.ProductQualityBias;

            if (state.HealthState != AnimalHealthState.Healthy)
                score = score / 2;

            if (!state.FedToday)
                score = 0;

            if (score >= ExcelentThreshold)
                return AnimalProductQuality.Excelente;

            if (score >= BoaThreshold)
                return AnimalProductQuality.Boa;

            return AnimalProductQuality.Normal;
        }
    }
}
