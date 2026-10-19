# WAVE INTEGRATION 11B — Movement Actions Second Fix — Execution Report

## Status
BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE

## Date
2026-06-09

## Summary

Second-pass fix for `WAVE_INTEGRATION_11B_dash_dodge_block_runtime_fix`. The first fix (commit `22fd772`) resolved the `FixedUpdate`/`MovePosition` race condition by adding an `IsBeingDisplaced` guard on `PlayerController`. This second pass addresses the deeper root cause: the `PlayerMovementActionRuntimeBootstrap` was attaching movement controllers to `PlayerManager.gameObject` (a pure data manager with no `Rigidbody2D`) instead of to `PlayerController.gameObject`. Additionally, the displacement coroutine was not synced to the physics step, self-collision filtering was absent, and the arrow-key direction mapping in `DirectionalDoubleTapDetector` had index 5 and 6 swapped.

## Root Cause

```
ROOT_CAUSE_ATTACHING_MOVEMENT_CONTROLLERS_TO_PLAYERMANAGER_INSTEAD_OF_PLAYERCONTROLLER
```

`PlayerManager` is a pure data manager (gold/HP/save). It has no `PlayerController` and no `Rigidbody2D`. The bootstrap was calling `bootstrap.PlayerManager.gameObject` as the attach target, so all movement action controllers were attached to the wrong object and could never move the player.

## Fixes Applied

| Fix | File | Detail |
|---|---|---|
| Bootstrap resolution | `PlayerMovementActionRuntimeBootstrap.cs` | `ResolvePlayerController()` searches via `GetComponent`, `GetComponentInChildren`, `GetComponentInParent`, `FindAnyObjectByType` fallback |
| Bootstrap validation | `PlayerMovementActionRuntimeBootstrap.cs` | Validates `Rigidbody2D` before attaching; logs error and returns if missing |
| Physics step sync | `PlayerMovementDisplacementResolver.cs` | `yield return null` → `yield return new WaitForFixedUpdate()` |
| Self-collision filter | `PlayerMovementDisplacementResolver.cs` | `ShouldIgnoreHit()` filters same collider, same rigidbody, triggers, parent/child |
| Blocked-at-origin exit | `PlayerMovementDisplacementResolver.cs` | Early break if target equals origin (sqrMagnitude < 0.0001) |
| Arrow mapping bug | `DirectionalDoubleTapDetector.cs` | Index 5 (DownArrow) was `Vector2.left`; index 6 (LeftArrow) was `Vector2.down` — corrected |
| Input helper | `PlayerMovementActionInput.cs` | New `internal static class`; `WasDashPressed`, `IsBlockHeld`, `GetMoveDirectionHeld`, `WasDirectionalKeyPressed` with `#if ENABLE_INPUT_SYSTEM` guards |
| Dash canonical values | `PlayerDashController.cs` | Distance: 4.0f (was 3.5f); Duration: 0.22f (was 0.14f) |
| Dodge canonical values | `PlayerDodgeController.cs` | Distance: 1.8f (was 1.5f); Duration: 0.32f (was 0.10f) |
| Block canonical values | `PlayerBlockController.cs` | SlowMultiplier: 0.45f (was 0.5f); StaminaDrain: 18f/s (was 10f/s) |
| Block logging | `PlayerBlockController.cs` | `Block started speedMultiplier=X` and `Block stopped speedMultiplier restored=X` |
| Validator update | `ValidateWave11MovementActionsRuntime.cs` | Checks new canonical values, input helper, arrow mapping, bootstrap resolution, `WaitForFixedUpdate`, `ShouldIgnoreHit` |

## Canonical Values (Combat Core)

| Action | Distance | Duration | Stamina | Input | Range |
|---|---|---|---|---|---|
| Dash | 4.0 tiles | 0.22s | 40 | Space + direction | 3.2–4.0 tiles / 0.18–0.30s |
| Dodge | 1.8 tiles | 0.32s | 40 | Double-tap directional | 1.2–1.8 tiles / 0.28–0.45s |
| Block | — | — | 18/s drain | Left Shift held | 35%–55% speed (0.45 multiplier) |

## Files Changed

| File | Change |
|---|---|
| `Assets/_Game/Scripts/Player/Movement/PlayerMovementActionRuntimeBootstrap.cs` | Full rewrite: `ResolvePlayerController()`, `AttachControllers(PlayerController)`, Rigidbody2D guard, `FindAnyObjectByType` fallback |
| `Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs` | `WaitForFixedUpdate`, `ShouldIgnoreHit`, blocked-at-origin exit |
| `Assets/_Game/Scripts/Player/Movement/PlayerMovementActionInput.cs` | NEW — input abstraction helper |
| `Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs` | Arrow mapping fix + use `PlayerMovementActionInput` indirectly via switch |
| `Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs` | Values + `PlayerMovementActionInput` usage |
| `Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs` | Values update |
| `Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs` | Values + `PlayerMovementActionInput.IsBlockHeld()` + logs |
| `Assets/_Game/Scripts/Editor/Validation/ValidateWave11MovementActionsRuntime.cs` | Updated validation checks |
| `Assets/_Game/Scripts/Player/PlayerController.cs` | `IsBeingDisplaced` flag (first fix, retained) |
| `docs/validation/WAVE_INTEGRATION_11B_MOVEMENT_ACTIONS_SECOND_FIX_AUDIT.md` | Root cause audit document |

## Build Validation

| Target | Result | Errors | Warnings |
|---|---|---|---|
| Assembly-CSharp | PASS | 0 | 0 |
| Assembly-CSharp-Editor | PASS | 0 | 3 (pre-existing legacy) |

Validation method: `dotnet build` with explicit `$LASTEXITCODE` check.

## Docs Validation

Status: `EXPECTED_FAIL_LEGACY_ONLY` — all failures pre-date this change (spec header fields in `spec_test_harness_editmode_playmode_quality_gate.md`, missing `validated_adrs`/`validated_game_rules` in old execution reports, amendment citations in implemented specs). Zero new errors introduced.

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (input mapping, physics sync, bootstrap resolution)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO
Automated tests command: NOT RUN
Manual Play Mode scenario: docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md (existing; covers Dash/Dodge/Block scenarios)
Justification if no automated tests: Bootstrap resolution and physics coroutine behavior require Unity Play Mode; movement controllers are attached at runtime via RuntimeInitializeOnLoadMethod and cannot be tested in EditMode without a real Rigidbody2D scene context.
Residual risk: Human Play Mode required to verify player actually moves with Dash/Dodge/Block; double-tap window and self-collision behaviour not EditMode-testable.
```

## Human Play Mode Requirements

Before marking WAVE_INTEGRATION_11B as ACCEPTED:

1. Open FarmScene or scene with PlayerController having `Rigidbody2D`.
2. Press Play.
3. Confirm `[PlayerMovementActionRuntimeBootstrap] Resolved PlayerController '<name>'` in Console.
4. Press Space + WASD: player should dash 4 tiles in ~0.22s.
5. Double-tap WASD quickly: player should dodge ~1.8 tiles in ~0.32s.
6. Hold Left Shift: player moves at ~45% speed; stamina drains at 18/s.
7. Release Left Shift: normal speed restored.
8. Confirm no stuck movement, no teleporting, no ghost displacement.

## Decision

- Can start next wave: YES (WAVE_INTEGRATION_12 already complete)
- Human Play Mode validation required: YES (all movement actions need in-scene verification)
- Assembly-CSharp: PASS
- Assembly-CSharp-Editor: PASS
