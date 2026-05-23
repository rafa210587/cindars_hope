using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat.StatusEffect
{
    [CreateAssetMenu(fileName = "StatusEffect_", menuName = "CindarsHope/Combat/StatusEffect")]
    public class StatusEffectSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public StatusEffectType Type;
        public int DurationTurns;
        public int DamagePerTurn;
        public Color VisualColor = Color.white;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            DurationTurns = Mathf.Max(0, DurationTurns);
            DamagePerTurn = Mathf.Max(0, DamagePerTurn);
        }
    }

    public enum StatusEffectType
    {
        None,
        Poison,
        Burn,
        Bleed,
        Stun,
        Weakness,
        Vulnerable
    }
}
