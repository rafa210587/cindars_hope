# SPEC — Quest / Objective / Event Contract Runtime

> **Spec ID:** `03_spec_quest_objective_event_contract_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P0  
> **Type:** Runtime / Data Contracts / Quest System / Hardening  
> **Domain:** Quest / Objective / Event / State / Definitions  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, QuestDefinition, reward application, quest save/load, quest log, dialogue hooks, farm orders, festival quests, cave contracts ou main progression.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Core/Events/**Quest*`, `Assets/_Game/Scripts/Save/**Quest*`, `docs/validation/03_spec_quest_objective_event_contract_runtime_execution_report.md`.  
> **Depends on:**  
- `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`
- `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`
- `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`
- `docs/specs/a_implementar/01_spec_invalid_id_fallback_rules.md`
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
> **Blocks:**  
- `03_spec_quest_condition_trigger_runtime.md`
- `03_spec_quest_reward_application_idempotency_runtime.md`
- `03_spec_quest_flags_registry_runtime.md`
- `03_spec_quest_state_save_load_runtime.md`
- `03_spec_quest_log_visibility_spoiler_runtime.md`
- `farm orders adapter`
- `festival quest integration`
- `cave contracts`
> **Scope:** criar/consolidar os contratos base de QuestDefinition, QuestState, QuestStep, Objective, categorias, estados, visibility policy e eventos conceituais, sem implementar rewards completos, UI completa ou main quest conteúdo.  
> **Out of scope:** conteúdo de main quest, UI final de quest log, aplicação completa de rewards, adapters de farm/cave/festival, ferramenta dev completa, social/companion/pet runtime.

---

# /speckit.specify

## 1. Contexto

O direction de Quest/Objective/Event define o sistema genérico que deve ser usado por main quest, side quests, farm orders, festival quests, social future, companion future, cave contracts, tutorial e hidden quests. Ele separa Definition, State, Steps, Objectives, Conditions, Triggers, Rewards e Flags.

Esta spec é a primeira WAVE 03 e deve criar o contrato base que as demais specs consomem.

---

## 2. Problema

Sem contratos base, cada domínio tende a criar flags próprias, contadores próprios ou hacks de diálogo/save. Isso causa softlock, reward duplicado, quest log inconsistente, spoiler, trigger perdido e save/load quebrado.

O risco aumenta porque farm orders, festival quests, cave contracts e main quest precisam do mesmo sistema base com categorias e adapters diferentes.

---

## 3. Objetivo

