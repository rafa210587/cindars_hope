using System;
using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.World
{
    [Serializable]
    public class ItemPickupSaveData
    {
        public int PickupIndex;
        public string PickupInstanceId;
        public string RuntimeGeneratedPickupId;
        public string ItemId;
        public string ItemInstanceId;
        public int Amount;
        public Vector2 Position;
        public bool IsCollected;
        public bool IsDynamic;
        public string SceneName;
        public int OriginSchemaVersion;
        public List<ItemOriginLotSaveData> OriginLots;
    }
}
