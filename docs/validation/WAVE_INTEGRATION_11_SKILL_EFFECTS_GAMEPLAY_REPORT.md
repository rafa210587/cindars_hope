# WAVE_INTEGRATION_11 — Skill Effects Gameplay Report

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Patch:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH

---

## Summary

A WAVE_INTEGRATION_11 implementou o pipeline de execução de efeitos de skill:
- Interface `ISkillEffectExecutor` e registry `SkillEffectRegistry`
- Executor real: `FarmCropSkillEffectExecutor` (farm.crop.water_skill)
- Controller: `ActiveSkillExecutionController` (teclas 1-4)
- Dash: `PlayerDashController` (Space + direção, 3.5 tiles, 40 Stamina)
- Dodge: `PlayerMovementAbilityController` (double-tap, 1.5 tiles, 40 Stamina)
- Block: BLOCK_RUNTIME_DEFERRED

O patch de balanceamento de action skills adicionou:
- 13 novas action skills ao `DefaultSkillCatalog.cs` (3 Melee, 2 Magic, 5 Survival, 4 Crafting)
- 14 novos EffectId mappings em `ActiveSkillExecutionController.SkillActionToEffectId`
- `FeedbackOnlySkillEffectExecutor`: executor genérico de feedback para DEFERRED_RUNTIME_EFFECT
- 14 registros de FeedbackOnlySkillEffectExecutor no registry

---

## Skill Counts by Tree

| Tree | Total nodes | Active slot skills | Passive/Modifier | Capstone |
|---|---:|---:|---:|---:|
| Melee | 14 | 8 | 5 | 1 |
| Ranged | 11 | 5 | 5 | 1 |
| Magic | 13 | 8 | 4 | 1 |
| Survival | 16 | 7 | 8 | 1 |
| Crafting | 15 | 6 | 8 | 1 |
| **Total** | **69** | **34** | **30** | **5** |

---

## Effect Execution Pipeline Status

| Component | Status | Notes |
|---|---|---|
| ISkillEffectExecutor | BUILD_VALIDATED | Interface stable |
| SkillEffectRegistry | BUILD_VALIDATED | register/resolve/hasExecutor |
| SkillEffectContext | BUILD_VALIDATED | all context fields available |
| SkillEffectResult | BUILD_VALIDATED | Succeeded/Failed factories |
| SkillEffectCategory | BUILD_VALIDATED | Farm/Combat/Magic/Survival/Utility |
| SkillEffectTargetType | BUILD_VALIDATED | None/Self/CurrentInteractable/etc. |
| SkillTargetResolver | BUILD_VALIDATED | resolves targets per type |
| FarmCropSkillEffectExecutor | BUILD_VALIDATED | farm.crop.water_skill — real effect |
| FeedbackOnlySkillEffectExecutor | BUILD_VALIDATED | 14 effects — feedback only, DEFERRED |
| ActiveSkillExecutionController | BUILD_VALIDATED | 1-4 keys, cooldown, feedback |
| SkillActionToEffectId | BUILD_VALIDATED | 32 mappings total |

---

## Movement Abilities Status

| Ability | Controller | Status | Notes |
|---|---|---|---|
| Dash | PlayerDashController | BUILD_VALIDATED | Space+dir, 3.5t, 40sp, 1.0s cd |
| Dodge | PlayerMovementAbilityController | BUILD_VALIDATED | double-tap, 1.5t, 40sp, 0.6s cd |
| Block | — | BLOCK_RUNTIME_DEFERRED | Left Shift; combat runtime pendente |

---

## Debt Summary

```text
1. FeedbackOnlySkillEffectExecutor: 14 efeitos retornam feedback sem gameplay real.
   Substituir quando: combat/utility/farm-utility runtime existir.
   TODO_INTEGRATION_NOT_FINAL em cada executor.

2. SkillActionToEffectId mappings sem executor: 10+ combat effects (melee, ranged, magic).
   Esses retornam "Efeito X sem executor. (Deferred)" ao jogador.

3. Block: BLOCK_RUNTIME_DEFERRED — aguarda CombatManager/PostureSystem.

4. Cooldown hardcoded: 1.5s todos os slots. Final value depende de SkillDefinition.

5. Stamina cost hardcoded: 40 para Dash e Dodge. Final value depende de refinement.

6. Passive modifiers registered but not consumed: todos os SkillPassiveModifier registrados
   no catálogo aguardam que os sistemas consumidores (CombatManager, StaminaManager, etc.)
   leiam SkillPassiveApplicator.
```

---

## Validation Results (Build)

```text
Assembly-CSharp: PASS (exit code 0, 2 warnings pré-existentes, 0 errors)
Assembly-CSharp-Editor: PASS (exit code 0, 7 warnings pré-existentes, 0 errors)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing governance errors only)
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: NO (FeedbackOnlySkillEffectExecutor sempre retorna Success — sem lógica)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO
Automated tests not added: JUSTIFIED
  Reason: FeedbackOnlySkillEffectExecutor is trivial (always returns Succeeded); no branching logic.
          DefaultSkillCatalog additions are data-only; no test harness for catalog count exists.
          Existing FarmCropSkillEffectExecutor unchanged.
Manual Play Mode scenario: docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md (updated)
Residual risk: Novas action skills visíveis na skill tree mas efeitos são apenas feedback;
               jogador pode ficar confuso sobre por que "Avanço de Aço" não causa dano real.
               Mitigado: FeedbackOnlySkillEffectExecutor exibe mensagem "(Efeito de combate pendente.)".
```

---

## Files Changed (Balance Patch)

| File | Change |
|---|---|
| `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` | +13 nodes (3 Melee, 2 Magic, 5 Survival, 4 Crafting) |
| `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` | +14 SkillActionToEffectId mappings; RegisterFeedbackExecutors() method |
| `Assets/_Game/Scripts/Skills/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs` | NEW — generic feedback executor |
| `Assembly-CSharp.csproj` | +1 Compile Include entry for FeedbackOnlySkillEffectExecutor |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM.md` | NEW |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_ACTION_SLOT_MAPPING.md` | NEW |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_PASSIVE_MODIFIER_MAPPING.md` | NEW |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_MOVEMENT_ACTIONS_MAPPING.md` | NEW |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md` | UPDATED — 15 effects total |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md` | CREATED |
| `docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md` | UPDATED — action balance section |
| `docs/project/CURRENT_STATE.md` | UPDATED |

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
