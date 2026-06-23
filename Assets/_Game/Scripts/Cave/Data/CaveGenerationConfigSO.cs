using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveGenerationConfig", menuName = "CindarsHope/Cave/Generation Config")]
    public sealed class CaveGenerationConfigSO : ScriptableObject, IIdentifiedData
    {
        public string Id;

        [Header("Map Dimensions")]
        public int TargetWidth = 160;
        public int TargetHeight = 96;

        [Header("Rooms")]
        public int MinRooms = 8;
        public int MaxRooms = 14;
        public int MinRoomWidth = 12;
        public int MaxRoomWidth = 28;
        public int MinRoomHeight = 8;
        public int MaxRoomHeight = 20;
        public int ExtraConnectionChancePercent = 20;

        [Header("Corridors")]
        // Bumped 2->3 / 3->4 after the player VisualScale went 1.5->2.0: a ~1.2-unit-wide player collider
        // filled ~60% of a 2-tile corridor (1 tile = 1 world unit). 3 tiles restores comfortable clearance
        // with room for an enemy to share the corridor. Drives a GenerationConfigVersion bump below.
        [Tooltip("Minimum corridor width in tiles (1 = single tile, spec target >= 2).")]
        public int CorridorMinWidth = 3;
        [Tooltip("Maximum corridor width in tiles.")]
        public int CorridorMaxWidth = 4;

        [Header("Boss Arena")]
        [Tooltip("Minimum side length for boss arena rooms in tiles.")]
        public int BossArenaMinSize = 20;

        [Header("Spawn")]
        public int EnemyPointCount = 10;
        public int ResourcePointCount = 12;
        [Tooltip("Minimum distance in tiles from a spawn point to any room or corridor wall.")]
        public float SpawnSafeRadius = 2.0f;
        [Tooltip("Minimum distance in tiles between resource node spawn points.")]
        public float ResourceSpacing = 4f;

        [Header("Versioning")]
        [Tooltip("Increment when generation params change to invalidate old snapshots.")]
        // fable_78 (14.1): bumped 3->4 — o tamanho-base do mapa passou a escalar por banda
        // (CaveBiomeLayoutProfile.ResolveMapSize), mudando layout/LayoutHash de níveis. Snapshots
        // legados são invalidados → regeneração determinística limpa. O asset
        // CaveGenerationConfig_Default.asset deve ser subido para 4 no Editor (DEFERRED_UNITY).
        public int GenerationConfigVersion = 4;

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
            CorridorMinWidth = Mathf.Max(1, CorridorMinWidth);
            CorridorMaxWidth = Mathf.Max(CorridorMinWidth, CorridorMaxWidth);
            BossArenaMinSize = Mathf.Max(4, BossArenaMinSize);
            EnemyPointCount = Mathf.Max(0, EnemyPointCount);
            ResourcePointCount = Mathf.Max(0, ResourcePointCount);
            SpawnSafeRadius = Mathf.Max(0f, SpawnSafeRadius);
            ResourceSpacing = Mathf.Max(0f, ResourceSpacing);
            GenerationConfigVersion = Mathf.Max(1, GenerationConfigVersion);
        }
    }
}
