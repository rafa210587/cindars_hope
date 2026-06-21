using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Gerencia metas diárias de farm.
    /// Escuta CropHarvestedEvent (primeira colheita) e EconomyTransactionCompletedEvent (primeira venda).
    /// Persiste estado via FarmDailyGoalsSaveData no GameSaveData.DailyGoals.
    /// Usa GameEventBus — sem chamadas diretas entre sistemas.
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmDailyGoalService : MonoBehaviour
    {
        // ids estáveis — NUNCA renomear (id-stability). Definições no catálogo canônico único.
        private const string GoalFirstHarvest = FarmDailyGoalCatalog.GoalFirstHarvest;
        private const string GoalSellFirstCrop = FarmDailyGoalCatalog.GoalSellFirstCrop;
        // fable_65: metas novas (todas ligadas a eventos REAIS verificados na Fase 0).
        private const string GoalHarvestThree = FarmDailyGoalCatalog.GoalHarvestThree;
        private const string GoalPlantThree = FarmDailyGoalCatalog.GoalPlantThree;
        private const string GoalGatherResource = FarmDailyGoalCatalog.GoalGatherResource;
        private const string GoalTalkToNpc = FarmDailyGoalCatalog.GoalTalkToNpc;

        // Catálogo único — sem segunda lista de definições (ver FarmDailyGoalCatalog).
        private IReadOnlyList<FarmDailyGoalDefinition> _definitions => FarmDailyGoalCatalog.All;

        private readonly Dictionary<string, FarmDailyGoalState> _states =
            new Dictionary<string, FarmDailyGoalState>();

        private int _currentDay = 1;

        // fable_65: canal de recompensa (ouro+XP). Injetável p/ teste; default resolve via GameBootstrap.
        private IDailyGoalRewardSink _rewardSink = new GameBootstrapDailyGoalRewardSink();

        // Ponto de acesso estático para save/load externo (via FarmDailyGoalRuntimeBootstrap)
        private static FarmDailyGoalService _instance;

        public static FarmDailyGoalService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            InitializeGoalStates();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            // fable_65: gatilhos das metas novas (eventos reais existentes).
            GameEventBus.Subscribe<SeedPlantedEvent>(OnSeedPlanted);
            GameEventBus.Subscribe<TreeChoppedEvent>(OnTreeChopped);
            GameEventBus.Subscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<SeedPlantedEvent>(OnSeedPlanted);
            GameEventBus.Unsubscribe<TreeChoppedEvent>(OnTreeChopped);
            GameEventBus.Unsubscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
        }

        /// <summary>
        /// fable_65: injeta um canal de recompensa alternativo (testes). Null restaura o default.
        /// </summary>
        public void SetRewardSink(IDailyGoalRewardSink rewardSink)
        {
            _rewardSink = rewardSink ?? new GameBootstrapDailyGoalRewardSink();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// fable_54 (hook aditivo): progride a meta diária de COLHEITA por uma fonte externa
        /// (ex.: depósito na caixa de envio conta como colheita entregue — WI-24). Idempotente
        /// quanto à conclusão (não passa de Completed). Sem segundo caminho de meta: reusa AddProgress.
        /// </summary>
        public void ProgressHarvestGoal(int amount)
        {
            if (amount <= 0) return;
            AddProgress(GoalFirstHarvest, amount);
        }

        /// <summary>
        /// Lê snapshot das metas atuais (somente leitura para UI).
        /// </summary>
        public IReadOnlyList<FarmDailyGoalState> GetCurrentGoals()
        {
            var list = new List<FarmDailyGoalState>();
            foreach (var state in _states.Values)
            {
                list.Add(state);
            }
            return list;
        }

        /// <summary>
        /// Captura o estado para save.
        /// </summary>
        public FarmDailyGoalsSaveData CaptureSaveData()
        {
            var data = new FarmDailyGoalsSaveData();
            foreach (var state in _states.Values)
            {
                data.Goals.Add(new FarmDailyGoalState
                {
                    GoalId = state.GoalId,
                    Day = state.Day,
                    CurrentProgress = state.CurrentProgress,
                    RequiredProgress = state.RequiredProgress,
                    Completed = state.Completed,
                    Claimed = state.Claimed
                });
            }
            return data;
        }

        /// <summary>
        /// Restaura o estado a partir do save.
        /// Idempotente: não duplica progresso após load.
        /// </summary>
        public void RestoreFromSaveData(FarmDailyGoalsSaveData saveData)
        {
            if (saveData == null || saveData.Goals == null)
            {
                return;
            }

            foreach (var savedState in saveData.Goals)
            {
                if (string.IsNullOrWhiteSpace(savedState.GoalId))
                {
                    continue;
                }

                if (_states.TryGetValue(savedState.GoalId, out var existing))
                {
                    existing.Day = savedState.Day;
                    existing.CurrentProgress = savedState.CurrentProgress;
                    existing.RequiredProgress = savedState.RequiredProgress;
                    existing.Completed = savedState.Completed;
                    existing.Claimed = savedState.Claimed;
                }
            }

            Debug.Log($"[FarmDailyGoalService] Restored {saveData.Goals.Count} daily goal states from save.", this);
        }

        private void InitializeGoalStates()
        {
            _states.Clear();
            foreach (var def in _definitions)
            {
                _states[def.GoalId] = new FarmDailyGoalState
                {
                    GoalId = def.GoalId,
                    Day = _currentDay,
                    CurrentProgress = 0,
                    RequiredProgress = def.RequiredProgress,
                    Completed = false,
                    Claimed = false
                };
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;
            ResetDailyGoals();
        }

        private void ResetDailyGoals()
        {
            foreach (var def in _definitions)
            {
                if (_states.TryGetValue(def.GoalId, out var state))
                {
                    state.Day = _currentDay;
                    state.CurrentProgress = 0;
                    state.RequiredProgress = def.RequiredProgress;
                    state.Completed = false;
                    state.Claimed = false;
                }
            }

            Debug.Log($"[FarmDailyGoalService] Daily goals reset for day {_currentDay}.", this);
        }

        private void OnCropHarvested(CropHarvestedEvent evt)
        {
            AddProgress(GoalFirstHarvest, 1);
            AddProgress(GoalHarvestThree, 1);
        }

        private void OnSeedPlanted(SeedPlantedEvent evt)
        {
            AddProgress(GoalPlantThree, 1);
        }

        private void OnTreeChopped(TreeChoppedEvent evt)
        {
            AddProgress(GoalGatherResource, 1);
        }

        private void OnNpcInteractionStarted(NpcInteractionStartedEvent evt)
        {
            AddProgress(GoalTalkToNpc, 1);
        }

        private void OnEconomyTransaction(EconomyTransactionCompletedEvent evt)
        {
            if (!evt.WasSuccessful)
            {
                return;
            }

            // Apenas transações de venda avançam as metas de venda
            if (evt.TransactionType == "sell" || evt.TransactionType == "Sell" ||
                evt.TransactionType == "shipping" || evt.GoldDelta > 0)
            {
                AddProgress(GoalSellFirstCrop, 1);
            }
        }

        private void AddProgress(string goalId, int amount)
        {
            if (!_states.TryGetValue(goalId, out var state))
            {
                return;
            }

            if (state.Completed)
            {
                return;
            }

            state.CurrentProgress += amount;

            GameEventBus.Publish(new DailyGoalProgressedEvent(goalId, state.CurrentProgress, state.RequiredProgress));

            if (state.CurrentProgress >= state.RequiredProgress)
            {
                state.Completed = true;
                GameEventBus.Publish(new DailyGoalCompletedEvent(goalId));
                Debug.Log($"[FarmDailyGoalService] Goal '{goalId}' completed on day {_currentDay}.", this);
                TryClaimReward(state, _rewardSink, publishToast: true);
            }
            else
            {
                Debug.Log($"[FarmDailyGoalService] Goal '{goalId}' progress: {state.CurrentProgress}/{state.RequiredProgress}.", this);
            }
        }

        /// <summary>
        /// fable_65 — ponto ÚNICO de pagamento de recompensa de meta diária. Pura quanto à
        /// decisão: paga (via sink) EXATAMENTE 1× se a meta está Completed e ainda !Claimed e
        /// tem entrada na tabela. Idempotente — reload no meio do dia (state já Claimed) não
        /// re-paga; reset diário limpa Claimed e re-habilita. Testável em EditMode com um sink fake.
        /// Retorna true se pagou nesta chamada.
        /// </summary>
        public static bool TryClaimReward(FarmDailyGoalState state, IDailyGoalRewardSink rewardSink, bool publishToast)
        {
            if (state == null || !state.Completed || state.Claimed)
            {
                return false;
            }

            if (!FarmDailyGoalRewardTable.TryGet(state.GoalId, out var reward))
            {
                // Meta sem entrada de recompensa: marca Claimed para nao reavaliar, mas nao paga valor inventado.
                state.Claimed = true;
                return false;
            }

            state.Claimed = true;
            rewardSink?.Grant(reward.Gold, reward.Xp);

            if (publishToast)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(
                    $"Meta concluída: +{reward.Gold} ouro, +{reward.Xp} XP", 3.5f));
            }

            return true;
        }
    }
}
