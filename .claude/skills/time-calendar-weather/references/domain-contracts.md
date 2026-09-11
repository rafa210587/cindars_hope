# Time, calendar and weather contracts

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
