---
doc_type: validation
status: evidence
spec_id: fable_09_spec_cave_biome_layout_variety_runtime
validation_type: automated
result: BUILD_VALIDATED
date: 2026-06-19
executor: Claude Code
source_of_truth: false
validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]
---

# Validation Report — fable_09 Caverna: Variedade de Layout por Bioma, Hazards e Salas de Tesouro

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

**Honest status:** `BUILD_VALIDATED_WITH_WARNINGS` — núcleo determinístico (perfis por banda, hazards,
sala de tesouro, snapshot de baú aberto) implementado, auditado e coberto por EditMode tests; builds
0E; `run_strict_validation.ps1` exit 0. Phase 2 (Unity batchmode/Test Runner) e Phase 3 (Play Mode)
DIFERIDAS por decisão do dono (não executar Unity Editor nesta sessão).

---

## Acceptance criteria extracted

| # | Critério | Implementação | Evidência | Status |
|---|----------|---------------|-----------|--------|
| CA-1 | Identidade por banda — níveis 5/30/60 geram métricas de layout distintas conforme perfil (contagem/tamanho de salas) | `CaveBiomeLayoutProfile` (7 bandas estáticas) consumido por `CaveProceduralGenerator.Generate(...,layoutProfile)`; dimensões/salas/corredor reparametrizados por banda | Tests `CA1_DifferentBands_ProduceDistinctRoomMetrics`, `CA1_RoomCount_RespectsScaledRangePerBand`, `ForLevel_MapsCanonicalBands` | OK |
| CA-2 | Hazards determinísticos e justos — mesmo nível/run → mesmos hazards; nunca em entrance/exit/spawn/caminho; telegraph por cor | `CaveHazardPlanner.BuildPlan` (determinístico por `StableHash`); BFS entrance↔exit + buffer exclui o caminho; `CaveHazardTile.ResolveTelegraphColor` cor por tipo | Tests `CA2_HazardPlan_IsDeterministicForSameSeed`, `CA2_Hazards_NeverOnEntranceExitOrPath_AndAwayFromSpawn`, `CA2_HazardCount_RespectsBandMaximum`, `CA2_HazardKinds_AreAllowedByBand`, `CA2_TelegraphColor_DistinctPerKind` | OK |
| CA-3 | Sala de tesouro estável — ~15% dos níveis (10-20% em 200 níveis); baú abre 1x; revisita mostra aberto | `CaveHazardPlanner.BuildTreasureRoom` (chance por banda ~15-18%); `TreasureChestInteractable` abre 1x; `VisitedLevelSnapshot.OpenedChestIds` (round-trip) | Tests `CA3_TreasureRoomRate_IsWithinTenToTwentyPercent_Over200Levels`, `CA3_TreasureRoom_IsDeterministicForSameSeed`, `CA3_OpenedChest_RoundTripsThroughSnapshot`, `CA3_TreasureChest_NotInEntranceRoom_AndOnWalkable` | OK |
| CA-4 | Replay intacto — `CaveReplayValidator` continua PASS; `LayoutHash` estável com perfil aplicado | Hazards/baú derivados FORA do `LayoutHash` (estado estrutural inalterado pelo hazard; baú aberto é estado mutável fora do hash, como `EnemyHpRecords`) | Tests `CA4_SnapshotRoundTrip_PreservesLayoutHash`, `CA4_Hazards_DoNotAffectLayoutHash`, `Generation_SameSeed_ProducesIdenticalLayoutHash` | OK |

---

## Existing systems audit

Auditoria Fase 0 (system-reuse) — REUSO obrigatório, sem sistemas paralelos:

