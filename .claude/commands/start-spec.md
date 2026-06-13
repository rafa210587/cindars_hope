# /start-spec

Use when preparing to implement a spec. Delivers an execution plan without touching code.

**Arguments:** `$ARGUMENTS` — spec number or filename (e.g., `spec_09` or `spec_09_status_effect_database`)

---

## Objective

Understand spec scope, dependencies, and risks. Deliver a concise plan. Do NOT implement.

---

## Required Reads

1. `CLAUDE.md` — routing and stop conditions
2. `docs/project/CURRENT_STATE.md` — active queue, blockers
3. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution governance and phase taxonomy
4. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation requirements matrix
5. Target spec: `.specs/a_implementar/spec_<name>.md`

## Optional Reads (only if spec cites them)

- A specific refinement listed in the spec's `depends_on` or `required_read` frontmatter
- An implemented spec listed as a dependency
- A specific architecture document referenced by the spec

## Do NOT Read By Default

```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
memory/ (unless task cites prior pattern explicitly)
SPEC_EXECUTION_ORDER.md (use CURRENT_STATE.md queue instead)
ROADMAP.md
```

---

## Procedure

1. Read required files (above)
2. From the spec, identify:
   - Objective and deliverables
   - Scope: permitted files, forbidden files
   - Dependencies: blocked? ready?
   - Mandatory validations (docs, build, Unity, Play Mode)
   - Applicable skills from `.claude/skills/`
   - Phase 2-3 requirements (does this spec need Unity validators or Play Mode?)
3. Deliver plan (see output format)
4. Stop. Wait for human confirmation before implementing.

---

## Allowed Edits

None. This command is read-only.

---

## Forbidden Edits

- No code changes
- No spec movement
- No status updates

---

## Validation

None needed. This command does not change files.

---

## Stop Conditions

- Spec and CURRENT_STATE conflict (e.g., spec says dependency is done but CURRENT_STATE says blocked)
- Spec is not in `a_implementar/` — may have been moved or was never there
- Spec ID not found

---

## Output Format

```
Spec: <SPEC_ID> — <Title>
Objective: <1-2 sentences>

Dependencies:
  - <DEP_ID> (<status: ready/blocked>)

Scope:
  Permitted: <list>
  Forbidden: <list>

Validations required:
  - [ ] docs validation (if docs change)
  - [ ] dotnet build runtime (if C# changes)
  - [ ] dotnet build editor (if editor C# changes)
  - [ ] Unity validators (if spec requires Phase 2)
  - [ ] Play Mode (if spec requires Phase 3)

Phase 2-3 requirement: <YES / NO — docs-only spec>

Skills applicable:
  - <skill-name>: <why>

Risks:
  - <risk>

Ready to proceed?
```
