using CindarsHope.UI.Modal;

namespace CindarsHope.UI.Input
{
    /// <summary>
    /// SPEC 04: UI Focus State — 10 mandatory focus states per spec requirement.
    /// Maps gameplay/UI modals to focus states and determines input blocking rules.
    /// </summary>
    public enum UIFocusState
    {
        GameplayFocus,
        DialogueFocus,
        MenuFocus,
        ShopFocus,
        InventoryFocus,
        CraftingFocus,
        SkillTreeFocus,
        QuestLogFocus,
        SystemFocus,
        DebugFocus,
        // Auxiliary substates (not primary modal focus):
        ConfirmationFocus,
        TooltipFocus
    }

    /// <summary>
    /// SPEC 04: UI Focus Router — determines gameplay input blocking and modal focus rules.
    /// Integrates with existing ModalManager to provide focus routing contract.
    /// </summary>
    public static class UIFocusRouter
    {
        /// <summary>
        /// Returns true if this focus state blocks gameplay movement/combat/hotbar input.
        /// Only GameplayFocus and DebugFocus (configurable) allow gameplay input.
        /// </summary>
        public static bool BlocksGameplayInput(UIFocusState focus)
        {
            return focus switch
            {
                UIFocusState.GameplayFocus => false,
                UIFocusState.DebugFocus => false, // Debug focus can allow gameplay input if configured
                _ => true // All modal focuses block gameplay input
            };
        }

        /// <summary>
        /// Determine focus state from ModalType (integration with existing ModalManager).
        /// </summary>
        public static UIFocusState GetFocusFromModalType(ModalType modalType)
        {
            return modalType switch
            {
                ModalType.None => UIFocusState.GameplayFocus,
                ModalType.Dialogue => UIFocusState.DialogueFocus,
                ModalType.ShopMenu or ModalType.Buy or ModalType.Sell => UIFocusState.ShopFocus,
                ModalType.Inventory => UIFocusState.InventoryFocus,
                ModalType.Crafting => UIFocusState.CraftingFocus,
                ModalType.SkillTree => UIFocusState.SkillTreeFocus,
                ModalType.Pause => UIFocusState.SystemFocus,
                ModalType.Death or ModalType.CorpseRecovery => UIFocusState.SystemFocus,
                ModalType.CharacterEquipment => UIFocusState.InventoryFocus,
                ModalType.AnyaFountain => UIFocusState.ShopFocus,
                ModalType.CaveCheckpoint => UIFocusState.SystemFocus,
                _ => UIFocusState.GameplayFocus
            };
        }

        /// <summary>
        /// Contract: only GameplayFocus allows movement.
        /// </summary>
        public static bool CanMove(UIFocusState focus) => focus == UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: only GameplayFocus allows combat input (attack, dash, block).
        /// </summary>
        public static bool CanCombat(UIFocusState focus) => focus == UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: only GameplayFocus allows hotbar input.
        /// </summary>
        public static bool CanUseHotbar(UIFocusState focus) => focus == UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: only GameplayFocus allows world interaction (Fonte, objects, NPCs).
        /// </summary>
        public static bool CanInteractWorld(UIFocusState focus) => focus == UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: all non-gameplay focus states allow UI navigation.
        /// </summary>
        public static bool CanNavigateUI(UIFocusState focus) => focus != UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: UI confirm/accept button submits current UI action.
        /// Not valid in GameplayFocus (interact with world instead).
        /// </summary>
        public static bool CanConfirmUI(UIFocusState focus) => focus != UIFocusState.GameplayFocus;

        /// <summary>
        /// Contract: UI cancel/back button closes current modal or navigates back in menu.
        /// Not valid in GameplayFocus.
        /// </summary>
        public static bool CanCancelUI(UIFocusState focus) => focus != UIFocusState.GameplayFocus;
    }

    /// <summary>
    /// SPEC 04: Modal Stack Router — manages modal open/close sequences and focus transitions.
    /// Used by ModalManager integration; headless (no scene/prefab references).
    /// </summary>
    public class ModalStackRouter
    {
        private System.Collections.Generic.Stack<UIFocusState> _focusStack = new();

        public UIFocusState CurrentFocus => _focusStack.Count > 0 ? _focusStack.Peek() : UIFocusState.GameplayFocus;
        public int Depth => _focusStack.Count;
        public bool HasActiveModal => _focusStack.Count > 0;

