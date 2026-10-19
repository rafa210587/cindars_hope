---
doc_type: execution_report
spec: fable_65_spec_daily_goals_closeout_reward_hud
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-21
validated_adrs: [ADR-0007-event-bus-gameplay-communication.md, ADR-0006-save-data-contracts-simple-dtos.md]
validated_game_rules: [farm_rules.md, save_rules.md, event_rules.md]
---

# Execution Report — fable_65 Daily Goals: Recompensa + HUD + Catálogo 4-6 metas

**Status:** BUILD_VALIDATED_WITH_WARNINGS (Phase 2-3 Play Mode DEFERRED_TO_FINAL_VALIDATION,
autorizado pelo dono). Single session; Unity Editor/Play Mode não executados.

## Acceptance criteria extracted

| CA | Critério | Implementação | Evidência |
|----|----------|---------------|-----------|
| CA-1 | Recompensa idempotente (paga 1x, reload não re-paga, reset re-habilita) | `FarmDailyGoalService.TryClaimReward` (estático, puro) — paga só se `Completed && !Claimed && table.TryGet`; `Claimed=true` ao pagar; reset diário limpa Claimed | 6 testes idempotência + reset |
| CA-2 | Widget no HUD read-only, atualizado por eventos, respeita visibilidade | `DailyGoalsHudView` (padrão WI-23, headless) + `DailyGoalsHudProjection` (puro); registrado no `GameplayHudCanvasController`; assina `HudVisibilityChangedEvent` | 2 testes projeção + cenário humano |
| CA-3 | Catálogo 4-6 metas, eventos reais, n/m correto | `FarmDailyGoalCatalog` (6 metas); cada uma com evento real (CropHarvested/Economy/SeedPlanted/TreeChopped/NpcInteractionStarted); RequiredProgress 1 e 3 | testes integridade catálogo + n/m |
| CA-4 | Débitos WI-24 fechados/provados | SAVE_LOAD_DAILY_GOAL_DEBT provado fechado por F13 (SaveManager.cs já chama Capture/Restore); DAILY_GOAL_EDITMODE_TESTS fechado aqui; matriz anotada | matriz WI-24 + suite de testes |
| CA-5 | Economia contida + tuning pendente | Tabela única `FarmDailyGoalRewardTable`; total diário máx **90 ouro + 53 XP** documentado como tuning humano pendente; pagamento só com !Claimed | teste `MaxDailyTotals` + matriz |

## Existing systems audit

| Sistema | Encontrado? | Decisão |
|---------|-------------|---------|
| FarmDailyGoalService (MonoBehaviour, estado/reset/eventos/Capture-Restore, campo `Claimed` órfão) | SIM | ESTENDIDO — gancho `Claimed` agora paga via `TryClaimReward`; catálogo movido p/ `FarmDailyGoalCatalog`; +4 metas/eventos |
| DailyGoalProgressedEvent / DailyGoalCompletedEvent | SIM | REUSADOS (gatilhos prontos; nenhum evento novo — `Adds events: NO`) |
| PlayerProgressionManager.AddXp | SIM | CONSUMIDO via sink (curva F42 intacta) |
| PlayerManager.AddGold | SIM | CONSUMIDO via sink (canal único de ouro) |
| GameBootstrap.Instance (PlayerManager/PlayerProgressionManager) | SIM | injeção do sink de produção sem global search (idiom do projeto) |
| GameplayHudCanvasController + views WI-23 (QuestTrackerHudView) | SIM | view nova registrada de forma ADITIVA; views existentes intactas |
| HudVisibilityController / HudVisibilityChangedEvent | SIM | REUSADO (widget some com o HUD) |
| FarmLoopFeedbackBridge | SIM | toast genérico de conclusão REMOVIDO (substituído pelo toast rico de recompensa — sem duplicação) |
| SaveManager (DailyGoals capture/restore) | SIM | JÁ ligado (F13) — SAVE_LOAD_DAILY_GOAL_DEBT provado fechado; nenhuma mudança necessária |
| CropWateredEvent | NÃO (só em validator) | meta "regar N" fica FORA da v1 (meta futura) — sem evento real |

Nenhum segundo serviço/canal/widget criado. Reward table em ponto único.

## Spec Compliance Matrix

