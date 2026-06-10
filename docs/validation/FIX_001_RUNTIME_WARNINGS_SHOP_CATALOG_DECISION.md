# FIX-001 / FIX-001B — Decision Record

Date: 2026-06-10
Status: BUILD_VALIDATED

---

## Problems Found in FIX-001 (7 items from spec scope)

1. `CraftingStationRuntimeBootstrap` used `FindObjectsOfType<CraftingRuntime>()` / `FindObjectsByType<CraftingRuntime>()` (CS0618 + rule violation)
2. `CaveRuntimeBridge.PublishCaveExitedEvent` used `FindAnyObjectByType<CaveRunManager>()` / `FindObjectOfType<CaveRunManager>()` (CS0618 + rule violation)
3. `CaveRuntimeBridge.ValidateCaveEntryState` used `FindAnyObjectByType<CaveRunManager>()` / `FindObjectOfType<CaveRunManager>()` (CS0618 + rule violation)
4. `QuestOfferPanelController.Awake` had incomplete duplicate guard: `if (Instance == null) Instance = this` — duplicate instance was not destroyed
5. `QuestLogPanelController.Awake` had incomplete duplicate guard: same pattern as above
6. `ValidateTownShopCatalogIntegrity` only covered 5 hardcoded shop IDs (shop_thalindra, shop_corvus, shop_savra, shop_mirela, shop_hund) — 20 additional shops were unvalidated
7. All 5 original validated shops were clean (no missing ItemIds) — no catalog mismatch errors found

---

## Decision for Each Fix

### Fix 1: CraftingRuntime — Static ActiveInstances Registry

Decision: Add `public static readonly List<CraftingRuntime> ActiveInstances` to `CraftingRuntime`. Instances register in `OnEnable()` and unregister in `OnDisable()`. `CraftingStationRuntimeBootstrap` reads from this list instead of calling any global search API.

Rationale:
- Eliminates all `FindObjectsOfType` / `FindObjectsByType` calls at runtime
- No CS0618 — uses no deprecated API at all
- Pattern consistent with `QuestRuntimeBootstrap` static accessors (`QuestService`, `QuestRegistry`)
- List is safe: `OnEnable/OnDisable` lifecycle guarantees correct population when bootstrap coroutine runs

### Fix 2: CaveRunManager — Static Instance Singleton

Decision: Add `public static CaveRunManager Instance` property to `CaveRunManager`. Singleton set in `Awake()` with duplicate guard (`Destroy(gameObject)` for extras), cleared in `OnDestroy()`. `CaveRuntimeBridge` uses `CaveRunManager.Instance` instead of global searches.

Rationale:
- `CaveRunManager` is `[DisallowMultipleComponent]` with `DontDestroyOnLoad` semantics (single per run)
- Static singleton is the correct pattern for this type of manager
- Null-check + warning in `CaveRuntimeBridge` handles the case where `CaveRunManager` is absent (CaveScene not loaded yet or not wired)
- Consistent with the project's other bootstrap singletons

### Fix 3: QuestOfferPanelController Duplicate Guard

Decision: Replace `if (Instance == null) Instance = this` with proper guard:
```csharp
if (Instance != null && Instance != this) { Destroy(gameObject); return; }
Instance = this;
```

Rationale:
- Previous guard silently allowed duplicate instances to exist and subscribe to events
- After scene reload, both instances would receive `QuestGiverInteractedEvent` — double UI behavior
- Proper destroy guard ensures only one instance is active at any time

### Fix 4: QuestLogPanelController Duplicate Guard

Decision: Same pattern as Fix 3.

Rationale: Same as Fix 3. `QuestLogPanelController` subscribes to 4 events — duplicates would cause double subscription and double IMGUI rendering.

### Fix 5: ValidateTownShopCatalogIntegrity — Full Scope

Decision: Remove hardcoded list of 5 shop IDs. Scan ALL `ShopDataSO` assets in `Assets/_Game/Data/Economy` using `AssetDatabase.FindAssets("t:ShopDataSO", ...)`. Add duplicate `Id` detection.

Rationale:
- 25 shop assets exist in Data/Economy; the original validator only covered 5
- CreateMvpTownScene.RefinedCanonicalTownNpcSpecs references 20 NPC shops — all a subset of the 25 in Data/Economy
- Additional shops (Shop_Blacksmith, Shop_Cave_Supplies, Shop_General_Store, Shop_Seeds_Tools, Shop_Weapons_Armor) were not previously validated
- Full-scope scan catches regressions for any shop, not just the original 5 NPC shops
- Uses the same strongly-typed `ShopDataSO` API (not SerializedObject reflection) — cleaner, type-safe

---

## What FIX-001 Got Right

- Shop catalog audit found zero missing ItemIds in the 5 shops it checked
- `ValidateTownShopCatalogIntegrity` was created (did not previously exist)
- CS0618 in `QuestRuntimeBootstrap` was fully resolved (now uses static Instance accessors)
- `QuestOfferPanelController.Instance` and `QuestLogPanelController.Instance` were added

## What FIX-001B Adds

1. Removes remaining CS0618/global-search calls in `CraftingStationRuntimeBootstrap` and `CaveRuntimeBridge`
2. Adds `CaveRunManager.Instance` singleton
3. Adds `CraftingRuntime.ActiveInstances` static registry
4. Strengthens duplicate guards in Quest UI controllers
5. Expands shop validator from 5 shops to all 25 shops in Data/Economy
6. Creates this decision doc, the full execution report, and the Play Mode checklist

---

*Author: Claude Sonnet 4.6 (FIX-001B)*
*See: docs/validation/FIX_001_RUNTIME_WARNINGS_SHOP_CATALOG_REPORT.md*
