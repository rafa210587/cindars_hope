using System.Linq;
using CindarsHope.NPC;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcDebugExpressionChoicePolicy: shape/ordem dos choices de debug e o round-trip
    /// de parsing do prefixo "dbg:" para NpcExpression.
    /// </summary>
    [TestFixture]
    public sealed class NpcDebugExpressionChoicePolicyTests
    {
        [Test]
        public void BuildExpressionChoices_ReturnsCanonicalOrderWithDbgPrefixAndBackChoice()
        {
            var choices = NpcDebugExpressionChoicePolicy.BuildExpressionChoices();

            Assert.That(choices.Select(choice => choice.ChoiceId), Is.EqualTo(new[]
            {
                "dbg:Neutral", "dbg:Happiness", "dbg:Love", "dbg:Disdain", "dbg:Hatred",
                NpcDebugExpressionChoicePolicy.BackChoiceId
            }));
            Assert.That(choices.Select(choice => choice.Label), Is.EqualTo(new[]
            {
                "Neutro", "Felicidade", "Amor", "Desdem", "Odio", "Voltar"
            }));
        }

        [TestCase("dbg:Neutral", true, NpcExpression.Neutral)]
        [TestCase("dbg:Happiness", true, NpcExpression.Happiness)]
        [TestCase("dbg:Love", true, NpcExpression.Love)]
        [TestCase("dbg:Disdain", true, NpcExpression.Disdain)]
        [TestCase("dbg:Hatred", true, NpcExpression.Hatred)]
        public void TryParseExpressionChoice_RoundTripsEveryBuiltChoice(
            string choiceId, bool expectedParsed, NpcExpression expectedExpression)
        {
            var parsed = NpcDebugExpressionChoicePolicy.TryParseExpressionChoice(choiceId, out var expression);

            Assert.That(parsed, Is.EqualTo(expectedParsed));
            Assert.That(expression, Is.EqualTo(expectedExpression));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("dbg_back")]
        [TestCase("dbg:NotARealExpression")]
        [TestCase("Neutral")]
        public void TryParseExpressionChoice_ReturnsFalse_ForNonExpressionOrUnknownChoiceIds(string choiceId)
        {
            var parsed = NpcDebugExpressionChoicePolicy.TryParseExpressionChoice(choiceId, out var expression);

            Assert.That(parsed, Is.False);
            Assert.That(expression, Is.EqualTo(default(NpcExpression)));
        }
    }
}
