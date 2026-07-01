---
name: time-calendar-weather
description: Estende o eixo de tempo do mundo — day transition, clock, season/year, weather + forecast, lunar cycle e eventos/festivais por data — reusando os services existentes (TimeManager, GameTimeManager, GameCalendarService, WorldWeatherService, LunarCycleService, WorldEventResolver). Use em specs da wave 02_* (time_clock_day_transition, calendar_season_year, weather_generation_forecast, lunar_cycle_event, calendar_festivals_events, time_calendar_weather_lunar_save_state, rain_irrigation_crop_integration).
---

# Skill: Tempo, Calendário e Clima

O eixo de tempo já existe e é o source of truth do mundo: `TimeManager` guarda `CurrentDay`, `GameTimeManager` roda o relógio Day/Night e chama `AdvanceDay()`, e `GameCalendarService`/`LunarCycleService` derivam date/lua de `AbsoluteDay`. Antes de tocar qualquer coisa de tempo, audite o que já está aqui — não crie um segundo relógio, calendário ou lua.

## Quando usar

A spec mexe em:
- day transition / sleep / TAB advance / virada de fase Day↔Night;
- clock (hora-do-dia, normalized phase);
- season / year / day-of-week derivados do dia;
- weather generation + forecast (clima de hoje e de amanhã);
- lunar cycle / picos lunares nomeados;
- festivais e eventos de mundo por data;
- save state desse eixo (dia, fase, year, lua).

## Quando NÃO usar

- Crop growth/watering reagindo ao dia/chuva → use `crop-farming-systems` (esta skill só PUBLICA o tempo; a farm CONSOME).
- NPC schedule por hora → consome `GameTimeManager.CurrentHourOfDay`; não é desta skill.
- Novo HUD de calendário/relógio → use `ui-projection-pattern`.

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `docs/game_rules/time_rules.md` (se citado — contém a divergência documentada do ciclo lunar de 28 dias / Rule 6)

## Sistemas existentes (reusar, não duplicar)

| Conceito | Classe / arquivo | Papel |
|---|---|---|
| Dia absoluto | `CindarsHope.Core.Time.TimeManager` — `Assets/_Game/Scripts/Core/Time/TimeManager.cs` | `CurrentDay`, `AdvanceDay()` publica `DayStartedEvent`, `SetCurrentDay(int)` |
| Relógio Day/Night | `CindarsHope.Core.GameTimeManager` — `Assets/_Game/Scripts/Core/GameTimeManager.cs` | `CurrentPhase`, `CurrentPhaseNormalized`, `CurrentHourOfDay`, chama `AdvanceDay()` na virada para Day |
| Balance do tempo | `GameTimeBalanceSO` — `Assets/_Game/Scripts/Core/Data/GameTimeBalanceSO.cs` | `DayDurationSeconds`, `NightDurationSeconds` |
| Date derivado | `GameDate` (struct) — `Assets/_Game/Scripts/World/Calendar/GameDate.cs` | `FromAbsoluteDay`, `Year`, `CurrentSeason`, `DayInSeason`, `DayOfWeek` (28 dias/estação, 4 estações, 112 dias/ano) |
| Serviço de calendário | `GameCalendarService` — `Assets/_Game/Scripts/World/Calendar/GameCalendarService.cs` | espelha `GameDate` a partir de `TimeManager.CurrentDay` |
| Clima do dia | `WorldWeatherService` — `Assets/_Game/Scripts/World/Weather/WorldWeatherService.cs` | fixa clima no `DayStartedEvent`, publica `WeatherChangedEvent`, expõe `TomorrowWeather` (forecast) |
| Gerador de clima | `WeatherGenerator` (static) — `Assets/_Game/Scripts/World/Weather/WeatherGenerator.cs` | `GenerateWeather(GameDate)` determinístico |
| Ciclo lunar | `LunarCycle` (struct) + `LunarCycleService` — `Assets/_Game/Scripts/World/Lunar/` | fase lunar derivada de `AbsoluteDay` (28 dias) |
| Eventos/festivais | `WorldEventDefinitions` + `WorldEventResolver` — `Assets/_Game/Scripts/World/Events/` | tabelas de festivais/picos/eventos e resolução pura por `AbsoluteDay`/`worldSeed` |
| Save do eixo | `WorldTimeProvider` + `WorldTimeSaveData`/`CalendarSaveData` — `Assets/_Game/Scripts/World/`, `Assets/_Game/Scripts/Save/` | captura/restaura dia, year, fase lunar |

