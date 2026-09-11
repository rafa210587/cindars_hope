using CindarsHope.Farm;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmLandmarkCompositionTests
    {
        [Test]
        public void EveryLandmark_HasNamedRequiredObjectsAndInBounds()
        {
            foreach (FarmLandmarkId landmark in System.Enum.GetValues(typeof(FarmLandmarkId)))
            {
                var names = FarmLandmarkCompositionContract.RequiredSceneObjects(landmark);
                Assert.That(names.Count, Is.GreaterThan(0), landmark.ToString());
                for (var i = 0; i < names.Count; i++)
                {
                    Assert.That(names[i], Is.Not.Null.And.Not.Empty, landmark.ToString());
                }

                var bounds = FarmLandmarkCompositionContract.GetBounds(landmark);
                Assert.That(bounds.min.x, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinX), landmark + " min.x");
                Assert.That(bounds.max.x, Is.LessThanOrEqualTo(FarmLevel1LayoutContract.MaxX), landmark + " max.x");
                Assert.That(bounds.min.y, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinY), landmark + " min.y");
                Assert.That(bounds.max.y, Is.LessThanOrEqualTo(FarmLevel1LayoutContract.MaxY), landmark + " max.y");
            }
        }

        [Test]
        public void CentralField_RequiresFourApproachesWithoutPermanentFakeCrops()
        {
            var names = FarmLandmarkCompositionContract.RequiredSceneObjects(FarmLandmarkId.CentralField);
            var rowCount = 0;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].StartsWith("Visual_CropRow_", System.StringComparison.Ordinal)) rowCount++;
            }

            Assert.That(rowCount, Is.Zero);
            Assert.That(FarmLandmarkCompositionContract.RequiredApproachCount(FarmLandmarkId.CentralField), Is.EqualTo(4));
        }
    }
}
