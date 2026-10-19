# Execution Report — fable_31 Magic Items: Unidentified + Identification + Unique Effects

> **Spec:** `.specs/a_implementar/fable/fable_31_spec_magic_items_unidentified_runtime.md`
> **Date:** 2026-06-19
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Wave/Batch:** FABLE Batch 4 (E20)
> **validated_adrs:** [ADR-0005]
> **validated_game_rules:** [cave_rules.md]

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`. The full runtime+data core of the spec is implemented, compiles
clean (Assembly-CSharp 0E/0W, Assembly-CSharp-Editor 0E/3W pre-existing), and is covered by 26
EditMode tests authored under `Assets/_Game/Tests/EditMode/Items/`. Two honest deferrals justify the
`_WITH_WARNINGS` suffix (not `BUILD_VALIDATED`):

1. **Play Mode / Unity Test Runner NOT RUN** — by explicit owner directive for this batch (no Unity
   Editor / Play Mode this session). EditMode tests are authored and compile-validated via the
   `dotnet` fallback; the NUnit runner result is deferred to the wave-end checkpoint.
2. **Named hooks for systems that do not exist yet.** The Phase 0 audit confirmed there is **no**
   light system, **no** enemy-HP HUD toggle, and **no** ambush/mimic reveal system in the codebase.
   The four passive flags (pendant/lantern/pouch/candle) and the use-effect side-effects
   (mirror teleport, bell ward, hourglass dawn, whetstone repair) are therefore published/exposed as
   **named flags/events** for the future consumers (HUD, fable_24, the light system, the cave runtime,
   the time system). This is exactly the spec's "hooks nomeados nos sistemas existentes" intent; where
   the target system is absent, the hook is a named flag/event rather than a direct call. The pure
   decision logic behind every effect is fully implemented and tested.

No premature acceptance is claimed. Phase 2-3 (Unity/Play Mode) is `NOT RUN` (deferred).

---

## Acceptance criteria extracted

| ID | Criterion | Status | Evidence |
|----|-----------|--------|----------|
| CA-1 | Rare chest (level 10+) drops unidentified; tooltip shows "???"; identify swaps 1:1 to real item | IMPLEMENTED (logic + tests) | `MagicItemLootWeighting` (level≥10 gate, deterministic), `ItemIdentificationService.IdentifyItem` (1:1 swap), unidentified DisplayName already "???" in fable_32 catalog. Tests: `IdentifyItem_SwapsOneForOne_*`, `LootWeight_*` |
| CA-2 | Pouch of Holding: +6 on gain, −6 on loss; with full inventory on loss, refuse drop safely (overflow → "mochila transbordando", nothing destroyed) | IMPLEMENTED (logic + tests) | `MagicItemPassiveState` (+6/−6), `PouchCapacityGuard.EvaluateDropPouch` (overflow block). Tests: `Pouch_GrantsSixSlots_*`, `Pouch_DropWithOverflow_IsBlocked_*` |
| CA-3 | Mirror of Return teleports to entrance of the SAME level — no regen of layout/enemies/resources (stable-run) | IMPLEMENTED (guard + tests) | `MirrorOfReturnDecision.Evaluate` (rejects any run-seed/level change), `MagicItemUseHandler.TryRequestMirrorReturn` reads `CaveRunManager` and never calls `GenerateNewRunSeed`. Tests: `Mirror_SameRunAndLevel_*`, `Mirror_ChangedSeed/Level_IsRejected` |
| CA-4 | Veska's unidentified offer is deterministic per week (same week = same offer; next week rotates) | IMPLEMENTED (logic + tests) | `VeskaWeeklyRotationService` (week = (day-1)/7, seeded token). Tests: `Veska_SameWeek_*`, `Veska_NextWeek_RotationChanges` |

---

## Existing systems audit

(Phase 0 — system-reuse audit; nothing parallel was created.)

| System | Found | Reused / Decision |
|--------|-------|-------------------|
| `ItemDataSO` / `ItemDatabaseSO` | `Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs`, `Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs` | **Reused.** Added 3 additive fields only (`IsUnidentified`, `IdentifiedItemId`, `PassiveFlag`); no existing field changed. |
| Magic item IDs + unidentified + scroll | fable_32 `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` (`AddCaveMagicItems`) | **Reused.** Runtime `MagicItemCatalog` mirrors the exact ids (`item_magic_*`, `item_unidentified_trinket`, `item_consumable_scroll_identify`). No new SKUs. |
| Consumable use pipeline (F08) | `Assets/_Game/Scripts/Inventory/ItemUseManager.cs` (`RegisterHandler` + `ItemUseHandler`) | **Reused.** Magic use items + scroll register handlers; no parallel consumption flow. |
| Inventory (stacks/slots) | `Assets/_Game/Scripts/Inventory/InventoryManager.cs` | **Reused via adapter** (`InventorySwapAdapter`). Identification = `RemoveItem`+`AddItem` (1:1). No per-instance metadata, no inventory-core edit. |
| Cave run + stable-run | `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` (`CurrentCaveLevel`, `CaveRunSeed`, `State`) | **Read-only.** Mirror reads run seed/level; never calls `GenerateNewRunSeed`. The existing stable-revisit path is `CaveLevelRuntimeController.RestoreFromSnapshot` + `CaveSpawnAnchor.Entrance` (documented hook target). |
| Calendar/time | `Assets/_Game/Scripts/Core/Time/TimeManager.cs` (`CurrentDay`) | **Reused.** Week = (CurrentDay-1)/7. Hourglass publishes a named dawn-request (TimeManager owns the phase clock; no parallel time system). |
| Loot resolver (F06) | `Assets/_Game/Scripts/Loot/*` (SO-based + designed `LootTableResolver`) | **Not duplicated.** Exposed a pure deterministic weighting decision (`MagicItemLootWeighting`) the cave loot path consults; no second resolver. |
| Tooltip | `Assets/_Game/Scripts/UI/Shop/ItemDisplayNameFormatter.cs` | **Reused as-is.** Reads `ItemDataSO.DisplayName`; the unidentified item's catalog DisplayName is already "???" — flows through with no UI edit. |
| Light system / enemy-HP HUD / ambush reveal | searched — **none exist** | Exposed **named flags** for future consumers (pendant→`magic_flag_show_enemy_health`, lantern→`magic_flag_reveal_ambush`, candle→`magic_flag_persistent_light`). No parallel system. |
| Repair | `Assets/_Game/Scripts/Equipment/EquipmentManager.cs` (`RepairItem`) | **Hook (named event).** Whetstone publishes `FreeRepairRequestedEvent`; daily gate enforced upstream by `MagicItemUseService`. |

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|--------------------|----------------|--------|
| Par de itens (unidentified ↔ real), swap 1:1, zero instance metadata | `ItemIdentificationService` + `InventorySwapAdapter` (RemoveItem+AddItem) | OK |
| `ItemDataSO` additive fields `isUnidentified`/`identifiedItemId`/`passiveFlag` | `ItemDataSO.IsUnidentified` / `IdentifiedItemId` / `PassiveFlag` | OK |
| `ItemIdentifiedEvent` (payload: revealed itemId) | `Core/Events/ItemIdentifiedEvent.cs` | OK |
| `IdentifyItem(stack)` consumed by F25 (Veska) and scroll_identify | `ItemIdentificationService.IdentifyItem` + `ScrollIdentifyUseHandler` | OK |
| 8 magic items with real effects (4 use + 4 passive) | `MagicItemUseService` (bell/mirror/hourglass/whetstone) + `MagicItemPassiveState` (pendant/lantern/pouch/candle) | OK (passive side-effects via named flags where target system absent) |
| `ItemPassiveTracker` — presence → named flag, single point per effect | `MagicItemPassiveState` (pure) + `Items/Runtime/ItemPassiveTracker.cs` | OK |
| Use effects routed through F08 (no second consume flow) | `MagicItemUseHandler` / `ScrollIdentifyUseHandler` registered on `ItemUseManager` | OK |
| Pouch +6 / overflow-safe (drop block, nothing destroyed) | `PouchCapacityGuard` | OK |
| Mirror respects stable-run (no reroll) | `MirrorOfReturnDecision` + read-only `CaveRunManager` use | OK |
| Rare-chest weight at level 10+ (F06), deterministic by seed | `MagicItemLootWeighting` (FNV-1a, level gate) | OK |
| Veska weekly deterministic rotation (seed per week) | `VeskaWeeklyRotationService` | OK |
| No new save schema; no migration; no Unity refs in DTOs | No save DTO touched; all derived from inventory/calendar/run seed | OK |
| No `GameObject.Find`/`FindObjectOfType` in runtime | Singletons (`ItemUseManager.Instance`, `CaveRunManager.Instance`, `GameBootstrap.Instance`) + injection (`Bind`/`Configure`) | OK |
| Gameplay comms via `GameEventBus` | `ItemIdentifiedEvent`, `MirrorReturnRequestedEvent`, `EnemyWardRequestedEvent`, `AdvanceToDawnRequestedEvent`, `FreeRepairRequestedEvent`; subscribes to `InventoryChangedEvent` | OK |

---

## Determinism notes (cave stable-run / ADR-0005, cave_rules.md)

- **Which real item a found trinket reveals** is `MagicItemCatalog.ResolveRevealedItemId(caveRunSeed, caveLevel, sourceSalt)` — an FNV-1a `StableHash` over run seed + level + per-source salt. Same drop in the same run always reveals the same item. The explicit per-item pair (`ItemDataSO.IdentifiedItemId`) takes precedence when set; the deterministic reveal is the fallback for the generic trinket.
- **Rare-chest drop** (`MagicItemLootWeighting.RollUnidentifiedTrinket`) is gated to `caveLevel >= 10` and rolled from the same `StableHash(runSeed|level|chestId|...)` — the same chest in the same run is deterministic; below level 10 the chance is always 0.
- **Mirror of Return** never changes `CaveRunSeed` nor the current `CaveLevel`. `MirrorOfReturnDecision.Evaluate` rejects any teleport where either changed; the handler reads `CaveRunManager` and never invokes `GenerateNewRunSeed` (the sole regeneration gate, used only on new game/defeat/debug). Stable-run snapshot is untouched.
- **Veska rotation** is `(CurrentDay-1)/7` → week, then `StableHash(salt|week)` — stable within a week, rotates across weeks, recomputed from the calendar (no drift on reload).
- No `System.Random` without seed, no `Guid`/timestamp, no `string.GetHashCode()` used for stable-run content.

## Save expectations (zero schema)

- **No new save section, no migration, no DTO change.** Identified vs. unidentified items are ordinary
  stacks keyed by id (the pair pattern eliminates instance metadata).
- **Whetstone daily cooldown** is derived from `TimeManager.CurrentDay` (the last-used day is held in
  the runtime service; the gate compares against the current calendar day — no persisted field). After a
  reload the worst case is that the free repair is available again on the loaded day, which is acceptable
  and documented; no item or state is corrupted.
- **Veska weekly offer** is derived from the current week — not stored.
- **No Unity object reference** appears in any data structure introduced by this spec.

---

## Validation

| Check | Result |
|-------|--------|
| Validation method | `run_strict_validation.ps1` |
| Assembly-CSharp | PASS (exit 0, 0E/0W) |
| Assembly-CSharp-Editor | PASS (exit 0, 0E/3W pre-existing) |
| `validate_docs.ps1` | PASS (see strict result) |
| `check_spec_diff_completeness.ps1` | PASS |
| `run_strict_validation.ps1` | exit 0 — `STRICT_VALIDATION_RESULT: VALIDATION_PASS` |
| Result artifact | inline console result (this harness version prints `STRICT_VALIDATION_RESULT: VALIDATION_PASS`; no JSON file emitted) |
| Spec quality check | PASS |

### Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (identification swap, reveal, loot weight, pouch overflow, Veska rotation, whetstone gate)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Items/MagicItemsTests.cs — 26 tests)
Automated tests command: tools/unity/RunUnityEditModeTests.ps1 — NOT RUN (Unity batchmode disabled this session by owner directive); dotnet compile fallback PASS (0E)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (per spec: Human validation timing = DEFERRED_TO_FINAL_VALIDATION)
Justification if no automated tests: N/A (automated EditMode tests authored; runner deferred — not a missing-test case)
Residual risk: (a) EditMode NUnit runner not executed this session — covered logic is pure and compiles; (b) passive side-effects (HUD HP / ambush reveal / light) and mirror teleport / dawn skip / repair are named flags/events pending their consumer systems (light/HUD/ambush systems do not yet exist; cave teleport + time skip + repair to be wired by their owning systems). The pure decisions are tested.
```

---

## Files changed

### Created — runtime (Assembly-CSharp)
- `Assets/_Game/Scripts/Items/MagicItemCatalog.cs` — runtime source of truth (ids, passive flags, deterministic reveal, FNV-1a StableHash).
- `Assets/_Game/Scripts/Items/ItemIdentificationService.cs` — 1:1 identification swap + `IItemSwapInventory` + result types.
- `Assets/_Game/Scripts/Items/MagicItemPassiveState.cs` — pure passive-flag + slot-bonus projection.
- `Assets/_Game/Scripts/Items/PouchCapacityGuard.cs` — pure overflow-safe drop decision (CA-2).
- `Assets/_Game/Scripts/Items/MagicItemUseService.cs` — pure use-effect resolution + whetstone daily gate.
- `Assets/_Game/Scripts/Items/MirrorOfReturnDecision.cs` — pure stable-run guard (CA-3).
- `Assets/_Game/Scripts/Items/VeskaWeeklyRotationService.cs` — deterministic weekly rotation (CA-4).
- `Assets/_Game/Scripts/Items/MagicItemLootWeighting.cs` — deterministic level≥10 loot weighting (CA-1).
- `Assets/_Game/Scripts/Items/Runtime/InventorySwapAdapter.cs` — InventoryManager → `IItemSwapInventory`.
- `Assets/_Game/Scripts/Items/Runtime/ItemPassiveTracker.cs` — observes inventory, keeps flags current.
- `Assets/_Game/Scripts/Items/Runtime/RuntimeMagicTimeGateway.cs` — time gateway over TimeManager.
- `Assets/_Game/Scripts/Items/Runtime/MagicItemUseHandler.cs` — F08 handler for the 4 use items.
- `Assets/_Game/Scripts/Items/Runtime/ScrollIdentifyUseHandler.cs` — F08 handler for scroll_identify.
- `Assets/_Game/Scripts/Items/Runtime/MagicItemRuntimeBootstrap.cs` — self-wiring (RuntimeInitializeOnLoad).
- `Assets/_Game/Scripts/Core/Events/ItemIdentifiedEvent.cs` — new event.
- `Assets/_Game/Scripts/Core/Events/MagicItemEvents.cs` — mirror/ward/dawn/repair named hook events.

### Modified
- `Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs` — 3 additive fields (`IsUnidentified`, `IdentifiedItemId`, `PassiveFlag`).
- `Assembly-CSharp.csproj` — Compile includes for the 16 new runtime/event files + the test file.

### Created — tests (Assembly-CSharp)
- `Assets/_Game/Tests/EditMode/Items/MagicItemsTests.cs` — 26 EditMode tests (identification, fallback, reveal, loot weight, passive flags, pouch overflow, mirror stable-run, Veska rotation, use effects + whetstone gate, taxonomy).

---

## Remaining work (deferred, named)

- **Unity Test Runner** for the 26 EditMode tests (wave-end checkpoint).
- **Consumer wiring of named hooks** (separate specs / their owning systems):
  - `MirrorReturnRequestedEvent` → cave runtime: `CaveLevelRuntimeController.RestoreFromSnapshot` at `CaveSpawnAnchor.Entrance` (stable revisit; no regen).
  - `magic_flag_show_enemy_health` (pendant) → enemy-HP HUD (system not yet present).
  - `magic_flag_reveal_ambush` (lantern) → fable_24 ambush/mimic reveal.
  - `magic_flag_persistent_light` + `CandleLightRadius` (candle) → cave light system (not yet present).
  - `magic_flag_inventory_slot_bonus` / `ItemPassiveTracker.SlotBonus` (pouch +6) → inventory capacity surface (InventoryManager core is locked out of this spec; tracker exposes the bonus + the overflow-safe guard is implemented and tested).
  - `EnemyWardRequestedEvent` (bell) → enemy AI flee.
  - `AdvanceToDawnRequestedEvent` (hourglass) → TimeManager/GameTimeManager phase clock.
  - `FreeRepairRequestedEvent` (whetstone) → `EquipmentManager.RepairItem` (daily gate already enforced here).
- **Asset materialization** of the 8 magic items + trinket + scroll as `ItemDataSO` assets is owned by the fable_32 generator (this spec consumes the ids; per spec, asset generation is out of scope here).
- **F32/F25 integration**: F25 (Veska) calls `ItemIdentificationService.IdentifyItem` for the 120g service; F06 loot path consults `MagicItemLootWeighting`.

---

## Dependency Chain

```
Original target: fable_31
Dependency chain: F32 (catalog ids — DONE, commit f5c37c1c) ; F08 (consumable use — DONE) ; F06 (loot resolver — present)
Forbidden dependencies: none
Resolved depth: 0 (all dependencies already satisfied in the codebase)
Can continue original target: YES
```
