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

        [Header("Tuning")]
        [SerializeField] private float _decisionTickSeconds = 0.3f;

        // State machine
        private EnemyBrainState _currentState = EnemyBrainState.Idle;
        private float _decisionTimer;
        private float _patrolDirectionTimer;

        // Target
        private GameObject _playerTarget;

        // Action runtime
        private EnemyActionSetSO _activeActionSet;
        private readonly Dictionary<string, EnemyActionRuntime> _actionCooldowns = new Dictionary<string, EnemyActionRuntime>();
        private EnemyActionSO _pendingAction;
        private float _actionTimer;
        private bool _actionResolved;

        // Components
        private Rigidbody2D _rb;
        private EnemyTelegraphController _telegraph;
        private EnemyHealth _health;
        private EnemyVulnerabilityState _vulnerabilityState;

        public EnemyBrainState CurrentState => _currentState;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _telegraph = GetComponent<EnemyTelegraphController>();
            _health = GetComponent<EnemyHealth>();
            _vulnerabilityState = GetComponent<EnemyVulnerabilityState>();
        }

        private void OnEnable()
        {
            _decisionTimer = 0f;
            _patrolDirectionTimer = 0f;
            _actionResolved = false;
            _pendingAction = null;
            _currentState = EnemyBrainState.Idle;

            _playerTarget = GameBootstrap.Instance?.PlayerManager?.gameObject;

            if (_vulnerabilityState != null)
                _vulnerabilityState.Initialize(_enemyData?.enemyId);

            InitActionSet();

            if (_movementProfile != null && _movementProfile.DecisionTickSeconds > 0f)
                _decisionTickSeconds = _movementProfile.DecisionTickSeconds;
        }

        private void OnDisable()
        {
            StopMovement();
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

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = _decisionTickSeconds;
                EvaluateState();
            }

            TickActionTimers();
            ExecuteMovement();
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

            switch (_currentState)
            {
                case EnemyBrainState.Idle:
                    _currentState = inDetect ? EnemyBrainState.Alert : EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Patrol:
                    if (inDetect) _currentState = EnemyBrainState.Chase;
                    break;

                case EnemyBrainState.Alert:
                    _currentState = inDetect ? EnemyBrainState.Chase : EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Chase:
                case EnemyBrainState.Kite:
                case EnemyBrainState.GuardHold:
                case EnemyBrainState.CastPrepare:
                    if (!inLeash)
                    {
                        _currentState = EnemyBrainState.Patrol;
                        break;
                    }
                    TryBeginAction(dist);
                    break;
            }
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
                GameEventBus.Publish(new PlayerHitEvent(result.FinalDamage));
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
                _vulnerabilityState.OpenWindow(1.5f, 1.5f, 10f);
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
                case EnemyBrainState.AttackWindup:
                case EnemyBrainState.AttackRecover:
                case EnemyBrainState.GuardHold:
                    StopMovement();
                    break;
            }
        }

        private void MoveChase()
        {
            if (_playerTarget == null) return;

            var moveType = _movementProfile?.MovementType ?? EnemyMovementType.GroundChase;
            float speed = MoveSpeed();

            switch (moveType)
            {
                case EnemyMovementType.SwarmErratic:
                    MoveSwarmErratic(speed);
                    return;
                case EnemyMovementType.TankSlowPush:
                    speed = Mathf.Min(speed, 1.5f);
                    break;
            }

            _rb.linearVelocity = DirectionToPlayer() * speed;
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
            _rb.linearVelocity = Random.insideUnitCircle.normalized * (MoveSpeed() * 0.4f);
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
        private float MoveSpeed() => _movementProfile?.MoveSpeed ?? _enemyData?.moveSpeed ?? 2f;

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
