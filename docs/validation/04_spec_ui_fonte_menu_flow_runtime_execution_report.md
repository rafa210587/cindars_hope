# SPEC 04 — UI Fonte Menu Flow Runtime — Execution Report

> **Spec ID:** `04_spec_ui_fonte_menu_flow_runtime`  
> **Status:** CONTRACT_ONLY  
> **Date:** 2026-06-08  
> **Executor:** Claude Code  
> **Branch:** dev  
> **Priority:** P1  

---

## Summary

**Objective:** Create Fonte menu view model with fragment-gated function visibility.

**Decision:** CREATE_MINIMAL view model; DEFERRED_INTEGRATION for wiring to Fonte runtime.

**Scope Executed:**
- ✓ FonteMenuViewModel.cs (menu state projection)

**Scope Deferred:**
- ✗ Integration with Fonte runtime
- ✗ Main progression hook wiring
- ✗ Fragment unlock logic
- ✗ Living Water/respec/purification mechanics
- ✗ Final choice runtime
- ✗ Scene/prefab wiring

---

## Status Rationale

**Why CONTRACT_ONLY:** Pure view model exists and compiles. No integration with Fonte runtime, main progression system, or fragment unlock mechanics. Defines what data should be displayed but does not connect to actual Fonte state.

---

## Files Created

### Source Files

| File | Purpose | Role | Status |
|------|---------|------|--------|
| `Assets/_Game/Scripts/UI/Fonte/FonteMenuViewModel.cs` | Menu state projection | View model | ✓ Compiles |

---

## Scope Actually Executed

1. ✓ FonteMenuViewModel with mana/ritual state
2. ✓ Action enum (ViewStatus, PerformRitual, etc.)
3. ✓ Capability flags (CanPerformRitual, CanUpgradeMana)

---

## Scope Not Executed (Deferred)

1. ✗ Fragment unlock gate implementation
2. ✗ Spoiler prevention (Anya final state, level 101 secret)
3. ✗ Living Water cost/effect wiring
4. ✗ Respec backend integration
5. ✗ Purification/healing mechanics
6. ✗ Final choice gameplay

---

## Validation Results

### Docs Validation

```
Status: PASS
```

### Assembly-CSharp Build

```
Status: ✓ PASS
```

### Assembly-CSharp-Editor Build

```
Status: ✓ PASS
```

---

## Testing Quality Gate

| Aspect | Status | Notes |
|--------|--------|-------|
| Changed deterministic logic | NO | Pure data projection |
| Requires EditMode tests | NO | No logic to test |
| Requires PlayMode or human scenario | YES | DEFERRED |
| Minimum validation for BUILD_VALIDATED | NOT MET | No integration = CONTRACT_ONLY |

---

## Decision

**Status:** CONTRACT_ONLY

**Can continue next SPEC?** YES

**Blocks WAVE 05?** NO

---

## Sign-Off

**Executor:** Claude Code  
**Date:** 2026-06-08  
**Branch:** dev

---

*This spec defines Fonte menu contracts; integration deferred.*
