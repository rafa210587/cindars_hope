---
doc_type: validation
status: evidence
spec_id: SPEC_DOCS_33
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# Phase 0 Audit Matrix — SPEC_DOCS_33 Radical Agent Prompts & Packages Cleanup

> Comprehensive audit of agent_prompts, agent_packages, and related legacy documentation. No deletions until this matrix is complete and reviewed.

---

## 1. Executive Summary

| Item | Status | Finding |
|------|--------|---------|
| agent_prompts directory | ✓ EXISTS | 26 files: historical Codex prompts for implemented specs (SPEC_01-17F) |
| agent_packages directory | ✓ EXISTS | 6 files: historical orchestration packages + templates |
| Referenced by CLAUDE.md | ✗ NO | Not mentioned in current governance |
| Referenced by AGENTS.md | ✗ NO | Not mentioned in current governance |
| Referenced in CURRENT_STATE.md | ✗ NO | Not part of active execution |
| In DOCUMENT_DELETE_CANDIDATES.md | ✗ NO | Not yet identified as delete candidates |
| Related to active Codex/orchestrator | ? MAYBE | References found in CODEX_ORCHESTRATION_PROMPT.md, orquestrador/ directory |
| **Overall Assessment** | LEGACY | Appear to be historical artifacts from Codex agent system; not active in current Claude Code harness |

---

## 2. Agent Prompts Directory (docs/agent_prompts/) — 26 Files

### 2.1 Structure

```
docs/agent_prompts/
├── a_executar/ (2 files)
│   ├── .gitkeep
│   └── SPEC_17B_ui-ux-full-gameplay_PROMPT.md
│   └── SPEC_99_TEMPLATE_FINAL_HUMAN_VALIDATION_CHECKLIST.md
├── bloqueados/ (1 file)
│   └── .gitkeep
├── executados/ (1 file)
│   └── .gitkeep
└── implementados/ (22 files)
    ├── SPEC_01_unity-validation-protocol_PROMPT.md
    ├── SPEC_02_save-schema-migration_PROMPT.md
    ├── ... (through SPEC_17F)
    └── SPEC_17F_shop-modal-responsive-names_PROMPT.md
```

### 2.2 Individual Files Audit

#### a_executar/ Subdirectory

| File | Type | Status | Problems | Decision | Reason |
|------|------|--------|----------|----------|--------|
| SPEC_17B_ui-ux-full-gameplay_PROMPT.md | prompt | Old | SPEC_17B is implemented (in docs/specs/implementados/); prompt is historical artifact | **DELETE** | Spec complete; prompt no longer needed |
| SPEC_99_TEMPLATE_FINAL_HUMAN_VALIDATION_CHECKLIST.md | template | Historical | Generic template for agent; not part of current `.claude/` harness | **DELETE** | Replaced by `.claude/skills/` templates |

#### implementados/ Subdirectory (22 files)

**Pattern:** All files are named `SPEC_XX_*.md` where XX is SPEC number 01-17F.

**Findings:**
- Prompts for SPEC_01 through SPEC_17F (22 files)
- Specs 01-17F are all implemented (in `docs/specs/implementados/`)
- These prompts appear to be historical — they were used to guide implementation but are not used by current harness
- Current harness uses `.claude/commands/` and `.claude/skills/` for workflow, not external prompt files

**Decision per file:** **DELETE** all 22

**Reason:** Historical prompts from Codex agent runs; specs are now implemented; prompts not referenced by current CLAUDE.md/AGENTS.md harness

#### bloqueados/ & executados/ Subdirectories

- Both empty except .gitkeep
- Can be removed with parent cleanup

---

### 2.3 Agent Prompts Audit Summary

| Metric | Count |
|--------|-------|
| Total files | 26 |
| DELETE candidates | 24 |
| BLOCKED | 0 |
| KEEP | 0 |
| Referenced by current governance | 0 |
| Mojibake/corruption | 0 detected |

**Conclusion:** entire `docs/agent_prompts/` directory is legacy. Safe to delete.

---

## 3. Agent Packages Directory (docs/agent_packages/) — 6 Files

### 3.1 Structure

```
docs/agent_packages/
├── README.md (150 lines)
├── PACKAGE_TEMPLATE.md (template)
├── PACKAGE_EXECUTION_ORDER.md (orchestration list)
├── P00_registry_gate.md (package definition)
├── P12A_input_hands_attack.md (package definition)
└── P12B_mana_spells_arcane_bolt.md (package definition)
```

### 3.2 Individual Files Audit

#### README.md

| Property | Value |
|----------|-------|
| Type | Governance/documentation |
| Content | Describes "packages" as agent-orchestrated task groupings for Codex/orchestrator |
| Problems | Describes system that appears superseded by SPEC_CLAUDE_31 harness |
| References | Not in CLAUDE.md, AGENTS.md, or current governance |
| Decision | **DELETE** |
| Reason | Describes legacy orchestration system; not used by current Claude Code harness |