Criar ou consolidar contratos runtime/data para QuestDefinition, QuestState, QuestStep, Objective, QuestCategory, QuestState enum, CompletionMode, VisibilityPolicy e eventos base de lifecycle, preservando separação entre QuestState, MainProgression e FonteAnya.

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
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`

### 4.3 Covered from directions

```text
QuestDefinition, QuestState, QuestStep and Objective conceptual contracts.
Quest categories: Main, Side, FarmOrder, Festival, CaveContract, Tutorial, Hidden, System, futures marked.
QuestState enum: Unknown, Discovered, Available, Active, Waiting, ReadyToComplete, Completed, Failed, Expired, HiddenCompleted, Blocked.
CompletionMode core values.
VisibilityPolicy requirement for anti-spoiler.
QuestState references QuestDefinition by QuestId.
Quest system consumes events and does not replace farm/combat/inventory/economy/bestiary/Fonte systems.
```

### 4.4 Deferred / future from directions

```text
Condition runtime implementation.
Trigger/event matching runtime.
Reward application/idempotency.
Quest save/load.
Quest Log UI.
QuestFlags registry.
Farm orders adapter.
Festival expiry implementation.
Main quest content/progression.
Social/companion/pet runtime.
```

### 4.5 Explicitly not redefined here

```text
Não redefine main quest lore/progression.
Não redefine FonteAnyaSection/MainProgressionSection.
Não redefine UI presentation beyond contract needs.
Não redefine GameEventBus.
Não redefine save root DTO.
```

---

## 5. Estado atual do repo

```text
Quest direction exists and defines generic quest/objective/event contracts.
Audit report classifies quest/objective/event as PARTIAL / UNKNOWN.
Town NPC/dialogue/schedule quests exist in code according to audit, but generic quest engine is not proven by connector.
WAVE 01 stable IDs/events/save contracts must be in place before execution.
```

---

## 6. User stories / engineering stories

```text
Como main quest, quero usar o sistema genérico sem esconder MainProgression dentro de QuestState.
Como farm order, quero categoria própria sem sistema paralelo.
Como festival quest, quero categoria com expiry clara.
Como cave contract, quero objetivos de depth/enemy/resource sem hacks.
Como Quest Log, quero saber o que pode ser mostrado sem spoiler.
```

---

## 7. Escopo

```text
Auditar código existente de quests/dialogue/orders.
Criar/consolidar QuestDefinition/QuestState/QuestStep/Objective contracts.
Criar enums/categorias/policies mínimas.
Definir lifecycle events de quest.
Documentar separação QuestState/MainProgression/FonteAnya.
Criar tests/validators quando praticável.
Criar execution report.
```

---

## 8. Fora de escopo

```text
Não implementar condition/trigger engine completa.
Não aplicar rewards.
Não criar Quest Log UI.
Não criar conteúdo de quests.
Não implementar farm/cave/festival adapters.
Não implementar social/companion/pet.
```

---

## 9. Regras de não duplicação

```text
Não criar sistema paralelo de farm orders.
Não usar QuestFlag como substituto de QuestState.
Não colocar main progression completa dentro de QuestState genérico.
Não usar quest system como fonte primária de farm/combat/inventory/economy.
Não revelar hidden objectives/rewards.
```

---

## 10. Critérios de aceite

### 10.1 Contratos base

- Existem contratos ou data classes para QuestDefinition, QuestState, QuestStep e Objective, ou equivalentes existentes foram consolidados.
- QuestId é stable ID.
- QuestDefinition é dado autorado; QuestState é estado runtime/persistido.

### 10.2 Categorias e estados

- Categorias canônicas existem ou são documentadas em data enum/contract.
- QuestState enum cobre os estados do direction.
- CompletionMode core existe ou fica claramente preparado.

### 10.3 Anti-spoiler

- VisibilityPolicy existe no contrato ou como campo preparatório.
- Hidden quest não aparece antes de descoberta.
- Main quest não revela boss/final cedo por contrato.

### 10.4 Evidence

- Validator/teste mínimo criado quando praticável.
- Execution report criado em `docs/validation/03_spec_quest_objective_event_contract_runtime_execution_report.md`.

---

# /speckit.plan

## 11. Arquitetura alvo

Possíveis alvos:
```text
Assets/_Game/Scripts/Quests/QuestDefinition.cs
Assets/_Game/Scripts/Quests/QuestState.cs
Assets/_Game/Scripts/Quests/QuestStepDefinition.cs
Assets/_Game/Scripts/Quests/QuestObjectiveDefinition.cs
Assets/_Game/Scripts/Quests/QuestCategory.cs
Assets/_Game/Scripts/Quests/QuestStateKind.cs
Assets/_Game/Scripts/Quests/QuestVisibilityPolicy.cs
Assets/_Game/Scripts/Core/Events/QuestEvents.cs
Assets/_Game/Tests/EditMode/Quests/QuestContractTests.cs
docs/validation/03_spec_quest_objective_event_contract_runtime_execution_report.md
```

Se já houver classes equivalentes, consolidar sem duplicar.

---

## 12. Contratos, dados e eventos

### Data contracts

QuestDefinition é autorado por QuestId. QuestState persiste progresso por QuestId.

### Runtime contracts

Quest runtime deve usar Definition -> State -> Steps -> Objectives.

### Event contracts

Quest lifecycle events são notificação. Gameplay events vêm de sistemas primários.

### Save contracts

QuestState, MainProgression e FonteAnya são seções separadas.

### UI contracts

Quest Log apresenta apenas o que VisibilityPolicy permite.

---

## 13. Sistemas afetados

```text
Quest contracts
Stable IDs
GameEventBus quest events
Save future QuestState
Quest Log future UI
Dialogue/farm/festival/cave adapters future
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**Quest*
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_objective_event_contract_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset salvo test/data contract asset se explicitamente necessário e reportado
docs/specs/SPEC_EXECUTION_ORDER.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
rg -n "Quest|Objective|QuestState|QuestDefinition|QuestFlag|Reward|DialogueChoice" Assets/_Game/Scripts
```

### Fase 1 — Contratos

Consolidar/definir contracts mínimos sem implementar todos os behaviors.

### Fase 2 — Validation

Adicionar tests de contract/enum/visibility simples quando possível.

### Fase 3 — Report

Reportar classes criadas/consolidadas, gaps e dependências.

---

## 17. Ordem segura de execução

```text
Ler fontes obrigatórias.
Auditar quest/dialogue/order code existente.
Consolidar contracts base.
Adicionar enums/policies mínimos.
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
  - qualquer spec que altere QuestState, QuestDefinition, reward application, quest save/load, quest log, dialogue hooks, farm orders, festival quests, cave contracts ou main progression.
