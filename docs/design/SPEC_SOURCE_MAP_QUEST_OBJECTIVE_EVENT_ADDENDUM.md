# Cindar's Hope — Spec Source Map Quest Objective Event Addendum

> **Status:** addendum temporário de source map para Quest / Objective / Event System  
> **Local:** `docs/design/SPEC_SOURCE_MAP_QUEST_OBJECTIVE_EVENT_ADDENDUM.md`  
> **Fonte principal relacionada:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Fonte nova:** `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`  
> **Função:** registrar imediatamente a rastreabilidade do direction genérico de quests/objectives/events enquanto o `SPEC_SOURCE_MAP.md` principal não for alterado via patch parcial seguro.  
> **Não é spec implementável.**

---

## 0. Regra de precedência

Este addendum deve ser tratado como extensão do `SPEC_SOURCE_MAP.md` até que o bloco equivalente seja aplicado diretamente no arquivo principal.

Toda spec que envolva quest system, QuestId, QuestDefinition, QuestState, QuestStep, Objective, Condition, Trigger, Reward, QuestFlag, QuestEvent, branching, failure, expiry, quest UI, quest log, quest save/load, anti-softlock, anti-spoiler, main quest hooks, Fonte hooks, time/weather/lunar hooks, NPC/dialogue hooks, farm orders, festival quests, cave contracts, hidden quests ou tutorial quests deve ler obrigatoriamente:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
```

---

## 1. Specs de main quest

Specs de main quest que envolvam atos, fragmentos, Fonte, Cindar, Arco da Memória, Pedra Negra, nível 100/101, Arquivista do Silêncio, final choices, gates, events ou quest states devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regra:

```text
Main quest lore/progression continuam em documentos próprios.
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md define o sistema genérico de steps, objectives, conditions, triggers, rewards e flags usado por essas quests.
```

---

## 2. Specs de quest UI/log

Specs que envolvam Quest Log, Quest Detail, objective visibility, tracked quest, hidden quest, known hints, quest notification, spoiler control ou quest log HUD devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Regra:

```text
Quest Log não revela objetivo, reward, boss, condição ou final oculto cedo.
```

---

## 3. Specs de save/load de quests

Specs que envolvam QuestState, CurrentStepId, ObjectiveStates, KnownObjectiveIds, KnownHints, StartedAtDay, CompletedAtDay, ExpiresAtDay, Tracked, Discovered, ChoiceHistory, GrantedRewardIds, GrantedFlagIds ou QuestFlags devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

Regra:

```text
QuestState, MainProgression e FonteAnya são seções separadas de save/load.
QuestFlag não substitui QuestState.
```

---

## 4. Specs de tempo, clima, lua e festivais em quests

Specs que envolvam WaitForTime, WaitForDay, WaitForSeason, WaitForWeather, WaitForLunarEvent, AttendFestival, FestivalStarted, FestivalEnded ou quest expiry por calendário devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Regra:

```text
Quest obrigatória com tempo/lua precisa dar pista e controle razoável.
Main quest não expira por tempo.
FarmOrder e Festival quests podem expirar se prazo for claro.
```

---

## 5. Specs de NPC/dialogue hooks

Specs que envolvam diálogo iniciando quest, diálogo avançando step, DialogueChoiceBranch, NPC availability, NPC schedule integration, NPC teaching, service unlocks ou quest flags por diálogo devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
```

---

## 6. Specs de farm/city/cave objectives

Specs que envolvam FarmOrder, crop objective, shipping objective, city board, cave contract, enemy defeat objective, cave depth objective, corpse recovery objective, Fonte use objective ou world interaction objective devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
```

---

## 7. Specs de bestiary/knowledge objectives

Specs que envolvam DiscoverBestiaryKnowledge, DiscoverWeakness, ConfirmRumor, DocumentBehavior, CollectSample, ReadLoreNote ou knowledge unlock rewards devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 8. Specs futuras recomendadas

```text
spec_quest_objective_event_contract_runtime.md
spec_quest_state_save_load_runtime.md
spec_quest_condition_trigger_runtime.md
spec_quest_reward_application_idempotency_runtime.md
spec_quest_log_visibility_spoiler_runtime.md
spec_quest_flags_registry_runtime.md
spec_quest_farm_orders_adapter_runtime.md
spec_quest_festival_expiry_runtime_future.md
spec_quest_bestiary_discovery_objectives_future.md
spec_quest_fonte_main_progression_hooks_future.md
spec_quest_debug_validation_tools_future.md
spec_quest_anti_softlock_validation_future.md
```

---

## 9. Anti-regressão

```text
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md é fonte canônica de sistema genérico de quests, objectives, conditions, triggers, rewards, quest flags e quest events.
Main quest lore/progression continuam em documentos próprios.
Main quest, side quests, farm orders, social future, companion future, pet future, festival, cave contracts e tutorial usam o mesmo sistema base.
QuestState, MainProgression e FonteAnya são seções separadas de save/load.
QuestFlag não substitui QuestState.
Condition não avança quest sozinha; Trigger/event avança quando condições permitem.
Reward precisa ser idempotente.
Quest Log não revela objetivo, reward, boss, condição ou final oculto cedo.
Main quest não falha por tempo.
Main quest não pode ficar impossível por item vendido, NPC fora de schedule, clima/lua perdido, morte do jogador ou inventário cheio.
FarmOrder e Festival quests podem expirar se prazo for claro.
Hidden quest não aparece no log até ser descoberta.
Quest crítica precisa de anti-softlock.
Quest system consome eventos de gameplay; não substitui farm, combat, inventory, economy, bestiary ou Fonte.
```

---

## 10. Bloco a aplicar no SPEC_SOURCE_MAP.md principal

Quando houver patch parcial seguro, aplicar no `docs/design/SPEC_SOURCE_MAP.md` principal, preferencialmente antes da anti-regressão ou como nova seção de Quest System:

```md
# PARTE O — Quest / Objective / Event System

## Specs de sistema genérico de quests/objectives/events

Fontes obrigatórias:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
```

Specs que envolvam quest system, QuestId, QuestDefinition, QuestState, QuestStep, Objective, Condition, Trigger, Reward, QuestFlag, QuestEvent, branching, failure, expiry, quest UI, quest log, quest save/load, anti-softlock, anti-spoiler, main quest hooks, Fonte hooks, time/weather/lunar hooks, NPC/dialogue hooks, farm orders, festival quests, cave contracts, hidden quests ou tutorial quests devem ler obrigatoriamente esta fonte.
```
