using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Editor.Items
{
    // fable_32 — Canonical Item Catalog (PURE data table).
    //
    // This file contains ZERO Unity / AssetDatabase dependencies on purpose: the editor
    // generator (GenerateCanonicalItemCatalog) materializes these rows into ItemDataSO assets;
    // this class only DECLARES the canonical catalog as in-memory data, so the rows can be
    // covered by EditMode tests (ids unique, BV>0 where required, Silver/Gold variants, essence
    // count, recipe ingredients resolve) WITHOUT AssetDatabase or Play Mode.
    //
    // Source of truth (read in full, BVs copied verbatim):
    //   docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (PARTES A-I)
    //   + EMENDA 2026-06-13-V3 (E2.1 quality 3-level Gold x2.0; E2.2 potion/oil recipes;
    //     E2.3 cave magic items; E2.5 six essences; E2.6 high-tier gear; E2.8 orphan drops;
    //     E2.9 water/meat/milk; E2.10 fish roster; E2.11 durability/upgrade).
    //   docs/game_rules/economy_rules.md (BaseValue mandatory; quality multipliers).
    //
    // Anti-duplication: this is the LIST + BV surface only. Mechanical weapon/armor stats are
    // owned by EQUIPMENT_MECHANICAL_BASELINES (fable_03); final price/restock by ECONOMY_PRICING.
    public enum CatalogItemSource
    {
        Shop,
        Craft,
        Drop,
        Quest,
        Event,
        Foraging,
        Mining,
        Fishing,
        Farming,
        Starter
    }

    // A single canonical catalog row. Quality variants (_silver / _gold) are NOT stored as
    // separate rows; rows with QualityVariants == true are expanded by the generator/tests.
    public sealed class CatalogItemRow
    {
        public string Id;
        public string DisplayName;
        public ItemCategory Category;
        public ConsumableSubtype ConsumableSubtype = ConsumableSubtype.None;
        public int BaseValue;
        public int MaxStack = 99;
        public int HungerRestore;
        public int StaminaRestore;
        public bool IsEquippable;
        public int DurabilityRestoreAmount;
        public string AmmoType = string.Empty;
        // Crops / animal products emit base + _silver (x1.5) + _gold (x2.0) variants.
        public bool QualityVariants;
        // Sellable items must declare BaseValue > 0 (economy_rules: "Base Value mandatory").
        // Non-sellable (key items, water, Mana-Fruit-as-source, quest, lore) may be 0/special.
        public bool Sellable = true;
        // A dormant item is materialized as data now, but its CONSUMING system arrives in a
        // later spec (oils->F22 temper tags, essences->F22, accessories->F23, magic->F31,
        // relics/keys->quests). Documented, never a parallel system. Reported INFO by F30.
        public bool Dormant;
        public CatalogItemSource Source = CatalogItemSource.Shop;
        public string Notes = string.Empty;
    }

    public sealed class CatalogRecipeRow
    {
        public string Id;
        public string DisplayName;
        public string OutputItemId;
        public int OutputAmount = 1;
        // ingredientId -> amount (all must resolve to a catalog item id).
        public List<KeyValuePair<string, int>> Ingredients = new List<KeyValuePair<string, int>>();
        public string Notes = string.Empty;
    }

    public static class CanonicalItemCatalog
    {
        public const float SilverMultiplier = 1.5f;
        public const float GoldMultiplier = 2.0f; // EMENDA V3.1: Gold is x2.0, NOT x2.2.

        public const string SilverSuffix = "_silver";
        public const string GoldSuffix = "_gold";

        // Consistent rounding for BOTH silver and gold (CA-2): round half away from zero.
        public static int QualityValue(int baseValue, float multiplier)
        {
            return (int)Math.Round(baseValue * (double)multiplier, MidpointRounding.AwayFromZero);
        }

        // ── BASE catalog rows (no quality variants expanded) ─────────────────────────
        public static IReadOnlyList<CatalogItemRow> BaseRows()
        {
            var rows = new List<CatalogItemRow>();

            AddSeedsAndCrops(rows);   // §4  — 12 seeds + 12 crops (crops have quality variants)
            AddFoods(rows);           // §5  — 20 foods
            AddPotions(rows);         // §7  — 8 potions
            AddOilsAndArrows(rows);   // §8  — 4 oils + 6 arrows
            AddMaterials(rows);       // §9  — 10 materials
            AddEssences(rows);        // §10 — 6 essences (EMENDA V3.2)
            AddSpecials(rows);        // §11 — fruto_mana, agua_viva, stabilized_blackstone, corrupted_shard
            AddOrphanDrops(rows);     // E2.8 — ~25 monster-part materials
            AddFish(rows);            // E2.10 — 10 fish
            AddWeapons(rows);         // §12 — 18 shop/craft weapons + tool_shovel
            AddHighTierGear(rows);    // E2.6 — 14 high-tier craft/temper gear
            AddArmorsAndShields(rows);// §13 — 6 armors + 3 shields
            AddMagicWandsScrolls(rows);// §14 — 3 wands + 4 scrolls
            AddCaveMagicItems(rows);  // E2.3 — 8 magic items + unidentified_trinket + scroll_identify
            AddAccessories(rows);     // §16 — 12 accessories
            AddRelics(rows);          // §17 — 4 relics
            AddAnimalProducts(rows);  // §18 — 4 animal products (quality variants)
            AddToolsAndUtilities(rows);// §19 + E2.9 — lantern, bucket, water, repair kits
            AddKeys(rows);            // §20 — keys / documents (non-sellable)

            return rows;
        }

        // ── EXPANDED rows: base + _silver + _gold for QualityVariants rows ────────────
        // This is what the generator materializes and what the count/uniqueness tests use.
        public static IReadOnlyList<CatalogItemRow> ExpandedRows()
        {
            var expanded = new List<CatalogItemRow>();
            foreach (var row in BaseRows())
            {
                expanded.Add(row);
                if (!row.QualityVariants)
                {
                    continue;
                }

                expanded.Add(CloneVariant(row, SilverSuffix, " (Silver)", SilverMultiplier));
                expanded.Add(CloneVariant(row, GoldSuffix, " (Gold)", GoldMultiplier));
            }

            return expanded;
        }

        private static CatalogItemRow CloneVariant(CatalogItemRow basis, string idSuffix, string nameSuffix, float mult)
        {
            return new CatalogItemRow
            {
                Id = basis.Id + idSuffix,
                DisplayName = basis.DisplayName + nameSuffix,
                Category = basis.Category,
                ConsumableSubtype = basis.ConsumableSubtype,
                BaseValue = QualityValue(basis.BaseValue, mult),
                MaxStack = basis.MaxStack,
                HungerRestore = basis.HungerRestore,
                StaminaRestore = basis.StaminaRestore,
                IsEquippable = basis.IsEquippable,
                AmmoType = basis.AmmoType,
                QualityVariants = false,
                Sellable = basis.Sellable,
                Dormant = basis.Dormant,
                Source = basis.Source,
                Notes = "Quality variant of " + basis.Id
            };
        }

        // ────────────────────────────────────────────────────────────────────────────
        // §4 — Seeds (12) and Crops (12). Crops carry quality variants (_silver/_gold).
        private static void AddSeedsAndCrops(List<CatalogItemRow> rows)
        {
            // (seedId, seedName, seedBV) , (cropId, cropName, cropBV)
            var pairs = new (string seedId, string seedName, int seedBv, string cropId, string cropName, int cropBv)[]
            {
                ("item_seed_wheat", "Wheat Seed", 5, "item_crop_wheat", "Wheat", 12),
                ("item_seed_carrot", "Carrot Seed", 6, "item_crop_carrot", "Carrot", 14),
                ("item_seed_moonbean", "Moonbean Seed", 10, "item_crop_moonbean", "Moonbean", 24),
                ("item_seed_sunpepper", "Sunpepper Seed", 12, "item_crop_sunpepper", "Sunpepper", 30),
                ("item_seed_crystal_berry", "Crystal Berry Seed", 18, "item_crop_crystal_berry", "Crystal Berry", 48),
                ("item_seed_starroot", "Starroot Seed", 15, "item_crop_starroot", "Starroot", 38),
                ("item_seed_alihana_tear", "Alihana Tear Seed", 25, "item_crop_alihana_tear", "Alihana Tear", 70),
                ("item_seed_senya_pepper", "Senya Pepper Seed", 22, "item_crop_senya_pepper", "Senya Pepper", 60),
                ("item_seed_shadowroot", "Shadowroot Seed", 20, "item_crop_shadowroot", "Shadowroot", 55),
                ("item_seed_thandra_wheat", "Thandra Wheat Seed", 14, "item_crop_thandra_wheat", "Thandra Wheat", 32),
                ("item_seed_vale_pumpkin", "Vale Pumpkin Seed", 16, "item_crop_vale_pumpkin", "Vale Pumpkin", 44),
                ("item_seed_brigandini_grape", "Brigandini Grape Seed", 20, "item_crop_brigandini_grape", "Brigandini Grape", 52),
            };

            foreach (var p in pairs)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = p.seedId, DisplayName = p.seedName, Category = ItemCategory.Seed,
                    BaseValue = p.seedBv, MaxStack = 99, IsEquippable = true, Source = CatalogItemSource.Shop
                });
                rows.Add(new CatalogItemRow
                {
                    Id = p.cropId, DisplayName = p.cropName, Category = ItemCategory.Crop,
                    BaseValue = p.cropBv, MaxStack = 99, QualityVariants = true, Source = CatalogItemSource.Farming
                });
            }
        }

        // §5 — Foods (20). Hunger/stamina/MP buffs from the table; values authored where the
        // ItemDataSO exposes them (HungerRestore/StaminaRestore); buff details are dormant data.
        private static void AddFoods(List<CatalogItemRow> rows)
        {
            void Food(string id, string name, int bv, int hunger, int stamina, ConsumableSubtype sub = ConsumableSubtype.Food)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Consumable, ConsumableSubtype = sub,
                    BaseValue = bv, MaxStack = 99, HungerRestore = hunger, StaminaRestore = stamina,
                    IsEquippable = true, Source = CatalogItemSource.Craft
                });
            }

            Food("item_consumable_food_bread", "Bread", 20, 30, 0);
            Food("item_consumable_food_carrot_stew", "Carrot Stew", 35, 40, 10);
            Food("item_consumable_food_grilled_fish", "Grilled Fish", 40, 35, 0);
            Food("item_consumable_food_moonbean_soup", "Moonbean Soup", 50, 45, 0);
            Food("item_consumable_food_miners_ration", "Miner's Ration", 50, 50, 10);
            Food("item_consumable_food_spicy_sunpepper", "Spicy Sunpepper", 55, 35, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_pumpkin_soup", "Pumpkin Soup", 60, 55, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_grape_juice", "Grape Juice", 45, 20, 0);
            Food("item_consumable_food_egg_breakfast", "Egg Breakfast", 45, 40, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_goat_cheese", "Goat Cheese", 65, 35, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_starroot_pie", "Starroot Pie", 70, 60, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_shadow_salad", "Shadow Salad", 75, 40, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_crystal_jam", "Crystal Jam", 80, 50, 0);
            Food("item_consumable_food_hearty_omelette", "Hearty Omelette", 85, 60, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_mirrorfin_sashimi", "Mirrorfin Sashimi", 90, 40, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_thandra_loaf", "Thandra Loaf", 95, 70, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_tear_tonic", "Tear Tonic", 110, 0, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_vale_wine", "Vale Wine", 120, 0, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_pepper_feast", "Pepper Feast", 130, 0, 0, ConsumableSubtype.BuffFood);
            Food("item_consumable_food_festival_cake", "Festival Cake", 150, 0, 0, ConsumableSubtype.BuffFood);
        }

        // §7 — Potions (8). Effects are dormant payloads (F08 mapping); values are the % heals.
        private static void AddPotions(List<CatalogItemRow> rows)
        {
            void Potion(string id, string name, int bv, int staminaRestore = 0)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Consumable, ConsumableSubtype = ConsumableSubtype.Potion,
                    BaseValue = bv, MaxStack = 99, StaminaRestore = staminaRestore, IsEquippable = true,
                    Source = CatalogItemSource.Craft
                });
            }

            Potion("item_consumable_potion_hp_small", "Small HP Potion", 40);
            Potion("item_consumable_potion_hp_medium", "Medium HP Potion", 90);
            Potion("item_consumable_potion_mp_small", "Small MP Potion", 45);
            Potion("item_consumable_potion_mp_medium", "Medium MP Potion", 95);
            Potion("item_consumable_potion_antidote", "Antidote", 35);
            Potion("item_consumable_potion_fire_resist", "Fire Resist Potion", 60);
            Potion("item_consumable_potion_ice_resist", "Ice Resist Potion", 60);
            Potion("item_consumable_potion_stamina_draught", "Stamina Draught", 70);
        }

        // §8 — Weapon oils (4, BV 50) + Arrows (6). Oils are dormant until F22 temper tags wire.
        private static void AddOilsAndArrows(List<CatalogItemRow> rows)
        {
            foreach (var (id, name) in new[]
            {
                ("item_consumable_oil_fire", "Fire Oil"),
                ("item_consumable_oil_frost", "Frost Oil"),
                ("item_consumable_oil_shock", "Shock Oil"),
                ("item_consumable_oil_poison", "Poison Oil"),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Consumable, ConsumableSubtype = ConsumableSubtype.None,
                    BaseValue = 50, MaxStack = 99, IsEquippable = true, Dormant = true, Source = CatalogItemSource.Craft,
                    Notes = "Weapon oil: applies canonical status tag to one weapon 3min; consuming system = fable_22 temper."
                });
            }

            foreach (var (id, name, bv) in new[]
            {
                ("item_ammo_arrow_wood", "Wooden Arrow", 2),
                ("item_ammo_arrow_iron", "Iron Arrow", 4),
                ("item_ammo_arrow_steel", "Steel Arrow", 6),
                ("item_ammo_arrow_silver", "Silver Arrow", 12),
                ("item_ammo_arrow_fire", "Fire Arrow", 8),
                ("item_ammo_arrow_frost", "Frost Arrow", 8),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Ammo, BaseValue = bv, MaxStack = 999,
                    AmmoType = "arrow", Source = CatalogItemSource.Shop
                });
            }
        }

        // §9 — Materials (10).
        private static void AddMaterials(List<CatalogItemRow> rows)
        {
            void Mat(string id, string name, int bv, CatalogItemSource src = CatalogItemSource.Mining)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Material, BaseValue = bv, MaxStack = 999, Source = src
                });
            }

            Mat("item_material_wood", "Wood", 3, CatalogItemSource.Foraging);
            Mat("item_material_stone", "Stone", 3, CatalogItemSource.Mining);
            Mat("item_material_copper_ore", "Copper Ore", 8);
            Mat("item_material_iron_ore", "Iron Ore", 15);
            Mat("item_material_leather", "Leather", 15, CatalogItemSource.Drop);
            Mat("item_material_silver_ore", "Silver Ore", 30);
            Mat("item_material_arcane_crystal", "Arcane Crystal", 60);
            Mat("item_material_mithril_ore", "Mithril Ore", 80);
            Mat("item_material_bromecian_alloy", "Bromecian Alloy", 90, CatalogItemSource.Drop);
            Mat("item_material_star_iron", "Star Iron", 120, CatalogItemSource.Drop);
        }

        // §10 / EMENDA V3.2 — Essences (6, one per band/element + void). Dormant until F22.
        private static void AddEssences(List<CatalogItemRow> rows)
        {
            foreach (var (id, name, bv) in new[]
            {
                ("item_essence_fire", "Fire Essence", 60),
                ("item_essence_ice", "Ice Essence", 60),
                ("item_essence_toxic", "Toxic Essence", 45),
                ("item_essence_lightning", "Lightning Essence", 75),
                ("item_essence_arcane", "Arcane Essence", 75),
                ("item_essence_void", "Void Essence", 90),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Essence, BaseValue = bv, MaxStack = 99,
                    Dormant = true, Source = CatalogItemSource.Drop,
                    Notes = "Essence of Temper (fable_22) — permanent version of weapon oils."
                });
            }
        }

        // §11 — Specials. fruto_mana = special-channel-only (BV 5000, NOT ordinary sellable);
        // agua_viva = BV 0 anti-exploit (already exists); stabilized_blackstone canonical (E2.8);
        // corrupted_shard = raw form, distinct.
        private static void AddSpecials(List<CatalogItemRow> rows)
        {
            rows.Add(new CatalogItemRow
            {
                Id = "item_fruto_mana", DisplayName = "Mana Fruit", Category = ItemCategory.Misc,
                BaseValue = 5000, MaxStack = 10, Sellable = true, Dormant = true, Source = CatalogItemSource.Farming,
                Notes = "Special-channel-only sale (Finan caravan); never ordinary shipping. economy_rules Mana Fruit rule."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_consumable_agua_viva", DisplayName = "Agua Viva", Category = ItemCategory.Consumable,
                ConsumableSubtype = ConsumableSubtype.Potion, BaseValue = 0, MaxStack = 3, HungerRestore = 20,
                IsEquippable = true, Sellable = false, Source = CatalogItemSource.Event,
                Notes = "BV 0 anti mass-sale exploit (WAVE 10 canon). Existing asset; updated by id."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_material_stabilized_blackstone", DisplayName = "Stabilized Blackstone", Category = ItemCategory.Material,
                BaseValue = 200, MaxStack = 99, Dormant = true, Source = CatalogItemSource.Craft,
                Notes = "E2.8: canonical EN-only name; legacy item_pedra_negra_estabilizada DEPRECATED -> this id."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_blackstone_corrupted_shard", DisplayName = "Corrupted Blackstone Shard", Category = ItemCategory.Material,
                BaseValue = 180, MaxStack = 99, Dormant = true, Source = CatalogItemSource.Drop,
                Notes = "Raw/dangerous form; stabilizing (Brumdar) produces item_material_stabilized_blackstone."
            });
            // Lagrima da Deusa: consumivel de revive consumido DIRETO pela tela de morte
            // (DeathScreenCanvasController). Nao tem efeito de "use" generico no inventario — a
            // tela remove 1 e restaura o HP cheio in-place. Nao-vendavel (BV 0) para nao virar
            // exploit de venda; stackavel. Concedido 2x no inventario inicial via
            // RepairPlayerStartingItems.EnsureStartingItem (so ACRESCENTA, ver menu Inicializar).
            rows.Add(new CatalogItemRow
            {
                Id = "item_goddess_tear", DisplayName = "Lagrima da Deusa", Category = ItemCategory.Consumable,
                ConsumableSubtype = ConsumableSubtype.Potion, BaseValue = 0, MaxStack = 99,
                Sellable = false, Source = CatalogItemSource.Starter,
                Notes = "Revive item consumed directly by the death screen (no generic 'use'). BV 0 anti mass-sale; 2x granted at new game."
            });
        }

        // E2.8 — Orphan monster-part drops (~25). EN-only item_material_<slug>, BV by band.
        private static void AddOrphanDrops(List<CatalogItemRow> rows)
        {
            foreach (var (id, name, bv) in new[]
            {
                ("item_material_chitin", "Chitin", 10),
                ("item_material_chitin_plate", "Chitin Plate", 22),
                ("item_material_fiber", "Fiber", 6),
                ("item_material_glowcap", "Glowcap", 14),
                ("item_material_spores", "Spores", 12),
                ("item_material_grub_meat", "Grub Meat", 16),
                ("item_material_rot_gland", "Rot Gland", 18),
                ("item_material_sinew", "Sinew", 20),
                ("item_material_white_pelt", "White Pelt", 28),
                ("item_material_frost_core", "Frost Core", 45),
                ("item_material_ember_fang", "Ember Fang", 45),
                ("item_material_magma_chitin", "Magma Chitin", 60),
                ("item_material_shade_ash", "Shade Ash", 50),
                ("item_material_spark_dust", "Spark Dust", 40),
                ("item_material_mycel_thread", "Mycel Thread", 24),
                ("item_material_mycel_heart", "Mycel Heart", 70),
                ("item_material_gears", "Gears", 60),
                ("item_material_turret_core", "Turret Core", 90),
                ("item_material_warden_core", "Warden Core", 120),
                ("item_material_phantom_essence", "Phantom Essence", 110),
                ("item_material_night_essence", "Night Essence", 130),
                ("item_material_void_ichor", "Void Ichor", 160),
                ("item_material_abyssal_fang", "Abyssal Fang", 150),
                ("item_material_wyrmling_scale", "Wyrmling Scale", 140),
                ("item_material_lurker_eye", "Lurker Eye", 150),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.MonsterDrop, BaseValue = bv, MaxStack = 999,
                    Source = CatalogItemSource.Drop, Notes = "E2.8 orphan drop; per-family weight authored in fable_06."
                });
            }
        }

        // E2.10 — Fish roster (10). 4 farm-pond seasonal + 5 cave + 1 Moonless Pool.
        private static void AddFish(List<CatalogItemRow> rows)
        {
            foreach (var (id, name, bv) in new[]
            {
                ("item_fish_river_perch", "River Perch", 14),
                ("item_fish_sun_bass", "Sun Bass", 22),
                ("item_fish_amber_trout", "Amber Trout", 30),
                ("item_fish_frostfin", "Frostfin", 38),
                ("item_fish_pale", "Pale Fish", 24),
                ("item_fish_cave_eel", "Cave Eel", 45),
                ("item_fish_mirrorfin", "Mirrorfin", 55),
                ("item_fish_emberfish", "Emberfish", 70),
                ("item_fish_ruin_lamprey", "Ruin Lamprey", 90),
                ("item_fish_void_angler", "Void Angler", 160),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Fish, BaseValue = bv, MaxStack = 99,
                    Source = CatalogItemSource.Fishing
                });
            }
        }

        // §12 — Weapons (18 shop/craft nominal + tool_shovel). ItemDataSO is the catalog/economy
        // surface (BV + category); mechanical stats live in EquipmentDataSO/WeaponDataSO (fable_03).
        private static void AddWeapons(List<CatalogItemRow> rows)
        {
            void Weapon(string id, string name, int bv, bool dormant = true, CatalogItemSource src = CatalogItemSource.Shop)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Weapon, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = dormant, Source = src,
                    Notes = "Catalog/economy surface; mechanical stats = EQUIPMENT_MECHANICAL_BASELINES (fable_03)."
                });
            }

            Weapon("item_weapon_sword_iron", "Iron Sword", 120);
            Weapon("item_weapon_sword_steel", "Steel Sword", 300);
            Weapon("item_weapon_axe_iron", "Iron Axe", 130);
            Weapon("item_weapon_axe_steel", "Steel Axe", 320);
            Weapon("item_weapon_hammer_iron", "Iron Hammer", 140);
            Weapon("item_weapon_hammer_steel", "Steel Hammer", 340);
            Weapon("item_weapon_spear_iron", "Iron Spear", 125);
            Weapon("item_weapon_spear_steel", "Steel Spear", 310);
            Weapon("item_weapon_dagger_copper", "Copper Dagger", 90);
            Weapon("item_weapon_dagger_steel", "Steel Dagger", 260);
            Weapon("item_weapon_bow_wood", "Wooden Bow", 110);
            Weapon("item_weapon_bow_steel", "Steel Bow", 330);
            Weapon("item_weapon_staff_apprentice", "Apprentice Staff", 150);
            Weapon("item_weapon_staff_oak", "Oak Staff", 360);
            Weapon("item_weapon_tool_shovel", "Shovel", 60, dormant: false);
            Weapon("item_weapon_sword_silver", "Silver Sword", 420, src: CatalogItemSource.Craft);
        }

        // E2.6 — High-tier craftable gear (14). Craft/temper only (CanBuy=false in shops).
        private static void AddHighTierGear(List<CatalogItemRow> rows)
        {
            void Gear(string id, string name, int bv, ItemCategory cat)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = cat, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Source = CatalogItemSource.Craft,
                    Notes = "E2.6 high-tier: craft/temper only (no shop). Stats = EQUIPMENT_MECHANICAL_BASELINES."
                });
            }

            Gear("item_weapon_sword_mithril", "Mithril Sword", 640, ItemCategory.Weapon);
            Gear("item_weapon_dagger_mithril", "Mithril Dagger", 560, ItemCategory.Weapon);
            Gear("item_armor_light_mithril", "Mithril Light Armor", 700, ItemCategory.Armor);
            Gear("item_weapon_hammer_bromecian", "Bromecian Hammer", 760, ItemCategory.Weapon);
            Gear("item_weapon_spear_bromecian", "Bromecian Spear", 720, ItemCategory.Weapon);
            Gear("item_armor_medium_bromecian", "Bromecian Medium Armor", 820, ItemCategory.Armor);
            Gear("item_shield_bromecian_kite", "Bromecian Kite Shield", 700, ItemCategory.Shield);
            Gear("item_weapon_sword_blackstone", "Blackstone Sword", 1200, ItemCategory.Weapon);
            Gear("item_weapon_axe_blackstone", "Blackstone Axe", 1240, ItemCategory.Weapon);
            Gear("item_armor_heavy_blackstone", "Blackstone Heavy Armor", 1400, ItemCategory.Armor);
            Gear("item_weapon_sword_meteoric", "Meteoric Sword", 1300, ItemCategory.Weapon);
            Gear("item_weapon_staff_meteoric", "Meteoric Staff", 1350, ItemCategory.Weapon);
            Gear("item_weapon_bow_meteoric", "Meteoric Bow", 1280, ItemCategory.Weapon);
            Gear("item_armor_robe_meteoric", "Meteoric Robe", 1320, ItemCategory.Armor);
        }

        // §13 — Armors (6) and Shields (3).
        private static void AddArmorsAndShields(List<CatalogItemRow> rows)
        {
            void Armor(string id, string name, int bv)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Armor, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Source = CatalogItemSource.Shop,
                    Notes = "Stats = EQUIPMENT_MECHANICAL_BASELINES (ArmorType)."
                });
            }
            void Shield(string id, string name, int bv)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Shield, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Source = CatalogItemSource.Shop,
                    Notes = "Stats = EQUIPMENT_MECHANICAL_BASELINES (ShieldType)."
                });
            }

            Armor("item_armor_light_leather", "Leather Armor", 100);
            Armor("item_armor_light_studded", "Studded Armor", 240);
            Armor("item_armor_medium_iron", "Iron Armor", 220);
            Armor("item_armor_medium_steel", "Steel Armor", 420);
            Armor("item_armor_heavy_steel", "Heavy Steel Armor", 520);
            Armor("item_armor_robe_arcane", "Arcane Robe", 180);

            Shield("item_shield_buckler", "Buckler", 90);
            Shield("item_shield_round_iron", "Iron Round Shield", 200);
            Shield("item_shield_tower_steel", "Steel Tower Shield", 450);
        }

        // §14 — Magic items: wands (3) + scrolls (4). Wands are weapons; scrolls are consumables.
        private static void AddMagicWandsScrolls(List<CatalogItemRow> rows)
        {
            void Wand(string id, string name, int bv)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Weapon, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Source = CatalogItemSource.Shop,
                    Notes = "Wand (WeaponType.Wand); charges canonical; stats = baselines."
                });
            }
            void Scroll(string id, string name, int bv, bool dormant = true)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Magic, ConsumableSubtype = ConsumableSubtype.None,
                    BaseValue = bv, MaxStack = 99, IsEquippable = true, Dormant = dormant, Source = CatalogItemSource.Shop,
                    Notes = "Cast vs learn scroll: fable_07 magic learning owns the runtime."
                });
            }

            Wand("item_weapon_wand_simple", "Simple Wand", 150);
            Wand("item_weapon_wand_fire", "Fire Wand", 280);
            Wand("item_weapon_wand_frost", "Frost Wand", 280);

            Scroll("item_consumable_scroll_cast_fireburst", "Fireburst Scroll", 60);
            Scroll("item_consumable_scroll_cast_barrier", "Barrier Scroll", 70);
            Scroll("item_consumable_scroll_learn_fire_spark", "Scroll: Learn Fire Spark", 350);
            Scroll("item_consumable_scroll_learn_minor_heal", "Scroll: Learn Minor Heal", 400);
        }

        // E2.3 — Cave magic items (8) + unidentified trinket + identify scroll. Dormant -> fable_31.
        private static void AddCaveMagicItems(List<CatalogItemRow> rows)
        {
            void Magic(string id, string name, int bv)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Magic, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Source = CatalogItemSource.Drop,
                    Notes = "E2.3 cave magic item; identification/effect system = fable_31."
                });
            }

            Magic("item_magic_pendant_of_echoes", "Pendant of Echoes", 400);
            Magic("item_magic_lantern_of_true_sight", "Lantern of True Sight", 450);
            Magic("item_magic_pouch_of_holding", "Pouch of Holding", 600);
            Magic("item_magic_candle_of_the_depths", "Candle of the Depths", 350);
            Magic("item_magic_whetstone_eternal", "Eternal Whetstone", 500);
            Magic("item_magic_bell_of_warding", "Bell of Warding", 220);
            Magic("item_magic_mirror_of_return", "Mirror of Return", 260);
            Magic("item_magic_hourglass_of_dawn", "Hourglass of Dawn", 300);

            rows.Add(new CatalogItemRow
            {
                Id = "item_unidentified_trinket", DisplayName = "???", Category = ItemCategory.Magic,
                BaseValue = 120, MaxStack = 99, Dormant = true, Source = CatalogItemSource.Drop,
                Notes = "E2.3 unidentified trinket (tooltip ???). Identify = 1:1 swap; anti-arbitrage: cannot sell at real BV."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_consumable_scroll_identify", DisplayName = "Scroll of Identify", Category = ItemCategory.Magic,
                BaseValue = 140, MaxStack = 99, IsEquippable = true, Dormant = true, Source = CatalogItemSource.Shop,
                Notes = "E2.3 reveals 1 trinket; alternative to Veska IdentifyItem service (fable_25)."
            });
        }

        // §16 — Accessories (12). Slots Ring/Amulet/Charm. Dormant -> fable_23. BV "(—)" rows are
        // treasure/quest-only and non-priced; given a representative BV so they are not orphan
        // (economy_rules: sellable needs BV>0). The "(—)" items are flagged non-sellable.
        private static void AddAccessories(List<CatalogItemRow> rows)
        {
            void Acc(string id, string name, int bv, bool sellable = true)
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Accessory, BaseValue = bv, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Sellable = sellable, Source = CatalogItemSource.Shop,
                    Notes = "Accessory (Ring/Amulet/Charm); equip system = fable_23. No direct damage."
                });
            }

            Acc("item_acc_ring_thoren", "Ring of Thoren", 400);
            Acc("item_acc_ring_finan", "Ring of Finan", 600);
            Acc("item_acc_ring_alihana", "Ring of Alihana", 500, sellable: false);      // BV (—) treasure; non-sellable
            Acc("item_acc_ring_swiftcurrent", "Ring of Swiftcurrent", 500);
            Acc("item_acc_ring_rootguard", "Ring of Rootguard", 450);
            Acc("item_acc_ring_emberward", "Ring of Emberward", 450, sellable: false);  // BV (—) treasure; non-sellable
            Acc("item_acc_amulet_anya", "Amulet of Anya", 600, sellable: false);        // BV (—) quest Ato 1; non-sellable
            Acc("item_acc_amulet_kanthor", "Amulet of Kanthor", 700);
            Acc("item_acc_amulet_senya", "Amulet of Senya", 650);
            Acc("item_acc_amulet_nyx", "Amulet of Nyx", 600, sellable: false);          // BV (—) wandering merchant; non-sellable
            Acc("item_acc_charm_thandra", "Charm of Thandra", 600);
            Acc("item_acc_charm_stoneheart", "Charm of Stoneheart", 350);
        }

        // §17 — Divine relics (4). Occupy amulet/charm slot; only 1 relic equipped. Dormant.
        private static void AddRelics(List<CatalogItemRow> rows)
        {
            foreach (var (id, name) in new[]
            {
                ("item_relic_kanthor", "Relic of Kanthor"),
                ("item_relic_kaand", "Relic of Kaand"),
                ("item_relic_anya", "Relic of Anya"),
                ("item_relic_alihana", "Relic of Alihana"),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Relic, BaseValue = 1500, MaxStack = 1,
                    IsEquippable = true, Dormant = true, Sellable = false, Source = CatalogItemSource.Drop,
                    Notes = "Divine relic (rare, boss/quest drop); non-sellable lore item; equip system = fable_23."
                });
            }
        }

        // §18 — Animal products (4) with quality variants (_silver/_gold). Produced via fable_12.
        private static void AddAnimalProducts(List<CatalogItemRow> rows)
        {
            foreach (var (id, name, bv) in new[]
            {
                ("item_animal_egg", "Egg", 12),
                ("item_animal_goat_milk", "Goat Milk", 28),
                ("item_animal_cow_milk", "Cow Milk", 35),
                ("item_animal_wool", "Wool", 40),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.AnimalProduct, BaseValue = bv, MaxStack = 99,
                    QualityVariants = true, Source = CatalogItemSource.Farming, Notes = "Production via fable_12."
                });
            }
        }

        // §19 + E2.9 — Tools and utilities. lantern, bucket (E2.9), water (E2.9), repair kits.
        // Core tools (hoe/watering/pickaxe/axe/fishing_rod) already exist; not redefined here to
        // avoid stomping their existing data — only the catalog additions land here.
        private static void AddToolsAndUtilities(List<CatalogItemRow> rows)
        {
            rows.Add(new CatalogItemRow
            {
                Id = "item_tool_lantern", DisplayName = "Lantern", Category = ItemCategory.Tool, BaseValue = 120,
                MaxStack = 1, IsEquippable = true, Source = CatalogItemSource.Shop,
                Notes = "Light range; upgrade angler_lamp (Deep Angler drop)."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_tool_bucket", DisplayName = "Bucket", Category = ItemCategory.Tool, BaseValue = 60,
                MaxStack = 1, IsEquippable = true, Source = CatalogItemSource.Shop,
                Notes = "E2.9: collects water at farm well / lake. No bucket, no water."
            });
            rows.Add(new CatalogItemRow
            {
                Id = "item_material_water", DisplayName = "Water", Category = ItemCategory.Material, BaseValue = 1,
                MaxStack = 999, Sellable = false, Source = CatalogItemSource.Foraging,
                Notes = "E2.9: collectible material (BV 1, not profitably sellable). Required by water recipes."
            });

            foreach (var (id, name, restore, bv) in new[]
            {
                ("item_consumable_repair_kit_basic", "Basic Repair Kit", 50, 25),
                ("item_consumable_repair_kit_standard", "Standard Repair Kit", 100, 50),
                ("item_consumable_repair_kit_superior", "Superior Repair Kit", 200, 100),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.Consumable, ConsumableSubtype = ConsumableSubtype.RepairKit,
                    BaseValue = bv, MaxStack = 99, DurabilityRestoreAmount = restore, Source = CatalogItemSource.Shop,
                    Notes = "Existing repair kit; updated by id."
                });
            }
        }

        // §20 — Keys and documents (category Key). Non-sellable (CanSell=false; BV 0).
        private static void AddKeys(List<CatalogItemRow> rows)
        {
            foreach (var (id, name) in new[]
            {
                ("item_key_market_license", "Market License"),
                ("item_key_contract_farm_registry", "Farm Registry Contract"),
                ("item_key_lot_deed_north", "North Lot Deed"),
                ("item_key_lot_deed_east", "East Lot Deed"),
                // fable_41: lote Oeste = pomar (HUD_LAYOUT_SCENES §4). Id reconciliado de _south → _west
                // para casar com FarmLotId.West / FarmLotCatalog (3 lotes: norte/leste/oeste).
                ("item_key_lot_deed_west", "West Lot Deed"),
                ("item_key_warchief_crest", "Warchief Crest"),
                ("item_key_orc_warbanner", "Orc Warbanner"),
                ("item_key_nymirian_engraving", "Nymirian Engraving"),
            })
            {
                rows.Add(new CatalogItemRow
                {
                    Id = id, DisplayName = name, Category = ItemCategory.KeyItem, BaseValue = 0, MaxStack = 1,
                    Sellable = false, Source = CatalogItemSource.Quest, Notes = "Key/document; non-sellable."
                });
            }
        }

        // ── Recipes (§5 foods / §7-8 potions+oils via EMENDA E2.2/E2.9) ──────────────
        // Data-driven recipe rows. Every ingredient id resolves to a catalog item id (CA-4).
        // Materialized as RecipeDataSO by the generator; validated by EditMode tests.
        public static IReadOnlyList<CatalogRecipeRow> RecipeRows()
        {
            var recipes = new List<CatalogRecipeRow>();

            CatalogRecipeRow R(string id, string name, string outId, params (string ing, int amt)[] ingredients)
            {
                var row = new CatalogRecipeRow { Id = id, DisplayName = name, OutputItemId = outId, OutputAmount = 1 };
                foreach (var (ing, amt) in ingredients)
                {
                    row.Ingredients.Add(new KeyValuePair<string, int>(ing, amt));
                }
                recipes.Add(row);
                return row;
            }

            // Foods (§5) — ingredients from crops/fish/animal products/orphan drops/water.
            R("recipe_bread", "Bread", "item_consumable_food_bread", ("item_crop_wheat", 2));
            R("recipe_carrot_stew", "Carrot Stew", "item_consumable_food_carrot_stew", ("item_crop_carrot", 2), ("item_material_water", 1));
            R("recipe_grilled_fish", "Grilled Fish", "item_consumable_food_grilled_fish", ("item_fish_river_perch", 1));
            R("recipe_moonbean_soup", "Moonbean Soup", "item_consumable_food_moonbean_soup", ("item_crop_moonbean", 2));
            R("recipe_miners_ration", "Miner's Ration", "item_consumable_food_miners_ration", ("item_consumable_food_bread", 1), ("item_material_grub_meat", 1));
            R("recipe_spicy_sunpepper", "Spicy Sunpepper", "item_consumable_food_spicy_sunpepper", ("item_crop_sunpepper", 2));
            R("recipe_pumpkin_soup", "Pumpkin Soup", "item_consumable_food_pumpkin_soup", ("item_crop_vale_pumpkin", 1), ("item_material_water", 1));
            R("recipe_grape_juice", "Grape Juice", "item_consumable_food_grape_juice", ("item_crop_brigandini_grape", 3));
            R("recipe_egg_breakfast", "Egg Breakfast", "item_consumable_food_egg_breakfast", ("item_animal_egg", 2));
            R("recipe_goat_cheese", "Goat Cheese", "item_consumable_food_goat_cheese", ("item_animal_goat_milk", 2));
            R("recipe_starroot_pie", "Starroot Pie", "item_consumable_food_starroot_pie", ("item_crop_starroot", 2), ("item_crop_wheat", 1));
            R("recipe_shadow_salad", "Shadow Salad", "item_consumable_food_shadow_salad", ("item_crop_shadowroot", 1), ("item_crop_carrot", 1));
            R("recipe_crystal_jam", "Crystal Jam", "item_consumable_food_crystal_jam", ("item_crop_crystal_berry", 2));
            R("recipe_hearty_omelette", "Hearty Omelette", "item_consumable_food_hearty_omelette", ("item_animal_egg", 2), ("item_consumable_food_goat_cheese", 1));
            R("recipe_mirrorfin_sashimi", "Mirrorfin Sashimi", "item_consumable_food_mirrorfin_sashimi", ("item_fish_mirrorfin", 1));
            R("recipe_thandra_loaf", "Thandra Loaf", "item_consumable_food_thandra_loaf", ("item_crop_thandra_wheat", 3));
            R("recipe_tear_tonic", "Tear Tonic", "item_consumable_food_tear_tonic", ("item_crop_alihana_tear", 1), ("item_material_water", 1));
            R("recipe_vale_wine", "Vale Wine", "item_consumable_food_vale_wine", ("item_crop_brigandini_grape", 5));
            R("recipe_pepper_feast", "Pepper Feast", "item_consumable_food_pepper_feast", ("item_crop_senya_pepper", 2), ("item_material_grub_meat", 1));
            R("recipe_festival_cake", "Festival Cake", "item_consumable_food_festival_cake", ("item_crop_wheat", 1), ("item_animal_egg", 1), ("item_animal_cow_milk", 1), ("item_crop_crystal_berry", 1));

            // Potions (§7 / E2.2) — canonical ingredients copied verbatim from the EMENDA E2.2 table.
            R("recipe_potion_hp_small", "Small HP Potion", "item_consumable_potion_hp_small", ("item_material_glowcap", 2), ("item_material_fiber", 1));
            R("recipe_potion_hp_medium", "Medium HP Potion", "item_consumable_potion_hp_medium", ("item_material_glowcap", 3), ("item_material_sinew", 1), ("item_crop_crystal_berry", 1));
            R("recipe_potion_mp_small", "Small MP Potion", "item_consumable_potion_mp_small", ("item_material_spores", 2), ("item_material_mycel_thread", 1));
            R("recipe_potion_mp_medium", "Medium MP Potion", "item_consumable_potion_mp_medium", ("item_material_spores", 3), ("item_material_mycel_heart", 1));
            R("recipe_potion_antidote", "Antidote", "item_consumable_potion_antidote", ("item_material_rot_gland", 1), ("item_material_glowcap", 1));
            R("recipe_potion_fire_resist", "Fire Resist Potion", "item_consumable_potion_fire_resist", ("item_material_frost_core", 1), ("item_material_glowcap", 1));
            R("recipe_potion_ice_resist", "Ice Resist Potion", "item_consumable_potion_ice_resist", ("item_material_ember_fang", 1), ("item_material_glowcap", 1));
            R("recipe_potion_stamina_draught", "Stamina Draught", "item_consumable_potion_stamina_draught", ("item_crop_sunpepper", 1), ("item_crop_moonbean", 1), ("item_material_fiber", 1));

            // Oils (§8 / E2.2) — processed alchemy, ingredients verbatim from E2.2.
            R("recipe_oil_fire", "Fire Oil", "item_consumable_oil_fire", ("item_material_ember_fang", 1), ("item_material_fiber", 1));
            R("recipe_oil_frost", "Frost Oil", "item_consumable_oil_frost", ("item_material_frost_core", 1), ("item_material_fiber", 1));
            R("recipe_oil_shock", "Shock Oil", "item_consumable_oil_shock", ("item_material_spark_dust", 2), ("item_material_fiber", 1));
            R("recipe_oil_poison", "Poison Oil", "item_consumable_oil_poison", ("item_material_rot_gland", 1), ("item_material_spores", 2));

            return recipes;
        }

        // Convenience: every distinct expanded item id (for cross-validation in tests/generator).
        public static HashSet<string> AllItemIds()
        {
            return new HashSet<string>(ExpandedRows().Select(r => r.Id));
        }
    }
}
