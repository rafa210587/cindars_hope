# Execution Report — fable_60 Cave Traps by Biome/Tier (subset v1)

> **Spec:** `.specs/a_implementar/fable/fable_60_spec_cave_traps_by_biome_tier.md`
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Date:** 2026-06-19
> **Branch:** dev
> **Wave:** FABLE Batch 10
> **Validation method:** run_strict_validation.ps1 (exit 0)

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md, combat_rules.md, event_rules.md]

---

## Honest status rationale

Status: **BUILD_VALIDATED_WITH_WARNINGS**.

- All six acceptance criteria implemented against existing systems; nucleus is deterministic and
  covered by EditMode tests.
- Phase 2-3 (Unity batchmode / Play Mode) is **NOT RUN** — the owner explicitly deferred human/Play
  Mode validation for this batch (DEFERRED_TO_FINAL_VALIDATION per the spec). The runtime reveal of
  detection (TrapDetectionRuntime polling) and the in-scene telegraph/step/disarm/false-chest feel
  require Play Mode and are deferred.
- `_WITH_WARNINGS` reflects the deferred Play Mode surface, not a code defect: both assemblies build
  with **0 errors** (1 pre-existing runtime warning unrelated to this spec; 3 pre-existing editor
  warnings).

Not claimed: PLAYMODE_VALIDATED, ACCEPTED, Phase 2-3 PASS. Spec is NOT promoted to `implementados/`.

---

## Acceptance criteria extracted

| CA | Requirement | Implementation | Evidence |
|----|-------------|----------------|----------|
| CA-1 | Geração determinística por bioma/tier (mesmo seed = mesmo plano; faixas §24/§25; pool §26; fora do caminho crítico) | `CaveTrapPlanner.BuildPlan` (pure, StableHash, BFS path exclusion reused from fable_09) | `CaveTrapsTests`: `CA1_TrapPlan_IsDeterministicForSameSeed`, `CA1_TrapTypes_AlwaysFromBiomePool`, `CA1_TrapPositions_NeverOnCriticalPathOrAnchors`, `CA1_TrapCount_WithinSizeAndTierRanges`, `CA1_DifferentLevels_ProduceDistinctPlans`, `CA1_TrapInstanceIds_AreStableAndContainNoGuid`, `CA1_DifferentRunSeed_DivergesPlanOnAverage` |
| CA-2 | Telegraph + efeito com counterplay (telegraph > 0; dano pelo pipeline; status F01; sem dano inevitável) | `TrapBehaviour` state machine (Armed→Telegraphing→Triggered) + `TrapDefinition` (telegraph per type) | `Catalog_EveryTrapHasTelegraphGreaterThanZero`, `CA2_DamageScalesWithBand_AndIsZeroForPureStatus`, `Catalog_StatusTrapsMapToCanonicalF01Ids` |
| CA-3 | Desarme por ferramenta (chance por tier, determinística; sucesso = Disarmed; falha = ativa) | `TrapDisarmResolver` (pure, deterministic by attempt seed) + `TrapBehaviour.Interact` (IInteractable) | `CA3_DisarmChance_IncreasesWithToolTier`, `CA3_Disarm_IsDeterministicPerAttemptSeed`, `CA3_Disarm_NoneTierNeverSucceeds`, `CA3_Disarm_RollBelowChanceSucceeds_RollAtOrAboveFails` |
| CA-4 | Baú falso spawna Hoardmaw 1× pelo caminho existente e remove o falso baú | `FalseChestTrap` (IInteractable, spawn callback into existing `CreateEnemyRuntimeObject`) | `CA4_FalseChest_SpawnsHoardmawExactlyOnce` (spawn 1×, reentrada não duplica) |
| CA-5 | Revisita estável (snapshot; Triggered/Disarmed não rearmam) | Campos aditivos `VisitedLevelSnapshot.TrapStates` + `SetTrapState`/`GetTrapState`; materializer restaura | `CA5_SnapshotTrapState_RoundTripsAndOnlyAdvances`, `CA5_RevisitPlan_IsIdenticalForSameRun`, `CA5_LegacySnapshotWithoutTraps_DefaultsToArmed` |
| CA-6 | Detecção do amuleto de Nyx (flag ON/OFF) | `TrapDetectionService.ResolveDetectedTraps` (pure) + `TrapDetectionRuntime` (consumer) + `AccessoryEffectType.TrapDetectionRadiusFlat` (F23 flag) | `CA6_Detection_OnlyRevealsWithinRadius_WhenFlagOn`, `CA6_Detection_SkipsAlreadyResolvedTraps`, `CA6_DetectionService_IsInactiveByDefault` |

