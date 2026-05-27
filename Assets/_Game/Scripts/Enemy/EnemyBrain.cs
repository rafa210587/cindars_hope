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
        [SerializeField] private EnemyDataSO _enemyData;
        [SerializeField] private float _decisionTickSeconds = 0.3f;

        private EnemyBrainState _currentState = EnemyBrainState.Idle;
        private float _decisionTimer;
        private GameObject _playerTarget;
        private EnemyMovementProfileSO _movementProfile;
        private float _lastActionTime;

        public EnemyBrainState CurrentState => _currentState;

        private void OnEnable()
        {
            _decisionTimer = 0f;
            _lastActionTime = 0f;
            _currentState = EnemyBrainState.Idle;
            _playerTarget = GameBootstrap.Instance?.PlayerManager?.gameObject;

            if (_enemyData != null && !string.IsNullOrWhiteSpace(_enemyData.MovementProfileId))
            {
                _decisionTickSeconds = 0.3f;
            }
        }

        private void Update()
        {
            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = _decisionTickSeconds;
                EvaluateState();
            }

            ExecuteCurrentState();
        }

        private void EvaluateState()
        {
            if (_currentState == EnemyBrainState.Dead)
                return;

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTarget?.transform.position ?? Vector3.zero);
            bool playerInDetectionRange = _playerTarget != null && distanceToPlayer <= (_movementProfile?.DetectionRange ?? 10f);

            switch (_currentState)
            {
                case EnemyBrainState.Idle:
                    if (playerInDetectionRange)
                        _currentState = EnemyBrainState.Alert;
                    else
                        _currentState = EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Patrol:
                    if (playerInDetectionRange)
                        _currentState = EnemyBrainState.Chase;
                    break;

                case EnemyBrainState.Alert:
                    if (playerInDetectionRange)
                        _currentState = EnemyBrainState.Chase;
                    else
                        _currentState = EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Chase:
                    if (!playerInDetectionRange)
                        _currentState = EnemyBrainState.Alert;
                    else if (distanceToPlayer <= (_movementProfile?.AttackRange ?? 1.5f))
                        _currentState = EnemyBrainState.AttackWindup;
                    break;

                case EnemyBrainState.AttackWindup:
                    if (Time.time >= _lastActionTime + 0.5f)
                        _currentState = EnemyBrainState.AttackRecover;
                    break;

                case EnemyBrainState.AttackRecover:
                    if (Time.time >= _lastActionTime + 1f)
                    {
                        if (playerInDetectionRange)
                            _currentState = EnemyBrainState.Chase;
                        else
                            _currentState = EnemyBrainState.Patrol;
                    }
                    break;

                case EnemyBrainState.Stunned:
                    break;
            }
        }

        private void ExecuteCurrentState()
        {
            switch (_currentState)
            {
                case EnemyBrainState.Idle:
                    break;

                case EnemyBrainState.Patrol:
                    MovePatrol();
                    break;

                case EnemyBrainState.Alert:
                    break;

                case EnemyBrainState.Chase:
                    MoveTowardPlayer();
                    break;

                case EnemyBrainState.AttackWindup:
                    PrepareAttack();
                    break;

                case EnemyBrainState.AttackRecover:
                    break;
            }
        }

        private void MovePatrol()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 moveDir = Random.insideUnitCircle.normalized;
                rb.linearVelocity = moveDir * (_movementProfile?.MoveSpeed ?? 1f);
            }
        }

        private void MoveTowardPlayer()
        {
            if (_playerTarget == null)
                return;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                return;

            Vector2 direction = (_playerTarget.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * (_movementProfile?.MoveSpeed ?? 2f);
        }

        private void PrepareAttack()
        {
            if (_currentState == EnemyBrainState.AttackWindup && _lastActionTime == 0)
            {
                _lastActionTime = Time.time;
                PublishTelegraphEvent();
            }
        }

        private void PublishTelegraphEvent()
        {
            if (_enemyData != null)
            {
                GameEventBus.Publish(new EnemyTelegraphStartedEvent(_enemyData.enemyId, transform.position));
            }
        }

        public void TakeDamage(int amount)
        {
            var health = GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(amount);
                if (health.IsDead)
                {
                    _currentState = EnemyBrainState.Dead;
                    GameEventBus.Publish(new EnemyKilledEvent(_enemyData?.enemyId, transform.position));
                }
            }
        }

        public void SetState(EnemyBrainState newState)
        {
            _currentState = newState;
        }
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
