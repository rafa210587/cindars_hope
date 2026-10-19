# WAVE_INTEGRATION_17 — Loot Table Smoke Test

**Date:** 2026-06-10

---

## Strategy: USE_EXISTING_LOOT_LOOP

The loot loop is fully implemented via `EnemyDropSpawner` + `EnemyKilledEvent` + `InventoryManager.AddItem`.

---

## Loot Table

| LootTableId | ItemId | Item Asset Exists | Amount | DropChance | Temporary | Inventory Add Verified |
|------------|--------|------------------|--------|------------|-----------|----------------------|
| enemy_slime_basic (dropItemId field) | item_material_stone | YES | 1 | 100% (always drops) | NO — real item | STATIC_AUDIT_PASS |
| enemy_slime_basic (alt) | item_material_copper_ore | YES | 1 | 100% | NO — real item | STATIC_AUDIT_PASS |

---

## Loot Pipeline (REUSE_EXISTING)

```
EnemyHealth.Die()
  → GameEventBus.Publish(new EnemyKilledEvent(
        enemyId: "enemy_slime_basic",
        dropItemId: "item_material_stone",
        dropAmount: 1,
        deathPosition: transform.position,
        xpReward: N))
  
  [EnemyDropSpawner subscribes EnemyKilledEvent]
  → EnemyDropSpawner.OnEnemyKilled(evt)
  → if (string.IsNullOrWhiteSpace(evt.DropItemId) || evt.DropAmount <= 0) → skip
  → InventoryManager.AddItem("item_material_stone", 1)
  → Returns bool: true if added, false if inventory full
```

---

## InventoryManager.AddItem API

```csharp
// Signature (from InventoryManager.cs):
public bool AddItem(string itemId, int amount)
```

- Returns `true` = added successfully
- Returns `false` = inventory full or item not in database
- If `false`: `EnemyDropSpawner` logs a warning but does NOT mark as collected
  → On current loop, the drop is silently skipped (loot lost if inventory full)
  → This is pre-existing behavior; WAVE17 does not change it

---

## No LootDropper/LootTable/LootPickupInteractable Created

The spec proposed `LootDropper.cs`, `LootTable.cs`, `LootPickupInteractable.cs`. These were **not created** because `EnemyDropSpawner` + `ItemPickup` already implement this functionality. Creating them would be a PARALLEL system violation.

The existing `EnemyDropSpawner` adds items directly to inventory on enemy death — no physical pickup object is spawned. For WAVE17 smoke test this is sufficient.

If a future spec requires physical loot objects (WorldSpacePickup), it should extend `ItemPickup` or `EnemyDropSpawner`, not create a new parallel system.

---

## Preservation After Cave Exit

The inventory persists across scene transitions because `InventoryManager` is on a `DontDestroyOnLoad` bootstrap object (confirmed from WAVE16 state preservation analysis). Loot collected in CaveScene will be present after returning to FarmScene.
