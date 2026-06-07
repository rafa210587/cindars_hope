---
name: docs-governance
description: Organize, archive, index, and maintain documentation governance. Does not touch runtime code.
version: 1.0
when_to_use: Document organization, archive planning, delete candidates, roadmap cleanup, index updates
---

# Docs Governance Skill

## Use When

- Organizing documentation structure
- Identifying and logging delete candidates
- Moving files to archive
- Updating CURRENT_STATE.md or DOCUMENT_INDEX.md
- Cleaning up superseded specs/refinements

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. `docs/project/DOCUMENT_GOVERNANCE.md`
4. `docs/project/DOCUMENT_INDEX.md`

## Optional Reads

- `docs/project/DOCUMENT_DELETE_CANDIDATES.md` (if reviewing candidates)
- `docs/project/HISTORY_LOG_POLICY.md` (if PROJECT_LOG work involved)

## Do Not Read By Default

```
PROJECT_LOG.md (unless audit scope)
ROADMAP.md (unless roadmap cleanup)
Runtime code files
```

## Procedure

### For Delete Candidate Review

1. Read DOCUMENT_DELETE_CANDIDATES.md
2. Verify each candidate still exists
3. Check if any active spec, current_state, or validation report references the candidate
4. If referenced: mark as "cannot delete yet; referenced by X"
5. If unreferenced: confirm deletion readiness
6. Do NOT delete without explicit human authorization

### For Doc Move / Archive

1. Identify source and destination
2. Verify destination folder exists
3. Move file (git mv preferred for history preservation)
4. Update DOCUMENT_INDEX.md entry
5. Update any files that referenced the old path
6. Run docs validation

### For CURRENT_STATE.md Update

1. Read current CURRENT_STATE.md
2. Identify stale or missing information
3. Update minimally — keep file under ~100 lines
4. Verify nothing was accidentally removed

### For DOCUMENT_INDEX.md Update

1. Audit new files in `docs/` not yet indexed
2. Add entries with correct function and link
3. Remove entries for deleted files

## Rules

- Do NOT delete without `DOCUMENT_DELETE_CANDIDATES.md` entry and human authorization
- Do NOT move specs to `implementados/` from this skill (use `/finish-spec`)
- Always run `tools/docs/validate_docs.ps1` after any doc changes
- CURRENT_STATE.md must stay under ~100 lines
- Do NOT edit runtime files

## Validation

```powershell
.\tools\docs\validate_docs.ps1
```

Expected: PASS 14/14

## Common Regressions

- Accidentally deleting a file referenced by an active spec
- Marking a spec as superseded when it has active dependencies
- Making CURRENT_STATE.md too long (defeats token-efficiency purpose)

## Stop Conditions

- A file to be deleted is referenced by an active spec or CURRENT_STATE.md
- A move would break a path referenced in any governance file
- Docs validation fails after changes
