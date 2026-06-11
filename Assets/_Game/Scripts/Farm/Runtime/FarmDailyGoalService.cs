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
        private const string GoalFirstHarvest = "daily_goal_first_harvest";
        private const string GoalSellFirstCrop = "daily_goal_sell_first_crop";

        private readonly List<FarmDailyGoalDefinition> _definitions = new List<FarmDailyGoalDefinition>
        {
            new FarmDailyGoalDefinition
            {
                GoalId = GoalFirstHarvest,
                DisplayName = "Primeira colheita do dia",
                RequiredProgress = 1
            },
            new FarmDailyGoalDefinition
            {
                GoalId = GoalSellFirstCrop,
                DisplayName = "Vender primeiro item colhido",
                RequiredProgress = 1
            }
        };

        private readonly Dictionary<string, FarmDailyGoalState> _states =
            new Dictionary<string, FarmDailyGoalState>();

        private int _currentDay = 1;

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
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
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
        }

        private void OnEconomyTransaction(EconomyTransactionCompletedEvent evt)
        {
            if (!evt.WasSuccessful)
            {
                return;
            }

            // Apenas transações de venda avançam a meta de vender colheita
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
            }
            else
            {
                Debug.Log($"[FarmDailyGoalService] Goal '{goalId}' progress: {state.CurrentProgress}/{state.RequiredProgress}.", this);
            }
        }
    }
}
