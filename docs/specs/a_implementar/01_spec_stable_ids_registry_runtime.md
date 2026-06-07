# SPEC — Stable IDs Registry Audit and Hardening

> **Spec ID:** `01_spec_stable_ids_registry_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0  
> **Type:** Runtime / Data / Validation / Hardening  
> **Domain:** Core / Data / Stable IDs / Registries  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere save schema, event contracts, ScriptableObject registries, item data, enemy data, quest IDs, NPC IDs, cave IDs ou outros stable IDs.  
> **Repo lock scope:** `Assets/_Game/Scripts/Core/Data/**`, `Assets/_Game/Data/**`, qualquer `*DataSO.cs`, qualquer registry global, `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`, `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`.  
> **Depends on:**  
> - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> **Blocks:**  
> - event contracts hardening;  
> - save/load section ownership hardening;  
> - quest, inventory, enemy, NPC, bestiary and cave specs that persist or reference IDs.  
> **Scope:** auditar e endurecer o sistema existente de stable IDs/registries sem recriar a arquitetura já implementada.  
> **Out of scope:** criar novo registry paralelo, alterar IDs persistidos existentes sem migration, reescrever SaveManager, criar sistemas de gameplay, alterar cenas/prefabs ou validar Play Mode humano.

---

# /speckit.specify

## 1. Contexto

O roadmap original previa `01_spec_stable_ids_registry_runtime.md` como fundação. A auditoria documental mostrou que o projeto já tem uma base de IDs e registries implementada: `IIdentifiedData`, `IDataRegistry`, `DataRegistrySO` e dados em `Assets/_Game/Data/**`. Portanto, esta spec não deve criar o sistema do zero.

O objetivo agora é tratar a spec como **audit and hardening**:

```text
confirmar o estado real no código;
mapear todos os domínios que usam IDs persistidos;
validar duplicatas, nulos e IDs instáveis;
definir política de compatibilidade de save;
criar validação automatizada quando praticável;
preparar a base para event/save/quest/inventory/enemy/bestiary specs futuras.
```

---

## 2. Problema

Sem hardening de stable IDs, as próximas specs podem quebrar compatibilidade entre sistemas já existentes.

Riscos concretos:

```text
criar IDs duplicados em ScriptableObjects;
renomear ID usado em save antigo;
gerar registry paralelo por domínio;
persistir referência Unity em vez de ID;
criar item/enemy/quest/NPC ID fora de convenção;
quebrar restore de save por ID ausente;
permitir fallback silencioso sem warning;
executar specs em paralelo alterando o mesmo registry.
```

---

## 3. Objetivo

Ao final desta spec, o repo deve ter stable IDs auditados, regras de ID documentadas, validação automatizada de duplicatas/ausências quando praticável e política explícita para alteração de IDs persistidos.

Resultado esperado:

```text
O sistema existente de IIdentifiedData/IDataRegistry/DataRegistrySO é preservado.
IDs existentes não são quebrados.
Novos domínios sabem como declarar IDs estáveis.
Save/load futuro pode confiar em IDs, não em Unity references.
Specs futuras sabem quais arquivos/sistemas estão protegidos.
```

---

## 4. Fontes obrigatórias lidas

Para criar esta spec foram lidas/consideradas:

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md
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
```

Leitura condicional por domínio:

```text
Inventory/items:
  docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
  docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

Enemies/bestiary:
  docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

Quests:
  docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

Save/load:
  docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- `spec_data_001_ids_registries_e_scriptableobjects.md` está implementada.
- Evidência principal declarada: `Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs`.
- Evidências complementares declaradas: `IDataRegistry.cs`, `DataRegistrySO.cs` e `Assets/_Game/Data/**`.
- Pendência existente: completar taxonomia de itens futuros sem quebrar IDs existentes.
- `SPEC_EXISTING_IMPLEMENTATION_AUDIT.md` classifica Stable IDs / data registries como IMPLEMENTED / RESIDUAL.
```

Estado que precisa ser confirmado na execução local:

```text
- quais classes implementam IIdentifiedData;
- quais registries/DataRegistrySO existem;
- quais ScriptableObjects possuem IDs vazios, duplicados ou instáveis;
- quais IDs aparecem em saves/DTOs;
- quais validators já existem para Data IDs;
- se já há menu/validator editor para auditoria de IDs.
```

Regra:

```text
Não recriar sistema existente sem confirmar ausência real no repo.
```

---

## 6. User stories / engineering stories

```text
Como sistema de save, quero persistir IDs estáveis para restaurar estado sem depender de Unity references.
Como agente executor, quero validar IDs duplicados antes de criar novas specs de domínio.
Como maintainer, quero saber quais IDs são públicos/persistidos e não podem ser renomeados sem migration.
Como designer técnico, quero convenções de ID por domínio para evitar drift entre item, enemy, quest, NPC e cave data.
Como revisor, quero relatório de IDs inválidos, ausentes ou duplicados com severidade clara.
```

---

## 7. Escopo

Inclui:

```text
- auditar implementação existente de `IIdentifiedData`, `IDataRegistry` e `DataRegistrySO`;
- mapear domínios com IDs estáveis;
- definir convenções de stable ID por domínio;
- validar IDs vazios, duplicados, renomeados ou fora de padrão quando praticável;
- adicionar/ajustar validator editor ou EditMode test se o harness permitir;
- documentar política de alteração de ID persistido;
- documentar fallback esperado para ID inválido;
- criar execution report com Testing Quality Gate.
```

---

## 8. Fora de escopo

Não inclui:

```text
- criar novo sistema de registry paralelo;
- substituir `DataRegistrySO` por arquitetura nova;
- reescrever `SaveManager`;
- alterar IDs existentes em massa;
- migrar saves antigos sem spec de save migration;
- criar novos itens/inimigos/quests/NPCs de conteúdo;
- alterar scenes/prefabs;
- executar Play Mode humano;
- resolver todos os warnings de conteúdo futuro fora da base necessária.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo `IIdentifiedData`.
Não criar segundo `IDataRegistry`.
Não criar segundo `DataRegistrySO` se o existente cobre o caso.
Não criar registry local por domínio se já houver registry canônico reutilizável.
Não converter referência Unity persistida sem entender save migration.
Não renomear ID persistido apenas para padronizar nomenclatura.
```

Se for necessário criar adapter/validator novo, ele deve complementar o sistema existente.

---

## 10. Critérios de aceite

### 10.1 Auditoria de IDs existentes

- O executor lista todos os tipos/classes que implementam ou expõem stable ID.
- O executor lista registries/data assets auditados.
- O executor identifica IDs vazios, duplicados ou fora de convenção.
- O executor marca achados como ERROR/WARNING/INFO conforme risco.

### 10.2 Compatibilidade de save

- A spec não altera IDs persistidos existentes sem migration explícita.
- Qualquer ID considerado persistido é documentado como protegido.
- Qualquer fallback de ID inválido gera warning ou relatório.
- DTOs novos/futuros devem persistir ID/string, não referência Unity.

### 10.3 Validator/teste

Quando praticável, a execução cria ou ajusta validação automatizada para:

```text
IDs vazios;
IDs duplicados;
registries com entrada nula;
assets sem ID;
IDs fora de convenção quando regra existir;
IDs persistidos que não resolvem para data asset quando base existir.
```

Se não for praticável, o execution report deve registrar NOT RUN/NOT IMPLEMENTED com motivo e risco residual.

### 10.4 Relatório

Criar ou atualizar:

```text
docs/validation/01_spec_stable_ids_registry_runtime_execution_report.md
```

O relatório deve incluir:

```text
arquivos auditados;
validators/testes adicionados;
achados por severidade;
IDs protegidos;
risco residual;
resultado do Testing Quality Gate.
```

---

# /speckit.plan

## 11. Arquitetura alvo

A arquitetura alvo preserva o sistema existente.

Componentes esperados:

```text
Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs
Assets/_Game/Scripts/Core/Data/IDataRegistry.cs
Assets/_Game/Scripts/Core/Data/DataRegistrySO.cs
Assets/_Game/Data/**
```

Possíveis complementos, se ainda não existirem:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateStableIds.cs
Assets/_Game/Tests/EditMode/Core/Data/StableIdsValidationTests.cs
```

Documento/relatório:

```text
docs/validation/01_spec_stable_ids_registry_runtime_execution_report.md
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
IIdentifiedData remains the canonical stable ID contract.
IDataRegistry/DataRegistrySO remain canonical registry contracts unless local audit proves otherwise.
IDs are string identifiers intended to be stable across saves and content versions.
```

### 12.2 Runtime contracts

```text
Runtime systems resolve content by ID through existing registries/managers.
Runtime systems must not persist Unity object references in save DTOs.
```

### 12.3 Event contracts

N/A — esta spec não adiciona eventos.

### 12.4 Save contracts

```text
Persisted references must use stable IDs.
Changing a persisted ID requires migration or compatibility alias policy in a future save spec.
Invalid ID fallback must be explicit and reportable.
```

### 12.5 UI contracts

N/A — sem UI.

---

## 13. Sistemas afetados

```text
Core data contracts;
ScriptableObject registries;
Save/load compatibility;
Inventory/item data;
Equipment/tool data;
Enemy data;
Bestiary data;
Quest IDs;
NPC/service data;
Cave/resource data;
Editor validation;
EditMode validation where available.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/Data/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Core/Data/**
docs/validation/01_spec_stable_ids_registry_runtime_execution_report.md
```

Leitura permitida, mas alteração apenas se estritamente necessária e reportada:

```text
Assets/_Game/Data/**
Assets/_Game/Scripts/**/**DataSO.cs
Assets/_Game/Scripts/Save/**
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset, salvo correção pontual de ID auditada e explicitamente listada no report
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
rg -n "IIdentifiedData|IDataRegistry|DataRegistrySO|Id\b|ID\b|ItemId|EnemyId|QuestId|NpcId|Cave|Bestiary" Assets/_Game/Scripts Assets/_Game/Data
rg -n "class .*DataSO|ScriptableObject" Assets/_Game/Scripts
rg -n "Validate.*Id|DataId|Registry" Assets/_Game/Scripts/Editor Assets/_Game/Tests
```

### Fase 1 — Inventário

```text
1. Listar contratos existentes.
2. Listar registries existentes.
3. Listar domínios com IDs persistidos.
4. Listar validators existentes.
```

### Fase 2 — Hardening mínimo

```text
1. Adicionar validator/teste se não houver cobertura mínima.
2. Validar duplicatas/nulos/entradas quebradas.
3. Não alterar IDs em massa.
4. Registrar findings.
```

### Fase 3 — Report

```text
1. Criar execution report.
2. Documentar risco residual.
3. Indicar próximos hardenings por domínio.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar contratos existentes.
3. Auditar registries e assets.
4. Verificar validators existentes.
5. Implementar validator/teste mínimo se necessário.
6. Rodar validações obrigatórias.
7. Criar execution report.
8. Não mover specs nem atualizar execution order.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_01_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - event contracts hardening;
  - save restore/order specs;
  - item/enemy/quest/NPC data specs;
  - any data registry or ScriptableObject mass edit.
- Shared files/systems that require lock:
  - Core data contracts;
  - `Assets/_Game/Data/**`;
  - validators;
  - save DTO references by ID.
- Reason:
  - Stable IDs são fundação transversal. Rodar em paralelo pode criar IDs/registries incompatíveis.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO, unless executor proposes ID rename; then STOP and require separate migration spec.
Does this persist Unity references? MUST BE NO
```

---

## 20. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: CONDITIONAL — only if correcting explicit invalid ID and report lists exact asset.
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

---

## 22. Riscos técnicos

```text
Risco: alterar ID persistido e quebrar saves.
Mitigação: não renomear IDs sem migration spec.

Risco: validator gerar falsos positivos por dados futuros/incompletos.
Mitigação: classificar severity e permitir WARNING para future placeholders.

Risco: criar registry paralelo.
Mitigação: usar contratos existentes.

Risco: auditoria incompleta via busca textual.
Mitigação: reportar termos usados e findings UNKNOWN.
```

---

## 23. Rollback

```text
Remover validator/teste criado.
Reverter qualquer alteração pontual de asset, se houver.
Remover execution report.
Nenhum save/schema/runtime gameplay deve ser alterado.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar `IIdentifiedData`, `IDataRegistry`, `DataRegistrySO` e contratos relacionados.
- [ ] T003 — Mapear domínios com IDs estáveis/persistidos.
- [ ] T004 — Auditar validators existentes para IDs/registries.
- [ ] T005 — Criar/ajustar validator ou EditMode test mínimo para IDs vazios/duplicados se necessário.
- [ ] T006 — Rodar validações obrigatórias ou registrar NOT RUN.
- [ ] T007 — Criar `docs/validation/01_spec_stable_ids_registry_runtime_execution_report.md`.
- [ ] T008 — Registrar risco residual e próximos hardenings.

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

EditMode tests quando validator/teste for criado:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, if validator/test code is created or changed; otherwise NO.
- Requires EditMode tests: YES if deterministic validator logic is added and test harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: NO unless fixing a known ID bug.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; ID validator/test PASS or NOT RUN with justified risk; execution report created; no IDs persistently changed without migration.

---

## 27. Definition of Done

```text
Stable ID contracts audited.
No duplicate/empty high-risk IDs remain unreported.
No new registry parallel created.
No persisted IDs renamed without migration.
Validation added or residual risk documented.
Execution report created.
Assets/, scenes and prefabs untouched unless a specific asset ID correction is explicitly reported.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não quebrar IDs existentes.
Não persistir Unity references.
Não criar registry paralelo.
Não converter warnings de future placeholders em blockers sem justificativa.
Não rodar em paralelo com save/event/data specs.
Não declarar ACCEPTED apenas porque build compilou.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada antes de specs que criem ou alterem novos IDs persistidos.

Resultado esperado para próximas specs:

```text
Event contracts, save restore order, quest IDs, item IDs, enemy IDs, NPC IDs e bestiary IDs devem usar as regras endurecidas por esta spec.
```
