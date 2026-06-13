# SPEC — Save Restore Order Contract Audit and Hardening

> **Spec ID:** `01_spec_save_restore_order_contract_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0  
> **Type:** Runtime / Save / Validation / Hardening  
> **Domain:** Save / Load / Restore Order / Cross-scene State  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere `SaveManager`, `GameSaveData`, migrations, save providers, section ownership, stable IDs, event restore notifications, inventory/equipment/hotbar/farm/cave/player save DTOs.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/**/**SaveData.cs`, `Assets/_Game/Scripts/Core/Events/*Save*`, `docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md`.  
> **Depends on:**  
> - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md`  
> - `docs/specs/implementados/spec_save_002_schema_migration_v2.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`  
> **Blocks:**  
> - save section ownership registry;  
> - save provider architecture expansion;  
> - invalid ID fallback rules;  
> - inventory/equipment/hotbar save hardening;  
> - quest/player/cave/bestiary save specs.  
> **Scope:** auditar e documentar a ordem de restore/load existente, endurecer invariantes de restore order e criar validação/relatório sem substituir a arquitetura de save já implementada.  
> **Out of scope:** reescrever `SaveManager`, trocar formato JSON, criar migration nova sem necessidade comprovada, alterar scenes/prefabs, executar Play Mode humano ou resolver todos os save gaps de domínio.

---

# /speckit.specify

## 1. Contexto

O repo já possui sistema de save/load JSON cross-scene implementado parcialmente, com `SaveManager`, `GameSaveData`, múltiplos `*SaveData.cs`, schema version e migrations. Também há infraestrutura de migrations com registry, backup service, resultado estruturado e leitura por caminho comum.

Portanto esta spec não deve criar save/load do zero. O foco é **contrato de restore order**:

```text
mapear quais seções existem;
definir ordem segura de restore;
impedir dependência em eventos transitórios para restaurar estado primário;
identificar dependências entre seções;
validar round-trip determinístico quando praticável;
preparar a próxima spec de section ownership.
```

---

## 2. Problema

Save/load parcial pode funcionar em cenários simples, mas quebrar quando sistemas interdependentes forem restaurados em ordem errada.

Riscos concretos:

```text
restaurar inventory antes de registries/stable IDs estarem prontos;
restaurar equipment antes de item definitions;
restaurar hotbar antes de inventory/equipment;
restaurar player/cave/death antes de scene spawn/scene state;
restaurar quest ou bestiary antes de IDs/event contracts;
publicar eventos de restore antes da fonte primária estar completa;
aplicar migration depois de consumers lerem DTOs;
duplicar pickups/rewards/corpse/loot por restore parcial;
carregar save com seção ausente sem default explícito;
tratar compile como ACCEPTED sem round-trip ou risco residual.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter:

```text
restore order documentado e validado;
matriz de dependências entre save sections;
contrato para seções ausentes/defaults;
regra clara para post-restore notifications;
round-trip/EditMode test ou risco residual documentado;
execution report com gaps e próximos hardenings.
```

---

## 4. Fontes obrigatórias lidas

Para criar esta spec foram lidas/consideradas:

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md
docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md
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
```

Leitura condicional por domínio:

```text
Inventory/equipment/hotbar:
  docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

Quest state:
  docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

Cave/death/corpse:
  docs/design/gameplay/cave/CAVE_COMBAT_DEATH_DIRECTION.md, se existir; caso não exista, usar specs/refinements implementados do domínio.

Bestiary:
  docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- `spec_save_001_json_save_load_cross_scene.md` está implementada parcialmente.
