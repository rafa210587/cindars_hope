---
name: spec-execution
description: Execute a spec respecting scope, permitted files, and mandatory validations
version: 1.0
---

# Spec Execution Skill

Use when the task involves implementing or advancing a spec from `docs/specs/a_implementar/`.

## Execution Flow

### Phase 0: Preparation (Pre-Implementation)

1. Run `/start-spec <spec-name>` to get execution plan
2. Consult `memory/project_skills_available.md` if similar task done before
3. Verify dependencies in `docs/specs/SPEC_EXECUTION_ORDER.md`
4. Confirm with user before proceeding to Phase 1

### Phase 1: Scope Lock

- [ ] Identify permitted files (only in spec scope)
- [ ] Identify forbidden files (docs_old, root specs, etc.)
- [ ] Identify mandatory validations (docs, Unity compile, log scan)
- [ ] Record scope boundaries in working notes

### Phase 2: Implementation

- [ ] Follow spec requirements exactly
- [ ] Do NOT amplify scope
- [ ] Use applicable skills: Event Bus Pattern, Save/Load Pattern, etc.
- [ ] Commit frequently with Portuguese messages
- [ ] Do NOT move spec to implementados yet

### Phase 3: Validation

- [ ] Run `/validate-unity` if runtime changed
- [ ] Run `/review-non-regression` to audit diff
- [ ] Fix any regressions found
- [ ] Re-run validation if fixes made

### Phase 4: Closeout

- [ ] Run `/finish-spec` to assemble closeout report
- [ ] Include all validations (executed and not executed)
- [ ] Move spec to `docs/specs/implementados/` with evidence
- [ ] Update `docs/IMPLEMENTATION_STATUS.md`
- [ ] Update `PROJECT_LOG.md`
- [ ] Do NOT push to remote

## Rules

1. **Never amplify scope.** Spec defines boundaries.
2. **Never alter files outside scope.** Check before each edit.
3. **Never skip validation.** If not executable, document why.
4. **Never mark as implemented without evidence.** Spec file or validated logs required.
5. **Never use prohibited patterns.** Respect CLAUDE.md rules:
   - No `GameObject.Find()` or `FindObjectOfType()`
   - No direct MonoBehaviour calls (use GameEventBus)
   - No hardcoded game data (use ScriptableObject)
   - No Unity refs in save (IDs and simple types only)

## Applicable Sub-Skills

- **Unity Validation Skill** — mandatory if runtime changes
- **Save/Load Pattern** — if spec involves persistence
- **Event Bus Pattern** — if spec involves gameplay communication
- **Docs Migration Skill** — at closeout to move spec files
- **Non-Regression Review** — before finishing to audit diff
- **Implementation Closeout** — final phase checklist

## Example: SPEC 12 Execution

```
Phase 0: Preparation
- User: "Start spec 12"
- Run /start-spec spec_12_player_combat
- Identify dependencies: SPEC 11 (done), SPEC 08 (independent)
- Confirm scope: Player melee/ranged attacks + basic UI

Phase 1: Scope Lock
- Permitted: Assets/Scripts/Runtime/Combat/*, Assets/_Game/Data/Combat/*
- Forbidden: Assets/Gameplay/*, docs_old/**, ProjectSettings/**
- Validations: Docs PASS, Unity compile PASS

Phase 2: Implementation
- Create WeaponDataSO with melee/ranged stats
- Implement PlayerCombatManager with attack logic
- Create DamageRequest in attack event (use GameEventBus)
- Add weapon hotbar selection
- Commit: "feat: spec 12 - player combat melee/ranged attacks"

Phase 3: Validation
- Run /validate-unity → PASS (Tundra build success)
- Run /review-non-regression → PASS (no forbidden patterns)

Phase 4: Closeout
- Run /finish-spec
- Move spec_12_player_combat.md to implementados/
- Update IMPLEMENTATION_STATUS.md: "Player Combat: implemented"
- Add PROJECT_LOG entry: "2026-05-26 Implemented SPEC 12..."
- Delivered to user with summary and logs
```

## Signs of Success

- ✅ All changes within spec scope
- ✅ No validation failures blocking task
- ✅ Non-regression audit shows PASS
- ✅ Spec moved to implementados/ with evidence
- ✅ Documentation updated and validated
- ✅ User can review and approve

## Signs of Blocker

- ❌ Validation cannot run and risk cannot be documented
- ❌ Non-regression audit shows FAIL on architectural patterns
- ❌ Spec scope ambiguous or contradicts SPEC_EXECUTION_ORDER.md
- ❌ Permitted files list unclear after reading spec
