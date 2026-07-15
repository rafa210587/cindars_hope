using NUnit.Framework;
using CindarsHope.Dialogue;
using CindarsHope.UI.Dialogue;
using System.Collections.Generic;

namespace CindarsHope.Tests.EditMode.UI.Dialogue
{
    [TestFixture]
    public class DialogueChoiceTests
    {
        [Test]
        public void DialogueChoice_CanCreateWithDefaults()
        {
            var choice = new DialogueChoice("Accept", "choice_accept");
            Assert.AreEqual("Accept", choice.Label);
            Assert.AreEqual("choice_accept", choice.ChoiceId);
            Assert.AreEqual(DialogueChoice.ChoiceActionType.Neutral, choice.ActionType);
            Assert.IsTrue(choice.IsEnabled);
        }

        [Test]
        public void DialogueChoice_CanSetActionType()
        {
            var choice = new DialogueChoice
            {
                ActionType = DialogueChoice.ChoiceActionType.AcceptQuest
            };
            Assert.AreEqual(DialogueChoice.ChoiceActionType.AcceptQuest, choice.ActionType);
        }

        [Test]
        public void DialogueChoiceValidator_CanExecuteChoice_EnabledReturnsTrue()
        {
            var choice = new DialogueChoice { IsEnabled = true };
            Assert.IsTrue(DialogueChoiceValidator.CanExecuteChoice(choice));
        }

        [Test]
        public void DialogueChoiceValidator_CanExecuteChoice_DisabledReturnsFalse()
        {
            var choice = new DialogueChoice { IsEnabled = false };
            Assert.IsFalse(DialogueChoiceValidator.CanExecuteChoice(choice));
        }

        [Test]
        public void DialogueChoiceValidator_CanExecuteChoice_NullReturnsFalse()
        {
            Assert.IsFalse(DialogueChoiceValidator.CanExecuteChoice(null));
        }

        [Test]
        public void DialogueChoiceValidator_NeedsConfirmation_DeliverQuestReturnsTrue()
        {
            var choice = new DialogueChoice
            {
                ActionType = DialogueChoice.ChoiceActionType.DeliverQuest
            };
            Assert.IsTrue(DialogueChoiceValidator.NeedsConfirmation(choice));
        }

        [Test]
        public void DialogueChoiceValidator_NeedsConfirmation_ShopBuyReturnsTrue()
        {
            var choice = new DialogueChoice
            {
                ActionType = DialogueChoice.ChoiceActionType.ShopBuy
            };
            Assert.IsTrue(DialogueChoiceValidator.NeedsConfirmation(choice));
        }

        [Test]
        public void DialogueChoiceValidator_NeedsConfirmation_NeutralReturnsFalse()
        {
            var choice = new DialogueChoice
            {
                ActionType = DialogueChoice.ChoiceActionType.Neutral
            };
            Assert.IsFalse(DialogueChoiceValidator.NeedsConfirmation(choice));
        }
    }

    [TestFixture]
    public class DialogueStateViewModelTests
    {
        [Test]
        public void DialogueStateViewModel_HasChoices_WhenChoicesEmpty_ReturnsFalse()
        {
            var state = new DialogueStateViewModel();
            Assert.IsFalse(state.HasChoices);
        }

        [Test]
        public void DialogueStateViewModel_HasChoices_WhenChoicesExist_ReturnsTrue()
        {
            var state = new DialogueStateViewModel();
            state.Choices.Add(new DialogueChoice("Choice 1", "c1"));
            Assert.IsTrue(state.HasChoices);
        }

        [Test]
        public void DialogueStateViewModel_IsCompact_WhenNoChoices_ReturnsTrue()
        {
            var state = new DialogueStateViewModel { DialogueText = "Hello" };
            Assert.IsTrue(state.IsCompact);
        }

        [Test]
        public void DialogueStateViewModel_IsCompact_WhenHasChoices_ReturnsFalse()
        {
            var state = new DialogueStateViewModel { DialogueText = "Hello" };
            state.Choices.Add(new DialogueChoice("Choice 1", "c1"));
            Assert.IsFalse(state.IsCompact);
        }

        [Test]
        public void DialogueStateViewModel_SelectNextChoice_CyclesCorrectly()
        {
            var state = new DialogueStateViewModel();
            state.Choices.Add(new DialogueChoice("Choice 1", "c1"));
            state.Choices.Add(new DialogueChoice("Choice 2", "c2"));
            state.ResetSelection();

            Assert.AreEqual(0, state.SelectedChoiceIndex);
            state.SelectNextChoice();
            Assert.AreEqual(1, state.SelectedChoiceIndex);
            state.SelectNextChoice();
            Assert.AreEqual(0, state.SelectedChoiceIndex); // Wraps around
        }

        [Test]
        public void DialogueStateViewModel_SelectPreviousChoice_CyclesCorrectly()
        {
            var state = new DialogueStateViewModel();
            state.Choices.Add(new DialogueChoice("Choice 1", "c1"));
            state.Choices.Add(new DialogueChoice("Choice 2", "c2"));
            state.ResetSelection();

            Assert.AreEqual(0, state.SelectedChoiceIndex);
            state.SelectPreviousChoice();
            Assert.AreEqual(1, state.SelectedChoiceIndex); // Wraps around
            state.SelectPreviousChoice();
            Assert.AreEqual(0, state.SelectedChoiceIndex);
        }

        [Test]
        public void DialogueStateViewModel_GetSelectedChoice_ReturnsCurrentChoice()
        {
            var state = new DialogueStateViewModel();
            var choice1 = new DialogueChoice("Choice 1", "c1");
            var choice2 = new DialogueChoice("Choice 2", "c2");
            state.Choices.Add(choice1);
            state.Choices.Add(choice2);
            state.ResetSelection();

            Assert.AreEqual(choice1, state.GetSelectedChoice());
            state.SelectNextChoice();
            Assert.AreEqual(choice2, state.GetSelectedChoice());
        }

        [Test]
        public void DialogueStateViewModel_ResetSelection_SetsToZeroWhenHasChoices()
        {
            var state = new DialogueStateViewModel();
            state.Choices.Add(new DialogueChoice("Choice 1", "c1"));
            state.SelectedChoiceIndex = 5;
            state.ResetSelection();

            Assert.AreEqual(0, state.SelectedChoiceIndex);
        }

        [Test]
        public void DialogueFocusPolicy_ShouldBlockGameplayInput_WithDialogueTextReturnsTrue()
        {
            var state = new DialogueStateViewModel { DialogueText = "Hello, there!" };
            Assert.IsTrue(DialogueFocusPolicy.ShouldBlockGameplayInput(state));
        }

        [Test]
        public void DialogueFocusPolicy_ShouldBlockGameplayInput_WithoutTextReturnsFalse()
        {
            var state = new DialogueStateViewModel { DialogueText = "" };
            Assert.IsFalse(DialogueFocusPolicy.ShouldBlockGameplayInput(state));
        }

        [Test]
        public void DialogueFocusPolicy_AllowsPlayerMovement_ReverseOfBlockInput()
        {
            var state = new DialogueStateViewModel { DialogueText = "Hello" };
            Assert.IsFalse(DialogueFocusPolicy.AllowsPlayerMovement(state));

            state.DialogueText = "";
            Assert.IsTrue(DialogueFocusPolicy.AllowsPlayerMovement(state));
        }
    }
}
