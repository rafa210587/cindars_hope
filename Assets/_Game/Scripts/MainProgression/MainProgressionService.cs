using System.Collections.Generic;

namespace CindarsHope.MainProgression
{
    public class ActTransitionResult
    {
        public bool Success { get; set; }
        public MainAct NewAct { get; set; }
        public string FailureReason { get; set; }

        public static ActTransitionResult Fail(string reason) =>
            new ActTransitionResult { Success = false, FailureReason = reason };
    }

    public class FragmentIntegrationResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public bool AlreadyIntegrated { get; set; }

        public static FragmentIntegrationResult Fail(string reason) =>
            new FragmentIntegrationResult { Success = false, FailureReason = reason };
    }

    // Pure C# service — no Unity/MonoBehaviour, no scene state
    public class MainProgressionService
    {
        // Canonical fragment integration order: Water → Memory → Life → Hope
        private static readonly MainFragmentType[] FragmentOrder =
        {
            MainFragmentType.Water, MainFragmentType.Memory, MainFragmentType.Life, MainFragmentType.Hope
        };

        public FragmentIntegrationResult TryIntegrateFragment(
            MainProgressionSection section,
            MainFragmentType fragmentType,
            int currentDay,
            bool alreadyGranted)
        {
            if (section == null) return FragmentIntegrationResult.Fail("SECTION_NULL");

            var existing = section.GetFragment(fragmentType);
            if (existing != null && existing.IsIntegrated())
                return new FragmentIntegrationResult { Success = true, AlreadyIntegrated = true };

            if (alreadyGranted)
                return new FragmentIntegrationResult { Success = true, AlreadyIntegrated = true };

            // Validate order constraint
            var orderCheck = ValidateFragmentOrder(section, fragmentType);
            if (!orderCheck.Valid)
                return FragmentIntegrationResult.Fail(orderCheck.Reason);

            if (existing == null)
            {
                existing = new FragmentStateRecord { FragmentType = fragmentType };
                section.FragmentStates.Add(existing);
            }

            existing.AcquisitionState = FragmentAcquisitionState.Integrated;
            existing.IntegratedAtDay = currentDay;
            return new FragmentIntegrationResult { Success = true };
        }

        private (bool Valid, string Reason) ValidateFragmentOrder(MainProgressionSection section, MainFragmentType target)
        {
            // Water has no prerequisite
            if (target == MainFragmentType.Water) return (true, null);

            int targetIndex = System.Array.IndexOf(FragmentOrder, target);
            if (targetIndex <= 0) return (true, null);

            var prerequisite = FragmentOrder[targetIndex - 1];
            if (!section.IsFragmentIntegrated(prerequisite))
                return (false, $"FRAGMENT_OUT_OF_ORDER:{target} requires {prerequisite} to be integrated first");

            return (true, null);
        }

        // Act transition — idempotent, validates prerequisites
        public ActTransitionResult TryTransitionAct(MainProgressionSection section, MainAct targetAct)
        {
            if (section == null) return ActTransitionResult.Fail("SECTION_NULL");
            if (section.CurrentAct == targetAct)
                return new ActTransitionResult { Success = true, NewAct = targetAct };
            if (targetAct <= section.CurrentAct && targetAct != MainAct.PostGame)
                return ActTransitionResult.Fail($"ACT_REGRESSION:{targetAct} <= current {section.CurrentAct}");

            var prereq = CheckActPrerequisites(section, targetAct);
            if (!prereq.Valid)
                return ActTransitionResult.Fail(prereq.Reason);

            section.CurrentAct = targetAct;
            return new ActTransitionResult { Success = true, NewAct = targetAct };
        }

        private (bool Valid, string Reason) CheckActPrerequisites(MainProgressionSection section, MainAct target)
        {
            switch (target)
            {
                case MainAct.Act2_CindarAndMemoryArc:
                    if (!section.IsFragmentIntegrated(MainFragmentType.Water))
                        return (false, "ACT2_REQUIRES_WATER_INTEGRATED");
                    break;

                case MainAct.Act3_CultBlackStoneAndLife:
                    if (!section.IsFragmentIntegrated(MainFragmentType.Memory))
                        return (false, "ACT3_REQUIRES_MEMORY_INTEGRATED");
                    break;

                case MainAct.Act4_Level100101AndHope:
                    if (!section.IsFragmentIntegrated(MainFragmentType.Life))
                        return (false, "ACT4_REQUIRES_LIFE_INTEGRATED");
                    break;

                case MainAct.PostGame:
                    if (section.FinalChoiceState != FinalChoiceStatus.Resolved)
                        return (false, "POSTGAME_REQUIRES_FINAL_CHOICE_RESOLVED");
                    break;
            }
            return (true, null);
        }

        // Returns what may be revealed at the current spoiler stage — anti-spoiler projection
        public bool IsRevealAllowed(string revealType, MainProgressionSection section)
        {
            var stage = section?.GetCurrentSpoilerStage() ?? StorySpoilerStage.Act1_Intro;

            switch (revealType)
            {
                case "Cindar_FullIdentity":
                case "MemoryArc_Term":
                    return stage >= StorySpoilerStage.Act2_MemoryArc;

                case "Sethra_LeaderReveal":
                case "Archivist_Final":
                case "FinalChoice_Details":
                    return stage >= StorySpoilerStage.Act4_FinalArcAndLevel101;

                case "Level101_Content":
                case "Vaelrion_Antagonist":
                    return stage >= StorySpoilerStage.Act3_CultAndBlackStone;

                default:
                    return true;
            }
        }
    }
}
