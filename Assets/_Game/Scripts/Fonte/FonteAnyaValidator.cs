using System.Collections.Generic;

namespace CindarsHope.Fonte
{
    public class FonteValidationIssue
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class FonteAnyaValidator
    {
        public List<FonteValidationIssue> Validate(FonteAnyaSection section)
        {
            var issues = new List<FonteValidationIssue>();
            if (section == null) { issues.Add(B("SECTION_NULL", "FonteAnyaSection is null")); return issues; }

            // Living water exploit guard
            if (section.LivingWater.Unlocked && section.LivingWater.MaxCharges <= 0)
                issues.Add(B("LIVING_WATER_ZERO_MAX_CHARGES", "LivingWater.MaxCharges must be > 0 when unlocked"));

            if (section.LivingWater.Unlocked && section.LivingWater.CurrentCharges < 0)
                issues.Add(B("LIVING_WATER_NEGATIVE_CHARGES", "LivingWater.CurrentCharges cannot be negative"));

            if (section.LivingWater.Unlocked && section.LivingWater.CurrentCharges > section.LivingWater.MaxCharges)
                issues.Add(W("LIVING_WATER_OVER_MAX", "LivingWater.CurrentCharges exceeds MaxCharges — possible reload exploit"));

            // Respec requires Memory function unlocked
            if (section.Respec.Unlocked && !section.HasFunction(FonteFunction.Respec))
                issues.Add(W("RESPEC_STATE_UNLOCKED_NO_FUNCTION", "RespecState.Unlocked but Respec function not in UnlockedFunctions"));

            // Advanced purification requires AdvancedPurification function
            if (section.Purification.UnlockedAdvanced && !section.HasFunction(FonteFunction.AdvancedPurification))
                issues.Add(W("ADVANCED_PURIFICATION_NO_FUNCTION", "PurificationState.UnlockedAdvanced but AdvancedPurification not in UnlockedFunctions"));

            // Fonte state progression check
            if (section.HasFunction(FonteFunction.LimitedLivingWater) &&
                section.FonteState < FonteState.WaterFlowing)
                issues.Add(W("FONTE_STATE_BEHIND_FUNCTION", "LivingWater unlocked but FonteState is still Dormant/Awakened"));

            // Version guard
            if (section.Version <= 0)
                issues.Add(W("SECTION_VERSION_ZERO", "FonteAnyaSection.Version is 0"));

            return issues;
        }

        private FonteValidationIssue B(string code, string msg) =>
            new FonteValidationIssue { Code = code, Message = msg, IsBlocker = true };

        private FonteValidationIssue W(string code, string msg) =>
            new FonteValidationIssue { Code = code, Message = msg, IsBlocker = false };
    }
}
