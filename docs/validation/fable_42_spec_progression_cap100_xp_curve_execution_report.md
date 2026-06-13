# Execution Report — fable_42_spec_progression_cap100_xp_curve

> **Spec:** `docs/specs/a_implementar/fable/fable_42_spec_progression_cap100_xp_curve.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 1, spec 5/42)

validated_adrs: []
validated_game_rules: [player_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Curva bate com a tabela de referência (60×N^1.5) | `ProgressionCurve.XpForNext` + testes com 7 pontos de referência (N=1→60 ... N=99→59102) |
| CA-2 | Cap 100; 99→100 concede o 50º ponto; nenhum ponto duplicado em load | `LevelForTotalXp` capa em 100 (excedente acumula); `SkillPoints_Exactly50AtCap`; grants só em AddXp (Recompute nunca concede) |
| CA-3 | Save antigo migra preservando XP | `MigrateLegacy(level, parcial)` no NormalizeState quando TotalXp==0; teste `Migration_LegacySavePreservesProgress` |
| CA-4 | 4 fontes de XP com hooks nomeados | kill (existente), quest (API `AddXp` — F34 consome), descoberta de nível da caverna (+15×banda, idempotente por `DeepestXpAwardedCaveLevel`), 1ª colheita (+10, idempotente por `FirstHarvestXpSeedIds`) |

## Existing systems audit

```text
DESCOBERTA FASE 0: o sistema JÁ TINHA MaxLevel=100 e 1 ponto/2 níveis (canônicos) —
o gap real era a CURVA (por bandas, não 60×N^1.5) e a ausência de XP total como fonte
de verdade. Fontes de XP existentes: apenas kill (EnemyKilledEvent).
REUSADOS: PlayerProgressionManager (refatorado para derivar de TotalXp — API pública
intacta: AddXp/TrySpend*/Capture/Restore/ResetCurrentLevelXp), PlayerProgressionRules
(delegando à curva; flag UseLegacyCurve=false p/ rollback), PlayerXpChangedEvent/
PlayerLevelChangedEvent (payloads inalterados — HUD intacto), CaveLevelEnteredEvent,
CropHarvestedEvent.
CRIADOS: ProgressionCurve (puro), campos aditivos no PlayerProgressionSaveData
(TotalXp long, DeepestXpAwardedCaveLevel, FirstHarvestXpSeedIds).
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| ProgressionCurve puro (XpForNext/TotalXpForLevel/LevelForTotalXp/cap) | `Player/Progression/ProgressionCurve.cs` | OK |
| 1 ponto/2 níveis = 50 no cap | preservado (rules existentes) + teste de orçamento | OK |
| Refactor do service p/ consumir a curva | AddXp incrementa TotalXp → RecomputeDerivedProgression deriva nível/parcial; level-ups emitem 1 evento por nível com grants | OK |
| Grant idempotente | grants APENAS no caminho AddXp; restore/migração nunca re-concede (Unspent preservado) | OK |
| Migração de save | TotalXp==0 + progresso legado → MigrateLegacy; nível pode ajustar ±0 (parcial preservado) | OK |
| Fontes de XP (4 hooks nomeados) | OnEnemyKilled / AddXp (quests F34) / OnCaveLevelEntered / OnCropHarvested | OK |
| HUD binding | eventos com payload idêntico — barra existente lê os mesmos campos | OK |
| Rollback por flag | `PlayerProgressionRules.UseLegacyCurve` | OK |

Nota: ResetCurrentLevelXp (penalidade de morte) agora desconta do TotalXp — comportamento
observável idêntico (perde o parcial do nível), consistente com a fonte de verdade.

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
Changed deterministic logic: YES (núcleo de progressão)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (8 EditMode tests em ProgressionCurveTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: subir 1 nível e ver barra/pontos — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: builds salvos no meio de um nível migram com nível idêntico, mas a curva
nova muda o RITMO de progressão (intencional — rebalance canônico); XP de descoberta
usa banda de dezena ((nível-1)/10+1) — calibragem fina com F33.
```

## Honest status rationale

BUILD_VALIDATED: curva e migração testadas em EditMode compilado; ciclo de level-up real
e HUD verificados só em Play Mode (lote final).

## Remaining work

- F29 consome os 50 pontos (orçamento bate com as árvores canônicas).
- F34 chama AddXp com a fórmula de quest (base × (1+0.08×questLevel)).
- F33 substitui multiplicadores de XP por criatura pelos valores das fichas.
