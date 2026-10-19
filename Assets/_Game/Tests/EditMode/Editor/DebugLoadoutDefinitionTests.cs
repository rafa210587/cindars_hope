using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Editor.Validation;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Editor
{
    [TestFixture]
    public class DebugLoadoutDefinitionTests
    {
        private sealed class EntrySnapshot
        {
            public string Id;
            public int Amount;
            public int HotbarSlot;
            public string ToolType;
        }

        [Test]
        public void SmokeLoadout_ContainsCanonicalFarmToolsInExpectedSlots()
        {
            var entries = ReadEntries();

            AssertEntry(entries, "item_shop_tool_hoe_basic", 0, "Hoe");
            AssertEntry(entries, "item_shop_tool_watering_can_basic", 1, "WateringCan");
            AssertEntry(entries, "item_tool_pickaxe_iron", 2, "Pickaxe");
            AssertEntry(entries, "item_tool_fishing_rod_basic", 3, "FishingRod");
        }

        [Test]
        public void SmokeLoadout_ContainsPlantingAndCombatRequirements()
        {
            var entries = ReadEntries();

            Assert.That(entries.Any(entry => entry.Id.StartsWith("item_seed_")), Is.True, "Seed missing.");
            Assert.That(entries.Any(entry => entry.Id.StartsWith("item_weapon_")), Is.True, "Weapon missing.");
            Assert.That(entries.Any(entry => entry.Id.StartsWith("item_ammo_")), Is.True, "Ammo missing.");
            Assert.That(entries.Any(entry => entry.Id.StartsWith("item_consumable_")), Is.True, "Consumable missing.");
            Assert.That(entries.Count(entry => entry.Id.StartsWith("item_material_")), Is.GreaterThanOrEqualTo(2),
                "Crafting/sale materials missing.");
        }

        [Test]
        public void SmokeLoadout_HasUniqueValidHotbarSlotsAndPositiveAmounts()
        {
            var validationMethod = typeof(DebugLoadoutProvisioner).GetMethod(
                "TryValidateSmokeLoadout",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(validationMethod, Is.Not.Null);

            object[] arguments = { null };
            bool valid = (bool)validationMethod.Invoke(null, arguments);

            Assert.That(valid, Is.True, arguments[0] as string);
            Assert.That(arguments[0] as string, Is.Empty);
            Assert.That(ReadEntries().All(entry => entry.Amount > 0), Is.True);
        }

        [Test]
        public void SmokeLoadout_DoesNotContainKnownLegacyAliases()
        {
            var ids = new HashSet<string>(ReadEntries().Select(entry => entry.Id));

            Assert.That(ids, Does.Not.Contain("item_tool_hoe_basic"));
            Assert.That(ids, Does.Not.Contain("item_tool_watering_can_basic"));
            Assert.That(ids, Does.Not.Contain("item_wood"));
            Assert.That(ids, Does.Not.Contain("item_stone"));
        }

        private static List<EntrySnapshot> ReadEntries()
        {
            var property = typeof(DebugLoadoutProvisioner).GetProperty(
                "SmokeLoadout",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null);

            var rawEntries = property.GetValue(null) as IEnumerable;
            Assert.That(rawEntries, Is.Not.Null);

            var entries = new List<EntrySnapshot>();
            foreach (object rawEntry in rawEntries)
            {
                var type = rawEntry.GetType();
                entries.Add(new EntrySnapshot
                {
                    Id = (string)type.GetProperty("Id").GetValue(rawEntry),
                    Amount = (int)type.GetProperty("Amount").GetValue(rawEntry),
                    HotbarSlot = (int)type.GetProperty("HotbarSlot").GetValue(rawEntry),
                    ToolType = type.GetProperty("ToolType").GetValue(rawEntry).ToString(),
                });
            }

            return entries;
        }

        private static void AssertEntry(
            IEnumerable<EntrySnapshot> entries,
            string expectedId,
            int expectedSlot,
            string expectedToolType)
        {
            var entry = entries.SingleOrDefault(candidate => candidate.Id == expectedId);
            Assert.That(entry, Is.Not.Null, $"Missing {expectedId}.");
            Assert.That(entry.HotbarSlot, Is.EqualTo(expectedSlot));
            Assert.That(entry.ToolType, Is.EqualTo(expectedToolType));
        }
    }
}
