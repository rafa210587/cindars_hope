using NUnit.Framework;
using CindarsHope.World;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// spec_codex_04 — cobre a decisão pura de entrega de LockedOreNodeInteractable.ResolveDelivery,
    /// extraída para ser testável em EditMode sem MonoBehaviour/GameEventBus. Não cobre wiring de
    /// cena/Inspector nem o gate de progressão real (_unlocked continua fora de escopo — spec futura).
    /// </summary>
    [TestFixture]
    public class LockedOreNodeInteractableTests
    {
        [Test]
        public void ResolveDelivery_WhenNotUnlocked_ReturnsBlocked()
        {
            var result = LockedOreNodeInteractable.ResolveDelivery(unlocked: false, depleted: false, addItemSucceeded: true);

            Assert.AreEqual(LockedOreDeliveryResult.Blocked, result);
        }

        [Test]
        public void ResolveDelivery_WhenNotUnlocked_IgnoresDepletedAndAddItemResult()
        {
            var result = LockedOreNodeInteractable.ResolveDelivery(unlocked: false, depleted: true, addItemSucceeded: false);

            Assert.AreEqual(LockedOreDeliveryResult.Blocked, result);
        }

        [Test]
        public void ResolveDelivery_WhenUnlockedAndAlreadyDepleted_ReturnsAlreadyDepleted()
        {
            var result = LockedOreNodeInteractable.ResolveDelivery(unlocked: true, depleted: true, addItemSucceeded: true);

            Assert.AreEqual(LockedOreDeliveryResult.AlreadyDepleted, result);
        }

        [Test]
        public void ResolveDelivery_WhenUnlockedNotDepletedAndAddItemSucceeds_ReturnsDelivered()
        {
            var result = LockedOreNodeInteractable.ResolveDelivery(unlocked: true, depleted: false, addItemSucceeded: true);

            Assert.AreEqual(LockedOreDeliveryResult.Delivered, result);
        }

        [Test]
        public void ResolveDelivery_WhenUnlockedNotDepletedAndAddItemFails_ReturnsInventoryFull()
        {
            var result = LockedOreNodeInteractable.ResolveDelivery(unlocked: true, depleted: false, addItemSucceeded: false);

            Assert.AreEqual(LockedOreDeliveryResult.InventoryFull, result);
        }
    }
}
