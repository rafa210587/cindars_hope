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
| Legacy docs validation | ✓ YES — 25+ checks (SPEC_DOCS_37) |
| Generated specs validation | ⚠ RUN_WITH_ISSUES — naming/header mismatch (blocks WAVE 02+) |
| Legacy spec cleanup | ✓ YES — 7 specs absorbed (2026-06-07) |
| MVP Phase 2-3 (Unity/Play Mode) | ✗ DEFERRED TO FINAL ACCEPTANCE | Not required for WAVE 02 implementation start |
| MVP final accepted | ✗ NOT YET — pending Phase 2-3 execution |

---

## What to Read for Implementation Tasks

```
1. AGENTS.md
2. docs/project/CURRENT_STATE.md  (this file)
3. Active spec file
4. Source files referenced by the spec
5. Immediately prior validation report ONLY if listed as a dependency
```

## What NOT to Read by Default

```
- PROJECT_LOG.md  (use only for audit/reconciliation/regression)
- docs/project/ROADMAP.md  (planning only)
- docs/refinements/  (read only if spec is ambiguous)
- docs/specs/a_implementar/reorg/  (CLOSED — do not execute)
- docs/validation/spec_mvp_closeout_*  (evidence; read only if explicitly listed)
- docs/IMPLEMENTATION_STATUS.md  (broad status; read only for audit)
```

---

## Claude Code Harness

| Item | Status |
|------|--------|
| CLAUDE.md | Short router (~80 lines) — SPEC_CLAUDE_31 |
| AGENTS.md | Multi-agent rules — SPEC_CLAUDE_31 |
| `.claude/rules/` | 16 rules active — SPEC_DOCS_38/39 (incl. decision-and-game-rule-policy, legacy-doc-paths-forbidden) |
| `.claude/settings.json` | Updated with canonical paths and new hooks — SPEC_DOCS_39D |
| `.claude/commands/` | 11 commands (8 updated, 3 created) — SPEC_CLAUDE_31 |
| `.claude/skills/` | 15 skills (incl. decision-rule-extraction) — SPEC_DOCS_39B/39E |
| `.claude/agents/` | 7 agents (2 updated, 2 created) — SPEC_CLAUDE_31 |
| `.claude/hooks/` | 14 hooks (incl. decision-rule-reference-guard) — SPEC_DOCS_39B/39D |
| Decision Records | 9 ADRs (ADR-0001 to ADR-0009) — SPEC_DOCS_38 |
| Game Rules | 12 game_rules documents — SPEC_DOCS_38 |
| Docs validation | 25+ checks (validate_docs.ps1) — SPEC_DOCS_37/39C |
| Default context | `CURRENT_STATE.md` (not PROJECT_LOG.md) |
| Spec promotion | Phase-gated via `/finish-spec` |

---

## Active Spec Queue

| Spec | Status | Notes |
|------|--------|-------|
| SPEC_DOCS_31 | COMPLETE | Safe Batch 1 deletion (20 files); commit 956e4d1 |
| SPEC_DOCS_32 | PHASE 1 COMPLETE | Phase 0-1 complete: 6 obsolete files deleted; AGENTS.md updated; commit af8bb1b |
| SPEC_DOCS_33 | COMPLETE | Agent_prompts + agent_packages cleanup (39 files); commit 5c5ecae |
| SPEC_DOCS_34 | COMPLETE | Final legacy cleanup: 65 files (ARCH delta + orquestrador/); commit 3c6dda9 |
| SPEC_DOCS_35 | COMPLETE | Canonical folder consolidation: 5 numbered folders → canonical (14 files moved); commit [migração] |
| SPEC_DOCS_36 | COMPLETE | Final root/legacy cleanup: 159 files deleted (5 root + 86 docs_old + 68 prompts/templates/orquestrador); commit 9c4ffa5 |
| SPEC_DOCS_37 | COMPLETE | Refinements/specs/validation sweep: 12 references fixed, 14 refinements archived, 4 deleted, validation enhanced (25+ checks); commits b9ae4d5, 45e299f |
| SPEC_18-28 Phase 2-3 | PENDING HUMAN | Play Mode validation; requires local Unity Editor |
| SPEC_29 Phase 2-3 | PENDING HUMAN | Final acceptance; blocked on Phase 2-3 above |
| WAVE 00.04 Existing Implementation Audit | COMPLETE / BUILD_VALIDATED | Governance audit only; no code/gameplay changes; commit ee1c0fb+ |
| WAVE 01 Hardening & Quality Gate | CODE_COMPLETE | 9 specs executed (00.04, 01.01-01.07, 01Q); BUILD_VALIDATED; 36 EditMode tests compile successfully (0E/0W) |
| WAVE 02 Time/Calendar/Lunar/Weather/Rain/Save/Festivals | COMPLETED_WITH_DEFERRED_UI | 7 of 8 specs BUILD_VALIDATED; 1 deferred (Spec 3 Calendar UI); all runtime systems complete |
| WAVE 03 Quests/Objectives/Events | COMPLETED_WITH_DEFERRED_UI | 8 of 8 runtime specs BUILD_VALIDATED; 4 future specs blocked; quest system foundation complete |
| WAVE 04-12 Core Gameplay | **READY_FOR_WAVE_04** | 77 remaining specs; WAVE 02-03 foundation (time/quests/calendar/festivals/save) ready for integration |
| WAVE 17-24 Future | BLOCKED INTENTIONALLY | 32+ future/expansion specs marked; pets (WAVE 23) blocked as HOLD/BLOCKED_SCOPE |

