using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBossGate", menuName = "CindarsHope/Cave/Boss Gate")]
    public sealed class CaveBossGateDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;
        [SerializeField] private int _caveLevel;
        [SerializeField] private string _biomeId;
        [SerializeField] private string _bossEnemyId;
        [SerializeField] private int _checkpointUnlockedOnDefeat;
        [SerializeField] private string _requiredPreviousGateId;
        [SerializeField] private bool _isDebugCompletable;
        [SerializeField] private int _unlocksCheckpointPortalDestination;
        [SerializeField] private string[] _uniqueRewardIds;
        [SerializeField] private string _repeatableLootTableId;

        // Legacy field for backwards compatibility
        [SerializeField] private int _checkpointUnlockedOnDefeatLegacy;

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public int CaveLevel
        {
            get => _caveLevel;
            set => _caveLevel = value;
        }

        public string BiomeId
        {
            get => _biomeId;
            set => _biomeId = value;
        }

        public string BossEnemyId
        {
            get => _bossEnemyId;
            set => _bossEnemyId = value;
        }

        public int CheckpointUnlockedOnDefeat
        {
            get => _checkpointUnlockedOnDefeat;
            set => _checkpointUnlockedOnDefeat = value;
        }

        public string RequiredPreviousGateId => _requiredPreviousGateId;
        public bool IsDebugCompletable => _isDebugCompletable;
        public int UnlocksCheckpointPortalDestination => _unlocksCheckpointPortalDestination > 0 ? _unlocksCheckpointPortalDestination : _caveLevel;
        public string[] UniqueRewardIds => _uniqueRewardIds ?? System.Array.Empty<string>();
        public string RepeatableLootTableId => _repeatableLootTableId;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            _caveLevel = Mathf.Max(1, _caveLevel);
            _checkpointUnlockedOnDefeat = Mathf.Max(1, _checkpointUnlockedOnDefeat);
            if (_unlocksCheckpointPortalDestination <= 0)
            {
                _unlocksCheckpointPortalDestination = _caveLevel;
            }
        }
    }
}
