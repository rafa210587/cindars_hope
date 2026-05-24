using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.Core
{
    [DisallowMultipleComponent]
    public class GameTimeManager : MonoBehaviour
    {
        [SerializeField] private GameTimeBalanceSO _timeBalance;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private ModalManager _modalManager;

        private float _phaseTimer = 0f;
        private GamePhaseChangedEvent.GamePhase _currentPhase = GamePhaseChangedEvent.GamePhase.Day;
        private bool _isInitialized = false;

        public GamePhaseChangedEvent.GamePhase CurrentPhase => _currentPhase;
        public float PhaseTimer => _phaseTimer;
        public bool IsInitialized => _isInitialized;

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_timeBalance == null)
            {
                Debug.LogWarning("GameTimeManager: GameTimeBalanceSO not assigned.");
                return;
            }

            if (_timeManager == null)
            {
                Debug.LogWarning("GameTimeManager: TimeManager not assigned.");
                return;
            }

            _phaseTimer = 0f;
            _currentPhase = GamePhaseChangedEvent.GamePhase.Day;
            _isInitialized = true;
            Debug.Log("GameTimeManager initialized.");
        }

        public void Shutdown()
        {
            _isInitialized = false;
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            if (_modalManager != null && _modalManager.HasActiveModal)
                return;

            _phaseTimer += UnityEngine.Time.deltaTime;

            float phaseDuration = _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                ? _timeBalance.DayDurationSeconds
                : _timeBalance.NightDurationSeconds;

            if (_phaseTimer >= phaseDuration)
            {
                TransitionPhase();
            }
        }

        private void TransitionPhase()
        {
            _phaseTimer = 0f;
            _currentPhase = _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                ? GamePhaseChangedEvent.GamePhase.Night
                : GamePhaseChangedEvent.GamePhase.Day;

            if (_currentPhase == GamePhaseChangedEvent.GamePhase.Day)
            {
                if (_timeManager != null)
                    _timeManager.AdvanceDay();
            }

            GameEventBus.Publish(new GamePhaseChangedEvent(_currentPhase, _timeManager.CurrentDay));
            Debug.Log($"Phase changed to {_currentPhase} on day {_timeManager.CurrentDay}");
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            _phaseTimer = 0f;
            _currentPhase = GamePhaseChangedEvent.GamePhase.Day;
        }
    }
}
