# Rule: No Premature Acceptance Claims

## Rule

Do not write or state any of the following without corresponding evidence in the repository:

- "MVP accepted"
- "100% fulfilled" (as final acceptance claim)
- "Play Mode PASS"
- "Unity validated" (as blanket claim)
- "Human acceptance complete"
- "Phase 3 PASS"
- "Phase 2 PASS"

## Why

False acceptance claims propagate through documentation and mislead future agents and human reviewers. They create a false sense of project completion and can cause incorrect prioritization decisions. This was the root cause of the SPEC_29B reconciliation (2026-06-01): MVP_ACCEPTANCE_REPORT.md claimed "100% fulfilled" while Phase 2-3 were not executed.

## Applies To

All documentation edits, execution reports, closeout summaries, and chat responses.

## Violation Examples

- Writing "MVP ACCEPTANCE CRITERIA: 100% FULFILLED" in a release document when Play Mode was not run
- Stating "Unity validated" when only `dotnet build` was run (different validation level)
- Claiming "Phase 3 PASS" based on Phase 1 automated build evidence

## Allowed Alternatives

Use honest, specific claims:

| Honest Claim | What It Means |
|-------------|---------------|
| "Phase 0-1 COMPLETE" | Audit + automated builds done |
| "BUILD_VALIDATED" | dotnet build + docs PASS |
| "Phase 2 NOT RUN" | Unity validators not executed |
| "Phase 3 PENDING" | Play Mode not executed |
| "ACCEPTED pending Phase 2-3" | Clear about what is missing |

## What To Do If Exception Is Needed

If Phase 2-3 truly ran and passed, record the evidence (validator output, checklist with ✓ marks, date, executor) in the relevant validation report before making the claim.

## Validation / Detection

Hook `.claude/hooks/docs-status-honesty-check.ps1` (disabled by default) scans modified doc files for the prohibited phrases.
