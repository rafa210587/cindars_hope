using CindarsHope.NPC;
using CindarsHope.NPC.Services;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcShopServiceChoiceBuilder.Build: mapeia NpcServiceCatalog + NpcServiceAccess
    /// para choices de dialogo, preservando as opcoes desabilitadas com o motivo (CA-4 descoberta >
    /// ocultacao) em vez de escondê-las.
    /// </summary>
    [TestFixture]
    public sealed class NpcShopServiceChoiceBuilderTests
    {
        private sealed class StubServiceContext : INpcServiceContext
        {
            public bool FriendshipOk;

            public bool FriendshipAtLeast(string npcId, int level) => FriendshipOk;
            public bool FlagIsSet(string flagId) => false;
            public int CurrentGold() => 0;
            public bool SpendGold(int amount) => false;
            public bool HasItem(string itemId, int amount) => false;
            public bool RemoveItem(string itemId, int amount) => false;
        }

        [TearDown]
        public void TearDown()
        {
            NpcServiceAccess.ResetForTests();
        }

        [Test]
        public void Build_ReturnsEmpty_ForNpcWithoutServices()
        {
            var choices = NpcShopServiceChoiceBuilder.Build("npc_no_such_provider");

            Assert.That(choices, Is.Empty);
        }

        [Test]
        public void Build_ReturnsFailClosedLabelAndServiceId_WithoutExecutorBridge()
        {
            // Sem ExecutorProvider ligado, NpcServiceAccess.BuildOptions cai no fallback fail-closed:
            // opcao desabilitada com motivo "Servico indisponivel" anexado ao rotulo base do catalogo
            // (CA-4 descoberta > ocultacao), preservando o ServiceId canonico.
            var choices = NpcShopServiceChoiceBuilder.Build("npc_thalindra");

            Assert.That(choices, Has.Count.EqualTo(1));
            Assert.That(choices[0].Label, Is.EqualTo("Analise de Criatura (Servico indisponivel)"));
            Assert.That(choices[0].ServiceId, Is.EqualTo(NpcServiceCatalog.AnalysisServiceId));
        }

        [Test]
        public void Build_ShowsEnabledLabel_WhenFriendshipGateSatisfied()
        {
            var context = new StubServiceContext { FriendshipOk = true };
            NpcServiceAccess.ExecutorProvider = () => new NpcServiceExecutor(null, context, null);

            var choices = NpcShopServiceChoiceBuilder.Build("npc_brumdar");

            Assert.That(choices, Has.Count.EqualTo(1));
            Assert.That(choices[0].Label, Is.EqualTo("Reparo com Desconto"));
            Assert.That(choices[0].ServiceId, Is.EqualTo(NpcServiceCatalog.RepairDiscountServiceId));
        }

        [Test]
        public void Build_AppendsDisabledReason_WhenFriendshipGateNotSatisfied()
        {
            var context = new StubServiceContext { FriendshipOk = false };
            NpcServiceAccess.ExecutorProvider = () => new NpcServiceExecutor(null, context, null);

            var choices = NpcShopServiceChoiceBuilder.Build("npc_brumdar");

            Assert.That(choices, Has.Count.EqualTo(1));
            Assert.That(choices[0].Label, Is.EqualTo(
                $"Reparo com Desconto (Amizade {NpcServiceCatalog.RepairDiscountFriendshipMin} necessaria)"));
        }
    }
}
