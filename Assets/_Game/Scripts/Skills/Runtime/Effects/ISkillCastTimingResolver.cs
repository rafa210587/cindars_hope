namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>Optional rank-aware timing override evaluated after readiness and before cast start.</summary>
    public interface ISkillCastTimingResolver
    {
        float ResolveWindupSeconds(SkillEffectContext context, float authoredWindupSeconds);
    }
}
