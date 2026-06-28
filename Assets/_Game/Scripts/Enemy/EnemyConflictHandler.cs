using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_78 (SLICE 4) — Lida com o ecossistema de conflito inter-monstro.
    /// Isolado por design: o caminho player-only do EnemyBrain não é tocado quando
    /// nenhum CaveConflictCombatant está presente neste inimigo.
    /// Não é MonoBehaviour; instanciado e possuído pelo EnemyBrain.
    /// </summary>
    internal sealed class EnemyConflictHandler
    {
        // ─── Referências injetadas ────────────────────────────────────────────

        private Transform _transform;
        private EnemyDataSO _enemyData;
        private CindarsHope.Combat.EnemyHealth _health;

        // Callbacks para acessar estado do EnemyBrain
        private System.Func<float> _getDetectionRange;
        private System.Func<float> _getPhaseDamageMultiplier;
        private System.Func<EnemyActionSO> _getPendingAction;

        // ─── Estado de conflito ───────────────────────────────────────────────

        // fable_78 (SLICE 4): conflito inter-monstro. _conflictCombatant != null SOMENTE na visita em que
        // este inimigo é marcado como rival (injetado pelo materializer). Quando null, o caminho de
        // targeting/dano é EXATAMENTE o player-only existente (byte-for-byte). Quando presente, o alvo
        // hostil válido é {player} ∪ {rivais vivos}; o ramo de rival é totalmente isolado por este guard.
        private CindarsHope.Cave.Ecosystem.CaveConflictCombatant _conflictCombatant;
        private CindarsHope.Cave.Data.CaveEcosystemBalanceSO _ecosystemBalance;

        // Alvo rival corrente desta decisão (null = mirando o player). Quando não-null, _playerTarget é
        // apontado para o GameObject do rival para REUSAR o locomotor/estado existente; só a resolução de
        // dano diverge (TakeDamageFromEnemy em vez de PlayerDamageReceiver).
        private CindarsHope.Combat.EnemyHealth _rivalHealthTarget;

        // ─── Init ────────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa o handler com referências do EnemyBrain.
        /// Deve ser chamado em Awake e re-chamado quando necessário.
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

        /// <summary>Atualiza referências de dados após ConfigureRuntime do EnemyBrain.</summary>
        internal void UpdateRefs(EnemyDataSO enemyData, CindarsHope.Combat.EnemyHealth health)
        {
            _enemyData = enemyData;
            _health = health;
        }

        // ─── API pública de configuração ──────────────────────────────────────

        /// <summary>
        /// fable_78 (SLICE 4) — liga este handler ao conflito inter-monstro. Chamado pelo materializer
        /// logo após anexar o CaveConflictCombatant ao inimigo (injeção explícita; sem scene search).
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

        // ─── Targeting ────────────────────────────────────────────────────────

        /// <summary>
        /// fable_78 (SLICE 4): escolhe o alvo hostil corrente entre {player} ∪ {rivais vivos}, ponderado
        /// por PlayerAggroWeight/RivalAggroWeight. Quando um rival vence, retorna o GameObject do rival
        /// para que EnemyBrain aponte _playerTarget e registre _rivalHealthTarget. Quando o player vence
        /// (ou não há rival vivo), retorna null (= usar caminho player-only).
        /// Ramo isolado: no-op se este inimigo não é conflict-combatant.
        /// </summary>
        /// <param name="playerGo">GameObject do player visível (pode ser null).</param>
        /// <param name="currentPlayerTarget">_playerTarget atual do EnemyBrain.</param>
        /// <param name="rivalTarget">Saída: EnemyHealth do rival (null = mirando player).</param>
        /// <returns>O GameObject que deve ser o _playerTarget; null = sem mudança.</returns>
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
                // Sem rival vivo no raio → mira o player como sempre.
                return playerGo; // restaura playerGo (pode ser o mesmo já)
            }

            float playerWeighted = ResolveWeightedDistance(playerGo, _ecosystemBalance?.PlayerAggroWeight ?? 1f);
            float rivalDist = Vector2.Distance(_transform.position, rival.transform.position);
            float rivalWeighted = ResolveWeightedDistance(rivalDist, _ecosystemBalance?.RivalAggroWeight ?? 1f);

            if (rivalWeighted <= playerWeighted)
            {
                _rivalHealthTarget = rival;
                rivalTarget = rival;
                return rival.gameObject; // reusa movimento/distância/estado existentes
            }
            else if (playerGo != null)
            {
                return playerGo;
            }

            return null;
        }

        // ─── Dano inter-monstro ───────────────────────────────────────────────

        /// <summary>
        /// fable_78 (SLICE 4): resolve um ataque contra o rival corrente. Re-checa alcance (o rival pode ter
        /// se movido durante o windup, igual ao caminho do player) e roteia o dano base do action pelo
        /// caminho de origem-inimigo do EnemyHealth (×InterMonsterDamageMultiplier + "Ferido" + kill-by-enemy).
        /// Reusa o mesmo locomotor/estado: não cria projétil/pathfinding novo (área/ranged tratam como hit direto).
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

            rival.TakeDamageFromEnemy(rawDamage, dmgType, killerInstanceId, caveLevel, _ecosystemBalance);
        }

        // ─── Propriedades de consulta ─────────────────────────────────────────

        /// <summary>
        /// fable_78: true quando o alvo corrente é um rival (conflito), não o player.
        /// Byte-for-byte preservado: _rivalHealthTarget != null && !_rivalHealthTarget.IsDead.
        /// </summary>
        internal bool IsTargetingRival => _rivalHealthTarget != null && !_rivalHealthTarget.IsDead;

        /// <summary>True quando este inimigo tem um combatant de conflito registrado.</summary>
        internal bool HasConflictCombatant => _conflictCombatant != null;

        // ─── Helpers ─────────────────────────────────────────────────────────

        // Distância "ponderada" por peso de aggro: peso menor torna o alvo mais atraente (divide a distância).
        // Peso <= 0 desliga o alvo (distância infinita). Peso 1 = distância crua (default empate = mais próximo).
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
