# Spec Execution — Testing Quality Gate Addendum

> Applies to every implementation task using `spec-execution`.

## Rule source

Read and follow:

```text
.claude/rules/testing-quality-gate.md
```

## Required report block

Every code-changing spec execution report must include:

```text
Testing Quality Gate
────────────────────
Changed runtime code: YES/NO
Changed deterministic logic: YES/NO
Changed Unity scene/prefab/asset wiring: YES/NO
Automated tests added/updated: YES/NO
Automated tests command: <command or NOT RUN>
Manual Play Mode scenario: <path or NOT REQUIRED>
Justification if no automated tests: <text or N/A>
Residual risk: <text>
```

## Status limits

```text
Runtime/gameplay spec without Play Mode automated or human scenario:
  max status BUILD_VALIDATED.

Deterministic runtime logic without automated tests or explicit justification:
  max status PARTIAL.

Bugfix without regression test or explicit residual risk:
  max status PARTIAL.
```

## Minimum expectations

```text
Pure deterministic logic:
  EditMode tests when practical.

Gameplay/UI/scene/prefab flow:
  PlayMode automated test or human scenario.

Save/load:
  defaults, invalid IDs, no Unity refs, and round-trip behavior when practical.

Quest system:
  conditions, triggers, rewards, flags, spoiler visibility and save/load when relevant.

Economy/combat/status/skills:
  formula and rule tests when logic is deterministic.
```

## Closeout rule

Do not report `ACCEPTED` unless required build, Unity, automated test, Play Mode/manual scenario, and residual-risk evidence are present for the type of change.
