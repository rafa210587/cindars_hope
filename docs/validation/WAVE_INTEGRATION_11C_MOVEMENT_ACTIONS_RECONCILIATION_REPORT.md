# WAVE INTEGRATION 11C — Movement Actions Validation Reconciliation Report

## Status
BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE

## Date
2026-06-09

## Summary

Documental/validator hotfix for the WAVE_INTEGRATION_11B second fix. No runtime behavior changed.

---

## Issues Reconciled

### Issue 1 — Validator string mismatch

**File:** `Assets/_Game/Scripts/Editor/Validation/ValidateWave11MovementActionsRuntime.cs`

| | Value |
|---|---|
| Before | `"PlayerMovementActionBootstrap] Resolved PlayerController"` |
| After | `"PlayerMovementActionRuntimeBootstrap] Resolved PlayerController"` |

The validator was checking for a truncated prefix `PlayerMovementActionBootstrap]` while the actual log line in `PlayerMovementActionRuntimeBootstrap.cs` emits:
```
[PlayerMovementActionRuntimeBootstrap] Resolved PlayerController '<name>' and attached movement action controllers.
```
This caused a false-negative if the validator ran against the runtime file.

### Issue 2 — CURRENT_STATE.md canonical values

CURRENT_STATE.md already had the correct values from commit `7757538` (previous reconciliation pass). No change needed:

| Field | Value |
|---|---|
| Dash distance | 4.0 units |
| Dash duration | 0.22s |
| Dodge distance | 1.8 units |
| Dodge duration | 0.32s |
| Block speed multiplier | 0.45x |
| Block stamina drain | 18/s |

---

## Root Cause (historical)

The validator string mismatch originated when the validator was written: the class was originally planned as `PlayerMovementActionBootstrap` and was later renamed to `PlayerMovementActionRuntimeBootstrap` to match Unity's `RuntimeInitializeOnLoadMethod` naming convention. The validator check was not updated at rename time.

---

## Runtime Code Status (unchanged)

All runtime code from WAVE_INTEGRATION_11B second fix remains as committed in `de68e95`:

| File | Status |
|---|---|
| `PlayerMovementActionRuntimeBootstrap.cs` | Correct — resolves `PlayerController`, validates `Rigidbody2D` |
| `PlayerMovementDisplacementResolver.cs` | Correct — `WaitForFixedUpdate`, `ShouldIgnoreHit`, `IsBeingDisplaced` |
| `PlayerMovementActionInput.cs` | Correct — `#if ENABLE_INPUT_SYSTEM` guards |
| `DirectionalDoubleTapDetector.cs` | Correct — arrow mapping fixed (index 5=down, 6=left) |
| `PlayerDashController.cs` | Correct — 4.0f / 0.22f / 40 stamina |
| `PlayerDodgeController.cs` | Correct — 1.8f / 0.32f / 40 stamina |
| `PlayerBlockController.cs` | Correct — 0.45f slow / 18f/s drain |
| `PlayerController.cs` | Correct — `IsBeingDisplaced` guard in `FixedUpdate` |

---

## Files Changed in This Pass

| File | Change |
|---|---|
| `Assets/_Game/Scripts/Editor/Validation/ValidateWave11MovementActionsRuntime.cs` | Fixed bootstrap log prefix: `PlayerMovementActionBootstrap]` → `PlayerMovementActionRuntimeBootstrap]` |

---

## Build Validation

| Target | Result | Errors | Warnings |
|---|---|---|---|
| Assembly-CSharp | PASS | 0 | 0 |
| Assembly-CSharp-Editor | PASS | 0 | 3 (pre-existing legacy) |

---

## Docs Validation

Status: `EXPECTED_FAIL_LEGACY_ONLY` — all failures are pre-existing (spec header fields in `spec_test_harness_editmode_playmode_quality_gate.md`, missing `validated_adrs`/`validated_game_rules` in old execution reports, amendment citations in two implemented specs). Zero new errors introduced by this pass.

## Quality Check

Status: `QUALITY_CHECK_SCRIPT_EXCEPTION_PREEXISTING`

```
SCRIPT_EXCEPTION: The Should command may only be used inside a Describe block.
```

This is the same pre-existing Pester infrastructure issue recorded in the rules incident note (2026-06-08, commit d053a29). The exception originates from `check_spec_quality.ps1` using Pester `Should` assertions outside a `Describe` block. It is not caused by any code or doc change in this pass. The script would throw identically before and after this hotfix.

---

## Human Play Mode

Status: PENDING — no change from WAVE_INTEGRATION_11B.

Required before ACCEPTED:
1. Open FarmScene (or any scene with `PlayerController` + `Rigidbody2D`).
2. Press Play; confirm `[PlayerMovementActionRuntimeBootstrap] Resolved PlayerController` in Console.
3. Space + WASD: dash ~4 tiles in ~0.22s.
4. Double-tap WASD: dodge ~1.8 tiles in ~0.32s.
5. Hold Left Shift: movement at ~45% speed, stamina drains 18/s.

Checklist: `docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md`
