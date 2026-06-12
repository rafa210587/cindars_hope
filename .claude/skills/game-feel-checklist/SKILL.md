---
name: game-feel-checklist
description: Minimum feedback ("juice") checklist for player-facing interactions — combat hits, blocks, harvests, pickups, failures. Use for combat/movement/ability specs (fable_27 perfect block, fable_02 weapon actions, fable_05 boss) and any spec where the player performs a physical action.
---

# Skill: Game Feel Checklist

A mechanic without feedback reads as broken even when the logic is correct. This checklist defines the MINIMUM feedback per interaction, using the systems that already exist.

## Existing feedback infrastructure (reuse, don't reinvent)

- Events: `PlayerActionFeedbackEvent`, `PlayerHitEvent`, `StatusAndDamageEvents`, `CombatPostureEvents` (`Core/Events/`), HUD notification events (wave_integration_08).
- Telegraphs: `EnemyTelegraphController` + `EnemyTelegraphProfileSO` — enemy attacks announce themselves.
- Validator precedent: `ValidateSpec14AFix2CombatFeedback`.

> **Known foundation gap: the project has ZERO audio code** (no AudioSource/AudioClip anywhere in Scripts/). Audio items below are DEFERRED until an audio foundation spec exists — but every spec should still PUBLISH the feedback event so audio can subscribe later without touching gameplay code. Recommend an `audio foundation` spec before polish waves.

## Checklist per interaction type

### Player attack / ability (fable_02, fable_27)

- [ ] Windup readable (animation/sprite swap or scale pulse) before the active frames
- [ ] Impact: hit flash on target + knockback (existing `KnockbackForce` path) + damage number/HUD event
- [ ] Whiff feedback: missing must still show the swing (no input-eaten feeling)
- [ ] Cooldown/stamina refusal surfaces a reason via HUD event — never a silent no-op (project convention: `FailureReason` strings)
- [ ] Input buffering: presses during recovery queue the next action within a small window (~0.15s) instead of dropping
- [ ] **Perfect block (fable_27)**: the timing window must be data-tunable (SO field, not constant); success gets DISTINCT feedback from normal block (bigger flash + posture event); telegraph duration of the incoming attack defines the fairness of the window

### Taking damage

- [ ] Player hit: flash + brief knockback + HUD health reaction via `PlayerHitEvent` — never only a number changing
- [ ] Invulnerability window after hit must be visible (blink), not just internal state

### Farm / interactables (crops, mining, chopping)

- [ ] Each tool hit shows progress (sprite stage, shake or particle) — depletion guard already exists in interactables (skill: scene-interactable-wiring)
- [ ] Yield feedback: item fly-to-inventory or pickup pop + HUD notification event
- [ ] Invalid target/no-stamina shows reason (HUD event), not silence

### UI confirmations

- [ ] Purchases, crafts, quest turn-ins publish a feedback event the HUD can toast (already the wave_integration_08 pattern)

## Rules

1. **Feedback travels via GameEventBus events** — gameplay publishes, presentation subscribes (rule: unity-architecture). Never call UI/VFX directly from combat code.
2. **All timing values (windows, flash durations, buffer sizes) live in SOs/profiles** — feel is tuned in data, recompiling kills iteration.
3. **Pooled visuals**: damage numbers/hit particles spawn per hit — pool them (skill: object-pooling-pattern).
4. **Telegraph fairness**: enemy attack telegraph duration ≥ player reaction budget (~0.25s at minimum); boss one-shot moves need longer, distinct telegraphs (skill: enemy-ai-authoring).

## Closeout

Game-feel items are Play Mode-visible by nature: the human test scenario (skill: gameplay-test-scenario) must include a "feedback presence" section listing each checklist item as a check. EditMode covers the deterministic parts (buffer window math, perfect-block window evaluation, event published on state change).
