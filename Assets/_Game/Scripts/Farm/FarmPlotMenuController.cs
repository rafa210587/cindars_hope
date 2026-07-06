using System;
using System.Collections.Generic;
using CindarsHope.Gameplay.Input;
using UnityEngine;

namespace CindarsHope.Farm
{
    internal enum FarmMenuActionType
    {
        Till,
        Water,
        Plant,
        Harvest,
        Status,
        AdvanceGrowth,
        ClearDead,
        Analyze,
        Fertilize
    }

    internal readonly struct FarmMenuAction
    {
        public readonly FarmMenuActionType Type;
        public readonly string Label;
        public readonly string SeedId;
        public readonly string SeedItemId;

        public FarmMenuAction(FarmMenuActionType type, string label, string seedId = "", string seedItemId = "")
        {
            Type = type;
            Label = label;
            SeedId = seedId ?? string.Empty;
            SeedItemId = seedItemId ?? string.Empty;
        }
    }

    /// <summary>
    /// Controla o menu de ação do FarmPlot (input teclado + GUI). Extraído de FarmPlot.cs (refactor fable_decomp).
    /// Não é MonoBehaviour; é criado e possuído por FarmPlot.
    /// </summary>
    internal sealed class FarmPlotMenuController
    {
        internal static FarmPlot ActiveMenuPlot { get; private set; }

        private readonly FarmPlot _owner;
        private readonly List<FarmMenuAction> _menuActions = new List<FarmMenuAction>();
        private int _selectedMenuIndex;
        private int _menuOpenedFrame = -1;
        private string _feedback = string.Empty;
        private IDisposable _inputBlockLease;

        internal FarmPlotMenuController(FarmPlot owner)
        {
            _owner = owner;
        }

        internal void Update()
        {
            if (ActiveMenuPlot != _owner)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                _selectedMenuIndex = Mathf.Max(0, _selectedMenuIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                _selectedMenuIndex = Mathf.Min(_menuActions.Count - 1, _selectedMenuIndex + 1);
            }
            else if (Time.frameCount != _menuOpenedFrame &&
                     (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
            {
                ExecuteSelectedMenuAction();
            }
        }

        internal void OnGUI(int plotIndex, FarmPlotState state)
        {
            if (ActiveMenuPlot != _owner)
            {
                return;
            }

            CindarsHope.UI.MenuGuiStyle.Apply();
            var screenPosition = _owner.GetMenuScreenPositionInternal();
            var width = 260f;
            var height = Mathf.Clamp(70f + _menuActions.Count * 26f, 90f, 260f);
            var rect = new Rect(screenPosition.x - width * 0.5f, screenPosition.y - height, width, height);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label($"Plot {plotIndex}: {state}");

            if (_menuActions.Count == 0)
            {
                GUILayout.Label(string.IsNullOrWhiteSpace(_feedback) ? "Sem acao disponivel." : _feedback);
            }
            else
            {
                for (var index = 0; index < _menuActions.Count; index++)
                {
                    GUILayout.Label(index == _selectedMenuIndex ? $"> {_menuActions[index].Label}" : $"  {_menuActions[index].Label}");
                }
            }

            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GUILayout.Space(4f);
                GUILayout.Label(_feedback);
            }

            GUILayout.EndArea();
        }

        internal void OpenMenu(FarmPlotState state)
        {
            BuildMenuActions(state);
            if (_menuActions.Count == 0)
            {
                _owner.PublishFeedbackInternal(string.IsNullOrWhiteSpace(_feedback) ? "No farm action available." : _feedback);
                return;
            }

            ActiveMenuPlot = _owner;
            _inputBlockLease?.Dispose();
            _inputBlockLease = GameplayInputBlocker.Acquire(GameplayInputBlockReason.FarmActionMenu);
            _selectedMenuIndex = 0;
            _menuOpenedFrame = Time.frameCount;
        }

        internal void CloseMenu()
        {
            if (ActiveMenuPlot == _owner)
            {
                ActiveMenuPlot = null;
            }

            _inputBlockLease?.Dispose();
            _inputBlockLease = null;

            _menuActions.Clear();
            _selectedMenuIndex = 0;
        }

        internal void SetFeedback(string msg)
        {
            _feedback = msg ?? string.Empty;
        }

        internal string GetFeedback()
        {
            return _feedback;
        }

        private void BuildMenuActions(FarmPlotState state)
        {
            _menuActions.Clear();
            _feedback = string.Empty;

            switch (state)
            {
                case FarmPlotState.Raw:
                    if (_owner.HasRequiredToolInternal(CindarsHope.Tools.ToolType.Hoe) || _owner.TemporarySliceModeInternal)
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Till, "Arar solo"));
                    }
                    else
                    {
                        _feedback = "Hoe required.";
                    }

