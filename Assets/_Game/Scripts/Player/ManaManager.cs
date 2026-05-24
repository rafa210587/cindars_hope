using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class ManaManager : MonoBehaviour
    {
        [SerializeField] private int _maxMana = 100;
        private int _currentMana;
        private float _regenRate = 5f;

        public int CurrentMana => _currentMana;
        public int MaxMana => _maxMana;
        public float ManaPercent => _maxMana > 0 ? (float)_currentMana / _maxMana : 0f;
        public bool IsInitialized { get; private set; }

        public void Initialize(int maxMana = 100, int startingMana = 100)
        {
            _maxMana = Mathf.Max(1, maxMana);
            _currentMana = Mathf.Min(startingMana, _maxMana);
            IsInitialized = true;
        }

        private void Update()
        {
            if (!IsInitialized || _currentMana >= _maxMana)
                return;

            _currentMana = Mathf.Min(_currentMana + (int)(_regenRate * Time.deltaTime), _maxMana);
            PublishManaChanged();
        }

        public bool TrySpendMana(int amount)
        {
            if (amount <= 0)
                return true;

            if (_currentMana < amount)
                return false;

            _currentMana -= amount;
            PublishManaChanged();
            return true;
        }

        public void AddMana(int amount)
        {
            if (amount <= 0)
                return;

            _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
            PublishManaChanged();
        }

        public void FullRecover()
        {
            _currentMana = _maxMana;
            PublishManaChanged();
        }

        private void PublishManaChanged()
        {
            GameEventBus.Publish(new ManaChangedEvent(_currentMana, _maxMana));
        }
    }
}
