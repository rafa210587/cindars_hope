# Validation — Cindar's Hope

> **Validation reports are evidence, NOT an execution queue.**  
> **Agents should only read validation reports explicitly referenced by a spec or CURRENT_STATE.md.**

---

## Purpose

The `docs/05_VALIDATION/` folder (and `docs/validation/`) contains:

1. **Current validation status** — short file agents can read for latest state
2. **Execution reports** — evidence of what was done for each spec
3. **Play Mode checklists** — human-filled records of interactive testing
4. **Validator documentation** — what validators exist and how to run them

---

## Reading Policy

| When | Should Agent Read Validation? |
|------|------------------------------|
| Executing a new spec | NO — unless spec lists a report as dependency |
| Checking current build status | Read `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md` ONLY |
| Audit / reconciliation | YES — read relevant reports |
| Debugging a regression | YES — find last known good state |

---

## Folder Structure

```
docs/05_VALIDATION/
  README.md                              (this file)
  VALIDATION_REPORT_TEMPLATE.md          (template for new reports)
  current/
    LAST_VALIDATION_STATUS.md            (most recent automated results)
```

**Existing reports:** `docs/validation/` — all historical validation reports live here.  
Migration to `docs/05_VALIDATION/reports/` is planned for SPEC_DOCS_31. Do not move yet.

---

## Validation Levels (distinct)

| Level | What It Confirms | Tool |
|-------|-----------------|------|
| dotnet build | C# compiles without errors | `dotnet build` |
| docs validation | Documentation structure valid | `tools/docs/validate_docs.ps1` |
| Unity validators | Asset wiring, scene structure | Unity Editor menu |
| Play Mode | Gameplay behavior correct | Human in Unity |

**These are NOT equivalent.** Passing `dotnet build` does NOT mean Play Mode passes.

---

## Never Claim

```
❌ Unity PASS without running Unity
❌ Play Mode PASS without human checklist
❌ Phase 3 PASS based on Phase 1 results
```

---

*Created: 2026-06-01 (SPEC_DOCS_30)*
