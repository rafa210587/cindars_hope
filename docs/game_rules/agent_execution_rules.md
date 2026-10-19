---
doc_type: game_rule
status: accepted
domain: agent-operations
source_adrs:
  - ADR-0002
source_documents:
  - docs/decisions/ADR-0002-agent-context-minimum.md
  - .claude/rules/context-reading-policy.md
last_reviewed: 2026-06-01
---

# Agent Execution Rules

## Purpose

Defines what agents must read, must not read, and how they resolve conflicts during spec execution.

---

## Canonical Rules

### Rule: Agents Read Only Cited ADRs and Game Rules

- **Rule:** When an agent implements a spec, it reads:
  1. CLAUDE.md (or AGENTS.md) — project identity and rules
  2. CURRENT_STATE.md — active queue and blockers (~80 lines)
  3. The active spec itself
  4. ADRs explicitly listed in spec via `required_adrs: [ADR-0001, ...]`
  5. Game rules explicitly listed via `required_game_rules: [cave_rules.md, ...]`
  6. Files explicitly cited by the spec in its scope

- **Applies to:** All agent-run spec execution tasks
- **Exceptions:** Audit, reconciliation, wave planning may read PROJECT_LOG, ROADMAP by explicit human request

### Rule: Agents Must NOT Read by Default

- **Rule:** Do not read unless cited or justified:
  - PROJECT_LOG.md (500+ lines, historical only)
  - ROADMAP.md (planning only; use CURRENT_STATE for active queue)
  - All ADRs as a batch (read only cited ADRs)
  - All game_rules as a batch (read only cited game_rules)
  - GDD (use specs instead)
  - Old refinements (read only cited refinement)
  - Archived specs or validation reports

- **Why:** Heavy context increases token cost and drift risk; CURRENT_STATE.md (~80 lines) contains all operationally relevant information
- **Applies to:** All implementation, bug fix, and feature tasks

### Rule: Conflict Resolution Order

- **Rule:** If a conflict arises during execution:
  1. **Spec vs. CURRENT_STATE** → STOP and report to human. Spec must not conflict with CURRENT_STATE.
  2. **Spec vs. ADR/game_rule** → STOP and report. Do not implement until reconciled.
  3. **ADR vs. game_rule** → Game rule is current; ADR explains history. Update ADR or create superseding ADR.
  4. **Code vs. rule/ADR** → STOP if violation detected. Report what code contradicts.

- **Applies to:** All conflict detection during implementation

### Rule: Wiring and Bootstrap Context

- **Rule:** Agents may use GameBootstrap references for dependency injection setup. This is not gameplay communication; it is initialization.
- **Example:** `GameBootstrap.Instance.SpellDatabase` is a wiring reference, not a gameplay event.
- **Constraint:** Do not use bootstrap for runtime gameplay state queries; use game_rules and event bus instead.

### Rule: Spec Assumptions

- **Rule:** If a spec is self-contained (no `required_adrs` or `required_game_rules`), assume no external ADR/game_rule constraints apply (unless a rule forbids the action).
- **Example:** A UI spec without ADR citations can use direct component references locally; GameEventBus is still required for gameplay events.

---

## Execution Workflow

### Phase 0: Audit (Reading)

1. Read CLAUDE.md, AGENTS.md, CURRENT_STATE.md
2. Read spec frontmatter; extract `required_adrs`, `required_game_rules`
3. Read cited ADRs and game_rules
4. Read files in spec's declared scope
5. No other reads unless justified in stop condition

### Phase 1: Implementation (Coding)

1. Code changes in declared scope only
2. Check for ADR/game_rule violations
3. No changes outside scope without human approval
4. Commit progress with cited spec and any ADR references

### Phase 2-3: Validation

1. Run build validation (Phase 1)
2. Run Unity validators if applicable (Phase 2)
3. Run Play Mode if applicable (Phase 3)
4. Report status with clear phase completion

---

## Related ADRs

- [ADR-0002: Agent Context Minimum](../decisions/ADR-0002-agent-context-minimum.md)
- [ADR-0003: Spec Lifecycle](../decisions/ADR-0003-spec-lifecycle.md)
- [ADR-0004: Validation Evidence and Phase Gates](../decisions/ADR-0004-validation-evidence-phase-gates.md)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/decisions/ADR-0002-agent-context-minimum.md*
