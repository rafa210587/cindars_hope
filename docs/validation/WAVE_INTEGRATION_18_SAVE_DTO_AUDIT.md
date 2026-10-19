# WAVE_INTEGRATION_18 — Save DTO Audit

**Date:** 2026-06-10

---

## DTO Location Map

| DTO | Path | Found | Notes |
|-----|------|-------|-------|
| GameSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | Root save object, schema v5 |
| PlayerSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | CurrentHP, MaxHP, Gold, Position, Hunger, Mana |
| InventorySaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | Capacity + List<InventoryItemSaveData> + List<InventorySlotSaveData> |
| InventorySlotSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | SlotIndex, ItemId, Amount, IsEquipped |
| InventoryItemSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | ItemId, Amount (legacy format) |
| FarmSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | Plots + Trees |
| WorldSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | Pickups + Trees |
| EconomySaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | List<ShopStockSaveData> |
| ShopStockSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | ShopId, Items, LastRestockDay |
| StaminaSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | CurrentStamina, MaxStamina |
| EquipmentDurabilitySaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | List<DurabilityEntryData> |
| NpcManagerSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | List<NpcSaveData> |
| NpcSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | NpcId, SceneId, Position, HasMet |
| GameTimeSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | CurrentDay, CurrentPhase, PhaseElapsedSeconds |
| PlayerStatusEffectsSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | List<StatusEffectEntryData> |
| EquipmentSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | EquippedToolId, Slots |
| ActiveSkillSlotsSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | SlotR/T/Y/G SkillActionId |
| CompanionManagerSaveData | Assets/_Game/Scripts/Save/SaveData.cs | YES | List<CompanionSaveEntry> |
| CaveSaveData | Assets/_Game/Scripts/Save/CaveSaveData.cs | YES | Level, Seeds, Checkpoints, Snapshots, BossStates |
| DeathSaveData | Assets/_Game/Scripts/Save/DeathSaveData.cs | YES | ActiveCorpse + DeathStats |
| CraftingRuntimeSaveData | Implied by CraftingRuntime.CaptureSaveData() | YES (inferred) | Returned by CraftingRuntime |
| BestiarySaveData | Referenced in SaveManager | YES (inferred) | Returned by BestiaryManager |
| **QuestStateSectionSaveData** | Assets/_Game/Scripts/Save/SaveData.cs | **CREATED** | New — [Serializable] DTO for quest save |
| **QuestStateSaveData** | Assets/_Game/Scripts/Save/SaveData.cs | **CREATED** | New — per-quest record |
| **QuestObjectiveStateSaveData** | Assets/_Game/Scripts/Save/SaveData.cs | **CREATED** | New — per-objective record |

---

## GameSaveData Section Audit

| Section | Field | Status | Captured | Restored |
|---------|-------|--------|----------|---------|
| Player | Player | HAS_SECTION | YES (CapturePlayerSaveData) | YES (RestoreFromSaveData) |
| Inventory | Inventory | HAS_SECTION | YES (CaptureInventorySaveData) | YES (RestoreFromSaveData) |
| Equipment | Equipment | HAS_SECTION | YES (CaptureEquipmentSaveData) | YES (RestoreFromSaveData) |
| Hotbar | Hotbar | HAS_SECTION | YES (HotbarProvider.Capture) | YES (HotbarProvider.Restore) |
| Progression | Progression | HAS_SECTION | YES (CaptureProgressionSaveData) | YES |
| Farm | Farm | HAS_SECTION | YES (FarmScene only) | YES |
| World | World | HAS_SECTION | YES (FarmScene only) | YES |
| Cave | Cave | HAS_SECTION | YES (CaveRunManager) | YES |
| Death | Death | HAS_SECTION | YES (CaptureDeathSaveData) | YES |
| Economy | Economy | HAS_SECTION | YES (CaptureEconomySaveData) | YES |
| Crafting | Crafting | HAS_SECTION | YES (CaptureCraftingSaveData) | YES |
| Stamina | Stamina | HAS_SECTION | YES (CaptureStaminaSaveData) | YES |
| GameTime | GameTime | HAS_SECTION | YES (CaptureGameTimeSaveData) | YES |
| PlayerStatusEffects | PlayerStatusEffects | HAS_SECTION | YES | YES |
| EquipmentDurability | EquipmentDurability | HAS_SECTION | YES | YES |
| Npcs | Npcs | HAS_SECTION | YES (TownScene only; preserved elsewhere) | YES |
| ActiveSkillSlots | ActiveSkillSlots | HAS_SECTION | YES | YES |
| SkillTree | SkillTree | HAS_SECTION | YES | YES |
| Bestiary | Bestiary | HAS_SECTION | YES | YES |
| Companions | Companions | HAS_SECTION_BUT_NOT_CAPTURED | No Capture method found in SaveManager | Debt |
| **Quests** | **Quests** | **WAS_MISSING → NOW_IMPLEMENTED** | **YES (CaptureQuestSaveData)** | **YES (RestoreQuestSaveData)** |

---

## Quest DTO Field Validation

`QuestStateSaveData` fields (all simple types — compliant with save-dto-simple-types-only):

| Field | Type | Is Simple? |
|-------|------|-----------|
| QuestId | string | YES |
| State | int | YES |
| CurrentStepId | string | YES |
| CompletedStepIds | List<string> | YES |
| FailedStepIds | List<string> | YES |
| ObjectiveStates | List<QuestObjectiveStateSaveData> | YES (nested DTO) |
| KnownObjectiveIds | List<string> | YES |
| KnownHints | List<string> | YES |
| StartedAtDay | int | YES |
| StartedAtTime | int | YES |
| CompletedAtDay | int | YES |
| Tracked | bool | YES |
| Discovered | bool | YES |
| FailureReason | string | YES |
| GrantedRewardIds | List<string> | YES |
| GrantedFlagIds | List<string> | YES |
| RepeatInstanceId | string | YES |

No UnityEngine.Object references in any quest DTO field.

---

## Enemy/Loot Runtime DTO Assessment

| Entity | DTO Needed? | Reason | Decision |
|--------|------------|--------|---------|
| EnemyRuntimeSaveData | NO | WAVE17 enemies spawned at runtime; not persistent scene objects | WAVE17_DIRECT_DROP_DEBT |
| LootPickupRuntimeSaveData | NO | DROP_DIRECT_TO_INVENTORY — no ground pickups | NOT_APPLICABLE |
| EnemyHP mid-combat | NO (debt) | Enemy SetActive(false) on death; HP not persisted mid-fight | CAVE_ENEMY_HP_SAVE_DEBT |

Loot from defeated enemies goes directly to `InventoryManager.AddItem` → persisted via `Inventory` section of GameSaveData.
