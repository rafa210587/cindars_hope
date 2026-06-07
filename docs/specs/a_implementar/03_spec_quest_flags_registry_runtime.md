# SPEC — Quest Flags Registry Runtime

> **Spec ID:** `03_spec_quest_flags_registry_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P1  
> **Type:** Runtime / Quest / Validation / Hardening  
> **Domain:** Quest / Flags / Registry / Cross-system Facts  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, QuestRewards, dialogue unlocks, shop unlocks, Fonte/MainProgression hooks ou save/load.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_flags_registry_runtime_execution_report.md`.  
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
- `main progression hooks`
- `FonteAnya hooks`
- `dialogue unlocks`
- `shop unlocks`
- `night shop/Nyx rumor hooks`
- `quest anti-softlock validation`
> **Scope:** criar ou consolidar registry de QuestFlags como fatos persistentes consultáveis, sem substituir QuestState.  
> **Out of scope:** main progression completa, FonteAnyaSection completa, dialogue system completo, quest authoring UI, social/pet/companion flags futuras detalhadas.

---

# /speckit.specify

## 1. Contexto

O direction de quests define QuestFlag como fato persistente consultável por sistemas e alerta que QuestFlag não deve substituir QuestState. Exemplos incluem FonteRespawnUnlocked, FragmentWaterProtected, CindarDiaryRead, ShopNightUnlocked e CaveGateMemoryOpened.

Esta spec deve seguir o modelo operacional atual: gerar implementação incremental/residual quando o repo já possui base parcial, nunca recriar sistemas transversais sem auditoria local, e nunca promover runtime/gameplay para `ACCEPTED` apenas por compile.

---

## 2. Problema

Sem registry de flags, sistemas diferentes podem criar strings soltas para fatos importantes, gerando typos, duplicação, softlock e save/load inconsistentes.

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

Criar um registry de QuestFlags estáveis, com ID, descrição, owner, spoiler tier, persistência e validação de referências, mantendo a separação QuestFlag vs QuestState.

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
QuestFlag is persistent fact consultable by systems.
QuestFlag examples: FonteRespawnUnlocked, FonteLivingWaterUnlocked, FonteRespecUnlocked, FragmentWaterProtected, FragmentMemoryProtected, FragmentLifeProtected, FragmentHopeProtected, CindarDiaryRead, VaelrionIntroduced, SethraKnown, NyxCultRumorKnown, BlackStoneObserved, TownKnowsAnyaRumor, ShopNightUnlocked, CaveGateMemoryOpened.
Anti-pattern: do not use QuestFlag as substitute for all quest states.
QuestFlags can be granted/cleared by rewards.
Flags may unlock dialogue/shop/NPC schedules/Fonte hooks.
```

### 5.2 Deferred / future from directions

```text
Full main progression flags implementation.
Full FonteAnya runtime.
Dialogue tree authoring.
Social/companion/pet flags.
Localization/debug UI final.
All lore flags content population.
```

### 5.3 Explicitly not redefined here

```text
QuestState formal state.
Reward idempotency implementation.
MainProgressionSection.
FonteAnyaSection.
Dialogue runtime.
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
- existing flag strings/enums/constants;
- dialogue flags;
- NPC flags;
- shop unlock flags;
- Fonte/main quest flags;
- save fields that currently store flags;
- validators for string IDs or registries.
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
criar ou consolidar registry de QuestFlags como fatos persistentes consultáveis, sem substituir QuestState.
```

---

## 9. Fora de escopo

Não inclui:

```text
main progression completa, FonteAnyaSection completa, dialogue system completo, quest authoring UI, social/pet/companion flags futuras detalhadas.
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

### 11.1 Registry

- QuestFlag registry exists or is consolidated.
- Flags have stable FlagId.
- Each flag has owner domain and spoiler tier/debug description.
- Duplicate FlagId is rejected/warned.
- Unknown flag reference is reported.

### 11.2 Separation

- QuestFlags are not used to replace QuestState.
- QuestState stores formal progress; flags store cross-system facts.
- MainProgression/FonteAnya flags are allowed as hooks but not replacement for their sections.

### 11.3 Save/load

- Persisted flags use stable IDs.
- Missing flag store defaults safely.
- Deprecated/unknown flags have fallback/diagnostic.

### 11.4 Validation

