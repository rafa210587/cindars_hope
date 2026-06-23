using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
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
        private float _patrolDirectionTimer;
        private Vector2 _spawnAnchor;
        private float _retreatEndTime;
        private float _nextLeapTime;
        private float _nextBlinkTime;
        private bool _isLeaping;
        private float _leapEndTime;
        // Alternates per spawned brain so phase enemies do not all flank the same side.
        private static float s_nextBlinkFlankSide = 1f;
        private float _blinkFlankSide = 1f;

        // Target
        private GameObject _playerTarget;

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

        // fable_04: threat/aggro memory + pack coordination.
        private readonly EnemyThreatState _threatState = new EnemyThreatState();
        // Internal feature flag (rollback): disabling reverts to instant-distance leash behaviour.
        private bool _threatMemoryEnabled = true;
        private EnemyPackCoordinator _packCoordinator;
        private string _packId;
        private bool _packEngagedAnnounced;
        private bool _threatExpiredLogged;

        // Action runtime
        private EnemyActionSetSO _activeActionSet;
        private readonly Dictionary<string, EnemyActionRuntime> _actionCooldowns = new Dictionary<string, EnemyActionRuntime>();
        private EnemyActionSO _pendingAction;
        private float _actionTimer;
        private bool _actionResolved;
        // SPEC 13D: idempotency guard — death-trigger fires exactly once per lifetime.
        private bool _deathtriggerFired;

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

        // F01: modificadores externos aplicados por status effects (Chill/Slow/Root/Fear/ConfusionLite).
        private float _externalSpeedMultiplier = 1f;
        private float _externalSpeedUntil;
        private float _externalInvertUntil;
        private float _forcedRetreatUntil;

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

        // fable_24: named-elite affix (decided deterministically at spawn-plan time by the planner).
        private EliteAffix _eliteAffix = EliteAffix.None;
        private bool _wardedStatusConsumed;          // Warded resists exactly the FIRST status applied
        private bool _volatileExploded;              // Volatile fires its death explosion once
        private float _attackCadenceFactor = 1f;     // Frenzied shortens windup/recover (<1 = faster)

        // fable_24: move-specific runtime for the 12 new moves.
        private float _moveDecisionTimer;            // orbit/strafe re-aim cadence
        private float _orbitSign = 1f;               // CircleStrafe/FloatingOrbit rotation direction
        private bool _chargeTelegraphActive;
        private float _chargeTelegraphStart;
        private Vector2 _chargeLockedDirection;      // ChargeLine locks aim at telegraph time (no homing)
        private bool _isCharging;
        private float _chargeEndTime;
        private float _nextChargeTime;
        private bool _mimicActivated;                // TreasureIdleAmbush stays disguised until activated
        private float _flankerNoLeaderSince = -1f;   // timestamp the flanker last lacked a leader
        private float _nextCallForHelpTime;          // RetreatAndCall call-for-help throttle
        private Vector2 _anchorPoint;                // ProtectAnchor / BossArenaControl leash centre
        private float _anchorLeashTiles = EnemyMoveLogic.DefaultAnchorLeashTiles;

        // F02: stagger por quebra de postura — entra em Stunned e sai sozinho.
        private float _stunUntil;

        // fable_05: per-phase multipliers driven by BossBrainController (1 = unchanged / non-boss).
        private float _phaseMoveSpeedMultiplier = 1f;
        private float _phaseDamageMultiplier = 1f;

        public void ApplyStun(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _stunUntil = Mathf.Max(_stunUntil, Time.time + Mathf.Min(seconds, 5f));
            _currentState = EnemyBrainState.Stunned;
            _pendingAction = null;
            _telegraph?.EndTelegraph();
            StopMovement();
        }

        // SPEC 14A-FIX6: Public state for real runtime resolution checks
        public bool HasResolvedActionSet => _activeActionSet != null && _activeActionSet.ActionIds != null && _activeActionSet.ActionIds.Length > 0;
        public int ResolvedActionCount => _actionCooldowns.Count;
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
        public string ResolvedActionSetId => _activeActionSet?.ActionSetId ?? string.Empty;
        public string ResolvedMovementProfileId => _movementProfile?.MovementProfileId ?? string.Empty;
        public string ResolvedVulnerabilityProfileId => _vulnerabilityProfile?.VulnerabilityProfileId ?? string.Empty;

        public void Configure(EnemyDataSO data)
        {
            _enemyData = data;
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
            _conflictCombatant = GetComponent<CindarsHope.Cave.Ecosystem.CaveConflictCombatant>();
        }

        private void OnEnable()
        {
            _decisionTimer = 0f;
            _patrolDirectionTimer = 0f;
            _actionResolved = false;
            _pendingAction = null;
            _currentState = EnemyBrainState.Idle;
            _spawnAnchor = transform.position;
            _isLeaping = false;
            _blinkFlankSide = s_nextBlinkFlankSide;
            s_nextBlinkFlankSide = -s_nextBlinkFlankSide;

            // fable_04: reset transient aggro on (re)spawn; memory window follows movement type.
            _threatState.Clear();
            _threatState.SetMemorySeconds(EnemyThreatState.ResolveMemorySeconds(MovementType));
            _packEngagedAnnounced = false;
            _threatExpiredLogged = false;

            // fable_24: reset move/elite transient runtime on (re)spawn. The elite affix itself is
            // re-applied by the materializer via ConfigureElite after this; clear the one-shot flags.
            _wardedStatusConsumed = false;
            _volatileExploded = false;
            _mimicActivated = false;
            _isCharging = false;
            _chargeTelegraphActive = false;
            _flankerNoLeaderSince = -1f;
            _orbitSign = s_nextBlinkFlankSide; // reuse the alternating side so packs spread out
            _anchorPoint = transform.position;
            if (_movementProfile != null && _movementProfile.WanderRadius > 0f)
                _anchorLeashTiles = _movementProfile.WanderRadius;

            RefreshPlayerTarget();

            // fable_78: re-resolve o combatant caso ele tenha sido anexado após o Awake do brain
            // (o materializer adiciona o brain e depois, condicionalmente, o combatant).
            if (_conflictCombatant == null)
            {
                _conflictCombatant = GetComponent<CindarsHope.Cave.Ecosystem.CaveConflictCombatant>();
            }
            _rivalHealthTarget = null;

            if (_vulnerabilityState != null)
                _vulnerabilityState.Initialize(_enemyData?.enemyId);

            if (_spriteRenderer != null)
                _spriteBaseAlpha = _spriteRenderer.color.a;

            InitActionSet();

            if (_movementProfile != null && _movementProfile.DecisionTickSeconds > 0f)
                _decisionTickSeconds = _movementProfile.DecisionTickSeconds;
        }

        private void OnDisable()
        {
            StopMovement();
            SetSubmergedVisual(false);
        }

        private void InitActionSet()
        {
            _actionCooldowns.Clear();
            _activeActionSet = null;

            if (_enemyData == null || string.IsNullOrEmpty(_enemyData.ActionSetId)) return;
            if (_actionSetDatabase == null) return;
            if (!_actionSetDatabase.TryGetById(_enemyData.ActionSetId, out _activeActionSet)) return;
            if (_actionDatabase == null) return;

            foreach (var actionId in _activeActionSet.ActionIds)
            {
                if (_actionDatabase.TryGetById(actionId, out var action))
                    _actionCooldowns[actionId] = new EnemyActionRuntime(actionId, action.CooldownSeconds);
            }
        }

        // ─── Main Loop ────────────────────────────────────────────────────────

        private void Update()
        {
            if (_currentState == EnemyBrainState.Dead) return;

            // O player VISIVEL (PlayerController) pode nascer depois do inimigo materializar; re-resolve
            // a cada frame ate apontar para ele. Sem isto o target fica no PlayerManager (no _Bootstrap,
            // em (0,0,0)) e o inimigo mede distancia ate a origem do mundo — atacando o vazio, mas
            // roteando o dano ao player real longe dali (bug "dano invisivel de bicho que nao esta perto").
            RefreshPlayerTarget();

            // fable_78 (SLICE 4): targeting conflict-aware. Ramo ISOLADO — só roda quando este inimigo é um
            // conflict-combatant. Caso contrário, _rivalHealthTarget fica null e o caminho player-only é
            // intacto (RefreshPlayerTarget já restaurou _playerTarget para o player visível).
            RefreshConflictTarget();

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

            TickActionTimers();
            ExecuteMovement();

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
            bool inLeash = dist <= LeashRange();

            // fable_04: while the player is in range, refresh threat memory and (once) wake the pack.
            if (_threatMemoryEnabled && inDetect && _playerTarget != null)
            {
                _threatState.NoticeTarget(_playerTarget.transform.position, Time.time);
                _threatExpiredLogged = false;
                AnnouncePackEngagementOnce();
            }

            // F01: Fear externo força Retreat (fora de windup/recover — guard acima já retornou).
            if (Time.time < _forcedRetreatUntil && _currentState != EnemyBrainState.Retreat)
            {
                _currentState = EnemyBrainState.Retreat;
                _retreatEndTime = Mathf.Max(_retreatEndTime, _forcedRetreatUntil);
                return;
            }

            // Skittish roles break off and flee when badly hurt, regardless of current state.
            if (_currentState != EnemyBrainState.Retreat && ShouldRetreatAtLowHealth() && inLeash)
            {
                _currentState = EnemyBrainState.Retreat;
                _retreatEndTime = Time.time + _retreatDurationSeconds;
                return;
            }

            switch (_currentState)
            {
                case EnemyBrainState.Idle:
                    _currentState = inDetect ? EnemyBrainState.Alert : EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Patrol:
                    if (inDetect) _currentState = ResolveEngageState(dist);
                    break;

                case EnemyBrainState.Alert:
                    // fable_04: an alerted enemy chases the last known position until threat
                    // memory expires, even if it never personally saw the player (pack alert).
                    if (inDetect)
                        _currentState = ResolveEngageState(dist);
                    else if (HasActiveThreat())
                        _currentState = EnemyBrainState.Chase;
                    else
                        _currentState = EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Retreat:
                    if (Time.time >= _retreatEndTime)
                        _currentState = inDetect ? ResolveEngageState(dist) : EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Burrow:
                    if (!inLeash)
                    {
                        SetSubmergedVisual(false);
                        _currentState = EnemyBrainState.Patrol;
                        break;
                    }

                    if (dist <= _burrowEmergeDistance)
                    {
                        SetSubmergedVisual(false);
                        _currentState = EnemyBrainState.Chase;
                        TryBeginAction(dist);
                    }
                    break;

                case EnemyBrainState.Chase:
                case EnemyBrainState.Kite:
                case EnemyBrainState.GuardHold:
                case EnemyBrainState.CastPrepare:
                    if (!inLeash)
                    {
                        // fable_04: do not give up the instant the player crosses the leash edge.
                        // Keep pursuing the last known position while threat memory is valid; only
                        // disengage once it expires. Disengage is collective when the enemy belongs
                        // to a pack and the WHOLE pack is beyond leash (reset together + heal).
                        if (HasActiveThreat())
                        {
                            break; // remain engaged, MoveChase will pursue LastKnownPosition
                        }

                        LogThreatExpiredOnce();

                        if (TryCollectivePackLeashReset())
                        {
                            break;
                        }

                        _currentState = EnemyBrainState.Patrol;
                        break;
                    }
                    TryBeginAction(dist);
                    break;
            }
        }

        // Burrowers approach hidden underground; everyone else goes straight to Chase.
        private EnemyBrainState ResolveEngageState(float dist)
        {
            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;
            bool canBurrow = moveType == EnemyMovementType.BurrowAmbush || (_movementProfile != null && _movementProfile.CanBurrow);
            if (canBurrow && dist > _burrowEmergeDistance * 2f)
            {
                SetSubmergedVisual(true);
                return EnemyBrainState.Burrow;
            }

            return EnemyBrainState.Chase;
        }

        private bool ShouldRetreatAtLowHealth()
        {
            if (_health == null || _enemyData == null || _health.MaxHp <= 0)
                return false;

            var role = _enemyData.PrimaryRole;
            if (role != EnemyRole.Swarm && role != EnemyRole.Ranged && role != EnemyRole.Caster)
                return false;

            return (float)_health.CurrentHp / _health.MaxHp <= _lowHealthRetreatThreshold;
        }

        private void TryBeginAction(float dist)
        {
            var action = SelectBestAction(dist);
            if (action != null)
            {
                BeginAction(action);
                return;
            }

            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;
            switch (moveType)
            {
                case EnemyMovementType.GuardStationary:
                    _currentState = EnemyBrainState.GuardHold;
                    break;
                case EnemyMovementType.KiteRanged:
                case EnemyMovementType.CasterKeepAway:
                    _currentState = EnemyBrainState.Kite;
                    break;
                default:
                    _currentState = EnemyBrainState.Chase;
                    break;
            }
        }

        // ─── Action Selection ─────────────────────────────────────────────────

        private EnemyActionSO SelectBestAction(float dist)
        {
            if (_activeActionSet == null || _actionDatabase == null) return null;

            foreach (var actionId in _activeActionSet.ActionIds)
            {
                if (!_actionDatabase.TryGetById(actionId, out var action)) continue;
                if (!_actionCooldowns.TryGetValue(actionId, out var runtime)) continue;
                if (!runtime.IsReady(Time.time)) continue;

                if (action.ActionType == EnemyActionType.SelfBuff)
                    return action;

                if (dist >= action.MinRange && dist <= action.Range)
                    return action;
            }
            return null;
        }

        // ─── Action Execution ─────────────────────────────────────────────────

        private void BeginAction(EnemyActionSO action)
        {
            _pendingAction = action;
            // fable_24: Frenzied elites attack 30% faster — scale the windup (cadence factor <1).
            _actionTimer = action.WindupSeconds * _attackCadenceFactor;
            _actionResolved = false;
            _currentState = EnemyBrainState.AttackWindup;

            StartTelegraph(action);
            GameEventBus.Publish(new EnemyActionStartedEvent(_enemyData?.enemyId, action.ActionId));
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, transform.position));
        }

        private void TickActionTimers()
        {
            if (_currentState == EnemyBrainState.AttackWindup)
            {
                _actionTimer -= Time.deltaTime;
                if (_actionTimer <= 0f && !_actionResolved)
                {
                    _actionResolved = true;
                    ResolveAction();
                    _telegraph?.EndTelegraph();
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.DuringChargeWindup);
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterCast);
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterProjectileVolley);

                    // fable_24: Frenzied also shortens recovery (same cadence factor as windup).
                    _actionTimer = (_pendingAction?.RecoverSeconds ?? 0.5f) * _attackCadenceFactor;
                    _currentState = EnemyBrainState.AttackRecover;
                }
            }
            else if (_currentState == EnemyBrainState.AttackRecover)
            {
                _actionTimer -= Time.deltaTime;
                if (_actionTimer <= 0f)
                {
                    TryOpenVulnerabilityWindow(VulnerabilityTriggerMode.AfterAttackRecover);

                    if (_pendingAction != null && _actionCooldowns.TryGetValue(_pendingAction.ActionId, out var rt))
                        rt.MarkUsed(Time.time);

                    GameEventBus.Publish(new EnemyActionResolvedEvent(_enemyData?.enemyId, _pendingAction?.ActionId));
                    _pendingAction = null;

                    float dist = DistanceToPlayer();
                    _currentState = dist <= DetectionRange() ? EnemyBrainState.Chase : EnemyBrainState.Patrol;
                }
            }
        }

        private void ResolveAction()
        {
            if (_pendingAction == null) return;
            if (_pendingAction.ActionType == EnemyActionType.SelfBuff) return;
            if (_pendingAction.BaseDamage <= 0) return;
            if (_playerTarget == null) return;

            if (!System.Enum.TryParse<DamageType>(_pendingAction.DamageType, true, out var dmgType))
                dmgType = DamageType.Physical;

            // fable_78 (SLICE 4): quando o alvo é um rival (conflito inter-monstro), o dano vai para o
            // EnemyHealth do rival via o caminho de origem-inimigo (×0.10 + "Ferido" + kill-by-enemy).
            // Não toca o pipeline de dano ao jogador. Ramo isolado por IsTargetingRival.
            if (IsTargetingRival)
            {
                ResolveInterMonsterAction(dmgType);
                return;
            }

            // SPEC 13D: blink-strike teleports the enemy to the player then deals melee damage.
            if (_pendingAction.ActionType == EnemyActionType.BlinkStrike)
            {
                ExecuteBlinkStrike(_pendingAction, dmgType);
                return;
            }

            // Ranged and cast actions fire a real dodgeable projectile instead of
            // instant damage — the player can outplay them with movement.
            bool isProjectileAction = _pendingAction.ActionType == EnemyActionType.RangedProjectile
                || _pendingAction.ActionType == EnemyActionType.CastProjectile;
            if (isProjectileAction)
            {
                float speed = _pendingAction.ProjectileSpeed > 0f ? _pendingAction.ProjectileSpeed : 5f;
                // fable_05: per-phase damage multiplier folds into the action's base damage.
                int projectileDamage = Mathf.Max(0, Mathf.RoundToInt(_pendingAction.BaseDamage * _phaseDamageMultiplier));
                EnemyProjectileBehaviour.SpawnTowards(
                    transform.position,
                    DirectionToPlayer(),
                    speed,
                    Mathf.Max(_pendingAction.Range, 2f),
                    projectileDamage,
                    dmgType,
                    _enemyData?.contactKnockbackForce ?? 0f,
                    _enemyData?.enemyId ?? "enemy",
                    _enemyData?.DisplayName ?? "Enemy");
                return;
            }

            // Melee/area resolution: the player may have moved during windup. Re-check
            // distance with a small grace margin so dodging the telegraph actually works.
            float dist = DistanceToPlayer();
            float effectiveRange = _pendingAction.ActionType == EnemyActionType.AreaPulse && _pendingAction.AreaRadius > 0f
                ? _pendingAction.AreaRadius
                : _pendingAction.Range;
            if (dist > effectiveRange * 1.2f)
            {
                CindarsHope.Combat.CombatLog.Log($"CombatLog: EnemyActionMissed. EnemyId={_enemyData?.enemyId}, ActionId={_pendingAction.ActionId}, Distance={dist:F2}, Range={effectiveRange:F2}");
                return;
            }

            // fable_05: per-phase damage multiplier folds into the action's base damage.
            int meleeDamage = Mathf.Max(0, Mathf.RoundToInt(_pendingAction.BaseDamage * _phaseDamageMultiplier));
            var request = new DamageRequest(
                targetId: "player",
                baseDamage: meleeDamage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            request.SourcePosition = transform.position;
            request.KnockbackForce = _enemyData?.contactKnockbackForce ?? 0f;
            request.CanTriggerVulnerability = false;

            var result = DamageCalculator.Calculate(request, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                // F27: caminho central com atacante (perfect block reflete postura neste GO).
                var playerManager = GameBootstrap.Instance?.PlayerManager;
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, gameObject, _playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(_pendingAction);
                    ApplyVampiricLifesteal(applied);
                    var playerPos = _playerTarget != null ? (Vector2)_playerTarget.transform.position : (Vector2)playerManager.transform.position;
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, playerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(_playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }
        }

        // fable_78 (SLICE 4): resolve um ataque contra o rival corrente. Re-checa alcance (o rival pode ter
        // se movido durante o windup, igual ao caminho do player) e roteia o dano base do action pelo
        // caminho de origem-inimigo do EnemyHealth (×InterMonsterDamageMultiplier + "Ferido" + kill-by-enemy).
        // Reusa o mesmo locomotor/estado: não cria projétil/pathfinding novo (área/ranged tratam como hit direto).
        private void ResolveInterMonsterAction(DamageType dmgType)
        {
            var rival = _rivalHealthTarget;
            if (rival == null || rival.IsDead || _ecosystemBalance == null)
            {
                return;
            }

            float dist = Vector2.Distance(transform.position, rival.transform.position);
            float effectiveRange = _pendingAction.ActionType == EnemyActionType.AreaPulse && _pendingAction.AreaRadius > 0f
                ? _pendingAction.AreaRadius
                : _pendingAction.Range;
            if (dist > effectiveRange * 1.2f)
            {
                return;
            }

            int rawDamage = Mathf.Max(0, Mathf.RoundToInt(_pendingAction.BaseDamage * _phaseDamageMultiplier));
            if (rawDamage <= 0)
            {
                return;
            }

            int caveLevel = _conflictCombatant != null ? _conflictCombatant.CaveLevel : 0;
            string killerInstanceId = _health != null ? _health.EnemyInstanceId : gameObject.name;

            rival.TakeDamageFromEnemy(rawDamage, dmgType, killerInstanceId, caveLevel, _ecosystemBalance);
        }

        // SPEC 13D: Teleports to player then deals melee damage + applies status effects.
        // Destination uses _blinkFlankSide (set at spawn, alternating) — no UnityEngine.Random.
        private void ExecuteBlinkStrike(EnemyActionSO action, DamageType dmgType)
        {
            if (_playerTarget == null) return;

            float range = action.BlinkRange > 0f ? action.BlinkRange : Mathf.Max(action.Range, 0.5f);
            var blink = EnemyBlinkExecutor.CalculateDestination(
                transform.position, _playerTarget.transform.position, range, _blinkFlankSide);

            if (!blink.Success)
            {
                Debug.Log($"[EnemyBrain] BlinkStrike skipped: {blink.FailReason}");
                return;
            }

            if (_rb != null)
                _rb.position = blink.Destination;
            else
                transform.position = (Vector3)blink.Destination;

            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, blink.Destination));

            // Apply melee damage after teleport
            int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _phaseDamageMultiplier));
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
                var playerManager = GameBootstrap.Instance?.PlayerManager;
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, gameObject, _playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(action);
                    ApplyVampiricLifesteal(applied);
                    var playerPos = _playerTarget != null ? (Vector2)_playerTarget.transform.position : (Vector2)playerManager.transform.position;
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, playerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(_playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }
        }

        // SPEC 13D: Fires the death-trigger action (IsDeathtrigger=true) exactly once on death.
        // Guard: only inside cave (CaveRunManager present) to avoid out-of-cave effects.
        private void FireDeathTrigger()
        {
            if (_deathtriggerFired) return;
            if (_activeActionSet == null || _actionDatabase == null) return;
            if (CaveRunManager.Instance == null) return;

            foreach (var actionId in _activeActionSet.ActionIds)
            {
                if (!_actionDatabase.TryGetById(actionId, out var action)) continue;
                if (!action.IsDeathtrigger) continue;

                _deathtriggerFired = true;
                ExecuteDeathTrigger(action);
                return;
            }
        }

        // Applies AoE damage from the death-trigger action to the player if in range.
        private void ExecuteDeathTrigger(EnemyActionSO action)
        {
            if (action.BaseDamage <= 0) return;
            if (!System.Enum.TryParse<DamageType>(action.DamageType, true, out var dmgType))
                dmgType = DamageType.Fire;

            float radius = action.AreaRadius > 0f ? action.AreaRadius : action.Range;
            var playerManager = GameBootstrap.Instance?.PlayerManager;
            if (playerManager == null) return;

            var dtPlayerPos = _playerTarget != null ? _playerTarget.transform.position : playerManager.transform.position;
            float dist = Vector2.Distance(transform.position, dtPlayerPos);
            if (dist > radius * 1.2f) return;

            int damage = Mathf.Max(0, Mathf.RoundToInt(action.BaseDamage * _phaseDamageMultiplier));
            var req = new DamageRequest(
                targetId: "player",
                baseDamage: damage,
                damageType: dmgType,
                sourceId: _enemyData?.enemyId ?? "enemy"
            );
            req.SourcePosition = transform.position;
            req.CanTriggerVulnerability = false;
            var result = DamageCalculator.Calculate(req, _enemyData?.defense ?? 0);
            if (result.FinalDamage > 0)
            {
                var applied = CindarsHope.Combat.PlayerDamageReceiver.ApplyDamage(
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, gameObject, _playerTarget);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(action);
                    GameEventBus.Publish(new PlayerDamagedEvent(applied, (Vector2)dtPlayerPos, _enemyData?.enemyId ?? "enemy", _enemyData?.DisplayName ?? "Enemy"));
                    FloatingDamageNumberDisplayer.ShowAtTarget(_playerTarget ?? playerManager.gameObject, applied, dmgType, false, true);
                }
            }

            Debug.Log($"[EnemyBrain] DeathTrigger fired. EnemyId={_enemyData?.enemyId}, ActionId={action.ActionId}, Damage={damage}, PlayerDist={dist:F2}");
        }

        // F01: EnemyActionSO.StatusApplicationIds aplicados no player via PlayerStatusReceiver.
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
            if (_pendingAction == null || !_pendingAction.TriggersVulnerabilityWindow) return;
            if (_vulnerabilityState == null) return;

            var mode = _pendingAction.VulnerabilityWindowTrigger;
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

        // ─── Telegraph ────────────────────────────────────────────────────────

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

        // ─── Movement ─────────────────────────────────────────────────────────

        private void ExecuteMovement()
        {
            if (_rb == null) return;

            if (_isLeaping)
            {
                if (Time.time >= _leapEndTime)
                    _isLeaping = false;
                else
                    return; // leap velocity is in flight; do not override it
            }

            // fable_24: ChargeLine investida overrides normal movement while in flight (straight line).
            if (_isCharging)
            {
                if (Time.time >= _chargeEndTime)
                {
                    _isCharging = false;
                }
                else
                {
                    _rb.linearVelocity = EnemyMoveLogic.ResolveChargeVelocity(_chargeLockedDirection, MoveSpeed() * 3.2f);
                    return;
                }
            }

            switch (_currentState)
            {
                case EnemyBrainState.Patrol:
                    MovePatrol();
                    break;
                case EnemyBrainState.Chase:
                    MoveChase();
                    break;
                case EnemyBrainState.Kite:
                    MoveKite();
                    break;
                case EnemyBrainState.Retreat:
                    MoveRetreat();
                    break;
                case EnemyBrainState.Burrow:
                    MoveBurrow();
                    break;
                case EnemyBrainState.AttackWindup:
                case EnemyBrainState.AttackRecover:
                    StopMovement();
                    break;
                case EnemyBrainState.GuardHold:
                    MoveGuardHold();
                    break;
            }
        }

        private void MoveChase()
        {
            // fable_24: dispatch the new moves first; they fully own the chase velocity for their tick.
            if (TryMoveNewBehaviour())
            {
                return;
            }

            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;
            float speed = MoveSpeed();

            // fable_04: if the player slipped out of detection range but threat memory is still
            // valid, pursue the last known position instead of stopping. Special movement (leap,
            // blink, swarm jitter) only triggers when the player is actually visible/in range.
            bool playerVisible = _playerTarget != null && DistanceToPlayer() <= DetectionRange();
            if (!playerVisible)
            {
                if (_threatMemoryEnabled && HasActiveThreat())
                {
                    MoveTowardLastKnownPosition(speed);
                }
                else if (_playerTarget == null)
                {
                    StopMovement();
                }
                else
                {
                    _rb.linearVelocity = DirectionToPlayer() * speed;
                }
                return;
            }

            switch (moveType)
            {
                case EnemyMovementType.SwarmErratic:
                    MoveSwarmErratic(speed);
                    return;
                case EnemyMovementType.TankSlowPush:
                    speed = Mathf.Min(speed, 1.5f);
                    break;
                case EnemyMovementType.Leaper:
                    if (TryLeap(speed))
                        return;
                    break;
                case EnemyMovementType.PhaseShortBlink:
                    if (TryBlink())
                        return;
                    break;
            }

            _rb.linearVelocity = DirectionToPlayer() * speed;
        }

        // ─── fable_24: the 12 new moves ───────────────────────────────────────
        // Returns true when one of the new moves handled this tick's velocity (so MoveChase stops).
        private bool TryMoveNewBehaviour()
        {
            float speed = MoveSpeed();
            switch (EffectiveMove)
            {
                case EnemyMovementType.CircleStrafe:
                case EnemyMovementType.FloatingOrbit:
                    MoveOrbit(speed);
                    return true;

                case EnemyMovementType.FloatingSlow:
                    MoveFloatingSlow(speed);
                    return true;

                case EnemyMovementType.ChargeLine:
                    MoveChargeLine(speed);
                    return true;

                case EnemyMovementType.RetreatAndCall:
                    MoveRetreatAndCall(speed);
                    return true;

                case EnemyMovementType.HazardLure:
                    // Lure the player by backing away (toward a hazard the level designer placed);
                    // straight retreat reuses the existing retreat vector (no pathfinding).
                    MoveRetreat();
                    return true;

                case EnemyMovementType.TreasureIdleAmbush:
                    MoveMimicAmbush(speed);
                    return true;

                case EnemyMovementType.ProtectAnchor:
                case EnemyMovementType.BossArenaControl:
                    MoveAnchoredChase(speed);
                    return true;

                case EnemyMovementType.PackFlanker:
                    MovePackFlanker(speed);
                    return true;

                case EnemyMovementType.PackLeader:
                    // Leader chases normally; its death (handled via pack alert) turns flankers to
                    // RetreatAndCall. No special steering here — fall through to default chase.
                    return false;

                default:
                    return false;
            }
        }

        // CircleStrafe / FloatingOrbit: hold firing distance and orbit the player.
        private void MoveOrbit(float speed)
        {
            if (_playerTarget == null)
            {
                StopMovement();
                return;
            }

            float preferred = Mathf.Max(_movementProfile?.PreferredDistance ?? 4f, 1f);
            float dist = DistanceToPlayer();
            Vector2 toPlayer = DirectionToPlayer();
            Vector2 tangent = Vector2.Perpendicular(toPlayer) * _orbitSign;

            // Blend a radial correction (keep ~preferred distance) with the tangential orbit.
            float radialError = dist - preferred;
            Vector2 radial = toPlayer * Mathf.Clamp(radialError, -1f, 1f);
            _rb.linearVelocity = (tangent + radial).normalized * speed;
        }

        // FloatingSlow: hover toward the player at reduced speed, ignoring floor obstacles (no burrow).
        private void MoveFloatingSlow(float speed)
        {
            if (_playerTarget == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = DirectionToPlayer() * (speed * 0.6f);
        }

        // ChargeLine: telegraph a straight line, lock the aim, then charge straight (dodgeable).
        private void MoveChargeLine(float speed)
        {
            if (_playerTarget == null)
            {
                StopMovement();
                return;
            }

            bool offCooldown = Time.time >= _nextChargeTime;

            if (!_chargeTelegraphActive && offCooldown && !_isCharging)
            {
                // Begin telegraph: lock direction now so the player can sidestep the committed line.
                _chargeTelegraphActive = true;
                _chargeTelegraphStart = Time.time;
                _chargeLockedDirection = DirectionToPlayer();
                _telegraph?.StartTelegraph(Color.red);
                GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, transform.position));
                StopMovement();
                return;
            }

            if (_chargeTelegraphActive)
            {
                float elapsed = Time.time - _chargeTelegraphStart;
                float telegraphDuration = EnemyMoveLogic.CommonWindupSeconds;
                StopMovement(); // hold still during the windup
                if (EnemyMoveLogic.ShouldChargeLineFire(true, elapsed, telegraphDuration, offCooldown))
                {
                    _chargeTelegraphActive = false;
                    _isCharging = true;
                    _chargeEndTime = Time.time + 0.4f;
                    _nextChargeTime = Time.time + 3f;
                    _telegraph?.EndTelegraph();
                }
                return;
            }

            // Between charges: close in at normal speed.
            _rb.linearVelocity = DirectionToPlayer() * speed;
        }

        // RetreatAndCall: flee and periodically emit EnemyCallForHelpEvent so allies regroup.
        private void MoveRetreatAndCall(float speed)
        {
            if (_playerTarget == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = -DirectionToPlayer() * (speed * 1.15f);
            EmitCallForHelp();
        }

        // TreasureIdleAmbush (mimic): stay disguised and immobile until the player is < 2 tiles.
        private void MoveMimicAmbush(float speed)
        {
            if (!_mimicActivated)
            {
                if (EnemyMoveLogic.ShouldMimicActivate(DistanceToPlayer()))
                {
                    _mimicActivated = true;
                    GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, transform.position));
                }
                else
                {
                    StopMovement();
                    return;
                }
            }

            // Once sprung, behave like a chaser.
            if (_playerTarget != null)
            {
                _rb.linearVelocity = DirectionToPlayer() * speed;
            }
        }

        // ProtectAnchor / BossArenaControl: chase, but never leave the leash radius of the anchor.
        private void MoveAnchoredChase(float speed)
        {
            if (_playerTarget == null)
            {
                ReturnToAnchor(speed);
                return;
            }

            Vector2 desired = (Vector2)transform.position + DirectionToPlayer();
            Vector2 clamped = EnemyMoveLogic.ClampToAnchor(desired, _anchorPoint, _anchorLeashTiles);

            if (EnemyMoveLogic.IsBeyondAnchorLeash((Vector2)transform.position, _anchorPoint, _anchorLeashTiles))
            {
                ReturnToAnchor(speed);
                return;
            }

            Vector2 step = clamped - (Vector2)transform.position;
            _rb.linearVelocity = step.sqrMagnitude > 0.0001f ? step.normalized * speed : Vector2.zero;
        }

        private void ReturnToAnchor(float speed)
        {
            Vector2 toAnchor = _anchorPoint - (Vector2)transform.position;
            _rb.linearVelocity = toAnchor.sqrMagnitude > 0.09f ? toAnchor.normalized * speed : Vector2.zero;
        }

        // PackFlanker: only engage while a living leader is in range; otherwise hold, then after a
        // timeout fall back to a plain chase (no deadlock). Leader presence comes from the pack
        // coordinator (no scene search).
        private void MovePackFlanker(float speed)
        {
            bool leaderAlive = PackLeaderInRange(out float distToLeader, out float awareness);
            if (EnemyMoveLogic.ShouldFlankerEngage(leaderAlive, distToLeader, awareness))
            {
                _flankerNoLeaderSince = -1f;
                if (_playerTarget != null)
                {
                    // Flank: approach offset to the side of the player instead of head-on.
                    Vector2 toPlayer = DirectionToPlayer();
                    Vector2 flank = Vector2.Perpendicular(toPlayer) * _orbitSign;
                    _rb.linearVelocity = (toPlayer + flank * 0.5f).normalized * speed;
                }
                return;
            }

            // No leader in range — start/continue the fallback timer.
            if (_flankerNoLeaderSince < 0f)
            {
                _flankerNoLeaderSince = Time.time;
            }

            if (EnemyMoveLogic.ShouldFlankerFallbackToChase(Time.time - _flankerNoLeaderSince) && _playerTarget != null)
            {
                _rb.linearVelocity = DirectionToPlayer() * speed; // act as GroundChase
            }
            else
            {
                StopMovement(); // hold, waiting for a leader
            }
        }

        // Pack leader lookup via the coordinator anchor as a stand-in for leader position (no scene
        // search). A pack with at least one living member is treated as having a leader in range when
        // the anchor is within awareness radius. Solo enemies (no pack) report no leader.
        private bool PackLeaderInRange(out float distanceToLeader, out float awarenessRadius)
        {
            awarenessRadius = Mathf.Max(_movementProfile?.DetectionRange ?? 10f, 1f);
            distanceToLeader = float.MaxValue;

            if (_packCoordinator == null || string.IsNullOrWhiteSpace(_packId))
            {
                return false;
            }

            // A living pack still has members registered; use the deterministic anchor as the leader
            // reference point (centroid of the pack's spawn — stable run / ADR-0005).
            if (_packCoordinator.MemberCount(_packId) <= 1)
            {
                return false;
            }

            Vector2 anchor = _packCoordinator.GetAnchor(_packId);
            distanceToLeader = Vector2.Distance(transform.position, anchor);
            return true;
        }

        private void EmitCallForHelp()
        {
            if (Time.time < _nextCallForHelpTime)
            {
                return;
            }

            _nextCallForHelpTime = Time.time + 2f;
            float radius = Mathf.Max(_movementProfile?.DetectionRange ?? 8f, 4f);
            GameEventBus.Publish(new EnemyCallForHelpEvent(_enemyData?.enemyId, transform.position, radius));

            // If part of a pack, also wake siblings directly (same path as fable_04 pack alert).
            if (_packCoordinator != null && !string.IsNullOrWhiteSpace(_packId))
            {
                _packCoordinator.Alert(_packId, transform.position);
            }
        }

        // fable_04: walk toward the remembered position; once reached, drop velocity so the next
        // decision tick can resolve back to Patrol when the memory finally expires (legible reset).
        private void MoveTowardLastKnownPosition(float speed)
        {
            Vector2 toTarget = _threatState.LastKnownPosition - (Vector2)transform.position;
            if (toTarget.sqrMagnitude <= 0.09f)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = toTarget.normalized * speed;
        }

        // Leapers lunge in a fast burst when the player is in the mid-range band.
        private bool TryLeap(float baseSpeed)
        {
            if (Time.time < _nextLeapTime)
                return false;

            float dist = DistanceToPlayer();
            float attackRange = _movementProfile?.AttackRange ?? 1.5f;
            if (dist < attackRange || dist > attackRange * 3.5f)
                return false;

            _isLeaping = true;
            _leapEndTime = Time.time + 0.35f;
            _nextLeapTime = Time.time + _leapCooldownSeconds;
            _rb.linearVelocity = DirectionToPlayer() * baseSpeed * _leapSpeedMultiplier;
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, transform.position));
            return true;
        }

        // Phase enemies blink to the player's flank instead of walking the gap.
        private bool TryBlink()
        {
            if (Time.time < _nextBlinkTime || _playerTarget == null)
                return false;

            float dist = DistanceToPlayer();
            float preferred = Mathf.Max(_movementProfile?.PreferredDistance ?? 1f, 0.9f);
            if (dist < preferred * 2.5f)
                return false;

            _nextBlinkTime = Time.time + _blinkCooldownSeconds;
            Vector2 toPlayer = DirectionToPlayer();
            Vector2 flank = Vector2.Perpendicular(toPlayer) * _blinkFlankSide;
            Vector2 destination = (Vector2)_playerTarget.transform.position - toPlayer * preferred + flank * 0.5f;
            _rb.position = destination;
            _rb.linearVelocity = Vector2.zero;
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData?.enemyId, destination));
            return true;
        }

        private void MoveBurrow()
        {
            if (_playerTarget == null) return;
            _rb.linearVelocity = DirectionToPlayer() * (MoveSpeed() * _burrowSpeedMultiplier);
        }

        private void MoveRetreat()
        {
            if (_playerTarget == null)
            {
                StopMovement();
                return;
            }

            _rb.linearVelocity = -DirectionToPlayer() * (MoveSpeed() * 1.25f);
        }

        // Guards hold their post: drift back to the spawn anchor when displaced.
        private void MoveGuardHold()
        {
            Vector2 toAnchor = _spawnAnchor - (Vector2)transform.position;
            if (toAnchor.sqrMagnitude > 0.36f)
                _rb.linearVelocity = toAnchor.normalized * (MoveSpeed() * 0.6f);
            else
                StopMovement();
        }

        private void MoveKite()
        {
            // fable_24: orbit/strafe and other new moves own their steering even in the Kite state.
            if (TryMoveNewBehaviour())
            {
                return;
            }

            if (_playerTarget == null) return;

            float speed = MoveSpeed();
            float preferred = _movementProfile?.PreferredDistance ?? 5f;
            float dist = DistanceToPlayer();
            Vector2 toPlayer = DirectionToPlayer();

            if (dist < preferred * 0.8f)
                _rb.linearVelocity = -toPlayer * speed;
            else if (dist > preferred * 1.2f)
                _rb.linearVelocity = toPlayer * (speed * 0.5f);
            else
                StopMovement();
        }

        private void MovePatrol()
        {
            _patrolDirectionTimer -= Time.deltaTime;
            if (_patrolDirectionTimer > 0f) return;

            _patrolDirectionTimer = Random.Range(1.5f, 3.5f);

            // Anchor patrol to the spawn point so idle enemies stay in their room
            // instead of drifting across the level over time.
            float wanderRadius = Mathf.Max(_movementProfile?.WanderRadius ?? 5f, 1f);
            Vector2 fromAnchor = (Vector2)transform.position - _spawnAnchor;
            Vector2 direction;
            if (fromAnchor.sqrMagnitude > wanderRadius * wanderRadius)
            {
                direction = (-fromAnchor).normalized;
            }
            else
            {
                direction = Random.insideUnitCircle.normalized;
            }

            _rb.linearVelocity = direction * (MoveSpeed() * 0.4f);
        }

        private void MoveSwarmErratic(float speed)
        {
            _patrolDirectionTimer -= Time.deltaTime;
            if (_patrolDirectionTimer > 0f) return;

            _patrolDirectionTimer = Random.Range(0.15f, 0.5f);
            Vector2 toPlayer = DirectionToPlayer();
            Vector2 erratic = (toPlayer * 0.6f + (Vector2)Random.insideUnitCircle * 0.8f).normalized;
            _rb.linearVelocity = erratic * speed;
        }

        private void StopMovement()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        private void SetSubmergedVisual(bool submerged)
        {
            if (_spriteRenderer == null) return;
            var color = _spriteRenderer.color;
            color.a = submerged ? _spriteBaseAlpha * 0.25f : _spriteBaseAlpha;
            _spriteRenderer.color = color;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        // Prefere o PlayerController VISIVEL na cena (transform real do personagem). O
        // PlayerManager mora no _Bootstrap (DontDestroyOnLoad, em (0,0,0)) e nao representa
        // a posicao do player — usa-lo como alvo faz o inimigo mirar a origem do mundo.
        private void RefreshPlayerTarget()
        {
            var visiblePlayer = Player.PlayerController.ActiveInstance;
            if (visiblePlayer != null)
            {
                _playerTarget = visiblePlayer.gameObject;
            }
            else if (_playerTarget == null)
            {
                _playerTarget = GameBootstrap.Instance?.PlayerManager?.gameObject;
            }
        }

        // fable_78 (SLICE 4): escolhe o alvo hostil corrente entre {player} ∪ {rivais vivos}, ponderado por
        // PlayerAggroWeight/RivalAggroWeight (default 1/1 → empate = mais próximo). Quando um rival vence,
        // aponta _playerTarget para o GameObject do rival (REUSA o locomotor/estado existente) e registra
        // _rivalHealthTarget para a resolução de dano divergir. Quando o player vence (ou não há rival vivo),
        // restaura o caminho player-only. Ramo isolado: no-op se este inimigo não é conflict-combatant.
        private void RefreshConflictTarget()
        {
            _rivalHealthTarget = null;

            if (_conflictCombatant == null)
            {
                return;
            }

            var visiblePlayer = Player.PlayerController.ActiveInstance;
            var playerGo = visiblePlayer != null ? visiblePlayer.gameObject : _playerTarget;

            float detection = DetectionRange();
            var rival = _conflictCombatant.FindNearestLivingRival(transform.position, detection);
            if (rival == null)
            {
                // Sem rival vivo no raio → mira o player como sempre.
                if (playerGo != null)
                {
                    _playerTarget = playerGo;
                }
                return;
            }

            float playerWeighted = ResolveWeightedDistance(playerGo, _ecosystemBalance?.PlayerAggroWeight ?? 1f);
            float rivalDist = Vector2.Distance(transform.position, rival.transform.position);
            float rivalWeighted = ResolveWeightedDistance(rivalDist, _ecosystemBalance?.RivalAggroWeight ?? 1f);

            if (rivalWeighted <= playerWeighted)
            {
                _rivalHealthTarget = rival;
                _playerTarget = rival.gameObject; // reusa movimento/distância/estado existentes
            }
            else if (playerGo != null)
            {
                _playerTarget = playerGo;
            }
        }

        // Distância "ponderada" por peso de aggro: peso menor torna o alvo mais atraente (divide a distância).
        // Peso <= 0 desliga o alvo (distância infinita). Peso 1 = distância crua (default empate = mais próximo).
        private float ResolveWeightedDistance(GameObject target, float weight)
        {
            if (target == null)
            {
                return float.MaxValue;
            }

            return ResolveWeightedDistance(Vector2.Distance(transform.position, target.transform.position), weight);
        }

        private static float ResolveWeightedDistance(float distance, float weight)
        {
            return weight <= 0f ? float.MaxValue : distance / weight;
        }

        // fable_78: true quando o alvo corrente é um rival (conflito), não o player.
        private bool IsTargetingRival => _rivalHealthTarget != null && !_rivalHealthTarget.IsDead;

        private float DistanceToPlayer() =>
            _playerTarget != null
                ? Vector2.Distance(transform.position, _playerTarget.transform.position)
                : float.MaxValue;

        private Vector2 DirectionToPlayer() =>
            _playerTarget != null
                ? ((Vector2)(_playerTarget.transform.position - transform.position)).normalized
                : Vector2.zero;

        private float DetectionRange() => _movementProfile?.DetectionRange ?? _enemyData?.detectionRadius ?? 10f;
        private float LeashRange() => _movementProfile?.LeashRange ?? (DetectionRange() * 3f);
        private float MoveSpeed() => (_movementProfile?.MoveSpeed ?? _enemyData?.moveSpeed ?? 2f) * ExternalSpeedFactor() * _phaseMoveSpeedMultiplier;

        // ─── Public API ───────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (_health != null)
            {
                _health.TakeDamage(amount);
                if (_health.IsDead)
                {
                    FireDeathTrigger();
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

            InitActionSet();
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
            _packId = string.IsNullOrWhiteSpace(packId) ? null : packId;

            if (_movementProfile != null && _movementProfile.DecisionTickSeconds > 0f)
                _decisionTickSeconds = _movementProfile.DecisionTickSeconds;

            // Memory window depends on movement type, which is now resolved.
            _threatState.SetMemorySeconds(EnemyThreatState.ResolveMemorySeconds(MovementType));

            if (_vulnerabilityState != null)
                _vulnerabilityState.Initialize(_enemyData?.enemyId);

            InitActionSet();
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
            _conflictCombatant = combatant;
            _ecosystemBalance = balance;
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
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EliteWardedResistedStatus. EnemyId={_enemyData?.enemyId}, Affix=Warded.", this);
            return true;
        }

        // Vampiric elites heal 25% of damage dealt (melee/area path, where the applied amount is known).
        private void ApplyVampiricLifesteal(int damageDealt)
        {
            int heal = EliteAffixRules.ResolveLifestealHeal(_eliteAffix, damageDealt);
            if (heal <= 0 || _health == null || _health.MaxHp <= 0)
            {
                return;
            }

            _health.RestoreHp(Mathf.Min(_health.MaxHp, _health.CurrentHp + heal));
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EliteVampiricHeal. EnemyId={_enemyData?.enemyId}, Heal={heal}, HP={_health.CurrentHp}/{_health.MaxHp}.", this);
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
            int playerMaxHp = GameBootstrap.Instance?.PlayerManager?.MaxHP ?? 0;
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
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EliteVolatileExploding. EnemyId={_enemyData?.enemyId}, Damage={damage}, CapMaxHp={playerMaxHp}, Telegraph={EliteAffixRules.VolatileExplosionTelegraphSeconds:F2}s.", this);
        }

        // ─── fable_24: boss primitives (fable_05 orchestrates full phases) ─────

        /// <summary>
        /// BossArenaControl primitive: lock this enemy's leash to an explicit arena centre + radius.
        /// fable_05 sets the arena bounds; the brain enforces "cannot leave the arena" via the same
        /// anchor-leash code used by ProtectAnchor. Idempotent; safe to call on phase entry.
        /// </summary>
        public void SetArenaLeash(Vector2 arenaCenter, float arenaRadiusTiles)
        {
            _anchorPoint = arenaCenter;
            _anchorLeashTiles = Mathf.Max(1f, arenaRadiusTiles);
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
                _activeActionSet = newActionSet;
                _actionCooldowns.Clear();
                if (_actionDatabase != null)
                {
                    foreach (var actionId in newActionSet.ActionIds)
                    {
                        if (_actionDatabase.TryGetById(actionId, out var action))
                            _actionCooldowns[actionId] = new EnemyActionRuntime(actionId, action.CooldownSeconds);
                    }
                }
            }

            if (newMovementProfile != null)
            {
                _movementProfile = newMovementProfile;
            }

            // Interrupt any pending action so the new phase starts clean (telegraph cleared).
            _pendingAction = null;
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
                Debug.LogWarning($"CombatLog: BossSwapActionSetMissing. EnemyId={_enemyData?.enemyId}, ActionSetId={actionSetId}.", this);
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
            _threatExpiredLogged = false;
            if (_currentState == EnemyBrainState.Idle || _currentState == EnemyBrainState.Patrol)
            {
                _currentState = EnemyBrainState.Alert;
            }
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

        private void LogThreatExpiredOnce()
        {
            if (_threatExpiredLogged)
            {
                return;
            }

            _threatExpiredLogged = true;
            CindarsHope.Combat.CombatLog.Log($"CombatLog: EnemyThreatExpired. EnemyId={_enemyData?.enemyId}, PackId={_packId ?? "none"}.", this);
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
            StopMovement();
            _spawnAnchor = anchor;

            if (_health != null && _health.MaxHp > 0)
            {
                _health.RestoreHp(_health.MaxHp);
            }

            CindarsHope.Combat.CombatLog.Log($"CombatLog: EnemyPackLeashReset. EnemyId={_enemyData?.enemyId}, PackId={_packId}, Anchor=({anchor.x:F2},{anchor.y:F2}).", this);
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
