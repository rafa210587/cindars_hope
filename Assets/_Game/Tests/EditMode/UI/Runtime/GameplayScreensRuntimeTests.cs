using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Quest;
using CindarsHope.UI.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI.Runtime
{
    [TestFixture]
    public class UiFocusControllerTests
    {
        private static IReadOnlyList<string> Order(params string[] ids) => ids;

        [Test]
        public void SetElements_FocusesFirst()
        {
            var c = new UiFocusController();
            c.SetElements(Order("a", "b", "c"));

            Assert.AreEqual(0, c.FocusedIndex);
            Assert.AreEqual("a", c.FocusedElementId);
            Assert.IsTrue(c.IsFocused("a"));
        }

        [Test]
        public void SetElements_Empty_HasNoFocus()
        {
            var c = new UiFocusController();
            c.SetElements(new string[0]);

            Assert.IsFalse(c.HasFocus);
            Assert.IsNull(c.FocusedElementId);
        }

        [Test]
        public void Next_AdvancesAndWraps()
        {
            var c = new UiFocusController();
            c.SetElements(Order("a", "b"));

            Assert.AreEqual(UiFocusController.FocusResult.Moved, c.Apply(UiFocusController.FocusInput.Next));
            Assert.AreEqual("b", c.FocusedElementId);

            c.Apply(UiFocusController.FocusInput.Next);
            Assert.AreEqual("a", c.FocusedElementId, "Next wraps to first");
        }

        [Test]
        public void Previous_RetreatsAndWraps()
        {
            var c = new UiFocusController();
            c.SetElements(Order("a", "b", "c"));

            c.Apply(UiFocusController.FocusInput.Previous);
            Assert.AreEqual("c", c.FocusedElementId, "Previous wraps to last");
        }

        [Test]
        public void FocusOrder_IsDeterministic()
        {
            var c = new UiFocusController();
            c.SetElements(Order("slot0", "slot1", "use", "equip", "drop"));

            var visited = new List<string> { c.FocusedElementId };
            for (var i = 0; i < 4; i++)
            {
                c.Apply(UiFocusController.FocusInput.Next);
                visited.Add(c.FocusedElementId);
            }

            CollectionAssert.AreEqual(
                new[] { "slot0", "slot1", "use", "equip", "drop" },
                visited);
        }

        [Test]
        public void Confirm_ReportsConfirmedWithoutMoving()
        {
            var c = new UiFocusController();
            c.SetElements(Order("a", "b"));

            Assert.AreEqual(UiFocusController.FocusResult.Confirmed, c.Apply(UiFocusController.FocusInput.Confirm));
            Assert.AreEqual("a", c.FocusedElementId);
        }

        [Test]
        public void Cancel_OnEmpty_ReportsCancelled()
        {
            var c = new UiFocusController();
            c.SetElements(new string[0]);

            Assert.AreEqual(UiFocusController.FocusResult.Cancelled, c.Apply(UiFocusController.FocusInput.Cancel));
        }

        [Test]
        public void FocusElement_UnknownId_DoesNotChangeFocus()
        {
            var c = new UiFocusController();
            c.SetElements(Order("a", "b"));

            Assert.IsFalse(c.FocusElement("zzz"));
            Assert.AreEqual("a", c.FocusedElementId);
        }
    }

    [TestFixture]
    public class ModalPauseGateTests
    {
        [Test]
        public void OpeningModal_FreezesClock()
        {
            var gate = new ModalPauseGate();
            Assert.AreEqual(PauseTransition.Freeze, gate.Evaluate(true));
            Assert.IsTrue(gate.IsClockFrozen);
        }

        [Test]
        public void EmptyStack_ResumesClock()
        {
            var gate = new ModalPauseGate();
            gate.Evaluate(true);
            Assert.AreEqual(PauseTransition.Resume, gate.Evaluate(false));
            Assert.IsFalse(gate.IsClockFrozen);
        }

        [Test]
        public void NoChange_EmitsNoTransition()
        {
            var gate = new ModalPauseGate();
            gate.Evaluate(true);
            Assert.AreEqual(PauseTransition.None, gate.Evaluate(true));
            Assert.AreEqual(PauseTransition.None, gate.Evaluate(true));
        }

        [Test]
        public void StaysRunningWhenNeverOpened()
        {
            var gate = new ModalPauseGate();
            Assert.AreEqual(PauseTransition.None, gate.Evaluate(false));
            Assert.IsFalse(gate.IsClockFrozen);
        }
    }

    [TestFixture]
    public class GameplayScreensPanelModelTests
    {
        [Test]
        public void Open_SetsTabAndOpenFlag()
        {
            var m = new GameplayScreensPanelModel();
            Assert.IsTrue(m.Open(GameplayScreenTab.Quests));
            Assert.IsTrue(m.IsOpen);
            Assert.AreEqual(GameplayScreenTab.Quests, m.ActiveTab);
        }

        [Test]
        public void Open_SameTabAgain_IsNoOp()
        {
            var m = new GameplayScreensPanelModel();
            m.Open(GameplayScreenTab.Inventory);
            Assert.IsFalse(m.Open(GameplayScreenTab.Inventory));
        }

        [Test]
        public void Open_DifferentTabWhileOpen_SwitchesTab()
        {
            var m = new GameplayScreensPanelModel();
            m.Open(GameplayScreenTab.Inventory);
            Assert.IsTrue(m.Open(GameplayScreenTab.Skills));
            Assert.AreEqual(GameplayScreenTab.Skills, m.ActiveTab);
        }

        [Test]
        public void CycleNext_WrapsAcrossNineTabs()
        {
            var m = new GameplayScreensPanelModel();
            m.Open(GameplayScreenTab.System);
            m.CycleNext();
            Assert.AreEqual(GameplayScreenTab.Inventory, m.ActiveTab);
        }

        [Test]
        public void CyclePrevious_WrapsToSystem()
        {
            var m = new GameplayScreensPanelModel();
            m.Open(GameplayScreenTab.Inventory);
            m.CyclePrevious();
            Assert.AreEqual(GameplayScreenTab.System, m.ActiveTab);
        }

        [Test]
        public void Cycle_WhenClosed_IsIgnored()
        {
            var m = new GameplayScreensPanelModel();
            m.CycleNext();
            Assert.IsFalse(m.IsOpen);
            Assert.AreEqual(GameplayScreenTab.Inventory, m.ActiveTab);
        }

        [Test]
        public void Close_ReturnsPreviousOpenState()
        {
            var m = new GameplayScreensPanelModel();
            m.Open(GameplayScreenTab.Inventory);
            Assert.IsTrue(m.Close());
            Assert.IsFalse(m.IsOpen);
            Assert.IsFalse(m.Close());
        }

        [Test]
        public void CoreTabs_AreNotPlaceholders()
        {
            Assert.IsFalse(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Inventory));
            Assert.IsFalse(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Equipment));
            Assert.IsFalse(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Skills));
            Assert.IsFalse(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Quests));
        }

        [Test]
        public void FutureTabs_ArePlaceholders()
        {
            Assert.IsTrue(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Social));
            Assert.IsTrue(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Bestiary));
            Assert.IsTrue(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Calendar));
            Assert.IsTrue(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.Map));
            Assert.IsTrue(GameplayScreensPanelModel.IsPlaceholderTab(GameplayScreenTab.System));
        }

        [Test]
        public void ModalTypeMapping_RoundTripsForCoreTabs()
        {
            AssertRoundTrip(ModalType.Inventory, GameplayScreenTab.Inventory);
            AssertRoundTrip(ModalType.CharacterEquipment, GameplayScreenTab.Equipment);
            AssertRoundTrip(ModalType.SkillTree, GameplayScreenTab.Skills);
            AssertRoundTrip(ModalType.QuestLog, GameplayScreenTab.Quests);
        }

        [Test]
        public void TryGetTabForModalType_UnknownReturnsFalse()
        {
            Assert.IsFalse(GameplayScreensPanelModel.TryGetTabForModalType(ModalType.ShopMenu, out _));
        }

        private static void AssertRoundTrip(ModalType modalType, GameplayScreenTab expectedTab)
        {
            Assert.IsTrue(GameplayScreensPanelModel.TryGetTabForModalType(modalType, out var tab));
            Assert.AreEqual(expectedTab, tab);
            Assert.AreEqual(modalType, GameplayScreensPanelModel.GetModalTypeForTab(expectedTab));
        }
    }

    [TestFixture]
    public class SkillNodePurchaseFlowTests
    {
        [Test]
        public void SelectNode_OpensDetailNeverPurchases()
        {
            var f = new SkillNodePurchaseFlow();
            f.SelectNode("node_a");
            Assert.AreEqual(SkillNodePurchaseFlow.Stage.Detail, f.CurrentStage);
            Assert.IsFalse(f.CanPurchaseNow);
        }

        [Test]
        public void PurchaseRequiresDetailThenConfirm()
        {
            var f = new SkillNodePurchaseFlow();
            f.SelectNode("node_a");

            // First confirm: opens confirmation, does NOT purchase.
            Assert.IsFalse(f.Confirm());
            Assert.AreEqual(SkillNodePurchaseFlow.Stage.Confirm, f.CurrentStage);
            Assert.IsTrue(f.CanPurchaseNow);

            // Second confirm: authorizes purchase.
            Assert.IsTrue(f.Confirm());
            Assert.AreEqual(SkillNodePurchaseFlow.Stage.Browsing, f.CurrentStage);
        }

        [Test]
        public void Confirm_WhileBrowsing_NeverPurchases()
        {
            var f = new SkillNodePurchaseFlow();
            Assert.IsFalse(f.Confirm());
            Assert.IsFalse(f.CanPurchaseNow);
        }

        [Test]
        public void Cancel_StepsBackTowardBrowsing()
        {
            var f = new SkillNodePurchaseFlow();
            f.SelectNode("node_a");
            f.Confirm(); // -> Confirm stage
            f.Cancel();
            Assert.AreEqual(SkillNodePurchaseFlow.Stage.Detail, f.CurrentStage);
            f.Cancel();
            Assert.AreEqual(SkillNodePurchaseFlow.Stage.Browsing, f.CurrentStage);
        }
    }

    [TestFixture]
    public class QuestLogSpoilerProjectionTests
    {
        private static QuestLogViewModel BuildVm(params QuestLogEntryViewModel[] active)
        {
            var vm = new QuestLogViewModel();
            vm.ActiveQuests.AddRange(active);
            return vm;
        }

        [Test]
        public void NoActiveQuests_YieldsEmptyRows()
        {
            var vm = BuildVm();
            Assert.IsFalse(QuestLogSpoilerProjection.HasActiveQuests(vm));
            Assert.AreEqual(0, QuestLogSpoilerProjection.BuildVisibleRows(vm).Count);
        }

        [Test]
        public void VisibleQuest_ShowsRealNameAndProgress()
        {
            var vm = BuildVm(new QuestLogEntryViewModel
            {
                QuestId = "q1",
                DisplayName = "Coletar Madeira",
                ObjectiveProgress = 1,
                ObjectiveTarget = 2,
                IsHidden = false
            });

            var rows = QuestLogSpoilerProjection.BuildVisibleRows(vm);
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Coletar Madeira", rows[0].DisplayName);
            Assert.AreEqual("1/2", rows[0].ObjectiveLabel);
        }

        [Test]
        public void HiddenQuest_MasksNameAndObjective()
        {
            var vm = BuildVm(new QuestLogEntryViewModel
            {
                QuestId = "secret",
                DisplayName = "O Eco da Caverna",
                ObjectiveProgress = 3,
                ObjectiveTarget = 5,
                IsHidden = true
            });

            var rows = QuestLogSpoilerProjection.BuildVisibleRows(vm);
            Assert.AreEqual(QuestLogSpoilerProjection.HiddenObjectiveLabel, rows[0].DisplayName);
            Assert.AreEqual(QuestLogSpoilerProjection.HiddenObjectiveLabel, rows[0].ObjectiveLabel);
            Assert.IsFalse(rows[0].IsComplete, "Hidden quest never reports complete");
        }
    }
}
