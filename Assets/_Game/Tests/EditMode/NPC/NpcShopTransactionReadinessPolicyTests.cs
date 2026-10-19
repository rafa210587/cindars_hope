using CindarsHope.NPC;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class NpcShopTransactionReadinessPolicyTests
    {
        [TestCase(false, true, true, true, "_shopData")]
        [TestCase(true, false, true, true, "_shopData.Id")]
        [TestCase(true, true, false, true, "_shopManager")]
        [TestCase(true, true, true, false, "_itemDatabase")]
        public void Evaluate_FailsMissingRequiredContextInCanonicalOrder(
            bool hasData, bool hasId, bool hasManager, bool hasDatabase, string expectedField)
        {
            var snapshot = Snapshot(hasData, hasId, hasManager, hasDatabase);

            var decision = NpcShopTransactionReadinessPolicy.Evaluate(snapshot, "_buyPanel");

            Assert.That(decision.Action, Is.EqualTo(NpcShopTransactionReadinessAction.Fail));
            Assert.That(decision.FieldName, Is.EqualTo(expectedField));
        }

        [TestCase(false, true, true, true, NpcShopTransactionReadinessAction.InitializeController)]
        [TestCase(true, false, true, true, NpcShopTransactionReadinessAction.Fail)]
        [TestCase(true, true, false, true, NpcShopTransactionReadinessAction.RecoverSession)]
        [TestCase(true, true, true, false, NpcShopTransactionReadinessAction.Fail)]
        [TestCase(true, true, true, true, NpcShopTransactionReadinessAction.Ready)]
        public void Evaluate_ChoosesNextPreparationStep(
            bool controllerReady,
            bool managerInitialized,
            bool hasSession,
            bool panelReady,
            NpcShopTransactionReadinessAction expected)
        {
            var snapshot = new NpcShopTransactionReadinessSnapshot(
                true, true, true, true,
                controllerReady, managerInitialized, hasSession, panelReady);

            var decision = NpcShopTransactionReadinessPolicy.Evaluate(snapshot, "_sellPanel");

            Assert.That(decision.Action, Is.EqualTo(expected));
            if (controllerReady && managerInitialized && hasSession && !panelReady)
                Assert.That(decision.FieldName, Is.EqualTo("_sellPanel"));
        }

        private static NpcShopTransactionReadinessSnapshot Snapshot(
            bool hasData, bool hasId, bool hasManager, bool hasDatabase)
        {
            return new NpcShopTransactionReadinessSnapshot(
                hasData, hasId, hasManager, hasDatabase,
                true, true, true, true);
        }
    }
}
