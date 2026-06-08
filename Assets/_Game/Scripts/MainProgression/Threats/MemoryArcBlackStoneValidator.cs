using System.Collections.Generic;

namespace CindarsHope.MainProgression.Threats
{
    public class ThreatValidationIssue
    {
        public string Code { get; }
        public string Message { get; }
        public bool IsError { get; }
        public ThreatValidationIssue(string code, string message, bool isError = false)
        {
            Code = code; Message = message; IsError = isError;
        }
    }

    public static class MemoryArcBlackStoneValidator
    {
        // Returns validation issues — empty list means clean
        public static List<ThreatValidationIssue> Validate(
            MemoryArcState memoryArc,
            BlackStoneState blackStone,
            VaelrionArcState vaelrion,
            SethraCultState sethra,
            MainAct currentAct)
        {
            var issues = new List<ThreatValidationIssue>();

            // Anti-spoiler: Memory Arc term not before Act2
            if (memoryArc >= MemoryArcState.TermDiscovered && currentAct < MainAct.Act2_CindarAndMemoryArc)
                issues.Add(E("MEMORY_ARC_TERM_SPOILER_BEFORE_ACT2", "MemoryArc term discovered before Act 2"));

            // Anti-spoiler: Sethra not revealed before Act3
            if (sethra >= SethraCultState.SethraRevealed && currentAct < MainAct.Act3_CultBlackStoneAndLife)
                issues.Add(E("SETHRA_REVEAL_BEFORE_ACT3", "Sethra revealed before Act 3"));

            // Anti-spoiler: Vaelrion antagonist not before Act3
            if (vaelrion >= VaelrionArcState.BoundaryCrossed && currentAct < MainAct.Act3_CultBlackStoneAndLife)
                issues.Add(E("VAELRION_ANTAGONIST_BEFORE_ACT3", "Vaelrion boundary crossed before Act 3"));

            // BlackStone soul drain not before Act3
            if (blackStone >= BlackStoneState.DrainsSoul && currentAct < MainAct.Act3_CultBlackStoneAndLife)
                issues.Add(E("BLACKSTONE_SOUL_DRAIN_BEFORE_ACT3", "BlackStone soul drain state before Act 3"));

            // Stabilized BlackStone not before Act3 (deep/endgame material)
            if (blackStone >= BlackStoneState.StabilizedFragmentKnown && currentAct < MainAct.Act3_CultBlackStoneAndLife)
                issues.Add(W("BLACKSTONE_STABILIZED_BEFORE_ACT3", "Stabilized BlackStone known before Act 3"));

            // Final convergence not before Act4
            if (memoryArc >= MemoryArcState.FinalActivated && currentAct < MainAct.Act4_Level100101AndHope)
                issues.Add(E("MEMORY_ARC_FINAL_BEFORE_ACT4", "Memory Arc final activation before Act 4"));

            if (vaelrion >= VaelrionArcState.FinalConvergence && currentAct < MainAct.Act4_Level100101AndHope)
                issues.Add(E("VAELRION_CONVERGENCE_BEFORE_ACT4", "Vaelrion final convergence before Act 4"));

            return issues;
        }

        // Validate a BlackStone exposure record — commodity/economy guard
        public static List<ThreatValidationIssue> ValidateExposureRecord(BlackStoneExposureRecord record)
        {
            var issues = new List<ThreatValidationIssue>();
            if (record == null) { issues.Add(E("EXPOSURE_NULL", "Exposure record is null")); return issues; }
            if (string.IsNullOrEmpty(record.ExposureId))
                issues.Add(E("EXPOSURE_NO_ID", "Exposure record has no ID"));
            // Economy guardrail: cultist/unstable BlackStone must never be commodity
            if (record.IsCommodity && record.SourceType == BlackStoneSourceType.CultRitual)
                issues.Add(E("BLACKSTONE_CULTIST_COMMODITY", "Cultist BlackStone must not be commodity"));
            if (record.IsCommodity && record.ThreatLevel >= CorruptionThreatLevel.MemoryDrain)
                issues.Add(E("BLACKSTONE_DANGEROUS_COMMODITY", "Dangerous BlackStone must not be commodity"));
            // Corrupted LivingWater records must never be healing items
            if (record.AffectsLivingWater && !record.CanBePurified && !record.RequiresLifeFragment)
                issues.Add(W("CORRUPTED_WATER_NO_PURIF_GATE", "Corrupted LivingWater exposure should require purification gate"));
            return issues;
        }

        // Validate purification compatibility table
        public static List<ThreatValidationIssue> ValidatePurificationTable(List<PurificationCompatibility> table)
        {
            var issues = new List<ThreatValidationIssue>();
            if (table == null || table.Count == 0) { issues.Add(W("PURIF_TABLE_EMPTY", "Purification table empty")); return issues; }
            foreach (var entry in table)
            {
                // Soul drain cannot be cured by common item
                if (entry.ThreatLevel == CorruptionThreatLevel.SoulDrain && entry.CanBeFullyCured)
                    issues.Add(E("SOUL_DRAIN_CURED_BY_COMMON", "Soul drain cannot be fully cured by common item"));
                // Final convergence requires final route, not ordinary purification
                if (entry.ThreatLevel == CorruptionThreatLevel.FinalConvergence && entry.AllowsAdvancedPurification)
                    issues.Add(E("FINAL_CONVERGENCE_PURIFIED", "Final convergence cannot be cleared by purification"));
            }
            return issues;
        }

        private static ThreatValidationIssue E(string code, string msg) => new ThreatValidationIssue(code, msg, isError: true);
        private static ThreatValidationIssue W(string code, string msg) => new ThreatValidationIssue(code, msg, isError: false);
    }
}
