# SPEC_DOCS_30 Phase 0 — Document Inventory Audit Matrix

**Date:** 2026-06-01  
**Spec ID:** spec_docs_30_context_governance_and_document_reorg  
**Mode:** Audit Only — No file changes before matrix completion  

---

## Executive Summary

The repository has **~140+ documents** across `docs/` accumulated across FASE 1-9, architecture reorg (SPEC_04-12), and MVP closeout (SPEC_18-29).

Key findings:
- **Context risk:** PROJECT_LOG.md, IMPLEMENTATION_STATUS.md, SPEC_EXECUTION_ORDER.md can pull agents into reading large historical context
- **Drift risk:** Multiple specs in `a_implementar/reorg/` are CLOSED but still in active-looking folders
- **No delete candidates that are unsafe:** Nothing should be deleted in this spec
- **New structure needed:** `docs/00_PROJECT/` as execution context hub, templates, validation governance

---

## 1. Document Inventory by Type

### 1.1 agent_instruction

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| AGENTS.md | Execution context | KEEP | Add Context Reading Policy section |
| CLAUDE.md | Execution context | KEEP | No changes needed |
| docs/operations/AGENT_EXECUTION_PROTOCOL.md | Execution protocol | KEEP | Reference from CURRENT_STATE.md |
| docs/operations/READING_MATRIX.md | Reading guidance | MEDIUM | Reference from CURRENT_STATE.md |
| docs/operations/LLM_HANDOFF_INSTRUCTIONS.md | Handoff guidance | LOW | Archive candidate post-Phase 3 |

### 1.2 project_status

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/IMPLEMENTATION_STATUS.md | Broad status | MEDIUM | Add governance header; point to CURRENT_STATE.md |
| docs/release/MVP_ACCEPTANCE_REPORT.md | MVP acceptance | KEEP | Already reconciled in SPEC_29B |
| PROJECT_LOG.md (root) | Historical log | HIGH | Add "historical only" header |

### 1.3 roadmap

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md | Historical planning | MEDIUM | Mark planning-only, archive candidate |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md | Historical planning | MEDIUM | Mark planning-only, archive candidate |
| docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md | Cave roadmap | LOW | Archive candidate |
| docs/roadmap/README.md | Index | LOW | Keep |
| docs/00_PROJECT/ROADMAP.md | NEW — planning-only hub | KEEP | Create |

### 1.4 spec (active)

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/specs/a_implementar/closeout_mvp/SPEC_18-29 (11 files) | Closeout specs | HIGH | Phase 0-1 done; Phase 2-3 pending; do NOT move yet |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay*.md | Covered by SPEC_28 | HIGH | delete_candidate (future, after Phase 2-3) |
| docs/specs/a_implementar/spec_cave_runtime*.md | Covered by SPEC_24 | HIGH | delete_candidate (future, after Phase 2-3) |
| docs/specs/a_implementar/spec_enemy_ai*.md | Covered by SPEC_23 | HIGH | delete_candidate (future, after Phase 2-3) |
| docs/specs/a_implementar/spec_combat_movement*.md | Outdated? | MEDIUM | delete_candidate (future) |
| docs/specs/a_implementar/spec_14a*.md (3 files) | Cave enemy (SPEC_24 done) | HIGH | delete_candidate (future) |
| docs/specs/a_implementar/spec_14b*.md | Cave snapshot (SPEC_24 done) | HIGH | delete_candidate (future) |

### 1.5 spec (closed/superseded)

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/specs/a_implementar/reorg/SPEC_00-12 + READMEs (14 files) | CLOSED (README_STATUS says CLOSED) | HIGH — looks active | Mark in index as SUPERSEDED; do NOT move yet |
| docs/specs/implementados/ (~60 files) | Historical implemented | LOW | Keep; no action |

### 1.6 refinement

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/refinements/a_implementar/pre_refinamentos/ (20 files) | Pre-refinements | MEDIUM | Most covered by closeout; mark in index |
| docs/refinements/a_implementar/ref_futuro_map.md | Future map | LOW | Keep for planning |
| docs/refinements/implementados/ (~35 files) | Historical refinements | LOW | Keep; no action |
| docs/refinements/REFINEMENT_MIGRATION_AUDIT.md | Audit | LOW | Keep |

