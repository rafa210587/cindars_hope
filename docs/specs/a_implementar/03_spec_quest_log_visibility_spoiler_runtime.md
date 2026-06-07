# SPEC — Quest Log Visibility and Spoiler Control Runtime

> **Spec ID:** `03_spec_quest_log_visibility_spoiler_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P1  
> **Type:** Runtime / Quest / Validation / Hardening  
> **Domain:** Quest / UI Projection / Visibility / Anti-spoiler  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, QuestDefinition, QuestFlags, reward visibility, Quest Log UI prefab/canvas ou bestiary knowledge visibility.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_log_visibility_spoiler_runtime_execution_report.md`.  
> **Depends on:**  
- `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
- `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
- `docs/specs/a_implementar/03_spec_quest_condition_trigger_runtime.md`
- `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`
- `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`
- `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`
- `docs/specs/a_implementar/01_spec_save_section_ownership_registry.md`
- `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
- `docs/specs/a_implementar/03_spec_quest_state_save_load_runtime.md`
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
- `Quest Log UI implementation`
- `Main quest journal projection`
- `Farm order visibility`
- `Festival quest visibility`
- `Bestiary discovery objectives`
> **Scope:** criar ou consolidar a camada de projeção/visibility policy que decide o que o Quest Log pode mostrar sem revelar spoilers.  
> **Out of scope:** layout visual final completo do Quest Log, prefab/canvas final, localization final, tracked quest HUD completo, bestiary UI completa.

---

# /speckit.specify

## 1. Contexto

O direction de quests exige anti-spoiler e VisibilityPolicy para quest, step, objective, reward e hint. Os directions de UI definem que Quest Log mostra objetivo atual conhecido, categoria, pista e recompensa conhecida, sem exibir state interno cru, flags ocultas ou spoiler tier acima do permitido.

Esta spec deve seguir o modelo operacional atual: gerar implementação incremental/residual quando o repo já possui base parcial, nunca recriar sistemas transversais sem auditoria local, e nunca promover runtime/gameplay para `ACCEPTED` apenas por compile.

---

## 2. Problema

Sem uma camada de projeção, o Quest Log pode revelar steps futuros, bosses ocultos, final choices, condições lunares secretas, reward secreto ou informações de Bestiary ainda não descobertas.

Riscos concretos:

```text
duplicidade de estado;
softlock;
recompensa aplicada duas vezes;
quest avançando sem condição;
trigger perdido;
save/load quebrando progressão;
spoiler no log;
estado de Fonte/MainProgression escondido dentro de QuestState;
falha silenciosa por ID inválido;
execução paralela editando os mesmos contracts.
```

---

## 3. Objetivo

Criar runtime/projection contract para transformar QuestState + QuestDefinition + VisibilityPolicy em `QuestLogEntryViewModel`/equivalente seguro, sem tornar UI fonte de verdade.

---

## 4. Source Map Compliance

### 4.1 Global sources read

- `docs/design/SPEC_SOURCE_MAP.md`
- `docs/design/SPECIFICATION_PROCESS.md`
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
- `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
- `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `docs/project/CURRENT_STATE.md`
- `docs/IMPLEMENTATION_STATUS.md`

### 4.2 Domain directions read

- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`

### 4.3 Harness / execution sources expected during implementation

- `CLAUDE.md`
- `AGENTS.md`
- `.claude/rules/testing-quality-gate.md`
- `.claude/skills/spec-execution/SKILL.md`
- `.claude/skills/unity-validation/SKILL.md`

### 4.4 Compliance notes

```text
SPEC_SOURCE_MAP.md exige que specs de quest/objective/event leiam QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md.
Specs que envolvem Quest Log/UI também devem ler UI_UX_FULL_GAMEPLAY_DIRECTION.md e UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md.
Specs que persistem QuestState/QuestFlags/rewards devem ler SAVE_LOAD_FULL_STATE_DIRECTION.md.
Specs que usam tempo, clima, lua ou festival devem respeitar SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md.
Lore/canon deve respeitar VAALARA_GAME_CANON_DIRECTION_v1.0.md.
```

---

## 5. Direction / Refinement Coverage

### 5.1 Covered from directions

```text
VisibilityPolicy for quest, step, objective, reward and hint.
Spoiler control for Arquivista do Silêncio, Pedra Negra, final choices, level 101, Mana conditions, Nyx/cult events, hidden branches and undiscovered boss weaknesses.
Quest Log fields: category, known title, known summary, current objective, numeric progress, hint, NPC/location, deadline, discovered temporal condition, known reward, active/waiting/ready/completed/failed/expired.
UI rules: Quest Log presentation is not source of truth; drawer respects VisibilityPolicy; notification is short and non-blocking.
```

