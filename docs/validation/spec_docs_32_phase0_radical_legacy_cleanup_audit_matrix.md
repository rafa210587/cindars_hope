---
doc_type: validation
status: evidence
spec_id: SPEC_DOCS_32
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# Phase 0 Audit Matrix — SPEC_DOCS_32 Radical Legacy Documentation Cleanup

> Comprehensive audit of legacy documentation for cleanup. No deletions until this matrix is complete and reviewed.

---

## 1. Summary

| Category | Exists | Problems Found | DELETE Candidates | BLOCKED | PROTECTED_BUT_SUSPECT |
|----------|--------|-----------------|------------------|---------|----------------------|
| Operations files | 11 | 2 have mojibake + contradict governance | 5 | 2 | 1 |
| Old roadmaps | 3 | v1.1 delta obsolete, v1.0 already deleted | 1 | 1 | 0 |
| Old backlogs | 7 | FASE6 items old, some mojibake | 3 | 0 | 1 |
| Architecture/design | 2 | Deltas missing base, contradictory | 2 | 0 | 0 |
| **TOTAL** | 23 | 15 identified issues | 11 candidates | 3 blocked | 2 suspect |

---

## 2. Operations Files (docs/operations/)

### 2.1 Strong DELETE Candidates

#### LLM_HANDOFF_INSTRUCTIONS.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/governance (obsolete) |
| Problems | Mojibake encoding corruption, contradicts CLAUDE.md/AGENTS.md, outdated handoff protocol |
| Contradicts new governance | ✓ YES (says read old context layers) |
| Mojibake | ✓ YES (Ã¢â‚¬â€, Ã‚ patterns) |
| Substituto canônico | CLAUDE.md, AGENTS.md, DOCUMENT_GOVERNANCE.md |
| Referências ativas | Found in: DOCUMENT_DELETE_CANDIDATES.md (itself), historical specs only |
| Conteúdo útil a preservar | None — pure governance, all superceded |
| **Decisão** | **DELETE** |
| Motivo | Corrupted, contradictory, superseded. No active governance reference. |

---

