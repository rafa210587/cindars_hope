# WAVE_INTEGRATION_11 — Skill Effect Authoring Model

**Date:** 2026-06-08
**Status:** INITIAL

---

## Overview

This document describes how to author new skill effects in the WAVE_INTEGRATION_11 pipeline. The Effect Layer is designed to be extensible: each effect is a pure C# class implementing `ISkillEffectExecutor`, registered in `ActiveSkillExecutionController`, and cataloged in `WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md`.

---

## Authoring a New Effect

### Step 1: Create Executor

Create in `Assets/_Game/Scripts/Skills/Runtime/Effects/`:

```csharp
using CindarsHope.Skills.Runtime.Effects;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class MyNewSkillEffectExecutor : ISkillEffectExecutor
    {
        public string EffectId => "category.subcategory.action";
        public SkillEffectCategory Category => SkillEffectCategory.Utility;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.Self;

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            // context.Caster = player GameObject
            // context.Target = resolved target (may be null)
            // context.WorldPosition = target world position
            // context.SkillActionId = equip action ID
            // context.Time = Time.time at execution

            // ... do the effect ...

            return SkillEffectResult.Succeeded(
                message: "Effect applied!",
                costSpent: true,
                cooldownStarted: true);
        }
    }
}
```

### Step 2: Register in ActiveSkillExecutionController

In `ActiveSkillExecutionController.InitializeRegistry()`:

```csharp
_registry.Register(new MyNewSkillEffectExecutor());
```

### Step 3: Map Skill Action ID

In `ActiveSkillExecutionController.SkillActionToEffectId` (static readonly):

```csharp
{ "skill_your_category_your_action", "category.subcategory.action" },
```

The key must match the `UnlockedSkillActionId` of the skill tree node the player equips.

### Step 4: Catalog Entry

Add a row to `WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md`.

### Step 5: Add to Assembly-CSharp.csproj

Until Unity reimports and regenerates the csproj:

```xml
<Compile Include="Assets\_Game\Scripts\Skills\Runtime\Effects\MyNewSkillEffectExecutor.cs" />
```

---

## Target Types

| `SkillEffectTargetType` | Resolves To |
|------------------------|-------------|
| `None` | `null` |
| `Self` | `context.Caster` |
| `CurrentInteractable` | `InteractionSystem.GetCurrentInteractable()` |
| `NearestInteractable` | Not yet implemented (falls back to `null`) |
| `WorldPointInFrontOfPlayer` | Not yet implemented |
| `CropPlot` | `CurrentInteractable` if FarmPlot (same as CurrentInteractable) |
| `ResourceNode` | Not yet implemented |
| `Enemy` | Not yet implemented (requires combat scene) |
| `Area` | Not yet implemented |
| `DebugFixed` | `context.Caster` (for debug effects) |

`SkillTargetResolver.Resolve(targetType, caster)` handles the resolution. It requires `_interactionSystem` to be set via `SetInteractionSystem()` or injected from `ActiveSkillExecutionController.WireInteractionSystem()`.

---

## Effect Categories

| `SkillEffectCategory` | Use For |
|----------------------|---------|
| `Farm` | Crop watering, tilling, harvesting via skill |
| `Resource` | Wood cutting, mining, fishing via skill |
| `Combat` | Damage, status effects, area attacks |
| `Magic` | Spells, healing, buffs |
| `Survival` | Foraging, quick-heal, stamina restore |
| `Utility` | Quality-of-life, map markers, quick travel |
| `Debug` | Test-only effects |
| `Unknown` | Fallback; avoid in production |

---

## Stamina Costs

The `ActiveSkillExecutionController` does not currently deduct Stamina automatically (TODO: future integration). Each executor is responsible for deducting Stamina via the passed context or a cached reference. Document the cost in the executor class and in the skill catalog.

Example pattern (FarmCropSkillEffectExecutor approach):

```csharp
// TODO_INTEGRATION_NOT_FINAL: Stamina cost not yet deducted here.
// Will be handled by ActiveSkillExecutionController pre-execution check
// when StaminaManager is wired to the controller.
```

---

## Cooldowns

`ActiveSkillExecutionController` applies a **global cooldown per slot** of 1.5 seconds after any successful execution. Per-executor cooldowns are NOT yet implemented. If an executor needs a different cooldown, publish a `PlayerActionFeedbackEvent` from within the executor. Global cooldown tracking is per-slot (index 0-3 independently).

---

## Feedback

All skill execution results are published via:

```csharp
GameEventBus.Publish(new PlayerActionFeedbackEvent(result.Message));
```

This is handled by `ActiveSkillExecutionController` post-execution. Executors should populate `SkillEffectResult.Message` with a human-readable string (max 60 chars, Portuguese preferred).

---

*Authoring model created: 2026-06-08 (WAVE_INTEGRATION_11)*
