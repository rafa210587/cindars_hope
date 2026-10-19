using System.Collections.Generic;
using System.Linq;
using CindarsHope.Editor.Items;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class CraftingSkillActionTests
    {
        [Test]
        public void IrrigationLine_SelectsOnlyDryForwardPlotsInDeterministicOrder()
        {
            var selected = IrrigationLineRules.SelectDry(new[]
            {
                new IrrigationLineCandidate("far", 3f, 0f, true),
                new IrrigationLineCandidate("wet", 1f, 0f, false),
                new IrrigationLineCandidate("side", 2f, .6f, true),
                new IrrigationLineCandidate("near", 1f, 0f, true),
                new IrrigationLineCandidate("behind", -1f, 0f, true)
            }, 0f, 0f, 1f, 0f, 1);

            CollectionAssert.AreEqual(new[] { "near", "far" }, selected);
            Assert.That(IrrigationLineRules.ResolveLength(1), Is.EqualTo(3));
            Assert.That(IrrigationLineRules.ResolveLength(2), Is.EqualTo(4));
            Assert.That(IrrigationLineRules.ResolveLength(3), Is.EqualTo(5));
        }

        [Test]
        public void BombRules_ResolveRankDamageAndStableItemType()
        {
            Assert.That(CraftBombRules.ResolveDamage(1), Is.EqualTo(18));
            Assert.That(CraftBombRules.ResolveDamage(2), Is.EqualTo(21));
            Assert.That(CraftBombRules.ResolveDamage(3), Is.EqualTo(24));
            Assert.That(CraftingSkillItemIds.TryResolveBombDamageType(
                CraftingSkillItemIds.BombShock, out var type), Is.True);
            Assert.That(type, Is.EqualTo(DamageType.Lightning));
            Assert.That(CraftingSkillItemIds.BombIds.Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public void BombSelection_PrefersSavedStableIdThenUsesCanonicalOrder()
        {
            var inventory = new FakeInventory(new Dictionary<string, int>
            {
                [CraftingSkillItemIds.BombPhysical] = 1,
                [CraftingSkillItemIds.BombFire] = 1
            });
            Assert.That(CraftBombRules.SelectBomb(inventory, CraftingSkillItemIds.BombFire),
                Is.EqualTo(CraftingSkillItemIds.BombFire));
            Assert.That(CraftBombRules.SelectBomb(inventory, string.Empty),
                Is.EqualTo(CraftingSkillItemIds.BombPhysical));
        }

        [Test]
        public void Catalog_ContainsChargesBombsAndMinimalWorkbenchRecipes()
        {
            var itemIds = CanonicalItemCatalog.BaseRows().Select(row => row.Id).ToHashSet();
            Assert.That(itemIds.Contains(CraftingSkillItemIds.IrrigatorCharge), Is.True);
            foreach (var bombId in CraftingSkillItemIds.BombIds)
                Assert.That(itemIds.Contains(bombId), Is.True, bombId);

            var recipes = CanonicalItemCatalog.RecipeRows().ToDictionary(recipe => recipe.Id);
            Assert.That(recipes["recipe_irrigator_charges"].OutputAmount, Is.EqualTo(2));
            Assert.That(recipes["recipe_irrigator_charges"].Ingredients.Select(i => i.Key),
                Does.Contain("item_material_water"));
            Assert.That(recipes["recipe_bomb_physical"].Ingredients.Select(i => i.Key),
                Does.Contain("item_material_spark_dust"));
            Assert.That(recipes["recipe_bomb_fire"].Ingredients.Select(i => i.Key),
                Does.Contain("item_essence_fire"));
        }

        [Test]
        public void DefaultActions_ExposeApprovedCostsCooldownsAndShapes()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var irrigator = actions.Single(a => a.SkillActionId == "skill_crafting_irrigador_portatil");
                Assert.That(irrigator.StaminaCost, Is.EqualTo(18f));
                Assert.That(irrigator.CooldownSeconds, Is.EqualTo(10f));
                Assert.That(irrigator.NotYetExecutable, Is.False);
                CollectionAssert.AreEqual(new[] { 3f, 4f, 5f }, irrigator.EffectMagnitudeByRank);

                var bomb = actions.Single(a => a.SkillActionId == "skill_crafting_bomba_improvisada");
                Assert.That(bomb.ResolveRank(1).Damage, Is.EqualTo(18));
                Assert.That(bomb.ResolveRank(3).Damage, Is.EqualTo(24));
                Assert.That(bomb.CooldownSeconds, Is.EqualTo(6f));
                Assert.That(bomb.EffectRadius, Is.EqualTo(1.8f));
                Assert.That(bomb.MaxTargets, Is.EqualTo(4));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        private sealed class FakeInventory : ISkillItemInventory
        {
            private readonly Dictionary<string, int> _amounts;
            public FakeInventory(Dictionary<string, int> amounts) => _amounts = amounts;
            public int GetAmount(string itemId) => _amounts.TryGetValue(itemId, out var amount) ? amount : 0;
            public bool RemoveItem(string itemId, int amount)
            {
                if (GetAmount(itemId) < amount) return false;
                _amounts[itemId] -= amount;
                return true;
            }
        }
    }
}
