---
name: time-calendar-weather
description: Extend world clock, day, season, year, weather, forecast or lunar behavior. Use when time progression must remain canonical, deterministic, event-driven and save-safe.
---

# Skill: Time, calendar and weather

The canonical time owner advances world time and publishes transitions. Do not create a second
clock or let dependent systems derive conflicting day/season state.

## Essential workflow
1. Read the active spec and inspect current time, save and event contracts.
2. Define boundaries and ordering for minute/hour/day/season/year transitions, including large
   time skips, sleep and load. Publish each semantic transition once.
3. Read [domain contracts](references/domain-contracts.md) when changing determinism, event order,
   weather/forecast/lunar state or persistence. Load only the affected section.
4. Use [validation and closeout](references/validation-and-closeout.md) for deterministic
   rollover, save roundtrip and integration scenarios.
5. Keep balance/content outside MonoBehaviours, persist stable IDs/simple values, and restore
   state without replaying rewards or duplicating transition events.

Use `rng-and-determinism`, `event-bus-pattern`, `save-load-pattern` and
`editmode-test-authoring` only for their affected slices. Deliver changed contracts, transition
ordering, data/save impact, evidence and remaining runtime or human checks.
