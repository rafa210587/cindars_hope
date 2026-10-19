# Execution Report — fable_51 Zrix Cave Contracts

> Spec: `.specs/a_implementar/fable/fable_51_spec_zrix_cave_contracts.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Phase 2-3 Unity/Play Mode deferred by owner authorization)
> Wave: FABLE Batch 10 — E51
> Date: 2026-06-20

---

## Acceptance criteria extracted

| ID | Criterion | Implementation | Evidence | Status |
|----|-----------|----------------|----------|--------|
| CA-1 | Zrix board lists CaveContract quests; accept/turn-in through the F34 flow | `Board_Zrix` (reuses `QuestBoardInteractable`) posts the 8 `cc_*` ids; `QuestSource.CaveContract` already in enum; tab grouping = Contracts | `CaveContractsTests.Milestone_Instance_HasCaveContractSource_AndContractsTab`; `CreateZrixContractBoard` in `CreateMvpFarmScene.cs` | OK |
| CA-2 | `cc_depth_N` auto-completes at level N (1×), XP scaled + map-segment flag; reload does not duplicate | Milestone built as F34 dynamic instance, objective `ReachCaveDepth`, completed by `QuestService.OnCaveLevelEntered`; XP via `InstanceRewardXp` idempotent through `GrantedRewardIds`; map flag via additive reward | `Milestone_Completes_OnReachingDepth_ByCaveEvent`, `Milestone_NotCompleted_BelowTargetDepth`, `Milestone_RewardsOnlyOnce_NoDuplicateAfterReTurnIn` | OK |
| CA-3 | Weekly rematch deterministic by (worldSeed, week) among defeated gates only; rotates; unavailable if none defeated | `CaveContractCatalog.SelectRematchBoss` (StableHash, sorted eligible set); `BuildBossRematchInstance` returns null when none | `Rematch_SameWeekSameSeed_SameBoss`, `Rematch_OnlyAmongDefeatedGates`, `Rematch_Unavailable_WhenNoGateDefeated`, `Rematch_WeeksRotate_OverEligibleSet`, `RefreshWeek_NoGateDefeated_OffersOnlyNoHit`, `RefreshWeek_WithDefeatedGate_OffersBothWeeklies`, `RematchInstance_TurnIn_GrantsGuaranteedEssence` | OK |
| CA-4 | Any damage invalidates no-hit; clean clear completes + grants title + charm | `NoHitFloorTracker` (enter/damage/evaluate); `CaveContractService.OnLevelCleared` marks objective complete | `NoHit_*` (6 tests) incl. `NoHit_Contract_Completes_OnCleanClear`, `NoHit_Contract_NotCompleted_WhenDamaged` | OK |
| CA-5 | Weekly instances + progress survive save/load (simple params); no seed/snapshot of cave touched | Reuses F34 additive instance save (`QuestStateRecord`); no procedural/cave files changed (ADR-0005) | `Milestone_SurvivesSaveLoad_AndStillRewardsOnce`; scope diff has zero procedural-cave files | OK |

---

## Existing systems audit

| System | Found? | Decision |
|--------|--------|----------|
| `QuestSource.CaveContract` enum value (F34 emenda) | YES — already `= 5` (EMENDA 2026-06-12-B applied) | REUSE — no enum change; emenda dependency already satisfied (Dependency Chain depth 0) |
| `QuestService.AcceptDynamicInstance` / `RegisterDynamicInstance` / `OnCaveLevelEntered` / `OnEnemyKilled` / `MarkObjectiveComplete` (F34) | YES | REUSE — all cc_* flow through these; single accept/progress/turn-in/save |
| `QuestRewardScaling.Scale` (F34 single reward formula) | YES | REUSE — milestone/weekly XP scaled through this one point only |
| `QuestStableHash` (F34 FNV) | YES | REUSE — weekly target selection (no Unity Random/GUID/timestamp) |
| `QuestRewardApplicator` (Gold/Item/QuestFlagGrant) | YES | REUSE — additive item/flag rewards flow through it |
| `QuestBoardInteractable` (F34) | YES | REUSE — Zrix board is another access point, not a new component |
| `QuestRuntimeBootstrap` (board day-rotation host) | YES | EXTEND — host the `CaveContractService` alongside the notice board |
| `CaveBossDefeatedEvent` / `CaveBossGateService` | YES | CONSUME — gate-defeated eligibility (`BossGateId`) |
| `CaveLevelEnteredEvent` / `CaveExitedEvent` / `PlayerDamagedEvent` / `DayStartedEvent` | YES | CONSUME read-only via GameEventBus |
| `GameDate.DaysPerWeek = 7` / `VeskaWeeklyRotationService` idiom | YES | REUSE — `(day-1)/7` canonical week index |
| `LocalizationService` / `LocalizationStringTable` (F73) | YES | EXTEND — new player-facing text declared as keys |
| `Quests/CaveContracts/CaveContractDefinition+Adapter+Validator` (prior authoring-data layer) | YES — referenced only by `FestivalCaveContractAdapterTests` | LEFT UNTOUCHED — separate authoring layer; new runtime classes use distinct names (no duplication) |
| New systems created | — | `CaveContractCatalog`, `CaveContractService`, `NoHitFloorTracker`, `CaveContractsRefreshedEvent` only (all cc_* runtime; no parallel quest registry/manager/board/formula) |

---

## Spec Compliance Matrix

| Requirement | Implementation | OK |
|-------------|----------------|----|
| 6 milestones `cc_depth_5/_15/_30/_50/_70/_90` (canonical ids, 1×) | `CaveContractCatalog.MilestoneDepths` + `MilestoneId` | OK |
| Milestone reward: scaled XP + `map_segment_<band>` flag | `BuildMilestoneInstance` (RewardXp scaled, QuestFlagGrant) | OK |
| `cc_boss_rematch` weekly, deterministic, defeated-gate only, guaranteed essence | `BuildBossRematchInstance` + `SelectRematchBoss` + Item essence reward | OK |
| `cc_no_hit_floor` weekly, deterministic eligible level, title + charm | `BuildNoHitInstance` + `NoHitFloorTracker` + title flag + charm item | OK |
| Weekly rotation on week change; accepted weeklies not revoked | `CaveContractService.RefreshWeek` (skips same week; only registers new) | OK |
| `CaveContractsRefreshedEvent` published | `RefreshWeek` publishes it | OK |
| Save: dynamic instances + flags via F34 additive mechanism; no new section | `QuestInstance.AdditionalRewards` + existing `QuestStateRecord` DTO | OK |
| Zrix board interactable at cave entrance via generator | `CreateZrixContractBoard` in `CreateMvpFarmScene.cs` | OK |
| Single flow / no second board/registry/formula | All via `QuestService`; `QuestBoardInteractable` reused | OK |
| ADR-0005: cave seeds/snapshot untouched | Zero procedural/seed/snapshot files in diff | OK |
| Events only via GameEventBus; unsubscribe; no GameObject.Find | `CaveContractService.Subscribe/Unsubscribe`; bootstrap OnDisable | OK |
| New text via LocalizationService (F73) | `LocalizationStringTable` keys `interact.zrix_board.*`, `contract.cave.*` | OK |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1 (EXPECTED_FAIL_LEGACY_ONLY — see note; NOT caused by this spec)
Assembly-CSharp: PASS (0 errors, 1 pre-existing warning)
Assembly-CSharp-Editor: PASS (0 errors, 4 pre-existing warnings)
Quality check: FAIL only on pre-existing forbidden-file state + pre-existing report-section warnings (none in this spec's files)
Docs validation: PASS (validate_docs.ps1 exit 0)
Diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Strict-validation exit-1 root cause (honest): the working-tree-wide quality check reports three
git-LFS scene pointer files as "forbidden files altered" —
`Assets/_Game/Scenes/{CaveScene,FarmScene,TownScene}.unity`. These are **pre-existing
environmental modifications**: the scenes are git-LFS tracked and were rewritten by Unity in a
prior session (LastWriteTime 18:03), ~9 minutes BEFORE any file in this spec was created
(18:12–18:13). This spec touched **zero** `.unity/.prefab/.asset` files. They are NOT staged or
committed by this spec. The "report sections missing" entries are all WARNINGS on pre-existing
reports authored by earlier specs — this spec's own report has every mandatory section. Per
`spec_quality_gate` / environment policy, a pre-existing working-tree condition the spec did not
cause is not a spec failure; this is recorded as `EXPECTED_FAIL_LEGACY_ONLY` for the strict gate.
Every validator scoped to this spec's changes passes (builds 0E, docs exit 0, diff completeness
exit 0, no forbidden APIs / no parallel system / no Unity refs in save).

Build note: `dotnet build` first returned NETSDK1004 (transient `project.assets.json` missing);
`dotnet restore` on both csproj regenerated it, then `--no-restore` builds returned exit 0. The
csproj files are gitignored build artifacts (Unity-generated) and are NOT committed.

---

## Honest status rationale

Status is **BUILD_VALIDATED_WITH_WARNINGS**, not ACCEPTED. All core logic (milestone completion, weekly determinism, rematch eligibility, no-hit state machine, save round-trip, idempotency) is implemented and covered by EditMode tests. Phase 2-3 (Unity batchmode compile + Play Mode: accept a milestone and complete it by descending; accept a rematch; clear a no-hit level) is **NOT RUN** — deferred by explicit owner authorization for this batch. The Zrix board is wired into the FarmScene generator; a human must run `CreateMvpFarmScene` and the Play Mode scenario before ACCEPTED. The two pre-existing warnings (main) / four (editor) are unchanged from before this spec.

EditMode tests were authored but **NOT executed** here (no Unity Test Runner / Play Mode in this sandbox per owner authorization); compile of the test assembly is proven by `dotnet build` exit 0. Residual risk: tests are validated by compilation only until the human runs the Test Runner.

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (weekly rotation, rematch eligibility, no-hit machine, milestone idempotency)
Changed Unity scene/prefab/asset wiring: YES (FarmScene via generator only — no manual YAML)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/CaveContractsTests.cs — 20 tests)
Automated tests command: NOT RUN (Unity Test Runner deferred; compile proven via dotnet build exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (accept cc_depth + descend; accept cc_boss_rematch; clear a no-hit level)
Justification if no automated tests: N/A (tests added)
Residual risk: EditMode tests validated by compilation only until human runs Test Runner; FarmScene needs regeneration + Play Mode for board interaction; PlayerDamagedEvent coverage of DoT/hazard relies on the single damage point routing through it (audited true).
```

