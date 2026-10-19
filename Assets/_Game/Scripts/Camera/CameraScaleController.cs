using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Camera
{
    /// <summary>
    /// Reads CameraScaleConfigSO and adjusts Camera.orthographicSize per scene context.
    /// Attach alongside CameraFollow2D on the main camera.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public sealed class CameraScaleController : MonoBehaviour
    {
        [SerializeField] private CameraScaleConfigSO _config;

        private UnityEngine.Camera _camera;
        private float _targetSize;
        private float _currentVelocity;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        private void Start()
        {
            if (_config == null)
            {
                return;
            }

            _targetSize = ResolveTargetSize(SceneManager.GetActiveScene().name);
            _camera.orthographicSize = _targetSize;
        }

        private void Update()
        {
            if (_config == null || _camera == null)
            {
                return;
            }

            if (Mathf.Approximately(_camera.orthographicSize, _targetSize))
            {
                return;
            }

            _camera.orthographicSize = Mathf.SmoothDamp(
                _camera.orthographicSize,
                _targetSize,
                ref _currentVelocity,
                _config.SizeTransitionTime
            );
        }

        public void SetContext(CameraContext context)
        {
            if (_config == null)
            {
                return;
            }

            _targetSize = context switch
            {
                CameraContext.Farm => _config.FarmOrthographicSize,
                CameraContext.Town => _config.TownOrthographicSize,
                CameraContext.Cave => _config.CaveOrthographicSize,
                CameraContext.BossArena => _config.BossArenaOrthographicSize,
                _ => _config.DefaultOrthographicSize,
            };

            _targetSize = Mathf.Clamp(_targetSize, _config.MinOrthographicSize, _config.MaxOrthographicSize);
        }

        private float ResolveTargetSize(string sceneName)
        {
            if (sceneName.Contains("Farm"))
            {
                return _config.FarmOrthographicSize;
            }

            if (sceneName.Contains("Town"))
            {
                return _config.TownOrthographicSize;
            }

            if (sceneName.Contains("Cave"))
            {
                return _config.CaveOrthographicSize;
            }

            return _config.DefaultOrthographicSize;
        }
    }

    public enum CameraContext
    {
        Default,
        Farm,
        Town,
        Cave,
        BossArena,
    }
}
