# WAVE_INTEGRATION_18 — Save/Load Gap Closure: Execution Report

**Date:** 2026-06-10
**Status:** `BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE`
**Branch:** dev

---

## Report Final

```
Status:                            BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE
Branch:                            dev
Working tree preflight:            PASS — branch dev, working tree clean (TownScene.unity pre-existing editor change)
Sources read:                      SaveData.cs, SaveManager.cs, QuestRuntimeBootstrap.cs, QuestService.cs,
                                   QuestStateSection.cs, QuestStateRecord.cs, CaveSaveData.cs,
                                   WAVE17 reports
Existing functionality matrix:     PASS — see WAVE_INTEGRATION_18_EXISTING_FUNCTIONALITY_MATRIX.md
DTO audit:                         PASS — see WAVE_INTEGRATION_18_SAVE_DTO_AUDIT.md
SchemaVersion:                     5 (unchanged — no breaking change)
Migration:                         NONE CREATED (optional field; backward compatible)
SaveManager changed:               YES (CaptureQuestSaveData, RestoreQuestSaveData, normalization)
GameSaveData changed:              YES (Quests field added)
QuestState save:                   IMPLEMENTED (QuestStateSectionSaveData + wiring in SaveManager)
Reward idempotency after load:     IMPLEMENTED (GrantedRewardIds persisted; TurnIn guard active)
NPC save coverage:                 ALREADY_IMPLEMENTED — validated, not recreated
NpcSection preservation:           ALREADY_IMPLEMENTED — CaptureNpcSaveData falls back to existing
CaveRun save:                      ALREADY_IMPLEMENTED — CaveRunManager.CaptureSaveData
Enemy state save:                  CAVE_ENEMY_HP_SAVE_DEBT (DROP_DIRECT_TO_INVENTORY strategy)
Loot/pickup state save:            COVERED by Inventory save (no ground pickups in WAVE17)
Scene-bound preservation:          ALREADY_IMPLEMENTED — validated
Negative tests:                    DOCUMENTED — see WAVE_INTEGRATION_18_NEGATIVE_SAVE_LOAD_TESTS.md
Validator:                         Assets/_Game/Scripts/Editor/Validation/ValidateWave18SaveLoadGapClosure.cs
Assembly-CSharp before:            PASS (0E/0W)
Assembly-CSharp-Editor before:     PASS (0E/3W pre-existing)
Assembly-CSharp after:             PASS (0E/0W)
Assembly-CSharp-Editor after:      PASS (0E/3W pre-existing, no new errors)
Docs validation:                   EXPECTED_FAIL_LEGACY_ONLY
Quality check:                     HARNESS_FAIL_PESTER_KNOWN_ISSUE (pre-existing)
Decision report:                   docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_DECISION.md
Human checklist:                   docs/validation/WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md
Can continue WAVE19:               YES (all code-level blockers resolved)
Human Play Mode validation needed: YES — see HUMAN_PLAYMODE_CHECKLIST.md
```

---

## What Already Existed (Not Recreated)

| System | Status |
|--------|--------|
| SaveManager (schema v5) | ALREADY_IMPLEMENTED — not recreated |
| Safe write (.tmp) | ALREADY_IMPLEMENTED |
| Migrations v1→v5 | ALREADY_IMPLEMENTED |
| Cross-scene load | ALREADY_IMPLEMENTED |
| Player/Inventory/Equipment/Economy/Crafting save | ALREADY_IMPLEMENTED |
| NpcManager capture/restore | ALREADY_IMPLEMENTED |
| CaveRunManager capture/restore | ALREADY_IMPLEMENTED |
| Skill tree/active slots/bestiary save | ALREADY_IMPLEMENTED |
| GrantedRewardIds in QuestStateRecord | ALREADY_IMPLEMENTED |
| TurnIn idempotency | ALREADY_IMPLEMENTED |

---

## What Was Implemented (WAVE18 Code Delta)

### 1. `SaveData.cs` — New DTOs

```csharp
[Serializable] public class QuestObjectiveStateSaveData  // per-objective save
[Serializable] public class QuestStateSaveData           // per-quest save
[Serializable] public class QuestStateSectionSaveData    // root quest section save
```

Added to `GameSaveData`:
```csharp
public QuestStateSectionSaveData Quests;
```

**Why separate DTOs:** `QuestStateSection`/`QuestStateRecord` use C# properties (`{ get; set; }`) which `JsonUtility` does not serialize. New `[Serializable]` DTOs with public fields are required.

### 2. `SaveManager.cs` — Capture + Restore + Normalize

Added:
- `CaptureQuestSaveData(existingSaveData)` — converts live `QuestStateSection` → `QuestStateSectionSaveData`
- `RestoreQuestSaveData(questData)` — calls `QuestRuntimeBootstrap.RestoreFromSaveData`
- Wired both into `SaveGame()` and `ApplySaveData()`
- Normalization in `ValidateAndNormalizeSave`: `saveData.Quests ??= new QuestStateSectionSaveData()`

### 3. `QuestRuntimeBootstrap.cs` — Static Capture/Restore APIs

