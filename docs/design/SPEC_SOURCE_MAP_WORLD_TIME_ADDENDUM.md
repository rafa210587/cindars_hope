# Cindar's Hope — Spec Source Map World/Time Addendum

> **Status:** addendum temporário de source map para tempo, calendário, clima e luas  
> **Local:** `docs/design/SPEC_SOURCE_MAP_WORLD_TIME_ADDENDUM.md`  
> **Fonte principal relacionada:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Fonte nova:** `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> **Função:** registrar imediatamente a rastreabilidade da nova direction enquanto o `SPEC_SOURCE_MAP.md` principal não for alterado via patch parcial seguro.  
> **Não é spec implementável.**

---

## 0. Regra de precedência

Este addendum deve ser tratado como extensão do `SPEC_SOURCE_MAP.md` até que o bloco equivalente seja aplicado diretamente no arquivo principal.

Toda spec que envolva tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, calor, frio, previsão, passagem de dia, sono, colapso por horário, day transition, festivais, eventos lunares, Alihana, Senya, Nyx, crops sazonais, crops lunares, Mana por estação/lua, Fonte reagindo a clima/lua, schedules dependentes de clima/lua ou eventos temporais deve ler obrigatoriamente:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

---

## 1. Specs de fazenda

Specs de fazenda que envolvam crops sazonais, chuva, irrigação, morte de planta por falta de água, clima, estufa, eventos sazonais, festivais agrícolas, fertilizante lunar, crops mágicas/lunares, Mana, Água Viva, Fonte reagindo a lua/clima ou day transition devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Exemplos de specs afetadas:

```text
spec_farm_crop_death_quality_fertilizers.md
spec_farm_weather_rain_irrigation_automation.md
spec_farm_animals_pasture_products_care.md
spec_farm_pets_dog_cat_bond_buffs.md
spec_farm_companion_jobs_automation.md
spec_farm_fonte_anya_living_water.md
spec_farm_mana_root_arcane_soil_endgame.md
spec_farm_festivals_orders_processing_future.md
```

---

## 2. Specs de cidade

Specs de cidade que envolvam calendário público, quadro público, festivais, horários, portas por horário, lojas por horário, restock ligado a calendário, NPC schedules modificados por clima/lua, loja noturna, rumores, eventos de praça, aniversários futuros, evento de Alihana/Senya/Nyx, Jardim das Estátuas reagindo a lua ou cidade mudando por clima devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Continuar lendo também:

```text
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
```

Regra:

```text
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md vence para rotina individual de NPC, camas, waypoints, prédios, interiores e portas.
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md vence para condições globais de tempo, clima, estação, lua e festival que schedules consomem.
```

---

## 3. Specs de caverna

Specs de caverna que envolvam modificadores por clima/lua, eventos de Alihana/Senya/Nyx, Pedra Negra mais ativa em Nyx, caverna alterada por névoa/tempestade, lagos subterrâneos brilhando, inscrições reveladas, inimigos noturnos, criaturas caóticas, eventos de memória, boss gate condicionado por lua ou nível 100/101 usando alinhamento lunar devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

---

## 4. Specs de player / tempo / save-load

Specs que envolvam relógio, tempo rodando, pausa em UI modal, sono, colapso, recuperação diária, Cansaço por horário/clima, Fome por tempo, day transition, save/load de tempo, CurrentDay, CurrentSeason, CurrentYear, CurrentTime, CurrentWeather, TomorrowWeather, ActiveLunarEvent, FestivalState ou WeatherSeed devem ler:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
```

---

## 5. Specs de economia / lojas / restock

Specs que envolvam restock diário/semanal/sazonal, preço por festival, demanda sazonal, mercador raro de Finan, loja noturna por Nyx, seed/crop raro por Alihana, item mágico instável por Senya, orders/encomendas por prazo ou SellPoint por day transition devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Regra:

```text
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md define quando eventos sazonais/climáticos/lunares podem solicitar variação.
ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md vence para preço, BaseValue, restock, SellPoint, stock state e anti-arbitragem.
```

