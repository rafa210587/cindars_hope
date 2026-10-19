using System.Collections.Generic;
using CindarsHope.Items;

namespace CindarsHope.NPC.Gifting
{
    /// <summary>
    /// fable_72 — autoria EM CÓDIGO (pura, sem Unity) da matriz de gostos de presente por NPC
    /// (artefato A6: docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md §4) e do mapeamento
    /// de tags de presente / porteiro Giftable por id de item (§3).
    ///
    /// POR QUE EM CÓDIGO (caminho honesto da Fase 0): a definição viva de NPC é autorada como
    /// ScriptableObject (NpcDataSO .asset) que NÃO possui campo GiftPreferences; a definição viva de
    /// item é ItemDataSO .asset que NÃO possui Tags/LoreTags. A struct NpcGiftPreferences e o
    /// ItemDefinition (com Tags/LoreTags) só existem como C# puro. Popular as preferências por NPC e
    /// taguear gift_* exigiria editar YAML de .asset (PROIBIDO). Por isso a matriz vira a fonte de
    /// verdade EM CÓDIGO consumida pelo fluxo de presente; o que exigir materialização em .asset fica
    /// como DÉBITO documentado (ver report). Estende, não duplica: reusa NpcGiftPreferences (F26) e
    /// ItemTag.Giftable como porteiro (ItemDefinition.HasTag / EconomicFlags.CanGift).
    ///
    /// Vocabulário de tags (§3) e deltas (§2) NÃO são redefinidos aqui — os deltas vivem em
    /// GiftTasteClassifier (F26). Aqui só mapeamos quais tags cada item carrega e quais o NPC gosta.
    /// </summary>
    public static class GiftTasteMatrixData
    {
        // ── Vocabulário de tags de presente (§3) — strings canônicas usadas pela matriz ──────────────
        public const string TagFoodHearty = "gift_food_hearty";
        public const string TagFoodFine = "gift_food_fine";
        public const string TagDrink = "gift_drink";
        public const string TagFlowerCrop = "gift_flower_crop";
        public const string TagHerbReagent = "gift_herb_reagent";
        public const string TagOreMetal = "gift_ore_metal";
        public const string TagGemCrystal = "gift_gem_crystal";
        public const string TagBookLore = "gift_book_lore";
        public const string TagCraftTool = "gift_craft_tool";
        public const string TagReligiousKanthor = "gift_religious_kanthor";
        public const string TagReligiousThandra = "gift_religious_thandra";
        public const string TagReligiousSenya = "gift_religious_senya";
        public const string TagReligiousNyx = "gift_religious_nyx";
        public const string TagReligiousFinan = "gift_religious_finan";
        public const string TagReligiousMerithus = "gift_religious_merithus";
        public const string TagReligiousThoren = "gift_religious_thoren";
        public const string TagReligiousKaand = "gift_religious_kaand";
        public const string TagAnimalProduct = "gift_animal_product";
        public const string TagFish = "gift_fish";
        public const string TagJunkLowValue = "gift_junk_lowvalue";
        public const string TagVolatileExplosive = "gift_volatile_explosive";
        public const string TagUndocumentedBlackmarket = "gift_undocumented_blackmarket";

        /// <summary>
        /// Catálogo completo de tags de presente válidas (§3). Usado pelo validador (toda tag citada
        /// na matriz deve existir aqui) e como vocabulário canônico em código.
        /// </summary>
        public static readonly IReadOnlyList<string> AllGiftTags = new List<string>
        {
            TagFoodHearty, TagFoodFine, TagDrink, TagFlowerCrop, TagHerbReagent, TagOreMetal,
            TagGemCrystal, TagBookLore, TagCraftTool, TagReligiousKanthor, TagReligiousThandra,
            TagReligiousSenya, TagReligiousNyx, TagReligiousFinan, TagReligiousMerithus,
            TagReligiousThoren, TagReligiousKaand, TagAnimalProduct, TagFish, TagJunkLowValue,
            TagVolatileExplosive, TagUndocumentedBlackmarket
        };

