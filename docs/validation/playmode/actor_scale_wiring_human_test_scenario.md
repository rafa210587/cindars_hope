# Human Test Scenario — Actor & Prop Scale Wiring

> Runtime/scene change. Automated EditMode tests cover the pure scale math
> (`EnemyScaleResolverTests`); this scenario covers the scene/Play Mode behaviour that
> cannot be asserted headless. Status until executed: **BUILD_VALIDATED_WITH_WARNINGS**
> (Play Mode + EditMode run deferred to a machine with Unity).

## What changed

- Player & NPC now size from the authored `VisualScaleProfileSO` (VisualScale **2.0**) via
  `VisualScaleApplicator`, instead of a hardcoded `localScale (1, 1.5)`.
- Cave **bosses** now honour their per-boss `EnemyDataSO.VisualScale` (a Gargantuan boss is
  no longer flattened to the global 2.5). Boss **adds** and the legacy `CaveEnemySpawner`
  are now data-driven (scale + collider by size class) instead of hardcoded 1.0 / 0.4.
- Farm/Cave gameplay **props** (resource tree/rock, forage, shipping bin, contract board,
  crafting stations, sell point, pickup, portals) route through the scale profiles.
  Decorative multi-part buildings (houses, fountain, statue, market stall, greenhouse,
  animal housing, lake, bed, cave-entrance composite) **keep their authored scales** —
  uniform VisualScale would break their intentional non-uniform proportions.
- Camera unchanged (Cave orthographic size stays 7.0, by decision).

## Size language (player-relative)

All sizes are now declared as a multiple of the player in `CreateDefaultScaleAssets`
(`PlayerReferenceScale = 2.0`). Bump that one constant and the whole world rescales; change one
ratio and only that prop class moves. Expected on-screen sizes (× player):

| Entity | × player | Absolute VisualScale |
|---|---|---|
| Player / NPC | 1.00 | 2.0 |
| Pickup | 0.45 | 0.9 |
| Forage point | 0.55 | 1.1 |
| Resource rock / station / sell point | 0.65–0.75 | 1.3–1.5 |
| Chest | 0.75 | 1.5 |
| Contract board | 0.72 | 1.44 |
| Shipping bin | 0.90 | 1.8 |
| Checkpoint portal | 0.90 | 1.8 |
| Cave portal (landmark) | 1.10 | 2.2 |
| Resource tree | 1.20 | 2.4 |
| Decorative tree (medium) | 1.50 | 3.0 |
| Boss | per-boss (data) | 2.0–~7.5 |

`CindarsHope/Validate/Actor Scale Wiring` prints each category's "× player" ratio, so this table
is verifiable from the console after generating the assets.

## Pre-steps (Editor, in order — REQUIRED before Play Mode)

1. Open the project in Unity (let it recompile; the generated `.csproj` will re-include all files).
2. Menu: **CindarsHope/Generate/Data/Create Default Scale Assets**
   - Re-runs the now-idempotent generator: creates the 5 new profiles
     (`resource_tree`, `resource_rock`, `forage_point`, `shipping_bin`, `contract_board`)
     and updates station/pickup values. Confirm console: assets created/updated, no errors.
3. Menu: **CindarsHope/Validate/Actor Scale Wiring**
   - Expect `[ScaleWiring] PASS` (0 errors). Any "No VisualScaleProfileSO for category…"
     means step 2 did not run.
4. Regenerate the three scenes via their CreateMvp* menu entries (Farm, Town, Cave), then
   save. (These scene creators are what bake the applicator onto the actors/props.)
5. Run the **EditMode tests** (Window ▸ General ▸ Test Runner ▸ EditMode):
   `EnemyScaleResolverTests` (4 boss-scale cases + 6 collider-radius cases) must be green.

## Play Mode checks

### Farm scene
- [ ] Player is visibly larger than before (~⅓ taller) and reads as ~12% of screen height.
- [ ] Resource tree is clearly taller than the player; resource rock ~knee/waist height.
- [ ] Crafting stations (workbench/forge/cooking) read as waist/chest-high props, NOT
      player-sized towers.
- [ ] Pickup, forage points, shipping bin, contract board, sell point all visible and
      interactable (walk up, prompt appears, interaction still works).
- [ ] Town/Cave portal on the farm is a prominent gate, not a small tile.
- [ ] Houses / fountain / decorative props look unchanged (no squashed/stretched buildings).

### Town scene
- [ ] NPCs are the same size as the player (both VisualScale 2.0).
- [ ] NPC dialogue/shop interaction still triggers at the expected distance.

### Cave scene
- [ ] Player size matches the other scenes.
- [ ] The placeholder Slime reads as smaller than the player.
- [ ] Spawn into a level with a **boss**: the boss is visibly larger than normal enemies;
      a large-size-class boss is clearly bigger than a medium one (no longer all the same).
- [ ] Boss adds (if the boss summons) are sized like normal enemies of their class, not
      a flat 1.0.
- [ ] Normal enemies of different size classes (tiny vs large) are visibly different sizes.
- [ ] Contact-damage and hit detection still work (colliders scaled with the sprites).

## Regression watch
- [ ] No `[ScaleProfileLibrary] No VisualScaleProfileSO for category…` errors in the console
      during scene generation (would mean a category lacks a profile asset).
- [ ] Player movement, collisions and interaction radii feel correct at the new size
      (no walking through walls, no unreachable interactables).

## World spatial scale (follow-up)

After the player grew (1.5 -> 2.0), the world's *spatial* feel was reviewed separately from on-screen size:

- **Cave corridors** — the only genuine functional issue. 1 tile = 1 world unit; the scaled player
  collider is ~1.2 wide and filled ~60% of a 2-tile corridor. Fixed: `CaveGenerationConfigSO`
  `CorridorMinWidth` 2->3, `CorridorMaxWidth` 3->4, `GenerationConfigVersion` 2->3 (the documented
  way to invalidate old snapshots — new runs regenerate, in-progress snapshots are untouched).
  - [ ] Play Mode: enter a cave, walk corridors — player + one enemy fit comfortably, no wall clipping.
- **Cave levels** — already 160x96 tiles (2x, SPEC 17A). Not grown; the player is tiny relative to a level.
- **Farm / Town** — **intentionally NOT grown.** `CreateBounds` documents the farm was deliberately
  reduced 40x34 -> 28x22 "to reduce empty space". Growing it would reintroduce that empty space and
  fight an explicit design decision; the larger player in a compact farm matches the cozy/dense intent.
  If it still reads cramped in Play Mode, prefer a per-scene camera tweak over rescaling coordinates.
- **Lakes / rivers** — cosmetic; left as-is.

## Notes
- Prop VisualScale values are tunable in the `Assets/_Game/Data/Scale/VisualScaleProfile_*.asset`
  files — adjust there (then re-run the scene generators), never back in scene code.
- The two scale systems are intentionally disjoint: enemies use `EnemySizeProfileSO`
  (SPEC 13); player/NPC/props use `VisualScaleProfileSO` (SPEC 17A). This change wires the
  latter (which existed but was never applied) and closes the enemy/boss gaps in the former.
