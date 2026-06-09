# WAVE_INTEGRATION_11 — Skill Effect Catalog

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Last update:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH (2026-06-08)

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

## Registered Effects

| Effect ID | Executor Class | Category | Target Type | Description | Status |
|-----------|---------------|----------|-------------|-------------|--------|
| `farm.crop.water_skill` | `FarmCropSkillEffectExecutor` | Farm | CurrentInteractable | Waters the currently targeted FarmPlot | IMPLEMENTED |
| `melee.avanco_aco` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Avanço de Aço — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `melee.grito_desafio` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Grito de Desafio — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `melee.investida_quebra_guarda` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Investida Quebra-Guarda — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `magic.chama_breve` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Chama Breve — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `magic.rajada_gelida` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Rajada Gélida — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.sinal_retirada` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Sinal de Retirada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.isca_improvisada` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Isca Improvisada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.kit_emergencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Kit de Emergência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.instinto_sobrevivencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Instinto de Sobrevivência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.campo_seguro` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Campo Seguro — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.irrigador_portatil` | `FeedbackOnlySkillEffectExecutor` | Farm | None | Irrigador Portátil — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.bomba_improvisada` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Bomba Improvisada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.mecanismo_campo` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Mecanismo de Campo — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.marca_eficiencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Marca de Eficiência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |

---

## Skill Action → Effect ID Mappings

Defined in `ActiveSkillExecutionController.SkillActionToEffectId` (static dictionary):

### Original WI11 mappings

| Skill Action ID | Effect ID | Status |
|----------------|-----------|--------|
| `skill_survival_emergency_roll` | `farm.crop.water_skill` | BRIDGE (debug demo) |
| `skill_crafting_field_patch` | `farm.crop.water_skill` | BRIDGE (debug demo) |
| `skill_melee_offhand_cut` | `combat.melee.offhand_cut` | NO_EXECUTOR (deferred) |
| `skill_melee_guarded_block` | `combat.melee.block` | NO_EXECUTOR (deferred) |
| `skill_melee_battle_dash` | `combat.melee.battle_dash` | NO_EXECUTOR (deferred) |
| `skill_melee_leap_attack` | `combat.melee.leap_attack` | NO_EXECUTOR (deferred) |
| `skill_melee_whirl_cut` | `combat.melee.whirl_cut` | NO_EXECUTOR (deferred) |
| `skill_ranged_charged_shot` | `combat.ranged.charged_shot` | NO_EXECUTOR (deferred) |
| `skill_ranged_line_piercer` | `combat.ranged.line_piercer` | NO_EXECUTOR (deferred) |
| `skill_ranged_multishot_fan` | `combat.ranged.multishot_fan` | NO_EXECUTOR (deferred) |
| `skill_ranged_bleeding_arrow` | `combat.ranged.bleeding_arrow` | NO_EXECUTOR (deferred) |
| `skill_ranged_marked_prey` | `combat.ranged.marked_prey` | NO_EXECUTOR (deferred) |
| `skill_magic_fire_spark` | `combat.magic.fire_spark` | NO_EXECUTOR (deferred) |
| `skill_magic_ice_bind` | `combat.magic.ice_bind` | NO_EXECUTOR (deferred) |
| `skill_magic_toxic_cloud` | `combat.magic.toxic_cloud` | NO_EXECUTOR (deferred) |
| `skill_magic_lightning_chain` | `combat.magic.lightning_chain` | NO_EXECUTOR (deferred) |
| `skill_magic_elemental_ward` | `combat.magic.elemental_ward` | NO_EXECUTOR (deferred) |
| `skill_magic_slowing_sigils` | `combat.magic.slowing_sigils` | NO_EXECUTOR (deferred) |

### Balance Patch mappings (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)

| Skill Action ID | Effect ID | Status |
|----------------|-----------|--------|
| `skill_melee_avanco_aco` | `melee.avanco_aco` | FEEDBACK_EXECUTOR |
| `skill_melee_grito_desafio` | `melee.grito_desafio` | FEEDBACK_EXECUTOR |
| `skill_melee_investida_quebra_guarda` | `melee.investida_quebra_guarda` | FEEDBACK_EXECUTOR |
| `skill_magic_chama_breve` | `magic.chama_breve` | FEEDBACK_EXECUTOR |
| `skill_magic_rajada_gelida` | `magic.rajada_gelida` | FEEDBACK_EXECUTOR |
| `skill_survival_sinal_retirada` | `survival.sinal_retirada` | FEEDBACK_EXECUTOR |
| `skill_survival_isca_improvisada` | `survival.isca_improvisada` | FEEDBACK_EXECUTOR |
| `skill_survival_kit_emergencia` | `survival.kit_emergencia` | FEEDBACK_EXECUTOR |
| `skill_survival_instinto_sobrevivencia` | `survival.instinto_sobrevivencia` | FEEDBACK_EXECUTOR |
| `skill_survival_campo_seguro` | `survival.campo_seguro` | FEEDBACK_EXECUTOR |
| `skill_crafting_irrigador_portatil` | `crafting.irrigador_portatil` | FEEDBACK_EXECUTOR |
| `skill_crafting_bomba_improvisada` | `crafting.bomba_improvisada` | FEEDBACK_EXECUTOR |
| `skill_crafting_mecanismo_campo` | `crafting.mecanismo_campo` | FEEDBACK_EXECUTOR |
| `skill_crafting_marca_eficiencia` | `crafting.marca_eficiencia` | FEEDBACK_EXECUTOR |

```text
FEEDBACK_EXECUTOR = FeedbackOnlySkillEffectExecutor registered; returns Success + feedback message;
                    active slot cooldown triggers; no gameplay effect applied.
                    TODO_INTEGRATION_NOT_FINAL
NO_EXECUTOR = EffectId mapped but no executor registered → "Efeito X sem executor. (Deferred)"
BRIDGE = debug mapping to farm.crop.water_skill for vertical slice
```

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
4. Register in `ActiveSkillExecutionController.RegisterFeedbackExecutors()` or `Bootstrap()`:
   ```csharp
   _registry.Register(new YourSkillEffectExecutor());
   ```
5. Add mapping in `SkillActionToEffectId`:
   ```csharp
   { "skill_your_action_id", "your.effect.id" }
   ```
6. Add entry to this catalog
7. Add entry to `Assembly-CSharp.csproj` `<Compile Include>` block

---

## Movement Abilities (Not Skill Slots)

Movement abilities are NOT in the active skill slot pipeline. They have dedicated controllers:

| Ability | Input | Distance | Cost | Cooldown | Controller | Status |
|---------|-------|----------|------|---------|-----------|--------|
| Dash | Space + WASD/Arrow | 3.5 tiles | 40 Stamina | 1.0s | `PlayerDashController` | BUILD_VALIDATED |
| Dodge | Double-tap WASD/Arrow | 1.5 tiles | 40 Stamina | 0.6s | `PlayerMovementAbilityController` | BUILD_VALIDATED |
| Block | Left Shift | — | Stamina drain | — | BLOCK_RUNTIME_DEFERRED | DEFERRED |

---

*Catalog created: 2026-06-08 (WAVE_INTEGRATION_11)*
*Updated: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH — 14 new FeedbackOnlySkillEffectExecutor registrations)*
