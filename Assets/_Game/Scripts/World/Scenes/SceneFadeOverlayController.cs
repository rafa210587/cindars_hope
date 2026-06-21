using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.World.Scenes
{
    public sealed class SceneFadeOverlayController : MonoBehaviour
    {
        [SerializeField] private float _fadeOutDuration = 0.25f;
        [SerializeField] private float _fadeInDuration  = 0.35f;

        private const float TimeoutSeconds = 5f;

        private CanvasGroup _overlay;
        private GraphicRaycaster _raycaster;
        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _overlay   = GetComponent<CanvasGroup>();
            _raycaster = GetComponent<GraphicRaycaster>();
            if (_overlay != null) _overlay.alpha = 0f;
            SetRaycasterEnabled(false);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnTransitionStarted);
            GameEventBus.Subscribe<SceneTransitionCompletedEvent>(OnTransitionCompleted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnTransitionStarted);
            GameEventBus.Unsubscribe<SceneTransitionCompletedEvent>(OnTransitionCompleted);
        }

        private void OnTransitionStarted(SceneTransitionStartedEvent _)
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeToBlackRoutine());
        }

        private void OnTransitionCompleted(SceneTransitionCompletedEvent _)
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeFromBlackRoutine());
        }

        private IEnumerator FadeToBlackRoutine()
        {
            yield return FadeRoutine(1f, _fadeOutDuration);
            _activeCoroutine = null;

            float elapsed = 0f;
            while (elapsed < TimeoutSeconds)
            {
                if (_overlay.alpha < 0.99f) yield break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.LogWarning("[SceneFadeOverlay] Timeout — SceneTransitionCompletedEvent nao recebido; forcando fade-out.");
            _activeCoroutine = StartCoroutine(FadeFromBlackRoutine());
        }

        private IEnumerator FadeFromBlackRoutine()
        {
            yield return FadeRoutine(0f, _fadeInDuration);
            _activeCoroutine = null;
        }

        private IEnumerator FadeRoutine(float target, float duration)
        {
            if (_overlay == null) yield break;
            float start   = _overlay.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed     += Time.unscaledDeltaTime;
                _overlay.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                SetRaycasterEnabled(_overlay.alpha > 0.01f);
                yield return null;
            }
            _overlay.alpha = target;
            SetRaycasterEnabled(target > 0.01f);
        }

        private void SetRaycasterEnabled(bool enabled)
        {
            if (_raycaster != null) _raycaster.enabled = enabled;
        }
    }
}
