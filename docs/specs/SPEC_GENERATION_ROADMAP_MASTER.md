# Cindar's Hope — Spec Generation Roadmap Master

> **Status:** roadmap macro de geração de specs.  
> **Local:** `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> **Fonte de verdade relacionada:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Função:** listar todas as specs previstas, organizar a ordem de geração, indicar dependências, blocos paralelizáveis, escopo de primeira entrega e escopo futuro.  
> **Não é spec implementável.** Nenhum agente deve codificar a partir deste arquivo isoladamente.  
> **Regra:** este roadmap organiza a sequência; as regras canônicas continuam nos direction docs e no `SPEC_SOURCE_MAP.md`.

---

## 0. Objetivo deste documento

Este documento existe para responder:

```text
Quais specs ainda precisamos gerar?
Qual ordem faz sentido?
Quais specs podem rodar em paralelo?
Quais specs são fundação e bloqueiam outras?
Quais entram na primeira entrega executável?
Quais ficam explicitamente futuras?
Quais directions cada grupo precisa respeitar?
```

Este documento não substitui:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

Uso correto:

```text
1. Usar este roadmap para decidir o próximo lote de specs a gerar.
2. Para cada spec gerada, ler o SPEC_SOURCE_MAP.md e os direction docs obrigatórios.
3. Depois que uma spec existir, registrá-la nos registries corretos.
4. Depois que uma spec for implementada, atualizar status, logs e validações.
```

---

## 1. Premissas canônicas

O roadmap assume que os refinamentos/directions abaixo já existem e são fontes canônicas:

```text
UI/UX:
  docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
  docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

World/Time:
  docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

Save/Load:
  docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

Bestiary/Knowledge:
  docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

Quest System:
  docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

Main Quest:
  docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
  docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regras globais já centralizadas no `SPEC_SOURCE_MAP.md`:

```text
Specs de UI/HUD/menus precisam ler os directions de UI/UX.
Specs de tempo/calendário/clima/luas precisam ler o direction World/Time.
Specs com estado persistido precisam ler Save/Load Full State.
Specs de bestiário/conhecimento precisam ler Bestiary/Knowledge Discovery.
Specs de quests/objectives/events precisam ler Quest/Objective/Event System.
```

---

## 2. Contagem macro estimada

```text
Specs de governança/roadmap:
  5

Specs de fundação técnica:
  7

Specs de tempo/calendário/clima/lua:
  8 núcleo + 5 futuras

Specs de quest/objective/event:
  7 núcleo + 5 futuras

Specs de UI fundacional e telas:
  15 núcleo + 3 futuras

Specs de inventory/equipment/items:
  8 núcleo + 3 futuras

Specs de farm/economy/crafting:
  9 núcleo + 4 futuras

Specs de city/NPC/dialogue:
  8 núcleo + 4 futuras

Specs de player/skills/magic:
  11 núcleo + 3 futuras

Specs de cave/combat/enemies/death:
  12 núcleo + 5 futuras

Specs de bestiary/knowledge:
  7 núcleo + 5 futuras

Specs de pets/companions/social/endgame/future systems:
  13 futuras
```

Estimativa total:

```text
Núcleo / primeira grande execução:
  ~92 specs

Explicitamente futuras:
  ~47 specs

Total mapeado:
  ~139 specs
```

Observação:

```text
O número total pode reduzir quando specs próximas forem consolidadas.
O número pode aumentar se separarmos specs muito grandes por runtime, UI, data assets e save/load.
```

---

## 3. Política de escopo

### 3.1 Entra na primeira grande execução

```text
fundação de IDs/eventos/save;
time/calendar/weather/lunar básico;
quest/objective/event core;
UI modal/focus/HUD/telas essenciais;
inventory/item instance/equipment/hotbar;
farm/crops/crafting/economy/orders;
city/NPC/dialogue/services básico;
player/core resources/skills/magic;
cave/combat/enemies/death;
bestiary discovery sem UI completa.
```

### 3.2 Fica futuro explícito

```text
Bestiary UI/HUD completa;
research service;
pets avançados;
companions avançados;
social/romance/casamento/poliamor;
partner helper;
festival minigames;
endgame nível 100/101;
final choice cinematics;
política final de save em caverna;
weather/lunar deep modifiers na caverna;
full automation por NPC/partners.
```

### 3.3 Regra de não duplicação

```text
Uma spec não deve redefinir regra já autorada em direction.
Uma spec deve apontar a fonte canônica e implementar só o recorte dela.
Se a spec precisar alterar regra canônica, o direction e o SPEC_SOURCE_MAP.md devem ser atualizados antes.
```

---

## 4. Waves e dependências

Sequência canônica:

