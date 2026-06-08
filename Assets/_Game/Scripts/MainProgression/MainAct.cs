namespace CindarsHope.MainProgression
{
    public enum MainAct
    {
        None = 0,
        Act1_FonteAndForgetfulness = 1,
        Act2_CindarAndMemoryArc = 2,
        Act3_CultBlackStoneAndLife = 3,
        Act4_Level100101AndHope = 4,
        PostGame = 5
    }

    public enum MainFragmentType
    {
        Water = 0, Memory = 1, Life = 2, Hope = 3
    }

    public enum FragmentAcquisitionState
    {
        Unknown = 0, Hinted = 1, Located = 2, Contested = 3, Recovered = 4,
        BroughtToFonte = 5, Integrated = 6, Corrupted = 7, Sealed = 8, Protected = 9, Used = 10
    }

    public enum MainStoryGateStatus
    {
        Unknown = 0, Locked = 1, Hinted = 2, Available = 3,
        Opened = 4, Resolved = 5, Sealed = 6, FailedSafe = 7
    }

    public enum Level100GateStatus
    {
        Locked = 0, Approaching = 1, Available = 2, Entered = 3
    }

    public enum Level101AccessStatus
    {
        Locked = 0, Unlocked = 1, Resolved = 2
    }

    public enum FinalChoiceStatus
    {
        Unavailable = 0, Available = 1, Resolved = 2
    }

    // Spoiler staging per act — what may be revealed
    public enum StorySpoilerStage
    {
        Act1_Intro = 1,
        Act2_MemoryArc = 2,
        Act3_CultAndBlackStone = 3,
        Act4_FinalArcAndLevel101 = 4
    }

    // Integration event names for MainProgression — never mutate quest/cave/inventory directly
    public static class MainProgressionEventName
    {
        public const string OnMainActChanged             = "OnMainActChanged";
        public const string OnMainFragmentHinted         = "OnMainFragmentHinted";
        public const string OnMainFragmentRecovered      = "OnMainFragmentRecovered";
        public const string OnMainFragmentIntegrated     = "OnMainFragmentIntegrated";
        public const string OnMainStoryGateOpened        = "OnMainStoryGateOpened";
        public const string OnLevel100GateAvailable      = "OnLevel100GateAvailable";
        public const string OnLevel101AccessUnlocked     = "OnLevel101AccessUnlocked";
        public const string OnFinalChoiceAvailable       = "OnFinalChoiceAvailable";
        public const string OnFinalChoiceResolved        = "OnFinalChoiceResolved";
    }
}
