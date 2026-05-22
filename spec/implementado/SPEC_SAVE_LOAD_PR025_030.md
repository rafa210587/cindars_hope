# SPEC: Save/Load (PR-025 to PR-030)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-025 to PR-030

---

## Summary

Save and load player progress to JSON. Persists player state, inventory, farm plots, day counter, and scene position.

## Scope

- ✅ SaveData DTO (JSON-serializable contracts)
- ✅ SaveManager (file I/O to persistentDataPath)
- ✅ Player state persistence (gold, hunger, health, XP)
- ✅ Inventory persistence
- ✅ Farm plot persistence
- ✅ Scene and position persistence
- ✅ Cross-scene state rebinding via bootstrap
- ⚠️ Migration/versioning (partial)

## Architecture

### Save Format
- **Location**: `Application.persistentDataPath/cindars_hope/save.json`
- **Format**: JSON, no binary
- **Serialization**: Minimal types (int, string, List<T>) — NO Unity refs

### Save Data Structure
```csharp
GameSaveData {
  CurrentScene: string,
  PlayerPosition: Vector3,
  PlayerGold: int,
  PlayerHunger: float,
  PlayerHealth: int,
  PlayerXP: int,
  CurrentDay: int,
  Inventory: List<InventorySaveData>,
  FarmPlots: List<FarmPlotSaveData>
}
```

### Load Flow
1. SaveManager loads JSON
2. Bootstrap caches state
3. Scene loads, components query bootstrap
4. Bootstrap publishes rebind event
5. Components re-obtain references

## Key Files

- `Assets/_Game/Scripts/Save/SaveManager.cs` — File I/O
- `Assets/_Game/Scripts/Save/SaveData.cs` — DTO contracts
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — State caching
- `Assets/_Game/Scripts/Save/SaveSerializer.cs` — JSON conversion

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Save creates JSON file | ✅ |
| 2 | Load reads JSON file | ✅ |
| 3 | Player position persists | ✅ |
| 4 | Inventory persists | ✅ |
| 5 | Farm plots persist | ✅ |
| 6 | Day counter persists | ✅ |
| 7 | Cross-scene load works | ✅ |
| 8 | No Unity object references in save | ✅ |

## Pending

- Schema versioning/migration for future changes
- Encryption (optional)
- Multiple save slots
- Play Mode validation (F5 save, F9 load)

## Next Steps

Continue to World/Shop (PR-031 to PR-045).
