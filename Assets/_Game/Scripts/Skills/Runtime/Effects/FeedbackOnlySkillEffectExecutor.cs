namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH: Generic feedback-only executor.
    // Used for active skills that are registered in the catalog but whose runtime effect
    // depends on combat, utility, or farm systems not yet available in this wave.
    //
    // Status: DEFERRED_RUNTIME_EFFECT
    // TODO_INTEGRATION_NOT_FINAL: Replace with a concrete executor when the target runtime system
    // (combat, utility, or farm) is implemented. This executor always returns Success so the
    // active slot cooldown triggers and the player receives visual feedback.
    //
    // Blocks final acceptance: NO (authoring and pipeline are registered; effect is placeholder)
    public sealed class FeedbackOnlySkillEffectExecutor : ISkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _feedbackMessage;
        private readonly SkillEffectCategory _category;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => _category;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public FeedbackOnlySkillEffectExecutor(string effectId, string feedbackMessage, SkillEffectCategory category)
        {
            _effectId = effectId;
            _feedbackMessage = feedbackMessage;
            _category = category;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            // Returns success with feedback message so active slot cooldown triggers.
            // No actual gameplay effect applied; integration deferred.
            return SkillEffectResult.Succeeded(_feedbackMessage, costSpent: false, cooldownStarted: true);
        }
    }
}
