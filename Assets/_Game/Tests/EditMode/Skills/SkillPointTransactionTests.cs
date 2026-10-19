using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Progression;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Core.Time;
using CindarsHope.Core.Data;
using System.Reflection;
using CindarsHope.Skills;
using CindarsHope.UI.Skills;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillPointTransactionTests
    {
        private GameObject _host;
        private PlayerProgressionManager _progression;
        private SkillTreeManager _skills;
        private readonly List<Object> _ownedData = new List<Object>();

        [SetUp] public void SetUp()
        {
            _host = new GameObject("Skill transaction fixture");
            _progression = _host.AddComponent<PlayerProgressionManager>();
            _skills = _host.AddComponent<SkillTreeManager>();
            var nodes = DefaultSkillCatalog.BuildAllNodes();
            var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
            var actions = DefaultSkillActionCatalog.BuildAll();
            _ownedData.AddRange(nodes);
            _ownedData.AddRange(trees);
            _ownedData.AddRange(actions);
            SetField(_skills, "_nodeDatabase", Registry<SkillNodeDatabaseSO, SkillNodeDataSO>(nodes.ToArray()));
            SetField(_skills, "_treeRegistry", Registry<SkillTreeRegistrySO, SkillTreeDataSO>(trees.ToArray()));
            SetField(_skills, "_actionDatabase", Registry<SkillActionDatabaseSO, SkillActionSO>(actions.ToArray()));
            // EditMode does not invoke the MonoBehaviour scene lifecycle automatically.
            typeof(SkillTreeManager).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(_skills, null);
            _progression.GrantSkillPoints(15);
            _skills.RebindProgressionManager(_progression);
        }

        [TearDown] public void TearDown()
        {
            Object.DestroyImmediate(_host);
            for (int i = _ownedData.Count - 1; i >= 0; i--)
                if (_ownedData[i] != null) Object.DestroyImmediate(_ownedData[i]);
            _ownedData.Clear();
        }

        private TRegistry Registry<TRegistry, TItem>(params TItem[] values)
            where TRegistry : DataRegistrySO<TItem>
            where TItem : ScriptableObject, IIdentifiedData
        {
            var registry = ScriptableObject.CreateInstance<TRegistry>();
            _ownedData.Add(registry);
            typeof(DataRegistrySO<TItem>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(registry, values);
            typeof(DataRegistrySO<TItem>).GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(registry, null);
            return registry;
        }

        [Test] public void Purchase_EventSeesCoherentBalances_AndRejectsReentrancy()
        {
            int observedLedger = -1, observedTree = -1, events = 0;
            bool reentrant = true;
            Action<SkillNodePurchasedEvent> handler = e =>
            {
                events++;
                observedLedger = _progression.UnspentSkillPoints;
                observedTree = _skills.State.AvailableSkillPoints;
                reentrant = _skills.TryPurchaseNode("magic_mana_well", 100);
            };
            GameEventBus.Subscribe(handler);
            try { Assert.That(_skills.TryPurchaseNode("melee_iron_grip", 100), Is.True); }
            finally { GameEventBus.Unsubscribe(handler); }
            Assert.That(events, Is.EqualTo(1));
            Assert.That(observedLedger, Is.EqualTo(14));
            Assert.That(observedTree, Is.EqualTo(observedLedger));
            Assert.That(reentrant, Is.False);
            Assert.That(_skills.IsNodePurchased("magic_mana_well"), Is.False);
        }

        [Test] public void Purchase_NonUnitCostAndRank_RoundTripPreservesActualSpend()
        {
            _skills.NodeIndex["melee_iron_grip"].SkillPointCost = 2;
            Assert.That(_skills.TryPurchaseNode("melee_iron_grip", 100), Is.True);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(13));
            Assert.That(_skills.TryRankUpNode("melee_iron_grip", out _), Is.True);
            var progressionSave = _progression.CaptureSaveData();
            var skillSave = _skills.CaptureSaveData();
            _progression.RestoreFromSaveData(progressionSave);
            _skills.RestoreFromSaveData(skillSave, 100);
            Assert.That(_skills.State.SpentSkillPoints, Is.EqualTo(3));
            int gold = 0;
            Assert.That(_skills.TryRespec(ref gold, 100), Is.True);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(15));
        }

        [Test] public void FailedPurchase_DoesNotMutateOrPublishSuccess()
        {
            string before = JsonUtility.ToJson(_skills.CaptureSaveData());
            int success = 0;
            Action<SkillNodePurchasedEvent> handler = e => success++;
            GameEventBus.Subscribe(handler);
            try { Assert.That(_skills.TryPurchaseNode("melee_capstone_battle_rhythm", 100), Is.False); }
            finally { GameEventBus.Unsubscribe(handler); }
            Assert.That(JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(15));
            Assert.That(success, Is.Zero);
        }

        [Test] public void DormantPackOrder_NewPurchaseRejectsWithoutSpendOrSuccessEvent()
        {
            string before = JsonUtility.ToJson(_skills.CaptureSaveData());
            int pointsBefore = _progression.UnspentSkillPoints;
            int successEvents = 0;
            Action<SkillNodePurchasedEvent> handler = _ => successEvents++;
            GameEventBus.Subscribe(handler);
            try
            {
                Assert.That(_skills.TryPurchaseNode("crafting_pack_order", 100, out var reason), Is.False);
                Assert.That(reason, Is.EqualTo(SkillPurchaseService.NotYetExecutableFailureReason));
            }
            finally { GameEventBus.Unsubscribe(handler); }

            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(pointsBefore));
            Assert.That(JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
            Assert.That(successEvents, Is.Zero);
        }

        [Test] public void DormantPackOrder_LegacyOwnedSaveIsPreservedButRankUpRejectsAtomically()
        {
            var legacyOwned = new SkillTreeSaveData
            {
                Version = 3,
                NodeRanks = new List<SkillNodeRankEntry>
                {
                    new SkillNodeRankEntry("crafting_pack_order", 1) { SpentPoints = 1 }
                }
            };
            _skills.RestoreFromSaveData(legacyOwned, 100);

            Assert.That(_skills.IsNodePurchased("crafting_pack_order"), Is.True);
            string before = JsonUtility.ToJson(_skills.CaptureSaveData());
            int pointsBefore = _progression.UnspentSkillPoints;
            int successEvents = 0;
            Action<SkillNodePurchasedEvent> handler = _ => successEvents++;
            GameEventBus.Subscribe(handler);
            try
            {
                Assert.That(_skills.TryRankUpNode("crafting_pack_order", out var reason), Is.False);
                Assert.That(reason, Is.EqualTo(SkillPurchaseService.NotYetExecutableFailureReason));
            }
            finally { GameEventBus.Unsubscribe(handler); }

            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(pointsBefore));
            Assert.That(JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
            Assert.That(successEvents, Is.Zero);
        }

        [Test] public void DormantNode_ImGuiPurchaseButtonIsNeverEnabled()
        {
            var node = _skills.NodeIndex["crafting_pack_order"];
            Assert.That(node.NotYetExecutable, Is.True);
            Assert.That(SkillTreeGameplayPanelController.CanOfferPurchase(
                node, purchased: false, requirementsMet: true, hasPoints: true), Is.False);
        }

        [Test] public void Respec_PreservesTwelveSpentPlusThreeFree_ThenNextPurchase()
        {
            _skills.NodeIndex["melee_iron_grip"].SkillPointCost = 12;
            Assert.That(_skills.TryPurchaseNode("melee_iron_grip", 100), Is.True);
            int gold = 0;
            Assert.That(_skills.TryRespec(ref gold, 1), Is.True);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(15));
            Assert.That(_skills.State.AvailableSkillPoints, Is.EqualTo(15));
            Assert.That(_skills.TryPurchaseNode("magic_mana_well", 100), Is.True);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(14));
        }

        [Test] public void PaidRespec_InsufficientGoldPreservesEverything()
        {
            int gold = 249;
            Assert.That(_skills.TryRespec(ref gold, 1), Is.True);
            _skills.TryPurchaseNode("melee_iron_grip", 100);
            string before = JsonUtility.ToJson(_skills.CaptureSaveData());
            Assert.That(_skills.TryRespec(ref gold, 100), Is.False);
            Assert.That(gold, Is.EqualTo(249));
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(14));
            Assert.That(JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
            gold = 250;
            Assert.That(_skills.TryRespec(ref gold, 100), Is.True);
            Assert.That(gold, Is.Zero);
        }

        [Test] public void RepeatedCompleteRestore_UnknownNodeRefund_DoesNotMutateInputs()
        {
            var progression = new PlayerProgressionSaveData { UnspentSkillPoints = 3 };
            var skills = new SkillTreeSaveData
            {
                Version = 2,
                NodeRanks = new List<SkillNodeRankEntry>
                {
                    new SkillNodeRankEntry("obsolete_skill", 2),
                    new SkillNodeRankEntry("obsolete_skill", 2),
                    new SkillNodeRankEntry("melee_iron_grip", 1)
                }
            };
            string before = JsonUtility.ToJson(progression) + JsonUtility.ToJson(skills);
            for (int i = 0; i < 2; i++)
            {
                _progression.RestoreFromSaveData(progression);
                _skills.RestoreFromSaveData(skills, 1);
                Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(5));
                Assert.That(_skills.State.AvailableSkillPoints, Is.EqualTo(5));
                Assert.That(_skills.State.SpentSkillPoints, Is.EqualTo(1));
            }
            Assert.That(JsonUtility.ToJson(progression) + JsonUtility.ToJson(skills), Is.EqualTo(before));
            var normalizedProgression = _progression.CaptureSaveData();
            var normalizedSkills = _skills.CaptureSaveData();
            _progression.RestoreFromSaveData(normalizedProgression);
            _skills.RestoreFromSaveData(normalizedSkills, 1);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(5));
        }

        [Test] public void LegacyNullCollectionsAndExcessiveRank_NormalizeWithoutOverflow()
        {
            var state = new SkillTreeState();
            state.LoadFromSaveData(new SkillTreeSaveData { PurchasedNodeIds = null, NodeRanks = null, ActiveSkillSlots = null }, 5);
            Assert.That(state.AvailableSkillPoints, Is.EqualTo(5));
            state.LoadFromSaveData(new SkillTreeSaveData { Version = 2, NodeRanks = new List<SkillNodeRankEntry> { new SkillNodeRankEntry("old", int.MaxValue) } }, 10);
            Assert.That(state.SpentSkillPoints, Is.EqualTo(SkillTierRules.AbsoluteRankCap));
        }

        [Test] public void PaidRespec_CompletedEventObservesCommittedWalletAndLedger()
        {
            var player = _host.AddComponent<PlayerManager>();
            player.SetGold(300);
            int gold = player.CurrentGold;
            Assert.That(_skills.TryRespec(ref gold, 1, player.SetGold), Is.True);
            Assert.That(_skills.TryPurchaseNode("melee_iron_grip", 100), Is.True);
            int seenGold = -1, seenPoints = -1, events = 0;
            Action<SkillTreeRespecCompletedEvent> handler = e =>
            { events++; seenGold = player.CurrentGold; seenPoints = _progression.UnspentSkillPoints; };
            GameEventBus.Subscribe(handler);
            try { Assert.That(_skills.TryRespec(ref gold, 1, player.SetGold), Is.True); }
            finally { GameEventBus.Unsubscribe(handler); }
            Assert.That(events, Is.EqualTo(1));
            Assert.That(seenGold, Is.EqualTo(50));
            Assert.That(seenPoints, Is.EqualTo(15));
        }

        [Test] public void ValidationQueries_DoNotMutateOrPublish()
        {
            var service = new SkillPurchaseService(_skills.NodeIndex.Values);
            string before = JsonUtility.ToJson(_skills.CaptureSaveData());
            int events = 0;
            Action<SkillNodePurchasedEvent> purchased = e => events++;
            Action<SkillPurchaseFailedEvent> failed = e => events++;
            GameEventBus.Subscribe(purchased); GameEventBus.Subscribe(failed);
            try
            {
                Assert.That(service.ValidatePurchase("melee_iron_grip", _skills.State, 100, null, out _), Is.True);
                Assert.That(service.ValidatePurchase("missing", _skills.State, 100, null, out _), Is.False);
            }
            finally { GameEventBus.Unsubscribe(purchased); GameEventBus.Unsubscribe(failed); }
            Assert.That(events, Is.Zero);
            Assert.That(JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
        }

        [Test] public void V3UnknownRefund_UsesLastActualCost_AndNormalizesNegativeRespec()
        {
            var data = new SkillTreeSaveData { RespecCount = -10 };
            data.NodeRanks.Add(new SkillNodeRankEntry("obsolete", 1) { SpentPoints = 2 });
            data.NodeRanks.Add(new SkillNodeRankEntry("obsolete", 2) { SpentPoints = 7 });
            _skills.RestoreFromSaveData(data, 1);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(22));
            Assert.That(_skills.State.SpentSkillPoints, Is.Zero);
            Assert.That(_skills.State.RespecCount, Is.Zero);
        }

        [TestCase(true)] [TestCase(false)]
        public void CompleteSave_InvalidSkillsRejectBeforeAnyRestore(bool futureVersion)
        {
            var save = _host.AddComponent<SaveManager>();
            var time = _host.AddComponent<TimeManager>(); time.SetCurrentDay(7);
            var player = _host.AddComponent<PlayerManager>(); player.SetGold(123);
            SetField(save, "_timeManager", time);
            SetField(save, "_progressionManager", _progression);
            SetField(save, "_progressionProvider", new ProgressionSectionProvider(_progression));
            SetField(save, "_playerProvider", new PlayerSectionProvider(player, null, null, () => Vector2.zero));
            var input = new GameSaveData { CurrentDay = 99, Progression = new PlayerProgressionSaveData { UnspentSkillPoints = 100 } };
            input.SkillTree = new SkillTreeSaveData { Version = futureVersion ? 4 : 3 };
            if (!futureVersion) input.SkillTree.NodeRanks.Add(new SkillNodeRankEntry("old", 1) { SpentPoints = int.MaxValue });
            var before = JsonUtility.ToJson(_progression.CaptureSaveData()) + JsonUtility.ToJson(_skills.CaptureSaveData());
            var exception = Assert.Throws<TargetInvocationException>(() =>
                typeof(SaveManager).GetMethod("ApplySaveData", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { input }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(time.CurrentDay, Is.EqualTo(7), "Even the first restored section must remain unchanged.");
            Assert.That(player.CurrentGold, Is.EqualTo(123));
            Assert.That(JsonUtility.ToJson(_progression.CaptureSaveData()) + JsonUtility.ToJson(_skills.CaptureSaveData()), Is.EqualTo(before));
        }

        private static void SetField(object target, string name, object value)
            => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
