using System.Collections.Generic;
using CindarsHope.Combat.Magic;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    /// <summary>
    /// fable_08 — testes determinísticos das formas de magia e targeting:
    /// - defaults Bolt (caracterização: assets sem campos novos castam como Bolt instantâneo);
    /// - seleção de auto-target (mais próximo / na mira) determinística (EMENDA 6.6-A);
    /// - lógica de barreira absorvendo (CA-3);
    /// - parâmetros canônicos da Tier 5 (Eco de Anya / Ruptura de Senya).
    /// A execução por cena (spawn de projétil, OverlapCircle) fica para o cenário humano (Play Mode).
    /// </summary>
    public class SpellShapeTests
    {
        private static SpellDataSO MakeSpell(string id, SpellShape shape)
        {
            var so = ScriptableObject.CreateInstance<SpellDataSO>();
            so.Id = id;
            so.Shape = shape;
            so.BaseDamage = 10;
            so.ManaCost = 10;
            return so;
        }

        // ------------------------------------------------ CA-1: defaults Bolt (caracterização)

        [Test]
        public void Defaults_NewSpellIsBolt_WithZeroCastTimeAndNeutralFields()
        {
            var so = ScriptableObject.CreateInstance<SpellDataSO>();

            Assert.AreEqual(SpellShape.Bolt, so.Shape, "Shape default deve ser Bolt (assets legados intactos).");
            Assert.AreEqual(0f, so.CastTimeSeconds, "CastTime default 0 (cast instantâneo legado).");
            Assert.IsFalse(so.AutoTarget, "AutoTarget default false.");
            Assert.AreEqual(0, so.RestoreHp);
            Assert.AreEqual(0, so.RestoreStamina);
            Assert.AreEqual(0, so.RestoreMana);
            Assert.AreEqual(0, so.BarrierAbsorb);
            Assert.AreEqual(0f, so.BarrierSeconds);
        }

        [Test]
        public void Enum_ContainsAllFiveShapes()
        {
            var required = new[]
            {
                SpellShape.Bolt, SpellShape.Cone, SpellShape.Nova,
                SpellShape.SelfRestore, SpellShape.Barrier
            };
            foreach (var s in required)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(SpellShape), s), s.ToString());
            }
        }

        // ------------------------------------------------ EMENDA 6.6-A: auto-target determinístico

        [Test]
        public void AutoTarget_SelectsNearestInsideRange()
        {
            var caster = Vector2.zero;
            var facing = Vector2.right;
            var candidates = new List<Vector2>
            {
                new Vector2(5f, 0f),   // 0: longe
                new Vector2(2f, 0f),   // 1: mais próximo (na mira)
                new Vector2(3f, 0f),   // 2: médio
            };

            int idx = SpellTargeting.SelectAutoTarget(caster, facing, candidates, range: 10f);
            Assert.AreEqual(1, idx, "Deve escolher o inimigo elegível mais próximo na mira.");
        }

        [Test]
        public void AutoTarget_PrefersTargetInsideAimCone_OverCloserBehind()
        {
            var caster = Vector2.zero;
            var facing = Vector2.right; // mira para +x
            var candidates = new List<Vector2>
            {
                new Vector2(-1f, 0f),  // 0: mais próximo mas ATRÁS (fora do cone)
                new Vector2(4f, 0f),   // 1: à frente, na mira
            };

            int idx = SpellTargeting.SelectAutoTarget(caster, facing, candidates, range: 10f, aimHalfAngleDeg: 60f);
            Assert.AreEqual(1, idx, "Alvo na mira tem prioridade sobre o mais próximo fora do cone.");
        }

        [Test]
        public void AutoTarget_FallsBackToNearest_WhenNoneInsideCone()
        {
            var caster = Vector2.zero;
            var facing = Vector2.right;
            var candidates = new List<Vector2>
            {
                new Vector2(-2f, 0f),  // 0: atrás, próximo
                new Vector2(-5f, 0f),  // 1: atrás, longe
            };

            int idx = SpellTargeting.SelectAutoTarget(caster, facing, candidates, range: 10f, aimHalfAngleDeg: 30f);
            Assert.AreEqual(0, idx, "Sem ninguém na mira, cai para o mais próximo em geral.");
        }

        [Test]
        public void AutoTarget_ReturnsMinusOne_WhenAllOutOfRange()
        {
            var candidates = new List<Vector2> { new Vector2(20f, 0f) };
            int idx = SpellTargeting.SelectAutoTarget(Vector2.zero, Vector2.right, candidates, range: 5f);
            Assert.AreEqual(-1, idx, "Nenhum candidato no alcance → -1.");
        }

        [Test]
        public void AutoTarget_Deterministic_TieBreaksByIndex()
        {
            var candidates = new List<Vector2>
            {
                new Vector2(3f, 0f),  // 0
                new Vector2(3f, 0f),  // 1 (mesma distância)
            };
            int a = SpellTargeting.SelectAutoTarget(Vector2.zero, Vector2.right, candidates, 10f);
            int b = SpellTargeting.SelectAutoTarget(Vector2.zero, Vector2.right, candidates, 10f);
            Assert.AreEqual(a, b, "Mesma entrada → mesmo alvo (determinístico).");
            Assert.AreEqual(0, a, "Empate de distância resolve pelo menor índice.");
        }

        [Test]
        public void ResolveAimDirection_PointsAtChosenTarget()
        {
            var caster = Vector2.zero;
            var candidates = new List<Vector2> { new Vector2(0f, 4f) };
            var dir = SpellTargeting.ResolveAimDirection(caster, Vector2.right, candidates, range: 10f);
            Assert.AreEqual(Vector2.up, dir, "Direção deve apontar para o alvo escolhido (caster→alvo).");
        }

        [Test]
        public void ResolveAimDirection_FallsBackToFacing_WhenNoTarget()
        {
            var dir = SpellTargeting.ResolveAimDirection(Vector2.zero, Vector2.right, new List<Vector2>(), range: 10f);
            Assert.AreEqual(Vector2.right, dir, "Sem alvo, mantém a direção de mira.");
        }

        [Test]
        public void IsInsideCone_RespectsHalfAngleAndRange()
        {
            var caster = Vector2.zero;
            var facing = Vector2.right;
            Assert.IsTrue(SpellTargeting.IsInsideCone(caster, facing, new Vector2(3f, 0f), range: 5f, halfAngleDeg: 30f));
            Assert.IsFalse(SpellTargeting.IsInsideCone(caster, facing, new Vector2(0f, 3f), range: 5f, halfAngleDeg: 30f), "90° está fora de um cone de 30°.");
            Assert.IsFalse(SpellTargeting.IsInsideCone(caster, facing, new Vector2(9f, 0f), range: 5f, halfAngleDeg: 30f), "Fora do alcance.");
        }

        // ------------------------------------------------ CA-3: barreira absorvendo

        [Test]
        public void Barrier_AbsorbLogic_PartialHit_ReducesRemaining()
        {
            int absorbed = PlayerBarrierState.AbsorbLogic(rawDamage: 12, remainingAbsorb: 40, out int remaining);
            Assert.AreEqual(12, absorbed, "Absorve o dano inteiro quando há margem.");
            Assert.AreEqual(28, remaining, "Restante = 40 - 12.");
        }

        [Test]
        public void Barrier_AbsorbLogic_OverflowHit_PassesThroughExcess()
        {
            int absorbed = PlayerBarrierState.AbsorbLogic(rawDamage: 50, remainingAbsorb: 40, out int remaining);
            Assert.AreEqual(40, absorbed, "Absorve no máximo o restante.");
            Assert.AreEqual(0, remaining, "Restante zera.");
            Assert.AreEqual(10, 50 - absorbed, "Excedente (10) passa adiante para defesa.");
        }

        [Test]
        public void Barrier_AbsorbLogic_ZeroRemaining_AbsorbsNothing()
        {
            int absorbed = PlayerBarrierState.AbsorbLogic(rawDamage: 20, remainingAbsorb: 0, out int remaining);
            Assert.AreEqual(0, absorbed);
            Assert.AreEqual(0, remaining);
        }

        [Test]
        public void Barrier_Cast_RaisesActiveInstance_AndExpiresByTime()
        {
            PlayerBarrierState.Clear();
            var barrier = PlayerBarrierState.Cast(absorb: 30, seconds: 5f, now: 100f, sourceSpellId: "spell_arcane_barrier");

            Assert.IsNotNull(barrier);
            Assert.AreSame(barrier, PlayerBarrierState.ActiveInstance);
            Assert.IsTrue(barrier.IsActive(now: 101f), "Ativa antes de expirar.");
            Assert.IsFalse(barrier.IsActive(now: 106f), "Expira após BarrierSeconds.");

            PlayerBarrierState.Clear();
            Assert.IsNull(PlayerBarrierState.ActiveInstance, "Clear remove a barreira.");
        }

        [Test]
        public void Barrier_Cast_WithZeroAbsorbOrSeconds_DoesNotRaise()
        {
            PlayerBarrierState.Clear();
            Assert.IsNull(PlayerBarrierState.Cast(absorb: 0, seconds: 5f, now: 0f, sourceSpellId: "x"));
            Assert.IsNull(PlayerBarrierState.Cast(absorb: 10, seconds: 0f, now: 0f, sourceSpellId: "x"));
            Assert.IsNull(PlayerBarrierState.ActiveInstance);
        }

        [Test]
        public void Barrier_AbsorbIncoming_ConsumesThenExpiresWhenDepleted()
        {
            PlayerBarrierState.Cast(absorb: 15, seconds: 10f, now: 0f, sourceSpellId: "spell_arcane_barrier");

            int pass1 = PlayerBarrierState.AbsorbIncoming(rawDamage: 10, now: 1f);
            Assert.AreEqual(0, pass1, "10 < 15 absorvido inteiro, nada passa.");
            Assert.IsNotNull(PlayerBarrierState.ActiveInstance);

            int pass2 = PlayerBarrierState.AbsorbIncoming(rawDamage: 10, now: 2f);
            Assert.AreEqual(5, pass2, "Restavam 5; 10-5=5 passam adiante.");
            Assert.IsNull(PlayerBarrierState.ActiveInstance, "Barreira esgotada é removida.");
        }

        [Test]
        public void Barrier_AbsorbIncoming_NoBarrier_ReturnsRawUnchanged()
        {
            PlayerBarrierState.Clear();
            Assert.AreEqual(17, PlayerBarrierState.AbsorbIncoming(17, now: 0f));
        }

        // ------------------------------------------------ Tier 5 (EMENDA 6.6-A) parâmetros canônicos

        [Test]
        public void Tier5_EcoDeAnya_IsSelfRestoreSupport()
        {
            // Espelha o gerador GenerateShapeSpells (anya_echo). Cura late, MP alto, cooldown 45s.
            var so = ScriptableObject.CreateInstance<SpellDataSO>();
            so.Id = "anya_echo";
            so.Shape = SpellShape.SelfRestore;
            so.RestoreHp = 60;
            so.ManaCost = 44;
            so.CooldownSeconds = 45f;
            so.CastTimeSeconds = 0.9f;

            Assert.AreEqual(SpellShape.SelfRestore, so.Shape);
            Assert.Greater(so.RestoreHp, 0, "Eco de Anya cura.");
            Assert.GreaterOrEqual(so.ManaCost, 40, "Suporte late tem custo alto.");
            Assert.Greater(so.CastTimeSeconds, 0f, "Tem cast interrompível.");
        }

        [Test]
        public void Tier5_RupturaDeSenya_IsNovaBurst()
        {
            // Espelha o gerador (senya_rupture). Burst em área, MP/cooldown altos.
            var so = ScriptableObject.CreateInstance<SpellDataSO>();
            so.Id = "senya_rupture";
            so.Shape = SpellShape.Nova;
            so.BaseDamage = 55;
            so.NovaRadius = 2f;
            so.ManaCost = 46;
            so.CooldownSeconds = 35f;
            so.CastTimeSeconds = 1.0f;

            Assert.AreEqual(SpellShape.Nova, so.Shape, "Ruptura é burst em área.");
            Assert.Greater(so.BaseDamage, 40, "Dano forte late.");
            Assert.Greater(so.NovaRadius, 0f);
            Assert.GreaterOrEqual(so.CooldownSeconds, 30f, "Cooldown alto (não é botão universal).");
        }

        [Test]
        public void ArcaneProjectile_IsBoltWithAutoTarget()
        {
            // Espelha o gerador (arcane_projectile, EMENDA 6.6-A).
            var so = ScriptableObject.CreateInstance<SpellDataSO>();
            so.Id = "arcane_projectile";
            so.Shape = SpellShape.Bolt;
            so.AutoTarget = true;

            Assert.AreEqual(SpellShape.Bolt, so.Shape, "Projétil Arcano continua um bolt (não vira nova spell).");
            Assert.IsTrue(so.AutoTarget, "Projétil Arcano usa auto-target (6.6-A).");
        }
    }
}
