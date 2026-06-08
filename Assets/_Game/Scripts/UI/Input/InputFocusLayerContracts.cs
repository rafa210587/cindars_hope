using System.Collections.Generic;

namespace CindarsHope.UI.InputRouting
{
    public enum ModalKind
    {
        None = 0, LightOverlay = 1, ModalGameplayMenu = 2, DialogueModal = 3,
        ConfirmationModal = 4, SystemMenu = 5, DebugOverlay = 6
    }

    // Read-only layer record — all state is determined at open time
    public class UIFocusLayer
    {
        public string LayerId { get; set; }
        public UIFocusState FocusState { get; set; }
        public ModalKind ModalKind { get; set; }
        public bool BlocksGameplayMovement { get; set; }
        public bool BlocksGameplayActions { get; set; }
        public bool BlocksHotbar { get; set; }
        public bool ConsumesInteract { get; set; }
        public bool ConsumesCancel { get; set; }
        public bool ConsumesConfirm { get; set; }
        public string ParentLayerId { get; set; }
        public string OpenedBy { get; set; }
        public int? OpenedAtFrame { get; set; }
        public string DebugName { get; set; }

        public bool IsGameplayAllowed => FocusState == UIFocusState.GameplayFocus || FocusState == UIFocusState.DebugFocus;

        // Build a layer from a focus state with canonical defaults
        public static UIFocusLayer FromFocus(UIFocusState focus, string layerId, string openedBy = null)
        {
            bool isGameplay = focus == UIFocusState.GameplayFocus || focus == UIFocusState.DebugFocus;
            bool isSystem = focus == UIFocusState.SystemFocus;
            return new UIFocusLayer
            {
                LayerId = layerId,
                FocusState = focus,
                ModalKind = isSystem ? ModalKind.SystemMenu : (isGameplay ? ModalKind.None : ModalKind.ModalGameplayMenu),
                BlocksGameplayMovement = !isGameplay,
                BlocksGameplayActions = !isGameplay,
                BlocksHotbar = !isGameplay,
                ConsumesInteract = !isGameplay,
                ConsumesCancel = !isGameplay,
                ConsumesConfirm = !isGameplay,
                OpenedBy = openedBy,
                DebugName = focus.ToString()
            };
        }
    }

    // Routing decision record — result of input routing lookup
    public class InputRoutingDecision
    {
        public enum RoutingTarget { Gameplay, UI, Blocked }

        public string InputActionId { get; set; }
        public RoutingTarget Target { get; set; }
        public UIFocusState FocusState { get; set; }
        public string LayerId { get; set; }
        public string Reason { get; set; }
        public bool CanBubble { get; set; }
        public List<string> DebugNotes { get; set; } = new List<string>();

        public bool IsAllowed => Target != RoutingTarget.Blocked;

        public static InputRoutingDecision Allow(string actionId, UIFocusState focus, string layerId, bool forUI = false) =>
            new InputRoutingDecision
            {
                InputActionId = actionId,
                Target = forUI ? RoutingTarget.UI : RoutingTarget.Gameplay,
                FocusState = focus, LayerId = layerId,
                Reason = "ALLOWED"
            };

        public static InputRoutingDecision Block(string actionId, UIFocusState focus, string layerId, string reason) =>
            new InputRoutingDecision
            {
                InputActionId = actionId,
                Target = RoutingTarget.Blocked,
                FocusState = focus, LayerId = layerId,
                Reason = reason
            };
    }

    // Gameplay input gate — determines if a gameplay action is allowed
    public static class GameplayInputGate
    {
        public static InputRoutingDecision RouteMovement(UIFocusState focus, string layerId) =>
            UIFocusRouter.CanMove(focus)
                ? InputRoutingDecision.Allow("move", focus, layerId)
                : InputRoutingDecision.Block("move", focus, layerId, "MODAL_BLOCKS_MOVEMENT");

        public static InputRoutingDecision RouteAttack(UIFocusState focus, string layerId) =>
            UIFocusRouter.CanCombat(focus)
                ? InputRoutingDecision.Allow("attack", focus, layerId)
                : InputRoutingDecision.Block("attack", focus, layerId, "MODAL_BLOCKS_COMBAT");

        public static InputRoutingDecision RouteDash(UIFocusState focus, string layerId) =>
            UIFocusRouter.CanCombat(focus)
                ? InputRoutingDecision.Allow("dash", focus, layerId)
                : InputRoutingDecision.Block("dash", focus, layerId, "MODAL_BLOCKS_DASH");

        public static InputRoutingDecision RouteInteract(UIFocusState focus, string layerId) =>
            UIFocusRouter.CanInteractWorld(focus)
                ? InputRoutingDecision.Allow("interact", focus, layerId)
                : InputRoutingDecision.Allow("interact", focus, layerId, forUI: true);

        public static InputRoutingDecision RouteHotbar(UIFocusState focus, string layerId) =>
            UIFocusRouter.CanUseHotbar(focus)
                ? InputRoutingDecision.Allow("hotbar", focus, layerId)
                : InputRoutingDecision.Block("hotbar", focus, layerId, "MODAL_BLOCKS_HOTBAR");

        public static InputRoutingDecision RouteEscCancel(UIFocusState focus, string layerId) =>
            focus == UIFocusState.GameplayFocus
                ? InputRoutingDecision.Allow("esc", focus, layerId) // system/pause
                : InputRoutingDecision.Allow("esc", focus, layerId, forUI: true); // close top layer
    }
}
