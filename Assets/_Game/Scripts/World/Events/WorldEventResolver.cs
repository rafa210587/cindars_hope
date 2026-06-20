using System.Collections.Generic;
using CindarsHope.World.Calendar;

namespace CindarsHope.World.Events
{
    /// <summary>Resultado puro da resolução de um dia: o que o mundo "é" naquele AbsoluteDay.</summary>
    public sealed class WorldDayResolution
    {
        public int AbsoluteDay;
        public string FestivalId = string.Empty;                 // vazio = sem festival
        public string LunarPeakId = string.Empty;                // vazio = sem pico
        public WorldEventDefinitions.LunarPeakEffect LunarEffect = WorldEventDefinitions.LunarPeakEffect.None;
        public float LunarMagnitude;
        public string WorldEventId = string.Empty;               // vazio = nenhum evento aleatório
        public WorldEventDefinitions.WorldEventEffect WorldEffect = WorldEventDefinitions.WorldEventEffect.None;
        public float WorldEventMagnitude;

        public bool HasFestival => !string.IsNullOrEmpty(FestivalId);
        public bool HasLunarPeak => !string.IsNullOrEmpty(LunarPeakId);
        public bool HasWorldEvent => !string.IsNullOrEmpty(WorldEventId);
    }

    /// <summary>
    /// fable_37 — resolução 100% DETERMINÍSTICA e PURA (sem Unity) de {festival, pico lunar, evento
    /// aleatório} para um dado AbsoluteDay e worldSeed. Festivais e picos são lookup por calendário
    /// (Season, DayInSeason); o evento aleatório é um roll ponderado por StableHash(worldSeed|dia) —
    /// mesma entrada ⇒ mesma saída em qualquer recomputação (CA-3 / rng-and-determinism / ADR-0005).
    /// Sem GUID/timestamp/Random não-semeado. Nada aqui persiste.
    /// </summary>
    public static class WorldEventResolver
    {
        /// <summary>FNV-1a 32-bit — MESMO algoritmo de CaveLayoutStableHash/ForageStableHash, replicado
        /// no namespace World para que World não dependa de Cave/Farm.</summary>
        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        // ── Festival ───────────────────────────────────────────────────────────────────────────
        public static WorldEventDefinitions.FestivalDefinition ResolveFestival(int absoluteDay)
        {
            var date = GameDate.FromAbsoluteDay(absoluteDay);
            foreach (var festival in WorldEventDefinitions.Festivals)
            {
                if (festival.Season == date.CurrentSeason && festival.DayInSeason == date.DayInSeason)
                {
                    return festival;
                }
            }

            return null;
        }

        // ── Pico lunar ─────────────────────────────────────────────────────────────────────────
        public static WorldEventDefinitions.LunarPeakDefinition ResolveLunarPeak(int absoluteDay)
        {
            var date = GameDate.FromAbsoluteDay(absoluteDay);
            foreach (var peak in WorldEventDefinitions.LunarPeaks)
            {
                if (peak.DayInSeason == date.DayInSeason)
                {
                    return peak;
                }
            }

            return null;
        }

        // ── Evento aleatório diário (roll ponderado determinístico) ──────────────────────────────
        public static WorldEventDefinitions.WorldEventDefinition ResolveRandomEvent(string worldSeed, int absoluteDay)
        {
            var pool = WorldEventDefinitions.RandomEventPool;
            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            var totalEventWeight = 0;
            foreach (var e in pool)
            {
                if (e != null && e.Weight > 0) totalEventWeight += e.Weight;
            }

            if (totalEventWeight <= 0)
            {
                return null;
            }

            // Peso de "nenhum evento": deriva do peso total dos eventos e da fração NoEventWeight01.
            // Ex.: NoEventWeight01=0.45 e totalEventWeight=80 ⇒ noEventWeight ≈ 65, denom ≈ 145.
            var noEventWeight = (int)System.Math.Round(
                totalEventWeight * (WorldEventDefinitions.NoEventWeight01 / (1f - WorldEventDefinitions.NoEventWeight01)));
            if (noEventWeight < 0) noEventWeight = 0;
            var denom = totalEventWeight + noEventWeight;

            var seedKey = $"{WorldEventDefinitions.RandomEventSalt}|{worldSeed ?? string.Empty}|{absoluteDay}";
            var hash = StableHash(seedKey);
            var roll = (int)((uint)hash % (uint)denom); // 0..denom-1

            if (roll < noEventWeight)
            {
                return null; // dia comum, sem evento
            }

            var cursor = roll - noEventWeight; // 0..totalEventWeight-1
            foreach (var e in pool)
            {
                if (e == null || e.Weight <= 0) continue;
                if (cursor < e.Weight)
                {
                    return e;
                }

                cursor -= e.Weight;
            }

            return null; // inalcançável (soma confere), mas mantém determinismo
        }

        // ── Resolução completa do dia ────────────────────────────────────────────────────────────
        public static WorldDayResolution ResolveDay(string worldSeed, int absoluteDay)
        {
            var result = new WorldDayResolution { AbsoluteDay = absoluteDay < 1 ? 1 : absoluteDay };

            var festival = ResolveFestival(result.AbsoluteDay);
            if (festival != null)
            {
                result.FestivalId = festival.FestivalId;
            }

            var peak = ResolveLunarPeak(result.AbsoluteDay);
            if (peak != null)
            {
                result.LunarPeakId = peak.PeakId;
                result.LunarEffect = peak.Effect;
                result.LunarMagnitude = peak.Magnitude;
            }

            var worldEvent = ResolveRandomEvent(worldSeed, result.AbsoluteDay);
            if (worldEvent != null)
            {
                result.WorldEventId = worldEvent.WorldEventId;
                result.WorldEffect = worldEvent.Effect;
                result.WorldEventMagnitude = worldEvent.Magnitude;
            }

            return result;
        }

        // ── Mural: próximos festivais CONHECIDOS (spoiler gate F20 / CA-4) ────────────────────────
        /// <summary>
        /// Lista os próximos <paramref name="count"/> festivais a partir de <paramref name="fromAbsoluteDay"/>
        /// (inclusive), em ordem cronológica. Festivais são públicos (conhecidos) por padrão; passe
        /// <paramref name="knownFestivalIds"/> não-nulo para filtrar apenas conhecidos (eventos secretos
        /// nunca entram — eventos aleatórios NÃO são listados no mural). Determinístico e puro.
        /// </summary>
        public static List<WorldEventDefinitions.FestivalDefinition> NextKnownFestivals(
            int fromAbsoluteDay, int count, ISet<string> knownFestivalIds = null)
        {
            var result = new List<WorldEventDefinitions.FestivalDefinition>();
            if (count <= 0) return result;

            var from = fromAbsoluteDay < 1 ? 1 : fromAbsoluteDay;

            // Varre dia a dia até preencher count (festivais são esparsos; limite duro = 2 anos).
            var maxScan = from + GameDate.DaysPerYear * 2;
            for (var day = from; day <= maxScan && result.Count < count; day++)
            {
                var festival = ResolveFestival(day);
                if (festival == null) continue;
                if (knownFestivalIds != null && !knownFestivalIds.Contains(festival.FestivalId)) continue;
                result.Add(festival);
            }

            return result;
        }
    }
}
