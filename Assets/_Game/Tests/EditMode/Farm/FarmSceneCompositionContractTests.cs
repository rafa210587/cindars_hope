using CindarsHope.Farm;
using CindarsHope.World.Scale;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmSceneCompositionContractTests
    {
        [Test]
        public void AllCompositionGroups_HaveNonEmptyBounds()
        {
            foreach (FarmCompositionGroup group in System.Enum.GetValues(typeof(FarmCompositionGroup)))
            {
                var bounds = FarmSceneCompositionContract.GetTargetBounds(group);
                Assert.That(bounds.size.x, Is.GreaterThan(0f), group.ToString());
                Assert.That(bounds.size.y, Is.GreaterThan(0f), group.ToString());
            }
        }

        [Test]
        public void ApprovedAnchorPositions_AreInsideTheirGroups()
        {
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.NorthCliff).Contains(
                new Vector3(0f, FarmLevel1LayoutContract.MountainBaseY)), Is.True);
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.WestForest).Contains(new Vector3(-27f, 8.5f)), Is.True);
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.CentralAgriculture).Contains(Vector3.zero), Is.True);
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.NorthEastHomestead).Contains(new Vector3(16f, 9f)), Is.True);
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.SouthAnimalRow).Contains(
                new Vector3(FarmLevel1LayoutContract.CoopX, FarmLevel1LayoutContract.CoopY)), Is.True);
            Assert.That(FarmSceneCompositionContract.GetTargetBounds(FarmCompositionGroup.SouthEastLake).Contains(new Vector3(18f, -13f)), Is.True);
        }

        [Test]
        public void ScaleRanges_AcceptApprovedValuesAndRejectBridgeOutlier()
        {
            Assert.That(FarmSceneCompositionContract.IsScaleWithinApprovedRange(FarmSceneCompositionContract.BridgeScaleKey, FarmSceneCompositionContract.BridgeVisualLocalScale), Is.True);
            Assert.That(FarmSceneCompositionContract.IsScaleWithinApprovedRange("TreeNode_00", new Vector3(1.5f, 1.5f, 1f)), Is.True);
            Assert.That(FarmSceneCompositionContract.IsScaleWithinApprovedRange(FarmSceneCompositionContract.BridgeScaleKey, Vector3.one), Is.False);
        }

        [Test]
        public void BridgeVisualScale_IsReducedWithoutDefiningPhysicalFootprint()
        {
            Assert.That(FarmSceneCompositionContract.BridgeVisualLocalScale.x, Is.EqualTo(1.8f));
            Assert.That(FarmSceneCompositionContract.BridgeVisualLocalScale.y, Is.EqualTo(1.4f));
            Assert.That(FarmSceneCompositionContract.BridgeVisualLocalScale.x / FarmSceneCompositionContract.BridgeVisualLocalScale.y,
                Is.EqualTo(1.8f / 1.4f).Within(0.01f));
        }

        [TestCase(FarmSceneCompositionContract.CoopScaleKey, FarmSceneCompositionContract.CoopVisualTargetHeight)]
        [TestCase(FarmSceneCompositionContract.BarnScaleKey, FarmSceneCompositionContract.BarnVisualTargetHeight)]
        [TestCase(FarmSceneCompositionContract.SouthProcessingScaleKey, FarmSceneCompositionContract.SouthProcessingVisualTargetHeight)]
        [TestCase(FarmSceneCompositionContract.GreenhouseScaleKey, FarmSceneCompositionContract.GreenhouseVisualTargetHeight)]
        [TestCase(FarmSceneCompositionContract.HomesteadRoofScaleKey, FarmSceneCompositionContract.HomesteadRoofTargetWidth)]
        public void BuildingVisualExtent_AcceptsAuthoredTargetAndRejectsMiniature(string key, float target)
        {
            Assert.That(FarmSceneCompositionContract.IsBuildingVisualExtentApproved(key, target), Is.True);
            Assert.That(FarmSceneCompositionContract.IsBuildingVisualExtentApproved(key, target * 0.7f), Is.False);
        }

        [Test]
        public void BuildingToPlayerRatio_RejectsSouthRowMiniature()
        {
            Assert.That(FarmSceneCompositionContract.IsBuildingToPlayerRatioApproved(
                FarmSceneCompositionContract.CoopScaleKey, FarmSceneCompositionContract.CoopVisualTargetHeight, 1.1875f), Is.True);
            Assert.That(FarmSceneCompositionContract.IsBuildingToPlayerRatioApproved(
                FarmSceneCompositionContract.CoopScaleKey, 3.5f, 1.1875f), Is.False);
        }

        [Test]
        public void SouthBuildingCluster_IsCompactAndRejectsFormerSpread()
        {
            Assert.That(FarmSceneCompositionContract.IsSouthBuildingClusterCompact(
                FarmLevel1LayoutContract.CoopX,
                FarmLevel1LayoutContract.BarnX,
                FarmLevel1LayoutContract.ProcessingAX,
                FarmLevel1LayoutContract.ProcessingBX), Is.True);

            Assert.That(FarmSceneCompositionContract.IsSouthBuildingClusterCompact(
                -13.1f, -7f, -0.875f, 5.69f), Is.False);
            Assert.That(FarmSceneCompositionContract.SouthFrontPathWidth, Is.LessThanOrEqualTo(1.35f));
        }
    }
}
