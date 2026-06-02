# Rule: Legacy Doc Paths Forbidden

## Rule

Legacy documentation paths must not be recreated or edited.

Forbidden patterns:
- `docs_old/`
- `docs/00_PROJECT/`
- `docs/01_PRODUCT/`
- `docs/02_ARCHITECTURE/`
- `docs/03_SPECS/`
- `docs/04_REFINEMENTS/`
- `docs/05_VALIDATION/`
- `docs/06_BACKLOG/`
- `docs/07_RELEASES/`

## Canonical Paths

Use only:
- `docs/project/`
- `docs/specs/`
- `docs/refinements/`
- `docs/validation/`
- `docs/backlog/`
- `docs/architecture/`
- `docs/decisions/`
- `docs/game_rules/`
- `docs/release/`

## Why

The project was consolidated during SPEC_DOCS_35-37.
Legacy paths increase context drift and cause agents to read stale documents.

## Validation

- `tools/docs/validate_docs.ps1` checks for legacy folder creation
- `.claude/hooks/doc-location-guard.ps1` warns on legacy path references

---

*Created: 2026-06-01 (SPEC_DOCS_39)*