Se o conceito da spec já está na tabela, **estenda a classe existente**. Criar `WeatherSystem`, `CalendarManager` ou `MoonService` paralelos é `NEEDS_REWORK` (ver skill `system-reuse-audit`).

## Regra central: o dia é a única fonte de verdade

`AbsoluteDay` (em `TimeManager.CurrentDay`) é o eixo. Tudo o mais — season, year, day-of-week, fase lunar, clima, festival, pico, evento — **deriva** dele de forma pura e determinística:

- season/year/day-of-week → `GameDate.FromAbsoluteDay(day)` (não recalcular à mão);
- clima → `WeatherGenerator.GenerateWeather(GameDate)` (mesmo dia ⇒ mesmo clima);
- lua → `LunarCycle.FromAbsoluteDay(day)`;
- festival/pico/evento → `WorldEventResolver.ResolveDay(worldSeed, day)`.

Consequência prática: clima e eventos **não entram no save** — são re-deriváveis (ver `WorldWeatherService`: "Clima é re-derivável por dia — não entra no save"). O save persiste só o dia (e fase/year/lua quando aplicável).

## Determinismo (ligação com rng-and-determinism)

Qualquer randomness no eixo de tempo é seeded por dia/worldSeed — nunca `Guid.NewGuid()`, `DateTime.Now`, nem `UnityEngine.Random` sem seed. O `WorldEventResolver` já usa FNV-1a `StableHash($"{salt}|{worldSeed}|{absoluteDay}")` (mesmo algoritmo do `CaveLayoutStableHash`). Invariante: **mesmo dia + mesmo worldSeed ⇒ mesmo clima, mesma lua, mesmo festival/evento, mesmo após reload.** Ao adicionar weather/evento novo, derive da mesma forma. Ver `rng-and-determinism` para o padrão de salt por sistema (ADR-0005).

## Comunicação (ligação com event-bus)

Day transition e weather se propagam só via `GameEventBus`, nunca por chamada direta MonoBehaviour→MonoBehaviour:

```csharp
// AdvanceDay() publica:
GameEventBus.Publish(new DayStartedEvent(CurrentDay));        // Core/Events/DayStartedEvent.cs (struct, DayNumber)
// WorldWeatherService reage a DayStartedEvent e publica:
GameEventBus.Publish(new WeatherChangedEvent(CurrentDay, CurrentWeather)); // Core/Events/WeatherChangedEvent.cs (struct)
```

Quem precisa reagir ao novo dia/clima (farm, NPC, economy, cave) faz `GameEventBus.Subscribe<DayStartedEvent>` / `Subscribe<WeatherChangedEvent>` — não chama os services diretamente. Ver skill `event-bus-pattern`. Se introduzir um evento novo (ex.: `SeasonChangedEvent`), siga o padrão de `struct readonly` em `Core/Events/`.

## Interação com save (ligação com save-load-pattern)

Persistir **só** dia/year/fase lunar como simple types/IDs — nunca `GameObject`/`Transform`/`ScriptableObject` (rule: `save-dto-simple-types-only`). O padrão já existe:

