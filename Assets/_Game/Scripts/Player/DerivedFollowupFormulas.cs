using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// fable_47 — fórmulas puras (testáveis) dos 3 follow-ups da F18.
    /// Constantes nomeadas centralizadas; sem Unity scene/state. NÃO altera nenhuma fórmula
    /// do DerivedStatsCalculator (F18) — apenas converte as saídas dele em efeito mensurável.
    /// </summary>
    public static class DerivedFollowupFormulas
    {
        // --- Duração de status por resistência -----------------------------------------
        /// <summary>Cada ponto de resistência encurta a duração em 2%.</summary>
        public const float StatusDurationReductionPerResist = 0.02f;
        /// <summary>Redução máxima de duração por resistência (50%).</summary>
        public const float StatusDurationReductionCap = 0.5f;

        // --- Craft time reduction --------------------------------------------------------
        /// <summary>Redução máxima de tempo de craft (75%) — nunca instantâneo por bônus.</summary>
        public const float CraftTimeReductionCap = 0.75f;

        // --- Repair efficiency -----------------------------------------------------------
        /// <summary>Bônus máximo de eficiência de reparo (+200% de durabilidade restaurada).</summary>
        public const float RepairEfficiencyBonusCap = 2f;

        // --- Derived move speed (follow-up 1) -------------------------------------------
        /// <summary>
        /// Converte o bônus aditivo de MoveSpeed (F18, base 0) num FATOR multiplicativo para o
        /// composer: fator = 1 + bonus/base. Mantém a velocidade base no caminho próprio do
        /// PlayerController; o fator só representa o delta derivado. base &lt;= 0 → fator 1.
        /// </summary>
        public static float DerivedMoveSpeedFactor(float derivedMoveSpeedBonus, float baseMoveSpeed)
        {
            if (baseMoveSpeed <= 0f)
            {
                return 1f;
            }

            var factor = 1f + derivedMoveSpeedBonus / baseMoveSpeed;
            return factor < 0f ? 0f : factor;
        }

        /// <summary>
        /// Duração final do status = base × (1 − min(cap, resist × 0.02)).
        /// resist negativo é tratado como 0 (sem aumento de duração).
        /// </summary>
        public static float StatusDurationMultiplier(int resistance)
        {
            if (resistance <= 0)
            {
                return 1f;
            }

            var reduction = Mathf.Min(StatusDurationReductionCap, resistance * StatusDurationReductionPerResist);
            return 1f - reduction;
        }

        /// <summary>
        /// Aplica a redução à duração base e re-clampa em [minClamp, maxClamp] (preserva o clamp
        /// 1–30s da F01). minClamp só é aplicado quando a base original já estava acima dele.
        /// </summary>
        public static float ApplyStatusDurationReduction(float baseSeconds, int resistance, float minClamp, float maxClamp)
        {
            var reduced = baseSeconds * StatusDurationMultiplier(resistance);
            // Não deixa a redução por resistência levar abaixo do floor canônico (1s).
            var floor = Mathf.Min(baseSeconds, minClamp);
            return Mathf.Clamp(reduced, floor, maxClamp);
        }

        /// <summary>
        /// Multiplicador de tempo de craft = 1 − clamp(reduction, 0, cap). Resultado em (0,1].
        /// </summary>
        public static float CraftTimeMultiplier(float craftTimeReduction)
        {
            var clamped = Mathf.Clamp(craftTimeReduction, 0f, CraftTimeReductionCap);
            return 1f - clamped;
        }

        /// <summary>Duração efetiva de craft = base × multiplicador (nunca negativa).</summary>
        public static float EffectiveCraftSeconds(float baseSeconds, float craftTimeReduction)
        {
            return Mathf.Max(0f, baseSeconds * CraftTimeMultiplier(craftTimeReduction));
        }

        /// <summary>
        /// Durabilidade restaurada efetiva = round(base × (1 + clamp(bonus, 0, cap))).
        /// Garante pelo menos o valor base quando bonus &gt; 0 e base &gt; 0.
        /// </summary>
        public static int EffectiveRepairAmount(int baseRestore, float repairEfficiencyBonus)
        {
            if (baseRestore <= 0)
            {
                return baseRestore;
            }

            var bonus = Mathf.Clamp(repairEfficiencyBonus, 0f, RepairEfficiencyBonusCap);
            return Mathf.Max(baseRestore, Mathf.RoundToInt(baseRestore * (1f + bonus)));
        }
    }
}
