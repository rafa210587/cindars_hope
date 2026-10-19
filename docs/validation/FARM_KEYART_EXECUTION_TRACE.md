# FARM KEYART — Execution Trace and Audit Ledger

> **Wave:** FARM KEYART  
> **Started:** 2026-08-14  
> **Owner request:** aplicar em sequência contrato espacial, física, composição, terreno, marcos, decoração e aceitação Play Mode.  
> **Review audience:** Claude / Codex / humano.  
> **Truth rule:** um status só é registrado como concluído após verificação independente do orquestrador; relato de subagente não é evidência.

## Baseline audit

### Visual evidence

- Keyart: `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`.
- Current generated captures: `docs/validation/playmode/farm_capture.png`, `farm_capture_closeup.png`, `farm_capture_animals.png` (2026-08-14).
- In-game diagnostic overlay supplied by owner: zones are `FarmSceneZoneMarker` trigger areas and do not prove collision correctness.

### Known technical baseline

- `CreateMvpFarmScene` is the sole FarmScene generator; no manual scene YAML changes are allowed.
- `FarmLevel1LayoutContract` has 64×44 centred bounds.
- Water/path painting currently uses rectangular helpers; lake is composed from overlapping rectangles.
- `FarmSceneZoneMarker` owns trigger zones; solid collision is independently generated.
- Existing reusable art: crops, trees, foliage, well, fence, hay, animals, ore and base ground tiles.

### External dirty-worktree baseline (not owned by this wave)

```text
M Assets/_Game/Scenes/CaveScene.unity
M Assets/_Game/Scenes/TownScene.unity
?? .codex/config.toml.bak-20260812-1934
?? Assets/Resources/GDKEditionAutoGen/**
?? Assets/_Game/Art/Generated/World/_TileAssets/**
?? Assets/_Game/Art/Generated/World/tiles/*.png.meta (existing generated tile metadata)
?? Assets/_Game/Scripts/Editor/Dev.meta
?? docs/project/PLANO_FARM_KEYART_FIDELIDADE.md
```

## Dependency execution plan

| Order | Spec | Depends on | Owner | Status | Independent verification |
|---:|---|---|---|---|---|
| 1 | `spec_farm_scene_spatial_contract_v1` | relayout v4 baseline | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 2 | `spec_farm_scene_collision_navigation_v1` | 1 | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 3 | `spec_farm_scene_keyart_macro_composition_v1` | 2 | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 4 | `spec_farm_scene_organic_terrain_water_v1` | 3 | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 5 | `spec_farm_scene_landmarks_and_agriculture_v1` | 4 | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 6 | `spec_farm_scene_biome_decoration_v1` | 5 | spec implementer | PARTIAL — source reviewed | static PASS; Unity NOT RUN |
| 7 | `spec_farm_scene_keyart_playmode_acceptance_v1` | 6 | BLOCKED / NOT RUN | static PASS; Unity gate unavailable |

## Per-spec audit entries

### 1. spec_farm_scene_spatial_contract_v1

- **Implementation evidence:** `FarmSceneSpatialContract`, focused EditMode tests and `ValidateFarmSceneSpatialContract` were created; layout contract, bootstrap and scene generator consume the catalog.
- **Independent review:** initially rejected three regressions: PlayerSpawn was incorrectly replaced by House; legacy zone IDs were overwritten by spatial IDs; a rectangular lake collider was introduced. The implementer corrected all three. Root verification confirms PlayerSpawn remains `(24,3)` with `farm_zone_player_spawn`, all six catalog-backed zone calls preserve legacy IDs, and `FarmSpatialLakeCollider` is absent.
- **Static validation:** `git diff --check` exit 0; source inspection confirms the contract has 11 unique footprints and all catalog call sites import the correct namespace.
- **Build validation:** root reproduced Runtime build exit 1: generated `CindarsHope.Runtime.csproj` does not include the new source file, producing ten `CS0103 FarmSceneSpatialContract` errors. `Assembly-CSharp.csproj` also fails because its generated asset reference is missing; `Assembly-CSharp-Editor.csproj` does not exist. These are local generated-project/environment failures, not accepted as a build pass.
- **Unity validation:** NOT RUN. Configured Unity executable is absent; no import, regeneration, menu validator, EditMode or Play Mode result exists.
- **Decision:** dependency source is usable for the next implementation, but this spec remains `PARTIAL` until a real Unity import/regeneration and its validators/tests run.
- **Residual risk:** actual generated scene may still diverge from the catalog, including footprint/collider mapping; the next physics spec must not claim this is already proven.

