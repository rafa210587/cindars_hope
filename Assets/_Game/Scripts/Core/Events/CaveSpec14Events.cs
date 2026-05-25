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

    public readonly struct CaveCheckpointTeleportRequestedEvent
    {
        public readonly int DestinationLevel;

        public CaveCheckpointTeleportRequestedEvent(int destinationLevel)
        {
            DestinationLevel = destinationLevel;
        }
    }

    public readonly struct CaveCheckpointTeleportCompletedEvent
    {
        public readonly int DestinationLevel;
        public readonly bool Success;

        public CaveCheckpointTeleportCompletedEvent(int destinationLevel, bool success)
        {
            DestinationLevel = destinationLevel;
            Success = success;
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

    // Debug Events
    public readonly struct CaveDebugSkipUsedEvent
    {
        public readonly string Action;
        public readonly int TargetLevel;

        public CaveDebugSkipUsedEvent(string action, int targetLevel)
        {
            Action = action;
            TargetLevel = targetLevel;
        }
    }

    // Level Snapshot Events
    public readonly struct CaveLevelSnapshotCreatedEvent
    {
        public readonly int CaveLevel;
        public readonly string LayoutHash;

        public CaveLevelSnapshotCreatedEvent(int caveLevel, string layoutHash)
        {
            CaveLevel = caveLevel;
            LayoutHash = layoutHash;
        }
    }

    public readonly struct CaveLevelSnapshotLoadedEvent
    {
        public readonly int CaveLevel;
        public readonly string LayoutHash;

        public CaveLevelSnapshotLoadedEvent(int caveLevel, string layoutHash)
        {
            CaveLevel = caveLevel;
            LayoutHash = layoutHash;
        }
    }

    // Level Transition Events
    public readonly struct CaveLevelTransitionBlockedEvent
    {
        public readonly int BlockedLevel;
        public readonly string Reason;

        public CaveLevelTransitionBlockedEvent(int blockedLevel, string reason)
        {
            BlockedLevel = blockedLevel;
            Reason = reason;
        }
    }
}
