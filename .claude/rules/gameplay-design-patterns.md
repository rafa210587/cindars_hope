# Rule: Gameplay Design Pattern Selection

Pattern *selection* guidance. The hard invariants (GameEventBus for gameplay comms, no runtime global search, save DTOs) are in [unity-architecture.md](./unity-architecture.md); this rule says **which pattern to reach for**, mapped to systems that already exist here so new code matches the house style instead of inventing a parallel one.

## Core principle: domain logic in pure C#, engine as adapter

Keep tunable/testable gameplay rules in plain C# (no `UnityEngine` dependency) so they are EditMode-testable; let the `MonoBehaviour` adapt input, scene, physics, prefabs and UI. The project's projection/ViewModel convention (skill `ui-projection-pattern`) is the canonical example — copy it for new screens and systems, and see skill `monobehaviour-decomposition` to pull logic out of a god-`MonoBehaviour`.

## Pattern → when → project precedent

| Pattern | Reach for it when | Precedent / skill |
|---------|-------------------|-------------------|
| **State Machine** | many states/transitions/phases or conflicting boolean flags (player, enemy, boss, UI flow, game flow) | `BossPhaseLogic`; skill `state-machine-design` |
| **Strategy** | the rule changes by type/config (reward grant, pricing, drop selection) | reward strategy in scene interactables; skill `scene-interactable-wiring` |
| **Command** | an action can come from input, AI, replay or a queue | input/action handling |
| **Event Bus (Observer)** | different systems react to the same domain fact | `GameEventBus` (rule `event-bus-only-gameplay-communication`; skill `event-bus-pattern`) — **mandatory** for gameplay comms, not optional |
| **Factory** | creation needs prefab/config/deps/pooling | `ProceduralSfxFactory`, spawn services; skill `bootstrap-wiring` |
| **Object Pool** | frequent spawn/despawn (projectiles, floating text, drops, wave enemies, SFX) | skill `object-pooling-pattern` (project baseline: no pooling yet) |
| **Adapter / Ports** | isolate engine, save, audio, analytics from domain | projections; skill `runtime-bootstrap-pattern` |
| **Decorator / Modifier** | composable buffs/debuffs/affixes | `StatusEffectDatabase`; skills `enemy-ai-authoring`, `ability-effect-composition` |
| **Null Object** | a safe default beats a null-check at every call site | category-2 fallback in rule `error-handling-resilience` |
| **Flyweight / Data Asset** | shared, designer-tuned data | ScriptableObjects (rule `data-driven-content`; skill `data-catalog-authoring`) |

## Anti-patterns to flag

- A generic `Manager`/`Controller` accumulating input + rule + UI + audio + save + animation → split (skill `monobehaviour-decomposition`).
- `switch` on a type enum that grows every time content is added → Strategy or polymorphism.
- Deep inheritance for capability variation → Component/composition.
- A new global singleton to share state that an explicit dependency or a `GameEventBus` event would carry.

## Enforcement

Reviewed by agent `architecture-reviewer` (pre-wave / post-integration) and skill `non-regression-review`. Run skill `system-reuse-audit` before creating any new manager/service/SO type so a "new pattern" doesn't become a duplicate system.
