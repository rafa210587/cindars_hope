using CindarsHope.Foundation;
using CindarsHope.Craft;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class EfficiencyMarkSkillTests
    {
        [TestCase(.20f, 26, .4f, .4f)]
        [TestCase(.25f, 24, 0f, 2f)]
        [TestCase(.30f, 23, .6f, 3.6f)]
        public void FractionalCredit_FourBaseEightActionsPreserveExactNetBenefit(
            float reduction, int expectedCharged, float expectedCredit, float expectedNet)
        {
            var ledger = new FractionalWorkStaminaCredit();
            int charged = 0;
            for (int i = 0; i < 4; i++)
            {
                int cost = ledger.PreviewCost(8, reduction);
                charged += cost;
                ledger.CommitSpend(8, cost, reduction);
            }

            Assert.That(charged, Is.EqualTo(expectedCharged));
            Assert.That(ledger.Credit, Is.EqualTo(expectedCredit).Within(.0001f));
            float netBenefitAfterSixStaminaActivation =
                32f - (6f + charged) + ledger.Credit;
            Assert.That(netBenefitAfterSixStaminaActivation,
                Is.EqualTo(expectedNet).Within(.0001f));
        }

        [Test]
        public void Runtime_UsesInclusiveRadiusAndReplacementPreservesEarnedCredit()
        {
            var owner = new GameObject("Efficiency mark test owner");
            try
            {
                var runtime = owner.AddComponent<EfficiencyMarkRuntimeCoordinator>();
                Assert.That(runtime.Activate("station-a", runtime, Vector2.zero, 2.5f, 12f, .20f), Is.True);

                int edgeCost = runtime.PreviewCost(
                    WorkStaminaChannel.Agricultural, 8, 2.5f, 0f);
                Assert.That(edgeCost, Is.EqualTo(7));
                runtime.CommitSpend(WorkStaminaChannel.Agricultural, 8, edgeCost, 2.5f, 0f);
                Assert.That(runtime.FractionalCredit, Is.EqualTo(.6f).Within(.0001f));
                Assert.That(runtime.PreviewCost(
                    WorkStaminaChannel.Agricultural, 8, 2.51f, 0f), Is.EqualTo(8));

                Assert.That(runtime.Activate("station-b", runtime, Vector2.right * 10f,
                    2.5f, 20f, .30f), Is.True);
                Assert.That(runtime.StationInstanceId, Is.EqualTo("station-b"));
                Assert.That(runtime.FractionalCredit, Is.EqualTo(.6f).Within(.0001f));
                Assert.That(runtime.PreviewCost(
                    WorkStaminaChannel.Crafting, 8, 100f, 100f, "station-b"), Is.EqualTo(5));
                Assert.That(runtime.PreviewCost(
                    WorkStaminaChannel.Agricultural, 8, 0f, 0f), Is.EqualTo(8));
            }
            finally
            {
                Object.DestroyImmediate(owner);
                WorkStaminaCostModifierProvider.Source = null;
            }
        }

        [Test]
        public void Runtime_DisablesMarkWhenMaterializedStationDisappears()
        {
            var owner = new GameObject("Efficiency runtime owner");
            var stationObject = new GameObject("Materialized station");
            try
            {
                var runtime = owner.AddComponent<EfficiencyMarkRuntimeCoordinator>();
                var station = stationObject.AddComponent<CraftingPoint>();
                Assert.That(runtime.Activate("station-live", station, Vector2.zero,
                    2.5f, 12f, .2f), Is.True);
                Assert.That(runtime.IsActive, Is.True);

                stationObject.SetActive(false);

                Assert.That(runtime.IsActive, Is.False);
                Assert.That(runtime.PreviewCost(
                    WorkStaminaChannel.Agricultural, 8, 0f, 0f), Is.EqualTo(8));
            }
            finally
            {
                Object.DestroyImmediate(stationObject);
                Object.DestroyImmediate(owner);
                WorkStaminaCostModifierProvider.Source = null;
            }
        }
    }
}
