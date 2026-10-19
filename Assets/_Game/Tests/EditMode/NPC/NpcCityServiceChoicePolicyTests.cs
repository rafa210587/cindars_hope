using CindarsHope.City.Services;
using CindarsHope.NPC;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcCityServiceChoicePolicy: mapeamento provider->choice/label e a mensagem de
    /// compra delegada a CityServiceAccess.TryPurchase. Usa os hooks estaticos de teste
    /// (CityServiceAccess.ResetForTests) para isolar estado entre casos.
    /// </summary>
    [TestFixture]
    public sealed class NpcCityServiceChoicePolicyTests
    {
        [TearDown]
        public void TearDown()
        {
            CityServiceAccess.ResetForTests();
        }

        [Test]
        public void TryBuildChoice_ReturnsFalse_ForNonProviderNpc()
        {
            var built = NpcCityServiceChoicePolicy.TryBuildChoice("npc_thalindra", out var choice);

            Assert.That(built, Is.False);
        }

        [Test]
        public void TryBuildChoice_ReturnsCatalogLabel_WhenServiceNotOwned()
        {
            CityServiceAccess.OwnsServiceQuery = _ => false;

            var built = NpcCityServiceChoicePolicy.TryBuildChoice(CityServiceCatalog.TovinNpcId, out var choice);

            Assert.That(built, Is.True);
            Assert.That(choice.ChoiceId, Is.EqualTo(NpcCityServiceChoicePolicy.ChoiceId));
            Assert.That(choice.Label, Is.EqualTo(
                CityServiceCatalog.DisplayLabelFor(CityServiceCatalog.ServiceIdFor(CityServiceCatalog.TovinNpcId))));
        }

        [Test]
        public void TryBuildChoice_ReturnsOwnedLabel_WhenServiceAlreadyOwned()
        {
            CityServiceAccess.OwnsServiceQuery = _ => true;

            var built = NpcCityServiceChoicePolicy.TryBuildChoice(CityServiceCatalog.MaraNpcId, out var choice);

            Assert.That(built, Is.True);
            Assert.That(choice.Label, Is.EqualTo("Servico (ja contratado)"));
        }

        [Test]
        public void PurchaseMessageForProvider_ReturnsNull_ForNonProviderNpc()
        {
            var message = NpcCityServiceChoicePolicy.PurchaseMessageForProvider("npc_thalindra");

            Assert.That(message, Is.Null);
        }

        [Test]
        public void PurchaseMessageForProvider_ReturnsInsufficientGoldMessage_WhenGoldTooLow()
        {
            CityServiceAccess.OwnsServiceQuery = _ => false;
            CityServiceAccess.CurrentGoldFunc = () => 0;

            var message = NpcCityServiceChoicePolicy.PurchaseMessageForProvider(CityServiceCatalog.TovinNpcId);

            Assert.That(message,
                Is.EqualTo($"Ouro insuficiente: precisa de {CityServiceCatalog.LicenseMarketStallCost}g."));
        }

        [Test]
        public void PurchaseMessageForProvider_ReturnsPurchasedMessage_WhenGoldSufficient()
        {
            CityServiceAccess.OwnsServiceQuery = _ => false;
            CityServiceAccess.CurrentGoldFunc = () => 1000;
            CityServiceAccess.SpendGoldFunc = _ => true;
            CityServiceAccess.GrantPossessionAction = _ => { };

            var message = NpcCityServiceChoicePolicy.PurchaseMessageForProvider(CityServiceCatalog.MaraNpcId);

            Assert.That(message,
                Is.EqualTo($"Servico adquirido por {CityServiceCatalog.ContractFarmRegistryCost}g."));
        }
    }
}
