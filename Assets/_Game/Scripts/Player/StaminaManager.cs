using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class StaminaManager : MonoBehaviour
    {
        [SerializeField] private int _maxStamina = 100;
        private int _currentStamina;
        private float _regenRate = 10f;
        private float _regenDelay = 1f;
        private float _regenTimer = 0f;

        public int CurrentStamina => _currentStamina;
        public int MaxStamina => _maxStamina;
        public float StaminaPercent => _maxStamina > 0 ? (float)_currentStamina / _maxStamina : 0f;
        public bool IsInitialized { get; private set; }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
        }

        public void Initialize(int maxStamina = 100, int startingStamina = 100)
        {
            _maxStamina = Mathf.Max(1, maxStamina);
            _currentStamina = Mathf.Min(startingStamina, _maxStamina);
            _regenTimer = _regenDelay;
            IsInitialized = true;
        }

        private void Update()
        {
            if (!IsInitialized)
                return;

            _regenTimer -= Time.deltaTime;
            if (_regenTimer <= 0f && _currentStamina < _maxStamina)
            {
                _regenTimer = 1f / _regenRate;
                AddStamina(1);
            }
        }

        public bool TrySpendStamina(int amount)
        {
            if (amount <= 0)
                return true;

            if (_currentStamina < amount)
                return false;

            _currentStamina -= amount;
            PublishStaminaChanged();
            return true;
        }

        public void AddStamina(int amount)
        {
            if (amount <= 0)
                return;

            _currentStamina = Mathf.Min(_currentStamina + amount, _maxStamina);
            PublishStaminaChanged();
        }

        public void FullRecover()
        {
            _currentStamina = _maxStamina;
            PublishStaminaChanged();
        }

        private void PublishStaminaChanged()
        {
            GameEventBus.Publish(new StaminaChangedEvent(_currentStamina, _maxStamina));
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            FullRecover();
        }
    }
}
