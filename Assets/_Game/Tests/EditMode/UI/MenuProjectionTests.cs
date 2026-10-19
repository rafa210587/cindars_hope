using NUnit.Framework;
using System.Collections.Generic;
using CindarsHope.UI.Shop;
using CindarsHope.UI.Crafting;
using CindarsHope.UI.Skills;
using CindarsHope.UI.Quest;
using CindarsHope.UI.Fonte;
using CindarsHope.UI.Menus;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class MenuProjectionTests
    {
        // ---- ShopMenuViewModel ----

        [Test]
        public void Shop_BuyMode_HasShopInventory()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Buy };
            vm.ShopInventoryRows.Add(new ShopRowViewModel { ItemId = "sword_01", UnitPrice = 100, Stock = 5 });
            Assert.IsTrue(vm.HasShopInventory);
        }

        [Test]
        public void Shop_SellMode_PlayerSellableRows()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Sell };
            vm.PlayerSellableRows.Add(new ShopRowViewModel { ItemId = "herb_01", IsSellable = true });
            Assert.IsTrue(vm.HasPlayerSellable);
        }

        [Test]
        public void Shop_EmptyState_WhenNoInventory()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Buy, EmptyStateMessage = "SHOP_EMPTY_TODAY" };
            Assert.IsFalse(vm.HasShopInventory);
            Assert.IsNotEmpty(vm.EmptyStateMessage);
        }

        [Test]
        public void Shop_TotalPrice_CalculatedFromQuantity()
        {
            var vm = new ShopMenuViewModel { UnitPrice = 50, QuantitySelector = 3 };
            Assert.AreEqual(150, vm.TotalPrice);
        }

        [Test]
        public void Shop_QuestItem_CannotSell_Validated()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Sell, PlayerGold = 0 };
            vm.SelectedItemDetails = new ShopRowViewModel { ItemId = "quest_relic", IsQuestItem = true, IsSellable = false };
            var result = MenuProjectionValidator.ValidateShopSell(vm);
            Assert.AreEqual(MenuCommandValidationResult.InvalidBlocked, result);
        }

        [Test]
        public void Shop_Buy_InsufficientGold_Blocked()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Buy, PlayerGold = 10, UnitPrice = 100, QuantitySelector = 1 };
            vm.SelectedItemDetails = new ShopRowViewModel { ItemId = "sword_01", Stock = 5 };
            var result = MenuProjectionValidator.ValidateShopBuy(vm);
            Assert.AreEqual(MenuCommandValidationResult.InvalidInsufficientResources, result);
        }

        [Test]
        public void Shop_Buy_OutOfStock_Blocked()
        {
            var vm = new ShopMenuViewModel { Mode = ShopMode.Buy, PlayerGold = 1000, UnitPrice = 50, QuantitySelector = 1 };
            vm.SelectedItemDetails = new ShopRowViewModel { ItemId = "rare_01", Stock = 0 };
            var result = MenuProjectionValidator.ValidateShopBuy(vm);
            Assert.AreEqual(MenuCommandValidationResult.InvalidBlocked, result);
        }

        // ---- CraftingMenuViewModel ----

        [Test]
        public void Crafting_HasMissingInputs_WhenMaterialNotOwned()
        {
            var vm = new CraftingMenuViewModel { StationId = "forge" };
            vm.MissingInputs.Add(new CraftingMaterialRequirementViewModel { ItemId = "iron_bar", Required = 5, Owned = 2 });
            Assert.IsTrue(vm.HasMissingInputs);
        }


        [Test]
        public void Crafting_ProcessingJob_TracksProgress()
        {
            var vm = new CraftingMenuViewModel();
            vm.ProcessingJobs.Add(new ProcessingJobViewModel { JobId = "job_01", ElapsedSeconds = 30, TotalSeconds = 60 });
            Assert.IsTrue(vm.HasProcessingJobs);
            Assert.AreEqual(0.5f, vm.ProcessingJobs[0].ProgressPercent, 0.001f);
        }

        // ---- SkillTreeMenuViewModel ----


        [Test]
        public void SkillTree_ActiveSlotSummary_MaxIs4()
        {
            var vm = new SkillTreeMenuViewModel();
            Assert.AreEqual(4, vm.ActiveSlotSummary.MaxSlots);
        }



        // ---- QuestLogMenuState ----

        [Test]
        public void QuestLog_TabState_DefaultsToActive()
        {
            var state = new QuestLogMenuState();
            Assert.AreEqual(QuestLogTab.Active, state.SelectedTab);
        }


        // ---- FonteMenuViewModel ----

        [Test]
        public void Fonte_Respec_HiddenWhenNotUnlocked()
        {
            var vm = new FonteMenuViewModel();
            Assert.IsFalse(vm.IsRespecVisible);
        }

        [Test]
        public void Fonte_Respec_VisibleWhenUnlocked()
        {
            var vm = new FonteMenuViewModel();
            vm.UnlockedFunctions.Add(FonteUnlockedFunction.Respec);
            Assert.IsTrue(vm.IsRespecVisible);
        }

        [Test]
        public void Fonte_FinalChoice_HiddenUntilUnlocked()
        {
            var vm = new FonteMenuViewModel();
            Assert.IsFalse(vm.IsFinalChoiceVisible);
        }

        [Test]
        public void Fonte_Purification_HiddenUntilLifeFragment()
        {
            var vm = new FonteMenuViewModel();
            Assert.IsFalse(vm.IsPurificationVisible);
            vm.UnlockedFunctions.Add(FonteUnlockedFunction.Purification);
            Assert.IsTrue(vm.IsPurificationVisible);
        }

        [Test]
        public void Fonte_ValidateFunction_BlockedIfNotUnlocked()
        {
            var vm = new FonteMenuViewModel();
            var result = MenuProjectionValidator.ValidateFonteFunction(vm, FonteUnlockedFunction.FinalChoice);
            Assert.AreEqual(MenuCommandValidationResult.InvalidSpoilerGate, result);
        }

        [Test]
        public void Fonte_ValidateFunction_ValidWhenUnlocked()
        {
            var vm = new FonteMenuViewModel();
            vm.UnlockedFunctions.Add(FonteUnlockedFunction.LivingWaterBasic);
            var result = MenuProjectionValidator.ValidateFonteFunction(vm, FonteUnlockedFunction.LivingWaterBasic);
            Assert.AreEqual(MenuCommandValidationResult.Valid, result);
        }

        // ---- MenuCommand ----

        [Test]
        public void MenuCommand_IsValid_WhenValidResult()
        {
            var cmd = new MenuCommand
            {
                CommandId = "buy_01", MenuType = MenuType.Shop, CommandType = MenuCommandType.Confirm,
                ValidationResult = MenuCommandValidationResult.Valid
            };
            Assert.IsTrue(cmd.IsValid);
        }

        [Test]
        public void MenuCommand_PreviewOnly_IsAlsoValid()
        {
            var cmd = new MenuCommand { ValidationResult = MenuCommandValidationResult.PreviewOnly, PreviewOnly = true };
            Assert.IsTrue(cmd.IsValid);
        }
    }
}
