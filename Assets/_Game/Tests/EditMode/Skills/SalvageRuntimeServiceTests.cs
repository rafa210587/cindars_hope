using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class SalvageRuntimeServiceTests
    {
        private GameObject _go;
        private ItemDatabaseSO _database;
        private ItemDataSO _gear;
        private ItemDataSO _ore;
        private ItemDataSO _rare;

        [SetUp]
        public void SetUp()
        {
            _gear = Item("gear", 1, false);
            _ore = Item("ore", 99, true);
            _rare = Item("gem", 99, false);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var baseType = typeof(DataRegistrySO<ItemDataSO>);
            baseType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _gear, _ore, _rare });
            baseType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _go = new GameObject("SalvageInventoryTest");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_gear);
            Object.DestroyImmediate(_ore);
            Object.DestroyImmediate(_rare);
        }

        [Test]
        public void Salvage_ReturnsFortyPercentOfEligibleMaterial_AndNeverReturnsIneligible()
        {
            var inventory = _go.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            Assert.That(inventory.TryAddItemInstance("gear", "gear#crafted-1").Success, Is.True);
            var rng = new CraftingPassiveRngState("test");
            var service = new SalvageRuntimeService(inventory,
                new RecipeProvider(new SalvageIngredient("ore", 5), new SalvageIngredient("gem", 10)), rng);

            Assert.That(service.TrySalvage(0, .05f, out var reason), Is.True, reason);
            Assert.That(inventory.GetAmount("gear"), Is.Zero);
            Assert.That(inventory.GetAmount("ore"), Is.InRange(2, 3));
            Assert.That(inventory.GetAmount("gem"), Is.Zero);
            Assert.That(rng.GetNextAttempt("salvage", "gear#crafted-1"), Is.EqualTo(1));
        }

        [Test]
        public void FailedEquippedCandidate_DoesNotRemoveItemOrAdvanceRng()
        {
            var inventory = _go.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            inventory.TryAddItemInstance("gear", "gear#crafted-2");
            inventory.TryGetSlot(0, out var equippedSlot);
            equippedSlot.IsEquipped = true;
            var rng = new CraftingPassiveRngState("test");
            var service = new SalvageRuntimeService(inventory,
                new RecipeProvider(new SalvageIngredient("ore", 2)), rng);

            Assert.That(service.TrySalvage(0, .15f, out _), Is.False);
            Assert.That(inventory.GetAmount("gear"), Is.EqualTo(1));
            Assert.That(rng.GetNextAttempt("salvage", "gear#crafted-2"), Is.Zero);
        }

        [Test]
        public void AtomicInventoryCommit_CallbackFailureLeavesCandidateAndRewardsUntouched()
        {
            var inventory = _go.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            inventory.TryAddItemInstance("gear", "gear#crafted-3");

            Assert.That(inventory.TryCommitSalvage(0, "gear", "gear#crafted-3",
                new[] { new SalvageReward("ore", 2) }, () => false, out _), Is.False);
            Assert.That(inventory.GetAmount("gear"), Is.EqualTo(1));
            Assert.That(inventory.GetAmount("ore"), Is.Zero);
        }

        [Test]
        public void DedicatedModifierProvider_UsesFivePercentPerRankAndCapsAtFifteenPercent()
        {
            var previous = SalvageSkillModifierProvider.ChanceSource;
            try
            {
                SalvageSkillModifierProvider.ChanceSource = () => SalvageSkillModifierProvider.Resolve(8);
                Assert.That(SalvageSkillModifierProvider.CurrentChance, Is.EqualTo(.15f).Within(.0001f));
                Assert.That(SalvageSkillModifierProvider.Resolve(2), Is.EqualTo(.10f).Within(.0001f));
            }
            finally { SalvageSkillModifierProvider.ChanceSource = previous; }
        }

        [Test]
        public void UnlearnedSalvage_DoesNotTouchInventoryOrRng()
        {
            var inventory = _go.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            inventory.TryAddItemInstance("gear", "gear#crafted-4");
            var rng = new CraftingPassiveRngState("test");
            var service = new SalvageRuntimeService(inventory,
                new RecipeProvider(new SalvageIngredient("ore", 2)), rng);

            Assert.That(service.TrySalvage(0, 0f, out _), Is.False);
            Assert.That(inventory.GetAmount("gear"), Is.EqualTo(1));
            Assert.That(rng.GetNextAttempt("salvage", "gear#crafted-4"), Is.Zero);
        }

        [Test]
        public void UnidentifiedDefinition_IsRejectedWithoutAdvancingRng()
        {
            var inventory = _go.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            inventory.TryAddItemInstance("gear", "gear#crafted-5");
            _gear.IsUnidentified = true;
            var rng = new CraftingPassiveRngState("test");
            var service = new SalvageRuntimeService(inventory,
                new RecipeProvider(new SalvageIngredient("ore", 2)), rng);

            Assert.That(service.TrySalvage(0, .05f, out _), Is.False);
            Assert.That(inventory.GetAmount("gear"), Is.EqualTo(1));
            Assert.That(rng.GetNextAttempt("salvage", "gear#crafted-5"), Is.Zero);
        }

        private static ItemDataSO Item(string id, int maxStack, bool eligible)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id; item.MaxStack = maxStack; item.IsCommonMaterialBonusEligible = eligible;
            return item;
        }

        private sealed class RecipeProvider : ISalvageRecipeProvider
        {
            private readonly SalvageIngredient[] _ingredients;
            public RecipeProvider(params SalvageIngredient[] ingredients) { _ingredients = ingredients; }
            public bool TryGetCanonicalSalvageRecipe(string outputItemId, out SalvageRecipeDefinition recipe)
            {
                recipe = new SalvageRecipeDefinition { RecipeId = "recipe_gear", OutputItemId = outputItemId, OutputAmount = 1 };
                recipe.Ingredients.AddRange(_ingredients);
                return true;
            }
        }
    }
}