```text
WAVE 00 — Roadmap / governança
WAVE 01 — Core IDs / events / save baseline
WAVE 02 — Time / Calendar / Weather / Lunar
WAVE 03 — Quest / Objective / Event baseline
WAVE 04 — UI foundation
WAVE 05 — Inventory / ItemInstance / Equipment / Hotbar
WAVE 06 — UI screens essenciais
WAVE 07 — Farm / Economy / Crafting
WAVE 08 — City / NPC / Dialogue / Services
WAVE 09 — Player / Skills / Magic
WAVE 10 — Cave / Combat / Enemies / Death
WAVE 11 — Bestiary / Knowledge Discovery
WAVE 12 — Future systems
```

Grafo macro:

```text
00 -> 01
01 -> 02, 03, 04, 05
02 -> 07, 08, 03 temporal hooks
03 -> 06, 07 orders, 08 dialogue hooks, 10 cave contracts, 11 knowledge objectives
04 -> 06
05 -> 06, 07, 09, 10
06 -> validação de UI de domínio
07 -> 08 boards/orders e economia
08 -> social future, companion future
09 -> 10
10 -> 11
12 depende de waves específicas por domínio
```

---

## 5. Paralelismo seguro

Depois da WAVE 01, as lanes podem rodar em paralelo com integração por checkpoints.

```text
Lane A — World/Farm/Economy:
  WAVE 02 -> WAVE 07

Lane B — Quest/UI:
  WAVE 03 -> WAVE 04 -> WAVE 06

Lane C — Player/Combat/Cave:
  WAVE 05 -> WAVE 09 -> WAVE 10

Lane D — City/NPC:
  WAVE 02 + WAVE 03 -> WAVE 08

Lane E — Bestiary:
  WAVE 10 -> WAVE 11
```

Regras de paralelismo:

```text
Não implementar UI de sistema sem contrato runtime mínimo.
Não implementar save de sistema sem section ownership.
Não implementar quest hook sem quest event contract.
Não implementar weather/lunar modifier sem time/calendar contract.
Não implementar bestiary discovery sem enemy/vulnerability contract.
```

---

# WAVE 00 — Roadmap / governança

## Objetivo

Preparar a geração massiva de specs sem duplicar fontes nem quebrar rastreabilidade.

## Specs

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 00.01 | `00_spec_generation_roadmap_master.md` | governança | gerar agora | SPEC_SOURCE_MAP | todas |
| 00.02 | `00_spec_wave_execution_protocol.md` | governança | gerar depois | 00.01 | execução por agentes |
| 00.03 | `00_spec_source_map_numbering_cleanup.md` | governança | opcional antes das specs | 00.01 | limpeza documental |
| 00.04 | `00_spec_existing_implementation_audit.md` | auditoria | gerar antes de runtime | 00.01 | evitar duplicação de código |
| 00.05 | `00_spec_validation_matrix_master.md` | validação | gerar antes de runtime | 00.01 | critérios de aceite |

## Regras

```text
Não criar spec runtime antes de confirmar quais sistemas já existem.
Não mover spec para implementados sem evidência de validação.
Não usar este roadmap como substituto do SPEC_SOURCE_MAP.md.
```

---

# WAVE 01 — Core IDs / Events / Save baseline

## Objetivo

Criar a fundação técnica transversal para impedir specs isoladas e incompatíveis.

## Specs

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 01.01 | `01_spec_stable_ids_registry_runtime.md` | runtime/data | núcleo | 00 | todos os sistemas persistidos |
| 01.02 | `01_spec_game_event_contracts_runtime.md` | runtime | núcleo | 01.01 | quest, farm, combat, bestiary |
| 01.03 | `01_spec_save_restore_order_contract_runtime.md` | runtime | núcleo | 01.01 | todos os saves |
| 01.04 | `01_spec_save_section_ownership_registry.md` | data/registry | núcleo | 01.03 | specs com estado persistido |
| 01.05 | `01_spec_save_provider_architecture_runtime.md` | runtime | núcleo gradual | 01.03, 01.04 | refactor controlado save |
| 01.06 | `01_spec_invalid_id_fallback_rules.md` | runtime/validation | núcleo | 01.01, 01.03 | load robusto |
| 01.07 | `01_spec_playmode_validation_baseline.md` | validação | núcleo | 00.05 | todas as waves runtime |

## Regras obrigatórias

```text
Todo estado persistido usa ID estável.
DTOs não serializam referência Unity.
Evento não substitui fonte primária de sistema.
SaveManager continua orquestrador atual.
Provider architecture é alvo gradual, não refactor massivo imediato.
Restore order precisa ser explícito.
```

## Paralelismo

```text
01.01 e 01.02 podem ser paralelos se alinharem naming de IDs/eventos.
01.03, 01.04, 01.05 devem seguir juntos.
01.06 depende de IDs e save.
```

