using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Combat.Telemetry;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_59 — telemetria de combate (lógica PURA, sem Unity/PlayMode):
    /// agregação de TTK por enemyId, % de dano recebido, avaliador de alvos §6-§7 nas bordas,
    /// reset por nível, serialização do relatório (shape estável) e o lookup do bestiário.
    /// Cobre CA-1..CA-4 no plano determinístico (a coleta runtime real via eventos fica para o
    /// relatório de Play Mode do lote final).
    /// </summary>
    public class CombatTelemetryTests
    {
        // ── lookup fake p/ isolar o avaliador/sessão do catálogo (CA-3) ──────────────────────────
        private sealed class FakeClassLookup : ICreatureClassLookup
        {
            private readonly Dictionary<string, TelemetryCreatureClass> _map = new Dictionary<string, TelemetryCreatureClass>();
            public FakeClassLookup Add(string id, TelemetryCreatureClass c) { _map[id] = c; return this; }
            public TelemetryCreatureClass GetClass(string enemyId) =>
                _map.TryGetValue(enemyId, out var c) ? c : TelemetryCreatureClass.Unknown;
        }

        // ──────────────────────────────────────────────── CA-2: TTK por inimigo

        [Test]
        public void Ttk_SingleKill_IsEndMinusFirstDamage()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup().Add("enemy_a", TelemetryCreatureClass.Common));
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordEnemyDamaged("enemy_a", 10, "physical", 100f); // t0 = primeiro dano
            session.RecordEnemyDamaged("enemy_a", 10, "physical", 102f); // dano posterior não move t0
            session.RecordEnemyKilled("enemy_a", 104f);                  // t1 = morte

            Assert.AreEqual(4f, session.GetTtkAvg("enemy_a"), 0.0001f, "TTK = 104 - 100.");
            Assert.AreEqual(1, session.GetTtkSampleCount("enemy_a"));
        }

        [Test]
        public void Ttk_MultipleKills_AveragesMinMax()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup().Add("enemy_a", TelemetryCreatureClass.Common));
            session.BeginLevel(1, 1, "run", 0f);

            // kill 1: TTK = 4
            session.RecordEnemyDamaged("enemy_a", 5, "physical", 10f);
            session.RecordEnemyKilled("enemy_a", 14f);
            // kill 2 (mesma id, nova instância): TTK = 6
            session.RecordEnemyDamaged("enemy_a", 5, "physical", 20f);
            session.RecordEnemyKilled("enemy_a", 26f);

            Assert.AreEqual(5f, session.GetTtkAvg("enemy_a"), 0.0001f, "Média de 4 e 6.");
            Assert.AreEqual(4f, session.GetTtkMin("enemy_a"), 0.0001f);
            Assert.AreEqual(6f, session.GetTtkMax("enemy_a"), 0.0001f);
            Assert.AreEqual(2, session.GetTtkSampleCount("enemy_a"));
        }

        [Test]
        public void Ttk_KillWithoutDamage_IsDiscardedAsGap()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup().Add("enemy_a", TelemetryCreatureClass.Common));
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordEnemyKilled("enemy_a", 50f); // morte sem dano registrado

            Assert.AreEqual(0, session.GetTtkSampleCount("enemy_a"), "Sem amostra de TTK.");
            var report = session.BuildReport("now", null);
            var entry = report.Kills.Find(k => k.EnemyId == "enemy_a");
            Assert.IsNotNull(entry, "Kill aparece no relatório.");
            Assert.AreEqual("NO_TARGET", entry.TtkEvaluation, "Sem TTK => sem avaliação.");
            Assert.IsTrue(report.Gaps.Exists(g => g.Contains("enemy_a")), "Gap de kill sem dano registrado.");
        }

        // ──────────────────────────────────────────────── CA-3: avaliador de alvos nas bordas

        [Test]
        public void Evaluator_Common_Ttk_Borders()
        {
            // §6 Common 3-6s (bordas inclusivas)
            Assert.AreEqual(TelemetryEvaluation.Below,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, 2.9f, 1), "2.9s < 3 => BELOW.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, 3.0f, 1), "3.0s = min => WITHIN.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, 6.0f, 1), "6.0s = max => WITHIN.");
            Assert.AreEqual(TelemetryEvaluation.Above,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, 6.1f, 1), "6.1s > 6 => ABOVE.");
        }

        [Test]
        public void Evaluator_Elite_Miniboss_Boss_Ttk_Bands()
        {
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Elite, 12f, 1), "Elite min 12s.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Elite, 20f, 1), "Elite max 20s.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Miniboss, 45f, 1), "Miniboss min 45s.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Miniboss, 90f, 1), "Miniboss max 90s.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Boss, 120f, 1), "Boss min 120s.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Boss, 240f, 1), "Boss max 240s.");
            Assert.AreEqual(TelemetryEvaluation.Above,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Boss, 241f, 1), "Boss > 240s => ABOVE.");
        }

        [Test]
        public void Evaluator_NoSample_NoTarget()
        {
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Common, 4f, 0), "Sem amostra => NO_TARGET.");
            Assert.AreEqual(TelemetryEvaluation.NoTarget,
                TelemetryTargetEvaluator.EvaluateTtk(TelemetryCreatureClass.Unknown, 4f, 5), "Classe Unknown => NO_TARGET.");
        }

        [Test]
        public void Evaluator_DamageTakenPct_Borders()
        {
            // §7 Common 4-8% do HP máx
            Assert.AreEqual(TelemetryEvaluation.Below,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Common, 3.9f, 1), "3.9% < 4 => BELOW.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Common, 4f, 1), "4% = min => WITHIN.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Common, 8f, 1), "8% = max => WITHIN.");
            Assert.AreEqual(TelemetryEvaluation.Above,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Common, 8.1f, 1), "8.1% > 8 => ABOVE.");
            // Boss telegrafado 18-30%
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Boss, 18f, 1), "Boss min 18%.");
            Assert.AreEqual(TelemetryEvaluation.WithinTarget,
                TelemetryTargetEvaluator.EvaluateDamageTakenPct(TelemetryCreatureClass.Boss, 30f, 1), "Boss max 30%.");
        }

        // ──────────────────────────────────────────────── % de dano recebido (HPChanged + PlayerDamaged)

        [Test]
        public void DamageTaken_PctUsesLastKnownMaxHp()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup().Add("src", TelemetryCreatureClass.Common));
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordMaxHp(200);                 // HP máx conhecido = 200
            session.RecordPlayerDamaged(10, "src", 5f); // 10/200 = 5%

            Assert.AreEqual(10, session.DamageTakenTotal);
            Assert.AreEqual(5f, session.DamageTakenPctAvg, 0.0001f, "10 de 200 = 5%.");
        }

        [Test]
        public void DamageTaken_WithoutMaxHp_RecordsTotalButGapsPct()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup());
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordPlayerDamaged(10, "src", 5f); // MaxHP desconhecido

            Assert.AreEqual(10, session.DamageTakenTotal, "Total ainda conta.");
            Assert.AreEqual(0f, session.DamageTakenPctAvg, 0.0001f, "Sem MaxHP => % não computado.");
            var report = session.BuildReport("now", null);
            Assert.IsTrue(report.Gaps.Exists(g => g.Contains("MaxHP")), "Gap de % sem MaxHP.");
        }

        // ──────────────────────────────────────────────── stamina / mana / contadores

        [Test]
        public void Stamina_OnlyNegativeDeltasCountAsSpent()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup());
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordStamina(100, 1f); // baseline (não conta)
            session.RecordStamina(60, 2f);  // gastou 40
            session.RecordStamina(80, 3f);  // regen (não conta)
            session.RecordStamina(70, 4f);  // gastou 10

            Assert.AreEqual(50, session.StaminaSpent, "40 + 10 = 50.");
        }

        [Test]
        public void Mana_OnlyNegativeDeltasCountAsSpent()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup());
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordMana(50, 1f); // baseline
            session.RecordMana(30, 2f); // gastou 20
            session.RecordMana(45, 3f); // regen

            Assert.AreEqual(20, session.MpSpent);
        }

        [Test]
        public void Counters_AreAggregated()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup());
            session.BeginLevel(1, 1, "run", 0f);

            session.RecordDodge(1f);
            session.RecordDodge(2f);
            session.RecordBlock(2.5f);
            session.RecordPerfectBlock(3f);
            session.RecordPostureBreak(4f);
            session.RecordChargedAttack(5f);
            session.RecordDeath(6f);

            Assert.AreEqual(2, session.Dodges);
            Assert.AreEqual(1, session.Blocks);
            Assert.AreEqual(1, session.PerfectBlocks);
            Assert.AreEqual(1, session.PostureBreaks);
            Assert.AreEqual(1, session.ChargedAttacks);
            Assert.AreEqual(1, session.Deaths);
        }

        // ──────────────────────────────────────────────── reset por nível (CA-4 / risco de transição)

        [Test]
        public void NewSession_ResetsAggregatesPerLevel()
        {
            var lookup = new FakeClassLookup().Add("enemy_a", TelemetryCreatureClass.Common);

            var level1 = new CombatTelemetrySession(lookup);
            level1.BeginLevel(1, 1, "run", 0f);
            level1.RecordEnemyDamaged("enemy_a", 5, "physical", 10f);
            level1.RecordEnemyKilled("enemy_a", 14f);
            Assert.AreEqual(1, level1.KillEntryCount);

            // novo nível = nova sessão (como o service faz em CaveLevelEnteredEvent)
            var level2 = new CombatTelemetrySession(lookup);
            level2.BeginLevel(2, 1, "run", 100f);
            Assert.AreEqual(0, level2.KillEntryCount, "Nível novo zera as métricas.");
            Assert.AreEqual(0, level2.DamageDealtTotal);
        }

        // ──────────────────────────────────────────────── CA-4: serialização do relatório (shape estável)

        [Test]
        public void Report_Serializes_WithStableShapeVersion()
        {
            var session = new CombatTelemetrySession(new FakeClassLookup().Add("enemy_a", TelemetryCreatureClass.Common));
            session.BeginLevel(7, 4, "run-seed-xyz", 0f);
            session.RecordMaxHp(100);
            session.RecordEnemyDamaged("enemy_a", 20, "fire", 10f);
            session.RecordPlayerDamaged(6, "enemy_a", 11f); // 6%
            session.RecordEnemyKilled("enemy_a", 14f);      // TTK = 4 => WITHIN para Common

            var report = session.BuildReport("2026-06-19T00:00:00Z", null);
            var json = CombatTelemetryWriter.Serialize(report);

            Assert.AreEqual(1, report.ShapeVersion, "Shape version fixado.");
            StringAssert.Contains("\"ShapeVersion\": 1", json, "Versão do shape no payload.");
            StringAssert.Contains("\"CaveLevel\": 7", json);
            StringAssert.Contains("enemy_a", json);
            StringAssert.Contains("WITHIN_TARGET", json, "Avaliação legível no JSON.");

            var entry = report.Kills.Find(k => k.EnemyId == "enemy_a");
            Assert.IsNotNull(entry);
            Assert.AreEqual("Common", entry.CreatureClass);
            Assert.AreEqual("WITHIN_TARGET", entry.TtkEvaluation);
        }

        [Test]
        public void Writer_BuildFileName_IsDeterministicAndSafe()
        {
            var name = CombatTelemetryWriter.BuildFileName("run/seed:1", 12, "20260619_010203");
            Assert.AreEqual("combat_20260619_010203_run_seed_1_level12.json", name, "Path-unsafe chars saneados.");

            var sessionName = CombatTelemetryWriter.BuildFileName("run", -1, "ts");
            StringAssert.Contains("levelsession", sessionName, "Nível -1 => 'session'.");
        }

        // ──────────────────────────────────────────────── lookup do bestiário (F33, sem tabela paralela)

        [Test]
        public void BestiaryLookup_ClassifiesByRoleAndFlags()
        {
            var fichas = new List<BestiaryCreatureDef>
            {
                new BestiaryCreatureDef { EnemyId = "common_1", Role = EnemyRole.Chaser },
                new BestiaryCreatureDef { EnemyId = "elite_1", Role = EnemyRole.Elite },
                new BestiaryCreatureDef { EnemyId = "mini_1", Role = EnemyRole.Chaser, IsMiniBoss = true },
                new BestiaryCreatureDef { EnemyId = "boss_1", Role = EnemyRole.Boss, IsBoss = true },
            };
            var lookup = new BestiaryTelemetryClassLookup(fichas);

            Assert.AreEqual(TelemetryCreatureClass.Common, lookup.GetClass("common_1"));
            Assert.AreEqual(TelemetryCreatureClass.Elite, lookup.GetClass("elite_1"));
            Assert.AreEqual(TelemetryCreatureClass.Miniboss, lookup.GetClass("mini_1"));
            Assert.AreEqual(TelemetryCreatureClass.Boss, lookup.GetClass("boss_1"));
            Assert.AreEqual(TelemetryCreatureClass.Unknown, lookup.GetClass("does_not_exist"));
        }

        [Test]
        public void BestiaryLookup_RealCatalog_ResolvesKnownIds()
        {
            // Usa o catálogo F33 real para garantir que o adaptador não é tabela paralela.
            var lookup = new BestiaryTelemetryClassLookup();
            Assert.AreEqual(TelemetryCreatureClass.Boss, lookup.GetClass("boss_ithryndor"), "Final 4/4 é Boss.");
        }
    }
}