### 1.7 validation_report

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/validation/spec_mvp_closeout_18-29 reports (24 files) | MVP evidence | LOW | Keep as evidence |
| docs/validation/spec_arch_reorg_00-12 reports (14 files) | Reorg evidence | LOW | Keep as evidence |
| docs/validation/SPEC06-17 reports (20+ files) | Historical evidence | LOW | Keep as evidence |
| docs/validation/spec_29b_phase0_* | Reconciliation | KEEP | Already correct |

### 1.8 architecture

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/architecture/ARCH_fase4_v2.2.md | Historical | LOW | Archive candidate |
| docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md | Historical | LOW | Archive candidate |
| docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md | Reference | LOW | Keep |
| docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md | Reference | LOW | Keep |

### 1.9 product (design/GDD)

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/design/GDD_v2.6.md | Historical | MEDIUM | Do not read by default; archive candidate |
| docs/design/GDD_v2.7_FASE9C_DELTA.md | Active? | MEDIUM | Mark as planning-only |
| docs/design/CHANGELOG_ATUALIZACAO_v2.6.md | Historical | LOW | Archive candidate |

### 1.10 historical_log

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| PROJECT_LOG.md | Cumulative session log | HIGH | Add historical header |
| docs/implementation_runs/ (5 files) | Overnight runs | LOW | Archive candidate |
| docs/IMPLEMENTATION_DELIVERY_20260523.md | Delivery snapshot | LOW | Archive candidate |
| docs/operations/HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md | Handoff | LOW | Archive candidate |
| docs/operations/SPECKIT_DRIFT_CONTROL_v1.0.md | Drift control | LOW | Keep (useful reference) |

### 1.11 backlog

| Path | Function Current | Risk | Action |
|------|-----------------|------|--------|
| docs/backlog/post_mvp_backlog.md | Post-MVP items | KEEP | Already reconciled in SPEC_29B |
| docs/backlog/FASE6_FARM_backlog_v1.2.md | Old farm backlog | LOW | Archive candidate |
| docs/backlog/FASE6_INDEX_global_v1.2.md | Old index | LOW | Archive candidate |
| docs/backlog/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md | Old index | LOW | Archive candidate |
| docs/backlog/FUTURE_IDEAS_TODO_v1.0.md | Ideas | LOW | Keep for reference |
| docs/backlog/reorg_architecture_residual_backlog.md | Reorg residuals | LOW | Keep |
| docs/06_BACKLOG/current_backlog.md | NEW — current operational | KEEP | Create |

---

## 2. Document Inventory by Function

### execution_context (agents must read for implementation)
```
AGENTS.md
CLAUDE.md
docs/00_PROJECT/CURRENT_STATE.md  [NEW]
docs/03_SPECS/active/*  [NEW DESTINATION — move later]
docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md  [NEW]
```

### planning_context (human reads; agents only when creating specs)
```
docs/00_PROJECT/ROADMAP.md  [NEW]
docs/06_BACKLOG/current_backlog.md  [NEW]
docs/backlog/post_mvp_backlog.md
docs/design/GDD_v2.7_FASE9C_DELTA.md
```

### audit_context (read only for investigation, reconciliation)
```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/validation/*
```

### historical_context (archive; do not read by default)
```
docs/implementation_runs/*
docs/refinements/implementados/*
docs/specs/a_implementar/reorg/*  (CLOSED)
docs/roadmap/NEXT_WAVES_ROADMAP_v1.*
docs/design/GDD_v2.6.md
docs/architecture/ARCH_fase4_v2.2.md
docs/architecture/ARCH_fase4_v2.3_*.md
```

### evidence (validation reports; read only when explicitly referenced)
```
docs/validation/spec_mvp_closeout_*
docs/validation/spec_arch_reorg_*
docs/validation/SPEC06-17*
```

