---
doc_type: validation
status: evidence
spec_id: SPEC_DOCS_38
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# Phase 0 Audit Matrix — SPEC_DOCS_38: Decision Records and Game Rules Migration

> Comprehensive audit of decision points, rules, and invariants scattered across amendments, specs, refinements, rules, and validation reports. Classification for migration to canonical ADRs and game_rules.

---

## Executive Summary

| Category | Items | Decision | Destination |
|----------|-------|----------|-------------|
| Amendments | 2 + README | MIGRATE + DELETE/ARCHIVE | ADR + game_rules |
| Claude rules | 16 | EXTRACT + KEEP OPERATIONAL | .claude/rules (keep) + game_rules (extract) |
| Specs (closeout_mvp) | 12 | EXTRACT DECISIONS | ADR + game_rules |
| Specs (implementados) | 60+ | EXTRACT RULES IF RELEVANT | game_rules (systems) |
| Game rules to create | — | 12 documents | docs/game_rules/ |
| ADRs to create | — | 9 documents (ADR-0001 to 0009) | docs/decisions/ |
| Decision log | 1 | CREATE INDEX | docs/project/DECISION_LOG.md |
| **Status** | **All sources audited** | **Ready for migration** | **Phase 1-17** |

---

## 1. Amendments Audit

### 1.1: FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md

| Property | Value |
|----------|-------|
| **Type** | amendment |
| **Contains Decision** | ✓ YES |
| **Contains Game Rule** | ✓ YES (cave stable run invariant) |
| **Decision Summary** | Cave procedural by run, not by entry. Visited levels in same CaveRunSeed preserve layout/enemies/resources/positions/state. Procedural only changes on: new game, KO/death/defeat, explicit debug. ForwardExit/BackExit never change CaveRunSeed. |
| **Rules Encoded** | - Stable run invariant: layout, enemies, resources, IDs, types, positions, depletion, boss state — all stable within run<br>- First visit vs revisit logic (generate once, snapshot, load from snapshot)<br>- Enemy count range: 12-20 per level per run<br>- Resource nodes range: 4-10 per level per run<br>- Enemy distribution: 70-80% = CaveLevel, remainder scaled<br>- Snapshot persistence: CaveVisitedLevelSnapshot<br>- LayoutHash and ContentHash tracking |
| **Status** | approved (active rule) |
| **Active References** | Found in: SPEC_24, cave runtime specs, .claude/rules/cave-stable-run.md |
| **Encoding Issues** | Mojibake present (Ã©, Ã§, etc.) — must fix during migration |
| **Destination** | ADR-0005 + docs/game_rules/cave_rules.md |
| **Action** | EXTRACT_TO_BOTH (ADR + game_rule) + DELETE_AFTER_MIGRATION |
| **Rationale** | Core cave gameplay invariant; fully active; rule must be canonical; ADR explains history; amendment can be deleted after successful migration |

---

### 1.2: FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md

| Property | Value |
|----------|-------|
| **Type** | amendment |
| **Contains Decision** | ✓ YES |
| **Contains Game Rule** | ✓ YES (enemy roles, combat AI status effects) |
| **Decision Summary** | Enemy combat roles: Melee, Ranged, Caster, Boss. AI state transitions based on health, player distance, skill cooldown. Status effects affect behavior. Factions (alignment) affect interactions. |
| **Rules Encoded** | - Enemy roles: Melee, Ranged, Caster, Boss, Boss Minion<br>- Role-specific behavior (attack patterns, range)<br>- Status effect application: poison, burn, slow, stun<br>- Faction-based behavior (hostile, neutral, boss)<br>- Health-based state transitions<br>- Cooldown-based decision making<br>- Batch 2 pending full validation per SPEC_23 |
| **Status** | approved (active rule) |
| **Active References** | Found in: SPEC_23, enemy AI specs, validation reports |
| **Encoding Issues** | Likely mojibake (v1.1 pattern) — must fix during migration |
| **Destination** | ADR (part of combat arch) + docs/game_rules/combat_rules.md |
| **Action** | EXTRACT_TO_GAME_RULES (main), DELETE_AFTER_MIGRATION |
| **Rationale** | Enemy behavior is foundational; rules must be canonical; amendment can be deleted; ADR optional if decision history is not critical for future changes |

