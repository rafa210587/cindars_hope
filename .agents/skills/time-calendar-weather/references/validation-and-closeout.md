# Time system validation and closeout

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
