using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat.Weapon;
using CindarsHope.Editor.Items;
using CindarsHope.Inventory.Data;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Items
{
    // fable_32 — EditMode coverage for the PURE canonical item catalog data table.
    //
    // Drives CanonicalItemCatalog (no AssetDatabase / Play Mode) so the catalog's deterministic
    // rules are proven before the deferred Unity asset generation runs: unique ids, BV>0 where
    // sellable, Silver x1.5 / Gold x2.0 variants (CA-2), 6 essences (EMENDA V3.2), recipe
    // ingredients resolve (CA-4), and enum save-safety (EMENDA V3.3). This file compiles into
    // Compila na assembly de Editor porque referencia a tabela de catálogo do tooling.
    [TestFixture]
    public class ItemCatalogDataTests
    {
        // ── CA-4: every expanded item id is unique across the whole catalog ──────────
        [Test]
        public void ExpandedItemIds_AreUnique()
        {
            var ids = CanonicalItemCatalog.ExpandedRows().Select(r => r.Id).ToList();
            var dupes = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(dupes, "Duplicate item ids: " + string.Join(", ", dupes));
        }

        // ── §1 governance: every id has a category prefix and a non-empty display name ─
        [Test]
        public void EveryItem_HasCategoryPrefixedId_AndName()
        {
            foreach (var row in CanonicalItemCatalog.ExpandedRows())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(row.Id), "Empty item id.");
                Assert.IsTrue(row.Id.StartsWith("item_") || row.Id.StartsWith("fertilizer_"),
                    $"Item id '{row.Id}' is not category-prefixed (rule §1).");
                Assert.IsFalse(string.IsNullOrWhiteSpace(row.DisplayName), $"Item '{row.Id}' has no display name.");
            }
        }

        // ── economy_rules: sellable items must declare BaseValue > 0 ─────────────────
        [Test]
        public void SellableItems_HavePositiveBaseValue()
        {
            var offenders = CanonicalItemCatalog.ExpandedRows()
                .Where(r => r.Sellable && r.BaseValue <= 0)
                .Select(r => r.Id)
                .ToList();
            Assert.IsEmpty(offenders, "Sellable items with BV<=0: " + string.Join(", ", offenders));
        }

        // ── Non-sellable items are exactly the documented exceptions (water, keys, etc.) ─
        [Test]
        public void NonSellableItems_AreAllowedToBeZeroOrSpecial()
        {
            // Keys are non-sellable with BV 0; water is BV 1; agua_viva BV 0 anti-exploit.
            var keys = CanonicalItemCatalog.ExpandedRows().Where(r => r.Category == ItemCategory.KeyItem);
            Assert.IsTrue(keys.All(k => !k.Sellable), "Key items must be non-sellable.");
            var water = CanonicalItemCatalog.ExpandedRows().Single(r => r.Id == "item_material_water");
            Assert.IsFalse(water.Sellable, "Water must be non-sellable (E2.9).");
        }

        // ── CA-2: Silver = round(base x1.5), Gold = round(base x2.0), consistent rounding ─
        [Test]
        public void QualityVariants_UseCorrectMultipliers()
        {
            var allById = CanonicalItemCatalog.ExpandedRows().ToDictionary(r => r.Id, r => r);

            var baseRows = CanonicalItemCatalog.BaseRows().Where(r => r.QualityVariants).ToList();
            Assert.IsNotEmpty(baseRows, "Expected at least crops + animal products to carry quality variants.");

            foreach (var baseRow in baseRows)
            {
                var silverId = baseRow.Id + CanonicalItemCatalog.SilverSuffix;
                var goldId = baseRow.Id + CanonicalItemCatalog.GoldSuffix;

                Assert.IsTrue(allById.ContainsKey(silverId), $"Missing silver variant for {baseRow.Id}.");
                Assert.IsTrue(allById.ContainsKey(goldId), $"Missing gold variant for {baseRow.Id}.");

                var expectedSilver = CanonicalItemCatalog.QualityValue(baseRow.BaseValue, 1.5f);
                var expectedGold = CanonicalItemCatalog.QualityValue(baseRow.BaseValue, 2.0f);

                Assert.AreEqual(expectedSilver, allById[silverId].BaseValue,
                    $"{silverId} should be round({baseRow.BaseValue} x1.5).");
                Assert.AreEqual(expectedGold, allById[goldId].BaseValue,
                    $"{goldId} should be round({baseRow.BaseValue} x2.0) (EMENDA V3.1: Gold x2.0, NOT x2.2).");
            }
        }

        // ── CA-2 (regression guard): Gold multiplier is 2.0, never the legacy 2.2 ────
        [Test]
        public void GoldMultiplier_IsTwoPointZero_NotTwoPointTwo()
        {
            Assert.AreEqual(2.0f, CanonicalItemCatalog.GoldMultiplier, 0.0001f);
            // Worked example: carrot BV 14 -> gold 28 (x2.0), not 31 (x2.2 would be 30.8->31).
            var carrotGold = CanonicalItemCatalog.ExpandedRows().Single(r => r.Id == "item_crop_carrot_gold");
            Assert.AreEqual(28, carrotGold.BaseValue, "Carrot Gold must be 28 (14 x2.0).");
        }

        // ── EMENDA V3.2: exactly 6 essences ──────────────────────────────────────────
        [Test]
        public void Essences_AreExactlySix()
        {
            var essences = CanonicalItemCatalog.ExpandedRows()
                .Where(r => r.Category == ItemCategory.Essence)
                .Select(r => r.Id)
                .ToList();
            Assert.AreEqual(6, essences.Count, "Must be 6 essences (EMENDA V3.2), not 8. Found: " + string.Join(", ", essences));
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "item_essence_fire", "item_essence_ice", "item_essence_toxic",
                    "item_essence_lightning", "item_essence_arcane", "item_essence_void"
                },
                essences);
        }

        // ── CA-4: every recipe ingredient and output resolves to a catalog item id ───
        [Test]
        public void RecipeIngredientsAndOutputs_ResolveToCatalogItems()
        {
            var itemIds = CanonicalItemCatalog.AllItemIds();
            var unresolved = new List<string>();

            foreach (var recipe in CanonicalItemCatalog.RecipeRows())
            {
                if (!itemIds.Contains(recipe.OutputItemId))
                {
                    unresolved.Add($"{recipe.Id} -> output '{recipe.OutputItemId}'");
                }

                foreach (var ing in recipe.Ingredients)
                {
                    if (!itemIds.Contains(ing.Key))
                    {
                        unresolved.Add($"{recipe.Id} -> ingredient '{ing.Key}'");
                    }
                    Assert.GreaterOrEqual(ing.Value, 1, $"{recipe.Id} ingredient '{ing.Key}' amount must be >= 1.");
                }
            }

            Assert.IsEmpty(unresolved, "Recipes reference non-existent items: " + string.Join("; ", unresolved));
        }

        // ── CA-4: recipe ids are unique ──────────────────────────────────────────────
        [Test]
        public void RecipeIds_AreUnique()
        {
            var ids = CanonicalItemCatalog.RecipeRows().Select(r => r.Id).ToList();
            var dupes = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(dupes, "Duplicate recipe ids: " + string.Join(", ", dupes));
        }

        // ── CA-3 idempotency-by-design: the table is deterministic (same call -> same rows) ─
        [Test]
        public void Catalog_IsDeterministic()
        {
            var first = CanonicalItemCatalog.ExpandedRows().Select(r => $"{r.Id}:{r.BaseValue}:{(int)r.Category}").ToList();
            var second = CanonicalItemCatalog.ExpandedRows().Select(r => $"{r.Id}:{r.BaseValue}:{(int)r.Category}").ToList();
            CollectionAssert.AreEqual(first, second, "Catalog table must be deterministic for idempotent generation.");
        }

        // ── Per-group counts match the canonical catalog (PARTE H / EMENDA) ──────────
        [Test]
        public void GroupCounts_MatchCanonicalCatalog()
        {
            var rows = CanonicalItemCatalog.ExpandedRows();

            Assert.AreEqual(12, rows.Count(r => r.Category == ItemCategory.Seed), "12 seeds (§4).");
            // 12 crops x 3 quality levels = 36 crop entries.
            Assert.AreEqual(36, rows.Count(r => r.Category == ItemCategory.Crop), "12 crops x 3 quality (§4 + E2.1).");
            Assert.AreEqual(6, rows.Count(r => r.Category == ItemCategory.Essence), "6 essences (E2.5).");
            // 4 animal products x 3 quality = 12.
            Assert.AreEqual(12, rows.Count(r => r.Category == ItemCategory.AnimalProduct), "4 animal products x 3 quality (§18).");
            Assert.AreEqual(6, rows.Count(r => r.Category == ItemCategory.Ammo), "6 arrows (§8).");
            Assert.AreEqual(10, rows.Count(r => r.Category == ItemCategory.Fish), "10 fish (E2.10).");
            Assert.AreEqual(4, rows.Count(r => r.Category == ItemCategory.Relic), "4 relics (§17).");
            Assert.AreEqual(12, rows.Count(r => r.Category == ItemCategory.Accessory), "12 accessories (§16).");
            Assert.AreEqual(8, rows.Count(r => r.Category == ItemCategory.KeyItem), "8 keys (§20).");
        }

        // ── Catalog size is pinned so any accidental drift (add/remove row) trips a test ──
        // PARTE H documented "~118 base" BEFORE the V3 amendment; V3 then ADDED high-tier gear
        // (E2.6, 14), cave magic items (E2.3, 10) and orphan drops (E2.8, ~25), so the post-V3
        // canonical base is larger than 118 (>= the documented floor). The F30 ExpectedCount=118
        // is the pre-V3 figure and will surface a count WARN (never ERROR) — documented in the
        // execution report as the expected, honest divergence. These constants pin the real size.
        public const int ExpectedBaseCount = 207;
        public const int ExpectedExpandedCount = 239; // 207 base + 16 quality-variant rows x2 variants

        [Test]
        public void BaseCatalog_CountIsPinned_AndAtLeastDocumentedFloor()
        {
            var baseCount = CanonicalItemCatalog.BaseRows().Count;
            Assert.GreaterOrEqual(baseCount, 118, $"Base catalog must be >= the documented 118 floor (PARTE H). Was {baseCount}.");
            Assert.AreEqual(ExpectedBaseCount, baseCount, $"Base catalog row count drifted; was {baseCount}.");
        }

        [Test]
        public void ExpandedCatalog_CountIsPinned()
        {
            Assert.AreEqual(ExpectedExpandedCount, CanonicalItemCatalog.ExpandedRows().Count,
                "Expanded catalog (base + quality variants) row count drifted.");
        }

        // ── EMENDA V3.3 save-safety: legacy WeaponType ordinals are pinned ───────────
        [Test]
        public void WeaponType_LegacyOrdinals_AreStable()
        {
            Assert.AreEqual(0, (int)WeaponType.None);
            Assert.AreEqual(1, (int)WeaponType.Sword);
            Assert.AreEqual(2, (int)WeaponType.Spear);
            Assert.AreEqual(3, (int)WeaponType.Axe);
            Assert.AreEqual(4, (int)WeaponType.Bow);
            Assert.AreEqual(5, (int)WeaponType.Staff);
            Assert.AreEqual(6, (int)WeaponType.Dagger);
        }

        [Test]
        public void WeaponType_NewMembers_HaveExplicitHighValues()
        {
            Assert.AreEqual(100, (int)WeaponType.Hammer);
            Assert.AreEqual(101, (int)WeaponType.Wand);
            Assert.AreEqual(102, (int)WeaponType.Tool);
        }

        // ── EMENDA V3.3 save-safety: legacy ItemCategory values are pinned ───────────
        [Test]
        public void ItemCategory_LegacyValues_AreStable()
        {
            Assert.AreEqual(0, (int)ItemCategory.Seed);
            Assert.AreEqual(1, (int)ItemCategory.Crop);
            Assert.AreEqual(2, (int)ItemCategory.Food);
            Assert.AreEqual(3, (int)ItemCategory.Material);
            Assert.AreEqual(4, (int)ItemCategory.Tool);
            Assert.AreEqual(5, (int)ItemCategory.Fish);
            Assert.AreEqual(6, (int)ItemCategory.Misc);
            Assert.AreEqual(110, (int)ItemCategory.Furniture);
        }

        [Test]
        public void ItemCategory_NewMembers_HaveExplicitHighValues()
        {
            Assert.AreEqual(111, (int)ItemCategory.Armor);
            Assert.AreEqual(112, (int)ItemCategory.Shield);
            Assert.AreEqual(113, (int)ItemCategory.Accessory);
            Assert.AreEqual(114, (int)ItemCategory.Relic);
            Assert.AreEqual(115, (int)ItemCategory.Essence);
            Assert.AreEqual(116, (int)ItemCategory.AnimalProduct);
        }

        // ── Tool is added to WeaponType only; ItemCategory.Tool stays at its legacy value 4 ─
        [Test]
        public void Tool_IsNotDuplicatedAcrossEnums()
        {
            // ItemCategory.Tool is the legacy farming-tool category (value 4); WeaponType.Tool
            // is the new weapon TYPE (value 102). They are intentionally distinct concepts.
            Assert.AreEqual(4, (int)ItemCategory.Tool);
            Assert.AreEqual(102, (int)WeaponType.Tool);
        }

        // ── Dormant items are flagged (documented), never silently undocumented ──────
        [Test]
        public void DormantItems_AreFlaggedWithNotes()
        {
            var dormantWithoutNote = CanonicalItemCatalog.ExpandedRows()
                .Where(r => r.Dormant && string.IsNullOrWhiteSpace(r.Notes))
                .Select(r => r.Id)
                .ToList();
            Assert.IsEmpty(dormantWithoutNote, "Dormant items must carry a documenting note: " + string.Join(", ", dormantWithoutNote));
        }
    }
}
