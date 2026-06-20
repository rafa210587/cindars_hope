using CindarsHope.Cave.Generation;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — resolução DETERMINÍSTICA da chance de desarme por tier de ferramenta (CA-3).
    ///
    /// Pure C# (testável fora do Unity). Reusa o <see cref="ToolTier"/> canônico do projeto
    /// (None/Basic/Copper/Iron/Gold/Diamond) — NÃO cria um conceito novo de tier. Régua auditada na
    /// Fase 0 e documentada: base 50% no tier mínimo (Basic) + 15% por tier acima, com teto 95%
    /// (nunca 100% garantido — risco/recompensa §22). Tier None (mãos vazias) = chance 0.
    ///
    /// O resultado é determinístico por um seed de tentativa derivado de
    /// StableHash(runSeed|level|trapInstanceId|toolTier) — duas tentativas no mesmo estado dão o mesmo
    /// resultado (sem drift entre máquinas, sem UnityEngine.Random).
    /// </summary>
    public static class TrapDisarmResolver
    {
        public const float BaseChance = 0.50f;
        public const float ChancePerTier = 0.15f;
        public const float MaxChance = 0.95f;

        /// <summary>
        /// Chance de desarme [0,1] para um tier de ferramenta. Basic = 0.50; Copper = 0.65; Iron =
        /// 0.80; Gold = 0.95 (teto); Diamond = 0.95 (teto); None = 0.
        /// </summary>
        public static float ChanceForTier(ToolTier toolTier)
        {
            if (toolTier <= ToolTier.None)
            {
                return 0f;
            }

            var stepsAboveBasic = (int)toolTier - (int)ToolTier.Basic; // Basic → 0
            var chance = BaseChance + ChancePerTier * stepsAboveBasic;
            return Mathf.Clamp(chance, 0f, MaxChance);
        }

        /// <summary>
        /// Resolve uma tentativa de desarme DETERMINÍSTICA. true = sucesso (Disarmed); false = falha
        /// (a armadilha dispara). O seed de tentativa é estável por
        /// (runSeed, level, trapInstanceId, toolTier), então o desfecho é reproduzível na revisita e
        /// idêntico entre máquinas.
        /// </summary>
        public static bool TryDisarm(ToolTier toolTier, string caveRunSeed, int caveLevel, string trapInstanceId)
        {
            var chance = ChanceForTier(toolTier);
            if (chance <= 0f)
            {
                return false;
            }

            var roll = Roll01(toolTier, caveRunSeed, caveLevel, trapInstanceId);
            return roll < chance;
        }

        /// <summary>
        /// Valor sorteado [0,1) determinístico para a tentativa (ponto único — usado por
        /// <see cref="TryDisarm"/> e pelos testes de borda: roll &lt; chance = sucesso; ≥ chance = falha).
        /// </summary>
        public static float Roll01(ToolTier toolTier, string caveRunSeed, int caveLevel, string trapInstanceId)
        {
            var hash = CaveLayoutStableHash.Compute($"{caveRunSeed}|{caveLevel}|{trapInstanceId}|disarm|{(int)toolTier}");
            // Mapeia para [0,1): magnitude positiva sobre o espaço de uint.
            var unsigned = unchecked((uint)hash);
            return unsigned / (float)uint.MaxValue;
        }
    }
}
