using System.Collections.Generic;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Economy.Pricing;
using CindarsHope.Editor.Skills;
using CindarsHope.Farm.Shipping;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class LivingForgeOutputVariantTests
    {
        private ItemDataSO _baseItem;
        private ItemDataSO _variantItem;

        [SetUp]
        public void SetUp()
        {
            _baseItem = ScriptableObject.CreateInstance<ItemDataSO>();
            _variantItem = ScriptableObject.CreateInstance<ItemDataSO>();
            _baseItem.Id = "item_food_test_stew";
            _baseItem.DisplayName = "Test Stew";
            _baseItem.Description = "Authored payload";
            _baseItem.Category = ItemCategory.Food;
            _baseItem.ConsumableSubtype = ConsumableSubtype.BuffFood;
            _baseItem.MaxStack = 20;
            _baseItem.BaseValue = 10;
            _baseItem.HungerRestore = 30;
            _baseItem.StaminaRestore = 6;
            _baseItem.BuffDurationSeconds = 25f;
            _baseItem.StatusEffectIds = new[] { "status_focus" };
            _baseItem.UseKind = ItemUseKind.ConsumeFood;
            _baseItem.AllowedEquipmentSlots = new[] { EquipmentSlot.LeftHand };
            _baseItem.WeaponId = "weapon_payload";
            _baseItem.SpellId = "spell_payload";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_baseItem);
            Object.DestroyImmediate(_variantItem);
        }

        [Test]
        public void QualityChain_UsesStableDistinctIds_AndStopsAtQ2()
        {
            Assert.That(LivingForgeOutputVariantCatalog.TryResolveQualityUpgrade(
                _baseItem.Id, out var q1), Is.True);
            Assert.That(q1.ItemId, Is.EqualTo("item_food_test_stew_living_forge_q1"));
            Assert.That(q1.ValueMultiplier, Is.EqualTo(1f));
            Assert.That(q1.DurabilityMaxMultiplier, Is.EqualTo(1.05f));

            Assert.That(LivingForgeOutputVariantCatalog.TryResolveQualityUpgrade(q1.ItemId, out var q2), Is.True);
            Assert.That(q2.ItemId, Is.EqualTo("item_food_test_stew_living_forge_q2"));
            Assert.That(q2.ValueMultiplier, Is.EqualTo(1f));
            Assert.That(q2.DurabilityMaxMultiplier, Is.EqualTo(1.10f));
            Assert.That(LivingForgeOutputVariantCatalog.TryResolveQualityUpgrade(q2.ItemId, out _), Is.False);
        }

        [TestCase(LivingForgeOutputStage.Quality1, 35, 7)]
        [TestCase(LivingForgeOutputStage.Quality2, 41, 8)]
        public void QualityConsumable_ScalesPayloadWithoutChangingPriceOrDuration(
            LivingForgeOutputStage stage, int hunger, int stamina)
        {
            GenerateLivingForgeOutputVariants.ApplyVariant(_variantItem, _baseItem, stage);

            Assert.That(_variantItem.BaseValue, Is.EqualTo(10));
            Assert.That(_variantItem.HungerRestore, Is.EqualTo(hunger));
            Assert.That(_variantItem.StaminaRestore, Is.EqualTo(stamina));
            Assert.That(_variantItem.BuffDurationSeconds, Is.EqualTo(25f).Within(.0001f));
        }

        [Test]
        public void VariantGeneration_PreservesUseEquipmentStatusAndAuthoredPayload()
        {
            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, LivingForgeOutputStage.Quality1);

            Assert.That(_variantItem.Description, Is.EqualTo(_baseItem.Description));
            Assert.That(_variantItem.Category, Is.EqualTo(_baseItem.Category));
            Assert.That(_variantItem.ConsumableSubtype, Is.EqualTo(_baseItem.ConsumableSubtype));
            Assert.That(_variantItem.MaxStack, Is.EqualTo(_baseItem.MaxStack));
            Assert.That(_variantItem.UseKind, Is.EqualTo(_baseItem.UseKind));
            Assert.That(_variantItem.AllowedEquipmentSlots, Is.EqualTo(_baseItem.AllowedEquipmentSlots));
            Assert.That(_variantItem.StatusEffectIds, Is.EqualTo(_baseItem.StatusEffectIds));
            Assert.That(_variantItem.WeaponId, Is.EqualTo(_baseItem.WeaponId));
            Assert.That(_variantItem.SpellId, Is.EqualTo(_baseItem.SpellId));
        }

        [Test]
        public void PotencyVariant_ScalesNumericPayloadAndKeepsPriceAndDuration()
        {
            Assert.That(LivingForgeOutputVariantCatalog.TryResolvePotencyVariant(
                _baseItem.Id, _baseItem, 5, out var potencyVariant), Is.True);

            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, potencyVariant.Stage);

            Assert.That(_variantItem.Id, Is.EqualTo("item_food_test_stew_living_forge_potency"));
            Assert.That(_variantItem.BaseValue, Is.EqualTo(10));
            Assert.That(_variantItem.HungerRestore, Is.EqualTo(32));
            Assert.That(_variantItem.StaminaRestore, Is.EqualTo(6));
            Assert.That(_variantItem.BuffDurationSeconds, Is.EqualTo(25f).Within(.0001f));
        }

        [Test]
        public void QualityTwoPotencyVariant_CombinesPayloadAndKeepsPriceAndDuration()
        {
            Assert.That(LivingForgeOutputVariantCatalog.TryResolveQuality2PotencyVariant(
                _baseItem.Id, _baseItem, 5, out var combined), Is.True);

            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, combined.Stage);

            Assert.That(_variantItem.Id,
                Is.EqualTo("item_food_test_stew_living_forge_q2_potency"));
            Assert.That(_variantItem.BaseValue, Is.EqualTo(10));
            Assert.That(_variantItem.HungerRestore, Is.EqualTo(44));
            Assert.That(_variantItem.StaminaRestore, Is.EqualTo(9));
            Assert.That(_variantItem.BuffDurationSeconds, Is.EqualTo(25f).Within(.0001f));
        }

        [Test]
        public void ConsumableEligibility_RejectsNoIncreaseAndBatchOutsideOneToFive()
        {
            Assert.That(LivingForgeOutputVariantCatalog.TryResolvePotencyVariant(
                _baseItem.Id, _baseItem, 6, out _), Is.False);
            Assert.That(LivingForgeOutputVariantCatalog.TryResolvePotencyVariant(
                _baseItem.Id, _baseItem, 0, out _), Is.False);

            _baseItem.HungerRestore = 1;
            _baseItem.StaminaRestore = 0;
            _baseItem.DurabilityRestoreAmount = 0;
            Assert.That(LivingForgeOutputVariantCatalog.IsEligibleConsumable(
                _baseItem, 1, LivingForgeOutputVariantCatalog.PotencyMultiplier), Is.False);
            Assert.That(LivingForgeOutputVariantCatalog.IsEligibleConsumable(
                _baseItem, 1, LivingForgeOutputVariantCatalog.Quality1PayloadMultiplier), Is.False);
        }

        [Test]
        public void EquipmentQuality_LeavesPayloadUntouched_AndExposesDurabilityTier()
        {
            _baseItem.Category = ItemCategory.Weapon;
            _baseItem.ConsumableSubtype = ConsumableSubtype.None;
            _baseItem.IsEquippable = true;
            _baseItem.MaxStack = 1;
            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, LivingForgeOutputStage.Quality2);

            Assert.That(_variantItem.BaseValue, Is.EqualTo(10));
            Assert.That(_variantItem.HungerRestore, Is.EqualTo(30));
            Assert.That(_variantItem.StaminaRestore, Is.EqualTo(6));
            Assert.That(_variantItem.BuffDurationSeconds, Is.EqualTo(25f));
            Assert.That(LivingForgeOutputVariantCatalog.TryDescribe(_variantItem.Id, out var descriptor), Is.True);
            Assert.That(descriptor.Stage, Is.EqualTo(LivingForgeOutputStage.Quality2));
            Assert.That(descriptor.DurabilityMaxMultiplier, Is.EqualTo(1.10f));
        }

        [Test]
        public void MaterializedQualityVariants_PreservePriceAcrossLiveSellingResolvers()
        {
            _baseItem.BaseValue = 100;
            var qualityOne = ScriptableObject.CreateInstance<ItemDataSO>();
            var qualityTwo = ScriptableObject.CreateInstance<ItemDataSO>();
            var potency = ScriptableObject.CreateInstance<ItemDataSO>();
            var qualityTwoPotency = ScriptableObject.CreateInstance<ItemDataSO>();
            var shop = ScriptableObject.CreateInstance<ShopDataSO>();

            try
            {
                GenerateLivingForgeOutputVariants.ApplyVariant(
                    qualityOne, _baseItem, LivingForgeOutputStage.Quality1);
                GenerateLivingForgeOutputVariants.ApplyVariant(
                    qualityTwo, _baseItem, LivingForgeOutputStage.Quality2);
                GenerateLivingForgeOutputVariants.ApplyVariant(
                    potency, _baseItem, LivingForgeOutputStage.Potency);
                GenerateLivingForgeOutputVariants.ApplyVariant(
                    qualityTwoPotency, _baseItem, LivingForgeOutputStage.Quality2Potency);
                shop.Id = "living_forge_price_test_shop";
                shop.SellPriceMultiplier = .6f;

                var items = new[]
                {
                    _baseItem, qualityOne, qualityTwo, potency, qualityTwoPotency
                };
                var economyPrices = new int[items.Length];
                var shopPrices = new int[items.Length];
                var shippingPrices = new float[items.Length];
                var legacyResolverPrices = new int[items.Length];
                var economy = new EconomyPricingService();
                var shipping = new ShippingPriceResolver();

                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    economyPrices[i] = economy.CalculatePrice(new PriceRequest
                    {
                        ItemId = item.Id,
                        Category = item.Category,
                        BaseValue = item.BaseValue,
                        Channel = PriceChannel.SellPoint
                    }).UnitPrice;
                    shopPrices[i] = ShopManager.CalculateSellPrice(item, shop);
                    shippingPrices[i] = shipping.Resolve(new ShippingPriceInput
                    {
                        ItemId = item.Id,
                        BaseValue = item.BaseValue,
                        QualityTier = 0,
                        ChannelMultiplier = .95f
                    });
                    legacyResolverPrices[i] = ItemPriceResolver.ResolveSellingPrice(
                        item.BaseValue, SellContext.Shipping);
                }

                CollectionAssert.AreEqual(Repeated(economyPrices[0], items.Length),
                    economyPrices, "SellPoint must price Living Forge IDs exactly like the base item.");
                CollectionAssert.AreEqual(Repeated(shopPrices[0], items.Length),
                    shopPrices, "Shop sell pricing must ignore the Living Forge quality suffix.");
                CollectionAssert.AreEqual(
                    Repeated(shippingPrices[0], items.Length),
                    shippingPrices, "Shipping must not interpret Living Forge variants as farm quality tiers.");
                CollectionAssert.AreEqual(
                    Repeated(legacyResolverPrices[0], items.Length),
                    legacyResolverPrices, "The legacy selling resolver must preserve parity for every rank.");
                Assert.That(economyPrices[0], Is.GreaterThan(0));
                Assert.That(shopPrices[0], Is.GreaterThan(0));
                Assert.That(shippingPrices[0], Is.GreaterThan(0f));
                Assert.That(legacyResolverPrices[0], Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(shop);
                Object.DestroyImmediate(qualityTwoPotency);
                Object.DestroyImmediate(potency);
                Object.DestroyImmediate(qualityTwo);
                Object.DestroyImmediate(qualityOne);
            }
        }

        private static T[] Repeated<T>(T value, int count)
        {
            var result = new T[count];
            for (int i = 0; i < count; i++) result[i] = value;
            return result;
        }

        [Test]
        public void VariantMaterialization_IsDeterministicAcrossRepeatedApplication()
        {
            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, LivingForgeOutputStage.Quality1);
            var firstId = _variantItem.Id;
            var firstValue = _variantItem.BaseValue;
            var firstDuration = _variantItem.BuffDurationSeconds;
            var firstStatusIds = (string[])_variantItem.StatusEffectIds.Clone();

            GenerateLivingForgeOutputVariants.ApplyVariant(
                _variantItem, _baseItem, LivingForgeOutputStage.Quality1);

            Assert.That(_variantItem.Id, Is.EqualTo(firstId));
            Assert.That(_variantItem.BaseValue, Is.EqualTo(firstValue));
            Assert.That(_variantItem.BuffDurationSeconds, Is.EqualTo(firstDuration));
            Assert.That(_variantItem.StatusEffectIds, Is.EqualTo(firstStatusIds));
        }

        [Test]
        public void PotencyMaterialization_RejectsConsumableWithoutScalablePayload()
        {
            _baseItem.HungerRestore = 0;
            _baseItem.StaminaRestore = 0;
            _baseItem.DurabilityRestoreAmount = 0;
            Assert.Throws<System.ArgumentException>(() =>
                GenerateLivingForgeOutputVariants.ApplyVariant(
                    _variantItem, _baseItem, LivingForgeOutputStage.Potency));
        }

        [Test]
        public void RegisteredRecipeProjection_UsesOnlyProvidedRuntimeEntries_AndStableOrder()
        {
            var recipeB = ScriptableObject.CreateInstance<RecipeDataSO>();
            var recipeA = ScriptableObject.CreateInstance<RecipeDataSO>();
            var looseRecipe = ScriptableObject.CreateInstance<RecipeDataSO>();
            try
            {
                recipeB.SetId("recipe_b");
                recipeB.OutputItemId = "item_output_b";
                recipeA.SetId("recipe_a");
                recipeA.OutputItemId = "item_output_a";
                looseRecipe.SetId("recipe_loose");
                looseRecipe.OutputItemId = "item_output_loose";

                var result = GenerateLivingForgeOutputVariants.CollectRegisteredRecipeOutputIds(
                    new[] { recipeB, null, recipeA });

                CollectionAssert.AreEqual(new[] { "item_output_a", "item_output_b" }, result);
                CollectionAssert.DoesNotContain(result, looseRecipe.OutputItemId,
                    "A loose recipe asset was not part of the runtime registry input.");
            }
            finally
            {
                Object.DestroyImmediate(recipeB);
                Object.DestroyImmediate(recipeA);
                Object.DestroyImmediate(looseRecipe);
            }
        }

        [Test]
        public void BombConsumer_SelectsAndCommitsExactQualityVariant()
        {
            var variantId = LivingForgeOutputVariantCatalog.Quality1Id(
                CraftingSkillItemIds.BombFire);
            var inventory = new FakeInventory(new Dictionary<string, int>
            {
                [CraftingSkillItemIds.BombFire] = 2,
                [variantId] = 1
            });

            var selected = CraftBombRules.SelectBomb(inventory, variantId);
            Assert.That(selected, Is.EqualTo(variantId));
            Assert.That(CraftBombRules.TryResolveDamageType(selected, out var damageType), Is.True);
            Assert.That(damageType, Is.EqualTo(DamageType.Fire));

            var commit = new SkillItemTransaction(inventory).TryCommit(selected);
            Assert.That(commit.Success, Is.True);
            Assert.That(commit.ItemId, Is.EqualTo(variantId));
            Assert.That(inventory.GetAmount(variantId), Is.Zero);
            Assert.That(inventory.GetAmount(CraftingSkillItemIds.BombFire), Is.EqualTo(2),
                "Consuming a variant must not debit its base item stack.");
        }

        [Test]
        public void BombConsumer_FallbackFindsVariantWhenNoBaseBombExists()
        {
            var q2Shock = LivingForgeOutputVariantCatalog.Quality2Id(
                CraftingSkillItemIds.BombShock);
            var inventory = new FakeInventory(new Dictionary<string, int> { [q2Shock] = 1 });

            Assert.That(CraftBombRules.SelectBomb(inventory, string.Empty), Is.EqualTo(q2Shock));
            Assert.That(CraftBombRules.TryResolveDamageType(q2Shock, out var damageType), Is.True);
            Assert.That(damageType, Is.EqualTo(DamageType.Lightning));
        }

        [Test]
        public void IrrigationConsumer_SelectsAndCommitsExactQualityCharge()
        {
            var q2Charge = LivingForgeOutputVariantCatalog.Quality2Id(
                CraftingSkillItemIds.IrrigatorCharge);
            var inventory = new FakeInventory(new Dictionary<string, int>
            {
                [CraftingSkillItemIds.IrrigatorCharge] = 3,
                [q2Charge] = 1
            });

            var selected = IrrigationLineRules.SelectCharge(inventory, q2Charge);
            Assert.That(selected, Is.EqualTo(q2Charge));
            Assert.That(IrrigationLineRules.IsIrrigatorCharge(selected), Is.True);
            Assert.That(new SkillItemTransaction(inventory).TryCommit(selected).Success, Is.True);
            Assert.That(inventory.GetAmount(q2Charge), Is.Zero);
            Assert.That(inventory.GetAmount(CraftingSkillItemIds.IrrigatorCharge), Is.EqualTo(3));
        }

        [Test]
        public void IrrigationConsumer_FallbackFindsVariantAndRejectsForeignVariant()
        {
            var q1Charge = LivingForgeOutputVariantCatalog.Quality1Id(
                CraftingSkillItemIds.IrrigatorCharge);
            var foreign = LivingForgeOutputVariantCatalog.Quality1Id("item_material_water");
            var inventory = new FakeInventory(new Dictionary<string, int>
            {
                [q1Charge] = 1,
                [foreign] = 1
            });

            Assert.That(IrrigationLineRules.SelectCharge(inventory, foreign), Is.EqualTo(q1Charge));
            Assert.That(IrrigationLineRules.IsIrrigatorCharge(foreign), Is.False);
        }

        private sealed class FakeInventory : ISkillItemInventory
        {
            private readonly Dictionary<string, int> _amounts;

            public FakeInventory(Dictionary<string, int> amounts) => _amounts = amounts;

            public int GetAmount(string itemId) =>
                !string.IsNullOrWhiteSpace(itemId) && _amounts.TryGetValue(itemId, out var amount)
                    ? amount
                    : 0;

            public bool RemoveItem(string itemId, int amount)
            {
                if (amount <= 0 || GetAmount(itemId) < amount) return false;
                _amounts[itemId] -= amount;
                return true;
            }
        }
    }
}
