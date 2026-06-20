using System.Collections.Generic;

namespace CindarsHope.Skills
{
    // fable_29 — single recompute authority for skill PASSIVE effects.
    //
    // On every purchase/respec/rank change the manager calls Recompute(state). The aggregator:
    //   1. Rebuilds the flat SkillPassiveModifier list (scaled by RANK) that the DerivedStats
    //      provider (F02) consumes — single point of application per consumer.
    //   2. Computes named-hook aggregates (gold/harvest/craft/tool) and publishes them via
    //      SkillModifierHooks (consumer-future routes; "efeito pendente" until wired — item 6).
    //   3. Recomputes the punitive equip-revalidation gate from the live tier snapshot (item 6).
    //
    // This generalizes the F23 "one aggregator, one application point per system" technique.
    public class SkillEffectAggregator
    {
        private readonly Dictionary<string, SkillNodeDataSO> _nodeIndex;
        private readonly List<SkillPassiveModifier> _activeModifiers = new List<SkillPassiveModifier>();

        public SkillEffectAggregator(Dictionary<string, SkillNodeDataSO> nodeIndex)
        {
            _nodeIndex = nodeIndex ?? new Dictionary<string, SkillNodeDataSO>();
        }

        // Flat, rank-scaled modifier list for the DerivedStats provider.
        public List<SkillPassiveModifier> ActiveModifiers => _activeModifiers;

        public float GetTotalModifier(SkillModifierType type)
        {
            float total = 0f;
            foreach (var mod in _activeModifiers)
                if (mod.ModifierType == type) total += mod.Value;
            return total;
        }

        public void Clear()
        {
            _activeModifiers.Clear();
            SkillModifierHooks.ResetAggregates();
            SkillTierEquipGate.Recompute(new Dictionary<string, int>());
        }

        // Recompute everything from the current purchased ranks in `state`.
        public void Recompute(SkillTreeState state)
        {
            _activeModifiers.Clear();
            SkillModifierHooks.ResetAggregates();

            float goldDrop = 0f, harvest = 0f, craftCost = 0f, toolEff = 0f;

            if (state != null)
            {
                foreach (var nodeId in state.PurchasedNodeIds)
                {
                    if (!_nodeIndex.TryGetValue(nodeId, out var node) || node == null) continue;

                    int rank = state.GetRank(nodeId);
                    if (rank < 1) continue;

                    // (1) Stat modifiers → DerivedStats provider, scaled by rank.
                    if (node.SkillCategory == SkillCategory.PassiveSkill ||
                        node.SkillCategory == SkillCategory.CapstonePassive)
                    {
                        if (node.PassiveModifiers != null)
                        {
                            foreach (var mod in node.PassiveModifiers)
                            {
                                if (mod == null) continue;
                                _activeModifiers.Add(new SkillPassiveModifier(mod.ModifierType, mod.Value * rank));
                            }
                        }
                    }

                    // (2) Named-hook routes (consumer-future), scaled by rank.
                    switch (node.EffectRoute)
                    {
                        case SkillEffectRoute.GoldDropModifier:       goldDrop  += node.RoutePayloadPerRank * rank; break;
                        case SkillEffectRoute.HarvestYieldModifier:   harvest   += node.RoutePayloadPerRank * rank; break;
                        case SkillEffectRoute.CraftCostModifier:      craftCost += node.RoutePayloadPerRank * rank; break;
                        case SkillEffectRoute.ToolEfficiencyModifier: toolEff   += node.RoutePayloadPerRank * rank; break;
                    }
                }
            }

            // Publish named hooks (always — read-back for tests + "efeito pendente" tooltip).
            SkillModifierHooks.PublishGoldDropBonus(goldDrop);
            SkillModifierHooks.PublishHarvestYieldBonus(harvest);
            SkillModifierHooks.PublishCraftCostReduction(craftCost);
            SkillModifierHooks.PublishToolEfficiencyBonus(toolEff);

            // (3) Punitive equip-revalidation gate from the live tier snapshot.
            SkillTierEquipGate.Recompute(state != null ? state.SnapshotDeepestTierByTree() : null);
        }
    }
}
