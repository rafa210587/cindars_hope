using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    [Serializable]
    public sealed class CorpseSaveData
    {
        public string CorpseId;
        public int CorpseStatusValue;  // CorpseStatus enum as int
        public string RunId;
        public string CaveSeed;
        public int CaveLevel;
        public string SnapshotLayoutHash;
        public string SceneName;
        public Vector2 Position;
        public string SafeAnchorId = string.Empty;
        public int GoldAmount;
        public List<CorpseItemSaveData> InventoryItems = new();
        public List<CorpseItemSaveData> EquipmentItems = new();
        public int CreatedAtGameDay = -1;
        public float CreatedAtGameTime;
        public int RecoveredAtGameDay = -1;
        public string ReplacedByCorpseId = string.Empty;
    }

    [Serializable]
    public sealed class CorpseItemSaveData
    {
        public string ItemId;
        public int Amount = 1;
        public string ItemInstanceId = string.Empty;
        public float DurabilityCurrent = 1f;
        public float DurabilityMax = 1f;
        public bool IsBroken;
        public int SourceSlotType = -1;  // SlotType enum as int
        public int SourceSlotIndex = -1;
    }

    [Serializable]
    public sealed class DeathStatsSaveData
    {
        public int TotalCaveDeaths;
        public int LastDeathAtGameDay = -1;
        public int LastDeathAtCaveLevel = -1;
        public string LastCorpseId = string.Empty;
    }

    [Serializable]
    public sealed class DeathSaveData
    {
        public CorpseSaveData ActiveCorpse;
        public DeathStatsSaveData DeathStats = new();
    }
}
