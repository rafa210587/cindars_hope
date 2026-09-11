# SPEC — Correção de escala, passagem e respiro da FarmScene

> **Spec ID:** `spec_farm_scene_keyart_scale_physics_correction_v1`  
> **Status:** A implementar  
> **Wave:** FARM KEYART — corrective slice  
> **Priority:** P0  
> **Type:** Editor scene generation / physics integration  
> **Domain:** Farm  
> **Parallelizable:** NO — shared `CreateMvpFarmScene.cs`  
> **Repo lock scope:** `CreateMvpFarmScene.cs`, `FarmSceneCompositionContract.cs`, `FarmSceneSpatialContract.cs`, editor validators/tests and `docs/validation/**`  
> **Depends on:** specs FARM KEYART 1–7 source implementation  
> **Blocks:** visual acceptance  
> **Scope:** correct the observed oversized crafts, missing craft solidity, bridge traversal proof, house/greenhouse visual overlap, and unsafe decoration margins.  
> **Out of scope:** new art, YAML, save schema, new gameplay systems, changing the lake/river layout.  
> **Validation target:** Editor build + Unity generated-scene physics validator + human capture.  
> **Executor:** Codex / Claude

required_adrs: []
required_game_rules: []

# /speckit.specify

## 9. Observed baseline — 2026-08-14

- Capture 1: farmhouse visual overlaps greenhouse; its mass is far larger than the keyart homestead cluster.
- Capture 2: workbench/forge/cooking props visually dwarf the player and each station has only an interaction trigger.
- Capture 3: player cannot prove traversal through the bridge even though the river contract declares y `[2,4]` as its free corridor; visual buffers are also too tight.
- Existing systems: `CraftingPoint` owns interaction; `FarmSceneSpatialContract` owns water collision; `FarmDecorationPlanner` owns visual-only clutter. Reuse all three.

## 13. Non-duplication rules

Do not create a second crafting system, bridge controller, navigation service, player collider, or scene YAML. A crafting station keeps its existing trigger and gains at most one child solid collider. Decoration remains child `SpriteRenderer` only.

# /speckit.plan

## 15. Files

```text
MODIFY
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs
Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneNavigation.cs

CREATE
Assets/_Game/Tests/EditMode/Farm/FarmSceneScalePhysicsCorrectionTests.cs
docs/validation/spec_farm_scene_keyart_scale_physics_correction_v1_execution_report.md
docs/validation/playmode/spec_farm_scene_keyart_scale_physics_correction_v1_human_test_scenario.md
```

## 16. Contracts

```csharp
public static class FarmSceneCompositionContract
{
    public const float CraftingVisualTargetHeight = 1.8f;
    public const float HomesteadRoofTargetWidth = 7.5f;
    public const float HomesteadDecorationClearance = 2f;
    public static readonly Vector2 BridgePassageProbe = new(15f, 3f);
}
```

## 20. Implementation phases

1. In `CreateCraftingStation`, replace the hardcoded visual target `3f` with the contract height. Keep the root trigger for `CraftingPoint`; create `SolidBody` child with a non-trigger `BoxCollider2D` fitted below the sprite and leave a south approach gap.
2. In `CreateFarmWalkInHouse`, use the corrected roof target width. Move only the greenhouse visual anchor if its sprite bounds still intrude in the homestead clear area; root colliders remain contract-owned.
3. In `FarmDecorationPlanner.IsForbiddenCell`, exclude the expanded house/greenhouse/crafting-yard bounds by the named clearance, in addition to existing exclusions.
4. Extend navigation validator to query `Physics2D.OverlapPoint` after scene regeneration: bridge probe must have no non-trigger collider; two water probes above/below must find the river collider. Do not claim pass without menu output.
5. Add pure tests: corrected scale constants, bridge probe belongs to bridge/free corridor and water probes do not, decoration clearance rejects house-adjacent cells.

## 14. Binary acceptance

- Craft scale: contract reports `CraftingVisualTargetHeight == 1.8`; a generated capture shows craft height at most twice player height.
- Craft physics: each of three stations has exactly one trigger root and one non-trigger `SolidBody`; approach point is free.
- Bridge: `ValidateFarmSceneNavigation` prints `Bridge corridor physics: PASS` after regeneration; probes `(15,3)` free and `(15,5)/(15,1)` blocked.
- Composition: farmhouse roof width is `7.5`; greenhouse visual has no sprite-bounds overlap with roof in the generated scene.
- Buffer: `ValidateFarmSceneDecoration` reports `Forbidden decoration placements: 0` including the expanded homestead/crafting exclusion.

## 23. Edge cases

- Sprite missing: retain fallback visual but use named default collider dimensions; never omit the interaction trigger.
- Bridge visual is not a collider: it must not block the free corridor.
- Unity physics menu cannot run before regeneration: record `NOT RUN`, never infer it from pure tests.

# /speckit.tasks

- [ ] Implement phases 1–4 and record source/build/Unity evidence.
