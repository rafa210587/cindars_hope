# WAVE_INTEGRATION_11 Runtime Input Failure Audit

Date: 2026-06-09
Status: RUNTIME_INPUT_FAILURE_AUDITED

## Active Skills

| Question | Finding | Classification |
|---|---|---|
| ActiveSkillExecutionController exists in Play Mode? | It can be created by `RuntimeInitializeOnLoadMethod`; regenerated FarmScene also tries to add it. Current FarmScene does not serialize it, so runtime creation is required. | CONTROLLER_NOT_CREATED risk mitigated by runtime singleton |
| Created via RuntimeInitializeOnLoadMethod? | Yes, `EnsureRuntimeInstance()` creates `ActiveSkillExecutionController` after scene load. | PASS |
| Destroyed by duplicate? | Duplicate guard destroys later duplicates. Existing path is safe, but diagnostics were weak. | PASS_WITH_DIAGNOSTIC_GAP |
| Update runs? | Runs only when `GameBootstrap.Instance` exists and no modal is active. | PASS |
| Which keys does gameplay listen to? | `Alpha1` to `Alpha4`. | PASS |
| Which slots does UI equip? | 4 slots, displayed as `R/T/Y/G` in the SkillTree UI. | PASS |
| Which keys does UI use to equip? | `R/T/Y/G` while SkillTree UI is open. | PASS |
| Which keys activate gameplay slots? | `1/2/3/4` while gameplay is free. | PASS |
| Conflict between equip/use keys? | No direct conflict. UI equip keys differ from gameplay use keys. | PASS |
| What does `GetActiveSlotSkillActionId` return? | Intended value is `SkillActionId`; save/slot field name confirms action ID. | PASS |
| Could slot contain node id by mistake? | Prior controller rejected node IDs. Fix now accepts either `SkillActionId` or `SkillNodeId` and converts node ID to `UnlockedSkillActionId`. | SLOT_ID_MISMATCH fixed |
| Does purchased node have `UnlockedSkillActionId`? | Equippable nodes do. New patch nodes have action IDs. | PASS |
| Does slot accept unpurchased skill? | `TryAssignActiveSlot` validates unlocked action IDs. Controller also revalidates purchase before execution. | PASS |
| Does `SkillActionToEffectId` contain exact ID? | Existing mappings include old action IDs and 14 new patch IDs. | PASS |
| Executor exists in registry? | `farm.crop.water_skill` and 14 feedback effects exist. Older combat/ranged/magic mappings remain deferred and now report blocked reason clearly. | EXECUTOR_MISSING documented |
| Feedback appears through GameEventBus? | Controller publishes `PlayerActionFeedbackEvent`; success path no longer double-publishes. | PASS |
| ModalManager blocks input after close? | Controller blocks while any modal is active; SkillTree close pops `ModalType.SkillTree`. No stuck-modal code change was needed. | PASS |

## Dash

| Question | Finding | Classification |
|---|---|---|
| PlayerDashController exists? | Yes, `Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs`. | PASS |
| Attached to Player or runtime-created? | Previously only scene generator attached it; current FarmScene is not regenerated and lacks it. Fix adds runtime binding to `GameBootstrap.Instance.PlayerManager.gameObject`. | CONTROLLER_NOT_CREATED fixed |
| Update runs? | Runs when component is attached and modal is not active. | PASS |
| Reads Space + direction? | Yes. It now uses `UnityInput.GetKeyDown(KeyCode.Space)` and `PlayerController.MoveInput`, falling back to facing direction. | PASS |
| Direction used? | Current movement input or `PlayerController.LastFacingDirection`. | PASS |
| Moves player or logs only? | Moves with `Rigidbody2D.MovePosition` or transform fallback. | MOVEMENT_NOT_IMPLEMENTED fixed |
| Collision/bounds handling? | Uses `GridMovementDisplacementResolver` with the player's own collider cast so it does not hit itself and stops before blocking colliders. | PASS |
| Stamina gate? | Uses `StaminaManager.TrySpendStamina(40)` when available; no manager means fallback no-cost path. | PASS_WITH_TEMP_FALLBACK |
| Feedback appears? | Publishes success, cooldown, no-direction, and stamina feedback. | PASS |

## Dodge

| Question | Finding | Classification |
|---|---|---|
| Dodge controller exists? | Yes, `PlayerMovementAbilityController` with `DirectionalDoubleTapDetector`. | PASS |
| Attached to Player or runtime-created? | Previously only scene generator attached it; current FarmScene lacks it. Fix binds it at runtime together with Dash. | CONTROLLER_NOT_CREATED fixed |
| Double tap detector exists? | Yes, `DirectionalDoubleTapDetector`. | PASS |
| Double tap window exists? | Yes, 0.25s MVP window. | PASS |
| WASD/arrows work? | Yes, detector supports W/A/S/D and arrows. | PASS |
| Moves player or logs only? | Moves with `Rigidbody2D.MovePosition` or transform fallback. | MOVEMENT_NOT_IMPLEMENTED fixed |
| Collision/bounds handling? | Uses the same collider-cast resolver as Dash. | PASS |
| Stamina gate? | Uses `StaminaManager.TrySpendStamina(40)` when available; no manager means fallback no-cost path. | PASS_WITH_TEMP_FALLBACK |
| Feedback appears? | Publishes success, cooldown, and stamina feedback. | PASS |

## Root Cause Summary

- Active skills: new balance-patch nodes existed in `BuildAllNodes()` but were missing from the `BuildAllTrees()` lists, so they did not appear in the actual tree UI. Controller also did not tolerate a slot containing `SkillNodeId`.
- Dash: `PlayerDashController` was not present in the current FarmScene unless the scene was regenerated, so Play Mode had no Dash component.
- Dodge: `PlayerMovementAbilityController` was not present in the current FarmScene unless the scene was regenerated, so double tap had no runtime receiver.
- Movement collision: the previous resolver used a generic circle cast that could be fragile around the player's own collider. It now supports casting the actual moving collider.
