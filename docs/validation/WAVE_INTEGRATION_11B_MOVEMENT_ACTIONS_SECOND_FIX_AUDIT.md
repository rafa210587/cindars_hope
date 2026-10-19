# WAVE_INTEGRATION_11B — Movement Actions Second Fix Audit

**Date:** 2026-06-09  
**Status:** ROOT_CAUSE_CONFIRMED

---

## Root Cause

```
ROOT_CAUSE_ATTACHING_MOVEMENT_CONTROLLERS_TO_PLAYERMANAGER_INSTEAD_OF_PLAYERCONTROLLER
```

### Evidence

`PlayerMovementActionRuntimeBootstrap.AttachControllers` receives `GameBootstrap.Instance.PlayerManager.gameObject`.

`PlayerManager` is a pure data/state manager:
- No `PlayerController` component
- No `Rigidbody2D` component
- No `Collider2D` component
- No physics

All movement controllers call `GetComponent<PlayerController>()` on `this` (same GameObject):
- `PlayerDashController.Start()` → `GetComponent<PlayerController>()` → returns null
- `PlayerDodgeController.Start()` → `GetComponent<PlayerMovementDisplacementResolver>()` → null
- `PlayerBlockController.Start()` → `GetComponent<PlayerController>()` → returns null
- `PlayerMovementDisplacementResolver.Awake()` → `GetComponent<Rigidbody2D>()` → null

**Result:** All movement controllers find null references. Dash/Dodge do not call `TryDisplace` (resolver is null). Block cannot apply `SpeedMultiplier` (PlayerController is null).

The first fix (WAVE_INTEGRATION_11B `IsBeingDisplaced`) addressed the physics race condition correctly, but the bootstrap still attaches to the wrong GameObject. The race condition fix was correct but never actually exercised because Dash/Dodge never reached `TryDisplace`.

---

## Secondary Bug: DirectionalDoubleTapDetector Arrow Key Mapping

Current `_directionVectors` array index 5 and 6 are swapped:

| Index | Key | Current Vector | Correct Vector |
|-------|-----|---------------|----------------|
| 4 | UpArrow | up | up ✓ |
| 5 | DownArrow | **left** ✗ | down |
| 6 | LeftArrow | **down** ✗ | left |
| 7 | RightArrow | right | right ✓ |

Effect: Double tap DownArrow dodges left. Double tap LeftArrow dodges down.

---

## Audit Checklist

| Check | Result |
|-------|--------|
| `PlayerMovementActionRuntimeBootstrap` attaches to `PlayerManager.gameObject` | YES — BUG |
| `PlayerManager` has `PlayerController` | NO |
| `PlayerManager` has `Rigidbody2D` | NO |
| `PlayerDashController.GetComponent<PlayerController>()` finds component | NO (null) |
| `PlayerMovementDisplacementResolver.GetComponent<Rigidbody2D>()` finds component | NO (null) |
| `PlayerBlockController.GetComponent<PlayerController>()` finds component | NO (null) |
| Arrow key mapping correct in DirectionalDoubleTapDetector | NO (index 5-6 swapped) |
| `_rigidbody.MovePosition` called from non-FixedUpdate coroutine | YES (secondary) |
| Self-collision ignore in `ResolveTarget` | PARTIAL (collider check only) |

---

## Fix Plan

1. `PlayerMovementActionRuntimeBootstrap`: resolve `PlayerController` via `GetComponentInChildren`, `GetComponentInParent`, or `FindObjectOfType` fallback — attach to `playerController.gameObject`.
2. `DirectionalDoubleTapDetector`: fix index 5/6 swap (DownArrow→down, LeftArrow→left).
3. `PlayerMovementDisplacementResolver`: use `WaitForFixedUpdate` + add self-root collision ignore.
4. `PlayerMovementActionInput`: create #if helper for legacy/new input system.
5. Values: align to Combat Core canonical ranges.
