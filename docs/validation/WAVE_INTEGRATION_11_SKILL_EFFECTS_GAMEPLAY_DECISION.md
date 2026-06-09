# WAVE_INTEGRATION_11 — Skill Effects Gameplay Bridge: Technical Decision

**Date:** 2026-06-08
**Author:** Spec Implementer Agent
**Status:** DECIDED

---

## Context

WAVE_INTEGRATION_10 implemented the active slot equip UI and UI→SkillTreeManager wiring, but explicitly deferred the "Effect Layer" — the actual execution pipeline that takes an equipped skill action ID and produces a gameplay effect. WAVE_INTEGRATION_11 implements this bridge.

Key constraints coming in:

- `FarmPlot.TryWater()` was private → needed a public bridge
- `InteractionSystem.GetBestCandidate()` was private → needed public exposure
- `ActiveSkillSlots.cs` uses R/T/Y/G keys → numeric 1-4 used for new controller to avoid conflict
- `PlayerAttackController` and `PlayerDodgeController` both claim Space key → needed Dash disambiguation
- Unity auto-generated `.csproj` files with `<EnableDefaultItems>false</EnableDefaultItems>` → new files must be manually added to csproj

---

## Decision: Strategy REUSE_AND_EXTEND

### Active Skill Execution

Use `SkillTreeManager.State.GetActiveSlotSkillActionId(slotIndex)` (from WI10 decision) to read what action is equipped. Map action ID → effect ID via static `SkillActionToEffectId` dictionary in `ActiveSkillExecutionController`. Route to `SkillEffectRegistry.Resolve(effectId)` for the executor.

**Why not extend `SkillActionExecutor`?** The existing `SkillActionExecutor` is damage-only (works with `SkillActionSO` ScriptableObjects). The new system needs to handle farm, utility, and movement effects that have no damage target. A separate extensible registry pattern avoids polluting the combat executor.

### Farm Vertical Slice

Effect: `"farm.crop.water_skill"` mapped to equippable skill action IDs containing `"farm"` or `"watering"` in their names. The `FarmCropSkillEffectExecutor` resolves `GetCurrentInteractable()` → casts to `FarmPlot` → calls `TryWaterViaSkill()`. This allows the player to water the current targeted plot by pressing a skill slot key.

### Dash/Dodge Disambiguation

Design doc (COMBAT_CORE_DIRECTION.md §13, §14):
- **Dash**: Space + directional input → 3.5 tiles
- **Dodge**: double-tap directional → 1.5 tiles
- **Block**: Left Shift → DEFERRED (no combat scene in FarmScene MVP)

`PlayerDashController` is a NEW MonoBehaviour added to the player. It checks `Space` AND `_playerController.MoveInput != Vector2.zero` to distinguish from the existing `PlayerDodgeController` (Space only). The existing `PlayerDodgeController` is left in place as it handles Space without direction (backward compatibility, will be deprecated in combat wave).

`PlayerMovementAbilityController` uses `DirectionalDoubleTapDetector` for double-tap Dodge. It does NOT intercept Space.

### Scene Wiring: RuntimeInitializeOnLoadMethod

`ActiveSkillExecutionController` uses `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` to create itself in the scene if not present. This avoids requiring `CreateMvpFarmScene.cs` to have compile-time dependency on the new runtime assembly types. `CreateMvpFarmScene.cs` uses `System.Type.GetType(...)` for late-bound scene placement when Unity has fully compiled the runtime assembly.

### csproj Manual Registration

Since Unity regenerates `.csproj` files and we cannot run Unity in batch mode here, new files are manually added to `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj`. Unity will overwrite these entries on next reimport, but they serve as the `dotnet build` validation gate now.

---

## Rejected Alternatives

- **Extend SkillActionExecutor for all effects**: rejected — would couple farm/utility logic to combat data pipeline
- **Use direct MonoBehaviour-to-MonoBehaviour calls**: rejected — violates event-bus-only-gameplay-communication rule
- **Add FindObjectOfType in gameplay Update()**: rejected — spec authorizes it only for scene-load wiring
- **Block deferred**: accepted — BLOCK_RUNTIME_DEFERRED_WITH_REASON; FarmScene has no enemies

---

## Residual Risks

- `PlayerDodgeController` and `PlayerDashController` both listen to Space: if both are active, Dash takes priority when directional input is held. Backward compat for no-direction Space is handled by PlayerDodgeController. This is documented movement input debt.
- Stamina cost for `TryWaterViaSkill()` is bypassed (TODO_INTEGRATION_NOT_FINAL) — tracked in FarmPlot.cs comment.
- Unity `.csproj` will be overwritten on reimport; human must regenerate FarmScene after Unity re-imports.

---

*Decision created: 2026-06-08 (WAVE_INTEGRATION_11)*
