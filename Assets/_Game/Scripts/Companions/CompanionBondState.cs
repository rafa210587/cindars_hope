using System;

namespace CindarsHope.Companions
{
    [Serializable]
    public class CompanionBondState
    {
        public string CompanionId;
        public int BondLevel;
        public int TrustPoints;
        public int Fatigue;
        public InjuryState InjuryState = InjuryState.Healthy;
        public int UnlockedRoleFlags;
        public int JobRank;
        public int CaveRank;
        public int LastInteractionDay;
    }
}
