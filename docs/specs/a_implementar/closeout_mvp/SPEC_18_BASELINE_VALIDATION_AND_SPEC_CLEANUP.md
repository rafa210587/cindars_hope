# SPEC_18 - Baseline Validation and Spec Cleanup

**Status:** IN EXECUTION  
**Branch:** dev  
**Executor:** Claude Code  
**Mode:** Validation and Cleanup Only  
**Date Started:** 2026-06-01

## Objective

Create factual baseline validation after SPEC_04-11 (Architecture Reorganization) and residual fixes. Determine whether SPEC_19 (gameplay closeout) can be unblocked based on objective evidence.

---

## Scope

**Allowed:** Validating, documenting, creating repair scripts, running validators.  
**Prohibited:** Feature implementation, runtime refactoring, gameplay changes, manual YAML edits.

---

## Deliverables (By End of SPEC_18)

1. ✓ **Audit Matrix** — `docs/validation/spec_18_audit_matrix.md`
2. ✓ **Execution Report** — `docs/validation/spec_18_baseline_validation_and_spec_cleanup_execution_report.md`
3. ✓ **Reorg Closure Declaration** — `docs/specs/a_implementar/reorg/README_STATUS.md`
4. **Phase 1 Results** — Automated build/docs validation
5. **Phase 2 Results** — Manual Unity validators (if available)
6. **Phase 3 Results** — Play Mode testing (if available)
7. **PROJECT_LOG Update** — Append SPEC_18 closure entry
8. **Final Decision** — SPEC_19 unblocked or blocked, with evidence

---

## Phase 1: Automated Builds & Docs Validation

### Commands to Execute (Sequential)

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

**Expected Results:**
- `dotnet build Assembly-CSharp.csproj`: 0E/0W
- `dotnet build Assembly-CSharp-Editor.csproj`: 0E/2W (pre-existing)
- `tools/docs/validate_docs.ps1`: 14/14 PASS

**Stop Condition:** Any command fails with code error → STOP, document reason, mark SPEC_19 BLOCKED.

---

## Phase 2: Manual Unity Validators

**Requires:** Unity Editor available

### Validators to Execute (Sequential)

1. `CindarsHope/Repair/Scenes/Repair TownScene Combat Bootstrap Wiring`
2. `CindarsHope/Repair and Validate Project`
3. `CindarsHope/Validate/Combat/Validate Projectile Prefabs`
4. `CindarsHope/Validate/Combat/Validate Combat Databases`
5. `CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP`
6. `CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP`

**Stop Condition:** Any validator reports new critical error → document, do not proceed to Phase 3.

---

## Phase 3: Play Mode Testing

**Requires:** Unity Editor available

### Manual Checklist (Open TownScene, Enter Play Mode)

**Console Verification:**
- ✓ Should appear: `CombatRuntimeInstaller: Install completed. ItemDb=ItemDatabase, WeaponDb=WeaponDatabase, SpellDb=SpellDatabase, StatusEffectDb=StatusEffectDatabase...`
- ✗ Should NOT appear: `WeaponDatabase is null`, `SpellDatabase is null`, `ManaManager is null`

**Gameplay Checklist:**
- [ ] Starter inventory applied (bow, arrows, fireball)
- [ ] Shops initialized and interactable
- [ ] DebugHud functional
- [ ] Movement (WASD) works
- [ ] Interaction (E) works
- [ ] Attack (Space) works
- [ ] Spell (Q) works

**Stop Condition:** Any new critical error in Console → document, mark SPEC_19 BLOCKED.

---

## Phase 4: Documentation & Final Decision

### Updates to Make

1. `PROJECT_LOG.md` — Append SPEC_18 closure summary
2. Verify `docs/IMPLEMENTATION_STATUS.md` reflects SPEC_04-11 closure
3. Verify `docs/backlog/reorg_architecture_residual_backlog.md` updated

### Decision Criteria

**SPEC_19 UNBLOCKED if:**
- Phase 1: All builds PASS (or documented NOT RUN with environmental reason, last known PASS)
- Phase 2: Validators run without new critical errors (or documented NOT RUN with human constraint)
- Phase 3: Play Mode shows no new critical errors (or documented NOT RUN with human constraint)
- No stop condition triggered

**SPEC_19 BLOCKED if:**
- Any stop condition triggered
- Build fails with code issue
- Validator reports critical error not resolvable in SPEC_18
- Play Mode shows new critical error

---

## Reorg Status Summary

| Item | Status |
|------|--------|
| SPEC_04-11 Code | COMPLETE |
| Compilation (last known) | PASS 0E/0W runtime, 0E/2W editor |
| Residual Fix #1: Combat Assets | COMPLETE |
| Residual Fix #2: Projectile Prefabs | COMPLETE |
| Residual Fix #3: TownScene Wiring | COMPLETE |
| Repair Scripts Created | COMPLETE |
| Automated Validation (Phase 1) | PENDING |
| Manual Validators (Phase 2) | PENDING HUMAN |
| Play Mode Testing (Phase 3) | PENDING HUMAN |
| SPEC_19 Decision | PENDING |

---

## Execution Evidence

All results documented in `docs/validation/spec_18_*.md`.

Stop conditions checked at each phase; decision made objectively based on evidence.
