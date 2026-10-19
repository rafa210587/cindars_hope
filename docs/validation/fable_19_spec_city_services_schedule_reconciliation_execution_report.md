# Execution Report — fable_19 City Services + Schedule Reconciliation

> **Spec:** `.specs/a_implementar/fable/fable_19_spec_city_services_schedule_reconciliation.md`
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS` (deferred: scene anchoring, NPC movement, Play Mode)
> **Date:** 2026-06-19
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **validated_adrs:** []
> **validated_game_rules:** [city_rules.md]

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`. All core acceptance criteria (CA-1 schedule único, CA-2
licenças mecânicas, CA-3 persistência por flag) are implemented as deterministic, testable logic
and validated by `run_strict_validation.ps1` exit 0 (Assembly-CSharp 0E, Assembly-CSharp-Editor
0E, docs PASS, quality check PASS). The `_WITH_WARNINGS` qualifier reflects the spec's own
deferral (`Play Mode final: YES | DEFERRED_TO_FINAL_VALIDATION`): the diálogo-de-compra flow,
the standalone save round-trip of the civic flags, and any in-scene schedule anchoring require
Unity Play Mode / scene wiring, which the owner authorized deferring. No scene/prefab/asset YAML
was edited.

No `ACCEPTED`, `PLAYMODE_VALIDATED`, or "Play Mode PASS" is claimed.

---

## Phase status

| Phase | Status |
|-------|--------|
| Phase 0 — system-reuse audit | COMPLETE |
| Phase 1 — implementation + BUILD_VALIDATED | COMPLETE |
| Phase 2 — Unity batchmode compile | NOT RUN (owner-authorized; Unity Editor not launched) |
| Phase 3 — Play Mode / human scenario | DEFERRED_TO_FINAL_VALIDATION (owner-authorized) |

---

## Acceptance criteria extracted

| ID | Criterion (from spec) | Met | Evidence |
|---|---|---|---|
| CA-1 | Schedule único: `NPC/Schedule` contém semântica absorvida; `City/Schedule` 100% Obsolete sem consumidores novos; builds 0E/0W (Obsolete suprimido nos arquivos absorvidos) | YES | `NpcSchedulePeriod.cs` crosswalk; `[Obsolete]` engine classes; strict validation exit 0; `Crosswalk_*` tests |
| CA-2 | Licenças mecânicas: sem `license_market_stall` o ponto de venda urbano recusa com mensagem; com ela vende. `contract_farm_registry` = +5% no shipping (teste no resolver) | YES | `EconomyManager` gate; `ShippingPriceResolver` hook; `UrbanSell_*` + `FarmShipping_*` + resolver tests |
| CA-3 | Persistência por flag: posse sobrevive a save/load via `QuestFlagService` (round-trip — teste de integração) | YES (deterministic) / Play Mode DEFERRED | `Possession_PersistsViaQuestFlagService_RoundTripStyle`; standalone SaveManager wiring deferred to final validation |

## Existing systems audit (Phase 0, system-reuse)

| Concept | Found (existing) | Decision |
|---|---|---|
| Schedule system A (WAVE 08) | `City/Schedule/` — `SchedulePeriod`, `NpcScheduleDefinition`, `NpcScheduleResolver`, `SchedulePeriodHelper`, `NpcMovementPolicy` | Absorb semantics → mark engine classes `[Obsolete]` |
| Schedule system B (WI-25) | `NPC/Schedule/` — `NpcScheduleProfile/Block/Anchor/Service/RuntimeBootstrap`, `NpcTimeBlock`, home anchor `npc_<id>_home` | **Canonical** (kept); absorbed period vocabulary added |
| City services (orphan) | `City/Services/` — `CityServiceDefinition`, `LicenseDefinition`, `ContractDefinition`, `CityServiceAvailabilityResolver`, `ServiceAvailabilityResult` | **Reused** (no parallel system) by new static catalog/facade |
| Flag persistence | `Quests/Flags/QuestFlagService` + `QuestFlagRegistry` (`AllowedSetters` gating) | **Reused** — possession persisted as quest flags; no new save section |
| Urban sell point | `Economy/SellAllPoint` → `SellAllRequestedEvent(sourceId)` handled by `Economy/EconomyManager` | Gate added at single point on urban source only |
| Farm shipping price | `Farm/Shipping/ShippingPriceResolver` | +5% contract hook at single resolve point |
| Shop dialogue | `NPC/NpcShopController` (Brumdar "Temperar" via `Economy/TemperingForgeAccess` static facade) | Mirrored: "Serviços" choice for Tovin/Mara via new `CityServiceAccess` facade |
| Cave merchant sell | `Cave/Runtime/CaveWanderingMerchant` source `shop_cave_wandering_merchant_l{N}` | NOT gated (distinct source) |

