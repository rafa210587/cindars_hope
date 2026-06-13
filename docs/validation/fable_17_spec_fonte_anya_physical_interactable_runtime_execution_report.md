# Execution Report — fable_17_spec_fonte_anya_physical_interactable_runtime

> **Spec:** `docs/specs/a_implementar/fable/fable_17_spec_fonte_anya_physical_interactable_runtime.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 0, spec 3/42)

validated_adrs: []
validated_game_rules: [fonte_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Fonte física; menu honesto (Respawn sempre; Água Viva por fragmento; seladas = "???") | `CreateMvpFarmScene.CreateFonteAnya` (bacia+água+pedestal em (-3.5,-7)) + `FonteInteractable` IMGUI; testes `ReturnPoint_UnlocksWithoutFragment`, `LivingWater_RequiresWaterFragment`, `Respec_RequiresMemoryFragment` |
| CA-2 | Morte → respawn na Fonte | Fluxo já existia (DeathSystemBootstrap→AnyaRespawnService) mas era inerte: GameBootstrap._anyaFountain era null. Gerador agora cria AnyaFountain e WIRA o campo via SerializedObject — caminho completo |
| CA-3 | Água Viva 1×/dia idempotente; consumo restaura HP + reduz fadiga | `FonteRuntimeService.TryCollectLivingWater` (CanGrantToday + cargas) + recarga +1/dia; `ConsumableManager.ApplyLivingWaterEffects` (HP total, fadiga −40 via F16); testes `DailyGrant_IsIdempotentPerDay`, `UseRequest_*` |

## Existing systems audit

```text
DESCOBERTA DA FASE 0 (corrige premissa da spec): a infraestrutura de respawn JÁ EXISTIA —
AnyaFountain (marker), AnyaFountainInteractable/Menu (Canvas, sem prefab — permanece
caminho futuro F14), AnyaRespawnService (HP/stamina/mana + reposição), DeathSystemBootstrap
(consome PlayerDiedEvent; cave KO intacto), DeathScreenController. O elo faltante era o
OBJETO físico na cena (bootstrap._anyaFountain null → serviço nunca inicializava).
REUSADOS SEM REESCRITA: FonteAnyaSection/FonteFunctionUnlockService (WAVE 10),
MainProgressionSection/MainProgressionService (ordem de fragmentos), SkillTreeManager
(TryRespec/GetRespecCost — mesmo código do AnyaFountainMenu), ConsumableManager, ItemDataInitializer.
CRIADOS: FonteRuntimeService (+bootstrap, hospeda seção+progressão), FonteInteractable
(menu IMGUI honesto), FonteSaveData (seção aditiva), item Água Viva, visual no gerador.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| Visual composto + interactable no gerador | `CreateFonteAnya` (3 partes + collider + AnyaFountain + FonteInteractable + wiring bootstrap) | OK |
| Menu modal honesto (lock/unlock; seladas "???") | `FonteInteractable` IMGUI (decisão Fase 0: F14 não executada → padrão IMGUI dos painéis atuais) | OK |
| Respawn na Fonte | Wiring do `_anyaFountain` completa o fluxo existente (zero código novo de respawn — não duplicado) | OK |
| Água Viva 1×/dia + item | `TryCollectLivingWater` + `item_consumable_agua_viva` (BV 0 anti-exploit, canon) + efeito especial no ConsumableManager | OK |
| Respec se desbloqueado | Botão gated por `Respec.Unlocked` (Fragmento da Memória); usa SkillTreeManager.TryRespec existente | OK |
| Persistência | `FonteSaveData` aditiva no GameSaveData + capture/restore (saves legados = Fonte dormante) | OK |
| EditMode tests | `Assets/_Game/Tests/EditMode/Fonte/FontePhysicalTests.cs` (7 testes) | OK |

Decisões documentadas:
- `FonteMenuViewModel` (WAVE 04 CONTRACT_ONLY) NÃO foi consumido nesta fase: o menu IMGUI
  interino lê o estado direto do serviço; F14 (Canvas) consumirá o viewmodel — anotado.
- Recarga da Água Viva: +1 carga/dia até máx 3 (política limitada da LivingWaterState).
- FonteFunctionUsedEvent previsto na spec foi coberto por FonteUseRecord (estado WAVE 10)
  + PlayerActionFeedbackEvent; evento dedicado adiado até existir consumidor (sem evento morto).

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS | Assembly-CSharp: PASS (0E) | Assembly-CSharp-Editor: PASS (0E)
Quality check: PASS | Diff completeness: PASS (WARNs legados)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: via gerador editor apenas
Automated tests added/updated: YES (7 EditMode tests)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: morrer→acordar na Fonte; fragmento→coletar Água Viva — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: FarmScene precisa ser regenerada (Fonte + wiring do bootstrap); fluxo de
respawn depende de PlayerDiedEvent fora da caverna (DeathSystemBootstrap responde, mas só
cria corpse na caverna — respawn overworld validado no Play Mode final).
```

## Honest status rationale

BUILD_VALIDATED: builds 0E, lógica pura testada. Unity Editor não aberto; cena não
regenerada; Play Mode pendente do lote final. Sem claim de respawn validado em jogo.

## Remaining work

- Regenerar FarmScene (Fonte + cama F16 + RainIrrigation F15 numa só regeneração).
- F10: conceder Fragmento da Água via quest chamando `FonteRuntimeService.IntegrateFragment`.
- F14: substituir menu IMGUI por Canvas consumindo FonteMenuViewModel.
