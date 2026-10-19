using CindarsHope.Craft;
using CindarsHope.Foundation;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class Phase21EconomySustainTests
    {
        private const int EfficiencyBaseActionStaminaCost = 8;
        [TearDown]
        public void TearDown() => WorkStaminaCostModifierProvider.Source = null;

        [TestCase(1, 0.20f, 1, 7, 0.6f, -4.4f)]
        [TestCase(1, 0.20f, 2, 13, 0.2f, -2.8f)]
        [TestCase(1, 0.20f, 4, 26, 0.4f, 0.4f)]
        [TestCase(1, 0.20f, 6, 39, 0.6f, 3.6f)]
        [TestCase(3, 0.30f, 1, 6, 0.4f, -3.6f)]
        [TestCase(3, 0.30f, 2, 12, 0.8f, -1.2f)]
        [TestCase(3, 0.30f, 4, 23, 0.6f, 3.6f)]
        [TestCase(3, 0.30f, 6, 34, 0.4f, 8.4f)]
        public void MarcaEficiência_BreakEven_RealizaPreviewCommitDeterministico_NoCoordenadorDeFracao(
            int rank,
            float expectedReduction,
            int actionCount,
            int expectedCharged,
            float expectedCredit,
            float expectedNetAfterAtivacao)
        {
            var action = AssetDatabase.LoadAssetAtPath<SkillActionSO>(
                "Assets/_Game/Data/Skills/Actions/SkillAction_skill_crafting_marca_eficiencia.asset");
            Assert.That(action, Is.Not.Null, "A matriz precisa do asset canônico gerado.");
            Assert.That(action.EffectMagnitudeByRank.Length, Is.GreaterThanOrEqualTo(rank));
            float reduction = action.EffectMagnitudeByRank[rank - 1];
            Assert.That(reduction, Is.EqualTo(expectedReduction).Within(0.0001f));
            var host = new GameObject($"phase21-marca-r{rank}-h");
            var stationObject = new GameObject($"phase21-marca-r{rank}-station");
            const string stationInstanceId = "phase21-marca-station";

            try
            {
                var station = stationObject.AddComponent<CraftingPoint>();
                var runtime = host.AddComponent<EfficiencyMarkRuntimeCoordinator>();
                bool activated = runtime.Activate(stationInstanceId, station, Vector2.zero, 2.5f, 12f,
                    reduction);
                Assert.That(activated, Is.True);

                int totalCharged = 0;
                for (int i = 0; i < actionCount; i++)
                {
                    int cost = runtime.PreviewCost(WorkStaminaChannel.Crafting,
                        EfficiencyBaseActionStaminaCost,
                        0f,
                        0f,
                        stationInstanceId);
                    totalCharged += cost;
                    runtime.CommitSpend(WorkStaminaChannel.Crafting,
                        EfficiencyBaseActionStaminaCost,
                        cost,
                        0f,
                        0f,
                        stationInstanceId);
                }

                Assert.That(totalCharged, Is.EqualTo(expectedCharged));
                Assert.That(runtime.FractionalCredit, Is.EqualTo(expectedCredit).Within(0.0001f));

                float netAfterActivation = (actionCount * (float)EfficiencyBaseActionStaminaCost)
                                          - action.StaminaCost - totalCharged
                                          + runtime.FractionalCredit;
                Assert.That(netAfterActivation,
                    Is.EqualTo(expectedNetAfterAtivacao).Within(0.0001f),
                    $"Rank {rank} - {actionCount} actions");
            }
            finally
            {
                Object.DestroyImmediate(stationObject);
                Object.DestroyImmediate(host);
                WorkStaminaCostModifierProvider.Source = null;
            }
        }

        [Test]
        public void SurvivalUniqueUses_SaveLoadRespec_NaoRearmamLastBreathECampo()
        {
            var source = new SurvivalSkillState();
            Assert.That(source.BeginOrJoinEncounter("run-short", 2, "enemy-a"), Is.True);
            Assert.That(source.TryMarkCampUsed("run-short"), Is.True);
            Assert.That(source.TryArmLastBreath(4f), Is.True);
            Assert.That(source.TryConsumeLastBreathForActiveEncounter(), Is.True);
            var sourceProvider = new SurvivalSkillSectionProvider(source);
            var shortSave = (SurvivalSkillSaveData)sourceProvider.Capture(new GameSaveData());
            var restored = new SurvivalSkillState();
            restored.BeginRestore();
            restored.RestoreFromSaveData(shortSave, "run-short", 2);
            restored.EndRestore();

            Assert.That(restored.IsLastBreathConsumed(restored.ActiveEncounterId), Is.True);
            Assert.That(restored.TryConsumeLastBreathForActiveEncounter(), Is.False,
                "Último fôlego deve permanecer consumido após load.");
            Assert.That(restored.CampUsedRunId, Is.EqualTo("run-short"));

            var longRunData = new GameSaveData
            {
                SurvivalSkills = shortSave
            };
            var longRunRestore = new SurvivalSkillState();
            longRunRestore.RestoreFromSaveData((SurvivalSkillSaveData)longRunData.SurvivalSkills,
                "run-long", 2);

            Assert.That(longRunRestore.ActiveEncounterId, Is.Empty);
            Assert.That(longRunRestore.HasActiveEncounter, Is.False);
            Assert.That(longRunRestore.CampUsedRunId, Is.EqualTo("run-short"),
                "A marca de uso de Campo Seguro preserva identidade de run mesmo com contexto diferente.");

            var skillTree = new SkillTreeState(10);
            skillTree.FullRespec(restoredPoints: 10);

            Assert.That(restored.IsLastBreathConsumed(source.ActiveEncounterId), Is.True,
                "Full respec não deve rearmar flag de último fôlego.");
            Assert.That(restored.CampUsedRunId, Is.EqualTo("run-short"),
                "Respec de árvore não deve limpar estado de uso de sobrevivência.");
            Assert.That(skillTree.RespecCount, Is.EqualTo(1));
        }

        [Test]
        public void Thoren_SaveLoad_Respec_NaoRearmaCargaDiaria()
        {
            var source = new CraftingSkillState();
            Assert.That(source.TryReserve(11, "thoren-output"), Is.True);
            Assert.That(source.Commit(11, "thoren-output"), Is.True);
            Assert.That(source.TryReserve(12, "pending-other"), Is.True);

            Assert.That(source.IsChargeConsumed(11), Is.True);
            Assert.That(source.HasPendingReservation(12), Is.True);

            var section = new CraftingSkillSectionProvider(source);
            var captured = (CraftingSkillSaveData)section.Capture(null);

            var restored = new CraftingSkillState();
            var restoredSection = new CraftingSkillSectionProvider(restored);
            restoredSection.Restore(captured);

            Assert.That(restored.IsChargeConsumed(11), Is.True);
            Assert.That(restored.IsChargeConsumed(12), Is.False);
            Assert.That(restored.HasPendingReservation(12), Is.True);
            Assert.That(restored.TryReserve(11, "thoren-retry"), Is.False,
                "Carga já consumida não deve rearmar após load.");
            Assert.That(restored.Commit(11, "thoren-retry"), Is.False,
                "Retry de commit com token novo deve falhar.");
            Assert.That(restored.Commit(12, "thoren-output-other"), Is.False,
                "Token inválido não pode comprometer reserva persistida.");
            Assert.That(restored.Commit(12, "pending-other"), Is.True,
                "Reserva persistida no save precisa poder ser fechada por commit legítimo.");

            var olderDayPayload = new CraftingSkillSaveData
            {
                CurrentDayIndex = 11,
                ConsumedDayIndices = { 11 }
            };
            restored.RestoreFromSaveData(olderDayPayload);

            Assert.That(restored.IsChargeConsumed(11), Is.True,
                "Restore mais antigo não pode rearmar carga já consumida no dia.");

            var skillTree = new SkillTreeState(12);
            skillTree.Purchase("crafting_capstone_master_artisan", 1, "crafting");
            skillTree.RankUp("crafting_capstone_master_artisan", "crafting");
            skillTree.FullRespec(restoredPoints: 12);

            Assert.That(restored.IsChargeConsumed(11), Is.True,
                "Full respec não pode rearmar uso de carga diária.");
            Assert.That(skillTree.RespecCount, Is.EqualTo(1));
        }

    }
}

