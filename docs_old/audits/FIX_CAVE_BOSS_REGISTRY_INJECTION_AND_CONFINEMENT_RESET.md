# Fix Cave Boss Registry Injection & Confinement Reset Audit

**Date**: 2026-05-21  
**Branch**: `feature/fix-cave-boss-registry-injection-and-confinement-reset`  
**Status**: Implementation complete. Unity Play Mode validation pending.

---

## Executive Summary

Five critical bugs fixed in cave system:

1. **CaveBossGateRegistry null in runtime** → Injected via CreateMvpCaveScene
2. **Boss spawn failure at level 15** → Registry now available for CaveBossSpawner
3. **Confinement distance still large** → Corrected half-widths to 0.03/0.12
4. **lastValidPosition teleporting player** → Reset on level change
5. **Runtime reference installer missing registry** → Added rebind logic

---

## Correction 1: Boss Gate Registry Injection

### File: `CreateMvpCaveScene.cs`

#### New Constants
```csharp
private const string EnemyMeteorOozeKingDataPath = "Assets/_Game/Data/Combat/Enemy_Meteor_Ooze_King.asset";
private const string CaveBossGateRegistryPath = "Assets/_Game/Data/Cave/CaveBossGateRegistry.asset";
private const string CaveBossGateLevel15Path = "Assets/_Game/Data/Cave/BossGate_Level15.asset";
```

#### New Methods
- `EnsureCaveBossGateRegistry()` - Creates/loads boss gate registry with level 15 gate
- `EnsureCaveBossGateLevel15()` - Creates/loads gate config (Id: boss_gate_level_15, CaveLevel: 15, CheckpointUnlockedOnDefeat: 15)
- `EnsureMeteorOozeKingEnemyData()` - Creates/loads boss enemy data (HP: 30, damage: 3, difficulty: Hard)

#### Integration in CreateCaveRuntime()
```csharp
var bossGateRegistry = EnsureCaveBossGateRegistry();
var meteorOozeKingData = EnsureMeteorOozeKingEnemyData();

// Assign to CaveRunManager
SetReference(serializedRunManager, "_bossGateRegistry", bossGateRegistry);

// Create and configure CaveBossSpawner
var bossSpawner = runtimeObject.AddComponent<CaveBossSpawner>();
SetReference(serializedBossSpawner, "_bossGateRegistry", bossGateRegistry);
SetReference(serializedBossSpawner, "_caveRunManager", runManager);
SetReference(serializedBossSpawner, "_enemyDatabase", enemyDatabase);
SetReference(serializedBossSpawner, "_fallbackEnemyData", meteorOozeKingData);

// Assign to CaveLevelRuntimeController
SetReference(serializedController, "_bossSpawner", bossSpawner);
```

#### Rationale
- Registry and gate data must exist before runtime
- CaveBossSpawner needs explicit references to spawn boss correctly
- CaveLevelRuntimeController triggers boss spawn in OnMaterializationComplete
- Fallback enemy data ensures boss visual even if database lookup fails

---

## Correction 2: Runtime Reference Rebind

### File: `CaveSceneRuntimeReferenceInstaller.cs`

#### New Field
```csharp
[SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;
```

#### New Logic in Start()
```csharp
// Try to rebind CaveBossGateRegistry if null
if (_bossGateRegistry == null)
{
    _bossGateRegistry = Resources.Load<CaveBossGateRegistrySO>("CaveBossGateRegistry");
    if (_bossGateRegistry == null)
    {
        Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: CaveBossGateRegistry not found in Resources...", this);
    }
}
```