---

# WAVE 02 — Time / Calendar / Weather / Lunar

## Objetivo

Implementar a base temporal que afeta crops, NPCs, quests, clima, luas, Mana e Fonte.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 02.01 | `02_spec_time_clock_day_transition_runtime.md` | runtime | núcleo | 01 | calendar, crops, quests |
| 02.02 | `02_spec_calendar_season_year_runtime.md` | runtime/data | núcleo | 02.01 | season crops, festivals |
| 02.03 | `02_spec_weather_generation_forecast_runtime.md` | runtime/data | núcleo | 02.01, 02.02 | rain, schedules |
| 02.04 | `02_spec_rain_irrigation_crop_integration.md` | integration | núcleo | 02.03, farm base | crops |
| 02.05 | `02_spec_lunar_cycle_event_runtime.md` | runtime/data | núcleo | 02.01, 02.02 | Nyx/Alihana/Senya hooks |
| 02.06 | `02_spec_calendar_festivals_events_runtime.md` | runtime/data | núcleo | 02.02 | festival quests |
| 02.07 | `02_spec_time_calendar_weather_lunar_save_state.md` | save/load | núcleo | 02.01-02.06, 01.03 | persistence |
| 02.08 | `02_spec_calendar_ui_weather_lunar_display.md` | UI | núcleo | 02.01-02.07, 04 | calendar UI |

## Specs futuras

```text
02_spec_weather_farm_pet_companion_reactions_future.md
02_spec_lunar_fonte_mana_reactions_future.md
02_spec_npc_schedule_weather_lunar_modifiers_future.md
02_spec_shop_calendar_weather_lunar_modifiers_future.md
02_spec_cave_weather_lunar_modifiers_future.md
```

## Regras obrigatórias

```text
Tempo é fonte única.
Day transition é determinístico.
Chuva pode irrigar, mas regra de crop continua em farm/crop spec.
Quest obrigatória não depende de evento raro sem pista e forma razoável de esperar.
Calendar UI não revela segredo.
Weather/lunar state precisa ser salvo/carregado.
```

---

# WAVE 03 — Quest / Objective / Event System

## Objetivo

Criar o sistema genérico de quests, objectives, conditions, triggers, rewards, quest flags e quest events.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 03.01 | `03_spec_quest_objective_event_contract_runtime.md` | runtime/data | núcleo | 01.01, 01.02 | todas as quests |
| 03.02 | `03_spec_quest_condition_trigger_runtime.md` | runtime | núcleo | 03.01, 02 parcial | objectives temporais |
| 03.03 | `03_spec_quest_reward_application_idempotency_runtime.md` | runtime | núcleo | 03.01 | rewards seguros |
| 03.04 | `03_spec_quest_flags_registry_runtime.md` | runtime/data | núcleo | 03.01 | hooks cross-system |
| 03.05 | `03_spec_quest_state_save_load_runtime.md` | save/load | núcleo | 03.01-03.04, 01.03 | quest persistence |
| 03.06 | `03_spec_quest_log_visibility_spoiler_runtime.md` | UI/rules | núcleo | 03.01, 04 | quest log |
| 03.07 | `03_spec_quest_anti_softlock_validation_future.md` | validation | núcleo/future tooling | 03.01-03.06 | main quest safety |

## Specs futuras

```text
03_spec_quest_farm_orders_adapter_runtime.md
03_spec_quest_festival_expiry_runtime_future.md
03_spec_quest_bestiary_discovery_objectives_future.md
03_spec_quest_fonte_main_progression_hooks_future.md
03_spec_quest_debug_validation_tools_future.md
```

## Regras obrigatórias

```text
Condition não avança quest sozinha.
Trigger/event avança quando conditions permitem.
Reward é idempotente.
QuestFlag não substitui QuestState.
QuestState, MainProgression e FonteAnya são seções separadas.
Quest Log não revela spoiler.
Main quest não falha por tempo.
Quest crítica precisa de anti-softlock.
```

---

# WAVE 04 — UI foundation

## Objetivo

Criar base de input, foco, modal stack, HUD, notificações e estados de erro/vazio.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 04.01 | `04_spec_ui_input_focus_modal_routing.md` | runtime/UI | núcleo | 01 | todos os menus |
| 04.02 | `04_spec_ui_menu_focus_stack_runtime.md` | runtime/UI | núcleo | 04.01 | telas modais |
| 04.03 | `04_spec_ui_hud_main_gameplay_runtime.md` | UI | núcleo | 04.01 | HUD gameplay |
| 04.04 | `04_spec_ui_empty_error_confirmation_patterns_runtime.md` | UI | núcleo | 04.01 | actions custosas |
| 04.05 | `04_spec_ui_notification_runtime.md` | UI | núcleo | 04.01 | quest/bestiary/etc |
| 04.06 | `04_spec_ui_debug_hud_strip_policy.md` | rules | núcleo | 04.03 | build final |