---

## Blockers for WAVE 02 Start

1. **Generated specs validation:** Naming/header validator issues (wave-based pattern `NN_spec_*`) must resolve before WAVE 02+ runtime
2. **Quality gate completion:** WAVE 01Q EditMode tests must compile (RESOLVED 2026-06-08); human execution deferred to acceptance gate

## Deferred Gates (do NOT block WAVE 02 implementation start)

- **Human validation (Phase 2-3):** Required for final MVP acceptance; does NOT block WAVE 02 implementation
- **Unity Play Mode execution:** Required for final MVP acceptance; does NOT block WAVE 02 implementation

---

## Key File Locations

| What | Where |
|------|-------|
| Agent rules | `CLAUDE.md` (router), `AGENTS.md` (rules), `.claude/rules/RULES.md` |
| Active specs | `docs/specs/a_implementar/` (147 wave-based) |
| Absorbed legacy specs | `docs/specs/absorvidas/legacy_pre_wave_reconciliation/` (7 specs) |
| Commands | `.claude/commands/` (11 commands) |
| Skills | `.claude/skills/` (14 skills) |
| Validation evidence | `docs/validation/spec_mvp_closeout_*.md` |
| MVP acceptance | `docs/release/MVP_ACCEPTANCE_REPORT.md` |
| Post-MVP backlog | `docs/backlog/post_mvp_backlog.md` |
| Last validation status | `docs/validation/current/LAST_VALIDATION_STATUS.md` |
| Spec template | `docs/specs/_templates/SPEC_TEMPLATE.md` |
| Document governance | `docs/project/DOCUMENT_GOVERNANCE.md` |
| Decision log | `docs/project/DECISION_LOG.md` |
| ADRs | `docs/decisions/` (9 ADRs, ADR-0001 to ADR-0009) |
| Game rules | `docs/game_rules/GAME_RULES_INDEX.md` |

---

## Reading Policy for ADRs and Game Rules

- **Agents do NOT read all ADRs or all game_rules by default.**
- **Agents read only ADRs/game_rules explicitly listed in the active spec's `required_adrs:` and `required_game_rules:` fields.**
- **If a spec conflicts with an ADR or game_rule, STOP and report the conflict. Do not implement until reconciled.**
- **See `DECISION_LOG.md` for index of all ADRs and reading policy details.**
- **See `docs/game_rules/GAME_RULES_INDEX.md` for index of all game rules.**

---

## Do NOT Execute

```
- docs/specs/absorvidas/legacy_pre_wave_reconciliation/** (legacy specs — moved 2026-06-07)
- docs/specs/a_implementar/reorg/SPEC_00-12  (CLOSED per README_STATUS.md)
- SPEC_18-29 again  (already executed; Phase 0-1 complete)
```

**Legacy specs moved (2026-06-07):** All 7 legacy specs from pre-wave era absorbed into new wave-based specs.  
See `docs/specs/absorvidas/legacy_pre_wave_reconciliation/LEGACY_SPECS_CROSSWALK.md`

---

## Conflict Resolution Rules

```
spec + roadmap conflict  → follow spec
spec + refinement conflict → follow spec
spec + CURRENT_STATE conflict → STOP and report inconsistency
PROJECT_LOG + CURRENT_STATE conflict → prefer CURRENT_STATE; report mismatch
```

---

*Last updated: 2026-06-08 (WAVE 02 core foundation: time/calendar/lunar/weather)*  
*Next update: after WAVE 02 deferred specs (3, 5, 6, 8) or WAVE 03 start*
