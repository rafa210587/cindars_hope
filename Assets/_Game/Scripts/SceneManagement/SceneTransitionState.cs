namespace CindarsHope.SceneManagement
{
    public static class SceneTransitionState
    {
        public static string PendingSpawnId { get; private set; }

        public static void SetPendingSpawn(string spawnId)
        {
            PendingSpawnId = spawnId;
        }

        public static void ClearPendingSpawn()
        {
            PendingSpawnId = string.Empty;
        }
    }
}
