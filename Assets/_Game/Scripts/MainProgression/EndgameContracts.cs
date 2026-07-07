using System.Collections.Generic;

namespace CindarsHope.MainProgression
{
    public enum Level100GateStatus2
    {
        Unknown = 0, Locked = 1, Hinted = 2, RequirementsKnown = 3,
        Available = 4, Opened = 5, GuardianDefeated = 6, Consumed = 7, Blocked = 8
    }

    public enum Level101AccessStatus2
    {
        Unknown = 0, Forbidden = 1, Hinted = 2, Locked = 3,
        Unlocked = 4, Entered = 5, Completed = 6, PostGameLocked = 7, PostGameOpen = 8
    }

    public enum ArchivistRevealState
    {
        Hidden = 0, Foreshadowed = 1, NameKnown = 2, ConvergenceStarted = 3,
        Revealed = 4, BossActive = 5, Defeated = 6, Resolved = 7
    }

    public enum FinalChoiceType
    {
        None = 0, Protect = 1, Seal = 2, Use = 3
    }

    public enum FinalChoiceStatus2
    {
        Unavailable = 0, Hinted = 1, Available = 2, ConfirmationPending = 3,
        ChosenProtect = 4, ChosenSeal = 5, ChosenUse = 6, Applied = 7, LockedPostGame = 8
    }

    public class EndingEffectProfile
    {
        public string EndingId { get; set; }
        public FinalChoiceType FinalChoiceType { get; set; }
        public string FonteFinalState { get; set; }
        public string ManaBloomPolicy { get; set; }
        public string CavePostGamePolicy { get; set; }
        public string CityMemoryPolicy { get; set; }
        public string BromecianTechPolicy { get; set; }
        public string AdvancedResourcePolicy { get; set; }
        public string CorruptionContainmentPolicy { get; set; }
        public List<string> PostGameUnlockIds { get; set; } = new List<string>();
        public List<string> PostGameLockIds { get; set; } = new List<string>();
        public string WarningTextKey { get; set; }
        public bool RequiresStrongConfirmation { get; set; } = true;

        // Canonical ending effect profiles
        public static EndingEffectProfile Protect() => new EndingEffectProfile
        {
            EndingId = "ending_protect", FinalChoiceType = FinalChoiceType.Protect,
            FonteFinalState = "FinalizedProtected", ManaBloomPolicy = "RareNatural",
            CavePostGamePolicy = "StableLimited", CityMemoryPolicy = "Preserved",
            BromecianTechPolicy = "Reduced", AdvancedResourcePolicy = "SpiritualNatural",
            CorruptionContainmentPolicy = "PurifiedGuarded",
            WarningTextKey = "ending_protect_warning", RequiresStrongConfirmation = true
        };

        public static EndingEffectProfile Seal() => new EndingEffectProfile
        {
            EndingId = "ending_seal", FinalChoiceType = FinalChoiceType.Seal,
            FonteFinalState = "FinalizedSealed", ManaBloomPolicy = "RareRestricted",
            CavePostGamePolicy = "MoreStableReduced", CityMemoryPolicy = "PreservedWithLoss",
            BromecianTechPolicy = "LockedOrReduced", AdvancedResourcePolicy = "Reduced",
            CorruptionContainmentPolicy = "StrongSeal",
            WarningTextKey = "ending_seal_warning", RequiresStrongConfirmation = true
        };

        public static EndingEffectProfile Use() => new EndingEffectProfile
        {
            EndingId = "ending_use", FinalChoiceType = FinalChoiceType.Use,
            FonteFinalState = "FinalizedUsed", ManaBloomPolicy = "MorePredictable",
            CavePostGamePolicy = "NewAreas", CityMemoryPolicy = "MaterialProgress",
            BromecianTechPolicy = "Expanded", AdvancedResourcePolicy = "TechExpanded",
            CorruptionContainmentPolicy = "RiskOfRepeat",
            WarningTextKey = "ending_use_warning", RequiresStrongConfirmation = true
        };
    }

    public class FinalChoiceRequest
    {
        public FinalChoiceType ChoiceType { get; set; }
        public string ActorId { get; set; }
        public int Day { get; set; }
        public string RequiredConfirmationToken { get; set; }
        public bool PreviewOnly { get; set; } = false;
    }

    public interface IFinalChoiceFonteStateSink
    {
        void ApplyFinalFonteState(string fonteFinalState);
    }

    public class FinalChoiceResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public bool ChoiceApplied { get; set; }
        public string EndingEffectProfileId { get; set; }
        public bool MainProgressionUpdated { get; set; }
        public bool FonteStateUpdated { get; set; }
        public bool PostGameModifiersApplied { get; set; }
        public bool AlreadyApplied { get; set; }
        public List<string> DebugNotes { get; set; } = new List<string>();

        public static FinalChoiceResult Fail(string reason) =>
            new FinalChoiceResult { Success = false, FailureReason = reason };
    }
}
