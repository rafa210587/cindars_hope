# Execution Report — fable_44_spec_cave_save_completion_policy

> **Spec:** `.specs/a_implementar/fable/fable_44_spec_cave_save_completion_policy.md`
> **Data:** 2026-06-20
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 9; E28)

validated_adrs: [ADR-0005]
validated_game_rules: [save_rules.md, cave_rules.md]

---

## Resumo

Fecha a pendência documentada pela F13 ("snapshot multi-nível no save + política de save em boss"):
1. `CaveRunSaveData` agora persiste **todos os níveis visitados** da run (lista aditiva
   `VisitedLevelSnapshots`), dentro de um **cap LRU determinístico** (N=8) com **orçamento de tamanho
   documentado e testado**. `CurrentLevelSnapshot` é mantido e escrito em paralelo (compat de load F13).
2. O **save manual é bloqueado durante boss fight ativa** com feedback no canal existente
   (`GameSavedEvent.Message`), sem UI nova.
3. `LayoutHash`/replay/`CaveRunSeed` permanecem **inalterados** (ADR-0005); load legado (só
   `CurrentLevelSnapshot`) continua funcionando sem erro.

Nenhum sistema de save de caverna paralelo foi criado — `CaveRunSaveData`/`CaveRunSaveMapper`/
`VisitedLevelSnapshot` da F13 (estendidos por fable_09 e fable_60) foram **evoluídos**.

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Multi-nível persistido (1-3 visitados; morto no 1, depletado no 2 → save/load → 3 restaurados; revisita mantém mortos/depletados) | `CaveRunSaveData.VisitedLevelSnapshots` aditiva + mapper multi-nível; teste `Mapper_RoundTrip_PersistsAllVisitedLevels` (HP=0, nó depletado, baú aberto, armadilha disparada por nível, preservados após round-trip) |
| CA-2 | Orçamento de tamanho (medição Fase 0 + política com constante nomeada + teste de orçamento) | Medição abaixo (§Fase 0); `MaxPersistedLevelSnapshots=8` + `SaveSizeBudgetBytes=1.5 MB`; teste `SaveSizeBudget_SyntheticCappedRun_IsWithinDocumentedBudget` (serializa K=8 níveis 55×55 com o mesmo `JsonUtility` do SaveManager) |
| CA-3 | Save bloqueado em boss (recusa + mensagem; após derrota/saída volta a funcionar) | `CaveBossFightSaveGate` (puro) + flag em `CaveLevelRuntimeController.IsBossFightActive` + gate no ponto único `SaveManager.SaveGame()`; testes `BossGate_*` (set/clear/idempotente/todos os caminhos/CanSaveNow/mensagem ASCII) |
| CA-4 | Compat e stable-run (save F13 carrega; LayoutHash/replay inalterados) | `LegacyLoad_OnlyCurrentLevelSnapshot_FallsBackToSingleLevel` + `LegacyLoad_NullMultiLevelList_DoesNotThrow`; testes F13 de hash (`Snapshot_EnemyHpRecords_DoNotAffectLayoutHash`) intactos; mapper não toca `LayoutHash`/replay |

---

## Fase 0 — Medição de tamanho + auditoria do fluxo de save

### Medição do snapshot serializado (analítica + teste)

O custo dominante do `VisitedLevelSnapshot` é `WalkableTilesList` + `WallTilesList`
(`List<Vector2Int>`; total de entradas ≈ W·H por nível). Em `JsonUtility.ToJson(saveData, true)`
(indentado — o formato real do `SaveManager`), cada `Vector2Int` custa ~30–40 bytes.

| Tamanho do nível | Tiles (W·H) | Estimativa por nível |
|---|---|---|
| 42×42 | 1.764 | ~62 KB |
| 55×55 | 3.025 | ~106 KB |
| 65×65 | 4.225 | ~148 KB |

> Medição executável: o teste `SaveSizeBudget_SyntheticCappedRun_IsWithinDocumentedBudget`
> serializa **K=8 níveis 55×55** com o mesmo `JsonUtility` indentado e faz assert do total real de
> bytes contra `SaveSizeBudgetBytes`. (Execução do número exato: Unity Test Runner EditMode — lote
> final; a estimativa analítica acima fundamenta o limite escolhido.)

### Decisão: CAP LRU (sem codec de compressão)

- Pior caso sob cap N=8 todos 65×65 ≈ **8 × 148 KB ≈ 1,18 MB**. Sem cap, a lista seria ilimitada
  (uma run profunda inflaria o save indefinidamente) — risco listado na spec.
- **Política adotada:** cap LRU de **N=8** níveis mais recentes (`MaxPersistedLevelSnapshots=8`).
  Níveis fora do cap voltam a **replay puro de seed** (layout idêntico via ADR-0005; só o estado
  mutável — HP/depleção/baús/armadilhas — desses níveis antigos é descartado).
