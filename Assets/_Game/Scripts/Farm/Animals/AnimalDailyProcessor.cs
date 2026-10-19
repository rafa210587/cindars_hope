using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public class AnimalDailyProcessor
    {
        private AnimalCareService careService;

        public AnimalDailyProcessor(AnimalCareService careService)
        {
            this.careService = careService;
        }

        public void ProcessDayTransition(int currentDay)
        {
            var allAnimals = careService.GetAllAnimals();

            foreach (var kvp in allAnimals)
            {
                var animal = kvp.Value;
                var definition = careService.GetAnimalDefinition(animal.AnimalDataId);

                if (definition == null) continue;

                ProcessAnimalDayTransition(animal, definition, currentDay);
            }
        }

        private void ProcessAnimalDayTransition(AnimalInstanceState animal, AnimalDefinition definition, int currentDay)
        {
            // Reset fed state and check if food was provided yesterday
            if (!animal.FedToday)
            {
                animal.MarkUnfed();
            }
            else
            {
                animal.DaysWithoutFood = 0;
                if (animal.HealthState == AnimalHealthState.Hungry)
                {
                    animal.HealthState = AnimalHealthState.Healthy;
                }
            }

            // Check if product should be ready
            CheckProductEligibility(animal, definition, currentDay);

            // Reset daily fed state for next day
            animal.ResetDailyState();
        }

        private void CheckProductEligibility(AnimalInstanceState animal, AnimalDefinition definition, int currentDay)
        {
            // Animal must be healthy and fed to produce
            if (animal.HealthState != AnimalHealthState.Healthy)
            {
                animal.ProductReady = false;
                return;
            }

            if (definition.RequiresFedTodayToProduce && !animal.FedToday)
            {
                animal.ProductReady = false;
                return;
            }

            // Check if enough days have passed since last production
            int daysSinceLastProduct = animal.LastProductDay >= 0 ?
                currentDay - animal.LastProductDay :
                definition.ProductionCadenceDays;

            if (daysSinceLastProduct >= definition.ProductionCadenceDays)
            {
                animal.ProductReady = true;

                // Quality bias increases with care score
                if (definition.CareAffectsQuality && animal.CareScore > 0)
                {
                    animal.ProductQualityBias = System.Math.Min(
                        (animal.CareScore * 100) / definition.MaxCareScore,
                        100
                    );
                }
            }
            else
            {
                animal.ProductReady = false;
            }
        }

        public void MarkProductCollected(string animalInstanceId, int currentDay)
        {
            var animal = careService.GetAnimalInstance(animalInstanceId);
            if (animal != null)
            {
                animal.ProductReady = false;
                animal.LastProductDay = currentDay;
                animal.ProductQualityBias = 0;
            }
        }
    }
}
