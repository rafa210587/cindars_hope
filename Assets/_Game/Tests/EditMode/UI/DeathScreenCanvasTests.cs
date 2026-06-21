using NUnit.Framework;
using CindarsHope.UI.Death;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class DeathScreenCanvasTests
    {
        // ---- Cave death projection ----

        [Test]
        public void CaveDeath_WithCorpse_ProjectsItemsGoldAndLevel()
        {
            var corpse = new DeathScreenViewModel.CorpseSnapshot(itemCount: 5, gold: 120, caveLevel: 3);
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 3, corpse, xpLost: 0);

            Assert.AreEqual(DeathCause.Cave, vm.Cause);
            Assert.IsTrue(vm.HasCorpse);
            Assert.AreEqual(5, vm.CorpseItemCount);
            Assert.AreEqual(120, vm.CorpseGold);
            Assert.AreEqual(3, vm.CorpseCaveLevel);
            StringAssert.Contains("nivel 3", vm.LocationLabel);
            StringAssert.Contains("nivel 3", vm.RecoveryInstruction);
        }

        [Test]
        public void CaveDeath_CorpseLevelDiffersFromDeathLevel_RecoveryUsesCorpseLevel()
        {
            var corpse = new DeathScreenViewModel.CorpseSnapshot(itemCount: 1, gold: 0, caveLevel: 7);
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 9, corpse, xpLost: 0);

            Assert.AreEqual(7, vm.CorpseCaveLevel);
            StringAssert.Contains("nivel 7", vm.RecoveryInstruction);
        }

        [Test]
        public void CaveDeath_NoCorpse_CoherentEmptyState()
        {
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 2, corpse: null, xpLost: 0);

            Assert.IsFalse(vm.HasCorpse);
            Assert.AreEqual(0, vm.CorpseItemCount);
            Assert.AreEqual(0, vm.CorpseGold);
            StringAssert.Contains("nao deixou", vm.RecoveryInstruction);
        }

        [Test]
        public void CaveDeath_EmptyCorpseSnapshot_TreatedAsNoCorpse()
        {
            var corpse = new DeathScreenViewModel.CorpseSnapshot(itemCount: 0, gold: 0, caveLevel: 4);
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 4, corpse, xpLost: 0);

            Assert.IsFalse(vm.HasCorpse, "Corpse with nothing to recover is an empty state");
        }

        [Test]
        public void CaveDeath_WithXpLost_ReportsXp()
        {
            var corpse = new DeathScreenViewModel.CorpseSnapshot(itemCount: 2, gold: 10, caveLevel: 1);
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 1, corpse, xpLost: 250);

            Assert.IsTrue(vm.HasXpLoss);
            Assert.AreEqual(250, vm.XpLost);
        }

        [Test]
        public void CaveDeath_NoXpLost_HasXpLossFalse()
        {
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 1, corpse: null, xpLost: 0);

            Assert.IsFalse(vm.HasXpLoss);
            Assert.AreEqual(0, vm.XpLost);
        }

        [Test]
        public void CaveDeath_NegativeXp_ClampedToZero()
        {
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 1, corpse: null, xpLost: -50);

            Assert.AreEqual(0, vm.XpLost);
            Assert.IsFalse(vm.HasXpLoss);
        }

        // ---- Overworld death projection ----

        [Test]
        public void OverworldDeath_WithScene_ShowsSceneAndEmptyCorpse()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("FarmScene");

            Assert.AreEqual(DeathCause.Overworld, vm.Cause);
            Assert.AreEqual("FarmScene", vm.LocationLabel);
            Assert.IsFalse(vm.HasCorpse);
            Assert.IsFalse(vm.HasXpLoss);
            StringAssert.Contains("FarmScene", vm.Headline);
        }

        [Test]
        public void OverworldDeath_NoScene_CoherentFallback()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath(null);

            Assert.AreEqual(DeathCause.Overworld, vm.Cause);
            Assert.IsFalse(vm.HasCorpse);
            Assert.IsFalse(string.IsNullOrEmpty(vm.LocationLabel));
            Assert.IsFalse(string.IsNullOrEmpty(vm.Headline));
        }

        // ---- Action contract (honest actions) ----

        [Test]
        public void Actions_RespawnEnabled_FutureDisabled()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("FarmScene");

            Assert.AreEqual(2, vm.Actions.Count);

            var respawn = vm.Actions[0];
            Assert.AreEqual(DeathScreenViewModel.RespawnActionId, respawn.ActionId);
            Assert.IsTrue(respawn.Enabled, "Respawn action must be enabled");

            var future = vm.Actions[1];
            Assert.AreEqual(DeathScreenViewModel.FutureActionId, future.ActionId);
            Assert.IsFalse(future.Enabled, "Future slot must be disabled (honest label, no second lying button)");
            Assert.IsFalse(string.IsNullOrEmpty(future.Label));
        }

        // ---- Body text composition ----

        [Test]
        public void ComposeBodyText_CaveWithCorpse_IncludesAllSections()
        {
            var corpse = new DeathScreenViewModel.CorpseSnapshot(itemCount: 4, gold: 75, caveLevel: 5);
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 5, corpse, xpLost: 100);

            var text = DeathScreenCanvasController.ComposeBodyText(vm);

            StringAssert.Contains("Local:", text);
            StringAssert.Contains("Corpo:", text);
            StringAssert.Contains("4 itens", text);
            StringAssert.Contains("75 ouro", text);
            StringAssert.Contains("Nivel do corpo: 5", text);
            StringAssert.Contains("XP perdido: 100", text);
        }

        [Test]
        public void ComposeBodyText_OverworldNoCorpse_OmitsCorpseAndXp()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("TownScene");

            var text = DeathScreenCanvasController.ComposeBodyText(vm);

            StringAssert.Contains("Local:", text);
            Assert.IsFalse(text.Contains("Corpo:"), "No corpse section in overworld empty state");
            Assert.IsFalse(text.Contains("XP perdido:"), "No XP section when nothing lost");
        }

        [Test]
        public void ComposeBodyText_NullViewModel_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DeathScreenCanvasController.ComposeBodyText(null));
        }
    }
}
