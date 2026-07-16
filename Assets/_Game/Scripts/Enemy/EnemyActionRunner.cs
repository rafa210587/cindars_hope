using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Executa ações de combate do inimigo (windup, resolve, dano, status), delegado pelo EnemyBrain.
    /// Recebe referências via Init(); não é MonoBehaviour.
    /// </summary>
    internal sealed class EnemyActionRunner : IEnemyActionExecutionContext
    {
        // ─── Referências injetadas ────────────────────────────────────────────

        private Transform _transform;
        private EnemyDataSO _enemyData;
        private EnemyActionSetDatabaseSO _actionSetDatabase;
        private EnemyActionDatabaseSO _actionDatabase;
        private EnemyTelegraphProfileDatabaseSO _telegraphDatabase;
        private EnemyVulnerabilityProfileSO _vulnerabilityProfile;
        private EnemyTelegraphController _telegraph;
        private EnemyVulnerabilityState _vulnerabilityState;
        private CindarsHope.Combat.EnemyHealth _health;
        private IEnemyActionSelectionStrategy _selectionStrategy =
            new OrderedReadyEnemyActionSelectionStrategy();
        private readonly EnemyActionExecutionStrategyRegistry _executionStrategies =
            EnemyActionExecutionStrategyRegistry.Default;

        // Callbacks para acessar estado do EnemyBrain
        private System.Func<GameObject> _getPlayerTarget;
        private System.Func<float> _getDistanceToPlayer;
        private System.Func<Vector2> _getDirectionToPlayer;
        private System.Func<float> _getPhaseDamageMultiplier;
        private System.Func<bool> _isTargetingRival;
        private System.Action<DamageType> _resolveInterMonsterAction;
        private System.Func<float> _getDetectionRange;

        // ─── Estado de ação (antes em EnemyBrain) ────────────────────────────

        /// <summary>Set de ações ativo neste inimigo.</summary>
        internal EnemyActionSetSO ActiveActionSet;
        /// <summary>Cooldowns de ações individuais.</summary>
        internal readonly Dictionary<string, EnemyActionRuntime> ActionCooldowns = new Dictionary<string, EnemyActionRuntime>();
        /// <summary>Ação aguardando resolução (em windup ou recover).</summary>
        internal EnemyActionSO PendingAction;
        /// <summary>Timer genérico de windup/recover.</summary>
        internal float ActionTimer;
        /// <summary>True após o windup ter sido resolvido (evita double-fire).</summary>
        internal bool ActionResolved;
        /// <summary>SPEC 13D: idempotency guard — death-trigger fires exactly once per lifetime.</summary>
        internal bool DeathtriggerFired;

        // fable_83: ComboStrike state
        private int _pendingComboHits;
        private float _nextComboHitTime;
        private int _comboTotalHits;
        private int _comboCurrentHit;
        private int _comboBaseDamage;
        private DamageType _comboDmgType;
        private EnemyActionSO _comboAction;
        private const float ComboHitIntervalSeconds = 0.15f;

        // ─── Init ────────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa o runner com referências do EnemyBrain.
        /// Deve ser chamado em Awake e re-chamado em ConfigureRuntime.
        /// </summary>
        internal void Init(
            Transform transform,
            EnemyDataSO enemyData,
            EnemyActionSetDatabaseSO actionSetDatabase,
            EnemyActionDatabaseSO actionDatabase,
            EnemyTelegraphProfileDatabaseSO telegraphDatabase,
            EnemyVulnerabilityProfileSO vulnerabilityProfile,
            EnemyTelegraphController telegraph,
            EnemyVulnerabilityState vulnerabilityState,
            CindarsHope.Combat.EnemyHealth health,
            System.Func<GameObject> getPlayerTarget,
            System.Func<float> getDistanceToPlayer,
            System.Func<Vector2> getDirectionToPlayer,
            System.Func<float> getPhaseDamageMultiplier,
            System.Func<bool> isTargetingRival,
            System.Action<DamageType> resolveInterMonsterAction,
            System.Func<float> getDetectionRange)
        {
            _transform = transform;
            _enemyData = enemyData;
            _actionSetDatabase = actionSetDatabase;
            _actionDatabase = actionDatabase;
            _telegraphDatabase = telegraphDatabase;
            _vulnerabilityProfile = vulnerabilityProfile;
            _telegraph = telegraph;
            _vulnerabilityState = vulnerabilityState;
            _health = health;
            _getPlayerTarget = getPlayerTarget;
            _getDistanceToPlayer = getDistanceToPlayer;
            _getDirectionToPlayer = getDirectionToPlayer;
            _getPhaseDamageMultiplier = getPhaseDamageMultiplier;
            _isTargetingRival = isTargetingRival;
            _resolveInterMonsterAction = resolveInterMonsterAction;
            _getDetectionRange = getDetectionRange;
        }

        /// <summary>Atualiza referências de dados após ConfigureRuntime do EnemyBrain.</summary>
        internal void UpdateRefs(
            EnemyDataSO enemyData,
            EnemyActionSetDatabaseSO actionSetDatabase,
            EnemyActionDatabaseSO actionDatabase,
            EnemyTelegraphProfileDatabaseSO telegraphDatabase,
            EnemyVulnerabilityProfileSO vulnerabilityProfile,
            EnemyTelegraphController telegraph,
            EnemyVulnerabilityState vulnerabilityState,
            CindarsHope.Combat.EnemyHealth health)
        {
            _enemyData = enemyData;
            _actionSetDatabase = actionSetDatabase;
            _actionDatabase = actionDatabase;
            _telegraphDatabase = telegraphDatabase;
            _vulnerabilityProfile = vulnerabilityProfile;
            _telegraph = telegraph;
            _vulnerabilityState = vulnerabilityState;
            _health = health;
        }

        // ─── Action Cooldown Init ─────────────────────────────────────────────

        /// <summary>
        /// Inicializa o set de ações e os cooldowns. Equivalente ao InitActionSet() do EnemyBrain.
        /// </summary>
        internal void InitActionSet()
        {
            ActionCooldowns.Clear();
            ActiveActionSet = null;

            if (_enemyData == null || string.IsNullOrEmpty(_enemyData.ActionSetId)) return;
            if (_actionSetDatabase == null) return;
            if (!_actionSetDatabase.TryGetById(_enemyData.ActionSetId, out ActiveActionSet)) return;
            if (_actionDatabase == null) return;

            EnemyActionSO riseOnceAction = null;
            foreach (var actionId in ActiveActionSet.ActionIds)
            {
                if (_actionDatabase.TryGetById(actionId, out var action))
                {
                    ActionCooldowns[actionId] = new EnemyActionRuntime(actionId, action.CooldownSeconds);
                    if (action.RiseOnceEnabled && riseOnceAction == null)
                        riseOnceAction = action;
                }
            }

            // spec_enemy_attack_kits_v1 (Rise-once, primitiva P2): propaga os parametros do
            // EnemyActionSO marcado RiseOnceEnabled para o EnemyHealth deste inimigo. Sem acao
            // marcada (a maioria dos kits), ConfigureRiseOnce(false, ...) mantem Die() inalterado.
            if (_health != null)
            {
                _health.ConfigureRiseOnce(
                    riseOnceAction != null,
                    riseOnceAction?.RiseOnceHpPercent ?? 0f,
                    riseOnceAction?.RiseOnceBlockedByDamageTypes,
                    riseOnceAction?.RiseOnceCollapseSeconds ?? 0f);
            }
        }

        // ─── Action Selection ─────────────────────────────────────────────────

        /// <summary>
        /// Seleciona a melhor ação disponível para a distância atual.
        /// TODO: mover para EnemyDecisionCore quando Time wrapper for adicionado.
        /// </summary>
        internal EnemyActionSO SelectBestAction(float dist)
        {
            if (ActiveActionSet == null || _actionDatabase == null) return null;

            var context = new EnemyActionSelectionContext(
                ActiveActionSet.ActionIds,
                _actionDatabase,
                ActionCooldowns,
                dist,
                Time.time);
            return _selectionStrategy.Select(in context);
        }

        internal void SetActionSelectionStrategy(IEnemyActionSelectionStrategy strategy)
        {
            _selectionStrategy = strategy ?? new OrderedReadyEnemyActionSelectionStrategy();
        }

        // ─── Action Execution ─────────────────────────────────────────────────

        /// <summary>Inicia o windup de uma ação: define estado, publica eventos, inicia telegraph.</summary>
        internal void BeginAction(EnemyActionSO action, float attackCadenceFactor, ref EnemyBrainState currentState)
        {
            PendingAction = action;
            // fable_24: Frenzied elites attack 30% faster — scale the windup (cadence factor <1).
            ActionTimer = action.WindupSeconds * attackCadenceFactor;
            ActionResolved = false;
            currentState = EnemyBrainState.AttackWindup;

            StartTelegraph(action);
            GameEventBus.Publish(new EnemyActionStartedEvent(_enemyData?.enemyId, action.ActionId));
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, _transform.position));
        }

        /// <summary>
        /// Processa timers de windup/recover e hits pendentes de ComboStrike.
        /// Chamado a cada frame enquanto o inimigo não estiver morto/stunned.
        /// Retorna o novo estado de máquina de estados (pode mudar de AttackWindup → AttackRecover → Chase).
        /// </summary>
        internal void TickActionTimers(ref EnemyBrainState currentState, float attackCadenceFactor)
        {
            // fable_83: resolver hits pendentes do ComboStrike (executados com intervalo fixo).
            if (_pendingComboHits > 0 && _comboAction != null && Time.time >= _nextComboHitTime)
            {
                ApplySingleHitToPlayer(
                    _comboAction,
                    EnemyActionExecution.ComboHitDamage(_comboBaseDamage, _comboCurrentHit, _comboTotalHits),
                    _comboDmgType);
                _comboCurrentHit++;
                _pendingComboHits--;
                _nextComboHitTime = Time.time + ComboHitIntervalSeconds;
                if (_pendingComboHits <= 0) ResetComboState();
            }

            if (currentState == EnemyBrainState.AttackWindup)
            {
                ActionTimer -= Time.deltaTime;
                if (ActionTimer <= 0f && !ActionResolved)
                {
                    ActionResolved = true;
                    ResolveAction();
                    _telegraph?.EndTelegraph();
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.DuringChargeWindup);
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterCast);
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterProjectileVolley);

                    // fable_24: Frenzied also shortens recovery (same cadence factor as windup).
                    ActionTimer = (PendingAction?.RecoverSeconds ?? 0.5f) * attackCadenceFactor;
                    currentState = EnemyBrainState.AttackRecover;
                }
            }
            else if (currentState == EnemyBrainState.AttackRecover)
            {
                ActionTimer -= Time.deltaTime;
                if (ActionTimer <= 0f)
                {
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterAttackRecover);

                    if (PendingAction != null && ActionCooldowns.TryGetValue(PendingAction.ActionId, out var rt))
                        rt.MarkUsed(Time.time);

                    GameEventBus.Publish(new EnemyActionResolvedEvent(_enemyData?.enemyId, PendingAction?.ActionId));
                    PendingAction = null;

                    float dist = _getDistanceToPlayer();
                    float detectionRange = _getDetectionRange != null ? _getDetectionRange() : 10f;
                    currentState = dist <= detectionRange ? EnemyBrainState.Chase : EnemyBrainState.Patrol;
                }
            }
        }

        /// <summary>Resolve o dano/efeito da ação após o windup. Ponto central de damage dispatch.</summary>
        private void ResolveAction()
        {
            if (PendingAction == null) return;
            if (_executionStrategies.TryExecute(this, PendingAction)) return;

            if (PendingAction.BaseDamage <= 0) return;
            if (_getPlayerTarget() == null) return;

            if (!System.Enum.TryParse<DamageType>(PendingAction.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            // fable_78 (SLICE 4): quando o alvo é um rival (conflito inter-monstro), o dano vai para o
            // EnemyHealth do rival via o caminho de origem-inimigo (×0.10 + "Ferido" + kill-by-enemy).
            // Não toca o pipeline de dano ao jogador. Ramo isolado por IsTargetingRival.
            if (_isTargetingRival())
            {
                _resolveInterMonsterAction(dmgType);
                return;
            }

            // SPEC 13D: blink-strike teleports the enemy to the player then deals melee damage.
            if (PendingAction.ActionType == EnemyActionType.BlinkStrike)
            {
                ExecuteBlinkStrike(PendingAction, dmgType);
                return;
            }

            // Ranged and cast actions fire a real dodgeable projectile instead of
            // instant damage — the player can outplay them with movement.
            bool isProjectileAction = PendingAction.ActionType == EnemyActionType.RangedProjectile
                || PendingAction.ActionType == EnemyActionType.CastProjectile;
            if (isProjectileAction)
            {
                float speed = PendingAction.ProjectileSpeed > 0f ? PendingAction.ProjectileSpeed : 5f;
                // fable_05: per-phase damage multiplier folds into the action's base damage.
                int projectileDamage = Mathf.Max(0, Mathf.RoundToInt(PendingAction.BaseDamage * _getPhaseDamageMultiplier()));

                // spec_enemy_attack_kits_v1 (follow-up salvas): ProjectileCount<=1 mantem o caminho
                // single-projectile identico ao anterior (nenhuma mudanca de comportamento). Acima
                // disso, dispara N projeteis em leque (mesmo dano por projetil — cada um e um hit
                // independente esquivavel, o catalogo nao divide dano entre eles).
                if (PendingAction.ProjectileCount <= 1)
                {
                    EnemyProjectileBehaviour.SpawnTowards(
                        _transform.position,
                        _getDirectionToPlayer(),
                        speed,
                        Mathf.Max(PendingAction.Range, 2f),
                        projectileDamage,
                        dmgType,
                        _enemyData?.contactKnockbackForce ?? 0f,
                        _enemyData?.enemyId ?? "enemy",
                        _enemyData?.DisplayName ?? "Enemy");
                }
                else
                {
                    var salvoDirections = EnemyActionExecution.ResolveSalvoDirections(
                        _getDirectionToPlayer(), PendingAction.ProjectileCount, PendingAction.ProjectileSpreadAngleDegrees);
                    for (int i = 0; i < salvoDirections.Length; i++)
                    {
                        EnemyProjectileBehaviour.SpawnTowards(
                            _transform.position,
                            salvoDirections[i],
                            speed,
                            Mathf.Max(PendingAction.Range, 2f),
                            projectileDamage,
                            dmgType,
                            _enemyData?.contactKnockbackForce ?? 0f,
                            _enemyData?.enemyId ?? "enemy",
                            _enemyData?.DisplayName ?? "Enemy");
                    }
                }
                return;
            }

            // Melee/area resolution: the player may have moved during windup. Re-check
            // distance with a small grace margin so dodging the telegraph actually works.
            float dist = _getDistanceToPlayer();
            float effectiveRange = PendingAction.ActionType == EnemyActionType.AreaPulse && PendingAction.AreaRadius > 0f
                ? PendingAction.AreaRadius
                : PendingAction.Range;
            if (dist > effectiveRange * 1.2f)
            {
                CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EnemyActionMissed. EnemyId={_enemyData?.enemyId}, ActionId={PendingAction.ActionId}, Distance={dist:F2}, Range={effectiveRange:F2}");
                return;
            }

            // fable_05: per-phase damage multiplier folds into the action's base damage.
            int meleeDamage = Mathf.Max(0, Mathf.RoundToInt(PendingAction.BaseDamage * _getPhaseDamageMultiplier()));
            var request = new DamageRequest(
                targetId: "player",
                baseDamage: meleeDamage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            request.SourcePosition = _transform.position;
            request.KnockbackForce = _enemyData?.contactKnockbackForce ?? 0f;
            request.CanTriggerVulnerability = false;

            var result = DamageCalculator.Calculate(request, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                // F27: caminho central com atacante (perfect block reflete postura neste GO).
                var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
                var playerTarget = _getPlayerTarget();
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, _ownerGameObject, playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(PendingAction);
                    ApplyVampiricLifesteal(applied);
                    var playerPos = playerTarget != null ? (Vector2)playerTarget.transform.position : (Vector2)playerManager.transform.position;
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, playerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }
        }

        // ─── Telegraph ────────────────────────────────────────────────────────

        /// <summary>Inicia o telegraph de uma ação usando o EnemyTelegraphController.</summary>
        private void StartTelegraph(EnemyActionSO action)
        {
            if (_telegraph == null || string.IsNullOrEmpty(action.TelegraphProfileId)) return;

            if (_telegraphDatabase != null &&
                _telegraphDatabase.TryGetById(action.TelegraphProfileId, out var tp))
            {
                _telegraph.StartTelegraph(tp.BlinkColor, tp.BlinkFrequency);
            }
            else
            {
                _telegraph.StartTelegraph(Color.yellow);
            }
        }

        // ─── fable_83: Ataques-assinatura ─────────────────────────────────────────

        /// <summary>
        /// ComboStrike: aplica o primeiro hit imediatamente; agenda os hits restantes via
        /// _pendingComboHits / _nextComboHitTime (resolvidos em TickActionTimers).
        /// </summary>
        private void ExecuteComboStrike(EnemyActionSO action)
        {
            if (_getPlayerTarget() == null) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            int totalHits = EnemyActionExecution.ResolveComboHitCount(action.ComboHits);
            // Aplica o primeiro hit agora
            ApplySingleHitToPlayer(action, EnemyActionExecution.ComboHitDamage(action.BaseDamage, 0, totalHits), dmgType);
            // Agenda os restantes
            _pendingComboHits = totalHits - 1;
            _nextComboHitTime = Time.time + ComboHitIntervalSeconds;

            // Armazena hits e index para TickActionTimers
            _comboTotalHits = totalHits;
            _comboCurrentHit = 1;
            _comboBaseDamage = action.BaseDamage;
            _comboDmgType = dmgType;
            _comboAction = action;
        }

        private void ExecuteSelfBuff(EnemyActionSO action)
        {
            // Neutral defaults preserve the legacy no-op behavior.
            if (action.AllyHealPercent > 0f || !string.IsNullOrWhiteSpace(action.AllyBuffStatusId))
                ExecuteAllyHealBuff(action);
        }

        void IEnemyActionExecutionContext.ExecuteSelfBuff(EnemyActionSO action) => ExecuteSelfBuff(action);
        void IEnemyActionExecutionContext.ExecuteComboStrike(EnemyActionSO action) => ExecuteComboStrike(action);
        void IEnemyActionExecutionContext.ExecuteTelegraphedAoE(EnemyActionSO action) => ExecuteTelegraphedAoE(action);
        void IEnemyActionExecutionContext.ExecuteSummonAdds(EnemyActionSO action) => ExecuteSummonAdds(action);
        void IEnemyActionExecutionContext.ExecuteMultiHitCharge(EnemyActionSO action) => ExecuteMultiHitCharge(action);
        void IEnemyActionExecutionContext.ExecuteDebuffStrike(EnemyActionSO action) => ExecuteDebuffStrike(action);

        private void ResetComboState()
        {
            _pendingComboHits = 0;
            _comboTotalHits = 0;
            _comboCurrentHit = 0;
            _comboAction = null;
        }

        /// <summary>
        /// TelegraphedAoE: aplica dano em área (usando AreaRadius do action) centrada no player.
        /// Telegraph já correu no BeginAction (WindupSeconds = aoeDelay); aqui é o dano.
        /// </summary>
        private void ExecuteTelegraphedAoE(EnemyActionSO action)
        {
            if (_getPlayerTarget() == null) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            float radius = action.AoeRadius > 0f ? action.AoeRadius : (action.AreaRadius > 0f ? action.AreaRadius : 2.5f);
            Vector2 aoeCenter = _getPlayerTarget().transform.position;
            float dist = Vector2.Distance(_transform.position, aoeCenter);

            // spec_enemy_attack_kits_v1 (HazardZone, primitiva P2): quando a acao deixa uma zona
            // persistente (LeavesHazard), a zona nasce na origem do atacante (rastro de movimento —
            // lava_bulwark/magma_slug) INDEPENDENTE do player estar no raio agora — ela existe para
            // ser pisada depois. Spawn destacado (mesmo padrao de EnemyVolatileExplosionRunner);
            // nao duplica fisica (deteccao por distancia via EnemyActionExecution.IsInsideHazard).
            if (action.LeavesHazard)
            {
                EnemyHazardZoneRunner.Spawn(
                    _transform.position,
                    action.HazardRadius,
                    action.HazardDurationSeconds,
                    action.HazardTickSeconds,
                    Mathf.Max(0, Mathf.RoundToInt(action.HazardDamagePerTick * _getPhaseDamageMultiplier())),
                    action.HazardStatusId,
                    _enemyData?.enemyId ?? "enemy");
            }

            // Grace margin para AoE: se o player estava dentro do raio durante o telegraph, recebe dano.
            if (dist > radius * 1.4f) return;

            int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _getPhaseDamageMultiplier()));
            ApplySingleHitToPlayer(action, damage, dmgType);
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, aoeCenter));
        }

        /// <summary>
        /// spec_enemy_attack_kits_v1 (AllyHeal/AllyBuff, primitiva P2): seleciona o aliado-alvo
        /// (mais ferido, senao mais proximo) dentro de AllyTargetRadius via
        /// EnemyHealth.ActiveInstances (registro estatico — sem FindObjectsOfType, mesmo padrao de
        /// CraftingRuntime.ActiveInstances) e cura/buffa. Exclui o proprio invocador da busca.
        /// </summary>
        private void ExecuteAllyHealBuff(EnemyActionSO action)
        {
            var candidates = new List<EnemyActionExecution.AllyCandidate>();
            var candidateHealths = new List<CindarsHope.Combat.EnemyHealth>();
            Vector2 origin = _transform.position;

            var activeInstances = CindarsHope.Combat.EnemyHealth.ActiveInstances;
            for (int i = 0; i < activeInstances.Count; i++)
            {
                var candidate = activeInstances[i];
                if (candidate == null || candidate.IsDead) continue;
                if (candidate.transform == _transform) continue; // exclui o proprio invocador
                if (candidate.MaxHp <= 0) continue;

                float hpFraction = (float)candidate.CurrentHp / candidate.MaxHp;
                candidates.Add(new EnemyActionExecution.AllyCandidate(candidateHealths.Count, candidate.transform.position, hpFraction));
                candidateHealths.Add(candidate);
            }

            int bestIndex = EnemyActionExecution.ResolveAllyHealTarget(origin, action.AllyTargetRadius, candidates);
            if (bestIndex < 0 || bestIndex >= candidateHealths.Count) return;

            var target = candidateHealths[bestIndex];

            if (action.AllyHealPercent > 0f)
            {
                int healAmount = Mathf.Max(1, Mathf.RoundToInt(target.MaxHp * action.AllyHealPercent));
                int newHp = Mathf.Min(target.MaxHp, target.CurrentHp + healAmount);
                target.RestoreHp(newHp);
                CombatLog.Log($"CombatLog: AllyHeal. SourceId={_enemyData?.enemyId}, TargetId={target.EnemyId}, Amount={healAmount}, HP={newHp}/{target.MaxHp}.");
            }

            if (!string.IsNullOrWhiteSpace(action.AllyBuffStatusId))
            {
                // Buff de aliado reusa o caminho existente de status em EnemyHealth
                // (ApplyStatusEffect); resolucao do StatusEffectSO via o mesmo
                // StatusEffectDatabaseSO ja usado para status de inimigo (GameBootstrap).
                var statusDb = GameBootstrap.Instance?.StatusEffectDatabase;
                if (statusDb != null && statusDb.TryGetById(action.AllyBuffStatusId, out var statusEffect))
                {
                    target.ApplyStatusEffect(statusEffect);
                    CombatLog.Log($"CombatLog: AllyBuff. SourceId={_enemyData?.enemyId}, TargetId={target.EnemyId}, StatusId={action.AllyBuffStatusId}.");
                }
            }
        }

        /// <summary>
        /// SummonAdds: invoca fodder em posições deterministas (seed por contexto).
        /// Respeita cap por sala (MaxAddsPerRoom). Sem GUID/timestamp (ADR-0005).
        /// </summary>
        private void ExecuteSummonAdds(EnemyActionSO action)
        {
            if (string.IsNullOrWhiteSpace(action.SummonEnemyId)) return;

            string runSeed = CaveRunManager.Instance != null
                ? CaveRunManager.Instance.CaveRunSeed
                : "default";
            int caveLevel = CaveRunManager.Instance?.CurrentCaveLevel ?? 0;

            int seed = EnemyActionExecution.DeriveSummonSeed(runSeed, caveLevel, _enemyData?.enemyId ?? "summon");
            int roomAdds = 0; // sem scene search: trust design para cap; validação em PlayMode
            int count = EnemyActionExecution.ResolveSummonCount(action.SummonCount, roomAdds);
            if (count <= 0) return;

            var positions = EnemyActionExecution.GenerateSummonPositions(_transform.position, count, seed);
            for (int i = 0; i < positions.Count; i++)
            {
                // Publica evento de summon para o materializer/spawner reagir (sem scene search).
                GameEventBus.Publish(new EnemyAddsSummonedEvent(
                    _enemyData?.enemyId ?? "summon",
                    action.SummonEnemyId,
                    positions[i],
                    seed + i));
            }
            CindarsHope.DebugTools.CombatLog.Log(
                $"CombatLog: SummonAdds. SummonerId={_enemyData?.enemyId}, AddId={action.SummonEnemyId}, Count={count}, Seed={seed}");
        }

        /// <summary>
        /// MultiHitCharge: acerta todos os alvos ao longo da linha de charge até o player.
        /// Como EnemyBrain tem apenas o player como alvo explícito, aplica dano ao player
        /// se ele está na trajetória; em PlayMode o charge visual cobre a linha.
        /// </summary>
        private void ExecuteMultiHitCharge(EnemyActionSO action)
        {
            var playerTarget = _getPlayerTarget();
            if (playerTarget == null) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            Vector2 chargeOrigin = _transform.position;
            Vector2 chargeEnd = playerTarget.transform.position;
            var targetPositions = new System.Collections.Generic.List<Vector2> { chargeEnd };
            var hits = EnemyActionExecution.ResolveMultiHitChargeTargets(chargeOrigin, chargeEnd, targetPositions);
            if (hits.Count > 0)
            {
                int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _getPhaseDamageMultiplier()));
                ApplySingleHitToPlayer(action, damage, dmgType);
            }
        }

        /// <summary>
        /// DebuffStrike: ataque de dano normal que garante aplicação do status (DebuffStatusId).
        /// Reusar StatusApplicationIds se DebuffStatusId não estiver preenchido.
        /// </summary>
        private void ExecuteDebuffStrike(EnemyActionSO action)
        {
            if (_getPlayerTarget() == null) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            // Dano base (pode ser 0 para debuff puro)
            if (action.BaseDamage > 0)
            {
                float dist = _getDistanceToPlayer();
                if (dist <= action.Range * 1.2f)
                {
                    int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _getPhaseDamageMultiplier()));
                    ApplySingleHitToPlayer(action, damage, dmgType);
                }
            }

            // Aplicar o debuff com chance garantida (DebuffStrike ignora StatusApplyChance parcial:
            // garante pelo menos 1 aplicação por uso para identidade de arquétipo).
            string statusId = EnemyActionExecution.ResolveDebuffStatusId(action.DebuffStatusId, action.StatusApplicationIds);
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                var receiver = CindarsHope.Combat.StatusEffect.PlayerStatusReceiver.Instance;
                if (receiver != null)
                    receiver.TryApplyFromEnemyAction(statusId, 1f); // chance=1 garante aplicação
            }

            // spec_enemy_attack_kits_v1 (Pull, primitiva P2): desloca o player N tiles na direcao do
            // atacante (agarrao) ou do hazard/origem configurada — reusa DebuffStrike (dano+status ja
            // resolvidos acima), aditivo. Clamp contra paredes/obstaculos fica a cargo do proprio
            // Rigidbody2D/colisao do player (nao duplicamos deteccao de colisao aqui).
            if (action.PullDistanceTiles > 0f)
            {
                var playerTarget = _getPlayerTarget();
                if (playerTarget != null)
                {
                    Vector2 pullOrigin = action.PullFromAttackerOrigin ? (Vector2)_transform.position : (Vector2)playerTarget.transform.position;
                    Vector2 targetPos = EnemyActionExecution.ResolvePullTargetPosition(pullOrigin, playerTarget.transform.position, action.PullDistanceTiles);
                    var playerRb = playerTarget.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                        playerRb.position = targetPos;
                    else
                        playerTarget.transform.position = targetPos;
                }
            }
        }

        // SPEC 13D: Teleports to player then deals melee damage + applies status effects.
        // Destination uses BlinkFlankSide (set at spawn, alternating) — no UnityEngine.Random.
        private void ExecuteBlinkStrike(EnemyActionSO action, DamageType dmgType)
        {
            var playerTarget = _getPlayerTarget();
            if (playerTarget == null) return;

            // BlinkFlankSide is owned by EnemyBrain (SerializeField-adjacent; set in OnEnable).
            // We read it via a stored value passed from EnemyBrain.
            float range = action.BlinkRange > 0f ? action.BlinkRange : Mathf.Max(action.Range, 0.5f);
            var blink = EnemyBlinkExecutor.CalculateDestination(
                _transform.position, playerTarget.transform.position, range, _blinkFlankSide);

            if (!blink.Success)
            {
                Debug.Log($"[EnemyBrain] BlinkStrike skipped: {blink.FailReason}");
                return;
            }

            // Teleport via the stored Rigidbody2D reference (passed via _rbPosition action)
            _setRbPosition?.Invoke(blink.Destination);

            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, blink.Destination));

            // Apply melee damage after teleport
            int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _getPhaseDamageMultiplier()));
            var req = new DamageRequest(
                targetId: "player",
                baseDamage: damage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            req.SourcePosition = blink.Destination;
            req.KnockbackForce = _enemyData?.contactKnockbackForce ?? 0f;
            req.CanTriggerVulnerability = false;
            var result = DamageCalculator.Calculate(req, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, _ownerGameObject, playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(action);
                    ApplyVampiricLifesteal(applied);
                    var playerPos = playerTarget != null ? (Vector2)playerTarget.transform.position : (Vector2)playerManager.transform.position;
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, playerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }
        }

        // BlinkStrike needs to move the Rigidbody2D position and the blinkFlankSide.
        private System.Action<Vector2> _setRbPosition;
        private float _blinkFlankSide = 1f;
        private GameObject _ownerGameObject;

        /// <summary>Atualiza o flankside do blink (chamado pelo EnemyBrain em OnEnable).</summary>
        internal void SetBlinkFlankSide(float side) => _blinkFlankSide = side;

        /// <summary>Define o callback para mover o Rigidbody2D (necessário para BlinkStrike).</summary>
        internal void SetRbPositionCallback(System.Action<Vector2> setPosition) => _setRbPosition = setPosition;

        /// <summary>Define o GameObject dono (EnemyBrain) para passar como atacante no ApplyDamage.</summary>
        internal void SetOwnerGameObject(GameObject owner) => _ownerGameObject = owner;

        // ─── SPEC 13D: FireDeathTrigger ────────────────────────────────────────────

        /// <summary>
        /// SPEC 13D: dispara a ação de death-trigger (IsDeathtrigger=true) exatamente uma vez.
        /// Guard: only inside cave (CaveRunManager present) to avoid out-of-cave effects.
        /// </summary>
        internal void FireDeathTrigger()
        {
            if (DeathtriggerFired) return;
            if (ActiveActionSet == null || _actionDatabase == null) return;
            if (CaveRunManager.Instance == null) return;

            foreach (var actionId in ActiveActionSet.ActionIds)
            {
                if (!_actionDatabase.TryGetById(actionId, out var action)) continue;
                if (!action.IsDeathtrigger) continue;

                DeathtriggerFired = true;
                ExecuteDeathTrigger(action);
                return;
            }
        }

        /// <summary>Aplica AoE damage da ação de death-trigger ao player se estiver no alcance.</summary>
        private void ExecuteDeathTrigger(EnemyActionSO action)
        {
            if (action.BaseDamage <= 0) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Fire;

            float radius = action.AreaRadius > 0f ? action.AreaRadius : action.Range;
            var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
            if (playerManager == null) return;

            var playerTarget = _getPlayerTarget();
            var dtPlayerPos = playerTarget != null ? playerTarget.transform.position : playerManager.transform.position;
            float dist = Vector2.Distance(_transform.position, dtPlayerPos);
            if (dist > radius * 1.2f) return;

            int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _getPhaseDamageMultiplier()));
            var req = new DamageRequest(
                targetId: "player",
                baseDamage: damage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            req.SourcePosition = _transform.position;
            req.CanTriggerVulnerability = false;
            var result = DamageCalculator.Calculate(req, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, _ownerGameObject, playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(action);
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, (Vector2)dtPlayerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }

            Debug.Log($"[EnemyBrain] DeathTrigger fired. EnemyId={_enemyData?.enemyId}, ActionId={action.ActionId}, Damage={damage}, PlayerDist={dist:F2}");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Helper compartilhado: aplica um hit de dano ao player com knockback e eventos.
        /// Reusar o caminho PlayerDamageReceiver existente (F27/F01).
        /// </summary>
        internal void ApplySingleHitToPlayer(EnemyActionSO action, int damage, DamageType dmgType)
        {
            if (damage <= 0) return;
            var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
            if (playerManager == null) return;

            var req = new DamageRequest(
                targetId: "player",
                baseDamage: damage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            req.SourcePosition = _transform.position;
            req.KnockbackForce = _enemyData?.contactKnockbackForce ?? 0f;
            req.CanTriggerVulnerability = false;

            var result = DamageCalculator.Calculate(req, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                var playerTarget = _getPlayerTarget();
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, _ownerGameObject, playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(action);
                    ApplyVampiricLifesteal(applied);
                    var playerPos = playerTarget != null
                        ? (Vector2)playerTarget.transform.position
                        : (Vector2)playerManager.transform.position;
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, playerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }
        }

        /// <summary>F01: EnemyActionSO.StatusApplicationIds aplicados no player via PlayerStatusReceiver.</summary>
        private static void ApplyActionStatusesToPlayer(EnemyActionSO action)
        {
            if (action == null || action.StatusApplicationIds == null || action.StatusApplicationIds.Length == 0)
            {
                return;
            }

            var receiver = CindarsHope.Combat.StatusEffect.PlayerStatusReceiver.Instance;
            if (receiver == null)
            {
                return;
            }

            foreach (var statusId in action.StatusApplicationIds)
            {
                receiver.TryApplyFromEnemyAction(statusId, action.StatusApplyChance);
            }
        }

        private void TryOpenVulnerabilityWindow(VulnerabilityTriggerMode trigger)
        {
            if (PendingAction == null || !PendingAction.TriggersVulnerabilityWindow) return;
            if (_vulnerabilityState == null) return;

            var mode = PendingAction.VulnerabilityWindowTrigger;
            bool shouldOpen = (mode == trigger) ||
                              (mode == VulnerabilityTriggerMode.AlwaysForTest &&
                               trigger == VulnerabilityTriggerMode.AfterAttackRecover);

            if (shouldOpen)
            {
                float dur = _vulnerabilityProfile != null ? _vulnerabilityProfile.WindowDurationSeconds : 1.5f;
                float mul = _vulnerabilityProfile != null ? _vulnerabilityProfile.Multiplier : 1.5f;
                float cd  = _vulnerabilityProfile != null ? _vulnerabilityProfile.CooldownSeconds : 10f;
                _vulnerabilityState.OpenWindow(dur, mul, cd);
            }
        }

        /// <summary>Vampiric elites heal 25% of damage dealt (melee/area path, where the applied amount is known).</summary>
        private void ApplyVampiricLifesteal(int damageDealt)
        {
            if (_getEliteAffix == null) return;
            var affix = _getEliteAffix();
            int heal = EliteAffixRules.ResolveLifestealHeal(affix, damageDealt);
            if (heal <= 0 || _health == null || _health.MaxHp <= 0)
            {
                return;
            }

            _health.RestoreHp(Mathf.Min(_health.MaxHp, _health.CurrentHp + heal));
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: EliteVampiricHeal. EnemyId={_enemyData?.enemyId}, Heal={heal}, HP={_health.CurrentHp}/{_health.MaxHp}.");
        }

        private System.Func<EliteAffix> _getEliteAffix;

        /// <summary>Define o callback para ler o affix elite atual (necessário para Vampiric lifesteal).</summary>
        internal void SetEliteAffixCallback(System.Func<EliteAffix> getAffix) => _getEliteAffix = getAffix;

        /// <summary>
        /// Reseta o estado de ação ao (re)spawn. Chamado pelo EnemyBrain em OnEnable.
        /// </summary>
        internal void ResetOnSpawn()
        {
            ActionResolved = false;
            PendingAction = null;
            DeathtriggerFired = false;
            ResetComboState();
        }
    }
}
