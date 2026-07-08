using System;
using System.Collections.Generic;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Save
{
    [Serializable]
    public class DeathSaveData
    {
        public PlayerCorpseRecoverySaveData ActiveCorpse;
        public DeathStatsSaveData DeathStats = new DeathStatsSaveData();
    }

    [Serializable]
    public class DeathStatsSaveData
    {
        public int TotalCaveDeaths;
        public int LastDeathAtGameDay = -1;
        public int LastDeathAtCaveLevel = -1;
        public string LastCorpseId = string.Empty;
    }

    [Serializable]
    public class PlayerCorpseRecoverySaveData
    {
        public string CorpseId;
        public int CorpseStatusValue;
        public string RunId;
        public string CaveSeed;
        public int CaveLevel;
        public string SceneName;
        public Vector2 Position;
        public int GoldAmount;
        public int CreatedAtGameDay = -1;
        public float CreatedAtGameTime;
        public int RecoveredAtGameDay = -1;
        public string ReplacedByCorpseId = string.Empty;
        public List<InventorySlotSaveData> LostInventoryItems = new List<InventorySlotSaveData>();
        public List<InventorySlotSaveData> LostEquipmentItems = new List<InventorySlotSaveData>();
    }
}
