# Cindar's Hope — Validation Matrix Master

> Canonical selection, evidence and status policy. Commands, skills and templates defer
> to this matrix for gate selection and evidence format; do not maintain parallel matrices.

## Selection by scope and risk

Each spec declares exactly one blocking acceptance gate in its existing validation section:
name the existing test, command or executed scenario, its relevant scope and expected result.
The gate must prove all acceptance criteria and include applicable risk checks below;
multiple necessary checks form that one acceptance decision, not additional blocking gates.
Other checks are informational. Record their failures honestly, without turning them into
independent acceptance blockers or adding new gate types, runners or status taxonomies.
Do not reclassify a failure affecting acceptance criteria or protected risks as informational.
For an existing spec with several gates, identify its single acceptance gate before execution
and preserve its acceptance criteria; do not automatically promote historical specs.

| Change | Acceptance evidence | Unnecessary without another applicable risk |
|---|---|---|
| Docs/harness | Reviewed diff; applicable parser/link/tool contracts with result in commit message; no `docs/validation` artifact | Unity, screenshots, separate report |
| Pure logic/bugfix | Relevant EditMode test result and command/exit code in commit message | Separate report, PlayMode without integration |
| Comments only | Reviewed diff; reuse prior evidence when semantic equivalence holds | New behavior tests |
| Runtime/wiring/UI/prefab/SO/import | Relevant executed test/smoke and one-paragraph report | Unrelated validators and duplicate compile |
| Scene with approved keyart | Relevant test/smoke, one-paragraph report and one capture against the approved keyart | Multiple captures, HTML index |
| Save/schema/providers | Applicable compatibility, migration, round-trip and restore checks; integration test if wiring/order changed | Unrelated domain smokes |
| Cave runtime/procedural | Seed/snapshot/replay/revisit checks; executed flow if transition/materialization/save changed | All cave smokes on every spec |
| Event contracts | Relevant payload/subscriber/lifetime tests; integration when publisher/lifecycle changed | Field-assignment tests |
| asmdef/package/settings/bootstrap | Relevant compile/dependency/composition tests; Player only if target/build requires it | Repeating equivalent evidence |
| Broad integration/release | Relevant integrated flows, full suite/global checks only when required by the actual scope | Release build after every edit |

Choose checks by changed inputs and concrete risks. Test Runner can prove compile and
behavior together. Do not create tests that merely mirror setters, lists or renames;
reuse relevant existing tests. Bugfix evidence must exercise the regression.
Screenshots are required only for scene work with approved keyart, limited to one capture.
A capture does not replace a behavioral test or prove unseen flows.

## Protected behavioral contracts

The single gate includes these checks whenever the changed scope affects them. Their
failure or missing evidence cannot be waived by selecting a cheaper gate or labeling them
informational. Preserve applicable secret scanning and prevent disclosure of secrets.

| Domain | Checks to retain when affected |
|---|---|
| Save/migration | Real-state round-trip; legacy/missing sections; migration compatibility; unknown IDs; no Unity references in DTOs; restore order; backup/atomicity/recovery |
| IDs/data integrity | Stable/unique IDs and reference resolution; no silent corruption, loss or partial mutation |
| Quest/rewards | Conditions/triggers; idempotency; flags/progress/save; anti-softlock and visibility |
| Inventory/economy/equipment | Quantities/capacity; failure without partial mutation; transactions/events; pricing/stock/durability |
| Cave | Revisited level in the same seed uses snapshot; ForwardExit/BackExit preserve content; new run follows the contract |
| UI | Open/close/Esc; focus; movement/combat blocked by modal; empty/error states |
| Combat/skills | Formulas, cooldowns, status, costs/gates and changed feedback integration |
| EventBus | Publish/subscribe; unsubscribe/lifetime; reentrancy/allocation when relevant |

Before/after characterization for a substantial refactor is meaningful evidence.
Do not reduce statistical samples merely because they are slow.

## Tooling semantics

`tools/docs/run_strict_validation.ps1` keeps its existing behavior:

