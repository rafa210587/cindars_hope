using System.Linq;
using NUnit.Framework;
using CindarsHope.Farm;
using CindarsHope.Save;
using CindarsHope.Save.Providers;

namespace CindarsHope.Farm.Tests
{
    /// <summary>
    /// Testes de round-trip de save dos tiles araveis:
    /// — Capture -> Restore -> estado identico
    /// — Tile em estado default (Raw) nao e persistido
    /// — Multiplos tiles (arado, plantado, regado, com fertilizante)
    /// — Sem referencias Unity no DTO
    /// — Back-compat: sectionData null = nenhum tile arado
    /// </summary>
    [TestFixture]
    public class FarmTilesSaveRoundTripTests
    {
        private FarmNonArableZones _zones;
        private FarmTileGrid _grid;
        private FarmTilesSectionProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _zones = new FarmNonArableZones();
            _grid = new FarmTileGrid(_zones, tileSizeUnits: 1f);
            _provider = new FarmTilesSectionProvider(_grid);
        }

        private FarmPlotLogic GetOrCreateTile(int x, int y)
        {
            _grid.TryRegisterTilledTile(x, y, out var logic);
            if (logic == null)
            {
                logic = _grid.GetTilledLogic(x, y);
            }

            return logic;
        }

        // ── Tile unico arado ─────────────────────────────────────────────────────────────────────

        [Test]
        public void RoundTrip_SingleTilledDryTile_StatePreserved()
        {
            var logic = GetOrCreateTile(1, 2);
            // Estado: TilledDry (apos TryTill)
            logic.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);