### 5.2 Deferred / future from directions

```text
Final visual Quest Log layout/prefabs.
Localization/text keys.
Tracked quest marker/HUD.
Bestiary UI pages.
Main quest full journal art/content.
Gamepad navigation polish.
```

### 5.3 Explicitly not redefined here

```text
QuestState persistence.
QuestDefinition authoring.
Reward idempotency.
Condition/trigger logic.
UI central modal/input routing principles.
```

---

## 6. Estado atual do repo

Estado comprovado documentalmente:

```text
- Quest/objective/event direction é canônico para QuestDefinition, QuestState, QuestStep, Objective, Condition, Trigger, Reward, QuestFlag e QuestEvent.
- SPEC_EXISTING_IMPLEMENTATION_AUDIT.md classifica Quest/objective/reward como PARTIAL / UNKNOWN.
- Town NPC/dialogue/schedule/quests aparecem como completos em código em docs de status, mas quest engine genérica ampla não está comprovada por auditoria local.
- WAVE 01 IDs/events/save ainda são locks fortes para qualquer spec de quest.
- Validação humana real permanece deferida para o final do lote/wave.
```

Estado que precisa ser confirmado localmente:

```text
- existing Quest Log or quest UI components;
- current quest notifications;
- any visibility/spoiler fields in existing quest data;
- UI projection/view model patterns already used;
- whether hidden/completed quest states already exist;
- whether debug UI shows raw flags.
```

Regra:

```text
Não assumir que já existe quest engine genérica completa sem auditoria local.
Se existir implementação parcial, consolidar e endurecer.
Se não existir, criar apenas o contrato/runtime mínimo previsto nesta spec.
```

---

## 7. User stories / engineering stories

```text
Como designer técnico, quero autorar quests com dados estáveis e visibilidade controlada.
Como sistema de save/load, quero persistir QuestState por ID, sem salvar ScriptableObjects ou objetos Unity.
Como Quest Log, quero mostrar apenas informação conhecida e autorizada.
Como agente executor, quero validar referências quebradas, rewards duplicáveis e softlocks.
Como player, quero quests claras quando descobertas, sem spoiler de mistérios futuros.
```

---

## 8. Escopo

Inclui:

```text
criar ou consolidar a camada de projeção/visibility policy que decide o que o Quest Log pode mostrar sem revelar spoilers.
```

---

## 9. Fora de escopo

Não inclui:

```text
layout visual final completo do Quest Log, prefab/canvas final, localization final, tracked quest HUD completo, bestiary UI completa.
```

---

## 10. Regras de não duplicação

```text
Não criar sistema paralelo de quest se já houver QuestManager/QuestService equivalente.
Não usar QuestFlag como substituto universal de QuestState.
Não esconder MainProgression/FonteAnya dentro de QuestState genérico.
Não criar eventos próprios de quest que dupliquem GameEventBus/core events.
Não persistir ScriptableObject, GameObject, MonoBehaviour, Transform, UI state ou callback em save.
Não criar UI final se a spec é runtime/save/contract.
Não aplicar reward diretamente sem trilha idempotente.
```

---

## 11. Critérios de aceite

### 11.1 Projection safety

- Hidden quests do not appear before discovery.
- Hidden objectives/rewards/hints are not shown early.
- Known objective, known deadline and known condition appear when discovered.
- Raw flags/internal QuestState are not shown in player UI.

### 11.2 Spoiler tiers

- VisibilityPolicy or equivalent supports spoiler gating.
- Main quest final choices are not revealed early.
- Bestiary/weakness-related data is hidden until discovered.

### 11.3 Tests/validation

- Projection of hidden quest hides it.
- Projection of active quest shows only known current objective.
- Projection of reward unknown hides reward.
- Debug projection, if present, is separate and clearly marked.

### 11.x Execution report obrigatório

Criar:

```text
docs/validation/03_spec_quest_log_visibility_spoiler_runtime_execution_report.md
```

O relatório deve incluir:

```text
fontes lidas;
arquivos auditados;
estado real encontrado;
decisões de consolidação vs criação nova;
contratos/DTOs criados ou preservados;
validações rodadas;
NOT RUN entries com motivo/impacto/mitigação;
Testing Quality Gate;
risco residual;
próximas specs desbloqueadas.
```

