using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyChaseController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _moveSpeed = 1.2f;
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private float _stopDistance = 0.55f;

        private void FixedUpdate()
        {
            if (_target == null)
            {
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, _target.position);

            if (distanceToTarget > _detectionRadius || distanceToTarget <= _stopDistance)
            {
                return;
            }

            Vector2 direction = (_target.position - transform.position).normalized;

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(_rigidbody.position + direction * _moveSpeed * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += (Vector3)direction * _moveSpeed * Time.deltaTime;
            }
        }

        public void RebindTarget(Transform target)
        {
            _target = target;
        }

        public void Configure(float moveSpeed, float detectionRadius, float stopDistance)
        {
            _moveSpeed = moveSpeed;
            _detectionRadius = detectionRadius;
            _stopDistance = stopDistance;
        }
    }
}
