using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class KnockbackController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _duration = 0.15f;

        private float _remainingTime;
        private Vector2 _velocity;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        private void FixedUpdate()
        {
            if (_remainingTime <= 0)
            {
                return;
            }

            _remainingTime -= Time.fixedDeltaTime;

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += (Vector3)_velocity * Time.fixedDeltaTime;
            }
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            direction.Normalize();
            _velocity = direction * force;
            _remainingTime = _duration;
            Debug.Log($"KnockbackController: applying knockback on '{name}', force={force}.");
        }

        public bool IsKnockingBack => _remainingTime > 0;
    }
}