| Requisito da spec | Implementação | OK |
|-------------------|---------------|----|
| Recompensa no DailyGoalCompletedEvent, dentro do serviço, ponto único | `TryClaimReward` chamado em `AddProgress` na conclusão | OK |
| Tabela única {goalId → ouro, XP} | `FarmDailyGoalRewardTable` | OK |
| Ouro via canal existente; XP via AddXp | `GameBootstrapDailyGoalRewardSink` → AddGold/AddXp | OK |
| Claimed = true ao pagar; só se !Claimed | guarda em `TryClaimReward` | OK |
| Toast de recompensa pelo canal WI-23 | `PlayerActionFeedbackEvent("Meta concluída: +X ouro, +Y XP")` | OK |
| DailyGoalsHudView (padrão WI-23, read-only, n/m, check, HudVisibility) | `DailyGoalsHudView` + `DailyGoalsHudProjection` | OK |
| Catálogo 4-6 metas, só eventos reais, RequiredProgress > 1 | `FarmDailyGoalCatalog` (6 metas; 2 com Required 3) | OK |
| SAVE_LOAD_DAILY_GOAL_DEBT fechado/provado | provado fechado por F13 (SaveManager linhas 200/571/1151) | OK |
| DAILY_GOAL_EDITMODE_TESTS fechado | `FarmDailyGoalServiceTests.cs` (18 testes) | OK |
| Save aditivo, sem refs Unity, Claimed já no DTO | `FarmDailyGoalState`/`FarmDailyGoalsSaveData` inalterados (Claimed já existia) | OK |
| Sem GameObject.Find runtime; só GameEventBus | sink via GameBootstrap.Instance; eventos via bus | OK |
| ids originais nunca renomeados | consts apontam para os ids WI-24 idênticos | OK |
| Total diário documentado + tuning humano | 90 ouro / 53 XP na matriz | OK |

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 0 (após criação do execution report; falha anterior era apenas diff-completeness por report ausente)
Docs validation: PASS (validate_docs.ps1 exit 0)
Assembly-CSharp: PASS (0 erros; 1 warning pré-existente CombatTelemetrySession, não relacionado)
Assembly-CSharp-Editor: PASS (0 erros; warnings pré-existentes)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

EditMode tests: `Assets/_Game/Tests/EditMode/Farm/FarmDailyGoalServiceTests.cs` — 18 testes
(idempotência ×6, integridade catálogo/tabela ×6, reset diário, n/m, round-trip de save, projeção ×2,
empty state). NÃO executados via Unity Test Runner (Play Mode/Unity deferido); compilam em
Assembly-CSharp (0E). Risco residual: execução EditMode pendente no Unity (lote final).

## Honest status rationale

BUILD_VALIDATED_WITH_WARNINGS: núcleo determinista pronto e compilado (0E nos dois assemblies),
docs PASS, diff completeness PASS, débitos WI-24 fechados/provados. Phase 2-3 (Play Mode +
execução EditMode no Unity Test Runner + wiring visual dos labels do widget) deferida ao lote final
de validação humana, conforme autorização do dono. Não é ACCEPTED — não há evidência de Play Mode.

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (pagamento idempotente, catálogo, reset diário, projeção, round-trip)
Changed Unity scene/prefab/asset wiring: NO (widget registrado por código; labels visuais headless)
Automated tests added/updated: YES (FarmDailyGoalServiceTests.cs — 18 testes EditMode)
Automated tests command: Unity Test Runner EditMode (NOT RUN — Unity deferido); compile via dotnet build (PASS 0E)
Manual Play Mode scenario: docs/validation/playmode/fable_65_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: execução EditMode no Unity Test Runner e wiring visual do widget pendentes (lote final);
  valores de recompensa são v1 conservadores (tuning humano pendente).
```

## Files changed

Runtime (novos):
- `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalCatalog.cs`
- `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalRewardTable.cs`
- `Assets/_Game/Scripts/Farm/Runtime/IDailyGoalRewardSink.cs`
- `Assets/_Game/Scripts/Farm/Runtime/GameBootstrapDailyGoalRewardSink.cs`
- `Assets/_Game/Scripts/UI/HUD/Views/DailyGoalsHudProjection.cs`
- `Assets/_Game/Scripts/UI/HUD/Views/DailyGoalsHudView.cs`

Runtime (estendidos):
- `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs` (recompensa + catálogo + sink)
- `Assets/_Game/Scripts/Farm/Runtime/FarmLoopFeedbackBridge.cs` (toast de conclusão movido)
- `Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs` (registro aditivo da view)

Tests (novo):
- `Assets/_Game/Tests/EditMode/Farm/FarmDailyGoalServiceTests.cs`

Docs:
- `docs/validation/WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md` (débitos anotados/fechados + catálogo)
- `docs/validation/playmode/fable_65_human_test_scenario.md` (cenário humano)
- `docs/validation/fable_65_spec_daily_goals_closeout_reward_hud_execution_report.md` (este)

csproj: `Assembly-CSharp.csproj` (includes dos 6 runtime + 1 test) — NÃO commitado (gitignored/política).

## Remaining work

- Unity Test Runner EditMode (18 testes) + Play Mode (cenário humano) no lote final.
- Wiring visual dos labels do widget no GameplayHudCanvas (headless, como as demais views WI-23).
- Tuning humano dos valores de recompensa (v1: 90 ouro / 53 XP máx/dia).
- Meta futura "regar N plots" quando `CropWateredEvent` existir como evento runtime.
