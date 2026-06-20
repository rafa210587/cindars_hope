using System.Collections.Generic;
using CindarsHope.World.Calendar;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — tabelas DATA-DRIVEN puras (sem Unity) dos eventos de mundo: 8 festivais sazonais do
    /// QUEST_CATALOG §festivais (2/estação), os 4 picos lunares nomeados do §luas e o pool de eventos
    /// aleatórios diários. NÃO é um segundo calendário/lua — apenas DERIVA de (Season, DayInSeason) e do
    /// worldSeed. Nada disto persiste no save (recomputável — ver WorldEventResolver / time_rules.md).
    ///
    /// Mapeamento de pico ao modelo real (Fase 0): o runtime expõe um ciclo de 28 dias derivado de
    /// AbsoluteDay (LunarCycle, 8 fases genéricas — divergência documentada em time_rules.md Rule 6). Os
    /// 4 picos nomeados são ancorados aos dias canônicos da direção §8.2 (1 pico/lua/estação):
    /// Alihana=D7, Senya=D14, Nyx=D21, sazonal=D28 — addressados por DayInSeason, 100% determinístico.
    /// </summary>
    public static class WorldEventDefinitions
    {
        // ─── Festivais (QUEST_CATALOG §festivais): 2 por estação, dia fixo do calendário de 28 dias ───
        public sealed class FestivalDefinition
        {
            public string FestivalId;       // canônico "festival_<slug>" (casa com FestivalQuestDefinition.FestivalId / flag)
            public Season Season;
            public int DayInSeason;         // 1..28
            public string DisplayName;
            public string SpecialDishId;    // prato da Mirena (F25)
            public string GiftItemId;       // brinde da barraca
        }

        // ─── Picos lunares (QUEST_CATALOG §luas): efeito nomeado por lua ───
        public enum LunarPeakEffect
        {
            None = 0,
            AshUndeadSpawn = 1,     // Lua de Cinza: +30% spawn de undead nessa noite
            GreenCropGrowth = 2,    // Lua Verde: crops avançam +1 estágio
            AmberSellPrices = 3,    // Lua Âmbar: +10% preços de venda no dia
            PaleAguaViva = 4        // Lua Pálida: fonte dá Água Viva extra (F17)
        }

        public sealed class LunarPeakDefinition
        {
            public string PeakId;           // ex.: "peak_green"
            public int DayInSeason;         // dia canônico do pico (7/14/21/28)
            public LunarPeakEffect Effect;
            public float Magnitude;         // 0.30 = +30%, 1 = +1 estágio/unidade, etc.
            public string DisplayName;
        }

        // ─── Eventos aleatórios diários (decisão Q5.2 + EMENDA seca) ───
        public enum WorldEventEffect
        {
            None = 0,
            RareMerchant = 1,       // mercador raro na praça
            StarShowerLuck = 2,     // chuva de estrelas: buff sorte +5% loot raro
            Infestation = 3,        // infestação: +20% spawn banda 1
            TravelerGossip = 4,     // viajante com fofoca (lore)
            CropPriceSurge = 5,     // preço de cultivo em alta: +25% venda de 1 cultivo
            CalmCloudyDay = 6,      // dia nublado calmo: -spawn
            LakeBeachFind = 7,      // achado na praia do lago (item)
            MarketFairDay = 8,      // dia de feira: stock dobrado
            Drought = 9             // EMENDA 2026-06-12-D (5.5): seca — a chuva pode falhar nesse dia
        }

        public sealed class WorldEventDefinition
        {
            public string WorldEventId;     // ex.: "event_star_shower"
            public int Weight;              // peso no sorteio ponderado (>0)
            public WorldEventEffect Effect;
            public float Magnitude;         // payload do efeito (ex.: 0.05 = +5% loot)
            public string DisplayName;      // anúncio (toast)
        }

        /// <summary>Probabilidade (0..1) de NÃO haver evento aleatório num dia comum.</summary>
        public const float NoEventWeight01 = 0.45f;

        // ── Festivais canônicos (8) ──────────────────────────────────────────────────────────────
        public static readonly IReadOnlyList<FestivalDefinition> Festivals = new List<FestivalDefinition>
        {
            new FestivalDefinition { FestivalId = "festival_plantio",  Season = Season.Primavera, DayInSeason = 7,  DisplayName = "Festival do Plantio", SpecialDishId = "item_dish_planting_pie",  GiftItemId = "item_gift_seed_pouch" },
            new FestivalDefinition { FestivalId = "festival_caravana", Season = Season.Primavera, DayInSeason = 21, DisplayName = "Festival da Caravana", SpecialDishId = "item_dish_caravan_stew",  GiftItemId = "item_gift_trinket" },
            new FestivalDefinition { FestivalId = "festival_luas",     Season = Season.Verao,     DayInSeason = 14, DisplayName = "Festival das Luas",     SpecialDishId = "item_dish_moon_cake",     GiftItemId = "item_gift_moon_charm" },
            new FestivalDefinition { FestivalId = "festival_torneio",  Season = Season.Verao,     DayInSeason = 28, DisplayName = "Festival do Torneio",   SpecialDishId = "item_dish_champion_roast",GiftItemId = "item_gift_wooden_medal" },
            new FestivalDefinition { FestivalId = "festival_colheita", Season = Season.Outono,    DayInSeason = 7,  DisplayName = "Festival da Colheita",  SpecialDishId = "item_dish_harvest_feast", GiftItemId = "item_gift_corn_doll" },
            new FestivalDefinition { FestivalId = "festival_veus",     Season = Season.Outono,    DayInSeason = 21, DisplayName = "Festival dos Véus",     SpecialDishId = "item_dish_veil_bread",    GiftItemId = "item_gift_veil_mask" },
            new FestivalDefinition { FestivalId = "festival_vigilia",  Season = Season.Inverno,   DayInSeason = 14, DisplayName = "Festival da Vigília",   SpecialDishId = "item_dish_vigil_soup",    GiftItemId = "item_gift_candle" },
            new FestivalDefinition { FestivalId = "festival_ano_novo", Season = Season.Inverno,   DayInSeason = 28, DisplayName = "Festival do Ano Novo",  SpecialDishId = "item_dish_newyear_cake",  GiftItemId = "item_gift_lucky_coin" },
        };

        // ── Picos lunares (4 nomeados) ───────────────────────────────────────────────────────────
        public static readonly IReadOnlyList<LunarPeakDefinition> LunarPeaks = new List<LunarPeakDefinition>
        {
            new LunarPeakDefinition { PeakId = "peak_ash",   DayInSeason = 7,  Effect = LunarPeakEffect.AshUndeadSpawn,  Magnitude = 0.30f, DisplayName = "Lua de Cinza" },
            new LunarPeakDefinition { PeakId = "peak_green", DayInSeason = 14, Effect = LunarPeakEffect.GreenCropGrowth, Magnitude = 1f,    DisplayName = "Lua Verde" },
            new LunarPeakDefinition { PeakId = "peak_amber", DayInSeason = 21, Effect = LunarPeakEffect.AmberSellPrices, Magnitude = 0.10f, DisplayName = "Lua Âmbar" },
            new LunarPeakDefinition { PeakId = "peak_pale",  DayInSeason = 28, Effect = LunarPeakEffect.PaleAguaViva,    Magnitude = 1f,    DisplayName = "Lua Pálida" },
        };

        // ── Pool de eventos aleatórios diários (8 base + seca da emenda) ──────────────────────────
        public static readonly IReadOnlyList<WorldEventDefinition> RandomEventPool = new List<WorldEventDefinition>
        {
            new WorldEventDefinition { WorldEventId = "event_rare_merchant",  Weight = 10, Effect = WorldEventEffect.RareMerchant,    Magnitude = 0f,    DisplayName = "Um mercador raro chegou à praça." },
            new WorldEventDefinition { WorldEventId = "event_star_shower",    Weight = 8,  Effect = WorldEventEffect.StarShowerLuck,   Magnitude = 0.05f, DisplayName = "Chuva de estrelas: a sorte favorece os achados raros." },
            new WorldEventDefinition { WorldEventId = "event_infestation",    Weight = 8,  Effect = WorldEventEffect.Infestation,      Magnitude = 0.20f, DisplayName = "Infestação: criaturas se proliferam nas profundezas." },
            new WorldEventDefinition { WorldEventId = "event_traveler_gossip",Weight = 12, Effect = WorldEventEffect.TravelerGossip,   Magnitude = 0f,    DisplayName = "Um viajante traz fofocas de terras distantes." },
            new WorldEventDefinition { WorldEventId = "event_crop_surge",     Weight = 10, Effect = WorldEventEffect.CropPriceSurge,    Magnitude = 0.25f, DisplayName = "Procura em alta por um cultivo: venda valorizada hoje." },
            new WorldEventDefinition { WorldEventId = "event_calm_cloudy",    Weight = 10, Effect = WorldEventEffect.CalmCloudyDay,     Magnitude = 0.15f, DisplayName = "Dia nublado e calmo: menos criaturas à espreita." },
            new WorldEventDefinition { WorldEventId = "event_lake_find",      Weight = 8,  Effect = WorldEventEffect.LakeBeachFind,     Magnitude = 0f,    DisplayName = "Algo foi parar na praia do lago." },
            new WorldEventDefinition { WorldEventId = "event_market_fair",    Weight = 8,  Effect = WorldEventEffect.MarketFairDay,     Magnitude = 1f,    DisplayName = "Dia de feira: as lojas dobraram o estoque." },
            new WorldEventDefinition { WorldEventId = "event_drought",        Weight = 6,  Effect = WorldEventEffect.Drought,           Magnitude = 0f,    DisplayName = "Seca: a chuva pode falhar hoje." },
        };

        /// <summary>Salt exclusivo do sorteio diário de eventos de mundo (rng-and-determinism / ADR-0005).</summary>
        public const string RandomEventSalt = "fable_37_world_event_daily_v1";

        public static FestivalDefinition FindFestival(string festivalId)
        {
            if (string.IsNullOrEmpty(festivalId)) return null;
            foreach (var f in Festivals)
            {
                if (f.FestivalId == festivalId) return f;
            }
            return null;
        }

        public static LunarPeakDefinition FindPeak(string peakId)
        {
            if (string.IsNullOrEmpty(peakId)) return null;
            foreach (var p in LunarPeaks)
            {
                if (p.PeakId == peakId) return p;
            }
            return null;
        }

        public static WorldEventDefinition FindRandomEvent(string worldEventId)
        {
            if (string.IsNullOrEmpty(worldEventId)) return null;
            foreach (var e in RandomEventPool)
            {
                if (e.WorldEventId == worldEventId) return e;
            }
            return null;
        }
    }
}