            // Capture
            var data = _provider.Capture(null) as FarmTilesSaveData;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Tiles.Count);
            Assert.AreEqual(1, data.Tiles[0].TileX);
            Assert.AreEqual(2, data.Tiles[0].TileY);
            Assert.AreEqual(FarmPlotState.TilledDry.ToString(), data.Tiles[0].State);

            // Restore em novo grid
            var zonesB = new FarmNonArableZones();
            var gridB = new FarmTileGrid(zonesB, tileSizeUnits: 1f);
            var providerB = new FarmTilesSectionProvider(gridB);
            providerB.Restore(data);

            Assert.IsTrue(gridB.IsTilled(1, 2));
            var logicB = gridB.GetTilledLogic(1, 2);
            Assert.AreEqual(FarmPlotState.TilledDry, logicB.State);
        }

        // ── Tile plantado/regado ──────────────────────────────────────────────────────────────────

        [Test]
        public void RoundTrip_PlantedWetTile_AllFieldsPreserved()
        {
            var logic = GetOrCreateTile(3, 4);
            logic.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);
            logic.TryPlantSeed("seed_carrot", seasonAllowed: true, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            logic.TryWaterSilent();
            logic.TryFertilizePure("fert_basic", hasInInventory: true);

            var seed = new SeedParams
            {
                SeedId = "seed_carrot",
                GrowthDays = 5,
                RegrowDays = 0,
                HarvestPairs = new[] { ("item_carrot", 2) },
                IsValid = true
            };

            // Processar 2 dias (com rega manual a cada dia)
            logic.TryWaterSilent();
            logic.ProcessDay(1, seed);
            logic.TryWaterSilent();
            logic.ProcessDay(2, seed);

            // Capture
            var data = _provider.Capture(null) as FarmTilesSaveData;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Tiles.Count);

            var entry = data.Tiles[0];
            Assert.AreEqual(3, entry.TileX);
            Assert.AreEqual(4, entry.TileY);
            Assert.AreEqual("seed_carrot", entry.PlantedSeedId);
            Assert.AreEqual("fert_basic", entry.FertilizerId);
            Assert.AreEqual(2, entry.LastProcessedDay);

            // Restore
            var zonesB = new FarmNonArableZones();
            var gridB = new FarmTileGrid(zonesB, tileSizeUnits: 1f);
            var providerB = new FarmTilesSectionProvider(gridB);
            providerB.Restore(data);

            var logicB = gridB.GetTilledLogic(3, 4);
            Assert.IsNotNull(logicB);
            Assert.AreEqual(logic.PlantedSeedId, logicB.PlantedSeedId);
            Assert.AreEqual(logic.DaysGrown, logicB.DaysGrown);
            Assert.AreEqual(logic.LastProcessedDay, logicB.LastProcessedDay);
            Assert.AreEqual(logic.FertilizerId, logicB.FertilizerId);
            Assert.AreEqual(logic.WateredDaysCount, logicB.WateredDaysCount);
        }

        // ── Tile default nao persistido ───────────────────────────────────────────────────────────

        [Test]
        public void Capture_EmptyGrid_ReturnsTilesListEmpty()
        {
            // Grid sem tiles arados
            var data = _provider.Capture(null) as FarmTilesSaveData;
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Tiles.Count);
        }

        // ── Multiplos tiles ──────────────────────────────────────────────────────────────────────

        [Test]
        public void RoundTrip_MultipleDistinctTiles_AllRestoredCorrectly()
        {
            // Tile A: arado em (0,0)
            var logicA = GetOrCreateTile(0, 0);
            logicA.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);

            // Tile B: plantado em (5,5)
            var logicB = GetOrCreateTile(5, 5);
            logicB.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);
            logicB.TryPlantSeed("seed_wheat", true, true, false, true);

            // Capture
            var data = _provider.Capture(null) as FarmTilesSaveData;
            Assert.AreEqual(2, data.Tiles.Count);

            // Restore
            var zonesNew = new FarmNonArableZones();
            var gridNew = new FarmTileGrid(zonesNew, tileSizeUnits: 1f);
            var providerNew = new FarmTilesSectionProvider(gridNew);
            providerNew.Restore(data);

            Assert.IsTrue(gridNew.IsTilled(0, 0));
            Assert.IsTrue(gridNew.IsTilled(5, 5));

            var restoredA = gridNew.GetTilledLogic(0, 0);
            var restoredB = gridNew.GetTilledLogic(5, 5);

            Assert.AreEqual(FarmPlotState.TilledDry, restoredA.State);
            Assert.AreEqual("seed_wheat", restoredB.PlantedSeedId);
            Assert.AreEqual(FarmPlotState.PlantedDry, restoredB.State);
        }

        // ── Back-compat: sectionData null ─────────────────────────────────────────────────────────

        [Test]
        public void Restore_NullSectionData_LeavesGridEmpty()
        {
            // Garantir que o grid comeca vazio e permanece apos restore null
            _provider.Restore(null);
            Assert.AreEqual(0, _grid.TilledCount);
        }

        [Test]
        public void Restore_NullSectionData_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _provider.Restore(null));
        }

        // ── Sem referencias Unity no DTO ──────────────────────────────────────────────────────────

        [Test]
        public void FarmTileEntry_HasNoUnityRefs_AllFieldsAreSimpleTypes()
        {
            // Verifica que FarmTileEntry nao tem campos de tipo UnityEngine.Object
            var type = typeof(FarmTileEntry);
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(fieldType);
                Assert.IsFalse(isUnityObject,
                    $"Campo '{field.Name}' tem tipo Unity ref ({fieldType.Name}) — violacao de save-dto-simple-types-only");
            }
        }

        [Test]
        public void FarmTilesSaveData_HasNoUnityRefs_AllFieldsAreSimpleTypes()
        {
            var type = typeof(FarmTilesSaveData);
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(fieldType);
                Assert.IsFalse(isUnityObject,
                    $"Campo '{field.Name}' tem tipo Unity ref ({fieldType.Name}) — violacao de save-dto-simple-types-only");
            }
        }

        // ── ReadyToHarvest round-trip ─────────────────────────────────────────────────────────────

        [Test]
        public void RoundTrip_ReadyToHarvestTile_StatePreserved()
        {
            var logic = GetOrCreateTile(6, 6);
            logic.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            // Restaurar plantedSeedId manualmente (SetStateInternal em ReadyToHarvest preserva seedId)
            // ReadyToHarvest nao limpa o planted seed (ver FarmPlotLogic.SetStateInternal)

            var data = _provider.Capture(null) as FarmTilesSaveData;
            Assert.AreEqual(1, data.Tiles.Count);
            Assert.AreEqual(FarmPlotState.ReadyToHarvest.ToString(), data.Tiles[0].State);

            var zonesB = new FarmNonArableZones();
            var gridB = new FarmTileGrid(zonesB, tileSizeUnits: 1f);
            var providerB = new FarmTilesSectionProvider(gridB);
            providerB.Restore(data);

            var logicB = gridB.GetTilledLogic(6, 6);
            Assert.IsNotNull(logicB);
            Assert.AreEqual(FarmPlotState.ReadyToHarvest, logicB.State);
        }

        // ── Restore sobrescreve grid existente ───────────────────────────────────────────────────

        [Test]
        public void Restore_ClearsExistingTiles_BeforeRestoring()
        {
            // Preexistente no grid
            GetOrCreateTile(1, 1);

            var data = new FarmTilesSaveData();
            data.Tiles.Add(new FarmTileEntry
            {
                TileX = 5,
                TileY = 5,
                State = FarmPlotState.TilledDry.ToString(),
                PlantedSeedId = string.Empty,
                FertilizerId = string.Empty
            });

            _provider.Restore(data);

            // Tile (1,1) deve ter sido removido; (5,5) restaurado
            Assert.IsFalse(_grid.IsTilled(1, 1));
            Assert.IsTrue(_grid.IsTilled(5, 5));
            Assert.AreEqual(1, _grid.TilledCount);
        }
    }
}
