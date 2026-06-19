---
doc_type: validation_report
spec_id: fable_05_spec_cave_boss_phase_ai_runtime
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]
---

# Execution Report — fable_05 Cave Boss Phase AI Runtime

## Status

**BUILD_VALIDATED_WITH_WARNINGS**

Core criteria implemented and audited; runtime builds clean (0E). Phase-resolution and add
determinism logic covered by EditMode tests. Live boss-fight feel (3-phase legibility, telegraph
timing, add behaviour in scene) is the Play Mode acceptance step, intentionally DEFERRED
(`DEFERRED_TO_FINAL_VALIDATION` per spec §Impacto em UI/Unity and owner authorization). No
`.unity`/`.prefab`/`.asset` was hand-edited; new assets are produced by an editor generator (run
deferred to the human Unity checkpoint).

---

## Honest status rationale

- Build (Assembly-CSharp + Assembly-CSharp-Editor): PASS, exit 0, 0 errors.
- Deterministic phase + add logic: EditMode tests authored (`BossPhaseTests`, 14 cases) and compile
  into Assembly-CSharp; Unity Test Runner execution is the deferred EditMode checkpoint (same posture
  as prior FABLE batches — "~N testes novos … Unity Test Runner EditMode" run at checkpoint).
- The boss fight itself depends on scene objects / Unity lifecycle (EnemyBrain pursuit, telegraph
  visuals, real adds), so per the Testing Quality Gate the maximum honest status without a Play Mode
  scenario is BUILD_VALIDATED. Not claimed: ACCEPTED, PlayMode PASS, Unity validated.
- Asset generation (`AttachDefaultBossPhaseProfiles`) NOT RUN here (no Unity batchmode in this
  session). The boss-phase wiring is inert until a profile asset + registry exist, so existing bosses
  are unaffected (anti-regression preserved).

---

## Dependency Chain

```text
Original target: fable_05_spec_cave_boss_phase_ai_runtime
Dependency chain:
  - fable_02 (posture/stagger) — BUILD_VALIDATED (CHECKPOINT M1)
  - fable_04 (threat/pack coordination) — BUILD_VALIDATED (commit 039b17c1; EnemyThreatState/EnemyPackCoordinator)
  - fable_24 (enemy moves/elite affixes) — BUILD_VALIDATED (commit 34db5fbc; EnemyBrain.SetArenaLeash/ShiftPhase primitives)
Forbidden dependencies: none
Resolved depth: 0 (all dependencies already BUILD_VALIDATED before this spec)
Can continue original target: YES
```

EnemyBrain lock (`Must not run with: fable_02, fable_04`) is respected: F02/F04/F24 are all done; this
is the only session writing code.

---

## Existing systems audit (Phase 0 — System Reuse)

Goal: build phases on top of the live EnemyBrain; create no parallel boss state machine, no parallel
spawn path, no parallel defeat tracking (spec §Regras de não duplicação).

| Concept | Found existing? | Decision |
|---|---|---|
| Boss phase shift primitive | `EnemyBrain.ShiftPhase(EnemyActionSetSO, EnemyMovementProfileSO)` (fable_24) | REUSE — `SwapActionSet(string)` is a thin wrapper that resolves the id and delegates to `ShiftPhase` |
| Arena leash primitive | `EnemyBrain.SetArenaLeash(Vector2, float)` (fable_24) | REUSE — available for arena bosses (not forced by the ground ooze king default) |
| HP observation | `EnemyHealth.CurrentHp/MaxHp/IsDead` | REUSE |
| Vulnerability window | `EnemyVulnerabilityState.OpenWindow(dur, mul, cd)` | REUSE for phase-transition windows (CA-2) |
| Deterministic hash | `CaveEnemySpawnPlanner.StableHash(string)` (FNV-1a) | REUSE for add positions (ADR-0005 consistency) |
| Action set data | `EnemyActionSetSO` / `EnemyActionSetDatabaseSO` / `DataRegistrySO<T>` | REUSE |
| Combat databases bundle | `CombatRuntimeDatabasesRegistrySO` (resource-loadable, FIX10) | REUSE to wire the boss brain |
| Boss lifecycle / defeat | `CaveBossSpawner` / `CaveBossDeathReporter` / `CaveBossDefeatMonitor` | REUSE — defeat ownership unchanged; spawner only gains optional phase attach |
| Event bus | `GameEventBus` | REUSE for `BossPhaseChangedEvent` |
| Movement profiles | `EnemyMovementProfileDatabaseSO` + `EnemyMovementProfileSO` | REUSE for the EMENDA flight-phase Move swap |

No equivalent of "boss phase profile" / "phase resolver" / "phase controller" existed — those are the
genuinely new pieces this spec adds.

---

## Acceptance criteria extracted

