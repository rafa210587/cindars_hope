# Human Play Mode Test Scenario — fable_71 Combat Feel Pass

> Spec: `fable_71_spec_combat_feel_pass`
> Status: DEFERRED_TO_FINAL_VALIDATION (human Play Mode; not run by agent)
> Validates the subjective "feel" and the runtime Time.timeScale / camera restoration that EditMode
> cannot exercise. EditMode covers the deterministic logic (clamp, decay, no-stack, event shape).

## Wiring prerequisites (human, Unity Editor)

1. Add a `CombatHitStopController` component to a persistent gameplay object (e.g. the GameBootstrap
   or a dedicated `CombatFeel` GameObject). No serialized field is required (default 50 ms).
2. Add a `CameraShakeController` component and assign its **Camera transform** serialized field to the
   main camera transform (or inject it at bootstrap via `SetCamera`). Leave amplitude/duration at the
   light defaults.
3. (Optional, endgame) Add a `HudSuppressionBroadcaster` near the combat feel object so phase 0
   (restore-all-HUD) is published on enable/disable. F43 owns the per-phase suppression.

## Scenario A — Hit-stop on posture break (CA-1)

1. Enter the cave, engage an enemy with a posture bar (any band enemy).
2. Land heavy/charged hits until the posture **breaks** (stagger + CoreExposed open).
3. EXPECT: a short freeze-frame (~40-60 ms) at the moment of the break — tactile "weight".
4. EXPECT: the game resumes immediately at normal speed; no permanent slow-down or freeze.
5. Break posture on several enemies in quick succession.
6. EXPECT: no compounding slow-down; hit-stops never stack and never leave the game frozen.

## Scenario B — Hit-stop respects pause/modal (CA-1 safety)

1. Open a modal/pause (inventory `I`, pause menu) — the clock freezes.
2. While paused, trigger combat events (if reachable) or simply close the modal.
3. EXPECT: closing the modal restores normal speed exactly (the hit-stop never "ate" the pause).
4. Disable the `CombatHitStopController` component mid-fight (or load a new scene).
5. EXPECT: `Time.timeScale` is restored to its previous value (game never stuck at 0).

## Scenario C — Screen shake on charged attack / boss phase (CA-2)

1. Release a fully **charged** player attack.
2. EXPECT: a light camera shake (subtle, brief), then the camera returns exactly to its origin.
3. Fight a cave boss to a HP threshold so it changes phase (`BossPhaseChangedEvent`).
4. EXPECT: a light shake on the phase change; camera returns to origin (no drift over the fight).

## Scenario D — Screen shake no-op without camera (CA-2)

1. Remove/clear the camera serialized ref on `CameraShakeController`.
2. Trigger a charged attack / boss phase change.
3. EXPECT: no crash, no movement; a single clear wiring warning in the Console
   ("no main camera wired ... shake is a no-op"). No global search fallback.

## Scenario E — HUD suppression restore (CA-3, shared with F43)

1. Enter and exit the endgame Archivist fight (F43).
2. EXPECT: the HUD (including the pre-existing floating damage numbers) is fully visible again on
   exit (phase 0 = restore-all). The numbers are SUPPRESSED/RESTORED, never recreated by this spec.

## Pass criteria

- Hit-stop felt on posture break; game never frozen permanently; pauses intact.
- Light shake on charged/boss moments; camera always returns to origin; no-op + log when unwired.
- HUD always returns to fully visible (phase 0) at fight boundaries.
