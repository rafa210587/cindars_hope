namespace CindarsHope.Core.Events
{
    public readonly struct CaveBossDefeatedEvent
    {
        public readonly string BossGateId;
        public readonly int CaveLevel;
        public readonly int UnlockedCheckpointLevel;

        public CaveBossDefeatedEvent(string bossGateId, int caveLevel, int unlockedCheckpointLevel)
        {
            BossGateId = bossGateId;
            CaveLevel = caveLevel;
            UnlockedCheckpointLevel = unlockedCheckpointLevel;
        }
    }
}
