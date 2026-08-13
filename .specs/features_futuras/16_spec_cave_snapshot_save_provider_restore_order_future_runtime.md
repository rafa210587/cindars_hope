# SPEC — Cave Snapshot Save Provider Restore Order Future Runtime

> **Spec ID:** `16_spec_cave_snapshot_save_provider_restore_order_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 16 — Cave Save Policy / Endgame Presentation / Postgame Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Save Provider / Cave Snapshot Restore  
> **Domain:** Cave Save Provider / Snapshot Normalize / Restore Order / Cross-scene Preserve  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_16_CAVE_SAVE_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere global migration registry, SaveManager rewrite, cave procedural generator rewrite, enemy spawn resolver rewrite or schema migration without approval.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Enemies/**`, `Assets/_Game/Tests/EditMode/Save/**`, `docs/validation/16_spec_cave_snapshot_save_provider_restore_order_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - provider architecture rollout;
  - cave save restriction policy;
  - death/corpse provider;
  - bestiarization/cave knowledge restore;
  - final validation.
> **Scope:** definir/endurecer provider futuro da seção Cave: capture/preserve/restore/normalize, snapshot stability, depleted nodes, enemy spawn plans and boss defeat states.  
> **Out of scope:** SaveManager full refactor, schema migration, procedural generation rewrite, enemy AI rewrite, UI.

---

# /speckit.specify

## 1. Contexto

O direction de save define arquitetura alvo com providers por seção, mas curto prazo mantém SaveManager agregador. O direction de cave já tem CaveSaveData, CaveRunSeed, CurrentCaveLevel, DeepestLayerReached, UnlockedCheckpoints, DepletedNodeIds, VisitedLevelSnapshots, BossDefeatStates e EnemySpawnPlan em snapshot.

Esta spec cria o contrato futuro de provider cave.

---

## 2. Problema

Sem provider/restore order claro:

```text
salvar fora da Cave apaga Cave;
load restaura Cave antes de bootstrap/registries;
snapshot perde depleted nodes;
enemy ids mudam ao revisitar nível;
boss defeat state regride;
checkpoint unlock duplica;
cave state inválido passa sem normalização;
bestiarização/conhecimento de cave restaura fora de ordem.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CaveSaveSectionProvider;
CaveCapturePolicy;
CaveRestoreOrderPolicy;
CaveSectionPreservationPolicy;
CaveSnapshotNormalizer;
CaveSaveValidationResult;
CaveRestoreContext;
CaveSnapshotIntegrityValidator.
```

---

## 4. Regras de design

```text
Salvar fora da Cave não apaga Cave.
CaveRunSeed e snapshots são preservados.
VisitedLevelSnapshot define revisita, não reroll.
EnemyIds não mudam ao revisitar nível.
Boss derrotado não repete recompensa única.
Provider não substitui migration.
Provider não salva Unity references.
```

---

## 5. User stories / engineering stories

```text
Como SaveManager, quero delegar Cave capture/restore com política clara.
Como cave, quero preservar snapshot e boss gates.
Como player, quero revisitar nível sem reroll indevido.
Como death/corpse, quero restaurar recuperação sem duplicar.
Como validator, quero normalizar CaveSaveData inválido.
```

---

## 6. Escopo

Inclui:

```text
cave provider contract;
capture/preserve/restore policies;
snapshot normalization;
integrity validator;
restore order constraints;
tests.
```

Não inclui:

```text
SaveManager rewrite completo;
schema migration;
CaveProceduralGenerator rewrite;
EnemySpawnResolver rewrite;
UI.
```

## 7. Modelo de domínio

### 7.1 CaveCapturePolicy

```text
AlwaysCaptureIfCaveLoaded
PreserveIfCaveSceneMissing
CaptureCheckpointOnly
CaptureAtLevelBoundary
NeverCaptureDuringInvalidState
```

### 7.2 CaveRestoreOrderPolicy

```text
AfterPlayerInventoryEquipment
AfterWorldTime
AfterQuestMainProgression
BeforeDeathCorpseRecovery
BeforeBestiaryKnowledge
BeforeUiRebuild
```

### 7.3 CaveSectionPreservationPolicy

```text
PreserveExistingWhenSceneMissing
PreserveSnapshotsWhenNoRuntime
PreserveBossStatesAlways
PreserveCheckpointStatesAlways
PreserveDeathRecoveryIfPending
DropTransientCombatOnly
```

### 7.4 CaveSaveValidationResult

```text
Valid
Normalized
InvalidRecoverable
InvalidBlocked
Warnings[]
FixesApplied[]
```

### 7.5 CaveSnapshotIntegrityCheck

```text
RunSeedPresent
VisitedLevelSnapshotIdsUnique
EnemyIdsStable
DepletedNodeIdsStable
BossDefeatStatesStable
CheckpointUnlocksMonotonic
Level101NotCommonSnapshot
NoUnityReferences
```

---

## 8. Capture rules

```text
If Cave loaded and state valid:
  capture current CaveSaveData.

