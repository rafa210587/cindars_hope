---
doc_type: adr
status: accepted
adr_id: ADR-0003
title: Spec Lifecycle
date: 2026-06-01
source_documents:
  - docs/specs/SPEC_EXECUTION_ORDER.md
  - docs/project/CURRENT_STATE.md
supersedes: []
superseded_by: []
applies_to:
  - spec-execution
  - spec-status-promotion
---

# ADR-0003 — Spec Lifecycle

## Status

**accepted** (governance decision for all spec execution)

## Context

Specs exist in multiple states: planned, in progress, code complete, validated, and accepted. Question: How should specs transition between states, and what evidence is required at each gate?

## Decision

**Specs follow a three-location lifecycle with phase gates.**

1. **Location: `docs/specs/a_implementar/`** (in queue or in progress)
   - Phase 0: Audited but not started
   - Phase 1: Code written, not yet validated

2. **Validation Gates** (must pass before promotion)
   - Phase 0: Audit complete; risks documented
   - Phase 1: dotnet build + docs validation PASS
   - Phase 2: Unity Editor validators PASS (if applicable)
   - Phase 3: Play Mode checklist PASS (if applicable)

3. **Location: `docs/specs/implementados/`** (accepted)
   - Only after all required phases for that spec complete
   - Marked with `status: accepted` in frontmatter
   - Evidence preserved in `docs/validation/` (never deleted)

4. **Closeout specs** (SPEC_*_CLOSEOUT)
   - Closeout specs must wait for Phase 2-3 completion of their source specs
   - Do not close out a feature without Phase 2-3 validation of its implementation
   - `closeout_mvp/` specs require Phase 2-3 of SPEC 1-29 before acceptance

5. **Batch 2 Specs** (blocked)
   - Specs blocked until human Phase 2-3 validation of SPEC 1-29 complete
   - Include: spec_14a*, spec_14b*, spec_enemy_ai*, spec_cave_runtime*, spec_ui_ux_full_gameplay*, spec_combat_movement*
   - Do not move Batch 2 specs to implementados until Phase 2-3 gate opens

## Implementation

- Spec execution follows `/implement-spec` workflow
- Phase 0 audit creates `docs/validation/spec_*_phase0_*.md`
- Phase 1 build creates `docs/validation/current/LAST_VALIDATION_STATUS.md`
- Phase 2-3 evidence preserved in `docs/validation/spec_*_phase2_*.md` or `spec_*_phase3_*.md`
- Spec promotion to `implementados/` happens in Phase 4 closeout, only after required gates pass
- Update `docs/project/CURRENT_STATE.md` to reflect promotion

## Consequences

- Specs in `a_implementar/` are not final; evidence is separate
- Specs in `implementados/` are accepted with complete evidence trail
- Closeout specs do not hide unvalidated implementations
- Batch 2 is explicitly blocked, preventing premature promotion
- Each spec has clear evidence trail: audit → build → validators → play mode

## Applies To

- Spec execution planning (which specs can run now)
- Spec promotion decisions (when a spec moves to implementados)
- Closeout specifications (when to close out features)
- Phase gates (validation requirements per spec type)

## Source Documents

- [SPEC_EXECUTION_ORDER.md](./../specs/SPEC_EXECUTION_ORDER.md) — execution order and dependencies
- [CURRENT_STATE.md](./../project/CURRENT_STATE.md) — active queue and status
- [ADR-0002: Agent Context Minimum](./ADR-0002-agent-context-minimum.md) — agents read spec, not historical status

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0004 (validation gates), ADR-0009 (MVP acceptance phase gates)*
