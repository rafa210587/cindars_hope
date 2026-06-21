using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        // A CaveScene posiciona o player pelo caminho proprio (CaveLevelRuntimeController) e NAO
        // publica SceneTransitionCompletedEvent — so Farm/Town (PlayerSpawnResolver/SceneSpawnInstaller)
        // publicam. Sem este sinal o fade esperava o timeout de 5s ao entrar na caverna. Marcamos que
        // a cena destino carregou para fechar o fade sem o warning.
        private bool _destinationLoaded;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnTransitionStarted);
            GameEventBus.Unsubscribe<SceneTransitionCompletedEvent>(OnTransitionCompleted);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnTransitionStarted(SceneTransitionStartedEvent _)
        {
            _destinationLoaded = false;
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeToBlackRoutine());
        }

        // Fallback para cenas que nao publicam SceneTransitionCompletedEvent (caverna): assim que a
        // cena destino carrega, o fade-to-black em espera faz o fade-in sem aguardar o timeout.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                _destinationLoaded = true;
            }
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
                if (_overlay.alpha < 0.99f) yield break; // SceneTransitionCompletedEvent ja iniciou o fade-in.
                if (_destinationLoaded)
                {
                    // Cena destino carregou (ex.: caverna, que nao publica completed) -> fade-in agora.
                    _activeCoroutine = StartCoroutine(FadeFromBlackRoutine());
                    yield break;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.LogWarning("[SceneFadeOverlay] Timeout — cena destino nao carregou em 5s; forcando fade-out.");
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
