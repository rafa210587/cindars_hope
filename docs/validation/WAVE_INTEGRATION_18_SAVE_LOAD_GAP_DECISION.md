# WAVE_INTEGRATION_18 — Save/Load Gap Closure: Decision Report

**Date:** 2026-06-10
**Branch:** dev

---

## Sources Read

| Source | Exists | Notes |
|--------|--------|-------|
| docs/project/CURRENT_STATE.md | YES | Active state |
| Assets/_Game/Scripts/Save/SaveData.cs | YES | Full DTO audit |
| Assets/_Game/Scripts/Save/SaveManager.cs | YES | Full implementation audit |
| Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs | YES | Gap confirmed |
| Assets/_Game/Scripts/Quests/Runtime/QuestService.cs | YES | API audit |
| Assets/_Game/Scripts/Quests/Save/QuestStateSection.cs | YES | Property-based — not JsonUtility-compatible |
| Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs | YES | Property-based — not JsonUtility-compatible |
| Assets/_Game/Scripts/Save/CaveSaveData.cs | YES | Cave state confirmed |
| docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md | YES | WAVE17 strategy confirmed |
| docs/validation/WAVE_INTEGRATION_17_EXTRACTION_PRESERVATION_MATRIX.md | YES | DROP_DIRECT_TO_INVENTORY confirmed |

Design direction sources (not all docs exist yet — registered below):

| Source | Exists | Notes |
|--------|--------|-------|
| docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md | NOT_FOUND | Registered as missing; existing code is canonical |
| docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md | NOT_FOUND | QuestService code is canonical |
| docs/decisions/ADR-0005-cave-stable-run-and-replay.md | NOT_FOUND | CaveSaveData code is canonical |

Missing sources: used code as canonical fallback per spec rule.

---

## Repo Prevalidation

### Already Exists (ALREADY_IMPLEMENTED)

| System | Status | Evidence |
|--------|--------|---------|
| SaveManager | ALREADY_IMPLEMENTED | SaveManager.cs — schema v5, SaveGame, LoadGame, ApplySaveData |
| Safe write (.tmp) | ALREADY_IMPLEMENTED | WriteTextSafely uses slot_1.json.tmp |
| Migrations | ALREADY_IMPLEMENTED | SaveMigrationRegistry with 4 migrations (v1→v5) |
| Cross-scene load | ALREADY_IMPLEMENTED | LoadSceneAndApplySaveData coroutine |
| GameSaveData path | FOUND | Assets/_Game/Scripts/Save/SaveData.cs |
| Player save | ALREADY_IMPLEMENTED | CapturePlayerSaveData + RestoreFromSaveData |
| Inventory save | ALREADY_IMPLEMENTED | CaptureInventorySaveData + RestoreFromSaveData |
| Economy/shop save | ALREADY_IMPLEMENTED | CaptureEconomySaveData + RestoreEconomySaveData |
| Crafting save | ALREADY_IMPLEMENTED | CaptureCraftingSaveData + LoadFromSaveData |
| NPC position save | ALREADY_IMPLEMENTED (TownScene only) | CaptureNpcSaveData preserves outside Town |
| CaveRun save | ALREADY_IMPLEMENTED | CaveRunManager.CaptureSaveData/RestoreFromSaveData |
| Stamina save | ALREADY_IMPLEMENTED | CaptureStaminaSaveData |
| GameTime save | ALREADY_IMPLEMENTED | CaptureGameTimeSaveData |
| Death/corpse save | ALREADY_IMPLEMENTED | CaptureDeathSaveData + RestoreDeathSaveData |
| Skill tree save | ALREADY_IMPLEMENTED | CaptureSkillTreeSaveData |
| Active skill slots save | ALREADY_IMPLEMENTED | CaptureActiveSkillSlotsSaveData |
| Bestiary save | ALREADY_IMPLEMENTED | CaptureBestiarySaveData |

### Gaps Identified (MISSING/PARTIAL)

| Gap | Status | Action |
|-----|--------|--------|
| GameSaveData.Quests field | MISSING | IMPLEMENTED in this spec |
| QuestStateSection serializable DTO | MISSING | IMPLEMENTED (QuestStateSectionSaveData) |
| SaveManager capture quest | MISSING | IMPLEMENTED (CaptureQuestSaveData) |
| SaveManager restore quest | MISSING | IMPLEMENTED (RestoreQuestSaveData) |
| QuestRuntimeBootstrap Capture/Restore static APIs | MISSING | IMPLEMENTED |
| QuestService.RestoreFromSaveData | MISSING | IMPLEMENTED |
| QuestRuntimeBootstrap pending save data | MISSING | IMPLEMENTED (_pendingSaveData + SetPendingSaveData) |
| Enemy alive HP/position state | MISSING | WAVE17_DIRECT_DROP_DEBT (see section below) |
| Loot ground pickup state | N/A | DROP_DIRECT_TO_INVENTORY — no ground pickups in WAVE17 |