---

## Existing systems audit

Critical reuse: **fable_09 already shipped a hazard system** (`CaveHazardPlanner`, `CaveHazardKind`,
`CaveHazardTile`, `CaveHazardPlan` in Cave/Runtime + Cave/Generation). This spec **extends/reuses that
surface** rather than building a parallel trap system.

| System | Status | Reused / extended how |
|--------|--------|------------------------|
| `CaveHazardPlanner.ComputeEntranceExitPathWithBuffer` (BFS critical path) | EXISTING (fable_09) | **Reused as-is** by `CaveTrapPlanner` to exclude the entrance→exit path (no second BFS). |
| `CaveLayoutStableHash.Compute` (FNV-1a) | EXISTING (fable_09) | **Reused** for all trap RNG (positions, types, count, instance IDs, disarm rolls). Zero `UnityEngine.Random`/GUID/timestamp. |
| `CaveBiomeLayoutProfile.ForLevel` (band per level) | EXISTING (fable_09) | **Reused** as the single biome/band source (CA-1 / §26 pool keyed by `BandId`). No second biome source. |
| `CaveRuntimeMaterializer.MaterializeHazardsAndTreasure` | EXISTING (fable_09) | **Extended additively**: `MaterializeTraps` runs in the same place, same pattern (dedicated parent, trigger collider, placeholder visual, snapshot state). |
| `VisitedLevelSnapshot` / `CaveSnapshotService` | EXISTING | **Additive fields only** (`TrapStates`), mirroring the `OpenedChestIds`/`EnemyHpRecords` precedent (mutable state OUTSIDE LayoutHash). No migration. |
| Damage pipeline `PlayerDamageReceiver.ApplyDamage` | EXISTING | **Consumed** (no second damage path). |
| Status F01 `PlayerStatusReceiver.TryApplyFromEnemyAction` | EXISTING (fable_01) | **Consumed** for Poison/Slow/Stun/Chill/Burn/Root (all 6 assets confirmed). |
| Enemy spawn-by-id `CaveRuntimeMaterializer.CreateEnemyRuntimeObject` | EXISTING | **Reused** for the Hoardmaw false-chest spawn (no second spawner). |
| `IInteractable` (Interaction) | EXISTING | **Implemented** by `TrapBehaviour` (disarm) and `FalseChestTrap` (open). No parallel interaction system. |
| F23 dormant detection flag | PARTIAL (fable_23) | The Nyx detection flag did **not** exist as a concrete effect. Added `AccessoryEffectType.TrapDetectionRadiusFlat` (additive enum value) + `AccessoryEffectRouter.TrapDetectionRadiusSource` (single named read point, same idiom as the other hooks). `TrapDetectionService`/`TrapDetectionRuntime` are the real consumers. The flag's magnitude is still **0** until an accessory maps it (a future catalog change), so the effect remains inert in-game today — documented gap, no blocker. |
| `enemy_hoardmaw` | EXISTING (fable_33 bestiary, band 5 ruins) | Confirmed id `enemy_hoardmaw`; spawned by id. |
| `ToolTier` (Tools) | EXISTING | **Reused** by `TrapDisarmResolver` (None/Basic/Copper/Iron/Gold/Diamond) — no new tier concept. |

New code created (only what did not exist): `Cave/Traps/**` (TrapId/TrapDefinition catalog,
CaveTrapPlanner, CaveTrapPlan, TrapState, TrapBehaviour, FalseChestTrap, TrapDisarmResolver,
TrapDetectionService, TrapDetectionRuntime), `Core/Events/CaveTrapEvents.cs`, additive snapshot fields,
EditMode tests.

