# Document Governance — Cindar's Hope

> Rules for document types, states, and reading policy.

---

## 1. Document Types

| Type | Description | Examples |
|------|-------------|---------|
| `spec` | Executable contract defining a feature or task | docs/specs/a_implementar/*.md |
| `refinement` | Decision origin; not directly executable | docs/refinements/ |
| `validation` | Evidence of what was executed and the result | docs/validation/ |
| `architecture` | Technical structure and contracts | docs/architecture/ |
| `product` | GDD, MVP scope, gameplay loops | docs/product/ |
| `roadmap` | High-level planning; not a contract | docs/project/ROADMAP.md |
| `backlog` | Work items and priorities | docs/backlog/ |
| `release` | Acceptance reports and release notes | docs/release/ |
| `historical_log` | Session history and run logs | PROJECT_LOG.md, docs/implementation_runs/ |
| `agent_instruction` | Rules and protocols for agents | AGENTS.md, CLAUDE.md |
| `project_status` | Current state summary | docs/project/CURRENT_STATE.md |

---

## 2. Document States

| State | Meaning | Agents May Execute? |
|-------|---------|---------------------|
| `draft` | Not yet approved | NO |
| `proposed` | Under review | NO |
| `active` | Current and approved | YES (if it's a spec) |
| `ready` | Dependencies met, ready to execute | YES |
| `in_progress` | Being implemented now | YES (current session only) |
| `implemented` | Code complete; Phase 0-1 done | Informational only |
| `superseded` | Replaced by a newer document | NO |
| `archived` | Historical record | NO |
| `delete_candidate` | Marked for future deletion | NO |
| `do_not_execute` | Explicitly closed | NO |

---

## 3. Reading Policy by Agent Role

### Implementation Agent (executing a spec)

**Must read:**
```
1. AGENTS.md
2. docs/project/CURRENT_STATE.md
3. Active spec
4. Source files cited by spec
5. Prior validation report ONLY if listed as dependency
6. ADRs explicitly listed in spec's required_adrs field
7. Game rules explicitly listed in spec's required_game_rules field
```

**Must NOT read by default:**
```
- PROJECT_LOG.md
- docs/project/ROADMAP.md
- docs/product/GDD*.md
- docs/refinements/ (unless spec is ambiguous)
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/validation/* (unless spec cites them)
- All ADRs (read only those cited in spec)
- All game_rules (read only those cited in spec)
- docs/amendments/ (archived; content migrated to ADRs/game_rules)
```

### Audit Agent (reconciliation, investigation)

**May read:**
```
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/validation/*
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/specs/implementados/*
```

### Planning Agent (creating new specs)

**May read:**
```
- docs/00_PROJECT/ROADMAP.md
- docs/06_BACKLOG/current_backlog.md
- docs/design/GDD*.md
- docs/backlog/post_mvp_backlog.md
```

---

## 4. Spec Rules

1. Every spec must be in `docs/specs/a_implementar/` to be active
2. Specs in `docs/specs/a_implementar/reorg/` marked CLOSED → do not execute
3. Implemented specs go to `docs/specs/implementados/` with evidence
4. Specs covered by closeout package → mark as superseded; move in SPEC_DOCS_31
5. Never execute a spec from `docs_old/`
6. Specs must be self-contained; agents should not need refinement to execute a spec

---

## 5. Refinement Rules

1. Refinements are decision origin documents
2. Refinements are NOT directly executable
3. Refinement → must become a spec before execution
4. Agent executor reads refinement ONLY if spec is ambiguous and spec cites it
5. Refinement state flow: `inbox → accepted → implemented → archived`

---

## 6. Validation Rules

1. Validation reports are **evidence**, not execution queue
2. Agent reads validation report only if:
   - The active spec lists it as a dependency, OR
   - Performing audit/reconciliation
3. `dotnet build` passing ≠ Unity validation passing ≠ Play Mode passing
4. Never claim PASS for a phase not executed
5. Current validation status lives in `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md`

---

## 6.5. Human Test Scenario Rules

For any spec that changes gameplay, UI, save/load, cave, combat, events, or asset behavior at runtime:

1. **Mandatory deliverable:** Human test scenario file at `docs/validation/playmode/<spec_id>_human_test_scenario.md`
2. **Created by:** `/gameplay-test-scenario` skill (invoked before `/finish-spec`)
3. **Content:** Follows `PLAYMODE_TEST_SCENARIO_TEMPLATE.md` — feature summary, scenes, initial state, test scenarios, expected results, console expectations, pass/fail checklist
4. **Phase 3 evidence:** Human tester executes scenario, records results in checklist, signs off
5. **Closeout requirement:** Test scenario must be referenced in execution report Phase 3 section; without Phase 3 evidence, max promotion status is `BUILD_VALIDATED`, not `ACCEPTED`
6. **For docs-only or code-only specs:** Test scenario NOT required

---

## 7. Roadmap Rules

1. Roadmap is **planning-only** — not an implementation contract
2. Specs are the execution source of truth
3. Agents executing a spec must NOT read roadmap by default
4. If spec and roadmap conflict → follow spec
5. Roadmap is updated by human decision, not by agent implementation

---

## 8. Decision and Game Rule Governance

1. **ADRs (Architecture Decision Records)** live in `docs/decisions/`
   - ADRs explain **why** a decision was made
   - ADRs are numbered: ADR-0001 through ADR-NNNN
   - ADRs are indexed in `docs/project/DECISION_LOG.md`
   - Agents read only ADRs explicitly cited by the active spec in `required_adrs:` field

2. **Game Rules** live in `docs/game_rules/`
   - Game rules define **what is true now** (current behavior)
   - Game rules document operational, gameplay, and architectural constraints
   - Game rules are indexed in `docs/game_rules/GAME_RULES_INDEX.md` by domain
   - Agents read only game rules explicitly cited by the active spec in `required_game_rules:` field

3. **Amendments are historical**
   - All amendments (FASE9F, FASE9G, etc.) are migrated to ADRs and game_rules
   - `docs/amendments/` contains historical record only
   - Amendments are **not** canonical sources after migration
   - Specs and active docs must NOT cite amendments as canonical

4. **Conflict resolution**
   - **ADR vs. code:** ADR explains decision; if code violates it, stop and report
   - **Game rule vs. code:** Game rule is current; if code violates it, stop and report (code or rule is stale)
   - **ADR vs. game rule:** Game rule is current operational state; ADR explains history; update ADR or create superseding ADR if behavior changes

---

## 9. PROJECT_LOG.md Rules

See `docs/project/HISTORY_LOG_POLICY.md` for full policy.

Summary:
- Historical log only
- Not a source of truth for current status
- Agents read it only for audit, reconciliation, or explicit human request
- Use `docs/project/CURRENT_STATE.md` instead

---

## 10. Deletion Rules

A document can only be deleted in a **future spec** (SPEC_DOCS_31+) if ALL of:

1. Has a canonical substitute document
2. Is NOT validation evidence
3. Does NOT contain a unique decision not captured elsewhere
4. Is NOT referenced by AGENTS.md
5. Is NOT referenced by CURRENT_STATE.md
6. Is NOT an active spec
7. Has been listed in `docs/project/DOCUMENT_DELETE_CANDIDATES.md`
8. Has passed human review

In this spec (SPEC_DOCS_39), amendments that are 100% migrated may be deleted per SPEC_DOCS_38 resolution.

---

## 11. Conflict Resolution

| Conflict | Resolution |
|----------|-----------|
| spec vs roadmap | Follow spec |
| spec vs refinement | Follow spec |
| spec vs CURRENT_STATE | STOP — report inconsistency |
| spec vs ADR/game_rule | STOP — report inconsistency; do not implement |
| PROJECT_LOG vs CURRENT_STATE | Prefer CURRENT_STATE; report mismatch |
| Two specs conflict | STOP — report to human |

---

*Updated: 2026-06-01 (SPEC_DOCS_39 harness completion)*
