using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    [Serializable]
    public sealed class SurvivalSkillSaveData
    {
        public int Version = 4;
        public string ActiveRunId = string.Empty;
        public int ActiveCaveLevel;
        public string ActiveEncounterId = string.Empty;
        public int ActiveEncounterOrdinal;
        public string EncounterOrdinalRunId = string.Empty;
        public int NextEncounterOrdinal = 1;
        public List<string> ActiveEnemyInstanceIds = new List<string>();
        public List<string> LeashedEnemyInstanceIds = new List<string>();
        public float EncounterQuietRemainingSeconds;
        public string LunarConsumedEncounterId = string.Empty;
        public string LunarConsumedFirstTargetInstanceId = string.Empty;
        public string ActiveLunarTargetInstanceId = string.Empty;
        public string ActiveLunarVariant = string.Empty;
        public int ActiveLunarRank;
        public float ActiveLunarRemainingSeconds;
        public string ActiveLunarActionToken = string.Empty;
        public int LastOffensiveMagicDamageType;
        public List<string> ConsumedLastBreathEncounterIds = new List<string>();
        public float LastBreathArmedRemainingSeconds;
        public string CampUsedRunId = string.Empty;
        public string ActiveCampRunId = string.Empty;
        public int ActiveCampCaveLevel;
        public float ActiveCampRemainingSeconds;
        public float ActiveCampPositionX;
        public float ActiveCampPositionY;
        public string CavebornConsumedRunId = string.Empty;
        public string ActiveCavebornRunId = string.Empty;
        public int ActiveCavebornRank;
        public float ActiveCavebornRemainingSeconds;
        public bool CavebornWasInCombat;
        public bool CavebornRegenDoubled;
    }
}
