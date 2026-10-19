using NUnit.Framework;
using CindarsHope.Economy;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class ItemPriceResolverTests
    {
        private EconomyBalanceConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EconomyBalanceConfigSO>();
            // defaults: ShippingBuybackMultiplier=1.0, NpcSellMultiplier=0.9
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Shipping_ReturnsBaseValue_WithDefaultMultiplierOne()
        {
            Assert.AreEqual(100, ItemPriceResolver.ResolveSellingPrice(100, SellContext.Shipping, _config));
        }

        [Test]
        public void NpcBuy_Returns90Percent_WithDefaultMultiplier()
        {
            Assert.AreEqual(90, ItemPriceResolver.ResolveSellingPrice(100, SellContext.NpcBuy, _config));
        }

        [Test]
        public void Shipping_NullConfig_FallsBackToFullValue()
        {
            Assert.AreEqual(100, ItemPriceResolver.ResolveSellingPrice(100, SellContext.Shipping, null));
        }

        [Test]
        public void NpcBuy_NullConfig_FallsBackTo90Percent()
        {
            Assert.AreEqual(90, ItemPriceResolver.ResolveSellingPrice(100, SellContext.NpcBuy, null));
        }

        [Test]
        public void ResolvePrice_NeverNegative()
        {
            Assert.AreEqual(0, ItemPriceResolver.ResolveSellingPrice(0,   SellContext.Shipping, _config));
            Assert.AreEqual(0, ItemPriceResolver.ResolveSellingPrice(-10, SellContext.NpcBuy,   _config));
        }

        [Test]
        public void EventStall_UsesSameMultiplierAsShipping()
        {
            int shipping = ItemPriceResolver.ResolveSellingPrice(100, SellContext.Shipping,   _config);
            int stall    = ItemPriceResolver.ResolveSellingPrice(100, SellContext.EventStall, _config);
            Assert.AreEqual(shipping, stall);
        }

        [Test]
        public void CustomMultiplier_IsRespected()
        {
            _config.ShippingBuybackMultiplier = 0.8f;
            Assert.AreEqual(80, ItemPriceResolver.ResolveSellingPrice(100, SellContext.Shipping, _config));
        }
    }
}
