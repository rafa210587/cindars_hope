using System;

namespace CindarsHope.Combat.StatusEffect
{
    [Serializable]
    public class ActiveStatusEffect
    {
        public string StatusEffectId;
        public int RemainingTurns;

        public ActiveStatusEffect(string statusEffectId, int durationTurns)
        {
            StatusEffectId = statusEffectId;
            RemainingTurns = durationTurns;
        }

        public void Tick()
        {
            RemainingTurns = UnityEngine.Mathf.Max(0, RemainingTurns - 1);
        }

        public bool IsActive => RemainingTurns > 0;
    }
}
