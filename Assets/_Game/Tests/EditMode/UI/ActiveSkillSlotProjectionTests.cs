using CindarsHope.UI.HUD;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI
{
    // fable_71 — projeção pura do estado de active skill slot para a HUD (sem Unity).
    [TestFixture]
    public class ActiveSkillSlotProjectionTests
    {
        [Test]
        public void EmptySlot_IsBlockedEmpty()
        {
            var vm = new ActiveSkillSlotViewModel { SlotIndex = 0 };
            ActiveSkillSlotProjection.Project(isEquipped: false, cooldownRemaining: 0f, vm);

            Assert.IsFalse(vm.IsUsableInContext, "Slot vazio nao e usavel.");
            Assert.AreEqual(ActiveSlotBlockedReason.Empty, vm.BlockedReason);
            Assert.AreEqual(0f, vm.Cooldown, 0.0001f);
        }

        [Test]
        public void EquippedOnCooldown_IsBlockedCooldown()
        {
            var vm = new ActiveSkillSlotViewModel { SlotIndex = 1 };
            ActiveSkillSlotProjection.Project(isEquipped: true, cooldownRemaining: 2.5f, vm);

            Assert.IsFalse(vm.IsUsableInContext, "Em cooldown nao e usavel.");
            Assert.AreEqual(ActiveSlotBlockedReason.Cooldown, vm.BlockedReason);
            Assert.AreEqual(2.5f, vm.Cooldown, 0.0001f);
        }

        [Test]
        public void EquippedReady_IsUsable()
        {
            var vm = new ActiveSkillSlotViewModel { SlotIndex = 2 };
            ActiveSkillSlotProjection.Project(isEquipped: true, cooldownRemaining: 0f, vm);

            Assert.IsTrue(vm.IsUsableInContext, "Equipada e fora de cooldown e usavel.");
            Assert.AreEqual(ActiveSlotBlockedReason.None, vm.BlockedReason);
            Assert.IsTrue(vm.IsEquipped);
        }
    }
}