## Specs futuras

```text
04_spec_ui_gamepad_navigation_future.md
04_spec_ui_accessibility_preferences_future.md
04_spec_ui_pause_options_menu_future.md
```

## Regras obrigatórias

```text
Menu modal bloqueia gameplay input.
Diálogo não deixa personagem andar.
HUD final não mostra debug.
Confirmações existem para ações custosas.
Empty/error states não parecem bug.
Tooltip curto não substitui drawer complexo.
```

---

# WAVE 05 — Inventory / ItemInstance / Equipment / Hotbar

## Objetivo

Consolidar item runtime antes de shop, crafting, equipment UI, quest item delivery e loot avançado.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 05.01 | `05_spec_inventory_item_instance_runtime.md` | runtime/data | núcleo | 01 | equipment, crafting, quests |
| 05.02 | `05_spec_inventory_slot_stack_split_move_runtime.md` | runtime/UI adapter | núcleo | 05.01 | inventory UI |
| 05.03 | `05_spec_hotbar_binding_runtime.md` | runtime | núcleo | 05.01, 01.06 | hotbar UI |
| 05.04 | `05_spec_equipment_slots_runtime.md` | runtime/data | núcleo | 05.01 | equipment UI, combat |
| 05.05 | `05_spec_equipment_durability_runtime.md` | runtime | núcleo | 05.04 | repair/upgrades |
| 05.06 | `05_spec_equipment_repair_upgrade_runtime.md` | runtime | núcleo | 05.05 | crafting/economy |
| 05.07 | `05_spec_tool_weapon_material_tag_runtime.md` | runtime/data | núcleo | 05.04 | vulnerabilities/bestiary |
| 05.08 | `05_spec_item_instance_save_load_runtime.md` | save/load | núcleo | 05.01-05.07, 01.03 | persistence |

## Specs futuras

```text
05_spec_item_lock_favorite_future.md
05_spec_item_source_history_future.md
05_spec_equipment_upgrade_risk_quality_future.md
```

## Regras obrigatórias

```text
ItemDefinition não é salvo, só ID.
ItemInstance guarda estado mutável.
Quest/key item é protegido.
Hotbar valida binding inválido.
Equipment durability/upgrades não são UI state.
Material tags integram com vulnerability adapter e bestiary futuro.
```

---

# WAVE 06 — UI screens essenciais

## Objetivo

Transformar o direction de UI em telas jogáveis e consistentes.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 06.01 | `06_spec_ui_inventory_screen_flow_runtime.md` | UI | núcleo | 04, 05.01, 05.02 | inventory final |
| 06.02 | `06_spec_ui_storage_chest_screen_flow_runtime.md` | UI | núcleo | 06.01 | storage |
| 06.03 | `06_spec_ui_equipment_screen_compare_runtime.md` | UI | núcleo | 04, 05.04 | equipment compare |
| 06.04 | `06_spec_ui_weapon_armor_detail_drawer_runtime.md` | UI | núcleo | 05.04-05.07, 11 future hook | equipment details |
| 06.05 | `06_spec_ui_shop_buy_sell_screen_runtime.md` | UI | núcleo | 04, 05, 07 economy | shop final |
| 06.06 | `06_spec_ui_crafting_recipe_screen_runtime.md` | UI | núcleo | 04, 05, 07 crafting | crafting final |
| 06.07 | `06_spec_ui_quest_log_detail_runtime.md` | UI | núcleo | 03, 04 | quest log |
| 06.08 | `06_spec_ui_calendar_day_detail_runtime.md` | UI | núcleo | 02, 04 | calendar UI |
| 06.09 | `06_spec_ui_fonte_menu_flow_runtime.md` | UI | núcleo | 03, main/Fonte specs | Fonte UI |

## Specs futuras

```text
06_spec_ui_bestiary_menu_future.md
06_spec_ui_social_log_future.md
06_spec_ui_companion_pet_panels_future.md
```

## Regras obrigatórias

```text
Shop Buy usa estoque da loja.
Shop Sell usa inventário vendável do jogador.
Equipment compare não equipa por hover.
Weapon/armor detail separa overview/stats/effects/material/durability.
Quest Log respeita spoiler.
Calendar não revela segredo.
Fonte menu só mostra função desbloqueada.
```

---

# WAVE 07 — Farm / Economy / Crafting

## Objetivo

