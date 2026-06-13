# SPEC — Quest Objective Event Contract Runtime

> **Spec ID:** `09_spec_quest_objective_event_contract_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Data Contract / Quest / Objective / Event  
> **Domain:** Quest / Definition / Step / Objective / Event / Category / State Machine  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState save/load, condition/trigger resolver, reward application, quest log UI, farm orders adapter, main progression/Fonte hooks ou quest data assets finais.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_objective_event_contract_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - quest state save/load;
  - condition/trigger runtime;
  - reward idempotency;
  - quest log visibility/spoiler;
  - farm orders/festival/cave contracts/tutorial adapters.
> **Scope:** definir/endurecer contrato base de QuestDefinition, QuestState enum, QuestStep, Objective, QuestCategory e QuestEvent names sem implementar UI final nem main quest específica.  
> **Out of scope:** QuestState save migration, rewards implementation, quest log UI, main progression content, farm orders board, festival/social/pet content.

---

# /speckit.specify

## 1. Contexto

O direction de Quest/Objectives define que main quest, side quests, farm orders, social future, companion future, pet future, festival, cave contracts e tutorial usam o mesmo sistema base. O modelo comum é `Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags`.

Esta spec cria o contrato base. Ela não implementa cada adapter nem o conteúdo da main quest.

---

## 2. Problema

Sem contrato base:

```text
main quest cria flags próprias;
farm orders criam contadores próprios;
festival quests criam datas próprias;
cave contracts criam triggers próprios;
diálogo cria flags próprias;
Fonte cria unlocks próprios fora de contrato;
quest log mostra estado inconsistente;
QuestFlag vira substituto indevido de QuestState.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestDefinition;
QuestCategory;
QuestState enum;
QuestStep;
QuestObjective;
CompletionMode;
ObjectiveType;
QuestEventName;
VisibilityPolicy references;
RepeatPolicy references;
QuestDefinitionValidator;
No domain-specific hacks.
```

---

## 4. Regras de design

```text
QuestDefinition é dado autorado.
QuestState é estado persistido.
QuestFlag é fato persistente consultável, não substituto de QuestState.
Condition não avança quest sozinha.
Trigger/event avança quando condition permite.
Quest system consome eventos de gameplay; não substitui farm, combat, inventory, economy, bestiary ou Fonte.
MainProgression e FonteAnya ficam fora do QuestState genérico.
```

---

## 5. User stories / engineering stories

```text
Como designer, quero definir quests com steps, objectives, conditions, triggers, rewards e visibility.
Como runtime, quero avaliar QuestDefinition sem hardcode por feature.
Como farm/city/cave, quero adapters próprios usando contrato comum.
Como UI, quero ler representação segura sem conhecer lógica interna.
Como save/load, quero persistir QuestState referenciando QuestDefinition por QuestId.
```

---

## 6. Escopo

Inclui:

```text
QuestDefinition fields;
QuestCategory;
QuestState enum;
QuestStep;
QuestObjective;
CompletionMode;
ObjectiveType;
QuestEventName registry/enum/string-safe contract;
definition validation;
basic runtime data model.
```

Não inclui:

```text
QuestState save section;
condition/trigger evaluation engine;
reward application engine;
quest log UI;
farm orders adapter;
main quest concrete data;
festival/social/pet runtime.
```

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
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

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

- Todas as categorias usam o mesmo sistema base.
- Modelo comum: Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags.
- QuestDefinition contém QuestId, Category, DisplayName, HiddenName, Description, SpoilerTier, StartConditions, AutoStartTriggers, Steps, FailureRules, ExpiryRules, Rewards, QuestFlagsGranted, prerequisites, blocked quests, RepeatPolicy, Trackable, JournalVisibilityPolicy e DebugTags.
- QuestState enum canônico inclui Unknown, Discovered, Available, Active, Waiting, ReadyToComplete, Completed, Failed, Expired, HiddenCompleted e Blocked.
- Objective types abrangem talk, reach, interact, collect, deliver, farm, craft, shop, cave, Fonte, time/weather/lunar/festival, dialogue choice e final choice.
- Quest system consome eventos de gameplay e não vira fonte primária de farm/combat/inventory/economy.

### Deferred / future from directions

- Quest data assets finais.
- Quest Log UI.
- Reward application.
- Condition resolver.
- FarmOrder board.
- SocialFuture/CompanionFuture/PetFuture runtime.
- Main quest concrete implementation.

### Explicitly not redefined here

- Save/load section shape.
- EventBus implementation.
- QuestFlag registry.
- MainProgressionSection.
- FonteAnyaSection.
- Dialogue/farm/city/cave adapters.

## 7. Modelo de domínio

### 7.1 QuestCategory

```text
Main
Side
FarmOrder
SocialFuture
CompanionFuture
PetFuture
Festival
CaveContract
Tutorial
Hidden
System
```

Rules:

```text
SocialFuture/PetFuture are category reservations, not permission to implement those features now.
Main quest cannot expire by time.
FarmOrder and Festival can expire if deadline is clear.
Hidden is not shown in log until discovered.
```

### 7.2 QuestDefinition

```text
QuestId
Category
DisplayNameKey
HiddenDisplayNameKey
DescriptionKey
SpoilerTier
StartConditionIds[]
AutoStartTriggerIds[]
StepIds[]
FailureRuleIds[]
ExpiryRuleIds[]
RewardIds[]
QuestFlagGrantIds[]
PrerequisiteQuestIds[]
BlockedByQuestIds[]
RepeatPolicy
Trackable
JournalVisibilityPolicyId
DebugTags[]
```

### 7.3 QuestStep

```text
StepId
DisplayNameKey
DescriptionKnownKey
DescriptionHiddenKey
ObjectiveIds[]
CompletionMode
StartEventIds[]
CompleteEventIds[]
RewardIds[]
NextStepRules[]
BranchRules[]
VisibilityPolicyId
```

### 7.4 QuestObjective

```text
ObjectiveId
ObjectiveType
TargetId
RequiredAmount
Conditions[]
TriggerIds[]
VisibilityPolicyId
Optional
FailurePolicy
HintTextKey
MapMarkerPolicy
```

### 7.5 QuestState enum

```text
Unknown
Discovered
Available
Active
Waiting
ReadyToComplete
Completed
Failed
Expired
HiddenCompleted
Blocked
```

### 7.6 CompletionMode

```text
AllObjectivesRequired
AnyObjectiveRequired
ChoiceBranch
Timed
ManualEvent
ScriptedSequence
HiddenCondition
```

### 7.7 ObjectiveType

```text
TalkToNpc
ReachLocation
InteractWithObject
CollectItem
DeliverItem
UseItem
PlantCrop
WaterCrop
HarvestCrop
CraftItem
ProcessItem
BuyItem
SellItem
ShipItem
EarnGold
BuildOrUpgrade
FeedAnimalFuture
PetInteractionFuture
CompanionAssignedFuture
DefeatEnemy
DefeatEnemyFamily
DefeatBoss
SurviveCombat
DiscoverBestiaryKnowledge
DiscoverWeakness
ReachCaveDepth
CompleteCaveRun
RecoverCorpse
UnlockFonteFunction
UpgradeFonte
ProtectFragment
SealFragment
UseFragment
WaitForTime
WaitForDay
WaitForSeason
WaitForWeather
WaitForLunarEvent
AttendFestival
WinFestivalActivityFuture
ReadDocument
LearnSpell
EquipItem
RepairItem
UpgradeItem
MakeDialogueChoice
MakeFinalChoice
```

---

## 8. Quest event names

The contract must recognize at least:

```text
OnQuestAccepted
OnQuestStarted
OnQuestStepStarted
OnQuestStepCompleted
OnQuestCompleted
OnQuestFailed
OnQuestExpired
OnNpcDialogueStarted
OnNpcDialogueEnded
OnDialogueChoiceSelected
OnItemCollected
OnItemDelivered
OnItemUsed
OnItemCrafted
OnItemProcessed
OnItemSold
OnItemShipped
OnCropPlanted
OnCropWatered
OnCropHarvested
OnBuildingUpgraded
OnLocationReached
OnObjectInteracted
OnEnemyDefeated
OnEnemyFamilyDefeated
OnBossPhaseReached
OnCaveDepthReached
OnCaveRunCompleted
OnPlayerDeath
OnCorpseRecovered
OnFonteFunctionUnlocked
OnFonteUsed
OnFragmentProtected
OnFragmentSealed
OnFragmentUsed
OnBestiaryKnowledgeUnlocked
OnWeaknessDiscovered
OnSpellLearned
OnItemEquipped
OnDayStarted
OnTimeReached
OnSeasonStarted
OnWeatherChanged
OnLunarEventStarted
OnFestivalStarted
OnFestivalEnded
OnShopStockChanged
OnSocialStateChangedFuture
OnPetHintFuture
OnCompanionHintFuture
```

Future events may be declared but not implemented.

---

## 9. Validation rules

```text
QuestId stable and unique.
StepId unique within quest.
ObjectiveId unique within quest.
QuestDefinition must not contain runtime state.
QuestDefinition must not directly store Unity object references.
Every objective must have ObjectiveType and TargetId when needed.
QuestCategory Future values cannot trigger feature implementation.
Main quest cannot have expiry by calendar.
Hidden quest cannot have Trackable=true before discovery unless debug.
Critical quests require SoftlockPolicy reference.
SpoilerTier must be set for Main/Hidden/Lore-sensitive quests.
```

---

## 10. Criteria

```text
Quest contract exists or is hardened.
Categories, states, steps, objectives and completion modes match direction.
QuestEvent names are centralized or validated.
Definition and State are separated.
Future categories do not implement future systems.
Validators catch missing IDs, invalid categories, invalid hidden/main expiry and Unity references.
Tests cover core contracts and invalid definitions.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/QuestCategory.cs
Assets/_Game/Scripts/Quests/QuestState.cs
Assets/_Game/Scripts/Quests/QuestDefinition.cs
Assets/_Game/Scripts/Quests/QuestStepDefinition.cs
Assets/_Game/Scripts/Quests/QuestObjectiveDefinition.cs
Assets/_Game/Scripts/Quests/CompletionMode.cs
Assets/_Game/Scripts/Quests/ObjectiveType.cs
Assets/_Game/Scripts/Quests/QuestEventName.cs
Assets/_Game/Scripts/Quests/Validation/QuestDefinitionValidator.cs
Assets/_Game/Tests/EditMode/Quests/QuestDefinitionContractTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_objective_event_contract_runtime_execution_report.md
```

---

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia

```text
1. Auditar quest/event contracts existentes.
2. Consolidar enums/definitions.
3. Implementar validators.
4. Criar tests de contrato.
5. Criar report.
```

---

## 15. Paralelização

- Parallelizable: NO
- Must not run with:
  - quest state save/load;
  - condition/trigger resolver;
  - reward engine;
  - quest log UI;
  - main progression hooks.
- Reason: this is the base consumed by all quest specs.

---

## 16. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO by default; defines names/contracts.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: NO unless runtime subscribers are added.
```