#### READING_MATRIX.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/governance (obsolete) |
| Problems | Mojibake encoding corruption, contradicts CLAUDE.md/AGENTS.md, references old registries |
| Contradicts new governance | ✓ YES (references obsolete spec registries, wrong reading layers) |
| Mojibake | ✓ YES (Ã‚, ÃƒÂ¡ patterns) |
| Substituto canônico | CLAUDE.md (section "Default Reads"), AGENTS.md, DOCUMENT_GOVERNANCE.md |
| Referências ativas | Found in: DOCUMENT_DELETE_CANDIDATES.md (itself), `.claude/rules/context-reading-policy.md` references it historically |
| Conteúdo útil a preservar | None — pure governance, all superceded by CURRENT_STATE.md + DOCUMENT_GOVERNANCE.md |
| **Decisão** | **DELETE** |
| Motivo | Corrupted, contradictory to new governance (says read PROJECT_LOG by default; new says don't), references deleted registries. |

---

#### LLM_HANDOFF_INSTRUCTIONS.md (duplicate if exists)

Check: Appears to be same as "READING_MATRIX alternate" or historical variant.

---

### 2.2 Conditional DELETE Candidates

#### AGENT_EXECUTION_PROTOCOL.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/governance (conflicting with new harness) |
| Problems | Mojibake corruption, contradicts CLAUDE.md/AGENTS.md context reading policy, says read PROJECT_LOG as Layer 0 (wrong) |
| Contradicts new governance | ✓ YES (Layer 0 says read PROJECT_LOG + IMPLEMENTATION_STATUS by default; new says read CURRENT_STATE only) |
| Mojibake | ✓ YES (Ã©, Ã‚, ÃƒÂ­ patterns throughout) |
| Substituto canônico | CLAUDE.md + AGENTS.md + `.claude/commands/` + `.claude/skills/` |
| Referências ativas | Minimal — found in DOCUMENT_DELETE_CANDIDATES.md (itself) and historical validation reports. No reference in current CLAUDE.md/AGENTS.md/CURRENT_STATE.md |
| Conteúdo útil a preservar | Camada 0 concept is now "Default Reads" in CLAUDE.md; Camada 1 is implicit in spec scope; Camada 2 is now "Conditional Reads". Rewrite and absorb into CLAUDE.md if needed. |
| **Decisión** | **REWRITE_THEN_DELETE** |
| Motivo | Contradictory + corrupted. Check if any rule in it is not in CLAUDE.md/AGENTS.md/.claude/rules/. Absorb useful rules into DOCUMENT_GOVERNANCE.md if any missing. Then delete. |
| **Action before delete** | 1. Search for unique rules/concepts in AGENT_EXECUTION_PROTOCOL.md. 2. Consolidate into DOCUMENT_GOVERNANCE.md if needed. 3. Remove reference from DOCUMENT_DELETE_CANDIDATES.md protected list. 4. Delete. |

---

#### SPECKIT_DRIFT_CONTROL_v1.0.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | governance/quality (spec consistency) |
| Problems | Possibly outdated by new harness; check if rules exist in `.claude/rules/` |
| Contradicts new governance | ✓ MAYBE (if it mandates old spec template or old change control that's now in `.claude/` rules) |
| Mojibake | ? (need to read to check) |
| Substituto canônico | `.claude/rules/spec-promotion-requires-evidence.md`, `.claude/rules/no-premature-acceptance-claims.md`, `docs/03_SPECS/SPEC_TEMPLATE.md` |
| Referências ativas | Unknown — needs search |
| Conteúdo útil a preservar | Possibly — depends on what's in the file |
| **Decisión** | **CONDITIONAL - INVESTIGATE** |
| Motivo | May have useful governance; need to read and compare against existing rules. If all content is covered by `.claude/rules/` + SPEC_TEMPLATE.md, delete. If has unique rule, absorb and rewrite. |

---

### 2.3 BLOCKED Candidates

#### HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/historical (merge protocol for specific branch event) |
| Problems | Specific to 2026-05-23 merge event; now historical |
| Substituto canônico | PROJECT_LOG.md (contains this session in session history) |
| Referências ativas | None expected (specific to historical event) |
| **Decisión** | **DELETE** (no blocker found) |
| Motivo | Merge-specific handoff, now historical. Content absorbed in PROJECT_LOG.md. |

---

#### CODEX_SPEC_EXECUTION_HARNESS.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/governance (Codex integration) |
| Problems | Unknown — requires reading |
| Likely status | May be related to old Codex agent harness, now replaced by `.claude/` structure. |
| **Decisión** | **INVESTIGATE** |
| Motivo | If describes old Codex harness flow now in CLAUDE.md/.claude/, can delete. If describes unique Codex-specific behavior, may need to preserve or rewrite. |

---

#### CODEX_ORCHESTRATION_PROMPT.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | operational/configuration (Codex orchestration) |
| Problems | May be outdated by new harness in `.claude/commands/` |
| Likely status | Old Codex prompt; may be superseded by new Agent SDK approach in SPEC_CLAUDE_31. |
| **Decisión** | **INVESTIGATE** |
| Motivo | If this is old Codex prompt logic now in `.claude/commands/`, delete. If Codex still uses it, keep but review for accuracy. |

---

### 2.4 Protected (do not delete without explicit authorization)

- `README.md` — useful folder documentation
- `SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` — process documentation, may still be active
- `SPEC_EVOLUTION_POLICY_v1.0.md` — governance, may still be active
- `FASE5_ambiente_v1.2.md` — historical FASE documentation

---

## 3. Roadmap Files (docs/roadmap/)

### 3.1 Strong DELETE Candidates

#### NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | planning/delta (superseded roadmap) |
| Problems | v1.1 is delta to v1.0 (already deleted). v1.0 no longer exists; delta reference is broken. |
| Substituto canônico | `docs/00_PROJECT/ROADMAP.md` (current active roadmap) |
| Referências ativas | None in active docs; found in NEXT_WAVES_ROADMAP_v1.0.md reference (but v1.0 deleted) |
| Conteúdo útil a preservar | None — delta without base is orphaned. Current roadmap supersedes. |
| **Decisión** | **DELETE** |
| Motivo | Orphaned delta (references deleted base). Current ROADMAP.md replaces. No active reference. |

---

### 3.2 BLOCKED Candidates

#### FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | planning/feature roadmap (FASE9F cave feature) |
| Problems | Specific to FASE9F; FASE9F is now in amendment + current ROADMAP.md |
| Substituto canônico | `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md` + current ROADMAP.md |
| Referências ativas | Possibly in AGENTS.md cave-stable-run-guard rule (needs check) |
| **Decisión** | **BLOCKED - Check AGENTS.md reference** |
| Motivo | If AGENTS.md or `.claude/rules/cave-stable-run.md` cites this roadmap, update reference to amendment first. Then delete if no remaining reference. |

---

### 3.3 Protected

- `README.md` — folder documentation

---

## 4. Backlog Files (docs/backlog/)

### 4.1 Strong DELETE Candidates

#### FASE6_FARM_backlog_v1.2.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | backlog/planning (FASE6 historical) |
| Problems | FASE6 items now completed or moved to current backlog; may have mojibake |
| Substituto canônico | `docs/06_BACKLOG/current_backlog.md` + `docs/backlog/post_mvp_backlog.md` |
| Referências ativas | None expected (historical FASE) |
| Conteúdo útil a preservar | Check: if any open item not in current backlog, absorb first. |
| **Decisión** | **DELETE** (after absorbing any open items) |
| Motivo | FASE6 historical; items now tracked in current backlog. |

---

#### FASE6_INDEX_global_v1.2.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | backlog/index (FASE6 historical) |
| Problems | FASE6 is old; index now superfluous with current_backlog |
| Substituto canônico | `docs/06_BACKLOG/current_backlog.md` |
| Referências ativas | None expected |
| **Decisión** | **DELETE** |
| Motivo | FASE6 index, historical. Current backlog replaces. |

---

#### FASE6_INDEX_global_v1.3_FASE9C_DELTA.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | backlog/delta (FASE6 to FASE9C historical delta) |
| Problems | Delta of obsolete FASE6. No longer relevant to current state. |
| Substituto canônico | Current backlog |
| Referências ativas | None expected |
| **Decisión** | **DELETE** |
| Motivo | FASE6 delta is historical; current state supersedes. |

---

### 4.2 Protected/Suspicious

#### FUTURE_IDEAS_TODO_v1.0.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | backlog/ideas (brainstorm) |
| Status | May still be active brainstorm. Check if it's referenced or if content moved to post_mvp_backlog. |
| **Decisión** | **PROTECTED_BUT_SUSPECT** |
| Motivo | If it's active ideas list, may be useful. If superseded by post_mvp_backlog.md, can delete. Investigate. |

---

#### reorg_architecture_residual_backlog.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | backlog/architectural debt |
| Status | May be active architectural debt list. Check against current_backlog. |
| **Decisión** | **PROTECTED_BUT_SUSPECT** |
| Motivo | Possibly still relevant if architectural cleanup is pending. Verify it's not blocking critical work. |

---

### 4.3 Protected

- `README.md` — folder documentation
- `post_mvp_backlog.md` — active current backlog
- `docs/06_BACKLOG/current_backlog.md` — active operational backlog

---

## 5. Architecture/Design Delta Files

### 5.1 Strong DELETE Candidates

#### docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | architecture/delta (v2.3 delta to v2.2 — but v2.2 deleted in SPEC_DOCS_31) |
| Problems | Base file (v2.2) already deleted. Delta is orphaned. |
| Substituto canônico | `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` (current active contracts) |
| Referências ativas | None expected (delta without base) |
| Conteúdo útil a preservar | Check if CORE_CONTRACTS already covers the content. If yes, delete. |
| **Decisión** | **DELETE** |
| Motivo | Orphaned delta (base v2.2 deleted). Current CORE_CONTRACTS supersedes. |

---

#### docs/design/GDD_v2.7_FASE9C_DELTA.md

| Property | Value |
|----------|-------|
| Exists | ✓ YES |
| Type | design/delta (v2.7 delta to v2.6 — but v2.6 deleted in SPEC_DOCS_31) |
| Problems | Base file (v2.6) already deleted. Delta is orphaned. Mojibake possible. |
| Substituto canônico | Need to consolidate or create `docs/01_PRODUCT/GDD.md` as current GDD. OR delete if content superseded by specs in implementados. |
| Referências ativas | May be referenced in old specs if they cite GDD v2.7. Check. |
| Conteúdo útil a preservar | Unknown — depends on if product content changed or if implementados specs already capture current state. |
| **Decisión** | **INVESTIGATE_THEN_DELETE_OR_CONSOLIDATE** |
| Motivo | Orphaned delta. Options: A) Consolidate v2.7 delta content into new GDD.md if still relevant. B) Delete if implementados specs capture all relevant product decisions. |

