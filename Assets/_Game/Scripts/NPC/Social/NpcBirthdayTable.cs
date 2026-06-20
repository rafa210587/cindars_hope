using System.Collections.Generic;
using CindarsHope.World.Calendar;

namespace CindarsHope.NPC.Social
{
    /// <summary>
    /// fable_57 — tabela ESTÁTICA e determinística de aniversários dos 23 NPCs canônicos do roster
    /// (<see cref="NpcTownRosterRegistry"/>). Pura (sem Unity, sem random, sem estado salvo): a data
    /// é editorial e estável entre runs. NÃO é um sistema social novo — é só o conteúdo de data
    /// consumido pelo calendário (F37/F20) e pelo multiplicador de presente do FriendshipService (F26).
    ///
    /// Invariantes (CA-1):
    ///   - cobertura 23/23 (todos os NpcId do roster têm aniversário);
    ///   - datas válidas no calendário do jogo (season 0-3, dia 1..28);
    ///   - no máximo 1 NPC por dia do ano (sem aniversários empilhados);
    ///   - nenhum aniversário cai num dia de festival público (Primavera 14 / Verão 28 / Inverno 14),
    ///     para o calendário não sobrepor os dois conteúdos no mesmo dia (risco técnico da spec).
    /// </summary>
    public static class NpcBirthdayTable
    {
        /// <summary>Aniversário de um NPC: estação + dia da estação (1..28). Estrutura simples (data).</summary>
        public readonly struct Birthday
        {
            public readonly string NpcId;
            public readonly Season Season;
            public readonly int DayOfSeason;

            public Birthday(string npcId, Season season, int dayOfSeason)
            {
                NpcId = npcId;
                Season = season;
                DayOfSeason = dayOfSeason;
            }

            /// <summary>Dia-no-ano (1..112), no mesmo cálculo de FestivalCalendar/FestivalRegistry.</summary>
            public int DayInYear => DayOfSeason + ((int)Season * GameDate.DaysPerSeason);
        }

        // Distribuição editorial pelas 4 estações, 1 NPC/dia, desviando dos dias de festival
        // (Primavera 14, Verão 28, Inverno 14). Ordem = roster (7 MVP + 16 estendidos), 5-6/estação.
        private static readonly List<Birthday> s_birthdays = new List<Birthday>
        {
            // ── Primavera (Season 0) — evita dia 14 (Festival do Plantio) ──
            new Birthday("npc_pip",       Season.Primavera, 3),
            new Birthday("npc_sylveth",   Season.Primavera, 8),
            new Birthday("npc_eiran",     Season.Primavera, 12),
            new Birthday("npc_mirela",    Season.Primavera, 18),
            new Birthday("npc_liora",     Season.Primavera, 23),
            new Birthday("npc_savra",     Season.Primavera, 27),

            // ── Verão (Season 1) — evita dia 28 (Festival do Mercado) ──
            new Birthday("npc_brumdar",   Season.Verao, 4),
            new Birthday("npc_renko",     Season.Verao, 9),
            new Birthday("npc_zrix",      Season.Verao, 14),
            new Birthday("npc_yael",      Season.Verao, 19),
            new Birthday("npc_dagna",     Season.Verao, 24),

            // ── Outono (Season 2) — sem festival fixo ──
            new Birthday("npc_thalindra", Season.Outono, 2),
            new Birthday("npc_nimble",    Season.Outono, 7),
            new Birthday("npc_corvus",    Season.Outono, 11),
            new Birthday("npc_mara",      Season.Outono, 16),
            new Birthday("npc_gurd",      Season.Outono, 21),
            new Birthday("npc_ozzra",     Season.Outono, 26),

            // ── Inverno (Season 3) — evita dia 14 (Festival da Colheita) ──
            new Birthday("npc_hund",      Season.Inverno, 5),
            new Birthday("npc_gruta",     Season.Inverno, 10),
            new Birthday("npc_alaric",    Season.Inverno, 17),
            new Birthday("npc_orlan",     Season.Inverno, 22),
            new Birthday("npc_tovin",     Season.Inverno, 25),
            new Birthday("npc_maelor",    Season.Inverno, 28),
        };

        /// <summary>Todas as 23 entradas (somente leitura).</summary>
        public static IReadOnlyList<Birthday> All => s_birthdays;

        /// <summary>Quantidade canônica esperada (23, igual ao roster).</summary>
        public static int Count => s_birthdays.Count;

        /// <summary>Tenta obter o aniversário de um NPC. npcId desconhecido ⇒ false.</summary>
        public static bool TryGet(string npcId, out Birthday birthday)
        {
            birthday = default;
            if (string.IsNullOrEmpty(npcId)) return false;
            foreach (var b in s_birthdays)
            {
                if (b.NpcId == npcId)
                {
                    birthday = b;
                    return true;
                }
            }
            return false;
        }

        /// <summary>True se o aniversário de <paramref name="npcId"/> cai na data informada.</summary>
        public static bool IsBirthday(string npcId, Season season, int dayOfSeason)
        {
            return TryGet(npcId, out var b) && b.Season == season && b.DayOfSeason == dayOfSeason;
        }

        /// <summary>NPCs (0..1 com a tabela atual) que fazem aniversário na data informada.</summary>
        public static IEnumerable<Birthday> BirthdaysOn(Season season, int dayOfSeason)
        {
            foreach (var b in s_birthdays)
            {
                if (b.Season == season && b.DayOfSeason == dayOfSeason)
                {
                    yield return b;
                }
            }
        }
    }
}
