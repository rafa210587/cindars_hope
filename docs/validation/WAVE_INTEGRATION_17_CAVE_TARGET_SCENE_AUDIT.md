# WAVE_INTEGRATION_17 — Cave Target Scene Audit

**Date:** 2026-06-10

---

## Scene Inventory

| Scene | Exists | Has Spawn | Has Exit | Has Walkable | Has Collision | Has Camera | Decision |
|-------|--------|-----------|----------|--------------|---------------|------------|----------|
| CaveScene | LIKELY (per WAVE16 audit) | YES (CaveSpawnAnchor) | YES (CaveExitPortal) | UNKNOWN (procedural) | UNKNOWN | UNKNOWN | TARGET_SMOKE |
| FarmScene | YES | YES (player spawn) | N/A | YES | YES | YES | ENTRY_POINT |
| TownScene | YES | YES | N/A | YES | YES | YES | NOT_CAVE |

Note: CaveScene was audited in WAVE16. The full scene structure depends on human wiring in Unity Editor. WAVE17 does not modify any scene YAML.

---

## Human Wiring Required for Smoke Test

To enable the cave combat smoke test in Play Mode:

1. Open CaveScene (or create a test scene that simulates cave)
2. Create empty GameObject "CaveSmokeSpawner" → add `CaveSmokeTestSpawnerBridge` component
3. Assign `_smokeEnemyData` → drag `enemy_slime_basic.asset` (or `enemy_goblin_scout.asset`)
4. Set `_spawnOffset` → e.g. (5, 0) — 5 units right of the bridge's position
5. Set `_spawnOnStart = true`
6. Position the bridge near where the player spawns
7. Ensure an `EnemyDropSpawner` GameObject exists in the scene, wired to the scene's `InventoryManager`
8. Ensure `PlayerAttackController` is on the Player GameObject (wired via Bootstrap or inspector)
9. Press Play → enemy spawns → player attacks (Q key) → enemy dies → loot added to inventory

---

## Smoke Enemy Data

Available `EnemyDataSO` assets confirmed:
- `Assets/_Game/Data/Enemy/enemy_slime_basic.asset` — recommended for smoke test
- `Assets/_Game/Data/Enemy/enemy_goblin_scout.asset` — alternative
- `Assets/_Game/Data/Enemy/enemy_orc_warrior.asset` — alternative

Recommend verifying `enemy_slime_basic.dropItemId` is set to `item_material_stone` or `item_material_copper_ore` before running smoke test.
