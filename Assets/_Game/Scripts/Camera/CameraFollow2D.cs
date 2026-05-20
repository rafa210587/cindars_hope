using UnityEngine;

namespace CindarsHope.Camera
{
    [DisallowMultipleComponent]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.08f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private bool _snapOnStart = true;

        private Vector3 _velocity = Vector3.zero;

        private void Start()
        {
            if (_snapOnStart && _target != null)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var targetPosition = _target.position + _offset;
            var currentPosition = transform.position;
            var newPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref _velocity, _smoothTime);
            newPosition.z = _offset.z;
            transform.position = newPosition;
        }

        public void RebindTarget(Transform target)
        {
            _target = target;
        }

        public void SnapToTarget()
        {
            if (_target == null)
            {
                return;
            }

            var targetPosition = _target.position + _offset;
            targetPosition.z = _offset.z;
            transform.position = targetPosition;
            _velocity = Vector3.zero;
        }
    }
}
