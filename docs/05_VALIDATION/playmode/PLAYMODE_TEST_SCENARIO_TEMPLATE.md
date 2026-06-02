---
doc_type: human_test_scenario
spec_id: SPEC_NN
status: prepared
date: YYYY-MM-DD
executor: [to be filled by human tester]
result: NOT RUN
---

# Human Test Scenario — SPEC_NN <Title>

> **This is a human test scenario, NOT an automated test.**  
> **Human tester must follow steps and record results.**  
> **Do NOT claim Phase 3 PASS without executing this scenario.**

---

## Feature Summary

[1-2 sentence summary of what gameplay feature is being tested and why]

---

## Scenes

List the scenes where testing will occur:

- Scene 1: `Assets/_Game/Scenes/Scene1.unity`
- Scene 2: `Assets/_Game/Scenes/Scene2.unity`

Or: "Default game load (new game or existing save)"

---

## Required Initial State

What the player needs to set up before starting the test:

### Inventory

- Item A (quantity: X)
- Item B (quantity: Y)

### Character Stats

- Health: X / Y
- Mana: X / Y
- Level: X

### Game Flags

- [Flag name]: true/false (e.g., "quest_started": true)

### Equipment

- Weapon: [name]
- Armor: [name]

### Location

- Start location: [scene name or description]
- Map state: [e.g., "all chest unopened" or "boss already defeated"]

### Debug Helpers (if needed)

```
/spawn sword iron_sword 1
/set health 100
/set quest_flag my_quest true
```

---

## Scenario 1 — Happy Path

**Objective:** Test the feature working as intended.

### Steps

1. [Detailed step 1]
2. [Detailed step 2]
3. [Detailed step 3]
4. [...]

Each step should be unambiguous. Example:
- ✓ "Click the Inventory button in the hotbar" (clear)
- ✗ "Open the menu" (ambiguous — which menu?)

### Expected Result

What the player should observe:

- Visual feedback: [description]
- Audio feedback: [description or "none"]
- Game state change: [description]
- Console: [expected logs or "none"]

### Example Output

```
Player equips iron sword.
Sword appears in character's right hand.
Hotbar slot 1 shows iron sword icon with green highlight.
No console errors.
```

---

## Scenario 2 — Negative Path / Edge Cases

**Objective:** Test error conditions and boundary cases.

### Steps

1. [Test case 1: What should fail gracefully?]
   - Expected: Feature rejects action with feedback
   - Example: "Try to equip non-weapon to weapon slot"

2. [Test case 2: Boundary condition]
   - Expected: Feature handles edge case without crash
   - Example: "Unequip while combat is active"

3. [Test case 3: Invalid state]
   - Expected: Feature shows error message or silent fail
   - Example: "Try to use ability with 0 mana"

### Expected Result

- No crashes
- Clear error feedback (visual, audio, or log message)
- Game remains playable after error

---

## Scenario 3 — Save/Load (if applicable)

**Objective:** Test that the feature persists correctly across save/load.

### Steps

1. Perform Scenario 1 (complete the happy path)
2. Save game (`Ctrl+S` or via menu)
3. Load game (reload the save)
4. Verify feature state restored

### Expected Result

- Feature state persists after reload
- No state corruption
- No console errors during load

**If save is NOT in scope for this spec:**

Mark this scenario as:
```
## Scenario 3 — Save/Load

Not in scope for SPEC_NN. Skip.
```

---

## Expected Results Summary

List the key behaviors that should occur:

- Behavior 1: [description]
- Behavior 2: [description]
- No unexpected side effects
- No crashes
- No data loss

---

## Console Expectations

### Expected Logs (acceptable)

```
[INFO] Feature: Action started
[DEBUG] State changed to: X
```

Or: "None expected"

### Forbidden Errors (test fails if these appear)

```
[ERROR] NullReferenceException
[ERROR] OutOfRangeException
[EXCEPTION] Any unhandled exception
```

Or: "No errors allowed"

---

## Pass/Fail Checklist

Record test results here:

- [ ] Scenario 1 executed without crash
- [ ] Scenario 1: expected visual feedback observed
- [ ] Scenario 1: expected audio feedback (if any) observed
- [ ] Scenario 1: expected logs appear (if any)
- [ ] Scenario 2: error cases handled gracefully
- [ ] Scenario 3: save/load works (if in scope)
- [ ] No forbidden console errors
- [ ] No regressions in other features observed

### Overall Result

- [ ] **PASS** — All checks passed, feature ready for acceptance
- [ ] **FAIL** — Issues found; document below
- [ ] **PARTIAL** — Some checks passed; acceptable with notes

### Issues Found (if FAIL or PARTIAL)

```
Issue 1: [description]
  Severity: BLOCKER / HIGH / MEDIUM / LOW
  Reproduction: [steps to reproduce]
  Workaround: [if any]

Issue 2: [...]
```

---

## Notes

### Testing Environment

- Device: [PC/Mac/Linux]
- OS: [Windows 11 / macOS 13 / etc.]
- Unity Version: [2022.3 LTS / etc.]
- Build: Editor / Standalone / Other

### Tester Information

- Name: [tester name]
- Date: YYYY-MM-DD
- Time spent: X minutes

### Known Limitations or Deferred Items

- Limitation 1: [e.g., "Hotbar reordering not yet implemented (SPEC_XX)"]
- Limitation 2: [...]

### Regression Testing

Did you test these related features?

- [ ] Related Feature A: PASS / FAIL / SKIP
- [ ] Related Feature B: PASS / FAIL / SKIP
- [ ] Related Feature C: PASS / FAIL / SKIP

---

## Tester Sign-Off

```
Feature: SPEC_NN <Title>
Tested by: [name]
Date: YYYY-MM-DD
Result: PASS / FAIL / PARTIAL

Signature (or confirmation comment): [optional]
```

---

## How This Scenario Is Used

1. **Before** `/finish-spec`: Execution report references this file
2. **During Phase 3**: Human tester downloads, follows steps, records results
3. **After testing**: Results go into execution report's Phase 3 section
4. **Acceptance**: If PASS, spec can be promoted to `implementados/`; if FAIL, bug fixes loop

---

*Template version: 1.0*  
*Last updated: 2026-06-01*
