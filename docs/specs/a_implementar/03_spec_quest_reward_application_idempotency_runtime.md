# SPEC — Quest Reward Application Idempotency Runtime

> **Spec ID:** `03_spec_quest_reward_application_idempotency_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P0  
> **Type:** Runtime / Quest / Validation / Hardening  
> **Domain:** Quest / Rewards / Idempotency / Anti-duplication  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, QuestFlags, Quest Log, reward definitions, economy/shop unlocks, inventory rewards ou save/load.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_reward_application_idempotency_runtime_execution_report.md`.  
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
> **Blocks:**  
- `Quest completion runtime`
- `Farm orders rewards`
- `Festival rewards`
- `Fonte reward hooks`
- `Shop unlock rewards`
- `Skill/recipe/spell unlock rewards`
> **Scope:** implementar ou consolidar aplicação idempotente de rewards de quest, evitando double reward após save/load, trigger repetido ou reload.  
> **Out of scope:** criar todos os reward types finais, balancear economia, implementar main quest rewards finais, social/pet/companion rewards futuras, UI final de recompensa.

---

# /speckit.specify

## 1. Contexto

O direction de quests exige que rewards sejam idempotentes, registrem GrantedRewardIds quando necessário e não apliquem duas vezes após save/load. Esta spec transforma essa regra em runtime/validator executável.

Esta spec deve seguir o modelo operacional atual: gerar implementação incremental/residual quando o repo já possui base parcial, nunca recriar sistemas transversais sem auditoria local, e nunca promover runtime/gameplay para `ACCEPTED` apenas por compile.

---

## 2. Problema

Sem idempotência, uma quest pode aplicar Gold/Item/Recipe/SkillPoint/FonteUpgrade/ShopUnlock mais de uma vez por trigger repetido, reload, callback duplicado ou bug de completion.

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

Criar um contrato de reward application que torna cada reward transacional/idempotente, com `RewardId`, tracking de `GrantedRewardIds`, validação de duplicate rewards e regressão contra double application.

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
Reward types: Gold, Item, Recipe, ToolUnlock, EquipmentUnlock, SpellUnlock, SkillPoint, SkillTreeUnlock, KnowledgeUnlock, BestiaryEntryUnlock, FonteUpgrade, LivingWaterCharge, QuestFlagGrant, QuestFlagClear, AreaUnlock, CaveDepthUnlock, ShopUnlock, ShopStockUnlock, DialogueUnlock, NpcScheduleUnlock, FestivalUnlock.
Rule: Reward must be idempotent.
Rule: Reward must not apply twice after save/load.
Rule: Reward should register GrantedRewardIds when needed.
Reward respects economy and Bestiary spoiler/Fonte progression constraints.
```

### 5.2 Deferred / future from directions

```text
Full balancing of reward amounts.
Full main quest reward set.
Full social/companion/pet rewards.
All future reward types implementation.
Reward choice UI.
Localization/text feedback.
```

### 5.3 Explicitly not redefined here

```text
QuestState shape.
QuestFlag registry shape.
Inventory/economy/shop implementations.
FonteAnya runtime implementation.
Bestiary knowledge implementation.
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
- existing reward application classes/functions;
- quest completion handlers;
- whether GrantedRewardIds already exist in save;
- existing reward DTO/data assets;
- shop unlock/recipe unlock/spell unlock mechanisms;
- current behavior on reload after completion.
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
implementar ou consolidar aplicação idempotente de rewards de quest, evitando double reward após save/load, trigger repetido ou reload.
```

---

## 9. Fora de escopo

Não inclui:

```text
criar todos os reward types finais, balancear economia, implementar main quest rewards finais, social/pet/companion rewards futuras, UI final de recompensa.
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

### 11.1 Reward contract

- Each reward has stable RewardId.
- Reward application checks prior grant state before applying.
- Repeated trigger does not duplicate reward.
- Reload after completion does not duplicate reward.
- Failure is recorded with clear reason.

### 11.2 Reward ledger

- GrantedRewardIds is used or equivalent existing ledger is consolidated.
- Reward ledger is persisted.
- Ledger does not expose hidden/spoiler rewards to UI unless allowed.

### 11.3 Tests

- Applying same reward twice grants once.
- Save/load after reward does not regrant.
- Reward failure does not mark reward as granted unless explicitly designed.
- QuestFlagGrant/QuestFlagClear idempotency covered when implemented.

### 11.x Execution report obrigatório

