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
    }
}
