using System;
using System.Collections.Generic;
using CindarsHope.Farm.Crops;
using CindarsHope.Farm.Watering;
using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Servico de solo aravel por tile. Delega a logica de arar/plantar/regar/colher/processar-dia
    /// aos processors puros ja existentes (FarmPlotLogic, FarmWateringService, CropGrowthProcessor,
    /// CropQualityResolver) sem duplicar nenhuma formula. Nao tem dependencia de UnityEngine em
    /// logica de gameplay — Vector2Int e usado apenas como chave de coordenada.
    /// </summary>
    public class FarmTilledSoilService
    {
        private readonly FarmTileGrid _grid;
        private readonly FarmWateringService _wateringService;

        // Mapa de FarmPlotWaterState por tile (para FarmWateringService).
        private readonly Dictionary<Vector2Int, FarmPlotWaterState> _waterStates
            = new Dictionary<Vector2Int, FarmPlotWaterState>();

        public FarmTilledSoilService(FarmTileGrid grid, FarmWateringService wateringService)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _wateringService = wateringService ?? throw new ArgumentNullException(nameof(wateringService));
        }

        // ── Arar ─────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tenta arar o tile em (tileX, tileY). Retorna false se o tile nao e aravel, ja esta arado
        /// ou as condicoes de ferramenta/stamina nao sao atendidas.
        /// </summary>
        public bool TillTile(int tileX, int tileY, bool hasTool, bool temporarySliceMode, bool staminaOk)
        {
            // Verifica se e aravel e obtem/cria o logic
            if (!_grid.TryRegisterTilledTile(tileX, tileY, out var logic))
            {
                // Ja existe um tile arado nesta posicao — nao faz nada.
                // IsTillable false tambem cai aqui.
                return false;
            }

            // O logic foi criado com estado Raw. Delega para FarmPlotLogic.TryTill.
            var success = logic.TryTill(hasTool, temporarySliceMode, staminaOk);
            if (!success)
            {
                // Nao conseguiu arar — remove o tile que acabou de criar.
                _grid.RemoveTilledTile(tileX, tileY);
                return false;
            }

            // Cria o water state correspondente.
            EnsureWaterState(tileX, tileY);
            return true;
        }

        // ── Plantar ──────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tenta plantar uma semente no tile. Delega a FarmPlotLogic.TryPlantSeed.
        /// </summary>
        public bool PlantTile(int tileX, int tileY, string seedId, bool seasonAllowed,
            bool hasInInventory, bool temporaryBypass, bool staminaOk)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return false;
            }

            return logic.TryPlantSeed(seedId, seasonAllowed, hasInInventory, temporaryBypass, staminaOk);
        }

        // ── Regar ─────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tenta regar o tile manualmente. Delega a FarmWateringService + FarmPlotLogic.TryWaterSilent.
        /// </summary>
        public bool WaterTile(int tileX, int tileY, bool hasTool, bool temporarySliceMode,
            bool staminaOk, int currentDay)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return false;
            }

            // Verifica condicoes de ferramenta/stamina sem modificar estado.
            if (!hasTool && !temporarySliceMode)
            {
                return false;
            }

            if (!staminaOk)
            {
                return false;
            }

            // Aplica rega via FarmWateringService (registra fonte Manual no water state).
            var waterState = EnsureWaterState(tileX, tileY);
            _wateringService.ApplyManualWatering(waterState, currentDay);

            // Atualiza o estado do plot via FarmPlotLogic.
            logic.TryWaterSilent();
            return true;
        }

        /// <summary>
        /// Aplica chuva a todos os tiles araveis externos (nao-estufa). Delega a FarmWateringService.
        /// </summary>
        public void ApplyRainToAllTiles(int currentDay, bool isStorm = false)
        {
            _wateringService.ApplyRainWatering(GetAllWaterStates(), currentDay, isStorm);

            // Atualiza o estado dos plots que foram regados pela chuva.
            foreach (var kv in _grid.AllTilledTiles)
            {
                var key = kv.Key;
                if (!_waterStates.TryGetValue(key, out var waterState))
                {
                    continue;
                }

                if (waterState.IsWateredToday)
                {
                    kv.Value.TryWaterSilent();
                }
            }
        }

        // ── Colher ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tenta colher o tile. Retorna o HarvestOutcome do FarmPlotLogic.
        /// O chamador e responsavel por adicionar os itens ao inventario e entao chamar
        /// ConfirmHarvest para atualizar o estado pos-colheita.
        /// </summary>
        public HarvestOutcome TryHarvestTile(int tileX, int tileY, SeedParams seed,
            float yieldModifier, bool fertilizerActive)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return new HarvestOutcome { Success = false, FailureReason = "TileNotTilled" };
            }

            return logic.TryHarvestPure(seed, yieldModifier, fertilizerActive);
        }

        /// <summary>
        /// Confirma a colheita (deve ser chamado apos adicionar os itens ao inventario).
        /// Se o plot resetar para Raw apos a colheita, remove o tile do grid.
        /// </summary>
        public void ConfirmHarvest(int tileX, int tileY, SeedParams seed)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return;
            }

            logic.OnHarvestCompleted(seed);

            // Se o estado voltou para Raw (sem regrowth), remove o tile do grid.
            if (logic.State == FarmPlotState.Raw)
            {
                _grid.RemoveTilledTile(tileX, tileY);
                _waterStates.Remove(new Vector2Int(tileX, tileY));
            }
        }

        // ── Processar dia ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Processa a virada de dia para todos os tiles arados. Retorna lista de resultados por tile.
        /// </summary>
        public List<(Vector2Int tile, DayResult result)> ProcessDayAllTiles(int day, Func<string, SeedParams> seedResolver)
        {
            if (seedResolver == null)
            {
                throw new ArgumentNullException(nameof(seedResolver));
            }

            var results = new List<(Vector2Int, DayResult)>();

            // Iteramos sobre copia das chaves para permitir remocao segura.
            var keys = new List<Vector2Int>();
            foreach (var kv in _grid.AllTilledTiles)
            {
                keys.Add(kv.Key);
            }

            foreach (var key in keys)
            {
                var logic = _grid.GetTilledLogic(key.x, key.y);
                if (logic == null)
                {
                    continue;
                }

                // Resolve o seed se o tile tem planta.
                var seed = string.IsNullOrEmpty(logic.PlantedSeedId)
                    ? new SeedParams { IsValid = false }
                    : seedResolver(logic.PlantedSeedId);

                logic.SetCurrentDay(day);
                var dayResult = logic.ProcessDay(day, seed);
                results.Add((key, dayResult));

                // Seca o water state apos processar o dia.
                if (_waterStates.TryGetValue(key, out var waterState))
                {
                    waterState.ResetDayWaterState();
                }
            }

            return results;
        }

        // ── Fertilizar ───────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Aplica fertilizante ao tile. Delega a FarmPlotLogic.TryFertilizePure.
        /// </summary>
        public bool FertilizeTile(int tileX, int tileY, string fertilizerId, bool hasInInventory)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return false;
            }

            return logic.TryFertilizePure(fertilizerId, hasInInventory);
        }

        // ── Limpar planta morta ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tenta limpar um tile com planta morta. Se bem-sucedido e o tile voltar para TilledDry,
        /// mantemos o tile no grid. Retorna (success, clearedSeedId).
        /// </summary>
        public (bool success, string clearedSeedId) ClearDeadCrop(int tileX, int tileY,
            bool hasTool, bool temporarySliceMode, bool staminaOk)
        {
            var logic = _grid.GetTilledLogic(tileX, tileY);
            if (logic == null)
            {
                return (false, string.Empty);
            }

            return logic.TryClearDead(hasTool, temporarySliceMode, staminaOk);
        }

        // ── Internos ─────────────────────────────────────────────────────────────────────────────

        private FarmPlotWaterState EnsureWaterState(int tileX, int tileY)
        {
            var key = new Vector2Int(tileX, tileY);
            if (!_waterStates.TryGetValue(key, out var state))
            {
                state = new FarmPlotWaterState
                {
                    PlotId = $"tile_{tileX}_{tileY}",
                    TilePosition = key,
                    IsExternal = true,
                    IsInterior = false,
                    IsGreenhouse = false
                };
                _waterStates[key] = state;
            }

            return state;
        }

        private IEnumerable<FarmPlotWaterState> GetAllWaterStates()
        {
            return _waterStates.Values;
        }
    }
}
