# WAVE INTEGRATION 11 - Movement Actions Fix Report

Status: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE

## Root cause

Dash/Dodge/Block were documented, but controllers runtime were not reliably materialized/anexados ao player or did not cover the current Play Mode needs end to end. Dash and Dodge had partial movement implementations, while Block was only documented/deferred and did not apply slow.

## Fix

- `PlayerMovementActionRuntimeBootstrap`
- `PlayerDashController`
- `PlayerDodgeController`
- `DirectionalDoubleTapDetector`
- `PlayerBlockController`
- `PlayerMovementDisplacementResolver`

The bootstrap attaches the movement runtime to `GameBootstrap.Instance.PlayerManager.gameObject` after scene load, without manual scene edits.

## Runtime behavior

| Action | Input | Runtime behavior |
|---|---|---|
| Dash | Space + direction | Moves player 3.5 units over 0.14s |
| Dodge | Double tap direction | Moves player 1.5 units over 0.10s |
| Block | Hold Left Shift | Applies 0.5 movement speed multiplier while held |

Dash/Dodge/Block do not occupy active slots and are guarded by `ModalManager.HasActiveModal`.

## Stamina

| Action | Cost |
|---|---:|
| Dash | 40 stamina |
| Dodge | 40 stamina |
| Block | 10 stamina/s while held |

Values are marked `TODO_INTEGRATION_NOT_FINAL` in code and remain balance debt.

## Collision and bounds

Dash and Dodge use `Collider2D.Cast` through `PlayerMovementDisplacementResolver` when the player has a `Collider2D`.

Bounds depend on blocking colliders in the scene. If no player collider exists, movement falls back to direct displacement and records `COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER`.

## Debts

- combat damage reduction for block deferred
- final stamina/cooldown balance deferred
- bounds depend on colliders
- if no player collider, collision debt

## Expected Play Mode result

- Space + W/A/S/D moves the player about 3.5 units.
- Double tap W/A/S/D moves the player about 1.5 units in the tapped direction.
- Holding Left Shift reduces player movement speed.
- Releasing Left Shift restores normal movement speed.
- None of these actions occupy active slots.
- With a modal open, none of these actions execute.
