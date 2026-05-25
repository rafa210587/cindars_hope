using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    public sealed class Corpse
    {
        public string CorpseId { get; set; }
        public CorpseStatus Status { get; set; } = CorpseStatus.None;
        public string RunId { get; set; }
        public string CaveSeed { get; set; }
        public int CaveLevel { get; set; }
        public string SnapshotLayoutHash { get; set; }
        public string SceneName { get; set; }
        public Vector2 Position { get; set; }
        public string SafeAnchorId { get; set; } = string.Empty;
        public int GoldAmount { get; set; }
        public List<CorpseItem> InventoryItems { get; } = new();
        public List<CorpseItem> EquipmentItems { get; } = new();
        public int CreatedAtGameDay { get; set; } = -1;
        public float CreatedAtGameTime { get; set; }
        public int RecoveredAtGameDay { get; set; } = -1;
        public string ReplacedByCorpseId { get; set; } = string.Empty;

        public Corpse()
        {
        }

        public Corpse(string corpseId)
        {
            CorpseId = corpseId;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CorpseId) &&
                   !string.IsNullOrWhiteSpace(RunId) &&
                   Status != CorpseStatus.None &&
                   CaveLevel > 0;
        }

        public bool IsFullyRecovered()
        {
            return Status == CorpseStatus.Recovered &&
                   GoldAmount == 0 &&
                   InventoryItems.Count == 0 &&
                   EquipmentItems.Count == 0;
        }

        public int GetTotalRecoverableGold()
        {
            return GoldAmount;
        }

        public int GetTotalRecoverableItems()
        {
            return InventoryItems.Count + EquipmentItems.Count;
        }
    }

    public sealed class CorpseItem
    {
        public string ItemId { get; set; }
        public int Amount { get; set; } = 1;
        public string ItemInstanceId { get; set; } = string.Empty;
        public float DurabilityCurrent { get; set; } = 1f;
        public float DurabilityMax { get; set; } = 1f;
        public bool IsBroken { get; set; }
        public int SourceSlotType { get; set; } = -1;
        public int SourceSlotIndex { get; set; } = -1;

        public CorpseItem()
        {
        }

        public CorpseItem(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
