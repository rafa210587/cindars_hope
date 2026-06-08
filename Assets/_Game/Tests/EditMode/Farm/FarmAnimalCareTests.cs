using NUnit.Framework;
using CindarsHope.Farm.Animals;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmAnimalCareTests
    {
        private AnimalCareService careService;
        private AnimalDailyProcessor dailyProcessor;

        [SetUp]
        public void Setup()
        {
            careService = new AnimalCareService();
            dailyProcessor = new AnimalDailyProcessor(careService);

            RegisterTestAnimals();
            RegisterTestHousing();
        }

        private void RegisterTestAnimals()
        {
            var cowDef = new AnimalDefinition(
                "cow_definition",
                "Cow",
                FarmAnimalSpecies.Cow,
                "Barn",
                1,
                "Milk",
                2,
                true
            );
            careService.RegisterAnimalDefinition(cowDef);

            var chickenDef = new AnimalDefinition(
                "chicken_definition",
                "Chicken",
                FarmAnimalSpecies.Chicken,
                "Coop",
                1,
                "Egg",
                1,
                true
            );
            careService.RegisterAnimalDefinition(chickenDef);

            careService.RegisterValidFeedItem("hay");
            careService.RegisterValidFeedItem("grain");
            careService.RegisterValidFeedItem("vegetable_scraps");
        }

        private void RegisterTestHousing()
        {
            var barn = new AnimalHousingCapacityState("barn_001", AnimalHousingBuildingType.Barn, 5);
            careService.RegisterHousingState(barn);

            var coop = new AnimalHousingCapacityState("coop_001", AnimalHousingBuildingType.Coop, 10);
            careService.RegisterHousingState(coop);
        }

        [Test]
        public void TestAnimalDefinitionCreation()
        {
            var def = careService.GetAnimalDefinition("cow_definition");
            Assert.IsNotNull(def);
            Assert.AreEqual("Cow", def.DisplayName);
            Assert.AreEqual(FarmAnimalSpecies.Cow, def.Species);
            Assert.AreEqual(2, def.ProductionCadenceDays);
        }

        [Test]
        public void TestAnimalInstanceCreation()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            var retrieved = careService.GetAnimalInstance("cow_001");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Bessie", retrieved.Name);
            Assert.AreEqual(AnimalHealthState.Healthy, retrieved.HealthState);
            Assert.IsFalse(retrieved.FedToday);
        }

        [Test]
        public void TestFeedingAnimal_Success()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            var result = careService.FeedAnimal("cow_001", "hay");

            Assert.AreEqual(FeedingResult.Success, result);
            Assert.IsTrue(animal.FedToday);
            Assert.AreEqual(0, animal.DaysWithoutFood);
        }

        [Test]
        public void TestFeedingAnimal_InvalidFeed()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            var result = careService.FeedAnimal("cow_001", "invalid_item");

            Assert.AreEqual(FeedingResult.InvalidFeedItem, result);
            Assert.IsFalse(animal.FedToday);
        }

        [Test]
        public void TestFeedingAnimal_AnimalNotFound()
        {
            var result = careService.FeedAnimal("nonexistent_animal", "hay");

            Assert.AreEqual(FeedingResult.AnimalNotFound, result);
        }

        [Test]
        public void TestHousingCapacity_AddAnimal()
        {
            var barn = careService.GetHousingState("barn_001");
            Assert.IsTrue(barn.CanAddAnimal());

            barn.AddAnimal("cow_001");
            Assert.AreEqual(1, barn.AnimalInstanceIds.Count);
            Assert.AreEqual(4, barn.GetAvailableCapacity());
        }

        [Test]
        public void TestHousingCapacity_CapacityFull()
        {
            var barn = careService.GetHousingState("barn_001");
            barn.AddAnimal("cow_001");
            barn.AddAnimal("cow_002");
            barn.AddAnimal("cow_003");
            barn.AddAnimal("cow_004");
            barn.AddAnimal("cow_005");

            Assert.IsFalse(barn.CanAddAnimal());
            Assert.AreEqual(0, barn.GetAvailableCapacity());
        }

        [Test]
        public void TestHousingCapacity_RemoveAnimal()
        {
            var barn = careService.GetHousingState("barn_001");
            barn.AddAnimal("cow_001");
            barn.RemoveAnimal("cow_001");

            Assert.AreEqual(0, barn.AnimalInstanceIds.Count);
            Assert.IsTrue(barn.CanAddAnimal());
        }

        [Test]
        public void TestDailyProcessor_UnfedAnimal()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            animal.ResetDailyState();
            careService.RegisterAnimalInstance(animal);

            dailyProcessor.ProcessDayTransition(1);

            Assert.AreEqual(1, animal.DaysWithoutFood);
            Assert.AreEqual(AnimalHealthState.Hungry, animal.HealthState);
        }

        [Test]
        public void TestDailyProcessor_HungerEscalation()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            // Day 1: No food
            animal.ResetDailyState();
            dailyProcessor.ProcessDayTransition(1);
            Assert.AreEqual(AnimalHealthState.Hungry, animal.HealthState);

            // Day 2: No food
            animal.ResetDailyState();
            dailyProcessor.ProcessDayTransition(2);
            Assert.AreEqual(AnimalHealthState.Hungry, animal.HealthState);

            // Day 3: No food
            animal.ResetDailyState();
            dailyProcessor.ProcessDayTransition(3);
            Assert.AreEqual(AnimalHealthState.Unavailable, animal.HealthState);
        }

        [Test]
        public void TestDailyProcessor_ProductReadiness()
        {
            var animal = new AnimalInstanceState("chicken_001", "chicken_definition", "Clucka", "coop_001");
            careService.RegisterAnimalInstance(animal);

            // Day 1: Fed chicken should be ready next day
            animal.MarkFed(0);
            dailyProcessor.ProcessDayTransition(1);
            Assert.IsTrue(animal.ProductReady);
        }

        [Test]
        public void TestDailyProcessor_ProductNotReadyWithoutFood()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            // Unfed animal should not be ready
            animal.ResetDailyState();
            dailyProcessor.ProcessDayTransition(1);
            Assert.IsFalse(animal.ProductReady);
        }

        [Test]
        public void TestDailyProcessor_ProductCadence()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            // Day 0: Feed cow
            animal.MarkFed(0);
            dailyProcessor.ProcessDayTransition(1);
            Assert.IsTrue(animal.ProductReady);

            // Mark product collected
            dailyProcessor.MarkProductCollected("cow_001", 1);
            Assert.IsFalse(animal.ProductReady);
            Assert.AreEqual(1, animal.LastProductDay);

            // Day 2: Only 1 day has passed, cow produces every 2 days
            animal.MarkFed(2);
            dailyProcessor.ProcessDayTransition(2);
            Assert.IsFalse(animal.ProductReady);

            // Day 3: Now 2 days have passed, should be ready
            animal.MarkFed(3);
            dailyProcessor.ProcessDayTransition(3);
            Assert.IsTrue(animal.ProductReady);
        }

        [Test]
        public void TestCareScore_FavoriteFood()
        {
            var cowDef = new AnimalDefinition(
                "cow_favorite",
                "Cow",
                FarmAnimalSpecies.Cow,
                "Barn",
                1,
                "Milk",
                2,
                true
            );
            cowDef.FavoriteFeedItemIds.Add("hay");
            careService.RegisterAnimalDefinition(cowDef);

            var animal = new AnimalInstanceState("cow_001", "cow_favorite", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            int initialCare = animal.CareScore;
            careService.FeedAnimal("cow_001", "hay");

            Assert.Greater(animal.CareScore, initialCare);
        }

        [Test]
        public void TestCareScore_MaxCap()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            careService.RegisterAnimalInstance(animal);

            for (int i = 0; i < 50; i++)
            {
                careService.IncreaseCareScore("cow_001", 10);
            }

            var definition = careService.GetAnimalDefinition("cow_definition");
            Assert.AreEqual(definition.MaxCareScore, animal.CareScore);
        }

        [Test]
        public void TestProductQualityBias()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            animal.CareScore = 50;
            careService.RegisterAnimalInstance(animal);

            dailyProcessor.ProcessDayTransition(1);

            Assert.Greater(animal.ProductQualityBias, 0);
        }

        [Test]
        public void TestAnimalUnavailable_CannotFeed()
        {
            var animal = new AnimalInstanceState("cow_001", "cow_definition", "Bessie", "barn_001");
            animal.HealthState = AnimalHealthState.Unavailable;
            careService.RegisterAnimalInstance(animal);

            var result = careService.FeedAnimal("cow_001", "hay");

            Assert.AreEqual(FeedingResult.AnimalUnavailable, result);
        }
    }
}
