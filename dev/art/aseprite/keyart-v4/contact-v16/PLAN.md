# Contact and fishing v16 — offline technical proposal

Status: proposal for the executable spec; no Assets or Unity changes. Root owns final scope,
source release, generation and validation. Basis: `ref_farm_contact_pixel_consistency_fishing_v1`.
This folder contains an opt-in runtime candidate, not accepted geometry or evidence of Play behavior.

## Smallest implementation

1. Keep FishingSpot at its existing transform and stable ID. Give it a separately positioned,
   authored child trigger at the measured deck tip. Dock, boat and lake notch remain untouched.
2. Add child FountainDepthGroup at local zero/identity/unit scale under FonteAnya, with native
   SortingGroup World/order0. Reparent only Visual into it with worldPositionStays=true.
   Preserve all pixels, pivot and five frames; flowers stay outside the group and never move.
3. Change fountain solid geometry only after mapping basin/column ground-contact pixels and the
   actual player's body collider onto the same world coordinates. No numerical replacement
   for FountainBasin is proposed before that measurement.

## Files and minimal signatures

| File | Planned delta | Constraint |
| --- | --- | --- |
| `Scripts/World/FishingSpot.cs` | Serialized optional `_useStanceZone` and `_stanceZone`; `public void ConfigureStanceZone(BoxCollider2D zone)`; private `IsInteractorInStanceZone(GameObject interactor)` | Default false preserves all legacy spots. Null passed explicitly restores legacy. Missing/disabled zone after opt-in rejects interaction. |
| `Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Replace farm's hidden root discovery trigger/four ring triggers with one child stance trigger; call ConfigureStanceZone. Add FountainDepthGroup child and reparent only Visual with worldPositionStays=true; flowers stay put. | No movement of FishingSpot root, dock, boat, fountain Visual, RespawnPoint or scene IDs. No new runtime component. |
| `Scripts/Farm/FarmLevel1LayoutContract.cs` | Add final measured fishing stance center/size constants, separate from existing FishingSpotX/Y and DockEntranceX/Y. | Existing anchors/IDs stay fixed; no provisional coordinates promoted. |
| `Scripts/Farm/Scene/FarmSettlementPhysicsContract.cs` | Revise FountainBasin or add measured solid parts only if capture proves mismatch. | One shared geometry source for colliders and tilling masks; no full-sprite AABB. |
| `Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs` | Only needed if confirmed fountain ground support needs multiple parts; iterate the same parts as creator. | No save schema, grid identity or unrelated zone changes. |
| `Tests/EditMode/World/FishingStanceZoneTests.cs` | Focused optional-zone behavior fixture. | Legacy tests remain applicable; do not alter loot/weather/minigame implementations. |
| `Scripts/Editor/Art/FarmAmbientAnimationAuthoring.cs` | Line63 lookup becomes `Find("FountainDepthGroup/Visual")`. | No import, pivot, asset ID, frame timing or controller changes. |
| `Scripts/Editor/Dev/FarmAmbientPlaybackCapture.cs` | Line71 lookup becomes `Find("FountainDepthGroup/Visual")`. | Resolve new explicit hierarchy; no playback rerun required solely for this path change. |
| Existing farm contact/interaction probes | Add only the measured stance/edge checks and actual contact movement samples. | Do not rerun 41-second timing evidence or 18 static views merely because contact changed. |

All `Scripts/` and `Tests/` paths above are under `Assets/_Game/`. They are future scope, not files
modified by this proposal. The two ambient consumers need only the explicit lookup change.
`FarmPhysicalRouteProbe.cs:425-432` uses the FonteAnya root and recursive AnyaFountain lookup,
not Find("Visual"); no hierarchy fix is needed there. Its geometry samples change only if measured
contact geometry changes. The inspected FontePhysicalTests and FarmDoorAndWaterProbe do not use
the fountain Visual direct-child lookup either.

## Fishing candidate contract

`FishingSpot.cs.txt` is a full offline snapshot of the current live source with these small changes.
`candidate-source.json` records its live baseline hash. Rebase its changes if live source moves;
do not blindly overwrite a concurrently modified file.

- Stance is an enabled, active BoxCollider2D trigger descending from the same FishingSpot, so
  InteractionSystem's existing GetComponentInParent<IInteractable> discovery resolves correctly.
- Use zone.OverlapPoint(interactor.transform.position), matching the current player root/foot
  reference. The authored zone describes permitted FOOT positions, not full-body overlap or
  an enlarged interaction sensor. Validate root-at-feet against the actual generated actor.
- CanInteract requires inventory plus stance when opted in. Interact also enforces stance before
  tool feedback, stamina spending, coroutine start or confirmation, including direct callers.
- Default legacy ring behavior and `_requireEdgeInteraction` remain unchanged for other spots.
- The zone center/size are world measurements. FishingSpot still has scale6: convert world center
  with InverseTransformPoint and world size by lossyScale, rather than accidentally multiplying by6.
- Remove the old farm ring trigger creation, not shared legacy runtime support. The new stance
  trigger is sufficient for existing candidate discovery; verify actual selection at the tip.
- No cast-direction/target field until an existing consumer needs it. No new reward or rod system.

The current actor stance at (13.56,-8.75) and outer half-width .96u are baseline evidence only.
The notch ends at y=-10.798; this is a collision boundary, not automatically the correct foot
coordinate. The final stance must be inset from the visible walkable lip by the actual body's
directional extent plus measured clearance. Decorative posts do not define walkable floor.

## Fountain sorting: preferred native group

The existing renderer uses a central pivot, while support pixel (85,43), origin bottom-left,
is aligned to FonteAnya. Current source scale is 7/137 world units per source pixel; the central
sort reference is therefore (79.5-43)*7/137 =1.86496u above the support. SortingGroup on a local-zero FountainDepthGroup
can use the existing support transform without moving pixels or rebuilding persistent frames.

Unity documents group distance using the transform carrying the group; its children render as
one group. This is compatible with the live World0 player and CustomAxis/Y camera. Sources:
[Unity Sorting Groups](https://docs.unity.cn/Manual/class-SortingGroup.html) and
[Unity 2D Sorting](https://docs.unity.cn/6000.0/Documentation/Manual/2DSorting.html).
Installed Unity6000.5.7f1 CoreModule XML also confirms descendant grouping and sortAtRoot API.

Concrete hierarchy safeguard: creator's six Visual_ClearingFlower_* children and later
Visual_AnyaClearing currently descend from FonteAnya. Keep them exactly where they are.
Create only FountainDepthGroup under FonteAnya with localPosition zero, localRotation identity,
localScale one; add SortingGroup World/order0 to that child; reparent only the existing Visual
under it with worldPositionStays=true after existing support alignment. The group then contains
only the fountain renderer. Colliders, RespawnPoint, FonteInteractable, flowers and root remain
unchanged. No marker, shared runtime lookup utility or new MonoBehaviour is necessary.

Update the exact two direct-child consumers listed above to FountainDepthGroup/Visual. Native
Animator remains attached to Visual and its sprite binding path stays empty, so reparenting
changes neither its binding nor any persistent frame reference. Add an asset/editor assertion
that the group's renderer descendants contain only the fountain visual and its world matrix,
sprite IDs, pivot and PPU match the pre-group baseline. Camera and player World0/Y configuration
remain unchanged.

If real front/behind contact still cannot be represented by one support group, review source
layer separation; do not patch animation frames or add global player-front rules. Direct pivot
replacement is only a fallback: it would require world-position compensation and a new authored
sprite-subasset version because existing BuildClip deliberately rejects changed pivot geometry.

## Required measurement before solid geometry

Record the current animated canvas and stone contact outlines at native pixel scale, separately
from jets/foam and empty alpha. Record player collider size, offset and transformed bounds;
confirm whether the root is the bottom/feet in each approach, rather than assuming a centered body.

For the currently registered fountain, convert source pixel (px,py) from bottom-left using:
`world = (-28,3.1) + ((px,py) - (85,43)) * (7/137)`.
For top-left image coordinates use bottomY =159-topY. Pixel centers and pixel edges differ by
half a pixel: use one convention consistently when annotating a contact boundary.
SortingGroup changes none of this mapping.

Compare the resulting basin and column ground support to current rectangle x[-30.3,-25.7],
y[3.45,5.05], using the player's actual full body against it. Sprite upper-body overlap alone
is not proof that the player's feet entered stone. A column's visible vertical shaft is not its
ground footprint. Water animations must not receive changing colliders.

For cascade contact, compare registered foam/ledge and actual body at the existing river/lake
union. The water solids already overlap at the confluence. Add no duplicate water collider merely
because the ambient SpriteRenderer has none. Confirm any bank spill against all five frames.

## Focused behavior checks

EditMode optional-zone cases:

- Legacy unset: inside-ring accepted, center/outside rejected; legacy requireEdge=false still works.
- Authored zone under a scaled parent: foot inside tip accepted, deck midpoint/entrance rejected.
- A body collider merely overlapping the zone while its foot root is outside is rejected.
- Disabled/inactive/destroyed configured zone rejects; invalid non-trigger/foreign zone setup rejects.
- Explicit ConfigureStanceZone(null) restores legacy behavior.
- Direct Interact outside authored stance changes no stamina/inventory/fishing state and publishes
  no successful-cast feedback; valid tip selection reaches existing fishing behavior.

One targeted real-player Play scenario after source freeze:

- Walk to tip using controller input, get live selected FishingSpot, cast with the existing rod,
  and confirm the existing timing/minigame action. Record tool/stamina/weather behavior separately
  where a condition is deliberately exercised. Never claim successful fishing from CanInteract alone.
- No prompt at land entrance or deck midpoint; safe exit from tip; no stepping onto water beside
  the lip or through a visibly solid post. Verify camera view plus actual body position/bounds.
- Fountain eight approaches (cardinal/diagonal), pressing against measured supports: no entry,
  no sticking, reachable interaction, clear respawn; game view plus matching collider overlay.
- Front/behind stone occlusion remains stable during all five animation frames; ground decoration,
  dock, foam and water remain beneath the actor on valid dry ground. Do not force sorting for probes.
- Reuse unchanged lake/river route and ambient timing results; rerun relevant mask/geometry checks
  only if physical geometry changed.

Unity validation: NOT RUN.
Reason: offline candidate/planning ownership; root has not released Assets or Unity.
Command attempted: none.
Residual risk: candidate Unity compile and runtime stance behavior are unvalidated.
