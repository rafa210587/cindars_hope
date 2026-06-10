# WAVE_INTEGRATION_18 — Negative Save/Load Tests

**Date:** 2026-06-10

---

## Test Cases

### N-01: Corrupted JSON

| Aspect | Detail |
|--------|--------|
| Input | `slot_1.json` contains invalid JSON: `{ "SchemaVersion": 5, "Player": { invalid` |
| Expected | `TryDeserializeSave` returns false, `errorMessage` = "Save file could not be parsed: ..." |
| SaveManager behavior | `LoadGame()` returns false, logs warning, does NOT apply save |
| Good save preserved | YES — corrupt file not atomically written (WriteTextSafely); existing good file untouched |
| Code path | `TryReadSaveWithMigration` → `TryDeserializeSave` → catch → `return false` |

### N-02: Empty JSON File

| Aspect | Detail |
|--------|--------|
| Input | `slot_1.json` contains empty string `""` |
| Expected | `TryReadSaveWithMigration` returns false with "Save file is empty." |
| SaveManager behavior | `LoadGame()` returns false, does NOT crash |
| Code path | `if (string.IsNullOrWhiteSpace(rawJson)) → return false` |

### N-03: Future Schema Version

| Aspect | Detail |
|--------|--------|
| Input | `slot_1.json` has `"SchemaVersion": 99` |
| Expected | `TryReadSaveWithMigration` returns false: "Save schema version 99 is newer than supported version 5." |
| SaveManager behavior | Does NOT apply save, logs warning |
| Good save preserved | YES — future schema save is rejected before any restore |
| Code path | `sourceVersion > CurrentSchemaVersion → return false` |

### N-04: Missing Optional Quests Section

| Aspect | Detail |
|--------|--------|
| Input | `slot_1.json` has no `"Quests"` field (old pre-WAVE18 save) |
| Expected | `ValidateAndNormalizeSave` sets `saveData.Quests = new QuestStateSectionSaveData()` |
| SaveManager behavior | `RestoreQuestSaveData(new QuestStateSectionSaveData())` → `QuestService.RestoreFromSaveData` with 0 quests |
| Player effect | Quest state starts fresh (same as before WAVE18) |
| Code path | `saveData.Quests ??= new QuestStateSectionSaveData()` in ValidateAndNormalizeSave |

### N-05: Missing Optional Cave Enemy/Loot Section

| Aspect | Detail |
|--------|--------|
| Input | No enemy runtime state in save (expected — WAVE17 uses DROP_DIRECT_TO_INVENTORY) |
| Expected | No crash; enemies spawned fresh by CaveSmokeTestSpawnerBridge |
| Player effect | Enemies respawn fresh on cave entry; loot already in inventory from previous run |
| Code path | N/A — no enemy state section exists |

### N-06: Invalid QuestId in Saved State

| Aspect | Detail |
|--------|--------|
| Input | `QuestStateSaveData.QuestId = "quest_nonexistent"` in save file |
| Expected | `QuestService.RestoreFromSaveData` restores the record (string, valid) |
| Quest behavior | `QuestService.AcceptQuest("quest_nonexistent")` would fail (not in registry), but saved state is still present |
| Risk | LOW — record is inert if QuestId not in registry |

### N-07: Duplicate GrantedRewardId in Save

| Aspect | Detail |
|--------|--------|
| Input | `GrantedRewardIds = ["reward_gold_100", "reward_gold_100"]` (duplicate) |
| Expected | Restored list has duplicates; TurnIn still checks `!record.GrantedRewardIds.Contains(...)` |
| Result | Reward not duplicated because `Contains()` returns true |
| Risk | LOW — duplicates harmless due to guard |

### N-08: Invalid NpcId in NpcSaveData

| Aspect | Detail |
|--------|--------|
| Input | `NpcSaveData.NpcId = ""` or unregistered ID |
| Expected | NpcManager.RestoreFromSaveData skips or handles gracefully per implementation |
| Risk | LOW — empty NpcId is filtered in NpcManager |

### N-09: Duplicate EnemyRuntimeId

| Aspect | Detail |
|--------|--------|
| Input | N/A — no EnemyRuntimeSaveData section in this spec |
| Result | NOT_APPLICABLE |

### N-10: Duplicate PickupRuntimeId

| Aspect | Detail |
|--------|--------|
| Input | N/A — no LootPickupRuntimeSaveData section in this spec |
| Result | NOT_APPLICABLE |

---

## Load Safety Summary

| Scenario | Save file safe? | Save data overwritten? | Game crashes? |
|---------|----------------|----------------------|--------------|
| Corrupted JSON | YES (not touched) | NO | NO |
| Empty file | YES (not touched) | NO | NO |
| Future schema | YES (rejected before apply) | NO | NO |
| Missing Quests field | YES (normalized) | NO — empty init | NO |
| Missing enemy section | N/A | N/A | NO |
| Invalid QuestId | Accepted | Quest inert if not in registry | NO |
| Duplicate RewardIds | Accepted | Harmless due to Contains guard | NO |
