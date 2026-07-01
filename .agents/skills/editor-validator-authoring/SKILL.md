---
name: editor-validator-authoring
description: Create Unity Editor validators following the project's 60-validator pattern (MenuItem, error/warning counters, AssetDatabase queries). Use when a spec requires asset/scene/data validation evidence or a catalog consistency check (e.g., fable_30).
---

# Skill: Editor Validator Authoring

The project has ~60 validators in `Assets/_Game/Scripts/Editor/Validation/` — this is the most-produced artifact after tests. Follow the established pattern exactly.

## Template (matches ValidateSpec17AScaleConfig and siblings)

```csharp
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class Validate<SpecOrDomain><Thing>
    {
        private const string AssetPath = "Assets/_Game/Data/<Area>/<Asset>.asset";

        [MenuItem("CindarsHope/Validation/Validate <Spec> - <Thing>")]
        public static void Run()
        {
            var errors = 0;
            var warnings = 0;

            var guids = AssetDatabase.FindAssets("t:<TypeSO>", new[] { "Assets/_Game/Data/<Area>" });
            if (guids.Length == 0)
            {
                Debug.LogWarning("[<tag>] No <TypeSO> assets found. Run the generator menu first.");
                warnings++;
            }
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<TypeSO>(path);
                if (so == null) continue;

                if (string.IsNullOrEmpty(so.Id)) { Debug.LogError($"[<tag>] {path} has empty Id."); errors++; }
                // range checks, registry membership, duplicate ID checks...
            }

            Debug.Log($"[<tag>] Validation finished. Errors={errors}, Warnings={warnings}");
            if (errors > 0) Debug.LogError($"[<tag>] VALIDATION FAILED with {errors} error(s).");
        }
    }
}
```

## Conventions

- Namespace `CindarsHope.Editor.Validation`; file in `Assets/_Game/Scripts/Editor/Validation/`.
- Log prefix `[<spec-tag>]` on every line (e.g., `[17A]`) so ScanUnityLogs can attribute findings.
- `Debug.LogError` for violations (counted), `Debug.LogWarning` for missing-but-generatable (counted separately), final summary line with both counts.
- When a generator must run first, name the exact menu in the warning message.
- For batchmode use, also expose a static method suitable for `-executeMethod` and document it in the execution report (rule: unity-assets / generated-asset-evidence).

## What every catalog validator should check

1. No empty/duplicate stable IDs.
2. Every catalog entry is reachable from its registry/database SO (no orphans).
3. Every registry reference resolves to an existing asset (no dangling).
4. Numeric ranges within game_rules bounds when canonical values exist.

## Menu placement

Active validators: `CindarsHope/Validation/...`. One-shot historical spec validators may be archived under `CindarsHope/Archive/Validation/...` (existing precedent). Prefer ONE domain validator extended over time instead of a new per-spec validator when checks overlap — the per-spec pattern already produced 60 files.

## Evidence

Closeout must record: menu/method used, log path, exit code, counts. Blocked Unity = report `BLOCKED` with reason, never claim validated (rule: validation-truth).
