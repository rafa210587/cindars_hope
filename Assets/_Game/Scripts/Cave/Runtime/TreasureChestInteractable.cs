using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_09 — baú de sala de tesouro da caverna. IInteractable + InventoryManager.AddItem
    /// (mesmo padrão do ResourceNode — NÃO um sistema novo de loot). Abre 1x por chestId; o estado
    /// de aberto é persistido no VisitedLevelSnapshot (OpenedChestIds) pelo materializer.
    ///
    /// Loot determinístico por LootSeed (= StableHash(worldSeed|runSeed|level|"treasure_loot"|x|y)).
    /// Preferência por uma LootTableSO/resolver da F06 quando fornecida; caso contrário, tabela fixa
    /// embutida por banda (fallback documentado no escopo).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TreasureChestInteractable : MonoBehaviour, IInteractable
    {
        private string _chestId = string.Empty;
        private int _caveLevel;
        private int _lootSeed;
        private bool _isOpened;
        private InventoryManager _inventoryManager;
        private SpriteRenderer _spriteRenderer;
        private Action<string> _onOpenedPersist;
        // spec_cave_biome_art_profiles_runtime (CV01): sprites opcionais do bioma; null = placeholder
        // de cor atual (fallback-first, critério 14.2).
        private Sprite _closedSprite;
        private Sprite _openSprite;

        public string ChestId => _chestId;
        public bool IsOpened => _isOpened;
        public string InteractionPrompt => _isOpened ? "Baú vazio" : "Abrir baú";

        public void Configure(
            string chestId,
            int caveLevel,
            int lootSeed,
            bool alreadyOpened,
            InventoryManager inventoryManager,
            SpriteRenderer spriteRenderer,
            Action<string> onOpenedPersist,
            Sprite closedSprite = null,
            Sprite openSprite = null)
        {
            _chestId = chestId;
            _caveLevel = caveLevel;
            _lootSeed = lootSeed;
            _isOpened = alreadyOpened;
            _inventoryManager = inventoryManager;
            _spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
            _onOpenedPersist = onOpenedPersist;
            _closedSprite = closedSprite;
            _openSprite = openSprite;
            UpdateVisual();
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_isOpened;
        }

        public void Interact(GameObject interactor)
        {
            if (_isOpened)
            {
                return;
            }

            RebindInventoryIfNeeded();
            _isOpened = true;

            var rewards = RollLoot();
            if (_inventoryManager != null)
            {
                foreach (var reward in rewards)
                {
                    _inventoryManager.AddItem(reward.ItemId, reward.Amount);
                }
            }

            _onOpenedPersist?.Invoke(_chestId);
            GameEventBus.Publish(new TreasureChestOpenedEvent(_chestId, _caveLevel));
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Baú aberto!"));
            UpdateVisual();

            Debug.Log($"TreasureChestInteractable: chest '{_chestId}' opened. Rewards={rewards.Count}.", this);
        }

        /// <summary>Marca como aberto sem rolar loot (restauração de snapshot). Idempotente.</summary>
        public void MarkOpenedFromSnapshot()
        {
            _isOpened = true;
            UpdateVisual();
        }

        // --- Loot (determinístico por LootSeed) ---------------------------------------------------

        private List<TreasureReward> RollLoot()
        {
            var rewards = new List<TreasureReward>();
            var random = new System.Random(_lootSeed);

            // Tabela fixa por banda (fallback documentado). Itens canônicos do ITEM_CATALOG existente.
            var table = ResolveFallbackTable(_caveLevel);
            foreach (var entry in table)
            {
                if (random.NextDouble() <= entry.Chance)
                {
                    var amount = random.Next(entry.MinAmount, entry.MaxAmount + 1);
                    rewards.Add(new TreasureReward { ItemId = entry.ItemId, Amount = Mathf.Max(1, amount) });
                }
            }

            // Garante ao menos 1 recompensa (baú "vale o desvio" — CA-3 / engineering story).
            if (rewards.Count == 0 && table.Count > 0)
            {
                rewards.Add(new TreasureReward { ItemId = table[0].ItemId, Amount = 1 });
            }

            return rewards;
        }

        private static List<TreasureLootEntry> ResolveFallbackTable(int caveLevel)
        {
            // Recompensa escala por banda: minérios mais ricos em profundidade. IDs canônicos
            // confirmados no projeto (item_material_*). Mantém o baú útil sem assets novos.
            var table = new List<TreasureLootEntry>
            {
                new TreasureLootEntry("item_material_stone", 1f, 2, 4)
            };

            if (caveLevel >= 11)
            {
                table.Add(new TreasureLootEntry("item_material_copper_ore", 0.7f, 1, 3));
            }

            if (caveLevel >= 41)
            {
                table.Add(new TreasureLootEntry("item_material_iron_ore", 0.6f, 1, 3));
            }

            return table;
        }

        private void RebindInventoryIfNeeded()
        {
            if (_inventoryManager != null)
            {
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null)
            {
                _inventoryManager = bootstrap.InventoryManager;
            }
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            // spec_cave_biome_art_profiles_runtime (CV01): sprite do bioma vence quando presente;
            // ausência de qualquer um dos dois mantém o placeholder de cor atual (fallback-first).
            var customSprite = _isOpened ? _openSprite : _closedSprite;
            if (customSprite != null)
            {
                _spriteRenderer.sprite = customSprite;
                _spriteRenderer.color = Color.white;
                return;
            }

            _spriteRenderer.color = _isOpened
                ? new Color(0.45f, 0.4f, 0.25f, 0.7f)  // baú aberto/apagado
                : new Color(1f, 0.85f, 0.3f, 1f);      // baú dourado fechado (telegraph de recompensa)
        }

        private sealed class TreasureReward
        {
            public string ItemId = string.Empty;
            public int Amount;
        }

        private readonly struct TreasureLootEntry
        {
            public readonly string ItemId;
            public readonly float Chance;
            public readonly int MinAmount;
            public readonly int MaxAmount;

            public TreasureLootEntry(string itemId, float chance, int minAmount, int maxAmount)
            {
                ItemId = itemId;
                Chance = chance;
                MinAmount = minAmount;
                MaxAmount = maxAmount;
            }
        }
    }
}
