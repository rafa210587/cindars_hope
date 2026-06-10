# WAVE_INTEGRATION_18 — Existing Functionality Matrix

**Date:** 2026-06-10

---

## Save Infrastructure

| System | Exists Before WAVE18 | After WAVE18 | Notes |
|--------|---------------------|--------------|-------|
| SaveManager | YES | UNCHANGED | gap-closure methods added |
| Safe write (.tmp) | YES | UNCHANGED | WriteTextSafely already uses .tmp |
| Migrations v1→v5 | YES | UNCHANGED | no new migration needed |
| Cross-scene load | YES | UNCHANGED | LoadSceneAndApplySaveData coroutine |
| ValidateAndNormalizeSave | YES | EXTENDED | Quests normalization added |
| TryReadSaveWithMigration | YES | UNCHANGED | |
| WriteTextSafely | YES | UNCHANGED | |

---

## Data Sections

| Section | Before WAVE18 | After WAVE18 | Captured? | Restored? |
|---------|--------------|--------------|-----------|---------|
| Player | YES | UNCHANGED | YES | YES |
| Inventory | YES | UNCHANGED | YES | YES |
| Equipment | YES | UNCHANGED | YES | YES |
| Hotbar | YES | UNCHANGED | YES | YES |
| Progression | YES | UNCHANGED | YES | YES |
| Farm | YES | UNCHANGED | YES (FarmScene only) | YES |
| World | YES | UNCHANGED | YES (FarmScene only) | YES |
| Cave | YES | UNCHANGED | YES | YES |
| Death | YES | UNCHANGED | YES | YES |
| Economy | YES | UNCHANGED | YES | YES |
| Crafting | YES | UNCHANGED | YES | YES |
| Stamina | YES | UNCHANGED | YES | YES |
| GameTime | YES | UNCHANGED | YES | YES |
| PlayerStatusEffects | YES | UNCHANGED | YES | YES |
| EquipmentDurability | YES | UNCHANGED | YES | YES |
| Npcs | YES | UNCHANGED | YES (TownScene only) | YES |
| ActiveSkillSlots | YES | UNCHANGED | YES | YES |
| SkillTree | YES | UNCHANGED | YES | YES |
| Bestiary | YES | UNCHANGED | YES | YES |
| **Quests** | **NO (gap)** | **IMPLEMENTED** | **YES** | **YES** |

---

## Quest System (Before WAVE18)

| Aspect | Before | After |
|--------|--------|-------|
| QuestService exists | YES | UNCHANGED |
| QuestRegistry exists | YES | UNCHANGED |
| GrantedRewardIds in QuestStateRecord | YES | UNCHANGED |
| TurnIn idempotency | YES | UNCHANGED |
| QuestStateSection serializable | NO | IMPLEMENTED (QuestStateSectionSaveData) |
| GameSaveData.Quests field | NO | IMPLEMENTED |
| SaveManager captures Quests | NO | IMPLEMENTED |
| SaveManager restores Quests | NO | IMPLEMENTED |
| QuestRuntimeBootstrap.CaptureSaveData | NO | IMPLEMENTED |
| QuestRuntimeBootstrap.RestoreFromSaveData | NO | IMPLEMENTED |
| QuestService.RestoreFromSaveData | NO | IMPLEMENTED |
| Pending save data for pre-init restore | NO | IMPLEMENTED |

---

## NPC System (Before WAVE18)

| Aspect | Status |
|--------|--------|
| NpcManager.CaptureSaveData | ALREADY_IMPLEMENTED |
| NpcManager.RestoreFromSaveData | ALREADY_IMPLEMENTED |
| NpcSaveData fields (NpcId, SceneId, Position, HasMet) | ALREADY_IMPLEMENTED |
| Save preserves NPC section outside TownScene | ALREADY_IMPLEMENTED |
| All canonical NPCs covered | AUDIT_PASS (see NPC_SAVE_MATRIX) |

---

## Cave System (Before WAVE18)

| Aspect | Status |
|--------|--------|
| CaveRunManager.CaptureSaveData | ALREADY_IMPLEMENTED |
| CaveRunManager.RestoreFromSaveData | ALREADY_IMPLEMENTED |
| CaveWorldSeed / CaveRunSeed | ALREADY_IMPLEMENTED |
| VisitedLevelSnapshots | ALREADY_IMPLEMENTED |
| BossDefeatStates | ALREADY_IMPLEMENTED |
| DepletedNodeIds | ALREADY_IMPLEMENTED |
| Enemy HP mid-combat | NOT_IMPLEMENTED (debt) |

---

## Summary

```text
Gap closure spec correctly targeted only:
1. QuestStateSection → now persisted (main gap closed)
2. Reward idempotency → GrantedRewardIds preserved in save
3. Enemy/loot state → DROP_DIRECT_TO_INVENTORY; loot in inventory (covered)
4. NPC/Cave → already implemented; validated not recreated

SaveManager NOT rewritten.
SchemaVersion NOT changed.
No new migrations created.
```
