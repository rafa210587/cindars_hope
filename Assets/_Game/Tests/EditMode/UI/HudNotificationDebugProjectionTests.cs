using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.UI.HUD;
using CindarsHope.UI.Notifications;
// DebugHudProjection is in CindarsHope.UI.HUD namespace

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class HudNotificationDebugProjectionTests
    {
        // ---- GameplayHudViewModel tests ----

        [Test]
        public void HudViewModel_DefaultDebugVisible_IsFalse()
        {
            var hud = new GameplayHudViewModel();
            Assert.IsFalse(hud.DebugVisible);
        }

        [Test]
        public void HudViewModel_HpPercent_Computed()
        {
            var hud = new GameplayHudViewModel { Hp = 50, MaxHp = 200 };
            Assert.AreEqual(0.25f, hud.HpPercent);
            Assert.IsTrue(hud.IsHpLow);
        }

        [Test]
        public void HudViewModel_ShowMp_False_MpLowAlwaysFalse()
        {
            var hud = new GameplayHudViewModel { ShowMp = false, Mp = 0, MaxMp = 100 };
            Assert.IsFalse(hud.IsMpLow);
        }

        [Test]
        public void HudViewModel_CompanionPetProjection_Optional_NoRuntime()
        {
            var hud = new GameplayHudViewModel();
            Assert.IsNull(hud.CompanionProjection);
            Assert.IsNull(hud.PetProjection);
        }

        // ---- Active slot cap tests ----

        [Test]
        public void FinalHudGuard_ActiveSlotsLe4_Passes()
        {
            var hud = new GameplayHudViewModel();
            for (int i = 0; i < 4; i++)
                hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = i });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsEmpty(errors);
        }

        [Test]
        public void FinalHudGuard_ActiveSlots5_Fails()
        {
            var hud = new GameplayHudViewModel();
            for (int i = 0; i < 5; i++)
                hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = i });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsTrue(errors.Exists(e => e.StartsWith("ACTIVE_SLOTS_EXCEEDED")));
        }

        [Test]
        public void FinalHudGuard_DashInActiveSlot_Fails()
        {
            var hud = new GameplayHudViewModel();
            hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = 0, SkillId = "skill_dash_forward" });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsTrue(errors.Exists(e => e.StartsWith("DASH_DODGE_BLOCK_IN_ACTIVE_SLOT")));
        }

        [Test]
        public void FinalHudGuard_DodgeInActiveSlot_Fails()
        {
            var hud = new GameplayHudViewModel();
            hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = 0, SkillId = "skill_dodge_roll" });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsTrue(errors.Exists(e => e.StartsWith("DASH_DODGE_BLOCK_IN_ACTIVE_SLOT")));
        }

        [Test]
        public void FinalHudGuard_BlockInActiveSlot_Fails()
        {
            var hud = new GameplayHudViewModel();
            hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = 0, SkillId = "skill_block" });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsTrue(errors.Exists(e => e.StartsWith("DASH_DODGE_BLOCK_IN_ACTIVE_SLOT")));
        }

        [Test]
        public void FinalHudGuard_DebugVisible_Fails()
        {
            var hud = new GameplayHudViewModel { DebugVisible = true };
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsTrue(errors.Contains("DEBUG_VISIBLE_IN_FINAL_HUD"));
        }

        [Test]
        public void FinalHudGuard_ForbiddenField_IsForbidden()
        {
            Assert.IsTrue(FinalHudGuardValidator.IsForbiddenFieldId("Breath"));
            Assert.IsTrue(FinalHudGuardValidator.IsForbiddenFieldId("Folego"));
            Assert.IsTrue(FinalHudGuardValidator.IsForbiddenFieldId("BR"));
        }

        [Test]
        public void FinalHudGuard_NormalSkill_Passes()
        {
            var hud = new GameplayHudViewModel();
            hud.ActiveSkillSlots.Add(new ActiveSkillSlotViewModel { SlotIndex = 0, SkillId = "skill_fireball" });
            var errors = FinalHudGuardValidator.Validate(hud);
            Assert.IsEmpty(errors);
        }

        // ---- Notification tests ----

        [Test]
        public void NotificationViewModel_DebugOnly_NotAllowedInFinalHud()
        {
            var notif = new NotificationViewModel { Priority = NotificationPriority.DebugOnly };
            Assert.IsTrue(notif.IsDebugOnly);
            Assert.IsFalse(notif.IsAllowedInFinalHud);
        }

        [Test]
        public void NotificationViewModel_Important_AllowedInFinalHud()
        {
            var notif = new NotificationViewModel { Priority = NotificationPriority.Important };
            Assert.IsFalse(notif.IsDebugOnly);
            Assert.IsTrue(notif.IsAllowedInFinalHud);
        }

        [Test]
        public void NotificationQueuePolicy_Default_HasCap()
        {
            var policy = NotificationQueuePolicy.Default();
            Assert.AreEqual(20, policy.HardQueueCap);
            Assert.IsTrue(policy.DebugOnlyFilteredInFinalBuild);
        }

        [Test]
        public void NotificationQueuePolicy_CanStack_SameTypeAndKey()
        {
            var policy = NotificationQueuePolicy.Default();
            var a = new NotificationViewModel { Type = "loot", TextKey = "loot_picked", StackPolicy = NotificationStackPolicy.StackCount };
            var b = new NotificationViewModel { Type = "loot", TextKey = "loot_picked", StackPolicy = NotificationStackPolicy.StackCount };
            Assert.IsTrue(policy.CanStack(a, b));
        }

        [Test]
        public void NotificationQueuePolicy_CannotStack_DifferentType()
        {
            var policy = NotificationQueuePolicy.Default();
            var a = new NotificationViewModel { Type = "loot", TextKey = "loot_picked", StackPolicy = NotificationStackPolicy.StackCount };
            var b = new NotificationViewModel { Type = "quest", TextKey = "loot_picked", StackPolicy = NotificationStackPolicy.StackCount };
            Assert.IsFalse(policy.CanStack(a, b));
        }

        // ---- Debug HUD separation tests ----

        [Test]
        public void DebugHudProjection_IsNotAllowedInFinalBuild()
        {
            var debug = new DebugHudProjection();
            Assert.IsFalse(debug.IsAllowedInFinalBuild);
        }

        [Test]
        public void DebugHudProjection_DefaultVisibleIsFalse()
        {
            var debug = new DebugHudProjection();
            Assert.IsFalse(debug.IsVisible);
        }

        [Test]
        public void DebugHudProjection_HasDebugOnlyDisclaimer()
        {
            Assert.AreEqual("DEBUG_HUD_NOT_FOR_FINAL_BUILD", DebugHudProjection.DebugOnlyDisclaimer);
        }
    }
}
