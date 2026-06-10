# WAVE_INTEGRATION_17 — Cave First Combat + Loot Extraction Loop: Decision Document

**Date:** 2026-06-10
**Status:** USE_EXISTING_RUNTIME

---

## Sources Read

| Source | Status | Relevant Finding |
|--------|--------|-----------------|
| CURRENT_STATE.md | READ | WAVE16 = CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED |
| Assets/_Game/Scripts/Combat/EnemyHealth.cs | READ | EXISTING — TakeDamage(DamageRequest), IsDead, Configure(EnemyDataSO), OnKill→EnemyKilledEvent |
| Assets/_Game/Scripts/Combat/PlayerAttackController.cs | READ | EXISTING — Q/E melee, Physics2D.OverlapCircleAll, hits EnemyHealth.TakeDamage |
| Assets/_Game/Scripts/Combat/EnemyDropSpawner.cs | READ | EXISTING — subscribes EnemyKilledEvent, calls InventoryManager.AddItem |
| Assets/_Game/Scripts/Combat/EnemyChaseController.cs | READ | EXISTING — approach + stop at detectionRadius/stopDistance |
| Assets/_Game/Scripts/Combat/EnemyContactDamage.cs | READ | EXISTING — trigger-based player contact damage |
| Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs | READ | EXISTING — spawns EnemyHealth+EnemyChaseController+EnemyContactDamage |
| Assets/_Game/Scripts/Combat/EnemyDataSO.cs | READ | EXISTING — full data schema with dropItemId, dropAmount |
| Assets/_Game/Scripts/Core/Events/EnemyEvents.cs | READ | EXISTING — EnemyDamagedEvent, EnemyKilledEvent, EnemySpawnedEvent |
| Assets/_Game/Scripts/World/ItemPickup.cs | READ | EXISTING — IInteractable loot pickup |
| Assets/_Game/Scripts/Inventory/InventoryManager.cs | READ | EXISTING — AddItem(string itemId, int amount): bool |
| Assets/_Game/Scripts/Core/Events/PlayerActionFeedbackEvent.cs | READ | EXISTING — HUD feedback channel |
| Assets/_Game/Data/Enemy/ | READ | EXISTING — enemy_slime_basic.asset, enemy_goblin_scout.asset, enemy_orc_warrior.asset |
| Assets/_Game/Data/Items/item_material_stone.asset | READ | CONFIRMED EXISTS |
| Assets/_Game/Data/Items/item_material_copper_ore.asset | READ | CONFIRMED EXISTS |

---

## WAVE16 Baseline

Status: `CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED`
Classification: **WAVE16_SAFE_WITH_DEBT**

Cave entrance bridge code is complete and compiled. Human must still wire CaveEntranceInteractable + CaveRuntimeBridge + CaveExitPortal in Unity Editor scenes.
WAVE17 can proceed because it is code-only and does not depend on WAVE16 scene wiring.

---

## System State Table

| System | Status | Strategy |
|--------|--------|----------|
| EnemyHealth | EXISTING | USE_EXISTING |
| PlayerAttackController | EXISTING | USE_EXISTING |
| EnemyDropSpawner (loot→inventory) | EXISTING | USE_EXISTING |
| EnemyChaseController | EXISTING | USE_EXISTING |
| EnemyContactDamage | EXISTING | USE_EXISTING |
| EnemyBrain (full AI) | EXISTING | NOT NEEDED for smoke test |
| CaveEnemySpawner (full cave gen) | EXISTING | NOT USED (needs CaveGeneratedLevel) |
| EnemyDataSO (enemy_slime_basic) | EXISTING | USE_EXISTING |
| EnemyKilledEvent | EXISTING | USE_EXISTING (covers "defeated") |
| EnemyDamagedEvent | EXISTING | USE_EXISTING |
| EnemySpawnedEvent | EXISTING | USE_EXISTING |
| ItemPickup (IInteractable) | EXISTING | USE_EXISTING |
| InventoryManager.AddItem | EXISTING | USE_EXISTING |
| PlayerActionFeedbackEvent | EXISTING | USE_EXISTING |
| item_material_stone asset | EXISTING | USE_EXISTING as loot item |
| item_material_copper_ore asset | EXISTING | USE_EXISTING as loot item |
| DamagePayload (spec proposal) | NOT CREATED | PARALLEL — DamageRequest already exists |
| EnemyDefinition (spec proposal) | NOT CREATED | PARALLEL — EnemyDataSO already exists |
| LootDropper (spec proposal) | NOT CREATED | PARALLEL — EnemyDropSpawner already exists |
| LootPickupInteractable (spec proposal) | NOT CREATED | PARALLEL — ItemPickup already exists |

---

## Player Attack Strategy

