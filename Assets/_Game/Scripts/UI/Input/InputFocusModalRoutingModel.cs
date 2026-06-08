using CindarsHope.UI.Modal;

namespace CindarsHope.UI.InputRouting
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
    /// Provides contract mapping with ModalManager (GetFocusFromModalType) but maintains separate
    /// focus state routing. Concrete runtime synchronization with ModalManager is deferred to
    /// integration in concrete modal UI specs (Inventory, Shop, Dialogue, etc.).
    /// </summary>
    public static class UIFocusRouter
    {
        /// <summary>
        /// Returns true if this focus state blocks gameplay movement/combat/hotbar input.
        /// Only GameplayFocus and DebugFocus allow gameplay input.
        /// NOTE: DebugFocus policy is fixed (allows gameplay). Configuration deferred to future debug system spec.
        /// </summary>
        public static bool BlocksGameplayInput(UIFocusState focus)
        {
            return focus switch
            {
                UIFocusState.GameplayFocus => false,
                UIFocusState.DebugFocus => false, // Fixed policy: DebugFocus allows gameplay input. Config deferred.
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
    /// NOTE: This contract is headless/adapter-only. Concrete runtime synchronization with
    /// ModalManager (type checking, event publishing) is deferred to integration specs.
    /// </summary>
    public static class ModalBehaviorContract
    {
        /// <summary>
        /// When Back/Cancel is pressed: if modal stack has items, pop top modal from router.
        /// This restores the previous focus. Does NOT interact with ModalManager directly.
        ///
        /// Integration note: Concrete modal close on ModalManager (TryPopModal, event publish)
        /// is handled by the ModalBase.CloseModal() that manages ModalManager lifecycle.
        /// This contract only manages focus state routing.
        /// </summary>
        public static UIFocusState HandleBackButton(ModalStackRouter router)
        {
            if (router?.HasActiveModal == true)
            {
                return router.PopModal();
            }
            return UIFocusState.GameplayFocus; // No-op if already at gameplay
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
        /// Open a submodal (Confirmation, Tooltip) on the focus stack.
        /// Submodals use the stack but semantically sit "above" the current modal.
        ///
        /// Contract: Only ConfirmationFocus and TooltipFocus are valid submodals.
        /// Pushing a submodal changes CurrentFocus to the submodal type until it's popped.
        /// </summary>
        public static void OpenSubmodal(UIFocusState submodalFocus, ModalStackRouter router)
        {
            if (submodalFocus == UIFocusState.ConfirmationFocus || submodalFocus == UIFocusState.TooltipFocus)
            {
                router?.PushModal(submodalFocus);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"OpenSubmodal: {submodalFocus} is not a valid submodal type. Use ConfirmationFocus or TooltipFocus.");
            }
        }

        /// <summary>
        /// Close the current submodal if it is one of the valid submodal types.
        /// </summary>
        public static void CloseSubmodal(ModalStackRouter router)
        {
            var current = router?.CurrentFocus ?? UIFocusState.GameplayFocus;
            if (current == UIFocusState.ConfirmationFocus || current == UIFocusState.TooltipFocus)
            {
                router?.PopModal();
            }
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
