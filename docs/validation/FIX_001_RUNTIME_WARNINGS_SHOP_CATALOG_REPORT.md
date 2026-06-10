# FIX-001B Execution Report — Runtime Warnings + Shop Catalog Alignment

Date: 2026-06-10
Spec: FIX_001_runtime_warnings_town_shop_catalog_alignment.md
Branch: dev
Status: BUILD_VALIDATED

---

## Summary

FIX-001B closes out the FIX-001 spec by resolving the remaining global scene searches in runtime bootstrap files and expanding the shop catalog validator to cover all shop assets.

---

## Files Changed

### Code
- `Assets/_Game/Scripts/Craft/CraftingRuntime.cs` — Added `ActiveInstances` static list + `OnEnable`/`OnDisable` registration
- `Assets/_Game/Scripts/Craft/CraftingStationRuntimeBootstrap.cs` — Replaced `FindObjectsByType`/`FindObjectsOfType` with `CraftingRuntime.ActiveInstances`
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` — Added `Instance` static singleton (Awake guard + OnDestroy clear)
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeBridge.cs` — Replaced `FindAnyObjectByType`/`FindObjectOfType` with `CaveRunManager.Instance`
- `Assets/_Game/Scripts/UI/Quests/Runtime/QuestOfferPanelController.cs` — Strengthened Awake duplicate guard (`Destroy(gameObject)` for extras)
- `Assets/_Game/Scripts/UI/Quests/Runtime/QuestLogPanelController.cs` — Same as above
- `Assets/_Game/Scripts/Editor/Validation/ValidateTownShopCatalogIntegrity.cs` — Expanded scope: all 25 ShopDataSO assets, duplicate Id detection

### Docs
- `docs/validation/FIX_001_RUNTIME_WARNINGS_SHOP_CATALOG_DECISION.md` (new)
- `docs/validation/FIX_001_RUNTIME_WARNINGS_SHOP_CATALOG_REPORT.md` (this file, new)
- `docs/validation/FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md` (new)
- `docs/project/CURRENT_STATE.md` (updated — FIX-001 + FIX-001B status)

---

## Fix Descriptions

### Fix 1: CraftingRuntime Static Registry
- Added `public static readonly List<CraftingRuntime> ActiveInstances`
- `OnEnable()` adds instance; `OnDisable()` removes it
- `CraftingStationRuntimeBootstrap.BindWhenReady()` now reads `CraftingRuntime.ActiveInstances` (a `List<CraftingRuntime>`) instead of calling any global search
- `BindRuntimes` signature updated to accept `List<CraftingRuntime>`
- Summary comment in bootstrap updated to reflect no global search at any point

### Fix 2: CaveRunManager Instance Singleton
- Added `public static CaveRunManager Instance { get; private set; }`
- `Awake()` sets `Instance = this` with duplicate guard (`Destroy(gameObject)` if duplicate)
- `OnDestroy()` clears `Instance` if it is `this`
- `CaveRuntimeBridge.PublishCaveExitedEvent()` uses `CaveRunManager.Instance` with null-check + warning
- `CaveRuntimeBridge.ValidateCaveEntryState()` uses `CaveRunManager.Instance` — no global search

### Fix 3: QuestOfferPanelController Duplicate Guard
- Previous: `if (Instance == null) Instance = this` — duplicate not destroyed
- Fixed: `if (Instance != null && Instance != this) { Destroy(gameObject); return; }` followed by `Instance = this`
- Prevents double event subscription and double IMGUI rendering on scene reload

### Fix 4: QuestLogPanelController Duplicate Guard
- Same fix as Fix 3

### Fix 5: ValidateTownShopCatalogIntegrity Expanded Scope
- Previous scope: 5 hardcoded IDs (shop_thalindra, shop_corvus, shop_savra, shop_mirela, shop_hund)
- New scope: ALL ShopDataSO assets in Assets/_Game/Data/Economy (25 assets found)
- Added: duplicate Id detection across all shops
- Removed: hardcoded ID list requirement (validator now validates whatever exists)
- Kept: strongly-typed `ShopDataSO` API (not SerializedObject reflection)

