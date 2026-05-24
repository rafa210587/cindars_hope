using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class ManaManager : MonoBehaviour
    {
        [SerializeField] private int _maxMana = 100;
        [SerializeField] private float _manaRegenPerSecond = 5f;

        private int _currentMana;

        public int MaxMana => _maxMana;
        public int CurrentMana => _currentMana;
        public float ManaPercent => MaxMana > 0 ? (float)_currentMana / MaxMana : 0f;

        private void Start()
        {
            _currentMana = _maxMana;
        }

        private void Update()
        {
            if (_currentMana < _maxMana)
            {
                _currentMana = Mathf.Min(_currentMana + Mathf.RoundToInt(_manaRegenPerSecond * UnityEngine.Time.deltaTime), _maxMana);
            }
        }

        public bool TrySpendMana(int amount)
        {
            if (_currentMana >= amount)
            {
                _currentMana -= amount;
                return true;
            }

            return false;
        }

        public void RestoreMana(int amount)
        {
            _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
        }

        public void SetMana(int amount)
        {
            _currentMana = Mathf.Clamp(amount, 0, _maxMana);
        }

        public void SetMaxMana(int maxMana)
        {
            _maxMana = Mathf.Max(1, maxMana);
            _currentMana = Mathf.Min(_currentMana, _maxMana);
        }
    }
}
