# Rule: Unity Assets & Editor Safety

Consolidates: `unity-yaml-editing-policy`, `generated-asset-evidence`, `no-parallel-unity-batchmode` (originals are stubs pointing here).

## 1. No manual YAML edits

Do not manually edit `.unity`, `.prefab`, `.asset` files unless the spec explicitly authorizes it AND the Unity Editor API path (repair menus, editor scripts with `AssetDatabase`/`PrefabUtility`/`SerializedObject`) is unavailable.

Allowed: reading YAML for audit; writing `.cs` scripts; running repair menus; editor scripts using Unity APIs.

If a manual edit is unavoidable, document:

```text
Unity YAML edit: BLOCKED / EXECUTED WITH AUTHORIZATION
Reason: <Unity Editor unavailable>
Residual risk: GUID/ref integrity not verified; must be checked in Unity Editor
```

Enforcement: `permissions.ask` in `.claude/settings.json` prompts the human for every Edit/Write on `.unity/.prefab/.asset` — approval there counts as per-instance authorization.

## 2. Generated assets require evidence

When a spec depends on generated Unity assets, closeout must record: menu or `-executeMethod` used, log path, exit code, affected folders, expected vs. actual asset count, any menu/method divergence.

If generation cannot run:

```text
Asset generation: BLOCKED
Reason: <Unity lock | license | timeout | sandbox | approval | compile error>
Command attempted: <command>
Residual risk: assets may be stale/missing until generated in Unity
```

Never claim generated assets exist because the generator code exists. Never silently accept mismatched roster/registry assets.

## 3. No parallel Unity batchmode

Never run multiple Unity batchmode processes for the same project simultaneously. Run generators/validators sequentially, one log file per command, wait for exit. If Unity is already open: close it (if authorized) or record the validation as BLOCKED — never launch more instances hoping one wins.

Enforcement: hook `pre-bash-guard.ps1` (PreToolUse) blocks batchmode launch while a Unity process is running.
