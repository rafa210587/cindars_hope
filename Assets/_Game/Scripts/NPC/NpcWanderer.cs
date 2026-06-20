using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcWanderer : MonoBehaviour
    {
        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Vector2 _wanderBoundsMin = new Vector2(-6f, -3.5f);
        [SerializeField] private Vector2 _wanderBoundsMax = new Vector2(6f, 3.5f);

        private Vector2 _spawnPosition;
        private Vector3 _targetPosition;
        private float _pauseTimer = 0f;
        private bool _isPaused = true;
        private bool _isInteractionPaused;
        private Collider2D _collider;

        // fable_11: schedule destination override. When set, the NPC walks toward this point
        // regardless of its base movement mode (static shopkeepers still go home at night).
        private bool _hasScheduleDestination;
        private Vector3 _scheduleDestination;
        private float _scheduleArriveRadius = 0.3f;

        // Movement speed fallback for NPCs without WanderData (static shopkeepers being routed home).
        private const float DefaultScheduleMoveSpeed = 1.5f;

        private void Start()
        {
            _spawnPosition = transform.position;
            _targetPosition = _spawnPosition;
            _collider = GetComponent<Collider2D>();

            if (_npcData?.WanderData != null)
            {
                StartPause();
            }
        }

        private void FixedUpdate()
        {
            if (_isInteractionPaused)
            {
                StopMotion();
                return;
            }

            // fable_11: a schedule destination override takes priority over normal wandering and
            // applies to any NPC (including static ones being routed home/work by the schedule).
            if (_hasScheduleDestination)
            {
                MoveTowardScheduleDestination();
                return;
            }

            if (_npcData?.MovementMode != NpcMovementMode.RandomWander)
                return;

            if (_isPaused)
            {
                UpdatePause();
            }
            else
            {
                MoveTowardTarget();
            }
        }

        /// <summary>
        /// fable_11 — order the NPC to walk toward a schedule anchor. Overrides wandering until the
        /// NPC arrives within <paramref name="arriveRadius"/>. Works for static NPCs too. The schedule
        /// service applies a teleport fallback if the NPC is blocked en route for too long.
        /// </summary>
        public void SetDestination(Vector2 target, float arriveRadius)
        {
            _scheduleDestination = target;
            _scheduleArriveRadius = Mathf.Max(0.05f, arriveRadius);
            _hasScheduleDestination = true;
            _isPaused = false;
        }

        /// <summary>Clear any active schedule destination override and resume normal behavior.</summary>
        public void ClearDestination()
        {
            _hasScheduleDestination = false;
            StopMotion();
            if (_npcData?.WanderData != null)
            {
                StartPause();
            }
        }

        private void MoveTowardScheduleDestination()
        {
            float distance = Vector2.Distance(transform.position, _scheduleDestination);
            if (distance <= _scheduleArriveRadius)
            {
                _hasScheduleDestination = false;
                StopMotion();
                if (_npcData?.WanderData != null)
                {
                    // Re-anchor wander to the arrival point so the NPC mills around its current anchor.
                    _spawnPosition = transform.position;
                    StartPause();
                }
                return;
            }

            var direction = (Vector2)((_scheduleDestination - transform.position).normalized);
            var moveSpeed = _npcData?.WanderData != null ? _npcData.WanderData.WanderSpeed : DefaultScheduleMoveSpeed;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction * moveSpeed;
            }
            else
            {
                transform.position += (Vector3)direction * moveSpeed * Time.fixedDeltaTime;
            }
        }

        private void UpdatePause()
        {
            _pauseTimer -= Time.fixedDeltaTime;
            if (_pauseTimer <= 0f)
            {
                ChooseNewTarget();
            }
        }

        private void MoveTowardTarget()
        {
            float distance = Vector2.Distance(transform.position, _targetPosition);
            if (distance < 0.1f)
            {
                StartPause();
                return;
            }

            var direction = (Vector2)((_targetPosition - transform.position).normalized);
            var moveSpeed = _npcData.WanderData.WanderSpeed;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction * moveSpeed;
            }
            else
            {
                transform.position += (Vector3)direction * moveSpeed * Time.fixedDeltaTime;
            }
        }

        private void ChooseNewTarget()
        {
            _isPaused = false;
            var wanderData = _npcData.WanderData;
            var randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var randomDistance = Random.Range(0f, wanderData.WanderRadius);
            var offset = new Vector2(
                Mathf.Cos(randomAngle) * randomDistance,
                Mathf.Sin(randomAngle) * randomDistance
            );

            var intendedTarget = _spawnPosition + offset;
            _targetPosition = new Vector2(
                Mathf.Clamp(intendedTarget.x, _wanderBoundsMin.x, _wanderBoundsMax.x),
                Mathf.Clamp(intendedTarget.y, _wanderBoundsMin.y, _wanderBoundsMax.y));
        }

        private void StartPause()
        {
            _isPaused = true;
            var wanderData = _npcData.WanderData;
            _pauseTimer = Random.Range(wanderData.PauseMinDuration, wanderData.PauseMaxDuration);

            StopMotion();
        }

        public void SetInteractionPaused(bool isPaused)
        {
            _isInteractionPaused = isPaused;
            if (isPaused)
            {
                StopMotion();
            }
            else if (_npcData?.WanderData != null)
            {
                StartPause();
            }
        }

        private void StopMotion()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}
