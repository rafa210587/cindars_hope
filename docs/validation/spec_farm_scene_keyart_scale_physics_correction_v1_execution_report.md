# FarmScene Keyart Scale and Physics Correction — Execution Report

Status: `BUILD_VALIDATED_WITH_WARNINGS`

## Outcome

- Homestead roof target width: `9` → `7.5` world units.
- Craft visual target height: `3` → `1.8` world units.
- Each crafting station keeps its `CraftingPoint` trigger and now gets one non-trigger `SolidBody` child collider.
- Decoration excludes an additional two-unit clearance around house, greenhouse and crafting yard.
- Navigation validator now checks real `Physics2D` probes: bridge centre `(15,3)` free; water banks `(15,5)` and `(15,1)` solid.

## Existing Systems Audit

`CraftingPoint`, `FarmSceneSpatialContract`, `FarmDecorationPlanner` and the existing bridge collision paths were reused. No new gameplay service, player physics, scene YAML, asset, save schema or art was created.

## Validation

| Check | Result |
|---|---|
| `dotnet build .\CindarsHope.Editor.csproj` | PASS — 0 errors; existing obsolete-API/serialization warnings remain. |
| `git diff --check` | PASS. |
| Unity regeneration / navigation menu / capture | NOT RUN — requires regenerating FarmScene in Unity. |

## Required Unity Verification

1. Run `CindarsHope/Inicializar Projeto` to regenerate FarmScene.
2. Run `CindarsHope/Validar Navegacao FarmScene`; expect `Bridge corridor physics: PASS`.
3. In Play Mode, cross the bridge in both directions; walk into each craft from all sides and verify it blocks while interaction still opens.
4. Capture homestead and crafting closeups; confirm no house/greenhouse overlap and crafts are at most twice player height.

## Honest Status Rationale

The C# editor build proves the code compiles. Physical overlap and final keyart composition require the generated Unity scene, so this is not Unity or Play Mode validated.
