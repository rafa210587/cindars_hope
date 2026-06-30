---
doc_type: adr
status: accepted
adr_id: ADR-0008
title: Unity Scene/Asset YAML Editing Policy
date: 2026-06-01
source_documents:
  - .claude/rules/unity-assets.md
supersedes: []
superseded_by: []
applies_to:
  - unity-asset-editing
  - scene-management
  - prefab-wiring
---

# ADR-0008 — Unity Scene/Asset YAML Editing Policy

## Status

**accepted** (tooling decision for asset safety)

## Context

Unity serializes scenes, prefabs, and assets in YAML format. The format is fragile: manual edits frequently cause GUID mismatches, broken component references, missing fields, and silent data corruption. Question: When is manual YAML editing acceptable, and what is the safe path?

## Decision

**Do not manually edit `.unity`, `.prefab`, or `.asset` YAML unless the spec explicitly authorizes it AND Unity Editor API is unavailable.**

### Safe Paths (Preferred)

1. **Use Unity Editor UI**
   - Inspector for component setup
   - Prefab variants for inheritance
   - Scene pinning for hierarchies
   - Drag-and-drop for references

2. **Use Editor Scripts**
   ```csharp
   // Safe: use AssetDatabase, PrefabUtility, SerializedObject
   var prefab = PrefabUtility.LoadPrefabContents("Assets/Path/Prefab.prefab");
   var component = prefab.GetComponent<MyComponent>();
   component.field = newValue;
   PrefabUtility.SaveAsPrefabAsset(prefab, "Assets/Path/Prefab.prefab");
   ```

3. **Use Repair Menus**
   - `CindarsHope/Repair and Validate Project`
   - Validator menus (CombatDatabaseValidator, etc.)
   - Import/regenerate menus

4. **Use Editor Tools**
   - Asset generators (StatusEffectDatabase, ItemDatabase, etc.)
   - Serialization scripts
   - Batch processing via `-executeMethod`

### Prohibited Without Spec Authorization

```
Manual edits to:
- Assets/*.unity (scene files)
- Assets/**/*.prefab (prefab files)
- Assets/**/*.asset (ScriptableObject assets)
```

### When Manual YAML Edit Might Be Necessary

1. **GUID/reference broken** → Use repair menu, not manual edit
2. **Asset corrupted** → Use reimport/regenerate, not manual edit
3. **Prefab variant conflict** → Use PrefabUtility, not manual edit
4. **Emergency data fix** → Document clearly; prefer script next time

### If Spec Authorizes Manual Edit

Must include explicit statement:
```
Authorized: manual YAML edit of `Assets/Path/File.unity`
Reason: <specific blocker preventing Editor/script alternative>
Change: <exact change to make>
```

And record the risk:
```
Unity YAML edit: AUTHORIZED
File: Assets/Path/File.unity
Risk: GUID/reference integrity not verified until opened in Editor
Mitigation: Reopen file in Unity Editor after edit to validate
```

### If Unity Editor Is Unavailable

Document the situation:
```
Unity YAML edit: BLOCKED
Reason: <editor lock | sandbox | license timeout>
Command attempted: <command to open Editor>
Residual risk: Changes not verified; must test when Editor available
```

Do not proceed with manual edit unless the spec explicitly overrides this (rare).

## Consequences

- Scene/prefab/asset integrity is preserved
- GUID/reference corruption is prevented
- Changes are auditable and reversible (scripts vs. raw YAML)
- Manual edits create merge conflicts and versioning issues
- Editor scripts create repeatable, testable changes

## Applies To

- All scene wiring (character spawns, portal links, etc.)
- All prefab inheritance and variants
- All ScriptableObject asset creation/modification
- All test scene setup

## Validation

Hook `.claude/hooks/unity-yaml-edit-guard.ps1` detects when a task modifies `.unity`/`.prefab`/`.asset` files without spec authorization.

## Source Documents

- [unity-assets.md](./../.claude/rules/unity-assets.md) — YAML editing policy & asset generation evidence

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0001 (canonical structure for asset organization)*