---

# /speckit.plan

## 12. Arquitetura alvo

Possible files:

```text
Assets/_Game/Scripts/Quests/Visibility/QuestVisibilityPolicy.cs
Assets/_Game/Scripts/Quests/Visibility/QuestLogProjectionService.cs
Assets/_Game/Scripts/Quests/Visibility/QuestLogEntryViewModel.cs
Assets/_Game/Scripts/Editor/Validation/ValidateQuestVisibility.cs
Assets/_Game/Tests/EditMode/Quests/QuestLogVisibilityTests.cs
docs/validation/03_spec_quest_log_visibility_spoiler_runtime_execution_report.md
```

UI prefabs/canvases are out of scope unless already existing and only require binding.

---

## 13. Contratos, dados e eventos

### 13.1 Data contracts

```text
QuestVisibilityPolicy:
  SpoilerTier
  JournalVisibility
  RevealConditions
  HiddenUntilDiscovered
  ShowRewardPolicy
  ShowObjectivePolicy
  ShowHintPolicy

QuestLogProjection:
  QuestId
  Category
  DisplayTitle
  DisplaySummary
  CurrentObjectiveText
  ProgressText
  HintText
  DeadlineText
  KnownRewardText
  StateLabel
  Trackable
  Hidden
```

### 13.2 Runtime contracts

```text
Projection reads QuestState/QuestDefinition.
Projection never mutates quest state.
Hidden quest stays absent until discovered unless debug mode.
```

### 13.3 Event contracts

```text
QuestUpdated notification may be emitted after projection changes.
Notification text must not expose hidden data.
```

### 13.4 Save contracts

```text
Visibility uses persisted KnownObjectiveIds/KnownHints/Discovered/ChoiceHistory as needed.
UI selection/scroll state is not saved.
```

### 13.5 UI contracts

```text
Quest Log follows UI_UX_MENU_SCREEN_FLOWS_DIRECTION.
Quest detail drawer respects visibility.
Debug view, if any, must be marked debug.
```

---

## 14. Sistemas afetados

```text
Quest runtime;
Quest Log projection;
UI view models/bindings;
QuestState known/discovered fields;
Bestiary knowledge handoff;
Notifications;
Editor validation/EditMode tests.
```

---

