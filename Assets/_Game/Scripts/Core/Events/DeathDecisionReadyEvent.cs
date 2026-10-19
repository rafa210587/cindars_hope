namespace CindarsHope.Core.Events
{
    public readonly struct DeathDecisionReadyEvent
    {
        public readonly string SceneName;
        public readonly int TearCount;
        public DeathDecisionReadyEvent(string sceneName, int tearCount)
        { SceneName = sceneName; TearCount = tearCount; }
    }
    public readonly struct DeathReviveRequestedEvent { }
    public readonly struct DeathReviveCompletedEvent { }
}
