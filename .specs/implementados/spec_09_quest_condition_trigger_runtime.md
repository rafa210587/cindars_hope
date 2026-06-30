# SPEC — Quest Condition Trigger Runtime

> **Spec ID:** `09_spec_quest_condition_trigger_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Quest / Conditions / Triggers / Event Consumption  
> **Domain:** Quest / ConditionResolver / TriggerRouter / QuestEvent Consumption  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestDefinition contract, QuestState save/load, reward application, EventBus core, farm/city/cave systems, quest log UI ou main progression/Fonte hooks.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_condition_trigger_runtime_execution_report.md`  
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
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
> **Blocks:**  
  - objective progress runtime;
  - quest reward engine;
  - quest log updates;
  - farm orders/cave contracts adapters;
  - main/Fonte hooks.
> **Scope:** definir/endurecer ConditionResolver e TriggerRouter para quests, separando condition que habilita/bloqueia de trigger/event que avança progress.  
> **Out of scope:** EventBus rewrite, reward engine, quest content, UI, farm/city/cave system logic.

---

# /speckit.specify

## 1. Contexto

O direction é explícito: condition é estado que precisa ser verdadeiro; condition não avança quest sozinha. Trigger/event avança objective/step/quest quando condição permite. Quest system consome eventos de gameplay, mas não substitui farm, combat, inventory, economy, bestiary ou Fonte.

Esta spec cria o runtime de avaliação de conditions e roteamento de triggers.

---

## 2. Problema

Sem separação condition/trigger:

```text
uma condição true pode completar quest sozinha;
um evento pode avançar objective sem pré-condições;
eventos duplicados podem contar duas vezes;
quest pode depender de NPC em horário impossível;
tempo/lua/clima raro podem bloquear main quest sem fallback;
farm/cave/inventory podem ser alterados pelo quest system em vez de apenas observados;
trigger retroativo pode faltar.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestConditionDefinition;
QuestConditionType;
QuestTriggerDefinition;
QuestTriggerType;
ConditionResolver;
TriggerRouter;
ObjectiveProgressAdapter;
RetroactiveTriggerPolicy;
Event idempotency;
Context snapshots;
Debug failure reasons.
```

---

## 4. Regras de design

```text
Condition habilita/bloqueia.
Trigger/event avança quando conditions permitem.
Quest system consome eventos de gameplay.
Quest system não é fonte primária de farm/combat/inventory/economy/bestiary/Fonte.
Trigger duplicado não deve duplicar progresso.
Main quest não depende de spawn raro sem fallback.
Evento raro temporal/lunar deve ter pista e controle razoável quando crítico.
```

---

## 5. User stories / engineering stories

```text
Como objective, quero receber evento e testar conditions antes de progredir.
Como EventBus, quero publicar evento sem conhecer quest internals.
Como QuestLog, quero mostrar waiting reason quando conhecido.
Como anti-softlock, quero trigger retroativo para progresso feito antes da quest ativar.
Como debug, quero saber qual condition falhou.
```

---

## 6. Escopo

Inclui:

```text
condition definitions/types;
trigger definitions/types;
condition resolver;
trigger router;
event dedupe key;
objective progress adapter;
retroactive trigger policy;
failure/waiting reason;
tests.
```

Não inclui:

