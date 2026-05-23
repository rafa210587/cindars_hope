using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Skills
{
    public class SkillTreeManager
    {
        private HashSet<string> _unlockedNodes = new HashSet<string>();
        private List<string> _activeSlotNodes = new List<string>();
        private int _totalSpentPoints;

        public SkillTreeManager(int maxActiveSlots = 4)
        {
            MaxActiveSlots = maxActiveSlots;
        }

        public int MaxActiveSlots { get; set; }

        public bool UnlockNode(SkillNodeDataSO node, int currentLevel, int availableSkillPoints)
        {
            if (_unlockedNodes.Contains(node.Id))
                return false;

            if (currentLevel < node.RequiredLevel)
                return false;

            if (availableSkillPoints < node.SkillPointCost)
                return false;

            if (!string.IsNullOrEmpty(node.RequiredSkillNodeId) && !_unlockedNodes.Contains(node.RequiredSkillNodeId))
                return false;

            _unlockedNodes.Add(node.Id);
            _totalSpentPoints += node.SkillPointCost;
            return true;
        }

        public bool ActivateSkillSlot(string nodeId)
        {
            if (!_unlockedNodes.Contains(nodeId))
                return false;

            if (_activeSlotNodes.Count < MaxActiveSlots)
            {
                _activeSlotNodes.Add(nodeId);
                return true;
            }

            return false;
        }

        public bool DeactivateSkillSlot(string nodeId)
        {
            return _activeSlotNodes.Remove(nodeId);
        }

        public void Respec()
        {
            _unlockedNodes.Clear();
            _activeSlotNodes.Clear();
            _totalSpentPoints = 0;
        }

        public bool IsNodeUnlocked(string nodeId) => _unlockedNodes.Contains(nodeId);
        public List<string> GetActiveSlots() => new List<string>(_activeSlotNodes);
        public int GetTotalSpentPoints() => _totalSpentPoints;
    }
}
