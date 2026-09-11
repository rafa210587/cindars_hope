using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class SkillCraftingPassivesPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private float _previousCraftCostReduction;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousCraftCostReduction = SkillModifierHooks.CraftCostReduction;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SkillModifierHooks.PublishCraftCostReduction(_previousCraftCostReduction);
            foreach (var owned in _owned)
                if (owned != null) Object.Destroy(owned);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator StationFocus_ConsumesReducedCommonRequirementAndCancelRestoresExactAmounts()
        {
            var common = Item("play_common", 99, commonMaterial: true);
            var rare = Item("play_rare", 99, category: ItemCategory.Essence);
            var output = Item("play_output", 99);
            var inventory = Inventory(common, rare, output);
            Assert.That(inventory.AddItem(common.Id, 10), Is.True);
            Assert.That(inventory.AddItem(rare.Id, 10), Is.True);

            var recipe = Recipe("play_station_focus", WorkshopType.Workbench, output.Id, 10f,
                new RecipeIngredient(common.Id, 10), new RecipeIngredient(rare.Id, 10));
            var station = new CraftingStation("play_workbench", WorkshopType.Workbench);
            SkillModifierHooks.PublishCraftCostReduction(.25f);

            Assert.That(station.TryStartCraft(recipe, inventory, out var reason), Is.True, reason);
            Assert.That(inventory.GetAmount(common.Id), Is.EqualTo(2));
            Assert.That(inventory.GetAmount(rare.Id), Is.Zero);
            Assert.That(station.Job.IngredientsConsumed.Select(x => (x.ItemId, x.Amount)),
                Is.EquivalentTo(new[] { (common.Id, 8), (rare.Id, 10) }));

            Assert.That(station.TryCancelJob(inventory, out reason), Is.True, reason);
            Assert.That(inventory.GetAmount(common.Id), Is.EqualTo(10));
            Assert.That(inventory.GetAmount(rare.Id), Is.EqualTo(10));
            Assert.That(station.Job, Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DurableFinish_SnapshotsRoundTripsAndCollectsIdentifiedDurability()
        {
            var input = Item("play_ingot", 99, commonMaterial: true);
            var gear = Item("play_crafted_sword", 1, category: ItemCategory.Weapon);
            var inventory = Inventory(input, gear);
            Assert.That(inventory.AddItem(input.Id, 1), Is.True);

            var recipe = Recipe("play_durable_finish", WorkshopType.Forge, gear.Id, 1f,
                new RecipeIngredient(input.Id, 1));
            var station = new CraftingStation("play_forge", WorkshopType.Forge);
            Assert.That(station.TryStartCraft(
                recipe, inventory, out var reason,
                reserveOutputInstanceId: id => id + "#crafted-41",
                resolveBaseDurability: _ => 100,
                craftedDurabilityBonus: .24f), Is.True, reason);

            Assert.That(station.Job.OutputInstanceId,
                Is.EqualTo("play_crafted_sword#crafted-41"));
            Assert.That(station.Job.OutputDurabilityMax, Is.EqualTo(124));

            var roundTripped = new CraftingJob(station.Job.CaptureSaveData(), recipe);
            Assert.That(roundTripped.OutputInstanceId, Is.EqualTo(station.Job.OutputInstanceId));
            Assert.That(roundTripped.OutputDurabilityMax, Is.EqualTo(station.Job.OutputDurabilityMax));

            var tracker = new EquipmentDurabilityTracker();
            station.Update(1f);
            Assert.That(station.TryCollectOutput(
                inventory, out reason, tracker.InitializeEquipment), Is.True, reason);

            var collected = inventory.GetAllItems().Single(item => item.ItemId == gear.Id);
            Assert.That(collected.ItemInstanceId, Is.EqualTo("play_crafted_sword#crafted-41"));
            var durability = tracker.GetDurability(collected.ItemInstanceId);
            Assert.That(durability, Is.Not.Null);
            Assert.That(durability.CurrentDurability, Is.EqualTo(124));
            Assert.That(durability.MaxDurability, Is.EqualTo(124));
            Assert.That(station.Job, Is.Null);
            yield return null;
        }

        private ItemDataSO Item(string id, int maxStack,
            bool commonMaterial = false, ItemCategory category = ItemCategory.Material)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            item.Category = category;
            item.IsCommonMaterialBonusEligible = commonMaterial;
            _owned.Add(item);
            return item;
        }

        private InventoryManager Inventory(params ItemDataSO[] items)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            _owned.Add(database);
            typeof(DataRegistrySO<ItemDataSO>).GetField("_items",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, items);
            typeof(DataRegistrySO<ItemDataSO>).GetField("_indexBuilt",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, false);

            var host = new GameObject("Skill crafting passives PlayMode fixture");
            host.SetActive(false);
            _owned.Add(host);
            var inventory = host.AddComponent<InventoryManager>();
            inventory.Initialize(database);
            return inventory;
        }

        private RecipeDataSO Recipe(string id, WorkshopType stationType, string outputItemId,
            float craftTimeSeconds, params RecipeIngredient[] ingredients)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
            recipe.SetId(id);
            recipe.RequiredStationType = stationType;
            recipe.IsUnlockedByDefault = true;
            recipe.OutputItemId = outputItemId;
            recipe.OutputAmount = 1;
            recipe.CraftTimeSeconds = craftTimeSeconds;
            recipe.Ingredients = ingredients;
            _owned.Add(recipe);
            return recipe;
        }
    }
}