```text
reward application;
QuestState save migration;
UI quest log;
domain system implementation;
main quest data.
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
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md

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

- Condition é estado verdadeiro; trigger é evento que informa o quest system.
- Condition não avança quest sozinha; trigger/event avança quando condition permite.
- Conditions incluem Quest, Player, Inventory, World, Time, Weather, Lunar, NPC, Dialogue, Farm, Cave, Combat, Fonte, Bestiary e future Social/Pet/Companion.
- Eventos canônicos incluem dialogue, item, craft, ship, crop, building, location, enemy, cave, death, Fonte, fragments, bestiary, spell, time, weather, lunar, festival e shop stock.
- Quest não deve depender de NPC em horário impossível; se NPC fora de schedule, log pode mostrar disponibilidade se conhecida.
- Quest obrigatória com tempo/lua precisa dar pista e controle razoável; evento raro não pode ser requisito opaco de main quest.

### Deferred / future from directions

- Reward engine.
- Quest Log UI.
- Full adapter logic for all domains.
- Social/Pet/Companion runtime.
- EventBus rewrite.
- Main quest content.

### Explicitly not redefined here

- QuestDefinition contract.
- QuestState save/load.
- Domain event publishers.
- Inventory/farm/cave/city systems.
- World time/weather/lunar systems.

## 7. Modelo de domínio

### 7.1 QuestConditionType

```text
QuestCondition
PlayerCondition
InventoryCondition
WorldCondition
TimeCondition
WeatherCondition
LunarCondition
NpcCondition
DialogueCondition
FarmCondition
CaveCondition
CombatCondition
FonteCondition
BestiaryCondition
SocialConditionFuture
PetConditionFuture
CompanionConditionFuture
```

### 7.2 QuestConditionDefinition

```text
ConditionId
ConditionType
TargetId
Operator
ExpectedValue
Amount optional
VisibilityPolicyId
KnownFailureHintKey optional
IsCriticalPath
FallbackPolicyId optional
DebugTags[]
```

### 7.3 QuestTriggerType

```text
QuestLifecycle
Dialogue
Inventory
Crafting
Economy
Farm
City
Cave
Combat
Fonte
Bestiary
WorldTime
Festival
Shop
Debug
Future
```

### 7.4 QuestTriggerDefinition

```text
TriggerId
TriggerType
EventName
TargetId optional
AmountSource optional
AppliesToObjectiveIds[]
RequiredConditionIds[]
DeduplicationPolicy
RetroactivePolicy
DebugTags[]
```

### 7.5 QuestEventEnvelope

```text
EventId
EventName
SourceSystem
TargetId
Amount
ActorId optional
Day
Time
SceneId optional
RunSeed optional
PayloadIds[]
DeduplicationKey
```

### 7.6 ConditionEvaluationResult

```text
Success
FailedConditionIds[]
KnownFailureReasons[]
HiddenFailureReasons[]
CanShowInQuestLog
CanRetry
SuggestedFallbackId optional
```

---

## 8. Evaluation rules

```text
For event:
  1. Route by EventName.
  2. Find active/waiting/available quest objectives listening to event.
  3. Evaluate required conditions.
  4. If success, apply objective progress.
  5. If failure and known, update waiting/hint state if allowed.
  6. Deduplicate by event/objective policy.
```

Condition evaluation must be side-effect free.

---

## 9. Deduplication policy

```text
ByEventId:
  same event id cannot progress same objective twice.

ByDayTarget:
  e.g. talk to same NPC once per day.

ByRunSeedTarget:
  cave/depth events tied to run seed.

ByObjectiveCompletion:
  completed objective ignores repeat events.

ManualAllowRepeat:
  only for repeatable objectives with amount.
```

---

## 10. Retroactive policy

```text
Allowed:
  item already collected;
  location already reached;
  cave depth already reached;
  bestiary knowledge already discovered;
  crop already harvested today/season if tracked;
  quest flag already set.

Not allowed by default:
  dialogue choice not recorded;
  timed/festival attendance missed;
  unique irreversible event without state.
```

---

## 11. Future condition guardrails

```text
SocialConditionFuture:
  may exist as enum/contract but must not implement romance/social runtime.

PetConditionFuture:
  may exist as enum/contract but must not implement pets.

CompanionConditionFuture:
  may exist as enum/contract but must not implement companion full runtime outside companion specs.
