namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Contract for all skill effect executors.
    // Each executor handles a specific EffectId and applies the corresponding gameplay change.
    // Executors must NOT know about UI, active slots, or cost/cooldown management.
    // They receive a fully-resolved SkillEffectContext and return a SkillEffectResult.
    public interface ISkillEffectExecutor
    {
        // Stable effect identifier this executor handles (e.g. "farm.crop.water_skill").
        string EffectId { get; }

        // Category for routing and debugging.
        SkillEffectCategory Category { get; }

        // Target type this executor expects.
        SkillEffectTargetType TargetType { get; }

        // Execute the effect. Must not apply cost or cooldown — that is done by ActiveSkillExecutionController.
        SkillEffectResult Execute(SkillEffectContext context);
    }
}
