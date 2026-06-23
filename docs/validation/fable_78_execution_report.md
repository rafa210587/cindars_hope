# Execution Report — fable_78 (Caverna Viva: Povoamento e Ecossistema)

> **Spec:** `fable_78_spec_cave_ecosystem_population_runtime`
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS` — **SLICE 1 de 6** (governança + contratos de dados + 2 planners puros + testes). A spec como um todo **NÃO está completa** e **NÃO** deve ser promovida.
> **Date:** 2026-06-23
> **Branch:** dev

---

## Escopo executado nesta slice

Slice 1 (fundação, sem Unity necessário, build-validatable):
- **Governança (gate da Fase 1):** ADR-0018 (carve-out stable-run para conflito) + ADR-0019 (orçamento de mineráveis por bioma supersede "4-10 resource nodes") + atualização de `cave_rules.md`, `DECISION_LOG.md`, `GAME_RULES_INDEX.md`.
- **Data contracts (C#/SO):** `CaveEnvironmentElementKind`, `CaveEnvironmentElementPlacement`, `CaveEnvironmentElementPlan`, `CaveEcosystemConflictPlan`, `CaveEcosystemBalanceSO`, `CaveEnvironmentElementProfileSO`, `CaveEnvironmentElementDatabaseSO`.
- **Eventos:** `CaveEcosystemConflictStartedEvent`, `EnemyKilledByEnemyEvent`.
- **Planners PUROS:** `CaveEcosystemConflictPlanner` (14.5), `CaveEnvironmentElementPlanner` (14.2).
- **Testes EditMode:** `CaveEcosystemConflictPlannerTests`, `CaveEnvironmentElementPlannerTests`.

Slice 1 NÃO tocou: `EnemyBrain`, `EnemyHealth`, `EnemyPackCoordinator`, `CaveEnemySpawnPlanner`, `CaveBiomeLayoutProfile`, `CaveRuntimeMaterializer`, `VisitedLevelSnapshot`, `CaveSnapshotService`, `CaveSaveData`, `CaveWanderingMerchant`, nenhum `.asset/.unity/.prefab`, `Packages/`, `ProjectSettings/`.

---

## Acceptance criteria (cobertos nesta slice)

| Critério | Cobertura slice 1 | Evidência |
|---|---|---|
| 14.2 Elementos ambientais (determinismo, não-bloqueio do path, pedra/minério presentes) | Lógica do planner pronta + testes | `CaveEnvironmentElementPlanner.cs`, `CaveEnvironmentElementPlannerTests.cs` |
| 14.5 Conflito por entrada (5%/0,5%, espécies distintas, fallback <2, determinismo por entryIndex) | Planner puro pronto + testes | `CaveEcosystemConflictPlanner.cs`, `CaveEcosystemConflictPlannerTests.cs` |
| 14.10 Governança (ADRs G1/G2 + cave_rules) | Feito | ADR-0018, ADR-0019, `cave_rules.md` |
| 14.1 Tamanho por banda | DEFERRED (slice 2 — toca `CaveBiomeLayoutProfile`) | — |
| 14.3 Lagos→aquáticos | DEFERRED (slice 2/3 — toca spawn planner/materializer) | — |
| 14.4 Threat budget | DEFERRED (slice 2 — toca `CaveEnemySpawnPlanner`) | — |
| 14.6 Comportamento de conflito (aggro/dano/ferido/loot reduzido) | DEFERRED (slice 4 — toca `EnemyBrain`/`EnemyHealth`) | — |
| 14.7 Merchant enriquecido | DEFERRED (slice 5) | — |
| 14.8 Persistência/save aditivo | DEFERRED (slice 3 — toca snapshot/save) | — |
| 14.9 Invariantes arquitetura | OK na slice (sem global search; eventos via bus; sem refs Unity em DTO; FNV-1a) | diff + builds |

---

## Existing systems audit (Phase 0)

Reuso confirmado (não recriado): `CaveLayoutStableHash` (FNV-1a) para todo hashing determinístico;
`CaveHazardPlanner.ComputeEntranceExitPathWithBuffer` para a validação BFS de não-bloqueio;
`DataRegistrySO` como base do database SO; padrão de eventos de `Core/Events/`; convenção de balance SO
(`PlayerNeedsBalanceSO`). Nenhum sistema paralelo criado.

---

## Spec Compliance Matrix (slice 1)

| Spec Requirement | Implementation Evidence | Status |
|---|---|---|
| ADR carve-out stable-run | ADR-0018 | OK |
| ADR supersede 4-10 resource nodes | ADR-0019 | OK |
| Balance SO com todos os tunáveis §16.1 | `CaveEcosystemBalanceSO.cs` | OK |
| Conflict planner 5%/0,5% determinístico, espécies distintas, fallback | `CaveEcosystemConflictPlanner.cs` + tests | OK |
| Environment planner determinístico + não-bloqueio | `CaveEnvironmentElementPlanner.cs` + tests | OK |
| Eventos novos additivos | 2 event classes | OK |
| Runtime wiring (EnemyBrain/Health/materializer/save) | — | DEFERRED (slices 2-5) |
| Geradores de editor + data assets | — | DEFERRED (slice posterior; exige Unity) |

---

## Validation

```
Validation method: run_strict_validation.ps1 + builds individuais
Docs validation:            PASS (validate_docs.ps1 exit 0)
Assembly-CSharp:            PASS (exit 0, 0E/0W)
Assembly-CSharp-Editor:     PASS (exit 0, 0E/0W)
Spec diff completeness:     PASS
Spec quality check 1-10:    PASS
run_strict_validation.ps1:  EXIT 1 — APENAS pelo check "forbidden files altered",
                            que sinaliza arquivos .asset (bestiary_*/item_*/etc.) JÁ MODIFICADOS
                            no working tree ANTES desta spec (presentes no snapshot de início de
                            sessão e no preflight). Esta slice NÃO tocou nenhum .asset.
