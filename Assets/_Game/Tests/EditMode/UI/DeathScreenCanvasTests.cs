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

        // ---- Action contract (revive com Lagrima da Deusa + respawn) ----

        [Test]
        public void Actions_DefaultNoTear_ReviveDisabled_RespawnEnabled()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("FarmScene");

            Assert.AreEqual(2, vm.Actions.Count);

            var revive = vm.Actions[0];
            Assert.AreEqual(DeathScreenViewModel.ReviveActionId, revive.ActionId);
            Assert.IsFalse(revive.Enabled, "Revive must be disabled when there is no goddess tear");

            var respawn = vm.Actions[1];
            Assert.AreEqual(DeathScreenViewModel.RespawnActionId, respawn.ActionId);
            Assert.IsTrue(respawn.Enabled, "Respawn at the fountain is always enabled (anti-softlock)");
        }

        [Test]
        public void WithGoddessTearCount_Positive_EnablesReviveAndShowsCount()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("FarmScene").WithGoddessTearCount(2);

            Assert.IsTrue(vm.CanReviveWithTear);
            Assert.AreEqual(2, vm.GoddessTearCount);
            StringAssert.Contains("(2)", vm.ReviveActionLabel);

            var revive = vm.Actions[0];
            Assert.AreEqual(DeathScreenViewModel.ReviveActionId, revive.ActionId);
            Assert.IsTrue(revive.Enabled, "Revive must be enabled when at least one goddess tear is present");
            StringAssert.Contains("(2)", revive.Label);
        }

        [Test]
        public void WithGoddessTearCount_Zero_KeepsReviveDisabled()
        {
            var vm = DeathScreenViewModel.ForCaveDeath(deathCaveLevel: 3, corpse: null, xpLost: 0)
                .WithGoddessTearCount(0);

            Assert.IsFalse(vm.CanReviveWithTear);
            Assert.IsFalse(vm.Actions[0].Enabled);
            Assert.IsTrue(vm.Actions[1].Enabled);
        }

        [Test]
        public void WithGoddessTearCount_Negative_ClampedToZero()
        {
            var vm = DeathScreenViewModel.ForOverworldDeath("FarmScene").WithGoddessTearCount(-3);

            Assert.AreEqual(0, vm.GoddessTearCount);
            Assert.IsFalse(vm.CanReviveWithTear);
        }

        [Test]
        public void DeathTitle_IsVoceMorreu()
        {
            Assert.AreEqual("Voce Morreu", DeathScreenViewModel.DeathTitle);
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
