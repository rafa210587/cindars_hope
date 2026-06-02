---
name: gameplay-test-scenario
description: Generate human test scenario for gameplay/runtime implementation. Required for Phase 3 closeout.
version: 1.0
when_to_use: After implementing any spec that changes gameplay, UI, combat, save, cave, farm, shop, equipment, skill tree, or asset behavior
---

# Gameplay Test Scenario Skill

## Use When

Spec implementation touches:
- Game behavior (combat, movement, animation)
- UI state (modals, hotbar, inventory, equipment)
- Save/load mechanics
- Cave procedural generation or runtime
- Farm/shop economy
- Event publishing or gameplay communication
- ScriptableObject-based data that affects gameplay

## Do NOT Use When

- Spec is docs-only (no gameplay changes)
- Spec is pure code refactor (no behavior change)
- Spec is pure asset wiring (no gameplay behavior change) — maybe; unclear

## Procedure

After implementation complete, before `/finish-spec`:

### 1. Analyze What Changed

From execution report:
- What gameplay files were modified?
- What was the objective of the change?
- What new behavior should exist?
- What existing behavior might have regressed?

### 2. Create Human Test Scenario

Write file: `docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md`

Use template: `docs/05_VALIDATION/playmode/PLAYMODE_TEST_SCENARIO_TEMPLATE.md`

### 3. Document Each Section

**Feature Summary:** 1-2 sentences of what the human should test

**Scenes:** Which scenes to load / create for testing (e.g., "Town with player at shop")

**Required Initial State:** What save game state or setup is needed
- Inventory items
- Character stats
- Game progression flags
- Map state

**Scenario 1 — Happy Path:** The main feature working correctly
- Step-by-step instructions
- Expected visual/audio result
- Expected logs (none/some/specific)

**Scenario 2 — Negative/Edge Path:** Error cases, boundary conditions
- What could go wrong?
- How to trigger the error?
- Expected behavior (graceful fail? warning log?)

**Scenario 3 — Save/Load:** If spec touches save/persistence
- Perform action in Scenario 1
- Save game
- Load game
- Verify state persisted correctly

**Expected Results:** Summary of what should happen
- Game does not crash
- No unexpected errors
- Correct visual/audio feedback
- Correct state changes
- Correct save persistence

**Console Expectations:** What should (and should NOT) appear in logs
- Expected warnings: none / list them
- Expected errors: none / list them
- Forbidden errors: any console ERROR is a fail

**Pass/Fail Checklist:**
- [ ] Feature executes without crash
- [ ] Expected logs appear
- [ ] Forbidden logs do not appear
- [ ] Visual feedback correct
- [ ] Save/load (if applicable) works
- [ ] No regressions in other features

**Notes:** Anything special
- Known limitations
- Skip Scenario 3 if save not tested
- Tested on: Mac / Windows / both
- Requires debug mode or specific settings

### 4. Reference in Execution Report

In `docs/validation/<spec_id>_execution_report.md`:

Add section:
```
## How to Test

Human test scenario: docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md

Expected test time: X minutes
Tester: [human] (not automated)
Status: [NOT RUN until Phase 3]
```

### 5. Link from `/finish-spec` Output

When `/finish-spec` processes a runtime spec:
- Check for test scenario file
- If found: reference it in Output under "Expected Gameplay Behavior"
- If not found: error (if runtime changed) or warning (if unclear)

## Rules

- Do NOT skip for "simple" changes — humans are the final judge
- Test scenario is written IN ENGLISH or PORTUGUESE depending on spec language
- Each scenario should take 1-5 minutes to execute
- Be specific: "click inventory" not "explore menu"
- Include debug helpers needed (cheat codes, spawn items, etc.)
- Console errors block Phase 3 PASS

## Validation

After human tests:
- Fill in Pass/Fail Checklist
- Record date and tester name
- If any FAIL: loop back to debugging
- If all PASS: ready for Phase 3 signoff

## Stop Conditions

- Cannot identify what to test (spec objective unclear)
- Test would require modifying spec scope (e.g., "build 50 items to test")
- Test scenario would take >30 minutes (break into smaller steps)

## Output Example

```markdown
# Human Test Scenario — SPEC_18 Hotbar Management

## Feature Summary
Test the hotbar UI allows equipping/unequipping weapons and using hotkey switches.

## Scenes
- Town (default spawn location)

## Required Initial State
- Fresh save game
- Player has: wooden sword, iron sword, staff, healing potion
- Hotbar empty

## Scenario 1 — Happy Path
1. Open hotbar UI (press H)
2. Drag wooden sword to slot 1
3. Press 1 — verify wooden sword is held
4. Drag iron sword to slot 2
5. Press 2 — verify iron sword is held
6. Switch back to slot 1 — verify wooden sword returned
7. Close hotbar (press H again or click X)
8. Verify equipped weapon visible in hand

Expected logs: none

## Scenario 2 — Negative Path
1. Open hotbar
2. Try to drag non-weapon item (healing potion) to weapon slot
3. Verify drag is rejected (returns to original position or shows "invalid" feedback)
4. Try to equip slot 3 when empty
5. Verify hotkey 3 has no effect (or shows "empty" UI feedback)

Expected logs: none (or optional "slot empty" debug log)

## Scenario 3 — Save/Load
1. Perform Scenario 1 (equip sword 1 in slot 1)
2. Save game
3. Load game
4. Verify hotbar state restored (sword 1 still in slot 1)
5. Verify equipped weapon is still in hand

Expected logs: none

## Expected Results
- Hotbar UI functions without crashes
- Equipping/unequipping weapons works
- Hotkey switches equipment instantly
- Invalid equipment rejected gracefully
- Save/load preserves hotbar state

## Console Expectations
- Errors: NONE
- Warnings: none (optional "slot empty" is acceptable)
- Forbidden: Any ERROR or Exception

## Pass/Fail Checklist
- [x] Feature executes without crash
- [x] Expected behavior observed
- [x] Forbidden logs absent
- [x] Save/load works
- [ ] OTHER (specify)

**Overall:** PASS / FAIL

## Notes
- Tested on: Windows
- Tested by: human QA
- Date: 2026-06-01
- Known limitation: hotbar UI does not yet support drag-reorder (future SPEC)
```

---

## Key Difference from Automated Tests

Automated tests check: "does it compile and not crash?"
Human test scenario checks: "does it feel right and work as intended?"

Both are needed. This skill covers the latter.
