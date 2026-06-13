---
doc_type: validation
status: audit
spec_id: SPEC_DOCS_39
validation_type: phase_0
result: AUDIT_COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# SPEC_DOCS_39 Phase 0 — Decision/Game Rules Harness Audit Matrix

> **Audit of SPEC_DOCS_38 remaining harness items and reference documentation updates needed.**

---

## Executive Summary

| Category | Count | Status | Action |
|---|---|---|---|
| Reference docs needing path updates | 3 | STALE | Update: CURRENT_STATE, DOCUMENT_INDEX, DOCUMENT_GOVERNANCE |
| Templates needing field additions | 2 | INCOMPLETE | Update: SPEC_TEMPLATE, VALIDATION_REPORT_TEMPLATE |
| Harness components to create | 2 | PENDING | Create: skill, hook |
| Harness components to update | 1 | NEEDS_UPDATE | Update: validate_docs.ps1 |
| Rules to remove/replace | 1 | OBSOLETE | Replace: no-docs-old-edits.md → legacy-doc-paths-forbidden.md |
| Amendment handling | 2 | NEEDS_TRIAGE | Resolve: FASE9F (delete after 100% migration), FASE9G (archive as historical) |

**Overall:** ~91% of harness components present; reference docs have stale paths; 6 updates needed to close harness.

---

## Phase 0 Audit Details

### 1. Reference Documentation (3 files)

