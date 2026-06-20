using System;
using System.Collections.Generic;

namespace CindarsHope.World.Fishing
{
    /// <summary>
    /// fable_50 — modelo PURO de uma tabela de pesca (espelho em memória da <see cref="FishingTableSO"/>),
    /// para o resolver rodar 100% fora de cena/asset nos testes EditMode.
    /// </summary>
    public sealed class FishingTableModel
    {
        public string TableId;
        public readonly List<FishingTableEntryModel> Entries = new List<FishingTableEntryModel>();

        public static FishingTableModel FromAsset(FishingTableSO table)
        {
            var model = new FishingTableModel { TableId = table != null ? table.TableId : string.Empty };
            if (table?.Entries == null)
            {
                return model;
            }

            foreach (var entry in table.Entries)
            {
                if (entry == null)
                {
                    continue;
                }

                model.Entries.Add(new FishingTableEntryModel
                {
                    ItemId = entry.ItemId,
                    Weight = entry.Weight,
                    Rarity = entry.Rarity,
                    BaseQuality = entry.BaseQuality,
                    Seasons = entry.Seasons ?? Array.Empty<string>(),
                    Weathers = entry.Weathers ?? Array.Empty<string>(),
                    NightOnly = entry.NightOnly
                });
            }

            return model;
        }
    }

    /// <summary>fable_50 — entrada do modelo puro (campos = <see cref="FishingTableEntry"/>).</summary>
    public sealed class FishingTableEntryModel
    {
        public string ItemId;
        public int Weight = 1;
        public int Rarity;
        public int BaseQuality;
        public string[] Seasons = Array.Empty<string>();
        public string[] Weathers = Array.Empty<string>();
        public bool NightOnly;
    }

    /// <summary>fable_50 — contexto da tentativa de pesca (sem Unity; tudo derivável do serviço F15).</summary>
    public readonly struct FishingContext
    {
        public readonly int Seed;
        public readonly int Day;
        public readonly string SpotId;
        public readonly int CastIndex;
        public readonly string Season;   // token; vazio = qualquer
        public readonly string Weather;  // token; vazio = qualquer
        public readonly int Hour;        // 0..23

        public FishingContext(int seed, int day, string spotId, int castIndex, string season, string weather, int hour)
        {
            Seed = seed;
            Day = day;
            SpotId = spotId ?? string.Empty;
            CastIndex = castIndex;
            Season = season ?? string.Empty;
            Weather = weather ?? string.Empty;
            Hour = hour;
        }
    }

    /// <summary>fable_50 — resultado puro do resolver.</summary>
    public readonly struct FishingCatchOutcome
    {
        public readonly bool Success;
        public readonly string FailureReason;
        public readonly string ItemId;
        public readonly int Rarity;
        public readonly int Quality;
        public readonly FishingTimingGrade Grade;

        private FishingCatchOutcome(bool success, string failureReason, string itemId, int rarity, int quality, FishingTimingGrade grade)
        {
            Success = success;
            FailureReason = failureReason;
            ItemId = itemId;
            Rarity = rarity;
            Quality = quality;
            Grade = grade;
        }

        public static FishingCatchOutcome Caught(string itemId, int rarity, int quality, FishingTimingGrade grade) =>
            new FishingCatchOutcome(true, null, itemId, rarity, quality, grade);

        public static FishingCatchOutcome NoCatch(string reason, FishingTimingGrade grade) =>
            new FishingCatchOutcome(false, reason, null, 0, 0, grade);
    }

    /// <summary>
    /// fable_50 — resolver PURO e DETERMINÍSTICO da captura de pesca. ÚNICA fonte de verdade
    /// (fazenda E caverna chamam este mesmo método). Filtra por estação/clima/hora ANTES do roll
    /// ponderado; o roll usa StableHash(seed|dia|spot|cast) — ZERO Random (UnityEngine.Random ou
    /// System.Random sem seed), conforme skill rng-and-determinism e ADR-0005.
    ///
    /// Grades do minigame: Miss ⇒ NoCatch ("MissedTiming"); Good ⇒ captura base; Perfect ⇒
    /// raridade/qualidade +1 passo (clamp 0..3).
    /// </summary>
    public static class FishingCatchResolver
    {
        public const int MaxRarity = 3;
        public const int MaxQuality = 3;
        // Janela de DIA = [6..18); resto = NOITE. Alinhado ao GameTimeManager.CurrentHourOfDay, cuja
        // fase Night cobre 18:00→06:00 — assim NightOnly dispara exatamente na fase noturna do jogo.
        public const int DayStartHour = 6;
        public const int DayEndHour = 18;

        public static bool IsNight(int hour)
        {
            var h = ((hour % 24) + 24) % 24;
            return h < DayStartHour || h >= DayEndHour;
        }

