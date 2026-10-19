using System.Collections.Generic;

namespace CindarsHope.Skills
{
    // fable_29 (emenda V3 item 6) — punitive respec equipment revalidation gate.
    //
    // Some items require a given skill TREE to have reached a given TIER before they may be
    // equipped/used. When a respec lowers a tree below that tier, those items must become
    // NON-EQUIPPABLE (kept in inventory, not destroyed) until the tier is re-unlocked.
    //
    // This is a SKILLS-SIDE, search-free, deterministic gate. The equipment system only
    // READS it (IsEquipBlocked) — it does not own the rule. The SkillEffectAggregator
    // recomputes the blocked set after every purchase/respec from the live tier state.
    //
    // Save impact: NONE. The blocked set is DERIVED from points-spent-in-tree (already
    // persisted); no new save field. Item→required-tier requirements are catalog data.
    public static class SkillTierEquipGate
    {
        // itemId -> (treeId, requiredTier). Registered by the catalog/config; static so the
        // equipment "can equip?" check can consult it without a scene reference.
        private static readonly Dictionary<string, (string TreeId, int RequiredTier)> _requirements
            = new Dictionary<string, (string, int)>();

        // Currently blocked item ids (recomputed by the aggregator from live tier state).
        private static readonly HashSet<string> _blocked = new HashSet<string>();

        // Register an item's tier requirement (idempotent). Catalog/config call site.
        public static void RegisterRequirement(string itemId, string treeId, int requiredTier)
        {
            if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(treeId)) return;
            _requirements[itemId] = (treeId, requiredTier);
        }

        public static bool HasRequirement(string itemId)
            => !string.IsNullOrEmpty(itemId) && _requirements.ContainsKey(itemId);

        public static bool TryGetRequirement(string itemId, out string treeId, out int requiredTier)
        {
            if (!string.IsNullOrEmpty(itemId) && _requirements.TryGetValue(itemId, out var req))
            {
                treeId = req.TreeId;
                requiredTier = req.RequiredTier;
                return true;
            }
            treeId = string.Empty;
            requiredTier = 0;
            return false;
        }

        // The equipment system consults THIS (read-only) before equipping.
        public static bool IsEquipBlocked(string itemId)
            => !string.IsNullOrEmpty(itemId) && _blocked.Contains(itemId);

        public static IReadOnlyCollection<string> BlockedItems => _blocked;

        // Recompute the blocked set from a snapshot of deepest-unlocked-tier per tree.
        // Called by the aggregator after any purchase/respec. An item is blocked when its
        // required tier exceeds the tree's currently-unlocked depth.
        public static void Recompute(IReadOnlyDictionary<string, int> deepestTierByTree)
        {
            _blocked.Clear();
            if (deepestTierByTree == null) return;

            foreach (var kvp in _requirements)
            {
                var itemId = kvp.Key;
                var treeId = kvp.Value.TreeId;
                var requiredTier = kvp.Value.RequiredTier;

                int unlocked = deepestTierByTree.TryGetValue(treeId, out var t) ? t : SkillTierRules.MinTier;
                if (unlocked < requiredTier)
                    _blocked.Add(itemId);
            }
        }

        // Test/teardown helper.
        public static void ResetAll()
        {
            _requirements.Clear();
            _blocked.Clear();
        }
    }
}
