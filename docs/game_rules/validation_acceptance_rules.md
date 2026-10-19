---
doc_type: game_rule
status: accepted
domain: validation-acceptance
source_adrs:
  - ADR-0004
  - ADR-0009
source_documents:
  - docs/decisions/ADR-0004-validation-evidence-phase-gates.md
  - docs/decisions/ADR-0009-mvp-acceptance-phase-2-3.md
  - docs/validation/current/LAST_VALIDATION_STATUS.md
last_reviewed: 2026-06-01
---

# Validation and Acceptance Rules

## Purpose

Defines phase gates, acceptance criteria, and what validation levels mean for spec completion.

---

## Canonical Rules

### Rule: Four Validation Phases

- **Phase 0: Audit**
  - What: Analyze scope, dependencies, risks
  - Evidence: `spec_*_phase0_*.md` audit report
  - Status claim: "AUDITED"

- **Phase 1: Build Validation**
  - What: C# compile (dotnet build) + docs validation
  - Evidence: `dotnet build` exit 0; `validate_docs.ps1` exit 0; logs in report
  - Status claim: "BUILD_VALIDATED"

- **Phase 2: Unity Validation**
  - What: Unity Editor compile + validators (CombatDatabaseValidator, etc.)
  - Evidence: `unity -executeMethod <validator>.Run -batchmode` exit 0; validator PASS in log
  - Status claim: "UNITY_VALIDATED"

- **Phase 3: Play Mode**
  - What: Human gameplay test or Play Mode checklist
  - Evidence: Tester checklist ✓ marks; date; tester name; test environment
  - Status claim: "PLAYMODE_VALIDATED"

- **Applies to:** All feature specs requiring multi-phase validation

### Rule: Blocked Phase Honesty

- **Rule:** If a phase cannot run, report explicitly:
  ```
  Phase 2: NOT RUN
  Reason: <editor lock | license timeout | sandbox | compile error>
  Residual risk: Unity validators not verified locally
  Command attempted: <command>
  ```

- **Must NOT:** Convert blocked phase to PASS
- **Must NOT:** Hide blocked phase in report
- **Applies to:** All validation reports

### Rule: Spec Promotion Gates

- **Rule:** A spec may move to `implementados/` only when:
  1. All required phases complete (as declared in spec frontmatter)
  2. Phase 2-3 evidence is in repository or explicitly marked "out of scope"
  3. CURRENT_STATE.md updated to reflect promotion

- **For documentation specs:** Phase 2-3 out of scope; BUILD_VALIDATED sufficient
- **For feature specs:** Phase 0-3 required; PLAYMODE_VALIDATED required for "accepted"
- **For closeout specs:** Source specs must pass Phase 2-3 before closeout can be accepted

- **Validation:** Spec promotion guard hook detects premature moves

### Rule: MVP Acceptance Requires Phase 2-3

- **Rule:** MVP cannot be accepted without Phase 2-3 of all source specs
  - SPEC_*_MVP_ACCEPTANCE cannot accept unless SPEC_01 through SPEC_29 all Phase 2-3 PASS
  - Closeout_mvp specs must cite Phase 2-3 evidence of source specs

- **Prohibited:** "MVP accepted" without Phase 3 checklist
- **Allowed:** "ACCEPTED pending Phase 2-3 completion of source specs"

- **Applies to:** All MVP and closeout specs

### Rule: Batch 2 Blocked Until Phase 2-3

- **Rule:** Batch 2 specs are blocked until Phase 2-3 of SPEC_01-29 complete:
  - spec_14a*, spec_14b* (advanced combat)
  - spec_enemy_ai* (AI behaviors)
  - spec_cave_runtime* (advanced cave features)
  - spec_ui_ux_full_gameplay* (full gameplay UI)
  - spec_combat_movement* (advanced movement)

- **Cannot move to implementados until:** Phase 2-3 gate opens (human validation complete)
- **Applies to:** Batch 2 only; Batch 1 (SPEC_01-29) not affected

### Rule: Closeout MVP Specs Blocked

- **Rule:** closeout_mvp/ specs cannot be moved to implementados/ unless:
  1. All source specs (SPEC_01-29) have Phase 2-3 evidence in `docs/validation/`
  2. Closeout spec itself passes Phase 1-2 (documentation + validation compilation)
  3. Human approval to close MVP

- **Applies to:** SPEC_DOCS_29 (MVP_ACCEPTANCE_REPORT), any other MVP closeout specs

---

## Status Terminology

| Status | Phases | Acceptance? | When Allowed |
|---|---|---|---|
| AUDITED | 0 | No | Planning; no code yet |
| BUILD_VALIDATED | 0-1 | Doc specs only | Code written; not gameplay-tested |
| UNITY_VALIDATED | 0-2 | No | Compile + validators pass; Play Mode pending |
| PLAYMODE_VALIDATED | 0-3 | **YES** ✓ | All phases complete |
| CODE_COMPLETE | 0-1 (no 2-3) | No | Code done; validation pending |
| BLOCKED | any | No | Cannot proceed; blocker documented |
| PARTIAL | some phases | No | Some phases done; some pending |

---

## Prohibited Status Claims (Without Evidence)

```
✗ "MVP accepted" (without Phase 3)
✗ "100% fulfilled" (without all phases)
✗ "Play Mode PASS" (without tester checklist)
✗ "Unity validated" (without Phase 2 evidence)
✗ "Phase 3 PASS" (without checklist + tester)
✗ "Human acceptance" (without signed-off evidence)
```

**Use honest alternatives:**

```
✓ "Phase 0-1 COMPLETE"
✓ "Phase 2 NOT RUN: Editor unavailable"
✓ "ACCEPTED pending Phase 2-3 of source specs"
✓ "BLOCKED: Cannot proceed until <specific reason>"
```

---

## Related ADRs

- [ADR-0004: Validation Evidence and Phase Gates](../decisions/ADR-0004-validation-evidence-phase-gates.md)
- [ADR-0009: MVP Acceptance Requires Phase 2-3](../decisions/ADR-0009-mvp-acceptance-phase-2-3.md)
- [ADR-0003: Spec Lifecycle](../decisions/ADR-0003-spec-lifecycle.md)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/decisions/ADR-0004, ADR-0009*
