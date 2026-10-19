# WAVE_INTEGRATION_13 — Scene Transition Map

## Scenes

| SceneId | Scene Name | Path | Status |
|---------|-----------|------|--------|
| scene_farm | FarmScene | Assets/_Game/Scenes/FarmScene.unity | EXISTS |
| scene_town | TownScene | Assets/_Game/Scenes/TownScene.unity | EXISTS (PLACEHOLDER) |
| scene_cave | CaveScene | Assets/_Game/Scenes/CaveScene.unity | EXISTS |

## Transitions (Minimum 4)

| Transition ID | From Scene | To Scene | Gate ID | Spawn ID | Gate Component | Status |
|--------------|-----------|---------|---------|---------|----------------|--------|
| transition_farm_to_town | scene_farm | scene_town | gate_farm_town_exit | spawn_town_from_farm | SceneTransitionGate | CODE_READY — human must place in FarmScene |
| transition_town_to_farm | scene_town | scene_farm | gate_town_farm_exit | spawn_farm_from_town | SceneTransitionGate | CODE_READY — human must place in TownScene |
| transition_farm_to_cave | scene_farm | scene_cave | gate_farm_cave_entrance | spawn_cave_from_farm | SceneTransitionGate | CODE_READY — human must place in FarmScene |
| transition_cave_to_farm | scene_cave | scene_farm | gate_cave_farm_exit | spawn_farm_from_cave | SceneTransitionGate | CODE_READY — CaveExitPortal also exists as alternative |

## Spawn Anchors

| Anchor ID | Scene | Purpose | Component | Status |
|----------|-------|---------|-----------|--------|
| spawn_farm_from_town | FarmScene | Player position on arrival from Town | SceneSpawnAnchor | CODE_READY — human must place |
| spawn_farm_from_cave | FarmScene | Player position on return from Cave | SceneSpawnAnchor | CODE_READY — human must place |
| spawn_farm_default | FarmScene | Default/new-game spawn | SceneSpawnAnchor or SceneSpawnPoint | CODE_READY — may already exist as SceneSpawnPoint |
| spawn_town_from_farm | TownScene | Player position on arrival from Farm | SceneSpawnAnchor | CODE_READY — human must place |
| spawn_cave_from_farm | CaveScene | Player position on arrival from Farm | SceneSpawnAnchor | CODE_READY — human must place |

## State Preservation

| State | Preserved? | Mechanism |
|-------|-----------|-----------|
| Inventory | LIKELY_YES | SO-backed |
| Gold | LIKELY_YES if EconomyManager in dest | SO-backed |
| Skills | LIKELY_YES | SO-backed |
| Health/Stamina | LIKELY_NO | In-memory, scene-local |
| Quest flags | LIKELY_YES | SaveManager |
| Current scene ID | YES | SceneTransitionState (cleared after spawn) |

## Relationship with Legacy CaveExitPortal

CaveExitPortal.cs (CaveScene) already handles Cave→Farm transition via:
- SceneTransitionState.SetPendingSpawn("farm_from_cave")
- SceneTransitionStartedEvent published
- SceneManager.LoadScene("FarmScene")

The new SceneTransitionGate (gate_cave_farm_exit) uses the same SceneTransitionRouter and
SceneTransitionState pattern. Both can coexist. Human should choose one and remove the other
during scene wiring, or keep both as redundant paths.

Recommended: Replace CaveExitPortal with SceneTransitionGate for consistency with stable ID contracts.
