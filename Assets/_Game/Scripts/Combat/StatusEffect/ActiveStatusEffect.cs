using System;

namespace CindarsHope.Combat.StatusEffect
{
    [Serializable]
    public class ActiveStatusEffect
    {
        public string StatusEffectId;
        public string SourceId;
        public int RemainingTurns;

        public ActiveStatusEffect(string statusEffectId, int durationTurns, string sourceId = null)
        {
            StatusEffectId = statusEffectId;
            SourceId = sourceId ?? string.Empty;
            RemainingTurns = durationTurns;
        }

        public void Tick()
        {
            RemainingTurns = UnityEngine.Mathf.Max(0, RemainingTurns - 1);
        }

        public bool IsActive => RemainingTurns > 0;
    }
}
