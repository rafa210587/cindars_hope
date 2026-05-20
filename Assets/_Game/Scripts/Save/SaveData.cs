using System;
using System.Collections.Generic;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Player.Progression;
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
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int CurrentHP;
        public int MaxHP;
        public int Gold;
        public int CurrentHunger;
        public int MaxHunger;
        public Vector2 PlayerPosition;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemSaveData> Items = new List<InventoryItemSaveData>();
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
}
