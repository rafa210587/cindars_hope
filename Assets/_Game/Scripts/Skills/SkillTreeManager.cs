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
        // arch: Core|Skills (spec_arch_core_skills_cycle_reduction_v34) — self-registro estatico
        // (molde AudioManager/CraftingManager/ShopManager) para o GameBootstrap parar de segurar
        // esta referencia serializada.
        public static SkillTreeManager Instance { get; private set; }

        [SerializeField] private SkillTreeRegistrySO _treeRegistry;
        [SerializeField] private SkillNodeDatabaseSO _nodeDatabase;
        [SerializeField] private int _respecCostGold = 250;
        [SerializeField] private PlayerProgressionManager _progressionManager;

        private SkillTreeState _state;
        private SkillPurchaseService _purchaseService;
        private SkillRespecService _respecService;
        // fable_29: the aggregator replaces the old per-node applicator as the single recompute
        // authority (DerivedStats provider + named hooks + equip gate). Rank-scaled.
        private SkillEffectAggregator _aggregator;

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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _state = new SkillTreeState();
            BuildCatalog();

            _purchaseService = new SkillPurchaseService(_nodeIndex.Values);
            _respecService = new SkillRespecService(_respecCostGold);
            _aggregator = new SkillEffectAggregator(_nodeIndex);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
            => TryPurchaseNode(nodeId, playerLevel, null, out feedback);

        // fable_29 — purchase rank 1 of a node. For exclusive-capstone-variant nodes the caller
        // must pass `chosenVariant` (the UI must have shown confirmation; see ConfirmsCapstone).
        public bool TryPurchaseNode(string nodeId, int playerLevel, string chosenVariant, out string feedback)
        {
            feedback = string.Empty;
            if (_progressionManager != null)
            {
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
            }

            bool ok = _purchaseService.TryPurchase(nodeId, _state, playerLevel, chosenVariant, out feedback);
            if (ok)
            {
                if (_progressionManager != null && !_progressionManager.TrySpendSkillPoints(1))
                {
                    feedback = "Skill points changed before purchase could complete.";
                    return false;
                }

                _aggregator.Recompute(_state);

                if (_nodeIndex.TryGetValue(nodeId, out var node))
                {
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

        // fable_29 — rank up an already-purchased node (rank N -> N+1), bounded by the dynamic cap.
        public bool TryRankUpNode(string nodeId, out string feedback)
        {
            feedback = string.Empty;
            if (_progressionManager != null)
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);

            bool ok = _purchaseService.TryRankUp(nodeId, _state, out feedback);
            if (ok)
            {
                if (_progressionManager != null && !_progressionManager.TrySpendSkillPoints(1))
                {
                    feedback = "Skill points changed before rank-up could complete.";
                    return false;
                }
                _aggregator.Recompute(_state);
                feedback = "Rank aumentado.";
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            }
            return ok;
        }

        // fable_29 (CA-3) — does this node require an exclusive capstone-variant confirmation?
        public bool RequiresCapstoneConfirmation(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node)
               && node.CapstoneVariants != null && node.CapstoneVariants.Count > 0;

        public IReadOnlyList<string> GetCapstoneVariants(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node) ? node.CapstoneVariants : null;

        public int GetRank(string nodeId) => _state.GetRank(nodeId);
        public int GetDynamicRankCap(string treeId) => _state.DynamicRankCap(treeId);
        public int GetPointsSpentInTree(string treeId) => _state.PointsSpentInTree(treeId);

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

        // fable_29 (emenda V3 item 6) — punitive respec: refunds points AND recomputes the
        // equip-revalidation gate so items gated by re-locked tiers become non-equippable.
        public bool TryRespec(ref int gold, int playerLevel)
        {
            bool ok = _respecService.TryRespec(_state, playerLevel, ref gold);
            if (ok)
            {
                _aggregator.Recompute(_state); // recomputes hooks + equip gate from the empty tree
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            }
            return ok;
        }

        public int GetRespecCost() => _respecService.GetRespecCost(_state);

        public bool IsNodePurchased(string nodeId) => _state.IsPurchased(nodeId);

        public bool IsNodeUnlocked(string nodeId)
        {
            if (!_nodeIndex.TryGetValue(nodeId, out _)) return false;
            return _state.IsPurchased(nodeId);
        }

        // Single source of truth for the DerivedStats provider (rank-scaled, via aggregator).
        public List<SkillPassiveModifier> GetAllActivePassiveModifiers()
            => _aggregator != null ? _aggregator.ActiveModifiers : new List<SkillPassiveModifier>();

        private string AutoAssignActiveSkill(string skillActionId)
        {
            // fable_29 (emenda V3 item 7): the live slot path is ActiveSkillExecutionController
            // keys 1-4. The legacy ActiveSkillSlots (R/T/Y/G) is retired; slot indices are 0-3.
            string[] keys = { "1", "2", "3", "4" };
            for (var index = 0; index < keys.Length; index++)
            {
                if (!string.IsNullOrEmpty(_state.GetActiveSlotSkillActionId(index)))
                {
                    continue;
                }

                if (TryAssignActiveSlot(index, skillActionId))
                {
                    return $"Skill ativa alocada na tecla {keys[index]}.";
                }
            }

            return "Skill comprada. Slots ativos cheios; escolha manualmente depois.";
        }

        // ── Save / Load ────────────────────────────────────────────────────────

        public SkillTreeSaveData CaptureSaveData()
        {
            // fable_29 (emenda V3 item 7): slot input keys are 1-4 (ActiveSkillExecutionController).
            return _state.ToSaveData(i => i switch
            {
                0 => "1", 1 => "2", 2 => "3", 3 => "4", _ => string.Empty
            });
        }

        public void RestoreFromSaveData(SkillTreeSaveData data, int playerLevel)
        {
            if (data == null) return;
            int totalPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(playerLevel);

            // Load (ranks + variants), resolving each node's tree for per-tree point totals.
            _state.LoadFromSaveData(data, totalPoints, ResolveNodeTreeId);

            // fable_29 migration: refund any saved node id that no longer exists in the catalog.
            // Points return to the pool (lossless); each refund is logged.
            MigrateUnknownNodes(data);

            _aggregator.Recompute(_state);
            ValidateActiveSlots();
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
        }

        private string ResolveNodeTreeId(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node) ? node.TreeId : null;

        // fable_29 — drop saved node ids unknown to the live catalog, refunding their ranks.
        private void MigrateUnknownNodes(SkillTreeSaveData data)
        {
            var unknown = new List<string>();
            foreach (var nodeId in _state.PurchasedNodeIds)
                if (!_nodeIndex.ContainsKey(nodeId))
                    unknown.Add(nodeId);

            if (unknown.Count == 0) return;

            int totalRefunded = 0;
            foreach (var nodeId in unknown)
            {
                int refunded = _state.RemoveAndRefundNode(nodeId);
                totalRefunded += refunded;
                Debug.Log($"[SkillTreeManager] Save migration: refunded {refunded} point(s) for obsolete skill '{nodeId}'.");
            }

            Debug.Log($"[SkillTreeManager] Save migration complete: {unknown.Count} obsolete node(s), {totalRefunded} point(s) refunded to pool.");
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
