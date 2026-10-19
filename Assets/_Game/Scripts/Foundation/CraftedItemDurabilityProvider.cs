using System;

namespace CindarsHope.Foundation
{
    /// <summary>Typed bridge from the crafting skill rank to crafted item snapshots.</summary>
    public static class CraftedItemDurabilityProvider
    {
        public const string NodeId = "crafting_durable_finish";
        public const float PerRank = .08f;
        public const float Cap = .24f;

        public static Func<float> BonusSource;

        public static float CurrentBonus => Math.Min(Cap, Math.Max(0f, BonusSource?.Invoke() ?? 0f));

        public static float ResolveBonus(int rank)
            => Math.Min(Cap, Math.Max(0, rank) * PerRank);

        public static int ResolveMaxDurability(int baseMaxDurability, float bonus)
        {
            if (baseMaxDurability <= 0) return 0;
            var safeBonus = Math.Min(Cap, Math.Max(0f, bonus));
            return Math.Max(1, (int)Math.Ceiling(baseMaxDurability * (1d + safeBonus)));
        }
    }
}
