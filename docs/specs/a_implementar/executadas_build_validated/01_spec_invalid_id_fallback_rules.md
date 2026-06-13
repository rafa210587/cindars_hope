# SPEC — Invalid ID Fallback Rules Audit and Hardening

> **Spec ID:** `01_spec_invalid_id_fallback_rules`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0  
> **Type:** Runtime / Validation / Save / Data / Hardening  
> **Domain:** Core / Stable IDs / Save / Fallback / Validation  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere stable IDs, registries, `SaveManager`, `GameSaveData`, save providers, migration, inventory/equipment/item data, enemy data, quest IDs, NPC IDs, cave IDs, bestiary IDs ou UI que tente resolver IDs em runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/Core/Data/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/**/**DataSO.cs`, `Assets/_Game/Scripts/**/**SaveData.cs`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/**`, `docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md`.  
> **Depends on:**  
> - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`  
> - `docs/specs/a_implementar/01_spec_save_section_ownership_registry.md`  
> - `docs/specs/a_implementar/01_spec_save_provider_architecture_runtime.md`  
> - `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md`  
> - `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md`  
> - `docs/specs/implementados/spec_save_002_schema_migration_v2.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> **Blocks:**  
> - execution of runtime specs that load persisted IDs;  
> - inventory/equipment/hotbar save hardening;  
> - quest state save/load;  
> - enemy/bestiary/cave ID integration;  
> - robust load validation for Waves 02+.  
> **Scope:** define and implement minimal invalid ID fallback behavior for existing stable ID registries/save restore, without renaming IDs or changing save schema.  
> **Out of scope:** broad content cleanup, ID renames without migration, new gameplay systems, UI error screens, schema migration, scene/prefab edits, or human Play Mode validation.

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

- docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
- docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
- docs/specs/SPEC_SOURCE_OF_TRUTH.md
- docs/design/SPEC_SOURCE_MAP.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos e do roadmap macro.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Stable ID registry, invalid ID fallback, hardening de contratos de IDs e referências.
- Separação entre auditoria/residual/hardening e reescrita de sistemas canônicos.
- Regras de não duplicação, validação por audit report e bloqueio de runtime dependente de IDs instáveis.

### Deferred / future from directions

- Migração massiva de todos os domínios para novo registry.
- Reescrita de save/load ou GameEventBus.
- Correção de todos os dados de conteúdo em massa.

### Explicitly not redefined here

- IDs finais de conteúdo.
- Schema de save completo.
- Arquitetura total de registries além do hardening especificado.

---

# /speckit.specify

## 1. Contexto

WAVE 01 consolidou o entendimento de que stable IDs, event contracts and save/load already exist in partial or residual form. The next risk is load robustness: runtime systems must know what happens when a persisted ID no longer resolves to a registry entry.

This spec exists to define a small, enforceable fallback contract for invalid IDs without changing the save schema. It must build on the stable ID, restore order, section ownership and provider architecture specs. It must not create a new registry model.

Operational rule:

```text
An invalid ID during load must be visible, classified and recoverable where possible.
It must not silently become null state, duplicate reward/item/corpse/quest state, or crash without actionable evidence.
```

---

## 2. Problema

Invalid IDs can appear when:

```text
content was renamed or removed;
legacy saves reference old IDs;
future specs add data without registry validation;
scene/pickup/corpse/quest/bestiary state references an unknown ID;
partial save data is missing a section;
manual tests modify save JSON;
agent-generated data uses inconsistent naming.
```

Concrete risks:

```text
load crash during restore;
item/equipment disappearing silently;
quest objective becoming impossible;
bestiary entry unlocking wrong enemy;
corpse recovery losing inventory;
shop/restock containing missing item IDs;
player hotbar binding to invalid action/item;
future migrations hiding bad data instead of reporting it.
```

---

## 3. Objetivo

At the end of this spec, the repo must have a clear invalid ID fallback policy and minimal enforcement/reporting path.

Expected result:

```text
invalid ID categories are defined;
severity levels are defined;
fallback rules are explicit by section/domain;
load should prefer degraded safe state over crash when possible;
critical unrecoverable data is blocked with clear error/warning;
execution report lists risks and future domain-specific fallbacks;
no IDs are renamed without migration.
```

