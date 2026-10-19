using System;

namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Enemy — logica pura de Rise-once (spec_enemy_attack_kits_v1,
    /// primitiva P2) extraida de CindarsHope.Enemy.EnemyActionExecution para Foundation, para que
    /// Combat/EnemyHealth.cs decida o reerguimento sem nomear CindarsHope.Enemy. Comportamento e
    /// assinaturas identicos ao original; engine-free (Mathf substituido por Math puro).
    /// </summary>
    public static class EnemyRiseOnceRules
    {
        /// <summary>
        /// Verifica se o dano recebido bloqueia o reerguimento (ex.: fire/radiant). Comparacao
        /// case-insensitive; lista vazia/nula = nunca bloqueia.
        /// </summary>
        public static bool ShouldBlockRise(string lastDamageType, string[] blockedTypes)
        {
            if (blockedTypes == null || blockedTypes.Length == 0 || string.IsNullOrWhiteSpace(lastDamageType))
                return false;

            for (int i = 0; i < blockedTypes.Length; i++)
            {
                if (string.Equals(blockedTypes[i], lastDamageType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Decide se a morte deve ser interceptada pelo Rise-once: habilitado, ainda nao consumido,
        /// e o ultimo dano nao e de um elemento bloqueador.
        /// </summary>
        public static bool ShouldRiseOnce(bool riseOnceEnabled, bool alreadyConsumed, string lastDamageType, string[] blockedTypes)
        {
            if (!riseOnceEnabled || alreadyConsumed) return false;
            return !ShouldBlockRise(lastDamageType, blockedTypes);
        }

        /// <summary>
        /// Calcula o HP restaurado ao reerguer (fracao de maxHp, minimo 1 para nao reerguer morto).
        /// </summary>
        public static int ResolveRiseHp(int maxHp, float riseOnceHpPercent)
        {
            int clampedMaxHp = Math.Max(0, maxHp);
            float clampedPercent = riseOnceHpPercent < 0f ? 0f : (riseOnceHpPercent > 1f ? 1f : riseOnceHpPercent);
            int hp = (int)Math.Round(clampedMaxHp * clampedPercent, MidpointRounding.ToEven);
            return Math.Max(1, hp);
        }
    }
}
