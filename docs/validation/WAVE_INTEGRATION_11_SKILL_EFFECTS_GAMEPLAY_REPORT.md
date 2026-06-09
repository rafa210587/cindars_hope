# WAVE_INTEGRATION_11 — Skill Effects Gameplay Report

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_MOVEMENT_RUNTIME_FIX_PENDING_HUMAN_PLAYMODE
**Patch:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH

---

## Summary

A WAVE_INTEGRATION_11 implementou o pipeline de execução de efeitos de skill:
- Interface `ISkillEffectExecutor` e registry `SkillEffectRegistry`
- Executor real: `FarmCropSkillEffectExecutor` (farm.crop.water_skill)
- Controller: `ActiveSkillExecutionController` (teclas 1-4)
- Dash: `PlayerDashController` (Space + direção, 3.5 tiles, 40 Stamina)
- Dodge: `PlayerDodgeController` + `DirectionalDoubleTapDetector` (double-tap, 1.5 tiles, 40 Stamina)
- Block: `PlayerBlockController` (Left Shift, slow 0.5x, 10 Stamina/s; damage reduction deferred)

Runtime input fix (2026-06-09):
- `DefaultSkillCatalog.BuildAllTrees()` now includes the 14 balance-patch action nodes, so they appear in actual Skill Tree UI lists.
- `ActiveSkillExecutionController` resolves active slots containing either `SkillActionId` or `SkillNodeId`, then revalidates purchased/equippable node state before execution.
- Active slot gameplay keys remain `1/2/3/4`; Skill Tree equip keys remain `R/T/Y/G` while the panel is open.
- Dash and Dodge controllers now bind to `GameBootstrap.Instance.PlayerManager.gameObject` at runtime, so current FarmScene does not require scene edits for these controllers.
- Dash/Dodge movement now uses the player's own `Collider2D.Cast` path to avoid self-hit and stop before blocking colliders.

Movement actions runtime fix (2026-06-09):
- `PlayerMovementActionRuntimeBootstrap` now attaches Dash, Dodge, DirectionalDoubleTapDetector, Block and movement resolver to the Player without scene edits.
- Dash remains `Space + direction`, 3.5 units, 40 Stamina, non-slot.
- Dodge is now explicit `PlayerDodgeController`, double tap direction, 1.5 units, 40 Stamina, non-slot.
- Block now works as a runtime movement slow while Left Shift is held; frontal damage reduction remains deferred.

O patch de balanceamento de action skills adicionou:
- 14 novas action skills ao `DefaultSkillCatalog.cs` (3 Melee, 2 Magic, 5 Survival, 4 Crafting)
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
| ActiveSkillExecutionController | BUILD_VALIDATED_RUNTIME_INPUT_FIX | 1-4 keys, cooldown, feedback, SkillActionId/SkillNodeId slot resolution |
| SkillActionToEffectId | BUILD_VALIDATED | 32 mappings total |

---

## Movement Abilities Status

| Ability | Controller | Status | Notes |
|---|---|---|---|
| Dash | PlayerDashController | BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE | Space+dir/facing fallback, 3.5t, 40sp, 1.0s cd, runtime-bound to Player |
| Dodge | PlayerDodgeController | BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE | double-tap, 1.5t, 40sp, 0.6s cd, runtime-bound to Player |
| Block | PlayerBlockController | BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE_DAMAGE_REDUCTION_DEFERRED | Left Shift slow 0.5x, 10 stamina/s; damage reduction deferred |

---

## Debt Summary

```text
1. FeedbackOnlySkillEffectExecutor: 14 efeitos retornam feedback sem gameplay real.
   Substituir quando: combat/utility/farm-utility runtime existir.
   TODO_INTEGRATION_NOT_FINAL em cada executor.

2. SkillActionToEffectId mappings sem executor: 10+ combat effects (melee, ranged, magic).
   Esses retornam "Efeito X sem executor. (Deferred)" ao jogador.

3. Block: movement slow is runtime-ready; combat damage reduction remains deferred until CombatManager/PostureSystem.

4. Cooldown hardcoded: 1.5s todos os slots. Final value depende de SkillDefinition.

5. Stamina cost hardcoded: 40 para Dash e Dodge. Final value depende de refinement.

6. Passive modifiers registered but not consumed: todos os SkillPassiveModifier registrados
   no catálogo aguardam que os sistemas consumidores (CombatManager, StaminaManager, etc.)
   leiam SkillPassiveApplicator.
```

---

## Validation Results (Build)

```text
Assembly-CSharp: PASS (exit code 0, 0 warnings, 0 errors)
Assembly-CSharp-Editor: PASS (exit code 0, 7 warnings pré-existentes, 0 errors)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing governance errors only)
Quality check: HARNESS_FAIL_PESTER_3_4_0_KNOWN_ISSUE
Unity Play Mode: NOT RUN (human validation required)
```

Quality command attempted:

```text
powershell -ExecutionPolicy Bypass -File .\tools\docs\check_spec_quality.ps1
```

Result: exit code 1 before repository checks, with Pester 3.4.0 error `The Should command may only be used inside a Describe block.` This is a known harness issue already documented in WAVE 05 validation reports.

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
| `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` | +14 nodes (3 Melee, 2 Magic, 5 Survival, 4 Crafting) |
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

## Files Changed (Runtime Input Fix)

| File | Change |
|---|---|
| `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` | 14 balance-patch nodes added to actual tree lists |
| `Assets/_Game/Scripts/Skills/SkillTreeState.cs` | purchased count accepts `treeId_` and `treeId.` node IDs |
| `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` | runtime slot resolution, validation diagnostics, single feedback publish |
| `Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs` | runtime player binding, Space+direction/facing fallback, collider-cast movement |
| `Assets/_Game/Scripts/Player/Movement/PlayerMovementActionRuntimeBootstrap.cs` | runtime attaches movement controllers to Player |
| `Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs` | double-tap dodge collider-cast movement |
| `Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs` | Left Shift block slow runtime |
| `Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs` | explicit Unity input alias |
| `Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs` | shared Rigidbody2D/Collider2D displacement resolver |
| `Assets/_Game/Scripts/Player/Movement/GridMovementDisplacementResolver.cs` | optional moving collider cast path |
| `Assets/_Game/Scripts/Combat/PlayerAttackController.cs` | prevents legacy Space dodge from also firing on Space+direction Dash |
| `Assets/_Game/Scripts/Editor/Validation/ValidateWave11RuntimeInputBinding.cs` | new runtime input binding validator |
| `docs/validation/WAVE_INTEGRATION_11_RUNTIME_INPUT_FAILURE_AUDIT.md` | created |
| `docs/validation/WAVE_INTEGRATION_11_RUNTIME_INPUT_FIX_REPORT.md` | created |
| `docs/project/CURRENT_STATE.md` | updated |

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
*Updated: 2026-06-09 (WAVE_INTEGRATION_11_RUNTIME_INPUT_FIX_PENDING_HUMAN_PLAYMODE)*