- Without `-Gates`: GLOBAL, six checks (corruption, docs, build, architecture, diff, quality).
- With explicit `-Gates`: SCOPED; exit 0 plus `SCOPED_PASS` proves only that selection.
- `-ScopePath` requires explicit gates and task JSON with nonempty `changedFiles` and
  `allowedPaths` arrays of exact repo-relative paths; optional `reportPaths` must exist.
  Null entries are invalid. The manifest does not authorize Unity YAML edits.
- Exit 0 plus `GLOBAL_PASS` proves the global set. Exceptions, missing/invalid inputs
  and known failures remain FAIL; do not bypass legacy failures or inflate baselines.
- A global run is not required on each spec. Its unrelated failures remain visible;
  only failures belonging to the declared acceptance gate block that spec.
- Strict does not execute all Unity/Player/human tests; select the relevant checks explicitly.

Use `Invoke-UnityGeneratedProjectsBuild.ps1` when .NET is applicable; it discovers the
solution projects. .NET does not prove Unity import/lifecycle. Valid Test Runner output
can prove Editor compile and behavior once; do not repeat the wrapper's log scan.
`Test-ArchitectureRatchet.ps1` owns source debt, serialized GUID and predefined-assembly
name checks. Run once on relevant inputs. Invalid/missing/empty baselines never prove PASS.

## Evidence and reuse

Docs/harness use the diff and commit message; pure logic/bugfix use the EditMode result
and exit code in the commit message. Neither creates an artifact in `docs/validation`.
When a report is applicable, use one folder `docs/validation/<spec_id>/` and one report
of at most 60 lines; wiring needs only one paragraph. Include criteria/result, tested scope,
command/exit, actual artifact reference, status and genuine pending risk, without mandatory
section boilerplate. Do not create snapshots, backups, dated run folders or HTML indexes
by default. Keep runner-generated logs/XML in ignored tool output and reference them;
there is no requirement to duplicate or commit raw output.

For reuse, inspect real artifacts and record relevant input equivalence in the same evidence
location (commit message or report). Inputs include code/tests, dependencies, configuration,
defines, tools and affected assets/settings. HEAD/date alone is insufficient. Record the
actual test outcome/counts and enough configuration/version detail to identify the run.
A report-only edit does not invalidate code tests. Reexecute only when relevant inputs
change, evidence is missing/inconsistent or a new risk appears. Do not trust legacy
run-required-validations JSON or claim savings without measurement.

## Status and promotion

| Status | Meaning |
|---|---|
| CODE_COMPLETE | Implementation done; acceptance evidence may still be pending |
| BUILD_VALIDATED | Relevant compile proven; does not imply behavior or acceptance |
| UNITY_VALIDATED | Relevant Unity compile/validators passed; does not imply complete gameplay |
| PLAYMODE_VALIDATED | Required automated or human scenario actually passed |
| DEFERRED_TO_FINAL_HUMAN_VALIDATION | Integration scenario documented; human execution pending |
| ACCEPTED | Acceptance criteria and the single blocking gate passed, including applicable protected risk checks |
| PARTIAL | Implementation or acceptance evidence incomplete, with reason and risk |
| BLOCKED | Acceptance cannot proceed without a dependency, correction or decision |

Pending is not PASS. CONTRACT_ONLY, NEEDS_REWORK and BLOCKED_BY_DEPENDENCY_PENDING are
not approvals. Informational checks cannot justify a stronger validation claim than the
actual evidence. Docs-only can be ACCEPTED without inventing a Unity build. Physical spec
promotion follows `/finish-spec` and the wave protocol using this evidence policy.
Human validation may be grouped by flow/wave; link each relevant criterion to the scenario.
A written scenario is not execution. If it belongs to the acceptance gate, pending human
validation prevents ACCEPTED; do not ask for per-spec execution when the agreed flow is final.

## References

`SPEC_WAVE_EXECUTION_PROTOCOL.md`; `SPEC_IMPLEMENTABLE_TEMPLATE.md`;
`.claude/rules/validation-truth.md`; `.claude/skills/spec-execution/SKILL.md`;
`docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
