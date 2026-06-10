---
name: player-ability-runtime
description: Add a non-slot player ability (Dash, Dodge, Block, roll, teleport, etc.) with correct FixedUpdate/physics wiring
version: 1.0
when_to_use: Any task adding a new non-slot player ability that moves or modifies the player at runtime
---

# Player Ability Runtime Skill

## Use When

Task adds or fixes a player ability that:
- Is triggered by input (key down / hold / double-tap)
- Moves the player or changes movement speed
- Is **not** an active skill slot (1–4 keys)
- Runs at runtime (not editor-only)

Examples: Dash, Dodge, Block, roll, sprint toggle, teleport, blink.

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. `Assets/_Game/Scripts/Player/PlayerController.cs` — current SpeedMultiplier, IsBeingDisplaced, FixedUpdate
4. `Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs` — TryDisplace API

---

## Core Architecture

### Component Attachment

All ability controllers are attached to the player's GameObject via a `RuntimeInitializeOnLoadMethod` bootstrap.

**Reuse `PlayerMovementActionRuntimeBootstrap` if it already exists.** Only create a new bootstrap if it is truly a different lifecycle (e.g., combat system bootstrap).

```csharp
private void AttachControllers(GameObject playerObject)
{
    EnsureComponent<PlayerMovementDisplacementResolver>(playerObject);
    EnsureComponent<PlayerDashController>(playerObject);
    EnsureComponent<DirectionalDoubleTapDetector>(playerObject);
    EnsureComponent<PlayerDodgeController>(playerObject);
    EnsureComponent<PlayerBlockController>(playerObject);
    // Add new ability here:
    // EnsureComponent<PlayerRollController>(playerObject);
}
```

---

## The FixedUpdate Conflict Rule

**Critical**: Any ability that calls `Rigidbody2D.MovePosition()` from a coroutine (Update cadence) WILL be overwritten by `PlayerController.FixedUpdate` unless `IsBeingDisplaced` is set.

### Why it breaks

```
Frame N:
  FixedUpdate → rb.MovePosition(current_pos) → physics queues current_pos
  Update/Coroutine → rb.MovePosition(target_pos) → physics queues target_pos

Frame N+1:
  FixedUpdate → rb.MovePosition(current_pos2) ← OVERWRITES target_pos
  Physics → applies current_pos2 → player never moves
```

### Required fix pattern

In `PlayerController`:
```csharp
public bool IsBeingDisplaced { get; set; }

private void FixedUpdate()
{
    if (_rigidbody == null) { LogMissingRigidbodyOnce(); return; }
    if (IsBeingDisplaced) return;   // ← skip when displaced
    // ... normal movement
}
```

In `PlayerMovementDisplacementResolver.DisplaceRoutine`:
```csharp
if (_playerController != null)
{
    _playerController.SpeedMultiplier = 0f;
    _playerController.IsBeingDisplaced = true;
}
// ... displacement loop ...
if (_playerController != null)
{
    _playerController.SpeedMultiplier = previousSpeedMultiplier;
    _playerController.IsBeingDisplaced = false;
}
```

---

## Ability Controller Template

```csharp
[DisallowMultipleComponent]
public sealed class PlayerXController : MonoBehaviour
{
    [SerializeField] private float _distance = 3.5f;
    [SerializeField] private float _duration = 0.14f;
    [SerializeField] private float _cooldown = 1.0f;
    [SerializeField] private int _staminaCost = 40;

    [SerializeField] private PlayerMovementDisplacementResolver _resolver;
    [SerializeField] private StaminaManager _staminaManager;

    private float _lastUseTime = float.MinValue;

    private void Start()
    {
        if (_resolver == null) _resolver = GetComponent<PlayerMovementDisplacementResolver>();
        var bootstrap = GameBootstrap.Instance;
        if (bootstrap != null && _staminaManager == null)
            _staminaManager = bootstrap.StaminaManager;
    }

    private void Update()
    {
        // 1. Modal guard — always first
        if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;

        // 2. Read input
        if (!DetectInput(out var direction)) return;

        // 3. Try ability
        TryUse(direction);
    }

    private void TryUse(Vector2 direction)
    {
        // 4. In-progress guard
        if (_resolver == null || _resolver.IsDisplacing) return;

        // 5. Cooldown
        if (Time.time - _lastUseTime < _cooldown)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X em cooldown."));
            return;
        }

        // 6. Stamina (optional — graceful if missing)
        if (_staminaManager != null && !_staminaManager.TrySpendStamina(_staminaCost))
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente."));
            return;
        }

        _lastUseTime = Time.time;
        if (!_resolver.TryDisplace(direction, _distance, _duration, () =>
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X!"))))
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X bloqueado."));
        }
    }

    private bool DetectInput(out Vector2 direction)
    {
        direction = Vector2.zero;
        // implement per ability
        return false;
    }
}
```

