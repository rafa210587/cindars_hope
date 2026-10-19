# WAVE_INTEGRATION_11 — Dash & Dodge Movement Contract

**Date:** 2026-06-08
**Design source:** `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md` §13, §14
**Status:** IMPLEMENTED (BUILD_VALIDATED)

---

## Dash Contract

**Purpose:** High-speed directional burst for repositioning and gap-crossing.

| Property | Design Range | Implemented Value |
|----------|-------------|-------------------|
| Input | Space + directional WASD/Arrow | Space + `_playerController.MoveInput != Vector2.zero` |
| Distance | 3.2–4.0 tiles | 3.5 tiles |
| Duration | 0.18–0.28s | 0.22s |
| Stamina cost | 40 base | 40 |
| Cooldown | 0.75–1.2s | 1.0s |
| I-frame | 0.16–0.24s | NOT IMPLEMENTED (combat wave deferred) |
| Active slot | Does NOT occupy | Confirmed |
| Controller | `PlayerDashController` | Yes |

**Disambiguation from existing space handlers:**
- `PlayerAttackController.TryDodge()` is the old dodge (called on Space). `PlayerDashController` checks for directional input FIRST. If directional input is present, Dash fires and does NOT call through to the old handler.
- `PlayerDodgeController` (separate component) also listens to Space. In FarmScene MVP, both may coexist. Dash takes priority when direction is non-zero.

**Movement physics:** Uses `GridMovementDisplacementResolver.Resolve()` with `Physics2D.CircleCast` for obstacle detection, then `Rigidbody2D.MovePosition()` in a coroutine.

---

## Dodge Contract

**Purpose:** Short evasive roll for combat i-frame and reposition.

| Property | Design Range | Implemented Value |
|----------|-------------|-------------------|
| Input | Double-tap directional key | WASD + Arrow, 0.25s window |
| Distance | 1.2–1.8 tiles | 1.5 tiles |
| Duration | 0.28–0.45s | 0.32s |
| Stamina cost | 40 base | 40 |
| Cooldown | 0.45–0.90s | 0.6s |
| I-frame | 0.16–0.24s | NOT IMPLEMENTED (combat wave deferred) |
| Active slot | Does NOT occupy | Confirmed |
| Controller | `PlayerMovementAbilityController` | Yes |

**Double-tap detection:** `DirectionalDoubleTapDetector` tracks last tap time per key. A second tap within `0.25s` (generously above the 0.08–0.14s design minimum for MVP playability) triggers the direction vector.

---

## Block: DEFERRED

**Input:** Left Shift
**Reason:** Block requires combat/cave target context, EnemyHealth pipeline, and parry timing window. FarmScene MVP has no enemies.
**Status:** `BLOCK_RUNTIME_DEFERRED_WITH_REASON`
**Future:** Implement in combat integration wave targeting cave/combat scene.

---

## `GridMovementDisplacementResolver`

Static helper used by both Dash and Dodge:

```csharp
public static Vector2 Resolve(
    Vector2 origin,
    Vector2 direction,
    float maxDistance,
    float colliderRadius = 0.3f,
    int? obstacleLayer = null)
```

- If `direction == Vector2.zero`, returns `origin` (no displacement).
- Uses `Physics2D.CircleCast` to stop at obstacles.
- Returns the safe destination point.

---

## `DirectionalDoubleTapDetector`

Pure C# class (no MonoBehaviour):

```csharp
public Vector2? UpdateAndCheckDoubleTap()
```

- Call in `Update()`.
- Returns the direction vector if double-tap detected, else `null`.
- Resets tap timer after detection to prevent triple-tap.

---

## Known Issues / Residual Risks

1. **Space conflict:** `PlayerDodgeController` and `PlayerDashController` both check Space. If player has no directional input, the old `PlayerDodgeController` fires (backward compat). If direction is held, `PlayerDashController` fires. This is documented **movement input debt** to be resolved in the combat integration wave.

2. **I-frames not implemented:** Dash and Dodge do not grant invincibility frames. This is safe for FarmScene MVP (no enemies), but must be added before cave/combat wave.

3. **Stamina bypass in TryWaterViaSkill:** The farm crop water skill does not currently deduct Stamina from the player for the skill slot cost. The `FarmCropSkillEffectExecutor` marks this as `TODO_INTEGRATION_NOT_FINAL`. The Dash/Dodge controllers DO correctly deduct 40 Stamina.

4. **csproj manual registration:** New files were manually added to `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj`. Unity will overwrite these on reimport. Human must regenerate FarmScene using the CreateMvpFarmScene editor menu after Unity reimports.

---

*Contract created: 2026-06-08 (WAVE_INTEGRATION_11)*
