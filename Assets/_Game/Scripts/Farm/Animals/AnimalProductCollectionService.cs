using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public class AnimalProductCollectionService
    {
        private readonly Dictionary<string, AnimalInstanceState> _animals;
        private readonly Dictionary<string, AnimalProductDefinition> _definitions;

        public AnimalProductCollectionService(
            Dictionary<string, AnimalInstanceState> animals,
            Dictionary<string, AnimalProductDefinition> definitions)
        {
            _animals = animals ?? new Dictionary<string, AnimalInstanceState>();
            _definitions = definitions ?? new Dictionary<string, AnimalProductDefinition>();
        }

        public AnimalProductCollectionResult Collect(AnimalProductCollectionCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.AnimalInstanceId))
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFound);

            if (!_animals.TryGetValue(command.AnimalInstanceId, out var animal))
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFound);

            if (animal.HealthState == AnimalHealthState.Unavailable)
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalUnavailable);

            if (!animal.ProductReady)
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.ProductNotReady);

            // Find product definition matching this animal's state
            AnimalProductDefinition definition = null;
            foreach (var def in _definitions.Values)
            {
                if (!def.RequiresFedToday || animal.FedToday)
                {
                    definition = def;
                    break;
                }
            }

            if (definition == null)
            {
                if (!animal.FedToday)
                    return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFed);
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.InvalidProductDefinition);
            }

            if (definition.IsLateGameReserved)
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.LateGameReserved);

            var quality = AnimalProductQualityResolver.Resolve(animal, definition);

            // Consume ProductReady — idempotency guardrail
            animal.ProductReady = false;
            animal.LastProductDay = command.RequestedDay;

            return AnimalProductCollectionResult.Ok(definition.OutputItemId, quality, definition.BaseQuantity);
        }

        public AnimalProductCollectionResult CollectBySpecies(
            AnimalProductCollectionCommand command,
            FarmAnimalSpecies species)
        {
            if (command == null || string.IsNullOrEmpty(command.AnimalInstanceId))
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFound);

            if (!_animals.TryGetValue(command.AnimalInstanceId, out var animal))
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFound);

            if (animal.HealthState == AnimalHealthState.Unavailable)
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalUnavailable);

            if (!animal.ProductReady)
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.ProductNotReady);

            AnimalProductDefinition definition = null;
            foreach (var def in _definitions.Values)
            {
                if (def.SourceAnimalSpecies == species && !def.IsLateGameReserved)
                {
                    if (!def.RequiresFedToday || animal.FedToday)
                    {
                        definition = def;
                        break;
                    }
                }
            }

            if (definition == null)
            {
                if (!animal.FedToday)
                    return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.AnimalNotFed);
                return AnimalProductCollectionResult.Fail(AnimalProductCollectionFailureReason.InvalidProductDefinition);
            }

            var quality = AnimalProductQualityResolver.Resolve(animal, definition);

            animal.ProductReady = false;
            animal.LastProductDay = command.RequestedDay;

            return AnimalProductCollectionResult.Ok(definition.OutputItemId, quality, definition.BaseQuantity);
        }
    }
}
