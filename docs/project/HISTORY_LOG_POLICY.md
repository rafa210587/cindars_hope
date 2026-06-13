# History Log Policy

---

## PROJECT_LOG.md Is a Historical Log

`PROJECT_LOG.md` (root) is a **session-by-session historical record** of executed work.

It is **not** an execution context. It is **not** a source of truth for current status.

---

## When Agents Should Read PROJECT_LOG.md

Read PROJECT_LOG.md **only** when:

1. Performing an **audit** of past work (e.g., SPEC_29B reconciliation)
2. **Investigating a regression** — need to find when something was changed
3. Performing a **reconciliation** between states
4. An **explicit human request** to review history

In these cases, read only the **relevant recent entries**, not the entire file.

---

## When Agents Should NOT Read PROJECT_LOG.md

Do **not** read PROJECT_LOG.md when:

- Starting a new implementation task
- Checking current project status (use `docs/00_PROJECT/CURRENT_STATE.md`)
- Looking for the active spec queue (use CURRENT_STATE.md)
- Looking for last build/validation results (use `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md`)

---

## What Agents Should Use Instead

| Need | Read Instead |
|------|-------------|
| Current project status | `docs/00_PROJECT/CURRENT_STATE.md` |
| Last validation result | `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md` |
| Active spec queue | `docs/00_PROJECT/CURRENT_STATE.md` |
| Spec status | `.specs/SPEC_EXECUTION_ORDER.md` |
| Implementation context | `docs/IMPLEMENTATION_STATUS.md` (audit only) |

---

## Format of PROJECT_LOG.md

Each entry in PROJECT_LOG.md represents one session:

```markdown
## Sessao <date> (<SPEC_ID>) - <Title>
- Foco: ...
- Phase executed
- Files changed
- Results
- Next actions
```

Entries are prepended (newest at top).

---

## PROJECT_LOG.md Does Not Grant Execution Authority

An entry in PROJECT_LOG.md saying "SPEC_X done" does not mean:
- The spec was fully validated
- Phase 2-3 was executed
- The spec should not be re-examined if issues arise

Evidence is in `docs/validation/`, not PROJECT_LOG.md.

---

*Policy created: 2026-06-01 (SPEC_DOCS_30)*
