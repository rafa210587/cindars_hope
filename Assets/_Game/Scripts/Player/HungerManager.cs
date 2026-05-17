using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class HungerManager : MonoBehaviour
    {
        [SerializeField] private PlayerDataSO _playerData;

        private int _stepsPerHungerTick = 10;
        private int _hungerLossPerTick = 1;
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
                return;
            }

            MaxHunger = Mathf.Max(1, _playerData.MaxHunger);
            CurrentHunger = Mathf.Clamp(_playerData.StartingHunger, 0, MaxHunger);
            _stepsPerHungerTick = Mathf.Max(1, _playerData.StepsPerHungerTick);
            _hungerLossPerTick = Mathf.Max(1, _playerData.HungerLossPerTick);
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

        private void Awake()
        {
            Initialize(_playerData);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerStepEvent>(OnPlayerStep);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerStepEvent>(OnPlayerStep);
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

        private void LoseHunger(int amount)
        {
            if (amount <= 0 || CurrentHunger <= 0)
            {
                return;
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
            }
        }

        private int GetCriticalThreshold()
        {
            return Mathf.Max(1, Mathf.CeilToInt(MaxHunger * 0.2f));
        }
    }
}
