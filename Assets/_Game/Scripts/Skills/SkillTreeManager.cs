using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using CindarsHope.Player.Progression;
using UnityEngine;

namespace CindarsHope.Skills
{
    [DisallowMultipleComponent]
    public class SkillTreeManager : MonoBehaviour, ISkillTreeRuntime
    {
        private const string RuntimeCatalogResourcePath = "Skills/SkillRuntimeCatalogRegistry";
        // arch: Core|Skills (spec_arch_core_skills_cycle_reduction_v34) — self-registro estatico
        // (molde AudioManager/CraftingManager/ShopManager) para o GameBootstrap parar de segurar
        // esta referencia serializada.
        public static SkillTreeManager Instance { get; private set; }

        [SerializeField] private SkillTreeRegistrySO _treeRegistry;
        [SerializeField] private SkillNodeDatabaseSO _nodeDatabase;
        [SerializeField] private SkillActionDatabaseSO _actionDatabase;
        [SerializeField] private int _respecCostGold = 250;
        [SerializeField] private PlayerProgressionManager _progressionManager;

        private SkillTreeState _state;
        private SkillPurchaseService _purchaseService;
        private SkillRespecService _respecService;
        // fable_29: the aggregator replaces the old per-node applicator as the single recompute
        // authority (DerivedStats provider + named hooks + equip gate). Rank-scaled.
        private SkillEffectAggregator _aggregator;
        private bool _mutationInProgress;
        private System.Func<MarketPriceDirection, float> _ownedMarketMultiplierSource;
        private System.Func<float> _ownedCraftedDurabilityBonusSource;
        private System.Func<float> _ownedSalvageChanceSource;
        private CombatCapstoneState _combatCapstoneState;
        private CombatCapstoneRuntime _combatCapstoneRuntime;
        private RangedLunarRuntime _rangedLunarRuntime;
        private MagicConfluenceRuntime _magicConfluenceRuntime;

        private Dictionary<string, SkillTreeDataSO> _treeIndex = new Dictionary<string, SkillTreeDataSO>();
        private Dictionary<string, SkillNodeDataSO> _nodeIndex = new Dictionary<string, SkillNodeDataSO>();
        private Dictionary<string, SkillActionSO> _actionIndex = new Dictionary<string, SkillActionSO>();

        public SkillTreeState State => _state;
        public IReadOnlyDictionary<string, SkillNodeDataSO> NodeIndex => _nodeIndex;
        public IReadOnlyDictionary<string, SkillTreeDataSO> TreeIndex => _treeIndex;
        public IReadOnlyDictionary<string, SkillActionSO> ActionIndex => _actionIndex;

        public void RebindProgressionManager(PlayerProgressionManager progressionManager)
        {
            _progressionManager = progressionManager;
            if (_progressionManager != null && _state != null)
            {
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
            }
        }

