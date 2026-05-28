using System;
using System.Collections.Generic;
using CindarsHope.Craft;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
using CindarsHope.UI.Hotbar;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int SchemaVersion;
        public int CurrentDay;
        public string CurrentSceneName;
        public string CurrentScenePath;
        public PlayerSaveData Player;
        public InventorySaveData Inventory;
        public EquipmentSaveData Equipment;
        public HotbarSaveData Hotbar;
        public PlayerProgressionSaveData Progression;
        public FarmSaveData Farm;
        public WorldSaveData World;
        public CaveSaveData Cave;
        public DeathSaveData Death;
        public EconomySaveData Economy;
        public CraftingRuntimeSaveData Crafting;
        public StaminaSaveData Stamina;
        public EquipmentDurabilitySaveData EquipmentDurability;
        public NpcManagerSaveData Npcs;
        public GameTimeSaveData GameTime;
        public PlayerStatusEffectsSaveData PlayerStatusEffects;
        public ActiveSkillSlotsSaveData ActiveSkillSlots;
        public SkillTreeSaveData SkillTree;
        public BestiarySaveData Bestiary;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int CurrentHP;
        public int MaxHP;
        public int Gold;
        public int CurrentHunger;
        public int MaxHunger;
        public int CurrentMana;
        public int MaxMana;
        public Vector2 PlayerPosition;
    }

    [Serializable]
    public class InventorySaveData
    {
        public int Capacity;
        public List<InventorySlotSaveData> Slots = new List<InventorySlotSaveData>();
        public List<InventoryItemSaveData> Items = new List<InventoryItemSaveData>();
    }

    [Serializable]
    public class InventorySlotSaveData
    {
        public int SlotIndex;
        public string ItemId;
        public int Amount;
        public bool IsEquipped;
        public string EquipmentBindingId;
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string ItemId;
        public int Amount;
    }

    [Serializable]
    public class FarmSaveData
    {
        public List<FarmPlotSaveData> Plots = new List<FarmPlotSaveData>();
        public List<TreeSaveData> Trees = new List<TreeSaveData>();
    }

    [Serializable]
    public class WorldSaveData
    {
        public List<ItemPickupSaveData> Pickups = new List<ItemPickupSaveData>();
        public List<TreeSaveData> Trees = new List<TreeSaveData>();
    }

    [Serializable]
    public class ShopStockSaveData
    {
        public string ShopId;
        public List<ShopItemStockEntry> Items = new List<ShopItemStockEntry>();
        public int LastRestockDay;
    }

    [Serializable]
    public class ShopItemStockEntry
    {
        public string ItemId;
        public int CurrentStock;
    }

    [Serializable]
    public class EconomySaveData
    {
        public List<ShopStockSaveData> Shops = new List<ShopStockSaveData>();
    }

    [Serializable]
    public class StaminaSaveData
    {
        public int CurrentStamina;
        public int MaxStamina;
    }

    [Serializable]
    public class EquipmentDurabilitySaveData
    {
        public List<DurabilityEntryData> EquipmentDurabilities = new List<DurabilityEntryData>();
    }

    [Serializable]
    public class DurabilityEntryData
    {
        public string ItemInstanceId;
        public int CurrentDurability;
        public int MaxDurability;
    }

    [Serializable]
    public class NpcManagerSaveData
    {
        public List<NpcSaveData> Npcs = new List<NpcSaveData>();
    }

    [Serializable]
    public class NpcSaveData
    {
        public string NpcId;
        public string SceneId;
        public Vector2 Position;
        public bool HasMet;
    }

    [Serializable]
    public class GameTimeSaveData
    {
        public int CurrentDay;
        public int CurrentPhase;
        public float PhaseElapsedSeconds;
    }

    [Serializable]
    public class PlayerStatusEffectsSaveData
    {
        public List<StatusEffectEntryData> ActiveEffects = new List<StatusEffectEntryData>();
    }

    [Serializable]
    public class StatusEffectEntryData
    {
        public string EffectId;
        public float RemainingSeconds;
    }

    [Serializable]
    public class DurabilityEntry
    {
        public int CurrentDurability;
        public int MaxDurability;
    }

    [Serializable]
    public class EquipmentSaveData
    {
        public string EquippedToolId;
        public List<EquipmentSlotSaveData> Slots = new List<EquipmentSlotSaveData>();
    }

    [Serializable]
    public class EquipmentSlotSaveData
    {
        public EquipmentSlot SlotType;
        public string ItemInstanceId;
    }

    [Serializable]
    public class ActiveSkillSlotsSaveData
    {
        public string SlotRSkillActionId;
        public string SlotTSkillActionId;
        public string SlotYSkillActionId;
        public string SlotGSkillActionId;
    }

}
