using System;
using System.Collections.Generic;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public class EnemySpawnPlanEntry
    {
        public string PlannedEnemyInstanceId;
        public string EnemyId;
        public string InitialAnchorId;
        public string CurrentAnchorId;
        public bool IsDefeated;
        public int DefeatedAtGameDay = -1;
        public int RespawnAvailableAtGameDay = -1;
        public bool IsBoss;
        public List<string> UniqueDropsClaimed = new List<string>();
        public string RedistributionGroupId;

        public EnemySpawnPlanEntry()
        {
        }

        public EnemySpawnPlanEntry(string plannedInstanceId, string enemyId, string anchorId)
        {
            PlannedEnemyInstanceId = plannedInstanceId;
            EnemyId = enemyId;
            InitialAnchorId = anchorId;
            CurrentAnchorId = anchorId;
            IsDefeated = false;
            IsBoss = false;
        }
    }

    [Serializable]
    public class EnemyRedistributionState
    {
        public int RedistributionCount;
        public string LastRedistributionReason;
        public int RedistributionSeedOffset;
    }

    [Serializable]
    public class EnemyRespawnState
    {
        public int RespawnDelayGameDays = 2;
        public int LastRespawnEvaluationDay = -1;
    }

    public class CaveLevelEnemyPlan
    {
        public List<EnemySpawnPlanEntry> EnemyPlans = new List<EnemySpawnPlanEntry>();
        public EnemyRedistributionState RedistributionState = new EnemyRedistributionState();
        public EnemyRespawnState RespawnState = new EnemyRespawnState();

        public bool HasActiveEnemies()
        {
            foreach (var entry in EnemyPlans)
            {
                if (!entry.IsDefeated)
                {
                    return true;
                }
            }
            return false;
        }

        public List<EnemySpawnPlanEntry> GetActiveEnemies()
        {
            var active = new List<EnemySpawnPlanEntry>();
            foreach (var entry in EnemyPlans)
            {
                if (!entry.IsDefeated)
                {
                    active.Add(entry);
                }
            }
            return active;
        }

        public List<EnemySpawnPlanEntry> GetDefeatedEnemies()
        {
            var defeated = new List<EnemySpawnPlanEntry>();
            foreach (var entry in EnemyPlans)
            {
                if (entry.IsDefeated)
                {
                    defeated.Add(entry);
                }
            }
            return defeated;
        }
    }
}
