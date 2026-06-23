# Execution Report — fable_71 (HUD de Active Skills)

> **Spec:** `fable_71_spec_active_skill_hud_hotbar_runtime`
> **Data:** 2026-06-23
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Executor:** Claude (Opus) via continuação de `/execute-spec-strict`

## Resumo

Implementado o **lado-código completo** da HUD de active skills: exposição read-only de cooldown no controller, ponto único de uso (`TryUseSlot`) compartilhado por teclas 1-4 e clique, projeção pura testável do estado do slot, binder preenchendo cooldown/estado/usabilidade, e a View com botão de uso + fill radial de cooldown + overlay de bloqueio. O **wiring do canvas** (ligar os arrays serializados aos objetos de UI na cena) é um passo de Unity Editor (skill `hud-canvas-binding`) — a View é null-safe/headless até lá.

## Acceptance criteria extracted

| # | Critério (spec) | Resultado |
|---|---|---|
| 14.1 | Cooldown visível e correto (fill + fonte única) | OK (código) — fill via controller; PlayMode defere o visual |
| 14.2 | Botão de uso (clique == tecla) | OK — clique e teclas chamam `ActiveSkillExecutionController.TryUseSlot` |
| 14.3 | Custo e bloqueio | PARCIAL — bloqueio Empty/Cooldown OK; custo (NoMp/NoStamina) DEFERRED (custo é privado nos executores) |
| 14.4 | Guard respeitado | OK — `FinalHudGuardValidator.Validate` mantido no Refresh |

## Existing systems audit

- `ActiveSkillExecutionController` — **reutilizado**; adicionada API read-only (`Instance`, `GetSlotCooldownRemaining/Total`, `TryUseSlot`) sem duplicar a lógica de execução.
- `GameplayHudViewModel` / `ActiveSkillSlotViewModel` — **reutilizados** (campos já existiam; agora preenchidos).
- `GameplayHudRuntimeBinder` / `ActiveSkillSlotsHudView` — **editados** (não recriados).
- `FinalHudGuardValidator` — mantido.

## Scope executed

- Controller: `public static Instance`; `float[] _slotCooldownTotals`; `GetSlotCooldownRemaining/Total`; `public void TryUseSlot(int)` (teclas 1-4 já chamam o mesmo caminho).
- `ActiveSkillSlotProjection` (C# puro): calcula `IsUsableInContext`/`BlockedReason`/`Cooldown` a partir de (equipped, cooldownRemaining).
- Binder `RefreshActiveSkillSlots`: 4 slots (teclas 1-4), lê cooldown do controller, projeta estado (corrige rótulo legado "R/T/Y/G").
- View: arrays serializados (`Button[]`, `Image[]` fills, `GameObject[]` overlays); `WireButtons` (clique → `TryUseSlot`); Refresh seta fillAmount, overlay e interactable. Null-safe headless.

## Out of scope respected

Não tocado: catálogo (fable_70), save schema, Packages/ProjectSettings, gamepad, rebind, cooldown persistente entre cenas, animação de cast/charge. Custo na HUD (NoMp/NoStamina) deferido.

## Files changed (fable_71)

```
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs (exposição read-only)
Assets/_Game/Scripts/UI/HUD/ActiveSkillSlotProjection.cs                       (NOVO)
Assets/_Game/Scripts/UI/HUD/GameplayHudRuntimeBinder.cs                        (fill completo)
Assets/_Game/Scripts/UI/HUD/Views/ActiveSkillSlotsHudView.cs                   (botão + cooldown + bloqueio)
Assets/_Game/Tests/EditMode/UI/ActiveSkillSlotProjectionTests.cs               (NOVO)
docs/validation/fable_71_execution_report.md                                    (NOVO)
```

## Spec Compliance Matrix

| Spec Requirement | Implementation Evidence | Status | Notes |
|---|---|---|---|
| Expor cooldown do controller | GetSlotCooldownRemaining/Total | OK | totals capturados no disparo |
| Binder preenche VM | RefreshActiveSkillSlots + Projection | OK | custo (CostMp/CostStamina) não preenchido (DEFERRED) |
| Clique → uso (converge teclas) | View.WireButtons → TryUseSlot | OK | ponto único de uso |
| Cooldown fill + bloqueio | View.Refresh fillAmount/overlay | OK_WITH_WARNINGS | visual depende do wiring do canvas (Unity) |
| Guard respeitado | FinalHudGuardValidator | OK | — |
| Projeção testável | ActiveSkillSlotProjectionTests (3 casos) | OK | EditMode (Test Runner deferido ao lote) |

## Validation

```text
Validation method: dotnet build direto (ambos assemblies) + run_strict_validation.ps1
Assembly-CSharp: PASS (0 errors)
Assembly-CSharp-Editor: PASS (0 errors)
Docs validation: PASS
Quality check: FAIL — EXPECTED_FAIL_LEGACY_ONLY (reports de specs antigas; nenhum cita fable_71)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: todo o código compila (0 erros) e a lógica de projeção tem EditMode tests; a parte visual (fill/overlay/botões aparecendo) só pode ser confirmada em **PlayMode após o wiring do canvas** no Unity Editor — fora do que posso executar aqui. Custo na HUD ficou deferido porque o valor de custo vive privado em cada executor (expô-lo é um follow-up pequeno).

## Remaining work

1. **Wiring do canvas** (Unity Editor): ligar `_slotButtons`/`_cooldownFills`/`_blockedOverlays` aos objetos de UI na `GameplayHudCanvas` (4 slots).
2. **Custo na HUD** (NoMp/NoStamina): expor custo dos executores e preencher `CostMp`/`CostStamina` + bloqueio por recurso.
3. **PlayMode scenario**: usar skill → ver fill decrescer, clicar no slot, ver bloqueio quando vazio/cooldown.
