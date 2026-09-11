using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    /// <summary>Pure composition for Dodge Training and directional action modifiers.</summary>
    public static class DodgeCostModifierProvider
    {
        public const string DodgeTrainingNodeId = "melee_dodge_training";
        public const float MinimumMultiplier = 0.5f;
        public static Func<float> TrainingReductionSource;

        public static float ResolveCurrent(float directionalCostMultiplier = 1f)
            => Resolve(TrainingReductionSource?.Invoke() ?? 0f, directionalCostMultiplier);

        public static float Resolve(float trainingReduction, float directionalCostMultiplier = 1f)
            => Math.Max(MinimumMultiplier,
                Math.Max(0f, 1f - trainingReduction) * Math.Max(0f, directionalCostMultiplier));

        public static float Resolve(
            IList<SkillPassiveModifier> modifiers,
            float directionalCostMultiplier = 1f)
        {
            float trainingReduction = 0f;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    var modifier = modifiers[i];
                    if (modifier != null
                        && modifier.ModifierType == SkillModifierType.DodgeCostReduction
                        && string.Equals(modifier.SourceNodeId, DodgeTrainingNodeId, StringComparison.Ordinal))
                    {
                        trainingReduction += modifier.Value;
                    }
                }
            }

            return Resolve(trainingReduction, directionalCostMultiplier);
        }
    }
}
