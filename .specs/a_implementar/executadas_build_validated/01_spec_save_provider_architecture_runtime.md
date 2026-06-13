# SPEC — Save Provider Architecture Incremental Hardening

> **Spec ID:** `01_spec_save_provider_architecture_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P1  
> **Type:** Runtime / Save / Architecture / Validation / Hardening  
> **Domain:** Save / Providers / Section Ownership / Incremental Architecture  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere `SaveManager`, `GameSaveData`, migrations, save section ownership, domain save DTOs, invalid ID fallback, scene restore, inventory/equipment/hotbar/farm/cave/player save.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/**/**SaveData.cs`, `Assets/_Game/Scripts/**/**SaveProvider*.cs`, `.specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md`, `docs/validation/01_spec_save_provider_architecture_runtime_execution_report.md`.  
> **Depends on:**  
> - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `.specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `.specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `.specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`  
> - `.specs/a_implementar/01_spec_save_section_ownership_registry.md`  
> - `.specs/implementados/spec_save_001_json_save_load_cross_scene.md`  
> - `.specs/implementados/spec_save_002_schema_migration_v2.md`  
> - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> **Blocks:**  
> - broad save provider expansion;  
> - invalid ID fallback integration by section/owner;  
> - domain save hardening for inventory/equipment/hotbar/quest/player/cave/bestiary;  
> - future save migration ownership work.  
> **Scope:** consolidar uma arquitetura incremental de save section providers, baseada no ownership registry, sem reescrever `SaveManager` e sem migrar todas as seções de uma vez.  
> **Out of scope:** substituir `SaveManager`, mudar formato JSON, alterar schema sem migration, converter todos os domínios para providers, alterar scenes/prefabs/assets, executar Play Mode humano ou resolver save de todos os domínios.

---

# /speckit.specify

## 1. Contexto

O sistema de save/load já existe e deve ser preservado. A auditoria documental apontou `SaveManager`, `GameSaveData`, DTOs e migrations como base parcial/residual, não inexistente.

As specs anteriores da WAVE 01 preparam:

```text
stable IDs endurecidos;
event contracts endurecidos;
restore order auditado;
section ownership registry criado/consolidado.
```

Esta spec vem depois disso. Ela não deve fazer refactor massivo. O objetivo é criar ou consolidar a camada incremental que permite extrair seções para providers com segurança, uma de cada vez, mantendo `SaveManager` como orquestrador.

---

## 2. Problema

Sem uma arquitetura incremental de providers, o `SaveManager` tende a acumular lógica de todos os domínios.

Riscos concretos:

```text
SaveManager virar god object cada vez maior;
providers serem criados com interfaces incompatíveis;
section ownership divergir da implementação real;
ordem de restore ser quebrada por provider novo;
provider capturar/restaurar section sem default/migration policy;
provider persistir Unity references;
especs de domínio editarem SaveManager diretamente em paralelo;
provider architecture virar refactor massivo e arriscado.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter um contrato incremental de save providers pronto para ser usado por specs futuras.

Resultado esperado:

```text
SaveManager continua orquestrador;
providers, se existentes, são auditados e alinhados ao ownership registry;
se providers não existirem, criar interface/contrato mínimo somente se seguro;
existe pelo menos um provider piloto ou um adapter documental claramente definido;
restore order e section ownership permanecem fonte de controle;
execution report define quais sections podem ser migradas futuramente e em qual ordem.
```

---

## 4. Fontes obrigatórias lidas

Para criar esta spec foram consideradas:

```text
.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
.specs/a_implementar/01_spec_stable_ids_registry_runtime.md
.specs/a_implementar/01_spec_game_event_contracts_runtime.md
.specs/a_implementar/01_spec_save_restore_order_contract_runtime.md
.specs/a_implementar/01_spec_save_section_ownership_registry.md
.specs/implementados/spec_save_001_json_save_load_cross_scene.md
.specs/implementados/spec_save_002_schema_migration_v2.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
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
.specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md, se criado pela spec 01.04
docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md
docs/validation/01_spec_save_section_ownership_registry_execution_report.md
```

Leitura condicional por provider piloto:

```text
Hotbar/inventory/equipment:
  docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

Quest/player/cave/bestiary:
  direction específico do domínio, se o provider piloto tocar esse domínio.
