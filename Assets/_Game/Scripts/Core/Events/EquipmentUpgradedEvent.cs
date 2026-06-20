namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_49 — publicado quando um equipamento recebe +1/+2/+3 focado na forja. Consumido pelo toast
    /// existente (PlayerActionFeedbackEvent) — nenhum evento existente muda.
    /// </summary>
    public readonly struct EquipmentUpgradedEvent
    {
        public readonly string ItemInstanceId;
        public readonly int Level;
        public readonly string Focus; // estável: damage/durability/weight/stamina/block

        public EquipmentUpgradedEvent(string itemInstanceId, int level, string focus)
        {
            ItemInstanceId = itemInstanceId ?? string.Empty;
            Level = level;
            Focus = focus ?? string.Empty;
        }
    }
}
