using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CindarsHope.World.Scenes
{
    public sealed class SceneFadeOverlayController : MonoBehaviour
    {
        [SerializeField] private float _fadeOutDuration = 0.25f;
        [SerializeField] private float _fadeInDuration  = 0.35f;
        [SerializeField] private Text _townInteriorLoadingHint;
        [SerializeField] private Text _townInteriorRecoveryMessage;
        [SerializeField] private Button _townInteriorRecoveryRetry;

        private const float TimeoutSeconds = 5f;

        private CanvasGroup _overlay;
        private GraphicRaycaster _raycaster;
        private Coroutine _activeCoroutine;
        private Coroutine _loadingHintCoroutine;
        private string _townInteriorOperationId;
        private float _townInteriorFadeInSeconds;
        private bool _townInteriorCancelled;
        private bool _townInteriorRecoveryVisible;
        private GameObject _previousSelectedObject;

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
            SetTownInteriorLoadingHintVisible(false);
            HideRecoveryUi();
            SetRaycasterEnabled(false);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnTransitionStarted);
            GameEventBus.Subscribe<SceneTransitionCompletedEvent>(OnTransitionCompleted);
            GameEventBus.Subscribe<TownInteriorBlackoutRequestedEvent>(OnTownInteriorBlackoutRequested);
            GameEventBus.Subscribe<TownInteriorVisitCancelledEvent>(OnTownInteriorVisitCancelled);
            GameEventBus.Subscribe<TownInteriorPlacementReadyEvent>(OnTownInteriorPlacementReady);
            GameEventBus.Subscribe<TownInteriorRecoveryRequiredEvent>(OnTownInteriorRecoveryRequired);
            if (_townInteriorRecoveryRetry != null)
                _townInteriorRecoveryRetry.onClick.AddListener(OnRecoveryRetryClicked);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnTransitionStarted);
            GameEventBus.Unsubscribe<SceneTransitionCompletedEvent>(OnTransitionCompleted);
            GameEventBus.Unsubscribe<TownInteriorBlackoutRequestedEvent>(OnTownInteriorBlackoutRequested);
            GameEventBus.Unsubscribe<TownInteriorVisitCancelledEvent>(OnTownInteriorVisitCancelled);
            GameEventBus.Unsubscribe<TownInteriorPlacementReadyEvent>(OnTownInteriorPlacementReady);
            GameEventBus.Unsubscribe<TownInteriorRecoveryRequiredEvent>(OnTownInteriorRecoveryRequired);
            if (_townInteriorRecoveryRetry != null)
                _townInteriorRecoveryRetry.onClick.RemoveListener(OnRecoveryRetryClicked);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ClearTownInteriorMode();
        }

        private void OnTransitionStarted(SceneTransitionStartedEvent _)
        {
            ClearTownInteriorMode();
            _destinationLoaded = false;
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeToBlackRoutine());
        }

        // Fallback para cenas que nao publicam SceneTransitionCompletedEvent (caverna): assim que a
        // cena destino carrega, o fade-to-black em espera faz o fade-in sem aguardar o timeout.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && string.IsNullOrEmpty(_townInteriorOperationId))
            {
                _destinationLoaded = true;
            }
        }

        private void OnTransitionCompleted(SceneTransitionCompletedEvent _)
        {
            if (!string.IsNullOrEmpty(_townInteriorOperationId)) return;
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeFromBlackRoutine());
        }

        public void ConfigureLoadingHint(Text hint)
        {
            _townInteriorLoadingHint = hint;
            SetTownInteriorLoadingHintVisible(false);
        }

        public void ConfigureRecoveryUi(Text message, Button retryButton)
        {
            if (_townInteriorRecoveryRetry != null && isActiveAndEnabled)
                _townInteriorRecoveryRetry.onClick.RemoveListener(OnRecoveryRetryClicked);
            _townInteriorRecoveryMessage = message;
            _townInteriorRecoveryRetry = retryButton;
            HideRecoveryUi();
            if (_townInteriorRecoveryRetry != null && isActiveAndEnabled)
                _townInteriorRecoveryRetry.onClick.AddListener(OnRecoveryRetryClicked);
        }

        private void OnTownInteriorBlackoutRequested(TownInteriorBlackoutRequestedEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.OperationId) || !ValidDuration(evt.FadeOutSeconds) ||
                !ValidDuration(evt.FadeInSeconds) || !ValidDuration(evt.LoadingHintAfterSeconds))
            {
                Debug.LogWarning("[SceneFadeOverlay] Ignoring invalid town interior blackout request.");
                return;
            }
            if (!string.IsNullOrEmpty(_townInteriorOperationId)) return;

            StopActiveCoroutine();
            _destinationLoaded = false;
            _townInteriorOperationId = evt.OperationId;
            _townInteriorFadeInSeconds = evt.FadeInSeconds;
            _townInteriorCancelled = false;
            HideRecoveryUi();
            SetTownInteriorLoadingHintVisible(false);
            _activeCoroutine = StartCoroutine(FadeTownInteriorToBlackRoutine(evt.OperationId, evt.FadeOutSeconds));
            if (_townInteriorLoadingHint != null && evt.LoadingHintAfterSeconds > 0f)
                _loadingHintCoroutine = StartCoroutine(ShowTownInteriorLoadingHintRoutine(evt.OperationId, evt.LoadingHintAfterSeconds));
        }

        private void OnTownInteriorVisitCancelled(TownInteriorVisitCancelledEvent evt)
        {
            if (!IsActiveTownInteriorOperation(evt.OperationId)) return;
            _townInteriorCancelled = true;
            HideRecoveryUi();
            StopLoadingHintCoroutine();
            SetTownInteriorLoadingHintVisible(false);
            StopActiveCoroutine();
            SetBlackImmediately();
        }

        private void OnTownInteriorPlacementReady(TownInteriorPlacementReadyEvent evt)
        {
            if (!IsActiveTownInteriorOperation(evt.OperationId)) return;
            if (_overlay == null || _overlay.alpha < 0.99f) return;
            if (_townInteriorCancelled && !string.IsNullOrEmpty(evt.HouseId)) return;
            HideRecoveryUi();
            StopActiveCoroutine();
            StopLoadingHintCoroutine();
            SetTownInteriorLoadingHintVisible(false);
            _activeCoroutine = StartCoroutine(FadeTownInteriorFromBlackRoutine(evt.OperationId));
        }

        private void OnTownInteriorRecoveryRequired(TownInteriorRecoveryRequiredEvent evt)
        {
            if (!IsActiveTownInteriorOperation(evt.OperationId)) return;
            _townInteriorCancelled = true;
            StopActiveCoroutine();
            StopLoadingHintCoroutine();
            SetTownInteriorLoadingHintVisible(false);
            SetBlackImmediately();
            if (_townInteriorRecoveryMessage == null || _townInteriorRecoveryRetry == null)
            {
                Debug.LogError("[SceneFadeOverlay] Recovery UI is not wired; retaining blackout and Player lock.", this);
                return;
            }
            var eventSystem = EventSystem.current;
            if (!_townInteriorRecoveryVisible)
                _previousSelectedObject = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
            _townInteriorRecoveryVisible = true;
            _townInteriorRecoveryMessage.enabled = true;
            _townInteriorRecoveryRetry.gameObject.SetActive(true);
            _townInteriorRecoveryRetry.interactable = true;
            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(_townInteriorRecoveryRetry.gameObject);
        }

        private void OnRecoveryRetryClicked()
        {
            if (!_townInteriorRecoveryVisible || _townInteriorRecoveryRetry == null ||
                !_townInteriorRecoveryRetry.interactable || string.IsNullOrEmpty(_townInteriorOperationId)) return;
            _townInteriorRecoveryRetry.interactable = false;
            GameEventBus.Publish(new TownInteriorRecoveryRetryRequestedEvent(_townInteriorOperationId));
        }

        private void HideRecoveryUi()
        {
            var eventSystem = EventSystem.current;
            bool restoreFocus = _townInteriorRecoveryVisible && eventSystem != null &&
                _townInteriorRecoveryRetry != null &&
                eventSystem.currentSelectedGameObject == _townInteriorRecoveryRetry.gameObject;
            if (_townInteriorRecoveryMessage != null) _townInteriorRecoveryMessage.enabled = false;
            if (_townInteriorRecoveryRetry != null)
            {
                _townInteriorRecoveryRetry.interactable = false;
                _townInteriorRecoveryRetry.gameObject.SetActive(false);
            }
            if (restoreFocus)
            {
                eventSystem.SetSelectedGameObject(_previousSelectedObject != null &&
                    _previousSelectedObject.activeInHierarchy ? _previousSelectedObject : null);
            }
            _townInteriorRecoveryVisible = false;
            _previousSelectedObject = null;
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

        private IEnumerator FadeTownInteriorToBlackRoutine(string operationId, float duration)
        {
            yield return FadeRoutine(1f, duration);
            _activeCoroutine = null;
            if (!IsActiveTownInteriorOperation(operationId) || _townInteriorCancelled ||
                _overlay == null || _overlay.alpha < 0.99f) yield break;
            GameEventBus.Publish(new TownInteriorBlackoutReadyEvent(operationId));
        }

        private IEnumerator FadeTownInteriorFromBlackRoutine(string operationId)
        {
            yield return FadeRoutine(0f, _townInteriorFadeInSeconds);
            if (!IsActiveTownInteriorOperation(operationId)) yield break;
            _activeCoroutine = null;
            ClearTownInteriorMode();
            GameEventBus.Publish(new TownInteriorFadeFinishedEvent(operationId));
        }

        private IEnumerator ShowTownInteriorLoadingHintRoutine(string operationId, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _loadingHintCoroutine = null;
            if (IsActiveTownInteriorOperation(operationId) && !_townInteriorCancelled &&
                _overlay != null && _overlay.alpha >= 0.99f)
                SetTownInteriorLoadingHintVisible(true);
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

        private bool IsActiveTownInteriorOperation(string operationId) =>
            !string.IsNullOrWhiteSpace(operationId) && operationId == _townInteriorOperationId;

        private void StopActiveCoroutine()
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        private void ClearTownInteriorMode()
        {
            StopActiveCoroutine();
            StopLoadingHintCoroutine();
            _townInteriorOperationId = string.Empty;
            _townInteriorCancelled = false;
            SetTownInteriorLoadingHintVisible(false);
            HideRecoveryUi();
        }

        private void SetTownInteriorLoadingHintVisible(bool visible)
        {
            if (_townInteriorLoadingHint != null) _townInteriorLoadingHint.enabled = visible;
        }

        private void StopLoadingHintCoroutine()
        {
            if (_loadingHintCoroutine != null) StopCoroutine(_loadingHintCoroutine);
            _loadingHintCoroutine = null;
        }

        private void SetBlackImmediately()
        {
            if (_overlay != null) _overlay.alpha = 1f;
            SetRaycasterEnabled(true);
        }

        private static bool ValidDuration(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }
}
