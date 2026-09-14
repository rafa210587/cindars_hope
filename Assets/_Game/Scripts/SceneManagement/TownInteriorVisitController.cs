using System;
using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Localization;
using CindarsHope.World.Scenes;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    /// <summary>Town-owned visit transaction. Player mutation, fade and camera are event consumers.</summary>
    [DisallowMultipleComponent]
    public sealed class TownInteriorVisitController : MonoBehaviour, ITownInteriorLocationRuntime, ITownInteriorVisitAdmission
    {
        [Serializable]
        public sealed class HouseBinding
        {
            public string HouseId, EntryAnchorId, ReturnAnchorId;
            public GameObject Prefab;
            public Transform[] ReturnPockets;
        }

        [SerializeField] private string _sceneName = "TownInteriorScene";
        [SerializeField] private HouseBinding[] _houses = Array.Empty<HouseBinding>();
        [SerializeField] private Transform _player;
        [SerializeField] private Collider2D _playerBody;
        [SerializeField] private float _fadeOutSeconds = .20f, _fadeInSeconds = .25f;
        [SerializeField] private float _loadingHintAfterSeconds = .75f, _timeoutSeconds = 10f;
        [SerializeField] private Vector2 _presentationOrigin = new Vector2(4096f, 4096f);
        [SerializeField] private Vector2 _townBoundsCenter, _townBoundsSize;

        private readonly TownInteriorTransitionStateMachine _machine = new TownInteriorTransitionStateMachine();
        private readonly TownInteriorSceneLoader _loader = new TownInteriorSceneLoader();
        private TownInteriorLocationSnapshot _committed;
        private bool _blackoutReady, _placementCompleted, _placementSucceeded, _fadeFinished;
        private bool _cancelRequested, _lockOwned;
        private bool _singleOverride;
        private string _contentOwnerToken;
        private Vector2 _safeExteriorPocket;
        private Func<TownInteriorVisitRequest, bool> _admissionGate;
        private Func<bool> _restoreReadyGate;
        private Transform _defaultTownSpawn;
        private Func<Vector2> _facing;
        private bool _restoreFallbackRunning;
        private string _restoreFallbackToken;
        private bool _awaitSavePreparation, _restoreStateReady, _restoreStateSuccess;
        private Vector2 _restoreSafeExterior;
        private Coroutine _run;
        private string _recoveryOperationId;
        private Func<IEnumerator> _retryRecovery;

        public bool IsTransitioning => _machine.IsTransitioning || _restoreFallbackRunning;
        public float TimeoutSeconds => _timeoutSeconds;
        public bool TryGetPresentedInterior(string houseId, out TownInteriorInstance instance)
        {
            instance = _loader.Instance;
            if (!_committed.IsInside || _committed.HouseId != houseId || instance == null ||
                instance.HouseId != houseId)
            {
                instance = null;
                return false;
            }
            return true;
        }

        public void Configure(Transform player, Collider2D playerBody, HouseBinding[] houses,
            string sceneName, Bounds townBounds, Vector2 presentationOrigin,
            float fadeOutSeconds, float fadeInSeconds, float loadingHintAfterSeconds, float timeoutSeconds,
            Func<TownInteriorVisitRequest, bool> admissionGate,
            Func<bool> restoreReadyGate = null, Transform defaultTownSpawn = null,
            Func<Vector2> facing = null)
        {
            _player = player; _playerBody = playerBody; _houses = houses ?? Array.Empty<HouseBinding>();
            _sceneName = sceneName; _townBoundsCenter = townBounds.center; _townBoundsSize = townBounds.size;
            _presentationOrigin = presentationOrigin; _fadeOutSeconds = fadeOutSeconds;
            _fadeInSeconds = fadeInSeconds; _loadingHintAfterSeconds = loadingHintAfterSeconds;
            _timeoutSeconds = timeoutSeconds;
            _admissionGate = admissionGate;
            _restoreReadyGate = restoreReadyGate;
            _defaultTownSpawn = defaultTownSpawn;
            _facing = facing;
        }

        public void BindRuntime(Transform player, Collider2D body,
            Func<TownInteriorVisitRequest, bool> admissionGate, Func<bool> restoreReadyGate,
            Transform defaultTownSpawn, Func<Vector2> facing)
        {
            _player = player; _playerBody = body; _admissionGate = admissionGate;
            _restoreReadyGate = restoreReadyGate; _defaultTownSpawn = defaultTownSpawn;
            _facing = facing;
            _awaitSavePreparation = true;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<TownInteriorVisitRequestedEvent>(OnVisitRequested);
            GameEventBus.Subscribe<TownInteriorBlackoutReadyEvent>(OnBlackoutReady);
            GameEventBus.Subscribe<TownInteriorPlayerPlacementCompletedEvent>(OnPlacementCompleted);
            GameEventBus.Subscribe<TownInteriorFadeFinishedEvent>(OnFadeFinished);
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSingleTransitionStarted);
            GameEventBus.Subscribe<TownInteriorRestoreStateReadyEvent>(OnRestoreStateReady);
            GameEventBus.Subscribe<TownInteriorRecoveryRetryRequestedEvent>(OnRecoveryRetryRequested);
            if (_awaitSavePreparation && _restoreReadyGate != null && _restoreReadyGate())
                SceneTransitionRouter.RegisterTownInteriorVisitAdmission(this);
            if (_retryRecovery != null && !string.IsNullOrEmpty(_recoveryOperationId))
                GameEventBus.Publish(new TownInteriorRecoveryRequiredEvent(_recoveryOperationId));
        }

        private void OnDisable()
        {
            SceneTransitionRouter.UnregisterTownInteriorVisitAdmission(this);
            GameEventBus.Unsubscribe<TownInteriorVisitRequestedEvent>(OnVisitRequested);
            GameEventBus.Unsubscribe<TownInteriorBlackoutReadyEvent>(OnBlackoutReady);
            GameEventBus.Unsubscribe<TownInteriorPlayerPlacementCompletedEvent>(OnPlacementCompleted);
            GameEventBus.Unsubscribe<TownInteriorFadeFinishedEvent>(OnFadeFinished);
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSingleTransitionStarted);
            GameEventBus.Unsubscribe<TownInteriorRestoreStateReadyEvent>(OnRestoreStateReady);
            GameEventBus.Unsubscribe<TownInteriorRecoveryRetryRequestedEvent>(OnRecoveryRetryRequested);
            if (IsTransitioning)
            {
                _cancelRequested = true;
                string token = _restoreFallbackRunning ? _restoreFallbackToken : _machine.OperationId;
                if (!_singleOverride && !SafeAtCurrentInterior() && !SafeCurrentTown())
                {
                    // Disabling is not proof of a safe placement. Preserve the lease and
                    // recovery continuation so re-enabling can resume the same operation.
                    StopAllCoroutines();
                    _run = null;
                    _machine.TryFail(token, "Interior controller disabled without safe placement.");
                    if (_retryRecovery == null)
                    {
                        if (!_restoreFallbackRunning)
                            _restoreSafeExterior = _committed.IsInside
                                ? new Vector2(_committed.SafeTownX, _committed.SafeTownY) : _safeExteriorPocket;
                        RequireRecovery(token, () => FailRestoreInCurrentLocation(token,
                            "Interior controller resumed after unsafe disable."));
                    }
                    return;
                }
                if (_restoreFallbackRunning)
                {
                    StopAllCoroutines();
                    _restoreFallbackRunning = false;
                    _restoreFallbackToken = null;
                    if (SafeAtCurrentInterior() || SafeCurrentTown())
                    {
                        if (SafeCurrentTown())
                        {
                            _loader.CancelAndUnload(string.IsNullOrEmpty(_contentOwnerToken)
                                ? token : _contentOwnerToken);
                            _contentOwnerToken = null;
                            _committed = default;
                        }
                        GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                        ReleaseLock(token);
                        PublishFailedVisit(token, _committed.HouseId, "Interior restore controller disabled.");
                    }
                    else Debug.LogError("Restore disabled without confirmed safe Player position; retaining content.", this);
                    return;
                }
                if (_singleOverride)
                {
                    // A Single transition owns the overlay and scene teardown from this point.
                    _loader.CancelAndUnload(string.IsNullOrEmpty(_contentOwnerToken) ? token : _contentOwnerToken);
                    StopAllCoroutines();
                    _machine.TryFail(token, "Single transition superseded interior visit.");
                    _machine.CompleteRollback(token);
                    ReleaseLock(token);
                    return;
                }
                GameEventBus.Publish(new TownInteriorVisitCancelledEvent(token));
                if (SafeAtCurrentInterior())
                {
                    var bounds = _loader.Instance.WorldBounds;
                    GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                        bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
                    GameEventBus.Publish(new TownInteriorContextChangedEvent(token, _loader.Instance.HouseId, true));
                    GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                }
                else if (!_committed.IsInside && SafeCurrentTown())
                {
                    GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
                    GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                    _loader.CancelAndUnload(token);
                }
                else Debug.LogError("Town interior controller disabled without a confirmed safe Player location; retaining content.", this);
                StopAllCoroutines();
                _machine.TryFail(token, "Interior controller disabled.");
                _machine.CompleteRollback(token);
                ReleaseLock(token);
                PublishFailedVisit(token, _committed.HouseId, "Interior controller disabled during transition.");
            }
            // Idle inside: keep the floor under the Player. Caller must use TryRequestSafeExit first.
        }

        private void OnDestroy() => ClearRecovery();

        public bool TryRequestSafeExit(string operationId, out string reason)
        {
            if (!_committed.IsInside || !TryBinding(_committed.HouseId, out var binding) ||
                string.IsNullOrWhiteSpace(operationId))
            { reason = "Player is not in a restorable Town interior."; return false; }
            var request = new TownInteriorVisitRequest(operationId, binding.HouseId,
                binding.EntryAnchorId, binding.ReturnAnchorId, true);
            if (!CanBegin(request, out reason)) return false;
            Begin(request, default);
            return true;
        }

        public bool CanBegin(TownInteriorVisitRequest request, out string reason)
        {
            if (!isActiveAndEnabled || !request.IsValid || _admissionGate == null || !_admissionGate(request) ||
                _player == null || _playerBody == null || IsTransitioning ||
                (_loader.IsBusy && !_committed.IsInside) ||
                !TryBinding(request.HouseId, out var binding) || binding.Prefab == null ||
                binding.EntryAnchorId != request.EntryAnchorId || binding.ReturnAnchorId != request.ReturnAnchorId ||
                request.Exiting != _committed.IsInside ||
                (request.Exiting && _committed.HouseId != request.HouseId))
            { reason = "Interior visit is not ready for this house or direction."; return false; }
            if (!Finite(_timeoutSeconds) || _timeoutSeconds <= 0f ||
                !Finite(_fadeOutSeconds) || _fadeOutSeconds <= 0f ||
                !Finite(_fadeInSeconds) || _fadeInSeconds <= 0f ||
                !Finite(_loadingHintAfterSeconds) || _loadingHintAfterSeconds < 0f ||
                !Finite(_presentationOrigin.x) || !Finite(_presentationOrigin.y) ||
                !Finite(_townBoundsCenter.x) || !Finite(_townBoundsCenter.y) ||
                !Finite(_townBoundsSize.x) || !Finite(_townBoundsSize.y) ||
                _townBoundsSize.x <= 0f || _townBoundsSize.y <= 0f)
            { reason = "Interior transition configuration is invalid."; return false; }
            reason = string.Empty;
            return true;
        }

        public TownInteriorLocationSnapshot CaptureCommitted()
        {
            if (IsTransitioning) return _committed;
            if (!_committed.IsInside || _player == null || _loader.Instance == null) return _committed;
            var local = _loader.Instance.transform.InverseTransformPoint(_player.position);
            var facing = _facing != null ? _facing() : new Vector2(_committed.FacingX, _committed.FacingY);
            if (!Finite(facing.x) || !Finite(facing.y))
                facing = new Vector2(_committed.FacingX, _committed.FacingY);
            return new TownInteriorLocationSnapshot(_committed.HouseId, _committed.AnchorId,
                _committed.ReturnAnchorId, local.x, local.y, facing.x,
                facing.y, _committed.SafeTownX, _committed.SafeTownY);
        }

        public bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, out string reason) =>
            TryRequestRestore(snapshot, Guid.NewGuid().ToString("N"), out reason);

        public bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, string operationId, out string reason)
        {
            if (string.IsNullOrWhiteSpace(operationId) || !isActiveAndEnabled || IsTransitioning ||
                _player == null || _playerBody == null || _restoreReadyGate == null || !_restoreReadyGate() ||
                _defaultTownSpawn == null || !ValidTiming() ||
                (_loader.IsBusy && !_committed.IsInside) ||
                (_committed.IsInside && !_loader.IsLoaded))
            { reason = "Interior restore runtime is not ready."; return false; }
            HouseBinding binding = null;
            bool valid = snapshot.TryValidate(out var validationReason) && snapshot.IsInside &&
                TryBinding(snapshot.HouseId, out binding) && binding.Prefab != null &&
                binding.EntryAnchorId == snapshot.AnchorId && binding.ReturnAnchorId == snapshot.ReturnAnchorId;
            if (!TownInteriorRestoreCoordinator.TryFallback(snapshot, _houses, _defaultTownSpawn,
                position => SafeSpawn(position, TownBounds), out var fallback, out var fallbackReason))
            { reason = fallbackReason; return false; }
            _restoreSafeExterior = fallback;
            operationId = operationId.Trim();
            if (!valid)
            {
                reason = string.IsNullOrEmpty(validationReason)
                    ? "Unknown house or anchor; using safe Town fallback." : validationReason;
                BeginRestoreFallback(operationId, snapshot, fallback, reason + " " + fallbackReason);
                return true;
            }
            var request = new TownInteriorVisitRequest(operationId, binding.HouseId,
                binding.EntryAnchorId, binding.ReturnAnchorId, false);
            if (!_machine.TryBegin(request, out reason)) return false;
            StartBlackout(operationId);
            _run = StartCoroutine(RunRestore(request, snapshot, fallback));
            reason = string.Empty;
            return true;
        }

        private void OnVisitRequested(TownInteriorVisitRequestedEvent evt)
        {
            if (CanBegin(evt.Request, out _)) Begin(evt.Request, default);
        }

        private void Begin(TownInteriorVisitRequest request, TownInteriorLocationSnapshot restoreTarget)
        {
            if (!_machine.TryBegin(request, out _)) return;
            StartBlackout(request.OperationId);
            _run = StartCoroutine(Run(request, restoreTarget));
        }

        private void StartBlackout(string operationId)
        {
            _blackoutReady = _placementCompleted = _fadeFinished = _cancelRequested = _singleOverride = false;
            _lockOwned = true;
            GameEventBus.Publish(new TownInteriorPlayerLockEvent(operationId, true));
            GameEventBus.Publish(new TownInteriorBlackoutRequestedEvent(operationId,
                _fadeOutSeconds, _fadeInSeconds, _loadingHintAfterSeconds));
        }

        private Bounds TownBounds => new Bounds(_townBoundsCenter, _townBoundsSize);

        private bool ValidTiming() => Finite(_timeoutSeconds) && _timeoutSeconds > 0f &&
            Finite(_fadeOutSeconds) && _fadeOutSeconds > 0f && Finite(_fadeInSeconds) &&
            _fadeInSeconds > 0f && Finite(_loadingHintAfterSeconds) && _loadingHintAfterSeconds >= 0f &&
            Finite(_presentationOrigin.x) && Finite(_presentationOrigin.y) &&
            Finite(_townBoundsCenter.x) && Finite(_townBoundsCenter.y) &&
            Finite(_townBoundsSize.x) && Finite(_townBoundsSize.y) &&
            _townBoundsSize.x > 0f && _townBoundsSize.y > 0f;

        private void BeginRestoreFallback(string token, TownInteriorLocationSnapshot snapshot,
            Vector3 fallback, string reason)
        {
            _restoreFallbackRunning = true;
            _restoreFallbackToken = token;
            StartBlackout(token);
            _run = StartCoroutine(RunRestoreFallback(token, snapshot, fallback, reason, false));
        }

        private IEnumerator RunRestore(TownInteriorVisitRequest request,
            TownInteriorLocationSnapshot target, Vector3 fallback)
        {
            string token = request.OperationId;
            float started = Time.unscaledTime;
            _machine.Advance(token, TownVisitPhase.Opening, TownVisitPhase.Armed);
            _machine.Advance(token, TownVisitPhase.Armed, TownVisitPhase.FadingOut);
            while (!_blackoutReady && !TimedOut(token, started)) yield return null;
            if (_cancelRequested || _machine.Phase == TownVisitPhase.RollingBack)
            { yield return FailRestoreInCurrentLocation(token, "Restore blackout failed."); yield break; }
            yield return PrepareRestoreState(token);
            if (!_restoreStateSuccess)
            { yield return FailRestoreInCurrentLocation(token, "Save state preparation failed or timed out."); yield break; }
            if (!TownInteriorRestoreCoordinator.TryFallback(target, _houses, _defaultTownSpawn,
                position => SafeSpawn(position, TownBounds), out fallback, out _))
            { yield return FailRestoreInCurrentLocation(token, "No clear Town fallback after save preparation."); yield break; }
            _restoreSafeExterior = fallback;

            if (_committed.IsInside && _loader.Instance != null &&
                _committed.HouseId != request.HouseId)
            {
                // Move to a validated exterior floor before unloading the previous house.
                _machine.Advance(token, TownVisitPhase.FadingOut, TownVisitPhase.Loading);
                yield return Place(token, fallback, 0f, -1f);
                if (!_placementSucceeded || !SafeCurrentTown())
                { yield return FailRestoreInCurrentLocation(token, "Could not leave the previous interior safely."); yield break; }
                _loader.CancelAndUnload(_contentOwnerToken);
                while (_loader.IsBusy && !TimedOut(token, started)) yield return null;
                if (_loader.IsBusy)
                { yield return RunRestoreFallback(token, target, fallback,
                    "Previous interior cleanup timed out; safe Town fallback.", true); yield break; }
                _contentOwnerToken = null;
                _committed = default;
            }
            else _machine.Advance(token, TownVisitPhase.FadingOut, TownVisitPhase.Loading);

            TownInteriorInstance instance = _loader.Instance;
            if (instance == null || instance.HouseId != request.HouseId)
            {
                if (!TryBinding(request.HouseId, out var binding))
                { yield return RunRestoreFallback(token, target, fallback, "House binding vanished; safe Town fallback.", true); yield break; }
                bool loaded = false, completed = false;
                string loadReason = string.Empty;
                StartCoroutine(_loader.Load(token, _sceneName, request.HouseId, binding.Prefab,
                    _presentationOrigin, TownBounds,
                    (ok, why) => { loaded = ok; loadReason = why; completed = true; }));
                while (!completed && !TimedOut(token, started)) yield return null;
                if (!loaded || _cancelRequested || TimedOut(token, started))
                {
                    yield return RunRestoreFallback(token, target, fallback,
                        "Interior load failed: " + loadReason + " Safe Town fallback.", true);
                    yield break;
                }
                _contentOwnerToken = token;
                instance = _loader.Instance;
            }
            var destination = instance.transform.TransformPoint(new Vector3(target.LocalX, target.LocalY, 0f));
            if (!SafeSpawn(destination, instance.WorldBounds))
            { yield return RunRestoreFallback(token, target, fallback, "Saved local position is obstructed; safe Town fallback.", true); yield break; }
            _machine.Advance(token, TownVisitPhase.Loading, TownVisitPhase.Placing);
            yield return Place(token, destination, target.FacingX, target.FacingY);
            if (!_placementSucceeded || !SafeAtCurrentInterior() || TimedOut(token, started))
            { yield return RunRestoreFallback(token, target, fallback, "Interior placement failed; safe Town fallback.", true); yield break; }
            var local = instance.transform.InverseTransformPoint(_player.position);
            _committed = new TownInteriorLocationSnapshot(request.HouseId, request.EntryAnchorId,
                request.ReturnAnchorId, local.x, local.y, target.FacingX, target.FacingY,
                fallback.x, fallback.y);
            var bounds = instance.WorldBounds;
            GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
            GameEventBus.Publish(new CameraSnapRequestedEvent());
            GameEventBus.Publish(new TownInteriorContextChangedEvent(token, request.HouseId, true));
            GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, request.HouseId));
            _machine.Advance(token, TownVisitPhase.Placing, TownVisitPhase.FadingIn);
            while (!_fadeFinished && !TimedOut(token, started)) yield return null;
            if (_cancelRequested || _machine.Phase == TownVisitPhase.RollingBack)
            { yield return FailRestoreInCurrentLocation(token, "Restore fade did not finish."); yield break; }
            _machine.Complete(token);
            ReleaseLock(token);
            GameEventBus.Publish(new TownInteriorVisitFinishedEvent(token, request.HouseId, true, string.Empty));
            _run = null;
        }

        private IEnumerator RunRestoreFallback(string token, TownInteriorLocationSnapshot snapshot,
            Vector3 fallback, string reason, bool fromMachine)
        {
            if (!fromMachine)
            {
                float started = Time.unscaledTime;
                while (!_blackoutReady && !_cancelRequested && Time.unscaledTime - started < _timeoutSeconds)
                    yield return null;
                if (!_blackoutReady || _cancelRequested)
                { yield return FailRestoreInCurrentLocation(token, "Restore blackout failed."); yield break; }
                yield return PrepareRestoreState(token);
                if (!_restoreStateSuccess)
                { yield return FailRestoreInCurrentLocation(token, "Save state preparation failed or timed out."); yield break; }
            }
            if (_singleOverride)
            {
                if (fromMachine) { _machine.TryFail(token, "Single transition superseded restore."); _machine.CompleteRollback(token); }
                else { _restoreFallbackRunning = false; _restoreFallbackToken = null; }
                ReleaseLock(token);
                _run = null;
                yield break;
            }
            if (!TownInteriorRestoreCoordinator.TryFallback(snapshot, _houses, _defaultTownSpawn,
                position => SafeSpawn(position, TownBounds), out fallback, out var selected))
            { yield return FailRestoreInCurrentLocation(token, reason + " No safe Town fallback remains."); yield break; }
            _restoreSafeExterior = fallback;
            yield return Place(token, fallback, 0f, -1f);
            if (!_placementSucceeded || !SafeCurrentTown())
            { yield return FailRestoreInCurrentLocation(token, reason + " Town placement was not acknowledged."); yield break; }
            if (!string.IsNullOrEmpty(_contentOwnerToken))
                _loader.CancelAndUnload(_contentOwnerToken);
            else _loader.CancelAndUnload(token);
            _contentOwnerToken = null;
            _committed = default;
            GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
            GameEventBus.Publish(new CameraSnapRequestedEvent());
            GameEventBus.Publish(new TownInteriorContextChangedEvent(token, string.Empty, false));
            _fadeFinished = false;
            GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
            float fadeStart = Time.unscaledTime;
            while (!_fadeFinished && Time.unscaledTime - fadeStart < _timeoutSeconds) yield return null;
            if (!_fadeFinished)
            { yield return FailRestoreInCurrentLocation(token, "Town fallback fade did not finish."); yield break; }
            if (fromMachine)
            {
                _machine.TryFail(token, reason);
                _machine.CompleteRollback(token);
            }
            else { _restoreFallbackRunning = false; _restoreFallbackToken = null; }
            ReleaseLock(token);
            GameEventBus.Publish(new TownInteriorVisitFinishedEvent(token, string.Empty, true,
                reason + " " + selected));
            _run = null;
        }

        private IEnumerator FailRestoreInCurrentLocation(string token, string reason)
        {
            if (_singleOverride)
            {
                if (_machine.IsTransitioning) { _machine.TryFail(token, reason); _machine.CompleteRollback(token); }
                _restoreFallbackRunning = false; _restoreFallbackToken = null;
                ReleaseLock(token);
                _run = null;
                yield break;
            }
            if (SafeAtCurrentInterior())
            {
                var bounds = _loader.Instance.WorldBounds;
                var local = _loader.Instance.transform.InverseTransformPoint(_player.position);
                if (!TryBinding(_loader.Instance.HouseId, out var safeBinding))
                {
                    RequireRecovery(token, () => FailRestoreInCurrentLocation(token, reason));
                    yield break;
                }
                _committed = new TownInteriorLocationSnapshot(safeBinding.HouseId,
                    safeBinding.EntryAnchorId, safeBinding.ReturnAnchorId, local.x, local.y,
                    _committed.FacingX, _committed.FacingY,
                    _restoreSafeExterior.x, _restoreSafeExterior.y);
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                    bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
                GameEventBus.Publish(new TownInteriorContextChangedEvent(token, _loader.Instance.HouseId, true));
                _fadeFinished = false;
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                yield return WaitForRollbackFade();
            }
            else if (SafeCurrentTown())
            {
                _loader.CancelAndUnload(string.IsNullOrEmpty(_contentOwnerToken)
                    ? token : _contentOwnerToken);
                _contentOwnerToken = null;
                _committed = default;
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
                GameEventBus.Publish(new TownInteriorContextChangedEvent(token, string.Empty, false));
                _fadeFinished = false;
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                yield return WaitForRollbackFade();
            }
            else
            {
                RequireRecovery(token, () => FailRestoreInCurrentLocation(token, reason));
                yield break;
            }
            if (_machine.IsTransitioning) { _machine.TryFail(token, reason); _machine.CompleteRollback(token); }
            _restoreFallbackRunning = false; _restoreFallbackToken = null;
            ReleaseLock(token);
            PublishFailedVisit(token, _committed.HouseId, reason);
            _run = null;
        }

        private IEnumerator PrepareRestoreState(string token)
        {
            _restoreStateSuccess = !_awaitSavePreparation;
            if (!_awaitSavePreparation) yield break;
            _restoreStateReady = false;
            GameEventBus.Publish(new TownInteriorRestoreStateRequestedEvent(token));
            float started = Time.unscaledTime;
            while (!_restoreStateReady && !_cancelRequested &&
                   Time.unscaledTime - started < _timeoutSeconds) yield return null;
            if (!_restoreStateReady || _cancelRequested) _restoreStateSuccess = false;
        }

        private IEnumerator Run(TownInteriorVisitRequest request, TownInteriorLocationSnapshot restoreTarget)
        {
            string token = request.OperationId;
            var before = CaptureCommitted();
            var beforeWorld = _player.position;
            Transform safePocket = null;
            if (!request.Exiting && (!TryBinding(request.HouseId, out var entryBinding) ||
                !TryReturnPocket(entryBinding, out safePocket)))
            { _machine.TryFail(token, "No free exterior return pocket."); _cancelRequested = true; }
            else if (!request.Exiting) _safeExteriorPocket = safePocket.position;
            float started = Time.unscaledTime;
            string failure = string.Empty;
            _machine.Advance(token, TownVisitPhase.Opening, TownVisitPhase.Armed);
            _machine.Advance(token, TownVisitPhase.Armed, TownVisitPhase.FadingOut);
            while (!_blackoutReady && !TimedOut(token, started)) yield return null;
            if (_cancelRequested || _machine.Phase == TownVisitPhase.RollingBack)
            { failure = "Visit cancelled before blackout."; goto Rollback; }

            if (request.Exiting)
            {
                if (!_committed.IsInside || !TryBinding(request.HouseId, out var exitBinding) ||
                    !TryReturnPocket(exitBinding, out var pocket))
                { failure = "No clear return pocket."; goto Rollback; }
                _machine.Advance(token, TownVisitPhase.FadingOut, TownVisitPhase.Loading);
                _machine.Advance(token, TownVisitPhase.Loading, TownVisitPhase.Placing);
                yield return Place(token, pocket.position, 0f, -1f);
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
            }
            else
            {
                if (!TryBinding(request.HouseId, out var binding))
                { failure = "Interior house binding was removed."; goto Rollback; }
                _machine.Advance(token, TownVisitPhase.FadingOut, TownVisitPhase.Loading);
                bool loaded = false;
                bool loadFinished = false;
                string loadReason = string.Empty;
                StartCoroutine(_loader.Load(token, _sceneName, request.HouseId, binding.Prefab,
                    _presentationOrigin, new Bounds(_townBoundsCenter, _townBoundsSize),
                    (ok, reason) => { loaded = ok; loadReason = reason; loadFinished = true; }));
                while (!loadFinished && !TimedOut(token, started)) yield return null;
                if (!loaded || _cancelRequested || TimedOut(token, started))
                { failure = string.IsNullOrEmpty(loadReason) ? "Interior load cancelled or timed out." : loadReason; goto Rollback; }
                _contentOwnerToken = token;
                var instance = _loader.Instance;
                Vector3 destination = restoreTarget.IsInside
                    ? instance.transform.TransformPoint(new Vector3(restoreTarget.LocalX, restoreTarget.LocalY, 0f))
                    : instance.EntryAnchor.position;
                if (!SafeSpawn(destination, instance.WorldBounds))
                { failure = "Interior spawn does not fit the player body."; goto Rollback; }
                _machine.Advance(token, TownVisitPhase.Loading, TownVisitPhase.Placing);
                var bounds = instance.WorldBounds;
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                    bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
                yield return Place(token, destination, restoreTarget.IsInside ? restoreTarget.FacingX : 0f,
                    restoreTarget.IsInside ? restoreTarget.FacingY : 1f);
            }

            if (!_placementSucceeded || _cancelRequested || TimedOut(token, started))
            { failure = "Player placement failed or timed out."; goto Rollback; }
            if (request.Exiting)
                _committed = default;
            else
            {
                var instance = _loader.Instance;
                var local = instance.transform.InverseTransformPoint(_player.position);
                _committed = new TownInteriorLocationSnapshot(request.HouseId, request.EntryAnchorId,
                    request.ReturnAnchorId, local.x, local.y, restoreTarget.IsInside ? restoreTarget.FacingX : 0f,
                    restoreTarget.IsInside ? restoreTarget.FacingY : 1f,
                    before.IsInside ? before.SafeTownX : _safeExteriorPocket.x,
                    before.IsInside ? before.SafeTownY : _safeExteriorPocket.y);
            }
            GameEventBus.Publish(new CameraSnapRequestedEvent());
            GameEventBus.Publish(new TownInteriorContextChangedEvent(token, _committed.HouseId, _committed.IsInside));
            GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, request.HouseId));
            _machine.Advance(token, TownVisitPhase.Placing, TownVisitPhase.FadingIn);
            while (!_fadeFinished && !TimedOut(token, started)) yield return null;
            if (_cancelRequested || _machine.Phase == TownVisitPhase.RollingBack)
            { failure = "Fade did not finish."; goto Rollback; }
            if (request.Exiting)
            {
                yield return _loader.Unload(_contentOwnerToken);
                _contentOwnerToken = null;
            }
            _machine.Complete(token);
            ReleaseLock(token);
            GameEventBus.Publish(new TownInteriorVisitFinishedEvent(token, request.HouseId, true, string.Empty));
            _run = null;
            yield break;

        Rollback:
            yield return RollbackVisit(request, before, beforeWorld, failure);
        }

        private IEnumerator RollbackVisit(TownInteriorVisitRequest request,
            TownInteriorLocationSnapshot before, Vector3 beforeWorld, string failure)
        {
            string token = request.OperationId;
            if (_singleOverride)
            {
                // Single load supersedes this visit; no Town fade or relocation may follow it.
                _loader.CancelAndUnload(string.IsNullOrEmpty(_contentOwnerToken) ? token : _contentOwnerToken);
                ReleaseLock(token);
                _run = null;
                yield break;
            }
            _machine.TryFail(token, failure);
            GameEventBus.Publish(new TownInteriorVisitCancelledEvent(token));
            // Keep blackout until the original location, bounds and camera are restored.
            bool safeAtOrigin = request.Exiting ? SafeAtCurrentInterior() : SafeCurrentTown();
            if (!safeAtOrigin)
            {
                Vector3 recovery = beforeWorld;
                Transform returnPocket = null;
                bool recoveryAvailable = before.IsInside
                    ? _loader.Instance != null && SafeSpawn(beforeWorld, _loader.Instance.WorldBounds)
                    : TryBinding(request.HouseId, out var returnBinding) &&
                      TryReturnPocket(returnBinding, out returnPocket);
                if (!before.IsInside && recoveryAvailable) recovery = returnPocket.position;
                if (recoveryAvailable)
                {
                    yield return Place(token, recovery, before.FacingX, before.FacingY);
                    safeAtOrigin = _placementSucceeded && (before.IsInside ? SafeAtCurrentInterior() : SafeCurrentTown());
                }
            }
            if (!safeAtOrigin && SafeAtCurrentInterior())
            {
                // Placement back to Town failed, but the current body is safe on retained interior floor.
                var local = _loader.Instance.transform.InverseTransformPoint(_player.position);
                _committed = new TownInteriorLocationSnapshot(request.HouseId, request.EntryAnchorId,
                    request.ReturnAnchorId, local.x, local.y, before.FacingX, before.FacingY,
                    before.IsInside ? before.SafeTownX : _safeExteriorPocket.x,
                    before.IsInside ? before.SafeTownY : _safeExteriorPocket.y);
                var current = _loader.Instance.WorldBounds;
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                    current.min.x, current.min.y, current.max.x, current.max.y));
                GameEventBus.Publish(new TownInteriorContextChangedEvent(token, request.HouseId, true));
                _fadeFinished = false;
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                yield return WaitForRollbackFade();
                _machine.CompleteRollback(token);
                ReleaseLock(token);
                PublishFailedVisit(token, request.HouseId,
                    failure + " Player remains safely inside; retry exit.");
                _run = null;
                yield break;
            }
            if (!safeAtOrigin && SafeCurrentTown())
            {
                _committed = default;
                _loader.CancelAndUnload(string.IsNullOrEmpty(_contentOwnerToken) ? token : _contentOwnerToken);
                _contentOwnerToken = null;
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
                GameEventBus.Publish(new TownInteriorContextChangedEvent(token, string.Empty, false));
                _fadeFinished = false;
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                yield return WaitForRollbackFade();
                _machine.CompleteRollback(token);
                ReleaseLock(token);
                PublishFailedVisit(token, request.HouseId,
                    failure + " Player remains safely outside.");
                _run = null;
                yield break;
            }
            if (!safeAtOrigin)
            {
                RequireRecovery(token, () => RollbackVisit(request, before, beforeWorld, failure));
                yield break;
            }
            _committed = before;
            if (!request.Exiting)
            {
                // Async load may still be pending. Fence its callback and unload later by exact handle;
                // the Player is already safe outside, so do not wait indefinitely under blackout.
                _loader.CancelAndUnload(token);
                _contentOwnerToken = null;
            }
            if (before.IsInside && _loader.Instance != null)
            {
                var old = _loader.Instance.WorldBounds;
                GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, true,
                    old.min.x, old.min.y, old.max.x, old.max.y));
            }
            else GameEventBus.Publish(new TownInteriorCameraBoundsChangedEvent(token, false, 0f, 0f, 0f, 0f));
            GameEventBus.Publish(new CameraSnapRequestedEvent());
            GameEventBus.Publish(new TownInteriorContextChangedEvent(token, before.HouseId, before.IsInside));
            _fadeFinished = false;
            GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
            yield return WaitForRollbackFade();
            _machine.CompleteRollback(token);
            ReleaseLock(token);
            PublishFailedVisit(token, request.HouseId, failure);
            _run = null;
        }

        private void RequireRecovery(string token, Func<IEnumerator> retry)
        {
            _run = null;
            _recoveryOperationId = token;
            _retryRecovery = retry;
            // Keep the existing transition lease and loaded floor. Only an explicit retry
            // may revalidate placement; an obstructed body must never be unlocked here.
            GameEventBus.Publish(new TownInteriorRecoveryRequiredEvent(token));
        }

        private void OnRecoveryRetryRequested(TownInteriorRecoveryRetryRequestedEvent evt)
        {
            if (!isActiveAndEnabled || _singleOverride || _run != null || _retryRecovery == null ||
                string.IsNullOrWhiteSpace(evt.OperationId) || evt.OperationId != _recoveryOperationId)
                return;
            var retry = _retryRecovery;
            ClearRecovery();
            _run = StartCoroutine(RetryRecovery(retry));
        }

        private IEnumerator RetryRecovery(Func<IEnumerator> retry)
        {
            // Do not finish/re-arm synchronously inside the Button/EventBus dispatch.
            yield return null;
            if (!_singleOverride) yield return retry();
        }

        private void ClearRecovery()
        {
            _recoveryOperationId = null;
            _retryRecovery = null;
        }

        private static void PublishFailedVisit(string token, string houseId, string reason)
        {
            GameEventBus.Publish(new TownInteriorVisitFinishedEvent(token, houseId, false, reason));
            const string failureKey = "ui.town_interior.visit_failed";
            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                LocalizationService.Get(failureKey), failureKey: failureKey));
        }

        private IEnumerator WaitForRollbackFade()
        {
            float rollbackStart = Time.unscaledTime;
            while (!_fadeFinished && Time.unscaledTime - rollbackStart < _timeoutSeconds) yield return null;
        }

        private IEnumerator Place(string token, Vector3 position, float facingX, float facingY)
        {
            _placementCompleted = _placementSucceeded = false;
            GameEventBus.Publish(new TownInteriorPlayerPlacementRequestedEvent(token,
                position.x, position.y, facingX, facingY));
            float start = Time.unscaledTime;
            while (!_placementCompleted && Time.unscaledTime - start < _timeoutSeconds) yield return null;
        }

        private bool TimedOut(string token, float started) =>
            _cancelRequested || _machine.TryTimeout(token, Time.unscaledTime - started, _timeoutSeconds);

        private void ReleaseLock(string token)
        {
            ClearRecovery();
            if (!_lockOwned) return;
            _lockOwned = false;
            GameEventBus.Publish(new TownInteriorPlayerLockEvent(token, false));
        }

        private bool TryBinding(string houseId, out HouseBinding binding)
        {
            foreach (var candidate in _houses)
                if (candidate != null && candidate.HouseId == houseId) { binding = candidate; return true; }
            binding = null; return false;
        }

        private bool TryReturnPocket(HouseBinding binding, out Transform pocket)
        {
            if (binding.ReturnPockets != null)
                foreach (var candidate in binding.ReturnPockets)
                    if (candidate != null && SafeSpawn(candidate.position,
                        new Bounds(_townBoundsCenter, _townBoundsSize)))
                    { pocket = candidate; return true; }
            pocket = null; return false;
        }

        private bool SafeSpawn(Vector3 world, Bounds bounds)
        {
            if (_player == null || _playerBody == null || !Finite(world.x) || !Finite(world.y)) return false;
            var size = _playerBody.bounds.size + new Vector3(.30f, .30f, 0f);
            var box = new Bounds(world + (_playerBody.bounds.center - _player.position), size);
            if (box.min.x < bounds.min.x || box.max.x > bounds.max.x ||
                box.min.y < bounds.min.y || box.max.y > bounds.max.y) return false;
            var hits = new Collider2D[32];
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.OverlapBox(box.center, box.size, 0f, filter, hits);
            if (count == hits.Length) return false;
            for (int i = 0; i < count; i++)
                if (hits[i] != null && hits[i] != _playerBody &&
                    hits[i].attachedRigidbody != _playerBody.attachedRigidbody && !hits[i].isTrigger)
                    return false;
            return true;
        }

        private bool SafeAtCurrentInterior() => _player != null && _loader.IsLoaded && _loader.Instance != null &&
            SafeSpawn(_player.position, _loader.Instance.WorldBounds);

        private bool SafeCurrentTown() => _player != null && SafeSpawn(_player.position,
            new Bounds(_townBoundsCenter, _townBoundsSize));

        private void OnBlackoutReady(TownInteriorBlackoutReadyEvent evt)
        { if (_machine.OperationId == evt.OperationId || _restoreFallbackToken == evt.OperationId) _blackoutReady = true; }
        private void OnPlacementCompleted(TownInteriorPlayerPlacementCompletedEvent evt)
        {
            if (_machine.OperationId != evt.OperationId && _restoreFallbackToken != evt.OperationId) return;
            _placementCompleted = true; _placementSucceeded = evt.Success;
        }
        private void OnFadeFinished(TownInteriorFadeFinishedEvent evt)
        { if (_machine.OperationId == evt.OperationId || _restoreFallbackToken == evt.OperationId) _fadeFinished = true; }
        private void OnSingleTransitionStarted(SceneTransitionStartedEvent _)
        { if (IsTransitioning) { ClearRecovery(); _singleOverride = true; _cancelRequested = true; } }
        private void OnRestoreStateReady(TownInteriorRestoreStateReadyEvent evt)
        {
            if (evt.OperationId != _machine.OperationId && evt.OperationId != _restoreFallbackToken) return;
            _restoreStateReady = true;
            _restoreStateSuccess = evt.Success;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
