# Rule: Unity YAML Editing Policy

## Rule

Do not manually edit `.unity`, `.prefab`, or `.asset` YAML files unless the spec explicitly authorizes it AND Unity Editor API (repair menus, editor scripts) is unavailable.

## Why

Unity serializes scene, prefab, and asset data in a fragile YAML format. Manual edits frequently cause GUID mismatches, broken component references, missing serialized fields, and scene corruption that is difficult to diagnose. The correct path is always through Unity Editor UI, repair menu commands, or editor scripts.

## Applies To

All tasks touching scene files, prefab files, or ScriptableObject asset files.

## Violation Examples

- Editing `Assets/_Game/Scenes/Farm.unity` directly to wire a new component
- Manually patching a `*.prefab` YAML to add a serialized field
- Creating a `.asset` file by writing raw YAML instead of using `AssetDatabase.CreateAsset()`

## Allowed Operations

- Reading `.unity`/`.prefab`/`.asset` files to understand current structure (read-only audit)
- Writing new `.cs` MonoBehaviour or ScriptableObject scripts (not YAML)
- Running repair menus (`CindarsHope/Repair and Validate Project`)
- Writing editor scripts that use `AssetDatabase`, `PrefabUtility`, or `SerializedObject` APIs

## What To Do If Exception Is Needed

The spec must contain an explicit statement such as: "Authorized: manual YAML edit of `Assets/_Game/Data/Combat/StatusEffectDatabase.asset` to set `_entries` list."

If Unity Editor is unavailable and a YAML edit is the only path, document:

```text
Unity YAML edit: BLOCKED
Reason: Unity Editor unavailable
Manual edit attempted: <path>
Residual risk: GUID/ref integrity not verified; must be checked in Unity Editor
```

## Validation / Detection

Hook `.claude/hooks/unity-yaml-edit-guard.ps1` (disabled by default) detects when post-edit scope includes `.unity`, `.prefab`, or `.asset` files without spec authorization.
