using NUnit.Framework;
using CindarsHope.UI.Crafting;

namespace CindarsHope.Tests.EditMode.UI.Crafting
{
    [TestFixture]
    public class CraftingRecipeViewModelTests
    {
        [Test]
        public void RecipeState_KnownCraftable_CanCraftIsTrue()
        {
            var viewModel = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.KnownCraftable
            };
            Assert.IsTrue(viewModel.CanCraft);
        }

        [Test]
        public void RecipeState_Locked_IsLockedIsTrue()
        {
            var vm1 = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.KnownLockedBySkill
            };
            var vm2 = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.KnownLockedByQuest
            };
            Assert.IsTrue(vm1.IsLocked);
            Assert.IsTrue(vm2.IsLocked);
        }

        [Test]
        public void RecipeState_Hidden_IsHiddenIsTrue()
        {
            var viewModel = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.UnknownHidden
            };
            Assert.IsTrue(viewModel.IsHidden);
        }

        [Test]
        public void RecipeState_Processing_IsProcessingIsTrue()
        {
            var viewModel = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.ProcessingActive
            };
            Assert.IsTrue(viewModel.IsProcessing);
        }

        [Test]
        public void RecipeState_Ready_IsReadyIsTrue()
        {
            var viewModel = new CraftingRecipeViewModel
            {
                State = CraftingRecipeViewModel.RecipeState.ReadyToCollect
            };
            Assert.IsTrue(viewModel.IsReady);
        }

        [Test]
        public void EvaluateState_AllConditionsMet_ReturnsCraftable()
        {
            var state = RecipeStateEvaluator.EvaluateState(
                hasMaterials: true,
                hasStation: true,
                isLocked: false,
                isHidden: false,
                isProcessing: false,
                isReadyToCollect: false
            );
            Assert.AreEqual(CraftingRecipeViewModel.RecipeState.KnownCraftable, state);
        }

        [Test]
        public void EvaluateState_Hidden_ReturnsHidden()
        {
            var state = RecipeStateEvaluator.EvaluateState(
                hasMaterials: true,
                hasStation: true,
                isLocked: false,
                isHidden: true,
                isProcessing: false,
                isReadyToCollect: false
            );
            Assert.AreEqual(CraftingRecipeViewModel.RecipeState.UnknownHidden, state);
        }

        [Test]
        public void EvaluateState_MissingMaterials_ReturnsMissing()
        {
            var state = RecipeStateEvaluator.EvaluateState(
                hasMaterials: false,
                hasStation: true,
                isLocked: false,
                isHidden: false,
                isProcessing: false,
                isReadyToCollect: false
            );
            Assert.AreEqual(CraftingRecipeViewModel.RecipeState.KnownMissingMaterials, state);
        }

        [Test]
        public void EvaluateState_MissingStation_ReturnsMissing()
        {
            var state = RecipeStateEvaluator.EvaluateState(
                hasMaterials: true,
                hasStation: false,
                isLocked: false,
                isHidden: false,
                isProcessing: false,
                isReadyToCollect: false
            );
            Assert.AreEqual(CraftingRecipeViewModel.RecipeState.KnownMissingStation, state);
        }

        [Test]
        public void EvaluateState_Processing_ReturnsProcessing()
        {
            var state = RecipeStateEvaluator.EvaluateState(
                hasMaterials: true,
                hasStation: true,
                isLocked: false,
                isHidden: false,
                isProcessing: true,
                isReadyToCollect: false
            );
            Assert.AreEqual(CraftingRecipeViewModel.RecipeState.ProcessingActive, state);
        }
    }

    [TestFixture]
    public class CraftQuantityCalculatorTests
    {
        [Test]
        public void CalculateMaxCrafts_SingleMaterial_ReturnsCorrectMax()
        {
            var requirements = new System.Collections.Generic.List<CraftingMaterialRequirementViewModel>
            {
                new CraftingMaterialRequirementViewModel
                {
                    ItemId = "copper_ore",
                    ItemName = "Copper Ore",
                    Required = 5,
                    Owned = 20
                }
            };

            int maxCrafts = CraftQuantityCalculator.CalculateMaxCrafts(requirements);
            Assert.AreEqual(4, maxCrafts);
        }

        [Test]
        public void CalculateMaxCrafts_MultipleMaterials_ReturnsLimitingFactor()
        {
            var requirements = new System.Collections.Generic.List<CraftingMaterialRequirementViewModel>
            {
                new CraftingMaterialRequirementViewModel
                {
                    ItemId = "copper_ore",
                    Required = 5,
                    Owned = 20
                },
                new CraftingMaterialRequirementViewModel
                {
                    ItemId = "wood",
                    Required = 3,
                    Owned = 6
                }
            };

            int maxCrafts = CraftQuantityCalculator.CalculateMaxCrafts(requirements);
            Assert.AreEqual(2, maxCrafts); // Limited by wood (6/3 = 2)
        }

        [Test]
        public void CalculateMaxCrafts_InsufficientMaterials_ReturnsZero()
        {
            var requirements = new System.Collections.Generic.List<CraftingMaterialRequirementViewModel>
            {
                new CraftingMaterialRequirementViewModel
                {
                    ItemId = "copper_ore",
                    Required = 10,
                    Owned = 5
                }
            };

            int maxCrafts = CraftQuantityCalculator.CalculateMaxCrafts(requirements);
            Assert.AreEqual(0, maxCrafts);
        }

        [Test]
        public void ProjectCraftMany_CalculatesCorrectTotals()
        {
            var singleCraft = new System.Collections.Generic.List<CraftingMaterialRequirementViewModel>
            {
                new CraftingMaterialRequirementViewModel
                {
                    ItemId = "copper_ore",
                    ItemName = "Copper Ore",
                    Required = 5,
                    Owned = 20
                }
            };

            var projected = CraftQuantityCalculator.ProjectCraftMany(singleCraft, 3);

            Assert.AreEqual(1, projected.Count);
            Assert.AreEqual(15, projected[0].Required); // 5 * 3
            Assert.AreEqual(20, projected[0].Owned); // unchanged
        }
    }
}
