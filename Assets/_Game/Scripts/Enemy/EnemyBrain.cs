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

        // F02: stagger por quebra de postura — entra em Stunned e sai sozinho.
        private float _stunUntil;

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

            _playerTarget = GameBootstrap.Instance?.PlayerManager?.gameObject;

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
            _actionTimer = action.WindupSeconds;
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

                    _actionTimer = _pendingAction?.RecoverSeconds ?? 0.5f;
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

            // Ranged and cast actions fire a real dodgeable projectile instead of
            // instant damage — the player can outplay them with movement.
            bool isProjectileAction = _pendingAction.ActionType == EnemyActionType.RangedProjectile
                || _pendingAction.ActionType == EnemyActionType.CastProjectile;
            if (isProjectileAction)
            {
                float speed = _pendingAction.ProjectileSpeed > 0f ? _pendingAction.ProjectileSpeed : 5f;
                EnemyProjectileBehaviour.SpawnTowards(
                    transform.position,
                    DirectionToPlayer(),
                    speed,
                    Mathf.Max(_pendingAction.Range, 2f),
                    _pendingAction.BaseDamage,
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
                Debug.Log($"CombatLog: EnemyActionMissed. EnemyId={_enemyData?.enemyId}, ActionId={_pendingAction.ActionId}, Distance={dist:F2}, Range={effectiveRange:F2}");
                return;
            }

            var request = new DamageRequest(
                targetId: "player",
                baseDamage: _pendingAction.BaseDamage,
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
                    playerManager, result.FinalDamage, _enemyData?.enemyId ?? "enemy", dmgType, gameObject);
                if (applied > 0)
                {
                    ApplyActionStatusesToPlayer(_pendingAction);
                }
            }
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
        private float MoveSpeed() => (_movementProfile?.MoveSpeed ?? _enemyData?.moveSpeed ?? 2f) * ExternalSpeedFactor();

        // ─── Public API ───────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (_health != null)
            {
                _health.TakeDamage(amount);
                if (_health.IsDead)
                    _currentState = EnemyBrainState.Dead;
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
            Debug.Log($"CombatLog: EnemyThreatExpired. EnemyId={_enemyData?.enemyId}, PackId={_packId ?? "none"}.", this);
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

            Debug.Log($"CombatLog: EnemyPackLeashReset. EnemyId={_enemyData?.enemyId}, PackId={_packId}, Anchor=({anchor.x:F2},{anchor.y:F2}).", this);
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
