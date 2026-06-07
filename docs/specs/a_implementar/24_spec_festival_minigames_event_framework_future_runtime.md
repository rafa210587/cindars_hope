# SPEC — Festival Minigames Event Framework Future Runtime

> **Spec ID:** `24_spec_festival_minigames_event_framework_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 24 — Remaining Future Closure  
> **Priority:** P2  
> **Type:** Runtime / Future / Closure  
> **Domain:** Festival Minigames / Calendar Events / City Farm Social Hooks / Rewards  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_24_REMAINING_FUTURE_CLOSURE  
> **Can run with:** cave weather/lunar modifiers if no same event files.  
> **Must not run with:** qualquer spec que altere concrete festival content, NPC schedules, quest content, economy final, UI prefabs, scenes or social romance runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/World/Festivals/**`, `Assets/_Game/Scripts/UI/Festivals/**`, `Assets/_Game/Tests/EditMode/World/**`, `docs/validation/24_spec_festival_minigames_event_framework_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - festival content authoring;
  - city schedule festival overrides;
  - social/festival dialogue hooks;
  - calendar UI.
> **Scope:** fechar framework futuro de festivals/minigames: event definitions, eligibility, opt-in, scoring, rewards, calendar visibility, NPC/farm/city hooks e anti-softlock.  
> **Out of scope:** conteúdo final de cada festival, minigame visual/prefab, NPC schedules concretos, romance/social deep events, final economy balance.

---

# /speckit.specify

## 1. Contexto

O roadmap lista festival minigames como futuro explícito. World/Time define festivais como parte do calendário, com eventos de colheita, luz, juramento, memória e proteção, além de efeitos sazonais e eventos conhecidos no calendário público. O design deve enriquecer o mundo sem bloquear progresso essencial.

---

## 2. Problema

Sem framework:

```text
festival vira quest obrigatória escondida;
minigame dá reward forte demais;
evento secreto aparece no calendário cedo;
lojas/NPCs fecham sem aviso;
jogador perde item/progresso por não participar;
pontuação é opaca;
romance/festival vira obrigatório;
reload duplica reward de festival.
```

---

## 3. Objetivo

Criar:

```text
FestivalDefinition;
FestivalVisibilityState;
FestivalParticipationPolicy;
FestivalMinigameDefinition;
FestivalScoringPolicy;
FestivalRewardPolicy;
FestivalScheduleOverride;
FestivalAntiSoftlockPolicy;
FestivalResultRecord.
```

---

## 4. Regras de design

```text
Festivals enriquecem rotina, social, cidade, fazenda e lore.
Minigames são opcionais por padrão.
Rewards não são obrigatórios para main quest.
Eventos secretos só aparecem após descoberta.
Calendário mostra festival conhecido com antecedência.
Lojas/rotas impactadas precisam aviso.
Reload não duplica reward.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero saber quando há festival conhecido.
Como cidade, quero schedules/lojas alterados de forma previsível.
Como minigame, quero scoring transparente e opcional.
Como economy, quero reward útil sem quebrar progressão.
Como quest/lore, quero evento secreto com discovery gate.
```

---

## 6. Escopo

Inclui:

```text
festival definition;
visibility;
participation;
minigame definition;
scoring;
reward;
schedule override;
anti-softlock;
result record;
tests.
```

Não inclui:

```text
minigame UI/prefab;
festival prose;
NPC concrete schedule authoring;
social romance scenes;
scene assets;
final rewards.
```

## 7. Modelo de domínio

### 7.1 FestivalDefinition

```text
FestivalId
NameTextKey
Season
Day
StartTime
EndTime
LocationId
FestivalType: Harvest | Light | Oath | Memory | Protection | Lunar | Market | Fishing | Mining | CombatExhibition | Secret
VisibilityState
MinigameIds[]
RewardPolicyId
ScheduleOverrideId
SpoilerTier
```

### 7.2 FestivalVisibilityState

```text
Hidden
Rumored
KnownPublic
DiscoveredSecret
Active
CompletedThisYear
Missed
```

### 7.3 FestivalParticipationPolicy

```text
Optional
RequiredByQuestExplicit
RequiresInvite
RequiresTicket
RequiresRelationshipGate
AllowsLateJoin
AllowsSkip
SkipHasNoCoreSoftlock true
```

### 7.4 FestivalMinigameDefinition

```text
MinigameId
FestivalId
MinigameType: Timing | Fishing | Cooking | HarvestTurnIn | MiningScore | CombatArenaSafe | MemoryPuzzle | MarketHaggle | SocialQuiz
InputMode
Duration
DifficultyTier
PracticeAllowed
RetryPolicy
AccessibilityFallback
```

### 7.5 FestivalScoringPolicy

```text
ScoreInputs[]
ScoreCaps
FailureAllowed
PartialRewardAllowed
TiePolicy
NoRandomUnfairPenalty
CanShowScoreBreakdown
```

### 7.6 FestivalRewardPolicy

```text
RewardId
RewardType: Cosmetic | TitleFuture | RecipeHint | SmallCurrency | RelationshipSmall | ShopDiscountLimited | FestivalToken | LoreNote
UniquePerYear
IdempotencyKey
NoMandatoryPower true
NoEconomyLoop true
```

### 7.7 FestivalScheduleOverride

```text
OverrideId
AffectedNpcGroups[]
AffectedShopIds[]
ClosedOrShortHours
AlternativeLocations[]
PublicWarningRequired
SecretNoSpoilerWarning
```

---

## 8. Festival rules

```text
Known festivals:
  visible on calendar/public board.

