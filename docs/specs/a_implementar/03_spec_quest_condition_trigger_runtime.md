# SPEC — Quest Condition / Trigger Runtime

> **Spec ID:** `03_spec_quest_condition_trigger_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P0  
> **Type:** Runtime / Quest Conditions / Event Triggers / Validation  
> **Domain:** Quest / Conditions / Triggers / GameEventBus  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere Quest core contracts, GameEventBus contracts, reward application, quest save/load, time/calendar event contracts, dialogue hooks ou domain adapters.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_condition_trigger_runtime_execution_report.md`.  
> **Depends on:**  
- `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
- `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`
- `docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_season_year_runtime.md`
- `docs/specs/a_implementar/02_spec_weather_generation_forecast_runtime.md`
- `docs/specs/a_implementar/02_spec_lunar_cycle_event_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_festivals_events_runtime.md`
> **Blocks:**  
- `03_spec_quest_reward_application_idempotency_runtime.md`
- `03_spec_quest_state_save_load_runtime.md`
- `farm orders adapter`
- `festival quest expiry`
- `dialogue quest hooks`
- `cave contract conditions`
> **Scope:** implementar/consolidar runtime mínimo de Conditions e Triggers para quests, separando condição de avanço e consumindo eventos de gameplay sem substituir sistemas primários.  
> **Out of scope:** aplicação de rewards, quest save/load completo, Quest Log UI, adapters específicos completos de farm/cave/festival/dialogue, dev tool completo.

---

# /speckit.specify

## 1. Contexto

O direction de quests define que Condition é estado que precisa ser verdadeiro e não avança quest sozinha; Trigger/event avança quando as condições permitem. Também lista condition categories e eventos canônicos que o quest system deve consumir.

Esta spec vem depois do contrato base de QuestDefinition/State/Objective.

---

## 2. Problema

Sem separação clara entre Condition e Trigger, quests podem avançar apenas porque um estado ficou verdadeiro, ou podem perder progresso porque evento ocorreu antes da condição. Isso causa trigger perdido, softlock, reward duplicado e comportamento inconsistente após save/load.

Também há risco de o quest system virar fonte primária de farm/combat/inventory/economy em vez de consumidor de eventos.

---

## 3. Objetivo

Criar runtime mínimo e testável para avaliar Conditions e processar Triggers/QuestEvents, usando GameEventBus e stable IDs, sem aplicar rewards e sem implementar adapters completos de domínio nesta spec.

---

## 4. Source Map Compliance

### 4.1 Fontes globais lidas

- `docs/design/SPEC_SOURCE_MAP.md`
- `docs/design/SPECIFICATION_PROCESS.md`
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
- `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
- `docs/project/CURRENT_STATE.md`
- `docs/IMPLEMENTATION_STATUS.md`

### 4.2 Directions de domínio lidos

- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`

### 4.3 Covered from directions

```text
Condition categories: Quest, Player, Inventory, World, Time, Weather, Lunar, Npc, Dialogue, Farm, Cave, Combat, Fonte, Bestiary; future categories marked.
Condition não avança quest sozinha.
Trigger/event avança quando condição permite.
Quest system consome events canônicos como OnItemCollected, OnCropHarvested, OnDayStarted, OnWeatherChanged, OnLunarEventStarted, OnFestivalStarted/Ended.
Temporal conditions descobertas podem aparecer no Quest Log futuramente; secretas não aparecem.
```

### 4.4 Deferred / future from directions

```text
Reward application/idempotency.
Quest save/load.
Quest Log UI.
Adapters específicos completos de farm/cave/dialogue/festival.
Dev validation tool full.
Social/pet/companion future.
```

### 4.5 Explicitly not redefined here

```text
Não redefine GameEventBus.
Não redefine quest contracts base.
Não redefine reward rules.
Não redefine time/weather/lunar runtime.
Não redefine UI quest log.
```

---

## 5. Estado atual do repo

```text
Quest contract spec deve existir antes desta.
Event bus hardening deve existir antes desta.
Time/calendar/weather/lunar specs alimentam temporal conditions.
Generic Quest engine status ainda é PARTIAL/UNKNOWN segundo audit documental.
```

---

## 6. User stories / engineering stories

```text
Como quest objective, quero progredir quando evento correto ocorrer e condições permitirem.
Como main quest, quero não depender de evento raro opaco sem pista.
Como farm order, quero contar Harvest/Ship/Deliver apenas quando quest ativa ou aceita.
Como festival quest, quero expirar pelo evento de festival sem bloquear main quest.
Como maintainer, quero testar Conditions sem PlayMode.
```

---

## 7. Escopo

```text
Auditar events e quest contracts existentes.
Criar/consolidar Condition interfaces/types mínimos.
Criar/consolidar Trigger processing mínimo.
Integrar com GameEventBus para eventos canônicos básicos.
Implementar temporal condition adapters mínimos se runtime existir.
Adicionar EditMode tests para condition/trigger quando praticável.
Criar execution report.
```

---

## 8. Fora de escopo

```text
Não aplicar rewards.
Não criar save/load de QuestState.
Não criar UI Quest Log.
Não criar todos os adapters de domínio.
Não criar content de quests.
Não implementar social/pet/companion conditions agora.
```

---

## 9. Regras de não duplicação

```text
Não fazer Condition avançar quest sozinha.
Não fazer quest system substituir farm/combat/inventory/economy.
Não criar event bus paralelo.
Não esconder progressão em QuestFlags soltas.
Não revelar hidden condition no log antes de descoberta.
```

---

## 10. Critérios de aceite

### 10.1 Condition runtime

- Existe interface/contrato para avaliar Condition com contexto.
- Conditions retornam pass/fail/reason sem mutar quest state diretamente.
- Categories iniciais estão representadas ou mapeadas como deferred.

### 10.2 Trigger runtime

- Trigger/event processa objective/step apenas quando conditions permitem.
- Eventos canônicos básicos são mapeáveis.
- Eventos recebidos antes da quest estar apta não geram avanço indevido.

### 10.3 Temporal safety

- WaitForTime/Day/Season/Weather/Lunar/Festival são suportados ou explicitamente deferred com hooks.
- Main quest não pode exigir evento raro sem pista/controle.

### 10.4 Tests/report

- EditMode tests cobrem condition false/true e trigger gated quando possível.
- Execution report criado em `docs/validation/03_spec_quest_condition_trigger_runtime_execution_report.md`.

---

# /speckit.plan

## 11. Arquitetura alvo

Possíveis alvos:
```text
Assets/_Game/Scripts/Quests/Conditions/IQuestCondition.cs
Assets/_Game/Scripts/Quests/Conditions/QuestConditionContext.cs
Assets/_Game/Scripts/Quests/Triggers/IQuestTriggerHandler.cs
Assets/_Game/Scripts/Quests/Triggers/QuestEventRouter.cs
Assets/_Game/Scripts/Quests/Runtime/QuestProgressionService.cs
Assets/_Game/Tests/EditMode/Quests/QuestConditionTriggerTests.cs
docs/validation/03_spec_quest_condition_trigger_runtime_execution_report.md
```

Se equivalentes existirem, consolidar.

---

## 12. Contratos, dados e eventos

### Data contracts

Conditions reference IDs and typed parameters. They do not persist runtime objects.

### Runtime contracts

Conditions are read-only checks. Triggers mutate quest state only through quest progression service.

### Event contracts

QuestEventRouter subscribes to GameEventBus and unsubscribes safely.

### Save contracts

No new save schema in this spec. QuestState save fica em spec própria.

### UI contracts

No UI; visibility/hints are data for future Quest Log.

---

## 13. Sistemas afetados

```text
Quest runtime
GameEventBus integration
Time/calendar/weather/lunar condition adapters
Future farm/festival/cave/dialogue adapters
EditMode tests
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**Quest*
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_condition_trigger_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Save/** except read-only
docs/specs/SPEC_EXECUTION_ORDER.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
rg -n "QuestCondition|Condition|Trigger|QuestEvent|GameEventBus|OnCrop|OnItem|OnDay|OnWeather|OnLunar|OnFestival" Assets/_Game/Scripts
```

### Fase 1 — Contracts/runtime mínimo

Criar ou consolidar condition/trigger interfaces e router.

### Fase 2 — Tests

Criar EditMode tests para condition gating e trigger processing básico.

### Fase 3 — Report

Reportar adapters implementados vs deferred.

---

## 17. Ordem segura de execução

```text
Ler fontes obrigatórias.
Confirmar Quest contract base.
Auditar event bus and quest code.
Criar/consolidar condition interfaces.
Criar/consolidar trigger router.
Adicionar tests.
Rodar validações.
Criar execution report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_03_QUEST_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - qualquer spec que altere Quest core contracts, GameEventBus contracts, reward application, quest save/load, time/calendar event contracts, dialogue hooks ou domain adapters.
- Reason:
  - Conditions/triggers são core de progressão e acoplam quest contracts, events e temporal systems.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
QuestState mutation must be serializable later by save spec.
```

---

## 20. Impacto em eventos

```text
Adds event subscribers: YES if QuestEventRouter is created.
Requires unsubscribe pattern: YES.
Changes existing events: SHOULD BE NO.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO.
Requires PlayMode/final human scenario: NO for pure runtime tests.
Human validation timing: NOT REQUIRED.
```

---

## 22. Riscos técnicos

```text
Risco: condição mutar estado. Mitigação: interface read-only.
Risco: trigger duplicado aplicar progresso duas vezes. Mitigação: idempotent progression handled before rewards.
Risco: subscriber leak. Mitigação: unsubscribe pattern.
Risco: evento temporal raro gerar softlock. Mitigação: anti-softlock rules and visibility.
```

---

## 23. Rollback

```text
Remover interfaces/router/tests criados.
Reverter subscriptions.
Remover execution report.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias.
- [ ] T002 — Auditar condition/trigger/event code existente.
- [ ] T003 — Criar/consolidar condition context/interfaces.
- [ ] T004 — Criar/consolidar trigger event router.
- [ ] T005 — Adicionar basic temporal/event adapters se seguro.
- [ ] T006 — Adicionar EditMode tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar execution report.

---

## 25. Validações obrigatórias

Docs validation.

C# build/Unity compile if C# changed.

EditMode tests for deterministic condition/trigger logic.

---

## 26. Testing Quality Gate

```text
Changed deterministic logic: YES.
Requires EditMode tests: YES when harness available.
Requires PlayMode automated or final human scenario: NO.
Requires regression test: YES if fixing known quest trigger bug.
Human validation timing: NOT REQUIRED.
Minimum validation evidence for ACCEPTED: docs validation PASS; C# build/Unity compile PASS; EditMode tests PASS or justified NOT RUN; execution report created.
```

---

## 27. Definition of Done

```text
Condition and trigger contracts/runtime created or consolidated.
Condition does not mutate state directly.
Trigger gates by conditions.
QuestEventRouter unsubscribes safely.
Tests/validator added or risk documented.
Execution report created.
```

---

## 28. Anti-regressão

```text
Condition não avança quest sozinha.
Quest system não substitui sistemas primários.
Não aplicar rewards nesta spec.
Não salvar QuestState nesta spec.
Não revelar hidden conditions.
Não declarar ACCEPTED só por compile.
```

---

## 29. Notas para execução posterior

Esta spec prepara reward idempotency, quest save/load, flags and adapters.