Completar o loop agrícola/econômico com tempo, clima, crafting, processing e orders.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 07.01 | `07_spec_crop_season_weather_rules_runtime.md` | runtime | núcleo | 02, farm base | crops |
| 07.02 | `07_spec_crop_quality_fertilizer_runtime.md` | runtime | núcleo | 07.01 | economy/crafting |
| 07.03 | `07_spec_farm_storage_shipping_bin_runtime.md` | runtime | núcleo | 05, 02 | farm economy |
| 07.04 | `07_spec_crafting_recipe_station_runtime.md` | runtime/data | núcleo | 05 | crafting UI |
| 07.05 | `07_spec_processing_jobs_runtime.md` | runtime/save | núcleo | 02, 05, 07.04 | processing |
| 07.06 | `07_spec_shop_stock_refresh_runtime.md` | runtime/save | núcleo | 02, economy base | shop |
| 07.07 | `07_spec_economy_pricing_sell_buy_rules_runtime.md` | runtime/rules | núcleo | 05, 07.06 | buy/sell |
| 07.08 | `07_spec_farm_orders_adapter_runtime.md` | quest adapter | núcleo | 03, 07.01-07.07 | farm orders |
| 07.09 | `07_spec_farm_world_persistence_runtime.md` | save/load | núcleo | 01, farm/world | persistence |

## Specs futuras

```text
07_spec_farm_buildings_layout_runtime_future.md
07_spec_farm_animals_runtime_future.md
07_spec_partner_helper_farm_jobs_future.md
07_spec_automation_limits_future.md
```

## Regras obrigatórias

```text
Crop respeita estação/clima.
Rain pode irrigar.
Processing sobrevive troca de cena/dia.
Shop restock é controlado.
Preço final calculado não é fonte primária de save.
Farm orders usam quest system.
Shipping processa no day transition.
```

---

# WAVE 08 — City / NPC / Dialogue / Services

## Objetivo

Consolidar cidade, NPCs, schedules, serviços e hooks de diálogo/quest.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 08.01 | `08_spec_npc_identity_service_registry_runtime.md` | data/runtime | núcleo | 01 | NPC systems |
| 08.02 | `08_spec_npc_schedule_runtime.md` | runtime | núcleo | 02, 08.01 | city living |
| 08.03 | `08_spec_dialogue_runtime.md` | runtime/UI adapter | núcleo | 04, 08.01 | dialogue hooks |
| 08.04 | `08_spec_dialogue_choice_quest_hooks_runtime.md` | integration | núcleo | 03, 08.03 | main/side quests |
| 08.05 | `08_spec_city_building_interaction_runtime.md` | runtime | núcleo | city docs, 04 | buildings |
| 08.06 | `08_spec_notice_board_orders_contracts_runtime.md` | runtime/quest | núcleo | 03, 07, 10 partial | board quests |
| 08.07 | `08_spec_shop_npc_service_runtime.md` | runtime | núcleo | 07.06, 07.07, 08.01 | shops/services |
| 08.08 | `08_spec_npc_save_load_runtime.md` | save/load | núcleo | 01, 08.01-08.02 | persistence |

## Specs futuras

```text
08_spec_npc_weather_lunar_schedule_modifiers_future.md
08_spec_npc_relationship_dialogue_variants_future.md
08_spec_city_festival_service_overrides_future.md
08_spec_social_unlocks_by_npc_future.md
```

## Regras obrigatórias

```text
Rotina individual vence no city layout/schedule doc.
Schedule deriva de tempo/clima/lua quando modificadores existirem.
NPC dialogue pode iniciar/avançar quest.
NPC service pode desbloquear stock.
Notice board pode gerar FarmOrder/CaveContract/Festival quest.
Quest system não duplica schedule.
```

---

# WAVE 09 — Player / Skills / Magic

## Objetivo

Fechar progressão do jogador, recursos, skill tree, active slots, respec e magia.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 09.01 | `09_spec_player_core_stats_runtime.md` | runtime | núcleo | 01 | combat/skills |
| 09.02 | `09_spec_stamina_mana_hunger_integration_runtime.md` | runtime | núcleo | 09.01 | tools/combat/magic |
| 09.03 | `09_spec_skill_tree_runtime.md` | runtime/data | núcleo | 01, 09.01 | skills |
| 09.04 | `09_spec_skill_point_progression_runtime.md` | runtime | núcleo | 09.03 | progression |
| 09.05 | `09_spec_active_skill_slots_runtime.md` | runtime/UI adapter | núcleo | 09.03, 09.04 | skill actions |
| 09.06 | `09_spec_skill_respec_fonte_integration_runtime.md` | integration | núcleo | 09.03-09.05, Fonte | respec |
| 09.07 | `09_spec_magic_spell_action_runtime.md` | runtime | núcleo | 09.01, 09.02 | combat/magic |
| 09.08 | `09_spec_magic_learning_unlock_sources_runtime.md` | runtime/data | núcleo | 03, 05, 09.07 | spell unlocks |
| 09.09 | `09_spec_ui_skill_tree_node_detail_runtime.md` | UI | núcleo | 04, 06, 09.03 | skill UI |
| 09.10 | `09_spec_ui_active_slot_assignment_runtime.md` | UI | núcleo | 09.05, 04 | active slots UI |
| 09.11 | `09_spec_ui_spell_magic_detail_runtime.md` | UI | núcleo | 09.07, 09.08, 04 | spell UI |

