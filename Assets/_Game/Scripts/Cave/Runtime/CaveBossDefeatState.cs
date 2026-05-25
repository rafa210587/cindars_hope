using System;
using System.Collections.Generic;
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
        [SerializeField] public List<string> UniqueRewardsClaimed = new List<string>();

        public CaveBossDefeatState()
        {
            UniqueRewardsClaimed = new List<string>();
        }

        public CaveBossDefeatState(string bossGateId, int caveLevel, bool isDefeated)
        {
            BossGateId = bossGateId;
            CaveLevel = caveLevel;
            IsDefeated = isDefeated;
            DefeatedAt = isDefeated ? DateTime.UtcNow.ToString("O") : null;
            UniqueRewardsClaimed = new List<string>();
        }

        public void MarkAsDefeated()
        {
            IsDefeated = true;
            DefeatedAt = DateTime.UtcNow.ToString("O");
        }
    }
}
