---
name: ui-projection-pattern
description: Build UI using the project's projection/ViewModel convention — pure C# state projections with enums + computed properties, thin MonoBehaviour views, EditMode tests. Use for any new screen, HUD element or panel (fable_14 UI canvas integration, fable_20 calendar HUD, fable_38 minimap).
---

# Skill: UI Projection Pattern

The project's UI follows MVVM-lite ("projections", WAVE 04/11): 20+ ViewModels with ~96 EditMode tests. Follow this convention for every new screen.

## The pattern (from CraftingRecipeViewModel and siblings)

```csharp
namespace CindarsHope.UI.<Area>
{
    /// <summary>Recipe state projection for UI display.</summary>
    public class <Thing>ViewModel        // pure C# — NO UnityEngine.UI, no MonoBehaviour
    {
        public enum <Thing>State          // explicit state enum, not bool soup
        {
            KnownCraftable, KnownMissingMaterials, KnownLockedBySkill, UnknownHidden, ...
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public <Thing>State State { get; set; } = <Thing>State.UnknownHidden;

        // computed properties derive presentation answers from State
        public bool CanCraft => State == <Thing>State.KnownCraftable;
        public bool IsLocked => State == ...;
        public string LockReason { get; set; }   // human-readable reason, never silent
    }
}
```

Layers:

| Layer | Type | Lives in | Tested by |
|---|---|---|---|
| ViewModel / Projection / Model | pure C# class | `Scripts/UI/<Area>/` | EditMode (`Tests/EditMode/UI/<Area>/`) |
| Builder/Service that fills the VM from game state | pure C# | same area | EditMode |
| View (panel controller, Text/Image binding) | MonoBehaviour | same area | human Play Mode scenario |

## Rules

1. **ViewModel = zero Unity UI dependencies.** Only domain usings (e.g., `CindarsHope.Craft.Data`). Sprites/colors resolve in the View by ID/state.
2. **State enum over booleans.** One enum captures mutually-exclusive states; computed bools derive from it. Prevents impossible combinations (locked AND craftable).
3. **Every disabled/locked/error state carries a reason string** the view can show (existing convention: `StateDescription`, `LockReason`). No silently disabled buttons.
4. **Views rebuild from events, never poll.** Subscribe to GameEventBus events (skill: event-bus-pattern), rebuild the VM, rebind. Unsubscribe on disable.
5. **Modal screens** go through the ModalManager stack + input focus routing (skill: ui-modal-stack; `InputFocusModalRoutingModel` is itself a tested projection).
6. **Empty/error/confirmation states** are part of the VM design from day one (existing precedent: SPEC 04 empty/error/confirmation patterns; tests like `MenuProjectionTests`).

## Mandatory tests (skill: editmode-test-authoring)

For each VM/projection: state derivation per game-state input (one test per state), computed property consistency, reason populated for every non-actionable state, empty-collection projection. Precedents: `CraftingRecipeViewModelTests`, `MenuProjectionTests`, `InventoryEquipmentTooltipTests`, `HudNotificationDebugProjectionTests`.

The View (MonoBehaviour) gets a human scenario covering open/close, Esc, focus, input blocking (rule: testing-quality-gate UI section).
