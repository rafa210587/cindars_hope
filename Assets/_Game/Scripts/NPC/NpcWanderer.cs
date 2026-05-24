using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcWanderer : MonoBehaviour
    {
        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private Rigidbody2D _rigidbody;

        private Vector2 _spawnPosition;
        private Vector3 _targetPosition;
        private float _pauseTimer = 0f;
        private bool _isPaused = true;
        private Collider2D _collider;

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

            _targetPosition = _spawnPosition + offset;
        }

        private void StartPause()
        {
            _isPaused = true;
            var wanderData = _npcData.WanderData;
            _pauseTimer = Random.Range(wanderData.PauseMinDuration, wanderData.PauseMaxDuration);

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}
