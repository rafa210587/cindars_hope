using NUnit.Framework;
using CindarsHope.Farm;
using UnityEngine;

namespace CindarsHope.Farm.Tests
{
    /// <summary>
    /// Testes determinísticos do FarmTileGrid:
    /// — IsTillable com e sem zonas nao-araveis
    /// — Mapeamento world <-> tile
    /// — Zonas nao-araveis e estufa aravel
    /// — Registro e remocao de tiles arados
    /// </summary>
    [TestFixture]
    public class FarmTileGridTests
    {
        private FarmNonArableZones _zones;
        private FarmTileGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _zones = new FarmNonArableZones();
            // tileSizeUnits = 1: 1 tile == 1 unidade do Unity
            _grid = new FarmTileGrid(_zones, tileSizeUnits: 1f);
            // Bounds: origem (0,0), 32x24 tiles
            _grid.SetBounds(originTileX: 0, originTileY: 0, widthTiles: 32, heightTiles: 24,
                worldOriginX: 0f, worldOriginY: 0f);
        }

        // ── IsTillable: tile livre dentro dos bounds ──────────────────────────────────────────────

        [Test]
        public void IsTillable_FreeTileInsideBounds_ReturnsTrue()
        {
            Assert.IsTrue(_grid.IsTillable(5, 5));
        }

        [Test]
        public void IsTillable_TileOutsideBounds_ReturnsFalse()
        {
            Assert.IsFalse(_grid.IsTillable(100, 100));
        }

        [Test]
        public void IsTillable_NegativeTile_ReturnsFalse()
        {
            Assert.IsFalse(_grid.IsTillable(-1, 0));
        }

        // ── Zonas nao-araveis ─────────────────────────────────────────────────────────────────────

        [Test]
        public void IsTillable_BlockedTile_ReturnsFalse()
        {
            _zones.RegisterBlockedTile(3, 3);
            Assert.IsFalse(_grid.IsTillable(3, 3));
        }

        [Test]
        public void IsTillable_BlockedRect_BlocksAllTilesInRect()
        {
            _zones.RegisterBlockedRect(4, 4, 3, 2); // 3x2 a partir de (4,4)
            for (var y = 4; y < 6; y++)
            {
                for (var x = 4; x < 7; x++)
                {
                    Assert.IsFalse(_grid.IsTillable(x, y), $"Tile ({x},{y}) deve ser nao-aravel");
                }
            }
            // Tile adjacente nao bloqueado
            Assert.IsTrue(_grid.IsTillable(7, 4));
        }

        [Test]
        public void IsTillable_UnregisteredTile_RemainsTillable()
        {
            _zones.RegisterBlockedTile(3, 3);
            // Tile vizinho nao bloqueado
            Assert.IsTrue(_grid.IsTillable(4, 3));
        }

        // ── Estufa e aravel ──────────────────────────────────────────────────────────────────────

        [Test]
        public void IsTillable_GreenhouseTile_ReturnsTrue_EvenIfBlocked()
        {
            _zones.RegisterBlockedTile(10, 10);
            _zones.RegisterGreenhouseTile(10, 10);
            // Estufa tem prioridade sobre blocked
            Assert.IsTrue(_grid.IsTillable(10, 10));
        }

        [Test]
        public void IsTillable_GreenhouseRect_AllTilesAreTillable()
        {
            _zones.RegisterBlockedRect(6, 6, 4, 4);
            _zones.RegisterGreenhouseRect(6, 6, 4, 4);
            for (var y = 6; y < 10; y++)
            {
                for (var x = 6; x < 10; x++)
                {
                    Assert.IsTrue(_grid.IsTillable(x, y), $"Estufa ({x},{y}) deve ser aravel");
                }
            }
        }

        [Test]
        public void IsGreenhouseTile_ReturnsCorrectly()
        {
            _zones.RegisterGreenhouseTile(5, 5);
            Assert.IsTrue(_zones.IsGreenhouseTile(5, 5));
            Assert.IsFalse(_zones.IsGreenhouseTile(6, 5));
        }

        // ── Mapeamento world <-> tile ─────────────────────────────────────────────────────────────

        [Test]
        public void WorldToTile_OriginPosition_ReturnsTile0_0()
        {
            var tile = _grid.WorldToTile(0f, 0f);
            Assert.AreEqual(new Vector2Int(0, 0), tile);
        }

        [Test]
        public void WorldToTile_PositionInSecondTile_ReturnsTile1_0()
        {
            var tile = _grid.WorldToTile(1.5f, 0f);
            Assert.AreEqual(new Vector2Int(1, 0), tile);
        }

