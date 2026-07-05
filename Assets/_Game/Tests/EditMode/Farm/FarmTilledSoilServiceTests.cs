using NUnit.Framework;
using CindarsHope.Farm;
using CindarsHope.Farm.Watering;

namespace CindarsHope.Farm.Tests
{
    /// <summary>
    /// Testes determinísticos do FarmTilledSoilService:
    /// — Arar tile livre / recusar em zona nao-aravel
    /// — Ciclo plantar -> regar -> ProcessDay -> colher com os MESMOS numeros do motor existente
    /// — Limpeza de planta morta
    /// — Fertilizante
    /// </summary>
    [TestFixture]
    public class FarmTilledSoilServiceTests
    {
        private FarmNonArableZones _zones;
        private FarmTileGrid _grid;
        private FarmWateringService _wateringService;
        private FarmTilledSoilService _service;

        [SetUp]
        public void SetUp()
        {
            _zones = new FarmNonArableZones();
            _grid = new FarmTileGrid(_zones, tileSizeUnits: 1f);
            // Bounds permissivos (sem SetBounds) para facilitar testes
            _wateringService = new FarmWateringService();
            _service = new FarmTilledSoilService(_grid, _wateringService);
        }

        private static SeedParams MakeSeed(int growthDays = 3, int regrowDays = 0)
        {
            return new SeedParams
            {
                SeedId = "seed_carrot",
                GrowthDays = growthDays,
                RegrowDays = regrowDays,
                HarvestPairs = new[] { ("item_carrot", 2) },
                IsValid = true
            };
        }

