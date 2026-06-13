# Execution Report — fable_16_spec_player_fatigue_sleep_collapse_wiring

> **Spec:** `.specs/a_implementar/fable/fable_16_spec_player_fatigue_sleep_collapse_wiring.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 0, spec 2/42)

validated_adrs: []
validated_game_rules: [player_rules.md, time_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Fadiga sobe por hora acordado e por ações; thresholds com efeitos observáveis | `PlayerConditionService.OnGameTimeTick` (2 fadiga/h via FatigueSystem) + `OnStaminaChanged` (10% do gasto); VeryTired+ → SpeedMultiplier ×0.85; Tired+ → feedback. Testes `FatigueSystem_*`, `FatigueThresholds_Mapping` |
| CA-2 | Colapso 02:00 força novo dia com penalidade; dormir voluntário recupera mais | `IsCollapseHour` (noite ≥60% = 02:00) → `Collapse()` (recuperação tardia + residual 25 + fade 1s + AdvanceDay); `SleepInBed()` plena. Testes `IsCollapseHour_*`, `SleepRecovery_GoodSleep_RecoversMoreThanLateSleep`, `CollapseRecovery_LeavesResidualFatigue` |
| CA-3 | Fadiga persiste; save antigo carrega com 0 | `PlayerSaveData.Fatigue` (float aditivo) + capture/restore no SaveManager via `PlayerConditionService.Instance` |

## Existing systems audit

```text
REUSADOS SEM REESCRITA (módulos órfãos WAVE 05): FatigueSystem, FatigueState,
FatigueThreshold(+extensions), FatigueGainContext, SleepRecoveryCalculator.
REUSADOS DE APOIO: GameTimeManager (fases Day/Night — ganhou 2 propriedades read-only
aditivas CurrentPhaseDurationSeconds/CurrentPhaseNormalized para o mapeamento de hora),
TimeManager.AdvanceDay (caminho ÚNICO de day transition — dormir/colapso usam o mesmo),
StaminaChangedEvent, GameTimeTickEvent, GamePhaseChangedEvent, PlayerController.SpeedMultiplier,
SaveManager/PlayerSaveData, padrão bootstrap, DayAdvanceInput (mantido, debug).
CRIADOS: PlayerConditionService (+bootstrap), PlayerConditionEvents
(PlayerFatigueChangedEvent/PlayerCollapsedEvent), BedInteractable, cama no CreateMvpFarmScene.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| Host bootstrap com tick por hora + ganho por ação | `Player/Conditions/PlayerConditionService.cs` (tick via GameTimeTickEvent; relógio Dia 06-20/Noite 20-06) | OK |
| Thresholds: feedback / speed ×0.85 / stamina máx reduzida | Tired+ feedback; VeryTired+ speed ×0.85 composto (floor 0.5). Stamina máx: StaminaManager NÃO expõe API de redução temporária — efeito substituído por penalidade de velocidade no Exhausted (mesmo tier), documentado como adaptação | OK (adaptado) |
| Colapso 02:00 com fade + penalidade + acordar | `Collapse()` — fade OnGUI 1s, recuperação WentToBedLate + residual 25, AdvanceDay | OK |
| BedInteractable na FarmScene com confirmação | `World/BedInteractable.cs` (2-toque, janela 4s) + `CreateMvpFarmScene.CreateFarmBed` em Zone_HouseEntrance | OK |
| Persistência (campo aditivo) | `PlayerSaveData.Fatigue` + SaveManager capture/restore | OK |
| Eventos + unsubscribe | 2 eventos novos; OnDisable cancela todas as inscrições | OK |
| EditMode tests | `Assets/_Game/Tests/EditMode/Player/FatigueWiringTests.cs` (11 testes) | OK |
| Modal guard no colapso | GameTimeManager pausa ticks com modal ativo → colapso adia naturalmente (sem código novo; documentado) | OK |

Adaptação documentada: o campo `Fatigue` pré-existente no SaveData (linha 225) é de
COMPANION (CompanionSaveEntry), não do player — confirmado na Fase 0; player ganhou campo próprio.

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS (zero erros)
Assembly-CSharp: PASS (0 erros)
Assembly-CSharp-Editor: PASS (0 erros)
Quality check: PASS
Diff completeness: PASS (WARNs = reports legados pré-existentes)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: via gerador editor apenas
Automated tests added/updated: YES (11 EditMode tests)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: varar a noite até colapsar + dormir voluntário — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: FarmScene precisa ser regenerada para materializar a cama; interação
SpeedMultiplier com block/dash usa multiplicação composta (floor 0.5) — verificar no Play Mode.
```

## Honest status rationale

BUILD_VALIDATED apenas: builds 0E e helpers puros testados, mas Unity Editor não foi aberto
(cena não regenerada, Test Runner não executado) e o feel do colapso/fade é validação humana
do lote final.

## Remaining work

- Regenerar FarmScene (cama + RainIrrigation da F15 juntos).
- F19: cama da estalagem na cidade (hook previsto).
- F01 (status): compor lentidão de Chill com fadiga (floor 0.5 já definido aqui).
