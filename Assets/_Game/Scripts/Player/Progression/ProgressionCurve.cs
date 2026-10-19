using UnityEngine;

namespace CindarsHope.Player.Progression
{
    /// <summary>
    /// F42 — curva canônica de progressão (BALANCE_CURVES v1.0):
    /// XPnext(N) = round(60 × N^1.5), cap 100, 1 skill point a cada 2 níveis (= 50 no cap).
    /// Fonte única de verdade: XP TOTAL acumulado; nível é DERIVADO.
    /// </summary>
    public static class ProgressionCurve
    {
        public const int MaxLevel = 100;

        /// <summary>XP necessário para ir do nível n ao n+1.</summary>
        public static int XpForNext(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Mathf.RoundToInt(60f * Mathf.Pow(level, 1.5f));
        }

        /// <summary>XP total acumulado necessário para ESTAR no nível n (início do nível).</summary>
        public static long TotalXpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            long total = 0;
            for (var n = 1; n < level; n++)
            {
                total += XpForNext(n);
            }

            return total;
        }

        /// <summary>Nível derivado do XP total (cap 100 — excedente acumula sem subir).</summary>
        public static int LevelForTotalXp(long totalXp)
        {
            if (totalXp <= 0)
            {
                return 1;
            }

            var level = 1;
            long consumed = 0;
            while (level < MaxLevel)
            {
                var next = XpForNext(level);
                if (consumed + next > totalXp)
                {
                    break;
                }

                consumed += next;
                level++;
            }

            return level;
        }

        /// <summary>XP dentro do nível corrente (para a barra do HUD).</summary>
        public static int XpIntoCurrentLevel(long totalXp)
        {
            var level = LevelForTotalXp(totalXp);
            return (int)System.Math.Max(0, totalXp - TotalXpForLevel(level));
        }

        /// <summary>Migração de save legado {nível, xpParcial} → XP total equivalente.</summary>
        public static long MigrateLegacy(int legacyLevel, int legacyCurrentXp)
        {
            return TotalXpForLevel(Mathf.Clamp(legacyLevel, 1, MaxLevel)) + Mathf.Max(0, legacyCurrentXp);
        }
    }
}