        // ── Mapeamento item → tags gift_* (§3) ───────────────────────────────────────────────────────
        // Itens REAIS confirmados no catálogo canônico (fable_32) na Fase 0. Todo item aqui é
        // considerado Giftable (porteiro). Itens fora deste mapa caem no fallback neutral (débito A4).
        private static readonly Dictionary<string, string[]> s_itemGiftTags =
            new Dictionary<string, string[]>(System.StringComparer.Ordinal)
        {
            // comida farta
            { "item_consumable_food_pumpkin_soup", new[] { TagFoodHearty } },
            { "item_consumable_food_miners_ration", new[] { TagFoodHearty } },
            { "item_consumable_food_carrot_stew", new[] { TagFoodHearty } },
            // comida fina
            { "item_consumable_food_crystal_jam", new[] { TagFoodFine } },
            { "item_consumable_food_festival_cake", new[] { TagFoodFine } },
            { "item_consumable_food_starroot_pie", new[] { TagFoodFine } },
            // bebida
            { "item_consumable_food_vale_wine", new[] { TagDrink } },
            { "item_consumable_food_grape_juice", new[] { TagDrink } },
            // flores / crops ornamentais
            { "item_crop_alihana_tear", new[] { TagFlowerCrop } },
            { "item_crop_thandra_wheat", new[] { TagFlowerCrop, TagReligiousThandra } },
            { "item_crop_carrot", new[] { TagFlowerCrop } },
            // ervas / reagentes
            { "item_essence_toxic", new[] { TagHerbReagent } },
            { "item_crop_shadowroot", new[] { TagHerbReagent, TagReligiousNyx } },
            { "item_crop_senya_pepper", new[] { TagHerbReagent, TagReligiousSenya } },
            // minério / metal
            { "item_material_iron_ore", new[] { TagOreMetal, TagReligiousThoren } },
            { "item_material_silver_ore", new[] { TagOreMetal, TagReligiousThoren } },
            { "item_material_mithril_ore", new[] { TagOreMetal, TagReligiousThoren } },
            { "item_material_copper_ore", new[] { TagOreMetal, TagReligiousThoren } },
            // cristal / gema
            { "item_material_arcane_crystal", new[] { TagGemCrystal } },
            { "item_essence_arcane", new[] { TagGemCrystal } },
            // quinquilharia barata
            { "item_material_wood", new[] { TagJunkLowValue } },
            { "item_material_stone", new[] { TagJunkLowValue } },
        };

        // ── Matriz de gostos por NPC (§4) ──────────────────────────────────────────────────────────
        // Construída literalmente da matriz A6 §4 (23 NPCs do roster v1.1). loved por id de item ou
        // por tag conforme a matriz; liked/disliked/hated por tag. neutral é o default implícito.
        private static readonly Dictionary<string, NpcGiftPreferences> s_npcPrefs =
            BuildNpcPreferences();

        // ── API pública ──────────────────────────────────────────────────────────────────────────

        /// <summary>Preferências de presente do NPC (matriz §4). null se o NPC não está na matriz.</summary>
        public static NpcGiftPreferences TryGetPreferences(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return null;
            return s_npcPrefs.TryGetValue(npcId, out var p) ? p : null;
        }

        /// <summary>True se o NPC tem preferências mapeadas na matriz.</summary>
        public static bool HasPreferences(string npcId)
        {
            return !string.IsNullOrEmpty(npcId) && s_npcPrefs.ContainsKey(npcId);
        }

        /// <summary>Ids de todos os NPCs mapeados na matriz (cópia somente leitura).</summary>
        public static IReadOnlyCollection<string> MappedNpcIds => s_npcPrefs.Keys;

        /// <summary>Ids de todos os itens com tags gift_* mapeadas (cópia somente leitura).</summary>
        public static IReadOnlyCollection<string> TaggedItemIds => s_itemGiftTags.Keys;

        /// <summary>True se o item id tem tags gift_* mapeadas (logo, é Giftable nesta v1).</summary>
        public static bool IsMappedGiftItem(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && s_itemGiftTags.ContainsKey(itemId);
        }