---

## 6. Specs de quests

Specs de quests que envolvam condição temporal, calendário, estação, clima, previsão, festival, evento lunar, Alihana, Senya, Nyx, Fonte reagindo a lua/clima, Água Viva recarregando por lua, Mana florescendo por condição temporal, diário legível por Alihana, loja noturna por Nyx, ritual instável por Senya ou main quest condicionada por alinhamento lunar devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regra:

```text
Quest obrigatória não deve depender de evento raro sem calendário, pista e forma razoável de esperar.
```

---

## 7. Specs de pets e companions

Specs de pets/companions que envolvam reação a clima, estação, hora, lua, caverna, estado da Fonte, NPC visitante, quest state, trabalho externo em tempestade/neve, gato reagindo a Nyx, pet abrigo em chuva/neve, companion job com fallback climático ou partner helper respeitando clima devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

---

## 8. Specs de UI/HUD/calendário

Specs que envolvam relógio na HUD, calendário, clima atual, previsão, evento lunar conhecido, festival, evento de calendário, loja fechada/aberta por horário, quest esperando condição temporal, Fonte reagindo a lua/clima ou spoiler control de lua/clima devem ler:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

## 9. Specs futuras recomendadas

```text
spec_time_clock_day_transition_runtime.md
spec_calendar_season_year_runtime.md
spec_weather_generation_forecast_runtime.md
spec_rain_irrigation_crop_integration.md
spec_weather_farm_pet_companion_reactions.md
spec_lunar_cycle_event_runtime.md
spec_lunar_fonte_mana_reactions_future.md
spec_calendar_festivals_events_runtime.md
spec_npc_schedule_weather_lunar_modifiers.md
spec_shop_calendar_weather_lunar_modifiers.md
spec_cave_weather_lunar_modifiers_future.md
spec_calendar_ui_weather_lunar_display.md
spec_time_save_load_state.md
spec_calendar_quest_temporal_conditions.md
```

---

## 10. Anti-regressão

```text
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md é fonte canônica de tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, eventos lunares, Alihana, Senya, Nyx, festivais e impactos sistêmicos globais.
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md continua vencendo para rotina individual de NPC.
UI_UX_FULL_GAMEPLAY_DIRECTION.md continua vencendo para apresentação de relógio, calendário, clima, lua, notificações e feedback visual.
ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md continua vencendo para preço, BaseValue, restock, SellPoint e anti-arbitragem.
O jogo usa 4 estações por ano, 28 dias por estação, 112 dias por ano e 7 dias por semana.
Dia jogável começa às 06:00.
02:00 é limite padrão de colapso/sono forçado.
Escala inicial recomendada: 1 hora in-game = 60 segundos reais.
Tempo pausa em UI modal.
Chuva molha áreas externas.
As três luas são sistemas de gameplay, não decoração.
Alihana = memória, sonhos, Fonte, Cindar, Água Viva.
Senya = caos, magia, mutação, instabilidade.
Nyx = noite, segredo, loja noturna, Pedra Negra, esquecimento.
Mana pode depender de estação/lua/Água Viva/Fonte, mas nenhuma condição isolada basta.
Quest obrigatória com tempo/lua precisa dar pista e controle razoável ao jogador.
```

---

## 11. Bloco a aplicar no SPEC_SOURCE_MAP.md principal

Quando houver patch parcial seguro, aplicar este bloco diretamente no `docs/design/SPEC_SOURCE_MAP.md` principal, preferencialmente como nova seção antes da anti-regressão:

```md
# PARTE K — World / Time / Calendar / Weather / Lunar

## Specs de tempo, calendário, clima e luas

Fontes obrigatórias:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Specs que envolvam tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, calor, frio, previsão, passagem de dia, sono, colapso por horário, day transition, festivais, eventos lunares, Alihana, Senya, Nyx, crops sazonais, crops lunares, Mana por estação/lua, Fonte reagindo a clima/lua, schedules dependentes de clima/lua ou eventos temporais devem ler obrigatoriamente esta fonte.
```
