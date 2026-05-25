# Bugfix Validation - SPEC 07 Crafting Initializer and Data IDs

> Date: 2026-05-24
> Scope: editor initializer side effects and duplicate `ItemDataSO` IDs

## Defects Reproduced

- `ItemDatabase` contained both `Item_Trigo.asset` and generated `item_crop_wheat.asset` with `Id = item_crop_wheat`.
- A second latent duplicate existed outside the registry: `Item_Cenoura.asset` and generated `item_crop_carrot.asset`.
- `CraftingRecipeInitializer` used `[InitializeOnLoad]` and rewrote recipes, registries and `PlayerData` during each editor reload, producing asset import version conflicts.

## Correction

- Removed automatic reload execution from `CraftingRecipeInitializer`; content generation remains an explicit menu operation.
- Generator lookups now reuse an existing `ItemDataSO` with the same `Id` before creating an asset.
- Registry and starter-kit insertion now deduplicate by stable item `Id`.
- Preserved legacy crop assets used by `Seed_Trigo` and `Seed_Cenoura`; removed generated duplicate crop assets.
- Rebound the wheat starter entry to the preserved `Item_Trigo.asset`.

## Automated Evidence

| Check | Result | Evidence |
|---|---|---|
| Item asset ID scan | PASS | No duplicate `Id` in `Assets/_Game/Data/Items/*.asset` after correction. |
| Explicit initializer execution | PASS | `Logs/bugfix-crafting-initializer-generation.log`: Tundra success, return code 0, no duplicate/import-version error. |
| Crafting functional validation | PASS | `Logs/bugfix-crafting-validation.log`: `ValidateCraftingSystem.ValidateSpec07` passed, return code 0. |
| Domain reload without auto-write | PASS | `Logs/bugfix-unity-compile-validation.log`: `0 items updated`, Tundra success, internal return code 0, no bug signatures. |
| Standard wrapper process status | ANOMALY | `RunUnityCompileValidation.ps1` returned process code 1 although Unity log ended with internal return code 0. |
| Unity log scanner | FAIL (known noise) | Reports `Assembly-CSharp-Editor-firstpass.dll` and `Assembly-CSharp-firstpass.dll`; no `error CS` or bug signature. |

## Residual Risk

No interactive Play Mode action was required for this editor-data fix. Future explicit data-generation tools must retain ID-based deduplication when adding registry entries.