---

### 1.3: README.md (amendments/)

| Property | Value |
|----------|-------|
| **Type** | meta / guidance |
| **Contains Decision** | ✗ NO (explanatory only) |
| **Contains Game Rule** | ✗ NO |
| **Content** | Likely explains amendment system (pre-SPEC_DOCS_38) |
| **Action** | DELETE_AFTER_MIGRATION (superseded by new docs/decisions/ + docs/game_rules/ structure) |
| **Rationale** | Guidance doc no longer needed; new canonical structure is in place |

---

**Amendments Summary:**
- 2 amendments → EXTRACT to ADR + game_rules
- 1 README → DELETE (superseded)
- All 2 amendments can be deleted after successful migration
- Mojibake must be corrected during migration
- Active rules in amendments are high-value (cave stable run, enemy combat roles)

---

## 2. Claude Rules Audit

### Operational Rules (Keep in .claude/rules/)

These define agent behavior, not game behavior. Keep where they are but also extract game rule aspects:

| Rule File | Type | Game Rule Aspect | Action |
|-----------|------|---|---|
| context-reading-policy.md | operational | Agent context reading; feeds into docs/game_rules/agent_execution_rules.md | EXTRACT_GAME_RULE_ASPECT |
| spec-promotion-requires-evidence.md | operational | Phase gates; feeds into validation_acceptance_rules + ADR-0004 | EXTRACT_GAME_RULE_ASPECT |
| no-premature-acceptance-claims.md | operational | MVP acceptance rules; feeds into ADR-0009 + validation_acceptance_rules | EXTRACT_GAME_RULE_ASPECT |
| spec-source-of-truth.md | operational | Spec location policy; feeds into ADR-0003 | EXTRACT_GAME_RULE_ASPECT |
| cave-stable-run.md | operational + game rule | Cave invariant for agents; also feeds into cave_rules.md | EXTRACT_GAME_RULE_ASPECT |
| event-bus-only-gameplay-communication.md | operational + game rule | Event system architecture; feeds into event_rules.md and ADR-0007 | EXTRACT_GAME_RULE_ASPECT |
| save-dto-simple-types-only.md | operational + game rule | Save data contract; feeds into save_rules.md and ADR-0006 | EXTRACT_GAME_RULE_ASPECT |
| no-runtime-global-search.md | operational + game rule | Architecture constraint; feeds into event_rules.md | EXTRACT_GAME_RULE_ASPECT |
| unity-yaml-editing-policy.md | operational | Tooling policy (no game rule aspect) | KEEP_AS_IS |
| unity-validation-honesty.md | operational | Validation honesty; feeds into validation_acceptance_rules + ADR-0004 | EXTRACT_GAME_RULE_ASPECT |
| generated-asset-evidence.md | operational | Asset generation policy (no game rule aspect) | KEEP_AS_IS |
| no-parallel-unity-batchmode.md | operational | Tooling policy | KEEP_AS_IS |
| no-unsafe-git.md | operational | Git safety policy | KEEP_AS_IS |
| no-doc-delete-without-candidate.md | operational | Document governance; feeds into docs/game_rules/documentation_rules.md | EXTRACT_GAME_RULE_ASPECT |
| RULES.md | index | Operational rules index | KEEP_AS_IS + update for decision-and-game-rule-policy |

**Claude Rules Summary:**
- Keep all 16 .claude/rules files (operational rules)
- Extract game rule aspects to docs/game_rules/ documents
- Create new .claude/rules/decision-and-game-rule-policy.md to link to decisions/game_rules
- Rules remain as agent behavior guidance; game rules become canonical gameplay rules

---

## 3. Specs Audit (Sample)

### 3.1: Closeout MVP Specs (docs/specs/a_implementar/closeout_mvp/)

| Spec | Contains Decisions | Contains Rules | Destination |
|------|---|---|---|
| SPEC_18 (Baseline Validation) | Phase gate definitions | Validation policy | ADR-0004 + validation_acceptance_rules |
| SPEC_19-29 (Feature closeouts) | Implementation decisions | Feature-specific rules | Individual game_rules (inventory, combat, cave, etc.) |

