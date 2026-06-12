---
name: enemy-ai-authoring
description: Author enemy AI behaviors, boss phases, pack coordination and enemy moves using the live EnemyBrain + action/telegraph database pattern. Use for fable_04 (threat/pack coordination), fable_05 (cave boss phase AI), fable_24 (enemy moves/elite affixes) or any enemy behavior change.
---

# Skill: Enemy AI Authoring

## The LIVE system (use this)

`EnemyBrain` (`Assets/_Game/Scripts/Enemy/EnemyBrain.cs`) is the runtime brain:

- **Informal state machine**: `EnemyBrainState` enum (Idle/patrol/chase/retreat...), decisions on a tick (`_decisionTickSeconds` = 0.3s default), NOT per-frame.
- **Data-driven actions**: `EnemyActionSetDatabaseSO` → `EnemyActionSetSO` → `EnemyActionSO` (SPEC 13D), with per-action cooldowns tracked in `EnemyActionRuntime`.
- **Telegraphs**: `EnemyTelegraphProfileDatabaseSO` + `EnemyTelegraphController` — attacks announce before they hit.
- **Profiles**: `EnemyDataSO` (stats), `EnemyMovementProfileSO`, `EnemyVulnerabilityProfileSO` (14A-FIX4).
- **Packs/spawn**: `EnemySpawnPackSO`, `EnemySpawnProfileSO`, `EnemySpawnResolver` (ecology), `EnemyRoomSizeClass`.
- Movement executors: `EnemyChaseController` / `EnemyPatrolController` (Combat/), contact damage separate.

> **WARNING — orphan contract:** `AIBehaviorSO` (`Enemy/AIBehaviorSO.cs` + 3 assets in `Data/Enemy/AI/`) is referenced by NOTHING in runtime — only its editor initializer. Do not wire new behavior through it without a spec decision; either fable_04/05 integrates it deliberately or it should be retired (same pattern as the retired Scripts/Crafting). See skill `system-reuse-audit`.

## Rules for new behaviors

1. **New behavior = data first.** Prefer a new `EnemyActionSO` + entry in an action set over new hardcoded branches in EnemyBrain. Tuning values live in SOs/profiles, never as magic numbers (`_leapCooldownSeconds`-style serialized tuning is the existing idiom for brain-level params).
2. **Every damaging action needs a telegraph** (profile in the telegraph database) — windup the player can read. No telegraph = NEEDS_REWORK for combat fairness.
3. **Decisions on the tick, reactions on events.** Don't add per-frame logic to the brain; the 0.3s decision tick is the budget. Physics stays in the movement controllers.
4. **Boss phases (fable_05)**: model each phase as an explicit state with entry conditions on health thresholds (e.g., 100/60/30%), one-way transitions (no phase regression unless spec says so), per-phase action set swap (`_activeActionSet`), and a telegraphed phase-transition moment (invulnerable window + visual cue). Persist current phase in the cave snapshot if the boss can be left mid-fight (rule: cave-stable-run).
5. **Pack coordination (fable_04)**: coordination via shared deterministic data (spawn-pack roles, seeded flank side — the existing `s_nextBlinkFlankSide` alternation is the precedent), NOT via enemies searching the scene for each other (rule: unity-architecture). Pack composition comes seeded from the spawn resolver (rule: cave-stable-run — same level, same pack).
6. **Determinism**: any random choice that affects a saved/revisited cave level uses the seeded RNG pattern (skill: rng-and-determinism). Visual jitter may use UnityEngine.Random.

## Testability

EnemyBrain is a MonoBehaviour — extract decision RULES into pure C# (e.g., a phase-threshold resolver, action-eligibility evaluator) so fable_04/05 logic gets EditMode tests (skill: editmode-test-authoring): phase entry at exact thresholds, no phase regression, action cooldown gating, pack role assignment determinism. Live chase/feel behavior goes to a human Play Mode scenario (skill: gameplay-test-scenario).

## Closeout

- Validator coverage for new action/telegraph entries (skill: editor-validator-authoring): every action in a set exists in the action database; every action with damage has a telegraph profile.
- Asset generation evidence for new SO assets (rule: unity-assets).
