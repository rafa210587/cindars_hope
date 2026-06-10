# WAVE_INTEGRATION_13 — Human Unity Build Settings Instructions

## Why This Is Required

Unity's `SceneManager.LoadScene(sceneName)` requires scenes to be in Build Settings
for standalone builds. Without this, the build will fail to load scenes.

In Unity Editor Play Mode, `SceneTransitionRouter` falls back to
`EditorSceneManager.LoadSceneInPlayMode(path)` using the file path, so transitions
work in-editor without Build Settings.

For a standalone Windows/Mac/Linux build, all gameplay scenes MUST be in Build Settings.

## Current Status

`EditorBuildSettings.asset` audit result: `m_Scenes: []` — EMPTY.

No scenes are currently registered.

## Step-by-Step Instructions

### Method 1: Add Open Scenes (Recommended)

1. Open Unity Editor
2. Open FarmScene: Assets/_Game/Scenes/FarmScene.unity
3. Go to: File > Build Settings (Ctrl+Shift+B)
4. Click "Add Open Scenes"
5. FarmScene appears in the Scenes In Build list
6. Close Build Settings
7. Open TownScene: Assets/_Game/Scenes/TownScene.unity
8. Open Build Settings again
9. Click "Add Open Scenes"
10. Close Build Settings
11. Open CaveScene: Assets/_Game/Scenes/CaveScene.unity
12. Open Build Settings again
13. Click "Add Open Scenes"
14. Close Build Settings

### Method 2: Drag and Drop

1. Open Build Settings: File > Build Settings
2. In Project window: Assets/_Game/Scenes/
3. Drag FarmScene.unity into the "Scenes In Build" area
4. Drag TownScene.unity
5. Drag CaveScene.unity
6. Ensure order: FarmScene (0), TownScene (1), CaveScene (2)

## Recommended Build Settings Order

| Index | Scene | Path |
|-------|-------|------|
| 0 | FarmScene | Assets/_Game/Scenes/FarmScene.unity |
| 1 | TownScene | Assets/_Game/Scenes/TownScene.unity |
| 2 | CaveScene | Assets/_Game/Scenes/CaveScene.unity |

Index 0 is the default startup scene in standalone builds.
FarmScene at index 0 ensures the game starts at the farm.

## Verification

After adding scenes, `EditorBuildSettings.asset` should show non-empty `m_Scenes` list.

You can verify by checking:
- File > Build Settings shows 3 scenes with checkmarks
- Scenes have no red ! icon (which indicates a missing file)

## Note on Editor Play Mode vs Standalone

| Mode | Requires Build Settings? |
|------|--------------------------|
| Editor Play Mode (from Unity) | NO — SceneTransitionRouter uses EditorSceneManager fallback |
| Standalone build | YES — must have all scenes registered |

For Play Mode testing of WAVE_INTEGRATION_13, Build Settings are not required.
For final deliverable (build for player), Build Settings are mandatory.
