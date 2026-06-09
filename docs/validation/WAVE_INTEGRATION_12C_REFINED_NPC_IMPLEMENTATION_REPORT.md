# WAVE INTEGRATION 12C - Refined NPC Implementation Report

## Status

BUILD_VALIDATED_WITH_REFINED_NPC_DEBT

## Summary

WAVE12C promotes the full refined canonical city roster to visible TownScene population. The implementation remains data-driven: generic `NpcDataSO`, `DialogueTreeSO`, `ShopDataSO`, `NpcController`, `NpcShopController`, `NpcWanderer` and `NpcScenePlacementMarker` are reused. No class-per-NPC scripts, final quests, social/romance/reputation systems, companions or pets were created.

## Coverage

| Metric | Value |
|---|---:|
| Required canonical NPCs | 23 |
| NPC data assets | 23 |
| Dialogue trees >=10 entries | 23 |
| TownScene placement markers | 23 |
| Shop/service definitions | 20 |
| Shop controllers wired in scene | 17 |
| Dialogue-only no-shop NPCs | 3 |
| Legacy extra retained | 1 |

## Validation Evidence

| Check | Result |
|---|---|
| Unity batchmode generation | NOT RUN - blocked by another Unity instance already open |
| TownScene static YAML check | PASS - 23 canonical markers, 0 duplicate fileIDs |
| Dialogue static count | PASS - 23 trees with 10 node definitions |
| Assembly-CSharp | PASS, 0 warnings, 0 errors |
| Assembly-CSharp-Editor | PASS, 7 legacy warnings, 0 errors |
| Docs validation | FAIL_KNOWN_LEGACY_ONLY - `spec_test_harness_editmode_playmode_quality_gate.md`, older validation ADR/game-rule metadata, and older amendment citations |
| Quality check | FAIL_HARNESS - Pester `Should` used outside `Describe` |

## Debts

- Human Play Mode validation is still required.
- Advanced service UI is not final for Pip, Nimble and Thalindra.
- Patrol, night route and daily schedule semantics are marker/profile wired but final route/schedule simulation is deferred.
- Dialogue copy is seed/authoring text, not final narrative polish.
- Relationship, romance, reputation, personal quests, companions and pets remain out of scope.

## Decision

WAVE13 remains blocked until WAVE12C Play Mode is accepted by the human.
