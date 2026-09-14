using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
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
            if (transform.parent == null)
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

        public DepositResult DepositFromInventory(InventoryManager inventory, string itemId,
            int quantity, int qualityTier, float baseValue)
        {
            if (inventory == null || quantity <= 0 || !inventory.TryPrepareRemoval(itemId,
                    quantity, out var selection)) return DepositResult.Fail("InventoryUnavailable");
            if (_service == null) BuildService();
            var result = _service.Deposit(DefaultSellPointId, itemId, quantity, qualityTier,
                baseValue, _currentDay, selection.CopyLots());
            if (!result.Success) return result;
            if (!inventory.TryCommitRemoval(selection, false))
            {
                if (!_service.CancelDeposit(result.CreatedEntry))
                    throw new System.InvalidOperationException("Shipping deposit rollback failed.");
                return DepositResult.Fail("InventoryChanged");
            }
            inventory.PublishCommittedInventoryChange(itemId, -quantity);
            return result;
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
            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager as CindarsHope.Player.PlayerManager : null;
            ProcessOvernight(dayNumber, playerManager);
        }

        // Explicit wallet seam keeps the payment transaction independently testable.
        public void ProcessOvernight(int dayNumber, CindarsHope.Player.PlayerManager playerManager)
        {
            if (_service == null) BuildService();

            var batch = _service.ProcessDayBatch(dayNumber);
            if (batch == null || batch.State != ShippingBatchState.Pending
                || batch.Entries.Count == 0)
            {
                return;
            }

            var totalGold = batch.TotalGoldChecked;
            var itemCount = batch.ItemCount;

            if (playerManager == null || !playerManager.CanCreditGoldChecked(totalGold)
                || !playerManager.TryStageGoldMutation(totalGold, out var walletToken))
            {
                return;
            }

            var committed = false;
            var entriesMarked = false;
            try
            {
                if (!_service.TryCommitBatch(batch)) return;
                entriesMarked = true;
                // GoldChanged subscribers now observe both the wallet and shipping as committed.
                if (!playerManager.TryCommitGoldMutation(walletToken)) return;
                committed = true;
            }
            finally
            {
                if (!committed)
                {
                    var entriesRestored = !entriesMarked || _service.TryRollbackBatch(batch);
                    var walletRestored = playerManager.TryRollbackGoldMutation(walletToken);
                    if (!entriesRestored || !walletRestored)
                        throw new System.InvalidOperationException("Shipping payment rollback failed.");
                }
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
                    State = (int)entry.State,
                    ItemInstanceId = entry.ItemInstanceId,
                    ProcessedPaymentId = entry.ProcessedPaymentId,
                    OriginSchemaVersion = 1,
                    OriginLots = entry.OriginLots.ConvertAll(lot => lot.ToSaveData())
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
            if (saveData == null || saveData.Entries == null)
            {
                _pendingEntries.Clear();
                BuildService();
                return;
            }

            var restored = new List<PendingShippingEntry>();
            var seenIds = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var saved in saveData.Entries)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.ItemId)
                    || saved.Quantity <= 0 || string.IsNullOrWhiteSpace(saved.EntryId)
                    || !seenIds.Add(saved.EntryId))
                    throw new System.InvalidOperationException("Invalid shipping entry in save.");
                var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
                var inventory = bootstrap != null ? bootstrap.InventoryManager as InventoryManager : null;
                if (inventory != null && !inventory.TryGetItemData(saved.ItemId, out _))
                    throw new System.InvalidOperationException("Unknown shipping item ID in save.");
                var lots = saved.OriginSchemaVersion == 1
                    ? ItemOriginLot.FromSaveData(saved.OriginLots, saved.Quantity)
                    : ItemOriginLot.Invalid(saved.Quantity);
                var validLots = new List<ItemOriginLot>();
                var invalidQuantity = 0;
                foreach (var lot in lots)
                {
                    if (lot.Kind == ItemOriginKind.Invalid) invalidQuantity += lot.Quantity;
                    else validLots.Add(lot.Copy(lot.Quantity));
                }
                var pendingWithValidUnits = saved.State == (int)ShippingEntryState.Pending
                    && validLots.Count > 0;
                var entry = new PendingShippingEntry
                {
                    ShippingEntryId = saved.EntryId,
                    SellPointId = string.IsNullOrEmpty(saved.SellPointId) ? DefaultSellPointId : saved.SellPointId,
                    ItemId = saved.ItemId,
                    Quantity = pendingWithValidUnits ? saved.Quantity - invalidQuantity : saved.Quantity,
                    QualityTier = saved.QualityTier,
                    BaseValueSnapshot = saved.BaseValueSnapshot,
                    DepositedDay = saved.DepositedDay,
                    ProcessOnDay = saved.ProcessOnDay,
                    State = invalidQuantity > 0 && !pendingWithValidUnits
                        && saved.State == (int)ShippingEntryState.Pending
                        ? ShippingEntryState.Invalid
                        : System.Enum.IsDefined(typeof(ShippingEntryState), saved.State)
                        ? (ShippingEntryState)saved.State : ShippingEntryState.Invalid,
                    ItemInstanceId = saved.ItemInstanceId,
                    ProcessedPaymentId = saved.ProcessedPaymentId,
                    OriginLots = pendingWithValidUnits ? validLots : lots
                };
                restored.Add(entry);
                if (pendingWithValidUnits && invalidQuantity > 0)
                {
                    var retainedId = saved.EntryId + "_invalid";
                    if (!seenIds.Add(retainedId))
                        throw new System.InvalidOperationException("Duplicate shipping entry ID in save.");
                    restored.Add(new PendingShippingEntry
                    {
                        ShippingEntryId = retainedId,
                        SellPointId = entry.SellPointId,
                        ItemId = entry.ItemId,
                        ItemInstanceId = entry.ItemInstanceId,
                        Quantity = invalidQuantity,
                        QualityTier = entry.QualityTier,
                        BaseValueSnapshot = entry.BaseValueSnapshot,
                        DepositedDay = entry.DepositedDay,
                        ProcessOnDay = entry.ProcessOnDay,
                        State = ShippingEntryState.Invalid,
                        OriginLots = ItemOriginLot.Invalid(invalidQuantity)
                    });
                }
            }

            _pendingEntries.Clear();
            _pendingEntries.AddRange(restored);
            BuildService();

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
        public static ShippingBinRuntimeService Install(Transform owner)
        {
            var existing = Object.FindAnyObjectByType<ShippingBinRuntimeService>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("ShippingBinRuntimeService");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<ShippingBinRuntimeService>();
            Debug.Log("[ShippingBinRuntimeBootstrap] ShippingBinRuntimeService instanciado via bootstrap.");
            return service;
        }
    }
}
