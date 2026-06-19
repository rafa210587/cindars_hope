# Execution Report — fable_33 Bestiary Data Expansion (60 creatures + 4 finals)

> Spec: `.specs/a_implementar/fable/fable_33_spec_bestiary_data_expansion_60_creatures.md`
> Date: 2026-06-19
> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> validated_adrs: [ADR-0005]
> validated_game_rules: [cave_rules.md]

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS` — all core acceptance criteria are implemented as code-side
canonical data + a deterministic, EditMode-tested planner formula/table, and both assemblies build
exit 0. Two pieces are explicitly DEFERRED by owner authorization (no Unity in this session):

- **Asset generation** (`.asset` materialization of the 77 fichas) — generator is complete and
  idempotent; command documented below; NOT RUN.
- **F30 `64/64` log + replay validator PASS + EditMode test execution** — require Unity Editor /
  Test Runner; NOT RUN. The EditMode test suite is **compile-verified** (it builds into
  Assembly-CSharp with 0 errors) but not executed.

No premature `ACCEPTED` / `PLAYMODE_VALIDATED` / "F30 64/64 PASS" claim is made.

---

## Count reconciliation (important honesty note)

The catalog's PARTE J headline says "60 band creatures + 4 finals = 64 fichas". The seven band
tables, however, list every miniboss and gate boss inline, so the full set of **distinct enemy ids**
is larger. This spec materializes **all** distinct canonical fichas:

| Bucket | Count | Notes |
|---|---|---|
| Band commons (not miniboss/boss) | 50 | |
| Band minibosses | 14 | 2 wandering per band × 7 bands (decision Q12.3) |
| Gate bosses (SpoilerTier 3) | 9 | one per gate; RUINS (60+70) and VOID (90+100) have two each |
| Final Four (SpoilerTier 4) | 4 | the level-101 finale |
| **Total distinct fichas** | **77** | `CanonicalBestiaryCatalogCounts.TotalDistinct` |
| Catalog "band roster" headline | 64 | commons + minibosses (`BandRosterHeadline`) |

These exact numbers are asserted by `BestiaryDataTests` so the catalog can never silently drift.
The pre-existing legacy roster under `Assets/_Game/Data/Enemies/Roster` (60 heuristic assets, with
the OLD trademarked Drow/Duergar names) is left untouched; the canonical fichas live in their own
`Assets/_Game/Data/Enemies/Canonical` folder once generated.

---

## Acceptance criteria extracted

| ID | Criterion | Status | Evidence |
|----|-----------|--------|----------|
| CA-1 | Catalog complete + integral; zero broken drop refs; F30 64/64 | PARTIAL (deferred F30 run) | 77 fichas authored; `Catalog_Drops_ReferenceKnownItemIds` + `Catalog_AllIds_AreUniqueAndWellFormed` assert integrity. F30 asset-count log = DEFERRED (Unity). |
| CA-2 | Scale formula HP×1.12^(lvl−min), DMG×1.08^(lvl−min), DEF fixed | DONE | `CaveBandScaling`; `Scale_FollowsCanonicalFormula_PerBand`, `Scale_AtBandMinimum_ReturnsBaseValues`. |
| CA-3 | Same seed = same composition; revisits do not reroll | DONE (logic) | `CaveBandSpawnTable.ResolveComposition` (FNV-1a, planner seed); `Spawn_SameSeed_SameComposition`. Replay validator run = DEFERRED. |
| CA-4 | Veilkin/Gravedelver applied; zero trademarks; English names | DONE | `Catalog_NoTrademarkedNames_Anywhere`, `Catalog_RenamesApplied_VeilkinAndGravedelverPresent`. |

---

## Existing systems audit (Phase 0 — reuse, no parallel systems)

| System | Path | Decision |
|--------|------|----------|
| `EnemyDataSO` | `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` | **Extended** — 3 additive save-safe fields (`SpoilerTier`, `BestiarySize` enum, `Notes`); reused existing `BestiaryEntryId`/`IsMiniBoss`/`IsBoss`/`MoveSecondary`/`VulnerabilityMatrixProfileId`/`VisualScale`. No model rewrite. |
| `EnemyMovementType` (22 moves, fable_24) | `Combat/Data/EnemyMovementProfileSO.cs` | **Consumed only** — every ficha move is one of the 22; enum untouched. |
| `EliteAffix` / `EliteAffixRules` (fable_24) | `Scripts/Enemy/` | Untouched (named-elite overlay already deterministic). |
| Enemy roster generator | `Editor/EnemyTaxonomy/CreateRoster40EnemyData.cs` | **Mirrored, not modified** — new `GenerateCanonicalBestiary` follows the same idempotent `LoadAssetAtPath`→create/update pattern; legacy generator left intact. |
| `EnemyDatabaseSO` registry | `Scripts/Combat/EnemyDatabaseSO.cs` | Reused (DataRegistrySO). |
| `CaveEnemySpawnPlanner` | `Cave/Runtime/CaveEnemySpawnPlanner.cs` | **Not modified** — its FNV-1a `StableHash` seed already provides stable-run determinism; new band scaling/table are additive pure helpers reusing `StableHash`. No second selection path forced into the planner. |
| F30 validator | `Editor/Validation/ValidateCatalogConsistency.cs` | Reused — scans `t:EnemyDataSO`, cross-checks `dropItemId` → item, asserts unique `BestiaryEntryId`. |
| Item catalog (fable_32) | `Editor/Items/CanonicalItemCatalog.cs` | Consumed — every `PrimaryDropItemId` is an existing `item_*` id. |

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation |
|---|---|
| 64 fichas materialized with all fields (stats/move/family/vuln/drops/spoiler/bestiaryId/size) | `CanonicalBestiaryCatalog` (77 distinct fichas, 1 partial file per band) → `GenerateCanonicalBestiary` writes every field onto EnemyDataSO. |
| Idempotent generator by enemyId | `GenerateCanonicalBestiary.Generate()` (`LoadAssetAtPath` → create-or-update). |
| Scale +12%/+8% per level in the planner (not assets); DEF fixed | `CaveBandScaling.ScaleHp/ScaleDamage/ScaleDefense`; assets store band-base. |
| Minibosses (≥2 per gate) + bosses marked (+1 size category visually) | 14 minibosses (2/band) + 13 bosses; `VisualScale` ×1.5 miniboss / ×2.5 boss. |
| 4 finals as assets; special mechanics dormant with flag | `BandFinalFour` (Vel-Karaum, Cindrathel, Archivist, Ithryndor); mechanics documented in `Notes` (dormant). |
| Band spawn tables (deterministic; aquatic/nocturnal/non-aggr flags) | `CaveBandSpawnTable.BuildPool`/`ResolveComposition`. |
| Renames applied (Drow→Veilkin, Duergar→Gravedelver) | Catalog uses only renamed ids/names; legacy assets untouched (documented). |
| Size class Tiny..Gargantuan → placeholder scale | `BestiarySizeClass` enum + `ResolveVisualScale`. |
| No new Move / no parallel brain for uncovered behaviour | Dormant behaviours captured in `Notes` only. |
| Drops reference existing item ids (no broken F30 refs) | Every `PrimaryDropItemId` ∈ fable_32 catalog (asserted in tests). |
| EditMode tests (formula, counts, renames, weights, determinism) | `BestiaryDataTests` (21 tests). |

---

## Files changed

**Runtime (`Assembly-CSharp`):**
- `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` (3 additive fields + `BestiarySizeClass` enum)
- `Assets/_Game/Scripts/Combat/Bestiary/BestiaryCreatureDef.cs` (new)
- `Assets/_Game/Scripts/Combat/Bestiary/CanonicalBestiaryCatalog.cs` (+ `.BandStone/.BandFungal/.BandIce/.BandFire/.BandRuins/.BandDeep/.BandVoid/.BandFinalFour.cs`) (new, 9 files)
- `Assets/_Game/Scripts/Cave/Runtime/CaveBandScaling.cs` (new)
- `Assets/_Game/Scripts/Cave/Runtime/CaveBandSpawnTable.cs` (new)

**Editor (`Assembly-CSharp-Editor`):**
- `Assets/_Game/Scripts/Editor/Enemies/GenerateCanonicalBestiary.cs` (new generator)

**Tests (`Assembly-CSharp`):**
- `Assets/_Game/Tests/EditMode/Cave/BestiaryDataTests.cs` (new, 21 tests)

**Project files:**
- `Assembly-CSharp.csproj` (+13 Compile includes), `Assembly-CSharp-Editor.csproj` (+1)

**Docs:** this report.

No forbidden files touched (no `.unity`/`.prefab`/`.asset` manual YAML, no `Packages/`,
`ProjectSettings/`, scenes, `tools/docs`, `.claude/rules`, EnemyBrain/Moves enum, cave layout gen).

---

## Validation

```text
Validation method: dotnet build (both assemblies) + run_strict_validation.ps1
Assembly-CSharp:        PASS — exit 0, 0 warnings, 0 errors
Assembly-CSharp-Editor: PASS — exit 0, 0 errors (3 PRE-EXISTING warnings in
                        CreateEnemyActionsAndSets.cs / CSharpProjectPostprocessor.cs — not this spec)