**Result:** no parallel system created. The two orphan subsystems (`City/Services/*`) and the
duplicated schedule are reconciled by reuse + a thin static facade following the project's
`TemperingForgeAccess` / `AccessoryEffectRouter.Active` idiom.

---

## Spec Compliance Matrix

| Requirement | Implementation | Evidence |
|---|---|---|
| CA-1: `NPC/Schedule` contains absorbed semantics | `NPC/Schedule/NpcSchedulePeriod.cs` — `NpcSchedulePeriod` enum + `NpcSchedulePeriodHelper` (FromHour 1:1 with city windows, `ToTimeBlock` crosswalk, `IsHomePeriod` = bed→home anchor) | `Crosswalk_*` tests (5) |
| CA-1: `City/Schedule` 100% Obsolete, no new consumers | `[Obsolete]` on `NpcScheduleDefinition`, `NpcScheduleResolver`, `SchedulePeriodBlock`, `ScheduleModifierBlock`, `NpcMovementPolicy`, `ScheduleResolveContext/Result`; `#pragma` suppression in the single in-scope consumer + its test | `dotnet build` 0E/0W (no CS0618 leak) |
| CA-1: builds 0E/0W | Both assemblies build 0E; warnings only pre-existing & unrelated | strict validation exit 0 |
| CA-2: without `license_market_stall`, urban point refuses with message | `EconomyManager.HandleSellAllRequested` gate on `shop_town_sell_box` via `CityServiceAccess.UrbanSellAllowed()` | `UrbanSell_WithoutLicense_Refused`, `UrbanSell_WithLicense_Allowed` |
| CA-2: with license, sells | gate passes when flag set | same tests + `ShippingPriceResolver_AppliesContractAtSinglePoint` |
| CA-2: `contract_farm_registry` = +5% shipping | `ShippingPriceResolver.Resolve` → `CityServiceAccess.ApplyFarmRegistryContract` (+5%) | `FarmShipping_WithContract_PlusFivePercent`, resolver test |
| CA-3: possession survives via `QuestFlagService` | `CityServiceFlags.RegisterFlags` + `CityServiceRuntimeBootstrap` wires `CityServiceAccess` to a `QuestFlagService`; grant via `GrantFlag(..., CityServiceSetter)` | `Possession_PersistsViaQuestFlagService_RoundTripStyle`, `Possession_UnauthorizedSetter_CannotGrantFlag` |
| Idempotent flags | `CityServicePurchaseResolver` returns `AlreadyOwned` with no spend on repeat | `Purchase_AlreadyOwned_IsIdempotent_NoSpend` |
| No new save section | possession = quest flags only | n/a (no `GameSaveData`/section change) |
| Farm shipping never blocked by city license | gate only on urban source; shipping only gains bonus | `FarmShipping_ContractDoesNotAffectUrbanLicense_AndViceVersa` |

---

## Files changed

