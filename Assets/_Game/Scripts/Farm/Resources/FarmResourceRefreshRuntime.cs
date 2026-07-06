using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Farm.Integration;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.Farm.Resources
{
    /// <summary>
    /// Host runtime do FarmResourceRefreshProcessor (módulo WAVE 05, intocado).
    /// Interactables de recurso registram seu estado ao depletar; no DayStartedEvent o
    /// processor decide quais nós renovam pela política da definição e o host reativa o visual.
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmResourceRefreshRuntime : MonoBehaviour
    {
        private sealed class TrackedNode
        {
            public ResourceNodeInstanceState State;
            public Action OnRefreshed;
        }

        private static FarmResourceRefreshRuntime _instance;

        private readonly List<TrackedNode> _trackedNodes = new List<TrackedNode>();
        private FarmResourceRefreshProcessor _processor;

        public static FarmResourceRefreshRuntime Instance => _instance;

        /// <summary>
        /// Definições default por tipo de interactable da fazenda (dados de wiring;
        /// a lógica de refresh permanece 100% no processor).
        /// </summary>
        public static Dictionary<string, ResourceNodeDefinition> BuildDefaultDefinitions()
        {
            return new Dictionary<string, ResourceNodeDefinition>
            {
                ["farm_node_tree"] = new ResourceNodeDefinition
                {
                    NodeId = "farm_node_tree",
                    NodeType = ResourceNodeType.Tree,
                    DisplayName = "Árvore",
                    RefreshPolicy = ResourceNodeRefreshPolicy.FixedDays,
                    RefreshAfterDays = 3,
                    CanRegrow = true
                },
                ["farm_node_rock"] = new ResourceNodeDefinition
                {
                    NodeId = "farm_node_rock",
                    NodeType = ResourceNodeType.Rock,
                    DisplayName = "Rocha",
                    RefreshPolicy = ResourceNodeRefreshPolicy.FixedDays,
                    RefreshAfterDays = 3,
                    CanRegrow = true
                },
                ["farm_node_forage"] = new ResourceNodeDefinition
                {
                    NodeId = "farm_node_forage",
                    NodeType = ResourceNodeType.Forage,
                    DisplayName = "Coleta",
                    RefreshPolicy = ResourceNodeRefreshPolicy.NextDayChance,
                    NextDayRefreshChance = 0.5f,
                    CanRegrow = true
                }
            };
        }

        public static string ResolveNodeId(FarmResourceInteractableType type)
        {
            switch (type)
            {
                case FarmResourceInteractableType.Tree:
                    return "farm_node_tree";
                case FarmResourceInteractableType.Rock:
                    return "farm_node_rock";
                case FarmResourceInteractableType.Forage:
                    return "farm_node_forage";
                default:
                    return string.Empty;
            }
        }

        /// <summary>Registra um nó depletado para renovação futura. onRefreshed reativa o visual.</summary>
        public void RegisterDepletedNode(ResourceNodeInstanceState state, Action onRefreshed)
        {
            if (state == null)
            {
                return;
            }

            _trackedNodes.Add(new TrackedNode { State = state, OnRefreshed = onRefreshed });
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            _processor = new FarmResourceRefreshProcessor(BuildDefaultDefinitions());
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (_trackedNodes.Count == 0)
            {
                return;
            }

            var date = GameDate.FromAbsoluteDay(evt.DayNumber);
            var weatherService = WorldWeatherService.Instance;
            var context = new ResourceNodeRefreshContext
            {
                CurrentDay = evt.DayNumber,
                CurrentSeason = date.CurrentSeason.ToString(),
                CurrentWeather = weatherService != null ? weatherService.CurrentWeather.ToString() : string.Empty,
                FarmLevel = 0,
                RandomSeed = evt.DayNumber
            };

            for (var i = _trackedNodes.Count - 1; i >= 0; i--)
            {
                var tracked = _trackedNodes[i];
                if (tracked.State == null || !tracked.State.IsDepleted)
                {
                    _trackedNodes.RemoveAt(i);
                    continue;
                }

                if (_processor.TryRefresh(tracked.State, context))
                {
                    tracked.OnRefreshed?.Invoke();
                    _trackedNodes.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>Garante o host em runtime (padrão FarmDailyGoalRuntimeBootstrap).</summary>
    public static class FarmResourceRefreshRuntimeBootstrap
    {
        public static FarmResourceRefreshRuntime Install(Transform owner)
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<FarmResourceRefreshRuntime>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("FarmResourceRefreshRuntime");
            if (owner != null) go.transform.SetParent(owner, false);
            else UnityEngine.Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<FarmResourceRefreshRuntime>();
            Debug.Log("[FarmResourceRefreshRuntimeBootstrap] FarmResourceRefreshRuntime instanciado via bootstrap.");
            return service;
        }
    }
}