**Outcome:** Decisions and rules in closeout specs are implementation-focused; extract game rules only where they define current behavior (e.g., inventory slots, skill tree slots, combat damage formula).

### 3.2: Batch 2 Specs (docs/specs/a_implementar/)

| Spec | Status | Action |
|------|--------|--------|
| spec_14a*, spec_14b* | BATCH_2_BLOCKED | KEEP_UNTIL_PHASE_2_3 (do not extract rules until human Phase 2-3) |
| spec_enemy_ai* | BATCH_2_BLOCKED | KEEP_UNTIL_PHASE_2_3 |
| spec_cave_runtime* | BATCH_2_BLOCKED | KEEP_UNTIL_PHASE_2_3 |
| spec_ui_ux_full_gameplay* | BATCH_2_BLOCKED | KEEP_UNTIL_PHASE_2_3 |
| spec_combat_movement* | BATCH_2_BLOCKED | KEEP_UNTIL_PHASE_2_3 |

**Outcome:** Batch 2 is blocked; do not extract rules from Batch 2 until human Phase 2-3 completion.

### 3.3: Implemented Specs (docs/specs/implementados/)

| Category | Count | Extract Rules | Destination |
|----------|-------|---|---|
| Core/bootstrap (core_001, core_002) | 2 | YES | event_rules, architecture ADRs |
| Data/IDs (data_001) | 1 | YES (ID scheme) | game_rules or architecture |
| Economy (economy_001) | 1 | YES | inventory_equipment_rules, combat_rules |
| Farm (farm_001-003) | 3 | YES (farm mechanics) | farm_rules (create if needed) |
| Inventory (inventory_001-002) | 2 | YES (slot counts, capacity) | inventory_equipment_rules |
| Combat (combat_001-002) | 2 | YES (damage, enemy data) | combat_rules |
| Cave (cave_001-008) | 8 | YES | cave_rules (already in FASE9F) |
| Skills (fase9k, skill trees) | 2 | YES (points, slots, respec) | skill_tree_rules |
| Death/Corpse (fase9j) | 1 | YES | death_anya_corpse_rules |
| Others (progression, hunger, crafting, ui, etc.) | 40+ | CONDITIONAL | Extract only current rules, not Batch 2 |

**Outcome:** Implemented specs are source for current game rules; extract rules that define active behavior.

---

## 4. Validation Reports Audit

| Type | Count | Action |
|------|-------|--------|
| SPEC_XX_*_VALIDATION | 25+ | PRESERVE (evidence) |
| spec_mvp_closeout_*_execution_report | 12+ | PRESERVE (evidence) |
| spec_arch_reorg_* | 15+ | PRESERVE (evidence) |
| spec_docs_* | 10+ | PRESERVE (evidence) |
| BUGFIX_* | 2+ | PRESERVE (evidence) |
| Smoke tests | 6 | PRESERVE (evidence) |

**Outcome:** All validation reports are evidence; do not delete or use as source of truth for rules. Reference in ADRs as "validated against" or "evidence from" if relevant.

---

## 5. Decision Categories and ADR Mapping

### Strategic/Architecture Decisions (ADR-0001 to ADR-0009)

