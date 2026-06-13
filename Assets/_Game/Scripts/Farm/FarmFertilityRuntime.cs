using System.Collections.Generic;
using CindarsHope.Farm.Fertilizer;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Host runtime do FertilizerApplicationService (módulo WAVE 05, intocado).
    /// Mantém os modificadores de solo ativos por plotId e expõe a API que o FarmPlot
    /// consome ao fertilizar e ao colher.
    /// </summary>
    public static class FarmFertilityRuntime
    {
        private static FertilizerApplicationService _service;
        private static Dictionary<string, SoilModifierState> _modifiers;
        private static Dictionary<string, FertilizerDefinition> _definitions;

        public static IReadOnlyDictionary<string, FertilizerDefinition> Definitions
        {
            get
            {
                EnsureInitialized();
                return _definitions;
            }
        }

        public static FertilizerApplicationResult TryApply(string plotId, string fertilizerId, int currentDay)
        {
            EnsureInitialized();
            return _service.Apply(plotId, fertilizerId, currentDay);
        }

        public static bool HasActiveFertilizer(string plotId)
        {
            EnsureInitialized();
            return _service.HasActiveFertilizer(plotId);
        }

        public static SoilModifierState GetModifier(string plotId)
        {
            EnsureInitialized();
            return _service.GetModifier(plotId);
        }

        public static void ConsumeOnHarvest(string plotId)
        {
            EnsureInitialized();
            if (_modifiers.TryGetValue(plotId, out var modifier) && modifier.IsActive && modifier.ConsumedOnHarvest)
            {
                modifier.Consume();
            }
        }

        /// <summary>Reconstrói o modificador a partir do save (campo aditivo do FarmPlot).</summary>
        public static void RestoreModifier(string plotId, string fertilizerId, int appliedDay)
        {
            if (string.IsNullOrWhiteSpace(fertilizerId))
            {
                return;
            }

            EnsureInitialized();
            _service.Apply(plotId, fertilizerId, appliedDay);
        }

        public static void ResetForTests()
        {
            _service = null;
            _modifiers = null;
            _definitions = null;
        }

        private static void EnsureInitialized()
        {
            if (_service != null)
            {
                return;
            }

            _definitions = new Dictionary<string, FertilizerDefinition>();
            foreach (var definition in FertilizerDefinition.GetDefaults())
            {
                _definitions[definition.FertilizerId] = definition;
            }

            _modifiers = new Dictionary<string, SoilModifierState>();
            _service = new FertilizerApplicationService(_definitions, _modifiers);
        }
    }
}