---

## 6. Deletion Summary Decision Table

| File | Decision | Why |
|------|----------|-----|
| docs/operations/LLM_HANDOFF_INSTRUCTIONS.md | DELETE | Corrupted, contradictory, no active ref |
| docs/operations/READING_MATRIX.md | DELETE | Corrupted, contradicts CLAUDE.md, references deleted registries |
| docs/operations/AGENT_EXECUTION_PROTOCOL.md | REWRITE_THEN_DELETE | Contradictory, needs rule audit first |
| docs/operations/SPECKIT_DRIFT_CONTROL_v1.0.md | INVESTIGATE | May have unique governance rules |
| docs/operations/HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md | DELETE | Merge-specific, now historical |
| docs/operations/CODEX_SPEC_EXECUTION_HARNESS.md | INVESTIGATE | Old Codex harness, may be superseded |
| docs/operations/CODEX_ORCHESTRATION_PROMPT.md | INVESTIGATE | Old Codex prompt, may be superseded |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md | DELETE | Orphaned delta (base deleted) |
| docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md | BLOCKED | Check AGENTS.md reference first |
| docs/backlog/FASE6_FARM_backlog_v1.2.md | DELETE | FASE6 historical, items in current backlog |
| docs/backlog/FASE6_INDEX_global_v1.2.md | DELETE | FASE6 index, superseded |
| docs/backlog/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md | DELETE | FASE6 delta, historical |
| docs/backlog/FUTURE_IDEAS_TODO_v1.0.md | PROTECTED_INVESTIGATE | May be active brainstorm |
| docs/backlog/reorg_architecture_residual_backlog.md | PROTECTED_INVESTIGATE | May be active debt tracking |
| docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md | DELETE | Orphaned delta (base deleted) |
| docs/design/GDD_v2.7_FASE9C_DELTA.md | INVESTIGATE_THEN_DELETE_OR_CONSOLIDATE | Orphaned; decide on GDD consolidation |
| docs/operations/README.md | KEEP | Folder documentation |
| docs/operations/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md | KEEP | Process documentation |
| docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md | KEEP | May be active governance |
| docs/operations/FASE5_ambiente_v1.2.md | KEEP | Historical FASE documentation |
| docs/roadmap/README.md | KEEP | Folder documentation |
| docs/backlog/README.md | KEEP | Folder documentation |