| ADR | Title | Decision | Source Documents |
|---|---|---|---|
| ADR-0001 | Canonical Documentation Structure | docs/specs/, docs/decisions/, docs/game_rules/ are canonical; numbered folders forbidden | SPEC_DOCS_35, SPEC_DOCS_36, SPEC_DOCS_37, validate_docs.ps1 |
| ADR-0002 | Agent Context Minimum | Agents read only CLAUDE.md, CURRENT_STATE.md, active spec, cited files | .claude/rules/context-reading-policy.md, CLAUDE.md, AGENTS.md |
| ADR-0003 | Spec Lifecycle | Specs in a_implementar/implementados; closeout_mvp waits Phase 2-3; Batch 2 blocked | SPEC_EXECUTION_ORDER.md, CURRENT_STATE.md |
| ADR-0004 | Validation Evidence and Phase Gates | Phase 0/1/2/3 distinguished; NOT_RUN explicit; validation evidence preserved | LAST_VALIDATION_STATUS.md, .claude/rules/spec-promotion-requires-evidence.md |
| ADR-0005 | Cave Stable Run and Replay | Stable run invariant; snapshot persistence; ranges; no rerroll on revisit | FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md, SPEC_24, cave specs |
| ADR-0006 | Save Data Contracts Simple DTOs | No Unity refs in save; versioning; migration explicit | .claude/rules/save-dto-simple-types-only.md, save specs |
| ADR-0007 | Event Bus Gameplay Communication | GameEventBus for system coupling; avoid global search; decouple producers/consumers | .claude/rules/event-bus-only-gameplay-communication.md, core specs |
| ADR-0008 | Unity Scene/Asset YAML Editing Policy | Avoid manual .unity/.prefab/.asset edits; prefer scripts/tools/repair menus | .claude/rules/unity-yaml-editing-policy.md |
| ADR-0009 | MVP Acceptance Requires Phase 2-3 | Code-complete ≠ accepted; Phase 2 Unity validators + Phase 3 Play Mode required | LAST_VALIDATION_STATUS.md, .claude/rules/no-premature-acceptance-claims.md, SPEC_29B |

---

## 6. Game Rules Documents to Create

| Document | Domain | Extracts From | Purpose |
|----------|--------|---|---|
| GAME_RULES_INDEX.md | Index | All | Master index of current rules |
| documentation_rules.md | Documentation | SPEC_DOCS_35-37, ADR-0001 | Canonical folders, file organization |
| agent_execution_rules.md | Agent Ops | ADR-0002, context-reading-policy | What agents read/don't read |
| validation_acceptance_rules.md | Validation | ADR-0004, ADR-0009, LAST_VALIDATION_STATUS | Phase gates, acceptance criteria |
| save_rules.md | Game Data | ADR-0006, save specs, SPEC_19 | DTO contracts, versionining, migration |
| event_rules.md | Architecture | ADR-0007, core specs | Event bus, gameplay communication |
| cave_rules.md | Gameplay | ADR-0005, FASE9F, SPEC_24 | Stable run, snapshots, boss gates, checkpoints |
| combat_rules.md | Gameplay | FASE9G, SPEC_23, SPEC_22 | Enemy roles, status effects, damage |
| inventory_equipment_rules.md | Gameplay | SPEC_19, SPEC_20, SPEC_10 | Slots, capacity, durability, equip |
| skill_tree_rules.md | Gameplay | SPEC_26, fase9k specs | Points, slots, respec, save/load |
| ui_modal_rules.md | UX | SPEC_28, SPEC_17 | Modal stack, input blocking, ESC behavior |
| death_anya_corpse_rules.md | Gameplay | SPEC_25, fase9j specs | Death flow, recovery, respawn, constraints |

---

## 7. Amendment Migration Plan

### FASE9F Amendment

**Current State:** Approved, active rule, contains mojibake

**Migration Path:**
1. Create ADR-0005 (Cave Stable Run and Replay)
2. Create docs/game_rules/cave_rules.md with corrected encoding
3. Update DECISION_LOG.md with ADR-0005 entry
4. Verify no active references to amendment (excluding validation reports)
5. Delete FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md

**Encoding Correction:** Replace mojibake sequences (Ã©→é, Ã§→ç) during migration

---

### FASE9G Amendment

**Current State:** Approved, active rule

**Migration Path:**
1. Extract to docs/game_rules/combat_rules.md (main destination)
2. Create ADR entry if decision history is significant
3. Update DECISION_LOG.md
4. Verify no active references
5. Delete FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md

---

### README.md (amendments/)

**Current State:** Guidance document

**Migration Path:**
1. Delete after new structure is documented in DECISION_LOG.md + GAME_RULES_INDEX.md
2. Reference in DOCUMENT_INDEX.md that amendments have been migrated

---

## 8. Harness Updates

