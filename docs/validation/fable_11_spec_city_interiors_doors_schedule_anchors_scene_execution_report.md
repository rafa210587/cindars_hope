# Execution Report — fable_11 City Interiors, Doors & Schedule Anchors

> Spec: `.specs/a_implementar/fable/fable_11_spec_city_interiors_doors_schedule_anchors_scene.md`
> Date: 2026-06-19
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (scene placement = DEFERRED_UI_VISUAL; Play Mode DEFERRED per owner)
> Validated game rules: `city_rules.md` (Rules 3, 4, 5, 6)

---

## Honest status rationale

The deterministic core of fable_11 — per-hour Work/Social/Home/Night block resolution (closes
TIME_BLOCK_DEBT), availability gating (CA-4), the schedule profile/anchor model, the `DoorInteractable`
behavior (teleport interior + closed-shop block per decision 6.4-A), the `NpcWanderer.SetDestination`
movement override, and the scene generator emitting 3 anchors/NPC + 12 interiors + 24 paired doors
(closes SCENE_WIRING_DEBT) — is implemented, compiles clean (0 errors both assemblies), and is covered
by EditMode tests for the pure logic.

What is **NOT** claimed: the actual `.unity` scene was **not regenerated** (the generator runs only in
the Unity Editor, which the owner explicitly deferred for this batch) and Play Mode / human "living
town" validation was **not run**. Therefore anchors/interiors/doors exist as generator code, not yet as
committed scene objects. This is honestly reported as DEFERRED, never as PASS. Final acceptance requires
a human to run `CindarsHope/Create Scenes/Town Scene`, then `CindarsHope/Validate/Fable City Schedule
(fable_11)`, then the day/night Play Mode scenario.

---

## Acceptance criteria extracted

| CA | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| CA-1 | Service resolves Work/Social/Home/Night by hour; transitions at archetype hours (closes TIME_BLOCK_DEBT) | DONE (logic) | `NpcScheduleBlockResolver.ResolveBlock`; tests `Shopkeeper_*`, `Guard_*`, `Night_*`, `Wanderer_*` |
| CA-2 | Generator creates ≥3 anchors per NPC by npcId; validator confirms 23 covered (closes SCENE_WIRING_DEBT) | CODE DONE / scene DEFERRED | `CreateMvpTownScene.CreateNpcScheduleAnchors` (work/social/home per NPC); `ValidateFableCitySchedule` |
| CA-3 | Each house has a DoorInteractable that teleports to interior with return door; camera follows | CODE DONE / scene DEFERRED | `DoorInteractable`; `CreateMvpTownScene.CreateHouseInteriorsAndDoors` (12 interiors + 24 doors) |
| CA-4 | NPC in Home/Night does not open shop/dialogue and informs the reason; Yael inverts | DONE (logic) | `NpcScheduleAvailabilityGate`; controllers gated; tests `NightVendor_Unavailable_AtMidday` (14h=false), `NightVendor_Available_AtNight` (22h=true) |

---

## Existing systems audit (system-reuse)

| System | Found | Action |
|--------|-------|--------|
| `NpcScheduleProfile/Block/Anchor/RuntimeState/Service/RuntimeBootstrap` (WI-25) | yes | **REUSED + EXTENDED** — added archetype + per-hour blocks; no parallel system |
| `NpcSchedulePeriod` crosswalk (fable_19) | yes | Left intact; runtime collapses to Work/Social/Home/Night as the design specifies |
| `NpcTownRosterRegistry` (23 NPCs) | yes | REUSED — validator coverage + roster-driven shop flag |
| `NpcWanderer` | yes | **EXTENDED** — added `SetDestination`/`ClearDestination` override; no new wander system |
| `IInteractable` + `InteractionSystem` | yes | REUSED — `DoorInteractable` implements the existing contract; closed-shop prompt flows through `InteractionPromptChangedEvent` |
| `InteractionPromptChangedEvent` / `PlayerActionFeedbackEvent` | yes | REUSED for unavailability prompt + feedback |
| `GameTimeManager` (Day/Night phase) | yes | **EXTENDED** — added derived `CurrentHourOfDay` (no new clock, no save field) |
| `NpcController` / `NpcShopController` | yes | EXTENDED — availability gate before open |
| `City/Schedule/*` (obsolete, fable_19) | yes | NOT touched, NOT used (stays `[Obsolete]`) |

No second schedule system, no separate interior scene, no `SceneTransitionRouter`/Build Settings change.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | OK |
|--------------------|----------------|----|
| Extend `NpcScheduleProfile` with 4 hour blocks by archetype | `NpcScheduleProfile.CreateForArchetype` + `NpcScheduleArchetype` | OK |
| Service resolves current block by hour, moves NPC to anchor | `NpcScheduleService` subscribes `GameTimeTickEvent`; `ResolveNpc` → `SetDestination` | OK |
| `NpcWanderer.SetDestination(target, arriveRadius)` | added; works for static NPCs too | OK |
| Generator: 3 anchors/NPC (work=current; home=house door; social=archetype hub) | `CreateNpcScheduleAnchors` | OK |
| Functional doors on 12 houses → minimal 1-room interior (y>+40), return door, same scene | `CreateHouseInteriorsAndDoors`, `DoorInteractable` | OK |
| Closed shop → door BLOCKED with opening-hours notice (EMENDA 6.4-A) | `DoorInteractable.IsBlockedByShopHours` + notice; no teleport | OK |
| Unavailability: controllers consult availability → prompt + feedback, no open | `NpcScheduleAvailabilityGate` + controller gates | OK |
| Yael/Maelor inverted (NightOnly) | `ResolveArchetype` → Night; `NpcScheduleBlockResolver` Night windows | OK |
| Event `NpcScheduleBlockChangedEvent` (new, unsubscribe) | added; published on block change; service unsubscribes in `OnDisable` | OK |
| Save schema unchanged (block re-derives from hour) | no save DTO touched | OK |
| EditMode tests: block by hour, anchor-by-archetype, availability (Yael 14h/22h) | `NpcScheduleBlocksTests` | OK |
| Teleport fallback if stuck > 5s | `NpcScheduleService.Update` stuck timer → teleport | OK |
| NPC mid-interaction pauses schedule | existing `SetInteractionPaused` honored in `NpcWanderer.FixedUpdate` | OK |

