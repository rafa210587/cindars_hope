# WAVE 05 UI Input Namespace Repair Plan

## Status
IN_PROGRESS

## Problem
`namespace CindarsHope.UI.Input` shadows `UnityEngine.Input`, causing `Input.GetKeyDown` compile errors in 13+ UI files.

## Decision
Rename namespace to `CindarsHope.UI.InputRouting` (no behavior change, mechanical refactor only).

## Scope
- C# namespace declaration rename only
- Update `using` directives
- Update fully-qualified references
- No gameplay behavior change
- No scene/prefab/asset changes

## Files to Inspect
(will be populated after step 2)

## Steps
1. Map all references to `CindarsHope.UI.Input`
2. Rename namespace declaration
3. Update using statements
4. Update references
5. Verify Input.GetKeyDown resolves to UnityEngine.Input
6. Run strict validation
7. Report results

## Stop Conditions
- Rename requires scene/prefab/asset change
- Rename creates new compile errors outside references
- Build still fails after mechanical rename (indicates deeper issue)

---
*Plan: WAVE 05 UI Input Namespace Repair*
*Date: 2026-06-08*
