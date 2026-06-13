using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Conditions
{
    /// <summary>
    /// Host runtime do FatigueSystem (Player/Conditions WAVE 05, antes órfão).
    /// Acumula fadiga por hora acordado (relógio derivado das fases Day/Night) e por
    /// stamina gasta; aplica thresholds; força colapso às 02:00; dorme via BedInteractable.
    /// Relógio canônico: Dia = 06:00→20:00 (14h), Noite = 20:00→06:00 (10h); 02:00 = 60% da noite.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerConditionService : MonoBehaviour
    {
        private const float DayPhaseHours = 14f;
        private const float NightPhaseHours = 10f;
        private const float CollapseNightNormalized = 0.6f; // 20:00 + 0.6×10h = 02:00
        private const float ExhaustedSpeedMultiplier = 0.85f;
        private const float CollapseResidualFatigue = 25f;
        private const float CollapseFadeSeconds = 1f;

        private static PlayerConditionService _instance;

        private FatigueSystem _fatigueSystem;
        private CindarsHope.Core.GameTimeManager _gameTimeManager;
        private CindarsHope.Core.Time.TimeManager _timeManager;
        private PlayerController _playerController;
        private float _previousPhaseNormalized;
        private GamePhaseChangedEvent.GamePhase _trackedPhase = GamePhaseChangedEvent.GamePhase.Day;
        private int _previousStamina = -1;
        private FatigueThreshold _previousThreshold = FatigueThreshold.Rested;
        private bool _speedPenaltyApplied;
        private bool _collapsedTonight;
        private float _fadeRemaining;

        public static PlayerConditionService Instance => _instance;

        public float CurrentFatigue => _fatigueSystem != null ? _fatigueSystem.State.FatigueValue : 0f;
        public FatigueThreshold CurrentThreshold => _fatigueSystem != null ? _fatigueSystem.CurrentThreshold : FatigueThreshold.Rested;

        /// <summary>Hora do relógio (0-23, fração) derivada da fase e do progresso normalizado.</summary>
        public static float HourFromPhase(GamePhaseChangedEvent.GamePhase phase, float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (phase == GamePhaseChangedEvent.GamePhase.Day)
            {
                return 6f + normalized * DayPhaseHours;
            }

            return (20f + normalized * NightPhaseHours) % 24f;
        }

        public static bool IsCollapseHour(GamePhaseChangedEvent.GamePhase phase, float normalized)
        {
            return phase == GamePhaseChangedEvent.GamePhase.Night && normalized >= CollapseNightNormalized;
        }

        public void Configure(
            CindarsHope.Core.GameTimeManager gameTimeManager,
            CindarsHope.Core.Time.TimeManager timeManager,
            PlayerController playerController)
        {
            _gameTimeManager = gameTimeManager;
            _timeManager = timeManager;
            _playerController = playerController;
        }

        public void SetFatigue(float value)
        {
            EnsureSystem();
            _fatigueSystem.State.FatigueValue = value;
            _fatigueSystem.State.Clamp();
            ApplyThresholdEffects();
        }

        /// <summary>Dormir voluntário na cama: recuperação plena + novo dia (mesmo caminho de transition).</summary>
        public SleepRecoveryResult SleepInBed()
        {
            EnsureSystem();
            var hour = CurrentHour();
            var result = _fatigueSystem.ApplySleepRecovery(new SleepRecoveryContext
            {
                SleepStartHour = Mathf.FloorToInt(hour),
                SleepEndHour = 6,
                WentToBedLate = hour >= 22f || hour < 6f,
                HungerAtSleep = 1f,
                HasBuffs = false
            });

            _collapsedTonight = false;
            ApplyThresholdEffects();
            AdvanceDay();
            GameEventBus.Publish(new PlayerActionFeedbackEvent($"Voce dormiu. Qualidade do sono: {result.SleepQuality:P0}."));
            return result;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSystem();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameTimeTickEvent>(OnGameTimeTick);
            GameEventBus.Subscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameTimeTickEvent>(OnGameTimeTick);
            GameEventBus.Unsubscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (_fadeRemaining > 0f)
            {
                _fadeRemaining -= UnityEngine.Time.deltaTime;
            }
        }

        private void OnGUI()
        {
            if (_fadeRemaining <= 0f)
            {
                return;
            }

            var alpha = Mathf.Clamp01(_fadeRemaining / CollapseFadeSeconds);
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void OnGameTimeTick(GameTimeTickEvent evt)
        {
            if (_gameTimeManager == null)
            {
                return;
            }

            var phase = _gameTimeManager.CurrentPhase;
            var normalized = _gameTimeManager.CurrentPhaseNormalized;

            if (phase != _trackedPhase)
            {
                _trackedPhase = phase;
                _previousPhaseNormalized = 0f;
            }

            var deltaNormalized = Mathf.Max(0f, normalized - _previousPhaseNormalized);
            _previousPhaseNormalized = normalized;

            var phaseHours = phase == GamePhaseChangedEvent.GamePhase.Day ? DayPhaseHours : NightPhaseHours;
            if (deltaNormalized > 0f)
            {
                _fatigueSystem.AddTimePassingFatigue(deltaNormalized * phaseHours);
                ApplyThresholdEffects();
            }

            if (!_collapsedTonight && IsCollapseHour(phase, normalized))
            {
                Collapse();
            }
        }

        private void OnPhaseChanged(GamePhaseChangedEvent evt)
        {
            _trackedPhase = evt.NewPhase;
            _previousPhaseNormalized = 0f;
        }

        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            if (_previousStamina >= 0 && evt.CurrentStamina < _previousStamina)
            {
                _fatigueSystem.AddFatigueFromStaminaSpend(_previousStamina - evt.CurrentStamina);
                ApplyThresholdEffects();
            }

            _previousStamina = evt.CurrentStamina;
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _collapsedTonight = false;
            _previousPhaseNormalized = 0f;
        }

        private void Collapse()
        {
            _collapsedTonight = true;
            _fadeRemaining = CollapseFadeSeconds;

            // Recuperação penalizada (dormiu tarde) + fadiga residual de colapso.
            var result = _fatigueSystem.ApplySleepRecovery(new SleepRecoveryContext
            {
                SleepStartHour = 2,
                SleepEndHour = 6,
                WentToBedLate = true,
                HungerAtSleep = 1f,
                HasBuffs = false
            });

            _fatigueSystem.AddFatigue(new FatigueGainContext
            {
                Source = FatigueGainSource.StatusEffect,
                BaseAmount = CollapseResidualFatigue,
                Multiplier = 1f
            });

            ApplyThresholdEffects();
            GameEventBus.Publish(new PlayerCollapsedEvent(_timeManager != null ? _timeManager.CurrentDay : 1));
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Voce desmaiou de exaustao as 02:00 e acordou na cama."));
            AdvanceDay();
        }

        private void AdvanceDay()
        {
            if (_timeManager != null)
            {
                _timeManager.AdvanceDay();
            }
            else
            {
                Debug.LogWarning("PlayerConditionService: TimeManager ausente — day transition nao executada. " +
                    "Cena: qualquer | GameObject: PlayerConditionService | campo: _timeManager.");
            }
        }

        private float CurrentHour()
        {
            if (_gameTimeManager == null)
            {
                return 8f;
            }

            return HourFromPhase(_gameTimeManager.CurrentPhase, _gameTimeManager.CurrentPhaseNormalized);
        }

        private void ApplyThresholdEffects()
        {
            var threshold = _fatigueSystem.CurrentThreshold;
            if (threshold == _previousThreshold)
            {
                ApplySpeedPenalty(threshold);
                return;
            }

            if (threshold > _previousThreshold && threshold >= FatigueThreshold.Tired)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Fadiga: {threshold}."));
            }

            _previousThreshold = threshold;
            GameEventBus.Publish(new PlayerFatigueChangedEvent(_fatigueSystem.State.FatigueValue, (int)threshold));
            ApplySpeedPenalty(threshold);
        }

        private void ApplySpeedPenalty(FatigueThreshold threshold)
        {
            if (_playerController == null)
            {
                return;
            }

            var shouldApply = threshold >= FatigueThreshold.VeryTired;
            if (shouldApply == _speedPenaltyApplied)
            {
                return;
            }

            // Multiplicador composto com os demais sistemas; floor 0.5 documentado na F16.
            if (shouldApply)
            {
                _playerController.SpeedMultiplier = Mathf.Max(0.5f, _playerController.SpeedMultiplier * ExhaustedSpeedMultiplier);
            }
            else
            {
                _playerController.SpeedMultiplier = Mathf.Max(0.5f, _playerController.SpeedMultiplier / ExhaustedSpeedMultiplier);
            }

            _speedPenaltyApplied = shouldApply;
        }

        private void EnsureSystem()
        {
            if (_fatigueSystem == null)
            {
                _fatigueSystem = new FatigueSystem();
            }
        }
    }

    /// <summary>Garante o serviço em runtime (padrão FarmDailyGoalRuntimeBootstrap).</summary>
    public static class PlayerConditionRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<PlayerConditionService>() != null)
            {
                return;
            }

            var go = new GameObject("PlayerConditionService");
            Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<PlayerConditionService>();

            var bootstrap = GameBootstrap.Instance;
            var gameTimeManager = bootstrap != null ? bootstrap.GameTimeManager : Object.FindAnyObjectByType<CindarsHope.Core.GameTimeManager>();
            var timeManager = bootstrap != null ? bootstrap.TimeManager : Object.FindAnyObjectByType<CindarsHope.Core.Time.TimeManager>();
            var playerController = Object.FindAnyObjectByType<PlayerController>();
            service.Configure(gameTimeManager, timeManager, playerController);

            Debug.Log("[PlayerConditionRuntimeBootstrap] PlayerConditionService instanciado via bootstrap.");
        }
    }
}