Docs validation (validate_docs.ps1):        PASS (exit 0)
check_spec_diff_completeness.ps1:            PASS (exit 0; 13 runtime + 1 test + 1 report)
run_strict_validation.ps1:                   STRICT_VALIDATION_RESULT: VALIDATION_PASS, exit 0
                                             (stdout result; this harness prints to stdout and does
                                             not persist a JSON artifact)
```

run_strict_validation.ps1 step results (stdout): docs PASS, Assembly-CSharp PASS,
Assembly-CSharp-Editor PASS, spec diff completeness PASS, spec quality check PASS.

### Deferred asset generation (command for the human / Unity session)

```text
Asset generation: DEFERRED (owner-authorized; Unity not run in this session)
Menu:   CindarsHope/Generate/Data/Canonical Bestiary (fable_33)
Batch:  Unity -batchmode -quit -projectPath . \
          -executeMethod CindarsHope.Editor.Enemies.GenerateCanonicalBestiary.RunBatch \
          -logFile docs/validation/logs/fable_33_generate_bestiary.log
Expected: 77 EnemyDataSO assets created under Assets/_Game/Data/Enemies/Canonical
          (Created: 77, Updated: 0 on first run; Created: 0, Updated: 77 on re-run).
Then:   CindarsHope/Validate/Catalog Consistency  → bestiary count + drop cross-ref
        (expected: zero broken drop refs; bestiary distinct = 77).