- `CalendarSaveData.AbsoluteDayIndex` (int, default 1);
- `WorldTimeSaveData` → `CurrentDay`, `CurrentYear`, `CurrentLunarPhaseType` (int);
- `WorldTimeProvider.GetSaveData()` / `RestoreFromSaveData()` capturam e restauram via `SetCurrentDay` / `SetCurrentDate(GameDate.FromAbsoluteDay(...))` / `RestoreFromSaveData(absoluteDay)`.

Ao restaurar, o clima/eventos NÃO são lidos do save: são re-derivados do dia restaurado. Mantenha migrations backward-compatible (default seguro quando a section estiver ausente). Ver skill `save-load-pattern`.

## Testes (ligação com editmode-test-authoring)

Lógica de tempo é deterministic logic ⇒ EditMode tests obrigatórios (Testing Quality Gate). Cubra com NUnit em `Assets/_Game/Tests/EditMode/**` (ver skill `editmode-test-authoring`):

- `GameDate` / `LunarCycle`: boundaries de season/year/day-of-week e wrap de fase lunar (dia 1, dia 28, dia 112, dia 113);
- `WeatherGenerator`: mesmo `AbsoluteDay` ⇒ mesmo `WeatherType` (determinismo);
- `WorldEventResolver`: `ResolveFestival`/`ResolveLunarPeak` por `(Season, DayInSeason)`; `ResolveRandomEvent` estável para `(worldSeed, day)`; `NextKnownFestivals` em ordem cronológica;
- round-trip de save: capturar dia/year/lua → restaurar → clima/eventos re-derivados iguais.

O relógio em `GameTimeManager.Update()` (Day/Night phase, `CurrentHourOfDay`) depende de `Time.deltaTime`/Play Mode — para a parte de feel da virada, anexe human scenario em `docs/validation/playmode/<spec_id>_human_test_scenario.md`.

## Red flags (pare e reveja o escopo)

- Criar `WeatherManager`/`CalendarManager`/`MoonService` paralelo em vez de estender os services existentes.
- Persistir clima/festival/evento no save (são re-deriváveis — não entram).
- Usar `UnityEngine.Random`/`Guid.NewGuid()`/`DateTime.Now` para clima/lua/evento (quebra "mesmo dia ⇒ mesmo clima").
- Recalcular season/year à mão em vez de `GameDate.FromAbsoluteDay`.
- Propagar virada de dia/clima por chamada direta entre MonoBehaviours em vez de `GameEventBus`.
- Tratar o ciclo lunar de 28 dias como bug — é a divergência documentada (time_rules.md Rule 6); os 4 picos nomeados são ancorados por `DayInSeason` 7/14/21/28.
- Mudar o save schema do eixo de tempo sem migration backward-compatible.

## Onde se aplica

Wave 02_*: `time_clock_day_transition`, `calendar_season_year`, `weather_generation_forecast`, `lunar_cycle_event`, `calendar_festivals_events`, `time_calendar_weather_lunar_save_state`, `rain_irrigation_crop_integration`.

## Fechamento

Ao fechar uma tarefa de tempo, reporte:
- qual service existente foi estendido (e que nenhum paralelo foi criado);
- que season/year/clima/lua/evento continuam derivados puros de `AbsoluteDay`/`worldSeed`;
- o que entrou no save vs. o que é re-derivado;
- eventos publicados (`DayStartedEvent`/`WeatherChangedEvent`/novos) e quem os consome;
- evidência de EditMode tests + human scenario se a virada tem feel de Play Mode.

## Relacionados

- skill `crop-farming-systems` — consumidor primário (growth no day-transition, chuva rega)
- skill `rng-and-determinism` — clima/lua/evento seeded
- skill `save-load-pattern` + rule `save-dto-simple-types-only` — persistir dia, não objetos
- skill `event-bus-pattern` + rule `event-bus-only-gameplay-communication` — day-changed/weather-changed
- skill `editmode-test-authoring` — testes de deterministic logic
- skill `system-reuse-audit` — Phase 0 antes de criar qualquer service de tempo