| CA | Requirement | Implementation | Evidence |
|---|---|---|---|
| CA-1 | 3-phase boss swaps ActionSet + multipliers at 66%/33% | `BossPhaseLogic.ResolvePhaseIndex` resolves from HP%; `BossBrainController.EnterPhase` swaps via `EnemyBrain.SwapActionSet` + `ApplyPhaseMultipliers`; `BossPhaseChangedEvent` + `CombatLog: BossPhaseChanged` log | `BossPhaseTests.ResolvePhase_*` (boundaries 100/66/33); runtime log line |
| CA-2 | Each transition: telegraph >= 1s with no boss damage + vulnerability window | `BeginTransitionTelegraph` holds the boss via `EnemyBrain.ApplyStun(1s)` (stops attacks+movement → no boss damage) and publishes `EnemyTelegraphStartedEvent`; `CompleteTransition` opens `EnemyVulnerabilityState.OpenWindow` after the telegraph | `BossBrainController` transition path; `MinTransitionTelegraphSeconds=1f` constant; human Play Mode scenario (deferred) |
| CA-3 | Adds invoked exactly AddsCount once per phase, deterministic positions by run seed; revisit does not re-invoke past phases | `SummonAddsOnce` guarded by `_addsSpawnedForPhase` (idempotent); positions via `BossPhaseLogic.ResolveAddTiles` seeded by `StableHash(world|run|level|bossId|phase|i)`; phase derived from current HP so a revisit at the same HP resolves to the same phase | `BossPhaseTests.AddTiles_AreDeterministicForSameSeed / _DifferForDifferentRunSeed / _DifferPerPhase / _NeverPlaceOnBossTile_AndNoDuplicates / _RespectsCount` |

---

## Spec Compliance Matrix

| Spec requirement | Implementation | Status |
|---|---|---|
| `BossPhaseProfileSO` (IIdentifiedData `boss_phase_<bossId>`, phases desc) | `Assets/_Game/Scripts/Cave/Data/BossPhaseProfileSO.cs` | OK |
| Fields: HpThreshold, ActionSetId, MoveSpeedMul, DamageMul, AddsEnemyId, AddsCount, VulnerabilityWindowSeconds | `BossPhase` serializable class | OK |
| `BossBrainController` wraps EnemyHealth, resolves phase, swaps set, applies multipliers, telegraph+window, adds 1x/phase | `Assets/_Game/Scripts/Cave/Runtime/BossBrainController.cs` | OK |
| `EnemyBrain.SwapActionSet(string)` (new) | EnemyBrain.cs — resolves id, delegates to `ShiftPhase` | OK |
| `BossBrainController.CurrentPhaseIndex` | property exposed | OK |
| `BossPhaseChangedEvent(bossId, phaseIndex)` (new) | `Core/Events/EnemyEvents.cs` | OK |
| Deterministic adds via `StableHash(runSeed|level|bossId|phase|i)` near walkable tiles (ADR-0005) | `BossPhaseLogic.ResolveAddTiles` / `BuildAddSeedSource` | OK |
| Editor generator attaching default 3-phase profile to CreateCaveBossAssets bosses | `Editor/CaveData/AttachDefaultBossPhaseProfiles.cs` | OK (code; asset gen deferred) |
| Spawner integration: AddComponent BossBrainController when profile present | `CaveBossSpawner.TryAttachBossPhaseController` | OK |
| No parallel state machine — wrapper over EnemyBrain | controller drives the brain only | OK |
| No duplicate spawn — adds via boss-owned path (no second general spawner) | `CaveBossSpawner.SpawnAdd` | OK |
| Defeat tracking unchanged | `CaveBossDeathReporter` untouched | OK |
| Save schema unchanged | no save DTO/SaveManager edits | OK |
| EMENDA: per-phase Move swap (flight phase) | `BossBrainController.ApplyPhaseBindings` resolves `MovementProfileId` → `ShiftPhase(null, profile)` | OK (capability; ground ooze king leaves it empty) |
| Cap adds to DepthScalingHardCap (risk mitigation) | adds count is authored (default 2) and bounded by available walkable candidates | PARTIAL — global enemy-budget cap is the planner's; boss adds are small fixed counts, documented |

---

## Cave Stable Run / ADR-0005 compliance

- Boss/phase state is **derived from current HP**, not stored: a revisited level resolves to the same
  phase for the same HP. One-way progression (`BossPhaseLogic.ClampForwardOnly`) prevents a heal from
  regressing the phase.
- Add positions are seeded by `StableHash(CaveWorldSeed|CaveRunSeed|CaveLevel|bossId|phaseIndex|addIndex)`
  ordered over the level's walkable tiles — same run/level/boss/phase ⇒ identical tiles/order on every
  revisit. No GUIDs, timestamps, or unseeded `Random` on the stable path.