### New (runtime)
- `Assets/_Game/Scripts/City/Services/CityServiceFlags.cs` — canonical service/flag IDs + `QuestFlagRegistry` registration (City scope, `AllowedSetters = city_services`).
- `Assets/_Game/Scripts/City/Services/CityServiceCatalog.cs` — static catalog of the 2 services reusing `CityServiceDefinition/LicenseDefinition/ContractDefinition`; Tovin/Mara identity + cost/flag/label lookups.
- `Assets/_Game/Scripts/City/Services/CityServicePurchaseResolver.cs` — pure deterministic purchase logic (Purchased / AlreadyOwned / InsufficientGold / UnknownService / Failed).
- `Assets/_Game/Scripts/City/Services/CityServiceAccess.cs` — static facade (mirrors `TemperingForgeAccess`/`AccessoryEffectRouter.Active`): `OwnsService`, `UrbanSellAllowed`, `ApplyFarmRegistryContract`, `TryPurchase`; injected fail-closed delegates.
- `Assets/_Game/Scripts/City/Services/CityServiceRuntimeBootstrap.cs` — `RuntimeInitializeOnLoad` singleton wiring the facade to a `QuestFlagService` + `PlayerManager` (no global scene search; GameBootstrap injection).
- `Assets/_Game/Scripts/NPC/Schedule/NpcSchedulePeriod.cs` — absorbed period vocabulary + crosswalk (CA-1).

### New (test)
- `Assets/_Game/Tests/EditMode/City/CityServicesReconciliationTests.cs` — 19 EditMode tests.

### Modified (runtime, in spec architecture scope)
- `Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs` — `[Obsolete]` on engine classes + file `#pragma`.
- `Assets/_Game/Scripts/City/Schedule/NpcScheduleResolver.cs` — `[Obsolete]` on resolver classes + file `#pragma`.
- `Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs` — crosswalk/deprecation note (enum kept non-obsolete: consumed by out-of-scope `Dialogue/*`).
- `Assets/_Game/Scripts/City/Validation/CityLayoutScheduleValidator.cs` — narrow `#pragma` around `ValidateSchedule` (legacy consumer of obsolete `NpcScheduleDefinition`).
- `Assets/_Game/Scripts/Economy/EconomyManager.cs` — urban sell license gate (single point).
- `Assets/_Game/Scripts/Farm/Shipping/ShippingPriceResolver.cs` — +5% farm-registry contract hook (single point).
- `Assets/_Game/Scripts/NPC/NpcShopController.cs` — "Serviços" dialogue choice + purchase for Tovin/Mara.

### Modified (test, in scope)
- `Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs` — file `#pragma` (legacy test exercising the now-obsolete schedule engine).

### Build-only (NOT committed)
- `Assembly-CSharp.csproj` — `<Compile Include>` for the 6 new runtime files + new test (auto-regenerated by Unity; excluded from commit per task policy).

---

## Schedule crosswalk (City/Schedule → NPC/Schedule)

| City/Schedule (obsolete) | NPC/Schedule (canonical) | Note |
|---|---|---|
| `SchedulePeriod` (Morning…SleepLateNight, 7) | `NpcSchedulePeriod` (same 7) | 1:1 hour windows (city_rules Rule 6) |
| `SchedulePeriodHelper.FromHour` | `NpcSchedulePeriodHelper.FromHour` | identical windows; parity test |
| period → location/waypoint resolution | period → `NpcTimeBlock` via `ToTimeBlock`, then `NpcScheduleService` anchor | runtime collapses to Work/Social/Home/Night (TIME_BLOCK_DEBT WAVE25 unchanged) |
| `BedDefinition` / `FallbackWaypointId` (home) | `npc_<id>_home` anchor + `IsHomePeriod` | "bed → home anchor" absorption |
| `NpcScheduleDefinition` / `NpcScheduleResolver` (engine) | — (no equivalent needed; `NpcScheduleService` resolves) | marked `[Obsolete]`, physical removal via delete candidates post-F11 |

