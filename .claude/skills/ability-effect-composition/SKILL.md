---
name: ability-effect-composition
description: Design a composable ability/effect/modifier system — abilities built from small reusable effects (damage, heal, apply status, knockback) with validation/cost/cooldown/execution separated. Use when designing how abilities/spells/status effects COMPOSE, not when wiring existing combat data assets.
---

# Skill: Ability & Effect Composition

This is the **design** counterpart to two wiring skills: `combat-data-wiring` (populating `WeaponDatabase`/`SpellDatabase`/`StatusEffectDatabase`) and `player-ability-runtime` (non-slot movement abilities like dash/block). Reach for this skill when the question is "how should abilities be *built up from reusable pieces*" so a designer can author a new spell without new code.

## Procedure

1. **Separate data from runtime state.** The ability *definition* (id, cost, cooldown, effect list) is data (ScriptableObject, rule `data-driven-content`); the *runtime* (cooldown remaining, charges) is per-instance state.
2. **Model the execution context explicitly.** Caster id, target id(s), position, time, and **seed** travel in a context object — not pulled from globals. The seed makes outcomes deterministic and replayable (skill `rng-and-determinism`).
3. **Split the pipeline.** `CanExecute` (validation: target valid? in range?) → cost check → cooldown check → execute → apply effects. Each stage can fail as a category-1 expected failure (rule `error-handling-resilience`): return `bool` + `FailureReason`, surface via HUD event (skill `game-feel-checklist`), never throw.
4. **Effects are small and composable.** `IAbilityEffect.Apply(context)` units: deal damage, heal, apply status, knockback, spawn projectile. An ability is an ordered list of effects. Variation comes from new data, not new ability classes.
5. **Modifiers/buffs are decorators.** Buffs/debuffs/affixes wrap or adjust effect parameters (rule `gameplay-design-patterns`, Decorator) and route through `StatusEffectDatabase`. Define stacking and expiry rules up front.
6. **Deterministic effect order.** When order matters (DoT before death check, shield before damage), make it explicit and stable — don't depend on dictionary/iteration order.
7. **Emit domain events.** Each effect publishes `GameEventBus` events (`StatusAndDamageEvents`, hit/feedback events) so VFX/SFX/HUD react without combat code touching presentation (rule `event-bus-only-gameplay-communication`).

## Output

- Data model: `AbilityDefinition` (id, cost, cooldown, `IReadOnlyList<IAbilityEffect>`) + runtime state.
- Execution context (caster/target/position/time/seed).
- Pipeline with explicit failure points and reasons.
- Effect/modifier interfaces and the stacking/expiry policy.
- Events published for presentation.
- EditMode tests (skill `editmode-test-authoring`).

## Save & test expectations

- Persist the **outcome** (cooldowns remaining, active status ids + remaining duration) as simple types/ids — never effect objects (rule `unity-architecture`; skill `save-load-pattern`).
- Tests: cooldown gates re-use; cost is consumed only on success; same context+seed → identical effect outcome twice; status stacking/expiry; effect order deterministic; reload restores cooldowns/statuses without re-applying their on-apply effects (idempotency).
