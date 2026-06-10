# WAVE_INTEGRATION_17 — Human Play Mode Checklist

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Pre-Conditions

Before running this checklist:

1. Open Unity Editor with the project
2. Open CaveScene (or any cave-accessible scene)
3. Create a GameObject "CaveSmokeSpawner" in CaveScene
4. Add `CaveSmokeTestSpawnerBridge` component
5. Assign `_smokeEnemyData` → drag `Assets/_Game/Data/Enemy/enemy_slime_basic.asset`
6. Set `_spawnOffset` to `(5, 0)` — 5 units right of bridge position
7. Set `_spawnOnStart = true`
8. Ensure `EnemyDropSpawner` exists in the scene (wired to InventoryManager or relies on GameBootstrap fallback)
9. Ensure `PlayerAttackController` is on the Player (wired via GameBootstrap)
10. Verify `enemy_slime_basic.dropItemId` is set to `item_material_stone` or `item_material_copper_ore`

---

## Checklist

| Step | Action | Expected Result | Pass/Fail | Notes |
|------|--------|-----------------|-----------|-------|
| 1 | Open Farm/Town entrance scene | Scene opens without red errors | | |
| 2 | Press Play | Player spawns, no red errors in Console | | |
| 3 | Navigate to cave entrance (WAVE16 wired) | CaveScene loads via CaveEntranceInteractable | | |
| 4 | CaveScene loads | Player appears at safe cave spawn point | | |
| 5 | Observe scene | Red placeholder enemy visible ~5 units from spawn | | |
| 6 | Move toward enemy | Enemy begins approaching player (EnemyChaseController) | | |
| 7 | Press Q (melee left hand) near enemy | Hit feedback: floating damage number appears above enemy | | |
| 8 | Press Q repeatedly | Enemy HP decreases (floating numbers accumulate) | | |
| 9 | Continue attacking until enemy HP = 0 | Enemy deactivates (SetActive false), defeat log in Console | | |
| 10 | Check Console | Log: "EnemyDropSpawner: adding drop item_material_stone x1 to inventory" | | |
| 11 | Open inventory UI | item_material_stone (or configured drop) appears in inventory | | |
| 12 | Interact with enemy again | No interaction / no duplicate drop (enemy is inactive) | | |
| 13 | Navigate to cave exit | Surface scene loads | | |
| 14 | Check inventory after exit | item_material_stone still in inventory (not cleared) | | |
| 15 | Check gold after exit | Gold unchanged from before entering cave | | |
| 16 | Check quest state after exit | Quest state unchanged from before entering cave | | |
| 17 | Re-enter cave | No duplicate manager errors in Console | | |
| 18 | Stop Play | Scene not corrupted (Unity Editor stable) | | |

---

## Alternative Start (No WAVE16 Scene Wiring)

If WAVE16 cave entrance is not yet wired in Unity Editor:

1. Create a simple test scene with: Player, EnemyDropSpawner, CaveSmokeSpawner
2. Add GameBootstrap or minimal bootstrap stub
3. Press Play → enemy spawns → test combat loop directly

---

## Blocking Issues

| Issue | Severity | Resolution |
|-------|----------|------------|
| Enemy does not spawn | BLOCKING | Verify _smokeEnemyData is assigned and _spawnOnStart = true |
| Enemy does not chase | BLOCKING | Verify GameBootstrap.Instance.PlayerManager exists at runtime |
| Q key does not attack | BLOCKING | Verify PlayerAttackController is on Player; check StaminaManager wiring |
| Hit feedback absent | WARNING | FloatingDamageNumberDisplayer may need wiring |
| Drop not added to inventory | BLOCKING | Verify EnemyDropSpawner is in scene; verify enemy_slime_basic.dropItemId is set |
| Loot duplicates | BLOCKING | Check EnemyHealth.Die() — drops once; EnemyDropSpawner subscribes once |
| Inventory not preserved on exit | BLOCKING | InventoryManager must be DontDestroyOnLoad or re-initialized correctly |

---

## Status on Completion

After executing this checklist, update:

```text
docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md
docs/project/CURRENT_STATE.md
```

Set status to one of:
- `BUILD_VALIDATED_WITH_COMBAT_LOOT_DEBT` — if loop works but save/load not persistent
- `ACCEPTED` — if all steps pass and no blocking issues remain
- `BLOCKED_BY_SCENE_WIRING` — if scene wiring is incomplete
