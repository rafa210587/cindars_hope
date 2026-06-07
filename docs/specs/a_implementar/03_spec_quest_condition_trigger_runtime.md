# SPEC — Quest Condition / Trigger Runtime

> **Spec ID:** `03_spec_quest_condition_trigger_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

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

- QuestDefinition, QuestState, Objectives, Conditions, Triggers, Rewards, Flags, visibility, save/load ou anti-softlock conforme escopo da spec.
- Regra de anti-spoiler e separation of concerns entre QuestState, QuestFlags, MainProgression, Fonte e sistemas de domínio.
- Idempotência de rewards e save/load seguro quando aplicável.

### Deferred / future from directions

- Conteúdo final de quests.
- UI visual final.
- Main quest completa, cutscenes e boss gates.
- Authoring tools avançados.

### Explicitly not redefined here

- GameEventBus completo.
- Inventory/economy/backend de domínio.
- Calendar/weather/lunar runtime quando fora do escopo.
- Fonte/MainProgression deep state quando não for spec específica.

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

## Direction / Refinement Coverage

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

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de quest/objective/event runtime? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_condition_trigger_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|QuestState|Visibility|Softlock|Save|Fonte|MainProgression" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest/objective/event runtime existe ou foi criado de forma mínima
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

- Condition avançar quest sozinha sem trigger/event.
- Reward aplicado duas vezes após reload ou callback duplicado.
- QuestFlag usada como substituto de QuestState.
- Quest Log revelar objective/reward oculto.
- Main quest expirar por calendário.
- Save/load perder objective progress.
- Quest crítica sem fallback anti-softlock.
- Future hook executado como feature final antes das dependências.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Condition / Trigger Runtime

## Summary
- Spec:
- Wave: WAVE 03
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
