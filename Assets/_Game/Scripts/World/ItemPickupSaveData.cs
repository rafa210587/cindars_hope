using System;
using UnityEngine;

namespace CindarsHope.World
{
    [Serializable]
    public class ItemPickupSaveData
    {
        public int PickupIndex;
        public string ItemId;
        public int Amount;
        public Vector2 Position;
        public bool IsCollected;
    }
}