        [Test]
        public void WorldToTile_PositionAtTileBoundary_RoundsDown()
        {
            // Posicao exatamente no inicio do tile 2 => tile 2
            var tile = _grid.WorldToTile(2f, 0f);
            Assert.AreEqual(new Vector2Int(2, 0), tile);
        }

        [Test]
        public void TileToWorldCenter_Tile0_0_ReturnsCenterOf0_0()
        {
            var center = _grid.TileToWorldCenter(0, 0);
            // Centro do tile (0,0) com tileSizeUnits=1 => (0.5, 0.5)
            Assert.AreEqual(new Vector2(0.5f, 0.5f), center);
        }

        [Test]
        public void TileToWorldCenter_Tile2_3_IsCorrect()
        {
            var center = _grid.TileToWorldCenter(2, 3);
            Assert.AreEqual(new Vector2(2.5f, 3.5f), center);
        }

        [Test]
        public void WorldToTile_ThenTileToWorldCenter_RoundTripIsConsistent()
        {
            var worldX = 7.3f;
            var worldY = 5.8f;
            var tile = _grid.WorldToTile(worldX, worldY);
            var center = _grid.TileToWorldCenter(tile.x, tile.y);
            // O centro deve estar no mesmo tile
            var tileBack = _grid.WorldToTile(center.x, center.y);
            Assert.AreEqual(tile, tileBack);
        }

        // ── Registro de tiles arados ──────────────────────────────────────────────────────────────

        [Test]
        public void TryRegisterTilledTile_FreeTile_ReturnsTrue_AndCreatesLogic()
        {
            var result = _grid.TryRegisterTilledTile(1, 1, out var logic);
            Assert.IsTrue(result);
            Assert.IsNotNull(logic);
        }

        [Test]
        public void TryRegisterTilledTile_AlreadyTilled_ReturnsFalse()
        {
            _grid.TryRegisterTilledTile(2, 2, out _);
            var result = _grid.TryRegisterTilledTile(2, 2, out _);
            Assert.IsFalse(result);
        }

        [Test]
        public void TryRegisterTilledTile_BlockedTile_ReturnsFalse()
        {
            _zones.RegisterBlockedTile(3, 3);
            var result = _grid.TryRegisterTilledTile(3, 3, out var logic);
            Assert.IsFalse(result);
            Assert.IsNull(logic);
        }

        [Test]
        public void IsTilled_BeforeAndAfterRegister_ChangesCorrectly()
        {
            Assert.IsFalse(_grid.IsTilled(4, 4));
            _grid.TryRegisterTilledTile(4, 4, out _);
            Assert.IsTrue(_grid.IsTilled(4, 4));
        }

        [Test]
        public void RemoveTilledTile_ExistingTile_ReturnsTrueAndRemoves()
        {
            _grid.TryRegisterTilledTile(5, 5, out _);
            var removed = _grid.RemoveTilledTile(5, 5);
            Assert.IsTrue(removed);
            Assert.IsFalse(_grid.IsTilled(5, 5));
        }

        [Test]
        public void RemoveTilledTile_NonExistingTile_ReturnsFalse()
        {
            var removed = _grid.RemoveTilledTile(99, 99);
            Assert.IsFalse(removed);
        }

        [Test]
        public void TilledCount_TracksRegisteredTiles()
        {
            Assert.AreEqual(0, _grid.TilledCount);
            _grid.TryRegisterTilledTile(1, 1, out _);
            _grid.TryRegisterTilledTile(2, 2, out _);
            Assert.AreEqual(2, _grid.TilledCount);
            _grid.RemoveTilledTile(1, 1);
            Assert.AreEqual(1, _grid.TilledCount);
        }

        [Test]
        public void Clear_RemovesAllTilledTiles()
        {
            _grid.TryRegisterTilledTile(1, 1, out _);
            _grid.TryRegisterTilledTile(2, 2, out _);
            _grid.Clear();
            Assert.AreEqual(0, _grid.TilledCount);
        }

        // ── Bounds nao configurados (permissivo para testes sem SetBounds) ───────────────────────

        [Test]
        public void IsTillable_NoBoundsConfigured_AcceptsAnyTile()
        {
            var zonesNoBounds = new FarmNonArableZones();
            var gridNoBounds = new FarmTileGrid(zonesNoBounds, tileSizeUnits: 1f);
            // Sem SetBounds => widthTiles=0, heightTiles=0 => modo permissivo
            Assert.IsTrue(gridNoBounds.IsTillable(999, 999));
        }
    }
}
