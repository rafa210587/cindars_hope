using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>Transient melee-capstone state persisted with primitive fields only.</summary>
    [Serializable]
    public sealed class CombatCapstoneSaveData
    {
        public string ActiveVariant = string.Empty;
        public int ActiveRank;
        public float RemainingSeconds;
        public bool HealChargeArmed;
        public string LastResolutionId = string.Empty;
        public List<string> ProcessedResolutionIds = new List<string>();
        public string MagicArmedVariant = string.Empty;
        public int MagicArmedRank;
        public float MagicArmedRemainingSeconds;
        public float MagicLockoutRemainingSeconds;
        public int MagicMaxManaSnapshot;
        public List<int> MagicLedgerAmounts = new List<int>();
        public List<float> MagicLedgerAges = new List<float>();
        public int MagicEchoTotal;
        public int MagicEchoDelivered;
        public float MagicEchoDurationSeconds;
        public float MagicEchoElapsedSeconds;
    }
}
