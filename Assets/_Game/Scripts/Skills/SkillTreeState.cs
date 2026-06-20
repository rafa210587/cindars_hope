using System.Collections.Generic;

namespace CindarsHope.Skills
{
    // Runtime state for the skill tree: tracks node RANKS (fable_29), points spent per tree,
    // active slots, chosen capstone variants, and respec count.
    //
    // fable_29: a node now has a RANK (0 = not purchased, 1..cap = purchased ranks). Each rank
    // costs exactly 1 point (decision 1.2). "Purchased" == rank >= 1 (backward-compatible with the
    // old boolean model and with old saves that carried only PurchasedNodeIds).
    public class SkillTreeState
    {
        private readonly Dictionary<string, int> _nodeRanks = new Dictionary<string, int>();
        // Points spent per tree id (drives tier gating + dynamic rank cap). Recomputed on every mutation.
        private readonly Dictionary<string, int> _pointsByTree = new Dictionary<string, int>();
        // Chosen capstone variant per node (e.g. melee capstone -> "kanthor").
        private readonly Dictionary<string, string> _chosenVariants = new Dictionary<string, string>();
        private readonly string[] _activeSlotSkillActionIds = new string[4];
        private int _spentPoints;

        public int AvailableSkillPoints { get; private set; }
        public int SpentSkillPoints => _spentPoints;
        public int RespecCount { get; private set; }

        public SkillTreeState(int availableSkillPoints = 0)
        {
            AvailableSkillPoints = availableSkillPoints;
        }

        public void SetAvailablePoints(int points)
        {
            AvailableSkillPoints = points < 0 ? 0 : points;
        }

        public void AddSkillPoints(int amount)
        {
            if (amount > 0) AvailableSkillPoints += amount;
        }

        // ── Ranks ──────────────────────────────────────────────────────────────

        public int GetRank(string nodeId)
            => nodeId != null && _nodeRanks.TryGetValue(nodeId, out var r) ? r : 0;

        public bool IsPurchased(string nodeId) => GetRank(nodeId) >= 1;

        public IReadOnlyCollection<string> PurchasedNodeIds => _nodeRanks.Keys;

        // Purchase the FIRST rank of a node (rank 0 -> 1). Cost is deducted from the pool and
        // accrued to the node's tree (treeId required for tier gating).
        //
        // WARNING (test/validator only): this 2-arg overload passes treeId = null and therefore
        // does NOT accrue per-tree points (tier gating would not advance). The production runtime
        // path (SkillPurchaseService) always uses the 3-arg form with node.TreeId. Do not call the
        // 2-arg form from runtime gameplay code — it exists only for isolated cost-decrement asserts.
        public void Purchase(string nodeId, int cost) => Purchase(nodeId, cost, null);

        public void Purchase(string nodeId, int cost, string treeId)
        {
            _nodeRanks[nodeId] = 1;
            AvailableSkillPoints -= cost;
            _spentPoints += cost;
            AccruePoints(treeId, cost);
        }

        // Add ONE rank to an already-purchased node (rank N -> N+1). 1 point (decision 1.2).
        public void RankUp(string nodeId, string treeId)
        {
            int current = GetRank(nodeId);
            _nodeRanks[nodeId] = current < 1 ? 1 : current + 1;
            AvailableSkillPoints -= 1;
            _spentPoints += 1;
            AccruePoints(treeId, 1);
        }

        private void AccruePoints(string treeId, int amount)
        {
            if (string.IsNullOrEmpty(treeId)) return;
            _pointsByTree.TryGetValue(treeId, out var cur);
            _pointsByTree[treeId] = cur + amount;
        }

        // Points spent in a tree (authoritative for tier gating). Falls back to id-prefix
        // counting only for legacy data with no accrued per-tree totals.
        public int PointsSpentInTree(string treeId)
        {
            if (string.IsNullOrEmpty(treeId)) return 0;
            if (_pointsByTree.TryGetValue(treeId, out var p)) return p;
            return CountPurchasedInTree(treeId);
        }

        public int DeepestUnlockedTier(string treeId)
            => SkillTierRules.DeepestUnlockedTier(PointsSpentInTree(treeId));

        public int DynamicRankCap(string treeId)
            => SkillTierRules.DynamicRankCapForTree(DeepestUnlockedTier(treeId));

        // Snapshot of deepest-unlocked tier per tree (for the equip-revalidation gate).
        public Dictionary<string, int> SnapshotDeepestTierByTree()
        {
            var snapshot = new Dictionary<string, int>();
            foreach (var kvp in _pointsByTree)
                snapshot[kvp.Key] = SkillTierRules.DeepestUnlockedTier(kvp.Value);
            return snapshot;
        }

        // Legacy id-prefix tree count (kept for back-compat; preferred path is PointsSpentInTree).
        public int CountPurchasedInTree(string treeId)
        {
            int count = 0;
            foreach (var id in _nodeRanks.Keys)
                if (id.StartsWith(treeId + "_") || id.StartsWith(treeId + ".")) count++;
            return count;
        }

        // ── Capstone variants ──────────────────────────────────────────────────

        public string GetChosenVariant(string nodeId)
            => nodeId != null && _chosenVariants.TryGetValue(nodeId, out var v) ? v : null;