- Evidência principal declarada: `Assets/_Game/Scripts/Save/SaveManager.cs`.
- Evidências complementares declaradas: `GameSaveData.cs` e `*SaveData.cs`.
- Save/load usa JSON em Application.persistentDataPath/saves/slot_1.json, schema version, current scene e DTOs simples.
- Pendência declarada: migration robusta e Play Mode completo.
- `spec_save_002_schema_migration_v2.md` entregou infraestrutura mínima de migrations: `ISaveMigration`, `SaveMigrationContext`, `SaveMigrationRegistry`, `SaveMigrationResult`, `SaveBackupService`.
- `SaveManager` usa `TryReadSaveWithMigration`, `ValidateAndNormalizeSave` e escrita segura `.tmp` conforme spec implementada.
- `SPEC_EXISTING_IMPLEMENTATION_AUDIT.md` classifica Save/load como PARTIAL / RESIDUAL.
```

Estado que precisa ser confirmado localmente:

```text
- ordem real de capture/save;
- ordem real de restore/load;
- se migrations rodam antes de consumers;
- quais sections existem em `GameSaveData`;
- defaults para seções ausentes;
- quem publica eventos após load/restore;
- quais managers consomem estado no Awake/Start/OnEnable;
- se existe teste de round-trip ou validator de save.
```

Regra:

```text
Não substituir SaveManager nem mudar formato do save sem spec separada.
```

---

## 6. User stories / engineering stories

```text
Como sistema de save, quero restaurar seções em ordem previsível para evitar estado parcial.
Como inventory/equipment/hotbar, quero que item definitions e inventory existam antes de bindings/equipment.
Como quest/farm/cave, quero que eventos pós-restore só disparem depois do estado primário estar carregado.
Como maintainer, quero uma matriz de dependência de save sections antes de adicionar novas sections.
Como agente executor, quero saber onde adicionar defaults/migrations sem quebrar saves existentes.
```

---

## 7. Escopo

Inclui:

```text
- auditar `SaveManager`, `GameSaveData`, `SaveData` DTOs e migrations existentes;
- mapear capture order e restore order reais;
- documentar matriz de dependências entre seções;
- definir contrato de defaults para seções ausentes;
- definir regra de post-restore event/notification;
- validar que migrations rodam antes de restore para consumers;
- adicionar ou ajustar teste/validator de round-trip/restore order quando praticável;
- criar execution report.
```

---

## 8. Fora de escopo

Não inclui:

```text
- reescrever SaveManager;
- trocar JSON por outro formato;
- criar novo save slot UI;
- criar cloud save;
- implementar todas as migrations futuras;
- alterar schema sem migration;
- alterar gameplay de inventory/equipment/quest/cave;
- alterar scenes/prefabs;
- executar Play Mode humano;
- mover specs para implementados.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo SaveManager.
Não criar segundo GameSaveData.
Não criar migration registry paralelo.
Não duplicar DTOs de seção existentes.
Não publicar evento para substituir restore direto de estado.
Não misturar section ownership registry nesta spec além de preparar dados para a próxima.
Não resolver save gaps de domínio sem spec própria.
```

---

## 10. Critérios de aceite

### 10.1 Restore order auditado

O execution report lista:

```text
Save section;
DTO/path;
Owner atual inferido;
Capture order;
Restore order;
Dependencies;
Default behavior if missing;
Post-restore event/notification, se houver;
Risk level.
```

### 10.2 Invariantes de restore order

A execução documenta e, se praticável, valida:

```text
migrations antes de restore para consumers;
stable ID registries disponíveis antes de resolver IDs;
inventory antes de equipment/hotbar bindings;
scene/spawn state antes de player/cave/death/corpse positioning;
quest/bestiary depois de IDs/event contracts disponíveis;
post-restore notifications depois de estado primário restaurado;
seção ausente usa default explícito, não null crash.
```

### 10.3 Teste/validator

Quando praticável, criar ou ajustar teste/validator para:

```text
round-trip mínimo de GameSaveData;
seção ausente normalizada por ValidateAndNormalizeSave;
schema futuro rejeitado;
legacy/missing schema tratado conforme migration policy;
restore order documentado corresponde à chamada real.
```

Se não for praticável, registrar `NOT RUN`/`NOT IMPLEMENTED` com motivo e risco residual.

### 10.4 Relatório

Criar:

```text
docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md
```

O relatório deve incluir:

```text
matriz de sections;
restore order real;
gaps e riscos;
testes/validators adicionados;
validações rodadas;
Testing Quality Gate;
próximos passos para section ownership registry.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Preservar arquitetura existente:

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/GameSaveData.cs
Assets/_Game/Scripts/Save/*SaveData.cs
Assets/_Game/Scripts/Save/Migrations/**
```

Possíveis complementos, se ainda não existirem:

```text
Assets/_Game/Tests/EditMode/Save/SaveRestoreOrderTests.cs
Assets/_Game/Scripts/Editor/Validation/ValidateSaveRestoreOrder.cs
```

Documento/relatório:

```text
docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
GameSaveData remains canonical root DTO.
Existing *SaveData DTOs remain canonical for their domain unless future domain spec changes them with migration.
DTOs must remain simple serializable data and must not persist Unity references.
```

### 12.2 Runtime contracts

```text
SaveManager remains canonical orchestrator for save/load.
Migrations must run before runtime systems consume loaded data.
Restore order must be explicit, documented and stable.
```

### 12.3 Event contracts

```text
Post-restore events are notifications only.
Events must not be required to reconstruct primary state.
If a GameLoaded/GameSaved event exists, it must fire after primary state has been restored/captured.
```

### 12.4 Save contracts

```text
Missing optional sections must normalize to safe defaults.
Unknown future schema must be rejected clearly.
Legacy schema must pass through migration path or be rejected with clear reason.
Changing save schema requires migration spec.
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
inventory/equipment/hotbar restore;
farm/world/cave/player restore;
quest/bestiary restore when present;
event notifications after save/load;
editor validation/EditMode tests.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Tests/EditMode/Save/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md
```

Leitura permitida, alteração apenas se estritamente necessária e reportada:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Core/Events/**
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
rg -n "class SaveManager|GameSaveData|SaveData|TryReadSaveWithMigration|ValidateAndNormalizeSave|LoadGame|SaveGame|Restore|Capture|Apply|GameLoaded|GameSaved" Assets/_Game/Scripts/Save Assets/_Game/Scripts
rg -n "SchemaVersion|CurrentSchemaVersion|SaveMigration|SaveBackupService|WriteTextSafely|\.tmp" Assets/_Game/Scripts/Save
rg -n "ISaveSectionProvider|SaveSection|CaptureState|RestoreState" Assets/_Game/Scripts
```

### Fase 1 — Matriz de sections

```text
1. Listar fields/sections de GameSaveData.
2. Mapear DTOs por domínio.
3. Mapear owners atuais e consumers.
4. Mapear capture/restore order real no SaveManager/providers.
5. Mapear defaults e migrations.
```

### Fase 2 — Hardening mínimo

```text
1. Adicionar teste/validator se não houver cobertura mínima.
2. Corrigir apenas gaps pequenos de null/default se forem locais e seguros.
3. Se mudança exigir schema migration, STOP e abrir spec separada.
```

### Fase 3 — Report

```text
1. Criar execution report.
2. Documentar matriz e riscos.
3. Indicar preparação para section ownership registry.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar SaveManager/GameSaveData/migrations.
3. Mapear sections e order real.
4. Validar invariantes.
5. Criar teste/validator quando praticável.
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
  - save section ownership registry;
  - save provider architecture;
  - any domain save DTO change.
- Shared files/systems that require lock:
  - SaveManager;
  - GameSaveData;
  - migrations;
  - save providers;
  - domain DTOs.
- Reason:
  - Restore order is cross-cutting and can silently break many systems.

---

## 19. Impacto em save/load

```text
Does this change save schema? SHOULD BE NO. If YES, STOP and require migration spec.
Does this add a save section? NO.
Does this require migration? NO unless audit finds unavoidable schema issue; then separate spec.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: NO.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: NO, unless existing save events are audited and subscribers are touched.
```

Post-restore events must remain notifications, not primary restore path.

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO for pure save/order tests; YES only if runtime scene restore behavior is touched, then DEFERRED_TO_FINAL_VALIDATION.
Human validation timing: NOT REQUIRED unless scene/runtime restore scenario is added; then DEFERRED_TO_FINAL_VALIDATION.
```

---

## 22. Riscos técnicos

```text
Risco: teste de restore order não refletir scene lifecycle real.
Mitigação: separar pure DTO/order tests de cenário final humano.

Risco: corrigir order quebrar compatibilidade de save antigo.
Mitigação: não mudar schema/order sem report e migration consideration.

Risco: seção ausente ser normalizada de forma errada.
Mitigação: documentar defaults e cobrir em teste/validator.

Risco: event notification usado como restore.
Mitigação: post-restore events só depois do estado primário.
```

---

## 23. Rollback

```text
Remover teste/validator criado.
Reverter alteração pontual no SaveManager/DTOs, se houver.
Remover execution report.
Não há rollback de scene/prefab/assets porque não devem ser alterados.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar `SaveManager`, `GameSaveData`, DTOs e migrations.
- [ ] T003 — Mapear sections, owners inferidos, capture order e restore order.
- [ ] T004 — Mapear defaults para seções ausentes e migration timing.
- [ ] T005 — Identificar post-restore events/notifications e riscos.
- [ ] T006 — Criar/ajustar teste EditMode ou validator mínimo quando praticável.
- [ ] T007 — Rodar validações obrigatórias ou registrar NOT RUN.
- [ ] T008 — Criar `docs/validation/01_spec_save_restore_order_contract_runtime_execution_report.md`.
- [ ] T009 — Registrar risco residual e inputs para `01_spec_save_section_ownership_registry.md`.

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

- Changed deterministic logic: YES, if validator/test or restore/default logic is created/changed; otherwise NO.
- Requires EditMode tests: YES if restore/default/migration timing logic is changed and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO for pure DTO/order tests; YES only if scene/runtime restore behavior is touched, then DEFERRED_TO_FINAL_VALIDATION.
- Requires regression test: YES if fixing a known save/restore bug; otherwise NO.
- Human validation timing: NOT REQUIRED unless scene/runtime restore scenario is added; then DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; EditMode tests/validator PASS or NOT RUN with justified risk; execution report created; no schema change without migration spec; no SaveManager replacement.

---

## 27. Definition of Done

```text
SaveManager/GameSaveData audited and preserved.
Restore order matrix created in execution report.
Section defaults and migration timing documented.
Post-restore notification rules documented.
No save schema change made without migration spec.
No second SaveManager/GameSaveData created.
Validator/test added or residual risk documented.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não substituir SaveManager.
Não quebrar schema antigo.
Não persistir Unity references.
Não depender de evento para restaurar estado primário.
Não adicionar save section sem ownership registry.
Não rodar em paralelo com specs que alterem save/events/IDs.
Não declarar ACCEPTED apenas porque build compilou.
```

---

## 29. Notas para execução posterior

Esta spec prepara diretamente:

```text
01_spec_save_section_ownership_registry.md
01_spec_save_provider_architecture_runtime.md
01_spec_invalid_id_fallback_rules.md
```

Ela deve produzir insumos objetivos para a próxima spec, especialmente:

```text
lista de save sections;
owners inferidos;
dependências;
defaults;
riscos;
ordem recomendada de restore.
```
