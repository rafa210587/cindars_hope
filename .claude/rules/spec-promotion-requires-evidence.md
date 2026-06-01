# Rule: Spec Promotion Requires Evidence

## Rule

Do not move a spec from `a_implementar/` to `implementados/`, or mark a spec as COMPLETE/ACCEPTED, unless the required evidence for that spec's validation level exists in the repository.

## Why

Premature promotion hides technical debt and creates false confidence. A spec that is code-complete but has not passed Phase 2 (Unity validators) or Phase 3 (Play Mode) is NOT fully implemented — it is BUILD_VALIDATED at most.

## Applies To

Any task involving `/finish-spec`, spec closeout, or status updates for specs requiring multi-phase validation.

## Violation Examples

- Moving `spec_mvp_closeout_29` to `implementados/` because Phase 1 build passed, while Phase 2-3 were not run
- Marking a spec `COMPLETE` because dotnet build passes
- Auto-promoting a spec to `implementados/` in `implement-spec` Phase 4 without checking if Phase 2-3 evidence is required

## Phase Status Taxonomy

Use these statuses — never plain "COMPLETE":

| Status | Meaning |
|--------|---------|
| `AUDITED` | Phase 0 done; no code changed yet |
| `CODE_COMPLETE` | Code written; not yet validated |
| `BUILD_VALIDATED` | dotnet build + docs validation PASS |
| `UNITY_VALIDATED` | Unity Editor validators PASS |
| `PLAYMODE_VALIDATED` | Human Play Mode checklist PASS |
| `ACCEPTED` | All required phases complete |
| `PARTIAL` | Some phases done, some blocked/not run |
| `BLOCKED` | Cannot proceed; blocker documented |

## Allowed Exceptions

A spec that explicitly declares Phase 2-3 as out of scope (e.g., a docs-only spec) may be promoted after BUILD_VALIDATED.

## What To Do If Exception Is Needed

The spec itself must explicitly list Phase 2-3 as `NOT IN SCOPE`. If it does, note in the promotion commit why Unity/Play Mode phases were skipped.

## Validation / Detection

Hook `.claude/hooks/spec-promotion-guard.ps1` (disabled by default) checks for execution report presence before spec move.