        /// <summary>
        /// Push a new modal focus onto the stack. Blocks gameplay input.
        /// </summary>
        public void PushModal(UIFocusState focus)
        {
            if (focus == UIFocusState.GameplayFocus)
            {
                UnityEngine.Debug.LogWarning("Cannot push GameplayFocus; use PopModal instead.");
                return;
            }
            _focusStack.Push(focus);
        }

        /// <summary>
        /// Pop the top modal focus from the stack. Restores previous focus.
        /// </summary>
        public UIFocusState PopModal()
        {
            if (_focusStack.Count == 0)
            {
                UnityEngine.Debug.LogWarning("Cannot pop empty modal stack.");
                return UIFocusState.GameplayFocus;
            }
            return _focusStack.Pop();
        }

        /// <summary>
        /// Peek at the top modal focus without removing it.
        /// </summary>
        public UIFocusState PeekModal()
        {
            return _focusStack.Count > 0 ? _focusStack.Peek() : UIFocusState.GameplayFocus;
        }

        /// <summary>
        /// Clear all modals from the stack and return to GameplayFocus.
        /// </summary>
        public void ClearAllModals()
        {
            _focusStack.Clear();
        }

        /// <summary>
        /// Check if a specific focus is currently on the stack (for debugging/validation).
        /// </summary>
        public bool HasFocus(UIFocusState focus)
        {
            foreach (var f in _focusStack)
            {
                if (f == focus) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// SPEC 04: Back/Cancel/Confirm Behavior Contract.
    /// Defines routing rules for modal closing and UI navigation.
    /// </summary>
    public static class ModalBehaviorContract
    {
        /// <summary>
        /// When Back/Cancel is pressed: if modal stack has items, close top modal.
        /// Otherwise, no action (gameplay continues).
        /// </summary>
        public static void HandleBackButton(ModalStackRouter router, ModalManager manager)
        {
            if (router?.HasActiveModal == true)
            {
                router.PopModal();
                manager?.ClearAllModals();
            }
        }

        /// <summary>
        /// When Confirm is pressed: only valid if not in GameplayFocus.
        /// Subclass UI panels handle specific confirm logic.
        /// </summary>
        public static bool CanConfirm(ModalStackRouter router)
        {
            return router?.CurrentFocus != UIFocusState.GameplayFocus;
        }

        /// <summary>
        /// Submodal (Confirmation, Tooltip) does not become primary modal focus;
        /// it sits above the current focus but does not change CurrentFocus.
        /// </summary>
        public static void OpenSubmodal(string submodalType, ModalStackRouter router)
        {
            // Submodals are rendered on top but don't change focus routing.
            // They are closed by next action or explicit dismiss.
            // Implementation: UI layer handles visibility; router doesn't track submodals.
        }
    }

    /// <summary>
    /// Legacy: InputFocusState for backward compatibility with WAVE 04 code created before rework.
    /// Maps old FocusTarget enum to new UIFocusState. Deprecated in favor of UIFocusState + UIFocusRouter.
    /// </summary>
    [System.Obsolete("Use UIFocusState and UIFocusRouter instead.")]
    public class InputFocusState
    {
        public enum FocusTarget
        {
            Gameplay,
            MenuTop,
            MenuSecondary,
            DialogueTop,
            ConfirmationTop
        }

        public FocusTarget CurrentFocus { get; set; } = FocusTarget.Gameplay;
        public bool IsGameplayInputBlocked => CurrentFocus != FocusTarget.Gameplay;
        public bool CanPlayerMove => CurrentFocus == FocusTarget.Gameplay;
        public bool CanPlayerInteract => CurrentFocus == FocusTarget.Gameplay;

        public void BlockGameplayInput(FocusTarget newFocus)
        {
            if (newFocus != FocusTarget.Gameplay)
                CurrentFocus = newFocus;
        }

        public void UnblockGameplayInput()
        {
            CurrentFocus = FocusTarget.Gameplay;
        }
    }

    /// <summary>
    /// Legacy: ModalRoutingPolicy for backward compatibility.
    /// Deprecated in favor of UIFocusRouter.
    /// </summary>
    [System.Obsolete("Use UIFocusRouter instead.")]
    public class ModalRoutingPolicy
    {
        public static bool CanOpenModal(string modalType, InputFocusState currentState)
        {
            if (currentState == null || currentState.CurrentFocus == InputFocusState.FocusTarget.Gameplay)
                return true;

            return modalType == "Confirmation" || modalType == "Tooltip";
        }
    }
}
