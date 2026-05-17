using CindarsHope.Inventory;
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
        [SerializeField] private KeyCode _consumeKey = KeyCode.H;

        private void Update()
        {
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

            if (_hungerManager.CurrentHunger >= _hungerManager.MaxHunger)
            {
                Debug.Log("FoodConsumer skipped consumption because hunger is already full.", this);
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

                if (itemData.HungerRestore <= 0)
                {
                    Debug.Log($"FoodConsumer skipped '{itemId}' because it does not restore hunger.", this);
                    continue;
                }

                if (!_inventoryManager.RemoveItem(itemId, 1))
                {
                    Debug.LogWarning($"FoodConsumer could not remove '{itemId}' from inventory.", this);
                    continue;
                }

                _hungerManager.RestoreHunger(itemData.HungerRestore);
                Debug.Log($"Consumed '{itemId}' and restored {itemData.HungerRestore} hunger.", this);
                return;
            }

            Debug.Log("FoodConsumer found no available food.", this);
        }
    }
}
