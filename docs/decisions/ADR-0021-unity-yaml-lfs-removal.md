---
doc_type: adr
status: proposed
adr_id: ADR-0021
title: Unity YAML Out of Git LFS — Tracking Policy Change
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
  - docs/decisions/ADR-0008-unity-yaml-editing-policy.md
supersedes: []
superseded_by: []
applies_to:
  - git-tracking
  - unity-asset-editing
  - repo-history
---

# ADR-0021 — Unity YAML Out of Git LFS (Tracking Policy Change)

## Status

**proposed** (draft — requires human decision before any implementation; extends ADR-0008,
does not replace it)

## Relationship to ADR-0008

[ADR-0008](./ADR-0008-unity-yaml-editing-policy.md) governs **how** `.unity`/`.prefab`/`.asset`
YAML may be edited (Editor API vs. manual edit, with authorization/risk statements). This ADR is
a narrower, different question: **how Git stores those same files** — LFS pointer vs. plain
Git-tracked text/binary blob. It does not change ADR-0008's editing policy; it is an addendum
scoped to storage/tracking, and should be read together with it.

## Context

Reconfirmed 2026-07-03 via `.gitattributes`: Unity YAML files (`*.unity`, `*.prefab`, `*.asset`,
plus common binary asset extensions) are tracked via **Git LFS** today. LFS stores these files as
small pointer files in Git history and the actual content in a separate LFS store, fetched on
demand.

The trade-off surfaced by this session's convergence audit: LFS adds operational overhead (LFS
server/storage dependency, `git lfs pull` steps, potential quota/cost on hosted LFS, and slower
diffing since LFS blobs are not human-diffable even though Unity YAML is technically text). For a
single-developer or small-team project, plain Git tracking of these YAML files (many of which are
already diff-friendly text, e.g. small `.asset` ScriptableObjects) may be simpler to operate,
at the cost of a larger `.git` history for large binary-like assets (scenes, big prefabs) if they
are not moved to LFS-equivalent handling.

## Decision (proposed, not accepted)

Two candidate paths, **neither selected without human sign-off**:

1. **Option A — status quo (keep LFS):** no change. Lowest risk, zero migration cost.
2. **Option B — remove LFS tracking for Unity YAML:** migrate `.unity`/`.prefab`/`.asset` (and any
   other currently-LFS-tracked Unity file types) to plain Git tracking. This requires **rewriting
   how Git has stored these files historically** (LFS pointers vs. blobs) if a clean history
   migration is desired, or simply changing `.gitattributes` going forward and accepting that
   historical commits keep using LFS pointers (mixed history) if a rewrite is not authorized.

### Alternatives considered

- **Keep LFS (Option A):** no action; matches ADR-0008's current assumption. Selected as default
  unless the human explicitly authorizes Option B.
- **Full history rewrite to remove LFS (Option B, aggressive):** cleanest end state, but requires
  `git lfs migrate` or `git filter-repo`-class history rewrite — this is a destructive,
  irreversible-in-place operation on the entire repo history, explicitly gated by the
  `no-unsafe-git` rule (never run without per-instance human authorization).
- **Change `.gitattributes` only, going forward (Option B, conservative):** new commits stop using
  LFS for these types; old history keeps LFS pointers. Lower risk than a full rewrite, but leaves
  a permanently mixed-mode history and does not shrink the existing LFS store.

## Cost

- Option A: zero.
- Option B (conservative, no rewrite): low — `.gitattributes` edit + verify no CI/tooling assumes
  LFS smudge/clean filters for these paths; contributors on old clones keep working via existing
  LFS pointers for old commits.
- Option B (aggressive, full rewrite): high — requires coordinated re-clone by every contributor,
  invalidates all existing PR branches/forks, and is irreversible once pushed. Must not be run
  without explicit, per-instance human authorization per the `no-unsafe-git` rule.

## Risk

- Rewriting Git history is explicitly listed as a destructive operation requiring per-instance
  human authorization (`no-unsafe-git` rule) — no agent may perform `git lfs migrate`,
  `git filter-repo`, or equivalent without that authorization freshly given for this exact task.
- Mixed-mode history (conservative option) can confuse tooling that assumes "all `.unity`/
  `.prefab`/`.asset` are LFS pointers" — must audit CI/build scripts for that assumption before
  changing `.gitattributes`.
- Large binary scenes/prefabs tracked as plain Git blobs bloat `.git` size and slow `clone`/`fetch`
  for all contributors going forward if LFS is dropped without a replacement strategy.

## Requer decisão humana

**SIM** — motivo: (1) this changes repository-wide Git tracking policy affecting every
contributor's clone; (2) the aggressive option involves history rewrite, which is an irreversible,
explicitly-gated destructive git operation under the `no-unsafe-git` rule and must never be
executed by an agent without fresh, per-instance human authorization; (3) even the conservative
option changes a project-wide `.gitattributes` convention that ADR-0008 implicitly assumes today.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)
- [ADR-0008-unity-yaml-editing-policy.md](./ADR-0008-unity-yaml-editing-policy.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
