using System;

namespace CindarsHope.Foundation
{
    public static class SalvageSkillModifierProvider
    {
        public const string NodeId = "crafting_salvage_method";
        public const float PerRank = .05f;
        public const float Cap = .15f;

        public static Func<float> ChanceSource;
        public static float CurrentChance => Math.Max(0f, Math.Min(Cap, ChanceSource?.Invoke() ?? 0f));
        public static float Resolve(int rank) => Math.Min(Cap, Math.Max(0, rank) * PerRank);
    }
}
