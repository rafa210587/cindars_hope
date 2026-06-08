namespace CindarsHope.UI.Fonte
{
    /// <summary>
    /// SPEC 04: Fonte menu flow projection for mana/ritual interactions.
    /// </summary>
    public class FonteMenuViewModel
    {
        public enum FonteAction
        {
            ViewStatus,
            PerformRitual,
            ManaUpgrade,
            RestoreMana,
            Exit
        }

        public string FonteId { get; set; }
        public string DisplayName { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }
        public int ManaPercentage => MaxMana > 0 ? (CurrentMana * 100) / MaxMana : 0;

        public FonteAction SelectedAction { get; set; } = FonteAction.ViewStatus;
        public bool CanPerformRitual { get; set; }
        public bool CanUpgradeMana { get; set; }
        public string RitualRequirement { get; set; }
    }
}
