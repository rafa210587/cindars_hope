using System.Collections.Generic;
using CindarsHope.NPC;
using CindarsHope.NPC.Social;
using CindarsHope.World.Calendar;

namespace CindarsHope.UI.Calendar
{
    /// <summary>
    /// fable_57 — preenche a lista aditiva <see cref="CalendarDayDetailModel.Birthdays"/> a partir da
    /// <see cref="NpcBirthdayService"/> para a data do dia. PURO e determinístico (sem Unity, sem
    /// estado): a aba Calendário (F37/F20) chama isto ao montar o detalhe do dia. Não é um registro
    /// de eventos novo — é a tradução da tabela de aniversário para uma entrada de projection já
    /// existente (spoiler rules preservadas: aniversário é informação pública, sempre visível).
    ///
    /// O nome de exibição vem do <see cref="NpcTownRosterRegistry"/> (fallback = id), no mesmo padrão
    /// id→nome do resto do calendário.
    /// </summary>
    public static class CalendarBirthdayProjectionBuilder
    {
        /// <summary>Projeção dos aniversários (0..1 com a tabela atual) que caem em <paramref name="date"/>.</summary>
        public static List<NpcBirthdayProjection> BuildBirthdaysForDay(GameDate date)
        {
            var list = new List<NpcBirthdayProjection>();
            foreach (var b in NpcBirthdayService.BirthdaysOn(date))
            {
                string displayName = b.NpcId;
                if (NpcTownRosterRegistry.TryGet(b.NpcId, out var entry) && !string.IsNullOrEmpty(entry.DisplayName))
                {
                    displayName = entry.DisplayName;
                }
                list.Add(new NpcBirthdayProjection { NpcId = b.NpcId, DisplayName = displayName });
            }
            return list;
        }

        /// <summary>Conveniência: aplica os aniversários do dia ao modelo de detalhe (entrada aditiva).</summary>
        public static void ApplyTo(CalendarDayDetailModel model, GameDate date)
        {
            if (model == null) return;
            model.Birthdays = BuildBirthdaysForDay(date);
        }
    }
}