```

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- Save/load JSON cross-scene existe parcialmente via SaveManager, GameSaveData e *SaveData.cs.
- Migrations existem parcialmente via ISaveMigration, SaveMigrationContext, SaveMigrationRegistry, SaveMigrationResult e SaveBackupService.
- A auditoria documental classificou Save/load como PARTIAL / RESIDUAL.
- A spec 01.04 foi criada para produzir ownership registry.
```

Estado não comprovado pelo conector e que deve ser auditado localmente:

```text
- se `ISaveSectionProvider` já existe;
- se `HotbarSectionProvider` ou outro provider piloto já existe;
- se SaveManager já invoca providers;
- se providers existentes têm Capture/Restore/Default/Migration contract;
- se há tests/validators para providers;
- se algum domínio já usa provider parcial fora de SaveManager.
```

Regra:

```text
Não assumir provider architecture existente sem auditoria local.
Se existir, consolidar o existente.
Se não existir, criar apenas o contrato mínimo ou registrar backlog se for arriscado.
```

---

## 6. User stories / engineering stories

```text
Como SaveManager, quero delegar sections gradualmente sem perder controle de ordem de restore.
Como domínio futuro, quero implementar provider sem editar SaveManager diretamente.
Como maintainer, quero garantir que provider respeita ownership/default/migration policy.
Como executor, quero saber qual section pode virar provider piloto com menor risco.
Como revisor, quero impedir provider que persista Unity reference ou ignore migration/default.
```

---

## 7. Escopo

Inclui:

```text
- auditar providers/interfaces existentes;
- auditar integração atual do SaveManager com providers, se houver;
- definir contrato mínimo de provider alinhado ao ownership registry;
- criar ou consolidar interface/provider base somente se seguro;
- escolher no máximo uma section piloto de baixo risco, se a auditoria local indicar segurança;
- criar validator/teste de provider quando praticável;
- criar execution report com plano incremental de migração futura.
```

---

## 8. Fora de escopo

Não inclui:

```text
- migrar todas as sections para providers;
- remover lógica atual do SaveManager em massa;
- alterar formato JSON;
- alterar schema sem migration;
- alterar domain gameplay;
- alterar scenes/prefabs/assets;
- executar Play Mode humano;
- implementar cloud/multi-slot UI;
- resolver invalid ID fallback, salvo preparar integração.
```

---

## 9. Regras de não duplicação

```text
Não criar segunda interface provider se já houver `ISaveSectionProvider` equivalente.
Não criar provider paralelo para section já coberta.
Não criar registry de providers divergente do ownership registry.
Não criar segundo SaveManager.
Não mover ownership para provider se o registry não estiver consolidado.
Não criar provider piloto que exija mudança de schema.
Não transformar provider em fonte primária diferente de GameSaveData sem spec separada.
```

---

## 10. Critérios de aceite

### 10.1 Auditoria de provider architecture

A execução lista:

```text
Interfaces provider existentes;
providers existentes;
sections cobertas;
integração com SaveManager;
contratos de Capture/Restore/Default/Migration;
gaps;
riscos.
```

### 10.2 Contrato mínimo de provider

Se provider architecture existir, ela deve ser documentada e, se necessário, ajustada para declarar:

```text
section id/name;
owner;
DTO type;
capture entrypoint;
restore entrypoint;
default behavior;
dependencies before restore;
post-restore notification policy;
validation method.
```

Se provider architecture não existir, a execução deve escolher uma das opções:

```text
A) criar interface/contrato mínimo se for seguro e pequeno;
B) criar apenas documento/adapter plan e registrar provider runtime como backlog técnico.
```

A decisão precisa ser justificada no execution report.

### 10.3 Provider piloto

Se criar provider piloto, ele deve:

```text
cobrir no máximo uma section;
não alterar schema;
não alterar gameplay;
não persistir Unity references;
ter teste/validator ou risco residual documentado;
ser reversível sem quebrar save existente.
```

Se não criar provider piloto, registrar motivo e próximos passos.

### 10.4 Validação

Quando praticável, criar ou ajustar teste/validator para:

```text
provider declarado sem ownership;
provider com section desconhecida;
provider que ignora dependency;
provider que retorna DTO null sem default;
provider divergente de GameSaveData/ownership registry;
provider que tentaria persistir Unity reference.
```

