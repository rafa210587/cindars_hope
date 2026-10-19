using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public sealed class PipReceptionController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _stopDistance = 1.5f;

        private bool _hasWelcomedPlayer;
        private NpcWanderer _scheduleMover;
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _scheduleMover = GetComponent<NpcWanderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_hasWelcomedPlayer || _playerTransform == null)
            {
                return;
            }

            var offset = _playerTransform.position - transform.position;
            if (offset.sqrMagnitude <= _stopDistance * _stopDistance)
            {
                _hasWelcomedPlayer = true;
                return;
            }

            // The Town schedule owns locomotion whenever Pip has an NpcWanderer. The legacy
            // reception chase previously wrote transform.position concurrently, pulled Pip from
            // his valid work pocket through the house facade, and invalidated every later route.
            if (_scheduleMover != null) return;

            var next = Vector2.MoveTowards(transform.position, _playerTransform.position,
                Mathf.Max(0f, _walkSpeed) * Time.fixedDeltaTime);
            if (_rigidbody != null) _rigidbody.MovePosition(next);
        }
    }
}
