using CindarsHope.Farm;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// Testes do FarmLevel1LayoutContract v7 (origem centrada, 64x44 tiles).
    /// </summary>
    public class FarmLevel1LayoutContractTests
    {
        [Test]
        public void Level1DimensionsAreCorrect_V7()
        {
            Assert.That(FarmLevel1LayoutContract.Level1WidthTiles, Is.EqualTo(64f));
            Assert.That(FarmLevel1LayoutContract.Level1HeightTiles, Is.EqualTo(44f));
        }

        [Test]
        public void BoundsAreOriginCentered()
        {
            Assert.That(FarmLevel1LayoutContract.MinX, Is.EqualTo(-32f));
            Assert.That(FarmLevel1LayoutContract.MaxX, Is.EqualTo(32f));
            Assert.That(FarmLevel1LayoutContract.MinY, Is.EqualTo(-22f));
            Assert.That(FarmLevel1LayoutContract.MaxY, Is.EqualTo(22f));
        }

        [Test]
        public void Level1SizeValidation_64x44_IsTrue()
        {
            Assert.That(FarmLevel1LayoutContract.IsLevel1SizeValid(64f, 44f), Is.True);
        }

        [Test]
        public void Level1SizeValidation_WrongSize_IsFalse()
        {
            Assert.That(FarmLevel1LayoutContract.IsLevel1SizeValid(48f, 34f), Is.False);
            Assert.That(FarmLevel1LayoutContract.IsLevel1SizeValid(40f, 32f), Is.False);
        }

        [Test]
        public void FarmLevel1SizeContract_MeetsMinimum()
        {
            // FarmScaleContract.IsFarmLevel1SizeValid aceita >= minimo (32x24).
            Assert.That(FarmScaleContract.IsFarmLevel1SizeValid(64f, 44f), Is.True);
        }

        [Test]
        public void FonteAnchorIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.IsFonteInBounds(), Is.True);
        }

        [Test]
        public void LakeAnchorIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.IsLakeInBounds(), Is.True);
        }

        [Test]
        public void CaveEntranceIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.IsCaveEntranceInBounds(), Is.True);
        }

        [Test]
        public void CaveEntranceIsAtNorthwest()
        {
            // Caverna no canto NO: X negativo (oeste), Y positivo alto (norte).
            Assert.That(FarmLevel1LayoutContract.CaveEntranceX, Is.LessThan(0f));
            Assert.That(FarmLevel1LayoutContract.CaveEntranceY, Is.GreaterThan(5f));
        }

        [Test]
        public void CityExitIsAccessible()
        {
            Assert.That(FarmLevel1LayoutContract.IsCityExitAccessible(), Is.True);
        }

        [Test]
        public void CityExitIsAtEast()
        {
            // Saida da cidade na extrema direita: X proximo do maximo.
            Assert.That(FarmLevel1LayoutContract.CityExitX, Is.GreaterThan(10f));
        }

        [Test]
        public void MountainBaseIsBelowNorthEdge()
        {
            Assert.That(FarmLevel1LayoutContract.IsMountainInBounds(), Is.True);
            Assert.That(FarmLevel1LayoutContract.MountainBaseY, Is.LessThan(FarmLevel1LayoutContract.MaxY));
        }

        [Test]
        public void BridgeIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.IsBridgeInBounds(), Is.True);
        }

        [Test]
        public void EvolutionBoardAnchorIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.EvolutionBoardX, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinX));
            Assert.That(FarmLevel1LayoutContract.EvolutionBoardX, Is.LessThan(FarmLevel1LayoutContract.MaxX));
            Assert.That(FarmLevel1LayoutContract.EvolutionBoardY, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinY));
            Assert.That(FarmLevel1LayoutContract.EvolutionBoardY, Is.LessThan(FarmLevel1LayoutContract.MaxY));
        }

        [Test]
        public void CityExitIsNotAtSameLocationAsHouse()
        {
            Assert.That(FarmLevel1LayoutContract.CityExitX, Is.Not.EqualTo(FarmLevel1LayoutContract.HouseStartX));
            Assert.That(FarmLevel1LayoutContract.CityExitY, Is.Not.EqualTo(FarmLevel1LayoutContract.HouseStartY));
        }

        [Test]
        public void SellPointAnchorIsInBounds()
        {
            Assert.That(FarmLevel1LayoutContract.SellPointStartX, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinX));
            Assert.That(FarmLevel1LayoutContract.SellPointStartX, Is.LessThan(FarmLevel1LayoutContract.MaxX));
            Assert.That(FarmLevel1LayoutContract.SellPointStartY, Is.GreaterThanOrEqualTo(FarmLevel1LayoutContract.MinY));
            Assert.That(FarmLevel1LayoutContract.SellPointStartY, Is.LessThan(FarmLevel1LayoutContract.MaxY));
        }

        [Test]
        public void AllSpawnAnchorsAreInBounds()
        {
            // spawn_farm_default
            Assert.That(FarmLevel1LayoutContract.DefaultSpawnX, Is.InRange(FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MaxX));
            Assert.That(FarmLevel1LayoutContract.DefaultSpawnY, Is.InRange(FarmLevel1LayoutContract.MinY, FarmLevel1LayoutContract.MaxY));
            // spawn_farm_from_town
            Assert.That(FarmLevel1LayoutContract.SpawnFromTownX, Is.InRange(FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MaxX));
            Assert.That(FarmLevel1LayoutContract.SpawnFromTownY, Is.InRange(FarmLevel1LayoutContract.MinY, FarmLevel1LayoutContract.MaxY));
            // spawn_farm_from_cave
            Assert.That(FarmLevel1LayoutContract.SpawnFromCaveX, Is.InRange(FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MaxX));
            Assert.That(FarmLevel1LayoutContract.SpawnFromCaveY, Is.InRange(FarmLevel1LayoutContract.MinY, FarmLevel1LayoutContract.MaxY));
        }
    }
}
