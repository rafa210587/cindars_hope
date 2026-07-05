using CindarsHope.NPC;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class ThalindraQuestDialoguePolicyTests
    {
        [TestCase(false, false, true, QuestGiverInteractionMode.Offer)]
        [TestCase(true, false, false, QuestGiverInteractionMode.NoQuest)]
        [TestCase(true, true, true, QuestGiverInteractionMode.TurnIn)]
        public void Resolve_KeepsChoiceAndModeConsistent(bool hasState, bool canTurnIn,
            bool expectedVisible, QuestGiverInteractionMode expectedMode)
        {
            var decision = ThalindraQuestDialoguePolicy.Resolve(hasState, canTurnIn);

            Assert.That(decision.ShowChoice, Is.EqualTo(expectedVisible));
            Assert.That(decision.InteractionMode, Is.EqualTo(expectedMode));
        }
    }
}