---

## Input Patterns

### One-shot key press (Dash)

```csharp
if (Input.GetKeyDown(KeyCode.Space))
{
    var dir = ReadDirectionalInput();
    if (dir.sqrMagnitude <= 0.1f && _playerController != null)
        dir = _playerController.LastFacingDirection;
    if (dir.sqrMagnitude > 0.1f) TryUse(dir.normalized);
}
```

### Double-tap detection (Dodge)

Use `DirectionalDoubleTapDetector.UpdateAndCheckDoubleTap()` — already exists. Do not create a parallel detector.

### Hold key (Block, sprint)

```csharp
private void Update()
{
    if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
    {
        if (_isActive) Deactivate();
        return;
    }

    var holding = Input.GetKey(KeyCode.LeftShift);
    if (holding && !_isActive) Activate();
    else if (!holding && _isActive) Deactivate();
}
```

---

## Speed Modification (Block / slow)

Use `PlayerMovementSlowState` if it exists. Otherwise:

```csharp
// Activate
if (_playerController != null) _playerController.SpeedMultiplier *= _slowMultiplier;

// Deactivate — restore exact previous value
if (_playerController != null) _playerController.SpeedMultiplier = _previousMultiplier;
```

Store `_previousMultiplier = _playerController.SpeedMultiplier` before activation.

---

## Stamina Integration

Always optional and graceful:

```csharp
// One-shot cost
if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost)) { /* reject */ return; }

// Per-second drain (Block)
if (_staminaManager != null)
{
    _staminaManager.TrySpendStamina(_drainPerSecond * Time.deltaTime);
    if (_staminaManager.CurrentStamina <= 0f) Deactivate();
}
```

If `StaminaManager` or its API is absent → document debt:
```
STAMINA_MOVEMENT_ACTION_DEBT
```

---

## Feedback

Always publish, even if no HUD consumer exists yet:

```csharp
GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash!"));
```

If HUD doesn't show it yet → document debt:
```
HUD_FEEDBACK_CONSUMER_DEBT
```

---

## Debt Tags (use in comments + report)

```
TODO_INTEGRATION_NOT_FINAL
BALANCE_FINAL_PENDING
BLOCK_DAMAGE_REDUCTION_DEFERRED_TO_COMBAT_RUNTIME
DODGE_IFRAMES_DEFERRED_TO_COMBAT_RUNTIME
STAMINA_MOVEMENT_ACTION_DEBT
HUD_FEEDBACK_CONSUMER_DEBT
BOUNDS_FINAL_DEFERRED_IF_NO_BOUND_SYSTEM
```

---

## Validation

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

Human Play Mode checklist must cover:
- Ability executes (player actually moves / state changes)
- Modal guard blocks execution
- Cooldown prevents spam
- Stamina consumed or debt explicit
- Collision respected or debt explicit

---

## Common Regressions

- Not setting `IsBeingDisplaced = true` → player doesn't move (the original bug)
- Not restoring `SpeedMultiplier` after displacement → player stuck at 0 speed
- Not restoring `IsBeingDisplaced = false` on exception/early exit → player frozen forever
- Creating a second `DirectionalDoubleTapDetector` instead of reusing existing
- Adding ability to active slot (1–4 keys) — these are NON-SLOT actions

## Stop Conditions

- Moving player requires rewriting `PlayerController` core → stop, report
- No `Rigidbody2D` or `transform` reachable → `BLOCKED`
- Ability conflicts with existing non-slot action (same input key) → stop, report conflict
