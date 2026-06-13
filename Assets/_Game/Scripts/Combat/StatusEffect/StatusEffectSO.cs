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

        // F01: campos canônicos com defaults NEUTROS (assets antigos intactos).
        [Header("Canonical Semantics (F01)")]
        [Range(0f, 2f)] public float MoveSpeedMultiplier = 1f;
        public float BehaviorOverrideSeconds = 0f;
        [Range(1f, 5f)] public float DurabilityWearMultiplier = 1f;

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
        Vulnerable,
        // F01: conjunto canônico completo (COMBAT_CORE §31 / STATUS_EFFECTS_DIRECTION).
        // Hunger/Fatigue ficam nos sistemas próprios (HungerManager/F16) — fora do ticker.
        Chill,
        Root,
        Fear,
        ConfusionLite,
        DurabilityStress,
        Corruption,
        Slow,
        HeatStress,
        ColdStress
    }

    /// <summary>Semântica central por tipo (F01) — pura e testável.</summary>
    public static class StatusEffectSemantics
    {
        public static bool IsDamageOverTime(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Poison:
                case StatusEffectType.Burn:
                case StatusEffectType.Bleed:
                case StatusEffectType.Corruption:
                case StatusEffectType.HeatStress:
                case StatusEffectType.ColdStress:
                    return true;
                default:
                    return false;
            }
        }

        public static DamageType GetDamageType(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Burn:
                case StatusEffectType.HeatStress:
                    return DamageType.Fire;
                case StatusEffectType.Poison:
                case StatusEffectType.Corruption:
                    return DamageType.Toxic;
                case StatusEffectType.ColdStress:
                    return DamageType.Ice;
                default:
                    return DamageType.Physical;
            }
        }

        /// <summary>Multiplicador de velocidade efetivo (Root/Stun = 0; Chill/Slow/ColdStress = campo do asset).</summary>
        public static float GetMoveSpeedFactor(StatusEffectSO effect)
        {
            if (effect == null)
            {
                return 1f;
            }

            switch (effect.Type)
            {
                case StatusEffectType.Root:
                case StatusEffectType.Stun:
                    return 0f;
                case StatusEffectType.Chill:
                case StatusEffectType.Slow:
                case StatusEffectType.ColdStress:
                    return Mathf.Clamp(effect.MoveSpeedMultiplier, 0f, 1f);
                default:
                    return 1f;
            }
        }

        public static bool ForcesRetreat(StatusEffectType type) => type == StatusEffectType.Fear;
        public static bool InvertsMovement(StatusEffectType type) => type == StatusEffectType.ConfusionLite;
        public static bool BlocksPlayerAction(StatusEffectType type) => type == StatusEffectType.Stun;
    }
}
