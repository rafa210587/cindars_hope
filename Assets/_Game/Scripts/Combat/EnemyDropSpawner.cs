using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Loot;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyDropSpawner : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;

        // fable_06: banco de loot tables (resolve EnemyDataSO.lootTableId → LootTableSO).
        // Opcional: quando ausente OU a tabela não resolve, o spawner cai no caminho LEGADO
        // (DropItemId × DropAmount fixos), preservando 100% o comportamento anterior.
        [SerializeField] private LootTableDatabaseSO _lootTableDatabase;

        private static readonly List<string> ResolverWarnings = new List<string>();

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

            // fable_06: caminho por TABELA (determinístico, ADR-0005) quando há lootTableId + banco.
            if (TryResolveTable(evt.LootTableId, out var table))
            {
                RollAndGrantTable(table, evt, inventoryManager);
                return;
            }

            // Caminho LEGADO: dropItemId × dropAmount fixos.
            GrantLegacyDrop(evt, inventoryManager);
        }

        private bool TryResolveTable(string lootTableId, out LootTableSO table)
        {
            table = null;
            if (string.IsNullOrWhiteSpace(lootTableId) || _lootTableDatabase == null)
            {
                return false;
            }

            if (!_lootTableDatabase.TryGetById(lootTableId, out table) || table == null)
            {
                Debug.LogWarning($"EnemyDropSpawner: lootTableId '{lootTableId}' not found in '{_lootTableDatabase.name}'. Falling back to legacy drop.");
                table = null;
                return false;
            }

            return true;
        }

        private void RollAndGrantTable(LootTableSO table, EnemyKilledEvent evt, InventoryManager inventoryManager)
        {
            ResolverWarnings.Clear();
            var drops = EnemyLootResolver.Roll(
                table,
                evt.LootSeed,
                evt.IsElite,
                evt.IsMinibossOrBoss,
                ResolverWarnings);

            foreach (var warning in ResolverWarnings)
            {
                Debug.LogWarning($"EnemyDropSpawner: {warning}");
            }

            if (drops.Count == 0)
            {
                Debug.Log($"EnemyDropSpawner: table '{table.TableId}' rolled empty for enemy {evt.EnemyId} (seed {evt.LootSeed}).");
                return;
            }

            foreach (var drop in drops)
            {
                Debug.Log($"EnemyDropSpawner: table drop {drop.ItemId} x{drop.Amount} (enemy {evt.EnemyId}, seed {evt.LootSeed}).");
                if (!inventoryManager.AddItem(drop.ItemId, drop.Amount))
                {
                    Debug.LogWarning($"EnemyDropSpawner: inventory rejected table drop {drop.ItemId} x{drop.Amount}.");
                }
            }
        }

        private void GrantLegacyDrop(EnemyKilledEvent evt, InventoryManager inventoryManager)
        {
            if (string.IsNullOrWhiteSpace(evt.DropItemId) || evt.DropAmount <= 0)
            {
                Debug.Log($"EnemyDropSpawner: enemy {evt.EnemyId} has no valid drop configured.");
                return;
            }

            Debug.Log($"EnemyDropSpawner: adding legacy drop {evt.DropItemId} x{evt.DropAmount} to inventory.");
            if (inventoryManager.AddItem(evt.DropItemId, evt.DropAmount))
            {
                Debug.Log("EnemyDropSpawner: drop added successfully.");
            }
            else
            {
                Debug.LogWarning($"EnemyDropSpawner: inventory rejected drop {evt.DropItemId} x{evt.DropAmount}.");
            }
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

        // fable_06: permite ao bootstrap/cena injetar o banco de loot sem FindObjectOfType.
        public void RebindLootTableDatabase(LootTableDatabaseSO lootTableDatabase)
        {
            if (lootTableDatabase != null)
            {
                _lootTableDatabase = lootTableDatabase;
            }
            else
            {
                Debug.LogWarning("EnemyDropSpawner.RebindLootTableDatabase received null LootTableDatabaseSO.");
            }
        }
    }
}
