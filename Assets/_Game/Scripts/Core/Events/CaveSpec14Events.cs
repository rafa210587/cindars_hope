namespace CindarsHope.Core.Events
{
    // NOTE: EnemyKilledEvent and EnemySeenEvent already exist in EnemyEvents.cs
    // This file contains SPEC 14-specific cave events only

    // Boss Gate Events
    public readonly struct CaveBossGateCompletedEvent
    {
        public readonly string GateId;
        public readonly int CaveLevel;

        public CaveBossGateCompletedEvent(string gateId, int caveLevel)
        {
            GateId = gateId;
            CaveLevel = caveLevel;
        }
    }

    public readonly struct CaveBossUniqueRewardClaimedEvent
    {
        public readonly string GateId;
        public readonly string RewardId;

        public CaveBossUniqueRewardClaimedEvent(string gateId, string rewardId)
        {
            GateId = gateId;
            RewardId = rewardId;
        }
    }

    // Checkpoint Events
    public readonly struct CaveCheckpointPortalOpenedEvent
    {
        public readonly int CheckpointLevel;
        public readonly bool IsFarmEntrance;

        public CaveCheckpointPortalOpenedEvent(int checkpointLevel, bool isFarmEntrance)
        {
            CheckpointLevel = checkpointLevel;
            IsFarmEntrance = isFarmEntrance;
        }
    }

    // Enemy Respawn and Redistribution Events
    public readonly struct CaveEnemyRespawnScheduledEvent
    {
        public readonly string PlannedEnemyInstanceId;
        public readonly string EnemyId;
        public readonly int RespawnAtGameDay;

        public CaveEnemyRespawnScheduledEvent(string plannedEnemyInstanceId, string enemyId, int respawnAtGameDay)
        {
            PlannedEnemyInstanceId = plannedEnemyInstanceId;
            EnemyId = enemyId;
            RespawnAtGameDay = respawnAtGameDay;
        }
    }

    public readonly struct CaveEnemyRespawnedEvent
    {
        public readonly string PlannedEnemyInstanceId;
        public readonly string EnemyId;
        public readonly int CaveLevel;

        public CaveEnemyRespawnedEvent(string plannedEnemyInstanceId, string enemyId, int caveLevel)
        {
            PlannedEnemyInstanceId = plannedEnemyInstanceId;
            EnemyId = enemyId;
            CaveLevel = caveLevel;
        }
    }

    public readonly struct CaveEnemiesRedistributedEvent
    {
        public readonly int CaveLevel;
        public readonly string Reason;

        public CaveEnemiesRedistributedEvent(int caveLevel, string reason)
        {
            CaveLevel = caveLevel;
            Reason = reason;
        }
    }

}
