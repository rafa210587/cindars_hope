# /plan-wave

Plan the next FASE or development wave. Only command allowed to read ROADMAP.md by default.

**Arguments:** `$ARGUMENTS` — optional: wave name or FASE number (e.g., `FASE10` or `post-mvp`)

---

## Objective

Produce a planning document for the next development wave. No implementation. No spec execution.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md` — active queue and blockers
3. `.specs/SPEC_GENERATION_ROADMAP_MASTER.md` — macro roadmap of all planned specs and dependencies
4. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution phases, paralelization, batch size rules
5. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation requirements to plan for
6. `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — list of specs to plan and dependencies
7. `docs/backlog/current_backlog.md` — operational backlog items

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

1. Read CURRENT_STATE.md to understand blocking items and current queue
2. Read SPEC_GENERATION_ROADMAP_MASTER.md — use this as primary source for waves/specs
3. Read SPEC_WAVE_EXECUTION_PROTOCOL.md for:
   - Paralelization rules (which lanes can run in parallel)
   - Lock/checkpoint timing between waves
   - Batch size and execution capacity constraints
4. Read SPEC_VALIDATION_MATRIX_MASTER.md to understand validation costs (time, resources)
5. Read SPEC_REGISTRY_TO_IMPLEMENT.md to cross-check spec list
6. Produce wave plan:
   - Objectives of the wave
   - Specs to create (IDs, titles, rough effort, dependencies)
   - Dependency graph between proposed specs and waves
   - Specs that must complete first (from CURRENT_STATE.md)
   - Paralelization plan (which lanes, checkpoints)
   - Validation plan (types and timing per change)
   - Risks and open questions
7. Output planning document
8. Do NOT create specs or implement code — only plan.

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