        // arch: Core|Skills followup (spec_arch_core_skills_cycle_reduction_v34_followup) — porta
        // ISkillTreeRuntime.RebindProgressionManager() sem parametro: resolve o PlayerProgressionManager
        // ativo internamente (mesmo static Instance que o GameBootstrap ja usava) para o Core parar de
        // referenciar o tipo concreto SkillTreeManager diretamente.
        public void RebindProgressionManager()
        {
            RebindProgressionManager(PlayerProgressionManager.Instance);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DomainManagerRegistry.Register<ISkillTreeRuntime>(this);

            _state = new SkillTreeState();
            _combatCapstoneState = DomainManagerRegistry.Get<CombatCapstoneState>()
                ?? new CombatCapstoneState();
            DomainManagerRegistry.Register(_combatCapstoneState);
            _combatCapstoneRuntime = new CombatCapstoneRuntime(
                _combatCapstoneState,
                () => _state?.GetRank(CombatCapstoneRuntime.NodeId) ?? 0,
                () => _state?.GetChosenVariant(CombatCapstoneRuntime.NodeId),
                () => ResolvePlayer()?.CurrentHP ?? 0,
                () => ResolvePlayer()?.MaxHP ?? 1,
                amount => ResolvePlayer()?.RestoreHP(amount),
                amount => ResolveStamina()?.AddStamina(amount));
            CombatCapstoneModifierProvider.Source = _combatCapstoneRuntime;
            PlayerControlResistanceProvider.Source = _combatCapstoneRuntime;
            var survivalSkillState = DomainManagerRegistry.Get<SurvivalSkillState>()
                ?? new SurvivalSkillState();
            if (DomainManagerRegistry.Get<SurvivalSkillState>() == null)
                DomainManagerRegistry.Register(survivalSkillState);
            _rangedLunarRuntime = new RangedLunarRuntime(
                survivalSkillState,
                () => _state?.GetRank(RangedLunarRuntime.NodeId) ?? 0,
                () => _state?.GetChosenVariant(RangedLunarRuntime.NodeId));
            RangedLunarModifierProvider.Source = _rangedLunarRuntime;
            _magicConfluenceRuntime = new MagicConfluenceRuntime(
                _combatCapstoneState.MagicConfluence,
                () => _state?.GetRank(MagicConfluenceRuntime.NodeId) ?? 0,
                () => _state?.GetChosenVariant(MagicConfluenceRuntime.NodeId),
                amount => ResolveStamina()?.AddStamina(amount));
            SpellCastPreparationProvider.Source = _magicConfluenceRuntime;
            _ownedMarketMultiplierSource = direction => MarketSkillModifierProvider.Resolve(
                _state?.GetRank(MarketSkillModifierProvider.NodeId) ?? 0,
                _state?.GetChosenVariant(MarketSkillModifierProvider.NodeId),
                direction);
            MarketSkillModifierProvider.MultiplierSource = _ownedMarketMultiplierSource;
            _ownedCraftedDurabilityBonusSource = () => CraftedItemDurabilityProvider.ResolveBonus(
                _state?.GetRank(CraftedItemDurabilityProvider.NodeId) ?? 0);
            CraftedItemDurabilityProvider.BonusSource = _ownedCraftedDurabilityBonusSource;
            _ownedSalvageChanceSource = () => SalvageSkillModifierProvider.Resolve(
                _state?.GetRank(SalvageSkillModifierProvider.NodeId) ?? 0);
            SalvageSkillModifierProvider.ChanceSource = _ownedSalvageChanceSource;
            BuildCatalog();

            _purchaseService = new SkillPurchaseService(_nodeIndex.Values);
            _respecService = new SkillRespecService(_respecCostGold);
            _aggregator = new SkillEffectAggregator(_nodeIndex);
        }

        private void OnEnable()
        {
            _combatCapstoneRuntime?.Enable();
            _rangedLunarRuntime?.Enable();
            GameEventBus.Subscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            GameEventBus.Subscribe<SkillPointGrantedEvent>(OnSkillPointGranted);
        }

        private void OnDisable()
        {
            _combatCapstoneRuntime?.Disable();
            _rangedLunarRuntime?.Disable();
            GameEventBus.Unsubscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            GameEventBus.Unsubscribe<SkillPointGrantedEvent>(OnSkillPointGranted);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            DomainManagerRegistry.Unregister<ISkillTreeRuntime>(this);
            DomainManagerRegistry.Unregister(_combatCapstoneState);
            if (System.Object.ReferenceEquals(CombatCapstoneModifierProvider.Source,
                    _combatCapstoneRuntime))
                CombatCapstoneModifierProvider.Source = null;
            if (System.Object.ReferenceEquals(PlayerControlResistanceProvider.Source,
                    _combatCapstoneRuntime))
                PlayerControlResistanceProvider.Source = null;
            _combatCapstoneRuntime?.Dispose();
            if (System.Object.ReferenceEquals(RangedLunarModifierProvider.Source,
                    _rangedLunarRuntime))
                RangedLunarModifierProvider.Source = null;
            _rangedLunarRuntime?.Dispose();
            if (System.Object.ReferenceEquals(SpellCastPreparationProvider.Source,
                    _magicConfluenceRuntime))
                SpellCastPreparationProvider.Source = null;
            if (System.Object.ReferenceEquals(MarketSkillModifierProvider.MultiplierSource,
                    _ownedMarketMultiplierSource))
                MarketSkillModifierProvider.MultiplierSource = null;
            if (System.Object.ReferenceEquals(CraftedItemDurabilityProvider.BonusSource,
                    _ownedCraftedDurabilityBonusSource))
                CraftedItemDurabilityProvider.BonusSource = null;
            if (System.Object.ReferenceEquals(SalvageSkillModifierProvider.ChanceSource,
                    _ownedSalvageChanceSource))
                SalvageSkillModifierProvider.ChanceSource = null;
        }

        private void Update()
        {
            _combatCapstoneRuntime?.Tick(Time.deltaTime);
            _rangedLunarRuntime?.Tick(Time.deltaTime);
            _magicConfluenceRuntime?.Tick(Time.deltaTime);
        }

