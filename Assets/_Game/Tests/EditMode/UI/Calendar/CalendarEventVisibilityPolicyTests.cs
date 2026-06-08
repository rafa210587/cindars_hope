using NUnit.Framework;
using CindarsHope.UI.Calendar;

namespace CindarsHope.Tests.EditMode.UI.Calendar
{
    /// <summary>
    /// SPEC 04: Spoiler-safe visibility policy tests.
    /// Ensures no hidden, future, or secret events are revealed in day detail.
    /// </summary>
    [TestFixture]
    public class CalendarEventVisibilityPolicyTests
    {
        [Test]
        public void CanShowEvent_PublicEvent_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowEvent("festival_harvest"));
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowEvent("birthday_npc"));
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowEvent("quest_timer"));
        }

        [Test]
        public void CanShowEvent_Level101Final_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowEvent("level_101_final_event"));
        }

        [Test]
        public void CanShowEvent_ManaUnlock_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowEvent("mana_unlock_condition"));
        }

        [Test]
        public void CanShowEvent_NyxHidden_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowEvent("nyx_hidden_character"));
        }

        [Test]
        public void CanShowEvent_FuturePets_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowEvent("future_pets"));
        }

        [Test]
        public void CanShowFestival_PublicNotDiscovered_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowFestival(isHidden: false, isDiscovered: false));
        }

        [Test]
        public void CanShowFestival_HiddenNotDiscovered_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowFestival(isHidden: true, isDiscovered: false));
        }

        [Test]
        public void CanShowFestival_HiddenButDiscovered_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowFestival(isHidden: true, isDiscovered: true));
        }

        [Test]
        public void CanShowLunarEvent_Known_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowLunarEvent(isKnown: true));
        }

        [Test]
        public void CanShowLunarEvent_Unknown_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowLunarEvent(isKnown: false));
        }

        [Test]
        public void CanShowOrderDeadline_Active_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowOrderDeadline(isActive: true));
        }

        [Test]
        public void CanShowOrderDeadline_Inactive_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowOrderDeadline(isActive: false));
        }

        [Test]
        public void GetQuestConditionDisplay_DiscoveredCondition_ReturnsExactText()
        {
            string result = CalendarEventVisibilityPolicy.GetQuestConditionDisplay(
                "Collect 5 copper ore",
                isConditionDiscovered: true
            );
            Assert.AreEqual("Collect 5 copper ore", result);
        }

        [Test]
        public void GetQuestConditionDisplay_UndiscoveredCondition_ReturnsGenericHint()
        {
            string result = CalendarEventVisibilityPolicy.GetQuestConditionDisplay(
                "Reach level 50",
                isConditionDiscovered: false
            );
            Assert.AreEqual("Waiting for something...", result);
            Assert.IsFalse(result.Contains("Reach level 50"), "Exact condition must not be revealed");
        }

        [Test]
        public void CanShowShopState_Known_ReturnsTrue()
        {
            Assert.IsTrue(CalendarEventVisibilityPolicy.CanShowShopState(stateIsKnown: true));
        }

        [Test]
        public void CanShowShopState_Unknown_ReturnsFalse()
        {
            Assert.IsFalse(CalendarEventVisibilityPolicy.CanShowShopState(stateIsKnown: false));
        }
    }
}
