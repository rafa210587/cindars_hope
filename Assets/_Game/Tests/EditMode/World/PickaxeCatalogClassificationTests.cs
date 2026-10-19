using System.Linq;
using CindarsHope.Editor.Items;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
namespace CindarsHope.Tests.EditMode.World
{
    public class PickaxeCatalogClassificationTests
    {
        [TestCase("item_tool_pickaxe_iron", 90)]
        [TestCase("item_tool_pickaxe_steel", 220)]
        public void Pickaxe_IsToolWithStableEconomy(string id, int value)
        {
            var row = CanonicalItemCatalog.BaseRows().Single(x => x.Id == id);
            Assert.That(row.Category, Is.EqualTo(ItemCategory.Tool));
            Assert.That(row.IsEquippable, Is.True);
            Assert.That(row.BaseValue, Is.EqualTo(value));
            Assert.That(row.MaxStack, Is.EqualTo(1));
        }
    }
}
