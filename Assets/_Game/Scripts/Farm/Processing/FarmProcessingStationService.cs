using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — dono em runtime das estações de processamento físico da fazenda (queijaria/
    /// barril). Hospeda o motor puro <see cref="FarmProcessingStationModel"/> (única superfície de
    /// job POR DIA — decisão Fase 0), assina <see cref="DayStartedEvent"/> para avançar os jobs e
    /// publica <see cref="ProcessingJobCompletedEvent"/> na coleta.
    ///
    /// - Refs serializadas (InventoryManager) — sem GameObject.Find/FindObjectOfType.
    /// - Comunicação por <see cref="GameEventBus"/>.
    /// - Capture/Restore persistem só IDs/ints na seção farm do save (campo aditivo ProcessingJobs).
    /// - Singleton DontDestroyOnLoad: jobs avançam por dia independentemente da cena ativa.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FarmProcessingStationService : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;

        private readonly FarmProcessingStationModel _model = new FarmProcessingStationModel();
        private InventoryAdapter _inventoryAdapter;
        private int _currentDay = 1;

        private static FarmProcessingStationService _instance;
        public static FarmProcessingStationService Instance => _instance;

        public FarmProcessingStationModel Model => _model;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Liga o InventoryManager por referência (chamado pelo gerador/installer; sem Find).</summary>
        public void BindInventory(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
            _inventoryAdapter = null;
        }

        public bool HasActiveJob(string stationId) => _model.HasActiveJob(stationId);

        public FarmProcessingJob GetJob(string stationId) => _model.GetJob(stationId);

        /// <summary>
        /// Inicia o job da estação pela receita única associada a ela (queijaria→queijo,
        /// barril→vinho). Retorna true só quando o job começa (insumos consumidos).
        /// </summary>
        public bool TryStartJob(string stationId, out string feedback)
        {
            feedback = string.Empty;
            if (!ProcessingRecipeCatalog.TryGetByStation(stationId, out var recipe) || recipe == null)
            {
                feedback = "Estacao de processamento desconhecida.";
                return false;
            }

            var result = _model.StartJob(stationId, recipe.RecipeId, _currentDay, ResolveInventory());
            switch (result)
            {
                case FarmProcessingStationModel.StartResult.Started:
                    feedback = $"Em producao: {recipe.ProcessingDays} dia(s).";
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(feedback));
                    return true;
                case FarmProcessingStationModel.StartResult.StationBusy:
                    feedback = "A estacao ja esta em producao.";
                    return false;
                case FarmProcessingStationModel.StartResult.MissingInput:
                    feedback = $"Faltam insumos ({recipe.InputQuantity}x).";
                    return false;
                case FarmProcessingStationModel.StartResult.InventoryUnavailable:
                    feedback = "Inventario indisponivel.";
                    return false;
                default:
                    feedback = "Receita indisponivel.";
                    return false;
            }
        }

        /// <summary>Coleta o output pronto da estação (idempotente). Publica o evento de conclusão.</summary>
        public bool TryCollect(string stationId, out string feedback)
        {
            feedback = string.Empty;
            var recipeId = ProcessingRecipeCatalog.TryGetByStation(stationId, out var recipe) && recipe != null
                ? recipe.RecipeId
                : string.Empty;

            var result = _model.Collect(stationId, ResolveInventory());
            if (result.Success)
            {
                feedback = "Coletado!";
                GameEventBus.Publish(new ProcessingJobCompletedEvent(stationId, recipeId, result.OutputItemId, result.OutputQuantity));
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Coletado: {result.OutputItemId} x{result.OutputQuantity}."));
                return true;
            }

            feedback = result.InventoryFull ? "Inventario cheio." : "Nada pronto para coletar.";
            return false;
        }

        public FarmProcessingSaveData CaptureSaveData() => _model.Capture();

        /// <summary>Restaura jobs do save. Null (legado) = nenhuma estação em producao (CA-5).</summary>
        public void RestoreFromSaveData(FarmProcessingSaveData saveData)
        {
            _model.Restore(saveData);

            // Após o restore, jobs cujo FinishDay já passou (avanço de dias offline impossível, mas
            // robustez) viram ReadyToCollect imediatamente no dia atual.
            _model.AdvanceDay(_currentDay);
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;
            var becameReady = _model.AdvanceDay(_currentDay);
            foreach (var job in becameReady)
            {
                if (job == null)
                    continue;

                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Producao pronta na estacao: {job.OutputItemId}."));
            }
        }

        private IProcessingInventory ResolveInventory()
        {
            if (_inventoryManager == null)
                return null;

            if (_inventoryAdapter == null || !_inventoryAdapter.Matches(_inventoryManager))
            {
                _inventoryAdapter = new InventoryAdapter(_inventoryManager);
            }

            return _inventoryAdapter;
        }

        /// <summary>Adapta o InventoryManager de cena à porta pura <see cref="IProcessingInventory"/>.</summary>
        private sealed class InventoryAdapter : IProcessingInventory
        {
            private readonly InventoryManager _inventory;

            public InventoryAdapter(InventoryManager inventory)
            {
                _inventory = inventory;
            }

            public bool Matches(InventoryManager inventory) => ReferenceEquals(_inventory, inventory);

            public bool HasItem(string itemId, int amount) => _inventory != null && _inventory.HasItem(itemId, amount);

            public bool RemoveItem(string itemId, int amount) => _inventory != null && _inventory.RemoveItem(itemId, amount);

            public bool AddItem(string itemId, int amount) => _inventory != null && _inventory.AddItem(itemId, amount);
        }
    }
}
