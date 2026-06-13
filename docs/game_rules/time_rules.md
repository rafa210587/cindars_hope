---
doc_type: game_rule
status: accepted
domain: world-time
source_adrs:
  - ADR-0006
  - ADR-0007
source_documents:
  - docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
  - docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Core/GameTimeManager.cs
  - Assets/_Game/Scripts/Core/Time/TimeManager.cs
  - Assets/_Game/Scripts/Core/Data/GameTimeBalanceSO.cs
  - Assets/_Game/Scripts/World/Calendar/GameDate.cs
  - Assets/_Game/Scripts/World/Calendar/Season.cs
  - Assets/_Game/Scripts/World/Calendar/GameCalendarService.cs
  - Assets/_Game/Scripts/World/Lunar/LunarCycle.cs
  - Assets/_Game/Scripts/World/Lunar/LunarCycleService.cs
  - Assets/_Game/Scripts/World/Weather/WeatherType.cs
  - Assets/_Game/Scripts/World/Weather/WeatherGenerator.cs
  - Assets/_Game/Scripts/World/Weather/WorldWeatherService.cs
  - Assets/_Game/Scripts/Save/CalendarSaveData.cs
last_reviewed: 2026-06-13
---

# Time Rules

> **This document is current operational behavior, not desired future state.**
> **Change this rule only via a new ADR or spec.**
>
> Referenced by fable_15 (time/clock/day-transition runtime), fable_16 (calendar/season/year
> runtime), fable_20 (calendar HUD/UI) and fable_37 (festivals / lunar events, night shop).
> Where the live runtime is narrower than the binding design direction, this rule states the
> implemented behavior first and marks the unimplemented design intent as **proposta a calibrar**.

## Purpose

Defines the global time systems of Cindar's Hope and their canonical numbers:

- the day/night cycle and how in-game time advances;
- the calendar (named seasons Semeio / Brasa / Véu / Gelo, the 28-day season, the 7-day week, the year);
- the three moons of Vaalara (Alihana, Senya, Nyx) and the lunar cycle;
- day transition / sleep / overnight processing;
- the single clock-pause gate that stops time whenever any modal / panel / dialogue / shop is open;
- temporal triggers for festivals and lunar events.

It centralizes **when and why the world changes over time**. It does not define **who/where/how**
each entity reacts — NPC schedules, crop catalogs, weather VFX, shop pricing and UI presentation are
owned by their sibling rules and directions (see Cross-References). This rule never duplicates them; it
only declares the global time/calendar/lunar contract those systems consume.

---

## Definitions and Terms