- `ForwardExit`/`BackExit` do not change `CaveRunSeed` (untouched).
- **Documented limit:** boss HP itself does not persist between visits (so adds already summoned in a
  prior visit re-summon on a fresh visit of the same level if the boss is back at that phase's HP). This
  is the known `CAVE_ENEMY_HP_SAVE_DEBT`, owned by F13, exactly as the spec states (CA-3 note + §Save
  contracts + §Notas).

---

## Validation

```text
Validation method: dotnet build (both csproj) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp:        PASS (exit 0, 0 errors, 0 warnings)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings)
Docs validation:        PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (after report; exit 0)
Strict validation:      PASS (run_strict_validation.ps1 exit 0) — artifact docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Note: the csproj files are git-ignored (Unity-generated); `<Compile Include>` entries were added
locally so dotnet picks up the new files. They are not committed (project convention).

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (phase resolution, one-way progression, add seeding)
Changed Unity scene/prefab/asset wiring: NO (generator authors assets; not run here)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/World/BossPhaseTests.cs — 14 cases)
Automated tests command: Unity Test Runner EditMode (NOT RUN this session — deferred to checkpoint); tests compile into Assembly-CSharp (build PASS)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (boss fight legibility / telegraph / adds in scene)
Justification if no automated tests: N/A (tests added for the deterministic core)
Residual risk:
  - Boss-fight feel (phase pacing, telegraph readability, two-driver handoff EnemyChaseController→EnemyBrain) unverified until Play Mode.
  - Default profile references action-set ids that exist now; if a referenced id is later removed the swap no-ops gracefully (logged), but the phase would keep the prior set.
  - Asset generation not executed: profiles/registry do not exist until the human runs the generator, so phases are inert (existing bosses unaffected — this is the intended anti-regression state).
Regression test: boss WITHOUT a profile keeps EnemyChaseController-only behaviour (TryAttachBossPhaseController early-returns) — covered structurally; Play Mode confirmation deferred.
```

---

## Files changed

New (runtime):
- `Assets/_Game/Scripts/Cave/Data/BossPhaseProfileSO.cs`
- `Assets/_Game/Scripts/Cave/Data/BossPhaseProfileRegistrySO.cs`
- `Assets/_Game/Scripts/Cave/Runtime/BossPhaseLogic.cs` (pure, testable)
- `Assets/_Game/Scripts/Cave/Runtime/BossBrainController.cs`

New (editor):
- `Assets/_Game/Scripts/Editor/CaveData/AttachDefaultBossPhaseProfiles.cs`

New (tests):
- `Assets/_Game/Tests/EditMode/World/BossPhaseTests.cs`

Modified (runtime):
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` — `SwapActionSet(string)`, `ApplyPhaseMultipliers`, `PhaseDamageMultiplier`; phase move/damage multipliers folded into `MoveSpeed()` and the melee/projectile damage paths.
- `Assets/_Game/Scripts/Core/Events/EnemyEvents.cs` — `BossPhaseChangedEvent`.
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs` — optional `BossPhaseProfileRegistrySO`/`CombatRuntimeDatabasesRegistrySO` fields, `TryAttachBossPhaseController`, boss-owned `SpawnAdd`, add cleanup.

Local-only (git-ignored, not committed): `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj` compile includes.

---

## Remaining work (deferred)

1. Run `CindarsHope/Archive/Generate/Scenes/Attach Default Boss Phase Profiles` in Unity to author
   `BossPhaseProfileRegistry.asset` + `boss_phase_enemy_meteor_ooze_king.asset`.
2. Wire the registry + `CombatRuntimeDatabasesRegistry` into the CaveScene `CaveBossSpawner` (or rely on
   the Resources.Load fallback for the combat registry).
3. Unity Test Runner EditMode (BossPhaseTests + the accumulated FABLE suite).
4. Play Mode scenario: drive a boss through all 3 phases, confirm telegraph (>=1s, no boss damage),
   vulnerability window, and exactly 2 adds once in the final phase at deterministic positions.
5. WAVE 19 (level 100/101 final boss) reuses `BossPhaseProfileSO`/`BossBrainController` with bespoke
   per-boss phases (EMENDA bestiary ★ bosses, incl. flight phases via FloatingOrbit movement profiles).

---

## Anti-regression

- `CaveBossDefeatMonitor`/`CaveBossDeathReporter` remain the source of boss defeat (untouched).
- Gates/checkpoints intact (no edits to gate registry/data).
- No reroll of stable content (ADR-0005): phase + add determinism documented above.
- Bosses without a phase profile behave exactly as before (the spawner branch is gated on profile
  presence, and `SwapActionSet`/`ApplyPhaseMultipliers`/`PhaseDamageMultiplier` are inert at default
  multipliers of 1 with no caller).