EditMode Test Runner:       NOT RUN (Unity não invocado nesta slice; testes compilam no build).
```

**Honest status rationale:** todos os gates que dependem do diff desta slice passam (builds 0E/0W,
docs, diff-completeness, quality 1-10). O `run_strict_validation` retorna exit 1 unicamente por causa de
modificações `.asset` pré-existentes no working tree, fora do escopo e do diff desta spec — não é uma
falha desta implementação. Por rigor (validation-truth), o status fica em `BUILD_VALIDATED_WITH_WARNINGS`
e não em BUILD_VALIDATED limpo até que o working tree esteja livre do ruído `.asset` pré-existente.

---

## Testing Quality Gate

```
Changed runtime code:           YES (planners puros + DTOs + SOs + eventos)
Changed deterministic logic:    YES (ambos planners)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES (2 suites EditMode)
Automated tests command:        NOT RUN (Unity Test Runner não invocado; compilados no build)
Manual Play Mode scenario:      NOT REQUIRED nesta slice (lógica pura; runtime/materialização = slices futuras)
Justification if no tests:      N/A — testes escritos
Residual risk:                  Testes compilam mas não executados via Test Runner nesta slice;
                                asserts de frequência usam tolerância sobre amostra grande. Execução
                                EditMode deferida a uma fase com Unity disponível.
```

---

## Remaining work (próximas slices)

- **Slice 2:** tamanho por banda (`CaveBiomeLayoutProfile`) + threat budget min/max + densidade + gating aquático (`CaveEnemySpawnPlanner`).
- **Slice 3:** materialização dos elementos (`CaveRuntimeMaterializer`) + persistência aditiva (`VisitedLevelSnapshot`/`CaveSnapshotService`/`CaveSaveData`) + `GenerationConfigVersion++`.
- **Slice 4 (alto risco):** conflito em runtime — targeting de rival (`EnemyBrain`), dano inter-monstro + "Ferido" + corpo com loot reduzido (`EnemyHealth`), feedback de HUD obrigatório. Gate de `architecture-reviewer` + `non-regression`.
- **Slice 5:** mercador errante enriquecido.
- **Slice 6:** geradores de editor + data assets (Unity), EditMode Test Runner, replay validator, cenário humano de Play Mode.

**Não promover a `implementados/` até todas as slices + Play Mode humano.**
