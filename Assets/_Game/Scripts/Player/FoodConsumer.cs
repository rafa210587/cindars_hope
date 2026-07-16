using CindarsHope.Inventory;
using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class FoodConsumer : MonoBehaviour
    {
        private static readonly string[] FoodPriority =
        {
            "item_crop_carrot",
            "item_crop_wheat",
            "item_fish_common"
        };

        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;
        [SerializeField] private KeyCode _consumeKey = KeyCode.H;

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                return;
            }

            if (Input.GetKeyDown(_consumeKey))
            {
                TryConsumeFood();
            }
        }

        private void TryConsumeFood()
        {
            if (_inventoryManager == null || _hungerManager == null)
            {
                Debug.LogWarning("FoodConsumer cannot consume because InventoryManager or HungerManager is missing.", this);
                return;
            }

            foreach (var itemId in FoodPriority)
            {
                if (!_inventoryManager.HasItem(itemId))
                {
                    continue;
                }

                if (!_inventoryManager.TryGetItemData(itemId, out var itemData))
                {
                    Debug.LogWarning($"FoodConsumer could not resolve food item '{itemId}'.", this);
                    continue;
                }

                var staminaManager = _staminaManager ?? GameBootstrap.Instance?.StaminaManager as StaminaManager;
                var statusManager = _statusEffectManager ?? GameBootstrap.Instance?.StatusEffectManager as StatusEffectManager;
                var restoresHunger = itemData.HungerRestore > 0 && _hungerManager.CurrentHunger < _hungerManager.MaxHunger;
                var restoresStamina = itemData.StaminaRestore > 0
                    && staminaManager != null
                    && staminaManager.CurrentStamina < staminaManager.MaxStamina;
                var appliesStatus = statusManager != null
                    && itemData.StatusEffectIds != null
                    && itemData.StatusEffectIds.Length > 0;
                if (!restoresHunger && !restoresStamina && !appliesStatus)
                {
                    Debug.Log($"FoodConsumer skipped '{itemId}' because no effect is currently applicable.", this);
                    continue;
                }

                if (!_inventoryManager.RemoveItem(itemId, 1))
                {
                    Debug.LogWarning($"FoodConsumer could not remove '{itemId}' from inventory.", this);
                    continue;
                }

                // fable_23 — ponto ÚNICO do FoodEffectModifier (Charm de Thandra +15% no efeito de
                // comida). Escala fome/stamina restauradas pela consulta síncrona ao roteador; sem
                // acessório => valor base (helper trata default 0 como neutro). Sem if espalhado.
                var hungerRestore = CindarsHope.Equipment.AccessoryEffectRouter.ApplyFoodEffect(itemData.HungerRestore);
                var staminaRestore = CindarsHope.Equipment.AccessoryEffectRouter.ApplyFoodEffect(itemData.StaminaRestore);

                _hungerManager.RestoreHunger(hungerRestore);

                staminaManager?.AddStamina(staminaRestore);

                if (statusManager != null && itemData.StatusEffectIds != null)
                {
                    foreach (var effectId in itemData.StatusEffectIds)
                    {
                        statusManager.TryAddEffect(effectId, itemData.BuffDurationSeconds);
                    }
                }

                Debug.Log($"Consumed '{itemId}' and applied hunger/stamina/status effects.", this);
                return;
            }

            Debug.Log("FoodConsumer found no available food.", this);
        }
    }
}