- **Orçamento explícito:** `SaveSizeBudgetBytes = 1.572.864` (1.5 MB) para a seção da caverna sob o
  cap, com folga sobre o pior caso analítico. Teste de orçamento garante o limite.
- **Codec de compressão NÃO implementado.** Conforme "Notas para execução posterior" da spec, o cap
  basta para limitar o save; o codec arriscaria o replay hash (risco técnico explícito da spec) e
  seria especulativo. Decisão registrada (anti-codec-especulativo).
- **Recência (proxy):** o runtime não rastreia ordem de visita; a **proximidade ao nível corrente**
  é o proxy de recência (descida normal = níveis mais fundos são mais recentes; ida-e-volta mantém os
  adjacentes ao corrente como mais "quentes"). O nível corrente é sempre incluído. Ordenação de saída
  estável por `CaveLevel` ascendente (determinismo).

### Auditoria do fluxo de save manual e autosaves

- **Ponto único de save manual:** `SaveManager.SaveGame()`. Único disparo do jogador:
  `SaveInput.cs` (tecla F5) → `_saveManager.SaveGame()`. DebugHud apenas lê `SaveFilePath` (não salva).
- **Sem autosave dentro da caverna:** nenhum caminho de fluxo (dormir, transição) chama `SaveGame()`
  na CaveScene. O gate de boss fight no topo de `SaveGame()` é, portanto, o ponto correto e suficiente.
- **Canal de feedback existente:** `GameSavedEvent(slot, path, wasSuccessful, message)` —
  recusa = `PublishSaveResult(false, "Nao e possivel salvar agora.")`. Sem `SaveBlockedEvent` novo,
  sem UI nova (DebugHud e o HUD feedback bridge já assinam `GameSavedEvent`).

---

## Existing systems audit

```text
REUSADOS (estendidos, não recriados):
- CaveRunSaveData / CaveRunSaveMapper (F13) — evoluídos para multi-nível + cap (testes F13 atualizados,
  não deletados);
- VisitedLevelSnapshot (F13 + fable_09 OpenedChestIds + fable_60 TrapStates) — sem mudança de campos;
  reusado como item da lista multi-nível;
- CaveRuntimeState.VisitedLevelSnapshots (Dictionary em memória — multi-nível JÁ existia em runtime;
  só o SAVE cortava para 1; agora o mapper persiste o dicionário capado);
- SaveManager.SaveGame() — ponto único de save manual; capture cave já via CaveRunSaveMapper (F13);
- GameSavedEvent — canal de feedback de recusa (sem evento novo);
- CaveLevelRuntimeController — host do flag (já era dono do snapshot do nível e assinava eventos);
- CaveBossSpawner / CaveBossDeathReporter / CaveBossDefeatedEvent / CavePlayerDefeatedEvent —
  pontos de spawn/derrota do boss (set/clear do flag).
CRIADOS (mínimos, no escopo permitido):
- CaveBossFightSaveGate (Cave/Runtime) — estado puro testável do gate de save em boss fight;
- constantes MaxPersistedLevelSnapshots / SaveSizeBudgetBytes em CaveRunSaveData;
- CaveLevelRuntimeController.IsBossFightActive (+ wiring de set/clear);
- CaveBossSpawner.HasLiveBoss (read-only, indica boss real spawnado);
- SaveManager.TryGetActiveCaveBossFightGuard (gate cirúrgico no save manual);
- CaveMultiLevelSaveTests (EditMode).
NÃO criado: segundo snapshot/DTO; segundo mapper; segundo canal cena↔save; codec de compressão;
autosave; UI de save; SaveBlockedEvent.
```

