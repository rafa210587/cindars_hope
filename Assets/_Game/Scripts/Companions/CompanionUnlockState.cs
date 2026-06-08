using System;
using System.Collections.Generic;

namespace CindarsHope.Companions
{
    [Serializable]
    public class CompanionUnlockState
    {
        public string CompanionId;
        public string NpcId;
        public UnlockState State = UnlockState.Locked;
        public List<string> UnlockedRoles = new List<string>();
        public List<string> UnlockedByQuestIds = new List<string>();
        public int UnlockedByReputationTier;
        public int UnlockedDay;
    }
}
