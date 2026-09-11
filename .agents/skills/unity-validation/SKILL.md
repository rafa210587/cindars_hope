---
name: unity-validation
description: Obtains Unity compile, test and asset evidence according to scope, reusing verifiable results and recording pending validation.
---

# Skill: Unity validation

This skill drives verification, not implementation.

- One owner runs Unity. Do not start competing editor instances or kill the user's running editor session.
- All evidence claims must be sourced from real artifacts (commands, logs, TestResult XML, captures).
- Core rule: select gates from `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`; no status elevation without matrix coverage and evidence.
- Do not add fake PASS states; NOT RUN is explicit.

## When to use

Use for C#, lifecycle, scenes/prefabs, assets/import, settings, and runtime behavior.

## Checklist

- [ ] Scope, assemblies/configuration, and acceptance criteria are extracted from the current spec and matrix.
- [ ] Previous valid evidence is inspected; rerun only missing, invalidated, or risk-extended gates.
- [ ] Deterministic execution plan defined (seed, clock/source of randomness, startup state).
- [ ] Explicit TestFilter and output paths for each runner invocation.
- [ ] Distinguish compile, EditMode, PlayMode, Player/runtime, and human acceptance evidence.
- [ ] Source file hashes and TestResult XML with actual case counts are captured.
- [ ] Failures record test id, scene/input/state, first failing stack trace and assertion; transaction tests also compare state and events before/after failure.
- [ ] Tests cover behavior and failure boundaries; do not add tests that merely mirror setters or aliases.
- [ ] Unity availability checked and applicable headless runners attempted; missing UI capability is not a blocker.

## Procedure

When using an available live Editor bridge, apply [unity-mcp-operations](../unity-mcp-operations/SKILL.md)
for instance identity, ownership and reload recovery. MCP does not replace the evidence gates below.

1. Select gate IDs in matrix with GLOBAL/SCOPED context and expected evidence types.
2. Build deterministic preconditions before every behavior run (seed, clock, scene state, required assets preloaded).
3. For testable logic, run `tools/unity/RunUnityEditModeTests.ps1` with exact `-TestFilter` and scoped assemblies.
4. If behavior depends on scene/lifecycle/input/physics/animation/audio/UI state, run PlayMode equivalents.
5. For compile-only paths, run `RunUnityCompileValidation.ps1` and parse integrated scan once; do not rescan the same log.
6. For visual or UX-sensitive regressions, obtain real runtime captures/playback with available tools and inspect them. Record scenario, viewport, timing and reviewer; distinguish agent visual review from human acceptance. Missing native UI control does not prevent headless PlayMode or camera captures, but camera-only renders do not prove screen-overlay UI or keyboard interaction. Request human work only for acceptance that actually requires it and cannot be obtained through authorized tools.
7. Preserve command, editor version (`ProjectSettings/ProjectVersion.txt`), exit code, log paths, XML path, counts, seed/clock, and changed input list.
8. Reuse equivalent evidence only with explicit rationale; no redundant runs.
9. If evidence is missing, stale, or inconsistent: set status to NOT RUN or FAIL with residual risk.

## Expected output

Each output uses PASS / FAIL / NOT RUN / NOT APPLICABLE with:

- Gate ID(s) and scope
- Command/version/exit
- Log/XML path
- Total/failed test cases and evidence of freshness
- Source hash list or another verifiable identification of the actual code/tests/assets/settings inputs; command hash alone cannot identify those inputs
- Failure taxonomy for failed cases
- Explicit mention of whether acceptance is automated or human-reviewed

Runner semantics:

- Exit 2 = NOT RUN / preflight
- Exit 1 = execution/evidence failure
- For Test Runner claims, missing XML, zero cases or stale artifacts fail the evidence gate. Documentation omissions are corrected in the report, not mislabeled as test failures.

A visual capture file alone is not human acceptance.

## AC → Deterministic behavior → Runtime reality

- AC-first: validate every required acceptance criterion that is in scope.
- Behavior: control initial state, seed and clock where relevant; deterministic logic uses exact assertions, physics uses a specified timestep/tolerance. Do not require artificial RNG or clock injection in code that has neither.
- Reality: only consider PlayMode/scene lifecycle evidence when the claim touches runtime behavior.
- Review: an independent validation agent inspects the implementation, tests and artifacts for sufficiency on save/cross-system/UI risks; it reports findings without fixing runtime. Root resolves findings and validates the final integrated state. Human acceptance remains separate when the spec/matrix requires it.

## When NOT to use

Docs-only changes or semantically neutral comments that do not alter runtime inputs.

## When to stop and report

Lock, missing Unity availability, timeout, stale artifacts, or irreproducible runs.
Record command/reason/residual risk and leave status explicit.

## Related

- `(skill: unity-validation-triage)`; `(skill: implementation-closeout)`.
- `(rule: validation-truth)`; `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
