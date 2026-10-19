---
name: editmode-test-authoring
description: Write focused NUnit EditMode tests for observable domain contracts, deterministic logic, save compatibility and regressions. Reuse existing domain fixtures; no test-per-class quota.
---

# Skill: EditMode test authoring

Tests live in `Assets/_Game/Tests/EditMode/<Domain>/`, never `Scripts/`. Inspect the closest
relevant fixture/assembly and reuse its conventions; test counts and domain lists are not fixed.

## Choose the behavior and oracle

1. Map the changed acceptance criterion to an observable result and failure case. Reuse a
   covering test rather than duplicating it for each extracted class.
2. Compute expected values from the contract or a reviewed example, not the same production
   algorithm. Test real rule logic; replace external clock/RNG/I/O through existing seams.
3. Control initial state, clock/ticks, seed and ordering. No real sleeps, current date,
   probabilistic assertions or dependency on previous tests for pure logic.
4. Assert state plus relevant side effects: quantity/gold conservation, rejected operation
   without partial mutation, one reward after retry, committed state visible to observers.
5. Reset/dispose owned state and subscriptions in TearDown/finally, including on assertion
   failure. Do not globally reset unrelated runtime systems to make an integration test pass.

## Risk-specific cases

| Changed contract | Evidence |
|---|---|
| Transaction | Success, capacity/validation failure, rollback and event count/order |
| Save | Nontrivial state round-trip, missing/legacy/invalid input per real policy, recovery and idempotency |
| Formula/bounds | Boundary values, representative reviewed examples, overflow/rounding when relevant |
| Substitutable policy | Same observable contract for applicable implementations |
| RNG/time | Explicit inputs and replay; follow `rng-and-determinism` |
| Validator/tool | Valid fixture passes; deliberately invalid fixture fails, including missing inputs |

Pure tests cover services/DTO behavior. Editor asset tests may use Editor APIs in the correct
assembly; lifecycle, physics, scene transitions and actual input need selected PlayMode tests.
Do not call a method by reflection and claim the real player flow passed.

For a bugfix, demonstrate the regression fails before and passes after when feasible in an
isolated fixture/checkout; never revert concurrent edits to obtain that evidence. Otherwise
state the limitation. Inspect that the runner actually selected and executed the intended
tests; zero tests, ignored cases and missing XML do not prove acceptance.

Run `tools/unity/RunUnityEditModeTests.ps1` with the relevant filter. The Test Runner compiles;
do not demand a duplicate .NET build on equivalent inputs. Record command, outcome/counts,
exit code and artifacts in the location selected by `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.

Reference on demand: [Microsoft unit testing practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices).
