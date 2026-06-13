# SPEC — Save Section Ownership Registry Audit and Hardening

> **Spec ID:** `01_spec_save_section_ownership_registry`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0  
> **Type:** Runtime / Save / Registry / Validation / Hardening  
> **Domain:** Save / Section Ownership / Providers / Restore Coordination  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere `SaveManager`, `GameSaveData`, `*SaveData.cs`, migrations, save providers, domain save DTOs, stable IDs ou event restore notifications.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/**/**SaveData.cs`, `Assets/_Game/Scripts/**/**SaveProvider*.cs`, `docs/validation/01_spec_save_section_ownership_registry_execution_report.md`.  
> **Depends on:**  
> - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`  
> - `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md`  
> - `docs/specs/implementados/spec_save_002_schema_migration_v2.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> **Blocks:**  
> - save provider architecture expansion;  
> - invalid ID fallback rules;  
> - inventory/equipment/hotbar save hardening;  
> - quest state save/load;  
> - player/cave/death/bestiary save specs.  
> **Scope:** criar ou consolidar um registry documental/runtime leve de ownership de save sections, usando a ordem de restore auditada, sem reescrever a arquitetura de save existente.  
> **Out of scope:** substituir `SaveManager`, migrar todas as sections para providers, alterar schema sem migration, implementar save gaps de domínio, alterar scenes/prefabs ou executar Play Mode humano.

---

# /speckit.specify

## 1. Contexto

A spec anterior da WAVE 01 (`01_spec_save_restore_order_contract_runtime.md`) exige que a execução produza uma matriz de save sections, owners inferidos, dependências, defaults e ordem recomendada de restore.

Esta spec é o próximo passo: transformar essa matriz em um contrato canônico de ownership.

O objetivo não é migrar tudo para uma arquitetura nova de providers em uma única etapa. O objetivo é impedir que novas specs adicionem save sections sem declarar:

```text
quem é dono da seção;
quem captura;
quem restaura;
quais dependências existem;
qual ordem segura;
qual default para ausência;
qual política de migration;
quais validações são obrigatórias.
```

---

## 2. Problema

Sem ownership registry de save sections, cada domínio pode evoluir save/load de forma isolada.

Riscos concretos:

```text
duas specs editarem a mesma section;
um domínio criar DTO sem owner claro;
provider novo capturar/restaurar estado fora da ordem segura;
section nova não ter default quando ausente;
section ser restaurada antes de IDs/registries/dependencies;
future migration não saber qual owner deve migrar section;
SaveManager virar ponto de acoplamento crescente sem mapa de ownership;
invalid ID fallback não saber qual section/domínio falhou.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter um registry de ownership de save sections que sirva como fonte de verdade para specs futuras de save.

Resultado esperado:

```text
cada save section conhecida tem owner declarado;
capture/restore/default/migration responsibility está documentada;
dependências entre sections estão explícitas;
novas specs sabem como declarar nova section;
provider architecture futura tem caminho incremental;
SaveManager existente é preservado.
```

---

## 4. Fontes obrigatórias lidas

Para criar esta spec foram lidas/consideradas:

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md
docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md
docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
docs/specs/implementados/spec_save_002_schema_migration_v2.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
```

A execução futura deve ler também:

```text
CLAUDE.md
AGENTS.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md
```

Leitura condicional por domínio:

```text
Inventory/equipment/hotbar:
  docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

Quest state:
  docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

Bestiary:
  docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- Save/load JSON cross-scene existe parcialmente via `SaveManager`, `GameSaveData` e `*SaveData.cs`.
