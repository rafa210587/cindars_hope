# WAVE_INTEGRATION_18 — Scene-Bound State Preservation Matrix

**Date:** 2026-06-10

---

## How Scene-Bound Preservation Works

`SaveManager.SaveGame()` captures a section from its live manager if available, otherwise falls back to `existingSaveData?.Section`. This means saving from any scene preserves data that belongs to other scenes.

---

## Preservation Matrix

| Active Scene When Saving | Farm | World | Cave | Npcs (Town) | Quests | Economy |
|--------------------------|------|-------|------|-------------|--------|---------|
| FarmScene | LIVE CAPTURE | LIVE CAPTURE | EXISTING PRESERVED | EXISTING PRESERVED | LIVE CAPTURE* | LIVE CAPTURE if ShopManager wired |
| TownScene | EXISTING PRESERVED | EXISTING PRESERVED | EXISTING PRESERVED | LIVE CAPTURE | LIVE CAPTURE* | LIVE CAPTURE if ShopManager wired |
| CaveScene | EXISTING PRESERVED | EXISTING PRESERVED | LIVE CAPTURE (CaveRunManager) | EXISTING PRESERVED | LIVE CAPTURE* | EXISTING PRESERVED |
| Any other scene | EXISTING PRESERVED | EXISTING PRESERVED | EXISTING PRESERVED | EXISTING PRESERVED | LIVE CAPTURE* | EXISTING PRESERVED |

*Quest capture is scene-independent: `QuestRuntimeBootstrap.CaptureSaveData()` reads from the in-memory `QuestService` which survives all scene transitions (DontDestroyOnLoad).

---

## Code Evidence

### Farm (scene-bound)

```csharp
private FarmSaveData CaptureFarmSaveData(GameSaveData existingSaveData)
{
    var activeScene = SceneManager.GetActiveScene();
    if (activeScene.name == FarmSceneName && _farmPlotRegistry != null)
        return _farmPlotRegistry.CaptureSaveData();  // live
    if (existingSaveData?.Farm != null)
        return existingSaveData.Farm;  // preserve
    return new FarmSaveData();
}
```

### Npcs (scene-bound to Town)

```csharp
private NpcManagerSaveData CaptureNpcSaveData(GameSaveData existingSaveData)
{
    if (SceneManager.GetActiveScene().name == TownSceneName && _npcManager != null)
        return _npcManager.CaptureSaveData();  // live
    return existingSaveData?.Npcs ?? new NpcManagerSaveData();  // preserve
}
```

### Cave (captured by CaveRunManager regardless of scene)

```csharp
private CaveSaveData CaptureCaveSaveData(GameSaveData existingSaveData)
{
    if (_caveRunManager != null)
        return _caveRunManager.CaptureSaveData();  // live (DontDestroyOnLoad)
    return existingSaveData?.Cave ?? new CaveSaveData();  // preserve
}
```

### Quests (DontDestroyOnLoad — always available)

```csharp
private static QuestStateSectionSaveData CaptureQuestSaveData(GameSaveData existingSaveData)
{
    var liveSection = QuestRuntimeBootstrap.CaptureSaveData();
    if (liveSection != null) return liveSection;
    return existingSaveData?.Quests ?? new QuestStateSectionSaveData();
}
```

---

## Risk Assessment

| Risk | Status |
|------|--------|
| Save from Cave erases Farm data | NO — Farm preserved via existingSaveData fallback |
| Save from Farm erases NPC state | NO — NpcSaveData preserved via existingSaveData fallback |
| Save from Town erases Cave data | NO — Cave preserved (CaveRunManager DontDestroyOnLoad or existingSaveData fallback) |
| Quest save erases other sections | NO — Quests captured independently via QuestRuntimeBootstrap |
| Quests overwritten with empty if QuestService not initialized | NO — CaptureQuestSaveData falls back to existingSaveData?.Quests |
