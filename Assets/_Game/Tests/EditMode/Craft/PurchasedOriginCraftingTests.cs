using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Craft
{
    public sealed class PurchasedOriginCraftingTests
    {
        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _host;
            private readonly List<Object> _assets = new List<Object>();
            public InventoryManager Inventory { get; }
            public CraftingStation Station { get; }
            public RecipeDataSO Recipe { get; }

            public Fixture(bool instant)
            {
                var input = Item("item_origin_input", 50, 5);
                var output = Item("item_origin_output", 50, 20);
                var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                _assets.Add(database);
                typeof(DataRegistrySO<ItemDataSO>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database,
                    new[] { input, output });
                typeof(DataRegistrySO<ItemDataSO>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, false);
                _host = new GameObject("origin-craft-fixture");
                _host.SetActive(false);
                Inventory = _host.AddComponent<InventoryManager>();
                Inventory.Initialize(database);
                Assert.IsTrue(Inventory.TryAddPurchasedItem(input.Id, 2, 10).Success);
                Recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                _assets.Add(Recipe);
                Recipe.SetId("recipe_origin_fixture");
                Recipe.RequiredStationType = WorkshopType.Workbench;
                Recipe.OutputItemId = output.Id;
                Recipe.OutputAmount = 1;
                Recipe.CraftTimeSeconds = instant ? 0f : 10f;
                Recipe.Ingredients = new[] { new RecipeIngredient(input.Id, 2) };
                Station = new CraftingStation("station_origin_fixture", WorkshopType.Workbench);
            }

            private ItemDataSO Item(string id, int maxStack, int baseValue)
            {
                var item = ScriptableObject.CreateInstance<ItemDataSO>();
                item.Id = id;
                item.MaxStack = maxStack;
                item.BaseValue = baseValue;
                _assets.Add(item);
                return item;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_host);
                foreach (var asset in _assets)
                    if (asset != null) Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void InstantCraftPurchasedInput_ProducesRestrictedOutput()
        {
            using (var fixture = new Fixture(true))
            {
                Assert.IsTrue(fixture.Station.TryStartCraft(fixture.Recipe,
                    fixture.Inventory, out var reason), reason);
                Assert.AreEqual(0, fixture.Inventory.GetAmount("item_origin_input"));
                Assert.IsTrue(fixture.Inventory.TryPrepareRemoval("item_origin_output", 1,
                    out var selected));
                Assert.AreEqual(ItemOriginKind.RestrictedDerived, selected.CopyLots()[0].Kind);
                Assert.AreEqual(8, selected.CopyLots()[0].UnitResaleCapGold);
            }
        }

        [Test]
        public void TimedCraftCancel_ReturnsPurchasedLotsAfterJobSave()
        {
            using (var fixture = new Fixture(false))
            {
                Assert.IsTrue(fixture.Station.TryStartCraft(fixture.Recipe,
                    fixture.Inventory, out var reason), reason);
                var saved = fixture.Station.CaptureSaveData();
                Assert.AreEqual(1, saved.Job.OriginSchemaVersion);
                Assert.AreEqual(2, ItemOriginLot.FromSaveData(
                    saved.Job.IngredientsConsumed[0].OriginLots, 2)[0].Quantity);
                Assert.IsTrue(fixture.Station.TryCancelJob(fixture.Inventory,
                    out reason), reason);
                Assert.IsTrue(fixture.Inventory.TryPrepareRemoval("item_origin_input", 2,
                    out var selected));
                Assert.AreEqual(ItemOriginKind.Purchased, selected.CopyLots()[0].Kind);
                Assert.AreEqual(4, selected.CopyLots()[0].UnitResaleCapGold);
            }
        }
    }
}
