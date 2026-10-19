---
doc_type: adr
status: accepted
adr_id: ADR-0002
title: Agent Context Minimum
date: 2026-06-01
source_documents:
  - CLAUDE.md
  - AGENTS.md
  - .claude/rules/context-reading-policy.md
supersedes: []
superseded_by: []
applies_to:
  - agent-execution
---

# ADR-0002 — Agent Context Minimum

## Status

**accepted**

## Context

Agents executing specs need context but not everything. Reading heavy files (PROJECT_LOG.md 500+ lines, full IMPLEMENTATION_STATUS.md, all amendments, all ADRs, all validation reports) increases token cost and drift risk from stale data.

## Decision

Agent reading policy: **minimal by default, explicit when needed.**

### Must Read (Always)
- AGENTS.md or CLAUDE.md (harness rules)
- docs/project/CURRENT_STATE.md (active queue, blockers)
- Active spec file
- Files explicitly cited by the spec

### Conditional Read (Only if Spec Lists Them)
- Specific ADR (if spec has `required_adrs: [ADR-0005]`)
- Specific game rule (if spec has `required_game_rules: [cave_rules.md]`)
- Specific refinement (if spec cites it)
- Prior validation report (if spec lists as dependency)

### Must NOT Read (By Default)
- PROJECT_LOG.md (history only)
- docs/project/ROADMAP.md (planning, not execution)
- Unrelated validation reports
- Full GDD
- All ADRs/game_rules (read only cited ones)
- Archived specs

## Consequences

- Faster agent execution
- Lower token cost
- Reduced drift risk
- Spec must be self-contained or cite dependencies explicitly

## Applies To

- All agent-run spec execution
- All bug fix and documentation tasks

## Source Documents

- [Context Reading Policy Rule](./../../../.claude/rules/context-reading-policy.md)

---

*Created: 2026-06-01*  
*Status: accepted*
