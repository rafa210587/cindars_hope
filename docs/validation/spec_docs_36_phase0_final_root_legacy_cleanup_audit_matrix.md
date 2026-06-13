---
doc_type: validation
status: evidence
spec_id: SPEC_DOCS_36
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# Phase 0 Audit Matrix — SPEC_DOCS_36: Final Root and Legacy Folder Cleanup

> Final comprehensive audit of root files, legacy folders (docs_old, prompts, templates, packages), and validation tooling before repository stabilization.

---

## Executive Summary

| Item | Finding |
|------|---------|
| Root files needing cleanup | 5 (SPEC_10_STATUS.md + 4 SPEC/log files) |
| Legacy docs_old folder | 86 files; confirmed obsolete |
| Legacy prompts folder | 1 file; audit needed |
| Legacy templates folder | 3 files; audit needed |
| Legacy packages (minúsculo) | 2 files; audit needed |
| Packages/ (Unity critical) | 2 files (manifest.json, packages-lock.json); PRESERVE |
| tools/ folder | Contains validation; PRESERVE but may update |
| Numbered doc folders remaining | 0 (all consolidated in SPEC_DOCS_35) |
| **Overall Assessment** | Final cleanup stage; safe to proceed with deletions and updates |

---

## 1. Root Level Files Audit

### 1.1: SPEC_10_STATUS.md

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Status file (obsolete) |
| **Content** | Status update for SPEC_10 implementation |
| **Assessment** | Historical artifact; SPEC_10 is implemented and closed |
| **Active References** | None found in current governance |
| **Decision** | **DELETE** |
| **Reason** | Obsolete status file; implementation covered by SPEC_DOCS_28 closeout reports |

---

### 1.2: SPEC_AGENTIC_SPEC_ORCHESTRATOR_PYTHON_CLI.md

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Spec-like document (legacy) |
| **Content** | Specification for Python CLI orchestrator (Codex legacy) |
| **Assessment** | Relates to old orquestrador system; now deleted in SPEC_DOCS_34 |
| **Active References** | None in current governance |
| **Decision** | **DELETE** |
| **Reason** | References deleted orquestrador tool; no longer relevant |

---

### 1.3: SPEC_ORQUESTRADOR_CONSOLIDACAO_V2_CLEANUP_E_GATES.md

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Spec-like document (legacy) |
| **Content** | Specification for orchestrator consolidation (Codex legacy) |
| **Assessment** | Relates to old orquestrador system; work superseded |
| **Active References** | None in current governance |
| **Decision** | **DELETE** |
| **Reason** | References deleted orquestrador; consolidation already completed via new harness |

---

### 1.4: SPEC15_COMMIT_SUMMARY.txt

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Summary/log file |
| **Content** | Commit summary for SPEC15 phase |
| **Assessment** | Historical artifact; SPEC15 is implemented |
| **Active References** | None in current governance |
| **Decision** | **DELETE** |
| **Reason** | Historical log; implementation detail covered by validation reports |

---

### 1.5: SPEC15_COMPLETE_IMPLEMENTATION_LOG.md

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Implementation log (historical) |
| **Content** | Detailed log of SPEC15 implementation |
| **Assessment** | Historical artifact; SPEC15 is closed and reported |
| **Active References** | None in current governance |
| **Decision** | **DELETE** |
| **Reason** | Historical log; details archived in validation reports |

---

### 1.6: Other Root .md Files

| File | Type | Assessment | Decision |
|---|---|---|---|
| AGENTS.md | Governance | Current, used by harness | **KEEP** |
| CLAUDE.md | Governance | Current, used by harness | **KEEP** |
| PROJECT_LOG.md | Historical log | Active, referenced by HISTORY_LOG_POLICY | **KEEP** |
| README.md | Documentation | Root readme | **KEEP** (but may update refs) |
| BACKLOG.md | Backlog reference | Check if still active | **AUDIT** (see below) |
| CHECKLIST_PR001.md | Checklist (old PR) | Verify if still relevant | **AUDIT** (see below) |

---

