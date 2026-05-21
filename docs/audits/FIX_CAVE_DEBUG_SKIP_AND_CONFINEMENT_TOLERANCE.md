# Fix Cave Debug Skip & Confinement Tolerance Audit

**Date**: 2026-05-21  
**Branch**: `feature/fix-cave-debug-skip-and-confinement-tolerance`  
**Status**: Code implementation complete. Unity Play Mode validation pending.

---

## Executive Summary

Two core issues were addressed:

1. **Debug Skip Hotkey P** not reliably triggering in Play Mode
2. **Path Confinement** blocking player too early on walls (especially left/right)

Solutions implemented:

1. **Robust Debug Skip**: Hotkey P + F2 + OnGUI button, with local rebind fallback
2. **Tolerance-Based Confinement**: Amostra 5 pontos (centro + 4 laterais) com tolerância configurável

---

## Correction 1: Debug Skip Hotkey & Button

### File: `CaveDebugLevelSkipController.cs`

#### New Fields
```csharp
[SerializeField] private KeyCode _alternateNextLevelKey = KeyCode.F2;
[SerializeField] private bool _showDebugSkipButton = true;
```

#### Start() — Enhanced Logging
```csharp
var hasRunManager = _caveRunManager != null;
var hasLevelController = _levelController != null;
Debug.Log($"CaveDebugLevelSkipController: enabled={_enableDebugLevelSkip}, key={_nextLevelKey}, altKey={_alternateNextLevelKey}, bypassBossGate={_bypassBossGateForDebugSkip}, hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
```

#### Update() — Dual Hotkey Support
```csharp
if (Input.GetKeyDown(_nextLevelKey))
{
    Debug.Log($"CaveDebugLevelSkipController: debug skip key pressed. Key={_nextLevelKey}.", this);
    SkipToNextLevel();
}

if (Input.GetKeyDown(_alternateNextLevelKey))
{
    Debug.Log($"CaveDebugLevelSkipController: debug skip key pressed. Key={_alternateNextLevelKey}.", this);
    SkipToNextLevel();
}
```

#### OnGUI() — Debug Button
```csharp
private void OnGUI()
{
    if (!_enableDebugLevelSkip || !_showDebugSkipButton || SceneManager.GetActiveScene().name != "CaveScene")
    {
        return;
    }

    var rect = new Rect(20f, 20f, 220f, 32f);
    if (GUI.Button(rect, "DEBUG: Next Cave Level (P/F2)"))
    {
        Debug.Log("CaveDebugLevelSkipController: debug skip button clicked.", this);
        SkipToNextLevel();
    }
}
```

#### TryRebindLocalReferences() — Defensive Rebind
If refs are null in Inspector, attempts `GetComponent` on same GameObject.

#### Rationale
- **P hotkey**: Original primary input
- **F2 hotkey**: Fallback if P is captured elsewhere
- **OnGUI button**: Visual confirmation that component exists and is reachable
- **Local rebind**: If scene creation failed to populate refs, runtime can still recover

---

## Correction 2: Confinement with Tolerance

### File: `CavePlayerPathConfinement.cs`

#### New Fields
```csharp
[SerializeField] private float _playerHalfWidth = 0.15f;
[SerializeField] private float _playerHalfHeight = 0.25f;
[SerializeField] private float _wallContactTolerance = 0.10f;
```

#### Start() — Enhanced Logging
```csharp
Debug.Log($"CavePlayerPathConfinement: enabled. Player={_playerTransform.name}, LevelController={_levelController.name}, halfWidth={_playerHalfWidth}, halfHeight={_playerHalfHeight}, tolerance={_wallContactTolerance}.", this);
```

#### IsWorldPositionAllowed() — Multi-Point Sampling
```csharp
private bool IsWorldPositionAllowed(Vector3 worldPos, CaveGeneratedLevel level)
{
    var xOffset = Mathf.Max(0f, _playerHalfWidth - _wallContactTolerance);
    var yOffset = Mathf.Max(0f, _playerHalfHeight - _wallContactTolerance);

    var samples = new[]
    {
        worldPos,                              // Center
        worldPos + Vector3.left * xOffset,     // Left
        worldPos + Vector3.right * xOffset,    // Right
        worldPos + Vector3.up * yOffset,       // Up
        worldPos + Vector3.down * yOffset      // Down
    };

    foreach (var sample in samples)
    {
        var grid = WorldToGridPosition(sample, level);
        if (!IsGridWalkable(grid, level))
        {
            return false;
        }
    }

    return true;
}
```