If Cave not loaded:
  preserve existing Cave section.

If invalid state:
  block save or capture safe checkpoint depending CaveSavePolicy.

If level boundary:
  commit snapshot changes.

If transient combat:
  do not persist transient AI action queue unless explicitly required.
```

---

## 9. Restore rules

```text
Restore global time/world first.
Restore player/inventory/equipment/hotbar before cave if cave needs loadout.
Restore main progression before level101/gate state.
Restore cave snapshots and boss states before death/corpse recovery.
Restore bestiary/knowledge after enemy IDs/snapshot known.
Rebuild UI last.
```

---

## 10. Normalization rules

```text
Duplicate snapshot entries merge by level and latest valid checksum.
BossDefeatState cannot go from defeated to undefeated.
Checkpoint unlocks are monotonic.
DepletedNodeIds cannot duplicate.
EnemySpawnPlan IDs must remain stable for existing snapshot.
Missing optional future fields get defaults.
Invalid critical field blocks or falls back to safe checkpoint.
```

---

## 11. Criteria

```text
Cave provider contracts exist.
Capture/preserve/restore order explicit.
Snapshot normalization exists.
Save outside Cave preserves Cave section.
Boss/checkpoint state monotonic.
No Unity refs.
Tests cover capture loaded, preserve missing, invalid state, snapshot merge, boss monotonic, checkpoint monotonic, enemy IDs stable and restore order.
```

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Save/Cave/CaveSaveSectionProvider.cs
Assets/_Game/Scripts/Save/Cave/CaveCapturePolicy.cs
Assets/_Game/Scripts/Save/Cave/CaveRestoreOrderPolicy.cs
Assets/_Game/Scripts/Save/Cave/CaveSectionPreservationPolicy.cs
Assets/_Game/Scripts/Save/Cave/CaveSnapshotNormalizer.cs
Assets/_Game/Scripts/Save/Cave/CaveSaveValidationResult.cs
Assets/_Game/Scripts/Save/Cave/CaveSnapshotIntegrityValidator.cs
Assets/_Game/Tests/EditMode/Save/CaveSnapshotSaveProviderRestoreOrderTests.cs
```

