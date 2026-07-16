using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyDataSO _enemyData;
        [SerializeField] private EnemyMovementProfileSO _movementProfile;

        [Header("Databases (SPEC 13D)")]
        [SerializeField] private EnemyActionSetDatabaseSO _actionSetDatabase;
        [SerializeField] private EnemyActionDatabaseSO _actionDatabase;
        [SerializeField] private EnemyTelegraphProfileDatabaseSO _telegraphDatabase;

        [Header("Runtime Profiles (SPEC 14A-FIX4)")]
        [SerializeField] private EnemyVulnerabilityProfileSO _vulnerabilityProfile;

        [Header("Tuning")]
        [SerializeField] private float _decisionTickSeconds = 0.3f;
        [SerializeField] private float _lowHealthRetreatThreshold = 0.25f;
        [SerializeField] private float _retreatDurationSeconds = 2.5f;
        [SerializeField] private float _leapCooldownSeconds = 3.5f;
        [SerializeField] private float _leapSpeedMultiplier = 3.2f;
        [SerializeField] private float _blinkCooldownSeconds = 5f;
        [SerializeField] private float _burrowSpeedMultiplier = 1.7f;
        [SerializeField] private float _burrowEmergeDistance = 1.4f;

        // State machine
        private EnemyBrainState _currentState = EnemyBrainState.Idle;
        private float _decisionTimer;
        private float _retreatEndTime;
        // Alternates per spawned brain so phase enemies do not all flank the same side.
        private static float s_nextBlinkFlankSide = 1f;
        private float _blinkFlankSide = 1f;

        // Target
        private readonly EnemyTargetingController _targeting = new EnemyTargetingController();

        // fable_04: threat/aggro memory + pack coordination.
        private readonly EnemyThreatState _threatState = new EnemyThreatState();
        // Internal feature flag (rollback): disabling reverts to instant-distance leash behaviour.
        private bool _threatMemoryEnabled = true;
        private EnemyPackCoordinator _packCoordinator;
        private string _packId;
        private bool _packEngagedAnnounced;
        private readonly EnemyDebugTelemetry _telemetry = new EnemyDebugTelemetry();

        // Components
        private Rigidbody2D _rb;
        private EnemyTelegraphController _telegraph;
        // SPEC 14A-FIX10: explicitly qualified - previously bound to the now-removed legacy
        // CindarsHope.Enemy.EnemyHealth via same-namespace resolution, which never got Configure'd.
        private CindarsHope.Combat.EnemyHealth _health;
        private EnemyVulnerabilityState _vulnerabilityState;
        private SpriteRenderer _spriteRenderer;
        private float _spriteBaseAlpha = 1f;

        public EnemyBrainState CurrentState => _currentState;

        /// <summary>ActionId da acao em windup/recover agora (null se nenhuma). Passthrough somente-leitura
        /// do <see cref="EnemyActionRunner.PendingAction"/> para o EnemyAnimator (componente irmao) escolher
        /// a folha de ataque (normal vs "_special"). Nao altera comportamento de combate.</summary>
        public string CurrentActionId => _actions != null ? _actions.PendingAction?.ActionId : null;

        // F01: modificadores externos aplicados por status effects (Chill/Slow/Root/Fear/ConfusionLite).
        private float _externalSpeedMultiplier = 1f;
        private float _externalSpeedUntil;
        private float _externalInvertUntil;
        private float _forcedRetreatUntil;

        // fable_24: named-elite affix (decided deterministically at spawn-plan time by the planner).
        private EliteAffix _eliteAffix = EliteAffix.None;
        private bool _wardedStatusConsumed;          // Warded resists exactly the FIRST status applied
        private bool _volatileExploded;              // Volatile fires its death explosion once
        private float _attackCadenceFactor = 1f;     // Frenzied shortens windup/recover (<1 = faster)

        // F02: stagger por quebra de postura — entra em Stunned e sai sozinho.
        private float _stunUntil;

        // fable_05: per-phase multipliers driven by BossBrainController (1 = unchanged / non-boss).
        private float _phaseMoveSpeedMultiplier = 1f;
        private float _phaseDamageMultiplier = 1f;

        // fable_83: pathing leve — estado de desvio local (histerese).
        [SerializeField] private LayerMask _obstacleLayerMask = 0; // configurar no prefab; 0 = sem raycast

        // ─── Colaboradores ────────────────────────────────────────────────────

        private readonly EnemyMovementExecutor _movement = new EnemyMovementExecutor();
        private readonly EnemyActionRunner _actions = new EnemyActionRunner();
        private readonly EnemyConflictHandler _conflict = new EnemyConflictHandler();

        // ─── Comportamento externo ─────────────────────────────────────────────

        /// <summary>
        /// F01 — override externo de comportamento usado pelo EnemyStatusRuntimeTicker.
        /// speedMultiplier 0 = Root/Stun; invert = ConfusionLite; forceRetreat = Fear.
        /// </summary>
        public void ApplyExternalBehaviorOverride(float speedMultiplier, float speedSeconds, bool invertMovement, float invertSeconds, bool forceRetreat, float retreatSeconds)
        {
            var now = Time.time;
            if (speedSeconds > 0f)
            {
                _externalSpeedMultiplier = Mathf.Clamp(speedMultiplier, 0f, 2f);
                _externalSpeedUntil = now + Mathf.Min(speedSeconds, 10f);
            }

            if (invertMovement && invertSeconds > 0f)
            {
                _externalInvertUntil = now + Mathf.Min(invertSeconds, 10f);
            }

            // Fear não interrompe windup/recover (mitigação de risco da spec) — EvaluateState respeita.
            if (forceRetreat && retreatSeconds > 0f)
            {
                _forcedRetreatUntil = now + Mathf.Min(retreatSeconds, 10f);
            }
        }

        private float ExternalSpeedFactor()
        {
            return Time.time < _externalSpeedUntil ? _externalSpeedMultiplier : 1f;
        }

        // SPEC 14A-FIX6: Public state for real runtime resolution checks
        public bool HasResolvedActionSet => _actions.ActiveActionSet != null && _actions.ActiveActionSet.ActionIds != null && _actions.ActiveActionSet.ActionIds.Length > 0;
        public int ResolvedActionCount => _actions.ActionCooldowns.Count;
        public bool HasResolvedMovementProfile => _movementProfile != null;
        public bool HasResolvedVulnerabilityProfile => _vulnerabilityProfile != null;
        public EnemyMovementType MovementType => _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;

        // fable_24: the move actually driving behaviour this tick. Floaters alternate primary
        // FloatingSlow ↔ MoveSecondary (FloatingOrbit) once engaged, if a secondary is configured.
        private EnemyMovementType EffectiveMove
        {
            get
            {
                var primary = MovementType;
                if (primary == EnemyMovementType.FloatingSlow
                    && _enemyData != null
                    && _enemyData.MoveSecondary == EnemyMovementType.FloatingOrbit
                    && (_currentState == EnemyBrainState.Chase || _currentState == EnemyBrainState.Kite))
                {
                    return EnemyMovementType.FloatingOrbit;
                }

                return primary;
            }
        }

        public EliteAffix CurrentEliteAffix => _eliteAffix;
        public bool IsElite => _eliteAffix != EliteAffix.None;
        public string ResolvedActionSetId => _actions.ActiveActionSet?.ActionSetId ?? string.Empty;
        public string ResolvedMovementProfileId => _movementProfile?.MovementProfileId ?? string.Empty;
        public string ResolvedVulnerabilityProfileId => _vulnerabilityProfile?.VulnerabilityProfileId ?? string.Empty;

        public void Configure(EnemyDataSO data)
        {
            _enemyData = data;
        }

        /// <summary>
        /// spec_codex_13: wiring de obstacle avoidance a partir do materializer/gerador de
        /// spawn (nao hardcoded no runtime — o CALLER resolve o layer por nome, ex.:
        /// CindarsHope.Core.Physics.GameplayLayerNames.GetMaskSafe("WorldSolid")). Seguro
        /// chamar antes ou depois de Awake/Configure; atualiza tanto o campo serializado
        /// quanto o executor de movimento ja inicializado.
        /// </summary>
        public void SetObstacleLayerMask(LayerMask mask)
        {
            _obstacleLayerMask = mask;
            _movement.ObstacleLayerMask = mask;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _telegraph = GetComponent<EnemyTelegraphController>();
            _health = GetComponent<EnemyHealth>();
            _vulnerabilityState = GetComponent<EnemyVulnerabilityState>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            // fable_78: resolvido no mesmo GameObject (sem scene search). Presente só quando o materializer
            // anexou o combatant de conflito a este inimigo nesta visita.
            _conflict.SetCombatant(GetComponent<CindarsHope.Cave.Ecosystem.CaveConflictCombatant>());

            InitCollaborators();
        }

        /// <summary>Inicializa os colaboradores com as referências necessárias.</summary>
        private void InitCollaborators()
        {
            _movement.Init(
                _rb,
                transform,
                _enemyData,
                _movementProfile,
                _telegraph,
                getPlayerTarget: () => _targeting.CurrentTarget,
                getMoveSpeed: MoveSpeed,
                getDistanceToPlayer: DistanceToPlayer,
                getDirectionToPlayer: DirectionToPlayer,
                setSubmergedVisual: submerged => SetSubmergedVisual(submerged),
                getPackCoordinator: () => _packCoordinator,
                getPackId: () => _packId,
                hasActiveThreat: HasActiveThreat,
                getLastKnownPosition: () => _threatState.LastKnownPosition,
                leapCooldownSeconds: _leapCooldownSeconds,
                leapSpeedMultiplier: _leapSpeedMultiplier,
                blinkCooldownSeconds: _blinkCooldownSeconds,
                burrowSpeedMultiplier: _burrowSpeedMultiplier);

            _movement.ObstacleLayerMask = _obstacleLayerMask;

            _actions.Init(
                transform,
                _enemyData,
                _actionSetDatabase,
                _actionDatabase,
                _telegraphDatabase,
                _vulnerabilityProfile,
                _telegraph,
                _vulnerabilityState,
                _health,
                getPlayerTarget: () => _targeting.CurrentTarget,
                getDistanceToPlayer: DistanceToPlayer,
                getDirectionToPlayer: DirectionToPlayer,
                getPhaseDamageMultiplier: () => _phaseDamageMultiplier,
                isTargetingRival: () => _conflict.IsTargetingRival,
                resolveInterMonsterAction: dmg => _conflict.ResolveInterMonsterAction(dmg),
                getDetectionRange: DetectionRange);

            _actions.SetBlinkFlankSide(_blinkFlankSide);
            _actions.SetRbPositionCallback(pos =>
            {
                if (_rb != null) _rb.position = pos;
                else transform.position = (Vector3)pos;
            });
            _actions.SetOwnerGameObject(gameObject);
            _actions.SetEliteAffixCallback(() => _eliteAffix);

            _conflict.Init(
                transform,
                _enemyData,
                _health,
                getDetectionRange: DetectionRange,
                getPhaseDamageMultiplier: () => _phaseDamageMultiplier,
                getPendingAction: () => _actions.PendingAction);
        }

        private void OnEnable()
        {
            _decisionTimer = 0f;
            _movement.PatrolDirectionTimer = 0f;
            _actions.ResetOnSpawn();
            _currentState = EnemyBrainState.Idle;
            _movement.SpawnAnchor = transform.position;
            _movement.IsLeaping = false;
            _blinkFlankSide = s_nextBlinkFlankSide;
            s_nextBlinkFlankSide = -s_nextBlinkFlankSide;
            _movement.BlinkFlankSide = _blinkFlankSide;
            _actions.SetBlinkFlankSide(_blinkFlankSide);

            // fable_82: RNG seeded por hash da posição de spawn (estável por spawn point; sem GUID/timestamp).
            // Alternância de flanco simples (sem UnityEngine.Random) — igual ao padrão de _blinkFlankSide.
            int spawnHash = Mathf.RoundToInt(transform.position.x * 73856093f) ^
                            Mathf.RoundToInt(transform.position.y * 19349663f);
            _movement.EvasionRng = new System.Random(spawnHash != 0 ? spawnHash : 1);
            _movement.EvadeFlankSign = (spawnHash & 1) == 0 ? 1f : -1f;
            _movement.IsEvasiveSidestepping = false;
            _movement.IsRepositionDashing = false;
            _movement.PlayerWindupDetected = false;
            _movement.PlayerWindupClearTime = 0f;
            // Subscreve ao windup do player via GameEventBus (sem scene search).
            GameEventBus.Subscribe<CindarsHope.Core.Events.PlayerAttackWindupEvent>(OnPlayerAttackWindup);

            // fable_04: reset transient aggro on (re)spawn; memory window follows movement type.
            _threatState.Clear();
            _threatState.SetMemorySeconds(EnemyThreatState.ResolveMemorySeconds(MovementType));
            _packEngagedAnnounced = false;
            _telemetry.ResetThreatExpiredLog();

            // fable_24: reset move/elite transient runtime on (re)spawn. The elite affix itself is
            // re-applied by the materializer via ConfigureElite after this; clear the one-shot flags.
            _wardedStatusConsumed = false;
            _volatileExploded = false;
            _movement.MimicActivated = false;
            _movement.IsCharging = false;
            _movement.ChargeTelegraphActive = false;
            _movement.FlankerNoLeaderSince = -1f;
            _movement.OrbitSign = s_nextBlinkFlankSide; // reuse the alternating side so packs spread out
            _movement.AnchorPoint = transform.position;
            if (_movementProfile != null && _movementProfile.WanderRadius > 0f)
                _movement.AnchorLeashTiles = _movementProfile.WanderRadius;

            // fable_83: reset de estado de assinatura/pathing ao (re)spawn.
            _movement.IsAvoidingObstacle = false;

            _targeting.RefreshPlayerTarget();

            // fable_78: re-resolve o combatant caso ele tenha sido anexado após o Awake do brain
            // (o materializer adiciona o brain e depois, condicionalmente, o combatant).
            _conflict.TryResolveCombatant(() => GetComponent<CindarsHope.Cave.Ecosystem.CaveConflictCombatant>());

            if (_vulnerabilityState != null)
                _vulnerabilityState.Initialize(_enemyData?.enemyId);

            if (_spriteRenderer != null)
                _spriteBaseAlpha = _spriteRenderer.color.a;

            _actions.InitActionSet();

            _decisionTickSeconds = EnemyBrainConfigurationPolicy.ResolveDecisionTick(
                _movementProfile, _decisionTickSeconds);
        }

        private void OnDisable()
        {
            _movement.StopMovement();
            SetSubmergedVisual(false);
            // fable_82: desassina evento de windup do player ao desativar.
            GameEventBus.Unsubscribe<CindarsHope.Core.Events.PlayerAttackWindupEvent>(OnPlayerAttackWindup);
            _movement.IsEvasiveSidestepping = false;
            _movement.IsRepositionDashing = false;
        }

        // fable_82: callback de windup do player via GameEventBus. Marca flag por janela curta.
        private void OnPlayerAttackWindup(CindarsHope.Core.Events.PlayerAttackWindupEvent evt)
        {
            if (_movementProfile == null || !_movementProfile.CanReactiveEvade) return;

            float dist = Vector2.Distance(transform.position, evt.PlayerPosition);
            if (dist <= _movementProfile.EvadeReactionRadius)
            {
                _movement.PlayerWindupDetected = true;
                // Janela de windup válida por 0.5s (tempo de reação padrão de combate).
                _movement.PlayerWindupClearTime = Time.time + 0.5f;
            }
        }

        // ─── Main Loop ────────────────────────────────────────────────────────

        private void Update()
        {
            if (_currentState == EnemyBrainState.Dead) return;

            // O player VISÍVEL (PlayerController) pode nascer depois do inimigo materializar; re-resolve
            // a cada frame até apontar para ele. Sem isto o target fica no PlayerManager (no _Bootstrap,
            // em (0,0,0)) e o inimigo mede distância até a origem do mundo — atacando o vazio, mas
            // roteando o dano ao player real longe dali (bug "dano invisível de bicho que não está perto").
            _targeting.RefreshPlayerTarget();

            // fable_78 (SLICE 4): targeting conflict-aware. Ramo ISOLADO — só roda quando este inimigo é um
            // conflict-combatant. Caso contrário, _rivalHealthTarget fica null e o caminho player-only é
            // intacto (o targeting já restaurou o alvo para o player visível).
            _targeting.RefreshConflictTarget(_conflict);

            // fable_24: Volatile elites explode once when they die. Damage usually flows straight
            // through EnemyHealth (not EnemyBrain.TakeDamage), so detect the death transition here
            // and fire the telegraphed blast before the object is recycled.
            if (_eliteAffix == EliteAffix.Volatile && _health != null && _health.IsDead)
            {
                TriggerVolatileDeathExplosion();
                _currentState = EnemyBrainState.Dead;
                return;
            }

            // F02: saída do stagger (Stunned não é avaliado pelo EvaluateState).
            if (_currentState == EnemyBrainState.Stunned)
            {
                if (Time.time < _stunUntil)
                {
                    return;
                }

                _currentState = EnemyBrainState.Alert;
            }

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = _decisionTickSeconds;
                EvaluateState();
            }

            _actions.TickActionTimers(ref _currentState, _attackCadenceFactor);
            _movement.ExecuteMovement(_currentState, EffectiveMove);

            // F01: ConfusionLite inverte o movimento resultante (ponto único pós-estado).
            if (Time.time < _externalInvertUntil && _rb != null)
            {
                _rb.linearVelocity = -_rb.linearVelocity;
            }
        }

        // ─── State Evaluation ─────────────────────────────────────────────────

        private void EvaluateState()
        {
            if (_currentState == EnemyBrainState.AttackWindup ||
                _currentState == EnemyBrainState.AttackRecover ||
                _currentState == EnemyBrainState.Stunned)
                return;

            float dist = DistanceToPlayer();
            bool inDetect = dist <= DetectionRange();

            // fable_04: threat memory e pack alert (side effects — ficam no adapter)
            if (_threatMemoryEnabled && inDetect && _targeting.CurrentTarget != null)
            {
                _threatState.NoticeTarget(_targeting.CurrentTarget.transform.position, Time.time);
                _telemetry.ResetThreatExpiredLog();
                AnnouncePackEngagementOnce();
            }

            var input = new EnemyDecisionInput(
                distanceToTarget: dist,
                detectionRange: DetectionRange(),
                leashRange: LeashRange(),
                currentTime: Time.time,
                burrowEmergeDistance: _burrowEmergeDistance,
                lowHealthRetreatThreshold: _lowHealthRetreatThreshold,
                retreatEndTime: _retreatEndTime,
                forcedRetreatUntil: _forcedRetreatUntil,
                stunUntil: _stunUntil,
                currentHpFraction: (_health != null && _health.MaxHp > 0) ? (float)_health.CurrentHp / _health.MaxHp : float.NaN,
                currentState: _currentState,
                movementType: MovementType,
                primaryRole: _enemyData?.PrimaryRole ?? EnemyRole.Chaser,
                hasActiveThreat: HasActiveThreat(),
                targetIsValid: _targeting.CurrentTarget != null,
                healthIsValid: _health != null && _health.MaxHp > 0,
                canBurrow: _movementProfile != null && _movementProfile.CanBurrow,
                retreatDurationSeconds: _retreatDurationSeconds
            );

            var output = EnemyDecisionCore.Evaluate(input);

            // Aplicar side effects do output
            if (output.ClearSubmergedVisual) SetSubmergedVisual(false);
            if (output.ShouldSetSubmergedVisual) SetSubmergedVisual(true);
            if (output.ShouldSetRetreat)
                _retreatEndTime = output.RetreatEndTimeOverride;

            // fable_04: pack leash side effects that couldn't be handled in the pure core
            if (output.NextState == EnemyBrainState.Patrol && _currentState != EnemyBrainState.Patrol
                && !HasActiveThreat() && dist > LeashRange())
            {
                _telemetry.LogThreatExpiredOnce(_enemyData?.enemyId, _packId, this);
                if (TryCollectivePackLeashReset())
                {
                    // pack reset already set _currentState to Patrol; skip assignment below
                    if (output.ShouldTryAction) TryBeginAction(dist);
                    return;
                }
            }

            _currentState = output.NextState;

            if (output.ShouldTryAction)
                TryBeginAction(dist);
        }

        private void TryBeginAction(float dist)
        {
            var action = _actions.SelectBestAction(dist);
            if (action != null)
            {
                _actions.BeginAction(action, _attackCadenceFactor, ref _currentState);
                return;
            }

            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;

            // Roles corpo-a-corpo (Chaser/Tank/Guard/Burrower/Elite/MiniBoss/Boss) nunca entram em Kite
            // independente do MovementType atribuído. Garante que inimigos melee colem no player.
            // Só Ranged, Caster e Swarm podem kicar — são os únicos que fazem sentido recuar.
            var primaryRole = _enemyData?.PrimaryRole ?? EnemyRole.Chaser;
            bool isMeleeRole = primaryRole != EnemyRole.Ranged
                               && primaryRole != EnemyRole.Caster
                               && primaryRole != EnemyRole.Swarm;

            switch (moveType)
            {
                case EnemyMovementType.GuardStationary:
                    _currentState = EnemyBrainState.GuardHold;
                    break;
                case EnemyMovementType.KiteRanged:
                case EnemyMovementType.CasterKeepAway:
                    // Guard: roles corpo-a-corpo forçam Chase mesmo com perfil de kite.
                    _currentState = isMeleeRole ? EnemyBrainState.Chase : EnemyBrainState.Kite;
                    break;
                default:
                    _currentState = EnemyBrainState.Chase;
                    break;
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void SetSubmergedVisual(bool submerged)
        {
            _movement.SetSubmergedVisual(submerged, _spriteRenderer, _spriteBaseAlpha);
        }

        private float DistanceToPlayer() => _targeting.DistanceFrom(transform);

        private Vector2 DirectionToPlayer() => _targeting.DirectionFrom(transform);

        private float DetectionRange() => EnemyBrainTuningResolver.DetectionRange(_movementProfile, _enemyData);
        private float LeashRange() => EnemyBrainTuningResolver.LeashRange(_movementProfile, _enemyData);
        private float MoveSpeed() => EnemyBrainTuningResolver.MoveSpeed(
            _movementProfile, _enemyData, ExternalSpeedFactor(), _phaseMoveSpeedMultiplier);

        // ─── Public API ───────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (_health != null)
            {
                _health.TakeDamage(amount);
                if (_health.IsDead)
                {
                    _actions.FireDeathTrigger();
                    _currentState = EnemyBrainState.Dead;
                }
            }
        }

        public void Configure(EnemyDataSO enemyData, EnemyMovementProfileSO movementProfile = null)
        {
            _enemyData = enemyData;
            _movementProfile = movementProfile;

            if (_vulnerabilityState != null)
            {
                _vulnerabilityState.Initialize(_enemyData?.enemyId);
            }

            _actions.UpdateRefs(_enemyData, _actionSetDatabase, _actionDatabase, _telegraphDatabase, _vulnerabilityProfile, _telegraph, _vulnerabilityState, _health);
            _movement.UpdateRefs(_enemyData, _movementProfile, _telegraph);
            _conflict.UpdateRefs(_enemyData, _health);
            _actions.InitActionSet();
        }

        public void ConfigureRuntime(
            EnemyDataSO enemyData,
            EnemyMovementProfileSO movementProfile,
            EnemyActionSetDatabaseSO actionSetDatabase,
            EnemyActionDatabaseSO actionDatabase,
            EnemyTelegraphProfileDatabaseSO telegraphDatabase,
            EnemyVulnerabilityProfileSO vulnerabilityProfile,
            EnemyPackCoordinator packCoordinator = null,
            string packId = null)
        {
            _enemyData = enemyData;
            _movementProfile = movementProfile;
            _actionSetDatabase = actionSetDatabase;
            _actionDatabase = actionDatabase;
            _telegraphDatabase = telegraphDatabase;
            _vulnerabilityProfile = vulnerabilityProfile;

            // fable_04: pack wiring injected by the materializer (no scene search).
            _packCoordinator = packCoordinator;
            _packId = EnemyBrainConfigurationPolicy.NormalizePackId(packId);

            _decisionTickSeconds = EnemyBrainConfigurationPolicy.ResolveDecisionTick(
                _movementProfile, _decisionTickSeconds);

            // Memory window depends on movement type, which is now resolved.
            _threatState.SetMemorySeconds(EnemyThreatState.ResolveMemorySeconds(MovementType));

            if (_vulnerabilityState != null)
                _vulnerabilityState.Initialize(_enemyData?.enemyId);

            // Update collaborator references
            _movement.UpdateRefs(_enemyData, _movementProfile, _telegraph);
            _movement.UpdateTuning(_leapCooldownSeconds, _leapSpeedMultiplier, _blinkCooldownSeconds, _burrowSpeedMultiplier);
            _actions.UpdateRefs(_enemyData, _actionSetDatabase, _actionDatabase, _telegraphDatabase, _vulnerabilityProfile, _telegraph, _vulnerabilityState, _health);
            _conflict.UpdateRefs(_enemyData, _health);
            _actions.InitActionSet();
        }

        // ─── fable_24: elite affix runtime ────────────────────────────────────

        /// <summary>
        /// Apply a named-elite affix to this brain. Called by the materializer right after spawn
        /// configuration, using the affix the planner decided deterministically per slot
        /// (cave-stable-run / ADR-0005). Frenzied speeds up the action cadence immediately;
        /// Vampiric/Volatile/Warded take effect during damage/death/status handling.
        /// </summary>
        public void ConfigureElite(EliteAffix affix)
        {
            _eliteAffix = affix;
            _attackCadenceFactor = EliteAffixRules.ResolveAttackCadenceFactor(affix);
            _wardedStatusConsumed = false;
            _volatileExploded = false;
        }

        /// <summary>
        /// fable_78 (SLICE 4) — liga este brain ao conflito inter-monstro. Chamado pelo materializer logo
        /// após anexar o CaveConflictCombatant a este inimigo (injeção explícita; sem scene search). Com o
        /// combatant presente, o targeting passa a considerar rivais; sem ele, o caminho player-only segue
        /// intacto. balance carrega os pesos de aggro e os multiplicadores de dano/Ferido/loot.
        /// </summary>
        public void ConfigureConflict(
            CindarsHope.Cave.Ecosystem.CaveConflictCombatant combatant,
            CindarsHope.Cave.Data.CaveEcosystemBalanceSO balance)
        {
            _conflict.ConfigureConflict(combatant, balance);
        }

        /// <summary>
        /// Warded resists exactly the FIRST status applied to it. Returns true if the incoming status
        /// should be IGNORED (consumed the ward). EnemyStatusReceiver/ticker calls this before applying.
        /// </summary>
        public bool ShouldResistIncomingStatus()
        {
            if (!EliteAffixRules.ResistsFirstStatus(_eliteAffix) || _wardedStatusConsumed)
            {
                return false;
            }

            _wardedStatusConsumed = true;
            _telemetry.LogEliteWardedResisted(_enemyData?.enemyId, this);
            return true;
        }

        /// <summary>
        /// fable_24 — Volatile elites explode once on death after a telegraphed delay. The explosion
        /// damage is capped at 25% of the player's maxHP (anti one-shot, spec risk mitigation). Called
        /// by the death path; safe to call on non-Volatile (no-op). Telegraph uses the existing
        /// EnemyTelegraphStartedEvent so the player can read the windup before the blast.
        /// </summary>
        public void TriggerVolatileDeathExplosion()
        {
            if (!EliteAffixRules.ExplodesOnDeath(_eliteAffix) || _volatileExploded)
            {
                return;
            }

            _volatileExploded = true;
            int playerMaxHp = (GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager)?.MaxHP ?? 0;
            int raw = Mathf.Max(1, (_enemyData?.contactDamage ?? 1) * 3);
            int damage = EliteAffixRules.ResolveVolatileExplosionDamage(raw, playerMaxHp);

            // The host enemy is about to be deactivated by EnemyHealth.Die(); run the telegraph +
            // blast on a DETACHED runner (same survival pattern as enemy projectiles) so the windup
            // resolves even though this GameObject is gone. Player can still dodge by leaving the radius.
            EnemyVolatileExplosionRunner.Spawn(
                transform.position,
                EliteAffixRules.VolatileExplosionTelegraphSeconds,
                damage,
                _enemyData?.enemyId ?? "enemy");

            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, transform.position));
            _telemetry.LogEliteVolatileExploding(_enemyData?.enemyId, damage, playerMaxHp, EliteAffixRules.VolatileExplosionTelegraphSeconds, this);
        }

        // ─── fable_24: boss primitives (fable_05 orchestrates full phases) ─────

        /// <summary>
        /// BossArenaControl primitive: lock this enemy's leash to an explicit arena centre + radius.
        /// fable_05 sets the arena bounds; the brain enforces "cannot leave the arena" via the same
        /// anchor-leash code used by ProtectAnchor. Idempotent; safe to call on phase entry.
        /// </summary>
        public void SetArenaLeash(Vector2 arenaCenter, float arenaRadiusTiles)
        {
            _movement.AnchorPoint = arenaCenter;
            _movement.AnchorLeashTiles = Mathf.Max(1f, arenaRadiusTiles);
        }

        /// <summary>
        /// BossPhaseShift primitive: swap the active ActionSet (and optionally the movement profile)
        /// on an external trigger. fable_05 calls this at each phase threshold; the brain just rebinds
        /// — it does NOT decide phase order or thresholds (that is fable_05's job). The movement type
        /// switch is honoured immediately (next tick uses the new profile's MovementType).
        /// </summary>
        public void ShiftPhase(EnemyActionSetSO newActionSet, EnemyMovementProfileSO newMovementProfile = null)
        {
            if (newActionSet != null)
            {
                _actions.ActiveActionSet = newActionSet;
                _actions.ActionCooldowns.Clear();
                if (_actionDatabase != null)
                {
                    foreach (var actionId in newActionSet.ActionIds)
                    {
                        if (_actionDatabase.TryGetById(actionId, out var action))
                            _actions.ActionCooldowns[actionId] = new EnemyActionRuntime(actionId, action.CooldownSeconds);
                    }
                }
            }

            if (newMovementProfile != null)
            {
                _movementProfile = newMovementProfile;
                _movement.UpdateRefs(_enemyData, _movementProfile, _telegraph);
            }

            // Interrupt any pending action so the new phase starts clean (telegraph cleared).
            _actions.PendingAction = null;
            _telegraph?.EndTelegraph();
            _currentState = EnemyBrainState.Alert;
            GameEventBus.Publish(new EnemyActionResolvedEvent(_enemyData?.enemyId, "phase_shift"));
        }

        /// <summary>
        /// fable_05 contract: swap the active ActionSet by id, resolving it through the brain's own
        /// action-set database. Thin wrapper over <see cref="ShiftPhase"/> so boss phases have the
        /// named entry point the spec asks for WITHOUT a second swap mechanism. Returns true when the
        /// id resolved and the swap was applied; false (no-op) when the id is empty/unknown so the
        /// caller can keep the current set. Safe to call in any state (ShiftPhase clears pending action).
        /// </summary>
        public bool SwapActionSet(string actionSetId)
        {
            if (string.IsNullOrWhiteSpace(actionSetId) || _actionSetDatabase == null)
            {
                return false;
            }

            if (!_actionSetDatabase.TryGetById(actionSetId, out var actionSet) || actionSet == null)
            {
                _telemetry.LogBossSwapActionSetMissing(_enemyData?.enemyId, actionSetId, this);
                return false;
            }

            ShiftPhase(actionSet);
            return true;
        }

        /// <summary>
        /// fable_05: per-phase move/damage multipliers applied by the BossBrainController. The brain
        /// folds these into MoveSpeed()/external damage scaling so a single boss reads differently per
        /// phase without a parallel stat system. Idempotent; safe to re-apply on phase entry.
        /// </summary>
        public void ApplyPhaseMultipliers(float moveSpeedMultiplier, float damageMultiplier)
        {
            _phaseMoveSpeedMultiplier = moveSpeedMultiplier > 0f ? moveSpeedMultiplier : 1f;
            _phaseDamageMultiplier = damageMultiplier > 0f ? damageMultiplier : 1f;
        }

        /// <summary>fable_05: current damage multiplier from the active boss phase (1 when no phase).</summary>
        public float PhaseDamageMultiplier => _phaseDamageMultiplier;

        // ─── fable_04: threat / pack coordination ─────────────────────────────

        public string PackId => _packId;

        /// <summary>Test/runtime hook: true while threat memory keeps this enemy engaged.</summary>
        public bool HasActiveThreat()
        {
            return _threatMemoryEnabled && _threatState.HasThreat(Time.time);
        }

        public Vector2 LastKnownTargetPosition => _threatState.LastKnownPosition;

        /// <summary>Rollback switch (spec): disabling reverts to instant-distance leash behaviour.</summary>
        public void SetThreatMemoryEnabled(bool enabled) => _threatMemoryEnabled = enabled;

        /// <summary>
        /// External alert from the pack coordinator: a sibling engaged or died. Wake up toward the
        /// reported position so the whole pack converges within one decision tick (CA-2), unless
        /// busy attacking or stunned. Seeds threat memory so the alert outlives the trigger.
        /// </summary>
        public void OnPackAlert(Vector2 position)
        {
            if (!_threatMemoryEnabled)
            {
                return;
            }

            if (_currentState == EnemyBrainState.Dead ||
                _currentState == EnemyBrainState.AttackWindup ||
                _currentState == EnemyBrainState.AttackRecover ||
                _currentState == EnemyBrainState.Stunned)
            {
                // Still remember the threat; the state will resolve after the action/stun ends.
                _threatState.NoticeTarget(position, Time.time);
                return;
            }

            _threatState.NoticeTarget(position, Time.time);
            _telemetry.ResetThreatExpiredLog();
            if (_currentState == EnemyBrainState.Idle || _currentState == EnemyBrainState.Patrol)
            {
                _currentState = EnemyBrainState.Alert;
            }
        }

        public void ApplyStun(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _stunUntil = Mathf.Max(_stunUntil, Time.time + Mathf.Min(seconds, 5f));
            _currentState = EnemyBrainState.Stunned;
            _actions.PendingAction = null;
            _telegraph?.EndTelegraph();
            _movement.StopMovement();
        }

        // First time this enemy detects the player, alert its pack so siblings engage together.
        private void AnnouncePackEngagementOnce()
        {
            if (_packEngagedAnnounced || _packCoordinator == null || string.IsNullOrWhiteSpace(_packId))
            {
                return;
            }

            _packEngagedAnnounced = true;
            _packCoordinator.Alert(_packId, _threatState.LastKnownPosition);
        }

        // Collective leash: only reset when the WHOLE pack is beyond leash. Resets to Patrol and
        // heals to full at the deterministic pack anchor (CA-3). Solo enemies (no pack) return false.
        private bool TryCollectivePackLeashReset()
        {
            if (_packCoordinator == null || string.IsNullOrWhiteSpace(_packId))
            {
                return false;
            }

            if (!_packCoordinator.IsWholePackBeyondLeash(_packId, LeashRange()))
            {
                return false;
            }

            ResetToAnchorAndHeal(_packCoordinator.GetAnchor(_packId));
            return true;
        }

        // Reset this enemy to Patrol at the pack anchor and restore HP to full (heal on leash reset).
        // HP restore uses EnemyHealth's existing clamped RestoreHp API (no new save coupling).
        private void ResetToAnchorAndHeal(Vector2 anchor)
        {
            _threatState.Clear();
            _packEngagedAnnounced = false;
            _currentState = EnemyBrainState.Patrol;
            _movement.StopMovement();
            _movement.SpawnAnchor = anchor;

            if (_health != null && _health.MaxHp > 0)
            {
                _health.RestoreHp(_health.MaxHp);
            }

            _telemetry.LogEnemyPackLeashReset(_enemyData?.enemyId, _packId, anchor, this);
        }

        public void SetState(EnemyBrainState newState) => _currentState = newState;
    }

    public enum EnemyBrainState
    {
        Idle,
        Patrol,
        Alert,
        Chase,
        AttackWindup,
        AttackRecover,
        Stunned,
        Dead,
        GuardHold,
        Kite,
        Burrow,
        SwarmGroup,
        Retreat,
        CastPrepare
    }
}