---

## 4. Fontes obrigatórias lidas

For execution, read:

```text
CLAUDE.md
AGENTS.md
docs/project/CURRENT_STATE.md
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md
docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md
docs/specs/a_implementar/01_spec_save_section_ownership_registry.md
docs/specs/a_implementar/01_spec_save_provider_architecture_runtime.md
docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
docs/specs/implementados/spec_save_002_schema_migration_v2.md
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

Conditional domain reads:

```text
Inventory/equipment/hotbar: docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
Quest state: docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
Bestiary/enemy IDs: docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 5. Estado atual do repo

Documented current state:

```text
- Stable IDs/data registries are IMPLEMENTED / RESIDUAL.
- Save/load is PARTIAL / RESIDUAL.
- Save migrations exist and are not to be replaced.
- Ownership/provider specs are intended to make save sections auditable.
- Code search must be performed locally before implementation because previous GitHub connector search was insufficient.
```

Required local audit:

```text
- where IDs are resolved during load;
- where registry lookup returns null or throws;
- whether invalid ID handling already exists;
- which save sections contain item/enemy/quest/NPC/cave/bestiary IDs;
- whether validation tools already detect missing IDs;
- whether any current fallback silently drops state.
```

Do not recreate existing ID/registry/save infrastructure.

---

## 6. User stories / engineering stories

```text
As SaveManager, I need invalid persisted IDs to resolve through a known policy.
As a domain owner, I need to know if my section may ignore, replace, quarantine or block invalid IDs.
As a maintainer, I need warnings/errors that identify section, owner, field and invalid value.
As a player, I should not lose progress silently because an ID moved.
As an agent executor, I need a regression test for at least one representative invalid-ID load path when practical.
```

---

## 7. Escopo

Includes:

```text
- audit existing ID resolution paths;
- define InvalidId severity and fallback categories;
- define minimum fallback rules by section type;
- add or consolidate helper/policy only if safe;
- add validator/EditMode test when practical;
- document unresolved domain-specific fallback gaps;
- create execution report.
```

---

## 8. Fora de escopo

Does not include:

```text
- renaming existing IDs;
- creating migrations for renamed IDs;
- changing save schema;
- replacing SaveManager or registries;
- fixing all domain content assets;
- UI popup/error flow;
- scene/prefab/asset changes;
- human Play Mode validation;
- accepting runtime specs without later 01Q/testing gate evidence.
```

---

## 9. Regras de não duplicação

```text
Do not create a second stable ID registry.
Do not create a second save validation system if one exists.
Do not add domain-specific fallback inside SaveManager if a section/provider owner should handle it.
Do not rename IDs to pass validation.
Do not use Unity references as fallback persisted data.
Do not hide invalid ID findings behind INFO when they affect persisted state.
```

---

## 10. Critérios de aceite

### 10.1 Invalid ID categories

Define categories at minimum:

```text
MISSING_OPTIONAL_REFERENCE
MISSING_REQUIRED_REFERENCE
UNKNOWN_LEGACY_ID
REMOVED_CONTENT_ID
DUPLICATE_ID
EMPTY_ID
MALFORMED_ID
SECTION_OWNER_UNKNOWN
```

### 10.2 Severity and behavior

Define behavior at minimum:

```text
INFO: non-persisted/local-only issue, no gameplay impact.
WARNING: degraded but recoverable persisted state.
ERROR: state cannot be restored correctly but load can continue in safe quarantine/default mode.
BLOCKER: save cannot continue safely without migration/manual fix.
```

### 10.3 Fallback by section type

Document fallback rules for:

```text
inventory/items/equipment/hotbar;
quest flags/objectives/rewards;
NPC/services/schedules;
farm/crops/world resources;
cave run/corpse/loot;
bestiary/knowledge;
player skills/spells/status;
calendar/weather/lunar references when applicable.
```

If a domain cannot be decided yet, mark `UNKNOWN` and block `ACCEPTED` for that domain's future save spec until resolved.

### 10.4 Validation/test

When practical, add validator or EditMode test for:

