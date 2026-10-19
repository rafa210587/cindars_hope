---
doc_type: adr
status: accepted
adr_id: ADR-0001
title: Canonical Documentation Structure
date: 2026-06-01
source_documents:
  - SPEC_DOCS_35
  - SPEC_DOCS_36
  - SPEC_DOCS_37
  - tools/docs/validate_docs.ps1
supersedes: []
superseded_by: []
applies_to:
  - documentation
  - governance
---

# ADR-0001 — Canonical Documentation Structure

## Status

**accepted**

## Context

Prior to SPEC_DOCS_35-37, the repository had:
- Numbered folder structure (docs/00_PROJECT, docs/03_SPECS, docs/04_REFINEMENTS, docs/05_VALIDATION, docs/06_BACKLOG)
- Legacy archives (docs_old)
- Scattered decision documents and amendments
- No single source of truth for rules vs. evidence vs. specs

This created confusion about where information should live, where to read from, and made it difficult to find canonical sources.

## Decision

All documentation must use **canonical non-numbered folder structure:**

- `docs/project/` — project governance (CURRENT_STATE.md, DOCUMENT_GOVERNANCE.md, DECISION_LOG.md, etc.)
- `.specs/` — spec source (a_implementar/, implementados/, _templates/, SPEC_EXECUTION_ORDER.md)
- `docs/refinements/` — refinement source (a_implementar/, implementados/, archived/, _templates/)
- `docs/validation/` — validation evidence (current/, playmode/, _templates/, reports)
- `docs/backlog/` — backlog tracking (current_backlog.md, post_mvp_backlog.md)
- `docs/architecture/` — architecture documents
- `docs/amendments/` — temporary, migrated amendments only (not canonical source)
- `docs/decisions/` — ADRs (Architecture Decision Records)
- `docs/game_rules/` — canonical game rules and gameplay invariants
- `docs/release/` — release documentation

**Forbidden:**
- Numbered folder structure (docs/00_PROJECT, docs/01_PRODUCT, etc.)
- Root-level `spec/` or `specs/` folders
- `docs_old/`
- Root-level SPEC_*_STATUS.md files

## Consequences

### Positive

- Single source of truth for each document type
- Clearer reading policy (agents know where to look)
- Easier to enforce via validation scripts
- Reduced ambiguity about document authority

### Negative

- Historical references to old paths need updating
- Migration work required (completed in SPEC_DOCS_35-37)

### Operational

- `tools/docs/validate_docs.ps1` enforces this structure
- `.claude/rules/` references canonical paths
- New specs/rules must follow structure
- Migration from amendments to decisions/game_rules follows this structure

## Applies To

- All documentation files
- All agent reading policies
- All validation checks
- Project governance

## Source Documents

- [SPEC_DOCS_35: Canonical Folder Consolidation](../../validation/spec_docs_35_phase0_canonical_documentation_folder_audit_matrix.md)
- [SPEC_DOCS_36: Final Root and Legacy Cleanup](../../validation/spec_docs_36_phase0_final_root_legacy_cleanup_audit_matrix.md)
- [SPEC_DOCS_37: Refinements and Specs Sweep](../../validation/spec_docs_37_phase0_refinements_specs_validation_sweep_audit_matrix.md)

## Migration Notes

- All numbered folders consolidated into canonical non-numbered equivalents (SPEC_DOCS_35)
- All legacy files cleaned from root and docs_old (SPEC_DOCS_36)
- All refinements/specs reorganized (SPEC_DOCS_37)
- Migration completed 2026-06-01

---

*Created: 2026-06-01*  
*Status: accepted*  
*Implemented by: SPEC_DOCS_35, SPEC_DOCS_36, SPEC_DOCS_37*