Residual risk: assets/GUIDs/.meta not verified until generated in Unity; F30 count log,
        replay validator PASS, and EditMode test execution remain pending the Unity session.
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (band scaling formula + deterministic spawn composition)
Changed Unity scene/prefab/asset wiring: NO (asset generation deferred; no manual YAML)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Cave/BestiaryDataTests.cs, 21 tests)
Automated tests command: Unity Test Runner EditMode (DEFERRED — owner-authorized; tests compile
                         clean into Assembly-CSharp, exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (variety-by-band perceived check, lote)
Justification if no automated tests: N/A (tests added; only execution is deferred)
Residual risk: tests compile but are not yet executed; F30 64/64 log and replay validator PASS are
               pending the deferred Unity session. Catalog numbers/ids/renames are statically
               asserted by the suite, reducing drift risk to test-execution only.
```

---

## Dependency Chain

```text
Original target: fable_33
Dependencies: F24 (22 moves) BUILD_VALIDATED, F06 (loot/vuln) BUILD_VALIDATED,
              F32 (items) BUILD_VALIDATED, F30 (validator) BUILD_VALIDATED — all done.
Forbidden dependencies: none.
Resolved depth: 0 (no same-wave chain opened).
Can continue original target: YES.
```

---

## ADR / game_rule adherence

- **ADR-0005 (Cave Stable Run and Replay):** band composition + scaling are pure deterministic
  functions seeded by `CaveWorldSeed|CaveRunSeed|CaveLevel|slot` via the existing FNV-1a
  `CaveEnemySpawnPlanner.StableHash`. No `Guid.NewGuid()`, `DateTime`, or unseeded `Random`. Same
  run = same composition on revisit; `ForwardExit/BackExit` are untouched. Replay validator run is
  DEFERRED (Unity).
- **cave_rules.md:** density constants (16-32, cap 44 — ADR-0016) untouched; nocturnal decided at
  level generation (passed `isNight`), aquatic gated on lake — "stable-run beats the clock" honored.

---

## Remaining work (deferred, owner-authorized)

1. Run `GenerateCanonicalBestiary` in Unity → 77 assets + `.meta`.
2. Run `ValidateCatalogConsistency` (F30) → attach bestiary count + drop cross-ref log.
3. Run EditMode Test Runner (BestiaryDataTests, 21 tests).
4. Run cave replay validator → stable-run PASS.
5. Final human Play Mode: perceived variety per band.