- Reason:
  - Quest contracts bloqueiam todo o sistema de quests; execução paralela pode criar contratos incompatíveis.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO by default.
Does this add save section? NO; save/load fica em spec própria.
Uses stable IDs: YES.
```

---

## 20. Impacto em eventos

```text
Adds events: CONDITIONAL lifecycle events only.
Changes existing events: SHOULD BE NO.
Quest consumes gameplay events, not replaces them.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO.
Requires PlayMode/final human scenario: NO.
Quest Log UI deferred.
```

---

## 22. Riscos técnicos

```text
Risco: duplicar quest code existente. Mitigação: auditoria local primeiro.
Risco: colocar main progression no QuestState. Mitigação: separar sections.
Risco: definir contract grande demais. Mitigação: mínimo base + future hooks.
Risco: revelar spoiler por fields/UI. Mitigação: VisibilityPolicy.
```

---

## 23. Rollback

```text
Remover contracts/tests criados.
Reverter event contracts se criados.
Remover execution report.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias.
- [ ] T002 — Auditar quest-related code existente.
- [ ] T003 — Criar/consolidar QuestDefinition/State/Step/Objective.
- [ ] T004 — Criar enums/policies mínimas.
- [ ] T005 — Adicionar tests/validators quando praticável.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar execution report.

---

## 25. Validações obrigatórias

Docs validation.

C# build/Unity compile se C# alterado.

EditMode tests para contracts/visibility quando criados.

---

## 26. Testing Quality Gate

```text
Changed deterministic logic: YES if validators/contracts logic created.
Requires EditMode tests: YES for validation logic when practical.
Requires PlayMode automated or final human scenario: NO.
Requires regression test: NO unless fixing known quest contract bug.
Human validation timing: NOT REQUIRED.
Minimum validation evidence for ACCEPTED: docs validation PASS; C# build/Unity compile PASS if C# changed; tests/validators PASS or justified NOT RUN; report created.
```

---

## 27. Definition of Done

```text
Quest contracts created/consolidated.
Quest categories/states/visibility represented.
QuestState vs MainProgression/FonteAnya separation documented.
No gameplay domain replaced by quest system.
Execution report created.
```

---

## 28. Anti-regressão

```text
Não usar QuestFlag como QuestState.
Não esconder main progression genérica em quests.
Não revelar hidden quests.
Não aplicar rewards nesta spec.
Não criar UI Quest Log nesta spec.
```

---

## 29. Notas para execução posterior

Esta spec deve preceder condition/trigger, reward, flags, save/load and quest log specs.
