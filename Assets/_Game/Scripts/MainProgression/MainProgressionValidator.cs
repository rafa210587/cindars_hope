using System.Collections.Generic;

namespace CindarsHope.MainProgression
{
    public class MainProgressionValidationIssue
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class MainProgressionValidator
    {
        public List<MainProgressionValidationIssue> Validate(MainProgressionSection section)
        {
            var issues = new List<MainProgressionValidationIssue>();
            if (section == null) { issues.Add(B("SECTION_NULL", "MainProgressionSection is null")); return issues; }

            // Fragment order integrity
            bool waterIntegrated = section.IsFragmentIntegrated(MainFragmentType.Water);
            bool memoryIntegrated = section.IsFragmentIntegrated(MainFragmentType.Memory);
            bool lifeIntegrated = section.IsFragmentIntegrated(MainFragmentType.Life);
            bool hopeIntegrated = section.IsFragmentIntegrated(MainFragmentType.Hope);

            if (memoryIntegrated && !waterIntegrated)
                issues.Add(B("FRAGMENT_ORDER_MEMORY_WITHOUT_WATER", "Memory integrated without Water — out-of-order"));
            if (lifeIntegrated && !memoryIntegrated)
                issues.Add(B("FRAGMENT_ORDER_LIFE_WITHOUT_MEMORY", "Life integrated without Memory — out-of-order"));
            if (hopeIntegrated && !lifeIntegrated)
                issues.Add(B("FRAGMENT_ORDER_HOPE_WITHOUT_LIFE", "Hope integrated without Life — out-of-order"));

            // Act consistency
            if (section.CurrentAct >= MainAct.Act2_CindarAndMemoryArc && !waterIntegrated)
                issues.Add(W("ACT2_WATER_NOT_INTEGRATED", "Act 2+ but Water fragment not integrated — possible gap"));
            if (section.CurrentAct >= MainAct.Act3_CultBlackStoneAndLife && !memoryIntegrated)
                issues.Add(W("ACT3_MEMORY_NOT_INTEGRATED", "Act 3+ but Memory fragment not integrated — possible gap"));

            // Duplicate fragment records
            var seen = new HashSet<MainFragmentType>();
            foreach (var f in section.FragmentStates)
            {
                if (!seen.Add(f.FragmentType))
                    issues.Add(W($"FRAGMENT_DUPLICATE:{f.FragmentType}", $"Duplicate FragmentStateRecord for {f.FragmentType}"));
            }

            // FonteAnya separation guard — section must not contain QuestState data directly
            // (structural check: this is a separate section, not a sub-section)
            if (section.Version <= 0)
                issues.Add(W("SECTION_VERSION_ZERO", "MainProgressionSection.Version is 0 — may not have been initialized"));

            // Final choice state consistency
            if (section.FinalChoiceState == FinalChoiceStatus.Resolved &&
                section.CurrentAct != MainAct.PostGame)
                issues.Add(W("FINAL_CHOICE_RESOLVED_NOT_POSTGAME", "FinalChoice is Resolved but Act is not PostGame"));

            return issues;
        }

        private MainProgressionValidationIssue B(string code, string msg) =>
            new MainProgressionValidationIssue { Code = code, Message = msg, IsBlocker = true };

        private MainProgressionValidationIssue W(string code, string msg) =>
            new MainProgressionValidationIssue { Code = code, Message = msg, IsBlocker = false };
    }
}
