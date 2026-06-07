# SPEC — Quest State Save/Load Runtime

> **Spec ID:** `03_spec_quest_state_save_load_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P0  
> **Type:** Runtime / Quest / Validation / Hardening  
> **Domain:** Quest / Save / State / Persistence  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, QuestDefinition, QuestFlags, rewards, Quest Log, save/load, GameSaveData ou event triggers.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_state_save_load_runtime_execution_report.md`.  
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
> **Blocks:**  
- `03_spec_quest_reward_application_idempotency_runtime.md`
- `03_spec_quest_log_visibility_spoiler_runtime.md`
- `03_spec_quest_flags_registry_runtime.md`
- `farm orders adapter`
- `festival expiry runtime`
- `main/fonte progression hooks`
> **Scope:** persistir e restaurar QuestState genérico com IDs estáveis, sem esconder MainProgression ou FonteAnya dentro da seção genérica de quests.  
> **Out of scope:** main quest progression completa, FonteAnyaSection completa, Quest Log UI final, farm orders adapter, festival runtime, social/companion/pet quests futuras.

---

# /speckit.specify

## 1. Contexto

O direction de quests define QuestState como estado salvo da quest e QuestDefinition como dado autorado referenciado por QuestId. Ele também define que QuestState, MainProgression e FonteAnya precisam ser seções separadas de save. Esta spec transforma essa parte do refinement em contrato executável de persistência para quests genéricas.

Esta spec deve seguir o modelo operacional atual: gerar implementação incremental/residual quando o repo já possui base parcial, nunca recriar sistemas transversais sem auditoria local, e nunca promover runtime/gameplay para `ACCEPTED` apenas por compile.

---

## 2. Problema

O sistema de quests precisa sobreviver a save/load, troca de cena, day transition e reload no meio de eventos. Sem uma seção clara de QuestState, o projeto tende a espalhar progresso em NPC flags, dialogue flags, farm order state e handlers locais.

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

Criar ou consolidar `QuestStateSection` para quests genéricas, side quests, farm orders, festival quests, tutorial e hidden quests, com persistência por QuestId, ObjectiveState, KnownObjectiveIds, ChoiceHistory, GrantedRewardIds e GrantedFlagIds.

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
QuestState fields: QuestId, State, CurrentStepId, CompletedStepIds, FailedStepIds, ObjectiveStates, KnownObjectiveIds, KnownHints, StartedAtDay, StartedAtTime, CompletedAtDay, ExpiresAtDay, Tracked, Discovered, FailureReason, ChoiceHistory, GrantedRewardIds, GrantedFlagIds.
QuestDefinition remains authored data and is referenced by QuestId.
QuestStateSection is separate from MainProgressionSection and FonteAnyaSection.
Save/load persists quest state, not UI state.
Visibility/spoiler state is represented as known/discovered fields, not raw UI.
```

### 5.2 Deferred / future from directions

```text
MainProgressionSection detailed implementation.
FonteAnyaSection detailed implementation.
Quest Log visual UI.
FarmOrder board adapter.
Festival quest expiry runtime.
Social/companion/pet quest states.
Localization/text key final policy.
```

### 5.3 Explicitly not redefined here

```text
Generic quest contracts from 03_spec_quest_objective_event_contract_runtime.
Condition/Trigger runtime from 03_spec_quest_condition_trigger_runtime.
Save provider architecture and restore order from WAVE 01.
Quest reward idempotency, which has its own spec.
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
- existing QuestManager/QuestService/QuestState classes, if any;
- existing save fields that already store quest/dialogue/order flags;
- GameSaveData support for quest state or lack thereof;
- any NPC/dialogue quest state currently embedded in NPC save;
- objective progress models currently used by farm orders or town quests;
- migration/normalization behavior for missing quest state section.
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
persistir e restaurar QuestState genérico com IDs estáveis, sem esconder MainProgression ou FonteAnya dentro da seção genérica de quests.
```

---

## 9. Fora de escopo

Não inclui:

```text
main quest progression completa, FonteAnyaSection completa, Quest Log UI final, farm orders adapter, festival runtime, social/companion/pet quests futuras.
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

### 11.1 QuestStateSection

- A seção existe ou é consolidada sem duplicar GameSaveData.
- QuestState é persistido por QuestId.
- ObjectiveState persiste progresso por ObjectiveId.
- KnownObjectiveIds/KnownHints controlam informação descoberta.
- GrantedRewardIds e GrantedFlagIds existem ou são explicitamente delegados para reward spec.

### 11.2 Separação obrigatória

- MainProgression não é salvo dentro do QuestState genérico.
- FonteAnya não é salva dentro do QuestState genérico.
- NPC save não vira dono de quest state formal.

### 11.3 Load e normalização

- Save antigo sem QuestStateSection gera seção vazia segura.
- QuestId desconhecido não quebra load inteiro sem diagnóstico.
- ObjectiveState órfão é reportado.
- Reward já aplicado não é reaplicado por load.

### 11.4 Validação

- EditMode test ou validator cobre round-trip básico de QuestStateData.
- Validator detecta QuestState sem QuestId, duplicate QuestId e objective state sem objective id quando aplicável.

### 11.x Execution report obrigatório

Criar:

