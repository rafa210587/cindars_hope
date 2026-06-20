using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Shipping
{
    /// <summary>
    /// fable_54 — host runtime (padrao F15 / runtime-bootstrap) que da VIDA ao modulo orfao
    /// FarmShippingService. Dono da lista de envios pendentes. No DayStartedEvent processa o batch da
    /// manha (ProcessDayBatch idempotente, canal 0.95 via ShippingPriceResolver), credita o ouro UMA
    /// vez via PlayerManager (ref de bootstrap) e publica EconomyTransactionCompletedEvent (feedback
    /// existente via ShippingSummaryService) + ShippingBatchProcessedEvent (resumo p/ HUD).
    ///
    /// NUNCA reescreve FarmShippingService/ShippingPriceResolver — apenas hospeda e liga.
    /// Comunicacao de gameplay via GameEventBus; PlayerManager/InventoryManager sao refs de wiring
    /// (excecao de bootstrap permitida por event_rules), nunca FindObjectOfType de gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShippingBinRuntimeService : MonoBehaviour
    {
        public const string DefaultSellPointId = "farm_shipping_bin_01";

        private static ShippingBinRuntimeService _instance;
        public static ShippingBinRuntimeService Instance => _instance;

        private readonly List<PendingShippingEntry> _pendingEntries = new List<PendingShippingEntry>();
        private FarmShippingService _service;
        private int _currentDay = 1;

        public int CurrentDay => _currentDay;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            BuildService();
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

        private void BuildService()
        {
            // Sellability: protege itens de quest/key. SellableItemPolicy ja decide o resto na venda
            // imediata; aqui o servico orfao recusa quest/key por contrato. Sem itens proibidos
            // hardcoded alem do que o policy/sellability cobre — listas vazias = comportamento padrao.
            var sellability = new ItemSellabilityProvider();
            _service = new FarmShippingService(_pendingEntries, new ShippingPriceResolver(), sellability);
        }

        public void SetDay(int dayNumber)
        {
            _currentDay = Mathf.Max(1, dayNumber);
        }

        /// <summary>
        /// Deposita um item para envio. Recusa de sellability NAO remove do inventario (o chamador
        /// — o interactable — so remove apos Success). Retorna o resultado do servico orfao.
        /// </summary>
        public DepositResult Deposit(string itemId, int quantity, int qualityTier, float baseValue)
        {
            if (_service == null) BuildService();
            return _service.Deposit(DefaultSellPointId, itemId, quantity, qualityTier, baseValue, _currentDay);
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            SetDay(evt.DayNumber);
            ProcessOvernight(evt.DayNumber);
        }

        /// <summary>
        /// Processa o batch do dia: idempotente (entradas ja processadas sao ignoradas pelo servico),
        /// credito unico via PlayerManager, eventos de feedback/resumo. Seguro de chamar 2x no mesmo
        /// dia (segundo batch vem vazio) e apos reload (estado persistido).
        /// </summary>
        public void ProcessOvernight(int dayNumber)
        {
            if (_service == null) BuildService();

            var batch = _service.ProcessDayBatch(dayNumber);
            if (batch == null || batch.Entries.Count == 0)
            {
                return;
            }

            var totalGold = Mathf.RoundToInt(batch.TotalGold);
            var itemCount = 0;
            foreach (var entry in batch.Entries)
            {
                itemCount += entry.Quantity;
            }

            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
            if (playerManager != null && totalGold > 0)
            {
                playerManager.AddGold(totalGold);
            }

            // Feedback existente: ShippingSummaryService escuta EconomyTransactionCompletedEvent.
            // TransactionType "shipping" => FarmDailyGoalService tambem conta a venda overnight.
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(
                true, "shipping", string.Empty, itemCount, totalGold,
                $"Envio processado: {itemCount} itens por {totalGold}g."));

            GameEventBus.Publish(new ShippingBatchProcessedEvent(dayNumber, totalGold, itemCount, batch.Entries.Count));

            Debug.Log($"[ShippingBinRuntimeService] Batch dia {dayNumber}: {itemCount} itens, +{totalGold}g.", this);
        }

        // ─── Save (campo aditivo na secao farm; dono desta lista) ───

        public PendingShippingSaveData CaptureSaveData()
        {
            var data = new PendingShippingSaveData();
            foreach (var entry in _pendingEntries)
            {
                if (entry == null) continue;
                data.Entries.Add(new PendingShippingEntrySaveData
                {
                    EntryId = entry.ShippingEntryId,
                    SellPointId = entry.SellPointId,
                    ItemId = entry.ItemId,
                    Quantity = entry.Quantity,
                    QualityTier = entry.QualityTier,
                    BaseValueSnapshot = entry.BaseValueSnapshot,
                    DepositedDay = entry.DepositedDay,
                    ProcessOnDay = entry.ProcessOnDay,
                    State = (int)entry.State
                });
            }
            return data;
        }

        /// <summary>
        /// Restaura a batch pendente. saveData nulo/ausente (legado) => lista vazia, sem erro.
        /// Idempotente: limpa antes de repovoar (nao duplica entradas apos load).
        /// </summary>
        public void RestoreFromSaveData(PendingShippingSaveData saveData)
        {
            _pendingEntries.Clear();
            BuildService();

            if (saveData == null || saveData.Entries == null)
            {
                return;
            }

            foreach (var saved in saveData.Entries)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.ItemId)) continue;
                _pendingEntries.Add(new PendingShippingEntry
                {
                    ShippingEntryId = string.IsNullOrEmpty(saved.EntryId)
                        ? System.Guid.NewGuid().ToString("N").Substring(0, 16)
                        : saved.EntryId,
                    SellPointId = string.IsNullOrEmpty(saved.SellPointId) ? DefaultSellPointId : saved.SellPointId,
                    ItemId = saved.ItemId,
                    Quantity = saved.Quantity,
                    QualityTier = saved.QualityTier,
                    BaseValueSnapshot = saved.BaseValueSnapshot,
                    DepositedDay = saved.DepositedDay,
                    ProcessOnDay = saved.ProcessOnDay,
                    State = (ShippingEntryState)saved.State
                });
            }

            Debug.Log($"[ShippingBinRuntimeService] Restaurado batch pendente: {_pendingEntries.Count} entradas.", this);
        }

        // Para testes/diagnostico (sem expor mutacao externa do estado).
        public IReadOnlyList<PendingShippingEntry> PendingEntries => _pendingEntries;
    }

    /// <summary>
    /// Garante ShippingBinRuntimeService em runtime (mesmo padrao do FarmDailyGoalRuntimeBootstrap /
    /// WorldWeatherRuntimeBootstrap). FindAnyObjectByType e permitido aqui: wiring de bootstrap, nao
    /// comunicacao de gameplay.
    /// </summary>
    public static class ShippingBinRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<ShippingBinRuntimeService>() != null)
            {
                return;
            }

            var go = new GameObject("ShippingBinRuntimeService");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<ShippingBinRuntimeService>();
            Debug.Log("[ShippingBinRuntimeBootstrap] ShippingBinRuntimeService instanciado via bootstrap.");
        }
    }
}
