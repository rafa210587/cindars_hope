# Testing Quality Gate — Agent Operational Addendum

> Applies to all agents implementing specs in `docs/specs/a_implementar/`.

## Canonical rule

```text
.claude/rules/testing-quality-gate.md
```

## When it applies

Apply this addendum whenever a task changes:

```text
C# runtime code;
Editor code;
Unity tests;
scene/prefab/asset wiring;
save/load;
quest logic;
economy/inventory/equipment/combat formulas;
UI/modal/input flow;
bugfix behavior.
```

## Required closeout evidence

Every code-changing execution report must include:

```text
Testing Quality Gate
Changed runtime code: YES/NO
Changed deterministic logic: YES/NO
Changed Unity scene/prefab/asset wiring: YES/NO
Automated tests added/updated: YES/NO
Automated tests command: <command or NOT RUN>
Manual Play Mode scenario: <path or NOT REQUIRED>
Justification if no automated tests: <text or N/A>
Residual risk: <text>
```

## Status cap

```text
No required tests and no justification:
  max PARTIAL.

Runtime/gameplay without Play Mode automated or human scenario:
  max BUILD_VALIDATED.

Compile only:
  not enough for ACCEPTED runtime/gameplay specs.
```

## Implementation note

This addendum exists because the project is entering spec-by-spec execution and needs explicit quality gates before broad runtime changes.
