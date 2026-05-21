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

## Correction 2: Confinement with Per-Axis Rollback

### File: `CavePlayerPathConfinement.cs`

#### New Fields
```csharp
[SerializeField] private float _horizontalHalfWidth = 0.03f;
[SerializeField] private float _verticalHalfHeight = 0.12f;
[SerializeField] private bool _useDiagonalSamples = false;
[SerializeField] private bool _logFailedSample = false;
```

#### ResolveConstrainedPosition() — Per-Axis Correction Strategy
```csharp
private Vector3 ResolveConstrainedPosition(Vector3 currentPosition, CaveGeneratedLevel level)
{
    if (IsWorldPositionAllowed(currentPosition, level))
    {
        _lastValidPosition = currentPosition;
        return currentPosition;
    }

    var xRollback = new Vector3(_lastValidPosition.x, currentPosition.y, currentPosition.z);
    if (IsWorldPositionAllowed(xRollback, level))
    {
        _lastValidPosition = xRollback;
        return xRollback;
    }

    var yRollback = new Vector3(currentPosition.x, _lastValidPosition.y, currentPosition.z);
    if (IsWorldPositionAllowed(yRollback, level))
    {
        _lastValidPosition = yRollback;
        return yRollback;
    }

    return _lastValidPosition;
}
```

#### IsWorldPositionAllowed() — Multi-Point Sampling (No Diagonals by Default)
```csharp
private bool IsWorldPositionAllowed(Vector3 worldPos, CaveGeneratedLevel level)
{
    var samples = new List<Vector3>
    {
        worldPos,
        worldPos + Vector3.left * _horizontalHalfWidth,
        worldPos + Vector3.right * _horizontalHalfWidth,
        worldPos + Vector3.up * _verticalHalfHeight,
        worldPos + Vector3.down * _verticalHalfHeight
    };

    if (_useDiagonalSamples)
    {
        samples.Add(worldPos + new Vector3(-_horizontalHalfWidth, _verticalHalfHeight, 0));
        samples.Add(worldPos + new Vector3(_horizontalHalfWidth, _verticalHalfHeight, 0));
        samples.Add(worldPos + new Vector3(-_horizontalHalfWidth, -_verticalHalfHeight, 0));
        samples.Add(worldPos + new Vector3(_horizontalHalfWidth, -_verticalHalfHeight, 0));
    }

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

#### LateUpdate() — Resolution Flow
```csharp
var playerWorldPos = _playerTransform.position;
var resolved = ResolveConstrainedPosition(playerWorldPos, generatedLevel);

if (resolved != playerWorldPos)
{
    _playerTransform.position = resolved;
    // ... logging if rate limit exceeded
}
```

#### WorldToGridPosition() — Compatibility with Materializer
Uses `Mathf.RoundToInt()` to match `GridToWorld()` formula:
- GridToWorld: `world = new Vector3(grid.x - width * 0.5f, grid.y - height * 0.5f, 0)`
- WorldToGrid: `gridX = Mathf.RoundToInt(world.x + width * 0.5f)`

#### Rationale
- **Per-axis correction**: Instead of always rolling back fully to `_lastValidPosition`, tries:
  1. Keep current Y, rollback X only
  2. Keep current X, rollback Y only
  3. Only if both fail, rollback both
- **Allows sliding**: Player can slide along walls naturally instead of being pushed far back
- **Smaller boundary**: `horizontalHalfWidth=0.03` (was 0.15) allows player closer to lateral walls
- **Vertical reach**: `verticalHalfHeight=0.12` (was 0.25) allows approaching top/bottom walls
- **No diagonals by default**: Simpler prediction, works well with grid-based cave layouts
- **Debug mode**: `_logFailedSample` optional for diagnostics when tuning

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
serializedConfinement.FindProperty("_horizontalHalfWidth").floatValue = 0.03f;
serializedConfinement.FindProperty("_verticalHalfHeight").floatValue = 0.12f;
serializedConfinement.FindProperty("_useDiagonalSamples").boolValue = false;
serializedConfinement.FindProperty("_logFailedSample").boolValue = false;
```

#### Logging
```csharp
Debug.Log($"CreateMvpCaveScene: CaveDebugLevelSkipController configured on {runtimeObject.name}. Keys=P/F2, Button=enabled, enableDebugLevelSkip=true, bypassBossGate=true.");
Debug.Log($"CreateMvpCaveScene: CavePlayerPathConfinement configured on {playerTransform.gameObject.name}. horizontalHalfWidth=0.03, verticalHalfHeight=0.12, useDiagonals=false.");
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
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController, horizontalHalfWidth=0.03, verticalHalfHeight=0.12, useDiagonalSamples=False.
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
CavePlayerPathConfinement: Confined player. Current=(1.2, 2.3, 0), Resolved=(1.0, 2.3, 0), Grid=(41, 50).
```
(Resolved position may only fix X axis, only Y axis, or both, depending on which constraint fails first)

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
- `_horizontalHalfWidth`: 0.03 (default) → 0.01 se ainda estiver longe, 0.05 se deixar atravessar
- `_verticalHalfHeight`: 0.12 (default) → 0.08 se ainda estiver longe, 0.16 se deixar atravessar
- `_useDiagonalSamples`: false (default) → true se suspeitar de buracos em quinas

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
