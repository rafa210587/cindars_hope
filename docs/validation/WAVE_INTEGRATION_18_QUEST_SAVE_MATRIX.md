# WAVE_INTEGRATION_18 — Quest Save Matrix

**Date:** 2026-06-10

---

## Quest Save/Load Cases

| Case | Before WAVE18 | After WAVE18 | Evidence |
|------|--------------|--------------|---------|
| Quest accepted saves | NO (in-memory only) | YES | QuestStateRecord captured in QuestStateSectionSaveData |
| Quest Active state saves | NO | YES | State=1 (Active) persisted |
| Quest progress (objective CurrentProgress) saves | NO | YES | ObjectiveStates.CurrentProgress captured |
| ReadyToComplete saves | NO | YES | State=2 (ReadyToComplete) persisted |
| Quest Completed saves | NO | YES | State=3 (Completed) persisted |
| GrantedRewardIds saves | NO | YES | GrantedRewardIds List<string> persisted |
| GrantedFlagIds saves | NO | YES | GrantedFlagIds List<string> persisted |
| Quest reward does not duplicate after load | RUNTIME_ONLY | GUARANTEED | GrantedRewardIds loaded → TurnIn guard skips already-granted |
| Missing QuestRuntimeBootstrap preserves existing | YES | YES | CaptureQuestSaveData returns existingSaveData?.Quests if QuestService null |
| Pending save restores after delayed Initialize() | NO | YES | _pendingSaveData applied in Initialize() |

---

## Reward Idempotency After Load

The `QuestService.TurnIn()` method uses:

```csharp
var alreadyGranted = new HashSet<string>(record.GrantedRewardIds);
// ... apply only if not in alreadyGranted ...
if (!record.GrantedRewardIds.Contains(result.GrantedRewardId))
    record.GrantedRewardIds.Add(result.GrantedRewardId);
```

After load:
1. `QuestService.RestoreFromSaveData()` restores `GrantedRewardIds` from save
2. If player triggers turn-in again on a completed quest: `QuestService.TurnIn()` returns `QuestTurnInResult.AlreadyCompleted()` because `record.State == QuestStateStatus.Completed`
3. Even if state check were bypassed, `GrantedRewardIds` guard prevents re-granting

**Result:** Reward idempotency is maintained across save/load cycles.

---

## JsonUtility Serialization Note

`QuestStateSection` and `QuestStateRecord` use C# properties (`{ get; set; }`). `JsonUtility.ToJson` only serializes **public fields**, not properties. Therefore new `[Serializable]` DTOs were created:

- `QuestStateSectionSaveData` (public fields, `[Serializable]`)
- `QuestStateSaveData` (public fields, `[Serializable]`)
- `QuestObjectiveStateSaveData` (public fields, `[Serializable]`)

`QuestRuntimeBootstrap.CaptureSaveData()` converts from live `QuestStateSection` (properties) → `QuestStateSectionSaveData` (fields).
`QuestService.RestoreFromSaveData()` converts from `QuestStateSectionSaveData` (fields) → live `QuestStateRecord` objects (properties).

---

## Missing QuestRuntimeBootstrap Preservation

If `QuestRuntimeBootstrap` is not initialized (scene loads before bootstrap):

```csharp
private static QuestStateSectionSaveData CaptureQuestSaveData(GameSaveData existingSaveData)
{
    var liveSection = QuestRuntimeBootstrap.CaptureSaveData(); // returns null if QuestService == null
    if (liveSection != null) return liveSection;
    return existingSaveData?.Quests ?? new QuestStateSectionSaveData(); // preserve existing
}
```

Result: Existing Quests section from previous save is preserved, not overwritten with empty.
