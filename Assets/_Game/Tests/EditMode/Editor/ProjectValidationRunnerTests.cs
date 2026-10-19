using System;
using System.Text.RegularExpressions;
using CindarsHope.EditorTools.Validation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.EditMode.Editor
{
    public class ProjectValidationRunnerTests
    {
        private class FakePassingValidator : IProjectValidator
        {
            public string ValidatorId => "fake_passing_validator";
            public string DisplayName => "Fake Passing Validator";

            public ValidationReport Run()
            {
                return new ValidationReport();
            }
        }

        [Test]
        public void RunValidators_WithNoArgs_ReturnsNotConfigured()
        {
            LogAssert.Expect(LogType.Error,
                "[ProjectValidationRunner] NOT_CONFIGURED: no validators were registered to run. This is not a PASS — zero architecture rules were checked.");
            ValidationReport report = ProjectValidationRunner.RunValidators();

            Assert.That(report.IsConfigured, Is.False,
                "Zero validators must be reported as NOT_CONFIGURED, never inferred as PASS from an empty Issues list.");
        }

        [Test]
        public void RunValidators_WithNullArgs_ReturnsNotConfigured()
        {
            LogAssert.Expect(LogType.Error,
                "[ProjectValidationRunner] NOT_CONFIGURED: no validators were registered to run. This is not a PASS — zero architecture rules were checked.");
            ValidationReport report = ProjectValidationRunner.RunValidators(null);

            Assert.That(report.IsConfigured, Is.False);
        }

        [Test]
        public void RunValidators_WithAtLeastOneValidator_ReturnsConfigured()
        {
            ValidationReport report = ProjectValidationRunner.RunValidators(new FakePassingValidator());

            Assert.That(report.IsConfigured, Is.True);
            Assert.That(report.HasErrors, Is.False);
        }

        [Test]
        public void LoggedValidation_ErrorWithoutException_FailsAndStopsListeningAfterReturn()
        {
            LogAssert.Expect(LogType.Error, "broken registry fixture");
            var report = ProjectValidationRunner.RunLoggedValidation("registry", () =>
                Debug.LogError("broken registry fixture"));

            Assert.That(report.Passed, Is.False);
            Assert.That(report.ErrorCount, Is.EqualTo(1));

            LogAssert.Expect(LogType.Error, "later unrelated fixture log");
            Debug.LogError("later unrelated fixture log");
            Assert.That(report.ErrorCount, Is.EqualTo(1), "The completed report must not capture subsequent logs.");
        }

        [Test]
        public void LoggedValidation_ExceptionBecomesFailure_AndNextValidationRunsIndependently()
        {
            var failed = ProjectValidationRunner.RunLoggedValidation("throwing", () =>
                throw new InvalidOperationException("fixture exception"));
            var next = ProjectValidationRunner.RunLoggedValidation("next", () => { });

            Assert.That(failed.Passed, Is.False);
            Assert.That(failed.Issues[0].Code, Is.EqualTo("VALIDATOR_EXCEPTION"));
            Assert.That(next.Passed, Is.True);
        }

        [Test]
        public void LoggedValidation_WarningRemainsVisibleWithoutFailing()
        {
            LogAssert.Expect(LogType.Warning, "optional art fixture");
            var report = ProjectValidationRunner.RunLoggedValidation("art", () =>
                Debug.LogWarning("optional art fixture"));

            Assert.That(report.Passed, Is.True);
            Assert.That(report.WarningCount, Is.EqualTo(1));
        }

        [Test]
        public void MissingValidation_IsNotPassed_EvenWithNoIssues()
        {
            var report = ProjectValidationRunner.RunLoggedValidation("missing", null);
            Assert.That(report.Passed, Is.False);
            StringAssert.Contains("Status: NOT_CONFIGURED", report.GetSummary("missing"));
            StringAssert.DoesNotContain("Status: PASS", report.GetSummary("missing"));
            report.AddIssue("fixture", "MISSING_SCENE", ValidationSeverity.Error, "missing-fixture.unity");
            StringAssert.Contains("missing-fixture.unity", report.GetSummary("missing"));
        }

        [Test]
        public void ValidatorFailure_DoesNotSkipLaterValidatorsOrReportPass()
        {
            bool nextRan = false;
            LogAssert.Expect(LogType.Error, new Regex("(?s)VALIDATOR_EXCEPTION: InvalidOperationException: fixture.*Overall Status: FAIL"));
            var result = ProjectValidationRunner.RunValidators(
                new DelegateValidator(() => throw new InvalidOperationException("fixture")),
                new DelegateValidator(() => { nextRan = true; return new ValidationReport(); }));

            Assert.That(nextRan, Is.True);
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void UnconfiguredChild_PreventsAggregatePass()
        {
            LogAssert.Expect(LogType.Error, new Regex("Overall Status: FAIL"));
            var result = ProjectValidationRunner.RunValidators(
                new DelegateValidator(() => new ValidationReport { IsConfigured = false }));

            Assert.That(result.Passed, Is.False);
            Assert.That(result.IsConfigured, Is.False);
        }

        private sealed class DelegateValidator : IProjectValidator
        {
            private readonly Func<ValidationReport> _run;
            public string ValidatorId => "delegate_fixture";
            public string DisplayName => "Delegate fixture";
            public DelegateValidator(Func<ValidationReport> run) => _run = run;
            public ValidationReport Run() => _run();
        }
    }
}
