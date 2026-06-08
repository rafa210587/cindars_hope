using NUnit.Framework;
using CindarsHope.Economy.Pricing;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class EconomyPricingServiceTests
    {
        private EconomyPricingService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new EconomyPricingService(PricingProfile.Default());
        }

        private PriceRequest BaseRequest(int baseValue = 100, PriceChannel channel = PriceChannel.SellPoint)
            => new PriceRequest { ItemId = "item_carrot", BaseValue = baseValue, Category = ItemCategory.Crop, Channel = channel };

        [Test]
        public void QuestItem_Returns_Blocked()
        {
            var req = BaseRequest();
            req.IsQuestItem = true;
            var result = _service.CalculatePrice(req);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ProtectionFlags.Contains(PriceProtectionFlag.QuestItemBlocked));
        }

        [Test]
        public void KeyItem_Returns_Blocked()
        {
            var req = BaseRequest();
            req.IsKeyItem = true;
            var result = _service.CalculatePrice(req);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ProtectionFlags.Contains(PriceProtectionFlag.KeyItemBlocked));
        }

        [Test]
        public void LoreLocked_Returns_Blocked()
        {
            var req = BaseRequest();
            req.IsLoreLocked = true;
            var result = _service.CalculatePrice(req);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ProtectionFlags.Contains(PriceProtectionFlag.LoreLockedBlocked));
        }

        [Test]
        public void SellPoint_Applies_ChannelMultiplier()
        {
            var req = BaseRequest(100, PriceChannel.SellPoint);
            var result = _service.CalculatePrice(req);
            Assert.IsTrue(result.Success);
            // SellPoint x0.90 * all mods 1.0 = floor(90) = 90
            Assert.AreEqual(90, result.UnitPrice);
        }

        [Test]
        public void ShopSellToPlayer_PriceHigherThanBuyFromPlayer()
        {
            var req = BaseRequest(100);
            var buyResult = _service.CalculatePrice(new PriceRequest { ItemId = req.ItemId, BaseValue = 100, Category = req.Category, Channel = PriceChannel.ShopSellToPlayer });
            var sellResult = _service.CalculatePrice(new PriceRequest { ItemId = req.ItemId, BaseValue = 100, Category = req.Category, Channel = PriceChannel.GenericShopBuyFromPlayer });
            Assert.IsTrue(buyResult.Success && sellResult.Success);
            Assert.Greater(buyResult.UnitPrice, sellResult.UnitPrice, "ShopSellToPlayer must be > ShopBuyFromPlayer");
        }

        [Test]
        public void QualityMultiplier_Q2_Applies()
        {
            var req = BaseRequest(100, PriceChannel.SellPoint);
            req.Quality = 2; // Q2 x1.35
            var result = _service.CalculatePrice(req);
            Assert.IsTrue(result.Success);
            // 100 * 0.90 (SellPoint) * 1.35 (Q2) = 121.5 -> floor = 121
            Assert.AreEqual(121, result.UnitPrice);
        }

        [Test]
        public void Rarity_Uncommon_Increases_SellPrice()
        {
            var reqCommon = BaseRequest(100, PriceChannel.SellPoint);
            reqCommon.Rarity = 0;
            var reqUncommon = BaseRequest(100, PriceChannel.SellPoint);
            reqUncommon.Rarity = 1;

            var rCommon = _service.CalculatePrice(reqCommon);
            var rUncommon = _service.CalculatePrice(reqUncommon);
            Assert.Greater(rUncommon.UnitPrice, rCommon.UnitPrice, "Uncommon must sell for more than Common");
        }

        [Test]
        public void ZeroBaseValue_Returns_Fail()
        {
            var req = BaseRequest(0);
            var result = _service.CalculatePrice(req);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TotalPrice_Equals_UnitPrice_Times_Quantity()
        {
            var req = BaseRequest(100, PriceChannel.SellPoint);
            req.Quantity = 5;
            var result = _service.CalculatePrice(req);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(result.UnitPrice * 5, result.TotalPrice);
        }

        [Test]
        public void AntiArbitrageValidator_Normal_Item_Valid()
        {
            var validator = new AntiArbitrageValidator(_service);
            var req = BaseRequest(100);
            var check = validator.ValidateArbitrage(req);
            Assert.IsTrue(check.IsValid, string.Join(", ", check.Violations));
        }

        [Test]
        public void AntiArbitrageValidator_InfiniteLoop_Detected()
        {
            var validator = new AntiArbitrageValidator(_service);
            // Force specialized shop to pay same as sell-to-player by using a custom profile
            var profile = PricingProfile.Default();
            profile.ChannelSellMultipliers[PriceChannel.SpecializedShopBuyFromPlayer] = 1.40f; // higher than ShopSellToPlayer x1.30
            var svc = new EconomyPricingService(profile);
            var v2 = new AntiArbitrageValidator(svc);
            var req = BaseRequest(100);
            var check = v2.ValidateNoInfiniteLoop(req);
            Assert.IsFalse(check.IsValid, "Should detect infinite loop when sell-back >= buy-price");
        }

        [Test]
        public void Deterministic_SameInput_SameResult()
        {
            var req = BaseRequest(100, PriceChannel.SellPoint);
            req.Quality = 1;
            req.Rarity = 2;
            var r1 = _service.CalculatePrice(req);
            var r2 = _service.CalculatePrice(req);
            Assert.AreEqual(r1.UnitPrice, r2.UnitPrice);
        }
    }
}
