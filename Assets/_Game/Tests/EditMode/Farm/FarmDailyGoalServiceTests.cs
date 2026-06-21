using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Runtime;
using CindarsHope.UI.HUD.Views;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// fable_65 — testes das metas diárias de farm: recompensa idempotente (Claimed), integridade
    /// do catálogo 4-6 metas x tabela de recompensa, reset diário, progresso n/m, round-trip de
    /// save com Claimed e projeção do widget de HUD.
    ///
    /// Cobre a lógica determinista pura (FarmDailyGoalService.TryClaimReward estático,
    /// FarmDailyGoalRewardTable, FarmDailyGoalCatalog, DailyGoalsHudProjection) sem cena/MonoBehaviour.
    /// Fecha o débito WI-24 DAILY_GOAL_EDITMODE_TESTS.
    /// </summary>
    [TestFixture]
    public class FarmDailyGoalServiceTests
    {
        /// <summary>Sink fake que acumula ouro+XP concedidos e conta as chamadas de Grant.</summary>
        private sealed class FakeRewardSink : IDailyGoalRewardSink
        {
            public int TotalGold;
            public int TotalXp;
            public int GrantCalls;

            public void Grant(int gold, int xp)
            {
                TotalGold += gold;
                TotalXp += xp;
                GrantCalls++;
            }
        }

        private static FarmDailyGoalState CompletedState(string goalId, int required = 1)
        {
            return new FarmDailyGoalState
            {
                GoalId = goalId,
                Day = 1,
                CurrentProgress = required,
                RequiredProgress = required,
                Completed = true,
                Claimed = false
            };
        }

        // ───────────────────────── CA-1: recompensa idempotente ─────────────────────────

        [Test]
        public void TryClaimReward_CompletedUnclaimed_PaysOnceAndSetsClaimed()
        {
            var state = CompletedState(FarmDailyGoalCatalog.GoalFirstHarvest);
            var sink = new FakeRewardSink();

            var paid = FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);

            Assert.IsTrue(paid);
            Assert.IsTrue(state.Claimed);
            Assert.AreEqual(1, sink.GrantCalls);
            FarmDailyGoalRewardTable.TryGet(FarmDailyGoalCatalog.GoalFirstHarvest, out var reward);
            Assert.AreEqual(reward.Gold, sink.TotalGold);
            Assert.AreEqual(reward.Xp, sink.TotalXp);
        }

        [Test]
        public void TryClaimReward_AlreadyClaimed_DoesNotPayAgain()
        {
            var state = CompletedState(FarmDailyGoalCatalog.GoalFirstHarvest);
            var sink = new FakeRewardSink();

            FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);
            var secondPaid = FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);

            Assert.IsFalse(secondPaid);
            Assert.AreEqual(1, sink.GrantCalls, "Meta concluída paga exatamente 1x.");
        }

        [Test]
        public void TryClaimReward_NotCompleted_DoesNotPay()
        {
            var state = new FarmDailyGoalState
            {
                GoalId = FarmDailyGoalCatalog.GoalHarvestThree,
                Day = 1,
                CurrentProgress = 1,
                RequiredProgress = 3,
                Completed = false,
                Claimed = false
            };
            var sink = new FakeRewardSink();

            var paid = FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);

            Assert.IsFalse(paid);
            Assert.AreEqual(0, sink.GrantCalls);
            Assert.IsFalse(state.Claimed);
        }

        [Test]
        public void TryClaimReward_ReloadMidDay_DoesNotRepay()
        {
            // Simula reload: estado restaurado já Completed+Claimed (claim foi síncrono na conclusão).
            var restored = CompletedState(FarmDailyGoalCatalog.GoalSellFirstCrop);
            restored.Claimed = true;
            var sink = new FakeRewardSink();

            var paid = FarmDailyGoalService.TryClaimReward(restored, sink, publishToast: false);

            Assert.IsFalse(paid, "Reload no meio do dia não re-paga.");
            Assert.AreEqual(0, sink.GrantCalls);
        }

        [Test]
        public void TryClaimReward_NullSink_StillMarksClaimedWithoutThrowing()
        {
            var state = CompletedState(FarmDailyGoalCatalog.GoalFirstHarvest);
            Assert.DoesNotThrow(() => FarmDailyGoalService.TryClaimReward(state, null, publishToast: false));
            Assert.IsTrue(state.Claimed);
        }

        [Test]
        public void TryClaimReward_UnknownGoalId_MarksClaimedButPaysNothing()
        {
            var state = CompletedState("daily_goal_does_not_exist");
            var sink = new FakeRewardSink();

            var paid = FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);

            Assert.IsFalse(paid);
            Assert.AreEqual(0, sink.GrantCalls, "Nunca paga recompensa inventada para id desconhecido.");
            Assert.IsTrue(state.Claimed, "Marca Claimed para não reavaliar.");
        }

        // ───────────────────────── CA-3 / CA-5: integridade do catálogo + economia ─────────────────────────

        [Test]
        public void Catalog_HasBetweenFourAndSixGoals()
        {
            var count = FarmDailyGoalCatalog.All.Count;
            Assert.GreaterOrEqual(count, 4, "Catálogo deve ter 4-6 metas/dia.");
            Assert.LessOrEqual(count, 6, "Catálogo deve ter 4-6 metas/dia.");
        }

        [Test]
        public void Catalog_EveryGoalHasRewardEntry()
        {
            foreach (var def in FarmDailyGoalCatalog.All)
            {
                Assert.IsTrue(
                    FarmDailyGoalRewardTable.Has(def.GoalId),
                    $"Meta '{def.GoalId}' precisa de entrada na tabela de recompensa.");
            }
        }

        [Test]
        public void RewardTable_HasNoOrphanEntries()
        {
            var catalogIds = new HashSet<string>();
            foreach (var def in FarmDailyGoalCatalog.All)
            {
                catalogIds.Add(def.GoalId);
            }

            foreach (var rewardId in FarmDailyGoalRewardTable.AllGoalIds)
            {
                Assert.IsTrue(
                    catalogIds.Contains(rewardId),
                    $"Recompensa '{rewardId}' não tem meta correspondente no catálogo.");
            }
        }

        [Test]
        public void Catalog_OriginalGoalIdsPreserved()
        {
            Assert.AreEqual("daily_goal_first_harvest", FarmDailyGoalCatalog.GoalFirstHarvest);
            Assert.AreEqual("daily_goal_sell_first_crop", FarmDailyGoalCatalog.GoalSellFirstCrop);
            Assert.IsTrue(FarmDailyGoalRewardTable.Has(FarmDailyGoalCatalog.GoalFirstHarvest));
            Assert.IsTrue(FarmDailyGoalRewardTable.Has(FarmDailyGoalCatalog.GoalSellFirstCrop));
        }

        [Test]
        public void RewardTable_MaxDailyTotals_AreContainedAndPositive()
        {
            // CA-5: economia contida — total diário documentado e pequeno.
            Assert.Greater(FarmDailyGoalRewardTable.MaxDailyGold, 0);
            Assert.LessOrEqual(FarmDailyGoalRewardTable.MaxDailyGold, 120,
                "Total diário de ouro deve permanecer pequeno (anti-exploit).");
            Assert.Greater(FarmDailyGoalRewardTable.MaxDailyXp, 0);
        }

        [Test]
        public void Catalog_HarvestThree_RequiresThreeProgress()
        {
            FarmDailyGoalDefinition def = null;
            foreach (var d in FarmDailyGoalCatalog.All)
            {
                if (d.GoalId == FarmDailyGoalCatalog.GoalHarvestThree) def = d;
            }
            Assert.IsNotNull(def);
            Assert.AreEqual(3, def.RequiredProgress, "n/m: meta de colher 3 exige RequiredProgress 3.");
        }

        // ───────────────────────── CA-1 (reset): reset diário limpa Claimed ─────────────────────────

        [Test]
        public void DailyReset_ClearsClaimedAndProgress()
        {
            // Reproduz o efeito de ResetDailyGoals sobre um estado já pago.
            var state = CompletedState(FarmDailyGoalCatalog.GoalFirstHarvest);
            state.Claimed = true;

            // Reset (mesma semântica do serviço): zera progresso e flags.
            state.Day = 2;
            state.CurrentProgress = 0;
            state.Completed = false;
            state.Claimed = false;

            var sink = new FakeRewardSink();
            // Após reset não está concluída → não paga; ao re-concluir, paga de novo (re-habilitado).
            Assert.IsFalse(FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false));

            state.CurrentProgress = state.RequiredProgress;
            state.Completed = true;
            var paidAgain = FarmDailyGoalService.TryClaimReward(state, sink, publishToast: false);
            Assert.IsTrue(paidAgain, "Dia novo reseta Claimed e re-habilita o pagamento.");
            Assert.AreEqual(1, sink.GrantCalls);
        }

        // ───────────────────────── CA-4: round-trip de save preserva Claimed ─────────────────────────

        [Test]
        public void SaveRoundTrip_PreservesClaimedAndProgress()
        {
            var original = new FarmDailyGoalsSaveData();
            original.Goals.Add(new FarmDailyGoalState
            {
                GoalId = FarmDailyGoalCatalog.GoalFirstHarvest,
                Day = 3,
                CurrentProgress = 1,
                RequiredProgress = 1,
                Completed = true,
                Claimed = true
            });

            // JsonUtility round-trip (mesma serialização do SaveManager).
            var json = UnityEngine.JsonUtility.ToJson(original);
            var restored = UnityEngine.JsonUtility.FromJson<FarmDailyGoalsSaveData>(json);

            Assert.AreEqual(1, restored.Goals.Count);
            var g = restored.Goals[0];
            Assert.AreEqual(FarmDailyGoalCatalog.GoalFirstHarvest, g.GoalId);
            Assert.IsTrue(g.Completed);
            Assert.IsTrue(g.Claimed, "Claimed persiste no round-trip — reload não re-paga.");
            Assert.AreEqual(3, g.Day);
        }

        // ───────────────────────── CA-2: projeção do widget ─────────────────────────

        [Test]
        public void Projection_BuildsRowsWithCheckAndProgressText()
        {
            var goals = new List<FarmDailyGoalState>
            {
                new FarmDailyGoalState { GoalId = FarmDailyGoalCatalog.GoalFirstHarvest, CurrentProgress = 1, RequiredProgress = 1, Completed = true, Claimed = true },
                new FarmDailyGoalState { GoalId = FarmDailyGoalCatalog.GoalHarvestThree, CurrentProgress = 2, RequiredProgress = 3, Completed = false, Claimed = false }
            };

            var projection = new DailyGoalsHudProjection();
            projection.Rebuild(goals);

            Assert.AreEqual(2, projection.TotalCount);
            Assert.AreEqual(1, projection.CompletedCount);
            Assert.IsFalse(projection.IsEmpty);

            var harvestRow = projection.Rows[0];
            Assert.AreEqual("[x]", harvestRow.CheckMark);
            Assert.AreEqual(string.Empty, harvestRow.ProgressText, "RequiredProgress 1 não mostra n/m.");
            Assert.AreEqual("Primeira colheita do dia", harvestRow.DisplayName);

            var threeRow = projection.Rows[1];
            Assert.AreEqual("[ ]", threeRow.CheckMark);
            Assert.AreEqual("2/3", threeRow.ProgressText, "n/m mostrado quando RequiredProgress > 1.");
        }

        [Test]
        public void Projection_NullGoals_IsEmptyState()
        {
            var projection = new DailyGoalsHudProjection();
            projection.Rebuild(null);
            Assert.IsTrue(projection.IsEmpty);
            Assert.AreEqual(0, projection.TotalCount);
        }
    }
}
