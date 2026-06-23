using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 5 — criterio 14.7): the wandering merchant stock is themed per biome
    /// band and wider in variety, while remaining 100% deterministic per seed.
    ///
    /// Covers: stock determinism (same world/run/level -> same stock); selected ids are valid
    /// canonical item ids; no duplicate offer on the same level; different bands yield pools
    /// that differ. Deterministic (FNV-1a via CaveEnemySpawnPlanner.StableHash) — no
    /// UnityEngine.Random, no time.
    ///
    /// The canonical id allowlist below mirrors CanonicalItemCatalog (an Editor-assembly type
    /// that this EditMode test folder cannot reference). It is kept narrow on purpose: only the
    /// ids the merchant is allowed to stock. If the merchant adds an id not present here, the
    /// validity test fails — forcing the author to confirm the id is canonical first.
    /// </summary>
    [TestFixture]
    public class CaveWanderingMerchantStockTests
    {
        private const string WorldSeed = "world_seed_fable78_stock";
        private const string RunSeed = "run_seed_fable78_stock";

        // Representative level for each canonical band (stone/fungal/ice/fire/ruins/deep/void).
        private static readonly (string band, int level)[] BandRepresentativeLevels =
        {
            ("stone", 1),
            ("fungal", 11),
            ("ice", 26),
            ("fire", 41),
            ("ruins", 56),
            ("deep", 71),
            ("void", 86)
        };

        // Canonical item ids the merchant is allowed to sell (subset of CanonicalItemCatalog).
        private static readonly HashSet<string> CanonicalMerchantItemIds = new HashSet<string>
        {
            // baseline goods
            "item_consumable_potion_hp_small",
            "item_consumable_food_bread",
            "item_consumable_repair_kit_basic",
            "item_consumable_food_miners_ration",
            "item_material_wood",
            "item_material_stone",
            // stone
            "item_material_copper_ore",
            "item_material_iron_ore",
            // fungal
            "item_material_glowcap",
            "item_material_spores",
            "item_consumable_potion_mp_small",
            "item_essence_toxic",
            // ice
            "item_material_frost_core",
            "item_consumable_potion_ice_resist",
            "item_essence_ice",
            // fire
            "item_material_ember_fang",
            "item_consumable_potion_fire_resist",
            "item_essence_fire",
            // ruins
            "item_material_silver_ore",
            "item_consumable_repair_kit_standard",
            "item_consumable_potion_hp_medium",
            // deep
            "item_material_mithril_ore",
            "item_material_arcane_crystal",
            "item_essence_arcane",
            "item_consumable_repair_kit_superior",
            // void
            "item_essence_void",
            "item_material_star_iron",
            "item_consumable_potion_mp_medium"
        };

        [Test]
        public void ResolveBandId_MatchesCanonicalBandForRepresentativeLevels()
        {
            foreach (var (band, level) in BandRepresentativeLevels)
            {
                Assert.AreEqual(band, CaveWanderingMerchant.ResolveBandId(level),
                    $"Nivel {level} deveria mapear para a banda {band}.");
            }
        }

        [Test]
        public void ResolveBiomeStock_IsDeterministic_ForSameSeedAndLevel()
        {
            for (int level = 1; level <= 101; level += 3)
            {
                var first = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);
                var second = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);

                CollectionAssert.AreEqual(
                    first.Select(o => o.ItemId).ToList(),
                    second.Select(o => o.ItemId).ToList(),
                    $"Estoque mudou entre chamadas no nivel {level}.");
            }
        }

        [Test]
        public void ResolveBiomeStock_SelectedIds_AreCanonical()
        {
            for (int level = 1; level <= 101; level++)
            {
                var stock = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);
                foreach (var offer in stock)
                {
                    Assert.IsTrue(CanonicalMerchantItemIds.Contains(offer.ItemId),
                        $"Nivel {level}: id de item '{offer.ItemId}' nao esta na lista canonica permitida.");
                }
            }
        }

        [Test]
        public void ResolveBiomeStock_HasNoDuplicateOffer_OnSameLevel()
        {
            for (int level = 1; level <= 101; level++)
            {
                var stock = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);
                var ids = stock.Select(o => o.ItemId).ToList();
                CollectionAssert.AllItemsAreUnique(ids,
                    $"Nivel {level}: oferta duplicada no mesmo estoque ({string.Join(", ", ids)}).");
            }
        }

        [Test]
        public void ResolveBiomeStock_StocksConfiguredNumberOfOffers()
        {
            for (int level = 1; level <= 101; level++)
            {
                var stock = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);
                Assert.AreEqual(CaveWanderingMerchant.OffersPerLevel, stock.Count,
                    $"Nivel {level}: esperadas {CaveWanderingMerchant.OffersPerLevel} ofertas, obtidas {stock.Count}.");
            }
        }

        [Test]
        public void ResolveLevelOfferPool_DiffersByBand_ThemedOffersAreBandSpecific()
        {
            // Each band's pool must contain at least one themed offer that no other band's pool
            // contains — proving the pools are biome-specific (not just the shared baseline).
            var poolsByBand = BandRepresentativeLevels.ToDictionary(
                t => t.band,
                t => CaveWanderingMerchant.ResolveLevelOfferPool(t.level)
                        .Select(o => o.ItemId).ToHashSet());

            foreach (var (band, ids) in poolsByBand)
            {
                var otherBandIds = poolsByBand
                    .Where(kvp => kvp.Key != band)
                    .SelectMany(kvp => kvp.Value)
                    .ToHashSet();

                var uniqueToBand = ids.Where(id => !otherBandIds.Contains(id)).ToList();
                Assert.IsNotEmpty(uniqueToBand,
                    $"Banda '{band}' nao tem nenhuma oferta exclusiva — pool nao e tematico por bioma.");
            }
        }

        [Test]
        public void ResolveLevelOfferPool_AlwaysIncludesBaselineGoods()
        {
            // Every band keeps the band-agnostic baseline goods available.
            foreach (var (band, level) in BandRepresentativeLevels)
            {
                var poolIds = CaveWanderingMerchant.ResolveLevelOfferPool(level)
                    .Select(o => o.ItemId).ToHashSet();
                foreach (var baseline in CaveWanderingMerchant.OfferCatalog)
                {
                    Assert.IsTrue(poolIds.Contains(baseline.ItemId),
                        $"Banda '{band}': pool nao inclui o item baseline '{baseline.ItemId}'.");
                }
            }
        }

        [Test]
        public void ResolveBiomeStock_OffersHaveValidAmountsAndCosts()
        {
            for (int level = 1; level <= 101; level += 5)
            {
                var stock = CaveWanderingMerchant.ResolveBiomeStock(WorldSeed, RunSeed, level);
                foreach (var offer in stock)
                {
                    Assert.Greater(offer.Amount, 0, $"Nivel {level}: quantidade invalida para '{offer.ItemId}'.");
                    Assert.GreaterOrEqual(offer.TotalCost, 0, $"Nivel {level}: custo invalido para '{offer.ItemId}'.");
                    Assert.IsNotEmpty(offer.Prompt, $"Nivel {level}: prompt vazio para '{offer.ItemId}'.");
                }
            }
        }
    }
}
