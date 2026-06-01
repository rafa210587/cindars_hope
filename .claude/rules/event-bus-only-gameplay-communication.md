# Rule: Event Bus Only for Gameplay Communication

## Rule

Gameplay systems must communicate via `GameEventBus.Publish()` and `Subscribe()`. Direct MonoBehaviour-to-MonoBehaviour calls for gameplay events are prohibited.

## Why

Direct coupling between gameplay systems creates tight dependencies, breaks when either system changes, and bypasses the observable event stream needed for save/load, analytics, and UI updates. GameEventBus decouples producers from consumers and makes event flow auditable.

## Applies To

All gameplay systems: combat, farming, inventory, UI state, NPC dialogue, cave events, day cycle, economy.

## Violation Examples

```csharp
// PROHIBITED: direct call from combat to UI
_hudManager.UpdateHealth(newHealth);

// PROHIBITED: GetComponent for gameplay communication
GetComponent<InventoryManager>().AddItem(item);

// ALLOWED: event bus
GameEventBus.Publish(new PlayerHealthChangedEvent(newHealth));
GameEventBus.Publish(new ItemPickedUpEvent(item));
```

## Allowed Exceptions

- **Editor tools** under `Assets/_Game/Scripts/Editor/**` — may use direct refs
- **Bootstrap wiring** — `GameBootstrap` may hold direct refs for injection; not gameplay communication
- **Local component access** — `GetComponent<T>()` on the same GameObject for setup, not runtime gameplay events
- **Unity lifecycle calls** — Start(), Awake(), OnDestroy() internal setup

## What To Do If Exception Is Needed

State in the spec: "Direct call authorized: `X` calls `Y.Method()` because `[reason event bus is not suitable here]`."

## Validation / Detection

`/review-non-regression` and `.claude/hooks/check-runtime-forbidden-search.ps1` audit for direct calls in gameplay systems.