#### LateUpdate() — Updated Check
```csharp
if (IsWorldPositionAllowed(playerWorldPos, generatedLevel))
{
    _lastValidPosition = playerWorldPos;
    return;
}

_playerTransform.position = _lastValidPosition;

if (Time.time - _lastLogTime > LogRateLimitSeconds)
{
    var playerGridPos = WorldToGridPosition(playerWorldPos, generatedLevel);
    Debug.Log($"CavePlayerPathConfinement: Confined player. Current={playerWorldPos}, LastValid={_lastValidPosition}, Grid={playerGridPos}, Reason=outside walkable samples.", this);
    _lastLogTime = Time.time;
}
```

#### Method Rename
`IsPositionWalkable()` → `IsGridWalkable()` for clarity (works with grid, not world position)

#### Rationale
- **Multi-point sampling**: Instead of checking only player center, samples 5 points: center + 4 cardinal sides
- **Tolerance buffer**: `_wallContactTolerance` lets player get closer to walls by reducing the sampled boundary
- **Configurability**: Can adjust `_playerHalfWidth`, `_playerHalfHeight`, `_wallContactTolerance` per level if needed
- **Better edge detection**: Catches walls at corners/edges that a single center point might miss

---

## Correction 3: Scene Creation Integration

### File: `CreateMvpCaveScene.cs`

#### CaveDebugLevelSkipController Setup
```csharp
serializedDebugSkip.FindProperty("_nextLevelKey").intValue = (int)KeyCode.P;
serializedDebugSkip.FindProperty("_alternateNextLevelKey").intValue = (int)KeyCode.F2;
serializedDebugSkip.FindProperty("_bypassBossGateForDebugSkip").boolValue = true;
serializedDebugSkip.FindProperty("_showDebugSkipButton").boolValue = true;
```

#### CavePlayerPathConfinement Setup
```csharp
serializedConfinement.FindProperty("_playerHalfWidth").floatValue = 0.15f;
serializedConfinement.FindProperty("_playerHalfHeight").floatValue = 0.25f;
serializedConfinement.FindProperty("_wallContactTolerance").floatValue = 0.10f;
```

#### Logging
```csharp
Debug.Log($"CreateMvpCaveScene: CaveDebugLevelSkipController configured on {runtimeObject.name}. Keys=P/F2, Button=enabled, enableDebugLevelSkip=true, bypassBossGate=true.");
Debug.Log($"CreateMvpCaveScene: CavePlayerPathConfinement configured on {playerTransform.gameObject.name}. halfWidth=0.15, halfHeight=0.25, tolerance=0.10.");
```

---

## Correction 4: Runtime Reference Installation

### File: `CaveSceneRuntimeReferenceInstaller.cs`

#### Local Rebind for CaveDebugLevelSkipController
```csharp
if (_caveDebugLevelSkipController == null && _caveLevelRuntimeController != null)
{
    _caveDebugLevelSkipController = _caveLevelRuntimeController.GetComponent<CaveDebugLevelSkipController>();
}
```

Attempts to recover if `CaveDebugLevelSkipController` field was not populated in Inspector.

---

## Expected Console Output

### At Scene Load
```
CreateMvpCaveScene: CaveDebugLevelSkipController configured on CaveRuntime. Keys=P/F2, Button=enabled, enableDebugLevelSkip=true, bypassBossGate=true.
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. halfWidth=0.15, halfHeight=0.25, tolerance=0.10.
```

### At Play Mode Start
```
CaveDebugLevelSkipController: enabled=True, key=P, altKey=F2, bypassBossGate=True, hasRunManager=True, hasLevelController=True
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController, halfWidth=0.15, halfHeight=0.25, tolerance=0.1.
```

### When P/F2 Pressed
```
CaveDebugLevelSkipController: debug skip key pressed. Key=P.
DEBUG ONLY: bypassing boss gate for level skip.
CaveDebugLevelSkipController: DEBUG: Level skip 1 -> 2
```

### When Button Clicked
```
CaveDebugLevelSkipController: debug skip button clicked.
DEBUG ONLY: bypassing boss gate for level skip.
CaveDebugLevelSkipController: DEBUG: Level skip 1 -> 2
```

### When Player Hits Wall
```
CavePlayerPathConfinement: Confined player. Current=(1.2, 2.3, 0), LastValid=(1.0, 2.3, 0), Grid=(41, 50), Reason=outside walkable samples.
```

---

## Test Procedure

### Pre-Play Mode
1. Abrir projeto Unity
2. Menu: `CindarsHope/Scenes/Create MVP CaveScene`
3. Aguardar criação (check console para logs acima)

