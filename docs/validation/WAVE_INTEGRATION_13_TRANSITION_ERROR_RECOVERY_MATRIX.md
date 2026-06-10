# WAVE_INTEGRATION_13 — Transition Error Recovery Matrix

## Coverage: 9 Failure Modes

| # | Failure | Detection | Recovery | User Feedback | Blocks? |
|---|---------|-----------|----------|--------------|---------|
| 1 | Missing scene (LoadScene fails) | SceneManager throws exception | None automatic — game enters undefined state | None in MVP | YES — game crash or black screen |
| 2 | Scene not in Build Settings (standalone) | SceneManager.LoadScene returns immediately with error | Editor fallback: EditorSceneManager.LoadSceneInPlayMode with file path | Debug.LogWarning in Console | YES in build; NO in Editor (fallback) |
| 3 | Missing spawn anchor (no SceneSpawnAnchor or SceneSpawnPoint found) | PlayerSpawnResolver logs Warning | Player stays at current position (no crash) | Debug.LogWarning in Console | NO — game continues at unexpected position |
| 4 | Missing player transform in PlayerSpawnResolver | PlayerSpawnResolver.Start() logs Warning | Spawn skipped; player not repositioned | Debug.LogWarning in Console | NO — but player stuck at origin |
| 5 | Missing camera target after spawn | CameraFollow2D continues pointing at last known position or null | Scene continues, camera may be off-screen | None — visual glitch | NO |
| 6 | Transition in progress (re-entrant call) | SceneTransitionRouter._transitionInProgress guard | Request rejected; second transition ignored | Debug.LogWarning in Console | NO |
| 7 | Input locked after failure | Not implemented | No auto-unlock mechanism in MVP | None | POTENTIAL — if future input lock added without cleanup |
| 8 | HUD missing after transition | DebugHud / RuntimeInitializeOnLoad panels survive via DontDestroyOnLoad | N/A — HUD should survive | None | NO |
| 9 | Duplicate managers in destination scene | GameBootstrap._instance singleton | Second instance logs Warning + destroys self | Debug.LogWarning in Console | NO |

## Notes

### Failure 1 (Missing scene)
SceneManager.LoadScene will fail silently or throw exception if sceneName is wrong.
Prevention: use SceneId constants exclusively; never hardcode scene name strings.

### Failure 2 (Build Settings)
Editor Play Mode always works because SceneTransitionRouter uses EditorSceneManager.LoadSceneInPlayMode
with path fallback. Standalone builds require Build Settings entries.

See WAVE_INTEGRATION_13_HUMAN_UNITY_BUILD_SETTINGS_INSTRUCTIONS.md.

### Failure 3 (Missing spawn)
The recovery is graceful but results in player at wrong position. If spawn_town_from_farm is missing
in TownScene, player spawns at origin (0,0). Human must verify anchors are placed correctly.

### Failure 9 (Duplicate managers)
If TownScene or CaveScene has its own GameBootstrap AND FarmScene's bootstrap somehow persists,
both would conflict. Prevention: GameBootstrap._instance singleton check destroys duplicate.
This is standard Unity singleton pattern; no additional code needed.
