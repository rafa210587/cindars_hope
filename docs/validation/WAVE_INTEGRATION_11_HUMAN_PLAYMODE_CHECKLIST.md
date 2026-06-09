# WAVE_INTEGRATION_11 — Human Play Mode Checklist

**Date:** 2026-06-08
**Status:** PENDING — Not run yet

---

## Prerequisites

- FarmScene regenerated per `WAVE_INTEGRATION_11_HUMAN_UNITY_SKILL_EFFECTS_WIRING_INSTRUCTIONS.md`
- Unity validators passed (Validate Skill Effects Bridge, Validate Dash Dodge Movement)
- Player has Stamina available (> 40)
- At least one FarmPlot exists in scene in state TilledDry or PlantedDry

---

## Section A: Active Skill Slot Execution (1-4)

### A1: No skill equipped — slot feedback

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| A1.1 | Press 1 with no skill in slot 1 | HUD shows "Nenhuma habilidade equipada no slot 1" | |
| A1.2 | Press 4 with no skill in slot 4 | HUD shows "Nenhuma habilidade equipada no slot 4" | |

### A2: Farm watering skill equipped and executed

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| A2.1 | Equip a farm watering skill in slot 1 via skill tree panel | Slot 1 shows equipped skill | |
| A2.2 | Stand near a FarmPlot (TilledDry or PlantedDry) | InteractionSystem shows interaction prompt | |
| A2.3 | Press 1 | FarmPlot changes to TilledWet or PlantedWet; HUD shows "Soil watered by skill." or "Crop watered by skill." | |
| A2.4 | Press 1 again on same plot | HUD shows "Plot cannot be watered in current state." | |

### A3: Cooldown enforcement

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| A3.1 | Press 1, then immediately press 1 again | HUD shows cooldown message with remaining time | |
| A3.2 | Wait 1.5 seconds, press 1 again | Skill executes without cooldown block | |

---

## Section B: Dash (Space + Direction)

### B1: Dash basic

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| B1.1 | Hold W, press Space | Player dashes ~3.5 tiles upward; HUD shows "Dash!" | |
| B1.2 | Hold D, press Space | Player dashes ~3.5 tiles rightward | |
| B1.3 | Press Space with no directional input | No Dash; player does NOT dash | |

### B2: Dash stamina cost

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| B2.1 | With low stamina (<40), hold W + press Space | HUD shows "Stamina insuficiente para dash." | |
| B2.2 | After failed dash, stamina unchanged | Stamina bar same as before | |

### B3: Dash cooldown

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| B3.1 | Dash successfully | HUD shows "Dash!" | |
| B3.2 | Immediately try to Dash again | HUD shows "Dash em cooldown (N.Ns)." | |
| B3.3 | Wait 1.0 second | Dash available again | |

### B4: Dash obstacle

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| B4.1 | Dash toward a wall | Player stops at wall, does not clip through | |

---

## Section C: Dodge (Double-tap Direction)

### C1: Dodge basic

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| C1.1 | Double-tap W quickly (<0.25s) | Player dodges ~1.5 tiles upward; HUD shows "Dodge!" | |
| C1.2 | Double-tap A quickly | Player dodges ~1.5 tiles leftward | |
| C1.3 | Tap W slowly (>0.5s between taps) | No dodge triggered | |

### C2: Dodge stamina cost

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| C2.1 | With low stamina (<40), double-tap W | HUD shows "Stamina insuficiente para dodge." | |

### C3: Dodge cooldown

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| C3.1 | Dodge successfully | HUD shows "Dodge!" | |
| C3.2 | Immediately double-tap again | HUD shows "Dodge em cooldown (N.Ns)." | |
| C3.3 | Wait 0.6 second | Dodge available again | |

---

## Section D: No Regressions

| # | Check | Expected | Pass/Fail |
|---|-------|----------|-----------|
| D1 | Normal movement WASD | Player moves normally | |
| D2 | Interact with FarmPlot (F key) | Normal interaction still works | |
| D3 | Open skill tree panel | Panel opens; equip/unequip works | |
| D4 | Open inventory | Inventory opens; no input leak | |
| D5 | Close modals with Esc | Modals close; 1-4 keys do not fire during modal | |
| D6 | Save and reload | No crash; player position restored | |

---

---

## Section E: Action Skill Balance Checklist (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)

### E1: Skill tree — active skills visible per tree

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| E1.1 | Open skill tree (U), navigate to Melee | At least 5 active slot skills listed (offhand_cut, guarded_block, battle_dash, leap_attack, whirl_cut, + 3 novas) | |
| E1.2 | Navigate to Ranged | 5 active slot skills listed | |
| E1.3 | Navigate to Magic | At least 5 active slot skills listed (fire_spark, ice_bind, toxic_cloud, lightning_chain, elemental_ward, slowing_sigils + 2 novas) | |
| E1.4 | Navigate to Survival | At least 5 active utility skills listed (emergency_roll, last_breath + 5 novas) | |
| E1.5 | Navigate to Crafting | At least 5 active utility skills listed (field_patch, quick_repair + 4 novas) | |

### E2: Equip and execute new action skill (feedback-only)

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| E2.1 | Purchase and equip "Avanço de Aço" (Melee, Tier 2) in slot 1 | Slot 1 shows skill | |
| E2.2 | Press 1 in-game | HUD shows "Avanço de Aço ativado. (Efeito de combate pendente.)" | |
| E2.3 | Equip "Sinal de Retirada" (Survival, Tier 2) in slot 2 | Slot 2 shows skill | |
| E2.4 | Press 2 in-game | HUD shows "Sinal de Retirada ativado. (Efeito de utilidade pendente.)" | |
| E2.5 | Equip "Irrigador Portátil" (Crafting, Tier 3) in slot 3 | Slot 3 shows skill | |
| E2.6 | Press 3 in-game | HUD shows "Irrigador Portátil usado. (Efeito de farm pendente.)" | |

### E3: Verify Dash/Dodge/Block do not occupy active slot

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| E3.1 | Open skill tree — check Dash node | Dash (PlayerDashController) is NOT in any active slot 1-4 | |
| E3.2 | Press Space + direction | Dash fires via Space+dir; active slot 1-4 not consumed | |
| E3.3 | Double-tap direction | Dodge fires; active slot 1-4 not consumed | |
| E3.4 | Check if Block appears in active slot | Block NOT in slot; BLOCK_RUNTIME_DEFERRED is acceptable | |

### E4: Passive skills cannot be equipped in active slot

| # | Action | Expected | Pass/Fail |
|---|--------|----------|-----------|
| E4.1 | Try to equip "Pegada de Ferro" (Melee, Passive) in active slot | Not equippable (passive; EquippableSkill=false) | |
| E4.2 | Try to equip capstone "Ritmo de Batalha" in active slot | Not equippable (CapstonePassive) | |

---

## Result

| Status | Date | Executor |
|--------|------|---------|
| NOT RUN | 2026-06-08 | — |

---

*Checklist created: 2026-06-08 (WAVE_INTEGRATION_11)*
*Updated: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH — Section E: Action Skill Balance)*
