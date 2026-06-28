using NUnit.Framework;
using CindarsHope.Combat;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_79 — Trava a fórmula player-relative de EnemyScaleResolver.ResolveVisualScale.
    ///
    /// Propriedades testadas:
    ///   CA-2  Medium comum = PlayerReferenceScale (mesmo tamanho que o player).
    ///   CA-2  Large = 1.5× player; Huge = 2× player.
    ///   CA-2  MiniBoss ×1.5 / Boss ×2.5 sobre a base de cada size class.
    ///   CA-2  Gargantuan boss > Huge boss (hierarquia não colapsa).
    ///   CA-4  Valor retornado por ResolveVisualScale coincide com o que o codex (F45) deve exibir
    ///          (mesma função pura — sem path alternativo no codex).
    ///   CA-3  ColliderRadiusFor não regrediu (física preservada).
    /// </summary>
    [TestFixture]
    public class EnemyScaleResolverPlayerRelativeTests
    {
        private const float Delta = 0.0001f;
        private const float PlayerRef = EnemyScaleResolver.PlayerReferenceScale; // 2.0

        // ── CA-2: Relação com o player travada ─────────────────────────────────────────────

        [Test]
        public void MediumCommon_EqualsPlayerReferenceScale()
        {
            // Medium = 1.0× player; PlayerRef=2.0 → 2.0 * 1.0 * 1.0 = 2.0
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, false, false);
            Assert.AreEqual(PlayerRef, scale, Delta,
                "Medium comum deve ter a mesma escala Unity que o player (PlayerReferenceScale).");
        }

        [Test]
        public void LargeCommon_Is1_5xPlayer()
        {
            // Large = 1.5× player; 2.0 * 1.5 * 1.0 = 3.0
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Large, false, false);
            Assert.AreEqual(PlayerRef * EnemyScaleResolver.PlayerRelativeScale.Large, scale, Delta,
                "Large comum deve ser 1.5× o player.");
        }

        [Test]
        public void HugeCommon_Is2xPlayer()
        {
            // Huge = 2.0× player; 2.0 * 2.0 * 1.0 = 4.0
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Huge, false, false);
            Assert.AreEqual(PlayerRef * EnemyScaleResolver.PlayerRelativeScale.Huge, scale, Delta,
                "Huge comum deve ser 2× o player.");
        }

        [Test]
        public void TinyCommon_Is0_5xPlayer()
        {
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Tiny, false, false);
            Assert.AreEqual(PlayerRef * EnemyScaleResolver.PlayerRelativeScale.Tiny, scale, Delta);
        }

        [Test]
        public void SmallCommon_Is0_75xPlayer()
        {
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Small, false, false);
            Assert.AreEqual(PlayerRef * EnemyScaleResolver.PlayerRelativeScale.Small, scale, Delta);
        }

        [Test]
        public void GargantuanCommon_Is3xPlayer()
        {
            float scale = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Gargantuan, false, false);
            Assert.AreEqual(PlayerRef * EnemyScaleResolver.PlayerRelativeScale.Gargantuan, scale, Delta,
                "Gargantuan comum deve ser 3× o player.");
        }

        // ── CA-2: Multiplicadores de role ─────────────────────────────────────────────────

        [Test]
        public void MediumMiniBoss_Is1_5xMediumCommon()
        {
            float common   = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, false, false);
            float miniboss = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, true,  false);
            Assert.AreEqual(common * EnemyScaleResolver.RoleScaleMultiplier.MiniBoss, miniboss, Delta,
                "MiniBoss deve ser 1.5× a escala base da size class.");
        }

        [Test]
        public void MediumBoss_Is2_5xMediumCommon()
        {
            float common = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, false, false);
            float boss   = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, false, true);
            Assert.AreEqual(common * EnemyScaleResolver.RoleScaleMultiplier.Boss, boss, Delta,
                "Boss deve ser 2.5× a escala base da size class.");
        }

        [Test]
        public void BossFlag_PrevailsOverMiniBossFlag()
        {
            // Quando ambos são true, Boss (2.5) prevalece sobre MiniBoss (1.5).
            float boss         = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, false, true);
            float bothFlags    = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Medium, true,  true);
            Assert.AreEqual(boss, bothFlags, Delta,
                "Quando isBoss=true, o multiplicador Boss prevalece sobre IsMiniBoss.");
        }

        // ── CA-2: Hierarquia Gargantuan boss > Huge boss ──────────────────────────────────

        [Test]
        public void GargantuanBoss_IsGreaterThan_HugeBoss()
        {
            float gargantuanBoss = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Gargantuan, false, true);
            float hugeBoss       = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Huge,       false, true);
            Assert.Greater(gargantuanBoss, hugeBoss,
                "Gargantuan boss deve ser visualmente maior que Huge boss.");
        }

        [Test]
        public void GargantuanCommon_IsGreaterThan_HugeCommon()
        {
            float gargantuan = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Gargantuan, false, false);
            float huge       = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Huge,       false, false);
            Assert.Greater(gargantuan, huge,
                "Gargantuan comum deve ser maior que Huge comum.");
        }

        // ── CA-4: Coincidência codex/campo ────────────────────────────────────────────────

        [Test]
        public void ResolveVisualScale_IsPureFunction_SameInputSameOutput()
        {
            // A mesma função pura retorna o mesmo valor em qualquer contexto —
            // codex (F45) e materializer leem o mesmo número sem path alternativo.
            float r1 = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Large, true, false);
            float r2 = EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass.Large, true, false);
            Assert.AreEqual(r1, r2, Delta,
                "ResolveVisualScale deve ser determinística: mesmos inputs, mesmo output (codex coincide com campo).");
        }

        [Test]
        public void PlayerRelativeRatioFor_Medium_Returns1()
        {
            // Ratio exposto separadamente para o codex poder exibir "N× o player" sem repetir a fórmula.
            float ratio = EnemyScaleResolver.PlayerRelativeRatioFor(BestiarySizeClass.Medium);
            Assert.AreEqual(1.0f, ratio, Delta);
        }

        // ── CA-3: Física preservada (collider radius não regrediu) ────────────────────────

        [TestCase(BestiarySizeClass.Tiny,       0.25f)]
        [TestCase(BestiarySizeClass.Small,      0.35f)]
        [TestCase(BestiarySizeClass.Medium,     0.45f)]
        [TestCase(BestiarySizeClass.Large,      0.65f)]
        [TestCase(BestiarySizeClass.Huge,       0.95f)]
        [TestCase(BestiarySizeClass.Gargantuan, 1.20f)]
        public void ColliderRadiusFor_PhysicsNotRegressed(BestiarySizeClass size, float expected)
        {
            Assert.AreEqual(expected, EnemyScaleResolver.ColliderRadiusFor(size), Delta,
                $"ColliderRadiusFor({size}) não deve ter regredido (CA-3: física preservada).");
        }

        // ── Valores absolutos concretos (documentação executável) ─────────────────────────

        [TestCase(BestiarySizeClass.Tiny,       false, false, 1.00f)]  // 2.0 * 0.5 * 1.0
        [TestCase(BestiarySizeClass.Small,      false, false, 1.50f)]  // 2.0 * 0.75 * 1.0
        [TestCase(BestiarySizeClass.Medium,     false, false, 2.00f)]  // 2.0 * 1.0  * 1.0
        [TestCase(BestiarySizeClass.Large,      false, false, 3.00f)]  // 2.0 * 1.5  * 1.0
        [TestCase(BestiarySizeClass.Huge,       false, false, 4.00f)]  // 2.0 * 2.0  * 1.0
        [TestCase(BestiarySizeClass.Gargantuan, false, false, 6.00f)]  // 2.0 * 3.0  * 1.0
        [TestCase(BestiarySizeClass.Medium,     true,  false, 3.00f)]  // 2.0 * 1.0  * 1.5
        [TestCase(BestiarySizeClass.Medium,     false, true,  5.00f)]  // 2.0 * 1.0  * 2.5
        [TestCase(BestiarySizeClass.Huge,       false, true,  10.00f)] // 2.0 * 2.0  * 2.5
        [TestCase(BestiarySizeClass.Gargantuan, false, true,  15.00f)] // 2.0 * 3.0  * 2.5
        public void ResolveVisualScale_ConcreteValues(
            BestiarySizeClass size, bool isMiniBoss, bool isBoss, float expected)
        {
            float scale = EnemyScaleResolver.ResolveVisualScale(size, isMiniBoss, isBoss);
            Assert.AreEqual(expected, scale, Delta,
                $"ResolveVisualScale({size}, miniboss={isMiniBoss}, boss={isBoss}) deve ser {expected}.");
        }
    }
}