### Dependency Chain

```text
Original target: fable_51_spec_zrix_cave_contracts
Dependency chain: F34 (QuestSource.CaveContract emenda) — already applied (enum value = 5, EMENDA 2026-06-12-B)
Forbidden dependencies: none
Resolved depth: 0 (emenda already present; no same-wave resolution needed)
Plan file / Batch state: N/A (single spec execution)
Can continue original target: YES
```

---

## validated_adrs / validated_game_rules

```text
validated_adrs: [ADR-0005-cave-stable-run-and-replay.md (no cave seed/snapshot touched — read-only event consumption),
                 ADR-0007-event-bus-gameplay-communication.md (all comms via GameEventBus; unsubscribe in OnDisable)]
validated_game_rules: [cave_rules.md (stable-run contract preserved; no procedural change),
                       save_rules.md (simple types + stable ids only; no Unity refs; additive F34 instance save),
                       event_rules.md (1 new event CaveContractsRefreshedEvent; consumers read-only; unsubscribe pattern)]
```

---

## Remaining work

- Phase 2: Unity batchmode compile + Test Runner EditMode (deferred).
- Phase 3: human Play Mode scenario (accept a milestone and complete by descending; accept rematch; clear a no-hit level) — `DEFERRED_TO_FINAL_VALIDATION`.
- Human must regenerate FarmScene via `CreateMvpFarmScene` to materialize `Board_Zrix`.
- The "map segment" flag (`map_segment_<band>`) is delivered as a flag/item; F38 minimap consumes it later (out of scope here).