        private static IPlayerRuntime ResolvePlayer()
            => GameBootstrap.Instance?.PlayerManager as IPlayerRuntime;

        private static IStaminaRuntime ResolveStamina()
            => GameBootstrap.Instance?.StaminaManager as IStaminaRuntime;

        private void BuildCatalog()
        {
            var runtimeCatalog = Resources.Load<SkillRuntimeCatalogRegistrySO>(RuntimeCatalogResourcePath);
            if (runtimeCatalog != null)
            {
                if (_treeRegistry == null) _treeRegistry = runtimeCatalog.TreeRegistry;
                if (_nodeDatabase == null) _nodeDatabase = runtimeCatalog.NodeDatabase;
                if (_actionDatabase == null) _actionDatabase = runtimeCatalog.ActionDatabase;
            }

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
                Debug.LogError("SkillTreeManager: generated tree/node registries are not wired; using code fallback.", this);
                var nodes = DefaultSkillCatalog.BuildAllNodes();
                foreach (var node in nodes)
                    _nodeIndex[node.SkillNodeId] = node;

                var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
                foreach (var tree in trees)
                    _treeIndex[tree.TreeId] = tree;
            }

            IEnumerable<SkillActionSO> actions = _actionDatabase != null
                ? (IEnumerable<SkillActionSO>)_actionDatabase.All
                : DefaultSkillActionCatalog.BuildAll();
            if (_actionDatabase == null)
                Debug.LogError("SkillTreeManager: generated action database is not wired; using code fallback.", this);
            foreach (var action in actions)
                if (action != null && !string.IsNullOrWhiteSpace(action.SkillActionId))
                    _actionIndex[action.SkillActionId] = action;
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
            feedback = "Outra alteração de habilidades está em andamento.";
            if (_mutationInProgress) return false;
            _mutationInProgress = true;
            try
            {
                var candidate = _state.Copy();
                if (_progressionManager != null) candidate.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
                if (!_purchaseService.TryPurchase(nodeId, candidate, playerLevel, chosenVariant, out feedback, false))
                {
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, feedback));
                    return false;
                }
                var node = _nodeIndex[nodeId];
                if (_progressionManager != null && !_progressionManager.TrySpendSkillPoints(node.SkillPointCost))
                {
                    feedback = "Skill points changed before purchase could complete.";
                    return false;
                }
                _state = candidate;
                _aggregator.Recompute(_state);
                feedback = "Skill comprada.";
                if (node.SkillCategory == SkillCategory.EquippableSkill && !string.IsNullOrWhiteSpace(node.UnlockedSkillActionId))
                    feedback = AutoAssignActiveSkill(node.UnlockedSkillActionId);
                GameEventBus.Publish(new SkillNodePurchasedEvent(nodeId, node.TreeId, _state.AvailableSkillPoints));
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
                return true;
            }
            finally { _mutationInProgress = false; }
        }

        // fable_29 — rank up an already-purchased node (rank N -> N+1), bounded by the dynamic cap.
        public bool TryRankUpNode(string nodeId, out string feedback)
        {
            feedback = "Outra alteração de habilidades está em andamento.";
            if (_mutationInProgress) return false;
            _mutationInProgress = true;
            try
            {
                var candidate = _state.Copy();
                if (_progressionManager != null) candidate.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
                if (!_purchaseService.TryRankUp(nodeId, candidate, out feedback, false))
                {
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, feedback));
                    return false;
                }
                if (_progressionManager != null && !_progressionManager.TrySpendSkillPoints(1))
                {
                    feedback = "Skill points changed before rank-up could complete.";
                    return false;
                }
                _state = candidate;
                _aggregator.Recompute(_state);
                feedback = "Rank aumentado.";
                GameEventBus.Publish(new SkillNodePurchasedEvent(nodeId, _nodeIndex[nodeId].TreeId, _state.AvailableSkillPoints));
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
                return true;
            }
            finally { _mutationInProgress = false; }
        }

        // fable_29 (CA-3) — does this node require an exclusive capstone-variant confirmation?
        public bool RequiresCapstoneConfirmation(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node)
               && node.CapstoneVariants != null && node.CapstoneVariants.Count > 0;

        public IReadOnlyList<string> GetCapstoneVariants(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node) ? node.CapstoneVariants : null;

        public int GetRank(string nodeId) => _state.GetRank(nodeId);
        public string GetChosenVariant(string nodeId) => _state.GetChosenVariant(nodeId);
        public int GetDynamicRankCap(string treeId) => _state.DynamicRankCap(treeId);
        public int GetPointsSpentInTree(string treeId) => _state.PointsSpentInTree(treeId);
        public int GetEffectiveRankCap(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node) ? _purchaseService.GetEffectiveRankCap(node, _state) : 1;

        public bool TryResolveActionData(string nodeId, out SkillActionSO actionData, out int rank, out string failureReason)
        {
            actionData = null;
            rank = 1;
            failureReason = string.Empty;
            if (!_nodeIndex.TryGetValue(nodeId, out var node))
            {
                failureReason = "UnknownNode";
                return false;
            }
            if (node.NotYetExecutable)
            {
                failureReason = SkillPurchaseService.NotYetExecutableFailureReason;
                return false;
            }
            if (string.IsNullOrWhiteSpace(node.UnlockedSkillActionId)
                || !_actionIndex.TryGetValue(node.UnlockedSkillActionId, out actionData))
            {
                failureReason = "MissingActionData";
                return false;
            }
            if (actionData.NotYetExecutable)
            {
                failureReason = SkillPurchaseService.NotYetExecutableFailureReason;
                actionData = null;
                return false;
            }

            int storedRank = _state.GetRank(nodeId);
            rank = Mathf.Clamp(storedRank <= 0 ? 1 : storedRank, 1, GetEffectiveRankCap(nodeId));
            return true;
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

        // fable_29 (emenda V3 item 6) — punitive respec: refunds points AND recomputes the
        // equip-revalidation gate so items gated by re-locked tiers become non-equippable.
        public bool TryRespec(ref int gold, int playerLevel)
            => TryRespec(ref gold, playerLevel, null);

        public bool TryRespec(ref int gold, int playerLevel, System.Action<int> commitGold)
        {
            if (_mutationInProgress) return false;
            _mutationInProgress = true;
            try
            {
                var candidate = _state.Copy();
                if (_progressionManager != null) candidate.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
                int remainingGold = gold;
                if (!_respecService.TryRespec(candidate, playerLevel, ref remainingGold, false))
                {
                    GameEventBus.Publish(new SkillTreeRespecFailedEvent("Not enough gold."));
                    return false;
                }
                _progressionManager?.RestoreSkillPointBalance(candidate.AvailableSkillPoints);
                _state = candidate;
                gold = remainingGold;
                _aggregator.Recompute(_state);
                _magicConfluenceRuntime?.OnRespec();
                commitGold?.Invoke(gold);
                GameEventBus.Publish(new SkillTreeRespecCompletedEvent(_state.AvailableSkillPoints, _state.RespecCount));
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
                return true;
            }
            finally { _mutationInProgress = false; }
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
            var candidate = new SkillTreeState();
            candidate.LoadFromSaveData(data, totalPoints, ResolveNodeTreeId);
            if (_progressionManager != null) candidate.SetAvailablePoints(_progressionManager.UnspentSkillPoints);

            // fable_29 migration: refund any saved node id that no longer exists in the catalog.
            // Points return to the pool (lossless); each refund is logged.
            MigrateUnknownNodes(candidate);
            _progressionManager?.RestoreSkillPointBalance(candidate.AvailableSkillPoints);
            _state = candidate;

            _aggregator.Recompute(_state);
            ValidateActiveSlots();
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
        }

        private string ResolveNodeTreeId(string nodeId)
            => _nodeIndex.TryGetValue(nodeId, out var node) ? node.TreeId : null;

        // fable_29 — drop saved node ids unknown to the live catalog, refunding their ranks.
        private void MigrateUnknownNodes(SkillTreeState candidate)
        {
            var unknown = new List<string>();
            foreach (var nodeId in candidate.PurchasedNodeIds)
                if (!_nodeIndex.ContainsKey(nodeId))
                    unknown.Add(nodeId);

            if (unknown.Count == 0) return;

            int totalRefunded = 0;
            foreach (var nodeId in unknown)
            {
                int refunded = candidate.RemoveAndRefundNode(nodeId);
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
                if (_progressionManager != null) _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
                else _state.AddSkillPoints(evt.GrantedSkillPoints);
                GameEventBus.Publish(new SkillPointGrantedEvent(
                    evt.GrantedSkillPoints,
                    _state.AvailableSkillPoints,
                    evt.NewLevel));
            }
        }

        private void OnSkillPointGranted(SkillPointGrantedEvent evt)
        {
            if (_progressionManager != null && _state != null)
                _state.SetAvailablePoints(_progressionManager.UnspentSkillPoints);
        }
    }
}
