# TOWN.V1 — Composition execution report

Spec: `town_keyart_fidelity_v1`  
Date: 2026-09-11  
Owner slice: Town composition, ground, water, vegetation and props

## Baseline and scope lock

The working tree was already dirty before this slice (including TownScene, Town creator/layout,
Town tests, generated town art and unrelated Farm/Cave/runtime work). Existing edits were preserved.
The seven composition sources were fingerprinted at start; no prior diff was reverted:

```text
TownCityLayout.cs       9E6D2C8C9E7CE4E0D5E0203181754632EC4A66DE39EDD371AEC6FA5A78122655
TownDistrictLayout.cs   0966D5D5A45CCD2C259D7E0F7F6D10DABC03895774B647B72E56905E80406933
TownKeyartGround.cs     AC258EA501EF237E4910929CCCBB108CDB08C778847F78A78DCE994D934C559D
TownKeyartGeometry.cs   469C285DA36D15F6876B78DA68EADD1509A0BE78D260685281266AECF1FA23BA
TownKeyartSetDressing.cs C1E250BFC9DAE7F9BAA015D10FEBE9C6CF7163DCE5912FF877927972BCE719FF
TownKeyartSceneArt.cs   D7598A8CA437A6BFEDF176B1F9EDF4615944CD22BCBD5AD7A6A5BA2C3640CE09
CreateMvpTownScene.cs   EC51D940453D1A955CE4CA8E40ED3E7857E82987824EF346C4D8C7EF563B2B13
```

Allowed scope was limited to those composition sources, Town-only prop/foliage/water assets,
canonical Town scene regeneration, Town composition tests/validators, and this report. Building,
facade, roof, door, interior, NPC runtime, shared building accessors, Farm/Cave, save and global
registries were not edited by this slice.

## Existing systems audit

The current implementation reuses `WorldTilemapGround`, `WorldSpriteLibrary`, the canonical Town
creator, and the existing Town geometry/scene-art helpers. Ground and water are rasterized through
Tilemaps at the half-unit Town grid; decorative props remain separate from physical colliders.
No second runtime system or background/keyart image was introduced.

## Compliance matrix

| Requirement | Evidence | Status |
|---|---|---|
| Organic road/plaza composition | `TownKeyartGeometry` routes and `TownKeyartSceneArt` Tilemap composition | IMPLEMENTED IN SOURCE |
| Lake/river geometry and clearance contract | `TownKeyartGeometry` water polygons and `CreateWaterDistrict` audit | IMPLEMENTED IN SOURCE; scene audit pending |
| District set dressing / vegetation / props | `TownKeyartSetDressing` and `TownKeyartSceneArt` | IMPLEMENTED IN SOURCE |
| Preserve building/NPC/door/interior contracts | No changes made outside ownership lock | PRESERVED BY SCOPE |
| Town automated layout contracts | `TownLayoutTests` present and compiled | COMPILED; Unity execution pending |
| Canonical scene regeneration and real capture | Requires Unity editor generation/capture | NOT RUN |

## Validation

### Passed

- `dotnet restore CindarsHope.Editor.csproj --nologo` — exit 0.
- `dotnet build CindarsHope.Editor.csproj --no-restore --nologo` — exit 0, 0 errors (warnings are
  existing project warnings, including deprecated Unity APIs).
- `dotnet restore CindarsHope.Tests.EditMode.csproj --nologo` — exit 0.
- `dotnet build CindarsHope.Tests.EditMode.csproj --no-restore --nologo` — exit 0, 0 errors.
- `git diff --check` over the owned composition sources — no whitespace errors.
- Town tile/pattern assets required by the source contracts exist under the Town prop asset scope.

### Earlier checkpoint — Unity/editor and PlayMode

The initial checkpoint below was superseded by Pass 3 after the root requested concrete visual
changes. It remains recorded to preserve the phase history.

Reason: Unity processes currently running on the machine belong to `D:\Projetos\Jogos\game_semnome\Unity`,
not this project. The canonical `CindarsHope/Inicializar Projeto` command recreates all three scenes
and global data, which is outside this slice's lock; the Town creator's direct execute method would
bypass the project's three-command generation orchestration. No Unity process was terminated.

Command attempted: none (unsafe/out-of-scope canonical mutation avoided).

Residual risk: Unity import, scene materialization, collider-after-Awake behavior, fixed 1536×1024
capture, Town validator counts, and PlayMode visual/physics evidence are not freshly validated by
this slice. Source compilation does not establish visual acceptance or CA-1/CA-5/CA-9/CA-10/CA-12.

## Honest status

## Pass 3 concrete composition delta

The subsequent implementation pass changed only the owned Town composition sources and regenerated
the Town scene through the Town-specific creator:

- Added four animated, collider-free lake ripple accents and native Animation loops for water ripple,
  Town water-wheel marker and Town lamps. No collider-bearing root is animated.
- Added irregular lake-bank masses and north/west stone-bank segments with authored gaps and jitter;
  physical perimeter remains unchanged.
