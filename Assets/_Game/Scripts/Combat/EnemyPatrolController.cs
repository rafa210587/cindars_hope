using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyPatrolController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _patrolSpeed = 1f;
        [SerializeField] private float _patrolDistance = 5f;
        [SerializeField] private float _pauseDuration = 2f;
        [SerializeField] private EnemyChaseController _chaseController;

        private Vector2 _patrolDirection = Vector2.right;
        private Vector2 _startPosition;
        private float _pauseTimer;
        private bool _isPaused;

        private void Start()
        {
            _startPosition = transform.position;
            _pauseTimer = _pauseDuration;
            _isPaused = true;
        }

        private void FixedUpdate()
        {
            if (_chaseController != null && IsBeingChased())
                return;

            if (_isPaused)
            {
                UpdatePause();
            }
            else
            {
                Patrol();
            }
        }

        private void UpdatePause()
        {
            _pauseTimer -= Time.fixedDeltaTime;
            if (_pauseTimer <= 0f)
            {
                _isPaused = false;
                ChangeDirection();
                _pauseTimer = _pauseDuration;
            }
        }

        private void Patrol()
        {
            Vector2 currentPos = transform.position;
            float distanceFromStart = Vector2.Distance(currentPos, _startPosition);

            if (distanceFromStart >= _patrolDistance)
            {
                _isPaused = true;
                _pauseTimer = _pauseDuration;
                return;
            }

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(_rigidbody.position + _patrolDirection * _patrolSpeed * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += (Vector3)_patrolDirection * _patrolSpeed * Time.fixedDeltaTime;
            }
        }

        private void ChangeDirection()
        {
            _patrolDirection = -_patrolDirection;
        }

        private bool IsBeingChased()
        {
            if (_chaseController == null)
                return false;

            return _chaseController.IsActiveAndEnabled;
        }

        public void SetPatrolDistance(float distance)
        {
            _patrolDistance = Mathf.Max(0.5f, distance);
        }

        public void SetPatrolSpeed(float speed)
        {
            _patrolSpeed = Mathf.Max(0.1f, speed);
        }
    }
}