---

## 7. Investigations Needed Before Proceeding

1. **SPECKIT_DRIFT_CONTROL_v1.0.md** — Read and compare against `.claude/rules/`. If all rules exist, DELETE. If has unique rule, ABSORB and DELETE.

2. **AGENT_EXECUTION_PROTOCOL.md** — Read and compare against CLAUDE.md/AGENTS.md/DOCUMENT_GOVERNANCE.md/`. If all concepts exist in new governance, DELETE. If has unique rule, ABSORB and DELETE.

3. **CODEX_SPEC_EXECUTION_HARNESS.md** — Read and determine if old Codex logic or still-active. If old, DELETE. If active, UPDATE and KEEP.

4. **CODEX_ORCHESTRATION_PROMPT.md** — Read and determine if old Codex prompt or still-active. If old, DELETE. If active, UPDATE and KEEP.

5. **FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md** — Search for references in AGENTS.md, `.claude/rules/cave-stable-run.md`, any doc mentioning PR170 or PR192. If found, UPDATE reference to amendment first. Then DELETE.

6. **GDD_v2.7_FASE9C_DELTA.md** — Determine if consolidate into new GDD.md or DELETE. If consolidate, create GDD.md, absorb v2.7 delta, DELETE original.

7. **FUTURE_IDEAS_TODO_v1.0.md** — Check if content moved to post_mvp_backlog.md. If yes, DELETE. If no, content still active, KEEP and UPDATE.

8. **reorg_architecture_residual_backlog.md** — Check if items still blocking or resolved. If blocking, KEEP. If resolved, DELETE.

---

## 8. Next Steps

**If this audit is approved:**

1. Conduct the 8 investigations above
2. Re-run this matrix with investigation results
3. Execute deletions (only files marked DELETE after investigations)
4. Update DOCUMENT_DELETE_CANDIDATES.md
5. Update DOCUMENT_INDEX.md
6. Run docs validation
7. Create execution report

---

*Phase 0 Audit Matrix Complete: 2026-06-01*