### 10.5 Relatório

Criar:

```text
docs/validation/01_spec_save_provider_architecture_runtime_execution_report.md
```

O relatório deve incluir:

```text
auditoria de providers existentes;
contrato provider final;
provider piloto, se criado;
validações rodadas;
NOT RUN entries;
risco residual;
sections candidatas para migração futura;
inputs para invalid ID fallback.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possíveis arquivos alvo, dependendo da auditoria local:

```text
Assets/_Game/Scripts/Save/ISaveSectionProvider.cs
Assets/_Game/Scripts/Save/SaveSectionProviderContext.cs
Assets/_Game/Scripts/Save/SaveSectionProviderResult.cs
Assets/_Game/Scripts/Save/Providers/<Domain>SectionProvider.cs
Assets/_Game/Scripts/Editor/Validation/ValidateSaveSectionProviders.cs
Assets/_Game/Tests/EditMode/Save/SaveSectionProviderTests.cs
docs/validation/01_spec_save_provider_architecture_runtime_execution_report.md
```

Arquitetura desejada:

```text
SaveManager
  remains orchestrator
  uses ownership/restore order metadata
  may call providers incrementally

ISaveSectionProvider
  describes one owned section
  captures section DTO
  restores section DTO
  exposes dependencies/default validation

Ownership Registry
  remains source of owner/dependency/default metadata
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
GameSaveData remains canonical root DTO.
Providers operate on existing section DTOs.
Provider metadata must not become save payload unless separate schema spec approves.
```

### 12.2 Runtime contracts

```text
SaveManager remains canonical orchestrator.
Provider invocation order must respect restore order and ownership registry.
Providers must be idempotent for restore when possible.
Providers must not depend on Unity scene objects unless explicitly documented and deferred to PlayMode/final scenario.
```

### 12.3 Event contracts

```text
Providers should not publish events during primary restore unless SaveManager explicitly enters post-restore phase.
Post-restore notifications remain owned by SaveManager/event integration, not individual providers by default.
```

### 12.4 Save contracts

```text
No schema change by default.
No new save section by default.
No Unity references in saved DTOs.
Missing section default remains owner responsibility.
Migration remains migration registry responsibility.
```

### 12.5 UI contracts

N/A — sem UI.

---

## 13. Sistemas afetados

```text
SaveManager;
GameSaveData;
SaveData DTOs;
save providers;
section ownership registry;
restore order;
migrations;
invalid ID fallback future integration;
editor validation/EditMode tests.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Tests/EditMode/Save/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/01_spec_save_provider_architecture_runtime_execution_report.md
```

Leitura permitida, alteração apenas se estritamente necessária e reportada:

```text
.specs/SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY.md
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
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

Rodar buscas locais:

```bash
rg -n "ISaveSectionProvider|SaveSectionProvider|SectionProvider|CaptureState|RestoreState|CaptureSection|RestoreSection|SaveSectionProviderContext|SaveSectionProviderResult" Assets/_Game/Scripts
rg -n "class SaveManager|GameSaveData|SaveData|ValidateAndNormalizeSave|TryReadSaveWithMigration|LoadGame|SaveGame|Restore|Capture" Assets/_Game/Scripts/Save Assets/_Game/Scripts
rg -n "SaveSectionOwnership|SectionOwnership|SPEC_SAVE_SECTION_OWNERSHIP_REGISTRY|owner|restore order" docs Assets/_Game/Scripts/Save
```

### Fase 1 — Decisão arquitetural

```text
1. Se provider contract existir, consolidar e validar o existente.
2. Se não existir, avaliar se interface mínima é segura.
3. Se criação de interface exigir refactor grande, STOP parcial e registrar backlog técnico.
4. Não migrar múltiplas sections.
```

### Fase 2 — Provider piloto opcional

```text
1. Escolher no máximo uma section de baixo risco.
2. Não alterar schema.
3. Não alterar gameplay.
4. Garantir rollback simples.
5. Criar teste/validator se praticável.
```

### Fase 3 — Report