## Specs futuras

```text
09_spec_skill_capstone_choice_future.md
09_spec_spell_variants_future.md
09_spec_magic_scroll_wand_staff_deep_system_future.md
```

## Regras obrigatórias

```text
SkillPoint a cada 2 níveis, se mantido no direction.
4 active slots.
Active skill comprada não equipa automaticamente se slots cheios.
Respec depende da Fonte.
Spell source pode ser permanente, scroll, item ou quest.
UI mostra custo/cooldown/requisito sem spoiler.
```

---

# WAVE 10 — Cave / Combat / Enemies / Death

## Objetivo

Consolidar gameplay de caverna, combate, inimigos, loot, bosses, death e recovery.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 10.01 | `10_spec_cave_run_seed_state_runtime.md` | runtime/save | núcleo | 01, 02, 05 | cave persistence |
| 10.02 | `10_spec_cave_floor_generation_runtime.md` | runtime | núcleo | 10.01 | cave run |
| 10.03 | `10_spec_cave_checkpoint_runtime.md` | runtime/save | núcleo | 10.01, 10.02 | death/recovery |
| 10.04 | `10_spec_cave_resource_node_runtime.md` | runtime/loot | núcleo | 05, 07 economy | cave resources |
| 10.05 | `10_spec_cave_monster_roster_data_runtime.md` | data/runtime | núcleo | 01, enemy docs | enemies |
| 10.06 | `10_spec_enemy_behavior_module_runtime.md` | runtime | núcleo | 10.05 | combat AI |
| 10.07 | `10_spec_enemy_vulnerability_runtime.md` | runtime/data | núcleo | 05.07, 10.05 | bestiary/combat |
| 10.08 | `10_spec_combat_player_actions_runtime.md` | runtime | núcleo | 09, 05 | combat |
| 10.09 | `10_spec_status_effects_runtime.md` | runtime | núcleo | 09, 10.06-10.08 | damage/status |
| 10.10 | `10_spec_boss_gate_runtime.md` | runtime/quest | núcleo | 03, 10.01-10.07 | bosses/main quest |
| 10.11 | `10_spec_death_corpse_recovery_runtime.md` | runtime/save | núcleo | 10.01-10.03, Save | recovery |
| 10.12 | `10_spec_cave_loot_resource_runtime.md` | runtime/loot | núcleo | 05, 07, 10.04 | loot economy |

## Specs futuras

```text
10_spec_cave_weather_lunar_modifiers_future.md
10_spec_cave_boss_phase_memory_events_future.md
10_spec_cave_level_100_101_future.md
10_spec_save_cave_policy_checkpoint_future.md
10_spec_cave_fishing_resource_variants_future.md
```

## Regras obrigatórias

```text
Save em caverna continua permitido por enquanto.
Run seed precisa persistir.
Checkpoint/floor/depth precisam restaurar de forma consistente.
Enemy data usa IDs estáveis.
Vulnerabilidade vem de enemy/equipment adapter.
Death/corpse não depende só da cena atual.
Cave contract usa quest system.
```

---

# WAVE 11 — Bestiary / Knowledge Discovery

## Objetivo

Implementar descoberta de conhecimento sem entregar UI completa de bestiário ainda.

## Specs núcleo

| Ordem | Spec | Tipo | Status alvo | Depende de | Bloqueia |
|---|---|---|---|---|---|
| 11.01 | `11_spec_bestiary_entry_data_contract_future.md` | data | núcleo/future hook | 10.05 | knowledge state |
| 11.02 | `11_spec_enemy_knowledge_state_save_load_future.md` | save/load | núcleo/future hook | 11.01, Save | persistence |
| 11.03 | `11_spec_bestiary_discovery_events_runtime_future.md` | runtime/events | núcleo/future hook | 10.06-10.09, 01.02 | discovery |
| 11.04 | `11_spec_equipment_tooltip_known_interactions_future.md` | UI/integration | núcleo/future hook | 05, 06, 11.02 | equipment UI |
| 11.05 | `11_spec_spell_tooltip_known_effectiveness_future.md` | UI/integration | núcleo/future hook | 09, 06, 11.02 | spell UI |
| 11.06 | `11_spec_bestiary_boss_spoiler_control_future.md` | rules | núcleo/future hook | 03, 10, 11.01 | boss spoilers |
| 11.07 | `11_spec_quest_bestiary_discovery_objectives_future.md` | quest integration | núcleo/future hook | 03, 11.03 | research quests |

