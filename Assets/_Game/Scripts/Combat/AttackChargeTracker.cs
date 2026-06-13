using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>Peso do golpe (F02). Multiplicadores canônicos EQUIPMENT_MECHANICAL_BASELINES §6.</summary>
    public enum AttackWeight
    {
        Light = 0,
        Heavy = 1,
        ChargedShort = 2,
        ChargedLong = 3
    }

    /// <summary>Regras puras de carga/peso (testáveis sem Unity).</summary>
    public static class AttackChargeRules
    {
        public const float HeavyThresholdSeconds = 0.25f;
        public const float ChargedThresholdSeconds = 0.9f;
        public const float ChargedLongThresholdSeconds = 1.5f;

        public static AttackWeight ResolveWeight(float holdSeconds)
        {
            if (holdSeconds >= ChargedLongThresholdSeconds) return AttackWeight.ChargedLong;
            if (holdSeconds >= ChargedThresholdSeconds) return AttackWeight.ChargedShort;
            if (holdSeconds >= HeavyThresholdSeconds) return AttackWeight.Heavy;
            return AttackWeight.Light;
        }

        // Canônicos: light ×1.00 · heavy ×1.45 · charged curto ×1.65 · charged longo ×1.90.
        public static float DamageMultiplier(AttackWeight weight)
        {
            switch (weight)
            {
                case AttackWeight.Heavy: return 1.45f;
                case AttackWeight.ChargedShort: return 1.65f;
                case AttackWeight.ChargedLong: return 1.90f;
                default: return 1f;
            }
        }

        // Posture: ×1.00 · ×1.60 · ×1.80 · ×2.20.
        public static float PostureMultiplier(AttackWeight weight)
        {
            switch (weight)
            {
                case AttackWeight.Heavy: return 1.60f;
                case AttackWeight.ChargedShort: return 1.80f;
                case AttackWeight.ChargedLong: return 2.20f;
                default: return 1f;
            }
        }

        // Stamina: razões da matriz canônica (sword 25/40/48 → ×1.0/×1.6/×1.92).
        // F03 substitui por custos POR ARMA (BaseLight/Heavy/ChargedStaminaCost).
        public static float StaminaMultiplier(AttackWeight weight)
        {
            switch (weight)
            {
                case AttackWeight.Heavy: return 1.6f;
                case AttackWeight.ChargedShort: return 1.9f;
                case AttackWeight.ChargedLong: return 2.2f;
                default: return 1f;
            }
        }
    }

    /// <summary>Rastreador de hold por tecla (puro — tempos injetados).</summary>
    public class AttackChargeTracker
    {
        private float _beginTime = -1f;

        public bool IsCharging => _beginTime >= 0f;
        public float BeginTime => _beginTime;

        public void Begin(float time)
        {
            _beginTime = time;
        }

        public float HoldSeconds(float now)
        {
            return IsCharging ? Mathf.Max(0f, now - _beginTime) : 0f;
        }

        public AttackWeight Release(float now)
        {
            var weight = AttackChargeRules.ResolveWeight(HoldSeconds(now));
            _beginTime = -1f;
            return weight;
        }

        public void Cancel()
        {
            _beginTime = -1f;
        }
    }
}
