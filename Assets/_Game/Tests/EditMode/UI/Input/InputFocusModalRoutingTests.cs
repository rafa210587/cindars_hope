using NUnit.Framework;
using CindarsHope.UI.InputRouting;
using CindarsHope.UI.Modal;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class UIFocusRouterTests
    {
        [Test]
        public void GameplayFocus_DoesNotBlockGameplayInput()
        {
            Assert.IsFalse(UIFocusRouter.BlocksGameplayInput(UIFocusState.GameplayFocus));
        }

        [Test]
        public void DebugFocus_DoesNotBlockGameplayInput()
        {
            Assert.IsFalse(UIFocusRouter.BlocksGameplayInput(UIFocusState.DebugFocus));
        }

        [Test]
        public void AllModalFocuses_BlockGameplayInput()
        {
            var modalFocuses = new[]
            {
                UIFocusState.DialogueFocus,
                UIFocusState.MenuFocus,
                UIFocusState.ShopFocus,
                UIFocusState.InventoryFocus,
                UIFocusState.CraftingFocus,
                UIFocusState.SkillTreeFocus,
                UIFocusState.QuestLogFocus,
                UIFocusState.SystemFocus,
                UIFocusState.ConfirmationFocus,
                UIFocusState.TooltipFocus
            };

            foreach (var focus in modalFocuses)
            {
                Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(focus), $"{focus} should block gameplay input");
            }
        }

        [Test]
        public void GameplayFocus_AllowsMovement()
        {
            Assert.IsTrue(UIFocusRouter.CanMove(UIFocusState.GameplayFocus));
        }

        [Test]
        public void ModalFocuses_BlockMovement()
        {
            var modalFocuses = new[] { UIFocusState.InventoryFocus, UIFocusState.DialogueFocus, UIFocusState.ShopFocus };
            foreach (var focus in modalFocuses)
            {
                Assert.IsFalse(UIFocusRouter.CanMove(focus), $"{focus} should block movement");
            }
        }

        [Test]
        public void GameplayFocus_AllowsCombat()
        {
            Assert.IsTrue(UIFocusRouter.CanCombat(UIFocusState.GameplayFocus));
        }

        [Test]
        public void DialogueFocus_BlocksCombat()
        {
            Assert.IsFalse(UIFocusRouter.CanCombat(UIFocusState.DialogueFocus));
        }

        [Test]
        public void GameplayFocus_AllowsHotbar()
        {
            Assert.IsTrue(UIFocusRouter.CanUseHotbar(UIFocusState.GameplayFocus));
        }

        [Test]
        public void InventoryFocus_BlocksHotbar()
        {
            Assert.IsFalse(UIFocusRouter.CanUseHotbar(UIFocusState.InventoryFocus));
        }

        [Test]
        public void GameplayFocus_AllowsWorldInteraction()
        {
            Assert.IsTrue(UIFocusRouter.CanInteractWorld(UIFocusState.GameplayFocus));
        }

        [Test]
        public void ShopFocus_BlocksWorldInteraction()
        {
            Assert.IsFalse(UIFocusRouter.CanInteractWorld(UIFocusState.ShopFocus));
        }

        [Test]
        public void AllModalFocuses_AllowUINavigation()
        {
            var modalFocuses = new[] { UIFocusState.InventoryFocus, UIFocusState.DialogueFocus, UIFocusState.MenuFocus };
            foreach (var focus in modalFocuses)
            {
                Assert.IsTrue(UIFocusRouter.CanNavigateUI(focus), $"{focus} should allow UI navigation");
            }
        }

        [Test]
        public void GameplayFocus_DoesNotAllowUINavigation()
        {
            Assert.IsFalse(UIFocusRouter.CanNavigateUI(UIFocusState.GameplayFocus));
        }

        [Test]
        public void ModalFocuses_AllowConfirm()
        {
            Assert.IsTrue(UIFocusRouter.CanConfirmUI(UIFocusState.InventoryFocus));
            Assert.IsTrue(UIFocusRouter.CanConfirmUI(UIFocusState.ShopFocus));
        }

        [Test]
        public void GameplayFocus_DoesNotAllowConfirm()
        {
            Assert.IsFalse(UIFocusRouter.CanConfirmUI(UIFocusState.GameplayFocus));
        }

        [Test]
        public void ModalFocuses_AllowCancel()
        {
            Assert.IsTrue(UIFocusRouter.CanCancelUI(UIFocusState.InventoryFocus));
            Assert.IsTrue(UIFocusRouter.CanCancelUI(UIFocusState.DialogueFocus));
        }

        [Test]
        public void GameplayFocus_DoesNotAllowCancel()
        {
            Assert.IsFalse(UIFocusRouter.CanCancelUI(UIFocusState.GameplayFocus));
        }

        [Test]
        public void GetFocusFromModalType_MapsDialogueToDialogueFocus()
        {
            Assert.AreEqual(UIFocusState.DialogueFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Dialogue));
        }

        [Test]
        public void GetFocusFromModalType_MapsInventoryToInventoryFocus()
        {
            Assert.AreEqual(UIFocusState.InventoryFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Inventory));
        }

        [Test]
        public void GetFocusFromModalType_MapsShopToShopFocus()
        {
            Assert.AreEqual(UIFocusState.ShopFocus, UIFocusRouter.GetFocusFromModalType(ModalType.ShopMenu));
            Assert.AreEqual(UIFocusState.ShopFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Buy));
            Assert.AreEqual(UIFocusState.ShopFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Sell));
        }

        [Test]
        public void GetFocusFromModalType_MapsCraftingToCraftingFocus()
        {
            Assert.AreEqual(UIFocusState.CraftingFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Crafting));
        }

        [Test]
        public void GetFocusFromModalType_MapsSkillTreeToSkillTreeFocus()
        {
            Assert.AreEqual(UIFocusState.SkillTreeFocus, UIFocusRouter.GetFocusFromModalType(ModalType.SkillTree));
        }

        [Test]
        public void GetFocusFromModalType_MapsPauseToSystemFocus()
        {
            Assert.AreEqual(UIFocusState.SystemFocus, UIFocusRouter.GetFocusFromModalType(ModalType.Pause));
        }

        [Test]
        public void GetFocusFromModalType_NoneReturnsGameplayFocus()
        {
            Assert.AreEqual(UIFocusState.GameplayFocus, UIFocusRouter.GetFocusFromModalType(ModalType.None));
        }
    }

    [TestFixture]
    public class ModalStackRouterTests
    {
        private ModalStackRouter _router;

        [SetUp]
        public void SetUp()
        {
            _router = new ModalStackRouter();
        }

        [Test]
        public void InitialFocus_IsGameplayFocus()
        {
            Assert.AreEqual(UIFocusState.GameplayFocus, _router.CurrentFocus);
        }

        [Test]
        public void InitialDepth_IsZero()
        {
            Assert.AreEqual(0, _router.Depth);
        }

        [Test]
        public void InitialHasActiveModal_IsFalse()
        {
            Assert.IsFalse(_router.HasActiveModal);
        }

        [Test]
        public void PushModal_ChangesCurrentFocus()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
        }

        [Test]
        public void PushModal_IncrementsDepth()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.AreEqual(1, _router.Depth);
        }

        [Test]
        public void PushModal_SetsHasActiveModalTrue()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.IsTrue(_router.HasActiveModal);
        }

        [Test]
        public void PushMultipleModals_StacksCorrectly()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ConfirmationFocus);

            Assert.AreEqual(UIFocusState.ConfirmationFocus, _router.CurrentFocus);
            Assert.AreEqual(2, _router.Depth);
        }

        [Test]
        public void PopModal_RestoresPreviousFocus()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ConfirmationFocus);
            _router.PopModal();

            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
        }

        [Test]
        public void PopModal_DecrementsDepth()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PopModal();
            Assert.AreEqual(0, _router.Depth);
        }

        [Test]
        public void PopModal_EmptyStack_ReturnsGameplayFocus()
        {
            var popped = _router.PopModal();
            Assert.AreEqual(UIFocusState.GameplayFocus, popped);
        }

        [Test]
        public void PeekModal_DoesNotRemove()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            var peeked = _router.PeekModal();

            Assert.AreEqual(UIFocusState.InventoryFocus, peeked);
            Assert.AreEqual(1, _router.Depth);
        }

        [Test]
        public void ClearAllModals_ReturnsToGameplayFocus()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ShopFocus);
            _router.ClearAllModals();

            Assert.AreEqual(UIFocusState.GameplayFocus, _router.CurrentFocus);
            Assert.AreEqual(0, _router.Depth);
            Assert.IsFalse(_router.HasActiveModal);
        }

        [Test]
        public void HasFocus_ReturnsTrueIfOnStack()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.IsTrue(_router.HasFocus(UIFocusState.InventoryFocus));
        }

        [Test]
        public void HasFocus_ReturnsFalseIfNotOnStack()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.IsFalse(_router.HasFocus(UIFocusState.ShopFocus));
        }

        [Test]
        public void HasFocus_FindsDeepStacks()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ConfirmationFocus);

            Assert.IsTrue(_router.HasFocus(UIFocusState.InventoryFocus));
            Assert.IsTrue(_router.HasFocus(UIFocusState.ConfirmationFocus));
        }
    }

    [TestFixture]
    public class ModalBehaviorContractTests
    {
        private ModalStackRouter _router;

        [SetUp]
        public void SetUp()
        {
            _router = new ModalStackRouter();
        }

        [Test]
        public void CanConfirm_ReturnsTrueForModalFocus()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            Assert.IsTrue(ModalBehaviorContract.CanConfirm(_router));
        }

        [Test]
        public void CanConfirm_ReturnsFalseForGameplayFocus()
        {
            Assert.IsFalse(ModalBehaviorContract.CanConfirm(_router));
        }

        [Test]
        public void HandleBackButton_ClosesOnlyTopModal()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ConfirmationFocus);

            var popped = ModalBehaviorContract.HandleBackButton(_router);

            Assert.AreEqual(UIFocusState.ConfirmationFocus, popped);
            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
            Assert.IsTrue(_router.HasActiveModal, "Should still have inventory modal");
        }

        [Test]
        public void HandleBackButton_EmptyStack_ReturnsGameplayFocus()
        {
            var result = ModalBehaviorContract.HandleBackButton(_router);
            Assert.AreEqual(UIFocusState.GameplayFocus, result);
            Assert.AreEqual(UIFocusState.GameplayFocus, _router.CurrentFocus);
        }

        [Test]
        public void HandleBackButton_LastModal_ReturnsToGameplay()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            var popped = ModalBehaviorContract.HandleBackButton(_router);

            Assert.AreEqual(UIFocusState.InventoryFocus, popped);
            Assert.AreEqual(UIFocusState.GameplayFocus, _router.CurrentFocus);
            Assert.IsFalse(_router.HasActiveModal);
        }

        [Test]
        public void OpenSubmodal_PushesConfirmationFocus()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            ModalBehaviorContract.OpenSubmodal(UIFocusState.ConfirmationFocus, _router);

            Assert.AreEqual(UIFocusState.ConfirmationFocus, _router.CurrentFocus);
            Assert.AreEqual(2, _router.Depth);
        }

        [Test]
        public void OpenSubmodal_PushesTooltipFocus()
        {
            _router.PushModal(UIFocusState.ShopFocus);
            ModalBehaviorContract.OpenSubmodal(UIFocusState.TooltipFocus, _router);

            Assert.AreEqual(UIFocusState.TooltipFocus, _router.CurrentFocus);
            Assert.AreEqual(2, _router.Depth);
        }

        [Test]
        public void OpenSubmodal_InvalidFocus_DoesNotPush()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            ModalBehaviorContract.OpenSubmodal(UIFocusState.MenuFocus, _router);

            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
            Assert.AreEqual(1, _router.Depth, "Invalid submodal should not be pushed");
        }

        [Test]
        public void CloseSubmodal_ClosesConfirmation()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            _router.PushModal(UIFocusState.ConfirmationFocus);
            ModalBehaviorContract.CloseSubmodal(_router);

            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
            Assert.AreEqual(1, _router.Depth);
        }

        [Test]
        public void CloseSubmodal_NonSubmodalModal_DoesNotPop()
        {
            _router.PushModal(UIFocusState.InventoryFocus);
            ModalBehaviorContract.CloseSubmodal(_router);

            Assert.AreEqual(UIFocusState.InventoryFocus, _router.CurrentFocus);
            Assert.AreEqual(1, _router.Depth, "Non-submodal should not be closed");
        }
    }

    [TestFixture]
    public class UIFocusRouterIntegrationWithModalTypeTests
    {
        [Test]
        public void AllModalTypes_MapToValidFocusStates()
        {
            var allModalTypes = System.Enum.GetValues(typeof(ModalType)) as ModalType[];
            foreach (var modalType in allModalTypes)
            {
                var focus = UIFocusRouter.GetFocusFromModalType(modalType);
                Assert.IsNotNull(focus, $"ModalType {modalType} should map to a focus state");
            }
        }

        [Test]
        public void MappedFocusStates_BlockGameplayInput()
        {
            var testCases = new[] { ModalType.Dialogue, ModalType.Inventory, ModalType.ShopMenu, ModalType.Crafting };
            foreach (var modalType in testCases)
            {
                var focus = UIFocusRouter.GetFocusFromModalType(modalType);
                Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(focus),
                    $"Modal {modalType} (focus {focus}) should block gameplay input");
            }
        }
    }
}
