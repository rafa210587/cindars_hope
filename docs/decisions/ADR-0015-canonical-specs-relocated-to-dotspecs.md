---
doc_type: adr
status: accepted
adr_id: ADR-0015
title: Canonical Spec Tree Relocated from docs/specs to .specs
date: 2026-06-13
source_documents:
  - docs/project/DECISION_LOG.md
  - .claude/rules/docs-governance.md
  - tools/docs/validate_docs.ps1
supersedes: []
superseded_by: []
applies_to:
  - governance
  - documentation
  - spec-lifecycle
relates_to:
  - .claude/rules/docs-governance.md
  - .claude/rules/spec-source-of-truth.md
---

# ADR-0015 — Canonical Spec Tree Relocated from `docs/specs/` to `.specs/`

## Status

accepted (2026-06-13)

## Context

The repository carried **two** spec stores: `docs/specs/` (canonical — validated by
`tools/docs/validate_docs.ps1`, protected by the `protected-path-guard` / `stop-summary-check`
hooks, and mandated by the `docs-governance` rule) and a root-level `.specs/` working mirror
created during the 2026-06-12/13 refinement sessions. A byte-level audit confirmed `.specs/`
held **243 exact copies** of canonical files plus a byte-identical mirror of `fable_00C`, with
only the operational board / two audit indices as non-duplicated content (and even those already
existed canonically under `.specs/a_implementar/fable/fable_00*`). The mirror was **untracked in
git**, **unvalidated**, and **unprotected** — a second source of truth that had already drifted
(the 10 executed `fable` specs were marked "done" only in the mirror).

The project owner directed that the redundancy be resolved into a **single** spec home and chose
`.specs/` as that home (executed + to-execute in one place), with explicit awareness that this
required a harness migration and runs against the original `docs-governance` clause naming
`docs/specs/` as the only source.

## Decision

1. **Relocate the entire canonical spec tree** from `docs/specs/` to `.specs/` via `git mv`
   (history preserved; the move registered as 337 pure renames, R100). `docs/specs/` no longer
   exists. The internal structure is unchanged: `.specs/a_implementar/` (to execute),
   `.specs/implementados/` (executed), `.specs/_templates/`, and the `SPEC_*.md` governance docs.
2. **`.specs/` is now the single canonical spec source.** It is git-tracked, validated, and the
   path every tool reads.
3. **Rewire the harness**: a repo-wide `docs/specs` → `.specs` rewrite (412 files, 0 in code —
   only `.md`/`.ps1`) updated `validate_docs.ps1`, the hooks, the slash commands, the rules,
   `CLAUDE.md`, and `AGENTS.md`. `validate_docs.ps1` now asserts `.specs/` exists as the single
   official source. The `detect-change-scope` hook tracks `^\.specs/` for spec-migration detection.
4. **Forbidden paths unchanged**: root `specs/` and `spec/` remain forbidden (the guards use the
   bare names, which do not match the dotted `.specs/`). `docs/specs/` must **not** be recreated.

This **supersedes the `docs-governance` clause** that named `docs/specs/` as the only active spec
source; that clause now reads `.specs/`. Recorded here as the intentional, authorized governance
change the rule requires.

## Scope

- In scope: location of the canonical spec tree and all references to it across docs/harness.
- Out of scope: spec content, the spec lifecycle (`a_implementar` → `implementados` is unchanged),
  ADRs and game_rules (remain under `docs/decisions/` and `docs/game_rules/`), runtime code.

## Consequences

- **Positive**: one navigable home for executed + to-execute specs; the drift between mirror and
  canonical tree is eliminated; the single store is tracked, validated, and hook-protected.
- **Neutral**: the spec tree now lives in a dotfile directory (`.specs/`), which is less
  conventional than `docs/specs/` and can be easy to overlook in file explorers that hide
  dotfiles. The execution board lives at `.specs/a_implementar/fable/fable_00C_master_execution_plan.md`.
- **Cost paid**: a 412-file reference rewrite + a 337-file git rename; `validate_docs.ps1` exit 0
  re-confirmed after the move.
- **Risk retained**: any external tooling, bookmark, or documentation outside this repo that
  hard-codes `docs/specs/` will be stale; intra-repo references were all updated.

## Applies To

Governance, documentation structure, spec lifecycle, the docs validator and the path guards.

## Source Documents

- `docs/project/DECISION_LOG.md` — decision index (ADR-0015 row)
- `.claude/rules/docs-governance.md` — canonical-paths clause (now `.specs/`)
- `tools/docs/validate_docs.ps1` — gate (now asserts `.specs/`)
