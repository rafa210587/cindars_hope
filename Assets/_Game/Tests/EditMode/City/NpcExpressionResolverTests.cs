using System;
using NUnit.Framework;
using CindarsHope.NPC;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class NpcExpressionResolverTests
    {
        [Test]
        public void Resolve_AcrossRange_MapsToExpectedBands()
        {
            Assert.AreEqual(NpcExpression.Hatred, NpcExpressionResolver.Resolve(-100));
            Assert.AreEqual(NpcExpression.Hatred, NpcExpressionResolver.Resolve(-60));
            Assert.AreEqual(NpcExpression.Disdain, NpcExpressionResolver.Resolve(-59));
            Assert.AreEqual(NpcExpression.Disdain, NpcExpressionResolver.Resolve(-20));
            Assert.AreEqual(NpcExpression.Neutral, NpcExpressionResolver.Resolve(-19));
            Assert.AreEqual(NpcExpression.Neutral, NpcExpressionResolver.Resolve(0));
            Assert.AreEqual(NpcExpression.Neutral, NpcExpressionResolver.Resolve(19));
            Assert.AreEqual(NpcExpression.Happiness, NpcExpressionResolver.Resolve(20));
            Assert.AreEqual(NpcExpression.Happiness, NpcExpressionResolver.Resolve(59));
            Assert.AreEqual(NpcExpression.Love, NpcExpressionResolver.Resolve(60));
            Assert.AreEqual(NpcExpression.Love, NpcExpressionResolver.Resolve(100));
        }

        [Test]
        public void Resolve_IsMonotonicNonHostileAsOpinionRises()
        {
            int previous = (int)NpcExpressionResolver.Resolve(-100);
            for (int opinion = -100; opinion <= 100; opinion++)
            {
                int current = (int)NpcExpressionResolver.Resolve(opinion);
                Assert.GreaterOrEqual(current, previous, $"Expression regressed at opinion {opinion}");
                previous = current;
            }
        }

        [Test]
        public void LabelPtBr_AllExpressions_NonEmpty()
        {
            foreach (NpcExpression expression in Enum.GetValues(typeof(NpcExpression)))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(NpcExpressionResolver.LabelPtBr(expression)),
                    $"Missing PT-BR label for {expression}");
            }
        }
    }
}
