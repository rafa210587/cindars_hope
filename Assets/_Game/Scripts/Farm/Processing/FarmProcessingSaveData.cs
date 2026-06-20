using System;
using System.Collections.Generic;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — DTO de save de UM job de processamento ativo numa estação.
    /// Apenas tipos simples / IDs / ints (ADR-0006 / save-dto-simple-types-only): nenhuma
    /// referência Unity. State é o int de <see cref="ProcessingJobState"/>.
    /// </summary>
    [Serializable]
    public class FarmProcessingJobSaveData
    {
        public string StationId;
        public string RecipeId;
        public string OutputItemId;
        public int OutputQuantity;
        public int StartDay;
        public int FinishDay;
        public int State;
        public bool OutputCollected;
    }

    /// <summary>
    /// fable_55 — seção ADITIVA da seção farm do save (GameSaveData.Farm.ProcessingJobs):
    /// lista de jobs de processamento por estação. Ausente em save legado = lista vazia =
    /// nenhuma estação em produção (CA-5). Sem migração; só IDs/ints.
    /// </summary>
    [Serializable]
    public class FarmProcessingSaveData
    {
        public List<FarmProcessingJobSaveData> Jobs = new List<FarmProcessingJobSaveData>();
    }
}
