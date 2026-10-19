namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_53 — published when a festival quest (fq_*) is offered at the start of its festival day.
    /// Tipado e simples (sem refs Unity), espelhando os demais eventos de quest/festival. Consumido por
    /// UI/toast e diálogo do organizador. O fluxo de aceite/progresso/recompensa continua sendo o da
    /// F34 (QuestService) — este evento apenas anuncia a abertura da quest do dia.
    /// </summary>
    public readonly struct FestivalQuestOfferedEvent
    {
        /// <summary>Concrete per-year instance id (ex.: "fq_plantio_y1").</summary>
        public readonly string QuestId;

        /// <summary>Canonical template id (ex.: "fq_plantio").</summary>
        public readonly string TemplateId;

        /// <summary>Festival id that opened this quest (ex.: "festival_plantio").</summary>
        public readonly string FestivalId;

        /// <summary>Calendar year the quest is offered for (annual repetition key).</summary>
        public readonly int Year;

        public FestivalQuestOfferedEvent(string questId, string templateId, string festivalId, int year)
        {
            QuestId = questId ?? string.Empty;
            TemplateId = templateId ?? string.Empty;
            FestivalId = festivalId ?? string.Empty;
            Year = year;
        }
    }

    /// <summary>
    /// fable_53 — published when a festival quest expires at the end of its festival (CA-1). Expiry has
    /// NO punishment: the quest simply leaves the active log and reopens at the same festival next year.
    /// </summary>
    public readonly struct FestivalQuestExpiredEvent
    {
        /// <summary>Concrete per-year instance id that expired.</summary>
        public readonly string QuestId;

        /// <summary>Canonical template id.</summary>
        public readonly string TemplateId;

        public FestivalQuestExpiredEvent(string questId, string templateId)
        {
            QuestId = questId ?? string.Empty;
            TemplateId = templateId ?? string.Empty;
        }
    }
}
