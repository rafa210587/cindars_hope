using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Inventory;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — estado PURO das pendências temporais dos serviços (CA-3/CA-5). Sem Unity refs:
    /// EditMode-testável e persistido via <see cref="NpcServicesSaveData"/>. Guarda dias/semanas
    /// ABSOLUTOS do calendário (anti-exploit de save-load: ver Riscos da spec). O dia corrente vem do
    /// DayStartedEvent (injetado pelo host); a entrega de encomenda é resolvida quando o dia avança.
    /// </summary>
    public sealed class NpcServicesPendingState
    {
        private readonly List<NpcBookOrderSaveData> _bookOrders = new List<NpcBookOrderSaveData>();
        private int _nextBookOrderId = 1;
        private NpcHuntContractSaveData _huntContract;
        private int _dailyDishLastUsedDay;
        private int _premiumPastureActiveUntilDay;

        public int PendingBookOrderCount => _bookOrders.Count;
        public NpcHuntContractSaveData HuntContract => _huntContract;
        public int DailyDishLastUsedDay => _dailyDishLastUsedDay;
        public int PremiumPastureActiveUntilDay => _premiumPastureActiveUntilDay;

        // ── Encomenda de livro (Yael) — chega no dia +3 ───────────────────────────────────────────

        /// <summary>Registra uma encomenda que chega em <paramref name="deliveryDay"/> (dia da compra + 3).</summary>
        public void AddBookOrder(string bookItemId, int deliveryDay, int paidGold)
        {
            if (string.IsNullOrEmpty(bookItemId) || paidGold < 0) return;
            _bookOrders.Add(new NpcBookOrderSaveData
            {
                OrderId = _nextBookOrderId++, DeliveredBookItemId = bookItemId,
                DeliveryDay = deliveryDay, PaidGold = paidGold, OriginSchemaVersion = 1,
                OriginLots = ToSaveLots(ItemOriginLot.Purchased(1, paidGold))
            });
        }

        // Legacy caller compatibility; new paid orders must use the three-argument overload.
        public void AddBookOrder(string bookItemId, int deliveryDay)
        {
            if (string.IsNullOrEmpty(bookItemId)) return;
            _bookOrders.Add(new NpcBookOrderSaveData
            {
                OrderId = _nextBookOrderId++, DeliveredBookItemId = bookItemId,
                DeliveryDay = deliveryDay, OriginSchemaVersion = 1,
                OriginLots = ToSaveLots(new[] { new ItemOriginLot(ItemOriginKind.LegacyUntracked, 1) })
            });
        }

        public List<NpcBookOrderSaveData> GetDueBookOrders(int currentDay)
        {
            var due = new List<NpcBookOrderSaveData>();
            foreach (var order in _bookOrders)
                if (order != null && order.DeliveryDay <= currentDay) due.Add(CopyOrder(order));
            return due;
        }

        public bool TryCompleteBookOrder(int orderId)
        {
            for (var index = 0; index < _bookOrders.Count; index++)
            {
                if (_bookOrders[index].OrderId != orderId) continue;
                _bookOrders.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Coleta as encomendas cuja entrega chegou (DeliveryDay &lt;= currentDay), removendo-as do
        /// estado. Determinístico e idempotente: uma encomenda só é entregue uma vez. O caller faz o
        /// AddItem no inventário com os ids retornados.
        /// </summary>
        public List<string> CollectDueBookOrders(int currentDay)
        {
            var due = new List<string>();
            for (int i = _bookOrders.Count - 1; i >= 0; i--)
            {
                if (_bookOrders[i] != null && _bookOrders[i].DeliveryDay <= currentDay)
                {
                    due.Add(_bookOrders[i].DeliveredBookItemId);
                    _bookOrders.RemoveAt(i);
                }
            }

            due.Reverse(); // ordem de chegada estável
            return due;
        }

        // ── Contrato de caça (Kael/Zrix) — semanal ────────────────────────────────────────────────

        /// <summary>
        /// True se um novo contrato pode ser emitido nesta semana: não há contrato, OU o contrato atual
        /// é de uma semana anterior (renova semanalmente).
        /// </summary>
        public bool CanIssueHuntContract(int currentWeek)
        {
            return _huntContract == null || _huntContract.IssuedWeek < currentWeek;
        }

        /// <summary>Emite/renova o contrato semanal para o alvo informado.</summary>
        public void IssueHuntContract(string targetEnemyId, int currentWeek)
        {
            _huntContract = new NpcHuntContractSaveData
            {
                TargetEnemyId = targetEnemyId,
                IssuedWeek = currentWeek,
                Completed = false
            };
        }

        /// <summary>Marca o contrato concluído quando o alvo é abatido (idempotente).</summary>
        public bool TryCompleteHuntContract(string enemyId)
        {
            if (_huntContract == null || _huntContract.Completed) return false;
            if (string.IsNullOrEmpty(enemyId) || _huntContract.TargetEnemyId != enemyId) return false;
            _huntContract.Completed = true;
            return true;
        }

        // ── Prato do dia (Mirena/Orlan) — 1×/dia ──────────────────────────────────────────────────

        /// <summary>True se o prato do dia ainda não foi usado HOJE.</summary>
        public bool CanUseDailyDish(int currentDay) => _dailyDishLastUsedDay != currentDay;

        /// <summary>Marca o prato do dia consumido em <paramref name="currentDay"/>.</summary>
        public void MarkDailyDishUsed(int currentDay) => _dailyDishLastUsedDay = currentDay;

        // ── Pasto premium (Eiran) — alimenta automaticamente por N dias ───────────────────────────

        /// <summary>Ativa o pasto premium até <c>currentDay + days - 1</c> (inclusive hoje).</summary>
        public void ActivatePremiumPasture(int currentDay, int days)
        {
            int until = currentDay + System.Math.Max(1, days) - 1;
            if (until > _premiumPastureActiveUntilDay) _premiumPastureActiveUntilDay = until;
        }

        /// <summary>True se o pasto premium ainda cobre <paramref name="currentDay"/> (alimentar hoje).</summary>
        public bool IsPremiumPastureActive(int currentDay) => currentDay <= _premiumPastureActiveUntilDay;

        // ── Save round-trip ────────────────────────────────────────────────────────────────────────

        public NpcServicesSaveData Capture()
        {
            var data = new NpcServicesSaveData
            {
                PendingBookOrders = new List<NpcBookOrderSaveData>(_bookOrders.Count),
                HuntContract = _huntContract,
                DailyDishLastUsedDay = _dailyDishLastUsedDay,
                PremiumPastureActiveUntilDay = _premiumPastureActiveUntilDay
            };

            foreach (var order in _bookOrders)
            {
                if (order != null && !string.IsNullOrEmpty(order.DeliveredBookItemId))
                {
                    data.PendingBookOrders.Add(new NpcBookOrderSaveData
                    {
                        OrderId = order.OrderId,
                        DeliveredBookItemId = order.DeliveredBookItemId,
                        DeliveryDay = order.DeliveryDay,
                        PaidGold = order.PaidGold,
                        OriginSchemaVersion = order.OriginSchemaVersion,
                        OriginLots = CopySaveLots(order.OriginLots)
                    });
                }
            }

            return data;
        }

        /// <summary>Restaura do save. <paramref name="data"/> null (save legado) ⇒ estado vazio, sem erro.</summary>
        public void Restore(NpcServicesSaveData data, int schemaVersion = 5)
        {
            _bookOrders.Clear();
            _nextBookOrderId = 1;
            _huntContract = null;
            _dailyDishLastUsedDay = 0;
            _premiumPastureActiveUntilDay = 0;

            if (data == null) return;

            if (data.PendingBookOrders != null)
            {
                foreach (var order in data.PendingBookOrders)
                {
                    if (order != null && !string.IsNullOrEmpty(order.DeliveredBookItemId))
                    {
                        var restored = new NpcBookOrderSaveData
                        {
                            OrderId = order.OrderId > 0 ? order.OrderId : _nextBookOrderId,
                            DeliveredBookItemId = order.DeliveredBookItemId,
                            DeliveryDay = order.DeliveryDay,
                            PaidGold = order.PaidGold,
                            OriginSchemaVersion = 1,
                            OriginLots = RestoreOrderLots(order, schemaVersion)
                        };
                        // Legacy payloads have no identity; assigning it during migration is stable.
                        if (restored.OrderId >= _nextBookOrderId)
                            _nextBookOrderId = restored.OrderId == int.MaxValue ? int.MaxValue : restored.OrderId + 1;
                        _bookOrders.Add(restored);
                    }
                }
            }

            _huntContract = data.HuntContract;
            _dailyDishLastUsedDay = data.DailyDishLastUsedDay;
            _premiumPastureActiveUntilDay = data.PremiumPastureActiveUntilDay;
        }

        private static NpcBookOrderSaveData CopyOrder(NpcBookOrderSaveData order)
            => new NpcBookOrderSaveData
            {
                OrderId = order.OrderId, DeliveredBookItemId = order.DeliveredBookItemId,
                DeliveryDay = order.DeliveryDay, PaidGold = order.PaidGold,
                OriginSchemaVersion = order.OriginSchemaVersion,
                OriginLots = CopySaveLots(order.OriginLots)
            };

        private static List<ItemOriginLotSaveData> ToSaveLots(IReadOnlyList<ItemOriginLot> lots)
        {
            var saved = new List<ItemOriginLotSaveData>();
            foreach (var lot in lots) saved.Add(lot.ToSaveData());
            return saved;
        }

        private static List<ItemOriginLotSaveData> CopySaveLots(IReadOnlyList<ItemOriginLotSaveData> lots)
        {
            if (lots == null) return null;
            var saved = new List<ItemOriginLotSaveData>(lots.Count);
            foreach (var lot in lots)
                saved.Add(lot == null ? null : new ItemOriginLotSaveData
                {
                    Kind = lot.Kind, Quantity = lot.Quantity,
                    UnitPaidBasisGold = lot.UnitPaidBasisGold,
                    UnitResaleCapGold = lot.UnitResaleCapGold
                });
            return saved;
        }

        private static List<ItemOriginLotSaveData> RestoreOrderLots(NpcBookOrderSaveData order, int schemaVersion)
        {
            if (schemaVersion < 6)
                return ToSaveLots(new[] { new ItemOriginLot(ItemOriginKind.LegacyUntracked, 1) });
            var lots = order.OriginSchemaVersion == 1
                ? ItemOriginLot.FromSaveData(order.OriginLots, 1)
                : ItemOriginLot.Invalid(1);
            if (lots.Count != 1 || order.PaidGold < 0
                || (lots[0].Kind == ItemOriginKind.Purchased
                    && lots[0].UnitPaidBasisGold != order.PaidGold)
                || (lots[0].Kind == ItemOriginKind.LegacyUntracked && order.PaidGold != 0)
                || (lots[0].Kind != ItemOriginKind.Purchased
                    && lots[0].Kind != ItemOriginKind.LegacyUntracked
                    && lots[0].Kind != ItemOriginKind.Invalid))
                lots = ItemOriginLot.Invalid(1);
            return ToSaveLots(lots);
        }
    }
}
