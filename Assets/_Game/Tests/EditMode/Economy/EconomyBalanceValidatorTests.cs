using NUnit.Framework;
using CindarsHope.Economy;
using CindarsHope.Economy.Pricing;
using CindarsHope.Economy.Validation;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class EconomyBalanceValidatorTests
    {
        private EconomyBalanceConfigSO _config;
        private EconomyPricingService _pricing;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EconomyBalanceConfigSO>();
            _config.MaxSafeGoldPerHour = 800f;
            _pricing = new EconomyPricingService();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void ValidateGoldHour_Warns_WhenExceedsConfigMax()
        {
            var validator = new EconomyBalanceValidator(_pricing, _config);
            var report    = new EconomyValidationReport();
            validator.ValidateGoldHour(
                new GoldHourBudget { Source = "cave_deep", EstimatedGoldPerHour = 1000f }, report);

            Assert.AreEqual(1, report.Warnings.Count);
            StringAssert.Contains("GOLD_HOUR_BUDGET", report.Warnings[0]);
        }

        [Test]
        public void ValidateGoldHour_NoWarn_WhenWithinConfigMax()
        {
            var validator = new EconomyBalanceValidator(_pricing, _config);
            var report    = new EconomyValidationReport();
            validator.ValidateGoldHour(
                new GoldHourBudget { Source = "farm_basic", EstimatedGoldPerHour = 100f }, report);

            Assert.AreEqual(0, report.Warnings.Count);
        }

        [Test]
        public void ValidateGoldHour_NoConfig_UsesBudgetMaxSafe()
        {
            var validator = new EconomyBalanceValidator(_pricing);
            var report    = new EconomyValidationReport();
            var budget    = new GoldHourBudget { Source = "farm", EstimatedGoldPerHour = 900f, MaxSafeGoldPerHour = 800f };
            validator.ValidateGoldHour(budget, report);

            Assert.AreEqual(1, report.Warnings.Count);
        }

        [Test]
        public void GoldHourBudget_DefaultMaxIs800()
        {
            var budget = new GoldHourBudget { EstimatedGoldPerHour = 0f };
            Assert.AreEqual(800f, budget.MaxSafeGoldPerHour);
        }
    }
}