---

## Spec Compliance Matrix

| Spec requirement | Implementation | Status |
|------------------|----------------|--------|
| 10 canonical traps v1 (§23 subset) | `TrapDefinition.All` (10 entries) | OK |
| `TrapDefinition`: trapId, displayName, effect category, statusId (F01), damage per tier, telegraphSeconds, disarmable, biomes §26 | `TrapDefinition` | OK |
| `CaveTrapPlanner` pure deterministic: count §24/§25 clamped, pool §26, valid cells off critical path, stable IDs (no GUID) | `CaveTrapPlanner.BuildPlan` | OK |
| Materialization in `CaveRuntimeMaterializer` (same enemy/resource pattern) | `MaterializeTraps` | OK |
| `TrapBehaviour`: trigger→telegraph→damage (pipeline) + status (F01) | `TrapBehaviour` | OK |
| Disarm: IInteractable when detected/armed; chance per tool tier; success = Disarmed permanent; fail = trigger | `TrapBehaviour.Interact` + `TrapDisarmResolver` | OK |
| `trap_false_chest`: looks like chest; open = spawn `enemy_hoardmaw` via existing path; never disarmable; detectable | `FalseChestTrap` | OK |
| Detection (F23 consumer): radius reveals telegraph | `TrapDetectionService` + `TrapDetectionRuntime` + router flag | OK (flag magnitude 0 until catalog maps it — documented) |
| Snapshot additive fields {trapInstanceId, trapId, cell, state}; revisit restores; Triggered/Disarmed do not rearm | `VisitedLevelSnapshot.TrapStates` + materializer restore + exit/save refresh | OK |
| Events: TrapTriggeredEvent, TrapDisarmedEvent, TrapDetectedEvent (NEW) | `Core/Events/CaveTrapEvents.cs` | OK |
| EditMode tests (determinism, ranges, pool, positions, snapshot round-trip, disarm borders, false_chest 1×, stable IDs) | `CaveTrapsTests` (28 tests) | OK |
| Seed never changes on ForwardExit/BackExit | Not touched (LER seeds only) | OK |
| No `GameObject.Find`/`FindObjectOfType` at runtime; GameEventBus only | trigger-based + injected refs + bootstrap; events via `GameEventBus` | OK |

Out of scope (per spec, not implemented): the other ~10 §23 types (v2), scripted 100/101 traps,
pet/companion detection, permanent item loss, puzzle/alternate-route generation, final art.

---

## Cave stable-run clause (ADR-0005)

- Trap positions/types/quantity/instance IDs are **derived solely from**
  `StableHash(worldSeed|runSeed|level|"traps"|...)` — re-derivable, identical on revisit. Zero
  `UnityEngine.Random`, `Guid.NewGuid()`, or timestamp in stable content.
- Traps **never** land on the entrance/exit, the BFS entrance→exit critical path (+buffer, reused from
  fable_09), or within the player spawn radius (§22 — path never blocked).
- Mutable state (`Triggered`/`Disarmed`) persists in `VisitedLevelSnapshot.TrapStates` **outside** the
  LayoutHash (same treatment as `OpenedChestIds`/`EnemyHpRecords`). Revisit restores exactly and never
  re-arms a terminal trap within the same `CaveRunSeed`. Telegraphing (transient) collapses to Armed on
  revisit.
- Legacy snapshots without the field re-derive traps as `Armed` (deterministic plan guarantees identical
  composition) — no migration required.
- `ForwardExit`/`BackExit` never change `CaveRunSeed` (seed logic untouched — read-only).
- `CaveReplayValidator`/`CalculateLayoutHash` unaffected: trap state is outside the LayoutHash, so replay
  hash of pre-existing levels is unchanged (regression-safe).

---

## Validation