---

## Quest Save Decision

**Decision:** IMPLEMENT

**Reason:** `QuestRuntimeBootstrap.Initialize()` creates `new QuestStateSection()` — all quest progress is lost on restart.

**Approach:**
- `QuestStateSection` uses C# properties — not serializable by `JsonUtility`
- Created `QuestStateSectionSaveData` + `QuestStateSaveData` + `QuestObjectiveStateSaveData` as `[Serializable]` classes with public fields
- Added `GameSaveData.Quests` field
- Added `SaveManager.CaptureQuestSaveData()` and `RestoreQuestSaveData()`
- Added `QuestRuntimeBootstrap.CaptureSaveData()`, `RestoreFromSaveData()`, `SetPendingSaveData()`
- Added `QuestService.RestoreFromSaveData()` which preserves `GrantedRewardIds` (reward idempotency)
- Pending save data handled for the case where `LoadGame()` runs before `QuestRuntimeBootstrap.Initialize()`

---

## NPC Save Decision

**Decision:** ALREADY_IMPLEMENTED — validate only

**Evidence:** `NpcManager.CaptureSaveData()` captures NpcId, SceneId, Position, HasMet for all registered NPCs. `CaptureNpcSaveData()` in SaveManager preserves existing Npcs section when scene != TownScene.

---

## Cave Enemy/Loot Save Decision

**Decision:** WAVE17_DIRECT_DROP_STRATEGY — no enemy runtime state needed for current scope

**Reason:**
- WAVE17 uses `EnemyDropSpawner` → `InventoryManager.AddItem` (DROP_DIRECT_TO_INVENTORY)
- No ground pickup objects exist
- `CaveSmokeTestSpawnerBridge` spawns enemies dynamically at runtime — not persistent scene objects
- Defeated enemy state within a session is handled by `enemy.SetActive(false)` (in-memory)
- `InventoryManager` is DontDestroyOnLoad — loot from defeated enemies is preserved in inventory and saved with inventory save
- **Residual debt:** If player saves mid-cave with a damaged (not defeated) enemy, HP resets to max on load. Documented as `CAVE_ENEMY_HP_SAVE_DEBT`.

---

## SchemaVersion Decision

**Decision:** NO CHANGE — remain at v5

**Reason:** Adding optional `Quests` field to `GameSaveData`. `JsonUtility.FromJson` leaves new optional fields as null for old saves. `ValidateAndNormalizeSave` normalizes `Quests ??= new QuestStateSectionSaveData()`. Backward compatible — no migration needed.

---

## Migration Decision

**Decision:** NO NEW MIGRATION

**Reason:** No breaking schema change. Existing saves load fine — `Quests` defaults to null → normalized to empty section.

---

## Files Allowed/Prohibited

Allowed (modified):
- `Assets/_Game/Scripts/Save/SaveData.cs`
- `Assets/_Game/Scripts/Save/SaveManager.cs`
- `Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs`
- `Assets/_Game/Scripts/Quests/Runtime/QuestService.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateWave18SaveLoadGapClosure.cs` (new)

Prohibited (not touched):
- `Packages/**`, `ProjectSettings/**`, `Assets/_Game/Scenes/**`
- `GameEventBus.cs`, `SceneManagement/**`

---

## Design/Direction Compliance Matrix

| Source | Rule | Applied? | Evidence |
|--------|------|----------|---------|
| Save/load design intent | Persist state via DTOs/simple types | YES | QuestStateSectionSaveData has only string/int/bool/List fields |
| Save/load design intent | Preserve scene-bound sections when scene not loaded | YES | NpcSaveData preserved outside TownScene; CaveSaveData preserved outside Cave |
| Quest design intent | QuestState/rewards must be idempotent | YES | GrantedRewardIds restored; TurnIn already guards via GrantedRewardIds |
| NPC design intent | NPCs need stable identity | YES | NpcManager captures NpcId+SceneId+Position+HasMet |
| Cave design intent | Cave run identity/seed matter | YES | CaveRunManager captures CaveWorldSeed+CaveRunSeed+all state |
| WAVE15 report | QuestService uses in-memory QuestStateSection | CLOSED | QuestStateSection now persisted via QuestStateSectionSaveData |
| WAVE17 report | loot/enemy save was deferred | CLOSED | DROP_DIRECT_TO_INVENTORY — loot in inventory (persisted); enemy HP debt documented |
