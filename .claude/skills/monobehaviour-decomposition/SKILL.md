---
name: monobehaviour-decomposition
description: Safely decompose a large/god MonoBehaviour that mixes input, rules, UI, audio, save and animation into a thin adapter + pure-C# core, incrementally and without breaking scenes/prefabs. Use when a MonoBehaviour has grown to mix several responsibilities or is hard to test.
---

# Skill: MonoBehaviour Decomposition

The project's house style is a thin `MonoBehaviour` adapter over a pure-C# core (skill `ui-projection-pattern`, rule `gameplay-design-patterns`). This skill gets an existing god-`MonoBehaviour` to that shape **without** breaking scene/prefab serialization or changing behavior by accident.

## Hard constraints (read before extracting)

- **Don't reorder or rename `[SerializeField]` fields casually** — that breaks Inspector wiring in scenes/prefabs, and `.unity`/`.prefab` YAML edits are gated (rule `unity-assets`, `permissions.ask`). Keep serialized fields where they are; move *logic*, not field declarations, in the first passes.
- **Run skill `system-reuse-audit` first.** The extracted core may already exist (e.g., a service/projection). Extracting into a duplicate trips the `runtime-code-guard` duplicate-class check and creates a parallel system.

## Procedure

1. **Read the whole file first.** List every responsibility present: input, domain rule, presentation/UI, audio, animation, persistence, networking, engine lifecycle.
2. **Draw the seam.** Mark what is *pure rule* (no `UnityEngine`) vs *engine adapter* (input, components, prefabs, physics). The pure part is the extraction target.
3. **Extract pure C# first, smallest cohesive piece.** Move a rule into a plain class the `MonoBehaviour` owns and delegates to. No scene change, public API stable, behavior identical.
4. **Route cross-system calls through the bus.** If the god object was calling other gameplay objects directly, replace with `GameEventBus` publish/subscribe (rule `event-bus-only-gameplay-communication`) — this is usually the biggest coupling win.
5. **One step, one validation.** After each extraction: compile (skill `unity-validation`), and run the EditMode test you just added for the extracted core. Don't batch five extractions before validating.
6. **Add tests as you go.** Each extracted pure class gets EditMode coverage (skill `editmode-test-authoring`) — that coverage is the proof the refactor preserved behavior.
7. **Stop at the seam.** Leave the `MonoBehaviour` as a thin adapter: read input → call core → apply results / publish events / drive visuals. Don't gold-plate the parts you didn't need to touch (rule `00-operational-discipline`).

## Output

- Responsibility inventory (before).
- Extraction plan in safe order (pure rule → bus wiring → adapter slim-down).
- New class/projection structure (target shape).
- Validation + EditMode test after each step.
- Residual risk: any serialized-field move deferred, any behavior intentionally changed (must be explicit), Play Mode re-check if scene wiring was touched (skill `gameplay-test-scenario`).

## Do not

- Mix this refactor with a feature change in the same commit (rule `00-operational-discipline`).
- Rewrite the whole class at once — incremental, behavior-preserving steps only.
- Introduce `FindObjectOfType`/`GameObject.Find` to "simplify" wiring (rule `unity-architecture`).
