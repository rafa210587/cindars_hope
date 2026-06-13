namespace CindarsHope.Core.Events
{
    public class StatusAppliedEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }
        public string SourceId { get; }
        public float DurationSeconds { get; }

        public StatusAppliedEvent(string targetId, string statusId, string sourceId, float durationSeconds)
        {
            TargetId = targetId ?? string.Empty;
            StatusId = statusId ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            DurationSeconds = durationSeconds;
        }
    }

    public class StatusRefreshedEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }
        public float NewDurationSeconds { get; }

        public StatusRefreshedEvent(string targetId, string statusId, float newDurationSeconds)
        {
            TargetId = targetId ?? string.Empty;
            StatusId = statusId ?? string.Empty;
            NewDurationSeconds = newDurationSeconds;
        }
    }

    public class StatusTickedEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }
        public int Damage { get; }

        public StatusTickedEvent(string targetId, string statusId, int damage)
        {
            TargetId = targetId ?? string.Empty;
            StatusId = statusId ?? string.Empty;
            Damage = damage;
        }
    }

    public class StatusExpiredEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }

        public StatusExpiredEvent(string targetId, string statusId)
        {
            TargetId = targetId ?? string.Empty;
            StatusId = statusId ?? string.Empty;
        }
    }

    public class StatusRemovedEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }

        public StatusRemovedEvent(string targetId, string statusId)
        {
            TargetId = targetId ?? string.Empty;
            StatusId = statusId ?? string.Empty;
        }
    }

    public class PlayerDamagedEvent
    {
        public int DamageAmount { get; }
        public UnityEngine.Vector3 WorldPosition { get; }
        public string SourceId { get; }
        public string SourceName { get; }

        public PlayerDamagedEvent(int damageAmount, UnityEngine.Vector3 worldPosition, string sourceId = "", string sourceName = "")
        {
            DamageAmount = damageAmount;
            WorldPosition = worldPosition;
            SourceId = sourceId ?? string.Empty;
            SourceName = sourceName ?? string.Empty;
        }
    }

    public class DamageAppliedEvent
    {
        public CindarsHope.Combat.DamageResult DamageResult { get; }
        public UnityEngine.Vector3 TargetPosition { get; }

        public DamageAppliedEvent(CindarsHope.Combat.DamageResult result, UnityEngine.Vector3 targetPosition = default)
        {
            DamageResult = result;
            TargetPosition = targetPosition;
        }
    }

    public class VulnerabilityWindowStartedEvent
    {
        public string TargetId { get; }
        public float DurationSeconds { get; }

        public VulnerabilityWindowStartedEvent(string targetId, float durationSeconds)
        {
            TargetId = targetId ?? string.Empty;
            DurationSeconds = durationSeconds;
        }
    }

    public class VulnerabilityWindowEndedEvent
    {
        public string TargetId { get; }

        public VulnerabilityWindowEndedEvent(string targetId)
        {
            TargetId = targetId ?? string.Empty;
        }
    }
}
