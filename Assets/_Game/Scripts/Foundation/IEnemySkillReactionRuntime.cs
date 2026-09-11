namespace CindarsHope.Foundation
{
    /// <summary>Cross-domain port used by player skills to query and apply bounded enemy reactions.</summary>
    public interface IEnemySkillReactionRuntime
    {
        EnemyDifficulty Difficulty { get; }
        bool HasEliteClassification { get; }
        bool IsInAttackRecovery { get; }
        bool IsGuardHold { get; }

        void ApplyTemporaryTargetPriority(float sourceX, float sourceY, float seconds);
        void ApplyTemporaryAttraction(float targetX, float targetY, float seconds);
        void CancelTemporaryAttraction();
        bool IsTemporarilyAttracted { get; }
    }
}
