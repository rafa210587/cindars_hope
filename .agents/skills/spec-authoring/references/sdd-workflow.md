# Project SDD artifact contract

Adaptation of [GitHub Spec Kit](https://github.com/github/spec-kit), consulted 2026-09-09.
This is a local workflow contract, not a Spec Kit installation or a claim of CLI compatibility.

## Stages and authority

| Stage | Owns | Workflow |
|---|---|---|
| Refinement | Problem, evidence, alternatives, world fit, balance hypotheses, decisions | refinement-authoring |
| Spec | Observable requirements, scenarios, acceptance IDs, scope, exclusions | spec-authoring |
| Plan | Technical contracts, per-file edits, reuse, migration, ownership, validation strategy | spec-planning |
| Tasks | Ordered work, dependencies, acceptance traceability, evidence to obtain | spec-task-authoring |
| Consistency review | Contradictions, coverage, drift, readiness | spec-task-authoring plus relevant reviewer |
| Implementation | Authorized changes and task evidence | spec-execution / execute-spec-strict |
| Validation and closeout | Actual evidence and unresolved acceptance | validate-spec / finish-spec |

AGENTS, CURRENT_STATE, applicable rules and accepted ADRs already supply project principles.
Do not create a competing constitution. Resolve conflicts by the existing project hierarchy.
Clarification can recur at each stage. Existing authorization persists; routine technical
choices do not require renewed permission. Material unaccepted changes remain explicit proposals.

## Storage and compatibility

- Refinements stay in `docs/refinements/a_implementar/` and are not implementation authority.
- Canonical specs remain in `.specs/`. Never create root `specs/`, `spec/` or initialize
  another framework's directory tree as an incidental part of planning.
- Default to one canonical spec with distinct `Spec`, `Plan` and `Tasks` sections and a
  common revision. Deep-template sections can serve these roles without renaming them.
  Stage separation concerns responsibility, not mandatory file count.
- Draft packages with unresolved requirements/contracts stay outside the executable queue,
  e.g. `.specs/features_futuras/<batch>/`; label draft and implementation-not-authorized.
- A complete package enters `.specs/a_implementar/` only after its depth/readiness gate.
  Location and technical readiness do not themselves prove human approval for implementation.
- If separate plan/tasks files are warranted, link them explicitly from the spec, keep
  them out of the executable queue and register them as supporting documents, not new specs.
- Legacy complete specs stay valid. Do not migrate/reformat unrelated specs to adopt this flow.
- Preserve validator-consumed markers when preparing queue entries: `# /speckit.specify`,
  `# /speckit.plan`, `# /speckit.tasks`, dependency labels `Ordem de execucao`, `Depende de`,
  `Bloqueia`, and the current ADR/game-rule metadata schema. These are document markers,
  not evidence that the external CLI is installed. Check the live docs validator before promotion.

## Quality and change propagation

Give requirements/acceptance stable IDs, e.g. AC01. Each plan decision and task maps to them.
Record real source paths and label proposed APIs. No implementation task may decide an open
balance/lore requirement incidentally. Small maintenance uses proportional depth.

Requirement changes update the spec first, then affected plan/tasks; design changes update
the plan and dependent tasks. A stale stage is explicitly stale until reviewed. Preserve
task evidence that remains applicable, but never present old validation as current proof.
Before execution check coverage, dependency cycles, ownership conflicts, approval and
unresolved decisions. Expected PASS strings belong to criteria, not evidence status.
