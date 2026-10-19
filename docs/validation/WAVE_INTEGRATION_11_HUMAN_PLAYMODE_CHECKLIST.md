# WAVE_INTEGRATION_11 - Human Play Mode Checklist

**Date:** 2026-06-09
**Status:** PENDING_HUMAN_PLAYMODE_AFTER_MOVEMENT_RUNTIME_FIX - Not run yet

---

## Prerequisites

- FarmScene open in Play Mode. Scene regeneration is not required for Dash/Dodge/Block runtime input fix; controllers bind to Player at runtime.
- Unity validators passed: Validate Skill Effects Bridge, Validate Dash Dodge Movement, Validate WAVE11 Runtime Input Binding, Validate WAVE11 Movement Actions Runtime.
- Player has Stamina available (> 40).
- At least one FarmPlot exists in scene in state TilledDry or PlantedDry.

---

## Section A: Active Skill Slot Execution (1-4)

### A1: No skill equipped - slot feedback

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| A1.1 | Press `1` with no skill in slot 1 | HUD shows empty-slot feedback for slot 1 | |
| A1.2 | Press `4` with no skill in slot 4 | HUD shows empty-slot feedback for slot 4 | |

### A2: Farm watering skill equipped and executed

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| A2.1 | Equip a farm watering skill in slot 1 via skill tree panel (`R` while panel is open) | Slot 1 shows equipped skill; gameplay use key remains `1` | |
| A2.2 | Stand near a FarmPlot (TilledDry or PlantedDry) | InteractionSystem shows interaction prompt | |
| A2.3 | Press `1` | FarmPlot changes to TilledWet or PlantedWet; HUD shows watering feedback | |
| A2.4 | Press `1` again on same plot | HUD shows plot cannot be watered in current state | |

### A3: Cooldown enforcement

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| A3.1 | Press `1`, then immediately press `1` again | HUD shows cooldown message with remaining time | |
| A3.2 | Wait 1.5 seconds, press `1` again | Skill executes without cooldown block | |

---

## Section B: Dash (Space + Direction)

### B1: Dash basic

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| B1.1 | Hold `W`, press `Space` | Player dashes about 3.5 tiles upward; HUD shows "Dash!" | |
| B1.2 | Hold `D`, press `Space` | Player dashes about 3.5 tiles rightward | |
| B1.3 | Press `Space` with no directional input and no facing direction | No Dash; HUD shows no-direction feedback | |
| B1.4 | Press `Space` with no current movement but a valid facing direction | Dash uses `LastFacingDirection` | |

### B2: Dash stamina cost

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| B2.1 | With low stamina (<40), hold `W` + press `Space` | HUD shows "Stamina insuficiente para dash." | |
| B2.2 | After failed dash, stamina unchanged | Stamina bar same as before | |

### B3: Dash cooldown

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| B3.1 | Dash successfully | HUD shows "Dash!" | |
| B3.2 | Immediately try to Dash again | HUD shows "Dash em cooldown (N.Ns)." | |
| B3.3 | Wait 1.0 second | Dash available again | |

### B4: Dash obstacle/bounds

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| B4.1 | Dash toward a wall | Player stops at wall, does not clip through | |
| B4.2 | Dash toward scene edge/bounds collider | Player stops before collider; if no boundary collider exists, record scene-collider debt | |

---

## Section C: Dodge (Double-tap Direction)

### C1: Dodge basic

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| C1.1 | Double-tap `W` quickly (<0.25s) | Player dodges about 1.5 tiles upward; HUD shows "Dodge!" | |
| C1.2 | Double-tap `A` quickly | Player dodges about 1.5 tiles leftward | |
| C1.3 | Tap `W` slowly (>0.5s between taps) | No dodge triggered | |

### C2: Dodge stamina cost

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| C2.1 | With low stamina (<40), double-tap `W` | HUD shows "Stamina insuficiente para dodge." | |

### C3: Dodge cooldown

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| C3.1 | Dodge successfully | HUD shows "Dodge!" | |
| C3.2 | Immediately double-tap again | HUD shows "Dodge em cooldown (N.Ns)." | |
| C3.3 | Wait 0.6 second | Dodge available again | |

### C4: Dodge obstacle/bounds

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| C4.1 | Dodge toward a wall | Player stops at wall, does not clip through | |
| C4.2 | Dodge toward scene edge/bounds collider | Player stops before collider; if no boundary collider exists, record scene-collider debt | |

---

