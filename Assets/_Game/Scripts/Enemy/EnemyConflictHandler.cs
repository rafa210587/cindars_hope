using CindarsHope.Combat;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_78 (SLICE 4) â€” Lida com o ecossistema de conflito inter-monstro.
    /// Isolado por design: o caminho player-only do EnemyBrain nÃ£o Ã© tocado quando
    /// nenhum CaveConflictCombatant estÃ¡ presente neste inimigo.
    /// NÃ£o Ã© MonoBehaviour; instanciado e possuÃ­do pelo EnemyBrain.
    /// </summary>
    internal sealed class EnemyConflictHandler
    {
        // â”€â”€â”€ ReferÃªncias injetadas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private Transform _transform;
        private EnemyDataSO _enemyData;
        private CindarsHope.Combat.EnemyHealth _health;

        // Callbacks para acessar estado do EnemyBrain
        private System.Func<float> _getDetectionRange;
        private System.Func<float> _getPhaseDamageMultiplier;
        private System.Func<EnemyActionSO> _getPendingAction;

        // â”€â”€â”€ Estado de conflito â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // fable_78 (SLICE 4): conflito inter-monstro. _conflictCombatant != null SOMENTE na visita em que
        // este inimigo Ã© marcado como rival (injetado pelo materializer). Quando null, o caminho de
        // targeting/dano Ã© EXATAMENTE o player-only existente (byte-for-byte). Quando presente, o alvo
        // hostil vÃ¡lido Ã© {player} âˆª {rivais vivos}; o ramo de rival Ã© totalmente isolado por este guard.
        private CindarsHope.Cave.Ecosystem.CaveConflictCombatant _conflictCombatant;
        private CindarsHope.Cave.Data.CaveEcosystemBalanceSO _ecosystemBalance;

        // Alvo rival corrente desta decisÃ£o (null = mirando o player). Quando nÃ£o-null, _playerTarget Ã©
        // apontado para o GameObject do rival para REUSAR o locomotor/estado existente; sÃ³ a resoluÃ§Ã£o de
        // dano diverge (TakeDamageFromEnemy em vez de PlayerDamageReceiver).
        private CindarsHope.Combat.EnemyHealth _rivalHealthTarget;

        // â”€â”€â”€ Init â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Inicializa o handler com referÃªncias do EnemyBrain.
        /// Deve ser chamado em Awake e re-chamado quando necessÃ¡rio.
        /// </summary>
        internal void Init(
            Transform transform,
            EnemyDataSO enemyData,
            CindarsHope.Combat.EnemyHealth health,
            System.Func<float> getDetectionRange,
            System.Func<float> getPhaseDamageMultiplier,
            System.Func<EnemyActionSO> getPendingAction)
        {
            _transform = transform;
            _enemyData = enemyData;
            _health = health;
            _getDetectionRange = getDetectionRange;
            _getPhaseDamageMultiplier = getPhaseDamageMultiplier;
            _getPendingAction = getPendingAction;
        }

        /// <summary>Atualiza referÃªncias de dados apÃ³s ConfigureRuntime do EnemyBrain.</summary>
        internal void UpdateRefs(EnemyDataSO enemyData, CindarsHope.Combat.EnemyHealth health)
        {
            _enemyData = enemyData;
            _health = health;
        }

        // â”€â”€â”€ API pÃºblica de configuraÃ§Ã£o â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// fable_78 (SLICE 4) â€” liga este handler ao conflito inter-monstro. Chamado pelo materializer
        /// logo apÃ³s anexar o CaveConflictCombatant ao inimigo (injeÃ§Ã£o explÃ­cita; sem scene search).
        /// </summary>
        internal void ConfigureConflict(
            CindarsHope.Cave.Ecosystem.CaveConflictCombatant combatant,
            CindarsHope.Cave.Data.CaveEcosystemBalanceSO balance)
        {
            _conflictCombatant = combatant;
            _ecosystemBalance = balance;
        }

        /// <summary>Resolve o CaveConflictCombatant via GetComponent (para o re-resolve no OnEnable).</summary>
        internal void TryResolveCombatant(System.Func<CindarsHope.Cave.Ecosystem.CaveConflictCombatant> resolve)
        {
            if (_conflictCombatant == null)
            {
                _conflictCombatant = resolve();
            }
            _rivalHealthTarget = null;
        }

        /// <summary>Registra o combatant diretamente (do Awake do EnemyBrain).</summary>
        internal void SetCombatant(CindarsHope.Cave.Ecosystem.CaveConflictCombatant combatant)
        {
            _conflictCombatant = combatant;
        }

        // â”€â”€â”€ Targeting â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// fable_78 (SLICE 4): escolhe o alvo hostil corrente entre {player} âˆª {rivais vivos}, ponderado
        /// por PlayerAggroWeight/RivalAggroWeight. Quando um rival vence, retorna o GameObject do rival
        /// para que EnemyBrain aponte _playerTarget e registre _rivalHealthTarget. Quando o player vence
        /// (ou nÃ£o hÃ¡ rival vivo), retorna null (= usar caminho player-only).
        /// Ramo isolado: no-op se este inimigo nÃ£o Ã© conflict-combatant.
        /// </summary>
        /// <param name="playerGo">GameObject do player visÃ­vel (pode ser null).</param>
        /// <param name="currentPlayerTarget">_playerTarget atual do EnemyBrain.</param>
        /// <param name="rivalTarget">SaÃ­da: EnemyHealth do rival (null = mirando player).</param>
        /// <returns>O GameObject que deve ser o _playerTarget; null = sem mudanÃ§a.</returns>
        internal GameObject RefreshConflictTarget(
            GameObject playerGo,
            GameObject currentPlayerTarget,
            out CindarsHope.Combat.EnemyHealth rivalTarget)
        {
            _rivalHealthTarget = null;
            rivalTarget = null;

            if (_conflictCombatant == null)
            {
                return null; // no-op: caminho player-only intacto
            }

            float detection = _getDetectionRange();
            var rival = _conflictCombatant.FindNearestLivingRival(_transform.position, detection);
            if (rival == null)
            {
                // Sem rival vivo no raio â†’ mira o player como sempre.
                return playerGo; // restaura playerGo (pode ser o mesmo jÃ¡)
            }

            float playerWeighted = ResolveWeightedDistance(playerGo, _ecosystemBalance?.PlayerAggroWeight ?? 1f);
            float rivalDist = Vector2.Distance(_transform.position, rival.transform.position);
            float rivalWeighted = ResolveWeightedDistance(rivalDist, _ecosystemBalance?.RivalAggroWeight ?? 1f);

            if (rivalWeighted <= playerWeighted)
            {
                _rivalHealthTarget = rival;
                rivalTarget = rival;
                return rival.gameObject; // reusa movimento/distÃ¢ncia/estado existentes
            }
            else if (playerGo != null)
            {
                return playerGo;
            }

            return null;
        }

        // â”€â”€â”€ Dano inter-monstro â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// fable_78 (SLICE 4): resolve um ataque contra o rival corrente. Re-checa alcance (o rival pode ter
        /// se movido durante o windup, igual ao caminho do player) e roteia o dano base do action pelo
        /// caminho de origem-inimigo do EnemyHealth (Ã—InterMonsterDamageMultiplier + "Ferido" + kill-by-enemy).
        /// Reusa o mesmo locomotor/estado: nÃ£o cria projÃ©til/pathfinding novo (Ã¡rea/ranged tratam como hit direto).
        /// </summary>
        internal void ResolveInterMonsterAction(DamageType dmgType)
        {
            var rival = _rivalHealthTarget;
            var pendingAction = _getPendingAction();
            if (rival == null || rival.IsDead || _ecosystemBalance == null || pendingAction == null)
            {
                return;
            }

            float dist = Vector2.Distance(_transform.position, rival.transform.position);
            float effectiveRange = pendingAction.ActionType == EnemyActionType.AreaPulse && pendingAction.AreaRadius > 0f
                ? pendingAction.AreaRadius
                : pendingAction.Range;
            if (dist > effectiveRange * 1.2f)
            {
                return;
            }

            int rawDamage = Mathf.Max(0, Mathf.RoundToInt(pendingAction.BaseDamage * _getPhaseDamageMultiplier()));
            if (rawDamage <= 0)
            {
                return;
            }

            int caveLevel = _conflictCombatant != null ? _conflictCombatant.CaveLevel : 0;
            string killerInstanceId = _health != null ? _health.EnemyInstanceId : (_transform != null ? _transform.gameObject.name : "unknown");

            // arch: TakeDamageFromEnemy recebe os multiplicadores como float primitivo (nao mais o
            // CaveEcosystemBalanceSO inteiro) — corte do par mutuo Cave|Combat; este caller (Enemy)
            // ja nomeia CindarsHope.Cave.Data diretamente, entao le os campos aqui sem custo extra.
            rival.TakeDamageFromEnemy(
                rawDamage, dmgType, killerInstanceId, caveLevel,
                _ecosystemBalance.WoundedDefenseMultiplier, _ecosystemBalance.WoundedDurationSeconds,
                _ecosystemBalance.InterMonsterDamageMultiplier, _ecosystemBalance.InterMonsterKillLootMultiplier);
        }

        // â”€â”€â”€ Propriedades de consulta â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// fable_78: true quando o alvo corrente Ã© um rival (conflito), nÃ£o o player.
        /// Byte-for-byte preservado: _rivalHealthTarget != null && !_rivalHealthTarget.IsDead.
        /// </summary>
        internal bool IsTargetingRival => _rivalHealthTarget != null && !_rivalHealthTarget.IsDead;

        /// <summary>True quando este inimigo tem um combatant de conflito registrado.</summary>
        internal bool HasConflictCombatant => _conflictCombatant != null;

        // â”€â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // DistÃ¢ncia "ponderada" por peso de aggro: peso menor torna o alvo mais atraente (divide a distÃ¢ncia).
        // Peso <= 0 desliga o alvo (distÃ¢ncia infinita). Peso 1 = distÃ¢ncia crua (default empate = mais prÃ³ximo).
        private float ResolveWeightedDistance(GameObject target, float weight)
        {
            if (target == null)
            {
                return float.MaxValue;
            }

            return ResolveWeightedDistance(Vector2.Distance(_transform.position, target.transform.position), weight);
        }

        private static float ResolveWeightedDistance(float distance, float weight)
        {
            return weight <= 0f ? float.MaxValue : distance / weight;
        }
    }
}
