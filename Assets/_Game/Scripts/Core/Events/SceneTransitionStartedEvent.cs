namespace CindarsHope.Core.Events
{
    public readonly struct SceneTransitionStartedEvent
    {
        public readonly string SourceSceneName;
        public readonly string TargetSceneName;
        public readonly string TargetSpawnId;

        public SceneTransitionStartedEvent(string sourceSceneName, string targetSceneName, string targetSpawnId)
        {
            SourceSceneName = sourceSceneName;
            TargetSceneName = targetSceneName;
            TargetSpawnId = targetSpawnId;
        }
    }
}
