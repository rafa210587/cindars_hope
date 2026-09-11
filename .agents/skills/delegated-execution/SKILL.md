---
name: delegated-execution
description: Delegates a bounded slice with ownership and verifies artifacts, behavior and evidence before accepting the executor's result.
---

# Skill: Delegated execution

Reuses existing agents and the project's validation matrix.

**Core rule: verify the actual result; repeating a command is not an automatic condition
for trusting evidence already verifiable against the same inputs.**

## When to use

Delegated implementation, tooling, docs, asset wiring or bugfix work.

## Checklist before delegating

- [ ] Ownership and allowed/forbidden files defined.
- [ ] Tell the executor that other owners work in the repo; preserve prior dirty changes.
- [ ] Explicit acceptance and non-regression criteria.
- [ ] Relevant gates and tests selected through SPEC_VALIDATION_MATRIX_MASTER.
- [ ] Coordinate one owner for the actual Unity/build run on the integrated state.

## Procedure

1. Delegate a cohesive slice with minimal context. Do not require a build at every listed step.
2. Require created/edited files, commands, exit codes, log/XML paths and omissions.
3. Check artifacts and the diff, including small requirements and removals.
4. Inspect evidence: inputs, tool/configuration, result and executed cases.
   A PASS report alone is insufficient; so is an old XML file.
5. Rerun affected gates if later changes, uncovered integration, invalid/missing evidence
   or concrete suspicion justify it. Do not repeat builds just because the executor
   was a subagent. Later docs edits do not automatically invalidate C# tests.
6. Update the report with specific results and limits; scoped is not global.

## On-demand resources

When you need a reusable document, open the [template](assets/templates/delegation-packet.md).
To calibrate its contents, consult the [hypothetical example](references/examples/delegation-packet-example.md).
Do not load both by default; examples neither prove execution nor grant authorization.

## When NOT to use

A simple read-only search or discussion without a useful independent execution slice.

## When to stop and report

Scope conflicts with other owners; results differ from the report; a mandatory requirement
lacks evidence. Resolve within existing authorization and report pending work honestly.

## Related

- `(rule: subagent-results-not-evidence)`; `(rule: validation-truth)`.
- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`; `(skill: spec-execution)`.
