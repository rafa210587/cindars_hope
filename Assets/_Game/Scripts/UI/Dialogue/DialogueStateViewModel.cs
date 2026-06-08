using System.Collections.Generic;

namespace CindarsHope.UI.Dialogue
{
    /// <summary>
    /// SPEC 04: Dialogue state projection for compact UI rendering.
    /// Separates dialogue data from rendering, enabling safe modal behavior.
    /// </summary>
    public class DialogueStateViewModel
    {
        public string SpeakerId { get; set; }
        public string SpeakerName { get; set; }
        public string DialogueText { get; set; }
        public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();

        public bool HasChoices => Choices.Count > 0;
        public bool IsCompact => !HasChoices;

        public int SelectedChoiceIndex { get; set; } = -1;

        public DialogueChoice GetSelectedChoice()
        {
            if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < Choices.Count)
                return Choices[SelectedChoiceIndex];
            return null;
        }

        public void SelectNextChoice()
        {
            if (Choices.Count == 0)
                return;
            SelectedChoiceIndex = (SelectedChoiceIndex + 1) % Choices.Count;
        }

        public void SelectPreviousChoice()
        {
            if (Choices.Count == 0)
                return;
            SelectedChoiceIndex = (SelectedChoiceIndex - 1 + Choices.Count) % Choices.Count;
        }

        public void ResetSelection()
        {
            SelectedChoiceIndex = HasChoices ? 0 : -1;
        }
    }

    /// <summary>
    /// Dialogue focus blocker - ensures gameplay input blocked during dialogue.
    /// </summary>
    public static class DialogueFocusPolicy
    {
        public static bool ShouldBlockGameplayInput(DialogueStateViewModel state)
        {
            return state != null && (!string.IsNullOrEmpty(state.DialogueText));
        }

        public static bool AllowsPlayerMovement(DialogueStateViewModel state)
        {
            return !ShouldBlockGameplayInput(state);
        }
    }
}
