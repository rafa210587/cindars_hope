# WAVE_INTEGRATION_13 — Scene Transitions: Farm, Town e Cave Playable Path

<!-- /speckit.specify -->
# /speckit.specify

<!-- /speckit.plan -->
# /speckit.plan

<!-- /speckit.tasks -->
# /speckit.tasks

required_adrs: []
required_game_rules: []

## Ordem de execucao

WAVE_INTEGRATION_13 — executa após WAVE_INTEGRATION_12C_SHOP_PRICE_FIX.

## Depende de

- WAVE_INTEGRATION_12: BUILD_VALIDATED_WITH_NPC_DIALOGUE_SHOP_DEBT_PENDING_HUMAN_PLAYMODE

## Bloqueia

- WAVE_INTEGRATION_14 (PersistentManagers scene — not yet planned)

## Spec Identity

| Field | Value |
|-------|-------|
| Spec ID | WAVE_INTEGRATION_13 |
| Wave | WAVE_INTEGRATION |
| Status | IN_PROGRESS |
| Created | 2026-06-09 |
| Author | Agent |
| Priority | P0 — Required for playable path |

## Goal

Implement code-ready scene transition runtime for Farm↔Town and Farm↔Cave playable paths. Provides stable IDs, transition gate MonoBehaviour, spawn anchor MonoBehaviour, transition router, player spawn resolver, and 11 mandatory documentation files. Scene wiring (placing GameObjects in .unity files) is deferred to human Unity Editor action.

## Dependency

- WAVE_INTEGRATION_12: BUILD_VALIDATED_WITH_NPC_DIALOGUE_SHOP_DEBT_PENDING_HUMAN_PLAYMODE — DONE, not blocked

## Existing Infrastructure (Reutilize)

| File | Path | Role |
|------|------|------|
| SceneTransitionStartedEvent | Assets/_Game/Scripts/Core/Events/SceneTransitionStartedEvent.cs | Transition event (source, target, spawnId) |
| SceneTransitionCompletedEvent | Assets/_Game/Scripts/Core/Events/SceneTransitionCompletedEvent.cs | Completion event (scene, spawnId, position) |
| SceneTransitionState | Assets/_Game/Scripts/SceneManagement/SceneTransitionState.cs | Static pending spawn ID |
| ScenePortal | Assets/_Game/Scripts/SceneManagement/ScenePortal.cs | IInteractable, loads scene, publishes events |
| SceneSpawnInstaller | Assets/_Game/Scripts/SceneManagement/SceneSpawnInstaller.cs | Resolves spawn ID, repositions player |
| SceneSpawnPoint | Assets/_Game/Scripts/SceneManagement/SceneSpawnPoint.cs | MonoBehaviour with spawnId |
| SceneNames | Assets/_Game/Scripts/SceneManagement/SceneNames.cs | Scene name constants |
| CaveSpawnAnchor (enum) | Assets/_Game/Scripts/Cave/Runtime/CaveSpawnAnchor.cs | Cave-specific anchor enum |
| IInteractable | Assets/_Game/Scripts/Interaction/IInteractable.cs | Interaction interface |

## New Files to Create

| File | Path | Notes |
|------|------|-------|
| SceneId | Assets/_Game/Scripts/World/Scenes/SceneId.cs | Stable scene ID constants mapping to SceneNames |
| SceneTransitionRequest | Assets/_Game/Scripts/World/Scenes/SceneTransitionRequest.cs | Request DTO |
| SceneTransitionResult | Assets/_Game/Scripts/World/Scenes/SceneTransitionResult.cs | Result DTO |
| SceneSpawnAnchor | Assets/_Game/Scripts/World/Scenes/SceneSpawnAnchor.cs | MonoBehaviour with stable spawnAnchorId |
| SceneTransitionGate | Assets/_Game/Scripts/World/Scenes/SceneTransitionGate.cs | IInteractable gate, delegates to ScenePortal logic |
| SceneTransitionRouter | Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs | Service that processes SceneTransitionRequest |
| PlayerSpawnResolver | Assets/_Game/Scripts/World/Scenes/PlayerSpawnResolver.cs | Finds anchor by ID, repositions player |
| ValidateSceneTransitions | Assets/_Game/Scripts/Editor/Validation/ValidateSceneTransitions.cs | Editor-only validator |

## Stable IDs

### Scene IDs
- `scene_farm` → SceneNames.Farm → "FarmScene"
- `scene_town` → SceneNames.Town → "TownScene"
- `scene_cave` → SceneNames.CaveRuntime → "CaveScene"

### Gate IDs
- `gate_farm_town_exit`
- `gate_farm_cave_entrance`
- `gate_town_farm_exit`
- `gate_cave_farm_exit`

### Spawn Anchor IDs
- `spawn_farm_from_town`
- `spawn_farm_from_cave`
- `spawn_town_from_farm`
- `spawn_cave_from_farm`
- `spawn_farm_default`

## Transition Map

| Transition ID | From | To | Gate | Spawn |
|--------------|------|----|------|-------|
| transition_farm_to_town | scene_farm | scene_town | gate_farm_town_exit | spawn_town_from_farm |
| transition_town_to_farm | scene_town | scene_farm | gate_town_farm_exit | spawn_farm_from_town |
| transition_farm_to_cave | scene_farm | scene_cave | gate_farm_cave_entrance | spawn_cave_from_farm |
| transition_cave_to_farm | scene_cave | scene_farm | gate_cave_farm_exit | spawn_farm_from_cave |

## Architecture Rules

- No `GameObject.Find` / `FindObjectOfType` at runtime
- No `DontDestroyOnLoad` on scene-specific objects
- Event bus via `GameEventBus.Publish` for transition events
- All IDs stable strings (not GameObject.name)
- FADE_LOADING_DEFERRED_WITH_REASON — no fade system exists

## Scene Changes

Scene YAML editing is FORBIDDEN per `unity-yaml-editing-policy.md`. All scene wiring must be performed by human in Unity Editor following the authoring instructions.

## Final Status

`CODE_READY_HUMAN_UNITY_ACTION_REQUIRED` — all C# code ready; scene gates/anchors and Build Settings require human Unity Editor action.

## Required Documentation (11 mandatory + 2 optional)

See Step D in execution instructions.

## Testing Quality Gate

- Changed runtime code: YES
- Changed deterministic logic: NO (router delegates to SceneManager)
- Changed Unity scene/prefab/asset wiring: NO (human action required)
- Automated tests added: NO (PlayMode/scene wiring dependent)
- Manual Play Mode scenario: docs/validation/WAVE_INTEGRATION_13_HUMAN_PLAYMODE_CHECKLIST.md
- Justification: Transition behavior requires Unity scene with placed GameObjects; EditMode automation not practical
- Residual risk: All transitions untested until human wires scenes and runs Play Mode checklist
