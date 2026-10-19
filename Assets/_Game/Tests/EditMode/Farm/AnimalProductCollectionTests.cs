using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Animals;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class AnimalProductCollectionTests
    {
        private Dictionary<string, AnimalInstanceState> _animals;
        private Dictionary<string, AnimalProductDefinition> _definitions;
        private AnimalProductCollectionService _service;

        [SetUp]
        public void Setup()
        {
            _animals = new Dictionary<string, AnimalInstanceState>();
            _definitions = new Dictionary<string, AnimalProductDefinition>();

            var cowDef = new AnimalProductDefinition("milk", FarmAnimalSpecies.Cow, "item_milk", 1)
            {
                RequiresFedToday = true,
                RequiresProductReady = true,
                QualityEnabled = true
            };
            _definitions["milk"] = cowDef;

            var chickenDef = new AnimalProductDefinition("egg", FarmAnimalSpecies.Chicken, "item_egg", 1)
            {
                RequiresFedToday = true,
                RequiresProductReady = true,
                QualityEnabled = true
            };
            _definitions["egg"] = chickenDef;

            _service = new AnimalProductCollectionService(_animals, _definitions);
        }

        private AnimalInstanceState MakeCow(string id, bool fedToday, bool productReady, int careScore = 50)
        {
            var state = new AnimalInstanceState(id, "cow_data", "Bessie", "barn_01");
            state.FedToday = fedToday;
            state.ProductReady = productReady;
            state.CareScore = careScore;
            state.HealthState = AnimalHealthState.Healthy;
            return state;
        }

        [Test]
        public void Collection_Succeeds_WhenProductReady_AndFed()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("item_milk", result.OutputItemId);
            Assert.IsTrue(result.ProductReadyConsumed);
        }

        [Test]
        public void Collection_Fails_WhenProductNotReady()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, false);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AnimalProductCollectionFailureReason.ProductNotReady, result.FailureReason);
        }

        [Test]
        public void Collection_Fails_WhenAnimalUnfed_AndDefinitionRequiresFed()
        {
            _animals["cow_01"] = MakeCow("cow_01", false, true);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AnimalProductCollectionFailureReason.AnimalNotFed, result.FailureReason);
        }

        [Test]
        public void Collection_Fails_WhenAnimalNotFound()
        {
            var cmd = new AnimalProductCollectionCommand("nonexistent", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AnimalProductCollectionFailureReason.AnimalNotFound, result.FailureReason);
        }

        [Test]
        public void Collection_Fails_WhenAnimalUnavailable()
        {
            var animal = MakeCow("cow_01", true, true);
            animal.HealthState = AnimalHealthState.Unavailable;
            _animals["cow_01"] = animal;

            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AnimalProductCollectionFailureReason.AnimalUnavailable, result.FailureReason);
        }

        [Test]
        public void Collection_ConsumesProductReady_AfterSuccess()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            _service.Collect(cmd);

            Assert.IsFalse(_animals["cow_01"].ProductReady,
                "ProductReady must be false after successful collection");
        }

        [Test]
        public void DuplicateCollection_Fails_AfterFirstSuccess()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");

            var first = _service.Collect(cmd);
            var second = _service.Collect(cmd);

            Assert.IsTrue(first.Success);
            Assert.IsFalse(second.Success);
            Assert.AreEqual(AnimalProductCollectionFailureReason.ProductNotReady, second.FailureReason);
        }

        [Test]
        public void Quality_Normal_WhenCareScoreLow()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true, careScore: 10);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.AreEqual(AnimalProductQuality.Normal, result.Quality);
        }

        [Test]
        public void Quality_Boa_WhenCareScoreMedium()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true, careScore: 60);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.AreEqual(AnimalProductQuality.Boa, result.Quality);
        }

        [Test]
        public void Quality_Excelente_WhenCareScoreHigh()
        {
            _animals["cow_01"] = MakeCow("cow_01", true, true, careScore: 85);
            var cmd = new AnimalProductCollectionCommand("cow_01", 1, "inv_main");
            var result = _service.Collect(cmd);

            Assert.AreEqual(AnimalProductQuality.Excelente, result.Quality);
        }

        [Test]
        public void Quality_Normal_WhenAnimalUnhealthy()
        {
            var animal = MakeCow("cow_01", true, true, careScore: 90);
            animal.HealthState = AnimalHealthState.Tired;
            _animals["cow_01"] = animal;

            var cowDef = _definitions["milk"];
            var quality = AnimalProductQualityResolver.Resolve(animal, cowDef);

            Assert.IsTrue(quality <= AnimalProductQuality.Boa,
                "Unhealthy animal should not produce Excelente quality");
        }

        [Test]
        public void FertilizerBase_CanBeMarkedAsFertilizerBase()
        {
            var fertDef = new AnimalProductDefinition("fertilizer_base", FarmAnimalSpecies.Cow,
                "item_fertilizer_base", 1)
            {
                CanBeUsedAsFertilizerBase = true,
                CanBeProcessed = false
            };

            Assert.IsTrue(fertDef.CanBeUsedAsFertilizerBase);
            Assert.IsFalse(fertDef.CanBeProcessed);
            Assert.AreNotEqual("item_mana", fertDef.OutputItemId,
                "Fertilizer base must not be Mana item");
        }

        [Test]
        public void CollectBySpecies_Chicken_ReturnsEgg()
        {
            _animals["chicken_01"] = new AnimalInstanceState("chicken_01", "chicken_data", "Clucky", "coop_01");
            _animals["chicken_01"].FedToday = true;
            _animals["chicken_01"].ProductReady = true;
            _animals["chicken_01"].CareScore = 50;
            _animals["chicken_01"].HealthState = AnimalHealthState.Healthy;

            var cmd = new AnimalProductCollectionCommand("chicken_01", 1, "inv_main");
            var result = _service.CollectBySpecies(cmd, FarmAnimalSpecies.Chicken);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("item_egg", result.OutputItemId);
        }
    }
}
