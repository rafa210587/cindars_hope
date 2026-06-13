using CindarsHope.Combat;
using CindarsHope.Player.Movement;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// F27 — perfect block: janela 0.15s do INPUT, anti-spam 0.4s, mitigação normal,
    /// reflexo de postura (fração) e CoreExposed (já coberto por F02).
    /// </summary>
    public class PerfectBlockTests
    {
        // ------------------------------------------------ janela

        [Test]
        public void IsPerfect_WithinWindow()
        {
            Assert.IsTrue(BlockTimingRules.IsPerfect(10f, 10.10f, windowArmed: true), "0.10s de block.");
            Assert.IsTrue(BlockTimingRules.IsPerfect(10f, 10.15f, windowArmed: true), "Borda da janela.");
            Assert.IsFalse(BlockTimingRules.IsPerfect(10f, 10.30f, windowArmed: true), "0.30s = block normal.");
            Assert.IsFalse(BlockTimingRules.IsPerfect(10f, 9.9f, windowArmed: true), "Hit antes do block.");
        }

        [Test]
        public void IsPerfect_RequiresArmedWindow()
        {
            Assert.IsFalse(BlockTimingRules.IsPerfect(10f, 10.05f, windowArmed: false), "Janela não armada (anti-spam).");
        }

        // ------------------------------------------------ anti-spam

        [Test]
        public void RearmCooldown_BlocksQuickReblock()
        {
            Assert.IsFalse(BlockTimingRules.CanArmPerfectWindow(lastBlockEndTime: 10f, newStartTime: 10.3f), "Re-block em 0.3s não rearma.");
            Assert.IsTrue(BlockTimingRules.CanArmPerfectWindow(lastBlockEndTime: 10f, newStartTime: 10.4f), "0.4s rearma.");
            Assert.IsTrue(BlockTimingRules.CanArmPerfectWindow(lastBlockEndTime: -10f, newStartTime: 0f), "Primeiro block sempre arma.");
        }

        // ------------------------------------------------ mitigação normal

        [Test]
        public void NormalBlock_MitigatesHalf_FloorOne()
        {
            Assert.AreEqual(5, BlockTimingRules.MitigateNormalBlock(10));
            Assert.AreEqual(1, BlockTimingRules.MitigateNormalBlock(1), "Floor 1.");
            Assert.AreEqual(0, BlockTimingRules.MitigateNormalBlock(0));
        }

        // ------------------------------------------------ reflexo

        [Test]
        public void PostureReflect_IsFortyPercentOfRaw()
        {
            Assert.AreEqual(0.4f, BlockTimingRules.PostureReflectFraction, 0.001f, "Canon: 40% do dano bruto vira dano de postura no atacante.");
        }

        // ------------------------------------------------ CoreExposed (constantes F02)

        [Test]
        public void CoreExposed_CanonConstants()
        {
            Assert.AreEqual(1.2f, EnemyPostureState.StaggerSeconds, 0.001f);
            Assert.AreEqual(1.5f, EnemyPostureState.CoreExposedSeconds, 0.001f);
            Assert.AreEqual(1.3f, EnemyPostureState.CoreExposedMultiplier, 0.001f);
        }
    }
}
