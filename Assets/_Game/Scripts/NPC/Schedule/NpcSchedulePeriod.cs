namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// fable_19 (CA-1) — vocabulário SEMÂNTICO de períodos do dia ABSORVIDO do sistema duplicado
    /// <c>City/Schedule/SchedulePeriod</c> (WAVE 08) para o sistema canônico <c>NPC/Schedule/</c>
    /// (WI-25). As janelas horárias são as canônicas de city_rules.md Rule 6 / CITY_LAYOUT §19.
    ///
    /// O sistema canônico resolve a posição por <see cref="NpcTimeBlock"/> (granularidade grossa,
    /// TIME_BLOCK_DEBT do WAVE25). Este enum fino + o crosswalk
    /// <see cref="NpcSchedulePeriodHelper.ToTimeBlock"/> preservam a semântica nomeada sem perda,
    /// de modo que um consumidor que precise do período exato (ex.: condições de diálogo) tenha a
    /// fonte única canônica aqui, e o runtime continue colapsando em Work/Social/Home/Night.
    /// </summary>
    public enum NpcSchedulePeriod
    {
        Morning = 0,        // 06:00-09:00
        WorkStart,          // 09:00-12:00
        Midday,             // 12:00-14:00
        WorkAfternoon,      // 14:00-18:00
        Evening,            // 18:00-21:00
        Night,              // 21:00-00:00
        SleepLateNight      // 00:00-06:00
    }

    /// <summary>
    /// fable_19 — crosswalk determinístico hora → período fino → bloco grosso canônico.
    /// É o ponto único que reconcilia o vocabulário absorvido (period) com a resolução de runtime
    /// (block). Mantém paridade 1:1 com <c>City/Schedule/SchedulePeriodHelper.FromHour</c> (obsoleto).
    /// </summary>
    public static class NpcSchedulePeriodHelper
    {
        /// <summary>Período fino canônico para a hora [0..23] (city_rules.md Rule 6).</summary>
        public static NpcSchedulePeriod FromHour(int hour)
        {
            if (hour >= 0 && hour < 6)   return NpcSchedulePeriod.SleepLateNight;
            if (hour >= 6 && hour < 9)   return NpcSchedulePeriod.Morning;
            if (hour >= 9 && hour < 12)  return NpcSchedulePeriod.WorkStart;
            if (hour >= 12 && hour < 14) return NpcSchedulePeriod.Midday;
            if (hour >= 14 && hour < 18) return NpcSchedulePeriod.WorkAfternoon;
            if (hour >= 18 && hour < 21) return NpcSchedulePeriod.Evening;
            return NpcSchedulePeriod.Night;
        }

        /// <summary>Hora de início do período (espelha o helper obsoleto, sem perda).</summary>
        public static int PeriodStartHour(NpcSchedulePeriod period)
        {
            switch (period)
            {
                case NpcSchedulePeriod.Morning:       return 6;
                case NpcSchedulePeriod.WorkStart:     return 9;
                case NpcSchedulePeriod.Midday:        return 12;
                case NpcSchedulePeriod.WorkAfternoon: return 14;
                case NpcSchedulePeriod.Evening:       return 18;
                case NpcSchedulePeriod.Night:         return 21;
                default:                              return 0;
            }
        }

        /// <summary>
        /// Crosswalk do período fino absorvido para o bloco grosso de runtime (CA-1: sem perda
        /// semântica). Manhã/início de trabalho = Morning; meio-dia/tarde = Midday; noite social =
        /// Evening; noite tardia/madrugada = Night (o NPC vai para a âncora home — "BedDefinition →
        /// âncora home" absorvido).
        /// </summary>
        public static NpcTimeBlock ToTimeBlock(NpcSchedulePeriod period)
        {
            switch (period)
            {
                case NpcSchedulePeriod.Morning:
                case NpcSchedulePeriod.WorkStart:
                    return NpcTimeBlock.Morning;
                case NpcSchedulePeriod.Midday:
                case NpcSchedulePeriod.WorkAfternoon:
                    return NpcTimeBlock.Midday;
                case NpcSchedulePeriod.Evening:
                    return NpcTimeBlock.Evening;
                case NpcSchedulePeriod.Night:
                case NpcSchedulePeriod.SleepLateNight:
                    return NpcTimeBlock.Night;
                default:
                    return NpcTimeBlock.Default;
            }
        }

        /// <summary>Bloco grosso de runtime diretamente da hora (helper de conveniência).</summary>
        public static NpcTimeBlock BlockFromHour(int hour) => ToTimeBlock(FromHour(hour));

        /// <summary>
        /// True se o período corresponde a um bloco em que o NPC está na âncora home (noite tardia /
        /// madrugada). Reconcilia o conceito de "cama/home" do sistema absorvido (City/Schedule
        /// BedDefinition + FallbackWaypoint) com a âncora <c>npc_&lt;id&gt;_home</c> do canônico.
        /// </summary>
        public static bool IsHomePeriod(NpcSchedulePeriod period) =>
            period == NpcSchedulePeriod.SleepLateNight || period == NpcSchedulePeriod.Night;
    }
}
