# SPEC — Festival Event Minigames Seasonal Activities Future Runtime

> **Spec ID:** `15_spec_festival_event_minigames_seasonal_activities_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 15 — Calendar / Festival / Weather / Lunar Future Systems  
> **Priority:** P2  
> **Type:** Runtime / Future / Festival / Minigames / Seasonal Events  
> **Domain:** Festival / Seasonal Activities / Event Windows / Rewards / Participation  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_15_CALENDAR_WEATHER_LUNAR_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere quest core, economy pricing final, NPC schedule concrete data, UI prefab/layout, social/romance deep systems, pet runtime, save migration ou scene/tilemap.  
> **Repo lock scope:** `Assets/_Game/Scripts/World/Festivals/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Tests/EditMode/World/**`, `docs/validation/15_spec_festival_event_minigames_seasonal_activities_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - festival UI;
  - festival quest adapter;
  - seasonal economy modifiers;
  - NPC schedule reactions;
  - final human validation.
> **Scope:** definir/endurecer contratos futuros de festival/minigame/atividade sazonal: janelas, participação, rewards, expiries, UI projection e anti-exploit.  
> **Out of scope:** minigame implementation final, scene/prefab/tilemap, social/romance deep events, final dialogue, final balance.

---

# /speckit.specify

## 1. Contexto

As estações podem ter festivais agrícolas, mercado/comida/música, colheita, luz, juramento, memória e proteção. O direction não define eventos específicos finais, mas autoriza festivais e eventos públicos conectados ao calendário, economia, cidade e quests.

---

## 2. Problema

Sem contratos:

```text
festival vira evento hardcoded;
minigame quebra economia com reward infinito;
festival bloqueia fazenda sem aviso;
quest festival expira sem feedback;
evento social vira sistema profundo fora do escopo;
loja/serviço muda sem calendário;
participação duplica reward após reload;
festival secreto aparece antes de descoberta.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FestivalDefinition;
FestivalActivityDefinition;
FestivalWindow;
FestivalParticipationState;
FestivalRewardPolicy;
FestivalMinigameResult;
FestivalQuestHook;
FestivalCalendarProjection;
FestivalAntiExploitValidator.
```

---

## 4. Regras de design

```text
Festival público aparece no calendário.
Festival pode alterar NPCs, lojas, pedidos e economia, mas sem arbitragem fácil.
Minigame/atividade tem janela clara.
Reward aplica uma vez por regra.
Festival não bloqueia rotina agrícola essencial sem aviso.
Eventos secretos ou quest-gated permanecem ocultos.
Social/romance deep runtime fica fora.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero saber quando festival ocorre.
Como festival, quero atividades com participação e reward seguros.
Como quest, quero festival quest expirar/turn-in de forma clara.
Como economia, quero demanda sazonal sem exploit.
Como cidade, quero schedule/event tags sem reescrever rotina individual.
```

---

## 6. Escopo

Inclui:

```text
festival definitions;
activity/minigame contracts;
event windows;
participation state;
reward policy;
quest hooks;
calendar projection;
anti-exploit tests.
```

Não inclui:

```text
visual scene/event layout;
minigame gameplay implementation;
dialogue writing;
social/romance deep systems;
final economy values.
```

## 7. Modelo de domínio

### 7.1 FestivalDefinition

```text
FestivalId
Season
Day
StartTime
EndTime
LocationId
PublicVisibility
ThemeTags[]
RequiredDiscoveryFlags[]
NpcScheduleModifierTags[]
ShopModifierTags[]
QuestHookIds[]
ActivityIds[]
RewardPolicyId
```

### 7.2 FestivalActivityDefinition

```text
ActivityId
FestivalId
ActivityType: Contest | Market | Food | Music | Memory | Protection | Harvest | Light | MiniGameFuture
EntryRequirements[]
StartTime
EndTime
AttemptsPolicy
ScorePolicy
RewardTierPolicy
CanRepeat
CanFail
```

### 7.3 FestivalParticipationState

```text
FestivalId
Visited
ActivitiesStarted[]
ActivitiesCompleted[]
RewardsGranted[]
LastParticipationDay
Expired
TurnInAvailable
```

### 7.4 FestivalRewardPolicy

```text
RewardPolicyId
ParticipationReward
ScoreTierRewards[]
UniqueRewards[]
RepeatRewards[]
GoldCap
ItemRewardCap
RelationshipFutureTags[]
EconomyRiskTag
```

---

## 8. Activity rules

```text
Contest:
  uses submitted item/score; no free item creation.

Market:
  may expose festival stock; uses economy stock refresh, not menu-open reset.

Food/Music:
  can grant temporary buff or flavor reward; no permanent exploit.

Harvest/Memory/Protection:
  can connect to quest/festival tags; no main progression block without hint.

MiniGameFuture:
  adapter only; gameplay implementation separate.
```

---

## 9. Reward/idempotency rules

```text
Unique festival reward once per festival/year/policy.
Repeat reward bounded by AttemptsPolicy.
Participation reward once per festival visit/day unless policy says otherwise.
Rewards recorded by stable RewardGrantId.
Reload cannot duplicate reward.
Expired festival cannot grant completion reward unless TurnInAvailable.
```

---

## 10. Criteria

```text
Festival/activity contracts exist.
Calendar projection exists or integrates with calendar spec.
Participation/reward idempotency enforced.
Economy anti-arbitrage guard exists.
Festival quest hooks respect expiry.
No social/romance deep runtime.
Tests cover public festival, hidden festival, activity attempt cap, reward idempotency, expired festival, market stock no reset and farm routine warning.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/World/Festivals/FestivalDefinition.cs
Assets/_Game/Scripts/World/Festivals/FestivalActivityDefinition.cs
Assets/_Game/Scripts/World/Festivals/FestivalWindow.cs
Assets/_Game/Scripts/World/Festivals/FestivalParticipationState.cs
Assets/_Game/Scripts/World/Festivals/FestivalRewardPolicy.cs
Assets/_Game/Scripts/World/Festivals/FestivalService.cs
Assets/_Game/Scripts/World/Festivals/FestivalAntiExploitValidator.cs
Assets/_Game/Tests/EditMode/World/FestivalSeasonalActivitiesTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/World/Festivals/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/UI/Calendar/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/World/**
docs/validation/15_spec_festival_event_minigames_seasonal_activities_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

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

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar festival/calendar/quest/economy systems.
- [ ] T003 — Consolidar festival/activity/reward contracts.
- [ ] T004 — Implementar service/validator.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec deriva de SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.
Ela não redefine rotinas individuais de NPC, layouts, crops finais, preços finais, questlines completas, cave generation ou UI visual final.
Ela implementa apenas o recorte future/mapped declarado no escopo.
Eventos secretos, luas ocultas, eventos de quest não iniciados e condições exatas de Mana antes da lore não podem ser revelados.
Pets permanecem deferidos; qualquer menção a pet é compatibilidade/guardrail, não runtime.
Quando houver conflito com City, Farm, Quest, Economy, Cave, UI ou Save directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Tempo/calendário/clima/luas conectam fazenda, cidade, caverna, economia, Fonte, Mana, quests, NPCs e eventos.
- Tempo cria decisão, mas não deve impedir o jogador de jogar.
- Calendário público pode registrar dia, estação, ano, clima previsto, evento lunar conhecido, festivais, restock especial, encomendas com prazo, eventos de cidade conhecidos, eventos de quest descobertos e dias de loja fechada.
- Eventos secretos não aparecem até serem descobertos.
- Previsão pode vir de calendário/quadro público, NPC/serviço/observação/upgrade.
- Luas podem afetar mundo antes do jogador entender, mas efeitos que bloqueiam progressão precisam ser descobertos, sinalizados ou previsíveis.
- Economia pode variar por restock semanal/sazonal, itens sazonais, festival, mercador raro, loja noturna, sementes raras, itens mágicos instáveis e demanda temporária.
- Evitar arbitragem fácil, preço opaco, restock ao abrir menu, reset de loja sem day transition, economia quebrada por Mana ou evento lunar gerando dinheiro infinito.

### Deferred / future from directions

- Astronomia real completa.
- UI visual final de calendário/clima/lua.
- Eventos específicos finais de cada festival.
- Balance final de preços/crops/economia.
- Questlines completas.
- Pet runtime.
- Romance/social deep runtime.
- Scene/prefab/tilemap edits.
- Full cave procedural changes.

### Explicitly not redefined here

- NPC schedules concretos.
- Crop growth formulas finais.
- Pricing formula final.
- Enemy stats/spawns finais.
- QuestState/MainProgression/FonteAnya ownership.
- Pet systems.
- Companion systems.
- UI prefab/layout final.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu world/calendar/weather/lunar e domains afetados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a festival event/minigames/seasonal activities foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Eventos secretos, luas ocultas, Mana, Anya, Pedra Negra, 100/101 e final choice não vazam? | Tests/checklist. | PARTIAL |
| Anti-softlock | Evento temporal/lunar/climático não bloqueia progresso essencial sem pista/controle razoável? | Tests/checklist. | PARTIAL |
| Anti-economia | Variações de evento não criam arbitragem, restock indevido ou dinheiro infinito. | Tests/checklist. | PARTIAL |
| Pet deferido | Não há runtime pet. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/15_spec_festival_event_minigames_seasonal_activities_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Festival|SeasonalEvent|Minigame|Participation|FestivalReward|FestivalQuest|FestivalActivity|FestivalWindow" Assets/_Game/Scripts docs/design .specs
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|Fonte|BlackStone|PedraNegra|SecretEvent|Pet|Companion" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o estado de calendário/clima/lua/evento é conhecido ou descoberto
When o sistema desta spec projeta ou aplica seu recorte
Then somente efeitos autorizados e sinalizados ocorrem
And nenhum evento secreto/lore sensível é revelado cedo
And economia, quest, Fonte, Mana, cave e city schedules respeitam seus domínios de origem.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler temporal

```text
Given evento secreto, lua oculta, condição exata de Mana, evento de quest não iniciado, Pedra Negra, Anya, Arquivista, nível 100/101 ou final choice
When calendário/UI/forecast/economy/cave consulta eventos futuros
Then informação fica oculta, genérica ou gated
And só aparece quando descoberta/autorizada.
```

### Scenario 4 — Anti-softlock

```text
Given uma quest/progressão depende de clima, festival ou lua
When a condição não está ativa
Then o jogador tem pista, previsão, janela futura, alternativa ou wait/retry claro
And progresso essencial não fica bloqueado silenciosamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de calendário, festival, forecast, cave modifier, shop/event board ou notification
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Festival reward duplicates after reload.
- Festival market restocks when menu opens.
- Activity attempts infinite.
- Festival blocks farm routine without warning.
- Hidden/quest festival visible too early.
- Social/romance deep event implemented.
- Participation state not persisted safely.
- Expired festival still grants completion reward incorrectly.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Festival Event Minigames Seasonal Activities Future Runtime

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

## Calendar/weather/lunar compliance
- Secret events hidden:
- Known events visible:
- Quest conditions signaled:
- Economy anti-arbitrage:
- No pet runtime:
- No romance/social deep runtime:
- No scene/prefab/tilemap:
- Save/load safe:
- UI projection not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler temporal:
- Anti-softlock:
- Anti-economy exploit:
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
2. A implementação exigir scene/prefab/tilemap/asset wiring fora do escopo.
3. A implementação exigir save migration sem spec de migration.
4. A implementação revelar evento secreto/quest/lua/Mana/Pedra Negra/100/101/final cedo.
5. A implementação bloquear progressão essencial por clima/lua/festival sem pista/previsão/alternativa.
6. A implementação criar arbitragem, restock por abrir menu, preço opaco ou dinheiro infinito.
7. A implementação criar runtime pet.
8. A implementação criar romance/social deep runtime.
9. A implementação redefinir NPC schedules concretos, crop formulas, prices, enemy spawns ou cave generation.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|SecretEvent" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de calendário, forecast, festival, shop/event board, cave weather/lunar modifier ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, festival window/participation/reward/idempotency logic is deterministic.
- Requires EditMode tests: YES for visibility/window/attempt/reward/expiry/economy/farm-warning tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for festival UI/minigame visual validation.
- Requires regression test: YES if fixing existing festival reward/expiry bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; festival rewards idempotent; no market reset exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/15_spec_festival_event_minigames_seasonal_activities_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não revelar evento secreto cedo.
Não revelar Mana/Anya/Pedra Negra/100/101/final cedo.
Não bloquear progresso sem pista.
Não criar economia infinita.
Não resetar loja ao abrir menu.
Não implementar Pet runtime.
Não implementar romance/social deep runtime.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando calendar/time, economy, city schedules, UI, quest and cave foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
