using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — lista canônica ÚNICA dos itemIds de gear tier alto (espelha CanonicalItemCatalog.
    /// AddHighTierGear — E2.6). Fonte única para: o validador "loja não vende tier alto" e os testes de
    /// banda/durabilidade/custo. NÃO duplica stats (só IDs + banda + BaseValue de referência do catálogo).
    /// "SOMENTE craft/têmpera (não loja)" — ITEM_CATALOG §12.
    /// </summary>
    public static class HighTierGearCatalog
    {
        public readonly struct GearEntry
        {
            public readonly string ItemId;
            public readonly MaterialBand Band;
            public readonly int BaseValue;
            public readonly bool IsArmorOrShield;

            public GearEntry(string itemId, MaterialBand band, int baseValue, bool isArmorOrShield)
            {
                ItemId = itemId;
                Band = band;
                BaseValue = baseValue;
                IsArmorOrShield = isArmorOrShield;
            }
        }

        public static readonly IReadOnlyList<GearEntry> Entries = new[]
        {
            new GearEntry("item_weapon_sword_mithril", MaterialBand.Mithril, 640, false),
            new GearEntry("item_weapon_dagger_mithril", MaterialBand.Mithril, 560, false),
            new GearEntry("item_armor_light_mithril", MaterialBand.Mithril, 700, true),
            new GearEntry("item_weapon_hammer_bromecian", MaterialBand.Bromecian, 760, false),
            new GearEntry("item_weapon_spear_bromecian", MaterialBand.Bromecian, 720, false),
            new GearEntry("item_armor_medium_bromecian", MaterialBand.Bromecian, 820, true),
            new GearEntry("item_shield_bromecian_kite", MaterialBand.Bromecian, 700, true),
            new GearEntry("item_weapon_sword_blackstone", MaterialBand.Blackstone, 1200, false),
            new GearEntry("item_weapon_axe_blackstone", MaterialBand.Blackstone, 1240, false),
            new GearEntry("item_armor_heavy_blackstone", MaterialBand.Blackstone, 1400, true),
            new GearEntry("item_weapon_sword_meteoric", MaterialBand.Meteoric, 1300, false),
            new GearEntry("item_weapon_staff_meteoric", MaterialBand.Meteoric, 1350, false),
            new GearEntry("item_weapon_bow_meteoric", MaterialBand.Meteoric, 1280, false),
            new GearEntry("item_armor_robe_meteoric", MaterialBand.Meteoric, 1320, true),
        };

        /// <summary>Conjunto dos itemIds de tier alto (para o validador loja-sem-tier-alto).</summary>
        public static HashSet<string> ItemIdSet()
        {
            var set = new HashSet<string>();
            foreach (var e in Entries) set.Add(e.ItemId);
            return set;
        }

        /// <summary>Entrada do catálogo por itemId, ou null se não for tier alto.</summary>
        public static bool TryGet(string itemId, out GearEntry entry)
        {
            foreach (var e in Entries)
            {
                if (e.ItemId == itemId)
                {
                    entry = e;
                    return true;
                }
            }
            entry = default;
            return false;
        }
    }
}