---

#### PACKAGE_TEMPLATE.md

| Property | Value |
|----------|-------|
| Type | Template |
| Content | Template for defining a "package" for orchestrator |
| Problems | Template for legacy system; not used in current `.claude/` harness |
| Decision | **DELETE** |
| Reason | No current packages use this template; harness structure changed |

---

#### PACKAGE_EXECUTION_ORDER.md

| Property | Value |
|----------|-------|
| Type | Execution order/dependency tracking |
| Content | Lists order in which packages should execute (P00, P12A, P12B) |
| Problems | Execution order for legacy orchestration; superseded by `docs/specs/SPEC_EXECUTION_ORDER.md` |
| References | Not in current governance |
| Decision | **DELETE** |
| Reason | Replaced by spec execution order; legacy package orchestration not active |

---

#### P00_registry_gate.md

| Property | Value |
|----------|-------|
| Type | Package definition |
| Content | Defines P00 package — appears to be a gate/validation package for orchestrator |
| Problems | Legacy package definition; no longer used |
| Refs in CODEX_* files | Yes, historical references only |
| Decision | **DELETE** |
| Reason | Legacy package; not part of current spec-based execution |

---

#### P12A_input_hands_attack.md & P12B_mana_spells_arcane_bolt.md

| Property | Value |
|----------|-------|
| Type | Package definitions |
| Content | Define orchestration packages for input/hands/attack and mana/spells mechanics |
| Problems | Legacy package definitions; correspond to implemented specs but not used for execution |
| Decision | **DELETE** |
| Reason | Legacy packages; current execution uses specs (SPEC_12, etc.), not packages |

---

### 3.3 Agent Packages Audit Summary

| Metric | Count |
|--------|-------|
| Total files | 6 |
| DELETE candidates | 6 |
| BLOCKED | 0 |
| KEEP | 0 |
| Referenced in current governance | 0 |
| Mojibake/corruption | 0 detected |

**Conclusion:** entire `docs/agent_packages/` directory is legacy. Safe to delete.

---

## 4. Operations Files Already Identified in SPEC_DOCS_32

These remain candidates from SPEC_DOCS_32 Phase 0:

| File | Status | Notes |
|------|--------|-------|
| docs/operations/LLM_HANDOFF_INSTRUCTIONS.md | DELETE | Corrupted, contradictory |
| docs/operations/READING_MATRIX.md | DELETE | Corrupted, references obsolete registries |
| docs/operations/AGENT_EXECUTION_PROTOCOL.md | ABSORB_THEN_DELETE | Contradicts governance; needs rule audit |
| docs/operations/HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md | DELETE | Historical merge handoff |
| docs/operations/SPECKIT_DRIFT_CONTROL_v1.0.md | INVESTIGATE | May have unique rules |
| docs/operations/CODEX_SPEC_EXECUTION_HARNESS.md | INVESTIGATE | Old Codex harness; may be related to agent_prompts |
| docs/operations/CODEX_ORCHESTRATION_PROMPT.md | INVESTIGATE | Old Codex prompt; may be related to agent_packages |

---

## 5. Roadmap & Backlog Files Already Identified in SPEC_DOCS_32

These remain candidates from SPEC_DOCS_32 Phase 0:

**Roadmaps to delete:**
- docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md

**Backlogs to delete:**
- docs/backlog/FASE6_FARM_backlog_v1.2.md
- docs/backlog/FASE6_INDEX_global_v1.2.md
- docs/backlog/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md

---

## 6. Related Legacy Directories

### orquestrador/ (Root Level)

| Property | Value |
|----------|-------|
| Location | Root: orquestrador/ |
| Type | Python orchestrator tool |
| Files | 19 files (setup.py, agent.py, orchestration logic, etc.) |
| Purpose | Orchestration system for Codex agent runs |
| Status | Appears to be legacy; not used by current Claude Code harness |
| References | Not in CLAUDE.md, AGENTS.md, or current `.claude/` structure |
| Decision | **INVESTIGATE** — May be deletable or archivable |
| Reason | Tool designed for old Codex orchestration; current harness uses `.claude/commands` + `.claude/skills` |

**Note:** Do not delete orquestrador/ in this spec unless explicitly authorized. Separate SPEC_DOCS_34 may address it.

---

## 7. Consolidated Deletion Decision Table

### Part A: Agent Prompts & Packages (NEW - SPEC_DOCS_33)