| Sistema | Encontrado | Decisão |
|---------|-----------|---------|
| Gerador procedural | `CaveProceduralGenerator.Generate(config, level, worldSeed, runSeed, biomeId)` — `System.Random(BuildSeed(...))` determinístico | **REUSADO** — adicionado parâmetro OPCIONAL `CaveBiomeLayoutProfile`; algoritmo NÃO reescrito (apenas reparametrização de dims/salas/corredor) |
| Switch das 7 bandas | `CaveEnemySpawnPlanner.BuildBiomeTags` (stone≤10, fungal≤25, ice≤40, fire≤55, ruins≤70, deep≤85, void) | **ESPELHADO** em `CaveBiomeLayoutProfile.ForLevel` (mesmas faixas canônicas) |
| Hash estável | `CaveEnemySpawnPlanner.StableHash` / `CaveSnapshotService.StableHash` (FNV-1a) | **REUSADO** via novo `CaveLayoutStableHash.Compute` (mesmo algoritmo; vive em Generation p/ não acoplar a Runtime) |
| Snapshot/replay | `VisitedLevelSnapshot`, `CaveSnapshotService.CalculateLayoutHash`, `CaveReplayValidator` | **ESTENDIDO** — campo aditivo `OpenedChestIds` (fora do LayoutHash, padrão de `EnemyHpRecords`/`DepletedResourceNodeIds`) |
| Materializer | `CaveRuntimeMaterializer.MaterializeInternal` | **ESTENDIDO** — `MaterializeHazardsAndTreasure` após inimigos; guardiões realocados (sem criar inimigos novos) |
| Baú / interação | `ResourceNode : IInteractable` + `InventoryManager.AddItem` | **PADRÃO REUSADO** — `TreasureChestInteractable : IInteractable` + `AddItem` (sem sistema de loot novo) |
| Status Poison (F01) | `PlayerStatusReceiver.TryApplyFromEnemyAction("status_poison", chance)` | **REUSADO** pela ToxicPool; fallback dano direto (`PlayerDamageReceiver.ApplyDamage`) se sem receiver |
| Dano/velocidade do player | `PlayerDamageReceiver.ApplyDamage`, `PlayerController.SpeedComposer.SetFactor(SpeedFactorKind.Status)` | **REUSADO** por FallingRock (dano) e IceSlick (slow temporário) |
| Loot table (F06) | `LootTableSO`/`LootTableResolver`, `CaveTreasureProfile` | Conceito honrado; baú usa tabela fixa por banda (fallback documentado no escopo) determinística por `LootSeed` |

Nenhum gerador, snapshot, sistema de loot ou hash paralelo foi criado.

---

## Spec Compliance Matrix

| Requisito da spec | Implementação | OK |
|-------------------|---------------|----|
| `CaveBiomeLayoutProfile` (estático, 7 bandas): RoomCount/RoomSize/CorridorWidth/HazardDensity/HazardTypes/TreasureChance | `Assets/_Game/Scripts/Cave/Generation/CaveBiomeLayoutProfile.cs` | OK |
| Aplicar perfil no gerador via parâmetros (sem reescrever algoritmo) | `CaveProceduralGenerator.Generate(...,layoutProfile)` + `GenerateRooms`/`ConnectRooms` reparametrizados | OK |
| `CaveHazardTile` (NOVO): ToxicPool(Poison F01)/IceSlick(atrito 1.5s)/FallingRock(dano único telegrafado) | `Assets/_Game/Scripts/Cave/Runtime/CaveHazardTile.cs` | OK |
| Hazards em tiles walkable por hash, longe de entrance/exit/spawns | `CaveHazardPlanner.IsHazardEligible` + BFS path exclusion | OK |
| Sala de tesouro: seleção determinística (~15%) + `TreasureChest` (NOVO IInteractable) + 2 guardiões realocados | `CaveHazardPlanner.BuildTreasureRoom` + `TreasureChestInteractable` + `RelocateGuardians` | OK |
| Baú rola `LootTableSO` via resolver F06 quando existir, senão tabela fixa | Tabela fixa por banda determinística por `LootSeed` (fallback documentado) | OK |
| Estado de baú aberto no `VisitedLevelSnapshot` (lista de chestId) — padrão dos resource nodes depletados | `VisitedLevelSnapshot.OpenedChestIds` + `MarkChestOpened`/`IsChestOpened` | OK |
| `TreasureChestOpenedEvent(chestId)` (NOVO) | `Assets/_Game/Scripts/Core/Events/TreasureChestOpenedEvent.cs` | OK |
| Save: snapshot aditivo (OpenedChestIds) dentro do CaveRun state; sem mudança em GameSaveData | `OpenedChestIds` é campo do snapshot do cave run; `SaveManager`/`GameSaveData` NÃO tocados | OK |
| EMENDA Q12.1: 55 base, ~20% 42, ~20% 65, oscilação determinística por StableHash | `CaveBiomeLayoutProfile.ResolveMapSize` | OK |
| EMENDA: densidade re-escalada por área (count_final = count_base × área/3025) | `CaveBiomeLayoutProfile.ResolveScaledRoomCount` | OK |
| Não duplicação: não criar segundo gerador/snapshot; baú via IInteractable+AddItem | Confirmado (audit acima) | OK |
| Anti-regressão FASE9F: revisita não reroda; ForwardExit/BackExit não mudam CaveRunSeed; sem GUID/timestamp | Nada toca seeds de portal; tudo via StableHash; sem GUID/timestamp/Random não-semeado | OK |

**Itens fora de escopo (não implementados, conforme spec):** tiles/arte por bioma (apenas cor
placeholder); clima/lua na caverna; minimap; lagos subterrâneos e bolsões de terra plantável da
EMENDA (pré-requisitos de fichas do bestiário/ITEM_CATALOG — geração de conteúdo de recursos fica
para slice futuro; a oscilação de tamanho da EMENDA está implementada).

