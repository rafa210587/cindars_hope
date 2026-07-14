using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Items;
using CindarsHope.NPC.Events;
using CindarsHope.NPC.Friendship;
using UnityEngine;

namespace CindarsHope.NPC.Gifting
{
    /// <summary>
    /// fable_72 — serviço de runtime do ato de dar presente (singleton). Resolve as preferências do
    /// NPC (matriz A6 §4 via GiftTasteMatrixData) e a definição do item presenteável (id + Giftable +
    /// tags gift_* via GiftTasteMatrixData), chama o núcleo puro GiftGivingProcessor passando os
    /// adaptadores da F26 (FriendshipService) e do inventário (InventoryManager) e, no aceite, publica
    /// NpcGiftReactionEvent no GameEventBus.
    ///
    /// Arquitetura (ADR-0007 / unity-architecture): comunicação de gameplay só via GameEventBus; sem
    /// GameObject.Find/FindObjectOfType em runtime (refs obtidas por GameBootstrap.Instance e
    /// FriendshipService.Instance; o bootstrap usa FindAnyObjectByType só para evitar duplicata —
    /// padrão do projeto). NÃO reimplementa amizade/classificação/cap (tudo da F26).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GiftGivingService : MonoBehaviour
    {
        private static GiftGivingService _instance;
        public static GiftGivingService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// Dá de presente ao NPC o item indicado. Resolve prefs/item da matriz, aplica a ordem da spec
        /// e publica NpcGiftReactionEvent no aceite. Retorna o resultado detalhado (outcome + delta).
        /// </summary>
        public GiftGivingResult TryGiveGift(string npcId, string itemId)
        {
            var friendship = FriendshipService.Instance;
            if (friendship == null)
            {
                Debug.LogWarning(
                    "[GiftGivingService] FriendshipService ausente — presente nao pode ser aplicado " +
                    $"(npc='{npcId}', item='{itemId}'). Wiring/bootstrap da F26 esperado.", this);
                return new GiftGivingResult(GiftGivingOutcome.Failed, GiftTaste.Neutral, 0, false, false);
            }

            var inventory = ResolveInventory();
            if (inventory == null)
            {
                Debug.LogWarning(
                    "[GiftGivingService] InventoryManager ausente (GameBootstrap nao inicializado) — " +
                    $"presente nao pode ser consumido (npc='{npcId}', item='{itemId}').", this);
                return new GiftGivingResult(GiftGivingOutcome.Failed, GiftTaste.Neutral, 0, false, false);
            }

            // Preferências do NPC (matriz §4). Ausente ⇒ null ⇒ fallback neutral no classificador da F26.
            NpcGiftPreferences preferences = GiftTasteMatrixData.TryGetPreferences(npcId);

            // Definição presenteável do item (id + Giftable + tags gift_*). Item não mapeado ⇒ null ⇒
            // tratado como não-Giftable pelo processor (recusa silenciosa, gating honesto do débito A4).
            ItemDefinition item = GiftTasteMatrixData.BuildGiftItemDefinition(itemId);

            var result = GiftGivingProcessor.Process(
                npcId,
                itemId,
                item,
                preferences,
                new FriendshipGiftAdapter(friendship),
                new InventoryGiftAdapter(inventory));

            if (result.ReactionPublished)
            {
                GameEventBus.Publish(new NpcGiftReactionEvent(npcId, itemId, result.Taste, result.AppliedDelta));
                // arch: quebra da direcao Quests->NPC (2026-07-14) — versao minima (so o NpcId) do
                // mesmo fato, para consumidores fora de NPC (ex.: FestivalQuestService).
                GameEventBus.Publish(new NpcGiftAcceptedEvent(npcId));
            }

            return result;
        }

        private static InventoryManager ResolveInventory()
        {
            var boot = GameBootstrap.Instance;
            return boot != null ? boot.InventoryManager : null;
        }

        // ── Adaptadores (mantêm o núcleo puro) ───────────────────────────────────────────────────

        private sealed class FriendshipGiftAdapter : IGiftFriendship
        {
            private readonly FriendshipService _service;
            public FriendshipGiftAdapter(FriendshipService service) { _service = service; }

            public FriendshipState.GiftResult GiveGift(
                string npcId, NpcGiftPreferences preferences, ItemDefinition item)
            {
                return _service.GiveGift(npcId, preferences, item);
            }
        }

        private sealed class InventoryGiftAdapter : IGiftInventory
        {
            private readonly InventoryManager _inventory;
            public InventoryGiftAdapter(InventoryManager inventory) { _inventory = inventory; }

            public bool HasItem(string itemId, int amount) => _inventory.HasItem(itemId, amount);
            public bool RemoveItem(string itemId, int amount) => _inventory.RemoveItem(itemId, amount);
        }
    }
}
