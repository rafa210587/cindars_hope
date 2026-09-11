# Refinamento — Farm contact, pixel consistency and fishing v1

> Status: PROPOSED — requested analysis; implementation not performed
> Baseline: Farm stage15 and ambient_runtime15, Unity6000.5.7f1
> Scope: Farm presentation/contact and existing fishing interaction placement

## Outcome

The player must read as standing on walkable ground/deck, never inside solid stone or water.
Water effects must not cover the player on adjacent walkable ground. Fishing should start at
the outer end of the pier, with feet safely on its deck. Terrain, actors and buildings should
share an intentional pixel scale and material treatment.

## Evidence and limits

- `CreateMvpFarmScene.CreateFonteAnya` renders the fountain as one animated sprite on World/order0,
  with Pivot sorting and source-derived scaling to7world units. Its trigger is5.8×2.4;
  a separate `SolidBasin` exists. Therefore “there is no fountain physics” is not established.
- `FarmSettlementPhysicsContract.FountainBasin` is a4.6×1.6rectangle centered(-28,4.25).
  A single rectangle does not by itself prove correspondence to the basin/column contact shapes.
- `CameraTransparencySort2D` uses the Y axis. A sprite pivot, sorting layer/order and the player's
  foot reference all affect occlusion. Always drawing the entire player above every tall prop
  would break behind-object depth; the requested priority should apply to water/ground effects.
- `FarmAmbientAnimationAuthoring` preserves existing source collider/sorting setup; its PASS
  never asserted that the preexisting setup matched the animated visible contact regions.
- Native41second observation validated frame progression and timing, with repositioned camera;
  no player movement around these props was simulated. Previous BFS routes also do not certify
  all visual contacts between a player's upper body and a prop.
- `FarmLevel1LayoutContract` defines fishing(13.56,-8.75), dock land anchor(13.56,-6.75).
  Fishing is not at the outer end of the enlarged deck. `CreateFishingSpot` parents dock/boat
  visuals to the interaction root and installs a small four-sided interaction ring.
- Independent source audit: fishing root scale6 makes outer half-extents0.16 become0.96world
  units. Its ring does not reach the deck/notch south end at y=-10.798. Preserve visual world
  transforms when decoupling or moving this scaled interaction root.
- Fountain source center pivot versus authored support(85,43) gives an approximately1.865world
  unit vertical sort-reference offset. This is a confirmed registration mismatch to inspect
  against foot sorting, not proof of every reported overlap location.
- Stage15 animal-region capture visibly mixes fine, noisy terrain/road texture with softer,
  broader building shading. This is a visual finding, not a measured numerical pixel-density
  audit. Pixel-size ratios and source PPU/transform/camera contributions still need measurement.

## Proposed decisions

### D1 — Separate support, obstruction and visual height

Use existing scene composition APIs and spatial contracts. Measure the opaque ground-contact
outline of the fountain basin, column feet and waterfall rock ledge against the player foot/body
collider. Correct only mismatched solid footprints; triggers stay interaction-only. Do not create
colliders on animated water pixels, ripples or the jumping fish. Existing lake/river solids should
block access to water independently of their animation frames.

For each suspect edge, record four cardinal and four diagonal approaches, pushing against the
solid with the actual player body. Capture both collider overlay and unobstructed game view.
Check no entry, no sticking at joins, reachable fountain interaction and unobstructed respawn.

### D2 — Explicit draw-order contract

Ground/deck/water/foam: below player. Solid structures: depth by foot/contact reference.
Where one sprite mixes the basin, tall columns and water jets, split into stable base/body and
animated water layers only if contact captures show a single pivot cannot represent the overlap.
All animation frames keep registered canvas/pivot. Avoid an arbitrary globally higher player
sorting order or a collider covering the full rectangular image including empty space/height.

Acceptance: front-of-basin player remains visible; behind tall stone, intentional occlusion
is stable; no water foam covers feet on dry land; deck remains under feet. Check every used frame.

### D3 — Move fishing interaction, preserve the pier

Separate the interaction anchor from visual layout ownership before moving it. Keep world
positions of pier, boat and lake collision notch unchanged; retain `farm_spot_7_-10` and fishing
tables/save semantics. Place the fishing stance at the center of the outer deck lip, inset by
the actual player's front extent plus a small clearance. Determine the final coordinate from
the visible deck and full-body collision; do not equate decorative post tips with walkable deck.

Replace the four-sided ring with a narrow, reachable end-of-pier interaction region supported
by the existing interaction path. Define a separate cast target in the water if current VFX or
rod direction needs one. It is not a second fishing spot or a new reward system.

Acceptance: no fishing prompt at the land entrance or deck midpoint; prompt and successful cast
at the tip; feet remain dry; adjacent sides/water remain blocked; movement can approach/leave;
existing tool/stamina/weather/minigame rules still apply. Test real interaction, not only position.

### D4 — Normalize pixel treatment before adding detail

Create one small comparison board at the SAME gameplay camera zoom: player, house corner,
road/grass transition, fountain and dock. Record source canvas, import PPU, total world scale,
screen pixels per source pixel and visually authored cluster size. Equal importer PPU alone
does not ensure equal perceived pixel density or fix softly shaded generated source art.

Use the approved player silhouette and selected building material as the reference pair;
resolve that pair first if they conflict. Keep building proportions already improved. Reauthor
or bake candidate sprites onto a consistent native grid; use deliberate clustered shading and
limited material ramps. Remove isolated terrain noise, align light direction, unify outline and
contact-shadow strength. Point/None/no-mips remain required but are insufficient by themselves.

First candidate: one road/grass patch plus one building corner. Inspect at1× and integer zoom.
Do not regenerate the whole keyart, enlarge all sheets or apply a destructive blanket palette.
Use Aseprite for local edits and source control. GPT generation is for a missing structural
asset, followed by grid/palette cleanup, not repeated global style regeneration.

### D5 — Additional focused checks

- Pier posts/rails versus decorative deck: no walking through a visibly solid front post; no
  invisible barrier extending across the intended approach.
- Props in southern building gaps: move clutter only where it masks or blocks useful access.
- Foam at river/lake seam: no effect spilling onto traversable land; preserve bank continuity.
- Fountain respawn: solid correction must not spawn the player inside the basin or a column.
- Vegetation classification: low ground cover remains passable; trunk/rock support remains solid.
- Sparse animation evidence: null-sprite samples every5s do not prove every intermediate frame;
  retain native clip checks and visual movement evidence as different claims.

## Execution slices and cost control

1. Contact/sorting/fishing slice: establish annotated captures, implement confirmed geometry and
   interaction fixes together, run one targeted PlayMode scenario covering contacts and fishing.
2. Pixel consistency slice: approve the local1× candidate visually, then extend the same treatment
   to priority roads/buildings. Do not rebuild unaffected terrain or change gameplay scale.
3. One integrated before/after review. Reuse unchanged animation timing/layout tests; rerun only
   affected contracts and interaction behavior. Stop after two unsuccessful visual candidates
   and reconsider the art reference instead of adding more rules or parallel generators.

Root orchestrates; one bounded executor per slice and one independent reviewer. Unity requires
shared-source freeze. No runtime edits, tests or new Unity run occurred for this refinement.
Full farm/keyart approval remains pending. The next step is a scoped executable spec carrying
these criteria; exact collision outlines, tip stance and pixel ratios come from baseline capture.
