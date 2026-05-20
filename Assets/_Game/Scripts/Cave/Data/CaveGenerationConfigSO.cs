using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveGenerationConfig", menuName = "CindarsHope/Cave/Generation Config")]
    public sealed class CaveGenerationConfigSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public int TargetWidth = 80;
        public int TargetHeight = 48;
        public int MinRooms = 8;
        public int MaxRooms = 14;
        public int MinRoomWidth = 6;
        public int MaxRoomWidth = 14;
        public int MinRoomHeight = 4;
        public int MaxRoomHeight = 10;
        public int ExtraConnectionChancePercent = 20;
        public int EnemyPointCount = 6;
        public int ResourcePointCount = 8;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            TargetWidth = Mathf.Max(8, TargetWidth);
            TargetHeight = Mathf.Max(8, TargetHeight);
            MinRooms = Mathf.Max(1, MinRooms);
            MaxRooms = Mathf.Max(MinRooms, MaxRooms);
            MinRoomWidth = Mathf.Max(2, MinRoomWidth);
            MaxRoomWidth = Mathf.Max(MinRoomWidth, MaxRoomWidth);
            MinRoomHeight = Mathf.Max(2, MinRoomHeight);
            MaxRoomHeight = Mathf.Max(MinRoomHeight, MaxRoomHeight);
            ExtraConnectionChancePercent = Mathf.Clamp(ExtraConnectionChancePercent, 0, 100);
            EnemyPointCount = Mathf.Max(0, EnemyPointCount);
            ResourcePointCount = Mathf.Max(0, ResourcePointCount);
        }
    }
}