---

## Global Search Cleanup Table

| File | FindObjectOfType | FindObjectsOfType | FindFirstObjectByType | FindAnyObjectByType |
|------|-----------------|------------------|-----------------------|---------------------|
| CraftingStationRuntimeBootstrap.cs | NO | NO | NO | NO |
| CraftingRuntime.cs | NO | NO | NO | NO |
| CaveRuntimeBridge.cs | NO | NO | NO | NO |
| CaveRunManager.cs | NO | NO | NO | NO |
| QuestRuntimeBootstrap.cs | NO | NO | NO | NO |
| QuestOfferPanelController.cs | NO | NO | NO | NO |
| QuestLogPanelController.cs | NO | NO | NO | NO |

All "violations" in grep output were in **comments only** (doc strings explaining what was removed). No functional calls to global search APIs remain.

---

## Shop Validator Scope

Previous: 5 hardcoded shop IDs checked
New: All 25 ShopDataSO assets in Assets/_Game/Data/Economy:
- Shop_Blacksmith, Shop_Brumdar, Shop_Cave_Supplies, Shop_Corvus, Shop_Dagna
- Shop_Eiran, Shop_General_Store, Shop_Gruta, Shop_Gurd, Shop_Hund
- Shop_Mara, Shop_Mirela, Shop_Nimble, Shop_Orlan, Shop_Ozzra
- Shop_Pip, Shop_Renko, Shop_Savra, Shop_Seeds_Tools, Shop_Sylveth
- Shop_Thalindra, Shop_Tovin, Shop_Weapons_Armor, Shop_Yael, Shop_Zrix

All NPC shops referenced by CreateMvpTownScene.RefinedCanonicalTownNpcSpecs are a subset of the above (20 shops; 3 NPCs — npc_alaric, npc_liora, npc_maelor — have empty ShopDataPath).

---

## Validation

### Assembly-CSharp
Status: PASS
Exit code: 0
Errors: 0
Warnings: 0

### Assembly-CSharp-Editor
Status: PASS
Exit code: 0
Errors: 0
Warnings: 3 (all pre-existing: CS0649 x2 in CreateEnemyActionsAndSets.cs, UNT0006 in CSharpProjectPostprocessor.cs)

### Docs Validation
Status: EXPECTED_FAIL_LEGACY_ONLY
Exit code: 1
New errors introduced by FIX-001B: 0
Pre-existing errors (legacy): FIX_001 spec name without spec_ prefix, missing validated_adrs in old reports, old amendment citations in spec_fase9h/9i

---

## Testing Quality Gate

```
Testing Quality Gate
────────────────────
Changed runtime code: YES
Changed deterministic logic: NO (registration patterns, singleton guards only)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO
Automated tests command: NOT RUN
Manual Play Mode scenario: docs/validation/FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md
Justification if no automated tests: Registration patterns (ActiveInstances, Instance singleton, Awake duplicate guard) require live MonoBehaviour lifecycle (Awake/OnEnable/OnDisable/OnDestroy) — not testable in EditMode without scene context. Static list and singleton behaviors are observable only in Play Mode.
Residual risk: QuestOffer/LogPanelController duplicate guard requires Play Mode verification of scene reload behavior; CaveRunManager.Instance null-safety requires Play Mode cave entry/exit test.
```

---

## Honest Status Rationale

Status `BUILD_VALIDATED` is accurate because:
- All C# builds pass (exit code 0, 0 errors)
- All 7 target runtime files are clean (zero global search API calls in functional code)
- The specific acceptance criteria of FIX-001 spec (eliminate CS0618, validate shops) are implemented
- Status is NOT inflated to ACCEPTED or PLAYMODE_VALIDATED — Play Mode checklist is pending human execution

What is NOT claimed:
- Play Mode tested: NO — Unity Editor action required
- Unity batchmode validated: NOT RUN (not applicable for pure C# changes)
- Human acceptance: PENDING (checklist at docs/validation/FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md)

---

*Author: Claude Sonnet 4.6 (FIX-001B, 2026-06-10)*