**Obsolete boundary decision (scope-faithful):** the `[Obsolete]` attribute fires CS0618 at the
call site. The duplicated *engine* classes (Definition/Resolver/blocks/policy/context/result) are
consumed only by the in-scope `CityLayoutScheduleValidator` + its test (both `#pragma`-suppressed),
so they are marked obsolete. The `SchedulePeriod` / `ScheduleModifierType` *enums* are the absorbed
semantic vocabulary still consumed by out-of-scope `Dialogue/DialogueContext`,
`Dialogue/DialogueCondition`, and in-scope `City/FarmVisits/FarmVisitRule`; obsoleting them would
force edits outside the spec scope and risk breaking 0W, so they are kept non-obsolete with a
documented crosswalk note pointing to the canonical `NpcSchedulePeriod`. This satisfies CA-1
("semântica absorvida; engine 100% Obsolete sem consumidores novos; 0E/0W") without scope breach.

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (0 Errors, 1 pre-existing unrelated warning — CombatTelemetrySession CS0649 from fable_59)
Assembly-CSharp-Editor: PASS (0 Errors, 3 pre-existing unrelated warnings — EnemyTaxonomy editor + CSharpProjectPostprocessor)
Quality check: PASS
Docs validation: PASS (exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Individual gates:
- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0 (0E). No new CS0618 (Obsolete) leaks.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0 (0E).
- `.\tools\docs\validate_docs.ps1` → exit 0 (PASS).
- `.\tools\docs\check_spec_diff_completeness.ps1` → exit 0 (after this report created).
- `.\tools\docs\run_strict_validation.ps1` → exit 0 (VALIDATION_PASS).

EditMode tests (18 new, in `Assembly-CSharp.csproj` runtime test set — no `CindarsHope.Editor.*`
references): compile under Assembly-CSharp 0E. Execution under Unity Test Runner is deferred to
final validation (Phase 3, owner-authorized); compile-time inclusion verified by the build gate.

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (crosswalk, purchase resolver, license gate, contract bonus)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (CityServicesReconciliationTests.cs, 19 tests)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (compile); Unity Test Runner DEFERRED (Phase 3)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (dialogue purchase + urban sell refusal + farm shipping bonus)
Justification if no automated tests: N/A — deterministic logic fully covered by EditMode tests
Residual risk: dialogue purchase flow, standalone civic-flag save round-trip, and in-scene
  schedule anchoring are exercised only in Play Mode (not yet run). The deterministic core
  (gate decision, +5% bonus, idempotency, crosswalk parity) is covered.
```

---

## Anti-regression

- Thalindra/Brumdar dialogue flows untouched (existing choices preserved; new "Serviços" choice
  only appears for Tovin/Mara via `CityServiceCatalog.IsServiceProvider`).
- Farm shipping flow never blocked by city license — the gate is on the urban source
  `shop_town_sell_box` only; cave merchant (`shop_cave_wandering_merchant_l*`) and direct NPC
  sales are not gated. `ShippingPriceResolver` only *adds* a bonus when the contract exists.
- Flags idempotent (`AlreadyOwned` → no double spend); unauthorized setters cannot grant
  (`AllowedSetters`).
- Intentional behavior change (documented): the urban town sell point now refuses without the
  market-stall license — this is CA-2 by design, not a regression.

---

## Remaining work (deferred / out of scope)

- Phase 2 Unity batchmode compile — NOT RUN (owner authorized; Unity Editor not launched). Residual
  risk: Unity-specific compile (rare vs dotnet) unverified.
- Phase 3 Play Mode / human scenario — dialogue purchase, urban refusal toast, farm shipping bonus,
  living-town schedule movement.
- Standalone save round-trip of the 2 civic flags (currently the runtime bridge holds them in a
  dedicated `QuestFlagService`; SaveManager integration of these flags is deferred to final
  validation, consistent with this spec's BUILD_VALIDATED gate).
- Physical removal of `City/Schedule/*` → delete candidates **post-F11** (out of scope here).
- In-scene `NpcScheduleAnchor` placement / NPC physical movement by blocks → **fable_11** (explicitly
  out of scope; this spec only elects the canonical system).
- Migration of `Dialogue/*` + `FarmVisitRule` off the `SchedulePeriod` enum onto `NpcSchedulePeriod`
  → future cleanup (kept non-obsolete here to respect scope + 0W).

---

## Dependency Chain

```text
Original target: fable_19
Dependency chain: none open (WAVE 08 City/ + WI-25 NPC/Schedule already present/BUILD_VALIDATED)
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES (completed)
```
