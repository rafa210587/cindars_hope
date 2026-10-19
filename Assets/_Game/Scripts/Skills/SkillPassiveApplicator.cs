using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills
{
    // Tracks which passive modifiers are currently active based on purchased nodes.
    //
    // fable_29: SUPERSEDED by SkillEffectAggregator, which is the single recompute authority
    // (rank-scaled modifiers + named hooks + equip gate). This class is retained only for
    // backward compatibility and is no longer used by SkillTreeManager. Do not wire into new code.
    [System.Obsolete("fable_29: use SkillEffectAggregator (rank-scaled, named hooks, equip gate).")]
    public class SkillPassiveApplicator
    {
        private readonly Dictionary<string, SkillNodeDataSO> _nodeIndex;
        private readonly List<SkillPassiveModifier> _activeModifiers = new List<SkillPassiveModifier>();

        public SkillPassiveApplicator(Dictionary<string, SkillNodeDataSO> nodeIndex)
        {
            _nodeIndex = nodeIndex;
        }

        public void Apply(SkillNodeDataSO node, SkillTreeState state)
        {
            if (node == null) return;
            if (node.SkillCategory != SkillCategory.PassiveSkill &&
                node.SkillCategory != SkillCategory.CapstonePassive) return;
            if (node.PassiveModifiers == null || node.PassiveModifiers.Count == 0) return;

            foreach (var mod in node.PassiveModifiers)
                _activeModifiers.Add(mod);
        }

        public void Reset()
        {
            _activeModifiers.Clear();
        }

        public List<SkillPassiveModifier> GetAllActive() => _activeModifiers;

        public float GetTotalModifier(SkillModifierType type)
        {
            float total = 0f;
            foreach (var mod in _activeModifiers)
                if (mod.ModifierType == type) total += mod.Value;
            return total;
        }
    }
}
