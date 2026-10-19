# WAVE INTEGRATION 11 - Movement Actions Runtime Audit

Status: RUNTIME_FIX_APPLIED_PENDING_HUMAN_PLAYMODE

Scope: Dash, Dodge and Block as non-slot movement/defensive actions. WAVE12/13/14/15 were not executed.

## Dash

| Check | Result |
|---|---|
| Existe PlayerDashController? | YES |
| Esta no repo? | YES, `Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs` |
| Esta no csproj? | YES after local compile include update |
| E anexado ao Player? | FIXED via `PlayerMovementActionRuntimeBootstrap` |
| Update roda? | YES, controller reads input in `Update()` |
| Le Space + direcao? | YES, `Space + WASD/Arrows`; fallback to `LastFacingDirection` |
| Move de verdade ou so loga? | FIXED, uses `PlayerMovementDisplacementResolver.TryDisplace()` |
| Usa StaminaManager.TrySpendStamina? | YES, cost 40 |
| Usa Rigidbody2D/CharacterController/transform? | YES, resolver uses `Rigidbody2D.MovePosition`; fallback to `transform.position` |
| Respeita colisao? | YES when player has `Collider2D`; otherwise `COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER` |
| Publica feedback? | YES, `PlayerActionFeedbackEvent` |

Root cause classification: `CONTROLLER_NOT_ATTACHED`, `INPUT_NOT_WIRED`, `MOVEMENT_NOT_IMPLEMENTED_PARTIAL`, `STAMINA_NOT_WIRED_PARTIAL`, `CS_PROJ_MISSING_FILE`.

## Dodge

| Check | Result |
|---|---|
| Existe PlayerDodgeController ou PlayerMovementAbilityController? | `PlayerMovementAbilityController` existed; `PlayerDodgeController` created for explicit runtime |
| Existe DirectionalDoubleTapDetector? | YES |
| Esta anexado ao Player? | FIXED via `PlayerMovementActionRuntimeBootstrap` |
| Detecta double tap WASD/setas? | YES, `DirectionalDoubleTapDetector` handles WASD and arrow keys |
| Move de verdade ou so loga? | FIXED, uses `PlayerMovementDisplacementResolver.TryDisplace()` |
| Usa StaminaManager.TrySpendStamina? | YES, cost 40 |
| Respeita colisao? | YES when player has `Collider2D`; otherwise `COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER` |
| Publica feedback? | YES, `PlayerActionFeedbackEvent` |

Root cause classification: `CONTROLLER_MISSING` for explicit `PlayerDodgeController`, `CONTROLLER_NOT_ATTACHED`, `INPUT_NOT_WIRED`, `MOVEMENT_NOT_IMPLEMENTED_PARTIAL`, `CS_PROJ_MISSING_FILE`.

## Block

| Check | Result |
|---|---|
| Existe PlayerBlockController? | FIXED, created `PlayerBlockController.cs` |
| Esta anexado ao Player? | FIXED via `PlayerMovementActionRuntimeBootstrap` |
| Le Left Shift? | YES |
| Aplica slow de movimento? | YES, multiplies `PlayerController.SpeedMultiplier` by 0.5 |
| Consome stamina por segundo? | YES if `StaminaManager` exists, 10 stamina/s |
| Libera slow ao soltar? | YES |
| Publica feedback/estado? | YES, "Block." and "Block released." |

Root cause classification: `BLOCK_ONLY_DOCUMENTED`, `CONTROLLER_MISSING`, `CONTROLLER_NOT_ATTACHED`, `INPUT_NOT_WIRED`.

## Global root cause

Dash/Dodge were documented and partially implemented, but the runtime binding depended on scene generation or older controller ownership. Block was explicitly deferred and had no runtime controller applying movement slow. The fix introduces a scene-independent runtime bootstrap and concrete movement controllers that do not use active slots.

## Dirty tree classification before fix

Before this movement fix, the working tree already contained:

- `Assets/_Game/Scenes/TownScene.unity` modified, likely Unity/editor scene state from the previous Town validation flow. It was not touched or staged by this fix.
- `Assets/_Game/Scripts/Editor/Validation/ValidateShopPriceData.cs.meta` untracked, generated after the prior shop-price validator. It was not staged by this fix.
