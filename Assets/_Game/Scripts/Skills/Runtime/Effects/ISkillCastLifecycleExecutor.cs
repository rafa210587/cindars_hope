namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>Optional lifecycle for effects that must validate continuously before commit.</summary>
    public interface ISkillCastLifecycleExecutor
    {
        void OnCastStarted(SkillEffectContext context);
        SkillEffectResult TickBeforeCommit(SkillEffectContext context, float deltaSeconds);
        void OnCastCancelled(SkillEffectContext context);
    }
}