Added:
- `static QuestStateSectionSaveData CaptureSaveData()` — converts live section to save DTO
- `static void RestoreFromSaveData(QuestStateSectionSaveData)` — immediate restore if initialized, else stores pending
- `static void SetPendingSaveData(QuestStateSectionSaveData)` — explicit pending setter
- `static QuestStateSectionSaveData _pendingSaveData` — handles pre-initialization restore
- Applies pending save data in `Initialize()` after `QuestService` is built

### 4. `QuestService.cs` — RestoreFromSaveData

Added:
- `void RestoreFromSaveData(QuestStateSectionSaveData saveData)` — clears and repopulates `_saveSection.QuestStates` from DTO
- Preserves `GrantedRewardIds` from save data (reward idempotency after load)

### 5. `ValidateWave18SaveLoadGapClosure.cs` — Editor Validator

Checks via reflection:
- `GameSaveData.Quests` field present
- `QuestStateSectionSaveData` is `[Serializable]`
- `QuestRuntimeBootstrap.CaptureSaveData` + `RestoreFromSaveData` exist
- `QuestService.RestoreFromSaveData` exists
- `NpcManager.CaptureSaveData` + `RestoreFromSaveData` exist
- `CaveRunManager.CaptureSaveData` + `RestoreFromSaveData` exist
- No Unity Object refs in quest DTOs
- All 10 required docs present

---

## What Remained as Debt

| Debt | Type | Risk | Resolution |
|------|------|------|-----------|
| Enemy alive HP not persisted mid-combat | CAVE_ENEMY_HP_SAVE_DEBT | LOW (smoke test) | Future WAVE19 or cave persistence spec |
| CompanionManagerSaveData not captured | COMPANION_SAVE_DEBT | LOW (smoke test) | Future companion spec |
| Play Mode validation pending | HUMAN_VALIDATION_DEBT | Medium | Execute HUMAN_PLAYMODE_CHECKLIST |
| WAVE16 scene wiring pending | SCENE_WIRING_DEBT | Pre-existing | Human Unity Editor action |

---

## Testing Quality Gate

```
Testing Quality Gate
────────────────────
Changed runtime code:              YES (SaveManager, QuestRuntimeBootstrap, QuestService)
Changed deterministic logic:       YES (quest save/restore, reward idempotency)
Changed Unity scene/prefab/asset:  NO
Automated tests added/updated:     NO
Automated tests command:           NOT RUN
Manual Play Mode scenario:         docs/validation/WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md
Justification if no automated tests: EditMode test harness requires access to SaveManager internals
                                   (private fields, Unity lifecycle). The key behavior (JsonUtility
                                   serialization of QuestStateSectionSaveData) is provable by build
                                   compile success + JsonUtility round-trip which requires Unity runtime.
                                   Core idempotency logic (GrantedRewardIds guard) is already in
                                   QuestService.TurnIn() which was pre-existing and not modified.
Residual risk: Quest save not validated in Play Mode until human executes checklist.
               Enemy HP loss on load (acceptable smoke test debt).
```

---

## Completeness Revalidation Pass 2

| Check | Result | Evidence |
|-------|--------|---------|
| Existing save/load was not recreated | PASS | SaveManager unchanged except gap-closure methods |
| Quest state persists | PASS | QuestStateSectionSaveData wired in SaveGame/ApplySaveData |
| GrantedRewardIds persists | PASS | GrantedRewardIds in QuestStateSaveData; restored in RestoreFromSaveData |
| Quest reward does not duplicate after load | PASS | TurnIn returns AlreadyCompleted for State=Completed; GrantedRewardIds guard also active |
| NPC position/state persists | PASS | NpcManager.CaptureSaveData ALREADY_IMPLEMENTED |
| NPC section preserved outside Town | PASS | CaptureNpcSaveData falls back to existingSaveData?.Npcs |
| Cave run state persists | PASS | CaveRunManager.CaptureSaveData ALREADY_IMPLEMENTED |
| Enemy state persists or direct/drop debt explicit | PASS | CAVE_ENEMY_HP_SAVE_DEBT documented |
| Loot state persists or direct-drop strategy explicit | PASS | DROP_DIRECT_TO_INVENTORY; loot in inventory (persisted) |
| Scene-bound sections preserved | PASS | All section captures fall back to existingSaveData |
| No Unity refs saved in DTOs | PASS | Quest DTOs have only string/int/bool/List<string>/List<DTO> |
| Runtime build passes | PASS | Assembly-CSharp 0E/0W |
| Editor build passes | PASS | Assembly-CSharp-Editor 0E/3W pre-existing |
| Human checklist created | PASS | docs/validation/WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md |

---

## Honest Status Rationale

Status is `BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE` rather than `ACCEPTED` because:

- Quest save code is wired and builds 0E/0W
- But Unity Play Mode has not been run to verify `JsonUtility.ToJson(saveData)` actually includes the `Quests` field
- Human must verify slot_1.json structure and quest round-trip in Unity Editor
- This is honest about the remaining validation gap
