# Farm house and measured prop bases — candidate handoff

2026-09-10. Root owns application, shared runtime integration and Unity validation. No live C# or metadata edited by this worker.

## Apply scope

1. `farm-house-methods.patch`: changes only `CreateFarmWalkInHouse` and `CreateFarmHouseDoor` in the Farm scene creator. Depends on root's `FarmHouseDoorArtAuthoring` and runtime candidate `ConfigureFeetOccupancy`/door presentation API.
2. Copy `FarmHouseInteriorAuthoring.cs.txt` to Editor/Art as `.cs` during the same integration. The two method files are also delivered separately for exact method replacement if adjacent creator edits invalidate diff context.
3. Optional `farm-settlement-bases.patch`: only `Prop`, `PlaceKeyartBarrel` and new private `CreateMeasuredPropBase`. Fence renderer/plant distribution untouched. Mixed line endings in the live composer require `git apply --ignore-space-change`; its read-only `--check` passed with that flag. House patch passed ordinary `--check`.

## Physical room

- Existing house root/IDs remain `(15.3,13.5)`, width8.5. North wall stays16.875. Exterior staircase support stays10.125.
- Front wall and door use `FarmHouseDoorArtAuthoring.DoorGroundY` (~11.597). Side walls span this new south edge to the existing north edge. Wood floor uses inspected64×64`wall_plank` tiled with its pivot compensated.
- Useful reveal box is inset0.11u from each wall centerline. It is passed to `ConfigureFeetOccupancy`; interactions outside do not define occupancy.
- Bed remains `Bed`/`BedInteractable`, foot(12.7,14.0), uniform2.2u height, solid rectangle x11.825..13.575/y14.08..15.48.
- Chest remains `FarmHouseChest`/`FarmHouseChestInteractable`, foot(18.1,15.1), uniform1.1u height, solid rectangle x17.6..18.6/y15.14..15.62. Reuses the inspected128×115stone-cavern chest sprite only; no cave behavior.
- Bed and chest roots stay unscaled. Each has its original interaction component and separate trigger, plus a child WorldSolid base. Suggested front probes: bed(12.7,13.3), chest(18.1,14.4). Central room aisle remains open between furniture bases.
- BedLetter identity/component preserved; placed next to the bed at(14.35,14.2), outside its solid base.

## Door visibility and art correction

- New approved-source derivative `World/building/farmhouse_opening_keyart_v4.png` replaces the house roof sprite reference in the candidate.
- Canvas212×205 and all pixels outside x111..131/y146..164 remain identical to the original. Only those399pixels were cleared. Layered candidate: `animation/farmhouse_opening_keyart_v4.aseprite`; baseline hidden and preserved.
- Aseprite export was reopened and compared: every removed RGBA pixel is restored by the closed leaf. Verification: `animation/open-facade-verification.txt`. Original farmhouse hash remains B8B90714711A195C3BA5D853C85A22D0A27D27B9D9ED60C65B3386A9E4983399.
- Exterior reveal list contains Roof and Threshold only. Leaf is excluded from both exterior and interior renderer lists, preserving closed-door visibility from inside. Root's helper must put Leaf on World/order0 and Threshold on World/order-1 as coordinated.
- Leaf/threshold are configured after their transforms and physical blocker exist; `CreateFarmHouseDoor` returns the interactable for explicit wiring.

## Optional settlement bases: 19 objects

- Crates: PottingSupplies, HouseSupplies, HouseSuppliesSmall, GreenhouseSupplies, FieldNorthCrate, FieldNorthSmallCrate, FieldSouthCrate, CheeseSupplies, ShedSupplies.
- Barrels: HouseWestBarrel_A, HouseWestBarrel_B, HouseEastBarrel_A, HouseEastBarrel_B, FountainWestBarrel, FountainEastBarrel.
- Troughs: PastureTrough, CoopTrough.
- Hay: BarnHayStack, BarnHayStackSmall.

All use small bottom contact rectangles from the inspected source silhouettes. Existing visual coordinates, uniform scales and interaction anchors remain unchanged. No colliders added to flowers, low growth, flat stones or incidental fence props. Existing potting bench collider is preserved. New rectangles require root's actual approach/collision probes before acceptance; static placement is not proof of traversability.

## Validation

- Source pixel/alpha/recomposition assertions: PASS.
- Scoped patch applicability checks: PASS as described above; no patches applied by worker.
- Unity validation: NOT RUN. Reason: candidate-only ownership; root sole launcher. Command attempted: none. Residual risk: Unity compile, renderer ordering, feet occupancy, full player collider routes and furniture interaction must be checked after integrated application.
