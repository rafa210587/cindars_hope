using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 (SLICE 4) — matemática PURA e testável do combate inter-monstro (seção 14.6).
    /// Sem dependência de cena/MonoBehaviour: dano reduzido entre monstros, defesa do alvo "Ferido"
    /// e quantidade de loot reduzida no corpo de uma kill monstro-vs-monstro. Todos os multiplicadores
    /// vêm do <see cref="CindarsHope.Cave.Data.CaveEcosystemBalanceSO"/> (rule no-magic-balance-values).
    /// </summary>
    public static class InterMonsterCombatMath
    {
        /// <summary>
        /// Dano inter-monstro = dano_normal × InterMonsterDamageMultiplier (default 0.10). Garante ao
        /// menos 1 de dano quando o dano de entrada era positivo (mesma regra de mínimo do DamageCalculator),
        /// para que o gancho tático ("Ferido") realmente dispare em vez de um no-op por arredondamento.
        /// </summary>
        public static int ScaleInterMonsterDamage(int baseDamage, float damageMultiplier)
        {
            if (baseDamage <= 0)
            {
                return 0;
            }

            var scaled = Mathf.RoundToInt(baseDamage * Mathf.Max(0f, damageMultiplier));
            return Mathf.Max(1, scaled);
        }

        /// <summary>
        /// Defesa efetiva do alvo enquanto "Ferido": defesa × WoundedDefenseMultiplier (default 0.85),
        /// truncada para inteiro não-negativo. Defesa menor → o alvo recebe mais dano (gancho tático
        /// que recompensa o jogador que intervém no conflito).
        /// </summary>
        public static int ApplyWoundedDefense(int baseDefense, float woundedDefenseMultiplier)
        {
            if (baseDefense <= 0)
            {
                return 0;
            }

            var scaled = Mathf.FloorToInt(baseDefense * Mathf.Clamp(woundedDefenseMultiplier, 0f, 1f));
            return Mathf.Max(0, scaled);
        }

        /// <summary>
        /// Quantidade de loot/XP reduzida no corpo de uma kill monstro-vs-monstro:
        /// amount × InterMonsterKillLootMultiplier (default 0.40). Nunca zera um drop que existia
        /// (mínimo 1 quando o original era positivo) — corpo saqueável reduzido, não vazio.
        /// </summary>
        public static int ScaleReducedLoot(int baseAmount, float lootMultiplier)
        {
            if (baseAmount <= 0)
            {
                return 0;
            }

            var scaled = Mathf.RoundToInt(baseAmount * Mathf.Max(0f, lootMultiplier));
            return Mathf.Max(1, scaled);
        }
    }
}
