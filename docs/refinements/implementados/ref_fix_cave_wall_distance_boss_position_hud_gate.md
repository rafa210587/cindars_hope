# REF — Fix cave wall distance boss position HUD gate

> Origem histórica: $Source
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_
> - $_

---

# Fix Cave Wall Distance, Boss Position & HUD Gate Audit

**Date**: 2026-05-21  
**Branch**: `feature/fix-cave-wall-distance-boss-position-hud-gate`  
**Status**: Implementation complete. Unity Play Mode validation pending.

---

## Executive Summary

Three final polish corrections for cave system:

1. **Wall distance still large** → Lateral samples disabled by default, halfWidth reduced to 0.005
2. **Boss far from gate** → Strategy-based spawn (Adjacent → Diagonal → Radius → Fallback)
3. **HUD shows only defeated gates** → Show current level gate status always

---

## Correction 1: Wall Distance & Sample Control

### File: `CavePlayerPathConfinement.cs`

#### New Fields
```csharp
[SerializeField] private float _horizontalHalfWidth = 0.005f;      // was 0.03
[SerializeField] private float _verticalHalfHeight = 0.08f;        // was 0.12
[SerializeField] private bool _useLateralSamples = false;          // NEW: disable left/right by default
[SerializeField] private bool _useVerticalSamples = true;          // NEW: enable up/down by default
[SerializeField] private bool _useDiagonalSamples = false;         // unchanged
```

#### Updated IsWorldPositionAllowed()
```csharp
var samples = new List<Vector3> { worldPos };

if (_useLateralSamples)
{
    samples.Add(worldPos + Vector3.left * _horizontalHalfWidth);
    samples.Add(worldPos + Vector3.right * _horizontalHalfWidth);
}

if (_useVerticalSamples)
{
    samples.Add(worldPos + Vector3.up * _verticalHalfHeight);
    samples.Add(worldPos + Vector3.down * _verticalHalfHeight);
}

if (_useDiagonalSamples)
{
    // diagonal logic unchanged
}
```

#### Updated Logging
```csharp
Debug.Log($"CavePlayerPathConfinement: enabled. Player={_playerTransform.name}, ..., 
  useLateralSamples={_useLateralSamples}, useVerticalSamples={_useVerticalSamples}, ...", this);
```

#### Rationale
- **Lateral samples OFF by default**: Prevents left/right check, player not blocked far from sidewalls
- **Vertical samples ON**: Still validates up/down to prevent ceiling/floor clipping
- **Extremely small horizontalHalfWidth (0.005)**: ~0.5cm lateral tolerance; allows very close approach
- **Moderate verticalHalfHeight (0.08)**: ~8cm vertical tolerance; comfortable for stairs/slopes
- **Backward compatible**: New fields have defaults; old scenes work unchanged

---

## Correction 2: Boss Spawn Strategy Near Gate

### File: `CaveBossSpawner.cs`

#### New Method: IsValidBossSpawnTile()
```csharp
private bool IsValidBossSpawnTile(Vector2Int tile, CaveGeneratedLevel level, Transform playerTarget, Vector2Int exit)
{
    // Must be walkable
    if (!level.WalkableTiles.Contains(tile)) return false;
    
    // Cannot be exit or entrance
    if (tile == exit || tile == level.Entrance) return false;
    
    // Cannot be out of bounds
    if (tile.x < 0 || tile.x >= level.Width || tile.y < 0 || tile.y >= level.Height) return false;
    
    // Preferably not on player (hard constraint: < 2m distance rejection)
    if (playerTarget != null)
    {
        var world = GridToWorld(tile, level);
        if (Vector3.Distance(world, playerTarget.position) < 2f) return false;
    }
    
    return true;
}
```

#### Refactored ResolveBossSpawnNearGate()
Spawn priority:
1. **Adjacent tiles** (distance = 1): up, down, left, right of Exit
2. **Diagonal tiles** (distance â‰ˆ 1.4): all 4 diagonals
3. **Radius <= 3**: Any walkable tile within 3 distance, sorted by distance
4. **Fallback**: Closest EnemySpawnPoint

#### Enhanced Logging
```csharp
var strategy = "Unknown";
var distToExit = Vector2Int.Distance(bossGridPos, generatedLevel.Exit);
if (distToExit <= 1.5f) strategy = "Adjacent";
else if (distToExit <= 2.5f) strategy = "Diagonal";
else if (distToExit <= 3f) strategy = "Radius";
else strategy = "Fallback";

Debug.Log($"CaveBossSpawner: Boss spawn resolved near ForwardExit. 
  GateGrid={generatedLevel.Exit}, BossGrid={bossGridPos}, 
  DistanceToGate={distanceToGate}, Strategy={strategy}.", this);
```

