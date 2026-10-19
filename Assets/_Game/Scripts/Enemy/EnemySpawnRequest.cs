using System.Collections.Generic;

namespace CindarsHope.Enemy
{
    public class EnemySpawnRequest
    {
        public int CaveLevel;
        public List<string> BiomeTags = new List<string>();
        public List<string> EnvironmentTags = new List<string>();
        public EnemyRoomSizeClass RoomSizeClass = EnemyRoomSizeClass.Small;
        public List<string> RoomTags = new List<string>();
        public List<string> BossGateProgressIds = new List<string>();
        public List<string> UnlockedFactionLockIds = new List<string>();
        public int Seed;
        public int MaxEnemies = 4;
        public bool AllowElite;
        public string DebugReason;

        public EnemySpawnRequest Normalize()
        {
            CaveLevel = System.Math.Max(1, CaveLevel);
            BiomeTags ??= new List<string>();
            EnvironmentTags ??= new List<string>();
            RoomTags ??= new List<string>();
            BossGateProgressIds ??= new List<string>();
            UnlockedFactionLockIds ??= new List<string>();
            MaxEnemies = System.Math.Max(0, MaxEnemies);
            DebugReason ??= string.Empty;
            return this;
        }

        public string BuildSummary()
        {
            return $"Level={CaveLevel}; Room={RoomSizeClass}; Max={MaxEnemies}; Seed={Seed}; Biome=[{string.Join(",", BiomeTags ?? new List<string>())}]; Env=[{string.Join(",", EnvironmentTags ?? new List<string>())}]; Reason={DebugReason}";
        }
    }
}
