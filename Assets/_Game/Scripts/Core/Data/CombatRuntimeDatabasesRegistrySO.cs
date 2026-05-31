using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Core.Data
{
    /// <summary>
    /// SPEC 14A-FIX10: single resource-loadable registry of every database the runtime
    /// (CaveRuntimeMaterializer, PlayerAttackController, etc.) needs to wire enemies and combat.
    ///
    /// The asset lives under Assets/_Game/Resources/CombatRuntimeDatabasesRegistry.asset so that
    /// installers can load it at runtime via Resources.Load without inspector wiring being the
    /// single point of failure. When CaveScene loses serialized references (re-save, asset move,
    /// merge), the installer reloads the registry and rebinds via public Rebind* methods.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatRuntimeDatabasesRegistry", menuName = "CindarsHope/Data/Combat Runtime Databases Registry")]
    public sealed class CombatRuntimeDatabasesRegistrySO : ScriptableObject
    {
        [Header("Enemy stack")]
        public EnemyDatabaseSO EnemyDatabase;
        public EnemyMovementProfileDatabaseSO MovementProfileDatabase;
        public EnemyActionSetDatabaseSO ActionSetDatabase;
        public EnemyActionDatabaseSO ActionDatabase;
        public EnemyTelegraphProfileDatabaseSO TelegraphDatabase;
        public EnemyVulnerabilityProfileDatabaseSO VulnerabilityProfileDatabase;
        public EnemySizeProfileDatabaseSO SizeProfileDatabase;

        [Header("Player combat stack")]
        public ItemDatabaseSO ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
    }
}