**Decision: USE_EXISTING_PLAYER_ATTACK**

`PlayerAttackController.cs` is fully functional:
- Q key = left hand attack (tool/weapon)
- E key = right hand attack (weapon, blocked if interaction candidate present)
- Uses `Physics2D.OverlapCircleAll` centered on player facing direction
- Finds `EnemyHealth` via `GetComponentInParent/GetComponent`
- Calls `EnemyHealth.TakeDamage(DamageRequest)`
- Has modal guard (`GameBootstrap.Instance?.ModalManager?.HasActiveModal`)
- Has stamina guard (`StaminaManager.TrySpendStamina`)
- Has unarmed fallback (`UnarmedAttackDataSO` created at runtime if null)

No bridge or bridge adapter needed.

---

## Enemy Runtime Strategy

**Decision: CREATE_MINIMAL_SMOKE_SPAWNER_BRIDGE**

`CaveEnemySpawner` requires `CaveGeneratedLevel` (from the full procedural pipeline). For a standalone smoke test in CaveScene without triggering the full generator, a thin `CaveSmokeTestSpawnerBridge` is needed.

This bridge:
- Takes `[SerializeField] EnemyDataSO _smokeEnemyData`
- On `Start()` creates a single enemy GameObject using the same component pattern as `CaveEnemySpawner.SpawnEnemyAtPoint`
- Binds `EnemyChaseController` to the player via `GameBootstrap.Instance.PlayerManager.transform`
- Publishes `EnemySpawnedEvent` for HUD/bestiary
- Is idempotent (one spawn per instance)
- Does NOT create any parallel combat system

---

## Loot Strategy

**Decision: USE_EXISTING_LOOT_LOOP**

The loot loop is already complete:
1. `EnemyHealth.Die()` → `GameEventBus.Publish(EnemyKilledEvent(enemyId, dropItemId, dropAmount, ...))` 
2. `EnemyDropSpawner.OnEnemyKilled(evt)` → `InventoryManager.AddItem(evt.DropItemId, evt.DropAmount)`
3. `InventoryManager` updates slot state, inventory UI reflects change

For smoke test: set `enemy_slime_basic.dropItemId = "item_material_stone"` (or copper_ore).

**No `LootDropper`, `LootPickupInteractable`, or `LootTable` created — all are PARALLEL to existing systems.**

---

## Design / Direction Compliance Matrix

| Requirement | Compliance | Notes |
|-------------|-----------|-------|
| No Packages/ changes | PASS | None |
| No ProjectSettings/ changes | PASS | None |
| No parallel combat system | PASS | USE_EXISTING PlayerAttackController |
| No parallel InventoryManager | PASS | USE_EXISTING |
| No parallel item database | PASS | USE_EXISTING ItemDatabase + item_material_stone |
| No gore/explicit effects | PASS | Hit flash only |
| No ACCEPTED status | PASS | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED |
| No GameEventBus.cs edit | PASS | Untouched |
| No TownScene.unity edit | PASS | Untouched |
| No FarmScene.unity edit | PASS | Untouched |
| No NPC/** edit | PASS | Untouched |
| Commits in Portuguese | PASS | Will be |
| No GameObject.Find at runtime | PASS | Uses GameBootstrap.Instance only |
| No manual YAML edit | PASS | No .unity/.prefab/.asset edited |

---

## Files Created by WAVE17

| File | Purpose |
|------|---------|
| Assets/_Game/Scripts/Cave/Runtime/CaveSmokeTestSpawnerBridge.cs | Minimal smoke spawner using existing components |
| Assets/_Game/Scripts/Cave/Runtime/CaveSmokeTestSpawnerBridge.cs.meta | Meta |
| Assets/_Game/Scripts/Editor/Validation/ValidateWave17CaveCombatLootLoop.cs | Editor validator |
| Assets/_Game/Scripts/Editor/Validation/ValidateWave17CaveCombatLootLoop.cs.meta | Meta |
| docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_DECISION.md | This file |
| docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md | Execution report |
| docs/validation/WAVE_INTEGRATION_17_CAVE_TARGET_SCENE_AUDIT.md | Scene audit |
| docs/validation/WAVE_INTEGRATION_17_ENEMY_SMOKE_ENCOUNTER.md | Enemy smoke table |
| docs/validation/WAVE_INTEGRATION_17_PLAYER_COMBAT_BRIDGE.md | Player attack audit |
| docs/validation/WAVE_INTEGRATION_17_LOOT_TABLE_SMOKE_TEST.md | Loot smoke test |
| docs/validation/WAVE_INTEGRATION_17_EXTRACTION_PRESERVATION_MATRIX.md | State preservation |
| docs/validation/WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md | 17-step checklist |