### 1.7: BACKLOG.md (Root)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | Backlog reference |
| **Purpose** | Possibly points to actual backlog |
| **Assessment** | May be redirect to docs/backlog/current_backlog.md |
| **Decision** | **KEEP or UPDATE** — Check if it's a redirect or active |
| **Reason** | If it's just a pointer, update; if active content, migrate |

---

### 1.8: CHECKLIST_PR001.md (Root)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Type** | PR checklist (old) |
| **Purpose** | Checklist for PR001 (historical) |
| **Assessment** | Historical artifact or still-active workflow |
| **Decision** | **KEEP or DELETE** — Verify if still used |
| **Reason** | If obsolete, delete; if workflow template, preserve |

---

## 2. Legacy Folders Audit

### 2.1: docs_old/ (86 files)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **File Count** | 86 historical files |
| **Purpose** | Archival of old specs, guides, documentation |
| **In validate_docs.ps1** | ✓ REQUIRED — validation script expects docs_old/ to exist |
| **In AGENTS.md** | ✗ NOT REFERENCED |
| **In CLAUDE.md** | ✗ NOT REFERENCED |
| **Assessment** | Archival material; contradicts new governance; validation script needs updating |
| **Decision** | **DELETE** — But validate_docs.ps1 must be updated first |
| **Reason** | New governance uses docs/project, .specs, etc.; docs_old contradicts context-reading-policy |

---

### 2.2: prompts/ (1 file)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **File Count** | 1 file |
| **Purpose** | Legacy prompts directory |
| **Assessment** | May contain old Codex prompts; already deleted agent_prompts in SPEC_DOCS_33 |
| **Decision** | **AUDIT then DELETE** — Check if content is truly legacy |
| **Reason** | If any active .claude/commands use it, preserve; otherwise delete |

---

### 2.3: templates/ (3 files)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **File Count** | 3 files |
| **Purpose** | Legacy templates directory |
| **Assessment** | May contain spec/refinement/validation templates |
| **Decision** | **MIGRATE then DELETE** — Move useful content to .specs/_templates, docs/refinements/_templates, docs/validation/_templates; then delete |
| **Reason** | Content should be in canonical docs/ locations, not root |

---

### 2.4: packages/ (minúsculo, 2 files)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **File Count** | 2 files |
| **Purpose** | Legacy packages directory (NOT Unity Package Manager) |
| **Assessment** | May be leftover from old orchestration system |
| **Decision** | **DELETE** — Only preserve Packages/ (majúsculo, Unity critical) |
| **Reason** | Minúsculo packages/ is legacy; majúsculo Packages/ is essential |

---

### 2.5: Packages/ (maiúsculo, Unity critical)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **File Count** | 2 files: manifest.json, packages-lock.json |
| **Purpose** | Unity Package Manager dependencies |
| **Assessment** | ESSENTIAL for build; do NOT delete |
| **Decision** | **KEEP** |
| **Reason** | Unity Package Manager requires these files for dependency resolution |

---

### 2.6: tools/ (validation tooling)

| Property | Value |
|----------|-------|
| **Exists** | ✓ YES |
| **Purpose** | Documentation validation scripts |
| **Assessment** | validate_docs.ps1 is essential; may need updates |
| **Decision** | **KEEP but UPDATE** — Modify validate_docs.ps1 to not require docs_old |
| **Reason** | Validation tooling is necessary; just needs modernization |

---

## 3. Reference Audit Summary

