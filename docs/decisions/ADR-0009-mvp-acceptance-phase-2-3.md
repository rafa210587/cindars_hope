---
doc_type: adr
status: accepted
adr_id: ADR-0009
title: MVP Acceptance Requires Phase 2-3
date: 2026-06-01
source_documents:
  - .claude/rules/no-premature-acceptance-claims.md
  - docs/validation/current/LAST_VALIDATION_STATUS.md
supersedes: []
superseded_by: []
applies_to:
  - mvp-acceptance
  - validation-gates
  - closure-criteria
---

# ADR-0009 — MVP Acceptance Requires Phase 2-3

## Status

**accepted** (acceptance gate for all MVPs and phase-gated specs)

## Context

Specs have a distinct difference between "code complete" and "accepted". Code-complete means the developer wrote the code; accepted means the feature is validated and production-ready. Question: What validation must complete before a feature/MVP can be accepted?

## Decision

**Code-complete does not equal accepted. MVP acceptance requires Phase 2-3 validation.**

### Phase Hierarchy

```
Phase 0: Audit (plan, risks, scope)
Phase 1: Build (dotnet build, docs validation)
Phase 2: Unity Validation (Editor compile, validators, asset generation)
Phase 3: Play Mode (human gameplay test or Play Mode checklist)
```

### Acceptance Criteria by Spec Type

#### Feature Spec (e.g., SPEC_19_INVENTORY)

| Phase | Requirement | Acceptable Status |
|---|---|---|
| 0 | Audit complete | AUDITED |
| 1 | Build PASS | BUILD_VALIDATED |
| 2 | Unity validators PASS | UNITY_VALIDATED |
| 3 | Play Mode PASS | **ACCEPTED** ✓ |

Cannot claim "ACCEPTED" without Phase 3 evidence.

#### Documentation Spec (e.g., SPEC_DOCS_36)

| Phase | Requirement | Acceptable Status |
|---|---|---|
| 0 | Audit complete | AUDITED |
| 1 | Docs validation PASS | BUILD_VALIDATED |
| 2-3 | Deferred (scope out) | **ACCEPTED** ✓ (Phase 2-3 out of scope) |

Must explicitly state Phase 2-3 out of scope in spec frontmatter.

#### MVP Closeout Spec (e.g., SPEC_DOCS_29_MVP_ACCEPTANCE)

| Phase | Requirement | Acceptable Status |
|---|---|---|
| 0 | Audit complete | AUDITED |
| 1 | Build PASS | BUILD_VALIDATED |
| 2 | All source specs Phase 2 PASS | UNITY_VALIDATED |
| 3 | All source specs Phase 3 PASS | **ACCEPTED** ✓ |

MVP closeout cannot be accepted until ALL source specs (SPEC_01 through SPEC_29) have passed Phase 2-3.

### Prohibited Claims (Without Evidence)

```
"MVP accepted" without Phase 2-3 of SPEC_01-29
"Feature fulfilled" without Play Mode checklist
"100% complete" without all required phases
"Phase 3 PASS" without tester confirmation
"Human acceptance" without evidence (checklist, tester name, date)
```

### Honest Alternatives

```
"Code-complete; Phase 2-3 pending" (in progress)
"BUILD_VALIDATED; Phase 3 not run" (blocked)
"Phase 0-1 COMPLETE" (partial phases only)
"ACCEPTED pending Phase 2-3 of source specs" (clear blocker)
"Phase 2 PASS, Phase 3 NOT RUN (sandbox limitation)" (explicit reason)
```

### What Happens If Phase Is Blocked

If Phase 2-3 cannot run (Unity Editor unavailable, sandbox, time), document:

```
Phase 2: NOT RUN
Reason: <editor lock | license | sandbox | timeout>
Residual risk: Unity compile/validators not verified
Mitigation: Will validate when Editor available
Status: BLOCKED (not ACCEPTED)
```

Do **not** convert "blocked" into "pass".

## Implementation

- Spec frontmatter includes `phase_gates: [0, 1, 2, 3]` or `phase_gates: [0, 1] # 2-3 out of scope`
- Execution report has phase table showing which ran and result
- Closeout cannot move spec to `implementados/` without Phase 2-3 PASS
- MVP cannot be marked accepted without Phase 2-3 of all source specs

## Consequences

- No false "accepted" claims that mislead users/stakeholders
- Phase 3 blockers are visible (Editor unavailable, sandbox, time)
- Forced disclosure of what validation actually ran
- Features known to be production-ready before release
- Historical evidence trail prevents regressions

## Applies To

- All feature specs requiring Phase 2-3
- All MVP closeout specs
- All status updates in CURRENT_STATE.md, PROJECT_LOG.md
- All execution reports and validation summaries

## Source Documents

- [no-premature-acceptance-claims.md](./../.claude/rules/no-premature-acceptance-claims.md) — policy enforcement
- [ADR-0004: Validation Evidence](./ADR-0004-validation-evidence-phase-gates.md) — phase definitions
- [ADR-0003: Spec Lifecycle](./ADR-0003-spec-lifecycle.md) — spec promotion rules

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0004 (phase gates), ADR-0003 (spec lifecycle), ADR-0002 (agent context on validated specs)*
