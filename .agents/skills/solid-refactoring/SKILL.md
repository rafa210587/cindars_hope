---
name: solid-refactoring
description: Refactors existing classes and boundaries with SOLID, preserving behavior and reducing the context AI needs. Use for mixed responsibilities, hidden dependencies or contracts that are difficult to test.
---

# Skill: SOLID refactoring and class context

The project already has a composition root, domain installers, save providers and projections;
reuse these seams before inventing layers.

**Core rule: extract the smallest cohesive responsibility with evidence of preserved behavior; counts of classes, interfaces or lines do not measure quality.**

## When to use

- Authorized refactoring of a class mixing rules, persistence, input, UI or lifecycle.
- Broad contracts, concrete cross-domain dependencies or incompatible substitutes.
- A change requires reading whole modules because ownership and dependencies are implicit.

## Checklist before editing

- [ ] Spec and allowed files identified; other sessions' dirty changes preserved.
- [ ] Responsibility, owner-held state and actual consumers identified.
- [ ] `system-reuse-audit` completed before any new type/contract.
- [ ] Observable success/failure/boundary cases defined; characterization test when needed.
- [ ] Serialized fields, GUIDs, API, schema and event/lifecycle order preserved.
- [ ] The pattern addresses existing variation; a smaller local solution was considered.

## Minimal reading

`AGENTS.md` or `CLAUDE.md`, `CURRENT_STATE.md`, the spec, seam files and consumers.
To locate declarations, use `tools/architecture/Get-ClassContext.ps1 -Type 'InventoryManager'`.
Filters `-Module` and `-IncludeTests` broaden scope; `-Summary` summarizes the inventory and
`-OutputPath` exports JSONL when needed. The index does not replace reading the code.
Do not read history, all classes or all patterns by default.

## Procedure

1. **Characterize:** describe inputs, results, failures, side effects and relevant ordering.
   Identify concurrent changes and existing tests; run relevant tests before extraction.
2. **Design the seam:** separate domain decisions from Unity adapters. Keep state
   and invariants cohesive; helpers must not receive the whole manager as a service locator.
3. **Apply SOLID concretely:**
   - SRP: extract independent reasons to change, not blocks based on size.
   - OCP: Strategy/polymorphism only for demonstrated variation; a stable switch may remain.
   - LSP: preserve pre/postconditions and failures; test implementations against the same contract.
   - ISP: narrow the contract to its consumer; do not artificially fragment a cohesive protocol.
   - DIP: inject required contracts/values; wire through the composition root/installers.
4. **Choose a pattern:** use the `non-regression-review` table and discovered precedents.
   Justify the problem it solves. No obligation to use a pattern in every class.
5. **Extract incrementally:** preserve external API, serialized fields, GUIDs,
   save schema and lifecycle. `GameEventBus` remains for gameplay communication between
   systems; local helpers/queries/policies are explicit dependencies, not new events.
6. **Document intent:** short XML on declarations when responsibility/invariants are unclear.
   Do not generate manual YAML per class, method lists, authors or dates.
   Structural facts come from the generated index; review confirms comments remain true.
7. **Validate the seam:** obtain relevant test/gate evidence through the canonical matrix;
   reuse verifiable results. Metrics are clues; a test mirroring code is not evidence.
8. **Review:** look for forgotten consumers, incorrectly inverted dependencies,
   changed event order and failure behavior. Report the change and residual risk.

## Validation

Select gates through `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`; strict GLOBAL and SCOPED
are distinct results. Do not repeat separate build/ratchet runs already proven by the gate.
Docs, infrastructure or test failures remain FAIL/NOT RUN with their cause; do not adjust a
baseline to hide a regression. Unity/Play Mode are separate levels. For Unity-facing changes,
use `unity-validation` and a human scenario when applicable.

## Expected output

- Responsibilities before/after; reused or extracted class/contract and rationale.
- Preserved consumers and invariants; any intentional change made explicit.
- Commands, exit codes, behavioral tests and Unity/save/wiring risks.
- Files actually changed; no token-savings claim without measurement.

## On-demand resources

When you need a reusable document, open the [template](assets/templates/behavior-seam-plan.md).
To calibrate its contents, consult the [hypothetical example](references/examples/behavior-seam-example.md).
Do not load both by default; examples neither prove execution nor grant authorization.

## When NOT to use

- A specific bug without obstructive structural debt: `bugfix` and the smallest correction.
- An unapproved feature: create/approve a spec before implementation.
- Formatting, renaming or repetitive headers only: do not start structural refactoring.

## When to stop and report

The contract requires changing schema/GUID/API outside scope, the spec conflicts with current
state, or affected behavior cannot be verified. Explain the concrete obstacle;
independent work already authorized can continue.

## Related

- `(rule: solid-and-ai-context)`; `(rule: unity-architecture)`; `(rule: code-minimalism-ladder)`.
- `(skill: system-reuse-audit)`; `(skill: monobehaviour-decomposition)`; `(skill: non-regression-review)`.
- `(agent: architecture-reviewer)`; `(agent: non-regression-auditor)`.
