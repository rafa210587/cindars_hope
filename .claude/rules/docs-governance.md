# Rule: Docs Governance

Consolidates: `no-doc-delete-without-candidate`, `legacy-doc-paths-forbidden`, `decision-and-game-rule-policy`, `spec-source-of-truth` (originals are stubs pointing here).

## 1. No document deletion without candidate

No doc file may be deleted unless it appears in `docs/project/DOCUMENT_DELETE_CANDIDATES.md` AND the task explicitly authorizes deletion. Always allowed without pre-approval: moving to an archive folder (git-tracked), adding `status: delete_candidate`, adding to the candidates list.

## 2. Canonical paths only

Forbidden (never recreate or edit): `docs_old/`, `docs/00_PROJECT/` … `docs/07_RELEASES/`.
Canonical: `docs/project/`, `.specs/`, `docs/refinements/`, `docs/validation/`, `docs/backlog/`, `docs/architecture/`, `docs/decisions/`, `docs/game_rules/`, `docs/release/`.

The only active spec source is `.specs/` (queue in `a_implementar/`, done in `implementados/`). Never recreate root `specs/` or `spec/`, and do not recreate `docs/specs/` (the spec tree was relocated to `.specs/` — see ADR-0015).

## 3. ADRs and game_rules are canonical

- Decisions live in `docs/decisions/ADR-NNNN-*.md` (the *why*); current behavior lives in `docs/game_rules/*.md` (the *what*).
- Amendments (`docs/amendments/`) are historical record only — never cite them as canonical.
- Read only the ADRs/game_rules cited by the spec (`required_adrs`, `required_game_rules`), not all of them.
- Conflicts: code vs. ADR/game_rule → stop and report (one of them is stale). Behavior change → superseding ADR.

## Enforcement

- Hook `protected-path-guard.ps1` (PreToolUse) blocks Edit/Write on `docs_old/`, legacy numbered folders, and root `specs|spec/`.
- Hook `stop-summary-check.ps1` (Stop) blocks task completion if forbidden paths changed.
- Hook `delete-guard.ps1` (manual, invoked by docs tasks) checks the candidates list.
- `tools/docs/validate_docs.ps1` checks legacy folder creation.
