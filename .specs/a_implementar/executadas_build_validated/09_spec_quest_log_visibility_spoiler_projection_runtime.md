# SPEC — Quest Log Visibility Spoiler Projection Runtime

> **Spec ID:** `09_spec_quest_log_visibility_spoiler_projection_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / UI Projection / Quest Log / Visibility / Spoiler  
> **Domain:** Quest / Quest Log / VisibilityPolicy / Projection / Anti-spoiler  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_ADAPTERS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState save/load, condition/trigger runtime, reward application, UI prefabs/layout final, main progression/Fonte concrete state ou quest content.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_log_visibility_spoiler_projection_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
> **Blocks:**  
  - Quest Log UI screen;
  - quest notifications;
  - map markers;
  - main quest projections;
  - calendar/weather/lunar projections.
> **Scope:** definir/endurecer projection runtime do Quest Log com VisibilityPolicy, SpoilerTier, known objectives/hints/rewards e estados Waiting sem implementar prefab/layout final.  
> **Out of scope:** UI prefab final, text final/localization, map marker implementation, quest content, main progression concrete data.

---

# /speckit.specify

## 1. Contexto

Quest Log é apresentação, não fonte de verdade. O direction define que UI deve mostrar categoria, título conhecido, resumo conhecido, objetivo atual, progresso, pista, NPC/local, prazo, condição temporal descoberta, recompensa conhecida, estado e track/untrack. A UI não deve mostrar QuestState interno cru, flags ocultas, spoiler tier acima do permitido, objetivos ocultos ou recompensa secreta.

Esta spec cria apenas a projeção segura de dados para UI/Quest Log.

---

## 2. Problema

Sem projection/visibility:

```text
UI pode mostrar step futuro;
quest hidden pode aparecer cedo;
recompensa secreta pode vazar;
nível 101 pode aparecer no Ato 1;
condição lunar secreta pode ser exibida;
QuestFlag oculto pode aparecer cru;
main quest pode revelar que Anya não volta cedo demais;
UI pode virar fonte de estado.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestVisibilityPolicy;
QuestLogProjectionService;
QuestLogEntryViewModel;
QuestObjectiveProjection;
QuestRewardProjection;
Known/hidden hint filtering;
WaitingReason projection;
Track/untrack projection;
Anti-spoiler validator.
```

---

## 4. Regras de design

```text
Quest Log é apresentação, não fonte de verdade.
Quest Log mostra apenas conhecimento autorizado.
Hidden quest não aparece até descoberta.
Main quest não revela boss/final/nível 101 cedo.
Reward secreto permanece oculto.
Condition temporal secreta só aparece quando descoberta.
VisibilityPolicy existe para quest, step, objective, reward e hint.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ver próximo objetivo conhecido sem spoiler.
Como UI, quero ViewModel pronto sem acessar QuestState cru.
Como main quest, quero mostrar ato/fragmento/fonte apenas quando descobertos.
Como hidden quest, quero permanecer invisível até pista.
Como validator, quero bloquear projection que vaza spoiler.
```

---

## 6. Escopo

Inclui:

```text
visibility policy contract;
quest log projection service;
quest entry/category/detail view models;
known objective/hint/reward filtering;
waiting reason projection;
track/untrack safe command contract;
anti-spoiler tests/validators.
```

Não inclui:

```text
prefabs/UI layout final;
localization final;
map marker rendering;
quest content data;
main progression concrete implementation.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Quest Log é apresentação, não fonte de verdade.
- UI deve mostrar categoria, título conhecido, resumo conhecido, objetivo atual, progresso, pista, NPC/local, prazo, condição temporal descoberta, recompensa conhecida, estado e track/untrack.
- UI não deve mostrar QuestState interno cru, flags ocultas, spoiler tier acima do permitido, objetivos ocultos ou recompensa secreta.
- Quest detail drawer deve respeitar VisibilityPolicy.
- Quest Log categorias incluem Main Quest, Side Quest, Social Quest, Farm Order/Encomenda, Companion Quest, Pet/Farm Hint, Completed e Failed/Expired.
- Main quest deve mostrar ato atual, fragmento se descoberto, estado da Fonte se conhecido, objetivo cidade/fazenda/caverna, condição de lua/clima apenas se descoberta, sem spoiler de nível 101 cedo e sem revelar Anya completa.

### Deferred / future from directions

- Prefab/layout final do Quest Log.
- Textos finais/localization.
- Map marker rendering.
- Quest notification UI final.
- Social/Pet category runtime.
- Main progression concrete state.

### Explicitly not redefined here

- QuestState save/load.
- QuestDefinition contract.
- MainProgressionSection.
- FonteAnyaSection.
- UI input/focus runtime.
- Map/minimap systems.