```text
invalid item ID in representative save DTO;
missing registry entry;
empty ID;
duplicate ID in registry;
section owner reported in invalid-ID finding;
no Unity reference persisted as fallback.
```

If not practical, report `NOT RUN` with reason, impact, mitigation and max status.

### 10.5 Report

Create:

```text
docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md
```

Report must include:

```text
ID resolution paths audited;
policy created/consolidated;
validators/tests added;
sections/domains covered;
unknown domains;
NOT RUN entries;
residual risk;
Testing Quality Gate.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possible targets, depending on local audit:

```text
Assets/_Game/Scripts/Core/Data/InvalidIdFallbackPolicy.cs
Assets/_Game/Scripts/Core/Data/InvalidIdFinding.cs
Assets/_Game/Scripts/Core/Data/InvalidIdSeverity.cs
Assets/_Game/Scripts/Editor/Validation/ValidateInvalidIdFallbacks.cs
Assets/_Game/Tests/EditMode/Core/Data/InvalidIdFallbackTests.cs
docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md
```

If equivalent types already exist, use/extend them instead of creating new ones.

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
Stable IDs remain string identifiers owned by existing registry/data contracts.
Invalid ID findings are validation/runtime diagnostics, not save payload by default.
Fallback metadata must not alter save schema unless a migration spec approves it.
```

### 12.2 Runtime contracts

```text
Registry resolution should return result/finding where practical, not raw null ambiguity.
Load paths must classify invalid ID findings by section/owner.
Fallback must be deterministic.
```

### 12.3 Event contracts

```text
No new gameplay events required.
If invalid ID diagnostics are emitted, they must be local/log/report diagnostics unless a future spec defines events.
```

### 12.4 Save contracts

```text
No save schema change.
No migration unless separate spec approves.
Invalid persisted ID must not be silently converted to a different valid ID.
Safe defaults must be explicit by section owner.
```

### 12.5 UI contracts

N/A — UI error surfacing is future/out of scope.

---

## 13. Sistemas afetados

```text
Stable ID registries;
SaveManager/load normalization;
save section ownership;
save providers;
editor validation;
EditMode tests;
future domain save specs.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/Data/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Core/Data/**
Assets/_Game/Tests/EditMode/Save/**
docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md
```

