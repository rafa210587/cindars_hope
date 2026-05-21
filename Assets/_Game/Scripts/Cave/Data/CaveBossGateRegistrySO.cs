using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBossGateRegistry", menuName = "CindarsHope/Cave/Boss Gate Registry")]
    public sealed class CaveBossGateRegistrySO : ScriptableObject
    {
        [SerializeField] private List<CaveBossGateDataSO> _gates = new List<CaveBossGateDataSO>();

        public IReadOnlyList<CaveBossGateDataSO> Gates => _gates.AsReadOnly();

        public CaveBossGateDataSO GetGateById(string id)
        {
            foreach (var gate in _gates)
            {
                if (gate.Id == id)
                {
                    return gate;
                }
            }
            return null;
        }

        public CaveBossGateDataSO GetGateByLevel(int caveLevel)
        {
            foreach (var gate in _gates)
            {
                if (gate.CaveLevel == caveLevel)
                {
                    return gate;
                }
            }
            return null;
        }

        public bool IsBossGateLevel(int caveLevel)
        {
            return GetGateByLevel(caveLevel) != null;
        }

        public int GetNextCheckpointAfterDefeat(int caveLevel)
        {
            var gate = GetGateByLevel(caveLevel);
            return gate != null ? gate.CheckpointUnlockedOnDefeat : caveLevel;
        }
    }
}
