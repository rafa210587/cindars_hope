using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Foundation.Time;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Core
{
    [DisallowMultipleComponent]
    public class GameTimeManager : MonoBehaviour, IGameClock
    {
        [SerializeField] private GameTimeBalanceSO _timeBalance;
        [SerializeField] private TimeManager _timeManager;

        private float _phaseTimer = 0f;
        private float _tickTimer = 0f;
        private const float TickIntervalSeconds = 1f;
        private GamePhaseChangedEvent.GamePhase _currentPhase = GamePhaseChangedEvent.GamePhase.Day;
        private bool _isInitialized = false;

        // Fallback values if GameTimeBalanceSO not assigned
        private const float DefaultDayDurationSeconds = 1200f; // 20 min
        private const float DefaultNightDurationSeconds = 600f; // 10 min

        public GamePhaseChangedEvent.GamePhase CurrentPhase => _currentPhase;
        public float PhaseTimer => _phaseTimer;
        public bool IsInitialized => _isInitialized;

        public float CurrentPhaseDurationSeconds
        {
            get
            {
                if (_timeBalance != null)
                {
                    return _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                        ? _timeBalance.DayDurationSeconds
                        : _timeBalance.NightDurationSeconds;
                }

                return _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                    ? DefaultDayDurationSeconds
                    : DefaultNightDurationSeconds;
            }
        }

        public float CurrentPhaseNormalized
        {
            get
            {
                var duration = CurrentPhaseDurationSeconds;
                return duration <= 0f ? 0f : Mathf.Clamp01(_phaseTimer / duration);
            }
        }

        /// <summary>
        /// fable_11 — derived hour-of-day [0..23] for NPC schedule block resolution. There is no
        /// minute-accurate clock in the MVP time system, so the hour is mapped from the existing
        /// Day/Night phase plus its normalized progress: the Day phase spans 06:00→18:00 and the
        /// Night phase spans 18:00→06:00 (wrapping midnight). This lets <c>NpcScheduleService</c>
        /// consume an hour (per city_rules.md Rule 6) without introducing a new clock or save field.
        /// </summary>
        public int CurrentHourOfDay
        {
            get
            {
                var t = Mathf.Clamp01(CurrentPhaseNormalized);
                if (_currentPhase == GamePhaseChangedEvent.GamePhase.Day)
                {
                    // 06:00 (t=0) .. 18:00 (t=1)
                    return Mathf.Clamp(6 + Mathf.FloorToInt(t * 12f), 6, 17);
                }

                // Night: 18:00 (t=0) .. 30:00==06:00 (t=1), wrap into [0..23].
                var raw = 18 + Mathf.FloorToInt(t * 12f);
                return raw % 24;
            }
        }

        public int CurrentDay => _timeManager != null ? _timeManager.CurrentDay : 1;
        public bool IsDaytime => _currentPhase == GamePhaseChangedEvent.GamePhase.Day;

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
                Debug.LogWarning("GameTimeManager: GameTimeBalanceSO not assigned. Using fallback defaults (20min day, 10min night).");
            }

            if (_timeManager == null)
            {
                Debug.LogWarning("GameTimeManager: TimeManager not assigned. GameTimeManager will not advance days.");
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

        public void RestoreFromSaveData(GameTimeSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("GameTimeManager received null save data.");
                return;
            }

            _currentPhase = (GamePhaseChangedEvent.GamePhase)Mathf.Clamp(saveData.CurrentPhase, 0, 1);
            _phaseTimer = Mathf.Max(0, saveData.PhaseElapsedSeconds);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            // arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — resolvido via
            // DomainManagerRegistry.Get<IModalStateProvider>() (self-registrado por ModalManager em
            // CindarsHope.UI.Modal) em vez do campo serializado removido, para nao reintroduzir a
            // aresta Core->UI.
            var modalStateProvider = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IModalStateProvider>();
            if (modalStateProvider != null && modalStateProvider.HasActiveModal)
                return;

            _phaseTimer += UnityEngine.Time.deltaTime;
            _tickTimer += UnityEngine.Time.deltaTime;

            if (_tickTimer >= TickIntervalSeconds)
            {
                _tickTimer -= TickIntervalSeconds;
                GameEventBus.Publish(new GameTimeTickEvent(_phaseTimer, _timeManager != null ? _timeManager.CurrentDay : 1));
            }

            float phaseDuration;
            if (_timeBalance != null)
            {
                phaseDuration = _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                    ? _timeBalance.DayDurationSeconds
                    : _timeBalance.NightDurationSeconds;
            }
            else
            {
                phaseDuration = _currentPhase == GamePhaseChangedEvent.GamePhase.Day
                    ? DefaultDayDurationSeconds
                    : DefaultNightDurationSeconds;
            }

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

            GameEventBus.Publish(new GamePhaseChangedEvent(_currentPhase, _timeManager != null ? _timeManager.CurrentDay : 1));
            Debug.Log($"Phase changed to {_currentPhase} on day {(_timeManager != null ? _timeManager.CurrentDay : 1)}");
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            _phaseTimer = 0f;
            _currentPhase = GamePhaseChangedEvent.GamePhase.Day;
        }
    }
}
