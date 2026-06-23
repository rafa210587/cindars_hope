# Execution Report — fable_78 (Caverna Viva: Povoamento e Ecossistema)

> **Spec:** `fable_78_spec_cave_ecosystem_population_runtime`
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS` — **SLICES 1-4 de 6** concluídas (slice 4 com architecture-reviewer + non-regression PASS). A spec como um todo **NÃO está completa** e **NÃO** deve ser promovida.
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
| 14.1 Tamanho por banda | OK (slice 2) — size-with-depth determinístico + buckets preservados | `CaveBiomeLayoutProfile.cs`, `CaveMapSizeScalingTests.cs` |
| 14.3 Lagos→aquáticos | OK no nível de gating (slice 2): `FilterAquaticEligibility(hasWater)`; materialização de lago = slice 3 | `CaveEnemySpawnPlanner.cs`, `CaveThreatBudgetTests.cs` |
| 14.4 Threat budget | OK (slice 2) — piso/teto + densidade por sala + entrada segura | `CaveEnemySpawnPlanner.cs`, `CaveThreatBudgetTests.cs` |
| 14.6 Comportamento de conflito (aggro/dano/ferido/loot reduzido) | DEFERRED (slice 4 — toca `EnemyBrain`/`EnemyHealth`) | — |
| 14.7 Merchant enriquecido | DEFERRED (slice 5) | — |
| 14.8 Persistência/save aditivo | OK (slice 3) — snapshot/CaveSaveData aditivos + back-compat test; materialização runtime = Play Mode deferido | `VisitedLevelSnapshot.cs`, `CaveSnapshotService.cs`, `CaveSaveData.cs`, `CaveRuntimeMaterializer.cs`, `CaveSaveBackCompatTests.cs` |
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

- ~~**Slice 2:** tamanho por banda + threat budget + densidade + gating aquático~~ ✅ FEITO. **DEFERRED_UNITY:** humano deve subir `GenerationConfigVersion` 3→4 no asset `CaveGenerationConfig_Default.asset` (só o default em código foi alterado) para invalidar snapshots legados.
- ~~**Slice 3:** materialização + persistência aditiva~~ ✅ FEITO (persistência + back-compat testados; materialização runtime null-safe com **DEFERRED_UNITY:** prefabs `_decorElementPrefab`/`_waterTilePrefab` + database/profiles + nodes de minério, e **Play Mode deferido**). GenerationConfigVersion já bumpado na slice 2.
- ~~**Slice 4 (alto risco):** conflito em runtime — targeting de rival (`EnemyBrain`), dano inter-monstro + "Ferido" + corpo com loot reduzido (`EnemyHealth`), feedback de HUD obrigatório.~~ ✅ FEITO (ver seção SLICE 4 abaixo). non-regression PASS. **Play Mode deferido.**
- **Slice 5:** mercador errante enriquecido.
- **Slice 6:** geradores de editor + data assets (Unity), EditMode Test Runner, replay validator, cenário humano de Play Mode.

**Não promover a `implementados/` até todas as slices + Play Mode humano.**

---

# SLICE 4 — Conflito inter-monstro em runtime (ALTO RISCO)

> **Date:** 2026-06-23 · **Status:** `BUILD_VALIDATED_WITH_WARNINGS` (Play Mode deferido) · **Branch:** dev · **NÃO commitado.**

## Fase 0 — Pontos de integração reais encontrados (assinaturas confirmadas)

- **EnemyBrain target resolution:** `RefreshPlayerTarget()` (EnemyBrain.cs) roda a cada frame em `Update` e aponta `_playerTarget` para `Player.PlayerController.ActiveInstance` (fallback `GameBootstrap.Instance.PlayerManager`). Todo o movimento/distância/estado usa `_playerTarget` + `DistanceToPlayer()`/`DirectionToPlayer()`. Ataque resolve em `ResolveAction()` (chamado por `TickActionTimers` no fim do windup) e roteia dano ao player via `CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(...)`.
- **EnemyHealth.TakeDamage(DamageRequest):** origem registrada via `request.SourceId`; defesa aplicada por `DamageCalculator.Calculate(request, _enemyData.defense, null, vulnMult, 1f, elemMatMult)`. `Die()` publica `EnemyKilledEvent` (consumido por XP/quest/bestiário/loot). Contexto de loot estável ligado por `ConfigureLootContext(enemyInstanceId, caveRunSeed)`. **Instance id** = `gameObject.name` = `entry.EnemyInstanceId`.
- **Loot:** drop é 100% event-driven — `EnemyDropSpawner` assina `EnemyKilledEvent` e concede via `InventoryManager.AddItem` (caminho por `LootTableSO` ou legado `dropItemId×dropAmount`). Não há "drop component" por inimigo.
- **Status effects (fable_01):** pipeline = `StatusEffectManager` no `EnemyHealth` + `EnemyStatusRuntimeTicker` (resolve `StatusEffectSO` via `GameBootstrap.Instance.StatusEffectDatabase`). **NÃO existe** um status "Ferido"/defesa-reduzida (`StatusEffectType` tem Weakness/Vulnerable mas o ticker não aplica multiplicador de defesa). Criar o `StatusEffectSO` "Ferido" + registrá-lo no database exige Unity → **DEFERRED_UNITY**.
- **Materializer → injeção sem search:** `CaveRuntimeMaterializer` cria cada inimigo em `ConfigureEnemyRuntimeObject` e mantém todas as instâncias em `_materializedObjects` (nome == EnemyInstanceId). Injeção de refs já é o idioma (pack coordinator, loot context, elite affix) — injeção explícita no spawn, sem `GameObject.Find`.

## O que foi implementado (mínimo, aditivo)

### A) Atribuição do conflito na materialização
- **Roll por entrada** em `CaveLevelRuntimeController.ApplyInterMonsterConflict()` (novo): usa `CaveEcosystemConflictPlanner.Decide(worldSeed, runSeed, caveLevel, entryIndex=ConflictState.EntryCount, hasHadConflictBefore=ConflictState.HasHadConflict, presentEnemyIds, balance)`. Chamado em `GenerateCurrentLevel` (após `CaptureSnapshot`) E em `RestoreFromSnapshot` (re-roll por visita — comportamento, não composição, ADR-0018).
- **Marcação dos lados** em `CaveRuntimeMaterializer.ApplyConflict(plan, caveLevel)` (novo): anexa `CaveConflictCombatant` a cada inimigo dos lados A/B (varrendo só `_materializedObjects`, sem search), injeta as refs de `EnemyHealth` dos rivais e chama `EnemyBrain.ConfigureConflict(combatant, balance)`.
- **Persistência:** `VisitedLevelSnapshot.RecordConflictEntry(...)` (já existia da slice 3) incrementa `EntryCount`, trava `HasHadConflict` e grava `ConflictActive`/`Faction*` da visita.
- **Evento:** `CaveEcosystemConflictStartedEvent(caveLevel, factionA, factionB)` publicado quando ativo.

### B) Targeting conflict-aware no EnemyBrain (ramo isolado)
- `RefreshConflictTarget()` (novo) roda em `Update` logo após `RefreshPlayerTarget()`. **No-op quando `_conflictCombatant == null`** → caminho player-only intacto. Quando combatant presente: escolhe o alvo hostil mais próximo entre {player} ∪ {rivais vivos}, ponderado por `PlayerAggroWeight`/`RivalAggroWeight` (default 1/1 → empate = mais próximo). Se rival vence, aponta `_playerTarget` para o GO do rival (**reusa o locomotor/FSM existente**) e marca `_rivalHealthTarget`.
- `ResolveAction()` ganha um guard no topo: `if (IsTargetingRival) { ResolveInterMonsterAction(dmgType); return; }`. Sem conflito o método segue byte-for-byte.
- Same-type nunca é alvo: garantido pelo planner (lados sempre de enemyId diferente) **e** re-checado em `CaveConflictCombatant.FindNearestLivingRival` (`rival.EnemyId != OwnEnemyId`).

### C) Dano inter-monstro + "Ferido" + corpo reduzido no EnemyHealth
- `TakeDamageFromEnemy(rawDamage, dmgType, killerInstanceId, caveLevel, balance)` (novo): aplica `InterMonsterDamageMultiplier` (0.10), aplica "Ferido" e roteia pelo `TakeDamage` padrão marcando o killer-inimigo.
- Dano monstro↔jogador **inalterado** (esse caminho não chama `TakeDamageFromEnemy`).
- **Kill por inimigo:** `Die()` ramifica em `_pendingEnemyKillerInstanceId` → publica `EnemyKilledByEnemyEvent` (com payload de corpo reduzido ×`InterMonsterKillLootMultiplier`) e **NÃO** dispara `EnemyKilledEvent` (sem XP/quest/bestiário ao player). `EnemyDropSpawner` assina o novo evento e concede o loot reduzido reusando o mesmo resolver/inventário.
- Math pura extraída para `InterMonsterCombatMath` (dano ×0.10, defesa Ferido, loot reduzido ×0.40) — testável em EditMode.

### D) Feedback de HUD obrigatório
- `CaveConflictFeedbackBridge` (novo, `RuntimeInitializeOnLoadMethod` singleton, idioma de bridge de HUD do projeto): assina `CaveEcosystemConflictStartedEvent` → publica `PlayerActionFeedbackEvent("Criaturas em conflito!", 3s)`. Unsubscribe em `OnDisable`. Sem edição de cena.

### E) Testes EditMode
- `Assets/_Game/Tests/EditMode/Cave/InterMonsterCombatTests.cs`: dano ×0.10 exato; dano do player inalterado (caminhos distintos); loot reduzido ×0.40; defesa Ferido ×0.85; same-type nunca alvo (lados distintos do planner); <2 espécies → sem conflito. Math pura via `ScriptableObject.CreateInstance` do balance SO.

## Decisões de design (registradas)

- **Injeção de rivais:** componente `CaveConflictCombatant` por inimigo do conflito, preenchido pelo materializer no spawn com `OwnEnemyId`/`RivalEnemyId`/`CaveLevel` + lista de `EnemyHealth` rivais. Sem `GameObject.Find` (varre `_materializedObjects`).
- **Pesos de aggro:** distância ponderada = `distância / peso` (peso menor = alvo mais atraente; peso ≤ 0 desliga). Default 1/1 → empate resolve por mais próximo (conforme spec).
- **"Ferido" sem novo SO:** o `StatusEffectSO` "Ferido" + database são DEFERRED_UNITY (slice 6). Para não criar sistema paralelo nem bloquear, "Ferido" é uma janela runtime transitória em `EnemyHealth` (`_woundedUntil` + `WoundedDefenseMultiplier`), consumida pelo `DamageCalculator` existente (defesa efetiva reduzida) — mesmo idioma do `_stunUntil`/janela de vulnerabilidade. NÃO é serializado (comportamento por visita, fora do LayoutHash).
- **`EnemyKilledByEnemyEvent` enriquecido aditivamente:** mantém os 3 campos da §16.3 (Victim/Killer/CaveLevel) e adiciona o payload do corpo (DropItemId/DropAmount já reduzido/LootTableId/LootSeed/flags/multiplier) para que o `EnemyDropSpawner` existente seja o ÚNICO caminho de drop, sem segundo sistema de loot e sem search.

## Validação

```
Validation method:        builds individuais + validate_docs.ps1 (run_strict NÃO rodado — ver nota)
dotnet build Assembly-CSharp.csproj --no-restore:        EXIT 0 (0 erros; 1 warning pré-existente CombatTelemetrySession)
dotnet build Assembly-CSharp-Editor.csproj --no-restore: EXIT 0 (0 erros; 3 warnings pré-existentes)
.\tools\docs\validate_docs.ps1:                          EXIT 0 (Docs validation PASSED)
EditMode Test Runner:     NOT RUN (Unity não invocado nesta slice; testes compilam no build)
```

Nota sobre `run_strict_validation.ps1`: igual às slices 1-3, ele retorna exit 1 unicamente pelo check "forbidden files altered" devido a `.asset` PRÉ-EXISTENTES no working tree (Bestiary/Item/etc.) que NÃO foram tocados por esta slice. Os 3 comandos exigidos pelo prompt (2 builds + validate_docs) passam exit 0. csproj (gitignored/regenerado pelo Unity) recebeu apenas linhas `<Compile Include>` dos arquivos novos.

## Testing Quality Gate

```
Changed runtime code:           YES (EnemyBrain, EnemyHealth, EnemyDropSpawner, materializer, controller)
Changed deterministic logic:    YES (InterMonsterCombatMath; roll por entrada via planner)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES (InterMonsterCombatTests — math pura + invariantes do planner)
Automated tests command:        NOT RUN (Unity Test Runner não invocado; testes compilam no build)
Manual Play Mode scenario:      REQUIRED — DEFERRED (aggro real, instanciação, injeção de rivais, kill em runtime, toast, roll por entrada na cena)
Justification if no tests:      Lógica pura testada em EditMode; o restante depende de scene/lifecycle/prefab → Play Mode
Residual risk:                  Runtime de cena (targeting/dano/loot/Ferido/toast e roll por entrada) não exercitado por automação até a validação Play Mode da wave.
```

## Confirmações de scope

- Nenhum arquivo proibido tocado: `CaveEnemySpawnPlanner.cs`, `CaveBiomeLayoutProfile.cs`, `CaveWanderingMerchant.cs`, `SaveManager.cs` **intactos**.
- Nenhum `.asset` / `.unity` / `.prefab` criado ou editado por esta slice. Nenhum `Packages/` / `ProjectSettings/`.
- Caminho player-only do EnemyBrain preservado (ramo de rival 100% guardado por `_conflictCombatant`/`_rivalHealthTarget`).
- Sem `GameObject.Find/FindObjectOfType` novo; sem `UnityEngine.Random`/`DateTime.Now`/`Guid` em lógica/IDs; sem namespace `CindarsHope.Debug/Temp`; sem magic balance value (tudo do `CaveEcosystemBalanceSO`).

## Arquivos desta slice (criados/alterados por mim)

Criados:
- `Assets/_Game/Scripts/Cave/Ecosystem/InterMonsterCombatMath.cs`
- `Assets/_Game/Scripts/Cave/Ecosystem/CaveConflictCombatant.cs`
- `Assets/_Game/Scripts/Cave/Ecosystem/CaveConflictFeedbackBridge.cs`
- `Assets/_Game/Tests/EditMode/Cave/InterMonsterCombatTests.cs`

Alterados:
- `Assets/_Game/Scripts/Core/Events/EnemyKilledByEnemyEvent.cs` (payload aditivo do corpo)
- `Assets/_Game/Scripts/Combat/EnemyHealth.cs` (TakeDamageFromEnemy, Ferido, Die by-enemy, EnemyInstanceId getter)
- `Assets/_Game/Scripts/Combat/EnemyDropSpawner.cs` (assina EnemyKilledByEnemyEvent → loot reduzido)
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` (targeting conflict-aware + ResolveInterMonsterAction + ConfigureConflict)
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` (ApplyConflict/WireConflictSide + EcosystemBalance getter)
- `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` (ApplyInterMonsterConflict — roll por entrada)
- `Assembly-CSharp.csproj` (apenas `<Compile Include>` dos arquivos novos; gitignored/regenerado pelo Unity)

## Deferrals

- **Play Mode:** validação humana de conflito visível, dano 1/10, "Ferido", corpo com loot reduzido (sem XP/quest ao player), toast, e re-roll por entrada — deferida ao lote.
- **DEFERRED_UNITY:** `StatusEffectSO` "Ferido" + registro no database (se for desejado um status canônico em vez da janela runtime), e o asset `CaveEcosystemBalance.asset` ligado nos campos `_ecosystemBalance` do materializer (slice 6). Sem o balance ligado, `ApplyInterMonsterConflict` é no-op seguro (logado).
- **Localização:** toast `"Criaturas em conflito!"` é literal (string do exemplo da spec); localização fora do escopo desta slice.
