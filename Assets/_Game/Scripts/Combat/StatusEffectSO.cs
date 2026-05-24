using UnityEngine;

namespace CindarsHope.Combat
{
    public enum StatusType
    {
        Burn,
        Poison,
        Bleed,
        Slow,
        Stun
    }

    public enum StatusRefreshPolicy
    {
        RefreshDurationNoPowerStack
    }

    [CreateAssetMenu(fileName = "StatusEffect", menuName = "CindarsHope/Combat/Status Effect")]
    public class StatusEffectSO : ScriptableObject
    {
        public string StatusId;
        public string DisplayName;
        [TextArea] public string Description;
        public StatusType StatusType;
        public DamageType DamageType = DamageType.Physical;
        public int Power = 10;
        public float DurationSeconds = 5f;
        public float TickIntervalSeconds = 1f;
        public float MoveSpeedMultiplier = 1f;
        public bool BlocksActions;
        public bool CanPersist = true;
        public StatusRefreshPolicy RefreshPolicy = StatusRefreshPolicy.RefreshDurationNoPowerStack;

        private void OnValidate()
        {
            Power = Mathf.Max(1, Power);
            DurationSeconds = Mathf.Max(0.1f, DurationSeconds);
            TickIntervalSeconds = Mathf.Max(0.1f, TickIntervalSeconds);
            MoveSpeedMultiplier = Mathf.Max(0f, MoveSpeedMultiplier);
        }
    }
}