        /// <summary>Tags gift_* do item (vazio se não mapeado).</summary>
        public static IReadOnlyList<string> GetItemGiftTags(string itemId)
        {
            if (!string.IsNullOrEmpty(itemId) && s_itemGiftTags.TryGetValue(itemId, out var tags))
            {
                return tags;
            }
            return System.Array.Empty<string>();
        }

        /// <summary>
        /// Constrói um ItemDefinition mínimo (id + ItemTag.Giftable + LoreTags gift_*) para um item
        /// mapeado, a ser passado ao classificador da F26. Itens NÃO mapeados retornam null —
        /// o caller decide o fallback (recusa silenciosa por não-Giftable).
        /// </summary>
        public static ItemDefinition BuildGiftItemDefinition(string itemId)
        {
            if (!IsMappedGiftItem(itemId)) return null;
            var tags = s_itemGiftTags[itemId];
            return new ItemDefinition
            {
                ItemId = itemId,
                Tags = ItemTag.Giftable,
                LoreTags = new List<string>(tags)
            };
        }

        // ── Construção da matriz (§4) ──────────────────────────────────────────────────────────────

        private static NpcGiftPreferences Pref(
            string[] lovedIds, string[] liked, string[] disliked, string[] hated)
        {
            return new NpcGiftPreferences
            {
                LovedItemIds = new List<string>(lovedIds ?? System.Array.Empty<string>()),
                LikedItemTags = new List<string>(liked ?? System.Array.Empty<string>()),
                NeutralItemTags = new List<string>(),
                DislikedItemTags = new List<string>(disliked ?? System.Array.Empty<string>()),
                HatedItemTags = new List<string>(hated ?? System.Array.Empty<string>()),
                DailyGiftLimit = 1
            };
        }

