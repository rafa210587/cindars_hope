---
doc_type: game_rule
status: accepted
domain: documentation-governance
source_adrs:
  - ADR-0001
source_documents:
  - docs/decisions/ADR-0001-canonical-documentation-structure.md
last_reviewed: 2026-06-01
---

# Documentation Rules

## Purpose

Defines canonical folder structure, file organization, and source locations for project documentation.

---

## Canonical Rules

### Rule: Canonical Folder Structure

- **Rule:** All project documentation must exist under `docs/` with these canonical subfolders:
  - `docs/project/` — key files (CURRENT_STATE, ROADMAP, etc.)
  - `docs/specs/` — spec documents (a_implementar/, implementados/, closeout_mvp/)
  - `docs/refinements/` — refinement documents (a_implementar/, implementados/, archived/)
  - `docs/validation/` — validation reports and evidence
  - `docs/decisions/` — Architecture Decision Records
  - `docs/game_rules/` — canonical gameplay rules
  - `docs/backlog/` — backlog items and future work
  - `docs/architecture/` — architectural documentation
  - `docs/amendments/` — historical amendments (archived after migration)
  - `docs/release/` — release notes and changelogs

- **Applies to:** All documentation, no exceptions
- **Must NOT:** Create numbered folders (`docs/00_PROJECT`, `docs/01_SPECS`, etc.)
- **Validation:** validate_docs.ps1 forbids numbered folders

### Rule: Spec Locations

- **Rule:** 
  - Active specs: `docs/specs/a_implementar/` with prefix `SPEC_*_*`
  - Completed specs: `docs/specs/implementados/` (moved after acceptance)
  - MVP closeout specs: `docs/specs/a_implementar/closeout_mvp/` or `implementados/closeout_mvp/`
  
- **Must NOT:** Mix specs in other folders; do not treat refinements as specs
- **File naming:** `SPEC_NN_NAME.md` pattern (e.g., `SPEC_01_CORE_BOOTSTRAP.md`)
- **Source of truth:** Only `docs/specs/` contains active/historical specs

### Rule: Refinement Locations

- **Rule:**
  - Active refinements: `docs/refinements/a_implementar/`
  - Completed refinements: `docs/refinements/implementados/`
  - Old/pre-2026 refinements: `docs/refinements/archived/`

- **File naming:** `ref_*` prefix (e.g., `ref_pr170_cave_stable_run.md`)
- **Archival:** Refinements not used in current wave → archived (not deleted)

### Rule: Validation Evidence Locations

- **Rule:** All validation reports and evidence must be in `docs/validation/`
  - Phase 0 audits: `spec_*_phase0_*.md`
  - Phase 1 build logs: `spec_*_build.log` or `LAST_VALIDATION_STATUS.md`
  - Phase 2 Unity validators: `spec_*_phase2_*.md` or validator output logs
  - Phase 3 Play Mode checklists: `spec_*_phase3_*.md`
  - Status tracking: `docs/validation/current/LAST_VALIDATION_STATUS.md`

- **Must NOT:** Delete validation reports (they are immutable evidence)
- **Preservation:** Even after spec acceptance, validation reports remain in `docs/validation/`

### Rule: Decision and Game Rules Locations

- **Rule:**
  - ADRs: `docs/decisions/ADR-NNNN-*.md` pattern
  - Game rules: `docs/game_rules/*.md` (snake_case, except index/template)
  - Index files: `docs/project/DECISION_LOG.md`, `docs/game_rules/GAME_RULES_INDEX.md`

- **Applies to:** All architectural decisions and current gameplay rules
- **Must NOT:** Create ADRs in numbered folders; only `docs/decisions/`

### Rule: Backlog and Future Work

- **Rule:** Planned, not-yet-started work lives in `docs/backlog/`
  - Feature backlog: `docs/backlog/features.md`
  - Bug backlog: `docs/backlog/bugs.md`
  - Tech debt: `docs/backlog/tech_debt.md`

- **Applies to:** Anything beyond current active queue
- **Cross-reference:** CURRENT_STATE.md lists active queue; backlog is reference only

---

## No Numbered Folder Structure

**Prohibited Patterns:**

```
✗ docs/00_PROJECT
✗ docs/01_SPECS
✗ docs/02_REFINEMENTS
✗ docs/03_VALIDATION
✗ docs_old/
```

**Correct Structure:**

```
✓ docs/project/
✓ docs/specs/
✓ docs/refinements/
✓ docs/validation/
```

---

## Cross-References

- Specs cite required ADRs via `required_adrs: [ADR-0001, ...]` in frontmatter
- Specs cite required game_rules via `required_game_rules: [cave_rules.md, ...]`
- ADRs and game_rules are **not read by default**; agents read only cited ADRs/game_rules
- DECISION_LOG.md is the index for ADRs; GAME_RULES_INDEX.md is the index for game rules

---

## Related ADRs

- [ADR-0001: Canonical Documentation Structure](../decisions/ADR-0001-canonical-documentation-structure.md)
- [ADR-0003: Spec Lifecycle](../decisions/ADR-0003-spec-lifecycle.md)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/decisions/ADR-0001-canonical-documentation-structure.md*