        /// <summary>Resolve a partir de um <see cref="FishingTableSO"/> (caminho runtime).</summary>
        public static FishingCatchOutcome Resolve(FishingTableSO table, FishingContext context, FishingTimingGrade grade) =>
            Resolve(FishingTableModel.FromAsset(table), context, grade);

        /// <summary>Resolve a partir do modelo puro (caminho de teste e runtime).</summary>
        public static FishingCatchOutcome Resolve(FishingTableModel table, FishingContext context, FishingTimingGrade grade)
        {
            if (grade == FishingTimingGrade.Miss)
            {
                return FishingCatchOutcome.NoCatch("MissedTiming", grade);
            }

            if (table == null || table.Entries.Count == 0)
            {
                return FishingCatchOutcome.NoCatch("NoTable", grade);
            }

            var eligible = FilterEligible(table, context);
            if (eligible.Count == 0)
            {
                // Contexto sem entrada válida (ex.: inverno + clima incompatível) ⇒ NoCatch explícito,
                // nunca exception (mitigação de risco da spec).
                return FishingCatchOutcome.NoCatch("NoEligibleEntry", grade);
            }

            var totalWeight = 0;
            foreach (var entry in eligible)
            {
                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return FishingCatchOutcome.NoCatch("NoEligibleEntry", grade);
            }

            // Roll determinístico: hash estável do contexto → [0..totalWeight).
            var rollSeed = StableHash($"{context.Seed}|{context.Day}|{context.SpotId}|{context.CastIndex}|fishing_catch");
            var roll = (int)(((uint)rollSeed) % (uint)totalWeight);

            FishingTableEntryModel chosen = eligible[eligible.Count - 1];
            var cursor = 0;
            foreach (var entry in eligible)
            {
                cursor += entry.Weight;
                if (roll < cursor)
                {
                    chosen = entry;
                    break;
                }
            }

            var rarity = chosen.Rarity;
            var quality = chosen.BaseQuality;
            if (grade == FishingTimingGrade.Perfect)
            {
                rarity = Math.Min(MaxRarity, rarity + 1);
                quality = Math.Min(MaxQuality, quality + 1);
            }

            return FishingCatchOutcome.Caught(chosen.ItemId, rarity, quality, grade);
        }

        private static List<FishingTableEntryModel> FilterEligible(FishingTableModel table, FishingContext context)
        {
            var night = IsNight(context.Hour);
            var result = new List<FishingTableEntryModel>();
            foreach (var entry in table.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0)
                {
                    continue;
                }

                if (entry.NightOnly && !night)
                {
                    continue;
                }

                if (!SeasonAllowed(entry.Seasons, context.Season))
                {
                    continue;
                }

                if (!WeatherAllowed(entry.Weathers, context.Weather))
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }

        private static bool SeasonAllowed(string[] seasons, string current)
        {
            if (seasons == null || seasons.Length == 0)
            {
                return true; // sem restrição
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return true; // sem informação ⇒ não bloquear (consistente com FarmSeasonGate)
            }

            foreach (var s in seasons)
            {
                if (NormalizeSeasonKey(s) == NormalizeSeasonKey(current))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool WeatherAllowed(string[] weathers, string current)
        {
            if (weathers == null || weathers.Length == 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return true;
            }

            foreach (var w in weathers)
            {
                if (NormalizeWeatherKey(w) == NormalizeWeatherKey(current))
                {
                    return true;
                }
            }

            return false;
        }

        // Espelha FarmSeasonGate.NormalizeSeasonKey (PT + aliases EN) — chave canônica única.
        private static int NormalizeSeasonKey(string season)
        {
            if (string.IsNullOrWhiteSpace(season))
            {
                return -1;
            }

            switch (season.Trim().ToLowerInvariant())
            {
                case "primavera":
                case "spring":
                    return 0;
                case "verao":
                case "verão":
                case "summer":
                    return 1;
                case "outono":
                case "autumn":
                case "fall":
                    return 2;
                case "inverno":
                case "winter":
                    return 3;
                default:
                    return -1;
            }
        }

        private static int NormalizeWeatherKey(string weather)
        {
            if (string.IsNullOrWhiteSpace(weather))
            {
                return -1;
            }

            switch (weather.Trim().ToLowerInvariant())
            {
                case "clear":
                case "limpo":
                case "sunny":
                    return 0;
                case "cloudy":
                case "nublado":
                    return 1;
                case "rainy":
                case "rain":
                case "chuva":
                case "chuvoso":
                    return 2;
                case "stormy":
                case "storm":
                case "tempestade":
                    return 3;
                default:
                    return -1;
            }
        }

        // FNV-1a (mesma família do CaveSnapshotService.StableHash) — estável entre processos,
        // ao contrário de string.GetHashCode().
        private static int StableHash(string value)
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
    }
}
