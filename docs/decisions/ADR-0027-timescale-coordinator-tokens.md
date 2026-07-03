---
doc_type: adr
status: proposed
adr_id: ADR-0027
title: TimeScaleCoordinator with Tokens for Time.timeScale Writers
date: 2026-07-03
source_documents: []
supersedes: []
superseded_by: []
applies_to:
  - game-flow
  - combat-feel
  - ui-modal-stack
---

# ADR-0027 — TimeScaleCoordinator with Tokens for `Time.timeScale` Writers

## Status

**proposed** (draft — requires human decision before any implementation)

## Context

Reconfirmed 2026-07-03 by reading both files directly:

- `Assets/_Game/Scripts/Combat/Feel/CombatHitStopController.cs` writes `Time.timeScale` directly
  at lines 78 (`Time.timeScale = 0f;`, entering hit-stop) and 98/111 (`Time.timeScale =
  _restoreTimeScale;`, restoring on routine completion or forced restore), caching the prior value
  in a local `_restoreTimeScale` field before overwriting it.
- `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` writes `Time.timeScale` directly
  at lines 226 (`Time.timeScale = 0f;`, pausing on death-screen show) and 232 (restoring to
  `_timeScaleBeforeShow`), caching the prior value in its own local `_timeScaleBeforeShow` field.

Each controller independently snapshots-and-restores `Time.timeScale` using its own private field.
**If both were ever active at overlapping times** (e.g. a hit-stop triggers in the same frame the
player dies and the death screen opens), the second writer to restore would stomp the first
writer's intended value, because neither knows about the other — there is no shared coordinator
tracking "who currently owns the freeze" or "what should timeScale be once everyone who wanted it
frozen has released it." This is exactly the kind of bug that is easy to miss in isolated testing
(each system works fine alone) and only surfaces under a specific overlapping-trigger timing that
may not be exercised by either system's individual Play Mode checklist.

## Decision (proposed, not accepted)

Introduce a single `TimeScaleCoordinator` (or similarly named) service that owns all writes to
`Time.timeScale`, exposing a **token-based acquire/release** API:

```text
IDisposable token = TimeScaleCoordinator.RequestFreeze(reason: "HitStop");
// ... later ...
token.Dispose(); // releases this reason's hold
```

`Time.timeScale` becomes `0` while **any** token is held, and returns to `1` (or whatever the
coordinator defines as "normal") only when **all** tokens are released — solving the
multiple-independent-freezer overlap problem structurally, rather than relying on each caller to
remember the pre-freeze value correctly. `CombatHitStopController` and
`DeathScreenCanvasController` would each become one caller of this shared API instead of two
independent direct writers.

### Alternatives considered

- **Do nothing (status quo):** works today because hit-stop and death-screen-pause do not appear
  to have been observed overlapping in practice, per current validation reports; but this is
  fragile — any new system that also freezes time (a future pause menu, a cutscene system) adds a
  third independent writer with the same risk, compounding rather than resolving it.
- **Single shared boolean/counter instead of tokens:** simpler to implement than a full token API,
  but a plain "freeze count" without identifying *which* system holds a freeze makes debugging a
  stuck-frozen-game bug harder (no way to know which caller forgot to release). Tokens (disposable
  handles) make the release obligatory and traceable to the caller that acquired it.
- **Token-based coordinator (this proposal):** slightly more code than a bare counter, but gives
  each caller an explicit, hard-to-forget release point (`using` block or explicit `Dispose`) and
  a named `reason` for debugging, which fits the project's existing `observability-and-logging`
  preference for traceable wiring-error-style diagnostics.

## Cost

- Low-medium: one new small service class + migrating exactly 2 known call sites
  (`CombatHitStopController`, `DeathScreenCanvasController`) to acquire/release tokens instead of
  reading/writing `Time.timeScale` directly. Any future system that needs to pause time (pause
  menu, cutscenes) becomes a third caller of the same coordinator instead of a third independent
  writer.

## Risk

- Migration risk is concentrated in getting the token-release timing exactly right for the two
  existing callers — `CombatHitStopController`'s coroutine-based restore and
  `DeathScreenCanvasController`'s explicit dismiss-path restore both need to map cleanly onto
  "dispose the token at the same point the old code restored `Time.timeScale`," or timing-sensitive
  combat feel (`game-feel-checklist`) could regress.
- No automated test currently exercises the "hit-stop and death screen overlap" scenario (this is
  gameplay/UI timing, not deterministic logic), so this change should ship with a targeted Play
  Mode scenario for the overlap case specifically, not just re-running the two systems' existing
  independent checklists.

## Requer decisão humana

**NÃO** para a existência do problema (dois writers diretos e independentes de `Time.timeScale` é
um fato verificado no código, não uma interpretação). **SIM** para decidir se vale a pena investir
nisso agora (a sobreposição pode nunca ter sido observada em produção) versus esperar até que um
terceiro sistema de pause apareça e o risco fique mais concreto, e para aprovar a API exata do
coordinator antes da implementação.

## Source Documents

- `Assets/_Game/Scripts/Combat/Feel/CombatHitStopController.cs` (confirmed direct writer)
- `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` (confirmed direct writer)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