```

---

## 12. Criteria

```text
Condition and trigger contracts exist.
Condition resolver is side-effect free.
Trigger router consumes gameplay events.
Duplicate event cannot double progress.
Retroactive triggers supported where safe.
Waiting/failure reasons can be reported without spoiler.
Future condition types do not implement future systems.
Tests cover condition success/failure, trigger progress, dedupe, retroactive and waiting state.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Conditions/QuestConditionType.cs
Assets/_Game/Scripts/Quests/Conditions/QuestConditionDefinition.cs
Assets/_Game/Scripts/Quests/Conditions/ConditionEvaluationResult.cs
Assets/_Game/Scripts/Quests/Conditions/QuestConditionResolver.cs
Assets/_Game/Scripts/Quests/Triggers/QuestTriggerType.cs
Assets/_Game/Scripts/Quests/Triggers/QuestTriggerDefinition.cs
Assets/_Game/Scripts/Quests/Triggers/QuestEventEnvelope.cs
Assets/_Game/Scripts/Quests/Triggers/QuestTriggerRouter.cs
Assets/_Game/Tests/EditMode/Quests/QuestConditionTriggerTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_condition_trigger_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/World/**
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
1. Auditar EventBus/Quest condition/trigger systems.
2. Consolidar condition and trigger definitions.
3. Implementar side-effect-free resolver.
4. Implementar trigger router and dedupe.
5. Implementar retroactive policy minimal.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - quest definition contract;
  - quest state save/load;
  - reward engine;
  - domain event publishers;
  - main progression hooks.
- Reason: trigger routing connects quest state with gameplay events.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless adding event dedupe persistent history; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds event subscribers: YES if router subscribes to GameEventBus.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes waiting/failure reason for UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED if routed into gameplay event flow.
```

---

## 21. Riscos

```text
Risco: condition creates side effect.
Mitigação: tests and contract.

Risco: duplicate event increments twice.
Mitigação: dedupe tests.

Risco: event subscriptions leak.
Mitigação: unsubscribe policy.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar condition/trigger/event systems.
- [ ] T003 — Consolidar definitions.
- [ ] T004 — Implementar resolver/router.
- [ ] T005 — Implementar dedupe/retroactive policy.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest condition trigger runtime foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec preserva Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags? | Contrato/referências no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica não fica impossível por item perdido, NPC, tempo, lua, morte ou save/load? | Plano/validator. | PARTIAL |
| Anti-spoiler | Quest log/visibility/rewards ocultos não revelam cedo Arquivista, final, 101, boss ou segredo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_condition_trigger_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestCondition|ConditionResolver|QuestTrigger|TriggerRouter|QuestEvent|OnItemCollected|OnCropHarvested|OnCaveDepthReached|Retroactive|Deduplication" Assets/_Game/Scripts docs/design .specs
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a quest condition trigger runtime existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, quest state, quest flags, main progression, FonteAnya e UI projection permanecem consistentes
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

### Scenario 3 — Critical quest anti-softlock

```text
Given uma quest crítica depende de item, NPC, clima, lua, caverna, morte, reward ou save/load
When o jogador perde item, recarrega, morre, muda horário ou completa step antes da quest iniciar
Then existe fallback, trigger retroativo, item protegido, NPC alternativo, normalização no load ou bloqueio explícito
And a main quest não fica impossível.
```

### Scenario 4 — Idempotency and replay safety

```text
Given objective, trigger, reward, flag ou branch já foi aplicado
When o mesmo evento chega de novo após reload, scene reload, event bus duplicate ou reentrada de diálogo
Then estado/recompensa/flag não é aplicado duas vezes
And ChoiceHistory/GrantedRewardIds/GrantedFlagIds preservam a decisão.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, notificação, diálogo, entrega, mapa, farm order ou evento de main quest
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Condition advances quest by itself.
- Trigger ignores failed conditions.
- Duplicate event increments objective twice.
- Quest depends on impossible NPC schedule.
- Rare lunar/weather event blocks main quest opaquely.
- Quest system mutates farm/inventory/cave directly.
- Future Pet/Social condition implements runtime.
- Event subscription leak.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Condition Trigger Runtime

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
- Definition/State/Step/Objective/Condition/Trigger/Reward/Flag separation:
- QuestState vs QuestFlag separation:
- QuestState vs MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-softlock:
- Idempotency/replay safety:
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
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Condition vs Trigger Matrix

| Concept | Responsibility |
|---|---|
| Condition | check if state allows progress |
| Trigger | event that attempts progress |
| QuestEventEnvelope | normalized event payload |
| ObjectiveProgressAdapter | increments objective state |
| Reward engine | applies reward later |
| Domain systems | own actual gameplay state |

## 23H. Side-effect rule

```text
QuestConditionResolver must not:
  add/remove inventory;
  change crop state;
  defeat enemy;
  alter Fonte state;
  pay reward;
  change shop stock.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Quest Log, diálogo, entrega, reward, objective progress ou evento de main quest, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, condition/trigger/dedupe/retroactive logic is deterministic.
- Requires EditMode tests: YES for condition success/failure/trigger progress/dedupe/retroactive/waiting tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if integrated with live gameplay event flow.
- Requires regression test: YES if fixing existing quest trigger/dedupe bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no side-effect conditions; no duplicate progress.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_condition_trigger_runtime_execution_report.md.
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
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
