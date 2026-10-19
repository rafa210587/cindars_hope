using NUnit.Framework;
using CindarsHope.UI.InputRouting;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class InputFocusLayerContractTests
    {
        // ---- New focus states ----

        [Test]
        public void NewFocusStates_BlockGameplayInput()
        {
            Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(UIFocusState.SocialLogFocus));
            Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(UIFocusState.CalendarFocus));
            Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(UIFocusState.MapFocus));
            Assert.IsTrue(UIFocusRouter.BlocksGameplayInput(UIFocusState.FonteFocus));
        }

        [Test]
        public void NewFocusStates_BlockMovement()
        {
            Assert.IsFalse(UIFocusRouter.CanMove(UIFocusState.SocialLogFocus));
            Assert.IsFalse(UIFocusRouter.CanMove(UIFocusState.FonteFocus));
        }

        // ---- UIFocusLayer tests ----

        [Test]
        public void UIFocusLayer_FromFocus_GameplayFocus_AllowsGameplay()
        {
            var layer = UIFocusLayer.FromFocus(UIFocusState.GameplayFocus, "layer_gameplay");
            Assert.IsTrue(layer.IsGameplayAllowed);
            Assert.IsFalse(layer.BlocksGameplayMovement);
            Assert.AreEqual(ModalKind.None, layer.ModalKind);
        }

        [Test]
        public void UIFocusLayer_FromFocus_InventoryFocus_BlocksAll()
        {
            var layer = UIFocusLayer.FromFocus(UIFocusState.InventoryFocus, "layer_inv");
            Assert.IsFalse(layer.IsGameplayAllowed);
            Assert.IsTrue(layer.BlocksGameplayMovement);
            Assert.IsTrue(layer.BlocksGameplayActions);
            Assert.IsTrue(layer.BlocksHotbar);
            Assert.AreEqual(ModalKind.ModalGameplayMenu, layer.ModalKind);
        }

        [Test]
        public void UIFocusLayer_FromFocus_SystemFocus_IsSystemMenu()
        {
            var layer = UIFocusLayer.FromFocus(UIFocusState.SystemFocus, "layer_sys");
            Assert.AreEqual(ModalKind.SystemMenu, layer.ModalKind);
        }

        [Test]
        public void UIFocusLayer_ParentLayerId_Settable()
        {
            var parent = UIFocusLayer.FromFocus(UIFocusState.InventoryFocus, "inv");
            var child = UIFocusLayer.FromFocus(UIFocusState.ConfirmationFocus, "confirm");
            child.ParentLayerId = parent.LayerId;
            Assert.AreEqual("inv", child.ParentLayerId);
        }

        // ---- InputRoutingDecision tests ----

        [Test]
        public void InputRoutingDecision_Allow_IsAllowed()
        {
            var decision = InputRoutingDecision.Allow("move", UIFocusState.GameplayFocus, "layer_gp");
            Assert.IsTrue(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.Gameplay, decision.Target);
        }

        [Test]
        public void InputRoutingDecision_Block_IsNotAllowed()
        {
            var decision = InputRoutingDecision.Block("move", UIFocusState.InventoryFocus, "layer_inv", "MODAL_BLOCKS_MOVEMENT");
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.Blocked, decision.Target);
            Assert.AreEqual("MODAL_BLOCKS_MOVEMENT", decision.Reason);
        }

        // ---- GameplayInputGate tests ----

        [Test]
        public void GameplayInputGate_Movement_AllowedInGameplay()
        {
            var decision = GameplayInputGate.RouteMovement(UIFocusState.GameplayFocus, "layer_gp");
            Assert.IsTrue(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.Gameplay, decision.Target);
        }

        [Test]
        public void GameplayInputGate_Movement_BlockedInInventory()
        {
            var decision = GameplayInputGate.RouteMovement(UIFocusState.InventoryFocus, "layer_inv");
            Assert.IsFalse(decision.IsAllowed);
        }

        [Test]
        public void GameplayInputGate_Attack_BlockedInDialogue()
        {
            var decision = GameplayInputGate.RouteAttack(UIFocusState.DialogueFocus, "layer_dial");
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual("MODAL_BLOCKS_COMBAT", decision.Reason);
        }

        [Test]
        public void GameplayInputGate_Dash_BlockedInShop()
        {
            var decision = GameplayInputGate.RouteDash(UIFocusState.ShopFocus, "layer_shop");
            Assert.IsFalse(decision.IsAllowed);
        }

        [Test]
        public void GameplayInputGate_Interact_RoutesToUI_WhenModal()
        {
            var decision = GameplayInputGate.RouteInteract(UIFocusState.ShopFocus, "layer_shop");
            Assert.IsTrue(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.UI, decision.Target);
        }

        [Test]
        public void GameplayInputGate_Interact_RoutesToGameplay_InGameplayFocus()
        {
            var decision = GameplayInputGate.RouteInteract(UIFocusState.GameplayFocus, "layer_gp");
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.Gameplay, decision.Target);
        }

        [Test]
        public void GameplayInputGate_Hotbar_BlockedInModal()
        {
            var decision = GameplayInputGate.RouteHotbar(UIFocusState.InventoryFocus, "layer_inv");
            Assert.IsFalse(decision.IsAllowed);
        }

        [Test]
        public void GameplayInputGate_EscCancel_AllowedInGameplay()
        {
            var decision = GameplayInputGate.RouteEscCancel(UIFocusState.GameplayFocus, "layer_gp");
            Assert.IsTrue(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.Gameplay, decision.Target);
        }

        [Test]
        public void GameplayInputGate_EscCancel_RoutesToUI_WhenModal()
        {
            var decision = GameplayInputGate.RouteEscCancel(UIFocusState.InventoryFocus, "layer_inv");
            Assert.IsTrue(decision.IsAllowed);
            Assert.AreEqual(InputRoutingDecision.RoutingTarget.UI, decision.Target);
        }
    }
}
