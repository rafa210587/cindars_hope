---
name: ui-modal-stack
description: Implement and validate modal UI using ModalManager stack, input blocking, and Esc-close behavior
version: 1.0
when_to_use: Any task touching modal UI, inventory screen, shop, equipment, skill tree, pause, death screen, Anya UI
---

# UI Modal Stack Skill

## Use When

Task touches:
- `ModalManager` (push/pop/clear)
- Inventory, equipment, shop, skill tree panels
- Pause screen, death screen
- Anya cutscene/conversation UI
- Input blocking during modal open
- Esc key closing top modal

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. `Assets/_Game/Scripts/UI/` — ModalManager and relevant panels

## Do Not Read By Default

```
All scene files
All prefab files
Unrelated UI scripts
```

## Core Invariants

### Esc Always Closes Top Modal

```csharp
// ModalManager must handle Esc to pop top modal
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && _stack.Count > 0)
        PopModal();
}
```

### Input Blocked During Modal

When any modal is open:
- Player movement must be disabled
- Attack input must be disabled
- Use `GameEventBus.Publish(new ModalOpenedEvent())` / `ModalClosedEvent()`
- Player input handlers subscribe and disable themselves

### Stack Behavior

```csharp
// Open
ModalManager.Instance.Push(panelGameObject);

// Close top
ModalManager.Instance.Pop();

// Close all
ModalManager.Instance.Clear();
```

### No Modal Mismatch

- Never `Push` without a corresponding `Pop` path
- Closing the game/scene must call `Clear()`
- Each modal must have exactly one "close" trigger (Esc, X button, or explicit close call)

## Validation Checklist (Play Mode — Phase 3)

Manual checks:
- [ ] Esc closes the top-most modal only
- [ ] Player cannot move while modal is open
- [ ] Player cannot attack while modal is open
- [ ] Opening nested modals (e.g., shop from inventory) works correctly
- [ ] Closing nested modal returns to parent, not main game
- [ ] Scene transition clears all modals

## Common Regressions

- Modal opens but never pops (infinite input block)
- Player can still move while inventory is open
- Esc closes all modals instead of just the top one
- Missing `GameEventBus.Publish(new ModalClosedEvent())` on pop

## Stop Conditions

- `ModalManager` uses `FindObjectOfType` — violates no-global-search rule
- Modal state persists across scene loads without explicit clear
