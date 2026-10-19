---
doc_type: adr
status: proposed
adr_id: ADR-0022
title: Runtime Bootstrap Composition Root for Auto-Wiring Systems
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
  - .claude/skills/runtime-bootstrap-pattern/SKILL.md
supersedes: []
superseded_by: []
applies_to:
  - startup-order
  - runtime-bootstrap-pattern
  - manager-wiring
---

# ADR-0022 — Runtime Bootstrap Composition Root for Auto-Wiring Systems

## Status

**proposed** (draft — requires human decision before any implementation; reconciles with, does
not replace, the existing `runtime-bootstrap-pattern` skill)

## Relationship to `runtime-bootstrap-pattern`

The project already documents and uses the `*RuntimeBootstrap` idiom (skill
`.claude/skills/runtime-bootstrap-pattern/SKILL.md`): a system self-connects to a scene via
`[RuntimeInitializeOnLoadMethod]` instead of a runtime `FindObjectOfType`/`GameObject.Find` scene
search, which are forbidden by the `unity-architecture` rule. This ADR does **not** propose
removing that idiom — self-wiring via a static entry point remains correct. It proposes
**consolidating the entry points themselves**, i.e. how many independent
`[RuntimeInitializeOnLoadMethod]` call sites exist and in what order they run, not how each one
wires itself once invoked.

## Context

Reconfirmed 2026-07-03 via `Select-String -Pattern "RuntimeInitializeOnLoadMethod"` across
`Assets/_Game/Scripts/**`: **69 occurrences across 53 files** (close to the 70 cited by the prior
audit prompt; the small delta is expected drift since that count was taken and is not a discrepancy
worth investigating further for this ADR).

Each of these 53 files independently registers a method that Unity calls at
`RuntimeInitializeLoadType.AfterSceneLoad` (or `BeforeSceneLoad`, depending on the file). Today,
**the relative order between independent bootstraps is implicit** — it depends on Unity's
internal method-registration order, which is not contractually guaranteed to be stable across
Unity versions or even across domain-reload boundaries. If bootstrap B silently depends on
bootstrap A having already run (e.g. B reads a manager that A creates), that dependency is invisible
in code today and could break silently on a Unity upgrade or a refactor that changes file/method
ordering.

## Decision (proposed, not accepted)

Introduce a single **composition root** entry point (e.g. `GameRuntimeCompositionRoot`) that:

1. Is the **only** `[RuntimeInitializeOnLoadMethod]` in the project (or one of a very small,
   explicitly-ordered set — e.g. one for `BeforeSceneLoad` critical managers, one for
   `AfterSceneLoad` scene-dependent wiring).
2. Calls each existing `*RuntimeBootstrap`'s entry method **explicitly, in a declared order**,
   instead of each bootstrap self-registering via its own attribute.
3. Preserves every individual bootstrap's internal self-wiring logic (scene lookups, manager
   registration) — only the *invocation* moves from "automatic/implicit" to "explicit/ordered".

This directly implements the "sistema de apoio" pattern already implied by
`runtime-bootstrap-pattern`, just centralizing invocation instead of leaving 53 independent
implicit entry points.

### Alternatives considered

- **Do nothing (status quo):** no risk today, but any future bootstrap that has an undeclared
  order dependency on another is an invisible landmine; already-existing order dependencies (if
  any) are unverified.
- **Full composition root, single pass (this proposal):** requires touching all 53 files (change
  attribute-based registration to a method call from the root) in one coordinated change —
  meaningful regression surface, since a mistake in the declared order could reproduce or worsen a
  previously-invisible bug now made visible/exposed.
- **New bootstraps only, grandfather the existing 53:** lowest immediate risk — stop the bleeding
  by requiring new systems to register through the composition root, while leaving the 53
  existing self-registering bootstraps untouched until they need to move anyway. Weakest guarantee
  (order is still implicit for the pre-existing 53) but least regression risk.

## Cost

- Full consolidation: high — 53 files touched, each requiring careful verification that its
  self-wiring still fires at the correct relative time; every `*RuntimeBootstrap` human/Play Mode
  checklist referenced across `docs/validation/WAVE_INTEGRATION_*` reports would need re-validation
  since bootstrap timing is exactly what those checklists test.
- Grandfather-only: low — new composition root + convention doc; zero changes to the 53 existing
  files; risk is deferred, not eliminated.

## Risk

- **Silent regression risk is the primary concern.** Many `docs/validation/WAVE_INTEGRATION_*`
  reports document "Play Mode blocked/pending" specifically because these bootstraps wire scene
  state; changing invocation order without re-running every affected Play Mode checklist could
  reintroduce bugs that were fixed by the current (implicit) order, without any automated test
  catching it — most of this wiring has no EditMode coverage by nature (it needs a live scene).
- `RuntimeInitializeLoadType.AfterSceneLoad` ordering today may already encode unstated
  dependencies (e.g. `GameBootstrap` before an interactable's bootstrap) that only get discovered
  by making order explicit and comparing against current de facto behavior.

## Requer decisão humana

**SIM** — motivo: consolidating 53 independent auto-wiring entry points into one explicit-order
composition root is a startup-sequencing change with real regression risk to systems that are
largely validated only by manual Play Mode checklists, not automated tests. The human must choose
between full consolidation now vs. grandfathering the existing 53 and only enforcing the pattern
for new systems, and must schedule the Play Mode re-validation this implies.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)
- [runtime-bootstrap-pattern SKILL.md](../../.claude/skills/runtime-bootstrap-pattern/SKILL.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
