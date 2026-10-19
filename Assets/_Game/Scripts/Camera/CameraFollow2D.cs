using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
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

        // Alinha a posicao final da camera ao grid de pixel do mundo (1/PixelsPerUnit).
        // Sem isto, o SmoothDamp para a camera em coordenadas fracionarias a cada frame e os
        // tiles Point-filtered (grama/calcamento a 128 PPU) tremem/piscam nas bordas ao mover.
        [SerializeField] private bool _pixelSnap = true;
        [SerializeField] private float _pixelsPerUnit = 128f;

        private Vector3 _velocity = Vector3.zero;
        private UnityEngine.Camera _camera;
        private string _townInteriorOperationId;
        private bool _hasTownInteriorBounds;
        private Rect _townInteriorBounds;

        private Vector3 SnapToPixelGrid(Vector3 position)
        {
            if (!_pixelSnap || _pixelsPerUnit <= 0f)
            {
                return position;
            }

            position.x = Mathf.Round(position.x * _pixelsPerUnit) / _pixelsPerUnit;
            position.y = Mathf.Round(position.y * _pixelsPerUnit) / _pixelsPerUnit;
            return position;
        }

        private void Start()
        {
            if (_snapOnStart && _target != null)
            {
                SnapToTarget();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CameraSnapRequestedEvent>(HandleSnapRequested);
            GameEventBus.Subscribe<TownInteriorPlayerLockEvent>(HandleTownInteriorPlayerLock);
            GameEventBus.Subscribe<TownInteriorCameraBoundsChangedEvent>(HandleTownInteriorCameraBoundsChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CameraSnapRequestedEvent>(HandleSnapRequested);
            GameEventBus.Unsubscribe<TownInteriorPlayerLockEvent>(HandleTownInteriorPlayerLock);
            GameEventBus.Unsubscribe<TownInteriorCameraBoundsChangedEvent>(HandleTownInteriorCameraBoundsChanged);
        }

        // Corta direto para o jogador no mesmo frame em que ele foi teleportado por uma porta,
        // evitando o pan suave pelo mapa inteiro até o interior (y > +40).
        private void HandleSnapRequested(CameraSnapRequestedEvent _)
        {
            SnapToTarget();
        }

        private void HandleTownInteriorPlayerLock(TownInteriorPlayerLockEvent evt)
        {
            if (evt.Locked && !string.IsNullOrWhiteSpace(evt.OperationId)) _townInteriorOperationId = evt.OperationId;
        }

        private void HandleTownInteriorCameraBoundsChanged(TownInteriorCameraBoundsChangedEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.OperationId) || evt.OperationId != _townInteriorOperationId) return;
            if (!evt.HasBounds)
            {
                _hasTownInteriorBounds = false;
                SnapToTarget();
                return;
            }
            if (!Finite(evt.MinX) || !Finite(evt.MinY) || !Finite(evt.MaxX) || !Finite(evt.MaxY) ||
                evt.MinX > evt.MaxX || evt.MinY > evt.MaxY)
                return;
            _townInteriorBounds = Rect.MinMaxRect(evt.MinX, evt.MinY, evt.MaxX, evt.MaxY);
            _hasTownInteriorBounds = true;
            SnapToTarget();
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
            newPosition = SnapToPixelGrid(newPosition);
            newPosition = ClampToTownInteriorBounds(newPosition);
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
            targetPosition = SnapToPixelGrid(targetPosition);
            targetPosition = ClampToTownInteriorBounds(targetPosition);
            targetPosition.z = _offset.z;
            transform.position = targetPosition;
            _velocity = Vector3.zero;
        }

        private Vector3 ClampToTownInteriorBounds(Vector3 position)
        {
            if (!_hasTownInteriorBounds) return position;
            var camera = _camera != null ? _camera : _camera = GetComponent<UnityEngine.Camera>();
            var halfHeight = camera != null && camera.orthographic ? Mathf.Max(0f, camera.orthographicSize) : 0f;
            var halfWidth = camera != null ? halfHeight * Mathf.Max(0f, camera.aspect) : 0f;
            position.x = ClampAxis(position.x, _townInteriorBounds.xMin, _townInteriorBounds.xMax, halfWidth);
            position.y = ClampAxis(position.y, _townInteriorBounds.yMin, _townInteriorBounds.yMax, halfHeight);
            return position;
        }

        private static float ClampAxis(float value, float min, float max, float halfExtent)
        {
            if (max - min <= halfExtent * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + halfExtent, max - halfExtent);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
