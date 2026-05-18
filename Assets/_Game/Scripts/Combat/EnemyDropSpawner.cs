using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyDropSpawner : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            var inventoryManager = _inventoryManager;

            if (inventoryManager == null && GameBootstrap.Instance != null)
            {
                inventoryManager = GameBootstrap.Instance.InventoryManager;
            }

            if (inventoryManager == null)
            {
                Debug.LogWarning("EnemyDropSpawner: InventoryManager not found. Drop will be skipped.");
                return;
            }

            Debug.Log($"EnemyDropSpawner: adding drop {evt.DropItemId} x{evt.DropAmount} to inventory.");
            inventoryManager.AddItem(evt.DropItemId, evt.DropAmount);
            Debug.Log("EnemyDropSpawner: drop added successfully.");
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }
            else
            {
                Debug.LogWarning("EnemyDropSpawner.RebindInventoryManager received null InventoryManager.");
            }
        }
    }
}
