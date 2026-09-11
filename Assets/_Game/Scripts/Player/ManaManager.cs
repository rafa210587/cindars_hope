using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Player
{
    // arch: quebra do par mutuo Core|Player (2026-07-15) — implementa IManaRuntime (Foundation) para
    // que GameBootstrap resolva/chame Initialize() sem nomear CindarsHope.Player; o campo
    // GameBootstrap._manaManager virou MonoBehaviour, resolvido via cast local
    // (_manaManager as IManaRuntime).
    [DisallowMultipleComponent]
    public class ManaManager : MonoBehaviour, IManaRuntime
    {
        /// <summary>
        /// Valor canonico da regeneracao base de mana (MP/s). Fonte unica: o field serializado usa
        /// este default e o fixer de cena (ManaRegenSceneInitializer) aplica este mesmo valor as
        /// instancias do ManaManager nas cenas. Bonus de skill (ManaRegenFlat) entram via
        /// ExternalRegenBonus, somados a esta base.
        /// </summary>
        public const float DefaultManaRegenPerSecond = 2f;

        [SerializeField] private int _maxMana = 100;
        [SerializeField] private float _manaRegenPerSecond = DefaultManaRegenPerSecond;
        [SerializeField] private MonoBehaviour _modalManager;

        private int _currentMana;
        private float _regenAccumulator = 0f;
        public bool IsInitialized { get; private set; }

        public int MaxMana => _maxMana;
        public int CurrentMana => _currentMana;
        public float ManaPercent => MaxMana > 0 ? (float)_currentMana / MaxMana : 0f;
        public float BaseRegenPerSecond => Mathf.Max(0f, _manaRegenPerSecond);

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameTimeTickEvent>(HandleGameTimeTick);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameTimeTickEvent>(HandleGameTimeTick);
        }

        public void Initialize()
        {
            _currentMana = _maxMana;
            _regenAccumulator = 0f;
            IsInitialized = true;
        }

        public void Shutdown()
        {
            IsInitialized = false;
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

        public void RestoreMana(int amount)
        {
            if (amount <= 0)
                return;

            _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
            PublishManaChanged();
        }

        public void SetMana(int amount)
        {
            _currentMana = Mathf.Clamp(amount, 0, _maxMana);
            PublishManaChanged();
        }

        public void SetMaxMana(int maxMana)
        {
            _maxMana = Mathf.Max(1, maxMana);
            _currentMana = Mathf.Min(_currentMana, _maxMana);
        }

        // F18: bônus externo de regen (passivas derivadas).
        public float ExternalRegenBonus { get; set; }

        // F18: máximo derivado preservando a proporção corrente (floor 0 permitido p/ mana).
        public void SetMaxManaPreservingRatio(int newMaxMana)
        {
            newMaxMana = Mathf.Max(1, newMaxMana);
            if (newMaxMana == _maxMana)
            {
                return;
            }

            var ratio = _maxMana > 0 ? (float)_currentMana / _maxMana : 1f;
            _maxMana = newMaxMana;
            _currentMana = Mathf.Clamp(Mathf.RoundToInt(newMaxMana * ratio), 0, newMaxMana);
            PublishManaChanged();
        }

        public ManaManagerSaveData CaptureSaveData()
        {
            return new ManaManagerSaveData { CurrentMana = _currentMana, MaxMana = _maxMana };
        }

        public void RestoreFromSaveData(ManaManagerSaveData data)
        {
            if (data == null)
            {
                Initialize();
                return;
            }

            _maxMana = Mathf.Max(1, data.MaxMana);
            _currentMana = Mathf.Min(data.CurrentMana, _maxMana);
            IsInitialized = true;
            PublishManaChanged();
        }

        private void HandleGameTimeTick(GameTimeTickEvent evt)
        {
            if (!IsInitialized || _currentMana >= _maxMana)
                return;

            if (_modalManager is IModalStateProvider modalStateProvider && modalStateProvider.HasActiveModal)
                return;

            var player = PlayerController.ActiveInstance;
            var world = player != null ? player.transform.position : transform.position;
            float naturalMultiplier = NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.ManaRegen, world.x, world.y);
            _regenAccumulator += (_manaRegenPerSecond + Mathf.Max(0f, ExternalRegenBonus))
                * naturalMultiplier;
            if (_regenAccumulator >= 1f)
            {
                int manaGain = Mathf.FloorToInt(_regenAccumulator);
                _regenAccumulator -= manaGain;
                RestoreMana(manaGain);
            }
        }

        private void PublishManaChanged()
        {
            GameEventBus.Publish(new ManaChangedEvent(_currentMana, _maxMana));
        }
    }

    public class ManaManagerSaveData
    {
        public int CurrentMana;
        public int MaxMana;
    }
}
