---
doc_type: adr
status: proposed
adr_id: ADR-0028
title: Unified Async Scene Loading Flow
date: 2026-07-03
source_documents: []
supersedes: []
superseded_by: []
applies_to:
  - scene-management
  - scene-transitions
---

# ADR-0028 — Unified Async Scene Loading Flow

## Status

**proposed** (draft — requires human decision before any implementation)

## Context

Reconfirmed 2026-07-03 via `Select-String -Pattern "SceneManager\.LoadScene|EditorSceneManager\."`
across `Assets/_Game/Scripts/**`: **zero occurrences of `LoadSceneAsync`** anywhere in the codebase
today. Scene loading today is a mix of:

- **Editor-time scene creation/opening**, via `EditorSceneManager` calls inside
  `Assets/_Game/Scripts/Editor/SceneCreation/**` scene-creator scripts (these are Editor-only
  tooling, not a runtime path, and are out of scope for this ADR).
- **Runtime scene transitions**, handled by the existing `SceneTransitionRouter` /
  `SceneTransitionRequest` / `SceneTransitionGate` / `SceneSpawnAnchor` system (built by
  WAVE_INTEGRATION_13), which — per this session's confirmation — does not currently call
  `SceneManager.LoadSceneAsync` anywhere; the actual scene swap mechanism it uses was not
  re-verified line-by-line in this doc-only session and should be confirmed directly by whoever
  implements this ADR (it may currently rely on a synchronous `SceneManager.LoadScene` internally,
  which is consistent with "zero `LoadSceneAsync` occurrences" being literally true).

The practical consequence: any scene transition today either loads synchronously (blocking the
main thread for the load duration, with no built-in progress/loading-screen hook) or the loading
behavior is inconsistent across the different transition call sites
(`Assets/_Game/Scripts/SceneManagement/ScenePortal.cs`,
`Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs`, and any cave-entrance/exit-specific
logic), since none of them share one designated async-loading code path.

## Decision (proposed, not accepted)

Introduce one unified async scene-loading flow, funneled entirely through the existing
`SceneTransitionRouter` contract (not a new parallel scene-loading system — this must go through
`system-reuse-audit` first and build on `SceneTransitionRequest`/`SceneTransitionResult`/
`SceneTransitionGate`, not replace them):

1. `SceneTransitionRouter` internally uses `SceneManager.LoadSceneAsync` (with
   `allowSceneActivation` deferred until the target scene's `SceneSpawnAnchor` resolution and any
   fade/transition-screen visual completes) instead of a synchronous load.
2. Every runtime scene transition call site (`ScenePortal`, cave entrance/exit, any future
   transition trigger) goes exclusively through `SceneTransitionRouter` — no direct
   `SceneManager.LoadScene`/`LoadSceneAsync` call from gameplay code, mirroring the same
   "one designated entry point" discipline the project already applies to `GameEventBus` for
   cross-system communication.
3. `SceneTransitionStartedEvent`/`SceneTransitionCompletedEvent` (already existing, per
   WAVE_INTEGRATION_13/23) become the canonical hook for UI loading-screen/fade visuals to react
   to, instead of any per-caller ad hoc coroutine.

### Alternatives considered

- **Do nothing (status quo):** synchronous scene loads are simple and have shipped every
  WAVE_INTEGRATION scene-transition slice so far without a reported hitch/freeze complaint; the
  cost is a lost opportunity for a loading-screen/progress-bar UX and a guaranteed main-thread
  stall proportional to target-scene size, which becomes more noticeable as scenes grow (e.g. the
  TownScene forest GameObject count discussed in ADR-0023).
- **Async loading, but implemented ad hoc per call site:** would fix the stall for whichever
  system is if migrated first, but perpetuates the current inconsistency (some transitions async,
  some not) unless every call site is migrated together.
- **Async loading centralized in `SceneTransitionRouter` (this proposal):** matches the project's
  existing single-entry-point philosophy (`GameEventBus`, `GameBootstrap`) and reuses the already
  built `SceneTransitionRequest`/`Result`/`Gate`/`SpawnAnchor` contract rather than introducing a
  second scene-loading concept.

## Cost

- Medium: `SceneTransitionRouter`'s internal load call changes from synchronous to async with
  deferred activation; every existing transition (`ScenePortal`, cave entrance/exit portals) needs
  re-validation that spawn-anchor resolution still happens at the right moment relative to scene
  activation, since async loading changes exactly when the new scene's objects become live.

## Risk

- `SceneSpawnAnchor`/`PlayerSpawnResolver` timing is currently built against a synchronous-load
  assumption (implicitly, since no async load exists yet); moving to async risks a race where the
  player spawn resolver runs before the new scene has fully activated, or a `*RuntimeBootstrap`
  (see ADR-0022) in the new scene fires before state (inventory, quest flags) has been restored
  into it.
- Every WAVE_INTEGRATION scene-transition human Play Mode checklist (13, 16, and related) would
  need re-execution after this change, since they currently validate the synchronous behavior.

## Requer decisão humana

**SIM** — motivo: this changes the runtime behavior of every existing scene transition
(FarmScene/TownScene/CaveScene), interacts directly with the already-fragile
`*RuntimeBootstrap` startup-order question (ADR-0022), and would require re-validating multiple
existing Play Mode checklists (WAVE_INTEGRATION_13/16 and any transition added since). It is a
pure UX/robustness improvement, not required by any current spec's acceptance criteria, so
priority and timing should be a human call.

## Source Documents

- `Assets/_Game/Scripts/SceneManagement/ScenePortal.cs`
- `Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs`
- `docs/validation/WAVE_INTEGRATION_13_*` reports (existing SceneTransitionRouter contract)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
