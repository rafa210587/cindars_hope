using System;
using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public enum FeedingResult
    {
        Success = 0,
        InvalidFeedItem = 1,
        AnimalNotFound = 2,
        CannotPersist = 3,
        AnimalUnavailable = 4
    }

    public class AnimalCareService
    {
        private Dictionary<string, AnimalDefinition> animalDefinitions = new Dictionary<string, AnimalDefinition>();
        private Dictionary<string, AnimalInstanceState> animalInstances = new Dictionary<string, AnimalInstanceState>();
        private Dictionary<string, AnimalHousingCapacityState> housingStates = new Dictionary<string, AnimalHousingCapacityState>();
        private HashSet<string> validFeedItemIds = new HashSet<string>();

        public void RegisterAnimalDefinition(AnimalDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            animalDefinitions[definition.AnimalDataId] = definition;
        }

        public void RegisterAnimalInstance(AnimalInstanceState instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            animalInstances[instance.AnimalInstanceId] = instance;
        }

        public void RegisterHousingState(AnimalHousingCapacityState housing)
        {
            if (housing == null) throw new ArgumentNullException(nameof(housing));
            housingStates[housing.HomeBuildingId] = housing;
        }

        public void RegisterValidFeedItem(string itemId)
        {
            validFeedItemIds.Add(itemId);
        }

        public FeedingResult FeedAnimal(string animalInstanceId, string feedItemId)
        {
            if (!animalInstances.TryGetValue(animalInstanceId, out var animal))
            {
                return FeedingResult.AnimalNotFound;
            }

            if (animal.HealthState == AnimalHealthState.Unavailable)
            {
                return FeedingResult.AnimalUnavailable;
            }

            if (!validFeedItemIds.Contains(feedItemId))
            {
                return FeedingResult.InvalidFeedItem;
            }

            if (!animalDefinitions.TryGetValue(animal.AnimalDataId, out var definition))
            {
                return FeedingResult.CannotPersist;
            }

            bool isFavorite = definition.FavoriteFeedItemIds != null &&
                             definition.FavoriteFeedItemIds.Contains(feedItemId);

            animal.MarkFed(0);

            if (isFavorite && definition.CareAffectsQuality)
            {
                animal.CareScore = System.Math.Min(animal.CareScore + 5, definition.MaxCareScore);
            }

            return FeedingResult.Success;
        }

        public void IncreaseCareScore(string animalInstanceId, int amount)
        {
            if (animalInstances.TryGetValue(animalInstanceId, out var animal))
            {
                if (animalDefinitions.TryGetValue(animal.AnimalDataId, out var definition))
                {
                    animal.CareScore = System.Math.Min(animal.CareScore + amount, definition.MaxCareScore);
                }
            }
        }

        public AnimalInstanceState GetAnimalInstance(string animalInstanceId)
        {
            animalInstances.TryGetValue(animalInstanceId, out var instance);
            return instance;
        }

        public AnimalDefinition GetAnimalDefinition(string animalDataId)
        {
            animalDefinitions.TryGetValue(animalDataId, out var definition);
            return definition;
        }

        public AnimalHousingCapacityState GetHousingState(string homeBuildingId)
        {
            housingStates.TryGetValue(homeBuildingId, out var housing);
            return housing;
        }

        public IReadOnlyDictionary<string, AnimalInstanceState> GetAllAnimals()
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, AnimalInstanceState>(animalInstances);
        }
    }
}
