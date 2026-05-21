using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBossGate", menuName = "CindarsHope/Cave/Boss Gate")]
    public sealed class CaveBossGateDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] public string Id;
        [SerializeField] public int CaveLevel;
        [SerializeField] public string BiomeId;
        [SerializeField] public string BossEnemyId;
        [SerializeField] public int CheckpointUnlockedOnDefeat;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            CaveLevel = Mathf.Max(1, CaveLevel);
            CheckpointUnlockedOnDefeat = Mathf.Max(1, CheckpointUnlockedOnDefeat);
        }
    }
}
