using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Editor.Items;
using CindarsHope.Inventory.Data;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Items
{
    // fable_48 — CA-5 (data side): os 6 item_ammo_arrow_* do catálogo (F32) existem com a
    // categoria/AmmoType/BaseValue do §8, e a tabela do ArrowBallisticsResolver casa exatamente
    // com a munição do catálogo (anti-divergência: ponto único de stats x roster do catálogo).
    //
    // Compila na assembly de Editor (referencia a tabela pura CanonicalItemCatalog, que vive no
    // assembly de Editor). A parte runtime do CA-5 está em BowAmmoElementalArrowsTests.
    [TestFixture]
    public class BowAmmoCatalogTests
    {
        [Test]
        public void Catalog_AllSixArrowsExistWithCanonicalBaseValues()
        {
            var byId = CanonicalItemCatalog.ExpandedRows().ToDictionary(r => r.Id, r => r);

            var expected = new Dictionary<string, int>
            {
                { ArrowBallisticsResolver.ArrowWoodId, 2 },
                { ArrowBallisticsResolver.ArrowIronId, 4 },
                { ArrowBallisticsResolver.ArrowSteelId, 6 },
                { ArrowBallisticsResolver.ArrowSilverId, 12 },
                { ArrowBallisticsResolver.ArrowFireId, 8 },
                { ArrowBallisticsResolver.ArrowFrostId, 8 },
            };

            foreach (var kv in expected)
            {
                Assert.IsTrue(byId.ContainsKey(kv.Key), $"CA-5: catálogo (F32) tem {kv.Key}.");
                var row = byId[kv.Key];
                Assert.AreEqual(ItemCategory.Ammo, row.Category, $"{kv.Key} é Ammo.");
                Assert.AreEqual("arrow", row.AmmoType, $"{kv.Key} AmmoType=arrow.");
                Assert.AreEqual(kv.Value, row.BaseValue, $"{kv.Key} BaseValue do §8.");
            }
        }

        [Test]
        public void Catalog_CoversSixProfilesPlusStarterBasicAlias()
        {
            // Anti-divergência (risco técnico do plano fable_48): a tabela do resolver (ponto único de
            // stats) e os itens de munição do catálogo (roster) cobrem exatamente o mesmo conjunto.
            var catalogArrowIds = CanonicalItemCatalog.ExpandedRows()
                .Where(r => r.Category == ItemCategory.Ammo)
                .Select(r => r.Id)
                .OrderBy(id => id)
                .ToList();

            var resolverIds = ArrowBallisticsResolver.CanonicalOrder.OrderBy(id => id).ToList();
            var profileCatalogIds = catalogArrowIds
                .Where(id => id != ArrowBallisticsResolver.ArrowBasicId)
                .ToList();

            CollectionAssert.AreEqual(profileCatalogIds, resolverIds);
            Assert.AreEqual(7, catalogArrowIds.Count);
            Assert.AreEqual(6, resolverIds.Count);
            Assert.IsTrue(ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowBasicId).IsKnown);
        }
    }
}
