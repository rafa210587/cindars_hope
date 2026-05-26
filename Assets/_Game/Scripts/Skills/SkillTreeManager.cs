using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Progression;
using UnityEngine;

namespace CindarsHope.Skills
{
    [DisallowMultipleComponent]
    public class SkillTreeManager : MonoBehaviour
    {
        [SerializeField] private SkillTreeRegistrySO _treeRegistry;
        [SerializeField] private SkillNodeDatabaseSO _nodeDatabase;
        [SerializeField] private int _respecCostGold = 250;
        [SerializeField] private PlayerProgressionManager _progressionManager;

        private SkillTreeState _state;
        private SkillPurchaseService _purchaseService;
        private SkillRespecService _respecService;
        private SkillPassiveApplicator _passiveApplicator;

        private Dictionary<string, SkillTreeDataSO> _treeIndex = new Dictionary<string, SkillTreeDataSO>();
        private Dictionary<string, SkillNodeDataSO> _nodeIndex = new Dictionary<string, SkillNodeDataSO>();

        public SkillTreeState State => _state;
        public IReadOnlyDictionary<string, SkillNodeDataSO> NodeIndex => _nodeIndex;
        public IReadOnlyDictionary<string, SkillTreeDataSO> TreeIndex => _treeIndex;

        public void RebindProgressionManager(PlayerProgressionManager progressionManager)
        {
            _progressionManager = progressionManager;
            if (_progressionManager != null && _state != null)
            {
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
            }
        }

        private void Awake()
        {
            _state = new SkillTreeState();
            BuildCatalog();

            _purchaseService = new SkillPurchaseService(_nodeIndex.Values);
            _respecService = new SkillRespecService(_respecCostGold);
            _passiveApplicator = new SkillPassiveApplicator(_nodeIndex);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
        }

