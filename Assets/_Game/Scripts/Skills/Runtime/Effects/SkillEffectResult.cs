namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Result returned by ISkillEffectExecutor.Execute().
    public sealed class SkillEffectResult
    {
        public bool Success { get; private set; }
        public string FailureReason { get; private set; }
        public string FeedbackMessage { get; private set; }
        public bool CostSpent { get; private set; }
        public bool CooldownStarted { get; private set; }

        // Optional executor-suggested cooldown. <= 0 means "use the controller default".
        public float CooldownSeconds { get; private set; }

        public static SkillEffectResult Succeeded(string feedbackMessage, bool costSpent = false, bool cooldownStarted = false, float cooldownSeconds = 0f)
        {
            return new SkillEffectResult
            {
                Success = true,
                FeedbackMessage = feedbackMessage ?? "Skill applied.",
                CostSpent = costSpent,
                CooldownStarted = cooldownStarted,
                CooldownSeconds = cooldownSeconds
            };
        }

        public static SkillEffectResult Failed(string failureReason, string feedbackMessage = null)
        {
            return new SkillEffectResult
            {
                Success = false,
                FailureReason = failureReason ?? "Unknown failure.",
                FeedbackMessage = feedbackMessage ?? failureReason ?? "Skill failed."
            };
        }
    }
}
