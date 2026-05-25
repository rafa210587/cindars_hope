using UnityEngine;

namespace CindarsHope.Core.Data
{
    [CreateAssetMenu(fileName = "PlayerNeedsBalance", menuName = "CindarsHope/Data/PlayerNeedsBalance")]
    public class PlayerNeedsBalanceSO : ScriptableObject
    {
        [Header("Stamina Regen Rates")]
        [SerializeField] private float _baseStaminaRegenRate = 15f;

        [Header("Hunger Ranges and Stamina Regen Modifiers")]
        [SerializeField] private int _sufficientHungerMin = 70;
        [SerializeField] private float _sufficientHungerRegenModifier = 1.0f;

        [SerializeField] private int _adequateHungerMin = 30;
        [SerializeField] private float _adequateHungerRegenModifier = 1.0f;

        [SerializeField] private int _famineWarnHungerMin = 10;
        [SerializeField] private float _famineWarnHungerRegenModifier = 0.6f;

        [SerializeField] private int _famineCriticalHungerMin = 1;
        [SerializeField] private float _famineCriticalHungerRegenModifier = 0.3f;
        [SerializeField] private float _famineCriticalMoveSpeedModifier = 0.85f;

        [SerializeField] private float _zeroHungerRegenRate = 2f;
        [SerializeField] private float _zeroHungerDamagePerSecond = 1f;

        public float BaseStaminaRegenRate => _baseStaminaRegenRate;

        public int SufficientHungerMin => _sufficientHungerMin;
        public float SufficientHungerRegenModifier => _sufficientHungerRegenModifier;

        public int AdequateHungerMin => _adequateHungerMin;
        public float AdequateHungerRegenModifier => _adequateHungerRegenModifier;

        public int FamineWarnHungerMin => _famineWarnHungerMin;
        public float FamineWarnHungerRegenModifier => _famineWarnHungerRegenModifier;

        public int FamineCriticalHungerMin => _famineCriticalHungerMin;
        public float FamineCriticalHungerRegenModifier => _famineCriticalHungerRegenModifier;
        public float FamineCriticalMoveSpeedModifier => _famineCriticalMoveSpeedModifier;

        public float ZeroHungerRegenRate => _zeroHungerRegenRate;
        public float ZeroHungerDamagePerSecond => _zeroHungerDamagePerSecond;

        public float GetStaminaRegenModifier(int currentHunger)
        {
            if (currentHunger >= SufficientHungerMin)
                return SufficientHungerRegenModifier;

            if (currentHunger >= AdequateHungerMin)
                return AdequateHungerRegenModifier;

            if (currentHunger >= FamineWarnHungerMin)
                return FamineWarnHungerRegenModifier;

            if (currentHunger >= FamineCriticalHungerMin)
                return FamineCriticalHungerRegenModifier;

            return 0f;
        }

        public bool IsZeroHunger(int currentHunger) => currentHunger <= 0;

        public float GetMoveSpeedModifier(int currentHunger)
        {
            return currentHunger >= FamineCriticalHungerMin && currentHunger < FamineWarnHungerMin
                ? FamineCriticalMoveSpeedModifier
                : 1f;
        }
    }
}