        private void BuildCatalog()
        {
            // Use inspector-assigned registry if available; otherwise use default code catalog.
            if (_treeRegistry != null && _nodeDatabase != null)
            {
                foreach (var tree in _treeRegistry.All)
                    if (tree != null && !string.IsNullOrEmpty(tree.TreeId))
                        _treeIndex[tree.TreeId] = tree;

                foreach (var node in _nodeDatabase.All)
                    if (node != null && !string.IsNullOrEmpty(node.SkillNodeId))
                        _nodeIndex[node.SkillNodeId] = node;
            }
            else
            {
                var nodes = DefaultSkillCatalog.BuildAllNodes();
                foreach (var node in nodes)
                    _nodeIndex[node.SkillNodeId] = node;

                var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
                foreach (var tree in trees)
                    _treeIndex[tree.TreeId] = tree;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public bool TryPurchaseNode(string nodeId, int playerLevel)
        {
            return TryPurchaseNode(nodeId, playerLevel, out _);
        }

        public bool TryPurchaseNode(string nodeId, int playerLevel, out string feedback)
        {
            feedback = string.Empty;
            if (_progressionManager != null)
            {
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
            }

            bool ok = _purchaseService.TryPurchase(nodeId, _state, playerLevel, out feedback);
            if (ok)
            {
                if (_progressionManager != null && !_progressionManager.TrySpendSkillPoints(1))
                {
                    feedback = "Skill points changed before purchase could complete.";
                    return false;
                }

                if (_nodeIndex.TryGetValue(nodeId, out var node))
                {
                    _passiveApplicator.Apply(node, _state);
                    feedback = "Skill comprada.";
                    if (node.SkillCategory == SkillCategory.EquippableSkill
                        && !string.IsNullOrWhiteSpace(node.UnlockedSkillActionId))
                    {
                        feedback = AutoAssignActiveSkill(node.UnlockedSkillActionId);
                    }
                }

                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            }
            return ok;
        }

        public bool TryAssignActiveSlot(int slotIndex, string skillActionId)
        {
            if (!_nodeIndex.TryGetValue(skillActionId, out _))
            {
                // Validate the skillActionId belongs to an unlocked EquippableSkill
                bool isUnlocked = false;
                foreach (var node in _nodeIndex.Values)
                {
                    if (node.UnlockedSkillActionId == skillActionId
                        && node.SkillCategory == SkillCategory.EquippableSkill
                        && _state.IsPurchased(node.SkillNodeId))
                    {
                        isUnlocked = true;
                        break;
                    }
                }

                if (!isUnlocked)
                {
                    Debug.LogWarning($"SkillTreeManager: SkillActionId '{skillActionId}' not unlocked.");
                    return false;
                }
            }

            if (!_state.AssignActiveSlot(slotIndex, skillActionId)) return false;
            GameEventBus.Publish(new ActiveSkillSlotAssignedEvent(slotIndex, skillActionId));
            return true;
        }

        public bool TryClearActiveSlot(int slotIndex)
        {
            if (!_state.ClearActiveSlot(slotIndex)) return false;
            GameEventBus.Publish(new ActiveSkillSlotClearedEvent(slotIndex));
            return true;
        }

        public bool TryRespec(ref int gold, int playerLevel)
        {
            bool ok = _respecService.TryRespec(_state, playerLevel, ref gold);
            if (ok)
                _passiveApplicator.Reset();
            return ok;
        }

        public int GetRespecCost() => _respecService.GetRespecCost(_state);

        public bool IsNodePurchased(string nodeId) => _state.IsPurchased(nodeId);

        public bool IsNodeUnlocked(string nodeId)
        {
            if (!_nodeIndex.TryGetValue(nodeId, out var node)) return false;
            if (node.SkillCategory == SkillCategory.PassiveSkill) return _state.IsPurchased(nodeId);
            return _state.IsPurchased(nodeId);
        }

        public List<SkillPassiveModifier> GetAllActivePassiveModifiers()
            => _passiveApplicator.GetAllActive();

        private string AutoAssignActiveSkill(string skillActionId)
        {
            string[] keys = { "R", "T", "Y", "G" };
            for (var index = 0; index < keys.Length; index++)
            {
                if (!string.IsNullOrEmpty(_state.GetActiveSlotSkillActionId(index)))
                {
                    continue;
                }

                if (TryAssignActiveSlot(index, skillActionId))
                {
                    return $"Skill ativa alocada em {keys[index]}.";
                }
            }

            return "Skill comprada. Slots ativos cheios; escolha manualmente depois.";
        }

        // ── Save / Load ────────────────────────────────────────────────────────

        public SkillTreeSaveData CaptureSaveData()
        {
            return _state.ToSaveData(i => i switch
            {
                0 => "R", 1 => "T", 2 => "Y", 3 => "G", _ => string.Empty
            });
        }

        public void RestoreFromSaveData(SkillTreeSaveData data, int playerLevel)
        {
            if (data == null) return;
            int totalPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(playerLevel);
            _state.LoadFromSaveData(data, totalPoints);
            _passiveApplicator.Reset();

            foreach (var nodeId in _state.PurchasedNodeIds)
            {
                if (_nodeIndex.TryGetValue(nodeId, out var node))
                    _passiveApplicator.Apply(node, _state);
            }

            ValidateActiveSlots();
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
        }

        private void ValidateActiveSlots()
        {
            for (int i = 0; i < 4; i++)
            {
                var slotActionId = _state.GetActiveSlotSkillActionId(i);
                if (string.IsNullOrEmpty(slotActionId)) continue;

                bool valid = false;
                foreach (var node in _nodeIndex.Values)
                {
                    if (node.UnlockedSkillActionId == slotActionId
                        && node.SkillCategory == SkillCategory.EquippableSkill
                        && _state.IsPurchased(node.SkillNodeId))
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                {
                    Debug.LogWarning($"SkillTreeManager: Slot {i} has invalid skill '{slotActionId}', clearing.");
                    _state.ClearActiveSlot(i);
                    GameEventBus.Publish(new ActiveSkillSlotClearedEvent(i));
                }
            }
        }

        private void OnPlayerLevelChanged(PlayerLevelChangedEvent evt)
        {
            if (evt.GrantedSkillPoints > 0)
            {
                _state.AddSkillPoints(evt.GrantedSkillPoints);
                GameEventBus.Publish(new SkillPointGrantedEvent(
                    evt.GrantedSkillPoints,
                    _state.AvailableSkillPoints,
                    evt.NewLevel));
            }
        }
    }
}
