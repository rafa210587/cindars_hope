namespace CindarsHope.Core.Events
{
    public sealed class PlayerMagicCastCommittedEvent
    {
        public string ActionId { get; }
        public string ActionToken { get; }
        public CindarsHope.Foundation.SpellDiscipline Discipline { get; }
        public string DamageTypeId { get; }

        public PlayerMagicCastCommittedEvent(string actionId, string actionToken,
            CindarsHope.Foundation.SpellDiscipline discipline, string damageTypeId)
        {
            ActionId = actionId ?? string.Empty;
            ActionToken = actionToken ?? string.Empty;
            Discipline = discipline;
            DamageTypeId = damageTypeId ?? string.Empty;
        }
    }

    /// <summary>Single successful-commit path shared by item spells and active magic skills.</summary>
    public static class PlayerMagicCastCommit
    {
        public static bool TryCommit(CindarsHope.Foundation.SpellCastTransaction transaction,
            int maxManaAtCommit, string damageTypeId)
        {
            if (transaction == null || !transaction.Commit()) return false;
            var preparation = transaction.Preparation;
            CindarsHope.Foundation.SpellCastPreparationProvider.Commit(
                preparation, maxManaAtCommit);
            CindarsHope.Core.GameEventBus.Publish(new PlayerMagicCastCommittedEvent(
                preparation.Request.ActionId, preparation.Request.ActionToken,
                preparation.Request.Discipline, damageTypeId));
            return true;
        }
    }

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

    public class SkillActionExecutedEvent
    {
        public string SkillActionId { get; }

        public SkillActionExecutedEvent(string skillActionId)
        {
            SkillActionId = skillActionId ?? string.Empty;
        }
    }

    /// <summary>Transient presentation signal for skill animation, VFX and SFX bridges.</summary>
    public sealed class SkillCastPhaseChangedEvent
    {
        public string SkillActionId { get; }
        public string PhaseId { get; }
        public float DurationSeconds { get; }

        public SkillCastPhaseChangedEvent(string skillActionId, string phaseId, float durationSeconds)
        {
            SkillActionId = skillActionId ?? string.Empty;
            PhaseId = phaseId ?? string.Empty;
            DurationSeconds = durationSeconds < 0f ? 0f : durationSeconds;
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
