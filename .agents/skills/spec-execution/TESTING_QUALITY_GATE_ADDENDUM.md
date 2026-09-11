# Spec Execution — Addendum do Testing Quality Gate

> Aplica-se a toda tarefa de implementação que usa `spec-execution`.

## Fonte da regra

Leia e siga:

```text
.claude/rules/testing-quality-gate.md
```

## Bloco obrigatório no report

Todo execution report de spec que muda código deve incluir:

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

## Limites de status

```text
Runtime/gameplay spec without Play Mode automated or human scenario:
  max status BUILD_VALIDATED.

Deterministic runtime logic without automated tests or explicit justification:
  max status PARTIAL.

Bugfix without regression test or explicit residual risk:
  max status PARTIAL.
```

## Expectativas mínimas

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

## Regra de fechamento

Não reporte `ACCEPTED` a menos que a evidência exigida de build, Unity, automated test, Play Mode/manual scenario e residual risk esteja presente para o tipo de mudança.