| Check | Result |
|-------|--------|
| `dotnet build Assembly-CSharp.csproj --no-restore` | PASS (exit 0; 0 errors, 1 pre-existing warning: `CombatTelemetrySession._blocks`, fable_59) |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | PASS (exit 0; 0 errors, 3 pre-existing warnings) |
| `.\tools\docs\validate_docs.ps1` | PASS (exit 0) |
| `.\tools\docs\check_spec_diff_completeness.ps1` | PASS (exit 0) |
| `.\tools\docs\run_strict_validation.ps1` | PASS (exit 0) — artifact `docs/validation/LAST_STRICT_VALIDATION_RESULT.json` |
| Result artifact | `docs/validation/LAST_STRICT_VALIDATION_RESULT.json` |

Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS
Assembly-CSharp-Editor: PASS
Quality check: PASS
Docs validation: PASS (EXPECTED_FAIL_LEGACY_ONLY not triggered — full PASS)

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (planner, disarm resolver, state machine, snapshot, detection)
Changed Unity scene/prefab/asset wiring: NO (runtime materialization only; no .unity/.prefab/.asset edits)
Automated tests added/updated: YES
Automated tests command: Unity EditMode (CaveTrapsTests, 28 tests) — compiled into Assembly-CSharp; runner NOT RUN (Unity deferred)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (step a trap, disarm, open a false chest → Hoardmaw, revisit level with state preserved)
Justification if no automated tests: N/A (tests added for all deterministic logic)
Residual risk: in-scene runtime wiring (collider triggers, TrapDetectionRuntime polling, false-chest spawn under generated root, telegraph/disarm feel) is unverified until Play Mode; deterministic core is test-covered.
```

EditMode tests compile in `Assembly-CSharp.csproj` (no `CindarsHope.Editor.*` types referenced — pure
planner/resolver/snapshot/detection tests, so they correctly stay in the runtime assembly, not the
editor assembly).

---

## Files changed

New (runtime — `Assets/_Game/Scripts/Cave/Traps/`):
- `TrapState.cs`
- `TrapDefinition.cs` (TrapId enum, TrapEffectCategory enum, 10-type catalog)
- `CaveTrapPlan.cs` (CaveTrapPlacement, CaveTrapPlan)
- `CaveTrapPlanner.cs` (pure deterministic planner)
- `TrapDisarmResolver.cs` (pure deterministic disarm by tier)
- `TrapBehaviour.cs` (state machine + IInteractable disarm)
- `FalseChestTrap.cs` (false chest → Hoardmaw)
- `TrapDetectionService.cs` (pure F23 consumer logic)
- `TrapDetectionRuntime.cs` (runtime detection consumer, no global search)

New (events): `Assets/_Game/Scripts/Core/Events/CaveTrapEvents.cs`

New (tests): `Assets/_Game/Tests/EditMode/Cave/CaveTrapsTests.cs`

Modified (additive):
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` (trap materialization + false-chest spawn callback)
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` (`TrapStates` field + `CaveTrapSnapshotEntry` + helpers)
- `Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs` (trapStates param in CaptureSnapshot)
- `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` (capture + `RefreshCurrentSnapshotTrapStates`)
- `Assets/_Game/Scripts/Cave/CaveExitPortal.cs` (refresh trap states on exit)
- `Assets/_Game/Scripts/Save/SaveManager.cs` (refresh chest + trap states before capture)
- `Assets/_Game/Scripts/Equipment/AccessoryEffectType.cs` (`TrapDetectionRadiusFlat` flag)
- `Assets/_Game/Scripts/Equipment/AccessoryEffectRouter.cs` (`TrapDetectionRadiusSource` hook)
- `Assembly-CSharp.csproj` (compile includes for new files — NOT committed; Unity regenerates)

---

## Remaining work (deferred)

- Phase 2-3: Unity batchmode compile + Play Mode human scenario (DEFERRED_TO_FINAL_VALIDATION by owner).
- Map `AccessoryEffectType.TrapDetectionRadiusFlat` to the Nyx amulet magnitude in the accessory catalog
  so the detection effect becomes active in-game (today the flag is wired but magnitude is 0 — inert).
- v2: the remaining ~10 §23 trap types; scripted 100/101 traps; pet detection.