- Migrations existem parcialmente via `ISaveMigration`, `SaveMigrationContext`, `SaveMigrationRegistry`, `SaveMigrationResult` e `SaveBackupService`.
- `SPEC_EXISTING_IMPLEMENTATION_AUDIT.md` registra Save/load como PARTIAL / RESIDUAL.
- `01_spec_save_restore_order_contract_runtime.md` foi criada para produzir matriz de sections/owners/dependencies.
```

Estado não comprovado pelo conector e que deve ser auditado localmente:

```text
- existência real de `ISaveSectionProvider` ou providers de section;
- existência de provider piloto, como HotbarSectionProvider;
- localização de section ownership atual, se já houver;
- quais sections estão hardcoded no SaveManager;
- quais domains já têm capture/restore isolável.
```

Regra:

```text
Não assumir que provider architecture existe sem auditoria local. Se existir, complementar. Se não existir, criar registry documental/runtime leve sem migração massiva.
```

---

## 6. User stories / engineering stories

```text
Como maintainer, quero saber quem é dono de cada save section antes de aceitar nova spec de domínio.
Como SaveManager, quero uma lista clara de sections e dependências para manter restore order previsível.
Como executor de spec futura, quero saber se posso editar uma section ou se preciso de lock de outro domínio.
Como migration author, quero saber qual owner responde por migration/default de cada section.
Como sistema de invalid ID fallback, quero mapear falha de ID para section e owner correto.
```

---

## 7. Escopo

Inclui:

```text
- consumir o execution report da restore-order spec;
- auditar sections existentes em `GameSaveData` e DTOs;
- auditar se já existem `ISaveSectionProvider`/providers;
- criar ou consolidar registry de ownership de save sections;
- definir campos obrigatórios por section;
- definir processo para nova section futura;
- criar validator/teste quando praticável;
- criar execution report desta spec.
```

---

## 8. Fora de escopo

Não inclui:

```text
- migrar todas as sections para providers;
- reescrever SaveManager;
- alterar formato JSON;
- criar nova migration sem necessidade explícita;
- alterar domain DTOs fora de ajuste mínimo de metadata/registry;
- implementar save/load de domínio ainda ausente;
- alterar scenes/prefabs/assets;
- executar Play Mode humano;
- atualizar SPEC_EXECUTION_ORDER.md.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo SaveManager.
Não criar segundo GameSaveData.
Não criar registry de ownership separado se já houver um equivalente.
Não criar provider paralelo para section que já tem provider canônico.
Não mover ownership para docs-only se o runtime já tem contrato claro; nesse caso documentar e validar o existente.
Não adicionar section sem declarar owner/dependencies/defaults.
```

---

## 10. Critérios de aceite

### 10.1 Registry de ownership criado ou consolidado

A execução deve criar/consolidar uma fonte canônica, podendo ser:

```text
runtime/data registry leve em `Assets/_Game/Scripts/Save/**`;
documento canônico em `docs/specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md`;
ou ambos, se fizer sentido e não duplicar fonte.
```

Critério obrigatório:

```text
A fonte escolhida precisa ser explicitamente referenciada no execution report e nas próximas specs de save.
```

### 10.2 Campos obrigatórios por save section

Cada section conhecida deve declarar:

```text
section name;
DTO type/path;
owner domain;
capture owner;
restore owner;
default provider/behavior;
migration owner;
dependencies before restore;
dependents after restore;
can be restored independently? YES/NO;
post-restore notification, if any;
risk level.
```

### 10.3 Processo para nova section

O registry deve definir que nova section futura precisa declarar:

```text
owner;
DTO;
default when missing;
schema/migration impact;
restore order slot;
validation strategy;
Testing Quality Gate;
locks.
```

### 10.4 Validação

Quando praticável, criar validator/teste para:

```text
section com owner ausente;
section com DTO desconhecido;
section sem default;
section com dependency ausente;
section nova sem migration/default policy;
registry divergente de GameSaveData fields.
```

Se não for praticável, registrar NOT RUN/NOT IMPLEMENTED com motivo e risco residual.

### 10.5 Relatório

Criar:

```text
docs/validation/01_spec_save_section_ownership_registry_execution_report.md
```

O relatório deve incluir:

```text
fonte canônica criada/consolidada;
matriz de ownership;
validators/testes;
validações rodadas;
risco residual;
inputs para save provider architecture e invalid ID fallback.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possíveis arquivos alvo, dependendo da auditoria local:

```text
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs
Assets/_Game/Scripts/Save/SaveSectionOwnershipEntry.cs
Assets/_Game/Scripts/Save/ISaveSectionProvider.cs, se já existir ou se criação mínima for necessária
Assets/_Game/Scripts/Editor/Validation/ValidateSaveSectionOwnership.cs
Assets/_Game/Tests/EditMode/Save/SaveSectionOwnershipTests.cs
docs/specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md
docs/validation/01_spec_save_section_ownership_registry_execution_report.md
```

Regra arquitetural:

```text
Preferir consolidar o contrato existente se ele já existir.
Se não existir, criar o mínimo necessário para evitar drift, sem migrar todas as sections para providers.
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
GameSaveData remains canonical root DTO.
Each section must have exactly one declared owner.
Ownership registry must not duplicate save payload data; it is metadata/contract.
```

### 12.2 Runtime contracts

```text
SaveManager remains current orchestrator.
Section ownership informs capture/restore responsibilities and future provider extraction.
Providers, if present, must align with registry ownership.
```

### 12.3 Event contracts

```text
Post-restore notifications must reference section ownership when scoped by domain.
Events do not define ownership; ownership defines who may publish/consume post-restore signals.
```

### 12.4 Save contracts

```text
New save sections require ownership entry before implementation.
Missing section defaults must be defined by owner.
Migration owner must be declared for versioned changes.
Invalid ID fallback must report section and owner when possible.
```

### 12.5 UI contracts

N/A — sem UI.

---

## 13. Sistemas afetados

```text
SaveManager;
GameSaveData;
SaveData DTOs;
Save migrations;
future save providers;
section defaults;
invalid ID fallback;
editor validation/EditMode tests;
documentation/spec governance.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Tests/EditMode/Save/**
Assets/_Game/Scripts/Editor/Validation/**
docs/specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md
docs/validation/01_spec_save_section_ownership_registry_execution_report.md
```