                    break;
                case FarmPlotState.TilledDry:
                    if (_owner.HasRequiredToolInternal(CindarsHope.Tools.ToolType.WateringCan) || _owner.TemporarySliceModeInternal)
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Water, "Molhar solo"));
                    }

                    AddPlantActions(state);
                    break;
                case FarmPlotState.TilledWet:
                    AddPlantActions(state);
                    break;
                case FarmPlotState.PlantedDry:
                    if (_owner.HasRequiredToolInternal(CindarsHope.Tools.ToolType.WateringCan) || _owner.TemporarySliceModeInternal)
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Water, "Molhar solo"));
                    }
                    else
                    {
                        _feedback = "Watering Can required.";
                    }

                    AddFertilizeActions();
                    break;
                case FarmPlotState.PlantedWet:
                    AddFertilizeActions();
                    if (_owner.TemporarySliceModeInternal)
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.AdvanceGrowth, "Simular crescimento"));
                    }
                    else
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Status, "Ja irrigado"));
                    }
                    break;
                case FarmPlotState.ReadyToHarvest:
                    _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Harvest, "Colher"));
                    break;
                case FarmPlotState.Dead:
                    if (_owner.HasRequiredToolInternal(CindarsHope.Tools.ToolType.Hoe) || _owner.TemporarySliceModeInternal)
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.ClearDead, "Limpar solo"));
                    }
                    else
                    {
                        _feedback = "Hoe required to clear.";
                    }

                    break;
                case FarmPlotState.Blocked:
                    _feedback = "Plot blocked.";
                    break;
            }

            if (state != FarmPlotState.Blocked)
            {
                _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Analyze, "Analisar solo"));
            }
        }

        private void AddFertilizeActions()
        {
            if (_owner.InventoryManagerInternal == null || FarmFertilityRuntime.HasActiveFertilizer(_owner.PlotId))
            {
                return;
            }

            foreach (var definition in FarmFertilityRuntime.Definitions.Values)
            {
                if (definition == null || definition.IsEndgameReserved)
                {
                    continue;
                }

                if (!_owner.InventoryManagerInternal.HasItem(definition.FertilizerId))
                {
                    continue;
                }

                _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Fertilize, $"Aplicar {definition.DisplayName}", definition.FertilizerId));
            }
        }

        private void AddPlantActions(FarmPlotState state)
        {
            if (_owner.InventoryManagerInternal == null || _owner.SeedDatabaseInternal == null)
            {
                _feedback = "Inventory or seed database missing.";
                return;
            }

            foreach (var item in _owner.InventoryManagerInternal.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Key) || item.Value <= 0)
                {
                    continue;
                }

                if (!_owner.TryResolveSeedDataForItemInternal(item.Key, out var seedData) || seedData == null)
                {
                    continue;
                }

                var label = seedData.SeedItem != null && !string.IsNullOrWhiteSpace(seedData.SeedItem.DisplayName)
                    ? $"Plantar {seedData.SeedItem.DisplayName}"
                    : $"Plantar {item.Key}";
                _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Plant, label, seedData.Id, item.Key));
            }

            if (_menuActions.Count == 0)
            {
                AddTemporaryPlantAction();
            }
        }

        private void AddTemporaryPlantAction()
        {
            if (!_owner.TemporarySliceModeInternal)
            {
                _feedback = "No seeds in inventory.";
                return;
            }

            if (!_owner.SeedDatabaseInternal.TryGetById(_owner.TemporarySequentialSeedIdInternal, out var seedData) || seedData == null)
            {
                _feedback = "Temporary seed not registered.";
                return;
            }

            var label = seedData.SeedItem != null && !string.IsNullOrWhiteSpace(seedData.SeedItem.DisplayName)
                ? $"Plantar {seedData.SeedItem.DisplayName}"
                : $"Plantar {seedData.Id}";
            var seedItemId = seedData.SeedItem != null ? seedData.SeedItem.Id : string.Empty;
            _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Plant, label, seedData.Id, seedItemId));
        }

        private void ExecuteSelectedMenuAction()
        {
            if (_selectedMenuIndex < 0 || _selectedMenuIndex >= _menuActions.Count)
            {
                return;
            }

            var action = _menuActions[_selectedMenuIndex];
            var closeAfterAction = true;
            switch (action.Type)
            {
                case FarmMenuActionType.Till:
                    _owner.TryTillInternal();
                    break;
                case FarmMenuActionType.Water:
                    _owner.TryWaterInternal();
                    break;
                case FarmMenuActionType.Plant:
                    _owner.TryPlantSeedInternal(action.SeedId, action.SeedItemId);
                    break;
                case FarmMenuActionType.Harvest:
                    _owner.TryHarvestInternal();
                    break;
                case FarmMenuActionType.Status:
                    _owner.PublishFeedbackInternal("Plot already watered.");
                    break;
                case FarmMenuActionType.AdvanceGrowth:
                    _owner.TryAdvanceTemporaryGrowthInternal();
                    break;
                case FarmMenuActionType.ClearDead:
                    _owner.TryClearDeadInternal();
                    break;
                case FarmMenuActionType.Analyze:
                    _owner.ExecuteAnalyzeInternal();
                    closeAfterAction = false;
                    break;
                case FarmMenuActionType.Fertilize:
                    _owner.TryFertilizeInternal(action.SeedId);
                    break;
            }

            if (closeAfterAction)
            {
                CloseMenu();
            }
        }
    }
}
