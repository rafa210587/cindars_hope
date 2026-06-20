using System.Collections.Generic;

namespace CindarsHope.World.Fishing
{
    /// <summary>
    /// fable_50 — fonte ÚNICA das 5 tabelas v1 de pesca (conteúdo dos peixes do ITEM_CATALOG §11 +
    /// peixe comum). Mesmo catálogo consumido pelo gerador de assets (Editor) E pelos testes EditMode,
    /// para não haver duas verdades. IDs estáveis batem com <see cref="CaveFishingTableSelector"/>.
    ///
    /// Decisões de conteúdo (CAVE_BESTIARY / ITEM_CATALOG / EMENDA 2026-06-12-D):
    /// - farm_pond: peixes de superfície por estação (river_perch/sun_bass/amber_trout) + frostfin no
    ///   inverno; EMENDA-D 5.6-A: Mirrorfin (raro) e fish_pale (Lake Lurker) como evento 0,5%, SÓ à noite.
    /// - cave_lake_band_1_10: fish_pale (drop do Lake Lurker 5-9) + cave_eel comum.
    /// - cave_lake_band_26_40: mirrorfin (raro) + frostfin/cave_eel da banda de gelo.
    /// - moonless_pool: tabela rara/noturna (void_angler/ruin_lamprey) — consumida quando F09
    ///   materializar a sala (flag de contexto isMoonlessPool).
    ///
    /// Pesos do evento raro do açude: 1 em ~200 (0,5%) — as entradas comuns somam 199.
    /// </summary>
    public static class CanonicalFishingTables
    {
        public const string ItemFishCommon = "item_fish_common";
        public const string ItemRiverPerch = "item_fish_river_perch";
        public const string ItemSunBass = "item_fish_sun_bass";
        public const string ItemAmberTrout = "item_fish_amber_trout";
        public const string ItemFrostfin = "item_fish_frostfin";
        public const string ItemPale = "item_fish_pale";
        public const string ItemCaveEel = "item_fish_cave_eel";
        public const string ItemMirrorfin = "item_fish_mirrorfin";
        public const string ItemRuinLamprey = "item_fish_ruin_lamprey";
        public const string ItemVoidAngler = "item_fish_void_angler";

        // Tokens de estação (PT, casam com o enum Season).
        public const string Primavera = "Primavera";
        public const string Verao = "Verao";
        public const string Outono = "Outono";
        public const string Inverno = "Inverno";

        /// <summary>Retorna as 5 tabelas v1 como modelos puros (id → modelo).</summary>
        public static Dictionary<string, FishingTableModel> BuildAll()
        {
            var all = new Dictionary<string, FishingTableModel>
            {
                [CaveFishingTableSelector.TableFarmPond] = FarmPond(),
                [CaveFishingTableSelector.TableFarmPondWinter] = FarmPondWinter(),
                [CaveFishingTableSelector.TableCaveBand1To10] = CaveBand1To10(),
                [CaveFishingTableSelector.TableCaveBand26To40] = CaveBand26To40(),
                [CaveFishingTableSelector.TableMoonlessPool] = MoonlessPool()
            };
            return all;
        }

        public static FishingTableModel FarmPond()
        {
            var t = new FishingTableModel { TableId = CaveFishingTableSelector.TableFarmPond };
            // Comuns por estação (river_perch o ano todo; sun_bass no verão; amber_trout no outono).
            t.Entries.Add(Entry(ItemFishCommon, 120, rarity: 0, quality: 0));
            t.Entries.Add(Entry(ItemRiverPerch, 50, rarity: 0, quality: 0));
            t.Entries.Add(Entry(ItemSunBass, 18, rarity: 1, quality: 1, seasons: new[] { Verao }));
            t.Entries.Add(Entry(ItemAmberTrout, 11, rarity: 1, quality: 1, seasons: new[] { Outono }));
            // EMENDA-D 5.6-A: evento raríssimo (0,5%), SÓ à noite — Mirrorfin + fish_pale (Lake Lurker).
            t.Entries.Add(Entry(ItemMirrorfin, 1, rarity: 2, quality: 2, nightOnly: true));
            t.Entries.Add(Entry(ItemPale, 1, rarity: 2, quality: 1, nightOnly: true));
            return t;
        }

        public static FishingTableModel FarmPondWinter()
        {
            var t = new FishingTableModel { TableId = CaveFishingTableSelector.TableFarmPondWinter };
            // Tabela própria de inverno (direction sazonal): frostfin entra; perch some.
            t.Entries.Add(Entry(ItemFishCommon, 120, rarity: 0, quality: 0, seasons: new[] { Inverno }));
            t.Entries.Add(Entry(ItemFrostfin, 40, rarity: 1, quality: 1, seasons: new[] { Inverno }));
            // Evento raro noturno preservado também no inverno.
            t.Entries.Add(Entry(ItemMirrorfin, 1, rarity: 2, quality: 2, seasons: new[] { Inverno }, nightOnly: true));
            t.Entries.Add(Entry(ItemPale, 1, rarity: 2, quality: 1, seasons: new[] { Inverno }, nightOnly: true));
            return t;
        }

        public static FishingTableModel CaveBand1To10()
        {
            var t = new FishingTableModel { TableId = CaveFishingTableSelector.TableCaveBand1To10 };
            t.Entries.Add(Entry(ItemFishCommon, 90, rarity: 0, quality: 0));
            t.Entries.Add(Entry(ItemPale, 40, rarity: 1, quality: 1));   // drop temático do Lake Lurker 5-9
            t.Entries.Add(Entry(ItemCaveEel, 22, rarity: 1, quality: 1));
            return t;
        }

        public static FishingTableModel CaveBand26To40()
        {
            var t = new FishingTableModel { TableId = CaveFishingTableSelector.TableCaveBand26To40 };
            t.Entries.Add(Entry(ItemCaveEel, 60, rarity: 1, quality: 1));
            t.Entries.Add(Entry(ItemFrostfin, 35, rarity: 1, quality: 1));
            t.Entries.Add(Entry(ItemMirrorfin, 12, rarity: 2, quality: 2)); // mirrorfin 27-34 (peixe raro)
            return t;
        }

        public static FishingTableModel MoonlessPool()
        {
            var t = new FishingTableModel { TableId = CaveFishingTableSelector.TableMoonlessPool };
            // Rara/noturna: água escura de Nyx. Peixes profundos raros.
            t.Entries.Add(Entry(ItemCaveEel, 30, rarity: 1, quality: 1, nightOnly: true));
            t.Entries.Add(Entry(ItemRuinLamprey, 14, rarity: 2, quality: 2, nightOnly: true));
            t.Entries.Add(Entry(ItemVoidAngler, 5, rarity: 3, quality: 3, nightOnly: true));
            return t;
        }

        private static FishingTableEntryModel Entry(string itemId, int weight, int rarity, int quality,
            string[] seasons = null, string[] weathers = null, bool nightOnly = false)
        {
            return new FishingTableEntryModel
            {
                ItemId = itemId,
                Weight = weight,
                Rarity = rarity,
                BaseQuality = quality,
                Seasons = seasons ?? System.Array.Empty<string>(),
                Weathers = weathers ?? System.Array.Empty<string>(),
                NightOnly = nightOnly
            };
        }
    }
}