- Derived the corral fences, animals and hay from `House_AnimalYard`'s current lot bounds instead of
  stale fixed coordinates, preserving the south gate opening.
- Added a deterministic temple-door/stair sightline cleanup that only disables loose Town decoration
  overlapping the temple apron; building, door and collider roots are untouched.
- Preserved the existing radial plaza ring, curved road geometry and pocket gardens, while adding
  the missing visual bank/corral support around those contracts.

### Fresh evidence

- Town-only generation: `Logs/town_v1_pass3_generate.log`, exit 0. Audit: 24 houses, 29 NPCs,
  84 anchors, 23 stalls, 2 spawns; `clearance=0 lote-lote=0 lote-via=0 porta-sem-acesso=0`.
- Town EditMode filter: `Logs/town_v1_pass3_editmode.xml`, `total=24 passed=24 failed=0`.
- Town keyart validator: `Logs/town_v1_pass3_keyart_validator.log`, `TownKeyartPhysics PASS:187
  FAIL:0`, `TownAccess PASS:8 FAIL:0`.
- Schedule validator: `Logs/town_v1_pass3_validator.log`, `PASS:21 | FAIL:0`.
- Saved-scene orthographic capture (1536×1024, ortho58):
  `art/town-keyart-rework/evidence/revision-03-pass3/town-overview-ortho58.png` and detail crops.
  This is saved-scene evidence, not PlayMode evidence.

Current status: `PARTIAL` / `UNITY_VALIDATED` for the scoped composition slice. The capture visibly
shows the radial plaza, curved approaches, irregular river/lake bank, denser pockets and active
corral. Independent score and human PlayMode traversal remain outside this execution report; no
global acceptance or spec promotion is claimed.

## Pass 4 material composition source delta (Unity intentionally pending)

After an independent visual review rejected the small pixel delta, the source pass was extended
without Unity regeneration:

- `MaterialPocketPolygons` now defines eight broad, irregular ground clusters with an explicit area
  contract; `CreateMaterialGround` paints three distinct existing grass materials through Tilemaps,
  filtered against lots, water and swept roads.
- Every other cluster receives a low-wall fragment, making pockets read as designed neighborhood
  spaces rather than isolated decorations.
- `CreateMarketDepth` adds a second material layer of canopies/crates to the west market.
- `BuildRockBanks` now adds a visible cascade spill and stronger south-gate material pillars, while
  retaining the physical perimeter and portals.
- Geometry tests now enforce minimum materialized ground area, lake radial irregularity and a minimum
  curved-road ratio. These are spatial/material contracts, not prop-count scores.

Unity generation, captures and Unity tests are intentionally NOT RUN for this pass because TOWN.A1
holds the Unity lock. The next serial owner must run the Town-only generation, the new geometry tests,
the saved-scene validator and a fresh ortho58 capture before updating the status above.

## Pass 5 material contract and serial evidence

This pass corrected the P1 measurement findings before the serial Town run:

- Material painting and tests now share `MaterialPocketPointAllowed`; effective area is raster-cell
  area after ClearPoint, water, lot and road filters, not nominal polygon area. The GateKeeper pocket
  (x31..46/y-47..-38) is explicitly required to contain both accepted and rejected cells.
- The scene validator measures the actual `Tilemap.HasTile` cells and compares their area to the
  effective geometry contract, reporting `actual=502.75`, `expected=502.75`, `invalid=0`.
- Decorative `MaterialPocketWall_*` objects are required to have no enabled solid collider, while
  `SouthStoneWall_*` objects are required to retain enabled physical support colliders.
- Validator occlusion checks cover market depth props, the cascade spill and south-gate pillars;
  the gate route remains present at the authored portal center.
- The road curvature metric now groups and orders sampled segments by route prefix, so dense smooth
  bends cannot be misclassified as straight merely because joins are interleaved.

Fresh evidence:

- Geometry EditMode: `Logs/town_v1_material_editmode3.xml`, total=23, passed=23, failed=0.
- Town-only generation plus saved-scene validators: `Logs/town_v1_material_generate3.log`; scene
  generation, `TownKeyartPhysics PASS:194 FAIL:0`, `TownAccess PASS:8 FAIL:0`, and schedule
  `PASS:21 | FAIL:0` completed before capture.
- Standalone saved-scene validator: `Logs/town_v1_material_validator.log`,
  `TownKeyartPhysics PASS:194 FAIL:0`, `TownAccess PASS:8 FAIL:0`.
- The requested fresh ortho58 render was attempted in a Town-only entrypoint and in the existing
  capture method. Unity crashed in `Camera.Render` under `-batchmode -nographics` before writing
  `revision-04-material-pass/town-overview-ortho58.png`; no prior A1 evidence was overwritten.

Current status: `PARTIAL` / `UNITY_VALIDATED` for source, generation, EditMode and saved-scene
validator contracts. Capture visual acceptance remains `NOT RUN` because of the Unity render crash;
PlayMode remains NOT RUN. Residual risk is limited to fresh visual comparison and runtime traversal.