## 15. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/**Quest**
Assets/_Game/Scripts/Editor/Validation/**Quest**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_log_visibility_spoiler_runtime_execution_report.md
```

---

## 16. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab, unless an existing quest UI prefab requires metadata-only safe binding and is explicitly listed
Assets/**/*.asset, salvo quest visibility data asset explicitamente listado
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 17. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
rg -n "QuestLog|QuestJournal|VisibilityPolicy|Spoiler|KnownObjective|KnownHint|TrackedQuest|QuestNotification|HiddenCompleted" Assets/_Game/Scripts
```

### Fase 1 — Projection

```text
1. Consolidar VisibilityPolicy.
2. Criar projection service/view model se não existir.
3. Separar player projection de debug projection.
4. Não criar layout visual final.
```

### Fase 2 — Tests

```text
1. Hidden quest hidden.
2. Known objective visible.
3. Unknown reward hidden.
4. Spoiler tier respected.
```

---

## 18. Ordem segura de execução

```text
1. Ler fontes obrigatórias e confirmar branch.
2. Auditar implementação existente com rg/git grep.
3. Decidir: consolidar existente / criar mínimo / deferir com backlog técnico.
4. Implementar apenas o escopo desta spec.
5. Criar/ajustar testes EditMode quando houver lógica determinística.
6. Rodar validações obrigatórias aplicáveis.
7. Criar execution report.
8. Não atualizar SPEC_EXECUTION_ORDER.md.
9. Não mover specs para implementados.
```

---

## 19. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_03_QUEST_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - quest contract/state/reward/flags/visibility specs em paralelo;
  - save/load specs;
  - event bus specs;
  - UI quest log specs;
  - farm orders/festival/cave contracts que adicionem objectives/triggers.
- Shared files/systems that require lock:
  - `Assets/_Game/Scripts/Quests/**`
  - `Assets/_Game/Scripts/Save/**`
  - `Assets/_Game/Scripts/Core/Events/**`
  - quest data assets/registries
  - execution reports
- Reason:
  - Visibility consumes QuestState and can be broken by simultaneous changes to quest contracts, flags or rewards.

---

## 20. Impacto em save/load

```text
Does this change save schema? CONDITIONAL — only if this spec explicitly creates/changes QuestState-related DTOs.
Does this add a save section? SHOULD BE NO unless prior save ownership spec has approved QuestStateSection.
Does this require migration? YES if existing persisted shape changes; otherwise NO with explicit justification.
Does this persist Unity references? MUST BE NO.
```

Save/load rules:

```text
QuestState persists by QuestId.
QuestDefinition is referenced by QuestId, not serialized as asset.
QuestFlags are persistable facts, not replacement for QuestState.
GrantedRewardIds/GrantedFlagIds prevent double application.
UI state is rebuilt from runtime state and never persisted as gameplay truth.
```

---

## 21. Impacto em eventos

```text
Adds events: CONDITIONAL — only if event contract spec allows it and no equivalent exists.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if adding runtime subscribers.
```

Event rules:

```text
Quest system consumes gameplay events.
Quest system does not become source of truth for farm/combat/inventory/economy/bestiary.
Conditions do not advance quests alone; triggers/events advance only when conditions allow.
```

---

## 22. Impacto em UI/Unity

```text
Changes UI: CONDITIONAL, only if this spec explicitly covers Quest Log/visibility presentation.
Changes scenes: NO.
Changes prefabs: NO unless a dedicated UI spec explicitly permits it.
Changes ScriptableObjects/assets: CONDITIONAL — quest data assets only if safe and listed.
Requires PlayMode automated or final human scenario: YES only if UI/scene/input flow is touched; otherwise NO.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION when UI/gameplay scenario exists; otherwise NOT REQUIRED.
```

---

## 23. Riscos técnicos

```text
Risco: criar quest engine incompatível com eventos/save existentes.
Mitigação: depender de IDs/events/save WAVE 01 e auditar implementação real.

Risco: reward aplicar duas vezes após reload.
Mitigação: GrantedRewardIds/transaction/idempotency.

Risco: Quest Log revelar spoiler.
Mitigação: VisibilityPolicy e SpoilerTier.

Risco: QuestFlag virar estado genérico caótico.
Mitigação: QuestFlag registry e separação QuestState/MainProgression/FonteAnya.

Risco: spec grande demais virar implementação massiva.
Mitigação: manter escopo estrito e registrar deferred/future.
```

---

## 24. Rollback

```text
Remover arquivos novos criados por esta spec.
Reverter ajustes pontuais em Quests/Save/UI/Event contracts.
Remover tests/validators criados.
Remover execution report.
Nenhum scene/prefab/ProjectSettings/Packages deve ter sido alterado.
```

---

# /speckit.tasks

## 25. Tasks

- [ ] T001 — Ler fontes obrigatórias.
- [ ] T002 — Auditar Quest Log/projection existente.
- [ ] T003 — Criar/consolidar VisibilityPolicy.
- [ ] T004 — Criar projection/view model seguro.
- [ ] T005 — Separar debug/raw state de player UI.
- [ ] T006 — Criar EditMode tests de spoiler/visibility.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar execution report.

---

## 26. Validações obrigatórias

Docs validation.
C# build if C# changed.
Unity compile if Unity C# changed.
EditMode tests for projection/visibility.
PlayMode/final human validation only if UI scene/prefab is touched, otherwise NOT REQUIRED.

---

## 27. Testing Quality Gate

- Changed deterministic logic: YES.
- Requires EditMode tests: YES for visibility projection.
- Requires PlayMode automated or final human scenario: YES only if UI/prefab/input is changed; then DEFERRED_TO_FINAL_VALIDATION.
- Requires regression test: YES if fixing spoiler leak or raw flag display.
- Human validation timing: NOT REQUIRED for pure projection; DEFERRED_TO_FINAL_VALIDATION if UI touched.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; visibility tests PASS; no hidden data exposed in tested projections; execution report created.

---

## 28. Definition of Done

```text
VisibilityPolicy/projection created or consolidated.
Quest Log player projection hides spoilers.
Debug/raw state separated.
Known/discovered fields respected.
Tests added.
Execution report created.
No final UI layout required.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 29. Anti-regressão

```text
Quest Log is not source of truth.
Do not reveal hidden objectives/rewards/finals early.
Do not show raw QuestFlags in player UI.
Do not save UI state.
Do not rely on debug HUD for player communication.
No ACCEPTED without visibility tests.
```

---

## 30. Notas para execução posterior

This spec can execute before final Quest Log UI, because it creates the safe projection layer future UI will bind to. It should not build the entire menu screen.
