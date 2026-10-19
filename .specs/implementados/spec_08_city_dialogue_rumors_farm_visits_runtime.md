# SPEC — City Dialogue Rumors Farm Visits Runtime

> **Spec ID:** `08_spec_city_dialogue_rumors_farm_visits_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 08 — City / NPC / Dialogue / Services  
> **Priority:** P1  
> **Type:** Runtime / City / Dialogue / Rumors / Farm Visits  
> **Domain:** City / DialogueSet / Rumor / FarmVisitRules / Relationship Thresholds  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_08_CITY_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere full dialogue writing, romance/casamento, companion events, quest dialogue branching, farm visit scene/prefab wiring, NPC schedule runtime, pets ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/NPC/**`, `Assets/_Game/Scripts/Dialogue/**`, `Assets/_Game/Scripts/Social/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/City/**`, `docs/validation/08_spec_city_dialogue_rumors_farm_visits_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - dialogue UI;
  - quest dialogue branches;
  - farm visit events;
  - relationship progression;
  - companion/social events.
  - rumor/knowledge discovery.
> **Scope:** definir/endurecer contrato de DialogueSet, RumorEntry, FarmVisitRule e relationship thresholds sem escrever diálogos finais nem romance profundo.  
> **Out of scope:** texto final de diálogos, romance/casamento, explicit social content, companion AI/events, pet events, scene/prefab farm visit placement.

---

# /speckit.specify

## 1. Contexto

A cidade deve parecer viva mesmo sem quest: horários, rotinas, relações, serviços, rumores, festivais, visitas à fazenda e respostas ao progresso do jogador. O direction define gatilhos de visita à fazenda por reputação, relacionamento, quest, serviço, casamento, festival, horário, estação, lua, cave progress, companion event e farm level.

Esta spec cria contrato para diálogo/rumores/visitas. Não escreve o conteúdo final dos diálogos nem implementa romance/casamento profundo.

---

## 2. Problema

Sem contrato de diálogo/visitas:

```text
rumor pode revelar spoiler cedo;
NPC pode visitar fazenda sem relação/flag;
visita pode acontecer quando NPC está indisponível;
diálogo de Anya pode revelar demais;
romance pode entrar como conteúdo profundo fora do escopo;
pet event pode ser criado antes do sistema de pets;
farm visit pode ignorar schedule/festival/clima/lua;
rumor pode duplicar quest/progress flag.
```

---

## 3. Objetivo

Criar/endurecer:

```text
DialogueSetDefinition;
DialogueContext;
DialogueCondition;
RumorEntry;
RumorPool;
FarmVisitRule;
FarmVisitEventState;
RelationshipThreshold hooks;
ProgressReactiveDialogue;
Spoiler/Lore gate;
validators/tests.
```

---

## 4. Regras de design

```text
Cidade deve responder a dia, estação, lua e progresso.
Rumores servem para orientar, criar cor local e plantar pistas.
Anya é memória obscura na cidade; não revelar tudo cedo.
Farm visits dependem de relação/reputação/quest/serviço/casamento/festival/horário/estação/lua/cave progress/companion/farm level.
Romance não é conteúdo explícito; foco em vínculo/confiança/quests/rotina.
Pets permanecem deferidos.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ouvir rumores coerentes com progresso e horário.
Como NPC, quero dialogar conforme schedule, relação, lua, estação e quest.
Como farm, quero receber visitas com regras claras.
Como quest/lore, quero gate de spoiler.
Como validator, quero impedir diálogo sem condition/gate e visita inválida.
```

---

## 6. Escopo

Inclui:

```text
dialogue set contract;
dialogue condition contract;
rumor entry/pool;
farm visit rule;
relationship threshold triggers;
lore/spoiler gates;
save/load state for seen/visited if existing;
tests/validators.
```

Não inclui:

```text
final dialogue writing;
dialogue UI final;
romance/casamento implementation;
companion event full runtime;
pet event runtime;
farm visit scene/prefab placement.
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

- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
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

- Cidade precisa ter rumores, pequenas mudanças por dia/estação/lua, visitas à fazenda e respostas ao progresso do jogador.
- Cidadãos podem visitar a fazenda conforme reputação geral, relacionamento individual, quest ativa, serviço contratado, casamento, festival, horário, estação, lua ativa, progresso na caverna, companion event e nível da fazenda.
- Gatilhos sugeridos: relacionamento 25 carta/comentário/presente/visita curta; 50 visita social/serviço/pedido; 75 ajuda/evento de confiança; 100 vínculo máximo/casamento se elegível/habilidade/rotina especial.
- Anya na cidade é memória obscura; Fonte da fazenda é a única representação física ativa.
- Romance é sem conteúdo explícito e focado em vínculo, confiança, quests e rotina.
- Pets estão deferidos; referências de pet no roster/layout não devem criar runtime nesta spec.

### Deferred / future from directions

- Texto final de todos os diálogos.
- Dialogue UI final.
- Romance/casamento profundo.
- Companion event runtime completo.
- Pet events.
- Festival minigames.
- Farm visit prefab placement.

### Explicitly not redefined here

- NPCData contract.
- Schedule runtime.
- Quest objective runtime.
- Knowledge/bestiary discovery.
- Farm scene visit placement.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 DialogueSetDefinition

```text
DialogueSetId
NpcId
DefaultLines[]
ContextualLines[]
RumorPoolIds[]
QuestDialogueRefs[]
RelationshipDialogueRefs[]
FarmVisitDialogueRefs[]
LoreGateTags[]
DebugTags[]
```

### 7.2 DialogueContext

```text
NpcId
LocationId
SchedulePeriod
CurrentDay
Season
Weather
LunarState
FestivalId optional
RelationshipValue
CityReputation
ActiveQuestIds[]
CompletedQuestIds[]
StoryFlags[]
CaveProgress
FarmLevel
CompanionEventState optional
IsFarmVisit
IsShopInteraction
IsServiceInteraction
```

### 7.3 DialogueCondition

```text
ConditionId
RequiredRelationshipMin optional
RequiredReputationMin optional
RequiredQuestFlags[]
ForbiddenQuestFlags[]
RequiredStoryFlags[]
ForbiddenStoryFlags[]
RequiredSeason optional
RequiredWeather optional
RequiredLunarState optional
RequiredSchedulePeriod optional
RequiredLocationId optional
RequiredCaveProgress optional
RequiredFarmLevel optional
SpoilerLevel
Priority
RepeatPolicy
```

### 7.4 RumorEntry

```text
RumorId
TextKey
NpcIdsAllowed[]
LocationTags[]
RequiredConditions
SpoilerLevel
TruthLevel: True | HalfTrue | False | Misleading | Unknown
TopicTags[]
OnceOnly
CooldownDays optional
CanAdvanceKnowledge
CanStartQuest
```

### 7.5 FarmVisitRule

```text
FarmVisitRuleId
NpcId
RequiredRelationshipMin
RequiredCityReputation optional
RequiredQuestFlag optional
RequiredServiceContract optional
RequiredMarriageState optional
RequiredFestival optional
RequiredSchedulePeriod optional
RequiredSeason optional
RequiredLunarState optional
RequiredCaveProgress optional
RequiredFarmLevel optional
VisitType
CooldownDays
DialogueSetId
RewardOrGiftPolicy optional
```

---

## 8. Relationship threshold triggers

```text
NpcRelationship >= 25:
  letter, comment, simple gift or short visit.

NpcRelationship >= 50:
  social visit, special service, personal request or friendship event.

NpcRelationship >= 75:
  farm help, trust event, warning visit or advanced quest.

NpcRelationship >= 100:
  maximum bond, marriage if eligible, unique ability or special routine.
```

This spec only defines thresholds/events. It does not implement marriage/romance.

