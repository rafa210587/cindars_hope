using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core.Events;
using CindarsHope.NPC;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class NpcShopDialogueChoicePolicyTests
    {
        [Test]
        public void BuildRootChoices_PreservesCanonicalOrderAndServiceIds()
        {
            var services = new List<NpcShopServiceChoiceDefinition>
            {
                new NpcShopServiceChoiceDefinition("Analisar", "analysis"),
                new NpcShopServiceChoiceDefinition("Ignorar", null)
            };

            var choices = NpcShopDialogueChoicePolicy.BuildRootChoices(true, "Licenca", services, true);

            Assert.That(choices.Select(choice => choice.ChoiceId), Is.EqualTo(new[]
            {
                "talk", "buy", "sell", "gift", "temper", "service", "svc:analysis", "dbg_open", "exit"
            }));
            Assert.That(choices[5].Label, Is.EqualTo("Licenca"));
        }

        [Test]
        public void BuildThalindraChoices_HidesCompletedQuestAndOptionalDebug()
        {
            var quest = new ThalindraQuestDialogueDecision(null, QuestGiverInteractionMode.NoQuest);
            var choices = NpcShopDialogueChoicePolicy.BuildThalindraChoices(quest, null, false);

            Assert.That(choices.Select(choice => choice.ChoiceId),
                Is.EqualTo(new[] { "buy", "sell", "exit" }));
        }

        [Test]
        public void BuildThalindraChoices_PutsQuestBeforeCommerceAndServices()
        {
            var quest = new ThalindraQuestDialogueDecision("Entregar", QuestGiverInteractionMode.TurnIn);
            var services = new[] { new NpcShopServiceChoiceDefinition("Analisar", "analysis") };
            var choices = NpcShopDialogueChoicePolicy.BuildThalindraChoices(quest, services, false);

            Assert.That(choices.Select(choice => choice.ChoiceId),
                Is.EqualTo(new[] { "quest", "buy", "sell", "svc:analysis", "exit" }));
        }
    }
}
