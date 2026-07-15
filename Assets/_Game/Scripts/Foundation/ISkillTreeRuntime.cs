using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    /// <summary>
    /// Core-side port for the skill tree runtime lifecycle. Keeps GameBootstrap from depending on
    /// the Skills module while preserving the existing progression rebind sequence
    /// (arch: Core|Skills, spec_arch_core_skills_cycle_reduction_v34_followup).
    ///
    /// arch: quebra dos pares mutuos Combat|Skills e Player|Skills (2026-07-15) — os dois membros
    /// abaixo foram adicionados para que PlayerAttackController/PlayerDamageReceiver (Combat) e
    /// InferredClassRuntime/PlayerVitalsApplier (Player) resolvam o skill tree via este port em vez
    /// do tipo concreto SkillTreeManager.
    /// </summary>
    public interface ISkillTreeRuntime
    {
        /// <summary>
        /// Re-binds this skill tree runtime to the currently active PlayerProgressionManager
        /// (resolved internally via its own static Instance, same molde as Craft/Economy/Skills)
        /// and pushes the current unspent skill points into the skill tree state.
        /// </summary>
        void RebindProgressionManager();

        /// <summary>All currently active passive modifiers (rank-scaled), consumed by DerivedStatsCalculator.</summary>
        List<SkillPassiveModifier> GetAllActivePassiveModifiers();

        /// <summary>Skill points spent in the given tree id (e.g. "melee"), used by class inference.</summary>
        int GetPointsSpentInTree(string treeId);
    }
}
