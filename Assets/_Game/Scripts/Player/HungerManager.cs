using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class HungerManager : MonoBehaviour
    {
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private PlayerNeedsBalanceSO _playerNeedsBalance;
        [SerializeField] private int _hpLossWhenHungerEmpty = 5;
        [SerializeField] private Vector2 _respawnPosition = Vector2.zero;

        private int _stepsPerHungerTick = 10;
        private int _hungerLossPerTick = 1;
        private int _hungerLossPerDay = 10;
        private float _distanceAccumulator;
        private bool _criticalEventPublished;
        private bool _emptyEventPublished;

        public int CurrentHunger { get; private set; }
        public int MaxHunger { get; private set; } = 1;
        public bool IsEmpty => CurrentHunger <= 0;
        public bool IsCritical => CurrentHunger > 0 && CurrentHunger <= GetCriticalThreshold();

        public void Initialize(PlayerDataSO playerData)
        {
            _playerData = playerData;

            if (_playerData == null)
            {
                Debug.LogWarning("HungerManager initialized without PlayerDataSO. Using safe fallback hunger values.", this);
                MaxHunger = 100;
                CurrentHunger = 100;
                _stepsPerHungerTick = 10;
                _hungerLossPerTick = 1;
                _hungerLossPerDay = 10;
                return;
            }

            MaxHunger = Mathf.Max(1, _playerData.MaxHunger);
            CurrentHunger = Mathf.Clamp(_playerData.StartingHunger, 0, MaxHunger);
            _stepsPerHungerTick = Mathf.Max(1, _playerData.StepsPerHungerTick);
            _hungerLossPerTick = Mathf.Max(1, _playerData.HungerLossPerTick);
            _hungerLossPerDay = Mathf.Max(0, _playerData.HungerLossPerDay);
            _distanceAccumulator = 0f;
            _criticalEventPublished = IsCritical;
            _emptyEventPublished = IsEmpty;

            GameEventBus.Publish(new HungerChangedEvent(CurrentHunger, CurrentHunger, MaxHunger));
        }

        public void RestoreHunger(int amount)
        {
            if (amount <= 0 || CurrentHunger >= MaxHunger)
            {
                return;
            }

            var previousHunger = CurrentHunger;
            CurrentHunger = Mathf.Min(MaxHunger, CurrentHunger + amount);
            var delta = CurrentHunger - previousHunger;
            if (delta <= 0)
            {
                return;
            }

            if (!IsCritical)
            {
                _criticalEventPublished = false;
            }

            if (!IsEmpty)
            {
                _emptyEventPublished = false;
            }

            GameEventBus.Publish(new HungerChangedEvent(delta, CurrentHunger, MaxHunger));
        }

        public void RestoreFromSaveData(int currentHunger, int maxHunger)
        {
            var previousHunger = CurrentHunger;
            MaxHunger = Mathf.Max(1, maxHunger);
            CurrentHunger = Mathf.Clamp(currentHunger, 0, MaxHunger);
            _distanceAccumulator = 0f;
            _criticalEventPublished = IsCritical;
            _emptyEventPublished = IsEmpty;

            GameEventBus.Publish(new HungerChangedEvent(CurrentHunger - previousHunger, CurrentHunger, MaxHunger));
        }

        private void Awake()
        {
            Initialize(_playerData);
        }

        private void OnValidate()
        {
            _hpLossWhenHungerEmpty = Mathf.Max(0, _hpLossWhenHungerEmpty);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerStepEvent>(OnPlayerStep);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<GameTimeTickEvent>(OnGameTimeTick);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerStepEvent>(OnPlayerStep);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<GameTimeTickEvent>(OnGameTimeTick);
        }

        private void OnPlayerStep(PlayerStepEvent evt)
        {
            if (IsEmpty)
            {
                return;
            }

            _distanceAccumulator += Mathf.Max(0f, evt.DistanceSinceLastStep);
            while (_distanceAccumulator >= _stepsPerHungerTick)
            {
                _distanceAccumulator -= _stepsPerHungerTick;
                LoseHunger(_hungerLossPerTick);

                if (IsEmpty)
                {
                    _distanceAccumulator = 0f;
                    return;
                }
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (IsEmpty || _hungerLossPerDay <= 0)
            {
                return;
            }

            LoseHunger(_hungerLossPerDay);
        }

        private void OnGameTimeTick(GameTimeTickEvent evt)
        {
            if (!IsEmpty)
            {
                return;
            }

            var starvationDamage = _playerNeedsBalance != null
                ? Mathf.CeilToInt(Mathf.Max(0f, _playerNeedsBalance.ZeroHungerDamagePerSecond))
                : 1;
            ApplyHungerDamage(starvationDamage, "Starvation tick");
        }

        // F18: redução de drain derivada (passivas/equipment). 1 = sem redução; clamp 0.25.
        public float DrainMultiplier { get; set; } = 1f;

        private void LoseHunger(int amount)
        {
            if (amount <= 0 || CurrentHunger <= 0)
            {
                return;
            }

            // F18: aplica a redução derivada preservando drain mínimo de 1 por tick.
            if (DrainMultiplier < 1f)
            {
                amount = Mathf.Max(1, Mathf.CeilToInt(amount * Mathf.Clamp(DrainMultiplier, 0.25f, 1f)));
            }

            var previousHunger = CurrentHunger;
            CurrentHunger = Mathf.Max(0, CurrentHunger - amount);
            var delta = CurrentHunger - previousHunger;
            if (delta == 0)
            {
                return;
            }

            GameEventBus.Publish(new HungerChangedEvent(delta, CurrentHunger, MaxHunger));

            if (IsCritical && !_criticalEventPublished)
            {
                _criticalEventPublished = true;
                GameEventBus.Publish(new HungerCriticalEvent(CurrentHunger, MaxHunger));
            }

            if (IsEmpty && !_emptyEventPublished)
            {
                _emptyEventPublished = true;
                GameEventBus.Publish(new HungerEmptyEvent(CurrentHunger, MaxHunger));
                ApplyHungerDamage(Mathf.Max(0, _hpLossWhenHungerEmpty), "Hunger reached zero");
            }
        }

        private void ApplyHungerDamage(int damage, string reason)
        {
            if (_playerManager == null)
            {
                Debug.LogWarning("HungerManager reached empty hunger without PlayerManager reference. HP consequence was skipped.", this);
                return;
            }

            if (damage <= 0)
            {
                return;
            }

            _playerManager.DamageHP(damage);
            Debug.Log($"{reason}. Player lost {damage} HP.", this);

            if (_playerManager.CurrentHP > 0)
            {
                return;
            }

            _playerManager.SetHP(_playerManager.MaxHP);
            RestoreFromSaveData(GetRespawnHunger(), MaxHunger);

            if (_playerTransform != null)
            {
                _playerTransform.position = _respawnPosition;
            }

            GameEventBus.Publish(new PlayerRespawnedEvent(_respawnPosition, _playerManager.CurrentHP, 0));
            Debug.Log("Player respawned after reaching 0 HP from hunger.", this);
        }

        private int GetRespawnHunger()
        {
            return Mathf.Max(1, Mathf.CeilToInt(MaxHunger * 0.2f));
        }

        private int GetCriticalThreshold()
        {
            return Mathf.Max(1, Mathf.CeilToInt(MaxHunger * 0.2f));
        }
    }
}