---

## Files changed

Runtime (Assembly-CSharp):
- `Assets/_Game/Scripts/Core/Events/NpcScheduleBlockChangedEvent.cs` (NEW)
- `Assets/_Game/Scripts/Core/GameTimeManager.cs` (added `CurrentHourOfDay`)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleArchetype.cs` (NEW)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlockResolver.cs` (NEW)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleAvailabilityGate.cs` (NEW)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleProfile.cs` (archetype + hour blocks)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlock.cs` (added `RuntimeBlock`)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleService.cs` (per-hour resolution, movement, events)
- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleRuntimeBootstrap.cs` (self-wire time + controllers + profiles)
- `Assets/_Game/Scripts/NPC/NpcWanderer.cs` (`SetDestination`/`ClearDestination`)
- `Assets/_Game/Scripts/NPC/NpcController.cs` (availability gate)
- `Assets/_Game/Scripts/NPC/NpcShopController.cs` (availability gate)
- `Assets/_Game/Scripts/World/DoorInteractable.cs` (NEW)

Editor (Assembly-CSharp-Editor):
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` (anchors + interiors + doors)
- `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs` (NEW validator)

Tests:
- `Assets/_Game/Tests/EditMode/City/NpcScheduleBlocksTests.cs` (NEW, 16 tests)

Project files (regenerated by Unity; NOT committed): `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`
(includes added so the dotnet gate compiles the new files).

---

## Validation

Validation method: `run_strict_validation.ps1`
Exit code: 0
- Assembly-CSharp: PASS (0 errors; 1 pre-existing unrelated warning CS0649 in CombatTelemetrySession)
- Assembly-CSharp-Editor: PASS (0 errors; pre-existing unrelated warnings only)
- Docs validation (`validate_docs.ps1`): PASS / EXPECTED_FAIL_LEGACY_ONLY (legacy-only)
- Diff completeness (`check_spec_diff_completeness.ps1`): PASS
- Quality check (`check_spec_quality.ps1`): PASS
- Result artifact: `docs/validation/LAST_STRICT_VALIDATION_RESULT.json`

EditMode tests: written (`NpcScheduleBlocksTests`, 16 tests). Authoritative execution is the Unity Test
Runner (NOT RUN — Unity Editor deferred this batch); the assertions compile under Assembly-CSharp and
cover CA-1/CA-4 with synthetic hours.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (block resolution, availability, hour derivation, profile factory)
Changed Unity scene/prefab/asset wiring: YES (generator code only — scene not regenerated)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/City/NpcScheduleBlocksTests.cs)
Automated tests command: Unity Test Runner (EditMode) — NOT RUN (Unity Editor deferred); dotnet build PASS
Manual Play Mode scenario: docs/validation/playmode/fable_11_human_test_scenario.md
Justification if no automated tests: N/A (tests added for the deterministic core)
Residual risk: scene placement of anchors/interiors/doors and the live "living town" movement,
  closed-shop door block, and door→interior→return are scene/runtime-dependent and validated only after
  the human regenerates TownScene and runs Play Mode. Movement timing/teleport fallback tuned by code,
  not yet observed in scene.
```

---

## Phase status

- Phase 0 (audit): COMPLETE
- Phase 1 (deterministic logic + tests): BUILD_VALIDATED
- Phase 2 (Unity compile via batchmode): NOT RUN — Unity Editor deferred this batch (dotnet build is the fallback compile signal, PASS)
- Phase 3 (Play Mode / human "living town"): DEFERRED_TO_FINAL_VALIDATION (owner-authorized)

NOT RUN items and residual risk:
- Unity batchmode compile: NOT RUN (reason: Editor deferred). Residual risk: Unity-only compile errors
  not surfaced by dotnet; low — no editor-only API used in runtime files.
- TownScene regeneration: NOT RUN. Residual risk: anchors/interiors/doors absent from committed scene
  until a human runs the generator menu.
- Play Mode day/night cycle: NOT RUN. Residual risk: movement feel, teleport fallback, door teleport and
  closed-shop block unobserved.

---

## Remaining work (for final acceptance)

1. Human: run `CindarsHope/Create Scenes/Town Scene` (regenerate TownScene with anchors/interiors/doors).
2. Human: run `CindarsHope/Validate/Fable City Schedule (fable_11)` → expect PASS (23 NPCs ≥3 anchors,
   12 interiors y>+40, 24 doors, ≥1 shop-gated door).
3. Human: run the Play Mode scenario (`docs/validation/playmode/fable_11_human_test_scenario.md`) over a
   full day/night cycle.
4. Run Unity Test Runner (EditMode) to execute `NpcScheduleBlocksTests`.

Out of scope (future specs, unchanged): weather/lunar/season schedule modifiers, festivals, friendship-
locked doors, rich interior furniture, obstacle pathfinding (direct move + teleport fallback only).

---

## Dependency Chain

Original target: fable_11
Dependency chain: WI-25 (NpcSchedule runtime — present, BUILD_VALIDATED), WAVE 02 (GameTimeManager — present), slice 2026-06-12 (TownScene v2 houses — present)
Forbidden dependencies: none
Resolved depth: 0 (all dependencies already implemented)
Can continue original target: YES
