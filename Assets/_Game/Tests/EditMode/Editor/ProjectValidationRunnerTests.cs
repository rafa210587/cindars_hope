using CindarsHope.EditorTools.Validation;
using NUnit.Framework;

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
            ValidationReport report = ProjectValidationRunner.RunValidators();

            Assert.That(report.IsConfigured, Is.False,
                "Zero validators must be reported as NOT_CONFIGURED, never inferred as PASS from an empty Issues list.");
        }

        [Test]
        public void RunValidators_WithNullArgs_ReturnsNotConfigured()
        {
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
