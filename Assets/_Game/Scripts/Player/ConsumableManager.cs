using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class ConsumableManager : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
        }

        public void Shutdown()
        {
            _isInitialized = false;
        }

        public bool TryConsumeItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || !_isInitialized)
                return false;

            if (_itemDatabase == null)
            {
                Debug.LogWarning("ConsumableManager: ItemDatabase not assigned.");
                return false;
            }

            if (!_itemDatabase.TryGetById(itemId, out var itemData) || itemData == null)
                return false;

            if (!_inventoryManager.HasItem(itemId))
                return false;

            if (!_inventoryManager.RemoveItem(itemId, 1))
                return false;

            ApplyConsumableEffects(itemData);
            GameEventBus.Publish(new PlayerActionFeedbackEvent($"Consumed {itemData.DisplayName}"));
            return true;
        }

        private void ApplyConsumableEffects(ItemDataSO itemData)
        {
            if (itemData.HungerRestore > 0 && _hungerManager != null)
            {
                _hungerManager.RestoreHunger(itemData.HungerRestore);
            }

            if (itemData.StaminaRestore > 0 && _staminaManager != null)
            {
                _staminaManager.AddStamina(itemData.StaminaRestore);
            }

            if (itemData.StatusEffectIds != null && itemData.StatusEffectIds.Length > 0 && _statusEffectManager != null)
            {
                foreach (var effectId in itemData.StatusEffectIds)
                {
                    if (!string.IsNullOrWhiteSpace(effectId))
                    {
                        _statusEffectManager.TryAddEffect(effectId, itemData.BuffDurationSeconds);
                    }
                }
            }
        }
    }
}
