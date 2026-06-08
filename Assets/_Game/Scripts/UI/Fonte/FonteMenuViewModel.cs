using System.Collections.Generic;

namespace CindarsHope.UI.Fonte
{
    public enum FonteAction
    {
        ViewStatus,
        PerformRitual,
        ManaUpgrade,
        RestoreMana,
        Exit
    }

    public enum FonteVisualStage { Dormant = 0, Awakened = 1, Flowing = 2, Purified = 3, Final = 4 }

    public enum FonteUnlockedFunction
    {
        ViewStatus = 0,
        RestoreMana = 1,
        LivingWaterBasic = 2,
        LivingWaterAdvanced = 3,
        Respec = 4,
        Purification = 5,
        FinalChoice = 6
    }

    public class FonteMenuViewModel
    {
        public string FonteId { get; set; }
        public string DisplayName { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }
        public int ManaPercentage => MaxMana > 0 ? (CurrentMana * 100) / MaxMana : 0;
        public FonteAction SelectedAction { get; set; } = FonteAction.ViewStatus;
        public bool CanPerformRitual { get; set; }
        public bool CanUpgradeMana { get; set; }
        public string RitualRequirement { get; set; }

        // Spec 11 hardening
        public string FonteState { get; set; }
        public FonteVisualStage VisualStage { get; set; }
        public List<FonteUnlockedFunction> UnlockedFunctions { get; set; } = new List<FonteUnlockedFunction>();
        public string LivingWaterProjection { get; set; }
        public string RespecProjection { get; set; }
        public string PurificationProjection { get; set; }
        public string FinalChoiceProjection { get; set; }
        public string HiddenFunctionsPlaceholder { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public bool CanUseSelectedFunction { get; set; }
        public string BlockedReason { get; set; }
        public bool RequiresConfirmation { get; set; }

        // Anti-spoiler gates
        public bool IsRespecVisible => UnlockedFunctions.Contains(FonteUnlockedFunction.Respec);
        public bool IsPurificationVisible => UnlockedFunctions.Contains(FonteUnlockedFunction.Purification);
        public bool IsFinalChoiceVisible => UnlockedFunctions.Contains(FonteUnlockedFunction.FinalChoice);
    }
}
