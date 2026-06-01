# Rule: No Document Delete Without Candidate

## Rule

No document file may be deleted unless it appears in `docs/00_PROJECT/DOCUMENT_DELETE_CANDIDATES.md` AND the executing task explicitly authorizes deletion.

## Why

Accidental or premature deletion of documentation is irreversible without git history. Some files that appear obsolete may still be referenced by other docs, used as evidence for audits, or needed for context in future reconciliations.

## Applies To

All tasks involving file deletion, archiving, or bulk cleanup operations.

## Violation Examples

- Deleting `docs/specs/a_implementar/reorg/SPEC_00.md` because it looks superseded, without it being in delete candidates
- Bulk-deleting `docs_old/` files without explicit authorization
- Running `Remove-Item` on any doc file during a spec execution task

## Allowed Operations Without Pre-Approval

- Moving a file to an archive folder (non-destructive, git-tracked)
- Adding a `status: delete_candidate` header to a file
- Adding a file to `DOCUMENT_DELETE_CANDIDATES.md`

## What To Do If Exception Is Needed

1. Add the file to `DOCUMENT_DELETE_CANDIDATES.md` with reason and risk assessment
2. Get explicit human confirmation: "delete this candidate"
3. Only then execute deletion

## Validation / Detection

Hook `.claude/hooks/delete-guard.ps1` (disabled by default) warns when a task plan includes file deletion commands targeting doc paths.
