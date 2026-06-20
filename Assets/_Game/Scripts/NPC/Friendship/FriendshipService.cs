using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Items;
using CindarsHope.Localization;
using UnityEngine;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — serviço de amizade (bootstrap). Envolve o núcleo puro FriendshipState, traduz os
    /// 4 hooks de fonte (conversa/quest/presente/compra) vindos do GameEventBus, publica
    /// FriendshipLevelChangedEvent na transição de nível (subida E descida) e expõe a API estável
    /// GetLevel/GetPoints/AddPoints/IsAtLeast + Capture/Restore para o SaveManager.
    ///
    /// Arquitetura: comunicação só via GameEventBus; sem GameObject.Find em gameplay (o bootstrap
    /// usa FindAnyObjectByType apenas para evitar duplicata — padrão do projeto, igual
    /// FarmDailyGoalRuntimeBootstrap). Estado interno PRIVADO — consumidores leem pela API.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FriendshipService : MonoBehaviour
    {
        private readonly FriendshipState _state = new FriendshipState();
        private int _currentDay = 1;

        // NPC com interação aberta no momento — usado para atribuir a compra (ShopBuy) ao NPC certo
        // sem editar o ShopManager (correlação por evento; uma compra só ocorre com 1 loja aberta).
        private string _activeInteractionNpcId;

        private static FriendshipService _instance;
        public static FriendshipService Instance => _instance;

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
            GameEventBus.Subscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
            GameEventBus.Subscribe<NpcInteractionEndedEvent>(OnNpcInteractionEnded);
            GameEventBus.Subscribe<QuestGiverInteractedEvent>(OnQuestGiverInteracted);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
            GameEventBus.Unsubscribe<NpcInteractionEndedEvent>(OnNpcInteractionEnded);
            GameEventBus.Unsubscribe<QuestGiverInteractedEvent>(OnQuestGiverInteracted);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ── API pública estável (consumida por F25/F28/F35) ──────────────────────────────────────

        public int GetPoints(string npcId) => _state.GetPoints(npcId);
        public int GetLevel(string npcId) => _state.GetLevel(npcId);
        public FriendshipLevel GetLevelEnum(string npcId) => _state.GetLevelEnum(npcId);
        public bool IsAtLeast(string npcId, int level) => _state.IsAtLeast(npcId, level);
        public bool IsAtLeast(string npcId, FriendshipLevel level) => _state.IsAtLeast(npcId, level);

        /// <summary>
        /// Adição genérica com a fonte declarada (aplica o cap da fonte). Mantida para o contrato
        /// AddPoints(npcId, n, fonte) do plano; fontes com cap usam o método específico abaixo.
        /// </summary>
        public void AddPoints(string npcId, int amount, FriendshipSource source)
        {
            switch (source)
            {
                case FriendshipSource.DailyConversation:
                    PublishIfLevelChanged(npcId, _state.RegisterDailyConversation(npcId, _currentDay));
                    break;
                case FriendshipSource.ShopPurchase:
                    PublishIfLevelChanged(npcId, _state.RegisterShopPurchase(npcId, _currentDay));
                    break;
                case FriendshipSource.PersonalQuest:
                    // sem questId aqui ⇒ delega ao hook de quest; soma direta como fallback explícito
                    PublishIfLevelChanged(npcId, _state.AddPoints(npcId, amount));
                    break;
                default:
                    PublishIfLevelChanged(npcId, _state.AddPoints(npcId, amount));
                    break;
            }
        }

        /// <summary>Linha de cabeçalho do Conversar (placeholder UI). Via LocalizationService (ADR-0012).</summary>
        public string GetFriendshipHeaderLine(string npcId)
        {
            int level = GetLevel(npcId);
            // Ex.: "Amizade: nível 2". A chave existe no string table; fallback = a própria chave.
            string label = LocalizationService.Get("ui.friendship.level_label");
            if (label == "ui.friendship.level_label") label = "Amizade: nível";
            return $"{label} {level}";
        }

        // ── Fonte 3: presente — API de integração (gating honesto da emenda V3 §5) ────────────────

        /// <summary>
        /// Presenteia o NPC com um item, lendo o gosto via NpcGiftPreferences (matriz A6). Caller
        /// fornece as preferências do NPC e a definição do item (não há registry central em runtime
        /// — débito registrado). Item sem ItemTag.Giftable = recusa silenciosa (0 ganho/perda).
        /// Acima do cap diário = recusa amigável (Accepted=false; item NÃO deve ser consumido).
        /// </summary>
        public FriendshipState.GiftResult GiveGift(string npcId, NpcGiftPreferences preferences, ItemDefinition item)
        {
            // Porteiro Giftable: item sem a tag não é presenteável.
            if (!GiftTasteClassifier.IsGiftable(item))
            {
                return new FriendshipState.GiftResult(false, GiftTaste.Neutral, default);
            }

            var taste = GiftTasteClassifier.Classify(preferences, item);
            int limit = preferences != null ? preferences.DailyGiftLimit : 1;
            var result = _state.RegisterGift(npcId, taste, _currentDay, limit);
            if (result.Accepted)
            {
                PublishIfLevelChanged(npcId, result.Apply);
            }
            return result;
        }

        // ── Hooks de evento (fontes de ganho) ────────────────────────────────────────────────────

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;
        }

        private void OnNpcInteractionStarted(NpcInteractionStartedEvent evt)
        {
            _activeInteractionNpcId = evt.NpcId;
            // Fonte 1: primeira conversa do dia (+1, 1×/dia/NPC).
            PublishIfLevelChanged(evt.NpcId, _state.RegisterDailyConversation(evt.NpcId, _currentDay));
        }

        private void OnNpcInteractionEnded(NpcInteractionEndedEvent evt)
        {
            if (_activeInteractionNpcId == evt.NpcId)
            {
                _activeInteractionNpcId = null;
            }
        }

        private void OnQuestGiverInteracted(QuestGiverInteractedEvent evt)
        {
            // Fonte 2: side quest do NPC concluída = turn-in no NPC (+8, idempotente por questId).
            if (evt.Mode != QuestGiverInteractionMode.TurnIn) return;
            PublishIfLevelChanged(evt.NpcId, _state.RegisterQuestCompleted(evt.NpcId, evt.QuestId));
        }

        private void OnEconomyTransaction(EconomyTransactionCompletedEvent evt)
        {
            // Fonte 4: compra na loja do NPC (+1, 1×/dia/NPC). Só compras bem-sucedidas, e só quando
            // há uma interação de NPC aberta (atribuição determinística sem editar o ShopManager).
            if (!evt.WasSuccessful) return;
            if (string.IsNullOrEmpty(_activeInteractionNpcId)) return;
            if (!IsBuyTransaction(evt.TransactionType)) return;

            PublishIfLevelChanged(_activeInteractionNpcId, _state.RegisterShopPurchase(_activeInteractionNpcId, _currentDay));
        }

        private static bool IsBuyTransaction(string transactionType)
        {
            if (string.IsNullOrEmpty(transactionType)) return false;
            return transactionType.IndexOf("Buy", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ── Save ─────────────────────────────────────────────────────────────────────────────────

        public FriendshipSaveData CaptureSaveData() => _state.CaptureSaveData();

        public void RestoreFromSaveData(FriendshipSaveData saveData)
        {
            _state.RestoreFromSaveData(saveData);
            Debug.Log($"[FriendshipService] Restored {(saveData?.Entries?.Count ?? 0)} friendship entries from save.", this);
        }

        /// <summary>Setter de dia para wiring externo/teste de borda (normalmente vem do DayStartedEvent).</summary>
        public void SetCurrentDay(int day) => _currentDay = day;

        // ── Publicação de evento de transição de nível (subida E descida) ────────────────────────

        private void PublishIfLevelChanged(string npcId, FriendshipState.ApplyResult result)
        {
            if (!result.LevelChanged) return;
            GameEventBus.Publish(new FriendshipLevelChangedEvent(npcId, result.NewLevel, result.PreviousLevel));
        }
    }
}
