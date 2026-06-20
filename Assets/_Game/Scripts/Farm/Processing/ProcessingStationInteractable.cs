using CindarsHope.Core.Events;
using CindarsHope.Core;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — estação física de processamento (queijaria/barril) como IInteractable.
    /// Estado ocioso: deposita insumos e inicia o job; estado pronto: coleta o output; estado em
    /// produção: mostra "Em producao: N dia(s)". Delega ao <see cref="FarmProcessingStationService"/>
    /// (única superfície de job). Ref do serviço resolvida por SerializeField OU pelo singleton
    /// Instance (sem GameObject.Find) — o gerador liga a ref; o Instance é fallback de runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProcessingStationInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _stationId = ProcessingRecipeCatalog.StationCheesePressId;
        [SerializeField] private FarmProcessingStationService _service;

        public string StationId => _stationId;

        public void Configure(string stationId, FarmProcessingStationService service)
        {
            if (!string.IsNullOrEmpty(stationId))
                _stationId = stationId;
            if (service != null)
                _service = service;
        }

        public string InteractionPrompt
        {
            get
            {
                var service = ResolveService();
                var displayName = ResolveStationDisplayName();
                if (service == null)
                    return displayName;

                var job = service.GetJob(_stationId);
                if (job == null || job.State == ProcessingJobState.Collected)
                {
                    return ResolveDepositPrompt();
                }

                if (job.State == ProcessingJobState.ReadyToCollect)
                {
                    return $"Coletar ({displayName})";
                }

                var remaining = Mathf.Max(0, job.FinishDay - CurrentDayHint(job));
                return remaining <= 0
                    ? $"Quase pronto ({displayName})"
                    : $"Em producao: {remaining} dia(s)";
            }
        }

        public bool CanInteract(GameObject interactor) => ResolveService() != null;

        public void Interact(GameObject interactor)
        {
            var service = ResolveService();
            if (service == null)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Estacao de processamento indisponivel."));
                return;
            }

            var job = service.GetJob(_stationId);
            if (job != null && job.State == ProcessingJobState.ReadyToCollect)
            {
                service.TryCollect(_stationId, out _);
                return;
            }

            if (job != null && job.State == ProcessingJobState.Processing)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(InteractionPrompt));
                return;
            }

            service.TryStartJob(_stationId, out _);
        }

        private FarmProcessingStationService ResolveService()
        {
            if (_service != null)
                return _service;

            // Fallback de runtime: singleton DontDestroyOnLoad (sem busca de cena).
            _service = FarmProcessingStationService.Instance;
            return _service;
        }

        private string ResolveDepositPrompt()
        {
            if (!ProcessingRecipeCatalog.TryGetByStation(_stationId, out var recipe) || recipe == null)
                return "Depositar";

            return $"Depositar {recipe.InputQuantity}x";
        }

        private string ResolveStationDisplayName()
        {
            if (string.Equals(_stationId, ProcessingRecipeCatalog.StationWineBarrelId, System.StringComparison.Ordinal))
                return "Barril de Vinho";
            if (string.Equals(_stationId, ProcessingRecipeCatalog.StationCheesePressId, System.StringComparison.Ordinal))
                return "Queijaria";
            return "Processamento";
        }

        // O job não guarda o dia atual; para o prompt usamos o StartDay como base inferior.
        // O número exibido é aproximado e puramente informativo (a transição real é por evento).
        private static int CurrentDayHint(FarmProcessingJob job)
        {
            return job.StartDay;
        }
    }
}
