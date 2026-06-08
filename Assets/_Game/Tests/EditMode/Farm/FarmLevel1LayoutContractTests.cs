using CindarsHope.Farm;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmLevel1LayoutContractTests
    {
        [Test]
        public void Level1DimensionsAreCorrect()
        {
            Assert.That(FarmLevel1LayoutContract.Level1WidthTiles, Is.EqualTo(40f));
            Assert.That(FarmLevel1LayoutContract.Level1HeightTiles, Is.EqualTo(32f));
        }

        [Test]
        public void InitialFieldDimensionsAreReasonable()
        {
            Assert.That(FarmLevel1LayoutContract.InitialFieldWidthTiles, Is.LessThan(FarmLevel1LayoutContract.Level1WidthTiles));
            Assert.That(FarmLevel1LayoutContract.InitialFieldHeightTiles, Is.LessThan(FarmLevel1LayoutContract.Level1HeightTiles));
        }

        [Test]
        public void InitialFieldIsInBounds()
        {
            bool inBounds = FarmLevel1LayoutContract.IsInitialFieldInBounds();
            Assert.That(inBounds, Is.True);
        }

        [Test]
        public void FonteAnchorIsInBounds()
        {
            bool inBounds = FarmLevel1LayoutContract.IsFonteInBounds();
            Assert.That(inBounds, Is.True);
        }

        [Test]
        public void FonteAnchorIsNotAtEdges()
        {
            Assert.That(FarmLevel1LayoutContract.FonteAnchorX, Is.GreaterThan(0));
            Assert.That(FarmLevel1LayoutContract.FonteAnchorX, Is.LessThan(FarmLevel1LayoutContract.Level1WidthTiles));
            Assert.That(FarmLevel1LayoutContract.FonteAnchorY, Is.GreaterThan(0));
            Assert.That(FarmLevel1LayoutContract.FonteAnchorY, Is.LessThan(FarmLevel1LayoutContract.Level1HeightTiles));
        }

        [Test]
        public void LakeAnchorIsInBounds()
        {
            bool inBounds = FarmLevel1LayoutContract.IsLakeInBounds();
            Assert.That(inBounds, Is.True);
        }

        [Test]
        public void LakeDimensionsAreReasonable()
        {
            Assert.That(FarmLevel1LayoutContract.LakeWidthTiles, Is.GreaterThan(0));
            Assert.That(FarmLevel1LayoutContract.LakeHeightTiles, Is.GreaterThan(0));
            Assert.That(FarmLevel1LayoutContract.LakeWidthTiles, Is.LessThan(FarmLevel1LayoutContract.Level1WidthTiles));
            Assert.That(FarmLevel1LayoutContract.LakeHeightTiles, Is.LessThan(FarmLevel1LayoutContract.Level1HeightTiles));
        }

        [Test]
        public void CaveEntranceIsInBounds()
        {
            bool inBounds = FarmLevel1LayoutContract.IsCaveEntranceInBounds();
            Assert.That(inBounds, Is.True);
        }

        [Test]
        public void CityExitIsAccessible()
        {
            bool accessible = FarmLevel1LayoutContract.IsCityExitAccessible();
            Assert.That(accessible, Is.True);
        }

        [Test]
        public void CityExitIsNotAtSameLocationAsHouse()
        {
            Assert.That(FarmLevel1LayoutContract.CityExitX, Is.Not.EqualTo(FarmLevel1LayoutContract.HouseStartX));
            Assert.That(FarmLevel1LayoutContract.CityExitY, Is.Not.EqualTo(FarmLevel1LayoutContract.HouseStartY));
        }

        [Test]
        public void HouseStartLocationIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.HouseStartX, Is.GreaterThanOrEqualTo(0));
            Assert.That(FarmLevel1LayoutContract.HouseStartX, Is.LessThan(FarmLevel1LayoutContract.Level1WidthTiles));
            Assert.That(FarmLevel1LayoutContract.HouseStartY, Is.GreaterThanOrEqualTo(0));
            Assert.That(FarmLevel1LayoutContract.HouseStartY, Is.LessThan(FarmLevel1LayoutContract.Level1HeightTiles));
        }

        [Test]
        public void SellPointStartLocationIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.SellPointStartX, Is.GreaterThanOrEqualTo(0));
            Assert.That(FarmLevel1LayoutContract.SellPointStartX, Is.LessThan(FarmLevel1LayoutContract.Level1WidthTiles));
            Assert.That(FarmLevel1LayoutContract.SellPointStartY, Is.GreaterThanOrEqualTo(0));
            Assert.That(FarmLevel1LayoutContract.SellPointStartY, Is.LessThan(FarmLevel1LayoutContract.Level1HeightTiles));
        }

        [Test]
        public void AllFixedAnchorsAreValidated()
        {
            Assert.That(FarmLevel1LayoutContract.IsLevel1SizeValid(40f, 32f), Is.True);
            Assert.That(FarmLevel1LayoutContract.IsLevel1SizeValid(39f, 32f), Is.False);
        }
    }
}
