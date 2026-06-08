using System;

namespace CindarsHope.Companions
{
    [Serializable]
    public class CompanionEligibilityFlags
    {
        public string NpcId;
        public bool CanBeFarmCompanion;
        public bool CanBeCaveCompanion;
        public bool CanBeQuestCompanion;
        public bool CanBeSocialCompanion;
        public bool CanBeRomanceCompanion;
        public bool CanBeSpouseCompanion;
        public bool CompanionLockedByStory;
        public bool CompanionLockedByReputation;
        public bool CompanionLockedByQuest;
        public bool CompanionUnavailable;
    }
}
