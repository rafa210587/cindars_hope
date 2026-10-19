# WAVE_INTEGRATION_11 Runtime Input Fix Report

Date: 2026-06-09
Status: BUILD_VALIDATED_RUNTIME_INPUT_FIX_PENDING_HUMAN_PLAYMODE

## Root Causes

Active skills:
- `DefaultSkillCatalog.BuildAllNodes()` had the 14 new balance-patch nodes, but `BuildAllTrees()` still listed only old node IDs.
- Result: new active skills could exist in data but not appear in the playable tree lists.
- `ActiveSkillExecutionController` only accepted raw `SkillActionId`; if any slot persisted a `SkillNodeId`, execution failed.

Dash:
- `PlayerDashController` existed in code and scene generator, but current `FarmScene.unity` was not regenerated and did not serialize the component on Player.
- Result: Space + direction had no Dash receiver in Play Mode.

Dodge:
- `PlayerMovementAbilityController` and `DirectionalDoubleTapDetector` existed in code and scene generator, but current `FarmScene.unity` was not regenerated and did not serialize the component on Player.
- Result: double tap directional had no Dodge receiver in Play Mode.

## Fixes

- Added the 14 WAVE11 nodes to the corresponding `DefaultSkillCatalog.BuildAllTrees()` lists.
- Updated `SkillTreeState.CountPurchasedInTree()` to count both `treeId_` and `treeId.` node ID styles.
- Updated `ActiveSkillExecutionController` to:
  - use `UnityInput` alias;
  - accept either `SkillActionId` or `SkillNodeId` in active slot storage;
  - validate purchased/equippable node before execution;
  - log `slotIndex`, `rawSlotValue`, `resolvedSkillActionId`, `nodeId`, `effectId`, and executor;
  - publish clear blocked reasons for missing effect/executor.
- Added runtime Player binding from `PlayerDashController` so Dash and Dodge controllers are attached to `GameBootstrap.Instance.PlayerManager.gameObject` without scene edits.
- Updated Dash/Dodge movement to:
  - use the Player collider for collision casts;
  - avoid self-hit in the movement sweep;
  - pause normal `PlayerController` movement during ability movement;
  - prevent Dash/Dodge overlap.
- Updated `PlayerAttackController` so legacy Space dodge does not also fire when Space + direction is used for Dash.
- Added `ValidateWave11RuntimeInputBinding` editor validator.

## Files Altered

- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeState.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
- `Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs`
- `Assets/_Game/Scripts/Player/Movement/PlayerMovementAbilityController.cs`
- `Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs`
- `Assets/_Game/Scripts/Player/Movement/GridMovementDisplacementResolver.cs`
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateWave11RuntimeInputBinding.cs`

## Validation

Assembly-CSharp:
- PASS
- Command: `dotnet restore .\Assembly-CSharp.csproj`; `dotnet build .\Assembly-CSharp.csproj --no-restore`
- Result: 0 warnings, 0 errors

Assembly-CSharp-Editor:
- PASS
- Command: `dotnet restore .\Assembly-CSharp-Editor.csproj`; `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
- Result: 7 pre-existing warnings, 0 errors

Static checks:
- No `GameObject.Find`, `FindObjectOfType`, or `FindObjectsByType` in touched runtime files.
- 14 WAVE11 node IDs present in `DefaultSkillCatalog`.
- 14 WAVE11 action IDs mapped in `ActiveSkillExecutionController`.

Unity Play Mode:
- NOT RUN
- Reason: human Play Mode validation required in local Unity Editor.
- Residual risk: actual controller creation and movement must be confirmed in Play Mode.

## Expected Manual Validation

1. Open FarmScene in Play Mode.
2. Open skill tree with `U`.
3. Buy/unlock an equippable skill.
4. Equip it to slot `R`.
5. Close skill tree.
6. Press `1`.
7. Confirm an effect, feedback-only message, or explicit blocked reason appears.
8. Press `Space + direction`.
9. Confirm player dashes and does not pass through blocking colliders.
10. Double tap a direction key.
11. Confirm player dodges and does not pass through blocking colliders.

Can continue WAVE12: NO. Human Play Mode validation is still required and status is not ACCEPTED.
