namespace CindarsHope.UI.Menus
{
    public enum MenuType { Shop = 0, Crafting = 1, SkillTree = 2, QuestLog = 3, Fonte = 4 }

    public enum MenuCommandType { Confirm = 0, Cancel = 1, Preview = 2, SelectItem = 3, SetQuantity = 4, SelectTab = 5 }

    public enum MenuCommandValidationResult { Valid = 0, InvalidMissingTarget = 1, InvalidInsufficientResources = 2, InvalidBlocked = 3, InvalidSpoilerGate = 4, PreviewOnly = 5 }

    public class MenuCommand
    {
        public string CommandId { get; set; }
        public MenuType MenuType { get; set; }
        public MenuCommandType CommandType { get; set; }
        public string TargetId { get; set; }
        public int? Quantity { get; set; }
        public bool RequiresConfirmation { get; set; }
        public bool PreviewOnly { get; set; }
        public string DomainServiceTarget { get; set; }
        public MenuCommandValidationResult ValidationResult { get; set; } = MenuCommandValidationResult.Valid;
        public string ValidationMessage { get; set; }
        public bool IsValid => ValidationResult == MenuCommandValidationResult.Valid || ValidationResult == MenuCommandValidationResult.PreviewOnly;
    }
}
