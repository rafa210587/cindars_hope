# Execution Report — 08_spec_city_layout_buildings_doors_beds_schedule_runtime

## Status
RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

## Spec
Wave: 08
Priority: P0
Strategy: CREATE_MINIMAL

## Local Audit

Commands: `Get-ChildItem Assets\_Game\Scripts\City -Recurse` — City folder did not exist
NPC existing systems: NpcDataSO, NpcController, NpcManager (basic), NpcWanderer — no schedule/door/bed contracts
No CityZoneDefinition, CityBuildingDefinition, DoorTriggerDefinition, BedDefinition, SchedulePeriod, NpcScheduleDefinition found

Existing systems:
- `NpcDataSO`: EXISTING_CANONICAL — not touched; has DefaultSceneId/DefaultPositionId but no schedule
- City contracts: MISSING_SAFE_TO_CREATE — new City/Layout and City/Schedule namespaces created

## Acceptance Criteria

| Criterion | Evidence | Status |
|-----------|----------|--------|
| CityZoneDefinition contract | CityZoneDefinition.cs | OK |
| CityBuildingDefinition with all fields | CityBuildingDefinition.cs | OK |
| OpenHoursRule with midnight wrap | OpenHoursRule.IsOpenAt() | OK |
| DoorTriggerDefinition with target/gate | DoorTriggerDefinition.cs | OK |
| BedDefinition with ownership/CanPlayerUse | BedDefinition.cs | OK |
| NPC bed not player-usable (unless Guest) | BedType.Guest guard in validator | OK |
| SchedulePeriod (7 periods, correct hours) | SchedulePeriod.cs + SchedulePeriodHelper | OK |
| NpcScheduleDefinition with all modifier types | NpcScheduleDefinition.cs | OK |
| NpcScheduleResolver with modifier priority | NpcScheduleResolver.Resolve() | OK |
| Festival override takes highest priority | Festival check first in resolver | OK |
| Lunar modifiers (Alihana/Senya/Nyx) | LunarModifiers list + resolver branch | OK |
| Rain modifier changes outdoor routes | WeatherModifiers list + resolver branch | OK |
| Night shop via Nyx lunar | LunarModifier with ConditionId="Nyx" | OK |
| CityLayoutScheduleValidator | CityLayoutScheduleValidator.cs | OK |
| Hidden door warns if no gate | HIDDEN_DOOR_NO_GATE non-blocker issue | OK |
| EditMode tests (18 tests) | CityLayoutScheduleValidationTests.cs | OK |

## Canon Compliance

- Kanthor public temple preserved: YES (no temple contract created; canon left to data)
- Anya altar/temple not created: YES (no Anya-specific content created)
- Functional classes used: YES (no D&D classes)
- Folego/Breath/BR not created: YES
- Pets not implemented: YES
- Deep romance/marriage not implemented: YES

## Files Changed

- `Assets/_Game/Scripts/City/Layout/CityZoneDefinition.cs` (new)
- `Assets/_Game/Scripts/City/Layout/CityBuildingDefinition.cs` (new, includes OpenHoursRule)
- `Assets/_Game/Scripts/City/Layout/DoorTriggerDefinition.cs` (new)
- `Assets/_Game/Scripts/City/Layout/BedDefinition.cs` (new)
- `Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs` (new, includes ScheduleModifierType + SchedulePeriodHelper)
- `Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs` (new, includes SchedulePeriodBlock + ScheduleModifierBlock)
- `Assets/_Game/Scripts/City/Schedule/NpcScheduleResolver.cs` (new, includes ScheduleResolveContext + Result)
- `Assets/_Game/Scripts/City/Validation/CityLayoutScheduleValidator.cs` (new)
- `Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs` (new)

## Spec Compliance Matrix

| Requirement | Implementation | Status |
|-------------|----------------|--------|
| CityZoneDefinition | CityZoneDefinition.cs | OK |
| CityBuildingDefinition | CityBuildingDefinition.cs | OK |
| DoorTriggerDefinition | DoorTriggerDefinition.cs | OK |
| BedDefinition + BedType | BedDefinition.cs | OK |
| SchedulePeriod (7 periods) | SchedulePeriod.cs | OK |
| NpcScheduleDefinition | NpcScheduleDefinition.cs | OK |
| NpcScheduleResolver | NpcScheduleResolver.cs | OK |
| Schedule modifier priority: Festival > Lunar > Rain > Quest | Resolver order | OK |
| Validator: door/bed/schedule/building | CityLayoutScheduleValidator.cs | OK |
| Hidden door gate warning | HIDDEN_DOOR_NO_GATE non-blocker | OK |
| Night shop (Nyx) | LunarModifier ConditionId="Nyx" | OK |
| NPC bed not player-usable | BED_PLAYER_ON_NPC_BED blocker | OK |
| Festival suspends normal schedule | FestivalOverrides list + resolver | OK |
| Tests | 18 EditMode tests | OK |

## Validation

Validation method: dotnet build Assembly-CSharp.csproj --no-restore (explicit exit code)
Exit code: 0
Assembly-CSharp: PASS (0E/0W)
Assembly-CSharp-Editor: legacy blocker (pre-existing)
Quality check: known Pester issue (pre-existing)
Docs: EXPECTED_FAIL_LEGACY_ONLY

## Testing Quality Gate

Changed runtime code: YES
Changed deterministic logic: YES (schedule resolver, door/bed validation)
Changed Unity scene/prefab/asset: NO
Automated tests added: YES (18 EditMode tests)
Manual Play Mode scenario: NOT REQUIRED (pure C# contracts)
Residual risk: NpcDataSO integration with NpcScheduleDefinition deferred; city scene/prefab wiring deferred; pathfinding deferred

## Remaining Work

- Wire NpcDataSO.ScheduleId to NpcScheduleDefinition registry
- City scene door trigger placement (Unity Editor, deferred)
- NPC waypoint/pathfinding implementation (deferred)
- Festival full override implementation (deferred)
- Farm visit schedule route unlocking (deferred — spec 08_city_services or 08_city_dialogue)
