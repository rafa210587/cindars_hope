namespace CindarsHope.Skills
{
    // fable_29 — typed routing for skill passive effects.
    // Each canonical passive node declares ONE SkillEffectRoute so the SkillEffectAggregator
    // knows which real system (or named-hook contract) the modifier publishes to.
    //
    // Routes that target an EXISTING consumer (DerivedStats provider F02) apply immediately.
    // Routes that target a FUTURE consumer (gold/harvest/craft/tool) publish a NAMED HOOK
    // via SkillModifierHooks; the node carries an "efeito pendente" tooltip until the
    // consumer spec (F06/F17/F31/F48/F49/F55) wires the hook (emenda V3 item 6).
    public enum SkillEffectRoute
    {
        // No route declared — node has no aggregated passive (e.g. pure active-skill unlock).
        None = 0,

        // Routed to the DerivedStats provider (F02) via DerivedStatsCalculator — consumer EXISTS.
        StatModifier,

        // Routed to a named hook, consumer FUTURE (tooltip "efeito pendente" until wired):
        GoldDropModifier,        // IGoldDropModifier      — F06 economia/loot
        HarvestYieldModifier,    // IHarvestYieldModifier  — F17 farm
        CraftCostModifier,       // ICraftCostModifier     — F31 craft
        ToolEfficiencyModifier,  // IToolEfficiencyModifier— F48/F49 ferramentas/recursos

        // Active skill unlock (executor lives in ActiveSkillExecutionController; not a passive).
        ActiveSkill,

        // Pointwise gameplay hook (e.g. perfect-block proc F27, status interaction F01).
        EventHook
    }

    // Named hook contracts (emenda V3 item 6). A future consumer spec implements one of these
    // and registers it with SkillModifierHooks; until then the aggregated value is merely
    // published and read-back via the static accessor for tests + "efeito pendente" tooltip.
    public interface IGoldDropModifier
    {
        // Multiplicative bonus to gold drops (0.10 == +10%). Aggregated across purchased ranks.
        void ApplyGoldDropBonus(float aggregatedBonus);
    }

    public interface IHarvestYieldModifier
    {
        void ApplyHarvestYieldBonus(float aggregatedBonus);
    }

    public interface ICraftCostModifier
    {
        void ApplyCraftCostReduction(float aggregatedReduction);
    }

    public interface IToolEfficiencyModifier
    {
        void ApplyToolEfficiencyBonus(float aggregatedBonus);
    }
}