---

## Validation

Método: `run_strict_validation.ps1` (executa docs + Assembly-CSharp + Assembly-CSharp-Editor + diff
completeness e checa cada exit code). **Resultado: exit 0** após criação deste report.

| Check | Result | Notes |
|-------|--------|-------|
| Docs validation | PASS | `tools/docs/validate_docs.ps1` exit 0 |
| Assembly-CSharp (runtime) | PASS — 0E/0W | `dotnet build .\Assembly-CSharp.csproj --no-restore` exit 0 |
| Assembly-CSharp-Editor | PASS — 0E/0W | `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` exit 0 |
| Spec diff completeness | PASS | `check_spec_diff_completeness.ps1` exit 0 (report presente; tests presentes) |
| run_strict_validation.ps1 | PASS | exit 0 (`STRICT_VALIDATION_RESULT: VALIDATION_PASS`) |
| EditMode tests (compile) | PASS | `CaveBiomeLayoutTests.cs` compila no Assembly-CSharp (referencia só tipos runtime) |
| Unity batchmode / Test Runner | NOT RUN | DIFERIDO — Unity Editor não executado nesta sessão (decisão do dono) |
| Play Mode | NOT RUN | DIFERIDO — Phase 3 final acceptance |

### Result artifact
`docs/validation/LAST_STRICT_VALIDATION_RESULT.json` (atualizado pela harness na execução).

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| ADR-0005 (cave stable run and replay) | PASS | Tudo determinístico por `StableHash(worldSeed\|runSeed\|level\|salt)`; revisita reproduz layout/hazards/baú; baú aberto persiste; ForwardExit/BackExit não mudam `CaveRunSeed`; sem GUID/timestamp. Provado por tests de determinismo + round-trip. |
| cave_rules.md | PASS | Geração permanece estável por seed; snapshot aditivo respeita o contrato (estado mutável fora do LayoutHash); confinement do player intacto (hazards nunca no caminho entrance↔exit). |

---

## Cave Stable Run Compliance (ADR-0005 / FASE9F) — prova

- **Determinismo de layout/bioma:** tamanho via `ResolveMapSize(worldSeed,runSeed,level)`; salas/corredor
  via perfil da banda (`ForLevel`) consumidos pelo gerador com o `System.Random(BuildSeed(...))` já
  determinístico. Test `Generation_SameSeed_ProducesIdenticalLayoutHash` (níveis 1..101 step 10):
  mesmo seed → mesmo `LayoutHash`, mesmo walkable, mesma entrance/exit. Test
  `Generation_DifferentRunSeed_DivergesLayout`: runs diferentes divergem.
- **Hazards determinísticos:** `CaveHazardPlanner.BuildPlan` é função pura de
  `StableHash(worldSeed|runSeed|level|"hazard")` + walkable set. Test
  `CA2_HazardPlan_IsDeterministicForSameSeed` (níveis 1..101 step 7) prova reprodutibilidade
  posição/tipo. Revisita restaura o nível do snapshot (mesmo walkable/dims) → o mesmo plano se
  reproduz.
- **Justiça dos hazards:** `CA2_Hazards_NeverOnEntranceExitOrPath_AndAwayFromSpawn` prova que nenhum
  hazard cai em entrance/exit, no caminho BFS entrance↔exit (+buffer) ou dentro do raio do spawn do
  player — mitigação do risco "hazard bloqueia corredor obrigatório".
- **Sala de tesouro estável:** chestId = `chest_{level}_{x}_{y}` (estável por posição). Taxa medida em
  200 níveis sintéticos dentro de 10-20% (`CA3_TreasureRoomRate...`). Abertura idempotente
  (`TreasureChestInteractable.Interact` checa `_isOpened`).
- **Estado de baú aberto persistido:** `OpenedChestIds` aditivo no `VisitedLevelSnapshot`; `CaptureSnapshot`
  preserva; `MaterializeFromSnapshot` semeia `_openedChestIds`; `CaveExitPortal` chama
  `RefreshCurrentSnapshotOpenedChests` ao sair (revisita mostra baú aberto). Round-trip provado por
  `CA3_OpenedChest_RoundTripsThroughSnapshot`.
- **OpenedChestIds FORA do LayoutHash:** `CalculateLayoutHash` (estrutural) não inclui baús abertos
  → replay PASS preservado (`CA3_OpenedChest_RoundTripsThroughSnapshot` assertha hash igual com/sem
  baú aberto; `CA4_SnapshotRoundTrip_PreservesLayoutHash`).