Leitura permitida, alteração apenas se estritamente necessária e reportada:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Bestiary/**
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

Rodar buscas locais:

```bash
rg -n "ISaveSectionProvider|SaveSectionProvider|SectionOwnership|SaveSectionOwnership|SaveSection|CaptureState|RestoreState" Assets/_Game/Scripts
rg -n "class GameSaveData|\bSaveData\b|public .*SaveData|\[Serializable\].*Save" Assets/_Game/Scripts/Save Assets/_Game/Scripts
rg -n "TryReadSaveWithMigration|ValidateAndNormalizeSave|LoadGame|SaveGame|Restore|Capture|Apply" Assets/_Game/Scripts/Save Assets/_Game/Scripts
```

### Fase 1 — Ownership matrix

```text
1. Ler report da restore-order spec.
2. Listar sections de GameSaveData.
3. Inferir owner atual por domain/DTO/consumer.
4. Validar dependências e defaults.
5. Identificar gaps UNKNOWN.
```

### Fase 2 — Registry mínimo

```text
1. Consolidar fonte canônica de ownership.
2. Não migrar tudo para provider architecture.
3. Adicionar metadata/entries suficientes para sections existentes.
4. Garantir que novas sections futuras tenham processo claro.
```

### Fase 3 — Validação

```text
1. Criar validator/teste se praticável.
2. Rodar validações.
3. Criar execution report.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Confirmar que restore-order report existe.
3. Auditar providers/ownership existentes.
4. Construir matriz de ownership.
5. Criar/consolidar registry mínimo.
6. Criar validator/teste quando praticável.
7. Rodar validações obrigatórias.
8. Criar execution report.
9. Não atualizar execution order nem promover specs.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_01_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - stable IDs hardening;
  - event contracts hardening;
  - save restore order;
  - save provider architecture;
  - invalid ID fallback;
  - any domain save DTO change.
- Shared files/systems that require lock:
  - SaveManager;
  - GameSaveData;
  - SaveData DTOs;
  - providers;
  - ownership registry.
- Reason:
  - Section ownership is the coordination point for future save specs and cannot drift.

---

## 19. Impacto em save/load

```text
Does this change save schema? SHOULD BE NO. If YES, STOP and require migration spec.
Does this add a save section? NO.
Does this require migration? NO.
Does this persist Unity references? MUST BE NO.
```

Ownership metadata must not become saved payload unless a separate schema/migration spec approves it.

---

## 20. Impacto em eventos

```text
Adds events: NO.
Changes existing events: NO.
Requires unsubscribe pattern: NO.
```

If owner-specific post-restore events are proposed, move them to event/save integration spec.

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

---

## 22. Riscos técnicos

```text
Risco: registry virar segunda fonte divergente do GameSaveData.
Mitigação: validator deve comparar registry com sections reais quando praticável.

Risco: owner inferido errado.
Mitigação: marcar UNKNOWN quando evidência for insuficiente.

Risco: transformar esta spec em provider migration massiva.
Mitigação: escopo é ownership, não migration para providers.

Risco: bloquear futuras specs com registry rígido demais.
Mitigação: permitir owner UNKNOWN temporário com risco documentado, mas bloquear ACCEPTED para section nova sem owner.
```

---

## 23. Rollback

```text
Remover registry/metadata criado.
Remover validator/teste criado.
Remover documento canônico de ownership se criado.
Remover execution report.
Nenhum save payload/schema deve ter sido alterado.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Confirmar existência do report da restore-order spec.
- [ ] T003 — Auditar `GameSaveData`, DTOs e providers/ownership existentes.
- [ ] T004 — Criar matriz de sections com owner/capture/restore/default/migration/dependencies.
- [ ] T005 — Criar/consolidar fonte canônica de ownership.
- [ ] T006 — Criar/ajustar validator ou EditMode test mínimo quando praticável.
- [ ] T007 — Rodar validações obrigatórias ou registrar NOT RUN.
- [ ] T008 — Criar `docs/validation/01_spec_save_section_ownership_registry_execution_report.md`.
- [ ] T009 — Registrar inputs para provider architecture e invalid ID fallback.

---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
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

EditMode tests quando teste for criado:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, if runtime registry/validator/test logic is created or changed; otherwise NO.
- Requires EditMode tests: YES if deterministic ownership validator/registry logic is changed and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: NO unless fixing a known save ownership/default bug.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; validator/test PASS or NOT RUN with justified risk; ownership registry/report created; no save schema change; no SaveManager replacement.

---

## 27. Definition of Done

```text
Save sections ownership registry created or consolidated.
Each known section has owner/default/migration/dependency status documented or UNKNOWN with risk.
No save schema changed.
No second SaveManager/GameSaveData created.
Validator/test added or residual risk documented.
Execution report created.
Inputs for provider architecture and invalid ID fallback recorded.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não substituir SaveManager.
Não alterar schema sem migration.
Não adicionar section sem owner.
Não duplicar provider/ownership contracts.
Não persistir ownership metadata no save payload sem spec própria.
Não rodar em paralelo com specs que alterem save/events/IDs/domain DTOs.
Não declarar ACCEPTED apenas porque build compilou.
```

---

## 29. Notas para execução posterior

Esta spec prepara diretamente:

```text
01_spec_save_provider_architecture_runtime.md
01_spec_invalid_id_fallback_rules.md
```

Specs futuras que criarem save section devem atualizar ou referenciar o ownership registry antes de implementação.
