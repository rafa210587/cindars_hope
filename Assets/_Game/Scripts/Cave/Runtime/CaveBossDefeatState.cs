using System;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public sealed class CaveBossDefeatState
    {
        [SerializeField] public string BossGateId;
        [SerializeField] public int CaveLevel;
        [SerializeField] public bool IsDefeated;
        [SerializeField] public string DefeatedAt; // ISO 8601 timestamp

        public CaveBossDefeatState()
        {
        }

        public CaveBossDefeatState(string bossGateId, int caveLevel, bool isDefeated)
        {
            BossGateId = bossGateId;
            CaveLevel = caveLevel;
            IsDefeated = isDefeated;
            DefeatedAt = isDefeated ? DateTime.UtcNow.ToString("O") : null;
        }

        public void MarkAsDefeated()
        {
            IsDefeated = true;
            DefeatedAt = DateTime.UtcNow.ToString("O");
        }
    }
}
