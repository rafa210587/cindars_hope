---
name: data-catalog-authoring
description: Author bulk game data catalogs (items, bestiary creatures, skills, status effects) as ScriptableObjects with stable IDs, registry membership and validator coverage. Use for catalog expansion specs — fable_01 status effects, fable_29 skill catalog, fable_32 item catalog, fable_33 bestiary 60 creatures.
---

# Skill: Data Catalog Authoring

The fable queue contains several bulk-data specs. The project already has 50+ `*SO` types and the pattern below — follow it instead of inventing per-spec structures.

## Established conventions

- **Type naming:** `<Thing>DataSO` for entries (`ItemDataSO`, `EnemyDataSO`, `NpcDataSO`, `SeedDataSO`), `<Thing>DatabaseSO` / `*RegistrySO` for collections (`RecipeDatabaseSO`, `CombatRuntimeDatabasesRegistrySO`, `CaveBossGateRegistrySO`).
- **Location:** class under the domain folder in `Assets/_Game/Scripts/<Domain>/` (or `<Domain>/Data/`); assets generated under `Assets/_Game/Data/<Area>/`.
- **Stable IDs:** every entry has a string id, lowercase_snake, never renamed after shipping in a save (`StableIdsValidationTests` exists — extend it). IDs come from the spec/GDD tables, not improvised.
- **No hand-written .asset YAML** (rule: unity-assets). Assets are created by an editor generator script using `AssetDatabase.CreateAsset()` — see `tools/unity/GenerateSpawnEcologyAssets.ps1` + `Editor/EnemyTaxonomy/GenerateAndWireSpec13GAssets.cs` precedent.

## Workflow for a catalog spec

1. **Schema first:** confirm/extend the `*DataSO` fields against the spec table. New fields get defaults that keep existing assets valid.
2. **Generator:** editor script (`[MenuItem("CindarsHope/Generate/Data/...")]` + batchmode `-executeMethod` entry) that creates/updates assets idempotently — re-running must not duplicate or reset hand-tuned values unless `force` is passed.
3. **Registry wiring:** every generated entry registered in its database/registry SO; database wired via GameBootstrap (skill: bootstrap-wiring).
4. **Validator:** extend or create the catalog validator (skill: editor-validator-authoring): no duplicate/empty IDs, no orphans, no dangling refs, ranges within game_rules.
5. **Tests:** `*ContractTests` for schema defaults + `*ValidationTests` for catalog integrity that can run without Unity (pure data rules) (skill: editmode-test-authoring).
6. **Evidence:** generation evidence block (command, log, exit code, expected vs. actual asset counts) — rule: unity-assets.

## Bulk-content quality rules

- Source every numeric from the spec's table; if the spec lacks a value, STOP and report (do not invent balance numbers — see agent game-design-reviewer).
- Batch in reviewable chunks (e.g., 15 creatures per commit for fable_33's 60), each chunk validator-clean.
- Cross-catalog refs (creature → loot table → item) must exist before the referencing entry, or the validator must fail loudly.
