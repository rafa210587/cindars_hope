using System.Collections.Generic;
using CindarsHope.World.Calendar;

namespace CindarsHope.NPC.Social
{
    /// <summary>
    /// fable_57 — consulta PURA e determinística de aniversário, fonte única para (a) o multiplicador
    /// de presente do FriendshipService (F26) e (b) a projection do calendário (F37/F20). Sem Unity,
    /// sem estado salvo, sem GameObject.Find: recebe o <see cref="GameDate"/> corrente do chamador
    /// (que já o conhece via DayStartedEvent / GameCalendarService) e cruza com a <see cref="NpcBirthdayTable"/>.
    ///
    /// NÃO é um tracker social novo (regra de não-duplicação da spec): só responde "hoje é aniversário?".
    /// </summary>
    public static class NpcBirthdayService
    {
        /// <summary>Multiplicador de pontos do presente entregue no dia do aniversário (×2).</summary>
        public const int BirthdayGiftMultiplier = 2;

        /// <summary>True se hoje (<paramref name="today"/>) é o aniversário de <paramref name="npcId"/>.</summary>
        public static bool IsBirthdayToday(string npcId, GameDate today)
        {
            if (string.IsNullOrEmpty(npcId)) return false;
            if (!NpcBirthdayTable.TryGet(npcId, out var b)) return false;
            return b.Season == today.CurrentSeason && b.DayOfSeason == today.DayInSeason;
        }

        /// <summary>Sobrecarga por dia absoluto (o que o FriendshipService rastreia internamente).</summary>
        public static bool IsBirthdayToday(string npcId, int absoluteDay)
        {
            return IsBirthdayToday(npcId, GameDate.FromAbsoluteDay(absoluteDay));
        }

        /// <summary>
        /// Multiplicador a aplicar no presente: ×2 no aniversário, ×1 nos demais dias.
        /// Ponto único consumido pelo FriendshipService (sem segundo caminho de pontos).
        /// </summary>
        public static int GiftMultiplierToday(string npcId, int absoluteDay)
        {
            return IsBirthdayToday(npcId, absoluteDay) ? BirthdayGiftMultiplier : 1;
        }

        /// <summary>NPCs que fazem aniversário na data (0..1 com a tabela atual) — consumido pelo calendário.</summary>
        public static IEnumerable<NpcBirthdayTable.Birthday> BirthdaysOn(GameDate date)
        {
            return NpcBirthdayTable.BirthdaysOn(date.CurrentSeason, date.DayInSeason);
        }
    }
}
