---
name: docs-migration
description: Move specs and refinements from a_implementar/ to implementados/ with evidence
version: 1.0
---

# Docs Migration Skill

Use when closing out a spec or refinement to move documentation files to implementados/ folders.

## Rules

1. **`docs/specs/` is sole source of truth.** Never recreate root-level `specs/` or `spec/`.
2. **Future specs stay in `a_implementar/`.** Move only when implementation is complete and validated.
3. **Completed specs move to `implementados/`.** File path: `docs/specs/implementados/spec_*.md`
4. **Future refinements in pre_refinements/.** Path: `docs/refinements/a_implementar/pre_refinamentos/ref_*.md`
5. **Completed refinements move to `implementados/`.** Path: `docs/refinements/implementados/ref_*.md`
6. **Registries must stay consistent.** Update after moving specs/refinements.
7. **IMPLEMENTATION_STATUS.md must reflect reality.** No claims without evidence.
8. **PROJECT_LOG.md must be updated.** Date, spec, and deliverables recorded.

## Migration Steps

### 1. Verify Evidence

Before moving a spec to implementados/:

- [ ] Code changes exist in repo (checked via `git diff` or `git log`)
- [ ] Validation passed (docs, Unity compile, log scan) OR documented as NOT RUN
- [ ] No gaps left in CLAUDE.md rules compliance
- [ ] Non-regression audit passed (PASS or acceptable WARNING)

**Example evidence:**
```
Spec 12 Implementation Evidence:
✓ Commit: "feat: spec 12 - player combat melee/ranged attacks"
✓ Files: Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs, WeaponDataSO.cs
✓ Validation: Unity compile PASS, Docs PASS
✓ Non-regression: PASS
```

### 2. Move Spec File

Move from `docs/specs/a_implementar/spec_*.md` to `docs/specs/implementados/spec_*.md`

```powershell
Move-Item -Path "docs/specs/a_implementar/spec_12_player_combat.md" `
          -Destination "docs/specs/implementados/spec_12_player_combat.md"
```

### 3. Update Spec File Header

Add implementation evidence header to moved spec:

```markdown
---
status: implemented
date_implemented: 2026-05-26
evidence:
  - Commit: feat: spec 12 - player combat melee/ranged attacks
  - Files: Assets/Scripts/Runtime/Combat/*
  - Validation: docs PASS, unity compile PASS
  - Non-regression: PASS
---

# Spec 12 - Player Combat (Implemented)

[rest of original spec content]
```

### 4. Move Related Refinement (if applicable)

If spec references a pre-refinement:

```powershell
Move-Item -Path "docs/refinements/a_implementar/pre_refinamentos/ref_spec12_*.md" `
          -Destination "docs/refinements/implementados/ref_spec12_*.md"
```

### 5. Update Registries (if they exist)

If these files exist, update them:

- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` — Add spec 12 to list
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — Remove spec 12 from list
- `docs/refinements/implementados/ref_implementados_map.md` — Add refinement if moved
- `docs/refinements/a_implementar/ref_futuro_map.md` — Remove refinement if moved

**Example update:**

```markdown
## Specs Implemented

- Spec 01: Core Event Bus
- Spec 02: Bootstrap Managers
- ...
- Spec 12: Player Combat ✅ Implemented 2026-05-26
```

### 6. Update IMPLEMENTATION_STATUS.md

Add or update capability entry with evidence:

```markdown
| Player Combat/Weapons/Spells | Implementado | spec_player_combat_melee_ranged.md, commit abc1234 |
```

**Rule:** Only add if:
- Spec moved to implementados/ OR
- Validated code exists with evidence

### 7. Update PROJECT_LOG.md

Add entry at top:

```markdown
## Sessão 2026-05-26 (NN) - Implementar SPEC 12 (Player Combat)

**Data:** 2026-05-26  
**Foco:** Implementar melee/ranged attacks com UI de combate  
**Status:** COMPLETO

### Deliverables

- PlayerCombatManager com attack logic
- WeaponDataSO com stats de melee/ranged
- DamageRequest via GameEventBus
- Weapon hotbar selection (if UI scope allowed)

### Validações

```text
Docs validation: PASS
Unity compile: PASS - Tundra build success
Log scanner: PASS
Non-regression: PASS
```

### Commit

```
abc1234 feat: spec 12 - player combat melee/ranged attacks
```

### Próxima SPEC

- SPEC 13: [name]
- Blocked? Status?
```

### 8. Run Docs Validation

```powershell
.\tools\docs\validate_docs.ps1
```

**Expected:** PASS or preexisting WARNING

**Action:**
- If PASS: Proceed to completion
- If FAIL: Fix docs issues, re-run, then proceed

## Common Patterns

### Single Spec Migration (Most Common)

```powershell
# 1. Move file
Move-Item "docs/specs/a_implementar/spec_12_*.md" "docs/specs/implementados/"

# 2. Update header in moved file with evidence

# 3. Update registries (if exist)

# 4. Update IMPLEMENTATION_STATUS.md

# 5. Update PROJECT_LOG.md

# 6. Validate
.\tools\docs\validate_docs.ps1
```

### Spec + Related Refinement

```powershell
# Same as above, plus:

# Move refinement
Move-Item "docs/refinements/a_implementar/pre_refinamentos/ref_spec12_*.md" `
          "docs/refinements/implementados/"

# Update refinement maps
```

## Audit Checklist

Before finalizing migration:

- [ ] Spec file exists in implementados/ (not in a_implementar/)
- [ ] Refinement file exists in implementados/ (if applicable)
- [ ] Evidence headers added to moved files
- [ ] Registries updated and consistent
- [ ] IMPLEMENTATION_STATUS.md reflects new state
- [ ] PROJECT_LOG.md has entry with date and deliverables
- [ ] Docs validation: PASS
- [ ] No broken links in moved files
- [ ] No orphaned references to a_implementar/ specs

## Red Flags (Do NOT Migrate)

- ❌ No code changes exist for spec (claim without evidence)
- ❌ Validation shows FAIL and not fixed
- ❌ Non-regression shows FAIL
- ❌ Spec scope amplified beyond what was implemented
- ❌ Save references changed without schema migration documented
- ❌ Validation not run and cannot document reason

## Output Format

```text
Migration Summary
─────────────────

Spec moved:
  Source: docs/specs/a_implementar/spec_12_player_combat.md
  Dest:   docs/specs/implementados/spec_12_player_combat.md
  Evidence header: ✓ Added

Refinement (if applicable):
  Source: docs/refinements/a_implementar/pre_refinamentos/ref_spec12_combat.md
  Dest:   docs/refinements/implementados/ref_spec12_combat.md

Registries updated:
  ✓ SPEC_REGISTRY_IMPLEMENTED.md
  ✓ SPEC_REGISTRY_TO_IMPLEMENT.md
  (if they exist)

Documentation updated:
  ✓ IMPLEMENTATION_STATUS.md
  ✓ PROJECT_LOG.md
  ✓ Docs validation: PASS

Status: READY FOR CLOSEOUT
```

## Integration

- **Spec Execution Skill** → Calls this at Phase 4: Closeout
- **Implementation Closeout** → Depends on this for final docs state
- **Docs Health Check** → Verifies registries remain consistent after migration
