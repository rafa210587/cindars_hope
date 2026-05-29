# Cindar's Hope Agent Rules

These rules are project invariants. Skills explain workflows; hooks provide checks; rules define what must not drift.

## Rule Index

1. [No Runtime Global Search](./no-runtime-global-search.md)
2. [Save DTO Simple Types Only](./save-dto-simple-types-only.md)
3. [Cave Stable Run](./cave-stable-run.md)
4. [Spec Source Of Truth](./spec-source-of-truth.md)
5. [No docs_old Edits](./no-docs-old-edits.md)
6. [Unity Validation Honesty](./unity-validation-honesty.md)
7. [Generated Asset Evidence](./generated-asset-evidence.md)
8. [No Parallel Unity Batchmode](./no-parallel-unity-batchmode.md)

## Application

- Apply these rules to every agent-run task in this repository.
- If a requested task conflicts with a rule, pause and request explicit human authorization.
- If a rule is intentionally bypassed, record the reason in `PROJECT_LOG.md` and the relevant `docs/validation/*.md`.
