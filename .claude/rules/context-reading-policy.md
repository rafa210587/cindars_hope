# Rule: Context Reading Policy

## Rule

Agents executing a spec must read only: CLAUDE.md (or AGENTS.md), `docs/project/CURRENT_STATE.md`, the active spec, and files explicitly cited by the spec. Do not read PROJECT_LOG.md, ROADMAP.md, full GDD, old refinements, archived specs, or unrelated validation reports by default.

## Why

Heavy default context (PROJECT_LOG.md ~500+ lines, full IMPLEMENTATION_STATUS.md) increases token cost and drift risk from stale historical data. CURRENT_STATE.md (~80 lines) contains all operationally relevant information for an execution task.

## Applies To

All agent-run spec execution, bug fix, and documentation tasks.

## Violation Examples

- Reading PROJECT_LOG.md as "Camada 0" before a code implementation task
- Reading all of IMPLEMENTATION_STATUS.md to find current spec status
- Reading full SPEC_EXECUTION_ORDER.md when CURRENT_STATE.md already lists active spec queue

## Allowed Exceptions

- **Audit/reconciliation tasks** (`/reconcile-status`): may read PROJECT_LOG.md and IMPLEMENTATION_STATUS.md
- **Regression investigation**: may read prior validation reports
- **Wave planning** (`/plan-wave`): may read ROADMAP.md and current_backlog.md
- **Explicit human request**: "read PROJECT_LOG for context"

## What To Do If Exception Is Needed

State the justification before reading the heavy file. Example: "Reading PROJECT_LOG.md for audit of SPEC_18-24 evidence."

## Validation / Detection

Hook `.claude/hooks/context-policy-check.ps1` (disabled by default) can detect when execution plans include heavy historical reads without justification.