Read-only unless strictly necessary and reported:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Data/**
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset, unless a single explicit test fixture asset is created under test path and compile-safe
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

```bash
rg -n "IIdentifiedData|IDataRegistry|DataRegistrySO|TryGet|GetById|FindById|ItemId|EnemyId|QuestId|NpcId|Bestiary|Invalid|Fallback" Assets/_Game/Scripts Assets/_Game/Data
rg -n "ValidateAndNormalizeSave|TryReadSaveWithMigration|Restore|LoadGame|GameSaveData|SaveData" Assets/_Game/Scripts/Save Assets/_Game/Scripts
rg -n "null|missing|unknown|warning|error" Assets/_Game/Scripts/Core Assets/_Game/Scripts/Save Assets/_Game/Scripts/Editor
```

### Fase 1 — Policy

```text
1. Consolidate existing invalid-ID handling, if any.
2. Define categories/severity/fallback behavior.
3. Map policy to save section ownership.
4. Stop if policy requires schema migration.
```

### Fase 2 — Minimal implementation

```text
1. Add helper/result/validator only if no equivalent exists.
2. Add representative test when practical.
3. Avoid broad domain changes.
```

### Fase 3 — Report

```text
1. Document covered domains and unknowns.
2. Document tests/validators and validations.
3. Record residual risk for future domain specs.
```

---

## 17. Ordem segura de execução

```text
1. Read required sources.
2. Confirm stable IDs/save ownership/provider specs are available or explicitly treated as dependencies.
3. Audit existing ID resolution and save restore paths.
4. Define fallback policy.
5. Implement minimal helper/validator/test only if safe.
6. Run validations.
7. Create execution report.
8. Do not update SPEC_EXECUTION_ORDER.md or promote specs.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_01_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - stable IDs hardening;
  - save restore order;
  - save section ownership;
  - save provider architecture;
  - any domain save DTO/data registry change.
- Shared files/systems that require lock:
  - stable ID contracts;
  - registry resolution;
  - save restore/load normalization;
  - editor validation.
- Reason:
  - Invalid ID behavior affects all persisted content and must not drift by domain.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO. If YES, STOP and require migration spec.
Does this add a save section? NO.
Does this require migration? NO unless ID rename/removal is being fixed; then separate migration spec.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: NO.
Changes existing events: NO.
Requires unsubscribe pattern: NO.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: SHOULD BE NO, except test fixture under test path if safe.
Requires Play Mode final validation: NO.
Human validation timing: NOT REQUIRED.
```

---

## 22. Riscos técnicos

```text
Risco: fallback hides broken content.
Mitigação: findings must include severity and section/owner; ERROR/BLOCKER cannot be downgraded silently.

Risco: invalid ID helper duplicates registry API.
Mitigação: integrate with existing registry contracts; do not create parallel lookup model.

Risco: fixing invalid IDs requires migration.
Mitigação: STOP and create migration spec; do not rename IDs here.

Risco: validator produces many warnings from future placeholders.
Mitigação: severity classification; future placeholders can be WARNING only when not persisted.
```

---

## 23. Rollback

```text
Remove helper/policy/validator/test files created.
Revert any local save/registry integration patch.
Remove execution report.
No save schema, scene, prefab or real data asset should need rollback.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Read required sources and confirm branch.
- [ ] T002 — Audit ID resolution and save restore paths.
- [ ] T003 — Audit existing invalid-ID handling/fallbacks.
- [ ] T004 — Define categories, severity and fallback behavior.
- [ ] T005 — Map fallback policy to save section ownership.
- [ ] T006 — Add helper/validator/EditMode test only if safe and non-duplicative.
- [ ] T007 — Run validations or record NOT RUN.
- [ ] T008 — Create `docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md`.
- [ ] T009 — Record unknown domains and residual risks.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de stable IDs / invalid ID fallback? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/01_spec_invalid_id_fallback_rules_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "StableId|Registry|ItemId|CropId|QuestId|InvalidId|Fallback|Save|GameSaveData|Migration" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a stable IDs / invalid ID fallback existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing implementation is found

```text
Given existe implementação parcial ou completa no repo
When a execução audita o estado real
Then ela muda para REUSE_EXISTING ou HARDEN_EXISTING
And não recria arquitetura paralela
And documenta residual/future gaps.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige interação visual, PlayMode ou gameplay integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Criar registry paralelo a sistema existente.
- Transformar fallback em silently ignore para erro crítico.
- Persistir Unity references.
- Quebrar save antigo sem migration.
- Normalizar ID inválido para conteúdo errado.
- Alterar domínios além do hardening/residual.
- Executar specs dependentes antes do hardening de IDs.
- Promover para ACCEPTED apenas com compile.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Invalid ID Fallback Rules Audit and Hardening

## Summary
- Spec:
- Wave: WAVE 01
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

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing implementation handling:
- Missing dependency:
- Edge cases:
- Negative cases:

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
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. A implementação executar WAVE 02+ em massa antes da 01Q ou exceção humana explícita.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

C# if code changed:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile if Unity C# or tests changed:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests when test/validator logic is added:

```text
Unity Test Runner — EditMode, or local equivalent documented by the repo.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if fallback/helper/validator/test logic is created or changed; otherwise NO.
- Requires EditMode tests: YES if deterministic fallback/validator logic is added and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: YES if fixing a known invalid-ID load bug; otherwise NO.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; EditMode/validator PASS or NOT RUN with justified risk; execution report created; no ID renames; no schema change; no silent fallback for persisted invalid IDs.

---

## 27. Definition of Done

```text
Invalid ID policy defined.
Severity/fallback categories documented.
Representative load/registry paths audited.
No existing IDs renamed.
No save schema changed.
Validator/test added or residual risk documented.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Do not silently drop invalid persisted state.
Do not convert invalid ID to another valid ID without migration.
Do not persist Unity references.
Do not create duplicate registry/lookup architecture.
Do not declare ACCEPTED just because compile passes.
```

---

## 29. Notas para execução posterior

This spec prepares all future domain save specs. Any spec that persists an ID must reference this policy and state how invalid IDs are handled for that section.