### 2. spec_farm_scene_collision_navigation_v1

- **Implementation evidence:** `FarmSceneNavigationPolicy`, `FarmSceneNavigationRaster`, a scene navigation validator, focused tests and a human scenario were created. The FarmScene generator now materializes a `FarmSpatialCollision` root after the visual terrain.
- **Independent review:** two regressions were caught and corrected. First, lake/river/mountain were declared as rectangles while the report claimed polygons; they now use canonical polygons and force `PolygonCollider2D`. Second, the builder created duplicate blockers around buildings; it now materializes only Lake/River/Mountain while buildings retain their existing owner colliders and approach requirement.
- **Static validation:** `git diff --check` exit 0. Root inspection confirms the bridge is omitted from both river collision paths; `Collision_<id>` is produced only for Lake/River/Mountain; the policy separates terrain materialization from existing building colliders.
- **Build/Unity validation:** NOT RUN/PARTIAL. Root reproduced .NET failure caused by stale generated Unity projects that omit the new contract source. Unity executable remains unavailable; no scene generation, editor validator, EditMode result or Play Mode result is claimed.
- **Residual risk:** polygon points were source-reviewed but have not been observed in a regenerated scene; door approach and Physics2D composition need Unity validation.

### 3. spec_farm_scene_keyart_macro_composition_v1

- **Implementation evidence:** composition contract, validator, focused tests and human scenario were created. Generator scale values are read from the contract.
- **Independent review:** initially rejected because the bridge scale was only centralized and still `(4.6, 2.0, 1)`. Corrected to visual-only `(1.8, 0.8, 1)` while the physical bridge footprint/corridor remains untouched; homestead roof target width is now 9 (was 11). The report distinguishes real visual changes from catalog-only groups.
- **Static validation:** `git diff --check` exit 0; root verified the generator consumes the updated bridge scale and tests assert the new values.
- **Build/Unity validation:** NOT RUN/PARTIAL due the previously recorded stale generated project and unavailable Unity executable. No capture validates the resulting composition yet.
- **Residual risk:** all remaining composition judgments (tree mass, field reading, lake balance and animal-row spacing) require regenerated-capture review.

### 4. spec_farm_scene_organic_terrain_water_v1

- **Implementation evidence:** `WorldTilemapGround` now exposes deterministic polygon rasterization and an eight-neighbour transition ring; a terrain-mask validator and focused EditMode tests were added. The generator paints lake and river from the canonical spatial polygons and paints the four main paths as polygons.
- **Independent review:** the first delivery still retained calls and definitions for legacy rectangular `CreateRiverSegment` / `CreateLakeBody` helpers. The root rejected that ambiguity. The corrected generator contains only `PaintWaterFootprint` for water and `PaintPath` for paths; a source scan for both helpers and `PaintWater` / `PaintShoreRing` call sites in the generator returns no results. A shared-generator truncation occurred during that correction; root restored the exact tracked base and had specs 1–3 reapplied serially before the terrain slice. The foundation-zone, bridge-scale and terrain-collision anchors were then rechecked alongside the corrected terrain calls.
- **Static validation:** `git diff --check` exit 0; root verified the lake and river use `FarmSceneSpatialContract`, all four path routes call the polygon helper, and no legacy rectangle painter is called by the generator.
- **Build/Unity validation:** NOT RUN/PARTIAL. The known stale generated Unity project still omits `FarmSceneSpatialContract`, so .NET cannot validate this source set; the configured Unity executable is unavailable. No scene regeneration, terrain menu validation, EditMode execution, capture, or bridge traversal proof exists.
- **Known art debt:** no authored grass-dirt or water-rock edge sprites exist. The deterministic rings temporarily use existing dirt/rock tiles; no PNG, meta, scene or asset was modified.
- **Residual risk:** actual Tilemap cell alignment, visual shore quality, overlap order and physical/visual bridge correspondence require a real Unity regeneration and capture.