- Validator/test detects duplicate flag IDs.
- Validator/test detects unknown flag references in quest definitions/rewards if data exists.
- Test covers grant/clear idempotency or defers to reward spec with explicit note.

### 11.x Execution report obrigatório

Criar:

```text
docs/validation/03_spec_quest_flags_registry_runtime_execution_report.md
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
Assets/_Game/Scripts/Quests/Flags/QuestFlagDefinition.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagRegistry.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagStore.cs
Assets/_Game/Scripts/Save/QuestFlagsSaveData.cs
Assets/_Game/Scripts/Editor/Validation/ValidateQuestFlags.cs
Assets/_Game/Tests/EditMode/Quests/QuestFlagsRegistryTests.cs
docs/validation/03_spec_quest_flags_registry_runtime_execution_report.md
```

If there is already a flag/condition registry, consolidate rather than duplicate.

---

## 13. Contratos, dados e eventos

### 13.1 Data contracts

```text
QuestFlagDefinition:
  FlagId
  OwnerDomain
  DescriptionForDev
  SpoilerTier
  Persisted
  InitialValue
  AllowedGrantSources
  AllowedClearSources
  DeprecatedAliasIds
```

### 13.2 Runtime contracts

```text
QuestFlagRegistry resolves FlagId.
QuestFlagStore reads/writes flag state.
QuestState remains separate.
Flags are facts, not step progress containers.
```

### 13.3 Event contracts

```text
QuestFlagChanged event optional.
Flag changes can unlock dialogue/shop/schedules but should be explicit.
```

### 13.4 Save contracts

```text
Persisted flags save as stable IDs and bool/state.
Deprecated aliases require migration/fallback policy.
Unknown FlagId logs diagnostic.
```

### 13.5 UI contracts

```text
Player UI does not show raw flags unless projected safely.
Debug UI may show flags if marked debug.
```

---

## 14. Sistemas afetados

```text
Quest runtime;
Quest rewards;
Dialogue unlocks;
Shop unlocks;
Main/Fonte hooks;
Save/load;
Debug validation;
EditMode tests.
```

---

## 15. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**Quest*Flag*.cs
Assets/_Game/Scripts/Editor/Validation/**Quest**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_flags_registry_runtime_execution_report.md
```

---

## 16. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset, salvo flag data asset explicitamente listado
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
rg -n "QuestFlag|FlagId|DialogueFlag|NpcFlag|ShopNightUnlocked|FonteRespawnUnlocked|Fragment.*Protected|CindarDiaryRead" Assets/_Game/Scripts docs
```

### Fase 1 — Registry

```text
1. Consolidar flags existentes.
2. Criar registry se não existir.
3. Definir owner/spoiler tier.
4. Conectar save store se necessário.
```

### Fase 2 — Validation

```text
1. Detectar duplicatas.
2. Detectar referências desconhecidas.
3. Testar grant/clear básico.
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
  - Flags are cross-system facts and can collide with quest state, rewards, dialogue and main progression when edited in parallel.

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
- [ ] T002 — Auditar flags existentes.
- [ ] T003 — Criar/consolidar QuestFlagDefinition/Registry/Store.
- [ ] T004 — Definir save/default behavior.
- [ ] T005 — Criar validator/test para duplicatas e unknown refs.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar execution report.

---

## 26. Validações obrigatórias

Docs validation.
C# build if C# changed.
Unity compile if Unity C# changed.
EditMode tests for registry lookup, duplicate detection, grant/clear/default save.
No PlayMode required unless scene/UI is touched.

---

## 27. Testing Quality Gate

- Changed deterministic logic: YES.
- Requires EditMode tests: YES.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: YES if fixing broken/duplicated flag bug.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; registry tests PASS; execution report created; QuestFlag not used as QuestState replacement.

---

## 28. Definition of Done

```text
QuestFlag registry/store created or consolidated.
Flags have stable IDs.
Duplicate/unknown refs detected.
Save/default behavior documented.
QuestFlag vs QuestState separation enforced.
Tests/validator added.
Execution report created.
```

---

## 29. Anti-regressão

```text
QuestFlag does not replace QuestState.
Do not store complex progress in loose flags.
Do not reveal hidden flags to player UI.
Do not use raw strings without registry if registry exists.
No ACCEPTED without duplicate/unknown validation.
```

---

## 30. Notas para execução posterior

This spec supports dialogue/shop/main/Fonte hooks but should not implement those features fully. It creates the safe registry they will reference later.
