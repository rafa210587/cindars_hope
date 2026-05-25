# /docs-health

Validate documentation structure and consistency.

## When to Run

- After moving specs or refinements to implementados/
- After updating status files
- After altering PROJECT_LOG.md or IMPLEMENTATION_STATUS.md
- As part of general health check

## Mandatory Checks

### 1. Spec Source of Truth

- [ ] `docs/specs/` exists and contains specs
- [ ] No root-level `specs/` or `spec/` directories exist
- [ ] `docs/specs/a_implementar/` contains future specs
- [ ] `docs/specs/implementados/` contains completed specs
- [ ] `docs/specs/SPEC_EXECUTION_ORDER.md` exists (if applicable)
- [ ] `docs/specs/SPEC_SOURCE_OF_TRUTH.md` exists or is not critical

### 2. Refinement Structure

- [ ] `docs/refinements/a_implementar/pre_refinamentos/` contains future refinements
- [ ] `docs/refinements/implementados/` contains completed refinements
- [ ] Maps are internally consistent

### 3. Active Documentation

- [ ] `docs/design/` exists (design specs)
- [ ] `docs/architecture/` exists (architecture)
- [ ] `docs/operations/` exists (operational docs)
- [ ] `docs_old/` preserved but not edited
- [ ] `docs/IMPLEMENTATION_STATUS.md` tracks current state

### 4. Status & Tracking

- [ ] `docs/IMPLEMENTATION_STATUS.md` entries have evidence (spec file or validated log)
- [ ] `PROJECT_LOG.md` has recent entry when task was significant
- [ ] No orphaned capacity entries (claimed but no spec file)
- [ ] No contradictions between IMPLEMENTATION_STATUS and actual spec state

### 5. Registries & Maps (if they exist)

- [ ] `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` consistent with `docs/specs/implementados/`
- [ ] `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` consistent with `docs/specs/a_implementar/`
- [ ] `docs/refinements/implementados/ref_implementados_map.md` consistent with folder
- [ ] `docs/refinements/a_implementar/ref_futuro_map.md` consistent with folder
- [ ] No duplicate IDs or specs listed twice

### 6. Links & References

- [ ] Links in docs point to existing files (check file paths)
- [ ] No broken cross-references between specs/refinements
- [ ] No references to removed or archived documents

### 7. Tools Validation Script

Run documentation validation:

```powershell
.\tools\docs\validate_docs.ps1
```

**Expected:** PASS (or list specific issues to fix)

## Output Format

```text
Status: PASS | WARNING | FAIL

Spec Structure:
  ✓ docs/specs/ is sole source of truth
  ✓ No root specs/ or spec/
  ✓ a_implementar/ and implementados/ consistent
  ✓ SPEC_EXECUTION_ORDER.md coherent

Refinement Structure:
  ✓ pre_refinamentos/ path correct
  ✓ implementados/ folder consistent
  ✓ Maps up to date

Active Documentation:
  ✓ docs/design/, docs/architecture/, docs/operations/ all present
  ✓ docs_old/ preserved, not edited

Status & Tracking:
  ✓ IMPLEMENTATION_STATUS.md has evidence for all claims
  ✓ PROJECT_LOG.md current
  ⚠️ WARNING: Spec X marked implemented but spec file missing

Registries & Maps:
  ✓ Consistent with source directories
  ✓ No duplicate IDs

Links & References:
  ✓ All cross-links valid

Tools Validation:
  ✓ tools/docs/validate_docs.ps1: PASS

Issues found:
  [if any]

Corrective actions:
  [if any]

Residual risk:
  [if any]
```

## Examples

### PASS

```text
Status: PASS

All checks passed:
- Source of truth structure: valid
- Specs consistent (a_implementar + implementados)
- Refinements consistent
- Status and tracking coherent with evidence
- No broken links
- Tools validation: PASS
```

### WARNING

```text
Status: WARNING

Issues found:
  - IMPLEMENTATION_STATUS.md line 42: "Spec 15 implemented" but spec file missing
  - PROJECT_LOG.md has 2 days without entry (acceptable gap)

Corrective actions:
  - Verify Spec 15 status: moved to implementados or deferred?
  - Update IMPLEMENTATION_STATUS if status changed

Residual risk:
  Minor: Tracking inconsistency, no broken functionality
```

### FAIL

```text
Status: FAIL

Issues found:
  - Root-level specs/ directory detected (should not exist)
  - SPEC_REGISTRY_IMPLEMENTED.md lists 20 specs but implementados/ contains 19

Corrective actions REQUIRED:
  1. Remove root-level specs/ directory
  2. Reconcile SPEC_REGISTRY_IMPLEMENTED.md with actual implementados/ folder
  3. Re-run validation to confirm fix

Residual risk:
  CRITICAL: Inconsistent state may confuse future spec execution
```

---

## Do NOT

- Claim PASS without running `tools/docs/validate_docs.ps1`
- Ignore WARNING level issues (they can become bugs)
- Leave FAIL state unresolved
- Edit docs_old/** (it's archive only)
- Create root-level specs/ or spec/

**Health check is complete. Status determines if task can proceed.**
