# SPEC — Game Event Contracts Audit and Hardening

> **Spec ID:** `01_spec_game_event_contracts_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0  
> **Type:** Runtime / Events / Validation / Hardening  
> **Domain:** Core / Events / GameEventBus / Contracts  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere `GameEventBus`, `Assets/_Game/Scripts/Core/Events/**`, save restore order, quest triggers, farm day transition, combat events, inventory/economy events ou UI event routing.  
> **Repo lock scope:** `Assets/_Game/Scripts/Core/GameEventBus.cs`, `Assets/_Game/Scripts/Core/Events/**`, quaisquer publishers/subscribers auditados, `docs/validation/01_spec_game_event_contracts_runtime_execution_report.md`.  
> **Depends on:**  
> - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `.specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `.specs/implementados/spec_core_001_event_bus_e_eventos_base.md`  
> - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> **Blocks:**  
> - quest/objective trigger contracts;  
> - farm/economy event integration;  
> - save/load event-adjacent restore notifications;  
> - combat/enemy/death events;  
> - bestiary discovery events;  
> - UI/HUD event subscriptions.  
> **Scope:** auditar e endurecer o `GameEventBus` e os contratos de eventos existentes sem criar um novo event bus.  
> **Out of scope:** substituir `GameEventBus`, migrar para framework externo, criar domínio de quests, alterar save schema, alterar cenas/prefabs, executar Play Mode humano ou criar eventos de gameplay fora do inventário auditado.

---

# /speckit.specify

## 1. Contexto

A WAVE 01 previa `01_spec_game_event_contracts_runtime.md` como fundação de eventos. O repo, porém, já possui `GameEventBus` e uma pasta de eventos base em `Assets/_Game/Scripts/Core/Events/**` conforme a spec implementada `spec_core_001_event_bus_e_eventos_base.md`.

A pendência real registrada é:

```text
Auditar payloads de eventos futuros e manter unsubscribe no ciclo de vida.
```

Portanto esta spec deve ser uma spec de **audit and hardening**, não uma reimplementação.

---

## 2. Problema

Sem hardening dos contratos de eventos, as próximas specs podem criar eventos incompatíveis ou inseguros.

Riscos concretos:

```text
criar segundo event bus;
publicar eventos com payload mutável ou referência Unity persistente;
usar eventos como fonte primária de estado;
criar eventos duplicados por domínio com nomes diferentes;
esquecer unsubscribe e gerar callbacks duplicados após reload/troca de cena;
criar eventos de quest/farm/combat/bestiary sem IDs estáveis;
permitir ordem de restore/save dependente de evento transitório;
mascarar erro de subscriber com fallback silencioso.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter:

```text
GameEventBus preservado como barramento central;
catálogo auditado de eventos existentes;
regras de payload para eventos novos;
validação ou teste para publish/subscribe/unsubscribe quando praticável;
orientação clara de lifecycle de subscription;
execution report com eventos auditados, riscos e gaps.
```

---

## 4. Fontes obrigatórias lidas

Para criar esta spec foram lidas/consideradas:

```text
.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
.specs/a_implementar/01_spec_stable_ids_registry_runtime.md
.specs/implementados/spec_core_001_event_bus_e_eventos_base.md
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
```

Leitura condicional por domínio:

```text
Quest/objectives:
  docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

Save/load:
  docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

World/time/farm:
  docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

Bestiary/enemies:
  docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- `spec_core_001_event_bus_e_eventos_base.md` está implementada.
- Evidência principal declarada: `Assets/_Game/Scripts/Core/GameEventBus.cs`.
- Evidência complementar declarada: `Assets/_Game/Scripts/Core/Events/**`.
- Pendência declarada: auditar payloads de eventos futuros e manter unsubscribe no ciclo de vida.
- `SPEC_EXISTING_IMPLEMENTATION_AUDIT.md` classifica Event bus / base events como IMPLEMENTED / RESIDUAL.
```

Estado que precisa ser confirmado na execução local:

```text
- APIs reais de `GameEventBus`: Subscribe/Unsubscribe/Publish ou equivalentes;
- lista completa de eventos em `Core/Events/**`;
- publishers e subscribers por domínio;
- padrões de lifecycle: OnEnable/OnDisable, Awake/OnDestroy, bootstrap, scene transition;
- eventos com payload contendo Unity references, mutable collections ou objetos runtime;
- duplicidade de eventos com semântica equivalente;
- testes/validators existentes para event bus.
```

Regra:

```text
Não recriar o event bus existente sem confirmar que ele está inadequado e sem decisão humana explícita.
```

---

## 6. User stories / engineering stories

```text
Como sistema de quest, quero receber eventos de gameplay com payload estável e IDs consistentes.
Como HUD/UI, quero assinar eventos sem duplicar callbacks após abrir/fechar telas.
Como sistema de save/load, quero que eventos não substituam a fonte primária de estado persistido.
Como agente executor, quero catálogo de eventos para não criar duplicatas semânticas.
Como maintainer, quero regra clara de unsubscribe para evitar memory leaks e double handling.
```

---

## 7. Escopo

Inclui:

```text
- auditar `GameEventBus` existente;
- auditar `Assets/_Game/Scripts/Core/Events/**`;
- mapear eventos existentes por domínio;
- mapear publishers/subscribers principais;
- definir regras de naming e payload para novos eventos;
- garantir que eventos usem stable IDs quando referenciam conteúdo persistido;
- documentar quando evento é notificação vs fonte de estado;
- adicionar/ajustar teste ou validator de publish/subscribe/unsubscribe quando praticável;
- criar execution report com catálogo, gaps e risco residual.
```

---

## 8. Fora de escopo

Não inclui:

```text
- substituir `GameEventBus`;
- trocar para UniRx, UnityEvent global, message broker externo ou event sourcing;
- criar quest system;
- criar weather/calendar events completos;
- criar combat/death/bestiary events fora dos gaps auditados;
- alterar save schema;
- alterar UI final;
- alterar scenes/prefabs;
- executar Play Mode humano;
- resolver todos os eventos futuros de todas as waves.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo `GameEventBus`.
Não criar evento novo se já houver evento equivalente.
Não criar evento de domínio dentro de pasta errada se `Core/Events/**` já for padrão.
Não publicar evento para substituir estado persistido.
Não colocar UnityEngine.Object, GameObject, Component, Transform ou ScriptableObject em DTO de evento que possa influenciar save/restore.
Não exigir Play Mode humano para validar publish/subscribe básico.
```

---

## 10. Critérios de aceite

### 10.1 Catálogo de eventos

O execution report lista eventos existentes com colunas mínimas:

```text
Event type;
Path;
Domain;
Payload fields;
Contains stable IDs? YES/NO/N/A;
Contains Unity reference? YES/NO;
Known publishers;
Known subscribers;
Lifecycle risk: LOW/MEDIUM/HIGH/UNKNOWN.
```

### 10.2 Regras de payload

A execução documenta e/ou implementa regra para eventos novos:

```text
eventos que referenciam content/data usam stable IDs;
eventos não carregam referências Unity quando o payload cruza sistema/save/quest;
eventos de UI local podem carregar referência runtime apenas se não persistida e escopo for local;
payloads devem ser pequenos, explícitos e imutáveis quando praticável;
eventos não são fonte primária de persistência.
```

### 10.3 Lifecycle de subscription

A execução deve validar/documentar:

```text
subscribers runtime usam unsubscribe simétrico;
objetos de cena não deixam subscription ativa após destroy/disable;
bootstrap/global managers não duplicam subscription em reload;
UI/modal subscribers não recebem callbacks duplicados após reopen.
```

### 10.4 Teste/validator

Quando praticável, criar ou ajustar teste/validator para:

```text
publish chama subscribers esperados;
unsubscribe remove subscriber;
subscriber exception handling não quebra estado global sem log;
subscription duplicada é evitada ou documentada;
evento de payload proibido é detectado por validator, se factível.
```

Se não for praticável, registrar `NOT RUN`/`NOT IMPLEMENTED` com motivo e risco residual.

### 10.5 Relatório

Criar:

```text
docs/validation/01_spec_game_event_contracts_runtime_execution_report.md
```

O relatório deve incluir:

```text
catálogo de eventos;
regras de payload;
publishers/subscribers auditados;
lifecycle risks;
testes/validators adicionados;
validações rodadas;
Testing Quality Gate;
risco residual.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Preservar arquitetura existente:

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/**
```

Possíveis complementos, se ainda não existirem:

```text
Assets/_Game/Tests/EditMode/Core/Events/GameEventBusTests.cs
Assets/_Game/Scripts/Editor/Validation/ValidateGameEventContracts.cs
```

Documento/relatório:

```text
docs/validation/01_spec_game_event_contracts_runtime_execution_report.md
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
Events referencing content should use stable IDs produced/validated by 01_spec_stable_ids_registry_runtime.
Events should not introduce new data identity models.
```

### 12.2 Runtime contracts

```text
GameEventBus remains the canonical publish/subscribe mechanism.
Events are notifications and integration signals, not primary storage.
Subscriptions must have clear lifecycle ownership.
```

### 12.3 Event contracts

```text
Existing events remain source-compatible unless a bugfix requires explicit migration.
New event contracts must be named by domain/action/result and have explicit payload fields.
Breaking changes to event payloads require auditing all publishers/subscribers.
```

### 12.4 Save contracts

```text
No save schema change.
Events must not be required to restore persisted state.
Save/load systems may publish post-restore notifications only after state has been restored from primary DTOs.
```

### 12.5 UI contracts

```text
UI subscribers must unsubscribe when closed/destroyed unless they are global managers with documented lifetime.
UI events must not leak gameplay input state across modal lifecycle.
```

---

## 13. Sistemas afetados

```text
Core GameEventBus;
Core event contracts;
Quest triggers;
Farm/time events;
Inventory/economy events;
Combat/enemy/death events;
Bestiary discovery events;
UI/HUD subscriptions;
Save/load post-restore notifications;
Editor validation/EditMode tests.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Tests/EditMode/Core/Events/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/01_spec_game_event_contracts_runtime_execution_report.md
```

Leitura permitida, alteração apenas se estritamente necessária e reportada:

```text
Assets/_Game/Scripts/**
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
rg -n "class GameEventBus|static class GameEventBus|Subscribe|Unsubscribe|Publish" Assets/_Game/Scripts
rg -n "Event\b|ChangedEvent|StartedEvent|EndedEvent|CompletedEvent|DiedEvent|SpawnedEvent|HarvestedEvent|PlantedEvent" Assets/_Game/Scripts/Core/Events Assets/_Game/Scripts
rg -n "GameEventBus\.Subscribe|GameEventBus\.Unsubscribe|GameEventBus\.Publish" Assets/_Game/Scripts
rg -n "OnEnable|OnDisable|OnDestroy|Awake|Start" Assets/_Game/Scripts
```

### Fase 1 — Catálogo

```text
1. Listar eventos existentes.
2. Classificar por domínio.
3. Mapear payload fields.
4. Mapear publishers/subscribers principais.
5. Identificar eventos duplicados ou payloads de risco.
```

### Fase 2 — Hardening mínimo

```text
1. Criar/ajustar teste ou validator se não houver cobertura mínima.
2. Corrigir apenas bugs pequenos de unsubscribe/payload se forem óbvios e dentro do escopo.
3. Se mudança for breaking, STOP e pedir spec separada.
```

### Fase 3 — Report

```text
1. Criar execution report.
2. Documentar riscos e próximos eventos por domínio.
3. Registrar validations.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar `GameEventBus` e eventos existentes.
3. Auditar publishers/subscribers.
4. Classificar riscos de payload/lifecycle.
5. Criar teste/validator mínimo quando praticável.
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
  - save restore order;
  - quest condition/trigger;
  - farm day transition;
  - combat/death events;
  - UI modal/input event routing.
- Shared files/systems that require lock:
  - `GameEventBus`;
  - `Core/Events/**`;
  - all publishers/subscribers being modified.
- Reason:
  - Event contracts are cross-cutting. Parallel edits can break payload/source compatibility.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? MUST BE NO
```

If a proposed event change affects save restore order, STOP and move to save restore/order spec.

---

## 20. Impacto em eventos

```text
Adds events: CONDITIONAL — only if audit finds missing base event and scope is explicit.
Changes existing events: SHOULD BE NO; breaking changes require separate spec.
Requires unsubscribe pattern: YES.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO for bus tests; YES only if a runtime UI lifecycle bug is fixed, then defer to final validation.
Human validation timing: NOT REQUIRED unless gameplay/UI lifecycle scenario is added; then DEFERRED_TO_FINAL_VALIDATION.
```

---

## 22. Riscos técnicos

```text
Risco: teste de event bus depender de Unity scene lifecycle.
Mitigação: preferir EditMode pure C# tests para bus básico.

Risco: alterar evento existente e quebrar subscriber.
Mitigação: mapear publishers/subscribers antes; evitar breaking changes.

Risco: validator gerar falsos positivos em eventos UI locais.
Mitigação: diferenciar eventos locais/transientes de eventos cross-system.

Risco: tentar resolver todos os eventos futuros.
Mitigação: catálogo + regras, não implementação massiva.
```

---

## 23. Rollback

```text
Remover teste/validator criado.
Reverter alteração pontual no GameEventBus/event contracts, se houver.
Remover execution report.
Nenhuma cena/prefab/asset/save schema deve ter sido alterada.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar `GameEventBus` existente e sua API.
- [ ] T003 — Catalogar eventos existentes em `Core/Events/**`.
- [ ] T004 — Mapear publishers/subscribers principais por domínio.
- [ ] T005 — Identificar riscos de payload e lifecycle/unsubscribe.
- [ ] T006 — Criar/ajustar teste EditMode ou validator mínimo quando praticável.
- [ ] T007 — Rodar validações obrigatórias ou registrar NOT RUN.
- [ ] T008 — Criar `docs/validation/01_spec_game_event_contracts_runtime_execution_report.md`.
- [ ] T009 — Registrar risco residual e próximos hardenings por domínio.

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

- Changed deterministic logic: YES, if validator/test or event bus behavior is created/changed; otherwise NO.
- Requires EditMode tests: YES if GameEventBus behavior or deterministic validator logic is changed and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO for pure bus/contract audit; YES only if fixing scene/UI lifecycle behavior, then DEFERRED_TO_FINAL_VALIDATION.
- Requires regression test: YES if fixing a known event/unsubscribe bug; otherwise NO.
- Human validation timing: NOT REQUIRED unless UI/gameplay lifecycle scenario is added; then DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS if C# changed; EditMode tests/validator PASS or NOT RUN with justified risk; execution report created; no event bus replacement; no breaking event payload change without separate spec.

---

## 27. Definition of Done

```text
GameEventBus audited and preserved.
Event catalog created in execution report.
Publishers/subscribers sampled or fully mapped for core events.
Lifecycle/unsubscribe risks documented.
No duplicate event bus created.
No breaking payload changes made without separate spec.
Validator/test added or residual risk documented.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não substituir GameEventBus.
Não criar eventos duplicados por domínio.
Não carregar Unity references em eventos cross-system.
Não usar evento como fonte primária de save state.
Não esquecer unsubscribe em subscribers de cena/UI.
Não rodar em paralelo com save/quest/farm/combat/UI specs que editem eventos.
Não declarar ACCEPTED apenas porque build compilou.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada depois do hardening de stable IDs, ou junto apenas se a execução estiver serializada no mesmo agente e com lock explícito.

Resultado esperado para próximas specs:

```text
Quest triggers, farm/time events, combat/death events, bestiary discovery and UI subscriptions must use the event payload/lifecycle rules hardened here.
```
