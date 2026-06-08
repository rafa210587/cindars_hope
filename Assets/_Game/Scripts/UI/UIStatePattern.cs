namespace CindarsHope.UI
{
    /// <summary>
    /// SPEC 04: UI state taxonomy for empty, error, blocked, and feature-future states.
    /// Consistent pattern across all screens.
    /// </summary>
    public class UIStatePattern
    {
        public enum StateType
        {
            Normal,
            Empty,
            Blocked,
            Error,
            FeatureFuture
        }

        public StateType State { get; set; } = StateType.Normal;
        public string Message { get; set; }
        public string NextStep { get; set; }
        public bool IsInteractive => State == StateType.Normal;

        public static UIStatePattern Empty(string message, string nextStep = null)
        {
            return new UIStatePattern { State = StateType.Empty, Message = message, NextStep = nextStep };
        }

        public static UIStatePattern Blocked(string reason, string nextStep)
        {
            return new UIStatePattern { State = StateType.Blocked, Message = reason, NextStep = nextStep };
        }

        public static UIStatePattern Error(string message)
        {
            return new UIStatePattern { State = StateType.Error, Message = message };
        }

        public static UIStatePattern FeatureFuture()
        {
            return new UIStatePattern { State = StateType.FeatureFuture, Message = "Feature coming soon" };
        }
    }

    /// <summary>
    /// Confirmation action contract - light vs strong confirmation.
    /// </summary>
    public class ConfirmationAction
    {
        public enum ConfirmationType
        {
            Light,
            Strong
        }

        public string ActionId { get; set; }
        public string ActionLabel { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string Cost { get; set; }
        public string Consequence { get; set; }
        public bool IsReversible { get; set; }
        public ConfirmationType Type { get; set; } = ConfirmationType.Light;

        public bool RequiresConfirmation => Type == ConfirmationType.Strong || !IsReversible;
    }

    /// <summary>
    /// Confirmation validator - ensures safe action dispatch.
    /// </summary>
    public static class ConfirmationValidator
    {
        public static bool NeedsConfirmation(ConfirmationAction action)
        {
            if (action == null)
                return true; // Default to safe (require confirmation)

            return action.Type == ConfirmationAction.ConfirmationType.Strong || !action.IsReversible;
        }

        public static string GetConfirmationMessage(ConfirmationAction action)
        {
            if (action == null)
                return "Are you sure?";

            var parts = new System.Collections.Generic.List<string>();
            parts.Add(action.ActionLabel);
            if (!string.IsNullOrEmpty(action.TargetName))
                parts.Add(action.TargetName);
            if (!string.IsNullOrEmpty(action.Cost))
                parts.Add($"Cost: {action.Cost}");
            if (!string.IsNullOrEmpty(action.Consequence))
                parts.Add(action.Consequence);

            return string.Join(" • ", parts);
        }
    }
}
