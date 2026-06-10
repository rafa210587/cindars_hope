# WAVE_INTEGRATION_18 — Cave Enemy/Loot Save Matrix

**Date:** 2026-06-10

---

## WAVE17 Runtime Strategy

**Confirmed strategy:** `DROP_DIRECT_TO_INVENTORY`

Per `WAVE_INTEGRATION_17_EXTRACTION_PRESERVATION_MATRIX.md` and `WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md`:

```
EnemyHealth.Die()
  → EnemyKilledEvent(enemyId, dropItemId, dropAmount, position, xpReward)
  → EnemyDropSpawner.OnEnemyKilled
  → InventoryManager.AddItem(dropItemId, dropAmount)
```

No ground pickup objects are created. There is no `LootPickupInteractable` or `WorldSpacePickup` in the WAVE17 implementation.

---

## Enemy State Save Assessment

| State | Persisted? | Strategy | Notes |
|-------|-----------|----------|-------|
| Enemy spawned (alive) | NO | WAVE17_SMOKE_SPAWNER_DEBT | CaveSmokeTestSpawnerBridge creates enemies at scene start |
| Enemy alive HP | NO | CAVE_ENEMY_HP_SAVE_DEBT | HP resets to max on load (acceptable debt) |
| Enemy defeated (SetActive false) | NO | CAVE_ENEMY_HP_SAVE_DEBT | Enemy respawns after load |
| Enemy dropEmitted flag | NO | COVERED_BY_INVENTORY | Drop already applied to InventoryManager |
| Enemy position | NO | CAVE_ENEMY_HP_SAVE_DEBT | Enemy respawns at spawn offset |

---

## Loot State Save Assessment

| State | Persisted? | Strategy | Notes |
|-------|-----------|----------|-------|
| Loot collected by player | YES | INVENTORY_COVERAGE | InventoryManager.AddItem → Inventory save section |
| Loot on ground (uncollected) | N/A | NO_GROUND_PICKUPS | DROP_DIRECT_TO_INVENTORY strategy |
| Loot duplicating on load | NO | INVENTORY_IDEMPOTENT | Enemy respawns on load, but won't drop again until defeated |

---

## Classification

```
WAVE17 drop strategy:    DROP_DIRECT_TO_INVENTORY
Enemy runtime save:      CAVE_ENEMY_HP_SAVE_DEBT (enemy respawns with full HP after load)
Loot persistence:        COVERED (InventoryManager DontDestroyOnLoad + Inventory section)
No pickup objects:       NOT_APPLICABLE
```

---

## Consequences

| Scenario | Result |
|---------|--------|
| Player defeats enemy, saves, loads | Enemy revives with full HP (debt); loot already in inventory (preserved) |
| Player damages enemy (no kill), saves, loads | Enemy revives with full HP (no partial HP save) |
| Player collects loot, saves, loads | Loot still in inventory (inventory section preserved) |
| Player loads into cave fresh | Enemy spawned fresh by CaveSmokeTestSpawnerBridge |

---

## Debt Documentation

```text
CAVE_ENEMY_HP_SAVE_DEBT:
  Enemy alive HP and defeated state not persisted.
  Consequence: Enemies revive with full HP after save/load.
  Risk: LOW — WAVE17 is smoke test; CaveSmokeTestSpawnerBridge requires human wiring.
  Resolution: Future WAVE19 or Cave persistence spec.
  Idempotency: Not broken — InventoryManager already has the loot.
```

---

## Not Created (WAVE18 scope)

Per spec scope, these were NOT created:

- `EnemyRuntimeSaveData` — not needed (DROP_DIRECT_TO_INVENTORY)
- `LootPickupRuntimeSaveData` — not needed (no ground pickups)
- `CaveEnemyLootSaveBridge` — not needed (no runtime manager for smoke enemies)

Creating these would be a pre-emptive abstraction not justified by current runtime. Future spec may add if full cave procedural pipeline is wired.
