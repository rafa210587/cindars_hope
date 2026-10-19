using System.Collections.Generic;

namespace CindarsHope.Enemy
{
    public class EnemySpawnResult
    {
        public string RequestSummary;
        public List<EnemySpawnSelection> SelectedEnemies = new List<EnemySpawnSelection>();
        public string SelectedPackId;
        public List<EnemySpawnRejectedCandidate> RejectedCandidates = new List<EnemySpawnRejectedCandidate>();
        public List<string> Warnings = new List<string>();
        public bool UsedFallback;
        public bool IsValid;
    }

    public class EnemySpawnSelection
    {
        public string EnemyId;
        public int Count;
        public string SpawnProfileId;
        public string PackId;
        public string SizeClass;
        public bool IsElite;
    }

    public class EnemySpawnRejectedCandidate
    {
        public string EnemyId;
        public string Reason;

        public EnemySpawnRejectedCandidate() { }

        public EnemySpawnRejectedCandidate(string enemyId, string reason)
        {
            EnemyId = enemyId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }
    }

    public class EnemySpawnCandidate
    {
        public string EnemyId;
        public string SpawnProfileId;
        public string PackId;
        public int Weight;
        public string SizeClass;
        public bool IsElite;
    }
}