#### Rationale
- Runtime safety: attempts to load registry from Resources if Inspector ref is null
- Logs warning if not found (doesn't silently fail)
- Prevents null reference errors when scene is loaded from Farm/Town

---

## Correction 3: Confinement Half-Width Correction

### File: `CavePlayerPathConfinement.cs`

#### Field Values Updated
```csharp
[SerializeField] private float _horizontalHalfWidth = 0.03f;    // was 0.01f, now allows closer approach
[SerializeField] private float _verticalHalfHeight = 0.12f;      // was 0.01f, now allows closer approach
```

#### Rationale
- Original values (0.01/0.01) were too small; player still blocked far from walls
- New values (0.03/0.12) allow natural proximity to walls
- Horizontal kept small (0.03) for tight lateral passages
- Vertical larger (0.12) for comfortable up/down movement
- Per-axis rollback algorithm prevents excessive distance clipping

---

## Correction 4: LastValidPosition Reset on Level Change

### File: `CavePlayerPathConfinement.cs`

#### New Fields
```csharp
private int _lastLevelHash;
```

#### New Public Method
```csharp
public void ResetLastValidPosition(Vector3 position)
{
    _lastValidPosition = position;
    Debug.Log($"CavePlayerPathConfinement: Reset last valid position to {position}.", this);
}
```

#### Updated LateUpdate()
```csharp
// Reset last valid position if level changed
var currentLevelHash = generatedLevel.GetHashCode();
if (currentLevelHash != _lastLevelHash)
{
    var playerWorldPos = _playerTransform.position;
    if (IsWorldPositionAllowed(playerWorldPos, generatedLevel))
    {
        ResetLastValidPosition(playerWorldPos);
    }
    else
    {
        Debug.LogWarning($"CavePlayerPathConfinement: Player spawned in invalid position after level load...", this);
    }
    _lastLevelHash = currentLevelHash;
}
```

#### Rationale
- Detects level change via hash comparison
- Resets lastValidPosition to player's current spawn location
- Prevents teleporting player to old position from previous level
- Validates spawn position is walkable; warns if not
- Applies before confinement check each frame

---

## Correction 5: Scene Creation Configuration Update

### File: `CreateMvpCaveScene.cs`

#### CavePlayerPathConfinement Setup
```csharp
serializedConfinement.FindProperty("_horizontalHalfWidth").floatValue = 0.03f;
serializedConfinement.FindProperty("_verticalHalfHeight").floatValue = 0.12f;
serializedConfinement.FindProperty("_useDiagonalSamples").boolValue = false;
serializedConfinement.FindProperty("_logFailedSample").boolValue = false;
```

#### Logging
```
CreateMvpCaveScene: CaveBossSpawner configured with registry, boss gate level 15, and enemy data.
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. horizontalHalfWidth=0.03, verticalHalfHeight=0.12, useDiagonals=false.
```

---

## Expected Console Output

### At Scene Load
```
CreateMvpCaveScene: CaveBossSpawner configured with registry, boss gate level 15, and enemy data.
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. horizontalHalfWidth=0.03, verticalHalfHeight=0.12, useDiagonals=false.
```

### At Play Mode Start
```
CaveRunManager: Cannot advance 15->16. CaveBossGateRegistry is null. ← SHOULD NO LONGER APPEAR
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController, horizontalHalfWidth=0.03, verticalHalfHeight=0.12, useDiagonalSamples=False.
```

### When Level 15 Materializes
```
CaveBossSpawner: Boss gate found for level 15: boss_gate_level_15.
CaveBossSpawner: Boss spawn resolved near ForwardExit. GateGrid=<>, BossGrid=<>, DistanceToGate=<>.
CaveBossSpawner: Spawned boss Meteor Ooze King (gate=boss_gate_level_15).
CavePlayerPathConfinement: Reset last valid position to <spawn_pos>.
```

### When Player Hits Wall (New Behavior)
```
CavePlayerPathConfinement: Confined player. Current=(19.8, 15.0, 0), Resolved=(19.5, 15.0, 0), Grid=(41, 50).
← Note: Only X adjusted; Y unchanged (per-axis rollback)
```

### When Boss Defeated
```
CaveBossDeathReporter: Boss enemy_meteor_ooze_king defeated (gate=boss_gate_level_15).
CaveRunManager: boss 'boss_gate_level_15' at level 15 marked as defeated.
CaveRunManager: Checkpoint 15 unlocked.
CaveRunManager: Boss gate 'boss_gate_level_15' defeated. Advancing 15->16 permitted.
```

---

## Test Procedure

### Pre-Play Mode
1. Abrir projeto Unity
2. Menu: `CindarsHope/Scenes/Create MVP CaveScene`
3. Aguardar criação e check console para logs acima

### Play Mode — Test 1: Registry Null Check (CRITICAL)
1. Pressionar P até level 15
2. **Esperado**: `CaveBossGateRegistry is null` não aparece
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 2: Boss Spawn at Level 15
1. Chegar nível 15 (P hotkey)
2. **Esperado**: Boss aparece com cor laranja (1.0, 0.5, 0.0) perto da ForwardExit
3. Logs aparecem: "Boss gate found for level 15"
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 3: Boss Blocks Advancement
1. Tentar ForwardExit 15 → 16 **antes** de matar boss
2. **Esperado**: Bloqueado, log: "Cannot advance. Boss gate not defeated"
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 4: Boss Defeat Unlocks Gate
1. Matar boss
2. **Esperado**:
   - Logs: "Boss defeated", "Checkpoint 15 unlocked"
   - Checkpoint 15 now appears in selection
3. Tentar ForwardExit 15 → 16 **após** derrota
4. **Esperado**: Permitido, avanço para level 16
5. ✅ PASS / ❌ FAIL

### Play Mode — Test 5: Wall Tolerance (Left)
1. Andar para parede esquerda
2. Pressionar E repetidamente para "encostar"
3. **Esperado**: Player consegue chegar mais perto (0.03 lateral)
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 6: Wall Tolerance (Right)
1. Andar para parede direita
2. Pressionar D repetidamente para "encostar"
3. **Esperado**: Player consegue chegar mais perto
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 7: LastValidPosition Not Old
1. Estar no level 10, encostado numa parede
2. Pressionar P para ir até level 15
3. **Esperado**: Player não teleporta para posição velha de level 10
4. Player fica onde está (ou reposicionado em spawn válido do level 15)
5. ✅ PASS / ❌ FAIL

### Play Mode — Test 8: Per-Axis Rollback (Smooth Sliding)
1. Andar contra parede lateral
2. Tentar subir enquanto encostado
3. **Esperado**: Player desliza lateralmente suave (não é jogado para trás)
4. Confinement ajusta só Y, deixa X como estava
5. ✅ PASS / ❌ FAIL

### Play Mode — Test 9: No Jitter
1. Manter player contra parede por 5 segundos
2. **Esperado**: Sem tremor, sem teleport, smooth confinement
3. Logs rate-limited (máx 1 por segundo)
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 10: Save/Load Preserves State
1. Derrotar boss em level 15
2. Save (F9)
3. Load (F5)
4. **Esperado**: 
   - Boss ainda marcado como vencido
   - Checkpoint 15 ainda desbloqueado
   - Player não teleportado para lastValid antigo
5. ✅ PASS / ❌ FAIL

---

## Acceptance Criteria (10/10 = PASS)

| # | Test | Esperado | Status |
|---|------|----------|--------|
| 1 | Registry null check | Sem erro "Registry is null" | ⬜ |
| 2 | Boss spawn level 15 | Boss visível perto ForwardExit | ⬜ |
| 3 | Boss blocks 15→16 | Avanço bloqueado antes derrota | ⬜ |
| 4 | Defeat unlocks gate | Avanço permitido após derrota | ⬜ |
| 5 | Wall tolerance left | Consegue chegar perto parede esq | ⬜ |
| 6 | Wall tolerance right | Consegue chegar perto parede dir | ⬜ |
| 7 | No old teleport | Não volta para lastValid antigo | ⬜ |
| 8 | Per-axis rollback | Desliza suave, não joga para trás | ⬜ |
| 9 | No jitter | Smooth contra parede, sem tremor | ⬜ |
| 10 | Save/load state | Boss defeat e checkpoint persistem | ⬜ |

**Final**: ⬜ ___ / 10 PASS

---

## Tuning Parameters

If tests don't pass perfectly, adjust in Inspector:

### CavePlayerPathConfinement
- `_horizontalHalfWidth`: 0.03 (default) → 0.01 se ainda longe, 0.05 se deixar atravessar
- `_verticalHalfHeight`: 0.12 (default) → 0.08 se ainda longe, 0.16 se deixar atravessar
- `_useDiagonalSamples`: false (default) → true se suspeitar buracos em quinas

### CaveBossGateRegistry
- Verifique em Inspector se registry tem gate level 15
- Se faltando, menu: `CindarsHope/Validate/Validate MVP Data`

---

## Code Quality Checklist

- ✅ No `GameObject.Find()` or `FindObjectOfType()`
- ✅ Registry injection via CreateMvpCaveScene (deterministic)
- ✅ Runtime fallback load via Resources.Load (safe)
- ✅ Level change detection via hash (cheap, effective)
- ✅ Per-axis rollback prevents teleporting far back
- ✅ Logs are informative and rate-limited
- ✅ No circular dependencies
- ✅ Backward compatible (new fields have sensible defaults)

---

## Notes for Future Maintenance

- **CaveBossGateRegistry**: Persist in Assets/_Game/Data/Cave/, referenced via CreateMvpCaveScene
- **LastValidPosition reset**: Automatic on level change; public method available for manual reset if needed
- **Confinement tolerance**: If player size changes (e.g., with equipment), may need half-width retuning
- **Boss spawn**: Depends on CaveRunManager registry ref; validate in CaveSceneRuntimeReferenceInstaller if needed

---

## Next Steps

1. ✅ Implement fixes (code)
2. 🔄 Regenerate CaveScene via `CindarsHope/Scenes/Create MVP CaveScene`
3. 🔄 Execute 10-test sequence above
4. 🔄 Adjust tuning parameters if needed
5. 🔄 Commit: `Fix cave boss registry injection and confinement reset`
6. 🔄 Create PR against `dev`

---

## Files Modified

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs` (added boss registry injection)
- `Assets/_Game/Scripts/SceneManagement/CaveSceneRuntimeReferenceInstaller.cs` (added registry rebind)
- `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs` (fixed confinement, added level reset)
