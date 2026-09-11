namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>Read-only readiness pass performed before a timed cast begins.</summary>
    public interface IPreparableSkillEffectExecutor
    {
        SkillEffectResult Validate(SkillEffectContext context);
    }
}
