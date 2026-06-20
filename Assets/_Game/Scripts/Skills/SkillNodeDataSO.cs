using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillNode_", menuName = "CindarsHope/Skills/SkillNode")]
    public class SkillNodeDataSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identity")]
        public string SkillNodeId;
        public string TreeId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Type")]
        public SkillNodeType NodeType = SkillNodeType.PassiveStat;
        public SkillCategory SkillCategory = SkillCategory.PassiveSkill;
        public bool IsCapstone;

        [Header("Cost & Prerequisites")]
        public int SkillPointCost = 1;
        public int MinimumPlayerLevel = 1;
        public List<string> PrerequisiteNodeIds = new List<string>();
        public int RequiredPurchasedNodesInTree = 0;

        // fable_29 (emenda V3): tier of the node (1-5). Tier gating compares points SPENT IN
        // THE TREE against SkillTierRules.TierPointThresholds[Tier]. The dynamic rank cap of
        // every node is driven by the deepest unlocked tier of its tree (decision 1.1).
        [Header("Tier & Rank (fable_29)")]
        public int Tier = 1;

        // fable_29: typed route this node's passive publishes to (decision 1.6).
        // StatModifier → DerivedStats provider (consumer exists); named routes → SkillModifierHooks
        // (consumer future, tooltip "efeito pendente").
        public SkillEffectRoute EffectRoute = SkillEffectRoute.None;

        // Aggregated payload value PER RANK for named-hook routes (gold/harvest/craft/tool).
        // StatModifier routes keep using PassiveModifiers (per-modifier values).
        public float RoutePayloadPerRank = 0f;

        // fable_29 (emenda V3 item 6): when true, the route targets a consumer that does NOT
        // exist yet. The node is still purchasable and the hook is still published; the F14
        // skill tree screen shows EffectPendingTooltip. NOT the same as NotYetExecutable.
        public bool EffectPending = false;
        [TextArea] public string EffectPendingTooltip = string.Empty;

        // fable_29 (decision 1.5): ACTIVE skill with no viable executor yet — dormant,
        // feedback-only, never a parallel executor. Distinct from EffectPending (passives).
        public bool NotYetExecutable = false;

        // fable_29 (CA-3): mutually-exclusive capstone VARIANTS chosen at purchase time
        // (e.g. Melee capstone: "kanthor" XOR "kaand"; Magic capstone: "anya" XOR "senya").
        // Choosing one variant permanently blocks the others on this node until a full respec.
        // Empty = node has no variant choice.
        public List<string> CapstoneVariants = new List<string>();

        [Header("Unlocks")]
        public string UnlockedSkillActionId;
        public string LinkedSpellId;

        [Header("Passive Modifiers")]
        public List<SkillPassiveModifier> PassiveModifiers = new List<SkillPassiveModifier>();

        // Legacy compatibility
        public string Id => SkillNodeId;
        string IIdentifiedData.Id => SkillNodeId;

        private void OnValidate()
        {
            SkillPointCost = Mathf.Max(1, SkillPointCost);
            MinimumPlayerLevel = Mathf.Max(1, MinimumPlayerLevel);
            Tier = Mathf.Clamp(Tier, SkillTierRules.MinTier, SkillTierRules.MaxTier);
        }
    }
}
