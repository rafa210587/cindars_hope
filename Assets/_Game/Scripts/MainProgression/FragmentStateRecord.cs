using System.Collections.Generic;

namespace CindarsHope.MainProgression
{
    // All simple types — no Unity refs (save-safe)
    public class FragmentStateRecord
    {
        public MainFragmentType FragmentType { get; set; }
        public FragmentAcquisitionState AcquisitionState { get; set; } = FragmentAcquisitionState.Unknown;
        public string DiscoveryQuestId { get; set; }
        public int? RecoveredAtDay { get; set; }
        public int? IntegratedAtDay { get; set; }
        public string AssociatedMoon { get; set; }
        public string AssociatedThreat { get; set; }
        public List<string> UnlockedFonteFunctionIds { get; set; } = new List<string>();
        public List<string> KnownLoreRevelationIds { get; set; } = new List<string>();
        public List<string> WorldChangeIds { get; set; } = new List<string>();
        public bool ConsumedByFinalChoice { get; set; } = false;

        public bool IsIntegrated() => AcquisitionState == FragmentAcquisitionState.Integrated;
        public bool IsRecovered() => AcquisitionState >= FragmentAcquisitionState.Recovered;
    }

    public class StoryGateRecord
    {
        public string GateId { get; set; }
        public MainStoryGateStatus Status { get; set; } = MainStoryGateStatus.Unknown;
        public MainAct RequiredAct { get; set; }
        public List<MainFragmentType> RequiredFragments { get; set; } = new List<MainFragmentType>();
        public List<string> RequiredQuestFlags { get; set; } = new List<string>();
        public int? RequiredCaveProgress { get; set; }
        public string RequiredLunarState { get; set; }
        public List<string> RequiredNpcKnowledge { get; set; } = new List<string>();
        public int SpoilerTier { get; set; } = 0;
        public int? OpenedAtDay { get; set; }
    }
}