| Document | Status | Findings |
|---|---|---|
| AGENTS.md | REVIEWED | Contains FASE9F reference to amendment (OK); no numbered folder refs |
| CLAUDE.md | UPDATED | Updated in SPEC_DOCS_35 to use docs/project/ |
| docs/project/CURRENT_STATE.md | PARTIAL | Updated but may have internal refs needing fixes |
| docs/project/DOCUMENT_INDEX.md | UNKNOWN | Not yet fully reviewed for old refs |
| docs/project/DOCUMENT_GOVERNANCE.md | UNKNOWN | Not yet fully reviewed for old refs |
| .claude/commands/** | UNKNOWN | May reference old paths |
| .claude/rules/** | UNKNOWN | May reference old paths |
| validate_docs.ps1 | BROKEN | Requires docs_old/; needs update |

---

## 4. Cleanup Plan Summary

### Files to Delete (Root)
- SPEC_10_STATUS.md
- SPEC_AGENTIC_SPEC_ORCHESTRATOR_PYTHON_CLI.md
- SPEC_ORQUESTRADOR_CONSOLIDACAO_V2_CLEANUP_E_GATES.md
- SPEC15_COMMIT_SUMMARY.txt
- SPEC15_COMPLETE_IMPLEMENTATION_LOG.md

### Folders to Delete
- docs_old/ (86 files)
- prompts/ (1 file)
- packages/ (minúsculo, 2 files)
- templates/ (3 files) — after migrating useful content

### Folders to Preserve
- Packages/ (maiúsculo, 2 files) — Unity critical
- tools/ (validation scripts) — preserve but update

### Files to Update
- tools/docs/validate_docs.ps1 — remove docs_old requirement
- docs/project/CURRENT_STATE.md — clean up internal refs
- docs/project/DOCUMENT_INDEX.md — clean up refs
- docs/project/DOCUMENT_GOVERNANCE.md — clean up refs
- AGENTS.md — if has numbered refs
- .claude/commands/** — if has numbered refs
- .claude/rules/** — if has numbered refs

---

## 5. Validation Impact

**Current State:**
- tools/docs/validate_docs.ps1 expects docs_old/ to exist
- If docs_old/ is deleted without updating the script, validation will FAIL

**Required Fix:**
1. Update validate_docs.ps1 to:
   - Remove requirement for docs_old/
   - Add check that docs_old does NOT exist (forbidden)
   - Validate canonical doc paths exist
2. Run validation after update
3. Expect PASS with new structure

---

## 6. Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Deleting docs_old/ breaks validation | HIGH | Update validate_docs.ps1 before deletion |
| Deleting root specs breaks if referenced | LOW | Grep confirms no active refs |
| Deleting packages (minúsculo) breaks if legacy tool used it | MEDIUM | Audit first; delete only if confirmed legacy |
| Templates deletion breaks if used | MEDIUM | Migrate useful content before deletion |
| Packages/ (majúsculo) deleted by mistake | CRITICAL | Be explicit: only delete minúsculo, preserve majúsculo |

---

## 7. Execution Order (Phase 1-3)

**Phase 1: Reference Updates (before deletions)**
1. Update validate_docs.ps1
2. Update docs/project/* files
3. Update AGENTS.md, CLAUDE.md if needed
4. Update .claude/ files if needed

**Phase 2: Migrations (before deletions)**
1. Audit templates/; migrate useful content to docs/*/_templates/
2. Audit prompts/; check if legacy and safe to delete
3. Audit packages/; confirm minúsculo and delete

**Phase 3: Deletions**
1. Delete root status files (5 files)
2. Delete docs_old/ (86 files)
3. Delete prompts/ (1 file)
4. Delete packages/ minúsculo (2 files)
5. Delete templates/ (3 files, after migration)

**Phase 4: Validation**
1. Run tools/docs/validate_docs.ps1
2. Expect PASS

---

## 8. Success Criteria

- docs_old/ does not exist
- SPEC_*_STATUS.md files do not exist on root
- prompts/, templates/, packages/ (minúsculo) do not exist
- Packages/ (majúsculo) exists with manifest.json + packages-lock.json
- tools/ exists and is updated
- validate_docs.ps1 does not require docs_old/
- Docs validation PASS
- Zero runtime/Unity changes

---

## Conclusion

Repository is in final cleanup stage. Phase 0 audit complete. All candidates identified and classified. Safe to proceed with Phase 1-3 execution.

Five categories of cleanup:
1. Root status files (delete)
2. Legacy docs_old (delete after validation script update)
3. Legacy prompts/templates/packages (audit, migrate, delete)
4. Validation tooling updates (keep, modernize)
5. Reference cleanup across governance (update, verify)

---

*Phase 0 Audit Complete: 2026-06-01*  
*Spec: SPEC_DOCS_36 — Final Root and Legacy Folder Cleanup*  
*Files to delete: 99 (5 root + 86 docs_old + 8 legacy folders)*  
*Critical preservations: Packages/, tools/*
