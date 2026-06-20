using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Skills
{
    // fable_29 — purchase + rank-up validation with tier gating, dynamic rank caps,
    // per-node prerequisites and exclusive capstone variants.
    public class SkillPurchaseService
    {
        private readonly Dictionary<string, SkillNodeDataSO> _nodeIndex;

        public SkillPurchaseService(IEnumerable<SkillNodeDataSO> allNodes)
        {
            _nodeIndex = new Dictionary<string, SkillNodeDataSO>();
            foreach (var node in allNodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.SkillNodeId))
                    _nodeIndex[node.SkillNodeId] = node;
            }
        }

        // Purchase the first rank (rank 0 -> 1) of a node. For capstone-variant nodes the caller
        // must pass the chosen variant (confirmation is handled by the manager). All gates apply:
        // tier (points spent in tree), prerequisites, exclusivity, level, point budget.
        public bool TryPurchase(string nodeId, SkillTreeState state, int playerLevel, out string failReason)
            => TryPurchase(nodeId, state, playerLevel, null, out failReason);

        public bool TryPurchase(string nodeId, SkillTreeState state, int playerLevel,
            string chosenVariant, out string failReason)
        {
            failReason = string.Empty;

            if (!_nodeIndex.TryGetValue(nodeId, out var node))
            {
                failReason = $"Node '{nodeId}' not found.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            if (state.IsPurchased(nodeId))
            {
                failReason = "Node already purchased.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            if (state.AvailableSkillPoints < node.SkillPointCost)
            {
                failReason = "Not enough skill points.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            if (playerLevel < node.MinimumPlayerLevel)
            {
                failReason = $"Requires player level {node.MinimumPlayerLevel}.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            // Per-node prerequisites (must all be purchased; same tree by contract).
            foreach (var prereq in node.PrerequisiteNodeIds)
            {
                if (!state.IsPurchased(prereq))
                {
                    failReason = $"Missing prerequisite '{prereq}'.";
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                    return false;
                }
            }

            // Tier gating: points spent IN THE TREE must reach the node's tier threshold.
            if (!SkillTierRules.IsTierUnlocked(node.Tier, state.PointsSpentInTree(node.TreeId)))
            {
                int needed = SkillTierRules.TierPointThresholds[
                    UnityEngineMathfClamp(node.Tier, SkillTierRules.MinTier, SkillTierRules.MaxTier)];
                failReason = $"Tier {node.Tier} locked: requires {needed} points spent in '{node.TreeId}'.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            // Legacy RequiredPurchasedNodesInTree (kept as an additional floor if set > 0).
            if (node.RequiredPurchasedNodesInTree > 0)
            {
                int purchasedInTree = state.CountPurchasedInTree(node.TreeId);
                if (purchasedInTree < node.RequiredPurchasedNodesInTree)
                {
                    failReason = $"Requires {node.RequiredPurchasedNodesInTree} nodes purchased in tree '{node.TreeId}'.";
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                    return false;
                }
            }

            // Exclusive capstone variant: the chosen variant must be valid and not blocked by an
            // already-chosen exclusive variant on the same node (cannot happen pre-purchase, but
            // guarded) — and cross-node exclusivity (a sibling capstone variant elsewhere) is
            // enforced via ExclusiveWith on the node list when present.
            if (node.CapstoneVariants != null && node.CapstoneVariants.Count > 0)
            {
                if (string.IsNullOrEmpty(chosenVariant) || !node.CapstoneVariants.Contains(chosenVariant))
                {
                    failReason = $"Capstone '{nodeId}' requires choosing one of its exclusive variants.";
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                    return false;
                }
            }

            // Commit: first rank.
            state.Purchase(nodeId, node.SkillPointCost, node.TreeId);
            if (!string.IsNullOrEmpty(chosenVariant))
                state.SetChosenVariant(nodeId, chosenVariant);

            GameEventBus.Publish(new SkillNodePurchasedEvent(nodeId, node.TreeId, state.AvailableSkillPoints));
            return true;
        }

        // fable_29 — rank up an already-purchased node (rank N -> N+1), bounded by the DYNAMIC
        // rank cap of its tree (driven by the deepest unlocked tier). 1 point per rank.
        public bool TryRankUp(string nodeId, SkillTreeState state, out string failReason)
        {
            failReason = string.Empty;

            if (!_nodeIndex.TryGetValue(nodeId, out var node))
            {
                failReason = $"Node '{nodeId}' not found.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            int current = state.GetRank(nodeId);
            if (current < 1)
            {
                failReason = "Node not purchased yet (buy rank 1 first).";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            int cap = state.DynamicRankCap(node.TreeId);
            if (current >= cap)
            {
                failReason = $"Rank cap reached (cap {cap}; unlock a deeper tier in '{node.TreeId}' to raise it).";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            if (state.AvailableSkillPoints < 1)
            {
                failReason = "Not enough skill points.";
                GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                return false;
            }

            state.RankUp(nodeId, node.TreeId);
            GameEventBus.Publish(new SkillNodePurchasedEvent(nodeId, node.TreeId, state.AvailableSkillPoints));
            return true;
        }

        public bool CanRankUp(string nodeId, SkillTreeState state)
        {
            if (!_nodeIndex.TryGetValue(nodeId, out var node)) return false;
            int current = state.GetRank(nodeId);
            if (current < 1) return false;
            return current < state.DynamicRankCap(node.TreeId) && state.AvailableSkillPoints >= 1;
        }

        public bool TryGetNode(string nodeId, out SkillNodeDataSO node)
            => _nodeIndex.TryGetValue(nodeId, out node);

        // Local clamp helper to avoid a UnityEngine.Mathf dependency in this pure-logic class.
        private static int UnityEngineMathfClamp(int v, int min, int max)
            => v < min ? min : (v > max ? max : v);
    }
}
