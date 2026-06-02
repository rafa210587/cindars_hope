# Cindar's Hope Agent Rules

These rules are project invariants. Skills explain workflows; hooks provide checks; rules define what must not drift.

## Rule Index

**Context & Governance**
1. [Context Reading Policy](./context-reading-policy.md) — minimal context; no PROJECT_LOG by default
2. [Spec Promotion Requires Evidence](./spec-promotion-requires-evidence.md) — phase-gated closeout only
3. [No Premature Acceptance Claims](./no-premature-acceptance-claims.md) — no MVP/Play Mode PASS without evidence
4. [No Doc Delete Without Candidate](./no-doc-delete-without-candidate.md) — delete only from candidates list
5. [Spec Source Of Truth](./spec-source-of-truth.md) — only `docs/specs/`
6. [Decision and Game Rule Policy](./decision-and-game-rule-policy.md) — ADRs/game_rules canonical; amendments archived

**Code Architecture**
7. [No Runtime Global Search](./no-runtime-global-search.md) — no GameObject.Find at runtime
8. [Event Bus Only Gameplay Communication](./event-bus-only-gameplay-communication.md) — GameEventBus required
9. [Save DTO Simple Types Only](./save-dto-simple-types-only.md) — no Unity refs in save
10. [Cave Stable Run](./cave-stable-run.md) — stable-run contract for cave procedural

**Unity & Git Safety**
11. [Unity Validation Honesty](./unity-validation-honesty.md) — no overstating validation results
12. [Unity YAML Editing Policy](./unity-yaml-editing-policy.md) — no manual .unity/.prefab/.asset edits
13. [Generated Asset Evidence](./generated-asset-evidence.md) — asset generation requires evidence
14. [No Parallel Unity Batchmode](./no-parallel-unity-batchmode.md) — sequential Unity processes only
15. [No Unsafe Git](./no-unsafe-git.md) — no push/reset/clean without per-instance authorization
16. [No docs_old Edits](./no-docs-old-edits.md) — docs_old is read-only

## Application

- Apply these rules to every agent-run task in this repository.
- If a requested task conflicts with a rule, pause and request explicit human authorization.
- If a rule is intentionally bypassed, record the reason in `PROJECT_LOG.md` and the relevant `docs/validation/*.md`.
