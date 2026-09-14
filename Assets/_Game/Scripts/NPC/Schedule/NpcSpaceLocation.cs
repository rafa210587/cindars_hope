using System;
using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    [Serializable]
    public struct NpcSpaceLocation
    {
        public string HouseId;
        public string AnchorId;
        public Vector2 LocalPosition;

        public bool IsInterior => !string.IsNullOrEmpty(HouseId);
    }
}
