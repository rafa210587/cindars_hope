---
name: runtime-bootstrap-pattern
description: Standardize how runtime systems self-wire into scenes (the *RuntimeBootstrap idiom) without forbidden global searches. Use for orphan-system wiring specs (fable_15, fable_16) or any new scene-runtime binding.
---

# Skill: Runtime Bootstrap Pattern

## Current state (honest)

The project grew a de-facto idiom: `*RuntimeBootstrap` classes (Quest, CraftingStation, NpcSchedule, FarmDailyGoal, PlayerMovementAction) that locate their dependencies with `FindObjectOfType` — which violates rule `unity-architecture` §1. ~14 runtime files carry this debt. **A human decision is pending: bless a constrained version of the idiom or schedule its removal.** Until then:

## Rules for ANY new scene-runtime binding

1. **Do not copy the `FindObjectOfType` pattern.** The `runtime-code-guard` hook flags it in new code.
2. Resolution order for dependencies:
   1. Serialized reference assigned by the scene creator (`Editor/SceneCreation/CreateMvp*Scene.cs` wires it) — preferred;
   2. GameBootstrap injection (`Core/Bootstrap/GameBootstrap.cs` holds the ref and injects via explicit method) — for cross-scene managers;
   3. `[RuntimeInitializeOnLoadMethod]` registration where the system registers ITSELF into a static service point at load (WAVE07 precedent: InventoryPanelController/CharacterEquipmentPanelController) — for controllers that must exist before scene wiring.
3. **Missing dependency = loud wiring error, never silent fallback search:**

```csharp
Debug.LogError($"WiringError: scene={gameObject.scene.name} object={name} component={GetType().Name} missingField=_questService affectedId={questId}");
```

4. Scene creators are the wiring source of truth: when adding a serialized field, update the corresponding `CreateMvp*Scene` generator in the same change (skill: scene-interactable-wiring) and document any Inspector wiring needed by the human.
5. Bind to gameplay flow via GameEventBus events, not by polling other systems' state in `Update()`.

## When touching an EXISTING *RuntimeBootstrap

- Spec scope includes it → migrate its lookups to the resolution order above and remove the `FindObjectOfType` calls.
- Spec scope does NOT include it → leave it, but list it in the execution report under known debt.

## Checklist for orphan-system wiring specs (fable_15/16 style)

- [ ] Audit which manager/service is orphaned and who should own its lifecycle (GameBootstrap vs. scene creator).
- [ ] Wire via the resolution order above; no new global searches.
- [ ] `*WiringTests` EditMode test asserting registration completeness (precedent: `OrphanSystemsWiringTests`).
- [ ] Human Play Mode scenario for the bound behavior (skill: gameplay-test-scenario).