### 5. spec_farm_scene_landmarks_and_agriculture_v1

- **Implementation evidence:** a pure `FarmLandmarkCompositionContract`, landmark validator, focused test and human scenario were added. The generator paints central soil from the canonical crop polygon, makes four `Visual_CropRow_*` roots, and adds visual-only props below existing functional roots.
- **Independent review:** root confirms the crop rows contain only `GameObject`/`SpriteRenderer`; no `FarmPlot`, `AnimalReleaseHandler`, `FishingSpot`, collider, save change or new gameplay system was added. The seven-landmark inventory refers to existing functional roots plus named visual children. Editor-only root discovery uses `Object.FindObjectsByType`, not a runtime search.
- **Static validation:** `git diff --check` exit 0; root reviewed the composition helper, contract, validator and tests. The earlier stale-project .NET build was rerun independently and remains exit 1 with the known ten missing-contract CS0103 errors.
- **Build/Unity validation:** NOT RUN/PARTIAL. Unity is unavailable; no regenerated scene proves positioning, approach counts, farming, animal release, Fonte de Anya or fishing behavior.
- **Residual risk:** visual local positions and the navigation raster’s sampled approaches need real-scene validation; the validator/human scenario must produce 7/7 and 10/10 before acceptance.

### 6. spec_farm_scene_biome_decoration_v1

- **Implementation evidence:** a pure seeded planner, decoration validator and focused tests were added. The generator creates one `FarmDecoration` root and materializes only `SpriteRenderer` children from the plan.
- **Independent review:** root verified FNV-style cell hashing with the named seed, all six biomes, a forbidden mask covering spatial water/solid/building/crop/trigger/bridge footprints plus path and approach masks, and no collider or gameplay component on decoration children. Existing art is reused; no PNG or asset was created.
- **Static validation:** `git diff --check` exit 0; same-plan, forbidden-cell and declared-range tests are present.
- **Build/Unity validation:** NOT RUN/PARTIAL due stale generated project and unavailable Unity. No validator menu result or keyart capture is claimed.
- **Residual risk:** actual materialized sprite resolution/counts, y-sorting, visual density and non-obscuring behavior require a regenerated capture.

### 7

- **Acceptance evidence:** a 16-check Play Mode route/8-criterion visual scenario and final execution report were created.
- **Independent validation:** `git diff --check` exit 0. All preceding feature specs remain PARTIAL and no acceptance criterion was promoted from a static scan.
- **Blocker:** configured Unity 6000.4.7f1 is absent and no Unity/Hub process was running. Unity 6000.5.7f1 was located but deliberately not used for a validation-only task because that would import/regenerate with an unconfigured version.
- **Decision:** `BLOCKED / NOT RUN`; run the documented scenario in the configured Unity version and attach the actual validator output and captures before promotion.

Entries are appended only after the predecessor has independent evidence.

## Validation policy

- Docs validation is rerun after documentation changes; pre-existing failures are distinguished from failures introduced by this wave.
- For every C# spec, the orchestrator reruns compile validation and checks the exact changed files and acceptance criteria.
- Unity scene generation, editor validators and Play Mode are never reported as PASS without their output/capture/checklist.
- A blocked validator or unavailable Unity is recorded as `NOT RUN` with command, reason and residual risk.