        // ── Arar ─────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void TillTile_FreeTile_Succeeds_AndTileIsRegistered()
        {
            var result = _service.TillTile(5, 5, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(result);
            Assert.IsTrue(_grid.IsTilled(5, 5));
            var logic = _grid.GetTilledLogic(5, 5);
            Assert.IsNotNull(logic);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void TillTile_BlockedByNonArableZone_ReturnsFalse()
        {
            _zones.RegisterBlockedTile(3, 3);
            var result = _service.TillTile(3, 3, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(result);
            Assert.IsFalse(_grid.IsTilled(3, 3));
        }

        [Test]
        public void TillTile_GreenhouseTile_CanBeTilled()
        {
            // Zona bloqueada mas que e estufa deve ser aravel
            _zones.RegisterBlockedTile(7, 7);
            _zones.RegisterGreenhouseTile(7, 7);
            var result = _service.TillTile(7, 7, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(result);
        }

        [Test]
        public void TillTile_WithoutToolAndNoSliceMode_ReturnsFalse()
        {
            var result = _service.TillTile(1, 1, hasTool: false, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(result);
            Assert.IsFalse(_grid.IsTilled(1, 1));
        }

        [Test]
        public void TillTile_WithoutStamina_ReturnsFalse()
        {
            var result = _service.TillTile(1, 1, hasTool: true, temporarySliceMode: false, staminaOk: false);
            Assert.IsFalse(result);
        }

        [Test]
        public void TillTile_AlreadyTilled_ReturnsFalse()
        {
            _service.TillTile(2, 2, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var result = _service.TillTile(2, 2, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(result);
        }

        // ── Plantar ──────────────────────────────────────────────────────────────────────────────

        [Test]
        public void PlantTile_AfterTilling_Succeeds()
        {
            _service.TillTile(1, 1, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var result = _service.PlantTile(1, 1, "seed_carrot",
                seasonAllowed: true, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            Assert.IsTrue(result);
            var logic = _grid.GetTilledLogic(1, 1);
            Assert.AreEqual("seed_carrot", logic.PlantedSeedId);
            Assert.AreEqual(FarmPlotState.PlantedDry, logic.State);
        }

        [Test]
        public void PlantTile_OnNonTilledTile_ReturnsFalse()
        {
            var result = _service.PlantTile(99, 99, "seed_carrot",
                seasonAllowed: true, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            Assert.IsFalse(result);
        }

        [Test]
        public void PlantTile_SeasonDenied_ReturnsFalse()
        {
            _service.TillTile(1, 1, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var result = _service.PlantTile(1, 1, "seed_carrot",
                seasonAllowed: false, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            Assert.IsFalse(result);
        }

        // ── Regar ─────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void WaterTile_AfterTilling_SetsWetState()
        {
            _service.TillTile(2, 2, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var result = _service.WaterTile(2, 2, hasTool: true, temporarySliceMode: false, staminaOk: true, currentDay: 1);
            Assert.IsTrue(result);
            var logic = _grid.GetTilledLogic(2, 2);
            Assert.AreEqual(FarmPlotState.TilledWet, logic.State);
        }

        [Test]
        public void WaterTile_PlantedDry_SetsPlantedWet()
        {
            _service.TillTile(3, 3, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(3, 3, "seed_carrot", true, true, false, true);
            var result = _service.WaterTile(3, 3, hasTool: true, temporarySliceMode: false, staminaOk: true, currentDay: 1);
            Assert.IsTrue(result);
            var logic = _grid.GetTilledLogic(3, 3);
            Assert.AreEqual(FarmPlotState.PlantedWet, logic.State);
        }

        [Test]
        public void WaterTile_NonTilledTile_ReturnsFalse()
        {
            var result = _service.WaterTile(99, 99, hasTool: true, temporarySliceMode: false, staminaOk: true, currentDay: 1);
            Assert.IsFalse(result);
        }

        // ── Ciclo completo: plantar -> regar -> ProcessDay -> colher ──────────────────────────────

        [Test]
        public void FullCropCycle_ThreeDays_HarvestSucceeds()
        {
            // Arrange: arar + plantar
            _service.TillTile(4, 4, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(4, 4, "seed_carrot", true, true, false, true);

            var seed = MakeSeed(growthDays: 3);

            // Dia 1: regar + processar
            _service.WaterTile(4, 4, hasTool: true, temporarySliceMode: false, staminaOk: true, currentDay: 1);
            _service.ProcessDayAllTiles(1, _ => seed);

            // Dia 2: regar + processar
            var logic = _grid.GetTilledLogic(4, 4);
            // Restaurar estado wet (ProcessDay seca apos processar)
            logic.TryWaterSilent();
            _service.ProcessDayAllTiles(2, _ => seed);

            // Dia 3: regar + processar — deve ficar ReadyToHarvest
            logic.TryWaterSilent();
            _service.ProcessDayAllTiles(3, _ => seed);

            Assert.AreEqual(FarmPlotState.ReadyToHarvest, logic.State);

            // Colher
            var outcome = _service.TryHarvestTile(4, 4, seed, yieldModifier: 0f, fertilizerActive: false);
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(1, outcome.Items.Length);
            Assert.AreEqual("item_carrot", outcome.Items[0].itemId);
            Assert.AreEqual(4, outcome.Items[0].amount,
                "A colheita atual inclui +2 unidades da qualidade Excellent.");

            // Confirmar colheita — sem regrow, o solo continua arado e seco no fluxo atual.
            _service.ConfirmHarvest(4, 4, seed);
            Assert.IsTrue(_grid.IsTilled(4, 4));
            Assert.AreEqual(FarmPlotState.TilledDry, _grid.GetTilledLogic(4, 4).State);
        }

        [Test]
        public void ProcessDayAllTiles_DryPlant_DiesAfterThreshold()
        {
            _service.TillTile(5, 5, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(5, 5, "seed_carrot", true, true, false, true);

            var seed = MakeSeed(growthDays: 5);

            // 3 dias sem rega
            for (var d = 1; d <= FarmPlotLogic.DefaultDeathThresholdDays; d++)
            {
                _service.ProcessDayAllTiles(d, _ => seed);
            }

            var logic = _grid.GetTilledLogic(5, 5);
            Assert.IsNotNull(logic);
            Assert.AreEqual(FarmPlotState.Dead, logic.State);
        }

        [Test]
        public void ProcessDayAllTiles_SameDay_IsIdempotent()
        {
            _service.TillTile(6, 6, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(6, 6, "seed_carrot", true, true, false, true);
            var logic = _grid.GetTilledLogic(6, 6);
            logic.TryWaterSilent(); // wet

            var seed = MakeSeed(growthDays: 5);
            _service.ProcessDayAllTiles(1, _ => seed);
            var daysAfterFirst = logic.DaysGrown;

            // Processar o mesmo dia novamente — idempotente
            _service.ProcessDayAllTiles(1, _ => seed);
            Assert.AreEqual(daysAfterFirst, logic.DaysGrown);
        }

        [Test]
        public void FullCycleWithRegrow_HarvestThenRegrows()
        {
            _service.TillTile(7, 7, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(7, 7, "seed_strawberry", true, true, false, true);
            var logic = _grid.GetTilledLogic(7, 7);

            // Seed com regrowth de 2 dias, crescimento total de 6
            var seed = new SeedParams
            {
                SeedId = "seed_strawberry",
                GrowthDays = 6,
                RegrowDays = 2,
                HarvestPairs = new[] { ("item_strawberry", 3) },
                IsValid = true
            };

            // Crescer ate ReadyToHarvest
            for (var d = 1; d <= 6; d++)
            {
                logic.TryWaterSilent();
                _service.ProcessDayAllTiles(d, _ => seed);
            }

            Assert.AreEqual(FarmPlotState.ReadyToHarvest, logic.State);

            // Colher com regrow
            var outcome = _service.TryHarvestTile(7, 7, seed, yieldModifier: 0f, fertilizerActive: false);
            Assert.IsTrue(outcome.Success);
            _service.ConfirmHarvest(7, 7, seed);

            // Tile deve permanecer no grid (regrow ativo)
            Assert.IsTrue(_grid.IsTilled(7, 7));
            Assert.AreEqual(FarmPlotState.PlantedDry, logic.State);
            Assert.AreEqual(4, logic.DaysGrown); // GrowthDays - RegrowDays = 6 - 2 = 4
        }

        // ── Limpeza de planta morta ───────────────────────────────────────────────────────────────

        [Test]
        public void ClearDeadCrop_DeadTile_WithTool_Succeeds()
        {
            _service.TillTile(8, 8, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var logic = _grid.GetTilledLogic(8, 8);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.Dead);

            var (success, _) = _service.ClearDeadCrop(8, 8, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(success);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void ClearDeadCrop_NonTilledTile_ReturnsFalse()
        {
            var (success, _) = _service.ClearDeadCrop(99, 99, hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(success);
        }

        // ── Fertilizante ─────────────────────────────────────────────────────────────────────────

        [Test]
        public void FertilizeTile_SetsFertilizerId()
        {
            _service.TillTile(9, 9, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var result = _service.FertilizeTile(9, 9, "fert_basic", hasInInventory: true);
            Assert.IsTrue(result);
            var logic = _grid.GetTilledLogic(9, 9);
            Assert.AreEqual("fert_basic", logic.FertilizerId);
        }

        [Test]
        public void FertilizeTile_NonTilledTile_ReturnsFalse()
        {
            var result = _service.FertilizeTile(99, 99, "fert_basic", hasInInventory: true);
            Assert.IsFalse(result);
        }

        // ── Harvest com modificador de yield ─────────────────────────────────────────────────────

        [Test]
        public void TryHarvestTile_WithYieldModifier_IncreasesAmount()
        {
            _service.TillTile(10, 10, hasTool: true, temporarySliceMode: false, staminaOk: true);
            var logic = _grid.GetTilledLogic(10, 10);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);

            var seed = MakeSeed(growthDays: 3);
            var outcome = _service.TryHarvestTile(10, 10, seed, yieldModifier: 0.5f, fertilizerActive: false);
            Assert.IsTrue(outcome.Success);
            // baseAmount=2, yieldModifier=0.5 => +1 => 3
            Assert.AreEqual(5, outcome.Items[0].amount,
                "Base 2 + bônus de fertilizante 1 + bônus de qualidade Excellent 2.");
        }

        [Test]
        public void TryHarvestTile_NonReadyState_ReturnsFalse()
        {
            _service.TillTile(11, 11, hasTool: true, temporarySliceMode: false, staminaOk: true);
            _service.PlantTile(11, 11, "seed_carrot", true, true, false, true);

            var seed = MakeSeed(growthDays: 3);
            var outcome = _service.TryHarvestTile(11, 11, seed, yieldModifier: 0f, fertilizerActive: false);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void TryHarvestTile_NonTilledTile_ReturnsFalse()
        {
            var seed = MakeSeed(growthDays: 3);
            var outcome = _service.TryHarvestTile(99, 99, seed, yieldModifier: 0f, fertilizerActive: false);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual("TileNotTilled", outcome.FailureReason);
        }
    }
}
