# Execution Report — fable_32 Item Catalog Data Expansion

> Spec: `.specs/a_implementar/fable/fable_32_spec_item_catalog_data_expansion.md`
> Date: 2026-06-19
> Branch: `dev`
> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> Validation method: `run_strict_validation.ps1` (exit 0); Unity asset generation + Play Mode DEFERRED (owner-authorized).

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS` (not `BUILD_VALIDATED`, not `ACCEPTED`):

- The deterministic core — the canonical item catalog **data table**, the **idempotent generator**, the two **save-safe enum extensions**, and the **EditMode tests** — is build-validated (both assemblies exit 0) and the pure data rules were verified by direct execution of the catalog logic (see Validation).
- The actual `.asset` **generation is DEFERRED** (owner authorized skipping Unity Editor / Play Mode / asset generation). The `GenerateCanonicalItemCatalog` menu/batch command and the `fable_30` validator (`CindarsHope/Validate/Catalog Consistency`) have NOT been run against real assets in this session. Their commands are documented below for the human checkpoint (generated-asset-evidence: BLOCKED/deferred).
- Therefore CA-1 (F30 reports items COMPLETE with attached log) is **PENDING the deferred Unity run**; CA-2/CA-3/CA-4 deterministic content is proven by EditMode logic now.

No premature acceptance claim is made: no "Play Mode PASS", no "Unity validated", no "100% fulfilled".

---

## Acceptance criteria extracted

| CA | Requirement (incl. EMENDA V3) | Status | Evidence |
|----|-------------------------------|--------|----------|
| CA-1 | F30 validator reports items COMPLETE (count = catalog) + log attached | PENDING deferred Unity run | F30 count is WARN-only (ExpectedCount=118, pre-V3); generator registers all items into `ItemDatabaseSO` + loose `t:ItemDataSO` sweep. See "F30 count divergence". |
| CA-2 | Quality variants: Silver = round(base ×1.5), **Gold = round(base ×2.0)** (V3.1, NOT ×2.2); canonical suffix | DONE | `CanonicalItemCatalog.QualityValue` + tests `QualityVariants_UseCorrectMultipliers`, `GoldMultiplier_IsTwoPointZero_NotTwoPointTwo` (carrot 14→gold 28). |
| CA-3 | Idempotency: 2nd generator run = zero changes | DONE (by design) + DEFERRED proof | Generator upserts by id, updates only changed fields, never deletes; `GenerationCounters.NoChanges` signal. Catalog table determinism test `Catalog_IsDeterministic`. Two-consecutive-run log is the deferred Unity evidence. |
| CA-4 | Integrity: no orphan w/o category/BV; unique ids; recipes → existing ingredients only | DONE | Tests: `ExpandedItemIds_AreUnique`, `EveryItem_HasCategoryPrefixedId_AndName`, `SellableItems_HavePositiveBaseValue`, `RecipeIngredientsAndOutputs_ResolveToCatalogItems`, `RecipeIds_AreUnique`. Verified: 0 dup ids, 0 unresolved recipe refs, 0 sellable BV≤0 (see Validation). |
| V3.2 | Exactly **6 essences** (not 8) | DONE | `Essences_AreExactlySix` + group-count `CAT Essence=6`. |
| V3.3 | Save-safe enum extensions (WeaponType +Hammer/Wand/Tool high values; ItemCategory +Armor/Shield/Accessory/Relic/Essence/AnimalProduct ≥111; no renumber; Tool not duplicated) | DONE | `WeaponType_LegacyOrdinals_AreStable`, `WeaponType_NewMembers_HaveExplicitHighValues`, `ItemCategory_LegacyValues_AreStable`, `ItemCategory_NewMembers_HaveExplicitHighValues`, `Tool_IsNotDuplicatedAcrossEnums`. |
| V3.4 | Unify the two parallel item models (ItemDataSO vs. ItemDefinition) — decide & record | DONE (decision recorded) | Direction **(b) DEMARCATE** chosen — see "V3.4 decision" below. No runtime changed. |
| V3.5 | Catalog data from ITEM_CATALOG (A4): recipes/gifts/high-tier gear/orphan drops/fish | DONE (data) | All groups materialized from ITEM_CATALOG_DIRECTION_v1.0 + EMENDA V3 (BVs copied verbatim). |

---

## Existing systems audit

Phase 0 reuse audit (skill `system-reuse-audit`) — nothing parallel created:

| Concern | Found | Decision |
|---------|-------|----------|
| Item asset type | `ItemDataSO` (`Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs`) | REUSE as the catalog/economy surface (V3.4 canonical). No new SO type. |
| Item generator pattern | `ItemDataInitializer` (`[InitializeOnLoad]`) + archived `ItemDataGenerator` | EXTEND the same `AssetDatabase.CreateAsset` + load-by-id pattern in a new `GenerateCanonicalItemCatalog` (menu + batch). No 2nd database/initializer. |
| Item registry | `ItemDatabaseSO : DataRegistrySO<ItemDataSO>` (`Assets/_Game/Scripts/Core/Data/`) | REUSE; generator appends to `_items` via `SerializedObject` (same idiom as `RemoveNullEntries`). |
| Recipes | `RecipeDataSO` + `RecipeIngredient` (`Assets/_Game/Scripts/Craft/Data/`) | REUSE; generator authors recipe assets data-driven. No new recipe system. |
| Consumable effects | `ItemDataSO.HungerRestore/StaminaRestore/StatusEffectIds` + ConsumableSubtype (fable_07/08 surface) | Mapped via existing fields; no parallel effect system. Oils/essences are documented DORMANT (consumer = fable_22). |
| Item POCO | `ItemDefinition` (Rarity/Quality/EconomicFlags, `Assets/_Game/Scripts/Items/`) | Kept as runtime projection (V3.4 (b)); not a 2nd canonical authoring surface; not modified. |
| F30 validator | `ValidateCatalogConsistency` + pure `CatalogConsistencyEngine` + `CatalogExpectations` | REUSE read-only at closeout (deferred Unity run). NOT modified (out of this spec's scope). |
| `ItemDataSO` schema | Audited all fields | No additive field required — existing fields (Category incl. new Essence/Armor/etc., BaseValue, ConsumableSubtype, MaxStack, Hunger/StaminaRestore, DurabilityRestoreAmount, AmmoType) cover the catalog. ItemDataSO NOT modified. |

### V3.4 decision (recorded)

**Chosen: direction (b) — DEMARCATE responsibilities.**
- `ItemDataSO` (ScriptableObject) is the **canonical persisted/registry surface** of item data (Id, Category, BaseValue, stack, consumable effect fields, ammo, durability) and is the destination of the idempotent generator.
- `ItemDefinition` (POCO with Rarity/Quality/EconomicFlags) remains a **runtime view/projection**; it is NOT a second source of truth and gains no duplicated canonical fields in this spec.
- Rationale: minimizes rework and respects the forbidden-files rule (this is a data/editor spec — no runtime rewrite authorized). Unifying (direction (a)) would require touching runtime read/write sites of `ItemDefinition`, which is out of scope. Boundary documented here for the future system that may converge them.

---

## Spec Compliance Matrix

| Requirement (spec / EMENDA) | Implementation | Result |
|------------------------------|----------------|--------|
| Idempotent generator (update by id / create / never delete) | `GenerateCanonicalItemCatalog.UpsertItem` + `ApplyItemRowIfChanged` field-diff | OK |
| Single data-driven table ordered by catalog sections §3-19 | `CanonicalItemCatalog.BaseRows()` (pure, ordered by section, line-referenced comments) | OK |
| IDs `item_<category>_<slug>` | All rows; test `EveryItem_HasCategoryPrefixedId_AndName` | OK |
| Quality variants as separate items (`_silver` ×1.5 / `_gold` ×2.0) | `ExpandedRows()` + `CloneVariant`; tests CA-2 | OK |
| Consumables mapped to existing effect fields (F08); oils dormant | Foods/potions carry Hunger/Stamina; oils `Dormant=true` w/ note | OK |
| Essences = 6 (V3.2) | `AddEssences` (6); test `Essences_AreExactlySix` | OK |
| Monster-part drops (orphan IDs F06/F33 reference) | `AddOrphanDrops` (25, EN-only `item_material_*`, BV by band) | OK |
| Ammo (arrows, 6) | `AddOilsAndArrows` arrows | OK |
| New food/potion/oil recipes (§5/§7-8, E2.2/E2.9) | `RecipeRows()` (32 recipes); ingredients verbatim from E2.2; water/grub_meat/cow_milk per E2.9 | OK |
| Shop entries per NPC (§20) | DEFERRED — documented (see Deferred work); editing existing shop assets needs Unity | DEFERRED |
| Save-safe enums (V3.3) | `ItemCategory` +6 (≥111), `WeaponType` +3 (≥100), legacy values pinned explicit | OK |
| Enums additive; no renumber/reorder | Legacy `Seed=0..Furniture=110` and `None=0..Dagger=6` unchanged, now pinned explicit | OK |
| Save DTOs simple types | No save DTO touched; enums are integer-serialized (save-safe) | OK |
| No manual `.unity/.prefab/.asset` YAML edits | Assets only via the editor generator (deferred); none hand-edited | OK |
| `*Tests.cs` only under `Tests/EditMode/**` | `Assets/_Game/Tests/EditMode/Items/ItemCatalogDataTests.cs` | OK |
| No `GameObject.Find/FindObjectOfType` runtime | None added (data/editor only) | OK |
| Pricing/restock economy rules intact | Only BV authored; no pricing code touched | OK |
| High-tier gear nominal (E2.6, ~14) | `AddHighTierGear` (14, craft/temper, dormant) | OK |
| Water as item (E2.9): `item_tool_bucket` + `item_material_water` | `AddToolsAndUtilities`; water non-sellable BV 1 | OK |
| ~10 fish (E2.10) | `AddFish` (10) | OK |
| Durability/upgrade (E2.11) | Cross-referenced to `economy_rules`/`inventory_equipment_rules`; not re-derived (mechanics owned by fable_03/forge) | OK (cross-ref) |
| Deprecated `item_pedra_negra_estabilizada` → `item_material_stabilized_blackstone` | Canonical id used; corrupted_shard distinct | OK |

---

## Files changed

Runtime (Assembly-CSharp):
- `Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs` — appended `Armor=111, Shield=112, Accessory=113, Relic=114, Essence=115, AnimalProduct=116` (save-safe; legacy values untouched).
- `Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs` — `WeaponType` legacy ordinals pinned explicit (`None=0..Dagger=6`) + appended `Hammer=100, Wand=101, Tool=102`.

Editor (Assembly-CSharp-Editor):
- `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` — NEW. Pure, testable canonical catalog data table (207 base rows, 239 expanded, 32 recipes) + quality-variant expansion + rounding.
- `Assets/_Game/Scripts/Editor/Items/GenerateCanonicalItemCatalog.cs` — NEW. Idempotent generator (menu + batch) → `ItemDataSO` + `RecipeDataSO`, registers items into `ItemDatabaseSO`.

Tests (Assembly-CSharp-Editor):
- `Assets/_Game/Tests/EditMode/Items/ItemCatalogDataTests.cs` — NEW. 18 EditMode tests (uniqueness, BV>0, variants ×1.5/×2.0, 6 essences, recipe resolution, pinned counts, enum save-safety).

Local-only (gitignored, NOT committed): `Assembly-CSharp-Editor.csproj` updated with the 3 new file includes so the dotnet gate compiles them (Unity regenerates this file from `Assets/**`).

NOT touched / NOT committed: `tools/docs/check_spec_quality.ps1` (modified by another session; out of this spec's scope).

---

## Catalog group breakdown (materialized)

Base 207 → expanded 239 (crops & animal products ×3 quality). Verified by direct execution of the pure table.

| Group (catalog §) | Base count | Notes |
|---|---:|---|
| Seeds (§4) | 12 | |
| Crops (§4, ×3 quality) | 12 → 36 expanded | base/silver(×1.5)/gold(×2.0) |
| Foods (§5) | 20 | |
| Potions (§7) | 8 | |
| Oils (§8) | 4 | dormant (fable_22) |
| Arrows (§8) | 6 | |
| Materials (§9) | 10 | |
| Essences (§10/E2.5) | 6 | dormant (fable_22) |
| Specials (§11) | 4 | fruto_mana (special-channel), agua_viva (BV0), stabilized_blackstone, corrupted_shard |
| Orphan drops (E2.8) | 25 | EN-only `item_material_*` (+fiber) |
| Fish (E2.10) | 10 | 4 pond + 5 cave + 1 Moonless Pool |
| Weapons (§12) | 16 | catalog/economy surface; dormant |
| High-tier gear (E2.6) | 14 | craft/temper only; dormant |
| Armors (§13) | 6 | dormant |
| Shields (§13) | 3 | dormant |
| Wands+Scrolls (§14) | 7 | |
| Cave magic (E2.3) | 10 | 8 + unidentified_trinket + scroll_identify; dormant (fable_31) |
| Accessories (§16) | 12 | dormant (fable_23); 4 are `(—)` treasure → non-sellable |
| Relics (§17) | 4 | non-sellable lore; dormant (fable_23) |
| Animal products (§18, ×3 quality) | 4 → 12 expanded | |
| Tools/utilities (§19/E2.9) | 6 | lantern, bucket, water, 3 repair kits |
| Keys (§20) | 8 | non-sellable, BV 0 |

### F30 count divergence (expected WARN, not ERROR)

`CatalogExpectations.cs` (fable_30) declares `Items.ExpectedCount = 118` with **no canonical id list** (so items produce no per-id ERROR — only a count WARN). That 118 is the **pre-V3** figure. The V3 amendment ADDED high-tier gear (14), cave magic (10) and orphan drops (25), so the real post-V3 base catalog is 207 (239 expanded). When the deferred F30 run executes it will emit a benign count WARN ("239/118 — more assets than documented"), which is WARN per the engine and does **not** fail the gate. The fable_30 expectation is stale post-V3; updating it is out of this spec's scope (it belongs to fable_30 and is asserted at 118 by `CatalogValidatorTests.CanonicalExpectations_HaveDocumentedCounts`). Flagged for a human/fable_30 follow-up.

---

## Validation

Validation method: `run_strict_validation.ps1`
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 warnings, 0 errors)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings)
Quality check: PASS
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing legacy doc errors only; no new failure introduced)
Diff completeness: PASS

Pure-logic verification (catalog table executed directly via a throwaway .NET host, then discarded):
- BASE=207, EXPANDED=239, RECIPES=32
- DUP_IDS=none
- UNRESOLVED_RECIPE_REFS=none
- SELLABLE_BV0=none
- CARROT_GOLD=28 (14 ×2.0 — confirms Gold ×2.0)
- Group counts match the table above and the EditMode group-count assertions.

EditMode tests: 18 tests added in `ItemCatalogDataTests.cs`. They were **not executed via Unity Test Runner** in this session (Play Mode/Editor run deferred); their pure assertions were cross-verified by the direct catalog execution above (same logic path). Unity EditMode run is part of the deferred human checkpoint.

---

## Testing Quality Gate

```
Changed runtime code: YES (two enums — additive, save-safe)
Changed deterministic logic: YES (catalog data table, BVs, quality variants, idempotency)
Changed Unity scene/prefab/asset wiring: NO (asset generation deferred; no YAML edited)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Items/ItemCatalogDataTests.cs, 18 tests)
Automated tests command: Unity Test Runner EditMode (DEFERRED — pure logic cross-verified via throwaway host)
Manual Play Mode scenario: NOT REQUIRED (data-only; consuming specs validate use in their batches — per spec)
Justification if no automated tests: N/A (tests added)
Residual risk: F30 Unity validator run + two-run idempotency log are deferred; the F30 ExpectedCount=118 will WARN on count (benign). Real .asset generation deferred — items inert until generated and until consumer specs (F06/F22/F23/F31/F12) wire them.
```

Regression guard (anti-regression): items updated by id, never deleted/recreated (saved stacks keep ids); enum legacy values pinned + tested stable (`*_LegacyValues_AreStable`); economy pricing/restock untouched; no runtime logic changed.

---

## Generated-asset evidence (DEFERRED — generated-asset-evidence rule)

```
Asset generation: BLOCKED / DEFERRED (owner-authorized; Unity Editor not run this session)
Command attempted: none this session
Command to run at human checkpoint (sequential, no parallel Unity batchmode):
  1. Generate assets (menu):     CindarsHope/Generate/Data/Canonical Item Catalog
     or batch: Unity -batchmode -quit -projectPath . -executeMethod CindarsHope.Editor.Items.GenerateCanonicalItemCatalog.RunBatch
  2. Re-run generator once more  → expect counters: created=0, updated=0, registry added=0 (idempotency proof, CA-3).
  3. Validate (menu):            CindarsHope/Validate/Catalog Consistency
     or batch: Unity -batchmode -quit -projectPath . -executeMethod CindarsHope.Editor.Validation.ValidateCatalogConsistency.RunBatch
  4. Attach both logs (generator counts + F30 report) to this report; expect F30 items = 0 ERROR (count WARN only).
Residual risk: assets stale/missing until generated in Unity; F30 items COMPLETE (CA-1) pending this run.
```

---

## validated_game_rules

- `docs/game_rules/economy_rules.md` — Base Value mandatory per sellable item (all sellable rows BV>0); quality multipliers Silver ×1.5 / Gold ×2.0 (NOT Q4 ×2.20) applied to crops + animal products only; Mana Fruit special-channel-only (BV ~5000, non-ordinary-shipping flagged); key/quest items non-sellable BV 0. No pricing/restock/anti-arbitrage code changed (BV authored here; price/restock owned by economy).

---

## Dependency Chain

Original target: fable_32
Dependency chain: F30 (validator — EXISTS, BUILD_VALIDATED, used read-only/deferred); F08 (consumable effects — existing ItemDataSO effect fields reused)
Forbidden dependencies: none triggered
Resolved depth: 0 (no same-wave unresolved dependency required to write this data/editor spec)
Can continue original target: YES

---

## Remaining work (deferred / out of scope)

1. **Run the generator in Unity** + attach generator log and two-run idempotency proof (CA-3 evidence).
2. **Run the F30 validator** in Unity + attach log (CA-1 items COMPLETE evidence; expect benign count WARN).
3. **Run Unity EditMode Test Runner** for the 18 new tests (cross-verified here via pure execution).
4. **Shop entries per NPC (§20)** — DEFERRED: authoring/editing existing `ShopDataSO` assets needs Unity; the catalog ids now exist for those shop entries to reference. To be done with the consuming/economy specs.
5. **fable_30 ExpectedCount** is stale post-V3 (118 vs 207/239) — human/fable_30 decision whether to bump it.
6. Consuming systems (oils/essences→F22, accessories→F23, magic→F31, animal products→F12, drops→F06) wire the dormant items in their own specs.