## Section C2: Block (Left Shift)

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| C2.1 | Hold `Left Shift` while walking | Player movement speed is visibly reduced, not stopped | |
| C2.2 | Release `Left Shift` | Player movement speed returns to normal | |
| C2.3 | Hold `Left Shift` with stamina available | HUD shows "Block." on entry and stamina drains over time | |
| C2.4 | Keep holding until stamina is insufficient | Block exits and HUD shows stamina feedback | |
| C2.5 | Open any modal, then hold `Left Shift` | Block does not enter while modal is open | |

---

## Section D: No Regressions

| # | Check | Expected | Pass/Fail |
|---|---|---|---|
| D1 | Normal movement WASD | Player moves normally | |
| D2 | Interact with FarmPlot (`F`) | Normal interaction still works | |
| D3 | Open skill tree (`U`) | Panel opens; equip/unequip works | |
| D4 | Open inventory | Inventory opens; no input leak | |
| D5 | Close modals with `Esc` | Modals close; `1-4` keys do not fire during modal | |
| D6 | Save and reload | No crash; player position restored | |

---

## Section E: Action Skill Balance Checklist

### E1: Skill tree - active skills visible per tree

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| E1.1 | Open skill tree (`U`), navigate to Melee | Old Melee active skills plus 3 new WAVE11 action nodes are listed | |
| E1.2 | Navigate to Ranged | 5 active slot skills listed | |
| E1.3 | Navigate to Magic | Old Magic active skills plus 2 new WAVE11 action nodes are listed | |
| E1.4 | Navigate to Survival | Old Survival active skills plus 5 new WAVE11 action nodes are listed | |
| E1.5 | Navigate to Crafting | Old Crafting active skills plus 4 new WAVE11 action nodes are listed | |

### E2: Equip and execute new action skill (feedback-only)

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| E2.1 | Purchase and equip "Avanco de Aco" (Melee, Tier 2) in slot 1 | Slot 1 shows skill | |
| E2.2 | Press `1` in-game | HUD shows feedback-only/deferred combat effect message | |
| E2.3 | Equip "Sinal de Retirada" (Survival, Tier 2) in slot 2 | Slot 2 shows skill | |
| E2.4 | Press `2` in-game | HUD shows feedback-only/deferred utility effect message | |
| E2.5 | Equip "Irrigador Portatil" (Crafting, Tier 3) in slot 3 | Slot 3 shows skill | |
| E2.6 | Press `3` in-game | HUD shows feedback-only/deferred farm effect message | |

### E3: Verify Dash/Dodge/Block do not occupy active slot

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| E3.1 | Open skill tree and check Dash node | Dash runtime action is not consumed by active slots 1-4 | |
| E3.2 | Press `Space + direction` | Dash fires via movement runtime; active slot 1-4 not consumed | |
| E3.3 | Double-tap direction | Dodge fires via movement runtime; active slot 1-4 not consumed | |
| E3.4 | Hold `Left Shift` | Block fires via movement runtime slow; active slot 1-4 not consumed | |

### E4: Passive skills cannot be equipped in active slot

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| E4.1 | Try to equip "Pegada de Ferro" (Melee, Passive) in active slot | Not equippable | |
| E4.2 | Try to equip capstone "Ritmo de Batalha" in active slot | Not equippable | |

### E5: Runtime input binding fix

| # | Action | Expected | Pass/Fail |
|---|---|---|---|
| E5.1 | Start Play Mode from current FarmScene without regenerating scene | Player receives PlayerDashController, PlayerDodgeController, DirectionalDoubleTapDetector, PlayerBlockController and PlayerMovementDisplacementResolver at runtime | |
| E5.2 | Execute menu `CindarsHope/Validate/Validate WAVE11 Runtime Input Binding` | Validator passes | |
| E5.3 | Equip a new balance-patch action skill | New node is visible in its actual tree list | |

---

## Result

| Status | Date | Executor |
|---|---|---|
| NOT RUN | 2026-06-09 | - |

---

*Checklist created: 2026-06-08 (WAVE_INTEGRATION_11)*
*Updated: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH - Section E: Action Skill Balance)*
*Updated: 2026-06-09 (WAVE_INTEGRATION_11_RUNTIME_INPUT_FIX_PENDING_HUMAN_PLAYMODE)*
*Updated: 2026-06-09 (WAVE_INTEGRATION_11_MOVEMENT_ACTIONS_RUNTIME_FIX_PENDING_HUMAN_PLAYMODE)*
