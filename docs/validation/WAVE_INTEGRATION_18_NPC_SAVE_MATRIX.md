# WAVE_INTEGRATION_18 — NPC Save Matrix

**Date:** 2026-06-10

---

## NpcManager Save Coverage

`NpcManager` saves via `NpcManagerSaveData`:

```csharp
public class NpcManagerSaveData
{
    public List<NpcSaveData> Npcs = new List<NpcSaveData>();
}

public class NpcSaveData
{
    public string NpcId;
    public string SceneId;
    public Vector2 Position;
    public bool HasMet;
}
```

`CaptureNpcSaveData()` in SaveManager:

```csharp
private NpcManagerSaveData CaptureNpcSaveData(GameSaveData existingSaveData)
{
    if (SceneManager.GetActiveScene().name == TownSceneName && _npcManager != null)
    {
        return _npcManager.CaptureSaveData(); // live capture from TownScene
    }
    return existingSaveData?.Npcs ?? new NpcManagerSaveData(); // preserve when not in Town
}
```

---

## Coverage Assessment

| NPC Type | Covered | Fields | Notes |
|----------|---------|--------|-------|
| Regular NPCs (_npcs list) | YES | NpcId, SceneId, Position, HasMet | Captured when in TownScene |
| Shop NPCs (_shopNpcs list) | YES | NpcId, SceneId, Position, HasMet | Captured via NpcManager.CaptureSaveData |
| NPCs in other scenes | PRESERVED | Existing save section kept | CaptureNpcSaveData falls back to existingSaveData?.Npcs |

---

## Canonical NPC Roster (WAVE12C)

The WAVE12C canonical NPC roster includes 23 NPCs in TownScene. All are registered via `NpcManager._npcs` or `_shopNpcs`. When saving from TownScene, all registered NPCs are captured. When saving from FarmScene or CaveScene, the existing NPC save section is preserved.

| Validation | Result |
|-----------|--------|
| NpcManager.CaptureSaveData exists | PASS (confirmed in SaveManager) |
| NpcManager.RestoreFromSaveData exists | PASS (called in ApplySaveData) |
| Save from Farm preserves Npcs | PASS (CaptureNpcSaveData fall-through) |
| Save from Town updates Npcs | PASS (TownSceneName check) |
| Save from Cave preserves Npcs | PASS (CaptureNpcSaveData fall-through) |
| NpcId not empty | CONTRACT (NpcManager responsibility) |
| SceneId defined | CONTRACT (NpcManager registration) |
| Position captured | YES (NpcSaveData.Position field) |
| HasMet captured | YES (NpcSaveData.HasMet field) |

---

## WAVE18 Action

**ALREADY_IMPLEMENTED — validate only.** NPC save coverage is complete per existing code. No changes required.

Validator `ValidateWave18SaveLoadGapClosure` checks for `NpcManager.CaptureSaveData` and `RestoreFromSaveData` via reflection.