## 7. Modelo de domínio

### 7.1 QuestVisibilityPolicy

```text
VisibilityPolicyId
QuestVisibilityState: Hidden | Discovered | Known | FullyKnown | DebugOnly
AllowedSpoilerTier
ShowTitleWhenUnknown
ShowDescriptionWhenUnknown
ShowFutureSteps
ShowHiddenRewards
ShowSecretTriggers
ShowUnknownTemporalConditions
ShowMapMarker
RequiresKnownObjectiveIds[]
RequiresKnownHintIds[]
RequiresQuestFlags[]
ForbiddenQuestFlags[]
```

### 7.2 QuestLogEntryViewModel

```text
QuestId
Category
DisplayTitle
DisplaySummary
StateDisplay
Priority
Tracked
CanTrack
KnownCurrentObjective
ProgressText optional
KnownNpcId optional
KnownLocationId optional
KnownDeadline optional
KnownTemporalCondition optional
KnownRewardSummary
HasHiddenRewards
SpoilerSafe
SortKey
```

### 7.3 QuestDetailViewModel

```text
QuestId
DisplayTitle
Category
KnownSummary
CurrentObjectiveRows[]
KnownHints[]
KnownNpc
KnownLocation
Deadline
WaitingReason
KnownRewards[]
HiddenRewardPlaceholder
CompletedStepHistory[]
CanTrack
CanUntrack
DebugWarnings[]
```

### 7.4 WaitingReason

```text
WaitingForTime
WaitingForWeather
WaitingForLunarEvent
WaitingForNPC
WaitingForItem
WaitingForCaveDepth
WaitingForCraftingOrProcessing
WaitingForFestival
WaitingForStoryFlag
HiddenWaitingReason
```

---

## 8. Projection rules

```text
Projection reads QuestState + QuestDefinition + VisibilityPolicy.
Projection never mutates QuestState.
Projection filters objective/reward/hint by KnownObjectiveIds, KnownHints and VisibilityPolicy.
Projection never exposes raw QuestFlag IDs unless authorized.
Projection converts internal state to player-facing state strings.
Projection can expose “desconhecido” placeholders.
Projection must not leak hidden future branch.
```

---

## 9. Main quest anti-spoiler rules

Block before discovery:

```text
Arquivista do Silêncio;
nível 101;
final choices Proteger/Selar/Usar;
natureza completa da Pedra Negra;
Anya não será restaurada;
Vaelrion boss/final corruption;
Sethra leader reveal;
fragmentos futuros;
boss hidden weaknesses.
```

Allowed early:

```text
Fonte antiga;
oração/litania esquecida;
pequenos esquecimentos;
primeira Pedra Negra como mistério;
pista de água/memória;
NPC/local conhecido;
próximo objetivo conhecido.
```

---

## 10. Quest categories display

```text
Main Quest:
  active main only or discovered main chain.

Side Quest:
  optional NPC/local quests.

Farm Order / Encomenda:
  timed/repeatable delivery orders.

Cave Contract:
  may appear under Side/Contract tab if UI supports; otherwise Side Quest with contract tag.

Festival:
  appears while known/active/expired.

Social/Companion/Pet:
  categories may exist in UI taxonomy but no runtime unless future systems exist.
```

---

## 11. Track/untrack rules

```text
CanTrack=false for Unknown/Hidden undiscovered quests.
CanTrack=false for completed/failed/expired unless history screen supports.
Tracking hidden quest must be normalized false.
Track/untrack does not change quest progress.
Track/untrack persists via QuestState Tracked if save system exists.
```

---

## 12. Criteria

```text
Quest Log projection service exists or is hardened.
VisibilityPolicy filters quest, step, objective, reward and hint.
Hidden undiscovered quests do not appear.
Main quest spoiler guard exists.
Quest Log projection never mutates QuestState.
Track/untrack safe command exists or is deferred with explicit contract.
Tests cover hidden quest, known objective, secret reward, main spoilers, waiting reason and track/untrack.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Visibility/QuestVisibilityPolicy.cs
Assets/_Game/Scripts/Quests/Visibility/QuestLogEntryViewModel.cs
Assets/_Game/Scripts/Quests/Visibility/QuestDetailViewModel.cs
Assets/_Game/Scripts/Quests/Visibility/QuestObjectiveProjection.cs
Assets/_Game/Scripts/Quests/Visibility/QuestLogProjectionService.cs
Assets/_Game/Scripts/Quests/Visibility/QuestSpoilerValidator.cs
Assets/_Game/Tests/EditMode/Quests/QuestLogVisibilityProjectionTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_log_visibility_spoiler_projection_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar quest log/UI projection systems.
2. Consolidar visibility policy and view models.
3. Implementar/harden projection service.
4. Implementar anti-spoiler validator.
5. Criar tests.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - QuestState save/load;
  - QuestDefinition contract;
  - UI prefab/layout implementation;
  - main progression hooks.
- Reason: projection consumes quest state and UI contracts.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless Track state absent and added; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO unless reactive UI subscriptions are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI data layer: YES.
Changes prefab/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for Quest Log visual flow.
```