### superseded (do not execute again)
```
docs/specs/a_implementar/reorg/SPEC_00-12 (CLOSED per README_STATUS.md)
docs/specs/a_implementar/spec_14a_*.md  (covered by SPEC_24)
docs/specs/a_implementar/spec_14b_*.md  (covered by SPEC_24)
docs/specs/a_implementar/spec_enemy_ai*.md (covered by SPEC_23)
docs/specs/a_implementar/spec_cave_runtime*.md (covered by SPEC_24)
docs/specs/a_implementar/spec_ui_ux_full_gameplay*.md (covered by SPEC_28)
```

---

## 3. Document Risk Inventory

### HIGH — can induce agent to execute old spec or misread status

| Path | Risk Reason |
|------|-------------|
| PROJECT_LOG.md (without header) | Agents might read entire history as current state |
| docs/specs/a_implementar/reorg/ | Still in a_implementar; README_STATUS says CLOSED but path looks active |
| docs/specs/a_implementar/spec_14a*.md | In a_implementar; covered by SPEC_24 closeout |
| docs/specs/a_implementar/spec_14b*.md | In a_implementar; covered by SPEC_24 closeout |
| docs/specs/a_implementar/spec_enemy_ai*.md | In a_implementar; covered by SPEC_23 closeout |
| docs/specs/a_implementar/spec_cave_runtime*.md | In a_implementar; two copies (one in implementados already!) |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay*.md | In a_implementar; covered by SPEC_28 closeout |

### MEDIUM — can generate excess context or ambiguity

| Path | Risk Reason |
|------|-------------|
| docs/IMPLEMENTATION_STATUS.md | 200+ lines; agents might read everything |
| docs/specs/SPEC_EXECUTION_ORDER.md | Long; history mixed with current |
| docs/design/GDD_v2.7_FASE9C_DELTA.md | Not labeled as planning-only |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md | Not labeled as planning-only |
| docs/refinements/a_implementar/pre_refinamentos/ | 20 refinements; most covered by closeout |

### LOW — evidence, archive, reference

Everything in `docs/validation/`, `docs/refinements/implementados/`, `docs/specs/implementados/`, `docs/backlog/FASE6_*`

### KEEP — official source of truth

```
AGENTS.md
CLAUDE.md
docs/specs/SPEC_EXECUTION_ORDER.md  (with governance header)
docs/IMPLEMENTATION_STATUS.md  (with governance header)
docs/release/MVP_ACCEPTANCE_REPORT.md  (already reconciled)
docs/backlog/post_mvp_backlog.md  (already reconciled)
docs/validation/spec_29b_phase0_*.md
docs/validation/spec_mvp_closeout_29_*.md
```

---

## 4. Proposed New Structure

```
docs/
  00_PROJECT/               [NEW]
    CURRENT_STATE.md        [NEW — ~80 lines; primary agent execution context]
    DOCUMENT_GOVERNANCE.md  [NEW — rules for types, states, reading policy]
    HISTORY_LOG_POLICY.md   [NEW — PROJECT_LOG.md usage policy]
    DOCUMENT_DELETE_CANDIDATES.md  [NEW — candidates list, not deleted yet]
    DOCUMENT_INDEX.md       [NEW — master document index]
    ROADMAP.md              [NEW — planning-only, replaces/condenses roadmap dir]

  03_SPECS/                 [NEW — future home for active specs]
    SPEC_TEMPLATE.md        [NEW]
    active/                 [future destination; no moves in SPEC_DOCS_30]
    implemented/            [future destination; no moves]

  04_REFINEMENTS/           [NEW — future home; no moves yet]
    README.md               [NEW]
    REFINEMENT_TEMPLATE.md  [NEW]

  05_VALIDATION/            [NEW]
    README.md               [NEW]
    current/                [NEW]
      LAST_VALIDATION_STATUS.md  [NEW — ~30 lines; current validation state]
    VALIDATION_REPORT_TEMPLATE.md  [NEW]

  06_BACKLOG/               [NEW]
    current_backlog.md      [NEW — short operational backlog]
```

**Existing structure not moved:**
- docs/specs/ → kept as-is; 03_SPECS is future home
- docs/refinements/ → kept as-is; 04_REFINEMENTS is future home
- docs/validation/ → kept as-is; 05_VALIDATION is future overlay
- docs/backlog/ → kept as-is; 06_BACKLOG is new parallel folder

---

## 5. Delete Candidates (identified, NOT deleted in this spec)