        // loved da matriz citado por TAG (não por id) entra como LikedItemTags? Não: a struct só tem
        // LovedItemIds (por id). Tags "loved" da matriz são modeladas como LikedItemTags do gosto forte
        // disponível pela struct atual; ids "loved" reais entram em LovedItemIds. Para manter o "loved
        // por tag" da matriz §4 sem inflar a struct (fora de escopo), tratamos a tag loved como a tag
        // de maior afinidade disponível = liked (+6). Documentado como débito de fidelidade da struct
        // (loved-por-tag não distinto de liked até a struct ganhar LovedItemTags — fora de escopo F72).
        private static Dictionary<string, NpcGiftPreferences> BuildNpcPreferences()
        {
            var d = new Dictionary<string, NpcGiftPreferences>(System.StringComparer.Ordinal)
            {
                // 4.1 Corvus — loved: religious_kanthor(tag), thandra_loaf(item)
                { "npc_corvus", Pref(
                    new[] { "item_consumable_food_thandra_loaf" },
                    new[] { TagReligiousKanthor, TagFoodHearty, TagBookLore, TagReligiousMerithus },
                    new[] { TagDrink, TagReligiousSenya },
                    new[] { TagReligiousNyx, TagUndocumentedBlackmarket }) },
                // 4.2 Mara
                { "npc_mara", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagReligiousMerithus, TagBookLore, TagFoodFine, TagReligiousKanthor },
                    new[] { TagJunkLowValue, TagVolatileExplosive },
                    new[] { TagUndocumentedBlackmarket, TagReligiousNyx }) },
                // 4.3 Sylveth — loved: flower_crop(tag), alihana_tear(item)
                { "npc_sylveth", Pref(
                    new[] { "item_crop_alihana_tear" },
                    new[] { TagFlowerCrop, TagHerbReagent, TagReligiousThandra, TagFoodFine },
                    new[] { TagOreMetal, TagGemCrystal },
                    new[] { TagVolatileExplosive, TagUndocumentedBlackmarket }) },
                // 4.4 Brumdar
                { "npc_brumdar", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagOreMetal, TagCraftTool, TagFoodHearty, TagDrink, TagReligiousThoren },
                    new[] { TagFlowerCrop, TagReligiousSenya },
                    new[] { TagUndocumentedBlackmarket, TagReligiousNyx }) },
                // 4.5 Nimble — loved: craft_tool(tag), wood(item)
                { "npc_nimble", Pref(
                    new[] { "item_material_wood" },
                    new[] { TagCraftTool, TagFoodHearty, TagReligiousFinan },
                    new[] { TagBookLore, TagReligiousMerithus },
                    new[] { TagVolatileExplosive }) },
                // 4.6 Gurd
                { "npc_gurd", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagFoodHearty, TagDrink, TagOreMetal, TagCraftTool, TagReligiousSenya },
                    new[] { TagBookLore, TagFlowerCrop },
                    new[] { TagReligiousKanthor, TagUndocumentedBlackmarket }) },
                // 4.7 Hund
                { "npc_hund", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagCraftTool, TagReligiousKanthor, TagFoodHearty, TagOreMetal, TagReligiousNyx },
                    new[] { TagVolatileExplosive, TagFoodFine },
                    new[] { TagUndocumentedBlackmarket }) },
                // 4.8 Ozzra
                { "npc_ozzra", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagHerbReagent, TagVolatileExplosive, TagGemCrystal, TagReligiousSenya, TagFoodFine },
                    new[] { TagReligiousKanthor, TagReligiousMerithus },
                    new[] { TagJunkLowValue }) },
                // 4.9 Gruta
                { "npc_gruta", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagFoodFine, TagDrink, TagFoodHearty, TagReligiousSenya, TagFish },
                    new[] { TagReligiousMerithus, TagBookLore },
                    new[] { TagVolatileExplosive, TagReligiousNyx }) },
                // 4.10 Zrix
                { "npc_zrix", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagReligiousFinan, TagBookLore, TagFoodHearty, TagOreMetal, TagCraftTool },
                    new[] { TagFlowerCrop, TagFoodFine },
                    new[] { TagUndocumentedBlackmarket }) },
                // 4.11 Yael — loved: blackmarket(tag), religious_nyx(tag), shadowroot(item)
                { "npc_yael", Pref(
                    new[] { "item_crop_shadowroot" },
                    new[] { TagUndocumentedBlackmarket, TagReligiousNyx, TagGemCrystal, TagFish, TagFoodFine },
                    new[] { TagReligiousKanthor, TagJunkLowValue },
                    new[] { TagReligiousSenya }) },
                // 4.12 Thalindra — loved: book_lore(tag), alihana_tear(item)
                { "npc_thalindra", Pref(
                    new[] { "item_crop_alihana_tear" },
                    new[] { TagBookLore, TagGemCrystal, TagHerbReagent, TagReligiousMerithus },
                    new[] { TagFoodHearty, TagDrink },
                    new[] { TagReligiousSenya, TagReligiousKaand }) },
                // 4.13 Dagna
                { "npc_dagna", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagOreMetal, TagGemCrystal, TagCraftTool, TagFoodHearty, TagReligiousThoren },
                    new[] { TagFlowerCrop, TagReligiousSenya },
                    new[] { TagReligiousNyx, TagUndocumentedBlackmarket }) },
                // 4.14 Pip
                { "npc_pip", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagFoodFine, TagReligiousFinan, TagFoodHearty, TagFish, TagJunkLowValue },
                    new[] { TagBookLore, TagReligiousMerithus },
                    new[] { TagVolatileExplosive, TagUndocumentedBlackmarket }) },
                // 4.15 Alaric
                { "npc_alaric", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagCraftTool, TagReligiousKanthor, TagFoodHearty, TagOreMetal, TagReligiousThoren },
                    new[] { TagDrink, TagReligiousFinan },
                    new[] { TagUndocumentedBlackmarket, TagReligiousNyx }) },
                // 4.16 Mirela
                { "npc_mirela", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagAnimalProduct, TagCraftTool, TagFoodFine, TagFlowerCrop, TagReligiousFinan },
                    new[] { TagOreMetal, TagVolatileExplosive },
                    new[] { TagUndocumentedBlackmarket }) },
                // 4.17 Renko
                { "npc_renko", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagReligiousFinan, TagGemCrystal, TagFoodFine, TagUndocumentedBlackmarket, TagOreMetal },
                    new[] { TagReligiousKanthor, TagJunkLowValue },
                    new[] { TagReligiousMerithus }) },
                // 4.18 Eiran
                { "npc_eiran", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagAnimalProduct, TagFlowerCrop, TagHerbReagent, TagReligiousThandra, TagFoodHearty },
                    new[] { TagOreMetal, TagCraftTool },
                    new[] { TagVolatileExplosive, TagUndocumentedBlackmarket }) },
                // 4.19 Liora — loved: book_lore(tag), alihana_tear(item)
                { "npc_liora", Pref(
                    new[] { "item_crop_alihana_tear" },
                    new[] { TagBookLore, TagFlowerCrop, TagFoodFine, TagReligiousThandra },
                    new[] { TagOreMetal, TagJunkLowValue },
                    new[] { TagReligiousNyx, TagVolatileExplosive }) },
                // 4.20 Orlan — loved: drink(item vale_wine), religious_finan(tag)
                { "npc_orlan", Pref(
                    new[] { "item_consumable_food_vale_wine" },
                    new[] { TagDrink, TagReligiousFinan, TagFoodFine, TagBookLore, TagFoodHearty },
                    new[] { TagVolatileExplosive, TagJunkLowValue },
                    new[] { TagReligiousNyx, TagUndocumentedBlackmarket }) },
                // 4.21 Savra — loved: herb_reagent(tag), essence_toxic(item)
                { "npc_savra", Pref(
                    new[] { "item_essence_toxic" },
                    new[] { TagHerbReagent, TagFlowerCrop, TagFoodHearty, TagReligiousThandra },
                    new[] { TagOreMetal, TagReligiousKanthor },
                    new[] { TagReligiousKaand, TagVolatileExplosive }) },
                // 4.22 Tovin
                { "npc_tovin", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagReligiousMerithus, TagCraftTool, TagBookLore, TagGemCrystal, TagReligiousKanthor },
                    new[] { TagDrink, TagReligiousSenya },
                    new[] { TagReligiousNyx, TagUndocumentedBlackmarket }) },
                // 4.23 Maelor — loved: religious_nyx(tag), book_lore(tag), shadowroot(item)
                { "npc_maelor", Pref(
                    new[] { "item_crop_shadowroot" },
                    new[] { TagReligiousNyx, TagBookLore, TagGemCrystal, TagUndocumentedBlackmarket, TagFoodFine },
                    new[] { TagReligiousKanthor, TagReligiousMerithus },
                    new[] { TagReligiousSenya }) },
                // Expansão canônica da vila: Velorin + quatro ofícios autossuficientes.
                { "npc_velorin", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagBookLore, TagReligiousMerithus, TagFoodFine, TagGemCrystal },
                    new[] { TagJunkLowValue, TagDrink },
                    new[] { TagUndocumentedBlackmarket, TagVolatileExplosive }) },
                { "npc_sael", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagFish, TagDrink, TagFoodHearty, TagReligiousFinan },
                    new[] { TagOreMetal, TagBookLore },
                    new[] { TagVolatileExplosive, TagJunkLowValue }) },
                { "npc_mella", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagFoodFine, TagFoodHearty, TagFlowerCrop, TagReligiousThandra },
                    new[] { TagOreMetal, TagVolatileExplosive },
                    new[] { TagUndocumentedBlackmarket }) },
                { "npc_hess", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagAnimalProduct, TagCraftTool, TagOreMetal, TagFoodHearty },
                    new[] { TagFlowerCrop, TagBookLore },
                    new[] { TagVolatileExplosive, TagUndocumentedBlackmarket }) },
                { "npc_tibbet", Pref(
                    System.Array.Empty<string>(),
                    new[] { TagReligiousKanthor, TagBookLore, TagFlowerCrop, TagFoodHearty },
                    new[] { TagDrink, TagJunkLowValue },
                    new[] { TagReligiousNyx, TagUndocumentedBlackmarket }) },
            };
            return d;
        }
    }
}
