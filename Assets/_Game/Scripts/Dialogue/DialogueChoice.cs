namespace CindarsHope.UI.Dialogue
{
    /// <summary>
    /// SPEC 04: Dialogue choice contract - action markers and hooks.
    /// UI reads this to display choice correctly and dispatch action.
    /// </summary>
    public class DialogueChoice
    {
        public enum ChoiceActionType
        {
            Neutral,
            AcceptQuest,
            DeliverQuest,
            ShopBuy,
            ShopSell,
            ServiceStart,
            FonteInteraction,
            Goodbye
        }

        public string Label { get; set; }
        public string ChoiceId { get; set; }
        public ChoiceActionType ActionType { get; set; } = ChoiceActionType.Neutral;
        public string TargetId { get; set; }
        public bool RequiresConfirmation { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string DisabledReason { get; set; }

        public DialogueChoice() { }

        public DialogueChoice(string label, string choiceId)
        {
            Label = label;
            ChoiceId = choiceId;
        }
    }

    /// <summary>
    /// Dialogue choice validator - ensures safe action dispatch.
    /// </summary>
    public static class DialogueChoiceValidator
    {
        public static bool CanExecuteChoice(DialogueChoice choice)
        {
            if (choice == null)
                return false;
            if (!choice.IsEnabled)
                return false;
            return true;
        }

        public static bool NeedsConfirmation(DialogueChoice choice)
        {
            if (choice == null)
                return false;

            return choice.ActionType switch
            {
                DialogueChoice.ChoiceActionType.DeliverQuest => true,
                DialogueChoice.ChoiceActionType.ShopBuy => true,
                DialogueChoice.ChoiceActionType.FonteInteraction => true,
                DialogueChoice.ChoiceActionType.ServiceStart => true,
                _ => choice.RequiresConfirmation
            };
        }
    }
}
