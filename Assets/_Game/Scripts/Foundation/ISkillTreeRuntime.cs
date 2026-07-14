namespace CindarsHope.Foundation
{
    /// <summary>
    /// Core-side port for the skill tree runtime lifecycle. Keeps GameBootstrap from depending on
    /// the Skills module while preserving the existing progression rebind sequence
    /// (arch: Core|Skills, spec_arch_core_skills_cycle_reduction_v34_followup).
    /// </summary>
    public interface ISkillTreeRuntime
    {
        /// <summary>
        /// Re-binds this skill tree runtime to the currently active PlayerProgressionManager
        /// (resolved internally via its own static Instance, same molde as Craft/Economy/Skills)
        /// and pushes the current unspent skill points into the skill tree state.
        /// </summary>
        void RebindProgressionManager();
    }
}