#### Rationale
- **Adjacent-first strategy**: Boss spawns "on" the gate if possible (1 tile away)
- **Never > 3 tiles away**: If any valid option exists within radius 3, use it
- **Player proximity check**: Soft reject if boss would spawn on player, but don't fail entirely
- **Strategy logging**: Diagnostic info shows which method succeeded (Adjacent/Diagonal/Radius/Fallback)

---

## Correction 3: HUD Always Shows Current Gate Status

### File: `CaveRunManager.cs`

#### New Public Methods
```csharp
public bool TryGetBossGateForLevel(int caveLevel, out CaveBossGateDataSO gate)
{
    gate = null;
    if (_bossGateRegistry == null) return false;
    
    gate = _bossGateRegistry.GetGateByLevel(caveLevel);
    return gate != null;
}

public bool IsCurrentLevelBossGate()
{
    return TryGetBossGateForLevel(_state.CurrentCaveLevel, out _);
}

public string GetCurrentBossGateId()
{
    return TryGetBossGateForLevel(_state.CurrentCaveLevel, out var gate) 
        ? gate.Id 
        : string.Empty;
}

public bool IsCurrentBossGateDefeated()
{
    return TryGetBossGateForLevel(_state.CurrentCaveLevel, out var gate) 
        && IsBossDefeated(gate.Id);
}

public bool CanAdvancePastCurrentBossGate()
{
    return CanAdvanceToLevel(_state.CurrentCaveLevel, _state.CurrentCaveLevel + 1);
}
```

#### Rationale
- **Consult registry directly**: Don't depend on BossDefeatStates (only has defeated gates)
- **Safe queries**: All methods handle null registry gracefully
- **Level-specific**: Always check against CurrentCaveLevel, not hard-coded logic
- **Advancement check**: CanAdvancePastCurrentBossGate includes registry null/gate missing checks

### File: `DebugHud.cs`

#### New HUD Section: "Current Level Boss Gate"
```
Before Gate History:

Current Level Boss Gate:
  boss_gate_level_15: active / blocked
  Can advance: false

OR

Current Level Boss Gate:
  No gate at this level
```

Then renamed old section to "Boss Gates History:" to show defeated gates encountered.

#### Rationale
- **Real-time feedback**: Shows gate status as player moves through levels
- **Defeated vs Active**: Clear visual indication of gate state
- **Advancement flag**: Tells player if they can exit or need to defeat boss first
- **Separated from history**: History section now only shows gates player has encountered

---

## Expected Console Output

### At Scene Load (CreateMvpCaveScene)
```
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. 
  horizontalHalfWidth=0.005, verticalHalfHeight=0.08, 
  useLateral=false, useVertical=true, useDiagonals=false.
```

### At Level 15 Entry
```
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController, 
  horizontalHalfWidth=0.005, verticalHalfHeight=0.08, 
  useLateralSamples=False, useVerticalSamples=True, useDiagonalSamples=False.

CaveBossSpawner: Boss gate found for level 15: boss_gate_level_15.
CaveBossSpawner: Boss spawn resolved near ForwardExit. 
  GateGrid=(40, 24), BossGrid=(41, 24), DistanceToGate=1, Strategy=Adjacent.
CaveBossSpawner: Spawned boss Meteor Ooze King (gate=boss_gate_level_15).
```

### HUD Display at Level 15 (Before Defeat)
```
Cave: procedural MVP
CaveLevel: 15
...
Current Level Boss Gate:
  boss_gate_level_15: active / blocked
  Can advance: false
```

### After Boss Defeated
```
CaveBossDeathReporter: Boss enemy_meteor_ooze_king defeated (gate=boss_gate_level_15).
CaveRunManager: boss 'boss_gate_level_15' at level 15 marked as defeated.
CaveRunManager: Checkpoint 15 unlocked.
```

### HUD Display After Defeat
```
Current Level Boss Gate:
  boss_gate_level_15: defeated / open
  Can advance: true
```

### At Level 16
```
Current Level Boss Gate:
  No gate at this level
```

---

## Test Procedure

### Pre-Play Mode
1. Abrir projeto Unity
2. Menu: `CindarsHope/Scenes/Create MVP CaveScene`
3. Aguardar criação