        public bool HasChosenVariant(string nodeId) => !string.IsNullOrEmpty(GetChosenVariant(nodeId));

        public void SetChosenVariant(string nodeId, string variant)
        {
            if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(variant)) return;
            _chosenVariants[nodeId] = variant;
        }

        // ── Active slots ─────────────────────────────────────────────────────────

        public bool AssignActiveSlot(int slotIndex, string skillActionId)
        {
            if (slotIndex < 0 || slotIndex >= _activeSlotSkillActionIds.Length) return false;
            _activeSlotSkillActionIds[slotIndex] = skillActionId;
            return true;
        }

        public bool ClearActiveSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _activeSlotSkillActionIds.Length) return false;
            _activeSlotSkillActionIds[slotIndex] = null;
            return true;
        }

        public string GetActiveSlotSkillActionId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _activeSlotSkillActionIds.Length) return null;
            return _activeSlotSkillActionIds[slotIndex];
        }

        // ── Respec ─────────────────────────────────────────────────────────────

        public void FullRespec(int restoredPoints)
        {
            _nodeRanks.Clear();
            _pointsByTree.Clear();
            _chosenVariants.Clear();
            for (int i = 0; i < _activeSlotSkillActionIds.Length; i++)
                _activeSlotSkillActionIds[i] = null;
            AvailableSkillPoints = restoredPoints;
            _spentPoints = 0;
            RespecCount++;
        }

        // ── Save / Load ────────────────────────────────────────────────────────

        public SkillTreeSaveData ToSaveData(System.Func<int, string> slotKeyResolver)
        {
            var data = new SkillTreeSaveData
            {
                RespecCount = RespecCount
            };

            foreach (var kvp in _nodeRanks)
            {
                data.PurchasedNodeIds.Add(kvp.Key);                 // back-compat flat list
                data.NodeRanks.Add(new SkillNodeRankEntry(kvp.Key, kvp.Value)); // fable_29 ranks
            }

            foreach (var kvp in _chosenVariants)
                data.ChosenCapstoneVariants.Add(new SkillCapstoneVariantEntry(kvp.Key, kvp.Value));

            for (int i = 0; i < _activeSlotSkillActionIds.Length; i++)
            {
                data.ActiveSkillSlots.Add(new ActiveSkillSlotSaveEntry(
                    i,
                    slotKeyResolver(i),
                    _activeSlotSkillActionIds[i] ?? string.Empty));
            }
            return data;
        }

        // Loads save data. Migration of unknown node ids is handled by the caller
        // (SkillTreeManager) which knows the catalog; this method restores what it is given.
        // nodeTreeResolver maps a node id -> its tree id so per-tree point totals can be rebuilt.
        public void LoadFromSaveData(SkillTreeSaveData data, int totalAvailablePoints,
            System.Func<string, string> nodeTreeResolver = null)
        {
            if (data == null) return;
            _nodeRanks.Clear();
            _pointsByTree.Clear();
            _chosenVariants.Clear();

            // Prefer explicit ranks (fable_29). Fall back to flat list (rank 1) for legacy saves.
            if (data.NodeRanks != null && data.NodeRanks.Count > 0)
            {
                foreach (var entry in data.NodeRanks)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.NodeId)) continue;
                    int rank = entry.Rank < 1 ? 1 : entry.Rank;
                    _nodeRanks[entry.NodeId] = rank;
                }
            }
            else
            {
                foreach (var id in data.PurchasedNodeIds)
                    if (!string.IsNullOrEmpty(id)) _nodeRanks[id] = 1;
            }

            if (data.ChosenCapstoneVariants != null)
            {
                foreach (var entry in data.ChosenCapstoneVariants)
                    if (entry != null && !string.IsNullOrEmpty(entry.NodeId))
                        _chosenVariants[entry.NodeId] = entry.Variant;
            }

            // Rebuild per-tree point totals + spent total from ranks.
            _spentPoints = 0;
            foreach (var kvp in _nodeRanks)
            {
                _spentPoints += kvp.Value; // 1 point per rank
                var treeId = nodeTreeResolver?.Invoke(kvp.Key);
                AccruePoints(treeId, kvp.Value);
            }

            AvailableSkillPoints = totalAvailablePoints - _spentPoints;
            if (AvailableSkillPoints < 0) AvailableSkillPoints = 0;
            RespecCount = data.RespecCount;

            for (int i = 0; i < _activeSlotSkillActionIds.Length; i++)
                _activeSlotSkillActionIds[i] = null;

            foreach (var slot in data.ActiveSkillSlots)
            {
                if (slot.SlotIndex >= 0 && slot.SlotIndex < _activeSlotSkillActionIds.Length)
                    _activeSlotSkillActionIds[slot.SlotIndex] = slot.SkillActionId;
            }
        }

        // fable_29 save migration: drop a node id (unknown to the catalog) and refund its ranks
        // to the available pool. Returns the number of points refunded for logging.
        public int RemoveAndRefundNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId) || !_nodeRanks.TryGetValue(nodeId, out var rank))
                return 0;
            _nodeRanks.Remove(nodeId);
            _chosenVariants.Remove(nodeId);
            _spentPoints -= rank;
            if (_spentPoints < 0) _spentPoints = 0;
            AvailableSkillPoints += rank; // lossless refund
            return rank;
        }
    }
}
