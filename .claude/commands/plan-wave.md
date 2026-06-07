# /plan-wave

Plan the next FASE or development wave. Only command allowed to read ROADMAP.md by default.

**Arguments:** `$ARGUMENTS` — optional: wave name or FASE number (e.g., `FASE10` or `post-mvp`)

---

## Objective

Produce a planning document for the next development wave. No implementation. No spec execution.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. `docs/project/ROADMAP.md`
4. `docs/backlog/current_backlog.md`

## Optional Reads

- `docs/backlog/post_mvp_backlog.md`
- `docs/project/DOCUMENT_INDEX.md`
- Relevant architecture docs for the wave domain

## Do NOT Read By Default

```
PROJECT_LOG.md
IMPLEMENTATION_STATUS.md (full)
All validation reports
docs_old/**
```

---

## Procedure

1. Read CURRENT_STATE.md to understand current blocking items
2. Read ROADMAP.md to understand planned waves
3. Read backlog to understand priorities
4. Produce wave plan:
   - Objectives of the wave
   - Specs to create (IDs, titles, rough effort)
   - Dependencies between proposed specs
   - Specs that must complete first (from CURRENT_STATE.md)
   - Risks and open questions
5. Output planning document

---

## Allowed Edits

Optional: create `docs/project/ROADMAP.md` update or a wave planning document in `docs/backlog/`.

## Forbidden Edits

- No spec implementation
- No spec movement
- No code changes
- No delete operations

---

## Output Format

```markdown
# Wave Plan — <FASE/Wave Name>

## Prerequisite (must complete first)
- <item from CURRENT_STATE.md>

## Wave Objectives
- <objective>

## Proposed Specs

| Spec ID | Title | Effort | Depends On |
|---------|-------|--------|-----------|
| SPEC_XX | <title> | Xh | <dep> |

## Risks and Open Questions
- <risk>

## Recommended Execution Order
1. <spec>
2. <spec>
```
