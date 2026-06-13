# Execution Report — fable_13_spec_save_debt_closure_runtime

> **Spec:** `.specs/a_implementar/fable/fable_13_spec_save_debt_closure_runtime.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 1, spec 4/42)

validated_adrs: [ADR-0005]
validated_game_rules: [save_rules.md, cave_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Run sobrevive a fechar o jogo (mesmo nível/RunSeed/bosses/checkpoints) | `CaveRunSaveData` + `CaveRunSaveMapper` + capture/restore no SaveManager via cache do bootstrap; teste `Mapper_RoundTrip_PreservesRunIdentity` |
| CA-2 | HP persistente no nível corrente; morto não rematerializa | `EnemyHpRecord` no snapshot + `CollectEnemyHpRecords`/`RestoreHp` no materializer (skip se HP≤0); refresh em TODA saída de portal e no save; testes `Snapshot_SetEnemyHpRecords_FiltersInvalid` |
| CA-3 | Daily goals wired | `CaptureFarmDailyGoalsSaveData` + restore chamando os métodos WAVE 24 existentes via `FarmDailyGoalService.Instance` |
| CA-4 | Save v5 sem seções carrega com defaults | seções aditivas null-safe; testes `Mapper_NoActiveRun_*`, `LegacySave_WithoutCaveRunSection_LoadsWithDefaults` |

## Existing systems audit

```text
REUSADOS: SaveManager (padrão WI-18 — 2 capturas + 2 restores adicionados, zero reescrita),
GameSaveData.DailyGoals (campo WAVE 24 órfão — agora atribuído), CaveRuntimeState,
VisitedLevelSnapshot/CaveSnapshotService (replay hash INTACTO — HP fora do hash),
CaveRunManager.State + GameBootstrap.SetCachedCaveRunState (canal único cena↔save),
CaveRuntimeMaterializer (plan-based materialization), EnemyHealth, FarmDailyGoalService
(Instance/Capture/Restore prontos desde WAVE 24).
CRIADOS: CaveRunSaveData/CaveBossDefeatSaveRecord/CaveRunSaveMapper, EnemyHpRecord,
EnemyHealth.RestoreHp (guard contra Start reset), RefreshCurrentSnapshotEnemyHp (controller),
RefreshSnapshotEnemyHpBeforeTransition (portal).
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| CaveRunSaveData espelho 1:1 + mapper testável | `Cave/Runtime/CaveRunSaveData.cs` (HashSet/Dictionary ↔ listas ordenadas determinísticas) | OK |
| Capture/Restore padrão WI-18 com pending data | restore grava no cache do bootstrap; CaveRunManager consome via TakeCachedCaveRunState ao entrar na caverna | OK |
| HP por instância no snapshot (aditivo) | `EnemyHpRecords` em VisitedLevelSnapshot; FORA do LayoutHash (teste `..._DoNotAffectLayoutHash` garante replay/stable-run) | OK |
| Materializer grava ao sair e aplica ao rematerializar | grava: portal Interact + save; aplica: skip HP≤0, RestoreHp após Configure (guard no Start) | OK |
| Snapshot apenas do nível CORRENTE | mapper inclui só `state.CurrentLevel` (limite documentado; multi-nível = 16_spec futura); teste `Mapper_IncludesOnlyCurrentLevelSnapshot` | OK |
| Wiring FarmDailyGoals | capture+restore ligados (DTO/methods WAVE 24 reusados) | OK |
| Restore order | CaveRun restaurado APÓS Player/World no fluxo existente (fim do RestoreFromSaveData); materialização só ocorre ao entrar na cena de caverna | OK |
| Sem bump de SchemaVersion | campos aditivos com default (JsonUtility ignora ausentes) — validado por teste de legado | OK |
| GenerationVersion p/ diagnóstico | string "fable_13" no DTO (não-gate) | OK |

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
Changed deterministic logic: YES (mappers/snapshot)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (7 EditMode tests em SaveDebtClosureTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: salvar no nível 8, fechar Editor, reabrir e retomar — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: aplicação de HP no fluxo real (Configure→RestoreHp→Start) depende do ciclo
Unity — coberto por guard testável mas confirmado só em Play Mode; snapshot do nível
corrente pode ser grande (walkable tiles) — tamanho de save monitorar no cenário humano.
```

## Honest status rationale

BUILD_VALIDATED: mappers e contratos testados em EditMode compilado; o ciclo completo
(fechar/reabrir Editor) é exatamente o que o Play Mode final valida. Sem claim além disso.

## Remaining work

- Cenário humano: save no meio da run + reload (lote final).
- 16_spec futura: snapshot multi-nível no save + política de save em boss.
- Backlog WAVE 22: marcar CAVE_ENEMY_HP_SAVE_DEBT / SAVE_LOAD_DAILY_GOAL_DEBT /
  CAVE_RUN_SAVE_LOAD_DEBT como fechados por esta spec.
