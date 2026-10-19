namespace CindarsHope.Skills
{
    // fable_29 — static registry of NAMED skill-effect hooks (emenda V3 item 6).
    //
    // The SkillEffectAggregator computes the aggregated bonus for each named route and
    // publishes it here. A future consumer spec (F06 gold, F17 harvest, F31 craft,
    // F48/F49 tools) registers its implementer; until then the value is merely stored
    // and read back (tests + "efeito pendente" tooltip). This is a single, search-free
    // accessor point — no GameObject.Find, no per-frame scan.
    //
    // Aggregated values are LAST-WRITER-WINS per recompute (the aggregator clears and
    // re-publishes on every purchase/respec), exactly mirroring the DerivedStats provider
    // recompute discipline.
    public static class SkillModifierHooks
    {
        // ── Aggregated values (always available for read-back; "efeito pendente" until consumed) ──
        public static float GoldDropBonus { get; private set; }
        public static float HarvestYieldBonus { get; private set; }
        public static float CraftCostReduction { get; private set; }
        public static float ToolEfficiencyBonus { get; private set; }

        // ── Optional consumers (registered by a future spec; null = "efeito pendente") ──
        public static IGoldDropModifier GoldConsumer;
        public static IHarvestYieldModifier HarvestConsumer;
        public static ICraftCostModifier CraftConsumer;
        public static IToolEfficiencyModifier ToolConsumer;

        public static bool GoldConsumerExists => GoldConsumer != null;
        public static bool HarvestConsumerExists => HarvestConsumer != null;
        public static bool CraftConsumerExists => CraftConsumer != null;
        public static bool ToolConsumerExists => ToolConsumer != null;

        public static void PublishGoldDropBonus(float aggregated)
        {
            GoldDropBonus = aggregated;
            GoldConsumer?.ApplyGoldDropBonus(aggregated);
        }

        public static void PublishHarvestYieldBonus(float aggregated)
        {
            HarvestYieldBonus = aggregated;
            HarvestConsumer?.ApplyHarvestYieldBonus(aggregated);
        }

        public static void PublishCraftCostReduction(float aggregated)
        {
            CraftCostReduction = aggregated;
            CraftConsumer?.ApplyCraftCostReduction(aggregated);
        }

        public static void PublishToolEfficiencyBonus(float aggregated)
        {
            ToolEfficiencyBonus = aggregated;
            ToolConsumer?.ApplyToolEfficiencyBonus(aggregated);
        }

        // Reset all aggregated values to zero (called by the aggregator before recompute).
        public static void ResetAggregates()
        {
            GoldDropBonus = 0f;
            HarvestYieldBonus = 0f;
            CraftCostReduction = 0f;
            ToolEfficiencyBonus = 0f;
        }

        // Test/teardown helper: clears both aggregates and registered consumers.
        public static void ResetAll()
        {
            ResetAggregates();
            GoldConsumer = null;
            HarvestConsumer = null;
            CraftConsumer = null;
            ToolConsumer = null;
        }
    }
}
