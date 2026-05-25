namespace CindarsHope.Core.Events
{
    public class PlayerDodgeStartedEvent
    {
    }

    public class PlayerDodgeEndedEvent
    {
    }

    public class SpellCastStartedEvent
    {
        public string SpellId { get; }

        public SpellCastStartedEvent(string spellId)
        {
            SpellId = spellId ?? string.Empty;
        }
    }

    public class SpellCastSucceededEvent
    {
        public string SpellId { get; }

        public SpellCastSucceededEvent(string spellId)
        {
            SpellId = spellId ?? string.Empty;
        }
    }

    public class SpellCastFailedEvent
    {
        public string SpellId { get; }
        public string Reason { get; }

        public SpellCastFailedEvent(string spellId, string reason = "Unknown")
        {
            SpellId = spellId ?? string.Empty;
            Reason = reason ?? "Unknown";
        }
    }

    public class SkillActionExecutedEvent
    {
        public string SkillActionId { get; }

        public SkillActionExecutedEvent(string skillActionId)
        {
            SkillActionId = skillActionId ?? string.Empty;
        }
    }

    public class ActiveSkillSlotChangedEvent
    {
        public int SlotIndex { get; }
        public string SkillActionId { get; }

        public ActiveSkillSlotChangedEvent(int slotIndex, string skillActionId)
        {
            SlotIndex = slotIndex;
            SkillActionId = skillActionId;
        }
    }
}
