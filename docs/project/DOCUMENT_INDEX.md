# Document Index — Cindar's Hope

> Master index of documentation by category.  
> For execution context, use `CURRENT_STATE.md`.

---

## Official Execution Context (agents must read)

| Document | Purpose |
|----------|---------|
| `AGENTS.md` | Agent rules, skills, protocol |
| `CLAUDE.md` | Claude Code project instructions |
| `docs/project/CURRENT_STATE.md` | Current project status; active queue |
| `docs/validation/current/LAST_VALIDATION_STATUS.md` | Latest automated validation results |

---

## Active Specs

| Location | Status |
|----------|--------|
| `.specs/a_implementar/closeout_mvp/SPEC_18-29` | Phase 0-1 done; Phase 2-3 pending human |
| `.specs/a_implementar/closeout_mvp/README_EXECUTION_ORDER.md` | Execution order for closeout package |

---

## Planning Documents (humans read; agents read only when creating specs)

| Document | Purpose |
|----------|---------|
| `docs/project/ROADMAP.md` | High-level project roadmap |
| `docs/backlog/current_backlog.md` | Current operational backlog |
| `docs/backlog/post_mvp_backlog.md` | Post-MVP work items |
| `docs/backlog/FUTURE_IDEAS_TODO_v1.0.md` | Future ideas |

---

## Audit Documents (read only for investigation or reconciliation)

| Document | Purpose |
|----------|---------|
| `PROJECT_LOG.md` | Session-by-session historical log |
| `docs/IMPLEMENTATION_STATUS.md` | Broad implementation status |
| `.specs/SPEC_EXECUTION_ORDER.md` | Spec registry and dependency order |
| `docs/backlog/reorg_architecture_residual_backlog.md` | Reorg residuals |

---

## MVP Acceptance Documents

| Document | Purpose |
|----------|---------|
| `docs/release/MVP_ACCEPTANCE_REPORT.md` | MVP acceptance status |
| `docs/validation/spec_mvp_closeout_29_final_mvp_acceptance_and_promotion_execution_report.md` | SPEC_29 Phase 0-1 report |
| `docs/validation/spec_29b_phase0_human_acceptance_reconciliation_audit_matrix.md` | SPEC_29B reconciliation |

---

## Validation Evidence (read only when cited by spec or audit)

| Location | Contents |
|----------|---------|
| `docs/validation/spec_mvp_closeout_18-29*` | MVP closeout evidence (22 reports) |
| `docs/validation/spec_arch_reorg_00-12*` | Architecture reorg evidence (14 reports) |
| `docs/validation/SPEC06-17*` | Historical validation evidence (20+ reports) |
| `docs/validation/current/LAST_VALIDATION_STATUS.md` | Most recent automated results |

---

## Decision Records (Architecture Decisions)

| Document | Purpose |
|----------|---------|
| `docs/project/DECISION_LOG.md` | Index of all ADRs with reading policy |
| `docs/decisions/` | Canonical Architecture Decision Records (ADR-0001 through ADR-0009) |
| `docs/decisions/_templates/ADR_TEMPLATE.md` | ADR template for new decisions |

**Reading policy:** Agents read only ADRs explicitly cited by the active spec in `required_adrs:` field.

---

## Game Rules (Current Operational Behavior)

| Document | Purpose |
|----------|---------|
| `docs/game_rules/GAME_RULES_INDEX.md` | Index of all game rules by domain (12 documents) |
| `docs/game_rules/*.md` | Current gameplay, architecture, and operational rules (cave, combat, inventory, skill tree, UI, etc.) |
| `docs/game_rules/_templates/GAME_RULE_TEMPLATE.md` | Game rule template for new rules |

**Reading policy:** Agents read only game_rules explicitly cited by the active spec in `required_game_rules:` field.

---

## Architecture Reference

| Document | Purpose |
|----------|---------|
| `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` | Core contracts (current) |
| `docs/amendments/` | **ARCHIVED HISTORICAL SOURCE** — Amendment content migrated to ADRs and game_rules; not canonical |
| `docs/amendments/README.md` | Migration map showing where amendment content now lives |

---

## Governance Documents (new in SPEC_DOCS_30)

| Document | Purpose |
|----------|---------|
| `docs/project/DOCUMENT_GOVERNANCE.md` | Rules for types, states, reading policy |
| `docs/project/HISTORY_LOG_POLICY.md` | When to read PROJECT_LOG.md |
| `docs/project/DOCUMENT_DELETE_CANDIDATES.md` | Candidates for future deletion |
| `docs/project/DOCUMENT_INDEX.md` | This file |
| `.specs/_templates/SPEC_TEMPLATE.md` | Template for new specs |
| `docs/refinements/_templates/REFINEMENT_TEMPLATE.md` | Template for new refinements |
| `docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md` | Template for validation reports |

---

## Superseded / Do Not Execute

| Location | Status |
|----------|--------|
| `.specs/a_implementar/reorg/` | README_STATUS.md preserved as historical marker; SPEC_00-12 and SPEC_00_STRATEGY_SUBAGENTS deleted in SPEC_DOCS_31 |
| `.specs/a_implementar/spec_14a*.md` | Covered by SPEC_24 — do not execute (Batch 2 candidates) |
| `.specs/a_implementar/spec_14b*.md` | Covered by SPEC_24 — do not execute (Batch 2 candidates) |
| `.specs/a_implementar/spec_enemy_ai*.md` | Covered by SPEC_23 — do not execute (Batch 2 candidates) |
| `.specs/a_implementar/spec_cave_runtime*.md` | Covered by SPEC_24, copy in implementados (Batch 2 candidates) |
| `.specs/a_implementar/spec_ui_ux_full_gameplay*.md` | Covered by SPEC_28 — do not execute (Batch 2 candidates) |

---

## Implemented Specs (historical reference)

Located in `.specs/implementados/` — ~60 files covering SPEC_00-17F.

---

## Implemented Refinements (historical reference)

Located in `docs/refinements/implementados/` — ~35 refinements covering FASE 1-9K.

---

*Created: 2026-06-01 (SPEC_DOCS_30)*  
*Update this file when new documents or folders are added.*