---

## 9. Rumor/spoiler rules

```text
Rumor must have topic and spoiler level.
Anya/Cindar/Bromecia/Elyndor/Pedra Negra rumors require story/cave/lore gates.
Half-true/misleading rumors are allowed but should not break objective clarity.
Rumor can start quest only through quest adapter.
Rumor can advance knowledge only through knowledge/bestiary/lore adapter.
```

---

## 10. Farm visit rules

```text
Farm visit must check NPC availability/schedule.
Farm visit must not spawn NPC if conflicting with quest/festival/story.
Farm visit can be social, service, gift, warning, quest, companion-related or spouse/future.
Farm visit does not require romance.
Marriage visit/move-in is future unless a dedicated social spec exists.
Pet-related visit remains future/deferred.
```

---

## 11. Save/load

Persist only mutable state:

```text
seen once-only rumors;
rumor cooldowns if needed;
farm visit cooldowns;
active/scheduled farm visit;
relationship-triggered event consumed flags;
dialogue seen flags if system requires.
```

Do not persist:

```text
static dialogue text;
NPC GameObject reference;
UI selected dialogue;
pet state;
romance deep state.
```

If no safe dialogue/farm visit save exists and state is needed, STOP for save/migration spec.

---

## 12. Criteria

```text
DialogueSet/DialogueContext/Condition contracts exist.
Rumor entries require spoiler gates and repeat policy.
FarmVisitRule checks relationship/reputation/quest/service/time/season/lunar/farm/cave gates.
Anya lore is gated.
Romance/casamento not implemented.
Pets not implemented.
Tests cover dialogue condition, rumor gating, once/cooldown, farm visit eligibility and Anya spoiler guard.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Dialogue/DialogueSetDefinition.cs
Assets/_Game/Scripts/Dialogue/DialogueContext.cs
Assets/_Game/Scripts/Dialogue/DialogueCondition.cs
Assets/_Game/Scripts/Dialogue/DialogueResolver.cs
Assets/_Game/Scripts/Dialogue/RumorEntry.cs
Assets/_Game/Scripts/City/FarmVisits/FarmVisitRule.cs
Assets/_Game/Scripts/City/FarmVisits/FarmVisitEligibilityResolver.cs
Assets/_Game/Tests/EditMode/City/DialogueRumorFarmVisitTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Dialogue/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/City/**
docs/validation/08_spec_city_dialogue_rumors_farm_visits_runtime_execution_report.md
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
1. Auditar dialogue/rumor/farm visit systems.
2. Consolidar DialogueSet/Context/Condition.
3. Consolidar RumorEntry/Pool.
4. Consolidar FarmVisitRule and eligibility.
5. Implementar validators for spoiler/pets/romance.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - NPCData contract;
  - schedule runtime;
  - quest objective runtime;
  - social/romance;
  - companion events;
  - pets.
- Reason: dialogue/visits consume NPC, schedule, quest, world and farm state.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if dialogue/visit flags exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding seen rumor/visit state; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. RumorSeenEvent, FarmVisitScheduledEvent, FarmVisitCompletedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes lines/context/availability for UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for dialogue/rumor/farm visit flow.
```

---

## 21. Riscos

