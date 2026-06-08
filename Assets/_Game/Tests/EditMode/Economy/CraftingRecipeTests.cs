using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Crafting;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class CraftingRecipeTests
    {
        private CraftingService _service;
        private CraftingStationDefinition _workbench;
        private RecipeDefinition _basicRecipe;

        [SetUp]
        public void SetUp()
        {
            _service = new CraftingService();
            _workbench = new CraftingStationDefinition
            {
                StationId = "station_workbench",
                StationType = StationType.Workbench,
                StationLevel = 1,
                AllowedRecipeTypes = new List<RecipeType> { RecipeType.CraftRecipe, RecipeType.BuildingRecipe }
            };
            _basicRecipe = new RecipeDefinition
            {
                RecipeId = "recipe_wooden_plank",
                RecipeType = RecipeType.CraftRecipe,
                OutputItemId = "item_wooden_plank",
                OutputQuantity = 2,
                RequiredStation = "station_workbench",
                RequiredStationLevel = 1,
                RequiredIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { ItemId = "item_log", Quantity = 1 }
                }
            };
        }

        private CraftingRequest BasicRequest() => new CraftingRequest
        {
            RecipeId = _basicRecipe.RecipeId,
            StationId = _workbench.StationId,
            KnownRecipes = new PlayerKnownRecipes { KnownRecipeIds = new HashSet<string> { "recipe_wooden_plank" } }
        };

        [Test]
        public void Craft_HappyPath_Success()
        {
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, BasicRequest());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.OutputsCreated.Contains("item_wooden_plank"));
        }

        [Test]
        public void Craft_WrongStation_Fails()
        {
            var req = BasicRequest();
            var alchemy = new CraftingStationDefinition
            {
                StationId = "station_alchemy",
                StationType = StationType.AlchemyTable,
                StationLevel = 1,
                AllowedRecipeTypes = new List<RecipeType> { RecipeType.PotionRecipe }
            };
            var result = _service.ValidateAndStart(_basicRecipe, alchemy, req);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Craft_RecipeNotKnown_Fails()
        {
            var req = BasicRequest();
            req.KnownRecipes = new PlayerKnownRecipes(); // empty
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("not known"));
        }

        [Test]
        public void Craft_RequiredFarmLevel_NotMet_Fails()
        {
            _basicRecipe.RequiredFarmLevel = 3;
            var req = BasicRequest();
            req.PlayerFarmLevel = 1;
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Craft_RequiredQuestFlag_NotActive_Fails()
        {
            _basicRecipe.RequiredQuestFlag = "quest_blacksmith_intro";
            var req = BasicRequest();
            req.ActiveQuestFlag = null;
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Craft_ProtectedIngredient_Fails()
        {
            _basicRecipe.RequiredIngredients.Add(new RecipeIngredient { ItemId = "item_fruto_mana", Quantity = 1, IsProtected = true });
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, BasicRequest());
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("Protected ingredient"));
        }

        [Test]
        public void Processing_CreatesJob_NotImmediate()
        {
            _basicRecipe.RequiredTimeTicks = 5;
            var req = BasicRequest();
            req.RequestedDayTime = 10;
            var result = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            Assert.IsTrue(result.Success);
            Assert.IsNotEmpty(result.ProcessingJobId);
            Assert.IsTrue(result.OutputsCreated.Count == 0, "Processing result should not have immediate outputs");
        }

        [Test]
        public void Processing_CollectBeforeFinish_Fails()
        {
            _basicRecipe.RequiredTimeTicks = 5;
            var req = BasicRequest();
            req.RequestedDayTime = 10;
            var startResult = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            var collectResult = _service.CollectJob(startResult.ProcessingJobId, 12); // too early (finish at 15)
            Assert.IsFalse(collectResult.Success);
        }

        [Test]
        public void Processing_CollectAfterFinish_Succeeds()
        {
            _basicRecipe.RequiredTimeTicks = 5;
            var req = BasicRequest();
            req.RequestedDayTime = 10;
            var startResult = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            var collectResult = _service.CollectJob(startResult.ProcessingJobId, 15); // at finish
            Assert.IsTrue(collectResult.Success);
            Assert.IsTrue(collectResult.OutputsCreated.Contains("item_wooden_plank"));
        }

        [Test]
        public void Processing_CollectIdempotent_SecondCollect_Fails()
        {
            _basicRecipe.RequiredTimeTicks = 5;
            var req = BasicRequest();
            req.RequestedDayTime = 10;
            var startResult = _service.ValidateAndStart(_basicRecipe, _workbench, req);
            _service.CollectJob(startResult.ProcessingJobId, 15);
            var second = _service.CollectJob(startResult.ProcessingJobId, 15);
            Assert.IsFalse(second.Success, "Second collect must fail — idempotency");
        }

        [Test]
        public void KnownRecipes_Learn_CanCraft()
        {
            var known = new PlayerKnownRecipes();
            Assert.IsFalse(known.CanCraft("recipe_sword"));
            known.Learn("recipe_sword");
            Assert.IsTrue(known.CanCraft("recipe_sword"));
        }
    }
}