```text
docs/validation/03_spec_quest_state_save_load_runtime_execution_report.md
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

Preservar `GameSaveData` como DTO raiz.

Possíveis arquivos-alvo:

```text
Assets/_Game/Scripts/Quests/Runtime/QuestState.cs
Assets/_Game/Scripts/Quests/Runtime/QuestObjectiveState.cs
Assets/_Game/Scripts/Save/QuestStateSaveData.cs
Assets/_Game/Scripts/Quests/Runtime/QuestStateSerializer.cs
Assets/_Game/Scripts/Editor/Validation/ValidateQuestStateSave.cs
Assets/_Game/Tests/EditMode/Quests/QuestStateSaveLoadTests.cs
docs/validation/03_spec_quest_state_save_load_runtime_execution_report.md
```

Se já existirem equivalentes, consolidar o existente.

---

## 13. Contratos, dados e eventos

### 13.1 Data contracts

```text
QuestStateData:
  QuestId
  State
  CurrentStepId
  CompletedStepIds
  FailedStepIds
  ObjectiveStates
  KnownObjectiveIds
  KnownHints
  StartedAtDay
  StartedAtTime
  CompletedAtDay
  ExpiresAtDay
  Tracked
  Discovered
  FailureReason
  ChoiceHistory
  GrantedRewardIds
  GrantedFlagIds

QuestObjectiveStateData:
  ObjectiveId
  CurrentAmount
  Completed
  Failed
  Known
  LastTriggerId, if needed
  UpdatedAtDay, if needed
```

### 13.2 Runtime contracts

```text
QuestDefinition is not saved.
Runtime resolves QuestDefinition by QuestId.
Unknown QuestId loads with clear fallback or error according to invalid ID rules.
Missing QuestStateSection normalizes to empty section without revealing quests.
```

### 13.3 Event contracts

```text
Load should not replay rewards.
Post-load quest notifications are optional and must not duplicate completion rewards.
```

### 13.4 Save contracts

```text
QuestStateSection is independent from MainProgressionSection and FonteAnyaSection.
QuestState stores known/discovered data needed for anti-spoiler Quest Log.
```

### 13.5 UI contracts

```text
Quest Log reads QuestState projection.
Quest Log does not persist selected tab, scroll or raw UI state.
```

---

## 14. Sistemas afetados

```text
Quest runtime;
SaveManager/GameSaveData;
QuestState DTOs;
Quest objective state;
Quest flags/rewards handoff;
Quest Log projection;
Editor validation/EditMode tests.
```

---

## 15. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**Quest*SaveData*.cs
Assets/_Game/Scripts/Editor/Validation/**Quest**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_state_save_load_runtime_execution_report.md
```

---

## 16. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset, salvo data asset de quest se explicitamente autorizado e listado
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
rg -n "QuestState|QuestDefinition|QuestManager|QuestService|QuestId|ObjectiveState|GrantedReward|GrantedFlag|QuestFlag" Assets/_Game/Scripts
rg -n "QuestState|QuestFlags|GameSaveData|SaveData" Assets/_Game/Scripts/Save Assets/_Game/Scripts
```

### Fase 1 — Contrato de save

```text
1. Mapear modelos existentes.
2. Decidir se QuestStateSection já existe.
3. Criar/consolidar DTO simples.
4. Normalizar seção ausente.
5. Validar por round-trip.
```

### Fase 2 — Handoff

```text
1. Deixar reward idempotency para spec própria.
2. Deixar Quest Log UI para spec própria.
3. Registrar gaps.
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
  - QuestState is the persistence backbone for multiple quest specs and must not drift while rewards, flags or UI are being implemented.

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

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar classes/DTOs de quest existentes.
- [ ] T003 — Auditar GameSaveData e save sections relacionadas.
- [ ] T004 — Criar ou consolidar QuestStateSection.
- [ ] T005 — Garantir separação de MainProgression/FonteAnya.
- [ ] T006 — Implementar normalização de seção ausente se aplicável.
- [ ] T007 — Criar EditMode tests/validator de round-trip.
- [ ] T008 — Rodar validações obrigatórias.
- [ ] T009 — Criar execution report.

---

## 26. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

C# builds quando C# mudar:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando Unity C# mudar:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests:

```text
QuestState round-trip;
missing section normalization;
unknown QuestId fallback/diagnostic if registry exists.
```

---

## 27. Testing Quality Gate

- Changed deterministic logic: YES if DTO normalization/serializer/validator is created or changed.
- Requires EditMode tests: YES.
- Requires PlayMode automated or final human scenario: NO for DTO/save round-trip; DEFERRED_TO_FINAL_VALIDATION only if scene-bound quest flow is touched.
- Requires regression test: YES if fixing existing quest reload bug.
- Human validation timing: NOT REQUIRED unless gameplay/UI flow is touched.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; EditMode round-trip/normalization PASS or justified NOT RUN; execution report created; no MainProgression/FonteAnya hidden inside QuestState.

---

## 28. Definition of Done

```text
QuestStateSection created or consolidated.
QuestState persists by QuestId.
ObjectiveState persists by ObjectiveId.
Known/discovered data exists for spoiler-safe UI.
MainProgression/FonteAnya kept separate.
EditMode test/validator added or residual risk documented.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 29. Anti-regressão

```text
QuestDefinition is not saved as ScriptableObject.
QuestState does not replace MainProgression/FonteAnya.
QuestFlag does not replace QuestState.
Load does not replay rewards.
UI state is not saved.
No schema change without migration decision.
No ACCEPTED by compile only.
```

---

## 30. Notas para execução posterior

This spec should run before reward idempotency and Quest Log visibility specs. If existing quest state is already embedded in NPC/dialogue systems, execution must report migration strategy instead of silently duplicating state.
