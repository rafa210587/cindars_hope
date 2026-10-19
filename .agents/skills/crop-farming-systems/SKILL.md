---
name: crop-farming-systems
description: Extend crop, soil, watering, fertilizer or harvest behavior. Use for farm growth and yield changes that must follow day transitions, deterministic data, save and economy contracts.
---

# Skill: Crop farming systems

Growth advances from the canonical day transition. Do not poll crop business state in Update
or create a parallel farm clock, inventory, economy or save model.

## Essential workflow
1. Read the active spec and inspect the current farm/time/save owners before editing.
2. Identify the affected lifecycle: soil preparation, planting, watering, daily growth,
   harvest, depletion and persistence. Keep balance/content in ScriptableObjects.
3. Read [domain contracts](references/domain-contracts.md) for implementation involving time,
   weather, RNG, scene nodes, economy or save. Load only the relevant section.
4. Validate behavior and failure boundaries using
   [validation and closeout](references/validation-and-closeout.md).
5. Publish cross-system gameplay changes through GameEventBus and unsubscribe correctly.
   Preserve stable IDs and simple save DTOs.

Route time/calendar changes to `time-calendar-weather`, balance tuning to
`economy-balance-tuning`, scene objects to `scene-interactable-wiring`, deterministic
randomness to `rng-and-determinism`, and persistence changes to `save-load-pattern`.
Report implemented crop states, data/wiring, tested transitions and residual runtime risks.
