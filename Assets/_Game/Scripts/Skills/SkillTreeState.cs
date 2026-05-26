using System.Collections.Generic;

namespace CindarsHope.Skills
{
    // Runtime state for skill tree: tracks purchased nodes, active slots and respec count.
    public class SkillTreeState
    {
        private readonly HashSet<string> _purchased = new HashSet<string>();
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

        public void Purchase(string nodeId, int cost)
        {
            _purchased.Add(nodeId);
            AvailableSkillPoints -= cost;
            _spentPoints += cost;
        }

        public bool IsPurchased(string nodeId) => _purchased.Contains(nodeId);

        public IReadOnlyCollection<string> PurchasedNodeIds => _purchased;

        public int CountPurchasedInTree(string treeId)
        {
            // Called during purchase validation; requires node metadata from caller.
            // We count by checking the treeId prefix convention in node IDs.
            int count = 0;
            foreach (var id in _purchased)
                if (id.StartsWith(treeId + "_")) count++;
            return count;
        }

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

        public void FullRespec(int restoredPoints)
        {
            _purchased.Clear();
            for (int i = 0; i < _activeSlotSkillActionIds.Length; i++)
                _activeSlotSkillActionIds[i] = null;
            AvailableSkillPoints = restoredPoints;
            _spentPoints = 0;
            RespecCount++;
        }

        public SkillTreeSaveData ToSaveData(System.Func<int, string> slotKeyResolver)
        {
            var data = new SkillTreeSaveData
            {
                RespecCount = RespecCount
            };
            foreach (var id in _purchased)
                data.PurchasedNodeIds.Add(id);

            for (int i = 0; i < _activeSlotSkillActionIds.Length; i++)
            {
                data.ActiveSkillSlots.Add(new ActiveSkillSlotSaveEntry(
                    i,
                    slotKeyResolver(i),
                    _activeSlotSkillActionIds[i] ?? string.Empty));
            }
            return data;
        }

        public void LoadFromSaveData(SkillTreeSaveData data, int totalAvailablePoints)
        {
            if (data == null) return;
            _purchased.Clear();
            foreach (var id in data.PurchasedNodeIds)
                _purchased.Add(id);
            _spentPoints = data.PurchasedNodeIds.Count; // 1 point per node
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
    }
}
