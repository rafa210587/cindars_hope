using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using CindarsHope.UI.Shop;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class NpcShopTransactionFacadeTests
    {
        [Test]
        public void TryOpenBuyPanel_StopsWhenControllerReadinessDoesNotConverge()
        {
            var shopManagerHost = new GameObject("shop-manager-test");
            var shopManager = shopManagerHost.AddComponent<ShopManager>();
            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            var itemDatabase = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var initializationAttempts = 0;
            var loggedErrors = 0;

            try
            {
                shopData.Id = "test_shop";
                var facade = new NpcShopTransactionFacade(
                    () => shopData,
                    () => shopManager,
                    () => null,
                    () => null,
                    () => itemDatabase,
                    () => null,
                    () => null,
                    () => null,
                    () => false,
                    _ =>
                    {
                        initializationAttempts++;
                        return true;
                    },
                    () => { },
                    (option, field, cause) =>
                    {
                        Assert.That(option, Is.EqualTo(ShopMenuOption.Buy));
                        Assert.That(field, Is.EqualTo("_isReady"));
                        Assert.That(cause, Does.Contain("did not converge"));
                        loggedErrors++;
                    });

                Assert.That(facade.TryOpenBuyPanel(), Is.False);
                Assert.That(initializationAttempts, Is.EqualTo(2));
                Assert.That(loggedErrors, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(shopData);
                Object.DestroyImmediate(itemDatabase);
                Object.DestroyImmediate(shopManagerHost);
            }
        }
    }
}
