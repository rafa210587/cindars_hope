# WAVE_INTEGRATION_13 — Scene Transition Authoring Model

## How to Add a New Scene

1. Create the `.unity` scene file in Unity Editor: File > New Scene
2. Save to `Assets/_Game/Scenes/<SceneName>.unity`
3. Add to Build Settings: File > Build Settings > Add Open Scenes
4. Add a SceneId constant in `SceneId.cs` mapping to the scene name
5. Add the scene to WAVE_INTEGRATION_13_SCENE_TOPOLOGY_MAP.md

## How to Add a New Transition Gate

1. In Unity Editor, open the source scene
2. Create an empty GameObject at the gate location
3. Add component: `SceneTransitionGate`
4. Set fields:
   - `TransitionId`: use a constant from SceneId (e.g., "gate_farm_town_exit")
   - `FromSceneId`: source scene name (e.g., "FarmScene")
   - `ToSceneId`: destination scene name (e.g., "TownScene")
   - `TargetSpawnAnchorId`: destination spawn anchor (e.g., "spawn_town_from_farm")
   - `InteractionPrompt`: display text (e.g., "Ir para a Cidade")
5. Optionally add a Collider2D for trigger detection by the interaction system
6. Save scene

## How to Add a New Spawn Anchor

1. In Unity Editor, open the destination scene
2. Create an empty GameObject at the spawn position
3. Add component: `SceneSpawnAnchor`
4. Set `SpawnAnchorId` to the canonical anchor ID (e.g., "spawn_town_from_farm")
5. Optionally set `FacingDirection` (1 = right, -1 = left)
6. Save scene

## How to Wire PlayerSpawnResolver

Each gameplay scene should have one `PlayerSpawnResolver` component:
1. Create an empty GameObject (e.g., "SceneSetup")
2. Add component: `PlayerSpawnResolver`
3. Assign `PlayerTransform`: drag the Player GameObject's Transform
4. Set `DefaultSpawnAnchorId`: the fallback anchor (e.g., "spawn_farm_default")

Note: Alternatively, the existing `SceneSpawnInstaller` + `SceneSpawnPoint` pattern works too.
`PlayerSpawnResolver` checks `SceneSpawnAnchor` first, then falls back to `SceneSpawnPoint`.

## Build Settings

All gameplay scenes must be in Build Settings for standalone builds:
- File > Build Settings
- Open each gameplay scene
- Click "Add Open Scenes"

Required order (suggested):
1. FarmScene (index 0)
2. TownScene (index 1)
3. CaveScene (index 2)

See WAVE_INTEGRATION_13_HUMAN_UNITY_BUILD_SETTINGS_INSTRUCTIONS.md for step-by-step.

## State Preservation Authoring

Current architecture: per-scene GameBootstrap. State preserved only if:
- Manager uses ScriptableObject-backed data (persists across scene loads)
- Manager uses DontDestroyOnLoad (survives transitions)
- Manager saves/loads via SaveManager before/after transitions

To add state preservation for a new state type:
1. Identify if data is SO-backed (good) or in-memory (bad)
2. If in-memory: add save hook in OnDestroy() before scene unload
3. If SO-backed: verify SO is not reset in Awake()
4. Document in STATE_PRESERVATION_MATRIX

## Known Debts

| Debt | Impact | Mitigation |
|------|--------|-----------|
| No PersistentManagers scene | Health/stamina/cooldowns may reset on Farm→Town | Use SO-backed data or add auto-save before transition |
| No fade/loading screen | Scene pops instantly | Visual polish deferred; low gameplay impact |
| No input lock during transition | Player could interact twice | Transition guard in SceneTransitionRouter mitigates |
| Build Settings empty | Standalone builds fail | Human must add scenes manually |
| TownScene GameBootstrap unknown | Economy/inventory may fail in Town | Human must audit TownScene managers |