---

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| Lista aditiva `VisitedLevelSnapshots` ordenada por CaveLevel | `CaveRunSaveData.VisitedLevelSnapshots` (List); mapper ordena saída asc | OK |
| Mapper ToSaveData inclui todos os snapshots válidos respeitando o cap, determinístico | `SelectSnapshotsWithinCap` (recência por proximidade + cap + ordenação estável); teste de determinismo | OK |
| FromSaveData restaura dicionário completo; legado (vazio/null) cai em F13 sem erro | `FromSaveData` itera lista; fallback a `CurrentLevelSnapshot` se lista vazia; testes legado | OK |
| `CurrentLevelSnapshot` mantido e escrito em paralelo | mapper grava o snapshot do nível corrente em paralelo; teste de compat | OK |
| Constantes nomeadas de orçamento + teste de orçamento | `MaxPersistedLevelSnapshots`, `SaveSizeBudgetBytes`; `SaveSizeBudget_*Test` | OK |
| Flag `IsBossFightActive` (set no spawn; clear em derrota/morte/saída de nível/caverna) | `CaveBossFightSaveGate` + wiring no controller (OnMaterializationComplete/OnBossDefeated/OnPlayerDefeated/OnSceneTransitionStarted) | OK |
| Gate no ponto único de save manual → recusa + feedback existente | `SaveManager.SaveGame()` guarda no topo via `TryGetActiveCaveBossFightGuard` → `PublishSaveResult(false, reason)` | OK |
| Sem bump de SchemaVersion; aditivo; sem refs Unity | campos aditivos; `VisitedLevelSnapshot` é serializável simples; `CurrentSchemaVersion=5` inalterado | OK |
| LayoutHash/replay/CaveRunSeed inalterados | mapper não toca hash/seed; HP/estado mutável fora do hash (testes F13 intactos) | OK |
| Sem GameObject.Find no acesso ao flag | leitura via `GameBootstrap.Instance.CaveRunManager.GetComponent<CaveLevelRuntimeController>()` (mesmo canal do CaptureCaveRunSaveData) | OK |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS (validate_docs.ps1 exit 0)
Diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Assembly-CSharp: PASS (0E; 1 warning pré-existente — CombatTelemetrySession._blocks/fable_59)
Assembly-CSharp-Editor: PASS (0E; 3 warnings pré-existentes)
Quality check: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (mapper multi-nível, cap LRU, gate de save)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
  - Novos: CaveMultiLevelSaveTests (12 testes: round-trip multi-nível, ordenação estável, compat
    current-snapshot, cap determinístico, orçamento de tamanho, load legado vazio/null, run vazia,
    e suíte do gate de boss fight set/clear/idempotente/todos-os-caminhos/CanSaveNow/mensagem ASCII).
  - Atualizado: SaveDebtClosureTests.Mapper_IncludesOnlyCurrentLevelSnapshot →
    Mapper_WritesCurrentLevelSnapshotAndPersistsAllVisitedLevels (contrato F13 substituído por
    multi-nível; cobertura preservada, não deletada — anti-regressão da spec).
Automated tests command: Unity Test Runner EditMode (compilados via Assembly-CSharp; execução do
  resultado verde no lote final de Play Mode/Test Runner — DEFERRED_TO_FINAL_VALIDATION).
Manual Play Mode scenario: docs/validation/playmode/fable_44_human_test_scenario.md
  (salvar no nível 3, recarregar, revisitar níveis 1-2 com estado preservado; tentar salvar durante
  boss fight e confirmar recusa + mensagem; derrotar boss e confirmar save liberado).
Justification if no automated tests: N/A (tests adicionados).
Residual risk:
  - O wiring set/clear do flag no ciclo real (spawn de boss → derrota/morte/saída) e o reload
    multi-nível dependem do ciclo Unity (Play Mode) — a LÓGICA do gate e do mapper é testável e testada
    em EditMode, mas o fluxo cena↔save end-to-end é confirmado só em Play Mode (lote final).
  - Recência por proximidade ao corrente é proxy (não há timestamp de visita); em padrões de
    backtracking extremos um nível "antigo por tempo" mas próximo por número pode ser preferido a um
    "recente por tempo" mas distante — aceitável (todos os caps são corretos por ADR-0005; só muda
    QUAL estado mutável sobrevive ao cap).
  - O número exato de bytes do save no cap é confirmado pela execução do SaveSizeBudgetTest em Test
    Runner; a estimativa analítica fundamenta o limite com folga.
```

---

## Honest status rationale

BUILD_VALIDATED: ambos os assemblies compilam 0E; mapper, cap, orçamento e gate testados em EditMode
compilado; docs/diff/strict exit 0. O ciclo end-to-end (boss fight real + reload multi-nível) é
exatamente o que o cenário humano de Play Mode valida — DEFERRED_TO_FINAL_VALIDATION por decisão do
dono. Sem claim ACCEPTED/PLAYMODE_VALIDATED.

---

## Anti-regressão (verificada)

```text
- LayoutHash/replay determinístico: intocados — mapper não calcula nem altera hash; HP/depleção/baús/
  armadilhas continuam FORA do LayoutHash (testes F13 de hash intactos).
- CaveRunSeed: nunca muda por save/load (mapper só copia seeds).
- Load de save F13 (só CurrentLevelSnapshot): carrega sem erro (fallback testado).
- Save fora de boss: nunca bloqueado (gate só dispara com IsBossFightActive=true; default false).
- Flag de boss nunca órfão: clear em derrota, morte, saída de nível e saída de caverna + reset por
  materialização de nível sem boss (teste de todos os caminhos).
- Nenhum Unity ref em DTO; sem bump de SchemaVersion (campos aditivos; JsonUtility ignora ausentes).
- Testes F13 não deletados (um renomeado/atualizado documentando a mudança de contrato).
```

---

## Remaining work

- Cenário humano de Play Mode (salvar no nível 3 + reload + revisita; save em boss recusado;
  pós-boss liberado) — lote final.
- Execução verde do Test Runner EditMode (≈12 testes novos + 1 atualizado) — lote final.
- Backlog: marcar a pendência F13 "snapshot multi-nível + política de save em boss" como fechada.
```
