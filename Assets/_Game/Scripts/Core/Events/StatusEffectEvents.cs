namespace CindarsHope.Core.Events
{
    /// <summary>F01 — publicado quando um status canônico é aplicado (inimigo ou player).</summary>
    public readonly struct StatusEffectAppliedEvent
    {
        public string TargetId { get; }
        public string StatusId { get; }

        public StatusEffectAppliedEvent(string targetId, string statusId)
        {
            TargetId = targetId;
            StatusId = statusId;
        }
    }
}