## Specs futuras

```text
11_spec_bestiary_ui_menu_future.md
11_spec_bestiary_hud_notifications_future.md
11_spec_bestiary_npc_books_quest_unlocks_future.md
11_spec_bestiary_pet_companion_hints_future.md
11_spec_bestiary_research_service_future.md
```

## Regras obrigatórias

```text
Bestiary não revela tudo.
Bestiary não cria vulnerabilidade.
Equipment UI não revela fraqueza desconhecida.
Spell UI não revela resistência desconhecida.
Drop raro não descoberto não aparece completo.
Boss/main quest respeitam spoiler control.
Bestiary UI/HUD completa não entra na primeira entrega atual.
```

---

# WAVE 12 — Future systems

## Objetivo

Registrar specs futuras explicitamente fora da primeira entrega atual.

## Specs futuras

| Ordem | Spec | Tipo | Status alvo | Depende de | Observação |
|---|---|---|---|---|---|
| 12.01 | `12_spec_pet_core_runtime_future.md` | future | futuro | farm/city/save | pet base |
| 12.02 | `12_spec_pet_bond_hint_runtime_future.md` | future | futuro | 12.01, bestiary | pet hints |
| 12.03 | `12_spec_companion_core_runtime_future.md` | future | futuro | NPC/social/save | companion base |
| 12.04 | `12_spec_companion_active_cave_runtime_future.md` | future | futuro | 12.03, cave | 1 companion ativo |
| 12.05 | `12_spec_companion_farm_job_runtime_future.md` | future | futuro | 12.03, farm | jobs limitados |
| 12.06 | `12_spec_social_relationship_runtime_future.md` | future | futuro | NPC/dialogue/save | social base |
| 12.07 | `12_spec_romance_marriage_polycule_runtime_future.md` | future | futuro | 12.06 | romance/poliamor |
| 12.08 | `12_spec_partner_helper_runtime_future.md` | future | futuro | 12.06, 12.07, farm | helper limitado |
| 12.09 | `12_spec_festival_activity_minigames_future.md` | future | futuro | calendar/UI | minigames |
| 12.10 | `12_spec_bestiary_research_service_future.md` | future | futuro | bestiary/NPC | research service |
| 12.11 | `12_spec_endgame_level_100_101_progression_future.md` | future | futuro | cave/main/bestiary | endgame |
| 12.12 | `12_spec_final_choice_cinematic_future.md` | future | futuro | main quest/Fonte | finais |
| 12.13 | `12_spec_full_accessibility_settings_future.md` | future | futuro | UI | opções avançadas |

## Regras obrigatórias

```text
Não entram na primeira entrega atual.
Social/romance/poliamor não é power path obrigatório.
Partner helper não vira automação infinita.
Baseline de caverna continua 1 companion ativo.
Pets ajudam, mas não resolvem sistemas sozinhos.
Endgame não revela spoiler cedo.
```

---

## 6. Lista consolidada de specs por prioridade

### P0 — Criar primeiro

```text
00_spec_generation_roadmap_master.md
00_spec_wave_execution_protocol.md
00_spec_validation_matrix_master.md
01_spec_stable_ids_registry_runtime.md
01_spec_game_event_contracts_runtime.md
01_spec_save_restore_order_contract_runtime.md
01_spec_save_section_ownership_registry.md
01_spec_invalid_id_fallback_rules.md
```

### P1 — Fundação jogável

```text
02_spec_time_clock_day_transition_runtime.md
02_spec_calendar_season_year_runtime.md
02_spec_weather_generation_forecast_runtime.md
02_spec_lunar_cycle_event_runtime.md
03_spec_quest_objective_event_contract_runtime.md
03_spec_quest_condition_trigger_runtime.md
03_spec_quest_reward_application_idempotency_runtime.md
04_spec_ui_input_focus_modal_routing.md
04_spec_ui_menu_focus_stack_runtime.md
04_spec_ui_hud_main_gameplay_runtime.md
05_spec_inventory_item_instance_runtime.md
05_spec_equipment_slots_runtime.md
```

### P2 — Loop de fazenda/cidade/UI

