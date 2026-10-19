using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Fase E (modularização EnemyBrain) — telemetria de diagnóstico extraída do brain: logs
    /// one-shot e avisos de estado incomum (fable_04 threat memory, fable_24 elite affixes, boss
    /// primitives SPEC 14). Preserva byte-a-byte texto/semântica dos logs originais; não decide
    /// gameplay — apenas relata. Instanciada uma vez por <see cref="EnemyBrain"/> porque o guard
    /// one-shot de threat-expired é por instância, como era antes da extração.
    /// </summary>
    public class EnemyDebugTelemetry
    {
        // fable_04: evita repetir o log de expiração de threat a cada tick fora do leash.
        private bool _threatExpiredLogged;

        /// <summary>Reseta o guard one-shot de threat-expired (no (re)spawn e ao reengajar o alvo).</summary>
        public void ResetThreatExpiredLog()
        {
            _threatExpiredLogged = false;
        }

        public void LogThreatExpiredOnce(string enemyId, string packId, Object context)
        {
            if (_threatExpiredLogged)
            {
                return;
            }

            _threatExpiredLogged = true;
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EnemyThreatExpired. EnemyId={enemyId}, PackId={packId ?? "none"}.", context);
        }

        public void LogBossSwapActionSetMissing(string enemyId, string actionSetId, Object context)
        {
            Debug.LogWarning($"CombatLog: BossSwapActionSetMissing. EnemyId={enemyId}, ActionSetId={actionSetId}.", context);
        }

        public void LogEliteWardedResisted(string enemyId, Object context)
        {
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EliteWardedResistedStatus. EnemyId={enemyId}, Affix=Warded.", context);
        }

        public void LogEliteVolatileExploding(string enemyId, int damage, int playerMaxHp, float telegraphSeconds, Object context)
        {
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EliteVolatileExploding. EnemyId={enemyId}, Damage={damage}, CapMaxHp={playerMaxHp}, Telegraph={telegraphSeconds:F2}s.", context);
        }

        public void LogEnemyPackLeashReset(string enemyId, string packId, Vector2 anchor, Object context)
        {
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EnemyPackLeashReset. EnemyId={enemyId}, PackId={packId}, Anchor=({anchor.x:F2},{anchor.y:F2}).", context);
        }
    }
}