---

## 18. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default.
```

---

## 19. Riscos

```text
Risco: enums break existing strings.
Mitigação: adapters/compatibility and audit.

Risco: too much implementation in contract spec.
Mitigação: no condition/reward/save engines here.

Risco: Future categories executed now.
Mitigação: validators block feature runtime.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest/event contracts.
- [ ] T003 — Consolidar QuestDefinition/Step/Objective/Category/State.
- [ ] T004 — Consolidar QuestEventName contract.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest objective event contract foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_objective_event_contract_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestDefinition|QuestCategory|QuestState|QuestStep|QuestObjective|ObjectiveType|CompletionMode|QuestEventName|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest objective event contract existe ou foi criado de forma mínima
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

- QuestDefinition stores runtime state.
- QuestState replaces MainProgression/FonteAnya.
- QuestFlag used as complex state.
- Main quest has expiry by calendar.
- Hidden quest visible before discovery.
- Future category creates Pet/Social runtime.
- Objective with missing target.
- Unity reference in quest definition.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Objective Event Contract Runtime

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


## 23G. Quest Contract Separation Matrix

| Concept | Static definition? | Persisted state? |
|---|---:|---:|
| QuestDefinition | YES | NO |
| QuestStepDefinition | YES | NO |
| QuestObjectiveDefinition | YES | NO |
| QuestState | NO | YES |
| QuestFlag | registry/static + save value | YES |
| MainProgressionSection | NO | YES, separate |
| FonteAnyaSection | NO | YES, separate |
| Quest Log UI | projection | NO source of truth |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, contract validation logic is deterministic.
- Requires EditMode tests: YES for valid/invalid quest definition/state/category/objective/event tests.
- Requires PlayMode automated or final human scenario: NO by default; contract/validation only.
- Requires regression test: YES if fixing existing quest contract bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Quest contract valid; no save schema change.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_objective_event_contract_runtime_execution_report.md.
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