```text
Risco: spoiler de Anya cedo.
Mitigação: spoiler/lore gates.

Risco: romance deep accidentally implemented.
Mitigação: threshold only, no romance content.

Risco: farm visit spawns NPC invalidly.
Mitigação: availability/schedule checks.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar dialogue/rumor/farm visit systems.
- [ ] T003 — Consolidar dialogue contracts.
- [ ] T004 — Consolidar rumor contracts.
- [ ] T005 — Consolidar farm visit rules.
- [ ] T006 — Implementar validators/tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a dialogue/rumors/farm visits foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de dialogue/rumors/farm visits? | Arquivos alterados e justificativa. | PARTIAL |
| Canon de cidade | Kanthor/Anya/classes funcionais/sem D&D/sem Fôlego foram preservados? | Checklist no report. | PARTIAL |
| Pets | A execução não criou runtime/UI/save/data asset de pets? | Checklist explícito no report. | BLOCKED se violar |
| Romance | A execução não criou romance/casamento profundo fora do escopo? | Checklist explícito no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/08_spec_city_dialogue_rumors_farm_visits_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "DialogueSet|DialogueContext|DialogueCondition|Rumor|FarmVisit|RelationshipThreshold|Spoiler|Anya|Romance|Pet" Assets/_Game/Scripts docs/design .specs
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a dialogue/rumors/farm visits existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, agenda, serviços, diálogo e UI projection permanecem consistentes
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

### Scenario 3 — Canon protection

```text
Given a spec toca NPC, serviço, religião, cidade, diálogo ou agenda
When validar os dados
Then templo público principal permanece Kanthor
And Anya não ganha templo/altar ativo independente
And NPCs usam classes funcionais do jogo, não classes de D&D
And Fôlego/Breath/BR não é criado como stat de NPC
And pets permanecem deferidos.
```

### Scenario 4 — Invalid state

```text
Given NPC, agenda, serviço, diálogo, farm visit, porta, cama, licença ou contrato inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save, não libera serviço indevido e não bloqueia progresso sem fallback.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Rumor reveals Anya/Cindar/Bromecia/Elyndor too early.
- Farm visit without schedule/availability check.
- Farm visit cooldown lost after reload.
- Dialogue starts quest without adapter.
- Romance/casamento deep content implemented accidentally.
- Pet event implemented accidentally.
- Seen-once rumor repeats incorrectly.
- Save schema altered without migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — City Dialogue Rumors Farm Visits Runtime

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

## Canon compliance
- Kanthor public temple preserved:
- Anya altar/temple not created:
- Functional classes used:
- Folego/Breath/BR not created:
- Pets not implemented:
- Deep romance/marriage not implemented:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Canon protection:
- Invalid state:
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
4. A implementação exigir reescrever City/NPC/Dialogue/Shop/Quest system canônico existente.
5. A implementação criar templo/altar ativo independente de Anya.
6. A implementação usar classe de D&D como classe mecânica de NPC.
7. A implementação recriar Fôlego/Breath/BR como stat/recurso/campo de NPC.
8. A implementação criar pets runtime/UI/save/data assets.
9. A implementação criar romance/casamento profundo ou conteúdo explícito.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Farm Visit Trigger Matrix

| Trigger | Expected use |
|---|---|
| City reputation | broad social trust |
| NPC relationship | personal visits |
| Active quest | quest-specific visit |
| Service contract | provider visit |
| Marriage | future/social spec |
| Festival | festival routing |
| Schedule period | time gate |
| Season | seasonal visit |
| Lunar state | Alihana/Nyx/Senya reactions |
| Cave progress | warning/lore visits |
| Companion event | companion spec hooks |
| Farm level | prosperity/service visit |

## 23H. Spoiler Levels

```text
0 = mundane rumor.
1 = soft hint.
2 = quest clue.
3 = lore-sensitive.
4 = major Anya/Cindar/Bromecia/Elyndor/Pedra Negra reveal.
5 = endgame reveal; blocked unless explicit.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, dialogue condition/rumor/farm visit eligibility logic is deterministic.
- Requires EditMode tests: YES for condition/rumor/spoiler/farm visit/cooldown tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for dialogue and farm visit visual flow.
- Requires regression test: YES if fixing existing dialogue/visit/spoiler bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver CityScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no spoiler leak; no pets; no deep romance.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/08_spec_city_dialogue_rumors_farm_visits_runtime_execution_report.md.
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
Não criar templo/altar ativo independente de Anya.
Não usar classes de D&D como classes mecânicas.
Não recriar Fôlego/Breath/BR em NPC.
Não implementar pets.
Não implementar romance/casamento profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
