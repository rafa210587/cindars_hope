using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBiomeData", menuName = "CindarsHope/Cave/Biome Data")]
    public sealed class CaveBiomeDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private int _minLevel = 1;
        [SerializeField] private int _maxLevel = 10;
        [SerializeField] private string _tilePaletteId;
        [SerializeField] private string[] _enemySpawnProfileIds;
        [SerializeField] private string[] _resourceSpawnProfileIds;
        [SerializeField] private string[] _environmentHazardIds;
        [SerializeField] private bool _fishingSpotAllowed;
        [SerializeField] private string _defaultRoomStyleId;

        // Legacy fields for backwards compatibility
        [SerializeField] private string[] _allowedEnemyIds;
        [SerializeField] private string[] _allowedResourceNodeIds;

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public int MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        public int MaxLevel
        {
            get => _maxLevel;
            set => _maxLevel = value;
        }

        public string TilePaletteId => _tilePaletteId;
        public string[] EnemySpawnProfileIds => _enemySpawnProfileIds ?? System.Array.Empty<string>();
        public string[] ResourceSpawnProfileIds => _resourceSpawnProfileIds ?? System.Array.Empty<string>();
        public string[] EnvironmentHazardIds => _environmentHazardIds ?? System.Array.Empty<string>();
        public bool FishingSpotAllowed => _fishingSpotAllowed;
        public string DefaultRoomStyleId => _defaultRoomStyleId;

        // Legacy property for backwards compatibility
        public string[] AllowedEnemyIds
        {
            get => _allowedEnemyIds ?? System.Array.Empty<string>();
            set => _allowedEnemyIds = value;
        }

        // Legacy property for backwards compatibility
        public string[] AllowedResourceNodeIds
        {
            get => _allowedResourceNodeIds ?? System.Array.Empty<string>();
            set => _allowedResourceNodeIds = value;
        }

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            _minLevel = Mathf.Max(1, _minLevel);
            _maxLevel = Mathf.Max(_minLevel, _maxLevel);
        }
    }
}
