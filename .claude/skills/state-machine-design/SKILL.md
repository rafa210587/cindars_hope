---
name: state-machine-design
description: Design a finite state machine for player, enemy, boss, UI flow or game flow when there are many states/transitions/phases or conflicting boolean flags. Produces a pure-C# (EditMode-testable) FSM with Enter/Tick/Exit and an explicit invalid-transition policy. Use before adding "just one more bool" to a system that already juggles several.
---

# Skill: State Machine Design

Use this when a system is starting to track state with several booleans (`isAttacking`, `isStunned`, `canMove`, `isDead`...) that can contradict each other. Distinct from skill `enemy-ai-authoring` (which authors *AI behaviors/moves/affixes* on top of the live `EnemyBrain`): this skill is about the **state structure itself**.

## When NOT to use

- One or two genuinely independent flags → leave them; an FSM adds ceremony.
- Boss *phase* behavior on the existing brain → use `enemy-ai-authoring` (precedent: `BossPhaseLogic`); apply this skill only if the phase wiring itself is a flag tangle.

## Procedure

1. **Enumerate states.** List every state explicitly (an `enum` or one class per state). If two booleans can't be true at once, they are one state machine, not two flags.
2. **Enumerate transitions and their triggers.** For each state, what events/conditions leave it and to where. Triggers come from input, AI, timers, or `GameEventBus` events — not from another system poking a field.
3. **Separate per-state data from shared data.** Per-state timers/counters live with the state; shared context (stats, refs) is passed in.
4. **Enter / Tick / Exit.** Each state implements entry side-effects, per-step logic (take `deltaTime`), and cleanup. Cleanup in `Exit` prevents leaked timers/subscriptions.
5. **Define the invalid-transition policy.** Ignore + log, clamp to a safe state, or throw in dev (rule `error-handling-resilience`, category 4). Decide it; don't leave it implicit.
6. **Keep the machine pure.** The FSM is plain C# (no `UnityEngine`); the `MonoBehaviour` feeds it input/`deltaTime` and reacts to its emitted domain events (rule `gameplay-design-patterns`). This is what makes it testable.
7. **Emit domain events, don't call presentation.** State changes publish via `GameEventBus`; VFX/SFX/anim/HUD subscribe (skill `game-feel-checklist`). The FSM never calls UI/audio directly.

## Output

- State list + transition table (from → trigger → to).
- Interfaces/classes (`IState` with Enter/Tick/Exit, a small `StateMachine` driver).
- Invalid-transition policy, stated.
- Which events the machine publishes for presentation.
- EditMode tests (skill `editmode-test-authoring`): valid transitions land in the expected state; invalid transitions follow the policy; Enter/Exit side-effects fire once; deterministic given the same inputs.

## Save interaction

If the state survives a reload, persist a **stable state id** (string/enum), never the state object — resolve back to the state on load (rule `unity-architecture` save DTOs; skill `save-load-pattern`). Persist the outcome/which-state, not transient per-tick timers unless the design requires it.