### Play Mode — Test 1: Wall Distance (Left)
1. Ir para parede esquerda
2. Manter botão A/E pressionado
3. **Esperado**: Player consegue ficar muito próximo (< 5mm visualmente)
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 2: Wall Distance (Right)
1. Ir para parede direita
2. Manter botão D pressionado
3. **Esperado**: Player consegue ficar muito próximo
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 3: Wall Distance (No Penetration)
1. Tentar forçar player através da parede
2. **Esperado**: Bloqueado, sem penetração
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 4: Boss Position at Level 15
1. P hotkey até level 15
2. **Esperado**: 
   - Boss spawna muito próximo da ForwardExit
   - Distância <= 1 tile (adjacente) idealmente
   - Log mostra "Strategy=Adjacent" ou "Strategy=Diagonal"
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 5: Boss Blocks Advancement
1. Tentar ForwardExit antes de matar boss
2. **Esperado**: Bloqueado
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 6: HUD Shows Gate Status (Before Defeat)
1. Estar no level 15
2. Abrir Debug HUD (F1 ou onscreen)
3. **Esperado**:
   - "Current Level Boss Gate: boss_gate_level_15: active / blocked"
   - "Can advance: false"
4. ✅ PASS / ❌ FAIL

### Play Mode — Test 7: Boss Defeat Unlocks Gate
1. Matar boss
2. **Esperado**:
   - Logs: "Boss defeated", "Checkpoint unlocked"
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 8: HUD Shows Gate Status (After Defeat)
1. Abrir Debug HUD após derrota
2. **Esperado**:
   - "Current Level Boss Gate: boss_gate_level_15: defeated / open"
   - "Can advance: true"
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 9: Advancement Now Allowed
1. Tentar ForwardExit após derrota
2. **Esperado**: Permitido, avança para level 16
3. ✅ PASS / ❌ FAIL

### Play Mode — Test 10: Level 16 No Gate
1. Estar no level 16
2. Abrir Debug HUD
3. **Esperado**:
   - "Current Level Boss Gate: No gate at this level"
4. ✅ PASS / ❌ FAIL

---

## Acceptance Criteria (10/10 = PASS)

| # | Test | Esperado | Status |
|---|------|----------|--------|
| 1 | Wall left | Muito perto, não penetra | â¬œ |
| 2 | Wall right | Muito perto, não penetra | â¬œ |
| 3 | Wall robust | Sem penetração forçada | â¬œ |
| 4 | Boss adjacent | Spawna distância <= 1 | â¬œ |
| 5 | Gate blocks | Não pode entrar 15→16 antes derrota | â¬œ |
| 6 | HUD before | Mostra "active / blocked" | â¬œ |
| 7 | Boss defeat | Mata boss, logs aparecem | â¬œ |
| 8 | HUD after | Mostra "defeated / open" | â¬œ |
| 9 | Advancement | Consegue avançar 15→16 | â¬œ |
| 10 | Level 16 | Mostra "No gate" | â¬œ |

**Final**: â¬œ ___ / 10 PASS

---

## Tuning Parameters

### CavePlayerPathConfinement
- `_horizontalHalfWidth`: 0.005 (default) → 0.001 muito apertado, 0.01 mais permissivo
- `_verticalHalfHeight`: 0.08 (default) → 0.05 mais apertado, 0.12 mais permissivo
- `_useLateralSamples`: false (default) → true se quiser validar left/right também
- `_useVerticalSamples`: true (default) → false para desabilitar validação vertical

### CaveBossSpawner
- Estratégia é automática, sem tunables
- Se boss quer ficar ainda mais próximo, reduzir raio máximo de 3 em ResolveBossSpawnNearGate

---

## Code Quality Checklist

- ✅ No GameObject.Find() or FindObjectOfType()
- ✅ Sample validation conditional on boolean flags
- ✅ Boss spawn tries 4 strategies before fallback
- ✅ HUD consults registry directly, not just BossDefeatStates
- ✅ All methods handle null registry gracefully
- ✅ Logs are diagnostic (strategy, distance, gate status)
- ✅ Backward compatible (new fields have defaults)
- ✅ Per-axis rollback still works with new sample logic

---

## Files Modified

- `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs` (samples, half-widths)
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs` (strategy spawn)
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` (public gate query methods)
- `Assets/_Game/Scripts/UI/DebugHud.cs` (current level gate display)
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs` (serialization)

---

## Next Steps

1. ✅ Implement corrections (code)
2. ðŸ”„ Regenerate CaveScene via `CindarsHope/Scenes/Create MVP CaveScene`
3. ðŸ”„ Execute 10-test sequence above
4. ðŸ”„ Commit: `Fix cave wall distance, boss position, and HUD gate status`
5. ðŸ”„ Create PR against `dev`