```text
06_spec_ui_inventory_screen_flow_runtime.md
06_spec_ui_shop_buy_sell_screen_runtime.md
06_spec_ui_crafting_recipe_screen_runtime.md
06_spec_ui_quest_log_detail_runtime.md
06_spec_ui_calendar_day_detail_runtime.md
07_spec_crop_season_weather_rules_runtime.md
07_spec_farm_storage_shipping_bin_runtime.md
07_spec_crafting_recipe_station_runtime.md
07_spec_processing_jobs_runtime.md
07_spec_shop_stock_refresh_runtime.md
07_spec_economy_pricing_sell_buy_rules_runtime.md
08_spec_npc_identity_service_registry_runtime.md
08_spec_npc_schedule_runtime.md
08_spec_dialogue_runtime.md
08_spec_dialogue_choice_quest_hooks_runtime.md
```

### P3 — Player/cave/combat

```text
09_spec_player_core_stats_runtime.md
09_spec_stamina_mana_hunger_integration_runtime.md
09_spec_skill_tree_runtime.md
09_spec_active_skill_slots_runtime.md
09_spec_magic_spell_action_runtime.md
10_spec_cave_run_seed_state_runtime.md
10_spec_cave_floor_generation_runtime.md
10_spec_cave_monster_roster_data_runtime.md
10_spec_enemy_behavior_module_runtime.md
10_spec_enemy_vulnerability_runtime.md
10_spec_combat_player_actions_runtime.md
10_spec_death_corpse_recovery_runtime.md
```

### P4 — Integrações avançadas do núcleo

```text
03_spec_quest_state_save_load_runtime.md
03_spec_quest_log_visibility_spoiler_runtime.md
03_spec_quest_flags_registry_runtime.md
05_spec_hotbar_binding_runtime.md
05_spec_equipment_durability_runtime.md
05_spec_equipment_repair_upgrade_runtime.md
05_spec_tool_weapon_material_tag_runtime.md
05_spec_item_instance_save_load_runtime.md
06_spec_ui_equipment_screen_compare_runtime.md
06_spec_ui_weapon_armor_detail_drawer_runtime.md
06_spec_ui_fonte_menu_flow_runtime.md
09_spec_skill_respec_fonte_integration_runtime.md
10_spec_boss_gate_runtime.md
10_spec_cave_loot_resource_runtime.md
11_spec_enemy_knowledge_state_save_load_future.md
11_spec_bestiary_discovery_events_runtime_future.md
```

### P5 — Futuro explícito

```text
pets;
companions;
social/romance/poliamor;
partner helper;
bestiary UI/HUD completa;
research service;
festival minigames;
endgame 100/101;
final choice cinematic;
política final de save em caverna.
```

---

## 7. Primeiro lote recomendado de geração real

Gerar primeiro este lote, em ordem:

```text
1. 00_spec_generation_roadmap_master.md
2. 00_spec_wave_execution_protocol.md
3. 00_spec_validation_matrix_master.md
4. 01_spec_stable_ids_registry_runtime.md
5. 01_spec_game_event_contracts_runtime.md
6. 01_spec_save_restore_order_contract_runtime.md
7. 01_spec_save_section_ownership_registry.md
8. 01_spec_invalid_id_fallback_rules.md
```

Motivo:

```text
Sem IDs, eventos, save order e validation matrix, qualquer spec de domínio pode nascer incompatível.
```

---

## 8. Segundo lote recomendado

```text
1. 02_spec_time_clock_day_transition_runtime.md
2. 02_spec_calendar_season_year_runtime.md
3. 02_spec_weather_generation_forecast_runtime.md
4. 02_spec_lunar_cycle_event_runtime.md
5. 03_spec_quest_objective_event_contract_runtime.md
6. 03_spec_quest_condition_trigger_runtime.md
7. 03_spec_quest_reward_application_idempotency_runtime.md
8. 04_spec_ui_input_focus_modal_routing.md
```

Motivo:

```text
Tempo, quests e input/foco são os três eixos que mais afetam todo o resto.
```

---

## 9. Critérios para considerar o roadmap completo

Antes de gerar specs de domínio em massa, validar:

```text
Todas as waves têm dependências claras.
Toda spec tem wave e prioridade.
Toda spec futura está marcada como future.
Não há spec de gameplay sem direction fonte.
Não há spec de save sem section ownership.
Não há spec de quest sem quest system.
Não há spec de UI sem UI direction.
Não há spec de bestiary sem spoiler control.
Não há spec de time/lunar sem calendar source.
```

---

## 10. Pendências abertas deste roadmap

```text
Revisar nomes finais das specs antes de criá-las.
Decidir se algumas specs devem ser consolidadas para reduzir volume.
Atualizar docs/specs/README.md para apontar para este roadmap após aprovação.
Atualizar docs/specs/SPEC_EXECUTION_ORDER.md depois que as specs forem realmente criadas.
Atualizar registries somente quando specs concretas existirem.
Limpar numeração antiga do SPEC_SOURCE_MAP.md em uma passada documental separada.
```
