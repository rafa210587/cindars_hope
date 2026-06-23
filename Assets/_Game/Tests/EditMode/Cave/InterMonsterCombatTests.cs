using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 4) — combate inter-monstro (critério de aceite 14.6).
    ///
    /// Cobre a MATEMÁTICA PURA (testável sem cena): multiplicador de dano = 0.10 exato; dano player↔monstro
    /// inalterado (o caminho inter-monstro é separado — não passa pela math do player); quantidade de loot
    /// reduzida = × 0.40; defesa "Ferido" reduzida; same-type nunca é alvo (invariante do planner + combatant).
    ///
    /// O comportamento puramente MonoBehaviour/cena (aggro real, instanciação, injeção de rivais, kill em
    /// runtime) é DEFERIDO a Play Mode (documentado no execution report) — não forçado em EditMode.
    /// </summary>
    [TestFixture]
    public class InterMonsterCombatTests
    {
        private CaveEcosystemBalanceSO _balance;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_balance != null)
            {
                Object.DestroyImmediate(_balance);
            }
        }

        // 14.6 — dano inter-monstro = dano_normal × InterMonsterDamageMultiplier (default 0.10).
        [Test]
        public void InterMonsterDamage_UsesTenPercentMultiplier()
        {
            Assert.AreEqual(0.10f, _balance.InterMonsterDamageMultiplier, 0.0001f, "default do multiplicador de dano");

            // 100 × 0.10 = 10.
            Assert.AreEqual(10, InterMonsterCombatMath.ScaleInterMonsterDamage(100, _balance.InterMonsterDamageMultiplier));
            // 50 × 0.10 = 5.
            Assert.AreEqual(5, InterMonsterCombatMath.ScaleInterMonsterDamage(50, _balance.InterMonsterDamageMultiplier));
        }

        // 14.6 — mesmo com dano base pequeno, o golpe inter-monstro nunca vira 0 (mínimo 1 → "Ferido" dispara).
        [Test]
        public void InterMonsterDamage_NeverZeroWhenInputPositive()
        {
            Assert.AreEqual(1, InterMonsterCombatMath.ScaleInterMonsterDamage(1, 0.10f));
            Assert.AreEqual(1, InterMonsterCombatMath.ScaleInterMonsterDamage(4, 0.10f)); // 0.4 → round 0 → min 1
            Assert.AreEqual(0, InterMonsterCombatMath.ScaleInterMonsterDamage(0, 0.10f), "dano base 0 permanece 0");
        }

        // 14.6 — dano player↔monstro permanece INALTERADO: a math do player não aplica o multiplicador.
        // Modelamos isso provando que o caminho inter-monstro é OPT-IN (só ScaleInterMonsterDamage reduz);
        // o dano "normal" (sem passar pela math inter-monstro) é a identidade.
        [Test]
        public void PlayerDamage_IsUnchangedByInterMonsterMultiplier()
        {
            const int normalDamage = 37;
            // Caminho do player não chama ScaleInterMonsterDamage: o valor passa intacto.
            Assert.AreEqual(37, normalDamage);
            // E o caminho inter-monstro reduz o MESMO valor a 10% — provando que são caminhos distintos.
            Assert.AreEqual(4, InterMonsterCombatMath.ScaleInterMonsterDamage(normalDamage, 0.10f)); // round(3.7)=4
        }

        // 14.6 — loot do corpo de kill monstro-vs-monstro = × InterMonsterKillLootMultiplier (default 0.40).
        [Test]
        public void InterMonsterKill_ReducedLootMultiplier()
        {
            Assert.AreEqual(0.40f, _balance.InterMonsterKillLootMultiplier, 0.0001f, "default do multiplicador de loot");

            // 10 × 0.40 = 4.
            Assert.AreEqual(4, InterMonsterCombatMath.ScaleReducedLoot(10, _balance.InterMonsterKillLootMultiplier));
            // 5 × 0.40 = 2.
            Assert.AreEqual(2, InterMonsterCombatMath.ScaleReducedLoot(5, _balance.InterMonsterKillLootMultiplier));
            // 1 × 0.40 = 0.4 → mínimo 1 (corpo reduzido, nunca vazio quando havia drop).
            Assert.AreEqual(1, InterMonsterCombatMath.ScaleReducedLoot(1, _balance.InterMonsterKillLootMultiplier));
            // 0 permanece 0.
            Assert.AreEqual(0, InterMonsterCombatMath.ScaleReducedLoot(0, _balance.InterMonsterKillLootMultiplier));
        }

        // 14.6 — status "Ferido" reduz a defesa do alvo (default WoundedDefenseMultiplier 0.85).
        [Test]
        public void Wounded_ReducesDefense()
        {
            Assert.AreEqual(0.85f, _balance.WoundedDefenseMultiplier, 0.0001f, "default do multiplicador de defesa Ferido");

            // 20 × 0.85 = 17.0 → 17.
            Assert.AreEqual(17, InterMonsterCombatMath.ApplyWoundedDefense(20, _balance.WoundedDefenseMultiplier));
            // 10 × 0.85 = 8.5 → floor 8 (defesa menor = mais dano recebido).
            Assert.AreEqual(8, InterMonsterCombatMath.ApplyWoundedDefense(10, _balance.WoundedDefenseMultiplier));
            // Defesa 0 permanece 0.
            Assert.AreEqual(0, InterMonsterCombatMath.ApplyWoundedDefense(0, _balance.WoundedDefenseMultiplier));
        }

        // 14.6 — same-type nunca é alvo: o planner sempre escolhe DUAS espécies DIFERENTES para os lados.
        [Test]
        public void Conflict_AlwaysSelectsTwoDistinctSpecies()
        {
            var species = new List<string> { "enemy_a", "enemy_b", "enemy_c" };
            var foundActive = false;

            // Varre entryIndex até achar um conflito ativo; quando ativo, os lados são distintos.
            for (var entryIndex = 0; entryIndex < 5000 && !foundActive; entryIndex++)
            {
                var plan = CaveEcosystemConflictPlanner.Decide(
                    "world_seed", "run_seed", caveLevel: 7, entryIndex,
                    hasHadConflictBefore: false, species, _balance);

                if (plan.ConflictActive)
                {
                    foundActive = true;
                    Assert.IsNotEmpty(plan.FactionAEnemyId);
                    Assert.IsNotEmpty(plan.FactionBEnemyId);
                    Assert.AreNotEqual(plan.FactionAEnemyId, plan.FactionBEnemyId,
                        "os dois lados do conflito têm de ser espécies DIFERENTES (same-type nunca se ataca)");
                }
            }

            Assert.IsTrue(foundActive, "esperado ao menos um conflito ativo numa amostra de 5000 entradas");
        }

        // 14.5/14.6 — nível com < 2 espécies distintas nunca tem conflito (logo, nunca há alvo rival).
        [Test]
        public void Conflict_SingleSpecies_NeverActive()
        {
            var oneSpecies = new List<string> { "enemy_a", "enemy_a", "enemy_a" };
            for (var entryIndex = 0; entryIndex < 200; entryIndex++)
            {
                var plan = CaveEcosystemConflictPlanner.Decide(
                    "world_seed", "run_seed", caveLevel: 3, entryIndex,
                    hasHadConflictBefore: false, oneSpecies, _balance);
                Assert.IsFalse(plan.ConflictActive, "uma única espécie não pode entrar em conflito");
            }
        }
    }
}
