using CindarsHope.Farm;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmScaleContractTests
    {
        [Test]
        public void TileSizeConstantIsValid()
        {
            Assert.That(FarmScaleContract.TileSizePixels, Is.EqualTo(32f));
        }

        [Test]
        public void PlayerVisualDimensionsAreValid()
        {
            Assert.That(FarmScaleContract.PlayerVisualWidthPixels, Is.EqualTo(32f));
            Assert.That(FarmScaleContract.PlayerVisualHeightPixels, Is.EqualTo(48f));
        }

        [Test]
        public void NPCVisualDimensionsMatchPlayer()
        {
            Assert.That(FarmScaleContract.NPCVisualWidthPixels, Is.EqualTo(FarmScaleContract.PlayerVisualWidthPixels));
            Assert.That(FarmScaleContract.NPCVisualHeightPixels, Is.EqualTo(FarmScaleContract.PlayerVisualHeightPixels));
        }

        [Test]
        public void FootboxHeightIsLessThanVisualHeight()
        {
            Assert.That(FarmScaleContract.PlayerFootboxHeightPixels, Is.LessThan(FarmScaleContract.PlayerVisualHeightPixels));
        }

        [Test]
        public void FootboxPivotIsBottomCenter()
        {
            Assert.That(FarmScaleContract.FootboxPivotX, Is.EqualTo(0.5f), "Pivot X should be center");
            Assert.That(FarmScaleContract.FootboxPivotY, Is.EqualTo(0f), "Pivot Y should be bottom");
        }

        [Test]
        public void SortingMethodIsYFoot()
        {
            Assert.That(FarmScaleContract.SortingMethod, Is.EqualTo("Y_Foot"));
        }

        [Test]
        public void CameraReferenceDimensionsAreReasonable()
        {
            Assert.That(FarmScaleContract.CameraWidthTilesMin, Is.EqualTo(20f));
            Assert.That(FarmScaleContract.CameraWidthTilesMax, Is.EqualTo(24f));
            Assert.That(FarmScaleContract.CameraHeightTilesMin, Is.EqualTo(12f));
            Assert.That(FarmScaleContract.CameraHeightTilesMax, Is.EqualTo(14f));
        }

        [Test]
        public void FarmLevel1MinimumSizeIsLargerThanOneScreen()
        {
            float widthInTiles = FarmScaleContract.FarmLevel1MinWidthTiles;
            float heightInTiles = FarmScaleContract.FarmLevel1MinHeightTiles;

            // Level 1 must be larger than one screen (camera view)
            Assert.That(widthInTiles, Is.GreaterThan(FarmScaleContract.CameraWidthTilesMax));
            Assert.That(heightInTiles, Is.GreaterThan(FarmScaleContract.CameraHeightTilesMax));
        }

        [Test]
        public void IsTileSizeValidReturnsTrueForContractSize()
        {
            bool isValid = FarmScaleContract.IsTileSizeValid(FarmScaleContract.TileSizePixels);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IsTileSizeValidReturnsFalseForInvalidSize()
        {
            bool isValid = FarmScaleContract.IsTileSizeValid(16f); // Wrong size
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void IsCameraDimensionValidReturnsTrueForMinimum()
        {
            bool isValid = FarmScaleContract.IsCameraDimensionValid(
                FarmScaleContract.CameraWidthTilesMin,
                FarmScaleContract.CameraHeightTilesMin
            );
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IsCameraDimensionValidReturnsTrueForMaximum()
        {
            bool isValid = FarmScaleContract.IsCameraDimensionValid(
                FarmScaleContract.CameraWidthTilesMax,
                FarmScaleContract.CameraHeightTilesMax
            );
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IsCameraDimensionValidReturnsFalseForTooSmallWidth()
        {
            bool isValid = FarmScaleContract.IsCameraDimensionValid(
                FarmScaleContract.CameraWidthTilesMin - 1f,
                FarmScaleContract.CameraHeightTilesMin
            );
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void IsCameraDimensionValidReturnsFalseForTooLargeHeight()
        {
            bool isValid = FarmScaleContract.IsCameraDimensionValid(
                FarmScaleContract.CameraWidthTilesMin,
                FarmScaleContract.CameraHeightTilesMax + 1f
            );
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void IsFarmLevel1SizeValidReturnsTrueForMinimum()
        {
            bool isValid = FarmScaleContract.IsFarmLevel1SizeValid(
                FarmScaleContract.FarmLevel1MinWidthTiles,
                FarmScaleContract.FarmLevel1MinHeightTiles
            );
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IsFarmLevel1SizeValidReturnsFalseForTooSmall()
        {
            bool isValid = FarmScaleContract.IsFarmLevel1SizeValid(
                FarmScaleContract.FarmLevel1MinWidthTiles - 1f,
                FarmScaleContract.FarmLevel1MinHeightTiles
            );
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void InteractionHitboxDistanceIsPositive()
        {
            Assert.That(FarmScaleContract.InteractionHitboxDistance, Is.GreaterThan(0f));
        }
    }
}
