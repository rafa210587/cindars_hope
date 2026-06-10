# WAVE_INTEGRATION_16 — Human Play Mode Checklist

## Status

NOT RUN — requires human Unity action to wire scene objects first.
See: `docs/validation/WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md`

---

## Pre-requisites

Before running this checklist:
- [ ] Complete all steps in `WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md`
- [ ] Confirm Assembly-CSharp builds with 0 errors
- [ ] Open FarmScene in Unity Editor

---

## Checklist (17 steps)

### Baseline verification

1. **Open FarmScene and press Play**
   - Player spawns at default farm spawn
   - No errors in Console

2. **Verify baseline state**
   - Open inventory (I key) — check items present
   - Note gold amount (DebugHud)
   - Check active quest (J key) if quest_first_supplies_for_cindar was accepted
   - Note cave run status (no active run)

### Cave entry

3. **Walk toward Zone_CaveEntrance**
   - Interaction prompt "Entrar na caverna" appears when near Zone_CaveEntrance
   - (Requires CaveEntranceInteractable component wired in Zone_CaveEntrance)

4. **Press Interact (E key)**
   - Console logs: "[CaveEntranceInteractable] Player entering cave..."
   - Console logs: "[SceneTransitionRouter] Transitioning FarmScene → CaveScene @spawn_cave_from_farm"
   - CaveScene loads

### Cave scene validation

5. **Player spawns in CaveScene**
   - Player appears at or near spawn_cave_from_farm position
   - No teleport to world origin (0,0,0)
   - No null reference errors in Console

6. **Cave level is generated**
   - Tilemap tiles appear (walls, floor)
   - Console logs: "CaveLevelRuntimeController: Cave level generated."
   - Rooms visible (min 8 rooms at 160x96 config)

7. **Camera follows player in cave**
   - Move player (WASD/arrow keys)
   - Camera follows or is repositioned near player
   - No camera stuck at world origin

8. **Movement in cave works**
   - Player moves in all 4 directions
   - No clipping through walls (collision tiles working)
   - CavePlayerPathConfinement active (player stays in walkable area)

9. **Dash/Dodge/Block smoke test (WAVE11)**
   - Space+direction = Dash fires (stamina cost 40)
   - Double-tap direction = Dodge fires
   - Hold Left Shift = Block slows movement

10. **Resource nodes present (if any)**
    - Mining nodes visible in cave
    - Interact with a node → Console logs depletion

11. **Interact with cave exit / BackExit portal (level 1)**
    - CaveExitPortal present (human must have wired it in CaveScene)
    - Interaction prompt "Voltar / Sair" or "Sair da caverna"
    - Press E → transitions to FarmScene

### Return to surface

12. **FarmScene loads after cave exit**
    - FarmScene loads without errors
    - Player spawns at spawn_farm_from_cave position (near cave entrance zone)
    - Not at world origin (0,0,0)

13. **State preserved after cave visit**
    - Open inventory (I key): items same as baseline
    - Gold: same as baseline (no gold gained/lost)
    - Active quest: same state as baseline
    - Day/time: progressed normally if time passed

14. **Console: CaveExitedEvent**
    - Console logs: "[CaveRuntimeBridge] Player exiting cave to surface."
    - Log shows RunSeed, Level, return scene

### Stability test

15. **Repeat enter/exit cycle (no duplicate managers)**
    - Walk back to Zone_CaveEntrance → Enter cave again
    - Cave loads at correct level (1 unless saved state)
    - Only ONE CaveRuntimeBridge in hierarchy (DontDestroyOnLoad singleton)
    - Only ONE QuestRuntimeBootstrap (from WAVE15)
    - No duplicate GameBootstrap instances

16. **CaveRuntimeBridge singleton check**
    - During cave/farm cycle: check Hierarchy → DontDestroyOnLoad section
    - Only one CaveRuntimeBridge object visible
    - No accumulation across transitions

17. **Stop Play Mode**
    - Press Stop in Unity
    - No errors on exit
    - Scene state not corrupted (FarmScene or CaveScene reopen cleanly)

---

## Expected Console Messages (Key)

```
[CaveEntranceInteractable] Player entering cave. Gate: gate_farm_cave_entrance | FarmScene → CaveScene @spawn_cave_from_farm
[SceneTransitionRouter] Transitioning FarmScene → CaveScene @spawn_cave_from_farm (gate: gate_farm_cave_entrance)
CaveRunManager: restored state from bootstrap cache. RunSeed=... Level=...
 OR
CaveRunManager: (no log = first entry, InitializeIfNeeded creates new seed)
CaveLevelRuntimeController: Cave level generated. Level=1 SpawnAnchor=Entrance ...
[CaveRuntimeBridge] Cave level 1 entered. RunSeed=... Cave runtime validated.
CaveRunManager: saved state to bootstrap cache before leaving CaveScene. RunSeed=...
[CaveRuntimeBridge] Player exiting cave to surface. RunSeed=... Level=1 → FarmScene @spawn_farm_from_cave
```

---

## Pass Criteria

All 17 steps must pass for PLAYMODE_VALIDATED status.
If any step fails, document in a WAVE16 postmortem and mark PARTIAL with specific failures.

---

*Created: 2026-06-10 (WAVE_INTEGRATION_16)*