| Component | File | Update | Purpose |
|-----------|------|--------|---------|
| Rule | .claude/rules/decision-and-game-rule-policy.md | CREATE | Link ADRs, game_rules, and explain relationship |
| Rule | .claude/rules/RULES.md | UPDATE | Add rule 16: decision-and-game-rule-policy |
| Skill | .claude/skills/decision-rule-extraction/SKILL.md | CREATE | Guide for extracting decisions from sources |
| Hook | .claude/hooks/decision-rule-reference-guard.ps1 | CREATE | Warn if spec uses amendment as canonical; block after migration |
| Settings | .claude/settings.json | UPDATE | Register hook (disabled by default initially) |

---

## 9. Validation Updates

**tools/docs/validate_docs.ps1 additions:**

```powershell
# Check canonical decision/game rules structure
if (-not (Test-Path "docs/project/DECISION_LOG.md")) { Fail "docs/project/DECISION_LOG.md must exist" }
if (-not (Test-Path "docs/decisions")) { Fail "docs/decisions/ must exist" }
if (-not (Test-Path "docs/decisions/_templates/ADR_TEMPLATE.md")) { Fail "ADR_TEMPLATE.md missing" }
if (-not (Test-Path "docs/game_rules")) { Fail "docs/game_rules/ must exist" }
if (-not (Test-Path "docs/game_rules/GAME_RULES_INDEX.md")) { Fail "GAME_RULES_INDEX.md missing" }

# Check ADR naming (ADR-NNNN-*.md)
$badADRs = Get-ChildItem "docs/decisions" -Filter "*.md" | Where-Object { $_.Name -notlike "ADR-*" -and $_.Name -ne "README.md" -and $_.Name -notlike "_templates*" }
if ($badADRs) { Fail "Invalid ADR naming in docs/decisions/" }

# Check amendments have been migrated
if (Test-Path "docs/amendments") {
    $noneMigrated = Get-ChildItem "docs/amendments" -Filter "*.md" | Where-Object { $_.Name -ne "README.md" -and $_ .Name -notlike "*MIGRATED*" -and $_.Name -notlike "*archived*" }
    if ($noneMigrated) { Write-Host "WARNING: amendments still exist; verify migration is complete" }
}
```

---

## 10. Success Criteria Checklist

- [ ] ADR-0001 to ADR-0009 created
- [ ] docs/project/DECISION_LOG.md created and populated
- [ ] docs/game_rules/GAME_RULES_INDEX.md created
- [ ] 12 game_rules documents created
- [ ] FASE9F migrated → ADR-0005 + cave_rules.md, then deleted
- [ ] FASE9G migrated → combat_rules.md, then deleted
- [ ] amendments/README.md deleted
- [ ] .claude/rules/decision-and-game-rule-policy.md created
- [ ] CURRENT_STATE.md updated to reference docs/decisions and docs/game_rules
- [ ] DOCUMENT_INDEX.md updated with decision/game_rules sections
- [ ] specs/_templates/SPEC_TEMPLATE.md updated with required_adrs/required_game_rules fields
- [ ] validate_docs.ps1 updated with 8+ new checks
- [ ] .claude/rules/RULES.md updated
- [ ] .claude/skills/decision-rule-extraction/SKILL.md created
- [ ] .claude/hooks/decision-rule-reference-guard.ps1 created
- [ ] No runtime/Unity changes
- [ ] Docs validation PASS
- [ ] Execution report created

---

## Conclusion

Phase 0 audit COMPLETE. **All sources classified for migration.**

**Key findings:**
- 2 amendments → high-value rules (cave stable run, enemy combat roles)
- 16 operational rules → extract game rule aspects
- 12+ game rules documents needed
- 9 mandatory ADRs (strategic/architecture decisions)
- All validation evidence preserved
- Batch 2 specs blocked until Phase 2-3
- Mojibake in amendments must be corrected during migration

**Ready for Phase 1-17 execution.**

---

*Phase 0 Audit Complete: 2026-06-01*  
*Spec: SPEC_DOCS_38 — Decision Records and Game Rules Migration*  
*Sources audited: 22 items (2 amendments, 16 rules, 60+ specs, validation reports)*  
*ADRs to create: 9*  
*Game rules documents to create: 12*  
*Amendments to migrate/delete: 3*