| Path | Type | Reason | Substitute | Risk to Delete |
|------|------|--------|------------|----------------|
| docs/specs/a_implementar/reorg/SPEC_00-12 (14 files) | spec (superseded) | CLOSED per README_STATUS | docs/validation/spec_arch_reorg_* | LOW after Phase 2-3 |
| docs/specs/a_implementar/spec_14a*.md (3 files) | spec (superseded) | Covered by SPEC_24 | SPEC_24 closeout report | LOW after Phase 2-3 |
| docs/specs/a_implementar/spec_14b*.md | spec (superseded) | Covered by SPEC_24 | SPEC_24 closeout report | LOW after Phase 2-3 |
| docs/specs/a_implementar/spec_enemy_ai_roster*.md | spec (superseded) | Covered by SPEC_23 | SPEC_23 closeout report | LOW after Phase 2-3 |
| docs/specs/a_implementar/spec_cave_runtime*.md | spec (superseded) | Covered by SPEC_24, copy in implementados | SPEC_24 closeout | LOW |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay*.md | spec (superseded) | Covered by SPEC_28 | SPEC_28 closeout | LOW after Phase 2-3 |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md | roadmap (old) | Superseded by v1.1 | ROADMAP.md new | LOW |
| docs/implementation_runs/ (5 files) | historical_log | Overnight runs, not needed | PROJECT_LOG.md | LOW |
| docs/architecture/ARCH_fase4_v2.2.md | architecture (old) | Superseded by v2.3 | v2.3 + CORE_CONTRACTS | LOW |
| docs/design/GDD_v2.6.md | product (old) | Superseded by v2.7 | GDD_v2.7 | LOW |
| docs/design/CHANGELOG_ATUALIZACAO_v2.6.md | historical | Embedded in GDD v2.7 | GDD_v2.7 | LOW |

---

## 6. Files Planned for Modification (SPEC_DOCS_30)

| Path | Change | Risk |
|------|--------|------|
| AGENTS.md | Add Context Reading Policy section | LOW |
| PROJECT_LOG.md | Add historical-only header at top | LOW |
| docs/IMPLEMENTATION_STATUS.md | Add governance note (already has SPEC_29B note) | LOW |
| docs/specs/SPEC_EXECUTION_ORDER.md | Add governance header (already has SPEC_29B note) | LOW |

---

## 7. Spec Promotion Status (for reference)

These specs are covered by closeout but NOT YET promoted to `implementados/` (pending Phase 2-3):

| Old Spec Path | Covered by Closeout | Status |
|---------------|---------------------|--------|
| docs/specs/a_implementar/spec_enemy_ai_roster*.md | SPEC_23 | Delete candidate; keep until Phase 2-3 |
| docs/specs/a_implementar/spec_cave_runtime*.md | SPEC_24 | Copy exists in implementados already |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay*.md | SPEC_28 | Delete candidate; keep until Phase 2-3 |
| docs/specs/a_implementar/reorg/ (SPEC_00-12) | Architecture reorg done | CLOSED; delete candidate after audit |

Note: `docs/specs/implementados/spec_cave_runtime_generation_checkpoints_boss_gates.md` and `docs/specs/implementados/spec_cave_entry_death_anya_corpse_recovery.md` already exist — these were likely pre-promoted. Verify these are not the only copy before deleting `a_implementar` version.

---

## Phase 0 Decision

**RESULT:** Matrix complete. Governance structure clear.

**Files to create (15):** docs/00_PROJECT/*, docs/03_SPECS/SPEC_TEMPLATE.md, docs/04_REFINEMENTS/*, docs/05_VALIDATION/*, docs/06_BACKLOG/current_backlog.md

**Files to update (4):** AGENTS.md, PROJECT_LOG.md, docs/IMPLEMENTATION_STATUS.md (minor), docs/specs/SPEC_EXECUTION_ORDER.md (minor)

**Files NOT to touch:** All runtime, all validation reports, all existing specs

**Files to mark as delete candidates:** 14 (see Section 5)

**Proceed to implementation.**

---

**Matrix Created:** 2026-06-01  
**Status:** Phase 0 Complete — Ready for implementation

