using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class StaminaManager : MonoBehaviour, CindarsHope.Foundation.IStaminaRuntime
    {
        [SerializeField] private int _maxStamina = 100;
        [SerializeField] private PlayerNeedsBalanceSO _playerNeedsBalance;
        [SerializeField] private HungerManager _hungerManager;

        private int _currentStamina;
        private float _regenRate = 15f;
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

        // arch: quebra do par mutuo Core|Player (2026-07-15) — implementacoes explicitas da porta
        // IStaminaRuntime: o campo GameBootstrap._staminaManager virou MonoBehaviour (Core nao pode
        // mais nomear StaminaManager); Core resolve esta porta via cast local
        // (_staminaManager as IStaminaRuntime) so para as chamadas de lifecycle. Initialize(int,int)
        // com defaults nao satisfaz IStaminaRuntime.Initialize() por assinatura (arity diferente),
        // entao o wrapper explicito chama a sobrecarga default.
        void CindarsHope.Foundation.IStaminaRuntime.Initialize() => Initialize();

        public void Initialize(int maxStamina = 100, int startingStamina = 100)
        {
            _maxStamina = Mathf.Max(1, maxStamina);
            _currentStamina = Mathf.Clamp(startingStamina, 0, _maxStamina);
            _regenRate = _playerNeedsBalance != null
                ? Mathf.Max(0f, _playerNeedsBalance.BaseStaminaRegenRate)
                : 15f;
            _regenTimer = _regenDelay;
            IsInitialized = true;
            PublishStaminaChanged();
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        private void Update()
        {
            if (!IsInitialized)
                return;

            _regenTimer -= Time.deltaTime;
            if (_regenTimer <= 0f && _currentStamina < _maxStamina)
            {
                float effectiveRegenRate = GetEffectiveRegenRate();
                if (effectiveRegenRate > 0)
                {
                    _regenTimer = 1f / effectiveRegenRate;
                    AddStamina(1);
                }
            }
        }

        public bool TrySpendStamina(int amount)
        {
            if (amount <= 0)
                return true;

            if (_currentStamina < amount)
                return false;

            _currentStamina -= amount;
            _regenTimer = _regenDelay;
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

        // F18: bônus externo de regen (passivas derivadas) somado à taxa efetiva.
        public float ExternalRegenBonus { get; set; }

        // F18: máximo derivado preservando a proporção corrente (floor 1).
        public void SetMaxStamina(int newMaxStamina, bool preserveRatio = true)
        {
            newMaxStamina = Mathf.Max(1, newMaxStamina);
            if (newMaxStamina == _maxStamina)
            {
                return;
            }

            var ratio = _maxStamina > 0 ? (float)_currentStamina / _maxStamina : 1f;
            _maxStamina = newMaxStamina;
            _currentStamina = preserveRatio
                ? Mathf.Max(1, Mathf.RoundToInt(newMaxStamina * ratio))
                : Mathf.Min(_currentStamina, newMaxStamina);
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

        private float GetEffectiveRegenRate()
        {
            if (_hungerManager == null || _playerNeedsBalance == null)
                return _regenRate + Mathf.Max(0f, ExternalRegenBonus);

            if (_playerNeedsBalance.IsZeroHunger(_hungerManager.CurrentHunger))
                return _playerNeedsBalance.ZeroHungerRegenRate;

            float modifier = _playerNeedsBalance.GetStaminaRegenModifier(_hungerManager.CurrentHunger);
            return _regenRate * modifier + Mathf.Max(0f, ExternalRegenBonus);
        }
    }
}
