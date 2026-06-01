#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Runner for executing a suite of architecture validators and reporting results.
    /// </summary>
    public static class ProjectValidationRunner
    {
        /// <summary>
        /// Run a list of validators and log summary.
        /// </summary>
        public static ValidationReport RunValidators(params IProjectValidator[] validators)
        {
            if (validators == null || validators.Length == 0)
            {
                Debug.Log("[ProjectValidationRunner] No validators to run. Returning PASS.");
                return new ValidationReport();
            }

            var aggregated = new ValidationReport();
            var sb = new StringBuilder();

            sb.AppendLine("\n========== ARCHITECTURE VALIDATION SUITE ==========");

            foreach (var validator in validators)
            {
                var report = validator.Run();
                aggregated.Issues.AddRange(report.Issues);

                sb.Append(report.GetSummary(validator.DisplayName));
            }

            // Overall summary
            sb.AppendLine("\n========== SUITE SUMMARY ==========");
            sb.AppendLine($"Total Issues: {aggregated.TotalCount} (Errors: {aggregated.ErrorCount}, Warnings: {aggregated.WarningCount}, Info: {aggregated.InfoCount})");

            if (aggregated.HasErrors)
            {
                sb.AppendLine("Overall Status: FAIL ✗");
                Debug.LogError(sb.ToString());
            }
            else if (aggregated.HasWarnings)
            {
                sb.AppendLine("Overall Status: WARNING");
                Debug.LogWarning(sb.ToString());
            }
            else
            {
                sb.AppendLine("Overall Status: PASS ✓");
                Debug.Log(sb.ToString());
            }

            sb.AppendLine("===================================");

            return aggregated;
        }
    }
}

#endif
