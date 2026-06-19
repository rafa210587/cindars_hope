using NUnit.Framework;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_24 CA-3 — named elites are decided by a DETERMINISTIC StableHash of the cave seed
    /// (8% of slots from level 6+). Revisiting the same run reproduces the SAME elites in the SAME
    /// slots (ADR-0005 / cave-stable-run). Also covers the +25% stat math and per-affix modifiers.
    /// </summary>
    [TestFixture]
    public class EliteAffixTests
    {
        private const string World = "world_seed_alpha";
        private const string Run = "run_seed_001";

        // ── Determinism (CA-3) ──────────────────────────────────────────────────────────────────

        [Test]
        public void TryResolveElite_IsDeterministic_AcrossRepeatedCalls()
        {
            // Same seed inputs must always yield the same elite/affix decision (stable run).
            for (int slot = 0; slot < 64; slot++)
            {
                bool first = EliteAffixRules.TryResolveElite(World, Run, 12, slot, "enemy_wisp", out var affixA);
                bool second = EliteAffixRules.TryResolveElite(World, Run, 12, slot, "enemy_wisp", out var affixB);

                Assert.AreEqual(first, second, $"Elite decision for slot {slot} must be stable.");
                Assert.AreEqual(affixA, affixB, $"Affix for slot {slot} must be stable.");
            }
        }

        [Test]
        public void TryResolveElite_DifferentRunSeed_CanChangeOutcome()
        {
            // A different run seed is allowed to reroll (new run = new composition). We only assert
            // that the two runs are not forced to be identical across all slots (sanity, not equality).
            int diffs = 0;
            for (int slot = 0; slot < 200; slot++)
            {
                EliteAffixRules.TryResolveElite(World, "run_A", 20, slot, "enemy_brute", out var a);
                EliteAffixRules.TryResolveElite(World, "run_B", 20, slot, "enemy_brute", out var b);
                if (a != b) diffs++;
            }

            Assert.Greater(diffs, 0, "Different run seeds should produce at least some different elite rolls.");
        }

        // ── Level gating (CA-3: "a partir do nível 6") ──────────────────────────────────────────

        [Test]
        public void TryResolveElite_BelowMinLevel_NeverElite()
        {
            for (int level = 0; level < EliteAffixRules.MinEliteCaveLevel; level++)
            {
                for (int slot = 0; slot < 100; slot++)
                {
                    bool isElite = EliteAffixRules.TryResolveElite(World, Run, level, slot, "enemy_x", out var affix);
                    Assert.IsFalse(isElite, $"No elites allowed below level {EliteAffixRules.MinEliteCaveLevel} (level {level}, slot {slot}).");
                    Assert.AreEqual(EliteAffix.None, affix);
                }
            }
        }

        [Test]
        public void TryResolveElite_AtAndAboveMinLevel_ProducesSomeElites()
        {
            int eliteCount = 0;
            int total = 0;
            for (int slot = 0; slot < 1000; slot++)
            {
                total++;
                if (EliteAffixRules.TryResolveElite(World, Run, EliteAffixRules.MinEliteCaveLevel, slot, "enemy_x", out _))
                {
                    eliteCount++;
                }
            }

            Assert.Greater(eliteCount, 0, "From the minimum level, some slots should become elite.");
            // ~8% expected; allow a wide tolerance band so the test is not flaky on hash distribution.
            float ratio = (float)eliteCount / total;
            Assert.That(ratio, Is.InRange(0.02f, 0.18f), $"Elite ratio {ratio:P1} should be roughly the 8% target.");
        }

        [Test]
        public void TryResolveElite_WhenElite_AffixIsNeverNone()
        {
            for (int slot = 0; slot < 500; slot++)
            {
                if (EliteAffixRules.TryResolveElite(World, Run, 30, slot, "enemy_y", out var affix))
                {
                    Assert.AreNotEqual(EliteAffix.None, affix, $"An elite slot ({slot}) must have a real affix.");
                }
            }
        }

        // ── Stat math (+25% base) ───────────────────────────────────────────────────────────────

        [Test]
        public void ApplyEliteStatBonus_AddsTwentyFivePercent_Rounded()
        {
            Assert.AreEqual(13, EliteAffixRules.ApplyEliteStatBonus(10), "10 * 1.25 = 12.5 -> 13 (round away from zero).");
            Assert.AreEqual(125, EliteAffixRules.ApplyEliteStatBonus(100));
            Assert.AreEqual(0, EliteAffixRules.ApplyEliteStatBonus(0), "Zero stays zero.");
            Assert.AreEqual(-5, EliteAffixRules.ApplyEliteStatBonus(-5), "Non-positive input passed through unchanged.");
        }

        [Test]
        public void ResolveEliteDefense_ArmoredStacksFiftyPercentOnTopOfBaseBonus()
        {
            // base 8 -> +25% = 10 -> Armored +50% = 15
            Assert.AreEqual(15, EliteAffixRules.ResolveEliteDefense(8, EliteAffix.Armored));
            // Non-armored elite only gets the +25% base bonus.
            Assert.AreEqual(10, EliteAffixRules.ResolveEliteDefense(8, EliteAffix.Frenzied));
        }

        // ── Per-affix behavioural modifiers ─────────────────────────────────────────────────────

        [Test]
        public void ResolveAttackCadenceFactor_FrenziedIsThirtyPercentFaster()
        {
            float frenzied = EliteAffixRules.ResolveAttackCadenceFactor(EliteAffix.Frenzied);
            Assert.That(frenzied, Is.EqualTo(1f / 1.30f).Within(1e-4), "Frenzied divides timers by 1.30.");
            Assert.Less(frenzied, 1f, "Frenzied cadence factor must shorten timers.");
            Assert.AreEqual(1f, EliteAffixRules.ResolveAttackCadenceFactor(EliteAffix.Armored), "Non-Frenzied keeps cadence 1.");
        }

        [Test]
        public void ResolveLifestealHeal_VampiricHealsTwentyFivePercent()
        {
            Assert.AreEqual(5, EliteAffixRules.ResolveLifestealHeal(EliteAffix.Vampiric, 20), "25% of 20 = 5.");
            Assert.AreEqual(3, EliteAffixRules.ResolveLifestealHeal(EliteAffix.Vampiric, 10), "25% of 10 = 2.5 -> 3.");
            Assert.AreEqual(0, EliteAffixRules.ResolveLifestealHeal(EliteAffix.Frenzied, 20), "Non-Vampiric heals nothing.");
            Assert.AreEqual(0, EliteAffixRules.ResolveLifestealHeal(EliteAffix.Vampiric, 0), "No damage -> no heal.");
        }

        [Test]
        public void ResolveVolatileExplosionDamage_CappedAtTwentyFivePercentPlayerMaxHp()
        {
            // player maxHP 100 -> cap = 25. Raw above the cap is clamped; below the cap is unchanged.
            Assert.AreEqual(25, EliteAffixRules.ResolveVolatileExplosionDamage(80, 100), "Clamped to 25% of 100.");
            Assert.AreEqual(10, EliteAffixRules.ResolveVolatileExplosionDamage(10, 100), "Below cap stays.");
            Assert.AreEqual(0, EliteAffixRules.ResolveVolatileExplosionDamage(-3, 100), "Negative raw -> 0.");
            Assert.AreEqual(50, EliteAffixRules.ResolveVolatileExplosionDamage(50, 0), "Unknown maxHP -> no cap.");
        }

        [Test]
        public void StatusAndDeathFlags_MatchAffix()
        {
            Assert.IsTrue(EliteAffixRules.ExplodesOnDeath(EliteAffix.Volatile));
            Assert.IsFalse(EliteAffixRules.ExplodesOnDeath(EliteAffix.Warded));
            Assert.IsTrue(EliteAffixRules.ResistsFirstStatus(EliteAffix.Warded));
            Assert.IsFalse(EliteAffixRules.ResistsFirstStatus(EliteAffix.Volatile));
        }

        [Test]
        public void BuildEliteDisplayName_PrefixesBaseName()
        {
            Assert.AreEqual("Frenzied Veilkin Scout", EliteAffixRules.BuildEliteDisplayName(EliteAffix.Frenzied, "Veilkin Scout"));
            Assert.AreEqual("Vampiric Wisp", EliteAffixRules.BuildEliteDisplayName(EliteAffix.Vampiric, "Wisp"));
            Assert.AreEqual("Mirelurk", EliteAffixRules.BuildEliteDisplayName(EliteAffix.None, "Mirelurk"), "No affix -> unchanged name.");
            Assert.AreEqual("Enemy", EliteAffixRules.BuildEliteDisplayName(EliteAffix.None, ""), "Empty base name -> safe default.");
        }
    }
}
