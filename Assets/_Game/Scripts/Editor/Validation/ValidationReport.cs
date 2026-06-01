#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Container for validation results with counters and summary generation.
    /// </summary>
    public class ValidationReport
    {
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();

        public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error);
        public int WarningCount => Issues.Count(i => i.Severity == ValidationSeverity.Warning);
        public int InfoCount => Issues.Count(i => i.Severity == ValidationSeverity.Info);
        public int TotalCount => Issues.Count;

        public bool HasErrors => ErrorCount > 0;
        public bool HasWarnings => WarningCount > 0;

        public void AddIssue(ValidationIssue issue)
        {
            Issues.Add(issue);
        }

        public void AddIssue(string area, string code, ValidationSeverity severity, string message,
            string assetPath = "", string objectName = "", string suggestedFix = "")
        {
            Issues.Add(new ValidationIssue(area, code, severity, message, assetPath, objectName, suggestedFix));
        }

        public string GetSummary(string validatorName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n========== {validatorName} ==========");
            sb.AppendLine($"Total Issues: {TotalCount} (Errors: {ErrorCount}, Warnings: {WarningCount}, Info: {InfoCount})");

            if (Issues.Count == 0)
            {
                sb.AppendLine("Status: PASS ✓");
            }
            else
            {
                foreach (var issue in Issues)
                {
                    sb.AppendLine(issue.ToString());
                }

                if (HasErrors)
                {
                    sb.AppendLine($"Status: FAIL ({ErrorCount} error(s))");
                }
                else
                {
                    sb.AppendLine("Status: WARNING");
                }
            }

            sb.AppendLine("==========================================");
            return sb.ToString();
        }
    }
}

#endif
