namespace CindarsHope.Enemy
{
    public class EnemyActionRuntime
    {
        public readonly string ActionId;
        private float _lastUsedTime;
        private readonly float _cooldownSeconds;

        public EnemyActionRuntime(string actionId, float cooldownSeconds)
        {
            ActionId = actionId;
            _cooldownSeconds = cooldownSeconds;
            _lastUsedTime = -cooldownSeconds; // ready on first tick
        }

        public bool IsReady(float currentTime) => currentTime >= _lastUsedTime + _cooldownSeconds;

        public void MarkUsed(float currentTime) => _lastUsedTime = currentTime;
    }
}
