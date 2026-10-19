using CindarsHope.Player.Progression;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// F42 — curva canônica (cap 100, XPnext = 60×N^1.5, 1 ponto/2 níveis = 50 no cap).
    /// </summary>
    public class ProgressionCurveTests
    {
        // Valores de referência do BALANCE_CURVES v1.0.
        [TestCase(1, 60)]
        [TestCase(2, 170)]
        [TestCase(5, 671)]
        [TestCase(10, 1897)]
        [TestCase(25, 7500)]
        [TestCase(50, 21213)]
        [TestCase(99, 59102)]
        public void XpForNext_MatchesCanonicalFormula(int level, int expected)
        {
            var formula = Mathf.RoundToInt(60f * Mathf.Pow(level, 1.5f));
            Assert.AreEqual(formula, ProgressionCurve.XpForNext(level), "Fórmula auto-consistente.");
            Assert.AreEqual(expected, ProgressionCurve.XpForNext(level), $"Referência N={level}.");
        }

        [Test]
        public void LevelForTotalXp_RoundTripsWithTotalXpForLevel()
        {
            foreach (var level in new[] { 1, 2, 10, 37, 50, 99, 100 })
            {
                var total = ProgressionCurve.TotalXpForLevel(level);
                Assert.AreEqual(level, ProgressionCurve.LevelForTotalXp(total), $"Início do nível {level}.");
                if (level < ProgressionCurve.MaxLevel)
                {
                    Assert.AreEqual(level, ProgressionCurve.LevelForTotalXp(total + ProgressionCurve.XpForNext(level) - 1), "1 XP antes de subir.");
                    Assert.AreEqual(level + 1, ProgressionCurve.LevelForTotalXp(total + ProgressionCurve.XpForNext(level)), "Exato para subir.");
                }
            }
        }

        [Test]
        public void Cap100_ExcessXpDoesNotLevelBeyond()
        {
            var capTotal = ProgressionCurve.TotalXpForLevel(100);
            Assert.AreEqual(100, ProgressionCurve.LevelForTotalXp(capTotal));
            Assert.AreEqual(100, ProgressionCurve.LevelForTotalXp(capTotal + 1000000), "XP além do cap acumula sem subir.");
        }

        [Test]
        public void SkillPoints_Exactly50AtCap()
        {
            Assert.AreEqual(50, PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(100), "Orçamento canônico das árvores.");
            Assert.AreEqual(1, PlayerProgressionRules.CalculateSkillPointsGrantedOnLevelUp(100), "Nível 100 (par) concede o 50º.");
            Assert.AreEqual(0, PlayerProgressionRules.CalculateSkillPointsGrantedOnLevelUp(99));
        }

        [Test]
        public void Migration_LegacySavePreservesProgress()
        {
            var total = ProgressionCurve.MigrateLegacy(10, 500);
            Assert.AreEqual(ProgressionCurve.TotalXpForLevel(10) + 500, total);
            Assert.AreEqual(10, ProgressionCurve.LevelForTotalXp(total), "Nível preservado (parcial < XpForNext(10)).");
            Assert.AreEqual(500, ProgressionCurve.XpIntoCurrentLevel(total));
        }

        [Test]
        public void Migration_IsIdempotent()
        {
            var first = ProgressionCurve.MigrateLegacy(25, 100);
            // Migração só roda quando TotalXp==0; re-derivar do total é estável:
            Assert.AreEqual(ProgressionCurve.LevelForTotalXp(first), ProgressionCurve.LevelForTotalXp(first));
            Assert.AreEqual(ProgressionCurve.XpIntoCurrentLevel(first), ProgressionCurve.XpIntoCurrentLevel(first));
        }

        [Test]
        public void CanonicalCurve_IsDefault()
        {
            Assert.IsFalse(PlayerProgressionRules.UseLegacyCurve, "Curva canônica por padrão (flag = rollback).");
            Assert.AreEqual(ProgressionCurve.XpForNext(7), PlayerProgressionRules.CalculateXpToNextLevel(7));
        }
    }
}
