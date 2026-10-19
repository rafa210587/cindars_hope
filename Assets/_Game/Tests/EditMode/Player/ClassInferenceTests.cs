using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_39 — INFERRED player class (Q9.1). Pure-function coverage for the inference rules:
    /// simple dominance (CA-1), hybrid (CA-2), Colono incl. ties (CA-3), the &lt;= 5% magnitude guard,
    /// recompute determinism (CA-4 — same points always yield the same profile, no saved state), and
    /// the title id table (5 pure + composite hybrids).
    ///
    /// 100% deterministic EditMode: <see cref="PlayerClassInference"/> is pure C# (no Unity, no event
    /// bus, no save). The recompute-by-event path (<see cref="InferredClassRuntime"/>) is validated in
    /// the batch Play Mode scenario; here we prove the function it calls is stable for equal inputs.
    /// </summary>
    [TestFixture]
    public class ClassInferenceTests
    {
        private static Dictionary<SkillTreeId, int> Points(
            int melee = 0, int ranged = 0, int magic = 0, int survival = 0, int crafting = 0)
        {
            return new Dictionary<SkillTreeId, int>
            {
                { SkillTreeId.Melee, melee },
                { SkillTreeId.Ranged, ranged },
                { SkillTreeId.Magic, magic },
                { SkillTreeId.Survival, survival },
                { SkillTreeId.Crafting, crafting },
            };
        }

        // ───────────────────────────────── CA-1: dominância simples ─────────────────────────────

        [Test]
        public void CA1_SimpleDominance_Melee_IsWarriorWithPostureBonus()
        {
            // 10 em Guerreiro (melee) e 2 em Caçador (ranged): >=6 pts e >=40% do total (12), 2ª <70%.
            var profile = PlayerClassInference.GetProfile(Points(melee: 10, ranged: 2));

            Assert.IsTrue(profile.HasDominant, "Deve ter dominante.");
            Assert.IsFalse(profile.IsHybrid, "10 vs 2 nao e hibrido (2 < 70% de 10).");
            Assert.IsFalse(profile.IsColono);
            Assert.AreEqual(SkillTreeId.Melee, profile.DominantTree);
            Assert.AreEqual("ui.class.warrior", profile.TitleId);
            Assert.AreEqual(PlayerClassInference.BonusAxis.PostureDealt, profile.PrimaryBonus.Axis);
            Assert.AreEqual(0.03f, profile.PrimaryBonus.Value, 0.0001f, "+3% postura causada.");
            Assert.AreEqual(0.03f, profile.BonusFor(PlayerClassInference.BonusAxis.PostureDealt), 0.0001f);
            Assert.IsTrue(profile.SecondaryBonus.IsEmpty, "Puro nao tem bonus secundario.");
        }

        [Test]
        public void CA1_Dominance_BelowSixPoints_IsColono()
        {
            // 5 em melee (maioria e 71% do total) mas < 6 pontos => sem dominante => Colono.
            var profile = PlayerClassInference.GetProfile(Points(melee: 5, ranged: 2));
            Assert.IsTrue(profile.IsColono, "5 pts nao alcanca o piso de 6.");
            Assert.AreEqual("ui.class.colono", profile.TitleId);
        }

        [Test]
        public void CA1_Dominance_BelowFortyPercentShare_IsColono()
        {
            // 6 em melee, mas total 16 => 37.5% < 40% => sem dominante => Colono.
            var profile = PlayerClassInference.GetProfile(Points(melee: 6, ranged: 4, magic: 3, survival: 3));
            Assert.IsTrue(profile.IsColono, "6/16 = 37.5% < 40%.");
            Assert.IsTrue(profile.PrimaryBonus.IsEmpty);
        }

        [Test]
        public void CA1_Dominance_OtherTrees_MapToCorrectTitles()
        {
            Assert.AreEqual("ui.class.hunter",
                PlayerClassInference.GetProfile(Points(ranged: 10, melee: 2)).TitleId);
            Assert.AreEqual("ui.class.mystic",
                PlayerClassInference.GetProfile(Points(magic: 10, melee: 2)).TitleId);
            Assert.AreEqual("ui.class.farmer",
                PlayerClassInference.GetProfile(Points(survival: 10, melee: 2)).TitleId);
            Assert.AreEqual("ui.class.bond",
                PlayerClassInference.GetProfile(Points(crafting: 10, melee: 2)).TitleId);
        }

        [Test]
        public void CA1_Farmer_StaminaBonus_IsFivePercent_AtCap()
        {
            var profile = PlayerClassInference.GetProfile(Points(survival: 10, melee: 2));
            Assert.AreEqual(PlayerClassInference.BonusAxis.StaminaOutOfCave, profile.PrimaryBonus.Axis);
            Assert.AreEqual(0.05f, profile.PrimaryBonus.Value, 0.0001f, "Lavrador +5% stamina (no teto da regua).");
        }

        // ───────────────────────────────── CA-2: híbrido ────────────────────────────────────────

        [Test]
        public void CA2_Hybrid_EightSix_CompositeTitle_AndHalfBonuses()
        {
            // 8 melee / 6 ranged: 6 >= 70% de 8 (5.6) => hibrido. Dominante melee, secundaria ranged.
            var profile = PlayerClassInference.GetProfile(Points(melee: 8, ranged: 6));

            Assert.IsTrue(profile.HasDominant);
            Assert.IsTrue(profile.IsHybrid, "6 >= 70% de 8 => hibrido.");
            Assert.AreEqual(SkillTreeId.Melee, profile.DominantTree);
            Assert.AreEqual(SkillTreeId.Ranged, profile.SecondaryTree);
            Assert.AreEqual("ui.class.warrior_hunter", profile.TitleId, "Titulo composto dominante_secundario.");

            // METADE de cada micro-bonus (3% -> 1.5%).
            Assert.AreEqual(PlayerClassInference.BonusAxis.PostureDealt, profile.PrimaryBonus.Axis);
            Assert.AreEqual(0.015f, profile.PrimaryBonus.Value, 0.0001f, "Metade de +3% postura.");
            Assert.AreEqual(PlayerClassInference.BonusAxis.CritChance, profile.SecondaryBonus.Axis);
            Assert.AreEqual(0.015f, profile.SecondaryBonus.Value, 0.0001f, "Metade de +3% crit.");
        }

        [Test]
        public void CA2_Hybrid_SecondaryJustUnderSeventyPercent_IsPureNotHybrid()
        {
            // 10 melee / 6 ranged: 6 < 70% de 10 (7) => NAO hibrido => puro Guerreiro.
            var profile = PlayerClassInference.GetProfile(Points(melee: 10, ranged: 6));
            Assert.IsTrue(profile.HasDominant);
            Assert.IsFalse(profile.IsHybrid, "6 < 70% de 10 => puro.");
            Assert.AreEqual("ui.class.warrior", profile.TitleId);
            Assert.AreEqual(0.03f, profile.PrimaryBonus.Value, 0.0001f, "Bonus cheio (nao hibrido).");
        }

        [Test]
        public void CA2_Hybrid_DominantSecondaryOrder_DeterminesCompositeId()
        {
            // 8 magic / 7 survival => mystic_farmer (dominante lidera o composto).
            var profile = PlayerClassInference.GetProfile(Points(magic: 8, survival: 7));
            Assert.IsTrue(profile.IsHybrid);
            Assert.AreEqual(SkillTreeId.Magic, profile.DominantTree);
            Assert.AreEqual(SkillTreeId.Survival, profile.SecondaryTree);
            Assert.AreEqual("ui.class.mystic_farmer", profile.TitleId);
        }

        // ───────────────────────────────── CA-3: Colono (incl. empates) ─────────────────────────

        [Test]
        public void CA3_NoInvestment_IsColono_NoBonus()
        {
            var profile = PlayerClassInference.GetProfile(Points());
            Assert.IsTrue(profile.IsColono);
            Assert.AreEqual("ui.class.colono", profile.TitleId);
            Assert.IsTrue(profile.PrimaryBonus.IsEmpty);
            Assert.IsTrue(profile.SecondaryBonus.IsEmpty);
            Assert.AreEqual(0f, profile.BonusFor(PlayerClassInference.BonusAxis.PostureDealt), 0.0001f);
        }

        [Test]
        public void CA3_LowSpread_NoDominant_IsColono()
        {
            // 2/2/2: nenhuma com >=6 e >=40% => Colono.
            var profile = PlayerClassInference.GetProfile(Points(melee: 2, ranged: 2, magic: 2));
            Assert.IsTrue(profile.IsColono);
            Assert.IsTrue(profile.PrimaryBonus.IsEmpty);
        }

        [Test]
        public void CA3_TieAtTop_IsColono_NoBonus()
        {
            // Empate no topo (8 melee / 8 ranged): sem dominante => Colono, mesmo acima dos limiares.
            var profile = PlayerClassInference.GetProfile(Points(melee: 8, ranged: 8));
            Assert.IsTrue(profile.IsColono, "Empate no topo => sem dominante.");
            Assert.AreEqual("ui.class.colono", profile.TitleId);
            Assert.IsTrue(profile.PrimaryBonus.IsEmpty);
        }

        [Test]
        public void CA3_NullPoints_IsColono()
        {
            var profile = PlayerClassInference.GetProfile(null);
            Assert.IsTrue(profile.IsColono);
            Assert.AreEqual("ui.class.colono", profile.TitleId);
        }

        [Test]
        public void CA3_MissingTreeKeys_TreatedAsZero()
        {
            // Apenas melee no dicionario (chaves ausentes contam como 0).
            var sparse = new Dictionary<SkillTreeId, int> { { SkillTreeId.Melee, 9 } };
            var profile = PlayerClassInference.GetProfile(sparse);
            Assert.AreEqual("ui.class.warrior", profile.TitleId, "Total=9, melee=9 => 100% => dominante.");
        }

        [Test]
        public void NegativePoints_ClampedToZero()
        {
            var profile = PlayerClassInference.GetProfile(Points(melee: 9, ranged: -5));
            Assert.AreEqual("ui.class.warrior", profile.TitleId, "Pontos negativos viram 0; total=9.");
        }

        // ───────────────────────────── magnitude guard (<= 5%) ──────────────────────────────────

        [Test]
        public void MicroBonus_NeverAboveFivePercent_ForAnyPureTree()
        {
            foreach (SkillTreeId tree in System.Enum.GetValues(typeof(SkillTreeId)))
            {
                var bonus = PlayerClassInference.PureBonus(tree);
                Assert.LessOrEqual(bonus.Value, 0.05f + 0.0001f,
                    $"Micro-bonus de {tree} excede a regua de identidade de 5%.");
                Assert.GreaterOrEqual(bonus.Value, 0f, $"Micro-bonus de {tree} negativo.");
            }
        }

        [Test]
        public void MicroBonus_Hybrid_NeverAboveFivePercent()
        {
            // Maior bonus (Lavrador 5%) em hibrido => 2.5%, ainda <= 5%.
            var profile = PlayerClassInference.GetProfile(Points(survival: 8, melee: 7));
            Assert.IsTrue(profile.IsHybrid);
            Assert.LessOrEqual(profile.PrimaryBonus.Value, 0.05f + 0.0001f);
            Assert.LessOrEqual(profile.SecondaryBonus.Value, 0.05f + 0.0001f);
            Assert.AreEqual(0.025f, profile.PrimaryBonus.Value, 0.0001f, "5% / 2 = 2.5%.");
        }

        // ───────────────────────── CA-4: determinismo (sem estado salvo) ────────────────────────

        [Test]
        public void CA4_Deterministic_SameInputsSameProfile()
        {
            // Recompute por evento e apos load reproduz IDENTICO o perfil: a funcao e pura.
            var p1 = PlayerClassInference.GetProfile(Points(melee: 8, ranged: 6));
            var p2 = PlayerClassInference.GetProfile(Points(melee: 8, ranged: 6));

            Assert.AreEqual(p1.TitleId, p2.TitleId);
            Assert.AreEqual(p1.IsHybrid, p2.IsHybrid);
            Assert.AreEqual(p1.DominantTree, p2.DominantTree);
            Assert.AreEqual(p1.SecondaryTree, p2.SecondaryTree);
            Assert.AreEqual(p1.PrimaryBonus.Value, p2.PrimaryBonus.Value, 0.0001f);
            Assert.AreEqual(p1.SecondaryBonus.Value, p2.SecondaryBonus.Value, 0.0001f);
        }

        [Test]
        public void CA4_RecomputeAfterRespec_FromZeroPoints_IsColono()
        {
            // Simula respec (volta a 0 pontos): perfil volta a Colono sem bonus, sem estado residual.
            var invested = PlayerClassInference.GetProfile(Points(melee: 10, ranged: 2));
            Assert.IsFalse(invested.IsColono);

            var afterRespec = PlayerClassInference.GetProfile(Points());
            Assert.IsTrue(afterRespec.IsColono, "Apos respec (0 pts) volta a Colono.");
            Assert.IsTrue(afterRespec.PrimaryBonus.IsEmpty, "Sem bonus residual apos respec.");
        }

        [Test]
        public void Colono_StaticProfile_HasNoBonusAndColonoTitle()
        {
            var colono = PlayerClassInference.Colono;
            Assert.IsTrue(colono.IsColono);
            Assert.AreEqual(PlayerClassInference.ColonoTitleId, colono.TitleId);
            Assert.IsTrue(colono.PrimaryBonus.IsEmpty);
            Assert.IsTrue(colono.SecondaryBonus.IsEmpty);
        }
    }
}
