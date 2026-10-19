using NUnit.Framework;
using CindarsHope.UI;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class UIStatePatternTests
    {
        [Test]
        public void UIStatePattern_Empty_CreatesEmptyState()
        {
            var state = UIStatePattern.Empty("No items", "Go to shop");
            Assert.AreEqual(UIStatePattern.StateType.Empty, state.State);
            Assert.AreEqual("No items", state.Message);
            Assert.AreEqual("Go to shop", state.NextStep);
        }

        [Test]
        public void UIStatePattern_Blocked_CreatesBlockedState()
        {
            var state = UIStatePattern.Blocked("Requires skill level 10", "Gain XP");
            Assert.AreEqual(UIStatePattern.StateType.Blocked, state.State);
            Assert.AreEqual("Requires skill level 10", state.Message);
        }

        [Test]
        public void UIStatePattern_Error_CreatesErrorState()
        {
            var state = UIStatePattern.Error("Connection failed");
            Assert.AreEqual(UIStatePattern.StateType.Error, state.State);
            Assert.AreEqual("Connection failed", state.Message);
        }

        [Test]
        public void UIStatePattern_Normal_IsInteractiveTrue()
        {
            var state = new UIStatePattern { State = UIStatePattern.StateType.Normal };
            Assert.IsTrue(state.IsInteractive);
        }

        [Test]
        public void UIStatePattern_Error_IsInteractiveFalse()
        {
            var state = UIStatePattern.Error("Failed");
            Assert.IsFalse(state.IsInteractive);
        }
    }

    [TestFixture]
    public class ConfirmationActionTests
    {
        [Test]
        public void ConfirmationAction_StrongType_RequiresConfirmationTrue()
        {
            var action = new ConfirmationAction
            {
                Type = ConfirmationAction.ConfirmationType.Strong
            };
            Assert.IsTrue(action.RequiresConfirmation);
        }

        [Test]
        public void ConfirmationAction_Irreversible_RequiresConfirmationTrue()
        {
            var action = new ConfirmationAction
            {
                Type = ConfirmationAction.ConfirmationType.Light,
                IsReversible = false
            };
            Assert.IsTrue(action.RequiresConfirmation);
        }

        [Test]
        public void ConfirmationValidator_NeedsConfirmation_StrongActionReturnsTrue()
        {
            var action = new ConfirmationAction
            {
                Type = ConfirmationAction.ConfirmationType.Strong
            };
            Assert.IsTrue(ConfirmationValidator.NeedsConfirmation(action));
        }

        [Test]
        public void ConfirmationValidator_NeedsConfirmation_LightReversibleReturnsFalse()
        {
            var action = new ConfirmationAction
            {
                Type = ConfirmationAction.ConfirmationType.Light,
                IsReversible = true
            };
            Assert.IsFalse(ConfirmationValidator.NeedsConfirmation(action));
        }

        [Test]
        public void ConfirmationValidator_GetConfirmationMessage_FormatsCorrectly()
        {
            var action = new ConfirmationAction
            {
                ActionLabel = "Delete",
                TargetName = "Item X",
                Cost = "Gold 100",
                Consequence = "Cannot undo"
            };
            var msg = ConfirmationValidator.GetConfirmationMessage(action);
            Assert.That(msg, Does.Contain("Delete"));
            Assert.That(msg, Does.Contain("Item X"));
            Assert.That(msg, Does.Contain("Gold 100"));
        }
    }
}