```text
1. Criar execution report.
2. Registrar arquitetura final e gaps.
3. Listar sections candidatas para migração futura.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Confirmar ownership registry/report existente.
3. Auditar providers/SaveManager.
4. Tomar decisão: consolidate existing / create minimal / defer.
5. Implementar apenas o mínimo aprovado pela auditoria.
6. Rodar validações obrigatórias.
7. Criar execution report.
8. Não atualizar execution order nem promover specs.
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
  - section ownership registry;
  - invalid ID fallback;
  - any domain save DTO change.
- Shared files/systems that require lock:
  - SaveManager;
  - provider contracts;
  - ownership registry;
  - GameSaveData/DTOs if touched.
- Reason:
  - Provider architecture changes cross-cutting save orchestration.

---

## 19. Impacto em save/load

```text
Does this change save schema? SHOULD BE NO. If YES, STOP and require migration spec.
Does this add a save section? NO.
Does this require migration? NO.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: NO.
Changes existing events: NO.
Requires unsubscribe pattern: NO.
```

Provider-level post-restore events are out of scope unless already existing and only documented.

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO for provider contract/test-only changes; YES only if runtime scene-bound restore changes occur, then DEFERRED_TO_FINAL_VALIDATION.
Human validation timing: NOT REQUIRED unless scene-bound restore behavior is touched; then DEFERRED_TO_FINAL_VALIDATION.
```

---

## 22. Riscos técnicos

```text
Risco: provider extraction virar refactor massivo.
Mitigação: limitar a no máximo um provider piloto ou contrato mínimo.

Risco: provider divergir do ownership registry.
Mitigação: validator/teste compara provider metadata com ownership quando praticável.

Risco: provider mudar ordem de restore.
Mitigação: SaveManager continua orquestrando ordem.

Risco: provider depender de scene object.
Mitigação: bloquear ou marcar como runtime scene-bound com validação final deferida.
```

---

## 23. Rollback

```text
Remover interface/context/result/provider piloto criado.
Reverter qualquer integração no SaveManager.
Remover validator/teste criado.
Remover execution report.
Nenhum save schema, scene, prefab ou asset deve ter sido alterado.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Confirmar existência do ownership registry/report da 01.04.
- [ ] T003 — Auditar providers/interfaces existentes e integração com SaveManager.
- [ ] T004 — Decidir consolidate existing / create minimal / defer with backlog.
- [ ] T005 — Criar ou ajustar contrato mínimo de provider se seguro.
- [ ] T006 — Criar no máximo um provider piloto se seguro e útil.
- [ ] T007 — Criar/ajustar validator ou EditMode test mínimo quando praticável.
- [ ] T008 — Rodar validações obrigatórias ou registrar NOT RUN.
- [ ] T009 — Criar `docs/validation/01_spec_save_provider_architecture_runtime_execution_report.md`.
- [ ] T010 — Registrar sections candidatas para migração futura e inputs para invalid ID fallback.

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

- Changed deterministic logic: YES, if provider contract/validator/test logic is created or changed; otherwise NO.
- Requires EditMode tests: YES if provider orchestration/validator logic is changed and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO for provider contract/test-only changes; YES only if scene-bound restore behavior is touched, then DEFERRED_TO_FINAL_VALIDATION.
- Requires regression test: YES if fixing a known provider/save orchestration bug; otherwise NO.
- Human validation timing: NOT REQUIRED unless scene-bound restore behavior is touched; then DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; EditMode tests/validator PASS or NOT RUN with justified risk; execution report created; no schema change; no SaveManager replacement; no mass provider migration.

---

## 27. Definition of Done

```text
Provider architecture audited.
Existing provider contract consolidated or minimal path documented/created safely.
At most one provider piloto created, if any.
SaveManager preserved as orchestrator.
No save schema changed.
No mass migration to providers performed.
Validator/test added or residual risk documented.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não substituir SaveManager.
Não alterar schema sem migration.
Não migrar todas as sections para providers.
Não criar provider divergente de ownership registry.
Não persistir Unity references.
Não publicar eventos durante restore primário por provider.
Não declarar ACCEPTED apenas porque build compilou.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada depois de `01_spec_save_section_ownership_registry.md`.

Ela prepara:

```text
01_spec_invalid_id_fallback_rules.md
domain save specs de inventory/equipment/hotbar/quest/player/cave/bestiary
future provider migrations por section
```
