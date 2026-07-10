using System.Collections.Generic;
using System.Linq;
using CindarsHope.NPC;
using CindarsHope.UI.Dialogue;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcShopChoiceUiAdapter.ToUiChoices: mapeamento 1:1 preservando ordem,
    /// Label/ChoiceId, e os defaults de DialogueChoice (ActionType Neutral, IsEnabled true) por nao
    /// serem setados pelo adapter.
    /// </summary>
    [TestFixture]
    public sealed class NpcShopChoiceUiAdapterTests
    {
        [Test]
        public void ToUiChoices_ReturnsEmptyList_ForEmptyDefinitions()
        {
            var result = NpcShopChoiceUiAdapter.ToUiChoices(new List<NpcShopChoiceDefinition>());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ToUiChoices_PreservesOrderAndLabelChoiceIdMapping()
        {
            var definitions = new List<NpcShopChoiceDefinition>
            {
                new NpcShopChoiceDefinition("Comprar", "buy"),
                new NpcShopChoiceDefinition("Vender", "sell"),
                new NpcShopChoiceDefinition("Adeus", "exit"),
            };

            var result = NpcShopChoiceUiAdapter.ToUiChoices(definitions);

            Assert.That(result.Select(choice => choice.Label), Is.EqualTo(new[] { "Comprar", "Vender", "Adeus" }));
            Assert.That(result.Select(choice => choice.ChoiceId), Is.EqualTo(new[] { "buy", "sell", "exit" }));
        }

        [Test]
        public void ToUiChoices_LeavesActionTypeAndEnabledAtDefaults()
        {
            var definitions = new List<NpcShopChoiceDefinition> { new NpcShopChoiceDefinition("Vender", "sell") };

            var result = NpcShopChoiceUiAdapter.ToUiChoices(definitions);

            Assert.That(result[0].ActionType, Is.EqualTo(CindarsHope.UI.Dialogue.DialogueChoice.ChoiceActionType.Neutral));
            Assert.That(result[0].IsEnabled, Is.True);
            Assert.That(result[0].TargetId, Is.Null);
            Assert.That(result[0].DisabledReason, Is.Null);
        }
    }
}
