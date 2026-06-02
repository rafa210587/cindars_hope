---
doc_type: adr
status: accepted
adr_id: ADR-0004
title: Validation Evidence and Phase Gates
date: 2026-06-01
source_documents:
  - docs/validation/current/LAST_VALIDATION_STATUS.md
  - .claude/rules/unity-validation-honesty.md
  - .claude/rules/no-premature-acceptance-claims.md
supersedes: []
superseded_by: []
applies_to:
  - validation-evidence
  - phase-gating
  - status-reporting
---

# ADR-0004 — Validation Evidence and Phase Gates

## Status

**accepted** (governance decision for validation levels)

## Context

Validation occurs at multiple levels: static build checks, Unity compile, Play Mode gameplay. A spec author must clearly distinguish which validations passed and which were not run. Question: What evidence is required for each phase, and how should blocked phases be reported?

## Decision

**Validation has four distinct phases, each with clear evidence requirements.**

### Phase 0: Audit
- **What:** Analyze spec, dependencies, scope, risks
- **Evidence:** `docs/validation/spec_*_phase0_*.md` file with:
  - Identified files to change
  - Risk assessment
  - External dependencies
  - Compliance check (CLAUDE.md, AGENTS.md, rules)
- **Status claim allowed:** "AUDITED"

### Phase 1: Build Validation
- **What:** C# compile check (dotnet build) + docs validation
- **Evidence:** 
  - `dotnet build Assembly-CSharp.csproj --no-restore` exit code 0
  - `tools/docs/validate_docs.ps1` exit code 0 (no errors)
  - Log output in report
- **Status claim allowed:** "BUILD_VALIDATED"
- **If blocked:** `Build validation: NOT RUN. Reason: <specific blocker>. Command: <command>. Residual risk: <impact>.`

### Phase 2: Unity Validation
- **What:** Unity Editor compile and validators (SceneValidator, CombatDatabaseValidator, etc.)
- **Evidence:**
  - Output from `unity -projectPath . -executeMethod <validator>.Run -batchmode -nographics -logfile log.txt`
  - Exit code 0 and "validation PASS" in log
  - Asset counts verified (if generators ran)
- **Status claim allowed:** "UNITY_VALIDATED"
- **If blocked:** `Unity validation: NOT RUN. Reason: <editor lock | license | sandbox | timeout | package | compile>. Residual risk: Unity compile/validators not verified locally.`

### Phase 3: Play Mode Validation
- **What:** Human gameplay test or Play Mode checklist
- **Evidence:**
  - Checkmark checklist in `docs/validation/spec_*_phase3_*.md`
  - Screenshots or video (if human review)
  - Date, tester name, test environment
- **Status claim allowed:** "PLAYMODE_VALIDATED"
- **If not run:** `Play Mode: NOT RUN. Reason: out of scope | blocked | sandbox. Residual risk: gameplay behavior not verified.`

### Combined Status Claims

| Phases Complete | Status | Meaning |
|---|---|---|
| 0 only | AUDITED | Planning complete; no changes made |
| 0-1 | BUILD_VALIDATED | Code written and compiled; no gameplay tested |
| 0-2 | UNITY_VALIDATED | Compile + validators pass; no Play Mode |
| 0-3 | ACCEPTED | All phases complete; safe to promote |
| 0-1, not 2-3 | CODE_COMPLETE (code done), PARTIAL (phases incomplete) | Clarify which phases pending |

### What NOT To Claim Without Evidence

**Prohibited phrases without corresponding evidence:**

```
"MVP accepted" (without Phase 2-3)
"100% fulfilled" (without all phases)
"Play Mode PASS" (without Play Mode evidence)
"Unity validated" (without Phase 2 evidence)
"Phase 2 PASS" (without validator output)
"Phase 3 PASS" (without checklist/tester confirmation)
```

Use honest alternatives:

```
"Phase 0-1 COMPLETE" (audit + build done)
"Phase 2 NOT RUN" (Unity Editor unavailable)
"Phase 3 PENDING" (Play Mode deferred)
"ACCEPTED pending Phase 2-3" (clear about what remains)
```

## Implementation

- Every spec execution report includes a phase table showing which phases ran and result
- Blocked phases always include `NOT_RUN` + reason + residual risk
- Evidence files are never deleted; they form the historical record
- If Phase 2-3 cannot run (sandbox, license, time), document explicitly — don't hide the gap
- Validation script (`validate_docs.ps1`) enforces honesty checks via hook

## Consequences

- No false "PASS" claims that mislead future phases
- Blocked phases are visible, not hidden
- Evidence is immutable and auditable
- Future specs can build on clear validation foundation
- Phase gates prevent premature promotion

## Applies To

- Every spec execution report
- Every closeout summary
- Every status update in CURRENT_STATE.md
- Every validation hook and script

## Source Documents

- [unity-validation-honesty.md](./../.claude/rules/unity-validation-honesty.md) — validation result integrity
- [no-premature-acceptance-claims.md](./../.claude/rules/no-premature-acceptance-claims.md) — evidence requirement
- [LAST_VALIDATION_STATUS.md](./../validation/current/LAST_VALIDATION_STATUS.md) — current phase status tracking

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0003 (spec lifecycle), ADR-0009 (MVP acceptance gates)*