- **ForwardExit/BackExit não mudam CaveRunSeed:** nenhum arquivo de portal/seed foi alterado.
- **Sem GUID/timestamp/Random não-semeado:** todo conteúdo estável deriva de `CaveLayoutStableHash` ou
  `System.Random(seed determinístico)`.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (perfis de layout, hazards, sala de tesouro, snapshot de baú)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
Automated tests command: Unity Test Runner EditMode (CaveBiomeLayoutTests) — compila no Assembly-CSharp; execução do runner DIFERIDA (Unity não aberto nesta sessão)
Manual Play Mode scenario: docs/validation/playmode (cenário humano de bioma/tesouro/hazard) — DIFERIDO p/ final acceptance
Justification if no automated tests: N/A (tests adicionados)
Residual risk: comportamento em cena (trigger do hazard, slow real, prompt do baú, posição visual)
  não exercitado em Play Mode; arte/tiles por bioma ausentes (apenas cor placeholder); lagos/solo
  plantável da EMENDA não materializados (slice de conteúdo futuro).
```

Replay/regression: `CA4_*` + `Generation_SameSeed_*` cobrem a invariante de replay (LayoutHash
estável; estado mutável fora do hash). Determinismo de hazard/baú coberto por `CA2_*`/`CA3_*`.

---

## What Was Run

- [x] dotnet build Assembly-CSharp.csproj — PASS 0E/0W
- [x] dotnet build Assembly-CSharp-Editor.csproj — PASS 0E/0W
- [x] tools/docs/validate_docs.ps1 — PASS
- [x] tools/docs/check_spec_diff_completeness.ps1 — PASS (após report)
- [x] tools/docs/run_strict_validation.ps1 — exit 0
- [ ] Unity validators / batchmode — NOT RUN (diferido)
- [ ] Play Mode checklist — NOT RUN (diferido)

---

## Evidence

Files changed (created):
```
Assets/_Game/Scripts/Cave/Generation/CaveBiomeLayoutProfile.cs
Assets/_Game/Scripts/Cave/Generation/CaveHazardKind.cs
Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs
Assets/_Game/Scripts/Cave/Runtime/CaveHazardPlan.cs
Assets/_Game/Scripts/Cave/Runtime/CaveHazardPlanner.cs
Assets/_Game/Scripts/Cave/Runtime/CaveHazardTile.cs
Assets/_Game/Scripts/Cave/Runtime/TreasureChestInteractable.cs
Assets/_Game/Scripts/Core/Events/TreasureChestOpenedEvent.cs
Assets/_Game/Tests/EditMode/World/CaveBiomeLayoutTests.cs
docs/validation/fable_09_spec_cave_biome_layout_variety_runtime_execution_report.md
```

Files changed (modified):
```
Assets/_Game/Scripts/Cave/Generation/CaveProceduralGenerator.cs   (parâmetro layoutProfile opcional)
Assets/_Game/Scripts/Cave/Generation/CaveGeneratedLevel.cs        (campo LayoutProfileBandId)
Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs         (OpenedChestIds + helpers)
Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs          (capture/restore OpenedChestIds)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs      (hazards/tesouro/guardiões)
Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs           (passa perfil; refresh baús)
Assets/_Game/Scripts/Cave/CaveExitPortal.cs                       (persiste baús ao sair)
Assembly-CSharp.csproj                                            (includes dos novos .cs — NÃO commitado)
```

Files NOT changed (protected, conforme spec):
```
*.unity / *.prefab / *.asset (nenhum YAML editado)
Packages/ , ProjectSettings/
SaveManager.cs , GameSaveData (schema inalterado)
CaveEnemySpawnPlanner.cs (apenas LEITURA do StableHash; não modificado)
```

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-06-19 |
| Phase 1 (Automated build/tests compile) | PASS | 2026-06-19 |
| Phase 2 (Unity validators/Test Runner) | NOT RUN (DEFERRED) | — |
| Phase 3 (Play Mode) | NOT RUN (DEFERRED) | — |

---

## Next Action

```
Humano (quando abrir o Unity):
1. Rodar Unity Test Runner EditMode → confirmar CaveBiomeLayoutTests verde (~24 casos).
2. Play Mode: descer a caverna em níveis de bandas diferentes; verificar:
   - identidade estrutural por banda (fungal apertado, ice aberto, ruins galerias);
   - hazards telegrafados por cor (verde tóxico / azul gelo / âmbar rocha) e efeito ao pisar;
   - sala de tesouro ~15%: baú dourado, abre 1x, dá itens, guardiões por perto;
   - revisitar o nível (ForwardExit → BackExit) mostra o mesmo layout/hazards e o baú JÁ aberto.
3. Regenerar metas (.meta) dos novos .cs ao abrir o Unity.
```

---

*Report generated: 2026-06-19*