Criar:

```text
docs/validation/03_spec_quest_reward_application_idempotency_runtime_execution_report.md
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
Assets/_Game/Scripts/Quests/Rewards/QuestRewardDefinition.cs
Assets/_Game/Scripts/Quests/Rewards/IQuestRewardApplier.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicationService.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicationResult.cs
Assets/_Game/Scripts/Editor/Validation/ValidateQuestRewards.cs
Assets/_Game/Tests/EditMode/Quests/QuestRewardIdempotencyTests.cs
docs/validation/03_spec_quest_reward_application_idempotency_runtime_execution_report.md
```

If equivalents exist, consolidate existing contracts.

---

## 13. Contratos, dados e eventos

### 13.1 Data contracts

```text
QuestRewardDefinition:
  RewardId
  RewardType
  TargetId
  Amount
  Parameters
  IdempotencyPolicy
  VisibilityPolicy
  DebugTags

QuestRewardApplicationResult:
  RewardId
  Applied
  AlreadyApplied
  Failed
  FailureReason
```

### 13.2 Runtime contracts

```text
Reward application must be atomic at quest-system level.
RewardId must be stable.
Reward is skipped if already in GrantedRewardIds.
Reward failures must be explicit, not silent.
```

### 13.3 Event contracts

```text
QuestCompleted may request reward application.
RewardApplied event is optional and must not be source of persistence.
```

### 13.4 Save contracts

```text
GrantedRewardIds persist in QuestState or equivalent reward ledger.
Reload must not reapply already granted reward.
```

### 13.5 UI contracts

```text
UI can show reward known/unknown, but cannot be required for reward application.
```

---

## 14. Sistemas afetados

```text
Quest runtime;
QuestState reward ledger;
Inventory/economy/recipe/spell/shop unlock adapters;
QuestFlags;
Save/load;
Editor validation/EditMode tests.
```

---

## 15. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**Quest*SaveData*.cs
Assets/_Game/Scripts/Editor/Validation/**Quest**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_reward_application_idempotency_runtime_execution_report.md
```

---

## 16. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset, salvo quest/reward data asset explicitamente listado
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
rg -n "Reward|GrantedReward|QuestReward|ApplyReward|QuestCompleted|RecipeUnlock|ShopUnlock|SkillPoint|FonteUpgrade|LivingWater" Assets/_Game/Scripts
```

### Fase 1 — Contrato

```text
1. Identificar reward paths existentes.
2. Criar/consolidar RewardId e ledger.
3. Implementar skip de reward já aplicado.
4. Separar aplicação real de UI feedback.
```

### Fase 2 — Testes

```text
1. Testar double application.
2. Testar reload/ledger.
3. Testar failure path.
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
  - Reward idempotency touches quest state, save and integrations; parallel quest state or flag edits could break reward ledger semantics.

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
- [ ] T002 — Auditar reward application existente.
- [ ] T003 — Criar/consolidar RewardId e reward ledger.
- [ ] T004 — Implementar idempotency check.
- [ ] T005 — Cobrir QuestFlagGrant/Clear quando aplicável.
- [ ] T006 — Criar EditMode tests de double application.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar execution report.

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

Unity compile quando Unity C# mudar.

EditMode tests:

```text
QuestRewardIdempotencyTests.
```

---

## 27. Testing Quality Gate

- Changed deterministic logic: YES.
- Requires EditMode tests: YES.
- Requires PlayMode automated or final human scenario: NO unless UI/gameplay completion flow is changed.
- Requires regression test: YES.
- Human validation timing: NOT REQUIRED unless gameplay/UI flow is touched.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; reward idempotency tests PASS; execution report created; no reward duplicated after save/load.

---

## 28. Definition of Done

```text
Reward application is idempotent.
GrantedRewardIds or equivalent ledger persisted.
Duplicate trigger/reload does not duplicate reward.
Failure path explicit.
Tests added.
Execution report created.
No balancing or content expansion disguised as system work.
```

---

## 29. Anti-regressão

```text
Reward never applies twice after save/load.
QuestCompleted event is not enough to persist reward.
Hidden reward is not revealed by ledger/UI.
Economy is not bypassed without explicit reward rule.
No ACCEPTED without regression test.
```

---

## 30. Notas para execução posterior

This spec must run after QuestState save/load or in a serialized run that creates the needed reward ledger. It unlocks farm orders, festival quests, shop unlocks and main progression reward hooks.