| Term | Meaning |
|---|---|
| **Phase** | The implemented unit of the day cycle: `Day` or `Night`. Tracked by `GameTimeManager` (`GamePhaseChangedEvent.GamePhase`). |
| **PhaseTimer** | Real-time seconds elapsed in the current phase. |
| **AbsoluteDay** | Monotonic day counter starting at 1; the single source from which season, weekday, year and lunar state are derived. `TimeManager.CurrentDay`. |
| **GameDate** | Pure value struct deriving `Year`, `CurrentSeason`, `DayInSeason`, `DayOfWeek` from `AbsoluteDay`. |
| **Season** | One of four named seasons. Enum order: Primavera/Verão/Outono/Inverno; canonical Vaalaran names Semeio/Brasa/Véu/Gelo (see Rule 4). |
| **Weekday (D1-D7)** | Day-of-week index 1..7; days have **no proper names** (decisão v2 6.1). |
| **Moon** | One of the three moons of Vaalara: Alihana (white), Senya (scarlet), Nyx (hidden). |
| **Lunar event / peak** | A dated lunar occurrence (e.g. a moon's seasonal peak) consumed by quests, festivals and the night shop. |
| **Day transition** | The overnight process triggered when a new `Day` phase begins (sleep / collapse / forced rest), advancing `AbsoluteDay` and running per-day world updates. |
| **Clock-pause gate** | The single condition in `GameTimeManager` that freezes the phase timer while any modal UI is active (decisão v3 4.1). |
| **proposta a calibrar** | Design intent from the binding direction that is **not yet implemented** in runtime code; numbers are placeholders pending a future spec/playtest, not current behavior. |

---

## Canonical Rules

### Rule 1 — Day/Night cycle (implemented runtime)

- **Rule:** The day cycle runs as a two-phase loop driven by `GameTimeManager`:
  - phases alternate `Day -> Night -> Day ...`;
  - the active phase advances by real time (`PhaseTimer += Time.deltaTime`);
  - when `PhaseTimer` reaches the phase duration, the phase flips;
  - crossing from `Night` to `Day` calls `TimeManager.AdvanceDay()` (this is the day transition, Rule 7).
- **Numbers (configurable, from `GameTimeBalanceSO`):**

  | Setting | Default | Source |
  |---|---|---|
  | Day duration | 10 real minutes (600s) | `GameTimeManager.cs:24`, `GameTimeBalanceSO.cs:9` |
  | Night duration | 5 real minutes (300s) | `GameTimeManager.cs:25`, `GameTimeBalanceSO.cs:10` |
  | Full cycle | 15 real minutes (900s) | `GameTimeBalanceSO.cs:17` (`FullCycleDurationSeconds`) |
  | Tick event interval | 1 real second | `GameTimeManager.cs:19` (`GameTimeTickEvent`) |

- **Fallback:** if `GameTimeBalanceSO` is not assigned, the manager logs a warning and uses the hard-coded 600s/300s defaults (`GameTimeManager.cs:24-25, 72-74`).
- **Events published:** `GameTimeTickEvent` (every ~1s), `GamePhaseChangedEvent` (on flip), `DayStartedEvent` (via `TimeManager.AdvanceDay`).
- **Applies to:** all real-time progression of farm work, town, cave, combat.

> **proposta a calibrar (NOT implemented):** the direction's literal in-game clock —
> playable window 06:00 -> 02:00, the schedule blocks (Morning 06:00-09:00 … Sleep 00:00-06:00),
> the baseline `1 in-game hour = 60 real seconds` and `1 playable day ≈ 20 real minutes`
> (direction §2.1-2.2) — is **not** in the runtime. The runtime exposes only the binary
> `Day`/`Night` phase, not an hour-of-day value. Treat any hour-of-day value as design intent
> pending a future spec; do not cite it as current behavior.

### Rule 2 — Time pause is a single gate on modal UI (decisão v3 4.1)

- **Rule:** The day clock **pauses whenever any modal / panel / dialogue / shop is open**. There is exactly **one gate** for this, in `GameTimeManager.Update`:
  - while `ModalManager.HasActiveModal` is true, the manager returns early — `PhaseTimer`, tick timer and phase transitions are all frozen (`GameTimeManager.cs:110-111`).
- **Why one gate:** decisão v3 4.1 (Stardew-style) consolidated the previously duplicated `MenuManager` × `PauseMenuController` pause logic into a single authority on `GameTimeManager`.
- **Time keeps running** in active gameplay with no modal: walking, farm work, cave, combat, short non-modal interactions (direction §2.3).
- **Constraint:** any new system that must pause time does so by opening a modal through `ModalManager`, never by adding a second, parallel pause flag. An explicit exception requires a spec.
- **Cross-ref:** modal stack, input blocking and Esc behavior are owned by `ui_modal_rules.md`. This rule only declares the time-side consequence (clock frozen while a modal is on the stack).
- **Applies to:** pause/system menu, inventory, equipment, skill tree, crafting, shop, dialogue, quest log, calendar, Fonte menu, cutscene and confirmation modals (direction §2.3 list).

### Rule 3 — Calendar structure (implemented, `GameDate`)

- **Rule:** Calendar quantities are fixed constants derived purely from `AbsoluteDay`:

  | Quantity | Value | Source |
  |---|---|---|
  | Days per season | 28 | `GameDate.cs:5` |
  | Seasons per year | 4 | `GameDate.cs:6` |
  | Days per year | 112 | `GameDate.cs:7` |
  | Days per week | 7 | `GameDate.cs:8` |

- **Derivation (all 1-based for display, 0-based internally):**
  - `Year = (AbsoluteDay - 1) / 112 + 1` (`GameDate.cs:17-22`);
  - `CurrentSeason = ((AbsoluteDay - 1) % 112) / 28` (`GameDate.cs:25-32`);
  - `DayInSeason = ((AbsoluteDay - 1) % 28) + 1`, i.e. 1..28 (`GameDate.cs:34-41`);
  - `DayOfWeek = ((AbsoluteDay - 1) % 7) + 1`, i.e. D1..D7 (`GameDate.cs:43-49`).
- **Weekday naming:** days **D1-D7 have no proper names** (decisão v2 6.1). UI shows the numeric/index weekday only.
- **Calendar authority:** `GameCalendarService` mirrors `TimeManager.CurrentDay` into a `GameDate` each frame and on restore; it never advances time itself (`GameCalendarService.cs:53-63`).
- **Matches:** direction §4.1-4.2 and §28 (4 seasons, 28 days, 112-day year, 7-day week).

### Rule 4 — Season names: canonical Vaalaran vs. enum (decisão v2 6.1)

- **Rule:** The **canonical, player-facing season names are Vaalaran**: **Semeio / Brasa / Véu / Gelo** (decisão v2 6.1), mapping in calendar order to spring / summer / autumn / winter.

  | Order | Canonical (v2 6.1) | `Season` enum value (`Season.cs`) | Common name |
  |---|---|---|---|
  | 0 | **Semeio** | `Primavera` | Spring |
  | 1 | **Brasa** | `Verao` | Summer |
  | 2 | **Véu** | `Outono` | Autumn |
  | 3 | **Gelo** | `Inverno` | Winter |

- **Implemented state (divergence — proposta a calibrar):** the runtime `Season` enum (`Season.cs:4-9`) still uses the common Portuguese names `Primavera/Verao/Outono/Inverno` from the early direction (§4.1 allowed placeholder names). The Vaalaran display names from decisão v2 6.1 are **not yet wired** into the enum or a display-name table. Until fable_20 lands the rename, code uses the enum names; the **binding canonical names for any new UI/string work are the Vaalaran four**.
- **Constraint:** the enum **order** is canonical and must not be reordered (saves and `GameDate` derivation depend on index 0=Spring..3=Winter). A rename must preserve order and add a display-name mapping, never change the integer values.

### Rule 5 — The three moons of Vaalara (lore canon + decisão v2 6.3)

- **Rule:** Vaalara has **three moons**, and the calendar **shows all three from the start of the game** (decisão v2 6.3). The moons are gameplay systems, not decoration (direction §8.1, lore §26).

  | Moon | Colour | Domains (lore §27-29, direction §9-11) |
  |---|---|---|
  | **Alihana** | white (branca) | memory, dreams, prophecy, silence, the Fonte, Água Viva, Cindar, rare/night seeds |
  | **Senya** | scarlet (escarlate) | chaos, magic, festivals, passion, mutation, instability, demand-driven prices |
  | **Nyx** | hidden (oculta) | night, secrets, death, shadow, cave, the night shop, cults, forgotten memory |

- **Visibility vs. understanding:** the player **knows the moons exist from the start**, but not all of their effects are explained up front; effects are revealed through progression (direction §8.3). Effects that **gate mandatory progression** must be discoverable, signposted or predictable — never opaque (direction §8.3, §21).
- **Cross-ref:** moon-driven economy, farm, cave and Fonte reactions are owned by `farm_rules.md`, `cave_rules.md`, `event_rules.md` and the economy direction; this rule only fixes moon identity, colour and the "shown from start" contract.

### Rule 6 — Lunar cycle (implemented runtime vs. design model)

- **Rule (implemented, `LunarCycle`):** lunar state is derived purely and deterministically from `AbsoluteDay`:
  - the cycle length is **28 days** (`LunarCycle.cs:5`, `DaysPerLunarCycle`), aligned with the season length;
  - the implemented runtime exposes **8 generic phases** (`NewMoon … WaningCrescent`, `LunarPhase.cs:4-13`), advancing ~every 3 days: `CurrentPhase = ((AbsoluteDay - 1) % 28) / 3` (`LunarCycle.cs:14-20`);
  - `DayInPhase` is 1..3 within each phase (`LunarCycle.cs:22-29`);
  - `LunarCycleService` mirrors `TimeManager.CurrentDay` into the cycle each frame and on restore; it never advances time (`LunarCycleService.cs:40-50`).

- **Divergence (proposta a calibrar):** the implemented `LunarPhase` enum is the **generic 8-phase Western model**, NOT the three named moons (Alihana/Senya/Nyx) of Rule 5. The named-moon model and its per-moon **peaks** are design intent not yet in code.
- **Design model for peaks (proposta a calibrar, direction §8.2):** within a 28-day season the intended cadence is **one peak per moon per season**:

  | Day in season | Lunar event (design intent) |
  |---|---|
  | 7 | Alihana minor |
  | 14 | Senya minor/medium |
  | 21 | Nyx minor/medium |
  | 28 | Seasonal / major lunar event |

  Baseline does **not** simulate real orbital astronomy; it uses dated lunar **events** (direction §8.2). Whether peaks are fixed-by-calendar or seed-based is an open pendency (direction §29).
- **Constraint:** lunar state must remain **re-derivable from `AbsoluteDay`** (no stored RNG, no wall-clock). This preserves determinism and lets save/load restore lunar state from the day alone (Rule 9).

### Rule 7 — Day transition / sleep / overnight processing

- **Rule (implemented):** a new day begins when the cycle flips from `Night` to `Day`. On that flip `GameTimeManager` calls `TimeManager.AdvanceDay()`, which increments `AbsoluteDay` and publishes `DayStartedEvent` (`GameTimeManager.cs:149-156`, `TimeManager.cs:23-33`). Subscribers (calendar, weather, lunar, farm, economy) react to that single event rather than calling each other.
- **Triggers (design intent, direction §2.4):** the day is meant to advance when the player **sleeps in a valid bed**, **collapses at the late-night limit**, a **narrative event** forces rest, or a **quest transition** occurs. The collapse-by-time / sleep-quality rules (direction §3.1-3.2: 21:00 normal, 00:00-02:00 reduced recovery, forced collapse after 02:00) are **proposta a calibrar** — the runtime currently advances the day purely by the Night->Day phase flip, with no hour-based collapse.
- **Overnight processing order (design intent, direction §2.4):** when the day turns, the world should process, in order: save (if allowed) -> crop growth -> water/irrigation -> plant death if unwatered -> shipping/SellPoint -> pending payments -> daily/weekly restock -> orders/commissions -> NPC schedules -> pets/companions -> next-day weather -> lunar cycle -> calendar events -> HP/MP/Stamina recovery -> Cansaço (fatigue) recovery/penalty -> Água Viva recharge if conditions allow -> light world flags. Each downstream effect is owned by its sibling system; this rule only fixes that they hang off the single day-transition event.
- **Constraint:** restock and other per-day effects fire **only on day transition**, never on opening a menu (anti-exploit, direction §26).
- **Cross-ref:** crop growth and daily farm reset details live in `farm_rules.md`; shipping/restock pricing in the economy direction; festival/lunar overnight effects in `event_rules.md`.

### Rule 8 — Festival and lunar-event temporal triggers (fable_37)

- **Rule:** Festivals and lunar events are **calendar-driven events**, not decoration (direction §22). They are addressed by `(Season, DayInSeason)` and may carry a time window. They can:
  - override/adjust NPC schedules and close some regular shops;
  - open temporary stalls or special stock;
  - alter prices/demand within anti-arbitrage limits;
  - expose quest hooks and social events.
- **Night shop (decisão v2 6.2 — binding):** the night shop is **not** open every night. It opens **only at a Nyx peak AND with the required quest state** (`pico de Nyx + quest`). This overrides any earlier "night shop opens every night" reading.
- **Discovery rule:** secret events do not appear on the calendar until discovered; the calendar shows only known events, known lunar events, festivals and forecasts (direction §4.3, §25). A **mandatory** quest must never depend on a rare event without a calendar hint and a reasonable way to wait for it (direction §21, §26).
- **Constraint (anti-exploit, direction §26):** lunar events and festivals must not grant infinite loot/money; sleeping repeatedly must not farm rare resources for free; the calendar must not be manipulable for infinite gold.
- **Cross-ref:** the concrete event/festival catalog, rewards and per-event behavior belong to `event_rules.md` and fable_37; this rule only fixes the temporal-trigger and night-shop gating contract.

### Rule 9 — Weather is re-derived per day, not persisted (implemented)

- **Rule:** Weather is **deterministic per day and not saved** — it is re-derived from `AbsoluteDay` on load (`WorldWeatherService.cs:9-12`).
  - implemented `WeatherType` has **4 values**: `Clear`, `Cloudy`, `Rainy`, `Stormy` (`WeatherType.cs:4-9`);
  - `WeatherGenerator.GenerateWeather` returns `(WeatherType)(AbsoluteDay % 4)` (`WeatherGenerator.cs:8-13`) — deterministic, no RNG;
  - `WorldWeatherService` fixes the day's weather on `DayStartedEvent`, exposes `CurrentWeather` and `TomorrowWeather`, and publishes `WeatherChangedEvent` (`WorldWeatherService.cs:34-39, 72-77`);
  - `Rainy` and `Stormy` count as **wet weather** (`WorldWeatherService.IsWetWeather`, line 29-32) — this is what farm watering integration keys off.
- **Divergence (proposta a calibrar):** the direction's wider 9-type set (Sunny/Cloudy/Rain/Storm/Fog/Wind/Heat/Cold/Snow, §6.1) and the season-weighted forecast tiers (§7) are **not** implemented; only the 4-type deterministic model exists. Seasonal weighting, fog/snow and multi-day forecast are future.
- **Cross-ref:** weather's effect on crops/watering is owned by `farm_rules.md`; VFX/SFX and forecast presentation by the UI direction. This rule only fixes that weather is day-derived and not persisted.

---

## Numbers Summary (canonical, code-backed)

| Concept | Value | Status | Source |
|---|---|---|---|
| Day phase duration | 10 real min (default, configurable) | implemented | `GameTimeBalanceSO.cs:9` |
| Night phase duration | 5 real min (default, configurable) | implemented | `GameTimeBalanceSO.cs:10` |
| Tick interval | 1 real s | implemented | `GameTimeManager.cs:19` |
| Days per season | 28 | implemented | `GameDate.cs:5` |
| Seasons per year | 4 | implemented | `GameDate.cs:6` |
| Days per year | 112 | implemented | `GameDate.cs:7` |
| Days per week (D1-D7, unnamed) | 7 | implemented | `GameDate.cs:8`, v2 6.1 |
| Season names (canonical) | Semeio/Brasa/Véu/Gelo | binding; enum not yet renamed | v2 6.1, `Season.cs` |
| Moons | 3 (Alihana/Senya/Nyx), shown from start | binding | v2 6.3, lore §27-29 |
| Lunar cycle length | 28 days | implemented | `LunarCycle.cs:5` |
| Lunar phases (runtime) | 8 generic phases | implemented (diverges from named-moon model) | `LunarPhase.cs` |
| Lunar peaks (1/moon/season: D7/D14/D21/D28) | as table | proposta a calibrar | direction §8.2 |
| Weather types | 4 (Clear/Cloudy/Rainy/Stormy) | implemented | `WeatherType.cs` |
| In-game hour clock (06:00-02:00) | n/a | proposta a calibrar | direction §2.1 |

---

## Edge Cases

- **No `GameTimeBalanceSO` assigned:** manager logs a warning and uses hard-coded 600s/300s defaults; time still advances (`GameTimeManager.cs:72-74`). Not a fatal error.
- **No `TimeManager` assigned:** manager logs a warning and the day **does not advance** (`GameTimeManager.cs:77-79`); phases still flip but `CurrentDay` stays 1.
- **Modal opened mid-phase:** the phase timer is frozen, not reset; closing the modal resumes from the same elapsed value (Rule 2; `GameTimeManager.cs:110-113`).
- **`DayStartedEvent` received directly:** `GameTimeManager.HandleDayStarted` resets to `Day` phase with `PhaseTimer = 0` (`GameTimeManager.cs:159-163`) — keeps the phase consistent when day advances are driven externally (e.g. sleep/load).
- **`AbsoluteDay` floor:** `GameDate`, `LunarCycle` and `CalendarSaveData` all clamp to a minimum of 1 (`GameDate.cs:14`, `LunarCycle.cs:11`, `CalendarSaveData.cs:11`); day 0 / negative days are impossible.
- **Day 1 boundary:** all derivations are `(AbsoluteDay - 1) % N` based, so day 1 is Year 1 / season 0 / DayInSeason 1 / DayOfWeek 1 / lunar phase 0 — never an off-by-one into the previous period.
- **Load with no calendar section:** `GameCalendarService.RestoreFromSaveData(null)` falls back to `AbsoluteDay = 1` (`GameCalendarService.cs:42-51`).
- **Mandatory progression gated by a rare lunar/weather event:** prohibited unless it is discoverable, signposted on the calendar, and waitable (direction §21, §26).

---

## Save / Load

**Persisted (canonical day source):**
- `GameTimeSaveData`: `CurrentDay`, `CurrentPhase` (0=Day, 1=Night), `PhaseElapsedSeconds` (`Save/SaveData.cs`, restored in `GameTimeManager.RestoreFromSaveData`, lines 93-103, clamped on restore).
- `CalendarSaveData.AbsoluteDayIndex` (`CalendarSaveData.cs:5`, clamped ≥ 1).

**Re-derived on load, NOT persisted (avoid duplicate sources of truth):**
- season, weekday, year — derived from `AbsoluteDay` via `GameDate`;
- lunar phase/cycle — derived from `AbsoluteDay` via `LunarCycle` (restored via `LunarCycleService.RestoreFromSaveData(absoluteDay)`);
- weather (current + tomorrow) — re-derived deterministically per day (`WorldWeatherService`); explicitly **not** saved (`WorldWeatherService.cs:11`).

**DTO discipline (ADR-0006 / `save_rules.md`):** all time/calendar DTOs hold simple types only (`int`, `float`, enum-as-int) — no Unity references. Schema changes must stay backward-compatible (missing section -> default day 1). The direction lists a richer future save set (TomorrowWeather, ActiveLunarEvent, KnownLunarEvents, FestivalState, WeatherSeed, PendingDayTransitionState, DayTransitionVersion, direction §23); those fields are **proposta a calibrar** and not yet persisted.

## Testing Expectations

Aligned with `.claude/rules/testing-quality-gate.md` (calendar/day-transition logic and lunar logic are deterministic → **EditMode tests mandatory**; scene-driven behavior → human Play Mode scenario).

**Automated (EditMode) — pure deterministic logic:**
- `GameDate` derivations across boundaries: day 1, day 28/29 (season roll), day 112/113 (year roll), weekday wrap D7->D1.
- `LunarCycle` phase derivation and `DayInPhase` across the 28-day boundary; re-derivation equivalence (same `AbsoluteDay` → same phase).
- `WeatherGenerator.GenerateWeather` determinism (same day → same weather) and `IsWetWeather` classification for all 4 types.
- Save round-trip: `GameTimeSaveData` / `CalendarSaveData` capture→restore, including the `AbsoluteDay` clamp and the missing-section default (day 1).
- Day-transition contract: Night->Day flip advances `CurrentDay` exactly once and publishes `DayStartedEvent` once.

**Play Mode / human scenario (scene + modal + lifecycle):**
- Clock-pause gate (Rule 2): open each modal type and confirm `PhaseTimer` is frozen and resumes on close; confirm there is no second pause path.
- Full day cycle in a live scene: phase flip events, HUD time update, weather change on day start.
- Night shop gating (decisão v2 6.2): opens only at a Nyx peak with the required quest state.

**Residual risk to track in any consuming spec:** the named-moon model (Rule 5) vs. the implemented 8-phase model (Rule 6), and the season enum rename (Rule 4), are documented divergences — a spec that wires lunar/season behavior must reconcile them rather than assume the direction's model is live.

---

## Open Questions / Pendencies

(From direction §29 — design pendencies, not current behavior.)

- Wire the canonical Vaalaran season names (Semeio/Brasa/Véu/Gelo) into a display table without changing enum order (fable_20).
- Replace or map the generic 8-phase lunar model onto the three named moons and their per-season peaks.
- Decide whether lunar events are fixed-by-calendar or seed-based.
- Implement the hour-of-day clock (06:00-02:00) and the late-night collapse / sleep-quality rules, or formally retire them.
- Expand weather to the full type set + seasonal weighting + multi-day forecast, if/when scheduled.
- Decide the richer day-transition / lunar / festival save fields (direction §23) when those systems become runtime.

---

## Source / Rationale

**Design directions:**
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md` (§2 time/clock/pause, §4 calendar, §5 seasons, §6 weather, §8-12 lunar, §22 festivals, §23 save, §26 anti-exploit, §28 closed decisions, §29 pendencies).
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md` (§26-29: the three moons as gameplay; moon colours Alihana=white, Senya=scarlet, Nyx=hidden, lines 554-556).

**Binding decisions:**
- `FABLE_DECISOES_RESPOSTAS_v2.0.md` 6.1 (seasons Semeio/Brasa/Véu/Gelo; days D1-D7 unnamed), 6.2 (night shop only at Nyx peak + quest), 6.3 (calendar shows the 3 moons from start).
- `FABLE_DECISOES_RESPOSTAS_v3.0.md` 4.1 (the day clock pauses in any modal/panel/dialogue/shop; single gate on `GameTimeManager`).

**Code (current behavior, file:line cited inline above):**
- `Assets/_Game/Scripts/Core/GameTimeManager.cs`, `Core/Time/TimeManager.cs`, `Core/Data/GameTimeBalanceSO.cs`.
- `Assets/_Game/Scripts/World/Calendar/GameDate.cs`, `Season.cs`, `GameCalendarService.cs`.
- `Assets/_Game/Scripts/World/Lunar/LunarCycle.cs`, `LunarPhase.cs`, `LunarCycleService.cs`.
- `Assets/_Game/Scripts/World/Weather/WeatherType.cs`, `WeatherGenerator.cs`, `WorldWeatherService.cs`.
- `Assets/_Game/Scripts/Save/CalendarSaveData.cs`, `Save/SaveData.cs` (`GameTimeSaveData`).

---

## Cross-References

**ADRs (the *why*):**
- [ADR-0006: Save Data Contracts (simple DTOs)](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) — time/calendar DTOs are simple types only; weather/lunar re-derived not persisted.
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) — `DayStartedEvent`, `GamePhaseChangedEvent`, `GameTimeTickEvent`, `WeatherChangedEvent` are the only coupling between time and downstream systems.

**Sibling game_rules (do not duplicate — defer to them):**
- [farm_rules.md](farm_rules.md) — crop growth per day, daily farm reset, weather/season effect on crops and watering.
- [event_rules.md](event_rules.md) — the GameEventBus contract these time events ride on; concrete festival/lunar event catalog.
- [ui_modal_rules.md](ui_modal_rules.md) — modal stack, input blocking and Esc behavior that drive the clock-pause gate (Rule 2).
- [cave_rules.md](cave_rules.md) — cave determinism; any lunar/weather cave modifier must respect the stable-run seed.
- [save_rules.md](save_rules.md) — DTO/versioning/migration discipline for the persisted day fields.

---

*Created: 2026-06-13 (fable_15/16/20/37 time/calendar/lunar contract)*
*Reconciled against live runtime (GameTimeManager, GameDate, LunarCycle, WorldWeatherService) and binding decisions v2 6.1-6.3 / v3 4.1.*
