# SPEC — Cave Final Save Restriction Policy Future Runtime

> **Spec ID:** `16_spec_cave_final_save_restriction_policy_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 16 — Cave Save Policy / Endgame Presentation / Postgame Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Save Policy / Cave  
> **Domain:** Cave Save Policy / Checkpoints / Safe Save Windows / Restriction Rules  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_16_CAVE_SAVE_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere SaveManager global, schema migration, cave procedural generation, death/corpse recovery core, UI save menu visual final or scene assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Death/**`, `Assets/_Game/Tests/EditMode/Save/**`, `docs/validation/16_spec_cave_final_save_restriction_policy_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - cave save/load UI;
  - cave checkpoint restore;
  - death/corpse recovery;
  - run snapshot provider;
  - final human validation.
> **Scope:** definir/endurecer política final futura de save dentro da caverna: permitido, restrito, checkpoint-only, safe room-only e save warning states.  
> **Out of scope:** SaveManager rewrite, schema migration, scene/prefab UI, cave generation, death/corpse core rewrite.

---

# /speckit.specify

## 1. Contexto

O save/load atual já existe, usa JSON, SaveManager, GameSaveData, SchemaVersion, migrations, safe write e seções amplas. O direction declara que save em caverna continua permitido por enquanto, mas a política final de restrição de save em caverna será refinada depois.

Esta spec é essa política final futura, sem alterar schema por si só.

---

## 2. Problema

Sem política final:

```text
salvar no meio de boss pode gerar softlock;
save em sala procedural pode restaurar posição inválida;
save no meio de death/corpse recovery duplica corpo;
checkpoint pode ser bypassado por save manual;
level 101 pode virar farm por save/reload;
save durante reward aplica reward duas vezes;
load pode restaurar inimigo/loot de forma inconsistente;
UI pode permitir salvar em estados perigosos sem aviso.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CaveSavePolicy;
CaveSavePermissionState;
CaveSaveContext;
CaveSaveRestrictionReason;
CaveSafeSaveWindow;
CaveSaveWarningProjection;
CaveSavePolicyValidator;
CaveSaveAttemptResult.
```

---

## 4. Regras de design

```text
Save em caverna não deve quebrar run seed/snapshot.
Save em caverna não deve bypassar checkpoints/boss gates.
Save em boss/final reward/level 101 deve ser restrito ou explicitamente controlado.
Save manual em estado perigoso deve avisar ou bloquear.
Auto-save em checkpoint/safe room é preferível para estados críticos.
Política deve ser transparente para jogador.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender quando posso salvar na caverna.
Como save/load, quero evitar softlock por posição/perigo/reward.
Como cave, quero preservar snapshot/seed/checkpoint.
Como death/corpse, quero evitar duplicação.
Como endgame, quero impedir farm por save/reload.
```

---

## 6. Escopo

Inclui:

```text
permission states;
restriction reasons;
safe save windows;
checkpoint/boss/101/death/corpse restrictions;
warning projection;
validator/tests.
```

Não inclui:

```text
SaveManager rewrite;
schema migration;
UI visual save menu;
procedural cave rewrite;
death/corpse mechanics rewrite.
```

## 7. Modelo de domínio

### 7.1 CaveSavePermissionState

```text
Allowed
AllowedWithWarning
CheckpointOnly
SafeRoomOnly
AutoSaveOnly
BlockedInCombat
BlockedInBoss
BlockedDuringReward
BlockedDuringDeathRecovery
BlockedLevel101
BlockedInvalidPosition
BlockedUnknown
```

### 7.2 CaveSaveContext

```text
CurrentScene
CurrentCaveLevel
CaveRunSeed
CurrentCheckpoint
IsInCombat
IsBossActive
IsRewardPending
IsDeathRecoveryActive
IsCorpseRecoveryPending
IsLevel101
IsInSafeRoom
IsNearCheckpoint
PlayerPositionValidation
SnapshotState
HasUnsavedUniqueReward
```

### 7.3 CaveSaveRestrictionReason

```text
CombatActive
BossActive
RewardPending
InvalidPosition
SnapshotNotStable
DeathRecoveryActive
CorpseRecoveryPending
Level101SpecialState
CheckpointBypassRisk
NoSafeRoom
UnknownCaveState
```

### 7.4 CaveSaveAttemptResult

```text
Allowed
Blocked
RequiresConfirmation
ShouldAutoSaveAtCheckpoint
PermissionState
RestrictionReasons[]
WarningTextKey
FallbackAction
```

---

## 8. Policy matrix

| Context | Policy |
|---|---|
| normal explored level, stable snapshot | AllowedWithWarning or Allowed |
| combat active | BlockedInCombat |
| boss active | BlockedInBoss |
| unique reward pending | BlockedDuringReward |
| checkpoint room | Allowed / AutoSave |
| safe room | Allowed |
| death/corpse recovery | BlockedDuringDeathRecovery |
| invalid position | BlockedInvalidPosition |
| level 101 | AutoSaveOnly or BlockedLevel101 depending substate |

---

## 9. Idempotency rules

```text
Save cannot be taken between reward grant and reward state commit.
Load cannot regrant boss unique reward.
Load cannot duplicate corpse/corpse marker.
Load cannot reroll CaveRunSeed.
Load cannot change checkpoint unlock state except by valid event.
```

---

## 10. Criteria

```text
Cave save policy contracts exist.
Validator classifies contexts.
Dangerous states block or warn.
Checkpoint/safe room policies are explicit.
Level101/final reward states protected.
Tests cover normal save, combat, boss, reward pending, checkpoint, safe room, corpse recovery, invalid position and level101.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Save/Cave/CaveSavePolicy.cs
Assets/_Game/Scripts/Save/Cave/CaveSavePermissionState.cs
Assets/_Game/Scripts/Save/Cave/CaveSaveContext.cs
Assets/_Game/Scripts/Save/Cave/CaveSaveRestrictionReason.cs
Assets/_Game/Scripts/Save/Cave/CaveSaveAttemptResult.cs
Assets/_Game/Scripts/Save/Cave/CaveSavePolicyValidator.cs
Assets/_Game/Scripts/UI/Save/CaveSaveWarningProjection.cs
Assets/_Game/Tests/EditMode/Save/CaveFinalSaveRestrictionPolicyTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Death/**
Assets/_Game/Scripts/UI/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Save/**
docs/validation/16_spec_cave_final_save_restriction_policy_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar save/cave/death systems.
- [ ] T003 — Consolidar policy/context/result contracts.
- [ ] T004 — Implementar validator.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
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

- Save/load atual existe e não deve ser recriado do zero.
- Save em caverna continua permitido por enquanto, mas política final deve ser refinada depois.
- Save/load persiste estado necessário, não Unity objects.
- DTOs usam tipos simples e IDs estáveis.
- Salvar fora da cena de um sistema deve preservar seção existente se o sistema não está carregado.
- Caverna preserva CaveRunSeed, snapshots, boss gates, checkpoints, death/corpse recovery.

### Deferred / future from directions

- UI visual final de save/load.
- Schema migration.
- SaveManager provider rewrite.
- Death/corpse rewrite.
- Scene/prefab wiring.

### Explicitly not redefined here

- SaveManager internals.
- Cave procedural generation.
- Boss fights.
- Corpse recovery core.
- Checkpoint unlock rules.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu save/load, cave, main progression, UI e world/economy directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a cave final save restriction policy foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Save policy | A mudança declara seção, dono, IDs, capture, preserve, restore order, migration e validação? | Checklist no report. | BLOCKED se persistir sem contrato |
| Cave snapshot | A mudança preserva CaveRunSeed, snapshots, boss defeat states, checkpoint e corpse/death state? | Tests/checklist. | PARTIAL |
| Endgame spoiler | Anya, Arquivista, nível 101, final choice e endings não vazam cedo? | Tests/visibility. | PARTIAL |
| Anti-farm | Nível 101/final/endgame não viram farm infinito, reward repeat ou boss repeat. | Tests/checklist. | PARTIAL |
| Pet/social | Nenhum runtime pet, romance ou social deep foi criado. | Checklist explícito. | BLOCKED se violar |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/16_spec_cave_final_save_restriction_policy_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CaveSavePolicy|CaveSavePermission|SaveManager|CaveSaveData|Checkpoint|CorpseRecovery|Level101|RewardPending" Assets/_Game/Scripts docs/design docs/specs
rg -n "SaveManager|GameSaveData|SchemaVersion|Migration|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Checkpoint|Corpse|Death|Level101|FinalChoice|Ending|PostGame|Archivist|Arquivista|Anya|Fonte|Mana|BlackStone|Pet|Romance|Spouse" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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

- Manual save during boss.
- Manual save while reward pending.
- Save in invalid procedural position.
- Save during corpse recovery.
- Level101 save/reload farm.
- Checkpoint bypass.
- Snapshot not stable.
- Save warning missing.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Cave Final Save Restriction Policy Future Runtime

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
rg -n "SaveManager|GameSaveData|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Level101|FinalChoice|Ending|PostGame|Archivist|Anya|Fonte|Mana" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, cave save permission/validation logic is deterministic.
- Requires EditMode tests: YES for normal/combat/boss/reward/checkpoint/corpse/invalid-position/101 tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for save UI/cave gameplay validation.
- Requires regression test: YES if fixing existing cave save bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; cave save policy blocks dangerous states.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/16_spec_cave_final_save_restriction_policy_future_runtime_execution_report.md.
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