Secret festivals:
  hidden until discovered, then safe hint.

Minigame:
  optional unless explicit quest requires.
  failure gives no hard softlock.
  scoring should be explainable.

Rewards:
  once per event/year or explicit repeat cap.
  no mandatory main quest power.
  no economy loop.
```

---

## 9. Criteria

```text
Festival framework contracts exist.
Visibility/participation/scoring/reward/schedule policies exist.
Secret events gated.
Minigames optional by default.
Rewards idempotent.
Tests cover known calendar, hidden secret, shop warning, optional skip, score cap, partial reward, reward once, yearly reset and no core softlock.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/World/Festivals/FestivalDefinition.cs
Assets/_Game/Scripts/World/Festivals/FestivalVisibilityState.cs
Assets/_Game/Scripts/World/Festivals/FestivalParticipationPolicy.cs
Assets/_Game/Scripts/World/Festivals/FestivalMinigameDefinition.cs
Assets/_Game/Scripts/World/Festivals/FestivalScoringPolicy.cs
Assets/_Game/Scripts/World/Festivals/FestivalRewardPolicy.cs
Assets/_Game/Scripts/World/Festivals/FestivalScheduleOverride.cs
Assets/_Game/Scripts/World/Festivals/FestivalAntiSoftlockPolicy.cs
Assets/_Game/Scripts/World/Festivals/FestivalResultRecord.cs
Assets/_Game/Tests/EditMode/World/FestivalMinigamesEventFrameworkTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/World/Festivals/**
Assets/_Game/Scripts/UI/Festivals/**
Assets/_Game/Scripts/City/Schedule/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/World/**
docs/validation/24_spec_festival_minigames_event_framework_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar festival/calendar/event/minigame systems.
- [ ] T003 — Consolidar festival framework contracts.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela fecha item explicitamente listado no roadmap macro como futuro.
Ela não altera código Unity durante geração da spec.
Ela não substitui specs núcleo já geradas.
Ela não altera Assets/, Packages/, ProjectSettings/, scenes, prefabs, tilemaps, ScriptableObject assets, audio, sprites, localization files ou save schema.
Se a execução futura exigir sistema fundacional inexistente, deve parar como BLOCKED ou DEFERRED, não improvisar arquitetura paralela.
Quando houver conflito entre directions, o domínio especializado vence para regra mecânica; UI_UX vence para apresentação/foco; Save/Load vence para persistência.
```

---

## Direction / Refinement Coverage

### Covered from directions

- World/Time define festivais como parte do calendário e impactos globais.
- Calendário público deve registrar festivais e eventos conhecidos.
- Eventos secretos não aparecem até serem descobertos.
- Outono pode ter festival de colheita.
- Inverno pode ter festivais de luz, juramento, memória e proteção.
- Eventos raros devem enriquecer o mundo, não bloquear progresso essencial sem aviso.

### Deferred / future from directions

- Minigame visual/prefab.
- Concrete festival writing.
- NPC concrete schedules.
- Social/romance festival scenes.
- Final reward balance.
- Localization.

### Explicitly not redefined here

- Calendar core.
- City NPC schedules.
- Quest content.
- Economy formula.
- Social/romance runtime.
- UI layout.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu todos os directions listados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a festival minigames/event framework foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Scope futuro | A spec foi autorizada para execução agora ou ainda fica future/deferred? | Decisão registrada no report. | BLOCKED/DEFERRED |
| Guardrails | A execução preserva limites de gameplay, economia, UI, save e spoilers? | Checklist/tests. | PARTIAL |
| Anti-exploit | Não gera loot, gold, power, party size, progresso ou automação infinita. | Tests/checklist. | PARTIAL |
| Anti-spoiler | Não revela Anya, Fonte, Level 101, final, boss, secret events ou quest flags cedo. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/24_spec_festival_minigames_event_framework_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Festival|Minigame|FestivalReward|FestivalSchedule|FestivalScoring|HarvestFestival|MemoryFestival|ProtectionFestival|SecretFestival" Assets/_Game/Scripts docs/design docs/specs
rg -n "Companion|Party|Tactical|AI|Squad|Festival|Minigame|Weather|Lunar|CaveModifier|Alihana|Senya|Nyx|Storm|Fog|Snow|Cold|Heat|Boss|Pet|Romance|FinalChoice|Level101" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given os sistemas fundacionais necessários existem
When o fluxo de festival minigames/event framework roda
Then ele aplica somente efeitos declarados
And usa IDs estáveis e estado persistível seguro
And não duplica progresso, reward, economy, party power ou spoiler.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Missing dependency

```text
Given dependência fundacional não existe ou não está ACCEPTED
When a execução tenta criar runtime avançado
Then deve parar como BLOCKED/DEFERRED
And não criar arquitetura paralela nem fallback frágil.
```

### Scenario 4 — Anti-exploit / anti-spoiler

```text
Given o jogador tenta forçar reload, repetir evento, acumular bônus, explorar economia ou antecipar informação secreta
When o sistema processa a ação
Then ele mantém idempotência, caps e spoiler gates
And registra risco residual se algum teste não for praticável.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Festival blocks main quest silently.
- Secret event visible early.
- Shop closes without warning.
- Minigame required for mandatory power.
- Reward duplicates by reload.
- Score opaque/unfair.
- Skipping festival softlocks player.
- Festival token creates economy loop.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Festival Minigames Event Framework Future Runtime

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

## Future-scope compliance
- Execution authorized now:
- Foundation dependencies accepted:
- No duplicate architecture:
- Anti-exploit:
- Anti-spoiler:
- Save/load safe:
- UI not source of truth:
- No scene/prefab/assets:
- No pet/social/romance/companion bleed outside scope:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER / BLOCKED
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Missing dependency:
- Anti-exploit/spoiler:
- Save/load:
- UI/final scenario:

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
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio/localization changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação reimplementar sistema já coberto em spec núcleo ou batch futuro anterior.
5. A implementação criar power creep, economia infinita, loot infinito, party size indevido, minigame obrigatório ou efeito secreto sem spoiler gate.
6. A implementação tornar companion, festival, weather/lunar, pet, romance, final choice ou Level101 obrigatório fora da regra canônica.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Festival|Minigame|FestivalReward|FestivalSchedule|FestivalScoring|HarvestFestival|MemoryFestival|ProtectionFestival|SecretFestival" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, visibility/participation/scoring/reward/schedule validation is deterministic.
- Requires EditMode tests: YES for calendar/secret/shop/skip/score/reward/yearly/no-softlock tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for festival UI/minigame validation.
- Requires regression test: YES if fixing existing festival event bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; festivals are optional and bounded.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/24_spec_festival_minigames_event_framework_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar sistema já especificado.
Não alterar scenes/prefabs/assets.
Não criar economia/power/progresso infinito.
Não vazar spoilers.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando as waves fundacionais relacionadas estiverem estáveis ou quando houver decisão humana explícita de antecipar este futuro.
