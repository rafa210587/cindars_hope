using CindarsHope.Core;
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
            if (_inventoryManager == null)
            {
                Debug.LogWarning("EnemyDropSpawner: InventoryManager is null. Drop will be skipped.", this);
                return;
            }

            _inventoryManager.AddItem(evt.DropItemId, evt.DropAmount);
            Debug.Log($"EnemyDropSpawner: Added {evt.DropAmount}x {evt.DropItemId} to inventory from enemy '{evt.EnemyId}'.", this);
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }
            else
            {
                Debug.LogWarning("EnemyDropSpawner.RebindInventoryManager received null InventoryManager.", this);
            }
        }
    }
}
