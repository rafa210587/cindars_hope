using System;
using System.Collections.Generic;

namespace CindarsHope.Economy.Validation
{
    public class EconomyValidationReport
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public int RulesRun { get; set; }
        public int Passed { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Blockers { get; set; } = new List<string>();
        public List<string> AffectedItems { get; set; } = new List<string>();
        public List<string> AffectedShops { get; set; } = new List<string>();
        public List<string> AffectedRecipes { get; set; } = new List<string>();
        public List<string> AffectedLootTables { get; set; } = new List<string>();
        public List<string> ResidualRisks { get; set; } = new List<string>();

        public bool IsValid => Blockers.Count == 0 && Errors.Count == 0;
        public bool HasBlockers => Blockers.Count > 0;
    }
}
