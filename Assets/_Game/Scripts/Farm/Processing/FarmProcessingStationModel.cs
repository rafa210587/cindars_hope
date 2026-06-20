using System;
using System.Collections.Generic;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — motor PURO (sem Unity) de processamento de fazenda por DIAS, reutilizando o
    /// órfão <see cref="FarmProcessingJob"/> (StartDay/FinishDay/estados/Collect idempotente).
    /// É a ÚNICA superfície que cria jobs de processamento por dia (decisão Fase 0): o banco de
    /// crafting WI-14 (segundos) permanece para o cook instantâneo, e o terceiro caminho órfão
    /// (Crafting/CraftingService) está ausente do repo.
    ///
    /// Responsabilidades:
    /// - StartJob: valida a receita/insumos, consome SÓ após validação completa, cria o job;
    /// - AdvanceDay: jobs Processing cujo FinishDay chegou viram ReadyToCollect;
    /// - Collect: idempotente (2ª coleta falha), entrega output e remove o job concluído;
    /// - Capture/Restore: somente IDs/ints (ADR-0006).
    ///
    /// Não conhece InventoryManager nem GameEventBus — recebe a porta de inventário e devolve
    /// resultados; o wrapper MonoBehaviour publica os eventos.
    /// </summary>
    public sealed class FarmProcessingStationModel
    {
        public enum StartResult
        {
            Started = 0,
            StationBusy = 1,
            UnknownRecipe = 2,
            MissingInput = 3,
            InventoryUnavailable = 4
        }

        public readonly struct CollectResult
        {
            public readonly bool Success;
            public readonly string OutputItemId;
            public readonly int OutputQuantity;
            public readonly bool InventoryFull;

            public CollectResult(bool success, string outputItemId, int outputQuantity, bool inventoryFull)
            {
                Success = success;
                OutputItemId = outputItemId;
                OutputQuantity = outputQuantity;
                InventoryFull = inventoryFull;
            }
        }

        // Um job ativo por estação (v1). Jobs já coletados são removidos do dicionário.
        private readonly Dictionary<string, FarmProcessingJob> _jobsByStation = new Dictionary<string, FarmProcessingJob>();

        public bool HasActiveJob(string stationId)
        {
            return !string.IsNullOrEmpty(stationId)
                   && _jobsByStation.TryGetValue(stationId, out var job)
                   && job != null
                   && job.State != ProcessingJobState.Collected;
        }

        public FarmProcessingJob GetJob(string stationId)
        {
            if (string.IsNullOrEmpty(stationId))
                return null;

            return _jobsByStation.TryGetValue(stationId, out var job) ? job : null;
        }

        /// <summary>
        /// Inicia o job da estação consumindo os insumos pelo inventário informado.
        /// Consome SÓ após validar receita + disponibilidade (mitiga insumo perdido em falha).
        /// </summary>
        public StartResult StartJob(string stationId, string recipeId, int currentDay, IProcessingInventory inventory)
        {
            if (inventory == null)
                return StartResult.InventoryUnavailable;

            if (HasActiveJob(stationId))
                return StartResult.StationBusy;

            if (!ProcessingRecipeCatalog.TryGetById(recipeId, out var recipe) || recipe == null)
                return StartResult.UnknownRecipe;

            if (!string.Equals(recipe.StationId, stationId, StringComparison.Ordinal))
                return StartResult.UnknownRecipe;

            if (!inventory.HasItem(recipe.InputItemId, recipe.InputQuantity))
                return StartResult.MissingInput;

            if (!inventory.RemoveItem(recipe.InputItemId, recipe.InputQuantity))
                return StartResult.MissingInput;

            var startDay = currentDay < 1 ? 1 : currentDay;
            var job = new FarmProcessingJob
            {
                JobId = Guid.NewGuid().ToString(),
                StationId = stationId,
                InputItemId = recipe.InputItemId,
                InputQuantity = recipe.InputQuantity,
                OutputItemId = recipe.OutputItemId,
                OutputQuantity = recipe.OutputQuantity,
                StartDay = startDay,
                FinishDay = startDay + recipe.ProcessingDays,
                State = ProcessingJobState.Processing,
                OutputCollected = false
            };

            _jobsByStation[stationId] = job;
            return StartResult.Started;
        }

        /// <summary>
        /// Avança todos os jobs para o dia informado: Processing cujo FinishDay já chegou viram
        /// ReadyToCollect. Idempotente por dia (chamar 2× no mesmo dia não muda nada além do 1º).
        /// Retorna os jobs que transicionaram para ReadyToCollect nesta chamada.
        /// </summary>
        public List<FarmProcessingJob> AdvanceDay(int currentDay)
        {
            var becameReady = new List<FarmProcessingJob>();
            foreach (var job in _jobsByStation.Values)
            {
                if (job == null)
                    continue;

                if (job.IsReadyOnDay(currentDay))
                {
                    job.AdvanceToReady();
                    becameReady.Add(job);
                }
            }

            return becameReady;
        }

        /// <summary>
        /// Coleta o output da estação. Idempotente: só funciona em ReadyToCollect e não coletado;
        /// a 2ª chamada (reload/duplo clique) falha. Em sucesso, entrega o output ao inventário e
        /// remove o job da estação (libera para novo job).
        /// </summary>
        public CollectResult Collect(string stationId, IProcessingInventory inventory)
        {
            if (inventory == null)
                return new CollectResult(false, string.Empty, 0, false);

            var job = GetJob(stationId);
            if (job == null || job.State != ProcessingJobState.ReadyToCollect || job.OutputCollected)
                return new CollectResult(false, job?.OutputItemId ?? string.Empty, 0, false);

            if (!inventory.AddItem(job.OutputItemId, job.OutputQuantity))
            {
                // Inventário cheio: output permanece na estação para nova tentativa.
                return new CollectResult(false, job.OutputItemId, job.OutputQuantity, true);
            }

            // Collect() marca Collected + OutputCollected (idempotência já garantida no job).
            job.Collect();
            var outputItemId = job.OutputItemId;
            var outputQuantity = job.OutputQuantity;
            _jobsByStation.Remove(stationId);
            return new CollectResult(true, outputItemId, outputQuantity, false);
        }

        public FarmProcessingSaveData Capture()
        {
            var data = new FarmProcessingSaveData();
            foreach (var pair in _jobsByStation)
            {
                var job = pair.Value;
                if (job == null || job.State == ProcessingJobState.Collected)
                    continue;

                data.Jobs.Add(new FarmProcessingJobSaveData
                {
                    StationId = job.StationId,
                    RecipeId = ResolveRecipeId(job),
                    OutputItemId = job.OutputItemId,
                    OutputQuantity = job.OutputQuantity,
                    StartDay = job.StartDay,
                    FinishDay = job.FinishDay,
                    State = (int)job.State,
                    OutputCollected = job.OutputCollected
                });
            }

            return data;
        }

        /// <summary>
        /// Restaura os jobs do save. Save legado (null / sem a lista) = nenhum job (CA-5).
        /// Reaplica idempotente: limpa o estado anterior antes de reidratar.
        /// </summary>
        public void Restore(FarmProcessingSaveData saveData)
        {
            _jobsByStation.Clear();
            if (saveData?.Jobs == null)
                return;

            foreach (var jobData in saveData.Jobs)
            {
                if (jobData == null || string.IsNullOrEmpty(jobData.StationId) || jobData.OutputCollected)
                    continue;

                // Resolve receita (fonte de verdade dos insumos/dias). ID desconhecido = ignora.
                if (!ProcessingRecipeCatalog.TryGetById(jobData.RecipeId, out var recipe) || recipe == null)
                    continue;

                var job = new FarmProcessingJob
                {
                    JobId = Guid.NewGuid().ToString(),
                    StationId = jobData.StationId,
                    InputItemId = recipe.InputItemId,
                    InputQuantity = recipe.InputQuantity,
                    OutputItemId = string.IsNullOrEmpty(jobData.OutputItemId) ? recipe.OutputItemId : jobData.OutputItemId,
                    OutputQuantity = jobData.OutputQuantity > 0 ? jobData.OutputQuantity : recipe.OutputQuantity,
                    StartDay = jobData.StartDay,
                    FinishDay = jobData.FinishDay,
                    State = NormalizeRestoredState(jobData.State),
                    OutputCollected = false
                };

                _jobsByStation[job.StationId] = job;
            }
        }

        private static ProcessingJobState NormalizeRestoredState(int state)
        {
            // Apenas Processing/ReadyToCollect são estados ativos persistidos; o resto reidrata
            // como Processing para evitar job preso fora do ciclo.
            switch ((ProcessingJobState)state)
            {
                case ProcessingJobState.ReadyToCollect:
                    return ProcessingJobState.ReadyToCollect;
                case ProcessingJobState.Processing:
                    return ProcessingJobState.Processing;
                default:
                    return ProcessingJobState.Processing;
            }
        }

        private static string ResolveRecipeId(FarmProcessingJob job)
        {
            return ProcessingRecipeCatalog.TryGetByStation(job.StationId, out var recipe) && recipe != null
                ? recipe.RecipeId
                : string.Empty;
        }
    }
}
