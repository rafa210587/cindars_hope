using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Progression
{
    [DisallowMultipleComponent]
    public class PlayerProgressionManager : MonoBehaviour
    {
        [SerializeField] private PlayerProgressionSaveData _state = new PlayerProgressionSaveData();

        public int Level => _state.Level;
        public int CurrentXp => _state.CurrentXp;
        public int XpToNextLevel => _state.XpToNextLevel;
        public int UnspentAttributePoints => _state.UnspentAttributePoints;
        public int UnspentSkillPoints => _state.UnspentSkillPoints;
        public int Strength => _state.Strength;
        public int Dexterity => _state.Dexterity;
        public int Intelligence => _state.Intelligence;
        public int Willpower => _state.Willpower;
        public int Constitution => _state.Constitution;
        public int Breath => _state.Breath;

        private void Awake()
        {
            NormalizeState();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        public void AddXp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            NormalizeState();
            _state.CurrentXp += amount;
            GameEventBus.Publish(new PlayerXpChangedEvent(amount, _state.CurrentXp, _state.XpToNextLevel, _state.Level));

            while (_state.Level < PlayerProgressionRules.MaxLevel && _state.CurrentXp >= _state.XpToNextLevel)
            {
                int oldLevel = _state.Level;
                _state.CurrentXp -= _state.XpToNextLevel;
                _state.Level++;
                int grantedAttributePoints = PlayerProgressionRules.CalculateAttributePointsGrantedOnLevelUp(_state.Level);
                int grantedSkillPoints = PlayerProgressionRules.CalculateSkillPointsGrantedOnLevelUp(_state.Level);
                _state.UnspentAttributePoints += grantedAttributePoints;
                _state.UnspentSkillPoints += grantedSkillPoints;
                _state.XpToNextLevel = PlayerProgressionRules.CalculateXpToNextLevel(_state.Level);
                GameEventBus.Publish(new PlayerLevelChangedEvent(oldLevel, _state.Level, grantedAttributePoints, grantedSkillPoints));
            }
        }

        public PlayerProgressionSaveData CaptureSaveData()
        {
            NormalizeState();
            return new PlayerProgressionSaveData
            {
                Level = _state.Level,
                CurrentXp = _state.CurrentXp,
                XpToNextLevel = _state.XpToNextLevel,
                UnspentAttributePoints = _state.UnspentAttributePoints,
                UnspentSkillPoints = _state.UnspentSkillPoints,
                Strength = _state.Strength,
                Dexterity = _state.Dexterity,
                Intelligence = _state.Intelligence,
                Willpower = _state.Willpower,
                Constitution = _state.Constitution,
                Breath = _state.Breath
            };
        }

        public void RestoreFromSaveData(PlayerProgressionSaveData saveData)
        {
            _state = saveData ?? new PlayerProgressionSaveData();
            NormalizeState();
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (evt.XpReward > 0)
            {
                AddXp(evt.XpReward);
            }
        }

        private void NormalizeState()
        {
            if (_state == null)
            {
                _state = new PlayerProgressionSaveData();
            }

            _state.Level = Mathf.Clamp(_state.Level, 1, PlayerProgressionRules.MaxLevel);
            _state.CurrentXp = Mathf.Max(0, _state.CurrentXp);
            _state.XpToNextLevel = Mathf.Max(1, _state.XpToNextLevel);
            _state.UnspentAttributePoints = Mathf.Max(0, _state.UnspentAttributePoints);
            _state.UnspentSkillPoints = Mathf.Max(0, _state.UnspentSkillPoints);
            _state.Strength = Mathf.Max(1, _state.Strength);
            _state.Dexterity = Mathf.Max(1, _state.Dexterity);
            _state.Intelligence = Mathf.Max(1, _state.Intelligence);
            _state.Willpower = Mathf.Max(1, _state.Willpower);
            _state.Constitution = Mathf.Max(1, _state.Constitution);
            _state.Breath = Mathf.Max(1, _state.Breath);
        }
    }
}