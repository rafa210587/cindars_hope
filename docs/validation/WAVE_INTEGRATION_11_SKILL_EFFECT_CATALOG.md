# WAVE_INTEGRATION_11 — Skill Effect Catalog

**Date:** 2026-06-08
**Status:** INITIAL — Extensible

---

## Effect Pipeline Overview

```
Player presses 1-4
  → ActiveSkillExecutionController reads equipped skill action ID from SkillTreeManager.State
  → SkillActionToEffectId maps action ID → effect ID
  → SkillEffectRegistry.Resolve(effectId) returns executor
  → SkillTargetResolver.Resolve(targetType, caster) returns target GameObject
  → ISkillEffectExecutor.Execute(SkillEffectContext) returns SkillEffectResult
  → PlayerActionFeedbackEvent published with result message
```

---

## Registered Effects (WAVE_INTEGRATION_11)

| Effect ID | Executor Class | Category | Target Type | Description |
|-----------|---------------|----------|-------------|-------------|
| `farm.crop.water_skill` | `FarmCropSkillEffectExecutor` | Farm | CurrentInteractable | Waters the currently targeted FarmPlot |

---

## Skill Action → Effect ID Mappings

Defined in `ActiveSkillExecutionController.SkillActionToEffectId` (static dictionary):

| Skill Action ID | Effect ID |
|----------------|-----------|
| `skill_melee_battle_dash` | `movement.dash` (not yet registered; falls back to "no executor") |
| `skill_farm_watering_aoe` | `farm.crop.water_skill` |
| `skill_farm_watering_targeted` | `farm.crop.water_skill` |
| `skill_survival_foraging_quick` | `survival.forage.quick` (not yet registered) |
| `skill_crafting_quick_repair` | `crafting.quick_repair` (not yet registered) |
| `skill_magic_healing_wave` | `magic.heal.wave` (not yet registered) |
| `skill_ranged_volley` | `combat.ranged.volley` (not yet registered) |

Effects without a registered executor produce a feedback message "Skill not yet implemented" and return `SkillEffectResult.Failed("no_executor")`.

---

## Effect Contract

### ISkillEffectExecutor

```csharp
public interface ISkillEffectExecutor
{
    string EffectId { get; }
    SkillEffectCategory Category { get; }
    SkillEffectTargetType TargetType { get; }
    SkillEffectResult Execute(SkillEffectContext context);
}
```

### SkillEffectContext

Fields available to every executor:
- `SkillActionId` — the action ID from the skill tree node
- `EffectId` — the resolved effect ID
- `ActiveSlotIndex` — which slot (0-3) was pressed
- `Caster` — the player GameObject
- `Target` — resolved target (may be null)
- `WorldPosition` — target world position
- `SceneName` — current scene name
- `Time` — `Time.time` at execution

### SkillEffectResult

Factory methods:
- `SkillEffectResult.Succeeded(message, costSpent, cooldownStarted)` — `Success = true`
- `SkillEffectResult.Failed(reason, message)` — `Success = false`

---

## Extension Guide: Adding a New Effect

1. Create `YourSkillEffectExecutor.cs` implementing `ISkillEffectExecutor`
2. Set `EffectId`, `Category`, `TargetType`
3. Implement `Execute(SkillEffectContext)` — return `SkillEffectResult.Succeeded/Failed`
4. Register in `ActiveSkillExecutionController.InitializeRegistry()`:
   ```csharp
   _registry.Register(new YourSkillEffectExecutor());
   ```
5. Add mapping in `SkillActionToEffectId`:
   ```csharp
   { "skill_your_action_id", "your.effect.id" }
   ```
6. Add entry to this catalog

---

## Movement Abilities (Not Skill Slots)

Movement abilities are NOT in the active skill slot pipeline. They have dedicated controllers:

| Ability | Input | Distance | Cost | Controller |
|---------|-------|----------|------|-----------|
| Dash | Space + WASD/Arrow | 3.5 tiles | 40 Stamina | `PlayerDashController` |
| Dodge | Double-tap WASD/Arrow | 1.5 tiles | 40 Stamina | `PlayerMovementAbilityController` |
| Block | Left Shift | — | — | BLOCK_RUNTIME_DEFERRED |

---

*Catalog created: 2026-06-08 (WAVE_INTEGRATION_11)*
*Extension: add new rows to the Registered Effects table when new executors are added.*
