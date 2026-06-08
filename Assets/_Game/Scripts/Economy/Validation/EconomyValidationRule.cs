namespace CindarsHope.Economy.Validation
{
    public enum ValidationSeverity { Info = 0, Warning, Error, Blocker }

    public enum ValidationScope
    {
        Item, PriceChannel, Shop, Stock, Recipe, Loot, Reward, Save
    }

    public class EconomyValidationRule
    {
        public string RuleId { get; set; }
        public string RuleType { get; set; }
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
        public ValidationScope Scope { get; set; } = ValidationScope.PriceChannel;
        public string[] Inputs { get; set; } = new string[0];
        public string ExpectedInvariant { get; set; }
        public string FailureMessage { get; set; }
        public string SuggestedAction { get; set; }
    }
}