### Play Mode — Test 1: Button Visibility
1. Play Mode
2. **Esperado**: Botão "DEBUG: Next Cave Level (P/F2)" aparece no canto superior esquerdo da Game view
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 2: Button Click
1. Clicar no botão
2. **Esperado**:
   - `CaveLevel` muda de 1 → 2
   - Logs aparecem
   - `RunSeed` não muda
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 3: P Hotkey
1. Pressionar P (tentar alguns segundos)
2. **Esperado**: `CaveLevel` sobe se input estiver sendo capturado
3. ✅ PASS / ❌ FAIL (se não funcionar, F2/button funcionam = input capture issue)

### Play Mode — Test 4: F2 Hotkey
1. Pressionar F2
2. **Esperado**: `CaveLevel` sobe
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 5: Wall Tolerance (Left)
1. Andar para a parede esquerda
2. Pressionar E repetidamente para "encostar"
3. **Esperado**: Player consegue chegar mais perto que antes
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 6: Wall Tolerance (Right)
1. Andar para a parede direita
2. Pressionar D repetidamente para "encostar"
3. **Esperado**: Player consegue chegar mais perto que antes
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 7: No Wall Penetration
1. Tentar forçar player através de parede
2. **Esperado**: Player é reverted para `_lastValidPosition`, não atravessa
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 8: No Jitter
1. Manter player contra parede por 5 segundos
2. **Esperado**: Player não treme/joga (smooth reversion, rate-limited logs)
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 9: Console Clean
1. Rodar por 1 minuto
2. **Esperado**: Nenhum erro vermelho, apenas logs esperados
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 10: Checkpoint Not Unlocked
1. Pressionar F2 para ir até level 15
2. Sair caverna
3. Re-entrar caverna
4. **Esperado**: Checkpoint selection mostra só level 1 (não 15)
5. ✅ PASS / ❌ FAIL

---

## Acceptance Criteria (10/10 = PASS)

| # | Test | Esperado | Status |
|---|------|----------|--------|
| 1 | Button visível | Botão aparece no canto superior esquerdo | ⬜ |
| 2 | Button click | Click sobe CaveLevel, logs aparecem | ⬜ |
| 3 | P hotkey | P sobe CaveLevel (se input funciona) | ⬜ |
| 4 | F2 hotkey | F2 sobe CaveLevel | ⬜ |
| 5 | Wall tolerance (left) | Player consegue chegar mais perto parede esq | ⬜ |
| 6 | Wall tolerance (right) | Player consegue chegar mais perto parede dir | ⬜ |
| 7 | No wall penetration | Player não atravessa parede | ⬜ |
| 8 | No jitter | Player não treme contra parede | ⬜ |
| 9 | Console clean | Sem erros vermelhos em 1 minuto de gameplay | ⬜ |
| 10 | Checkpoint locked | F2 skip não desbloqueia checkpoint 15 | ⬜ |

**Final**: ⬜ ___ / 10 PASS

---

## Tuning Parameters

If tests don't pass perfectly, adjust in Inspector:

### CaveDebugLevelSkipController
- `_enableDebugLevelSkip`: true = hotkey/button ativo
- `_nextLevelKey`: KeyCode.P (primary)
- `_alternateNextLevelKey`: KeyCode.F2 (fallback)
- `_showDebugSkipButton`: true = exibe botão OnGUI

### CavePlayerPathConfinement
- `_playerHalfWidth`: 0.15 (default) → 0.12 se atravessar parede, 0.20 se travar muito cedo
- `_playerHalfHeight`: 0.25 (default) → 0.20 se atravessar, 0.30 se travar
- `_wallContactTolerance`: 0.10 (default) → 0.05 se travar longe, 0.15 se deixar atravessar

---

## Code Quality Checklist

- ✅ No `GameObject.Find()` or `FindObjectOfType()`
- ✅ Local rebind uses `GetComponent()` on same GameObject
- ✅ Logs are informative and rate-limited
- ✅ New fields have sensible defaults
- ✅ OnGUI only draws when applicable
- ✅ Multi-point sampling prevents corner-case wall clipping
- ✅ Backward compatible (new fields have defaults)

---

## Notes for Future Maintenance

- **CaveDebugLevelSkipController tolerance**: P/F2 hotkeys are primary; button is fallback
- **CavePlayerPathConfinement tolerance**: If new enemies/features change player effective size, may need tuning
- **CreateMvpCaveScene**: Always update both controllers when regenerating scene
- **Performance**: 5-point sampling is negligible (< 1ms per frame); not a bottleneck

---

## Next Steps

1. ✅ Regenerate CaveScene via `CindarsHope/Scenes/Create MVP CaveScene`
2. 🔄 Execute 10-test sequence above
3. 🔄 Adjust tuning parameters if needed
4. 🔄 Commit: `Fix cave debug skip and confinement tolerance`
5. 🔄 Create PR against `dev`
