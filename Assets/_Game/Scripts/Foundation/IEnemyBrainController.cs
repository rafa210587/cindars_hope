namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Enemy — porta para consumidores de Combat que precisam
    /// disparar stun/behavior override no EnemyBrain do inimigo alvo, sem nomear
    /// CindarsHope.Enemy.EnemyBrain/EnemyBrainState diretamente. Implementada por EnemyBrain
    /// (Enemy/EnemyBrain.cs); resolvida via GetComponent no mesmo GameObject.
    /// </summary>
    public interface IEnemyBrainController
    {
        /// <summary>True quando o estado atual do brain e GuardHold (guarda ativa).</summary>
        bool IsGuardHold { get; }

        void ApplyStun(float seconds);

        void ApplyExternalBehaviorOverride(
            float speedMultiplier,
            float speedSeconds,
            bool invertMovement,
            float invertSeconds,
            bool forceRetreat,
            float retreatSeconds);
    }
}
