#if UNITY_EDITOR

using System;
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
        /// Adapts synchronous legacy validators that report through the Unity console.
        /// The listener belongs only to this call; later logs cannot change its result.
        /// </summary>
        public static ValidationReport RunLoggedValidation(string name, Action validation)
        {
            var report = new ValidationReport { IsConfigured = validation != null };
            if (validation == null)
            {
                return report;
            }

            void CaptureLog(string message, string stackTrace, LogType type)
            {
                if (type == LogType.Log)
                {
                    return;
                }

                var severity = type == LogType.Warning ? ValidationSeverity.Warning : ValidationSeverity.Error;
                report.AddIssue(name, type.ToString(), severity, message);
            }

            Application.logMessageReceived += CaptureLog;
            try
            {
                validation();
            }
            catch (Exception exception)
            {
                report.AddIssue(name, "VALIDATOR_EXCEPTION", ValidationSeverity.Error,
                    $"{exception.GetType().Name}: {exception.Message}");
            }
            finally
            {
                Application.logMessageReceived -= CaptureLog;
            }

            return report;
        }

        /// <summary>
        /// Run a list of validators and log summary.
        /// </summary>
        public static ValidationReport RunValidators(params IProjectValidator[] validators)
        {
            if (validators == null || validators.Length == 0)
            {
                Debug.LogError("[ProjectValidationRunner] NOT_CONFIGURED: no validators were registered to run. This is not a PASS — zero architecture rules were checked.");
                return new ValidationReport { IsConfigured = false };
            }

            var aggregated = new ValidationReport();
            var sb = new StringBuilder();

            sb.AppendLine("\n========== ARCHITECTURE VALIDATION SUITE ==========");

            foreach (var validator in validators)
            {
                if (validator == null)
                {
                    aggregated.IsConfigured = false;
                    aggregated.AddIssue("Runner", "MISSING_VALIDATOR", ValidationSeverity.Error,
                        "A registered validator is null.");
                    continue;
                }

                try
                {
                    var report = validator.Run();
                    if (report == null || !report.IsConfigured)
                    {
                        aggregated.IsConfigured = false;
                        aggregated.AddIssue(validator.DisplayName, "NOT_CONFIGURED", ValidationSeverity.Error,
                            "The validator did not return a configured result.");
                    }

                    if (report != null)
                    {
                        aggregated.Issues.AddRange(report.Issues);
                    }
                }
                catch (Exception exception)
                {
                    aggregated.AddIssue(validator.DisplayName, "VALIDATOR_EXCEPTION", ValidationSeverity.Error,
                        $"{exception.GetType().Name}: {exception.Message}");
                }
            }

            // Overall summary
            sb.Append(aggregated.GetSummary("SUITE SUMMARY"));

            if (!aggregated.Passed)
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
