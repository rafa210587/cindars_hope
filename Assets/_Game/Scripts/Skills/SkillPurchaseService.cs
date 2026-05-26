using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Skills
{
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

        public bool TryPurchase(string nodeId, SkillTreeState state, int playerLevel, out string failReason)
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

            foreach (var prereq in node.PrerequisiteNodeIds)
            {
                if (!state.IsPurchased(prereq))
                {
                    failReason = $"Missing prerequisite '{prereq}'.";
                    GameEventBus.Publish(new SkillPurchaseFailedEvent(nodeId, failReason));
                    return false;
                }
            }

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

            state.Purchase(nodeId, node.SkillPointCost);
            GameEventBus.Publish(new SkillNodePurchasedEvent(nodeId, node.TreeId, state.AvailableSkillPoints));
            return true;
        }

        public bool TryGetNode(string nodeId, out SkillNodeDataSO node)
            => _nodeIndex.TryGetValue(nodeId, out node);
    }
}
