using System;
using CindarsHope.Farm;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save para o sistema de tiles araveis da fazenda.
    /// Persiste apenas tiles nao-default (arados/plantados/colhiveis/mortos).
    /// Tiles em estado Raw nao sao gravados — ausencia = Raw (back-compat).
    /// ProviderId: "farm_tiles". Campo aditivo GameSaveData.FarmTiles.
    /// </summary>
    public class FarmTilesSectionProvider : ISaveSectionProvider
    {
        private readonly FarmTileGrid _grid;

        public string ProviderId => "farm_tiles";

        public FarmTilesSectionProvider(FarmTileGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        public object Capture(GameSaveData existingSaveData)
        {
            var data = new FarmTilesSaveData();

            foreach (var kv in _grid.AllTilledTiles)
            {
                var tile = kv.Key;
                var logic = kv.Value;

                if (logic == null)
                {
                    continue;
                }

                // Captura via FarmPlotLogic.CaptureSaveData usando plotIndex = 0 (nao usado aqui).
                var plotSave = logic.CaptureSaveData(plotIndex: 0);

                var entry = new FarmTileEntry
                {
                    TileX = tile.x,
                    TileY = tile.y,
                    State = plotSave.State,
                    PlantedSeedId = plotSave.PlantedSeedId ?? string.Empty,
                    DaysGrown = plotSave.DaysGrown,
                    GrowthProgressDays = plotSave.GrowthProgressDays,
                    IsWatered = plotSave.IsWatered,
                    RegrowRemainingDays = plotSave.RegrowRemainingDays,
                    LastUpdatedDay = plotSave.LastUpdatedDay,
                    DaysWithoutWater = plotSave.DaysWithoutWater,
                    LastProcessedDay = plotSave.LastProcessedDay,
                    FertilizerId = plotSave.FertilizerId ?? string.Empty,
                    WateredDaysCount = plotSave.WateredDaysCount
                };

                data.Tiles.Add(entry);
            }

            return data;
        }

        public void Restore(object sectionData)
        {
            if (sectionData == null)
            {
                // Secao ausente: nenhum tile arado (back-compat com saves legados).
                return;
            }

            var farmTilesData = sectionData as FarmTilesSaveData;
            if (farmTilesData == null || farmTilesData.Tiles == null)
            {
                return;
            }

            // Limpa tiles atuais antes de restaurar.
            _grid.Clear();

            foreach (var entry in farmTilesData.Tiles)
            {
                if (entry == null)
                {
                    continue;
                }

                // Cria ou obtem o FarmPlotLogic para o tile (bypassa IsTillable — e restore de save).
                var logic = _grid.GetOrCreateTileLogicForRestore(entry.TileX, entry.TileY);

                // Monta um FarmPlotSaveData a partir do FarmTileEntry para reusar RestoreFromSaveData.
                var plotSave = new CindarsHope.Farm.FarmPlotSaveData
                {
                    PlotIndex = 0,
                    State = entry.State ?? FarmPlotState.TilledDry.ToString(),
                    PlantedSeedId = entry.PlantedSeedId ?? string.Empty,
                    DaysGrown = entry.DaysGrown,
                    GrowthProgressDays = entry.GrowthProgressDays > 0 ? entry.GrowthProgressDays : entry.DaysGrown,
                    IsWatered = entry.IsWatered,
                    RegrowRemainingDays = entry.RegrowRemainingDays,
                    LastUpdatedDay = entry.LastUpdatedDay,
                    DaysWithoutWater = entry.DaysWithoutWater,
                    LastProcessedDay = entry.LastProcessedDay,
                    FertilizerId = entry.FertilizerId ?? string.Empty,
                    WateredDaysCount = entry.WateredDaysCount
                };

                // seedIsResolvable: assume true no restore — o adapter (FarmPlot ou bootstrap)
                // deve verificar se a semente ainda existe no catalogo. Por conservadorismo no
                // nivel do provider, assumimos true para preservar os dados; o sistema de growth
                // ira lidar com seeds invalidas via ProcessDay.
                logic.RestoreFromSaveData(plotSave, seedIsResolvable: true);
            }
        }
    }
}