---

## 21. Riscos

```text
Risco: spoiler leak.
Mitigação: tests and validator.

Risco: projection mutates state.
Mitigação: read-only service tests.

Risco: category for future feature implies implementation.
Mitigação: UI taxonomy only; no Pet/Social runtime.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest log/projection systems.
- [ ] T003 — Consolidar visibility policy.
- [ ] T004 — Consolidar view models.
- [ ] T005 — Implementar projection/spoiler validator.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest log visibility/spoiler projection foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec usa os contratos de QuestDefinition, QuestState, Condition, Trigger, Reward e QuestFlag existentes? | Referência explícita no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica, order ou contrato não cria bloqueio irreversível sem fallback? | Plano/validator. | PARTIAL |
| Anti-spoiler | UI/log/flags/adapters não revelam Arquivista, 101, finais ou Anya cedo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Economia | FarmOrder/Contract/Festival não gera reward infinito nem buy/sell exploit? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_log_visibility_spoiler_projection_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestLog|QuestVisibility|VisibilityPolicy|QuestDetail|QuestProjection|SpoilerTier|KnownObjective|KnownHint|WaitingReason|Track" Assets/_Game/Scripts docs/design .specs
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a quest log visibility/spoiler projection existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And QuestState, QuestFlag, rewards, UI projection e adapters permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler / visibility

```text
Given quest, flag, order, festival, cave contract ou adapter contém informação oculta
When a UI/log/projection/validator consulta o estado
Then só conhecimento autorizado aparece
And passos futuros, rewards secretos, boss oculto, nível 101, finais e segredos de Anya permanecem ocultos até descoberta.
```

### Scenario 4 — Idempotency and expiry

```text
Given reward, flag, order, festival ou contract já foi aplicado/completado/expirado
When reload, day transition, event repeat ou reentrada ocorre
Then reward/flag não duplica
And expired/completed state não retorna ativo sem repeat policy explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, board, festival, cave contract, notification ou delivery UI
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Quest Log reveals hidden future step.
- Secret reward shown before discovery.
- Level 101 or final choices shown too early.
- Raw QuestFlag displayed.
- Projection mutates QuestState.
- Hidden quest trackable before discovery.
- Future Pet/Social category creates runtime.
- UI prefab edit required.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Log Visibility Spoiler Projection Runtime

## Summary
- Spec:
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

## Canon / quest compliance
- Quest core contracts reused:
- QuestState vs QuestFlag separation:
- MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:
- No reward/economy exploit:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler/visibility:
- Idempotency/expiry:
- Save/load safety:
- Visual/final scenario:

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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação usar QuestFlag como substituto para estado complexo de quest.
7. A implementação permitir reward duplicado após reload/evento repetido.
8. A implementação revelar cedo Arquivista do Silêncio, final choices, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion full runtime fora do escopo.
11. A implementação gerar reward infinito por FarmOrder/Festival/CaveContract.
12. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Quest Log Projection Matrix

| Source field | UI projection rule |
|---|---|
| QuestState.State | map to safe display state |
| KnownObjectiveIds | show only known objectives |
| KnownHints | show only known hints |
| Rewards | show only known reward or placeholder |
| QuestFlags | never raw unless authorized |
| SpoilerTier | cap by VisibilityPolicy |
| Waiting reason | show known, hide secret |
| Track state | allowed only for visible quests |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de Quest Log, board, Festival, FarmOrder, CaveContract ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, visibility/filter/projection logic is deterministic.
- Requires EditMode tests: YES for hidden/known/spoiler/reward/waiting/track tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Quest Log screen visual flow.
- Requires regression test: YES if fixing existing quest log spoiler/projection bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no spoiler leak; projection read-only.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_log_visibility_spoiler_projection_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não misturar QuestState com MainProgression/FonteAnya.
Não usar QuestFlag como substituto de QuestState.
Não aplicar reward/flag duas vezes.
Não revelar spoiler oculto cedo.
Não fazer main quest expirar por tempo.
Não implementar PetFuture/SocialFuture/romance profundo.
Não criar exploit de reward por order/festival/contract.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
