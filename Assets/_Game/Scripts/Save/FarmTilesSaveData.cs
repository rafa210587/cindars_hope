using System;
using System.Collections.Generic;

namespace CindarsHope.Save
{
    /// <summary>
    /// DTO de save para tiles araveis da fazenda indexados por coordenada.
    /// Persistidos apenas os tiles NAO-default (tiles araveis/plantados).
    /// Tiles em estado Raw (nao arados) nao sao persistidos — ausencia = Raw.
    /// Sem referencias Unity (ADR-0006 / save-dto-simple-types-only).
    /// Campo ADITIVO em GameSaveData.FarmTiles: saves legados (campo nulo) carregam
    /// sem tiles arados, retrocompativel (CA-3).
    /// </summary>
    [Serializable]
    public class FarmTilesSaveData
    {
        /// <summary>
        /// Entradas de tiles com estado nao-default (arados/plantados/colhiveis/mortos).
        /// </summary>
        public List<FarmTileEntry> Tiles = new List<FarmTileEntry>();
    }

    /// <summary>
    /// Estado de um tile aravel persistido por coordenada (TileX, TileY).
    /// Todos os campos sao simple types — sem refs Unity.
    /// </summary>
    [Serializable]
    public class FarmTileEntry
    {
        // Coordenadas de tile (inteiros absolutos na grade da fazenda).
        public int TileX;
        public int TileY;

        // Estado do plot (nome do enum FarmPlotState — string para robustez de migration).
        public string State;

        // Semente plantada (vazio se nenhuma).
        public string PlantedSeedId;

        // Dias de crescimento acumulados.
        public int DaysGrown;

        // Alias para GrowthProgressDays — compatibilidade com FarmPlotSaveData.
        public int GrowthProgressDays;

        // Se estava regado no momento do save.
        public bool IsWatered;

        // Dias restantes de regrow (crops que re-brotam).
        public int RegrowRemainingDays;

        // Ultimo dia em que o estado foi atualizado.
        public int LastUpdatedDay;

        // Dias sem agua consecutivos.
        public int DaysWithoutWater;

        // Ultimo dia processado (idempotencia de ProcessDay).
        public int LastProcessedDay;

        // ID de fertilizante aplicado (vazio se nenhum).
        public string FertilizerId;

        // Contagem de dias em que foi regado (para calculo de qualidade).
        public int WateredDaysCount;
    }
}
