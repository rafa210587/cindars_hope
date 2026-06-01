# Current State — Cindar's Hope

> **Primary execution context for agents.** Read this + AGENTS.md + active spec.  
> **Do NOT read:** PROJECT_LOG.md, ROADMAP.md, GDD, old refinements (unless spec requires).

---

## Project Status

| Item | Status |
|------|--------|
| Branch (working) | `dev` |
| MVP code-complete | ✓ YES |
| MVP build-validated (C#) | ✓ YES — 0E/0W runtime + editor |
| Docs validation | ✓ YES — 14/14 |
| MVP Phase 2 (Unity validators) | ✗ NOT RUN — pending human execution |
| MVP Phase 3 (Play Mode) | ✗ NOT RUN — pending human execution |
| MVP final accepted | ✗ NOT YET |

---

## What to Read for Implementation Tasks

```
1. AGENTS.md
2. docs/00_PROJECT/CURRENT_STATE.md  (this file)
3. Active spec file
4. Source files referenced by the spec
5. Immediately prior validation report ONLY if listed as a dependency
```

## What NOT to Read by Default

```
- PROJECT_LOG.md  (use only for audit/reconciliation/regression)
- docs/00_PROJECT/ROADMAP.md  (planning only)
- docs/design/GDD_v2.7*.md  (product design; read only if spec cites it)
- docs/refinements/  (read only if spec is ambiguous)
- docs/specs/a_implementar/reorg/  (CLOSED — do not execute)
- docs/validation/spec_mvp_closeout_*  (evidence; read only if explicitly listed)
- docs/IMPLEMENTATION_STATUS.md  (broad status; read only for audit)
```

---

## Active Spec Queue

| Spec | Status | Notes |
|------|--------|-------|
| SPEC_DOCS_30 | IN PROGRESS | Context governance + doc reorg (this session) |
| SPEC_18-28 Phase 2-3 | PENDING HUMAN | Play Mode validation; requires local Unity Editor |
| SPEC_29 Phase 2-3 | PENDING HUMAN | Final acceptance; blocked on Phase 2-3 above |
| FASE 10+ | BLOCKED | Blocked until Phase 2-3 or explicit human decision to skip |

---

## Blockers

1. **Human Play Mode acceptance:** Phase 2-3 not yet executed for any SPEC_18-28  
2. **Doc reorg in progress:** SPEC_DOCS_30 creating governance structure (this session)

---

## Key File Locations

| What | Where |
|------|-------|
| Agent rules | `AGENTS.md`, `CLAUDE.md` |
| Active specs | `docs/specs/a_implementar/closeout_mvp/` |
| Validation evidence | `docs/validation/spec_mvp_closeout_*.md` |
| Execution protocol | `docs/operations/AGENT_EXECUTION_PROTOCOL.md` |
| MVP acceptance | `docs/release/MVP_ACCEPTANCE_REPORT.md` |
| Post-MVP backlog | `docs/backlog/post_mvp_backlog.md` |
| Last validation status | `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md` |
| Spec template | `docs/03_SPECS/SPEC_TEMPLATE.md` |
| Document governance | `docs/00_PROJECT/DOCUMENT_GOVERNANCE.md` |

---

## Do NOT Execute

```
- docs/specs/a_implementar/reorg/SPEC_00-12  (CLOSED per README_STATUS.md)
- docs/specs/a_implementar/spec_14a*.md      (covered by SPEC_24 closeout)
- docs/specs/a_implementar/spec_14b*.md      (covered by SPEC_24 closeout)
- docs/specs/a_implementar/spec_enemy_ai*.md (covered by SPEC_23 closeout)
- docs/specs/a_implementar/spec_cave_runtime*.md (covered by SPEC_24, also in implementados)
- SPEC_18-29 again  (already executed; Phase 0-1 complete)
```

---

## Conflict Resolution Rules

```
spec + roadmap conflict  → follow spec
spec + refinement conflict → follow spec
spec + CURRENT_STATE conflict → STOP and report inconsistency
PROJECT_LOG + CURRENT_STATE conflict → prefer CURRENT_STATE; report mismatch
```

---

*Last updated: 2026-06-01 (SPEC_DOCS_30)*  
*Next update: after Phase 2-3 human execution or next spec session*
