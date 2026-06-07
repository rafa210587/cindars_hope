# SPEC — Quest Flags Registry Runtime

> **Spec ID:** `03_spec_quest_flags_registry_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
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
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_flags_registry_runtime_execution_report.md`. | PARTIAL |

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
# Execution Report — Quest Flags Registry Runtime

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
