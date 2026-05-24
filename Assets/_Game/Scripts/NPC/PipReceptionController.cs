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

        private void Update()
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

            transform.position = Vector3.MoveTowards(
                transform.position,
                _playerTransform.position,
                Mathf.Max(0f, _walkSpeed) * Time.deltaTime);
        }
    }
}
