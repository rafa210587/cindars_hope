using System.Collections.Generic;

namespace CindarsHope.MainProgression
{
    // Kept separate from QuestStateSection — NEVER merged
    public class MainProgressionSection
    {
        public int Version { get; set; } = 1;
        public MainAct CurrentAct { get; set; } = MainAct.None;
        public string CurrentMainQuestId { get; set; }
        public List<FragmentStateRecord> FragmentStates { get; set; } = new List<FragmentStateRecord>();
        public List<StoryGateRecord> StoryGateStates { get; set; } = new List<StoryGateRecord>();
        public Dictionary<string, string> KnownMajorNpcStates { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> KnownThreatStates { get; set; } = new Dictionary<string, string>();
        public List<string> KnownLoreRevelations { get; set; } = new List<string>();
        public Level100GateStatus Level100GateState { get; set; } = Level100GateStatus.Locked;
        public Level101AccessStatus Level101AccessState { get; set; } = Level101AccessStatus.Locked;
        public FinalChoiceStatus FinalChoiceState { get; set; } = FinalChoiceStatus.Unavailable;
        public string PostGameWorldState { get; set; }
        public int LastValidatedVersion { get; set; } = 0;

        public FragmentStateRecord GetFragment(MainFragmentType type)
        {
            foreach (var f in FragmentStates)
                if (f.FragmentType == type) return f;
            return null;
        }

        public bool IsFragmentIntegrated(MainFragmentType type)
        {
            var f = GetFragment(type);
            return f != null && f.IsIntegrated();
        }

        public StoryGateRecord GetGate(string gateId)
        {
            foreach (var g in StoryGateStates)
                if (g.GateId == gateId) return g;
            return null;
        }

        // Spoiler stage derives from current act — used by UI projection
        public StorySpoilerStage GetCurrentSpoilerStage() => CurrentAct switch
        {
            MainAct.None => StorySpoilerStage.Act1_Intro,
            MainAct.Act1_FonteAndForgetfulness => StorySpoilerStage.Act1_Intro,
            MainAct.Act2_CindarAndMemoryArc => StorySpoilerStage.Act2_MemoryArc,
            MainAct.Act3_CultBlackStoneAndLife => StorySpoilerStage.Act3_CultAndBlackStone,
            MainAct.Act4_Level100101AndHope => StorySpoilerStage.Act4_FinalArcAndLevel101,
            MainAct.PostGame => StorySpoilerStage.Act4_FinalArcAndLevel101,
            _ => StorySpoilerStage.Act1_Intro
        };
    }
}
