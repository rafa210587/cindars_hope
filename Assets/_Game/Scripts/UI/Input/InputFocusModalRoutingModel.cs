namespace CindarsHope.UI.Input
{
    /// <summary>
    /// SPEC 04: Input/Focus/Modal routing contract - blocks gameplay input during UI.
    /// Foundation for all other UI specs.
    /// </summary>
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
    /// Modal routing contract - which modal can be open.
    /// </summary>
    public class ModalRoutingPolicy
    {
        public static bool CanOpenModal(string modalType, InputFocusState currentState)
        {
            if (currentState == null || currentState.CurrentFocus == InputFocusState.FocusTarget.Gameplay)
                return true;

            // Only one modal open at a time; secondary modals can't open over top-level
            return modalType == "Confirmation" || modalType == "Tooltip";
        }
    }
}