Consolidar existentes se houver.

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Death/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Save/**
docs/validation/16_spec_cave_snapshot_save_provider_restore_order_future_runtime_execution_report.md
```

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 15. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar SaveManager/CaveSaveData/snapshots.
- [ ] T003 — Consolidar provider contracts.
- [ ] T004 — Implementar normalizer/validator.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de SAVE_LOAD_FULL_STATE_DIRECTION, CAVE_DESIGN_DIRECTION e QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.
Ela não reimplementa SaveManager do zero.
Ela não reescreve caverna procedural, boss fights, UI visual final, cenas/prefabs ou economy final.
Ela não cria runtime pet nem romance/social deep.
Quando houver conflito entre save/load, cave, main progression, UI, economy ou world directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- SaveManager é o orquestrador atual, mas alvo futuro é provider architecture por seção.
- Providers declaram SectionId, CapturePolicy, RestoreOrder, Capture, Restore, Validate e Normalize.
- Salvar fora da cena de um sistema deve preservar seção existente.
- Restore order deve ser explícito.
- Caverna já tem CaveRunSeed, CurrentCaveLevel, DeepestLayerReached, UnlockedCheckpoints, DepletedNodeIds, VisitedLevelSnapshots, BossDefeatStates e CaveSaveData.
- Revisitar nível usa snapshot, não reroll; EnemyIds não mudam ao revisitar.

### Deferred / future from directions

- SaveManager provider rollout completo.
- Schema migration.
- Cave procedural rewrite.
- Enemy spawn resolver rewrite.
- UI.

### Explicitly not redefined here

- Migration registry.
- GameSaveData schema.
- Cave generation algorithm.
- Enemy AI.
- Boss reward rules.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu save/load, cave, main progression, UI e world/economy directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a cave snapshot save provider/restore order foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Save policy | A mudança declara seção, dono, IDs, capture, preserve, restore order, migration e validação? | Checklist no report. | BLOCKED se persistir sem contrato |
| Cave snapshot | A mudança preserva CaveRunSeed, snapshots, boss defeat states, checkpoint e corpse/death state? | Tests/checklist. | PARTIAL |
| Endgame spoiler | Anya, Arquivista, nível 101, final choice e endings não vazam cedo? | Tests/visibility. | PARTIAL |
| Anti-farm | Nível 101/final/endgame não viram farm infinito, reward repeat ou boss repeat. | Tests/checklist. | PARTIAL |
| Pet/social | Nenhum runtime pet, romance ou social deep foi criado. | Checklist explícito. | BLOCKED se violar |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/16_spec_cave_snapshot_save_provider_restore_order_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CaveSaveSectionProvider|CaveSaveData|VisitedLevelSnapshot|CaveSnapshotNormalizer|RestoreOrder|PreserveIfSceneMissing|EnemySpawnPlan" Assets/_Game/Scripts docs/design .specs
rg -n "SaveManager|GameSaveData|SchemaVersion|Migration|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Checkpoint|Corpse|Death|Level101|FinalChoice|Ending|PostGame|Archivist|Arquivista|Anya|Fonte|Mana|BlackStone|Pet|Romance|Spouse" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue save/cave/main progression directions
And usa IDs estáveis
And não persiste referências Unity
And não cria pet/social deep runtime
And não promove estado visual/UI para fonte de verdade.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler and staging

```text
Given o jogador ainda não desbloqueou gate 100, nível 101, Arquivista, final choice ou ending
When save/load/UI/calendar/quest/cave projection consulta o estado
Then detalhes ocultos permanecem escondidos
And apenas informação descoberta/autorizada aparece.
```

### Scenario 4 — Idempotency / no exploit

```text
Given boss, reward, final choice, ending modifier, cave checkpoint ou death/corpse state já foi aplicado
When reload, retry, scene reload ou reentrada ocorre
Then reward/effect não duplica
And estado terminal não regride sem política explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de save/load, cave checkpoint, nível 101, cinematic, final choice ou postgame world
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Save outside Cave clears cave section.
- Restore order breaks death/corpse.
- Snapshot duplicate corrupts level.
- Boss defeat state regresses.
- Checkpoint unlock regresses.
- Enemy IDs change on revisit.
- Transient combat state persisted incorrectly.
- Invalid field silently accepted.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Cave Snapshot Save Provider Restore Order Future Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Save/cave/endgame compliance
- Save section owner:
- Stable IDs:
- Capture policy:
- Preserve policy:
- Restore order:
- Migration needed:
- Unity references persisted:
- CaveRunSeed/snapshots preserved:
- Boss/reward idempotency:
- Endgame spoiler safe:
- No pet/social deep runtime:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Anti-spoiler/staging:
- Idempotency/no exploit:
- Save/load:
- UI/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/tilemap/asset wiring fora do escopo.
3. A implementação alterar save schema sem migration spec.
4. A implementação persistir ScriptableObject, GameObject, Transform, MonoBehaviour, Collider, Rigidbody, Sprite ou UI state como gameplay state.
5. A implementação reescrever SaveManager ou caverna procedural do zero.
6. A implementação rerollar snapshot estável sem política.
7. A implementação duplicar boss reward, final reward, ending modifier ou corpse/cave recovery.
8. A implementação revelar Anya/Arquivista/101/final/ending cedo.
9. A implementação criar pet runtime ou romance/social deep.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "SaveManager|GameSaveData|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Level101|FinalChoice|Ending|PostGame|Archivist|Anya|Fonte|Mana" Assets/_Game/Scripts docs/design .specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de save/load, caverna, 101, final choice, cinematic ou postgame, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, capture/preserve/restore/normalization logic is deterministic.
- Requires EditMode tests: YES for capture/preserve/normalize/monotonic/restore-order tests.
- Requires PlayMode automated or final human scenario: NO by default; provider/validation only.
- Requires regression test: YES if fixing existing cave save provider bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; cave section preserved and snapshots stable.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/16_spec_cave_snapshot_save_provider_restore_order_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar SaveManager.
Não reimplementar caverna procedural.
Não salvar Unity references.
Não salvar UI state como gameplay state.
Não quebrar CaveRunSeed/snapshots/checkpoints/corpse recovery.
Não duplicar rewards/final modifiers.
Não revelar spoilers endgame cedo.
Não transformar 101/final/postgame em farm infinito.
Não criar pet runtime.
Não criar romance/social deep runtime.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando save/load, cave, main progression, UI e endgame contracts estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
