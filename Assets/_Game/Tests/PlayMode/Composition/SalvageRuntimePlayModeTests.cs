using System.Collections;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class SalvageRuntimePlayModeTests
    {
        [UnityTest]
        public IEnumerator SuccessfulSalvage_PublishesStableInstanceIdAfterAtomicInventoryCommit()
        {
            var gear = Item("gear_play", 1, false);
            var ore = Item("ore_play", 99, true);
            var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            SetItems(database, gear, ore);
            var go = new GameObject("SalvagePlayModeInventory");
            var inventory = go.AddComponent<InventoryManager>();
            inventory.Initialize(database);
            Assert.That(inventory.TryAddItem("gear_play", 1).Success, Is.True);
            ItemSalvagedEvent observed = default;
            var received = false;
            System.Action<ItemSalvagedEvent> handler = evt => { observed = evt; received = true; };
            GameEventBus.Subscribe(handler);
            try
            {
                var service = new SalvageRuntimeService(inventory,
                    new Provider("gear_play", new SalvageIngredient("ore_play", 2)),
                    new CraftingPassiveRngState("playmode"));
                Assert.That(service.TrySalvage(0, .05f, out var reason), Is.True, reason);
                Assert.That(received, Is.True);
                Assert.That(observed.ItemInstanceId, Is.EqualTo("gear_play#inventory-1"));
                Assert.That(inventory.GetAmount("ore_play"), Is.InRange(1, 2));
                yield return null;
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
                Object.Destroy(go); Object.Destroy(database); Object.Destroy(gear); Object.Destroy(ore);
            }
        }

        private static ItemDataSO Item(string id, int maxStack, bool eligible)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id; item.MaxStack = maxStack; item.IsCommonMaterialBonusEligible = eligible;
            return item;
        }

        private static void SetItems(ItemDatabaseSO database, params ItemDataSO[] items)
        {
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(database, items);
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(database, null);
        }

        private sealed class Provider : ISalvageRecipeProvider
        {
            private readonly string _output;
            private readonly SalvageIngredient _ingredient;
            public Provider(string output, SalvageIngredient ingredient) { _output = output; _ingredient = ingredient; }
            public bool TryGetCanonicalSalvageRecipe(string outputItemId, out SalvageRecipeDefinition recipe)
            {
                recipe = new SalvageRecipeDefinition { RecipeId = "recipe_play", OutputItemId = _output, OutputAmount = 1 };
                recipe.Ingredients.Add(_ingredient);
                return outputItemId == _output;
            }
        }
    }
}