| File | Path | Status | Issue | Fix |
|---|---|---|---|---|
| CURRENT_STATE.md | docs/project/CURRENT_STATE.md | EXISTS | "Docs validation: 14/14" outdated; missing DECISION_LOG, decisions/, game_rules/ in Key File Locations; still mentions docs/design/GDD_v2.7* | Update validation count to "25+ checks"; add locations for DECISION_LOG, ADRs, game_rules; remove GDD path |
| DOCUMENT_INDEX.md | docs/project/DOCUMENT_INDEX.md | EXISTS | Missing "Decision Records" and "Game Rules" sections; amendments listed as active Architecture Reference | Add sections for decisions/game_rules; mark amendments as "archived/historical" |
| DOCUMENT_GOVERNANCE.md | docs/project/DOCUMENT_GOVERNANCE.md | EXISTS | Uses outdated paths: docs/00_PROJECT/*, docs/06_BACKLOG/*, docs/05_VALIDATION/*, docs/design/* | Replace paths with canonical: docs/project/*, docs/backlog/*, docs/validation/*, docs/product/* |

### 2. Templates (2 files)

| File | Path | Status | Issue | Fix |
|---|---|---|---|---|
| SPEC_TEMPLATE.md | .specs/_templates/SPEC_TEMPLATE.md | EXISTS | Missing `required_adrs: []` and `required_game_rules: []` fields; outdated required_read paths; mentions "14/14 docs validation" | Add ADR/game_rules fields; update paths to canonical; update validation reference |
| VALIDATION_REPORT_TEMPLATE.md | docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md | EXISTS | Missing `validated_adrs: []` and `validated_game_rules: []` fields; uses "N/14" validation format | Add ADR/game_rules fields; update to "PASS/FAIL" format; add section for ADR/game_rule validation |

### 3. Harness Components (3 items)

| Component | Path | Status | Need | Action |
|---|---|---|---|---|
| Skill: decision-rule-extraction | .claude/skills/decision-rule-extraction/SKILL.md | NOT_EXISTS | Extract decisions from specs/amendments into ADRs/game_rules | Create |
| Hook: decision-rule-reference-guard | .claude/hooks/decision-rule-reference-guard.ps1 | NOT_EXISTS | Warn if specs cite amendments as canonical; warn if missing required_adrs/required_game_rules | Create |
| Validator: validate_docs.ps1 | tools/docs/validate_docs.ps1 | EXISTS | Needs 8+ new checks for ADRs/game_rules structure validation | Update to add ADR/game_rule checks |

### 4. Rules (1 item)

| Rule | Path | Status | Issue | Fix |
|---|---|---|---|---|
| no-docs-old-edits.md | .claude/rules/no-docs-old-edits.md | EXISTS | Obsolete: docs_old/ already deleted; rule references outdated structure | Delete; replace with legacy-doc-paths-forbidden.md covering all numbered folder patterns |

### 5. Amendments (2 items)

| Amendment | Path | Status | Migration | Resolution |
|---|---|---|---|---|
| FASE9F | docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md | EXISTS | 100% migrated to ADR-0005 + cave_rules.md | Delete file; keep in README as archived reference |
| FASE9G | docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md | EXISTS | ~30% migrated (MVP to combat_rules.md); ~70% reserved for Batch 2 | Move to docs/amendments/archived/; mark as historical, not canonical |

---

## Reference Path Audit

### Stale Paths Found (to be replaced globally)

| Old Path | New Path | Occurrences in Scope |
|---|---|---|
| docs/00_PROJECT/ | docs/project/ | DOCUMENT_GOVERNANCE.md (4x), templates (2x) |
| docs/03_SPECS/ | .specs/ | None expected in active docs |
| docs/04_REFINEMENTS/ | docs/refinements/ | SPEC_TEMPLATE.md (1x) |
| docs/05_VALIDATION/ | docs/validation/ | DOCUMENT_GOVERNANCE.md (2x) |
| docs/06_BACKLOG/ | docs/backlog/ | DOCUMENT_GOVERNANCE.md (1x) |
| docs/design/GDD_v2.7* | docs/product/GDD.md (if exists) or remove | CURRENT_STATE.md (1x), SPEC_TEMPLATE.md (1x), DOCUMENT_GOVERNANCE.md (1x) |
| docs_old/ | N/A (deleted) | Should not exist in active docs |

---

## Validation Status

### Harness Completeness (Pre-update)

| Component | Exists | References | Status |
|---|---|---|---|
| docs/project/DECISION_LOG.md | ✓ | Active in ADRs | COMPLETE |
| docs/decisions/ | ✓ | 9 ADRs | COMPLETE |
| docs/game_rules/ | ✓ | 12 game_rules | COMPLETE |
| .claude/rules/decision-and-game-rule-policy.md | ✓ | Referenced in RULES.md | COMPLETE |
| .claude/rules/RULES.md | ✓ | Rule 6 added | COMPLETE |
| .claude/skills/decision-rule-extraction/ | ✗ | NEED TO CREATE | PENDING |
| .claude/hooks/decision-rule-reference-guard.ps1 | ✗ | NEED TO CREATE | PENDING |
| .claude/rules/legacy-doc-paths-forbidden.md | ✗ | NEED TO CREATE | PENDING |
| .claude/rules/no-docs-old-edits.md (obsolete) | ✓ | SHOULD BE DELETED | PENDING |

### Template Field Audit

| Template | required_adrs | required_game_rules | validated_adrs | validated_game_rules | Status |
|---|---|---|---|---|---|
| SPEC_TEMPLATE.md | ✗ MISSING | ✗ MISSING | N/A | N/A | INCOMPLETE |
| VALIDATION_REPORT_TEMPLATE.md | N/A | N/A | ✗ MISSING | ✗ MISSING | INCOMPLETE |

---

## Amendment Status

### FASE9F — Cave Stable Run and Replay

- **Migration status:** 100% complete
  - ADR-0005-cave-stable-run-and-replay.md: ✓ created
  - docs/game_rules/cave_rules.md: ✓ created
  - Mojibake: ✓ corrected
  - No active references: ✓ confirmed
  
- **Resolution:** DELETE from docs/amendments/; keep archived note in README.md

### FASE9G — Enemy Combat Roles, AI, Status

- **Migration status:** Partial (MVP 30%; Batch 2 reserved 70%)
  - combat_rules.md: ✓ created with MVP content
  - Advanced content: Documented as Batch 2 input
  - No active references: ✓ confirmed

- **Resolution:** MOVE to docs/amendments/archived/; update README with "historical source for Batch 2"

---

## Required Edits Summary

### A. Reference Docs (3 updates)

1. **CURRENT_STATE.md**
   - Line 15: "Docs validation: 14/14" → "Docs validation: ✓ YES — 25+ checks"
   - Remove: "docs/design/GDD_v2.7*.md" from line 37
   - Add Key File Locations: DECISION_LOG, ADRs, game_rules (new rows in table)

2. **DOCUMENT_INDEX.md**
   - Add section: "## Decision Records"
   - Add section: "## Game Rules"
   - Mark amendments as "archived/historical" in Architecture Reference

3. **DOCUMENT_GOVERNANCE.md**
   - Replace all docs/00_PROJECT → docs/project
   - Replace all docs/06_BACKLOG → docs/backlog
   - Replace all docs/05_VALIDATION → docs/validation
   - Replace docs/design/ → docs/product/ (or remove)
   - Add section: "## 6. Decision and Game Rule Governance"

### B. Templates (2 updates)

1. **SPEC_TEMPLATE.md**
   - Add fields: `required_adrs: []`, `required_game_rules: []`
   - Update required_read paths (docs/00_PROJECT → docs/project, etc.)
   - Update validation line: "14/14" → current format

2. **VALIDATION_REPORT_TEMPLATE.md**
   - Add frontmatter: `validated_adrs: []`, `validated_game_rules: []`
   - Add section: "## ADRs / Game Rules Validated"
   - Update validation format: "N/14" → "PASS/FAIL"

### C. Harness Creation (2 new files)

1. **.claude/skills/decision-rule-extraction/SKILL.md**
   - Procedure for extracting decisions → ADRs, rules → game_rules
   - Stop conditions
   - Output checklist

2. **.claude/hooks/decision-rule-reference-guard.ps1**
   - Check: specs cite amendments as canonical (FAIL/WARN)
   - Check: spec missing required_adrs/required_game_rules (WARN)
   - Check: rule created outside docs/decisions/ or docs/game_rules/ (WARN)
   - Behavior: warning only; no automatic fixes

### D. Validator Update (1 edit)

1. **tools/docs/validate_docs.ps1**
   - Add 8+ checks for ADR/game_rules structure
   - Add check for path patterns
   - Add check for template fields

### E. Rules Update (2 changes)

1. **Delete:** `.claude/rules/no-docs-old-edits.md` (obsolete; docs_old already gone)
2. **Create:** `.claude/rules/legacy-doc-paths-forbidden.md` (covers all numbered folders + docs_old)

### F. Amendment Resolution (2 actions)

1. **FASE9F:** Delete from docs/amendments/; note in README
2. **FASE9G:** Move to docs/amendments/archived/; mark as non-canonical

---

## Success Criteria (Pre-Implementation)

| Criterion | Current | Target |
|---|---|---|
| CURRENT_STATE.md updated | NO | YES |
| DOCUMENT_INDEX.md updated | NO | YES |
| DOCUMENT_GOVERNANCE.md updated | NO | YES |
| SPEC_TEMPLATE.md updated | NO | YES |
| VALIDATION_REPORT_TEMPLATE.md updated | NO | YES |
| decision-rule-extraction skill exists | NO | YES |
| decision-rule-reference-guard hook exists | NO | YES |
| legacy-doc-paths-forbidden rule exists | NO | YES |
| no-docs-old-edits rule deleted | NO | YES |
| validate_docs.ps1 checks ADRs/game_rules | NO | YES |
| FASE9F resolved | NO | YES (deleted) |
| FASE9G resolved | NO | YES (archived) |
| All stale paths replaced | NO | YES |
| Docs validation PASS | TBD | YES |
| Zero runtime/Unity changes | ✓ | ✓ |

---

## Risk Assessment

| Risk | Severity | Impact | Mitigation |
|---|---|---|---|---|
| Stale paths not found | MEDIUM | Future specs/agents use outdated locations | Grep search for all old patterns before finalizing |
| Template fields not added to all specs | MEDIUM | New specs don't cite required ADRs/game_rules | Template update + validation check |
| Amendment deletion breaks historical references | LOW | Can't trace where content migrated from | Keep amendment README.md with migration map |
| Batch 2 content lost if FASE9G deleted | MEDIUM | Advanced combat design lost | Move to archived/ + mark clearly as "Batch 2 input" |

---

## Audit Conclusion

**Status:** COMPLETE — Ready for Phase 1-16 execution

**Blockers:** None

**Critical path:**
1. Update reference docs (CURRENT_STATE, DOCUMENT_INDEX, DOCUMENT_GOVERNANCE)
2. Update templates (SPEC_TEMPLATE, VALIDATION_REPORT_TEMPLATE)
3. Create harness components (skill, hook)
4. Update validator
5. Replace obsolete rule
6. Resolve amendments
7. Final validation

**Estimated effort:** 2-3 hours; 12+ file updates, 2 new files, 1 delete, 1 move

---

*Audit completed: 2026-06-01*  
*SPEC_DOCS_38 harness closure: 6 components pending; all documented; ready to implement.*
