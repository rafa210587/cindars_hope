namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_37 — publicado pelo WorldEventService no início do dia quando um festival sazonal ativa.
    /// Tipado e simples (sem GameObject/SO): consumido por toast, mural (F34), falas de festival (F28)
    /// e prato especial da Mirena (F25). FestivalId no formato canônico "festival_&lt;slug&gt;" para casar
    /// com as flags festival_active(id) e com FestivalQuestDefinition.FestivalId.
    /// </summary>
    public readonly struct FestivalStartedEvent
    {
        /// <summary>Id canônico do festival (ex.: "festival_plantio"). Vazio = nenhum festival hoje.</summary>
        public readonly string FestivalId;

        /// <summary>Nome de exibição (ex.: "Festival do Plantio").</summary>
        public readonly string DisplayName;

        /// <summary>Dia absoluto em que o festival está ativo.</summary>
        public readonly int DayNumber;

        public FestivalStartedEvent(string festivalId, string displayName, int dayNumber)
        {
            FestivalId = festivalId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            DayNumber = dayNumber;
        }
    }

    /// <summary>
    /// fable_37 — publicado pelo WorldEventService no início do dia quando um pico lunar e/ou um evento
    /// aleatório diário resolvem. Carrega ids/efeitos como strings simples (sem refs Unity).
    /// </summary>
    public readonly struct WorldEventStartedEvent
    {
        /// <summary>Id do pico lunar ativo hoje (ex.: "peak_green"). Vazio = sem pico hoje.</summary>
        public readonly string LunarPeakId;

        /// <summary>Id do evento aleatório diário resolvido (ex.: "event_star_shower"). Vazio = nenhum.</summary>
        public readonly string WorldEventId;

        /// <summary>Mensagem curta de anúncio (toast). Pode estar vazia.</summary>
        public readonly string AnnouncementMessage;

        /// <summary>Dia absoluto da resolução.</summary>
        public readonly int DayNumber;

        public WorldEventStartedEvent(string lunarPeakId, string worldEventId, string announcementMessage, int dayNumber)
        {
            LunarPeakId = lunarPeakId ?? string.Empty;
            WorldEventId = worldEventId ?? string.Empty;
            AnnouncementMessage = announcementMessage ?? string.Empty;
            DayNumber = dayNumber;
        }
    }
}
