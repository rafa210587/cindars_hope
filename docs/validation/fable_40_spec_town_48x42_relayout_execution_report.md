---
doc_type: validation_report
spec: fable_40_spec_town_48x42_relayout
status: BUILD_VALIDATED_WITH_WARNINGS
scene_status: DEFERRED_UI_VISUAL
date: 2026-06-20
validated_adrs: []
validated_game_rules:
  - city_rules.md
---

# Execution Report — fable_40 TownScene 48×42 Relayout

> **Honest status:** `BUILD_VALIDATED_WITH_WARNINGS`.
> The generator logic (bounds, data-driven district map, element→district relayout, two new
> districts, count audit) is implemented and compiles clean (exit 0). The **scene regeneration
> itself is DEFERRED** (`DEFERRED_UI_VISUAL`): the generator is ready but Unity Editor / batchmode
> was **not run** (owner-authorized deferral of Unity/Play Mode). `TownScene.unity` is therefore
> **unchanged** on disk — no manual YAML edit was made (city_rules / unity-yaml-editing-policy).

---

## Summary

Updated the canonical town generator `CreateMvpTownScene` to the canonical 48×42 footprint
(bounds −24..24 / −21..21, city_rules Rule 1) with a **data-driven district map** of the seven
HUD_LAYOUT §3 districts, repositioned **every** existing element into the larger footprint by a
deterministic similarity transform (so element count and relative neighborhoods are preserved by
construction), added the two new districts (lake/park SW, town hall NE + mural), expanded the
perimeter colliders and NPC wander clamps, kept all schedule-anchor and spawn-point **IDs frozen**,
and added an in-generator element-count audit (before=after). No second generator was created; no
scene/prefab/asset YAML was edited.

---

## Dependency Chain

- Original target: `fable_40_spec_town_48x42_relayout`
- Dependency chain: F11 (interiors/doors/anchors) — **already implemented** (commit 38bc1b0a);
  its interior band (y > +40) and door pairing are preserved, not modified. F19 (Fonte/services) —
  schedule-anchor and service IDs untouched.
- Forbidden dependencies: none
- Resolved depth: 0 (no same-wave pending dependency)
- Can continue original target: YES (done)

---

## Acceptance criteria extracted

| CA | Requirement | Evidence | Status |
|----|-------------|----------|--------|
| CA-1 | Scene 48×42 (bounds −24..24/−21..21), 7 districts, element count preserved (23 NPCs, houses, shops, board, statue, trees) — log before=after | `TownDistrictLayout` bounds + `AllDistricts` (7); `LogRelayoutElementCountAudit` logs before(canonical)=after(scene) and errors on mismatch; `TownLayoutTests.Footprint_Is48x42` / `DistrictMap_HasSevenDistricts` / `Reposition_KeepsExtremeLegacyCornersInsideBounds` | LOGIC DONE; scene regen DEFERRED |
| CA-2 | All named schedule anchors resolve (same IDs, new positions); scene validator passes | Anchor IDs are `npc_<id>_<work|social|home>` — suffix strings frozen and asserted == runtime `NpcScheduleBlockResolver` constants (`ScheduleAnchorSuffixes_MatchRuntimeResolverConstants`); only positions repositioned | LOGIC DONE; validator run DEFERRED |
| CA-3 | Lake/park (SW) + town hall (NE) with mural exist; board stays in plaza | `CreateLakeParkDistrict` (LakeWater + 2 park benches), `CreateTownHallDistrict` (building + `TownHallMural`); board/statue left in central plaza; `LakeParkDistrict_*` / `TownHallDistrict_*` tests assert in-district + in-bounds | LOGIC DONE; visual DEFERRED |
| CA-4 | Border colliders cover the new perimeter; spawn IDs preserved | `CreateBounds` rebuilt for ±24.5/±21.5 spanning 49×43; spawn IDs `town_default`/`town_from_farm` frozen and asserted (`SpawnIds_AreStableCanonicalSet`); positions repositioned | LOGIC DONE; in-scene impassability DEFERRED |

---

## Existing systems audit