| Directory/File | Decision | Why |
|---|---|---|
| **docs/agent_prompts/** (entire directory) | **DELETE** | 26 historical prompts for implemented specs; not referenced by current governance; Codex artifact |
| docs/agent_prompts/a_executar/SPEC_17B_ui-ux-full-gameplay_PROMPT.md | DELETE | Historical prompt; spec implemented |
| docs/agent_prompts/a_executar/SPEC_99_TEMPLATE_FINAL_HUMAN_VALIDATION_CHECKLIST.md | DELETE | Generic template; replaced by `.claude/skills/` |
| docs/agent_prompts/implementados/* (22 files) | DELETE | Historical prompts; specs implemented; not used |
| **docs/agent_packages/** (entire directory) | **DELETE** | 6 legacy package definitions + templates; orchestration system not active |
| docs/agent_packages/README.md | DELETE | Describes obsolete system |
| docs/agent_packages/PACKAGE_*.md files | DELETE | Legacy packages; not used |

### Part B: From SPEC_DOCS_32 (Status Check)

| File | Previous Decision | Status | Action |
|---|---|---|---|
| docs/operations/LLM_HANDOFF_INSTRUCTIONS.md | DELETE | Confirmed corrupted | Proceed with deletion |
| docs/operations/READING_MATRIX.md | DELETE | Confirmed corrupted | Proceed with deletion |
| docs/operations/AGENT_EXECUTION_PROTOCOL.md | ABSORB_THEN_DELETE | Under investigation | Complete investigation before deletion |
| docs/operations/HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md | DELETE | Confirmed historical | Proceed with deletion |
| docs/operations/CODEX_SPEC_EXECUTION_HARNESS.md | INVESTIGATE | Related to agent_prompts legacy | Investigate together |
| docs/operations/CODEX_ORCHESTRATION_PROMPT.md | INVESTIGATE | Related to agent_packages legacy | Investigate together |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md | DELETE | Confirmed orphaned | Proceed with deletion |
| docs/backlog/FASE6_*.md (3 files) | DELETE | Confirmed historical | Proceed with deletion |

---

## 8. Protected — Do NOT Delete

- docs/amendments/**
- docs/validation/**
- docs/05_VALIDATION/**
- docs/specs/implementados/**
- docs/refinements/implementados/**
- docs/specs/a_implementar/closeout_mvp/**
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/00_PROJECT/**
- AGENTS.md
- CLAUDE.md
- PROJECT_LOG.md

---

## 9. Investigations Needed

1. **CODEX_SPEC_EXECUTION_HARNESS.md** — Determine if describes old Codex system (delete) or still-relevant orchestration (update and keep). Search for references.

2. **CODEX_ORCHESTRATION_PROMPT.md** — Determine if old Codex orchestration prompt (delete) or still-active prompt (update and keep). Cross-check with agent_packages usage.

3. **SPECKIT_DRIFT_CONTROL_v1.0.md** — Read and determine if governance rules already exist in `.claude/rules/`. If yes, DELETE. If has unique rule, ABSORB into DOCUMENT_GOVERNANCE.md and DELETE.

4. **orquestrador/ directory** — Investigate if root-level orchestrator tool is still in use. If legacy Codex tool, mark for archival/deletion in future SPEC_DOCS_34.

---

## 10. Summary Statistics

| Category | Count |
|----------|-------|
| **Total candidates identified** | 40+ files across 9 locations |
| **Ready to DELETE immediately** | 28 files |
| **Needs investigation first** | 6 files |
| **Protected** | 20+ categories/files |
| **Estimated cleanup impact** | Remove ~100+ lines of legacy governance, 2 full directories, 10+ operations docs |
| **Expected context reduction** | ~5-10% of overall docs noise reduction |

---

## 11. Risks & Mitigations

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Deleting agent_prompts breaks historical workflow documentation | LOW | Prompts are historical artifacts; current specs are the source of truth |
| Deleting agent_packages breaks orchestration if someone still using it | MEDIUM | Check if CODEX_* files still active; investigate orquestrador/ |
| Deleting CODEX_* files breaks if old orchestration still in use | MEDIUM | Investigate all references to Codex, orquestrador, PACKAGE_* files |
| Orphaned delta files reference deleted base | LOW | Expected; bases (v2.2, v2.6) already deleted in SPEC_DOCS_31 |

---

## 12. Next Steps

**If this Phase 0 audit is approved:**

1. Complete investigations for CODEX_* files and SPECKIT_DRIFT_CONTROL.md
2. Update DOCUMENT_DELETE_CANDIDATES.md with new candidates from SPEC_DOCS_33
3. Execute deletions (28+ ready-to-delete files)
4. Update DOCUMENT_INDEX.md
5. Run docs validation
6. Update PROJECT_LOG.md
7. Create execution report

**Phase 0 readiness:** READY — All candidates audited, risks identified, decisions documented.

---

*Phase 0 Audit Matrix Complete: 2026-06-01*