| System | Found? | Reused / Created | Note |
|--------|--------|------------------|------|
| Town generator `CreateMvpTownScene` | YES | **REUSED / EXTENDED** | Did NOT create a second generator (non-duplication rule). Bounds/positions/new districts added in place. |
| District layout (data-driven) | NO | **CREATED** `TownDistrictLayout` (editor-only, pure C#) | Single source of truth for bounds, 7 district rectangles, relayout transform, stable ID lists. |
| `NpcScheduleBlockResolver` (work/social/home suffix constants) | YES | REUSED | Test asserts anchor suffixes stay equal to these runtime constants. |
| `NpcWanderer` bounds wiring | YES | REUSED (clamp values expanded only) | Same contract; clamps widened to the 48×42 interior. |
| `SceneSpawnInstaller` / `ScenePortal` | YES | REUSED | Spawn/target IDs frozen; positions moved. |
| `DoorInteractable` + interiors (fable_11) | YES | REUSED / PRESERVED | Interior band y > +40 untouched; only exterior door world positions repositioned. |
| Scene validators (`ValidateRefinedCanonicalNpcTownPopulation`, `ValidateTownShop*`) | YES | REUSED (PASS gate) | Not duplicated; remain the post-regeneration PASS criteria (run DEFERRED). |

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | OK |
|--------------------|----------------|----|
| Bounds −24..24 / −21..21 (48×42) | `TownDistrictLayout.HalfWidth=24`, `HalfHeight=21`; `CreateBounds` uses them | OK |
| Data-driven district map (named rectangles §3) | `TownDistrictLayout.AllDistricts` — 7 named `District` structs within bounds | OK |
| Element→district reposition; nothing lost | `TownNpcSpec.LayoutPosition` + `Reposition()` applied at every consumption (NPCs, stalls, anchors, houses, interiors-exterior, decorations, trees, spawns, portal, wanderer) | OK |
| Schedule named-point IDs preserved | Anchor id format unchanged; suffixes frozen + asserted == resolver constants | OK |
| New: lake/park SW (water + benches) | `CreateLakeParkDistrict` | OK |
| New: town hall NE + mural on wall | `CreateTownHallDistrict` (building + `TownHallMural`) | OK |
| Wander bounds recalculated per work district | clamps widened to ±23/±20; per-NPC radius around `LayoutPosition` (unchanged contract) | OK |
| Border colliders new perimeter | `CreateBounds` spans 49×43 at ±24.5/±21.5 | OK |
| Spawn points by ID preserved (farm entrance S) | `town_default`/`town_from_farm` frozen; portal `farm_from_town` target unchanged | OK |
| Count before=after in generator log | `LogRelayoutElementCountAudit` (logs + LogError on mismatch) | OK |
| EditMode tests (district bounds, coverage, IDs) | `TownLayoutTests` (14 tests) | OK |
| No second generator | extended `CreateMvpTownScene` only | OK |
| No manual YAML edit | scene NOT touched on disk; regen DEFERRED to Unity | OK |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0  (STRICT_VALIDATION_RESULT: VALIDATION_PASS)
Docs validation: PASS
Assembly-CSharp: PASS (0 Erros, 0 Avisos)
Assembly-CSharp-Editor: PASS (0 Erros, 0 Avisos)
Spec diff completeness: PASS (Runtime code files=0 [editor-only], Test files=1, no forbidden files)
Spec quality check: PASS (no forbidden files, tests in correct location, no status inflation)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Individual gates also run directly:

```text
dotnet build .\Assembly-CSharp.csproj --no-restore         -> exit 0 (0E)
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore  -> exit 0 (0E)
.\tools\docs\validate_docs.ps1                             -> PASS
.\tools\docs\check_spec_diff_completeness.ps1              -> PASS
.\tools\docs\run_strict_validation.ps1                     -> exit 0
```

Scope check (git status): only `CreateMvpTownScene.cs` (M) + `TownDistrictLayout.cs(.meta)` and
`TownLayoutTests.cs(.meta)` (new). **No** `.unity/.prefab/.asset`, **no** ProjectSettings/Packages,
**no** runtime (`Assets/_Game/Scripts/**` non-Editor) changes.

---

## Testing Quality Gate

```text
Changed runtime code: NO (editor-only scene generator + editor EditMode test)
Changed deterministic logic: YES (district layout tables + relayout transform)
Changed Unity scene/prefab/asset wiring: NO on disk (scene regeneration DEFERRED; no YAML edit)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/City/TownLayoutTests.cs, 14 tests)
Automated tests command: NOT RUN (Unity Test Runner not executable in sandbox; tests COMPILE via
  Assembly-CSharp-Editor build, exit 0). Tests are pure (no scene) and run under EditMode.
Manual Play Mode scenario: REQUIRED but DEFERRED_TO_FINAL_VALIDATION (navigate the 7 districts;
  border colliders impassable; no element lost). To be batched at wave-end human validation.
Justification if no automated tests run: environment — EditMode Test Runner requires the Unity
  Editor, which the owner authorized skipping this session. Compilation of the test in the editor
  assembly is verified (exit 0).
Residual risk:
  1. Scene .unity NOT regenerated — the new layout is not yet materialized; must run
     `CindarsHope/Create Scenes/Town Scene` in Unity and confirm the element-count audit log
     shows before=after with no LogError, then verify the existing scene validators PASS.
  2. EditMode tests compiled but not executed — assertion truth verified by construction/manual
     geometry check (see report), not by a green Test Runner run.
  3. NPC routine timing after larger distances: per spec, NPC speed was NOT touched. If NPCs arrive
     late on the bigger map, calibrate schedule hours (city_rules Rule 6) — out of scope here.
```

---

## What is DEFERRED (explicit)

| Item | Why | How to close |
|------|-----|--------------|
| `TownScene.unity` regeneration | Owner authorized skipping Unity/Play Mode this session; never edit YAML manually | Run menu `CindarsHope/Create Scenes/Town Scene` in Unity; capture log (count audit) + exit code |
| Scene validators PASS evidence | Require the regenerated scene | After regen, run the existing town validators; record PASS |
| EditMode Test Runner green run | Requires Unity Editor | Run `run-editmode-tests` / Unity Test Runner; expect 14/14 in `TownLayoutTests` |
| Play Mode navigation (7 districts, impassable borders) | Scene-dependent | Wave-end human scenario (DEFERRED_TO_FINAL_VALIDATION) |

Generated-asset evidence (per unity-assets rule):

```text
Asset generation (scene regen): BLOCKED/DEFERRED
Reason: Unity Editor not run this session (owner-authorized deferral)
Command attempted: (none — not run)
Menu to run: CindarsHope/Create Scenes/Town Scene  (CreateMvpTownScene.CreateSceneFromMenu)
Residual risk: TownScene.unity is stale (still 36×30) until regenerated in Unity
```

---

## Files changed

```text
M  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs   (bounds 48×42, data-driven
     district relayout via TownDistrictLayout, new lake/park + town hall+mural districts,
     widened wander clamps, repositioned spawns/portal/decorations/trees, element-count audit)
A  Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs    (NEW — pure editor layout:
     bounds, 7 district rectangles, Reposition() transform, stable spawn/anchor ID lists, new
     district landmark anchors)
A  Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs.meta
A  Assets/_Game/Tests/EditMode/City/TownLayoutTests.cs               (NEW — 14 EditMode tests:
     footprint, 7 districts, within-bounds, no core overlap, new-district landmarks, element
     coverage, frozen schedule-anchor & spawn IDs, reposition determinism)
A  Assets/_Game/Tests/EditMode/City/TownLayoutTests.cs.meta
```

> Note: `Assembly-CSharp-Editor.csproj` was edited locally to include the two new files for the
> build gate, but it is **git-ignored / untracked** (Unity-generated) and is **not committed**.

---

## Honest status rationale

- Build/logic: **DONE and validated** — `run_strict_validation.ps1` exit 0, both assemblies 0E/0W,
  diff completeness + quality check PASS, scope clean (no forbidden paths).
- Scene/visual: **DEFERRED** — `DEFERRED_UI_VISUAL`; the canonical `.unity` is intentionally left
  unchanged (no manual YAML), to be regenerated in Unity with evidence.
- Therefore the spec is **not** eligible to move to `implementados/` yet (status `CONTRACT_ONLY*`/
  `DEFERRED_*` blocks promotion). It stays in `a_implementar/` pending the Unity regeneration +
  validator PASS + Test Runner green + wave-end Play Mode navigation.

## Remaining work

1. Regenerate `TownScene.unity` in Unity; confirm the element-count audit logs before=after with
   no `LogError`.
2. Run the existing town scene validators on the regenerated scene; record PASS.
3. Run EditMode Test Runner; record `TownLayoutTests` result.
4. Wave-end human Play Mode: walk the 7 districts, confirm borders impassable and nothing lost.
